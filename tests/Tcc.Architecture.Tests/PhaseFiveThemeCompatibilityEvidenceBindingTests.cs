using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Compatibility.Binding;

namespace Tcc.Architecture.Tests;

public sealed class PhaseFiveThemeCompatibilityEvidenceBindingTests
{
    private const string ThemePath = "theme.json";
    private const string CompatibilityPath = "compatibility.json";
    private const string IntegrityPath = "integrity.json";

    [Fact]
    public async Task SnapshotEnumeratesAndCapturesCanonicalFilesOnceAndDefensively()
    {
        byte[] original = [1, 2, 3];
        CountingReader source = new(
            [
                new("a.bin", 3, ThemePackageContentEntryKind.File, null),
                new("a.bin", 3, ThemePackageContentEntryKind.File, null),
                new("../invalid", 1, ThemePackageContentEntryKind.File, null),
                new("folder", 0, ThemePackageContentEntryKind.Directory, null),
            ],
            new Dictionary<string, byte[]>(StringComparer.Ordinal) { ["a.bin"] = original });

        ThemePackageRef packageRef = new("opaque:snapshot");
        ThemeCompatibilityContentSnapshot snapshot = await ThemeCompatibilityContentSnapshot.CaptureAsync(
            packageRef, source);
        original[0] = 9;
        IReadOnlyList<ThemePackageContentEntryV1> entries = await snapshot.EnumerateEntriesAsync(packageRef);
        ReadOnlyMemory<byte> first = await snapshot.ReadContentAsync(packageRef, "a.bin");
        byte[] exposed = first.ToArray();
        exposed[0] = 8;
        ReadOnlyMemory<byte> second = await snapshot.ReadContentAsync(packageRef, "a.bin");

        Assert.Equal(1, source.Enumerations);
        Assert.Equal(1, source.Count("a.bin"));
        Assert.Equal(0, source.Count("../invalid"));
        Assert.Equal(4, entries.Count);
        Assert.Equal(new byte[] { 1, 2, 3 }, first.ToArray());
        Assert.Equal(new byte[] { 1, 2, 3 }, second.ToArray());
    }

    [Fact]
    public async Task SnapshotCancellationPropagatesWithoutPartialSuccess()
    {
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await ThemeCompatibilityContentSnapshot.CaptureAsync(
                new ThemePackageRef("opaque:cancel"),
                new CountingReader([], new Dictionary<string, byte[]>(StringComparer.Ordinal)),
                cancellation.Token));
    }

    [Fact]
    public void MaterializerAcceptsOnlyStrictSupportedSchemaDocuments()
    {
        byte[] valid = Json(CreateCompatibility());
        Assert.NotNull(ThemeCompatibilityManifestMaterializer.Materialize(valid, out ImmutableArray<ThemeCompatibilityFailureV2> validFailures));
        Assert.Empty(validFailures);

        foreach ((string json, ThemeCompatibilityFailureKindV2 expected) in new[]
        {
            ("{", ThemeCompatibilityFailureKindV2.ManifestSchemaInvalid),
            ("/*x*/" + Encoding.UTF8.GetString(valid), ThemeCompatibilityFailureKindV2.ManifestSchemaInvalid),
            (Encoding.UTF8.GetString(valid)[..^1] + ",}", ThemeCompatibilityFailureKindV2.ManifestSchemaInvalid),
            (Encoding.UTF8.GetString(valid).Replace("\"schema_version\":\"1.0\"", "\"schema_version\":\"1.0\",\"schema_version\":\"1.0\"", StringComparison.Ordinal), ThemeCompatibilityFailureKindV2.ManifestSchemaInvalid),
            (Encoding.UTF8.GetString(valid).Replace("\"schema_version\":\"1.0\"", "\"schema_version\":\"2.0\"", StringComparison.Ordinal), ThemeCompatibilityFailureKindV2.UnsupportedManifestSchema),
            (Encoding.UTF8.GetString(valid).Replace("\"theme_id\":\"com.example.theme\",", string.Empty, StringComparison.Ordinal), ThemeCompatibilityFailureKindV2.ManifestSchemaInvalid),
            (Encoding.UTF8.GetString(valid).Replace("\"theme_id\":\"com.example.theme\"", "\"theme_id\":1", StringComparison.Ordinal), ThemeCompatibilityFailureKindV2.ManifestSchemaInvalid),
            (Encoding.UTF8.GetString(valid).Replace("\"theme_id\":\"com.example.theme\"", "\"theme_id\":\"com.example.theme\",\"unknown\":true", StringComparison.Ordinal), ThemeCompatibilityFailureKindV2.ManifestSchemaInvalid),
        })
        {
            Assert.Null(ThemeCompatibilityManifestMaterializer.Materialize(
                Encoding.UTF8.GetBytes(json), out ImmutableArray<ThemeCompatibilityFailureV2> failures));
            Assert.Equal(expected, Assert.Single(failures).Kind);
            Assert.Null(failures[0].DiagnosticCode);
        }
    }

    [Fact]
    public void CompatibilitySchemaResourceMatchesExactSourceBytesAndHash()
    {
        byte[] source = File.ReadAllBytes(Path.Combine(
            RepositoryPaths.Root, "contracts/theme/schemas/ThemeCompatibility.schema.json"));
        byte[] embedded = Tcc.Themes.Integrity.ThemeMetadataSchemaValidator.ReadSchemaResource(
            ThemeCompatibilityManifestMaterializer.SchemaResourceName);

        Assert.Equal("3278CC92A93AF636380739A34063F2A889E63AB89E0A56132F768836E126B2BC",
            Convert.ToHexString(SHA256.HashData(source)));
        Assert.Equal(source, embedded);
        Assert.Contains(ThemeCompatibilityManifestMaterializer.SchemaResourceName,
            typeof(Tcc.Themes.AssemblyMarker).Assembly.GetManifestResourceNames(), StringComparer.Ordinal);
    }

    [Fact]
    public async Task BinderUsesOneSourceSnapshotAndReturnsRealVerifiedSameContentContext()
    {
        Package package = CreatePackage();
        CountingReader reader = package.NewReader();

        Tcc.Themes.Compatibility.V2.ThemeCompatibilityContextV2 context =
            await ThemeCompatibilityEvidenceBinder.VerifyAndBindAsync(package.Request, reader);

        Assert.Empty(context.BindingFailures);
        Assert.True(context.Integrity!.IsVerified);
        Assert.Equal("com.example.theme", context.Manifest!.Package.ThemeId);
        Assert.Equal("com.example.theme", context.Compatibility!.ThemeId);
        Assert.Equal(Hash(package.Bytes[CompatibilityPath]), context.CompatibilityManifestHash);
        Assert.Equal(1, reader.Enumerations);
        Assert.All(package.Bytes.Keys, path => Assert.Equal(1, reader.Count(path)));
    }

    [Fact]
    public async Task BinderMapsMalformedCompatibilityWithoutReReadingTheSource()
    {
        Package package = CreatePackage(Encoding.UTF8.GetBytes("{}"));
        CountingReader reader = package.NewReader();

        Tcc.Themes.Compatibility.V2.ThemeCompatibilityContextV2 context =
            await ThemeCompatibilityEvidenceBinder.VerifyAndBindAsync(package.Request, reader);

        Assert.True(context.Integrity!.IsVerified);
        Assert.Null(context.Compatibility);
        Assert.Equal(ThemeCompatibilityFailureKindV2.ManifestSchemaInvalid,
            Assert.Single(context.BindingFailures).Kind);
        Assert.Equal(1, reader.Enumerations);
        Assert.All(package.Bytes.Keys, path => Assert.Equal(1, reader.Count(path)));
    }

    [Fact]
    public async Task BinderRejectsMissingAndDuplicatePackageEvidence()
    {
        Package missingPackage = CreatePackage();
        missingPackage.Bytes.Remove(CompatibilityPath);
        Tcc.Themes.Compatibility.V2.ThemeCompatibilityContextV2 missing =
            await ThemeCompatibilityEvidenceBinder.VerifyAndBindAsync(
                missingPackage.Request, missingPackage.NewReader());
        Assert.Equal(ThemeCompatibilityFailureKindV2.IntegrityNotVerified,
            Assert.Single(missing.BindingFailures).Kind);

        Package duplicatePackage = CreatePackage();
        ThemePackageContentEntryV1[] entries =
        [
            .. duplicatePackage.Bytes.Select(pair => new ThemePackageContentEntryV1(
                pair.Key, pair.Value.Length, ThemePackageContentEntryKind.File, null)),
            new(CompatibilityPath, duplicatePackage.Bytes[CompatibilityPath].Length,
                ThemePackageContentEntryKind.File, null),
        ];
        CountingReader duplicateReader = new(entries, duplicatePackage.Bytes);
        Tcc.Themes.Compatibility.V2.ThemeCompatibilityContextV2 duplicate =
            await ThemeCompatibilityEvidenceBinder.VerifyAndBindAsync(
                duplicatePackage.Request, duplicateReader);
        Assert.Equal(ThemeCompatibilityFailureKindV2.IntegrityNotVerified,
            Assert.Single(duplicate.BindingFailures).Kind);
        Assert.Equal(1, duplicateReader.Count(CompatibilityPath));
    }

    [Theory]
    [InlineData(ThemePath, false)]
    [InlineData(CompatibilityPath, false)]
    [InlineData(CompatibilityPath, true)]
    public async Task BinderRejectsThemeHashCompatibilityHashAndSizeMismatches(
        string path,
        bool changeLength)
    {
        Package package = CreatePackage();
        package.Bytes[path] = changeLength
            ? [.. package.Bytes[path], (byte)' ']
            : package.Bytes[path].Select((value, index) => index == 0 ? (byte)(value ^ 1) : value).ToArray();

        Tcc.Themes.Compatibility.V2.ThemeCompatibilityContextV2 context =
            await ThemeCompatibilityEvidenceBinder.VerifyAndBindAsync(package.Request, package.NewReader());

        Assert.Equal(ThemeCompatibilityFailureKindV2.IntegrityNotVerified,
            Assert.Single(context.BindingFailures).Kind);
    }

    [Fact]
    public async Task BinderRejectsCompatibilityIdentityMismatchAfterRealVerification()
    {
        ThemeCompatibilityManifest mismatched = CreateCompatibility() with
        {
            ThemeId = "com.example.other",
        };
        Package package = CreatePackage(Json(mismatched));

        Tcc.Themes.Compatibility.V2.ThemeCompatibilityContextV2 context =
            await ThemeCompatibilityEvidenceBinder.VerifyAndBindAsync(package.Request, package.NewReader());

        Assert.True(context.Integrity!.IsVerified);
        Assert.Equal(ThemeCompatibilityFailureKindV2.ContentEvidenceMismatch,
            Assert.Single(context.BindingFailures).Kind);
    }

    [Fact]
    public async Task BinderMapsSnapshotReadFailureAndPropagatesCancellation()
    {
        Package package = CreatePackage();
        CountingReader failed = package.NewReader();
        failed.Failures[CompatibilityPath] = new IOException("private source detail");
        Tcc.Themes.Compatibility.V2.ThemeCompatibilityContextV2 context =
            await ThemeCompatibilityEvidenceBinder.VerifyAndBindAsync(package.Request, failed);
        ThemeCompatibilityFailureV2 failure = Assert.Single(context.BindingFailures);
        Assert.Equal(ThemeCompatibilityFailureKindV2.ContentSnapshotUnavailable, failure.Kind);
        Assert.DoesNotContain("private", failure.Message, StringComparison.OrdinalIgnoreCase);

        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await ThemeCompatibilityEvidenceBinder.VerifyAndBindAsync(
                package.Request, package.NewReader(), cancellation.Token));
    }

    private static Package CreatePackage(byte[]? compatibilityOverride = null)
    {
        byte[] themeBytes = Json(CreateTheme());
        byte[] compatibilityBytes = compatibilityOverride ?? Json(CreateCompatibility());
        Dictionary<string, byte[]> bytes = new(StringComparer.Ordinal)
        {
            [ThemePath] = themeBytes,
            [CompatibilityPath] = compatibilityBytes,
        };
        ThemeIntegrityFileV2[] files =
        [
            new(ThemePath, ThemeIntegrityEntryKind.ThemeManifest, Hash(themeBytes), themeBytes.Length, true),
            new(CompatibilityPath, ThemeIntegrityEntryKind.Payload, Hash(compatibilityBytes), compatibilityBytes.Length, true),
        ];
        Array.Sort(files, (left, right) => StringComparer.Ordinal.Compare(left.CanonicalPath, right.CanonicalPath));
        StringBuilder records = new();
        foreach (ThemeIntegrityFileV2 file in files)
        {
            records.Append(file.CanonicalPath).Append('\0')
                .Append(bytes[file.CanonicalPath].Length.ToString(CultureInfo.InvariantCulture)).Append('\0')
                .Append(Hash(bytes[file.CanonicalPath])).Append('\n');
        }

        ThemeIntegrityManifestV2 integrity = new(
            ContractVersions.ThemeIntegritySchemaV2,
            ThemeIntegrityInventoryMode.ExhaustiveAllowedPayload,
            "com.example.theme",
            "1.2.3",
            "sha256",
            files,
            Hash(Encoding.UTF8.GetBytes(records.ToString())));
        bytes[IntegrityPath] = Json(integrity);
        ThemeIntegrityVerificationRequestV2 request = new(
            new("com.example.theme"),
            new("1.2.3"),
            new("opaque:compatibility-binding"),
            ThemeCanonicalPath.Parse(ThemePath),
            ThemeCanonicalPath.Parse(IntegrityPath),
            null,
            integrity,
            null,
            new("policy", "1", "trust", "1", ThemeDistributionChannel.Beta,
                ThemeSignatureRequirement.Optional, false),
            new("trust", "1", ImmutableArray<ThemeTrustedSignerV1>.Empty));
        return new Package(request, bytes);
    }

    private static ThemeCompatibilityManifest CreateCompatibility() => new(
        ContractVersions.Schema,
        "com.example.theme",
        "1.2.3",
        new(">=1.0.0 <2.0.0", ["1.0.0"]),
        new(">=1.0.0 <2.0.0", ["1.0.0"]),
        new(">=1.1.0 <2.0.0", ["1.1.0"]),
        new(true, true, true, ["100%", "150%"]),
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["deep"] = "pass_required",
            ["light"] = "pass_required",
            ["reduced_motion"] = "pass_required",
            ["reduced_transparency"] = "pass_required",
            ["high_contrast"] = "pass_required",
            ["color_vision"] = "pass_required",
            ["keyboard"] = "pass_required",
            ["screen_reader"] = "pass_required",
        });

    private static ThemeManifest CreateTheme() => new(
        ContractVersions.Schema,
        ContractVersions.ThemeApi,
        new("com.example.theme", "com.example.theme.package", "Example Theme", "Example Publisher",
            "example-publisher", "1.2.3", "beta", "Presentation-only example theme.", null, null,
            "private", ["deep"]),
        new(">=1.0.0 <2.0.0", ">=1.0.0 <2.0.0", ">=1.1.0 <2.0.0", ["windows"], true, 1m, 3m),
        [new("deep", "deep", "Deep", true, "tokens/deep.json", [], null)],
        ["presentation.tokens"],
        new([]),
        new(true, ["none", "reduced_decoration", "minimal_decoration", "safe_presentation_only"],
            ["safety_core", "critical_alerts", "keyboard", "screen_reader", "contrast",
                "risk_permission_semantics", "confirmation_semantics", "accessibility"], "safe_presentation_only"),
        new(true, true, true, true, true, true, true, true, true, true, true, true),
        new("assets/index.json", ["tier0"], 0, "sha256"),
        null,
        new("motion/profiles.json", "motion/reduced.json", true, true, true),
        null,
        new(IntegrityPath, null, "stable_or_store", "sha256"),
        null);

    private static byte[] Json<T>(T value) =>
        JsonSerializer.SerializeToUtf8Bytes(value, ThemeContractJson.CreateSerializerOptions());

    private static string Hash(byte[] value) => Convert.ToHexStringLower(SHA256.HashData(value));

    private sealed record Package(ThemeIntegrityVerificationRequestV2 Request, Dictionary<string, byte[]> Bytes)
    {
        internal CountingReader NewReader() => new(
            Bytes.Select(pair => new ThemePackageContentEntryV1(
                pair.Key, pair.Value.Length, ThemePackageContentEntryKind.File, null)).ToArray(),
            Bytes);
    }

    private sealed class CountingReader : IThemePackageContentReader
    {
        private readonly IReadOnlyList<ThemePackageContentEntryV1> _entries;
        private readonly Dictionary<string, byte[]> _bytes;
        private int _enumerations;

        internal CountingReader(
            IReadOnlyList<ThemePackageContentEntryV1> entries,
            Dictionary<string, byte[]> bytes)
        {
            _entries = entries.ToArray();
            _bytes = bytes;
        }

        internal int Enumerations => Volatile.Read(ref _enumerations);
        internal ConcurrentDictionary<string, int> ReadCounts { get; } = new(StringComparer.Ordinal);
        internal Dictionary<string, Exception> Failures { get; } = new(StringComparer.Ordinal);
        internal int Count(string path) => ReadCounts.GetValueOrDefault(path);

        public ValueTask<IReadOnlyList<ThemePackageContentEntryV1>> EnumerateEntriesAsync(
            ThemePackageRef packageRef,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Increment(ref _enumerations);
            return ValueTask.FromResult(_entries);
        }

        public ValueTask<ReadOnlyMemory<byte>> ReadContentAsync(
            ThemePackageRef packageRef,
            string canonicalPath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReadCounts.AddOrUpdate(canonicalPath, 1, (_, count) => count + 1);
            if (Failures.TryGetValue(canonicalPath, out Exception? failure)) throw failure;
            return ValueTask.FromResult<ReadOnlyMemory<byte>>(_bytes[canonicalPath]);
        }
    }
}
