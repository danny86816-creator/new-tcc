using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Integrity;

namespace Tcc.Architecture.Tests;

public sealed class PhaseFourThemeIntegrityVerifierTests
{
    [Fact]
    public void ValidOuterInputIsAcceptedForTheNextStageWithoutProducingAPackageVerdict()
    {
        RecordingContentReader reader = new();

        bool accepted = ThemeIntegrityRequestBoundary.TryAcceptForNextStage(
            CreateValidRequest(),
            reader,
            CancellationToken.None,
            out IReadOnlyList<ThemeIntegrityDiagnosticV1> diagnostics);

        Assert.True(accepted);
        Assert.Empty(diagnostics);
        Assert.Equal(0, reader.CallCount);
        MethodInfo method = typeof(ThemeIntegrityRequestBoundary).GetMethod(
            "TryAcceptForNextStage",
            BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("The preflight entry point was not found.");
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.DoesNotContain(
            typeof(ThemeIntegrityRequestBoundary).GetMethods(BindingFlags.Static | BindingFlags.NonPublic),
            candidate => candidate.ReturnType == typeof(ThemeIntegrityVerificationResultV2));
    }

    [Fact]
    public void NullRequestAndNullReaderAreRejectedWithTheApprovedMissingFieldDiagnostic()
    {
        AssertRejected(
            null,
            new RecordingContentReader(),
            ThemeIntegrityDiagnosticCodes.MissingSecurityField);
        AssertRejected(
            CreateValidRequest(),
            null,
            ThemeIntegrityDiagnosticCodes.MissingSecurityField);
    }

    [Fact]
    public void NullRequiredNestedObjectsAndCollectionItemsAreRejectedDeterministically()
    {
        ThemeIntegrityVerificationRequestV2 valid = CreateValidRequest();
        ThemeIntegrityVerificationRequestV2[] invalidRequests =
        [
            valid with { IntegrityManifest = null! },
            valid with { Policy = null! },
            valid with { TrustSnapshot = null! },
            valid with { ThemeManifestPath = null! },
            valid with { IntegrityManifestPath = null! },
            valid with { PackageRef = new ThemePackageRef(string.Empty) },
            valid with { SignatureEnvelope = CreateEnvelope(), SignatureEnvelopePath = null },
            valid with { IntegrityManifest = valid.IntegrityManifest with { Files = [null!] } },
            valid with
            {
                TrustSnapshot = valid.TrustSnapshot with { TrustedSigners = default },
            },
            valid with
            {
                TrustSnapshot = valid.TrustSnapshot with
                {
                    TrustedSigners = [null!],
                },
            },
        ];

        foreach (ThemeIntegrityVerificationRequestV2 invalid in invalidRequests)
        {
            AssertRejected(invalid, new RecordingContentReader(), ThemeIntegrityDiagnosticCodes.MissingSecurityField);
        }

        bool firstAccepted = ThemeIntegrityRequestBoundary.TryAcceptForNextStage(
            invalidRequests[^1],
            new RecordingContentReader(),
            CancellationToken.None,
            out IReadOnlyList<ThemeIntegrityDiagnosticV1> first);
        Assert.False(firstAccepted);
        for (int iteration = 0; iteration < 20; iteration++)
        {
            bool repeatedAccepted = ThemeIntegrityRequestBoundary.TryAcceptForNextStage(
                invalidRequests[^1],
                new RecordingContentReader(),
                CancellationToken.None,
                out IReadOnlyList<ThemeIntegrityDiagnosticV1> repeated);
            Assert.Equal(firstAccepted, repeatedAccepted);
            Assert.Equal(first, repeated);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\r\n")]
    public void PresentEnvelopeWithBlankSignatureRejectsWithExactInvalidSignatureForFiftyRuns(string? signature)
    {
        ThemeIntegrityVerificationRequestV2 request = CreateValidRequest() with
        {
            SignatureEnvelopePath = ThemeCanonicalPath.Parse("signature.sig"),
            SignatureEnvelope = CreateEnvelope() with { Signature = signature! },
        };

        AssertExactRejectionForFiftyRuns(request, ["P4I013"]);
    }

    [Fact]
    public void MissingRequiredObjectRetainsExactMissingSecurityFieldForFiftyRuns()
    {
        ThemeIntegrityVerificationRequestV2 request = CreateValidRequest() with
        {
            Policy = null!,
        };

        AssertExactRejectionForFiftyRuns(request, ["P4I020"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\r\n")]
    public void BlankSignatureAndMissingRequiredObjectHaveExactOrderedDiagnosticsForFiftyRuns(string? signature)
    {
        ThemeIntegrityVerificationRequestV2 request = CreateValidRequest() with
        {
            SignatureEnvelopePath = ThemeCanonicalPath.Parse("signature.sig"),
            SignatureEnvelope = CreateEnvelope() with { Signature = signature! },
            Policy = null!,
        };

        AssertExactRejectionForFiftyRuns(request, ["P4I013", "P4I020"]);
    }

    [Fact]
    public void UndefinedSecurityEnumsAreRejectedAtPreflight()
    {
        ThemeIntegrityVerificationRequestV2 valid = CreateValidRequest();
        ThemeTrustedSignerV1 signer = CreateSigner();
        ThemeSignatureEnvelopeV1 envelope = CreateEnvelope();
        ThemeIntegrityVerificationRequestV2[] invalidRequests =
        [
            valid with { IntegrityManifest = valid.IntegrityManifest with { InventoryMode = (ThemeIntegrityInventoryMode)999 } },
            valid with
            {
                IntegrityManifest = valid.IntegrityManifest with
                {
                    Files = [valid.IntegrityManifest.Files[0] with { EntryKind = (ThemeIntegrityEntryKind)999 }],
                },
            },
            valid with { Policy = valid.Policy with { DistributionChannel = (ThemeDistributionChannel)999 } },
            valid with { Policy = valid.Policy with { SignatureRequirement = (ThemeSignatureRequirement)999 } },
            valid with
            {
                TrustSnapshot = valid.TrustSnapshot with
                {
                    TrustedSigners = [signer with { TrustState = (ThemeTrustState)999 }],
                },
            },
            valid with
            {
                TrustSnapshot = valid.TrustSnapshot with
                {
                    TrustedSigners = [signer with { Algorithm = (ThemeSignatureAlgorithm)999 }],
                },
            },
            valid with
            {
                TrustSnapshot = valid.TrustSnapshot with
                {
                    TrustedSigners = [signer with { PublicKeyEncoding = (ThemePublicKeyEncoding)999 }],
                },
            },
            valid with
            {
                SignatureEnvelopePath = ThemeCanonicalPath.Parse("signature.sig"),
                SignatureEnvelope = envelope with { SignedPayloadType = (ThemeSignedPayloadType)999 },
            },
        ];

        foreach (ThemeIntegrityVerificationRequestV2 invalid in invalidRequests)
        {
            AssertRejected(invalid, new RecordingContentReader(), ThemeIntegrityDiagnosticCodes.InvalidSecurityValue);
        }

        AssertRejected(
            valid with
            {
                SignatureEnvelopePath = ThemeCanonicalPath.Parse("signature.sig"),
                SignatureEnvelope = envelope with { Algorithm = (ThemeSignatureAlgorithm)999 },
            },
            new RecordingContentReader(),
            ThemeIntegrityDiagnosticCodes.UnsupportedSignatureAlgorithm);
        AssertRejected(
            valid with
            {
                SignatureEnvelopePath = ThemeCanonicalPath.Parse("signature.sig"),
                SignatureEnvelope = envelope with { SignatureEncoding = (ThemeSignatureEncoding)999 },
            },
            new RecordingContentReader(),
            ThemeIntegrityDiagnosticCodes.UnsupportedSignatureEncoding);
    }

    [Theory]
    [InlineData("Com.Example.Theme", "1.2.3")]
    [InlineData("com_example_theme", "1.2.3")]
    [InlineData("com.example.theme", "01.2.3")]
    [InlineData("com.example.theme", "1.2")]
    public void MalformedBasicIdentityOrVersionIsRejected(string themeId, string version)
    {
        ThemeIntegrityVerificationRequestV2 valid = CreateValidRequest();
        ThemeIntegrityVerificationRequestV2 invalid = valid with
        {
            ThemeId = new ThemeId(themeId),
            Version = new ThemeVersion(version),
        };

        AssertRejected(invalid, new RecordingContentReader(), ThemeIntegrityDiagnosticCodes.InvalidSecurityValue);
    }

    [Fact]
    public void PreCancelledTokenPropagatesBeforeAnyBoundaryDecisionOrReaderCall()
    {
        RecordingContentReader reader = new();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            ThemeIntegrityRequestBoundary.TryAcceptForNextStage(
                CreateValidRequest(),
                reader,
                cancellation.Token,
                out _));
        Assert.Equal(0, reader.CallCount);
    }

    [Fact]
    public void PackageReferenceRemainsOpaqueAndTheReaderIsNotInvokedDuringPreflight()
    {
        RecordingContentReader reader = new();
        ThemeIntegrityVerificationRequestV2 request = CreateValidRequest();
        string[] relocatedReferences =
        [
            "transport:original/source",
            "transport:relocated/source",
            "path-like:../../opaque-provenance",
        ];

        foreach (string packageRef in relocatedReferences)
        {
            bool accepted = ThemeIntegrityRequestBoundary.TryAcceptForNextStage(
                request with { PackageRef = new ThemePackageRef(packageRef) },
                reader,
                CancellationToken.None,
                out IReadOnlyList<ThemeIntegrityDiagnosticV1> diagnostics);
            Assert.True(accepted);
            Assert.Empty(diagnostics);
        }

        Assert.Equal(0, reader.CallCount);
    }

    [Fact]
    public void TestFriendAccessIsNarrowAndProductionHasNoVerifierOrOracleDependency()
    {
        Assembly themesAssembly = typeof(Tcc.Themes.AssemblyMarker).Assembly;
        string[] friendAssemblies = themesAssembly
            .GetCustomAttributes<InternalsVisibleToAttribute>()
            .Select(attribute => attribute.AssemblyName)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["Tcc.Architecture.Tests"], friendAssemblies);
        Assert.False(typeof(ThemeIntegrityRequestBoundary).IsPublic);
        Assert.True(typeof(ThemeIntegrityRequestBoundary).IsAbstract && typeof(ThemeIntegrityRequestBoundary).IsSealed);
        Assert.Empty(typeof(ThemeIntegrityRequestBoundary).GetInterfaces());

        Type[] productionTypes = themesAssembly.GetTypes();
        Assert.DoesNotContain(
            productionTypes,
            type => type.IsClass && !type.IsAbstract && typeof(IThemeIntegrityVerifierV2).IsAssignableFrom(type));
        Assert.DoesNotContain(
            productionTypes,
            type => type.IsClass && !type.IsAbstract && typeof(IThemeIntegrityVerifier).IsAssignableFrom(type));
        Assert.DoesNotContain(productionTypes, type => type.Name == "ThemeIntegrityVerifier");
        Assert.DoesNotContain(
            themesAssembly.GetReferencedAssemblies(),
            reference => reference.Name == typeof(ThemeIntegrityContractConformanceOracle).Assembly.GetName().Name);
    }

    [Fact]
    public void ProjectMetadataContainsOnlyTheAuthorizedFriendAndExistingDependencies()
    {
        string themesProject = Path.Combine(RepositoryPaths.Root, "src", "Tcc.Themes", "Tcc.Themes.csproj");
        XDocument project = XDocument.Load(themesProject, LoadOptions.None);

        string[] friends = project.Descendants()
            .Where(element => element.Name.LocalName == "InternalsVisibleTo")
            .Select(element => element.Attribute("Include")?.Value)
            .OfType<string>()
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] projectReferences = project.Descendants()
            .Where(element => element.Name.LocalName == "ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .OfType<string>()
            .ToArray();

        Assert.Equal(["Tcc.Architecture.Tests"], friends);
        Assert.Equal(["../Tcc.Presentation.Contracts/Tcc.Presentation.Contracts.csproj"], projectReferences);
        Assert.DoesNotContain(
            project.Descendants(),
            element => element.Name.LocalName == "PackageReference");
        Assert.Equal(
            6,
            Directory.EnumerateFiles(RepositoryPaths.Root, "*.csproj", SearchOption.AllDirectories)
                .Count(path => !path.Split(Path.DirectorySeparatorChar).Any(segment => segment is "bin" or "obj")));
    }

    private static void AssertExactRejectionForFiftyRuns(
        ThemeIntegrityVerificationRequestV2 request,
        string[] expectedCodes)
    {
        IReadOnlyList<ThemeIntegrityDiagnosticV1>? first = null;
        RecordingContentReader reader = new();
        for (int iteration = 0; iteration < 50; iteration++)
        {
            bool accepted = ThemeIntegrityRequestBoundary.TryAcceptForNextStage(
                request,
                reader,
                CancellationToken.None,
                out IReadOnlyList<ThemeIntegrityDiagnosticV1> diagnostics);

            Assert.False(accepted);
            Assert.Equal(expectedCodes, diagnostics.Select(diagnostic => diagnostic.Code));
            Assert.All(diagnostics, diagnostic =>
            {
                Assert.Equal(ThemeDiagnosticsSeverity.Error, diagnostic.Severity);
                Assert.Null(diagnostic.CanonicalPath);
            });
            if (first is not null)
            {
                Assert.Equal(first, diagnostics);
            }

            first ??= diagnostics;
        }

        Assert.Equal(0, reader.CallCount);
    }

    private static void AssertRejected(
        ThemeIntegrityVerificationRequestV2? request,
        IThemePackageContentReader? reader,
        string expectedCode)
    {
        bool accepted = ThemeIntegrityRequestBoundary.TryAcceptForNextStage(
            request,
            reader,
            CancellationToken.None,
            out IReadOnlyList<ThemeIntegrityDiagnosticV1> diagnostics);

        Assert.False(accepted);
        Assert.Contains(expectedCode, diagnostics.Select(diagnostic => diagnostic.Code));
        Assert.All(diagnostics, diagnostic => Assert.Equal(ThemeDiagnosticsSeverity.Error, diagnostic.Severity));
        Assert.Equal(
            diagnostics,
            diagnostics
                .OrderBy(diagnostic => diagnostic.Code, StringComparer.Ordinal)
                .ThenBy(diagnostic => diagnostic.CanonicalPath, StringComparer.Ordinal)
                .ThenBy(diagnostic => diagnostic.Message, StringComparer.Ordinal));
    }

    private static ThemeIntegrityVerificationRequestV2 CreateValidRequest() => new(
        new ThemeId("com.example.theme"),
        new ThemeVersion("1.2.3"),
        new ThemePackageRef("transport:original/source"),
        ThemeCanonicalPath.Parse("theme.json"),
        ThemeCanonicalPath.Parse("integrity.json"),
        null,
        new ThemeIntegrityManifestV2(
            ContractVersions.ThemeIntegritySchemaV2,
            ThemeIntegrityInventoryMode.ExhaustiveAllowedPayload,
            "com.example.theme",
            "1.2.3",
            ThemeIntegrityContractSemantics.HashAlgorithm,
            [new ThemeIntegrityFileV2("theme.json", ThemeIntegrityEntryKind.ThemeManifest, new string('a', 64), 42, true)],
            new string('b', 64)),
        null,
        new ThemeIntegrityVerificationPolicyV1(
            "beta-channel-policy",
            "1",
            "default-trust",
            "1",
            ThemeDistributionChannel.Beta,
            ThemeSignatureRequirement.Optional,
            false),
        new ThemeTrustSnapshotV1("default-trust", "1", []));

    private static ThemeSignatureEnvelopeV1 CreateEnvelope() => new(
        ContractVersions.ThemeSignatureEnvelopeSchemaV1,
        ThemeSignatureAlgorithm.EcdsaP256Sha256,
        "publisher.example",
        "key-2026-01",
        ThemeSignatureEncoding.IeeeP1363FixedFieldConcatenation,
        Convert.ToBase64String(new byte[64]),
        ThemeSignedPayloadType.TccThemePackageSignatureV1);

    private static ThemeTrustedSignerV1 CreateSigner() => new(
        "publisher.example",
        "key-2026-01",
        ThemeSignatureAlgorithm.EcdsaP256Sha256,
        ThemePublicKeyEncoding.SubjectPublicKeyInfo,
        "opaque-public-key-at-preflight",
        ThemeTrustState.Trusted);

    private sealed class RecordingContentReader : IThemePackageContentReader
    {
        public int CallCount { get; private set; }

        public ValueTask<IReadOnlyList<ThemePackageContentEntryV1>> EnumerateEntriesAsync(
            ThemePackageRef packageRef,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            throw new InvalidOperationException("Phase 4A preflight must not enumerate package entries.");
        }

        public ValueTask<ReadOnlyMemory<byte>> ReadContentAsync(
            ThemePackageRef packageRef,
            string canonicalPath,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            throw new InvalidOperationException("Phase 4A preflight must not read package content.");
        }
    }
}
