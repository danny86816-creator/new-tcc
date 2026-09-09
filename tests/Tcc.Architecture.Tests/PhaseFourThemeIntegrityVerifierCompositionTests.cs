using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Integrity;
using Status = Tcc.Presentation.Contracts.Theme.ThemeSignatureVerificationStatus;
using EvidenceStatus = Tcc.Presentation.Contracts.Theme.ThemeIntegrityEvidenceStatus;

namespace Tcc.Architecture.Tests;

public sealed class PhaseFourThemeIntegrityVerifierCompositionTests
{
    private const string ThemePath = "theme.json";
    private const string IntegrityPath = "meta/integrity.json";
    private const string EnvelopePath = "meta/signature.json";
    private const string AssetPath = "assets/icon.png";
    // Public test vector only: SEC 2 P-256 generator and scalar 1, never a deployment key.
    private const string GeneratorX = "6B17D1F2E12C4247F8BCE6E563A440F277037D812DEB33A0F4A13945D898C296";
    private const string GeneratorY = "4FE342E2FE1A7F9B8EE7EB4A7C0F9E162BCE33576B315ECECBB6406837BF51F5";

    [Theory]
    [InlineData("signed", Status.Valid, "example-publisher", "test-key")]
    [InlineData("beta", Status.NotRequired, null, null)]
    [InlineData("developer", Status.DeveloperExceptionAccepted, null, null)]
    public async Task RealRawPackageSuccessMapsAllTenFieldsAndExactReads(
        string mode, Status status, string? publisher, string? key)
    {
        Package package = CreatePackage(mode);
        using CancellationTokenSource cancellation = new();
        Reader reader = package.NewReader();
        ThemeIntegrityVerificationResultV2 result = await new ThemeIntegrityVerifier()
            .VerifyAsync(package.Request, reader, cancellation.Token);

        AssertFields(result, true, package.Hashes, package.Evidence, status, publisher, key, []);
        AssertReads(reader, package, signed: mode == "signed");
        Assert.All(reader.Tokens, token => Assert.Equal(cancellation.Token, token));
        Assert.All(reader.PackageRefs, reference => Assert.Equal(package.Request.PackageRef, reference));
        Assert.IsType<ImmutableArray<ThemeIntegrityFileEvidenceV1>>(result.FileEvidence);
        Assert.IsType<ImmutableArray<ThemeIntegrityFileEvidenceV1>>(result.AssetEvidence);
        Assert.IsType<ImmutableArray<ThemeIntegrityDiagnosticV1>>(result.Diagnostics);
        if (mode == "signed")
        {
            using ECDsa independent = ECDsa.Create();
            independent.ImportSubjectPublicKeyInfo(Convert.FromBase64String(package.Request.TrustSnapshot.TrustedSigners[0].PublicKey), out int read);
            Assert.Equal(91, read);
            Assert.True(independent.VerifyData(package.SignedPayload,
                Convert.FromBase64String(package.Request.SignatureEnvelope!.Signature), HashAlgorithmName.SHA256,
                DSASignatureFormat.IeeeP1363FixedFieldConcatenation));
        }
    }

    [Theory]
    [InlineData("null request")]
    [InlineData("null reader")]
    [InlineData("multiple")]
    public async Task PreflightFailureMapsAllTenFieldsAndNeverEnumerates(string mode)
    {
        Package package = CreatePackage();
        Reader reader = package.NewReader();
        ThemeIntegrityVerificationRequestV2 request = package.Request;
        ThemeIntegrityDiagnosticV1[] expected;
        if (mode == "multiple")
        {
            request = request with { Policy = null!, SignatureEnvelope = request.SignatureEnvelope! with { Signature = " " } };
            expected = [Diagnostic("P4I013", null, "A present signature envelope must contain a nonblank signature value."),
                Diagnostic("P4I020", null, "A required preflight security object, value, collection, or collection item is absent.")];
        }
        else expected = [Diagnostic("P4I020", null, "The verification request and caller-provided package content reader are required.")];
        ThemeIntegrityVerificationResultV2 result = await new ThemeIntegrityVerifier().VerifyAsync(
            mode == "null request" ? null! : request, mode == "null reader" ? null! : reader);
        AssertFields(result, false, null, [], Status.NotEvaluated, null, null, expected);
        Assert.Equal(0, reader.Enumerations);
        Assert.Empty(reader.ReadCounts);
    }

    [Theory]
    [InlineData(EvidenceStatus.MissingRequired, "P4I005", "Required inventory entry is absent.")]
    [InlineData(EvidenceStatus.HashMismatch, "P4I008", "Declared file hash does not match raw content bytes.")]
    [InlineData(EvidenceStatus.LengthMismatch, "P4I007", "Declared or enumerated file length does not match content length.")]
    [InlineData(EvidenceStatus.Unavailable, "P4I021", "Package content bytes are unavailable for an enumerated file.")]
    [InlineData(EvidenceStatus.UnsupportedEntry, "P4I019", "Only regular file entries are supported by the integrity contract.")]
    public async Task InventoryFailuresPreserveCompleteEvidenceAndAssetStatus(
        EvidenceStatus status, string code, string message)
    {
        Package package = CreatePackage();
        Reader reader = package.NewReader();
        ThemeIntegrityFileEvidenceV1[] expected = package.Evidence.ToArray();
        int asset = Array.FindIndex(expected, item => item.CanonicalPath == AssetPath);
        ThemeIntegrityFileEvidenceV1 item = expected[asset];
        switch (status)
        {
            case EvidenceStatus.MissingRequired:
                reader.Entries = reader.Entries.Where(entry => entry.LogicalPath != AssetPath).ToArray();
                item = item with { IsPresent = false, ActualLengthBytes = null, ActualSha256 = null };
                break;
            case EvidenceStatus.HashMismatch:
                reader.Replacements[AssetPath] = [3, 2, 1];
                item = item with { ActualSha256 = Hash([3, 2, 1]) };
                break;
            case EvidenceStatus.LengthMismatch:
                reader.Replacements[AssetPath] = [1, 2];
                item = item with { ActualLengthBytes = 2, ActualSha256 = Hash([1, 2]) };
                break;
            case EvidenceStatus.Unavailable:
                reader.Failures[AssetPath] = new IOException("private reader detail");
                item = item with { ActualSha256 = null };
                break;
            case EvidenceStatus.UnsupportedEntry:
                reader.Entries = reader.Entries.Select(entry => entry.LogicalPath == AssetPath
                    ? entry with { EntryKind = ThemePackageContentEntryKind.Directory } : entry).ToArray();
                item = item with { ActualLengthBytes = null, ActualSha256 = null };
                break;
        }
        expected[asset] = item with { Status = status };
        ThemeIntegrityVerificationResultV2 result = await new ThemeIntegrityVerifier().VerifyAsync(package.Request, reader);
        AssertFields(result, false, null, expected, Status.NotEvaluated, null, null, [Diagnostic(code, AssetPath, message)]);
        Assert.Equal(1, reader.Enumerations);
        Assert.Equal(1, reader.Count(ThemePath));
        Assert.Equal(0, reader.Count(IntegrityPath));
        Assert.Equal(0, reader.Count(EnvelopePath));
        // Reader evidence is supplemented by the external source-copy invocation harness;
        // zero C reads alone cannot distinguish C's defensive zero-read branch.
    }

    [Theory]
    [InlineData("entry", "P4I027", "theme.json", "Theme Manifest must be present as verified package content before metadata validation.")]
    [InlineData("coherence", "P4I010", "theme.json", "Theme Manifest raw bytes do not match the content verified by the package inventory stage.")]
    [InlineData("envelope", "P4I012", "meta/signature.json", "Referenced Signature Envelope bytes are unavailable.")]
    public async Task MetadataFailuresKeepEvidenceAndEraseAllTopLevelHashes(string mode, string code, string path, string message)
    {
        Package package = CreatePackage();
        Reader reader = package.NewReader();
        ThemeIntegrityVerificationRequestV2 request = package.Request;
        ThemeIntegrityFileEvidenceV1[] evidence = package.Evidence;
        if (mode == "entry")
        {
            request = request with { IntegrityManifest = request.IntegrityManifest with
                { Files = request.IntegrityManifest.Files.Where(file => file.CanonicalPath != ThemePath).ToArray() } };
            reader.Entries = reader.Entries.Where(entry => entry.LogicalPath != ThemePath).ToArray();
            evidence = evidence.Where(item => item.CanonicalPath != ThemePath).ToArray();
        }
        if (mode == "coherence") reader.SecondThemeBytes = [0, 1];
        if (mode == "envelope") reader.Failures[EnvelopePath] = new FileNotFoundException("private path");
        ThemeIntegrityVerificationResultV2 result = await new ThemeIntegrityVerifier().VerifyAsync(request, reader);
        AssertFields(result, false, null, evidence, Status.NotEvaluated, null, null, [Diagnostic(code, path, message)]);
        Assert.Equal(mode == "entry" ? 0 : 2, reader.Count(ThemePath));
        Assert.Equal(mode == "envelope" ? 1 : 0, reader.Count(IntegrityPath));
    }

    [Theory]
    [InlineData("missing", "P4I012", Status.Missing, "A signature is required by the active theme integrity policy.")]
    [InlineData("invalid", "P4I013", Status.Invalid, "The signature or signer public-key material is invalid.")]
    [InlineData("unknown", "P4I014", Status.UnknownSigner, "No trusted signer entry matches the envelope publisher/key identity.")]
    [InlineData("revoked", "P4I015", Status.RevokedSigner, "The matching signer entry is revoked.")]
    [InlineData("profile", "P4I016", Status.UnsupportedAlgorithm, "The signer public key does not match the approved ECDSA P-256 SPKI profile.")]
    [InlineData("policy", "P4I023", Status.NotEvaluated, "The supplied trust snapshot does not match the expected trust policy identity or version.")]
    [InlineData("duplicate", "P4I022", Status.NotEvaluated, "The trust snapshot contains duplicate publisher/key signer identities.")]
    [InlineData("identity", "P4I029", Status.NotEvaluated, "A signing identity field is invalid or inconsistent.")]
    public async Task SignatureFailuresPassThroughStatusAndKeepAllThreeHashes(
        string mode, string code, Status status, string message)
    {
        Package package = FailurePackage(mode);
        Reader reader = package.NewReader();
        ThemeIntegrityVerificationResultV2 result = await new ThemeIntegrityVerifier().VerifyAsync(package.Request, reader);
        AssertFields(result, false, package.Hashes, package.Evidence, status, null, null, [Diagnostic(code, null, message)]);
        AssertReads(reader, package, signed: mode != "missing");
        Assert.DoesNotContain("private", result.Diagnostics[0].Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("assets/icon.png", true)]
    [InlineData("assets/ui/panel.png", true)]
    [InlineData("Assets/icon.png", false)]
    [InlineData("assets2/icon.png", false)]
    [InlineData("asset/icon.png", false)]
    public async Task ExactCaseSensitivePrefixIsTheOnlyMembershipRule(string path, bool included)
    {
        Package package = CreatePackage("beta", assetPath: path);
        ThemeIntegrityVerificationResultV2 result = await new ThemeIntegrityVerifier().VerifyAsync(package.Request, package.NewReader());
        AssertFields(result, true, package.Hashes, package.Evidence, Status.NotRequired, null, null, []);
        Assert.Equal(included ? 1 : 0, result.AssetEvidence.Count);
    }

    [Fact]
    public async Task OptionalMissingAndThemeManifestKindUnderAssetsAreRetained()
    {
        Package package = CreatePackage("beta", themePath: "assets/theme.json", missingOptional: true);
        ThemeIntegrityVerificationResultV2 result = await new ThemeIntegrityVerifier().VerifyAsync(package.Request, package.NewReader());
        AssertFields(result, true, package.Hashes, package.Evidence, Status.NotRequired, null, null, []);
        Assert.Contains(result.AssetEvidence, item => item.EntryKind == ThemeIntegrityEntryKind.ThemeManifest);
        Assert.Contains(result.AssetEvidence, item => item.Status == EvidenceStatus.MissingAllowed);
    }

    [Fact]
    public void ProjectionPreservesWholeItemsAndUnsortedStableSubsequenceWithoutMutatingFiles()
    {
        // Supplementary helper test deliberately uses unsorted evidence; lawful B output is sorted.
        // Undeclared is an existing enum but sealed B emits no evidence item for undeclared entries.
        ImmutableArray<ThemeIntegrityFileEvidenceV1>.Builder items = ImmutableArray.CreateBuilder<ThemeIntegrityFileEvidenceV1>();
        foreach (EvidenceStatus status in Enum.GetValues<EvidenceStatus>())
        {
            items.Add(new("other/" + status, ThemeIntegrityEntryKind.Payload, true, false, 3, null, new string('a', 64), null, status));
            items.Add(new("assets/z-" + status, ThemeIntegrityEntryKind.ThemeManifest, false, false, 3, null, new string('a', 64), null, status));
        }
        ImmutableArray<ThemeIntegrityFileEvidenceV1> files = items.ToImmutable();
        ImmutableArray<ThemeIntegrityFileEvidenceV1> assets = (ImmutableArray<ThemeIntegrityFileEvidenceV1>)typeof(ThemeIntegrityVerifier)
            .GetMethod("ProjectAssets", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [files])!;
        Assert.Equal(files.Length / 2, assets.Length);
        for (int index = 0; index < assets.Length; index++) Assert.Same(files[index * 2 + 1], assets[index]);
        Assert.Equal(items, files);
    }

    [Fact]
    public async Task MultipleInventoryDiagnosticsArePreservedInOrdinalOrder()
    {
        Package package = CreatePackage();
        Reader reader = package.NewReader();
        reader.Entries = reader.Entries.Where(entry => entry.LogicalPath != AssetPath && entry.LogicalPath != ThemePath).ToArray();
        ThemeIntegrityVerificationResultV2 result = await new ThemeIntegrityVerifier().VerifyAsync(package.Request, reader);
        ThemeIntegrityFileEvidenceV1[] evidence = package.Evidence.Select(item => item with
            { IsPresent = false, ActualLengthBytes = null, ActualSha256 = null, Status = EvidenceStatus.MissingRequired }).ToArray();
        AssertFields(result, false, null, evidence, Status.NotEvaluated, null, null,
            [Diagnostic("P4I005", AssetPath, "Required inventory entry is absent."), Diagnostic("P4I005", ThemePath, "Required inventory entry is absent.")]);
    }

    [Theory]
    [InlineData("enumeration")]
    [InlineData("assets/icon.png")]
    [InlineData("theme-second")]
    [InlineData("meta/integrity.json")]
    [InlineData("meta/signature.json")]
    public async Task SuspendedReaderCancellationPropagatesOriginalTokenWithoutResult(string location)
    {
        Package package = CreatePackage();
        Reader reader = package.NewReader();
        reader.SuspendAt = location;
        using CancellationTokenSource source = new();
        Task<ThemeIntegrityVerificationResultV2> pending = new ThemeIntegrityVerifier().VerifyAsync(package.Request, reader, source.Token).AsTask();
        await reader.Suspended.Task.WaitAsync(TimeSpan.FromSeconds(10));
        source.Cancel();
        OperationCanceledException failure = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.Equal(source.Token, failure.CancellationToken);
        Assert.All(reader.Tokens, token => Assert.Equal(source.Token, token));
        Assert.False(pending.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task PreCancellationPrecedesNullRequestDiagnostics()
    {
        using CancellationTokenSource source = new();
        source.Cancel();
        Reader reader = CreatePackage().NewReader();
        OperationCanceledException exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await new ThemeIntegrityVerifier().VerifyAsync(null!, reader, source.Token));
        Assert.Equal(source.Token, exception.CancellationToken);
        Assert.Equal(0, reader.Enumerations);
    }

    [Theory]
    [InlineData("enumeration", false)]
    [InlineData("enumeration", true)]
    [InlineData("assets/icon.png", false)]
    [InlineData("assets/icon.png", true)]
    [InlineData("theme-second", false)]
    [InlineData("theme-second", true)]
    [InlineData("meta/integrity.json", false)]
    [InlineData("meta/integrity.json", true)]
    [InlineData("meta/signature.json", false)]
    [InlineData("meta/signature.json", true)]
    public async Task UnexpectedReaderFaultsPropagateTheSameException(string location, bool disposed)
    {
        Package package = CreatePackage();
        Reader reader = package.NewReader();
        Exception expected = disposed ? new ObjectDisposedException("private detail") : new InvalidOperationException("private detail");
        reader.Failures[location] = expected;
        Exception? actual = await Record.ExceptionAsync(async () => await new ThemeIntegrityVerifier().VerifyAsync(package.Request, reader));
        Assert.Same(expected, actual);
    }

    [Fact]
    public async Task FiftyRepetitionsAcrossCulturesAndLegalPermutationsCompareAllTenFields()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo previousUi = CultureInfo.CurrentUICulture;
        try
        {
            Package[] matrix = [CreatePackage(), CreatePackage("beta"), CreatePackage("developer"), FailurePackage("revoked"), FailurePackage("policy")];
            ThemeIntegrityVerifier verifier = new();
            string[] expected = new string[matrix.Length];
            for (int i = 0; i < matrix.Length; i++) expected[i] = Serialize(await verifier.VerifyAsync(matrix[i].Request, matrix[i].NewReader()));
            foreach (string culture in new[] { "en-US", "tr-TR", "zh-TW" })
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
                for (int repeat = 0; repeat < 50; repeat++)
                {
                    for (int index = 0; index < matrix.Length; index++)
                    {
                        Reader reader = matrix[index].NewReader();
                        if (repeat % 2 == 1) reader.Entries = reader.Entries.Reverse().ToArray();
                        Assert.Equal(expected[index], Serialize(await verifier.VerifyAsync(matrix[index].Request, reader)));
                    }
                }
            }
        }
        finally { CultureInfo.CurrentCulture = previous; CultureInfo.CurrentUICulture = previousUi; }
    }

    [Theory]
    [InlineData("same")]
    [InlineData("different")]
    [InlineData("mixed")]
    public async Task SameVerifierSupportsOneHundredOverlappingCallsWithSafeReaders(string mode)
    {
        Package[] matrix = mode switch
        {
            "same" => [CreatePackage()],
            "different" => [CreatePackage(), CreatePackage("beta", assetPath: "assets/ui/panel.png"), CreatePackage("developer")],
            _ => [CreatePackage(), FailurePackage("unknown"), FailurePackage("revoked"), CreatePackage("beta")],
        };
        ThemeIntegrityVerifier verifier = new();
        string[] expected = new string[matrix.Length];
        for (int i = 0; i < matrix.Length; i++) expected[i] = Serialize(await verifier.VerifyAsync(matrix[i].Request, matrix[i].NewReader()));
        Reader[] readers = matrix.Select(package => package.NewReader()).ToArray();
        TaskCompletionSource start = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Task[] calls = Enumerable.Range(0, 100).Select(async index =>
        {
            await start.Task;
            int target = index % matrix.Length;
            Assert.Equal(expected[target], Serialize(await verifier.VerifyAsync(matrix[target].Request, readers[target])));
        }).ToArray();
        start.SetResult();
        await Task.WhenAll(calls);
        Assert.Equal(100, readers.Sum(reader => reader.Enumerations));
    }

    [Fact]
    public void ReleaseAssemblyContainsOnlyExactVerifierAndItsAttributedAsyncStateMachine()
    {
        Type owner = typeof(ThemeIntegrityVerifier);
        PhaseThreeScopeBoundaryTests.AssertExactVerifierImplementations(owner.Assembly.GetTypes());
        Assert.True(PhaseThreeScopeBoundaryTests.IsExactPublicVerifier(owner));
        MethodInfo method = owner.GetMethod("VerifyAsync", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
        Type machine = method.GetCustomAttribute<AsyncStateMachineAttribute>()!.StateMachineType;
        Assert.Same(machine, Assert.Single(owner.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)));
        Assert.True(PhaseThreeScopeBoundaryTests.IsApprovedVerifierAsyncStateMachine(machine));
        Assert.Empty(PhaseThreeScopeBoundaryTests.GetCompiledSurfaceViolations(owner.Assembly));
    }

    [Fact]
    public async Task ExternalInstrumentedSourceCopyProvesShortCircuitTerminalCancellationAndNoDiagnosticDedup()
    {
        // Supplementary observation of an explicitly instrumented COPY outside the repository.
        // A/B/C/D still execute the actual sealed production assembly. No production hook is added.
        // These observations are not claimed as injected callbacks in the unmodified public verifier.
        Assembly harness = await BuildExternalCompositionHarness();
        Type probe = harness.GetType("Tcc.Phase4E.Instrumented.Probe", true)!;
        IThemeIntegrityVerifierV2 verifier = (IThemeIntegrityVerifierV2)Activator.CreateInstance(
            harness.GetType("Tcc.Phase4E.Instrumented.ThemeIntegrityVerifier", true)!)!;
        foreach (string mode in new[] { "A", "B", "C", "D", "signed", "beta", "developer" })
        {
            Package package = mode == "D" ? FailurePackage("policy") : CreatePackage(mode is "beta" or "developer" ? mode : "signed");
            ThemeIntegrityVerificationRequestV2 request = package.Request;
            Reader reader = package.NewReader();
            if (mode == "A") request = null!;
            if (mode == "B") reader.Entries = reader.Entries.Where(entry => entry.LogicalPath != AssetPath).ToArray();
            if (mode == "C") reader.SecondThemeBytes = [0];
            List<string> calls = [];
            probe.GetField("Enter")!.SetValue(null, (Action<string>)calls.Add);
            ThemeIntegrityVerificationResultV2 expected = await new ThemeIntegrityVerifier().VerifyAsync(request, reader);
            reader = package.NewReader();
            if (mode == "B") reader.Entries = reader.Entries.Where(entry => entry.LogicalPath != AssetPath).ToArray();
            if (mode == "C") reader.SecondThemeBytes = [0];
            ThemeIntegrityVerificationResultV2 observed = await verifier.VerifyAsync(request, reader);
            Assert.Equal(Serialize(expected), Serialize(observed));
            string[] expectedCalls = mode switch { "A" => ["A"], "B" => ["A", "B"], "C" => ["A", "B", "C"], _ => ["A", "B", "C", "D"] };
            Assert.Equal(expectedCalls, calls);
            Assert.Equal(mode == "A" ? 0 : 1, reader.Enumerations);

            using CancellationTokenSource source = new();
            probe.GetField("Terminal")!.SetValue(null, (Action)source.Cancel);
            reader = package.NewReader();
            if (mode == "B") reader.Entries = reader.Entries.Where(entry => entry.LogicalPath != AssetPath).ToArray();
            if (mode == "C") reader.SecondThemeBytes = [0];
            OperationCanceledException canceled = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await verifier.VerifyAsync(request, reader, source.Token));
            Assert.Equal(source.Token, canceled.CancellationToken);
            probe.GetField("Terminal")!.SetValue(null, null);
        }

        ThemeIntegrityDiagnosticV1 duplicate = Diagnostic("P4I005", null, "a");
        ThemeIntegrityDiagnosticV1[] input =
        [Diagnostic("P4I008", null, "a"), Diagnostic("P4I005", "a", "z"), Diagnostic("P4I005", null, "z"),
            duplicate, Diagnostic("P4I005", "A", "z"), duplicate, Diagnostic("P4I005", "a", "a")];
        probe.GetField("Diagnostics")!.SetValue(null, (Action<List<ThemeIntegrityDiagnosticV1>>)(items => items.AddRange(input)));
        Package unsigned = CreatePackage("beta");
        ThemeIntegrityVerificationResultV2 composed = await verifier.VerifyAsync(unsigned.Request, unsigned.NewReader());
        AssertFields(composed, true, unsigned.Hashes, unsigned.Evidence, Status.NotRequired, null, null,
            [duplicate, duplicate, input[2], input[4], input[6], input[1], input[0]]);
        Assert.Same(duplicate, composed.Diagnostics[0]);
        Assert.Same(duplicate, composed.Diagnostics[1]);
        MethodInfo comparison = typeof(ThemeIntegrityVerifier).GetMethod("CompareDiagnostics", BindingFlags.NonPublic | BindingFlags.Static)!;
        Assert.Equal(0, comparison.Invoke(null,
            [duplicate, new ThemeIntegrityDiagnosticV1("P4I005", ThemeDiagnosticsSeverity.Warning, null, "a")]));
    }

    private static async Task<Assembly> BuildExternalCompositionHarness()
    {
        string root = Path.Combine(Path.GetTempPath(), "Tcc-Phase4E-49", "composition-harness-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string source = File.ReadAllText(Path.Combine(RepositoryPaths.Root, "src", "Tcc.Themes", "Integrity", "ThemeIntegrityVerifier.cs"));
        source = ReplaceOnce(source, "namespace Tcc.Themes.Integrity;", "using Tcc.Themes.Integrity;\nnamespace Tcc.Phase4E.Instrumented;");
        source = ReplaceOnce(source, "bool accepted =", "Probe.Enter?.Invoke(\"A\");\n        bool accepted =");
        source = ReplaceOnce(source, "ThemePackageInventoryEvaluation inventory =", "Probe.Enter?.Invoke(\"B\");\n            ThemePackageInventoryEvaluation inventory =");
        source = ReplaceOnce(source, "metadata = await", "Probe.Enter?.Invoke(\"C\");\n                metadata = await");
        source = ReplaceOnce(source, "signature = ThemePackageSignatureEvaluator.Evaluate(", "Probe.Enter?.Invoke(\"D\");\n                    signature = ThemePackageSignatureEvaluator.Evaluate(");
        source = ReplaceOnce(source, "diagnostics.Sort(", "Probe.Diagnostics?.Invoke(diagnostics);\n        diagnostics.Sort(");
        source = ReplaceOnce(source, "cancellationToken.ThrowIfCancellationRequested();", "Probe.Terminal?.Invoke();\n        cancellationToken.ThrowIfCancellationRequested();");
        File.WriteAllText(Path.Combine(root, "VerifierCopy.cs"), source);
        File.WriteAllText(Path.Combine(root, "Probe.cs"), """
            using Tcc.Presentation.Contracts.Theme;
            namespace Tcc.Phase4E.Instrumented;
            public static class Probe
            {
                public static Action<string>? Enter;
                public static Action? Terminal;
                public static Action<List<ThemeIntegrityDiagnosticV1>>? Diagnostics;
            }
            """);
        XDocument project = new(new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"),
            new XElement("PropertyGroup", new XElement("TargetFramework", "net10.0-windows"),
                new XElement("AssemblyName", "Tcc.Architecture.Tests"), new XElement("Nullable", "enable"),
                new XElement("ImplicitUsings", "enable"), new XElement("TreatWarningsAsErrors", "true")),
            new XElement("ItemGroup", new[] { typeof(ThemeIntegrityVerifier).Assembly, typeof(IThemeIntegrityVerifierV2).Assembly }
                .Select(assembly => new XElement("Reference", new XAttribute("Include", assembly.GetName().Name!), new XElement("HintPath", assembly.Location))))));
        project.Save(Path.Combine(root, "Harness.csproj"));
        ProcessStartInfo start = new("dotnet") { WorkingDirectory = root, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
        foreach (string argument in new[] { "build", "Harness.csproj", "-c", "Release", "-m:1", "--nologo" }) start.ArgumentList.Add(argument);
        using Process process = Process.Start(start)!;
        Task<string> stdout = process.StandardOutput.ReadToEndAsync();
        Task<string> stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        string output = await stdout + await stderr;
        File.WriteAllText(Path.Combine(root, "build.log"), output);
        Assert.True(process.ExitCode == 0, output);
        return Assembly.Load(File.ReadAllBytes(Path.Combine(root, "bin", "Release", "net10.0-windows", "Tcc.Architecture.Tests.dll")));
    }

    private static string ReplaceOnce(string source, string oldValue, string newValue)
    {
        int index = source.IndexOf(oldValue, StringComparison.Ordinal);
        Assert.True(index >= 0 && source.IndexOf(oldValue, index + oldValue.Length, StringComparison.Ordinal) < 0,
            "Source-copy instrumentation requires exactly one explicit production location.");
        return source.Replace(oldValue, newValue, StringComparison.Ordinal);
    }

    private static void AssertFields(ThemeIntegrityVerificationResultV2 actual, bool verified, string[]? hashes,
        IReadOnlyList<ThemeIntegrityFileEvidenceV1> evidence, Status status, string? publisher, string? key,
        IReadOnlyList<ThemeIntegrityDiagnosticV1> diagnostics)
    {
        Assert.Equal(verified, actual.IsVerified);
        Assert.Equal(hashes?[0], actual.PackageHash);
        Assert.Equal(hashes?[1], actual.ThemeManifestHash);
        Assert.Equal(hashes?[2], actual.IntegrityManifestHash);
        Assert.Equal(evidence.ToArray(), actual.FileEvidence.ToArray());
        Assert.Equal(evidence.Where(item => item.CanonicalPath.StartsWith("assets/", StringComparison.Ordinal)).ToArray(), actual.AssetEvidence.ToArray());
        Assert.Equal(status, actual.SignatureStatus);
        Assert.Equal(publisher, actual.PublisherId);
        Assert.Equal(key, actual.KeyId);
        Assert.Equal(diagnostics.ToArray(), actual.Diagnostics.ToArray());
        foreach (ThemeIntegrityFileEvidenceV1 asset in actual.AssetEvidence)
            Assert.Same(actual.FileEvidence.Single(item => item.CanonicalPath == asset.CanonicalPath), asset);
    }

    private static void AssertReads(Reader reader, Package package, bool signed)
    {
        Assert.Equal(1, reader.Enumerations);
        foreach (ThemeIntegrityFileEvidenceV1 item in package.Evidence)
            Assert.Equal(item.IsPresent ? item.CanonicalPath == package.Request.ThemeManifestPath.Value ? 2 : 1 : 0, reader.Count(item.CanonicalPath));
        Assert.Equal(1, reader.Count(IntegrityPath));
        Assert.Equal(signed ? 1 : 0, reader.Count(EnvelopePath));
        Assert.Equal(package.Evidence.Count(item => item.IsPresent) + (signed ? 3 : 2), reader.ReadCounts.Values.Sum());
    }

    private static Package FailurePackage(string mode)
    {
        Package package = CreatePackage(mode == "missing" ? "beta" : "signed");
        ThemeIntegrityVerificationRequestV2 request = package.Request;
        ThemeTrustedSignerV1? signer = request.TrustSnapshot.TrustedSigners.FirstOrDefault();
        switch (mode)
        {
            case "missing": request = request with { Policy = request.Policy with { SignatureRequirement = ThemeSignatureRequirement.Required } }; break;
            case "invalid":
                request = request with { SignatureEnvelope = request.SignatureEnvelope! with { Signature = Convert.ToBase64String(new byte[64]) } };
                package.Bytes[EnvelopePath] = Json(request.SignatureEnvelope);
                break;
            case "unknown": request = request with { TrustSnapshot = request.TrustSnapshot with { TrustedSigners = [] } }; break;
            case "revoked": request = request with { TrustSnapshot = request.TrustSnapshot with { TrustedSigners = [signer! with { TrustState = ThemeTrustState.Revoked }] } }; break;
            case "profile":
                byte[] spki = Convert.FromBase64String(signer!.PublicKey); spki[22] = 2; // named curve OID, same DER size
                request = request with { TrustSnapshot = request.TrustSnapshot with { TrustedSigners = [signer with { PublicKey = Convert.ToBase64String(spki) }] } }; break;
            case "policy": request = request with { Policy = request.Policy with { ExpectedTrustPolicyId = "other" } }; break;
            case "duplicate": request = request with { TrustSnapshot = request.TrustSnapshot with { TrustedSigners = [signer!, signer!] } }; break;
            case "identity": request = request with { TrustSnapshot = request.TrustSnapshot with { TrustedSigners = [signer! with { KeyId = "bad\u0000key" }] } }; break;
        }
        return package with { Request = request };
    }

    private static Package CreatePackage(string mode = "signed", string assetPath = AssetPath, string themePath = ThemePath, bool missingOptional = false)
    {
        byte[] themeBytes = Json(CreateTheme());
        byte[] payload = [1, 2, 3];
        Dictionary<string, byte[]> bytes = new(StringComparer.Ordinal) { [themePath] = themeBytes };
        if (!missingOptional) bytes[assetPath] = payload;
        ThemeIntegrityFileV2[] files = [new(themePath, ThemeIntegrityEntryKind.ThemeManifest, Hash(themeBytes), themeBytes.Length, true),
            new(assetPath, ThemeIntegrityEntryKind.Payload, Hash(payload), payload.Length, !missingOptional)];
        // Independent construction from raw fixture bytes, never from B/C/D or the public result.
        string records = string.Concat(files.Where(file => bytes.ContainsKey(file.CanonicalPath)).OrderBy(file => file.CanonicalPath, StringComparer.Ordinal)
            .Select(file => file.CanonicalPath + "\0" + bytes[file.CanonicalPath].Length.ToString(CultureInfo.InvariantCulture) + "\0" + Hash(bytes[file.CanonicalPath]) + "\n"));
        string packageHash = Hash(Encoding.UTF8.GetBytes(records));
        ThemeIntegrityManifestV2 integrity = new(ContractVersions.ThemeIntegritySchemaV2, ThemeIntegrityInventoryMode.ExhaustiveAllowedPayload,
            "com.example.theme", "1.2.3", "sha256", files, packageHash);
        byte[] integrityBytes = Json(integrity);
        bytes[IntegrityPath] = integrityBytes;
        string[] hashes = [packageHash, Hash(themeBytes), Hash(integrityBytes)];
        byte[] signedPayload = Encoding.UTF8.GetBytes("TCC-THEME-PACKAGE-SIGNATURE-V1\0com.example.theme\0" + "1.2.3\0"
            + string.Join('\0', hashes) + "\0example-publisher\0test-key");
        ThemeSignatureEnvelopeV1? envelope = null;
        ImmutableArray<ThemeTrustedSignerV1> signers = [];
        if (mode == "signed")
        {
            using ECDsa key = ECDsa.Create(new ECParameters { Curve = ECCurve.NamedCurves.nistP256,
                Q = new ECPoint { X = Convert.FromHexString(GeneratorX), Y = Convert.FromHexString(GeneratorY) },
                D = Convert.FromHexString(new string('0', 63) + "1") });
            envelope = new("1.0", ThemeSignatureAlgorithm.EcdsaP256Sha256, "example-publisher", "test-key",
                ThemeSignatureEncoding.IeeeP1363FixedFieldConcatenation,
                Convert.ToBase64String(key.SignData(signedPayload, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation)),
                ThemeSignedPayloadType.TccThemePackageSignatureV1);
            bytes[EnvelopePath] = Json(envelope);
            signers = [new("example-publisher", "test-key", ThemeSignatureAlgorithm.EcdsaP256Sha256,
                ThemePublicKeyEncoding.SubjectPublicKeyInfo, Convert.ToBase64String(key.ExportSubjectPublicKeyInfo()), ThemeTrustState.Trusted)];
        }
        ThemeIntegrityVerificationRequestV2 request = new(new("com.example.theme"), new("1.2.3"), new("opaque:composition-test"),
            ThemeCanonicalPath.Parse(themePath), ThemeCanonicalPath.Parse(IntegrityPath), envelope is null ? null : ThemeCanonicalPath.Parse(EnvelopePath), integrity, envelope,
            new("policy", "1", "trust", "1", mode == "signed" ? ThemeDistributionChannel.Stable : mode == "beta" ? ThemeDistributionChannel.Beta : ThemeDistributionChannel.Developer,
                mode == "signed" ? ThemeSignatureRequirement.Required : ThemeSignatureRequirement.Optional, mode == "developer"), new("trust", "1", signers));
        ThemeIntegrityFileEvidenceV1[] evidence = files.OrderBy(file => file.CanonicalPath, StringComparer.Ordinal).Select(file =>
            new ThemeIntegrityFileEvidenceV1(file.CanonicalPath, file.EntryKind, file.Required, bytes.ContainsKey(file.CanonicalPath), file.LengthBytes,
                bytes.ContainsKey(file.CanonicalPath) ? file.LengthBytes : null, file.Sha256, bytes.ContainsKey(file.CanonicalPath) ? file.Sha256 : null,
                bytes.ContainsKey(file.CanonicalPath) ? EvidenceStatus.Verified : EvidenceStatus.MissingAllowed)).ToArray();
        return new(request, bytes, evidence, hashes, signedPayload);
    }

    private static ThemeManifest CreateTheme() => new(ContractVersions.Schema, ContractVersions.ThemeApi,
        new("com.example.theme", "com.example.theme.package", "Example Theme", "Example Publisher", "example-publisher", "1.2.3", "stable", "Presentation-only example theme.", null, null, "private", ["deep"]),
        new(">=1.0.0 <2.0.0", ">=1.0.0 <2.0.0", ">=1.1.0 <2.0.0", ["windows"], true, 1m, 3m),
        [new("deep", "deep", "Deep", true, "tokens/deep.json", [], null)], ["presentation.tokens"], new([]),
        new(true, ["none", "reduced_decoration", "minimal_decoration", "safe_presentation_only"],
            ["safety_core", "critical_alerts", "keyboard", "screen_reader", "contrast", "risk_permission_semantics", "confirmation_semantics", "accessibility"], "safe_presentation_only"),
        new(true, true, true, true, true, true, true, true, true, true, true, true), new("assets/index.json", ["tier0"], 0, "sha256"), null,
        new("motion/profiles.json", "motion/reduced.json", true, true, true), null, new("integrity.json", "signature.sig", "stable_or_store", "sha256"), null);

    private static byte[] Json<T>(T value) => JsonSerializer.SerializeToUtf8Bytes(value, ThemeContractJson.CreateSerializerOptions());
    private static string Hash(byte[] value) => Convert.ToHexStringLower(SHA256.HashData(value));
    private static string Serialize(ThemeIntegrityVerificationResultV2 result) => JsonSerializer.Serialize(result, ThemeContractJson.CreateSerializerOptions());
    private static ThemeIntegrityDiagnosticV1 Diagnostic(string code, string? path, string message) => new(code, ThemeDiagnosticsSeverity.Error, path, message);
    private sealed record Package(ThemeIntegrityVerificationRequestV2 Request, Dictionary<string, byte[]> Bytes,
        ThemeIntegrityFileEvidenceV1[] Evidence, string[] Hashes, byte[] SignedPayload)
    {
        public Reader NewReader() => new(Bytes);
    }

    private sealed class Reader : IThemePackageContentReader
    {
        private readonly Dictionary<string, byte[]> bytes;
        private int enumerations;
        public Reader(Dictionary<string, byte[]> content)
        {
            bytes = content.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray(), StringComparer.Ordinal);
            Entries = bytes.Select(pair => new ThemePackageContentEntryV1(pair.Key, pair.Value.Length, ThemePackageContentEntryKind.File, null)).ToArray();
        }
        public IReadOnlyList<ThemePackageContentEntryV1> Entries { get; set; }
        public ConcurrentDictionary<string, int> ReadCounts { get; } = new(StringComparer.Ordinal);
        public ConcurrentQueue<CancellationToken> Tokens { get; } = new();
        public ConcurrentQueue<ThemePackageRef> PackageRefs { get; } = new();
        public Dictionary<string, Exception> Failures { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, byte[]> Replacements { get; } = new(StringComparer.Ordinal);
        public byte[]? SecondThemeBytes { get; set; }
        public string? SuspendAt { get; set; }
        public TaskCompletionSource Suspended { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int Enumerations => Volatile.Read(ref enumerations);
        public int Count(string path) => ReadCounts.GetValueOrDefault(path);
        public async ValueTask<IReadOnlyList<ThemePackageContentEntryV1>> EnumerateEntriesAsync(ThemePackageRef packageRef, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref enumerations); Tokens.Enqueue(cancellationToken); PackageRefs.Enqueue(packageRef);
            await Observe("enumeration", cancellationToken);
            return Entries;
        }
        public async ValueTask<ReadOnlyMemory<byte>> ReadContentAsync(ThemePackageRef packageRef, string canonicalPath, CancellationToken cancellationToken = default)
        {
            int count = ReadCounts.AddOrUpdate(canonicalPath, 1, (_, previous) => previous + 1);
            Tokens.Enqueue(cancellationToken); PackageRefs.Enqueue(packageRef);
            string location = canonicalPath == ThemePath && count == 2 ? "theme-second" : canonicalPath;
            await Observe(location, cancellationToken);
            if (canonicalPath == ThemePath && count == 2 && SecondThemeBytes is not null) return SecondThemeBytes;
            if (Replacements.TryGetValue(canonicalPath, out byte[]? replacement)) return replacement;
            return bytes.TryGetValue(canonicalPath, out byte[]? value) ? value : throw new FileNotFoundException();
        }
        private async Task Observe(string location, CancellationToken token)
        {
            if (SuspendAt == location)
            {
                Suspended.TrySetResult();
                await Task.Delay(Timeout.Infinite, token);
            }
            if (Failures.TryGetValue(location, out Exception? failure)) throw failure;
        }
    }
}
