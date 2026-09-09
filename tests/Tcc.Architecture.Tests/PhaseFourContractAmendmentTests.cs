using System.Collections.Immutable;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Tcc.Presentation.Contracts.Theme;

namespace Tcc.Architecture.Tests;

public sealed class PhaseFourContractAmendmentTests
{
    private const string IntegrityManifestPath = "metadata/integrity.json";
    private const string SignatureEnvelopePath = "metadata/signature.json";

    private static readonly string[] ForbiddenContractFragments =
    [
        "Risk",
        "MarketObservation",
        "AIRecommendation",
        "PositionIntelligence",
        "ShadowTrading",
        "Learning",
        "PreparedOrder",
        "MarketData",
        "TradeExecution",
        "ThemeCompatibilityResolver",
        "ThemeCapabilityGate",
        "ThemeLoader",
        "ThemeActivation",
        "ThemePreview",
        "ThemeSwitcher",
        "ThemeRollbackRuntime",
        "ThemeAssetLoader",
        "ThemeAssetCache",
        "ThemePersonalizationEngine",
        "ThemeMotionManager",
        "ThemeAudioRouter",
    ];

    private static readonly string[] ForbiddenReaderMethodFragments =
    [
        "write",
        "extract",
        "install",
        "move",
        "delete",
        "promote",
        "activate",
        "persist",
        "cache",
    ];

    [Fact]
    public void SealedV1SchemaAndSerializationRemainByteStable()
    {
        string schemaPath = Path.Combine(RepositoryPaths.ThemeContracts, "schemas", "ThemeIntegrity.schema.json");
        string schemaHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(schemaPath)));
        Assert.Equal("70173ED5066FA0B9F2C482F5604C57CAB364BB4C13BCE87E308CDCB07220A8E1", schemaHash);

        ThemeIntegrityManifest manifest = new(
            "1.0",
            "com.example.theme",
            "1.2.3",
            "sha256",
            [new ThemeIntegrityFile("theme.json", new string('a', 64), 42, true)],
            new string('b', 64),
            new DateTimeOffset(2026, 9, 7, 0, 0, 0, TimeSpan.Zero));
        ThemeIntegrityVerificationRequest request = new(
            new ThemeId("com.example.theme"),
            new ThemeVersion("1.2.3"),
            "package:example/theme/1.2.3",
            manifest,
            true);
        ThemeIntegrityVerificationResult result = new(true, true, new string('b', 64), new string('c', 64), []);
        JsonSerializerOptions options = ThemeContractJson.CreateSerializerOptions();

        Assert.Equal(
            "{\"schema_version\":\"1.0\",\"theme_id\":\"com.example.theme\",\"version\":\"1.2.3\",\"hash_algorithm\":\"sha256\",\"files\":[{\"path\":\"theme.json\",\"sha256\":\"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\",\"size_bytes\":42,\"required\":true}],\"package_hash\":\"bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb\",\"created_at\":\"2026-09-07T00:00:00+00:00\"}",
            JsonSerializer.Serialize(manifest, options));
        Assert.Equal(
            "{\"theme_id\":\"com.example.theme\",\"version\":\"1.2.3\",\"package_ref\":\"package:example/theme/1.2.3\",\"manifest\":{" +
            "\"schema_version\":\"1.0\",\"theme_id\":\"com.example.theme\",\"version\":\"1.2.3\",\"hash_algorithm\":\"sha256\",\"files\":[{\"path\":\"theme.json\",\"sha256\":\"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa\",\"size_bytes\":42,\"required\":true}],\"package_hash\":\"bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb\",\"created_at\":\"2026-09-07T00:00:00+00:00\"},\"signature_required\":true}",
            JsonSerializer.Serialize(request, options));
        Assert.Equal(
            "{\"is_verified\":true,\"signature_verified\":true,\"package_hash\":\"bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb\",\"manifest_hash\":\"cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc\",\"errors\":[]}",
            JsonSerializer.Serialize(result, options));
    }

    [Fact]
    public void SealedV1IntegrityPublicSurfaceRemainsUnchanged()
    {
        AssertRecordProperties<ThemeIntegrityDeclaration>(
            ("IntegrityManifest", typeof(string)),
            ("Signature", typeof(string)),
            ("SignatureRequiredForChannel", typeof(string)),
            ("HashAlgorithm", typeof(string)));
        AssertRecordProperties<ThemeIntegrityManifest>(
            ("SchemaVersion", typeof(string)),
            ("ThemeId", typeof(string)),
            ("Version", typeof(string)),
            ("HashAlgorithm", typeof(string)),
            ("Files", typeof(IReadOnlyList<ThemeIntegrityFile>)),
            ("PackageHash", typeof(string)),
            ("CreatedAt", typeof(DateTimeOffset)));
        AssertRecordProperties<ThemeIntegrityFile>(
            ("Path", typeof(string)),
            ("Sha256", typeof(string)),
            ("SizeBytes", typeof(long)),
            ("Required", typeof(bool)));
        AssertRecordProperties<ThemeIntegrityVerificationRequest>(
            ("ThemeId", typeof(ThemeId)),
            ("Version", typeof(ThemeVersion)),
            ("PackageRef", typeof(string)),
            ("Manifest", typeof(ThemeIntegrityManifest)),
            ("SignatureRequired", typeof(bool)));
        AssertRecordProperties<ThemeIntegrityVerificationResult>(
            ("IsVerified", typeof(bool)),
            ("SignatureVerified", typeof(bool)),
            ("PackageHash", typeof(string)),
            ("ManifestHash", typeof(string)),
            ("Errors", typeof(IReadOnlyList<string>)));

        MethodInfo verify = Assert.Single(typeof(IThemeIntegrityVerifier).GetMethods());
        Assert.Equal("VerifyAsync", verify.Name);
        Assert.Equal(typeof(ValueTask<ThemeIntegrityVerificationResult>), verify.ReturnType);
        Assert.Equal(
            [typeof(ThemeIntegrityVerificationRequest), typeof(CancellationToken)],
            verify.GetParameters().Select(parameter => parameter.ParameterType));
    }

    [Fact]
    public void V2IntegrityManifestSerializesDeterministicallyAndValidates()
    {
        ThemeIntegrityManifestV2 manifest = CreateV2Manifest();
        JsonSerializerOptions options = ThemeContractJson.CreateSerializerOptions();

        string first = JsonSerializer.Serialize(manifest, options);
        string second = JsonSerializer.Serialize(manifest, options);

        Assert.Equal(first, second);
        Assert.Contains("\"schema_version\":\"2.0\"", first, StringComparison.Ordinal);
        Assert.Contains("\"inventory_mode\":\"exhaustive_allowed_payload\"", first, StringComparison.Ordinal);
        Assert.Contains("\"entry_kind\":\"theme_manifest\"", first, StringComparison.Ordinal);
        AssertSchemaValid("ThemeIntegrity.v2.schema.json", first);

        ThemeIntegrityManifestV2? roundTripped = JsonSerializer.Deserialize<ThemeIntegrityManifestV2>(first, options);
        Assert.NotNull(roundTripped);
        Assert.Equal(first, JsonSerializer.Serialize(roundTripped, options));
    }

    [Fact]
    public void SignatureEnvelopeSerializesDeterministicallyAndValidates()
    {
        ThemeSignatureEnvelopeV1 envelope = new(
            ContractVersions.ThemeSignatureEnvelopeSchemaV1,
            ThemeSignatureAlgorithm.EcdsaP256Sha256,
            "publisher.example",
            "key-2026-01",
            ThemeSignatureEncoding.IeeeP1363FixedFieldConcatenation,
            "pyTXo1K40x+l8IHxodTZ30zgKa2pAv4sW4QP/9j7ILZrEtHLbhSp9qfbcm8IYcv0fx7RaeRhk0gb9RnBfFSMaw==",
            ThemeSignedPayloadType.TccThemePackageSignatureV1);
        JsonSerializerOptions options = ThemeContractJson.CreateSerializerOptions();

        string serialized = JsonSerializer.Serialize(envelope, options);

        Assert.Equal(serialized, JsonSerializer.Serialize(envelope, options));
        Assert.Contains("\"algorithm\":\"ecdsa_p256_sha256\"", serialized, StringComparison.Ordinal);
        Assert.Contains("\"signature_encoding\":\"ieee_p1363_fixed_field_concatenation\"", serialized, StringComparison.Ordinal);
        Assert.Contains("\"signed_payload_type\":\"tcc_theme_package_signature_v1\"", serialized, StringComparison.Ordinal);
        AssertSchemaValid("ThemeSignatureEnvelope.v1.schema.json", serialized);
    }

    [Fact]
    public void V2RequestPolicyTrustEvidenceAndResultSerializeDeterministically()
    {
        ThemeTrustSnapshotV1 trust = new(
            "default-trust",
            "1",
            [Signer("publisher.example", "key-2026-01", ThemeTrustState.Trusted)]);
        ThemeIntegrityVerificationPolicyV1 policy = new(
            "stable-policy",
            "1",
            "default-trust",
            "1",
            ThemeDistributionChannel.Stable,
            ThemeSignatureRequirement.Required,
            false);
        ThemeSignatureEnvelopeV1 envelope = new(
            ContractVersions.ThemeSignatureEnvelopeSchemaV1,
            ThemeSignatureAlgorithm.EcdsaP256Sha256,
            "publisher.example",
            "key-2026-01",
            ThemeSignatureEncoding.IeeeP1363FixedFieldConcatenation,
            "pyTXo1K40x+l8IHxodTZ30zgKa2pAv4sW4QP/9j7ILZrEtHLbhSp9qfbcm8IYcv0fx7RaeRhk0gb9RnBfFSMaw==",
            ThemeSignedPayloadType.TccThemePackageSignatureV1);
        ThemeIntegrityVerificationRequestV2 request = new(
            new ThemeId("com.example.theme"),
            new ThemeVersion("1.2.3"),
            new ThemePackageRef("package:example/theme/1.2.3"),
            ThemeCanonicalPath.Parse("theme.json"),
            ThemeCanonicalPath.Parse(IntegrityManifestPath),
            ThemeCanonicalPath.Parse(SignatureEnvelopePath),
            CreateV2Manifest(),
            envelope,
            policy,
            trust);
        ThemeIntegrityVerificationResultV2 result = new(
            true,
            new string('a', 64),
            new string('b', 64),
            new string('c', 64),
            [new ThemeIntegrityFileEvidenceV1(
                "theme.json",
                ThemeIntegrityEntryKind.ThemeManifest,
                true,
                true,
                42,
                42,
                new string('b', 64),
                new string('b', 64),
                ThemeIntegrityEvidenceStatus.Verified)],
            [],
            ThemeSignatureVerificationStatus.Valid,
            "publisher.example",
            "key-2026-01",
            []);
        JsonSerializerOptions options = ThemeContractJson.CreateSerializerOptions();

        string requestJson = JsonSerializer.Serialize(request, options);
        string resultJson = JsonSerializer.Serialize(result, options);

        Assert.Equal(requestJson, JsonSerializer.Serialize(request, options));
        Assert.Equal(resultJson, JsonSerializer.Serialize(result, options));
        Assert.Contains("\"package_ref\":\"package:example/theme/1.2.3\"", requestJson, StringComparison.Ordinal);
        Assert.Contains("\"distribution_channel\":\"stable\"", requestJson, StringComparison.Ordinal);
        Assert.Contains("\"trust_state\":\"trusted\"", requestJson, StringComparison.Ordinal);
        Assert.Contains("\"signature_status\":\"valid\"", resultJson, StringComparison.Ordinal);
        Assert.Equal(
            typeof(ImmutableArray<ThemeTrustedSignerV1>),
            typeof(ThemeTrustSnapshotV1).GetProperty("TrustedSigners")!.PropertyType);
    }

    [Fact]
    public void VersionedSchemasFailClosedForUnsupportedVersionsAndMissingSecurityFields()
    {
        using JsonDocument integritySchema = ReadSchema("ThemeIntegrity.v2.schema.json");
        JsonObject manifest = Assert.IsType<JsonObject>(
            JsonSchemaSubsetValidator.CreateMinimumValidInstance(integritySchema.RootElement));
        manifest["schema_version"] = "3.0";
        using JsonDocument unsupportedIntegrity = JsonDocument.Parse(manifest.ToJsonString());
        Assert.NotEmpty(JsonSchemaSubsetValidator.Validate(integritySchema.RootElement, unsupportedIntegrity.RootElement));

        using JsonDocument envelopeSchema = ReadSchema("ThemeSignatureEnvelope.v1.schema.json");
        foreach (string field in new[]
                 {
                     "envelope_version", "algorithm", "publisher_id", "key_id", "signature_encoding", "signature",
                     "signed_payload_type",
                 })
        {
            JsonObject envelope = Assert.IsType<JsonObject>(
                JsonSchemaSubsetValidator.CreateMinimumValidInstance(envelopeSchema.RootElement));
            Assert.True(envelope.Remove(field));
            using JsonDocument missing = JsonDocument.Parse(envelope.ToJsonString());
            Assert.Contains(
                JsonSchemaSubsetValidator.Validate(envelopeSchema.RootElement, missing.RootElement),
                error => error.Contains($"missing required property '{field}'", StringComparison.Ordinal));
        }

        JsonObject unsupportedEnvelope = Assert.IsType<JsonObject>(
            JsonSchemaSubsetValidator.CreateMinimumValidInstance(envelopeSchema.RootElement));
        unsupportedEnvelope["algorithm"] = "rsa_sha256";
        using JsonDocument unsupportedAlgorithm = JsonDocument.Parse(unsupportedEnvelope.ToJsonString());
        Assert.NotEmpty(JsonSchemaSubsetValidator.Validate(envelopeSchema.RootElement, unsupportedAlgorithm.RootElement));
    }

    [Theory]
    [InlineData("theme.json")]
    [InlineData("assets/icons/icon.png")]
    [InlineData("資料/主題.json")]
    [InlineData("Assets/MixedCase.JSON")]
    public void CanonicalPathAcceptsSafeRelativeNfcPaths(string path)
    {
        Assert.True(ThemeCanonicalPath.TryCreate(path, out ThemeCanonicalPath? canonical));
        Assert.Equal(path.Normalize(NormalizationForm.FormC), canonical!.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("/absolute/theme.json")]
    [InlineData("C:/theme.json")]
    [InlineData("\\\\server\\share\\theme.json")]
    [InlineData("assets\\theme.json")]
    [InlineData("assets//theme.json")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("assets/./theme.json")]
    [InlineData("assets/../theme.json")]
    [InlineData("assets/theme.json.")]
    [InlineData("assets/theme.json ")]
    [InlineData("assets/key:value.json")]
    [InlineData("assets/con")]
    [InlineData("assets/NUL.txt")]
    [InlineData("COM1")]
    [InlineData("lpt9.log")]
    [InlineData("assets/control\u0001.json")]
    public void CanonicalPathRejectsUnsafeForms(string path)
    {
        Assert.False(ThemeCanonicalPath.TryCreate(path, out _));
    }

    [Fact]
    public void CanonicalPathUsesNfcAndRejectsNormalizedDuplicatesAndCaseCollisions()
    {
        Assert.True(ThemeCanonicalPath.TryCreate("assets/e\u0301.png", out ThemeCanonicalPath? decomposed));
        Assert.True(ThemeCanonicalPath.TryCreate("assets/é.png", out ThemeCanonicalPath? composed));
        Assert.Equal(composed, decomposed);
        Assert.Equal("assets/é.png", composed!.Value);
    }

    [Theory]
    [InlineData("/absolute.json")]
    [InlineData("C:/drive.json")]
    [InlineData("assets\\backslash.json")]
    [InlineData("assets/../escape.json")]
    [InlineData("assets/NUL.txt")]
    [InlineData("assets/trailing. ")]
    public void V2SchemaRejectsLexicallyInvalidCanonicalPaths(string invalidPath)
    {
        using JsonDocument schema = ReadSchema("ThemeIntegrity.v2.schema.json");
        JsonObject manifest = Assert.IsType<JsonObject>(
            JsonSchemaSubsetValidator.CreateMinimumValidInstance(schema.RootElement));
        JsonObject file = Assert.IsType<JsonObject>(Assert.IsType<JsonArray>(manifest["files"])[0]);
        file["canonical_path"] = invalidPath;

        using JsonDocument invalid = JsonDocument.Parse(manifest.ToJsonString());
        Assert.NotEmpty(JsonSchemaSubsetValidator.Validate(schema.RootElement, invalid.RootElement));
    }

    [Fact]
    public void ExhaustiveInventorySemanticsAreFailClosedAndMetadataExclusionsAreExact()
    {
        OracleVector vector = CreateValidOracleVector();
        Assert.True(ThemeIntegrityContractConformanceOracle.Evaluate(
            vector.Request, vector.Entries, vector.Contents).IsVerified);

        ThemePackageContentEntryV1 required = vector.Entries.Single(entry => entry.LogicalPath == "assets/payload.bin");
        Dictionary<string, byte[]> missingContents = vector.Contents
            .Where(pair => pair.Key != required.LogicalPath)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        ThemeIntegrityVerificationResultV2 missing = ThemeIntegrityContractConformanceOracle.Evaluate(
            vector.Request,
            vector.Entries.Where(entry => entry.LogicalPath != required.LogicalPath).ToArray(),
            missingContents);
        Assert.Contains(ThemeIntegrityDiagnosticCodes.MissingRequiredFile, missing.Diagnostics.Select(item => item.Code));

        ThemePackageContentEntryV1 similarMetadata = Present($"{IntegrityManifestPath}.bak");
        Dictionary<string, byte[]> extraContents = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        extraContents[similarMetadata.LogicalPath] = [0x01];
        ThemeIntegrityVerificationResultV2 undeclared = ThemeIntegrityContractConformanceOracle.Evaluate(
            vector.Request,
            [.. vector.Entries, similarMetadata],
            extraContents);
        Assert.Contains(ThemeIntegrityDiagnosticCodes.UndeclaredFile, undeclared.Diagnostics.Select(item => item.Code));
    }

    [Fact]
    public void PresentDeclaredFilesVerifyLengthAndHash()
    {
        OracleVector vector = CreateValidOracleVector();
        ThemeIntegrityVerificationResultV2 valid = ThemeIntegrityContractConformanceOracle.Evaluate(
            vector.Request, vector.Entries, vector.Contents);
        Assert.All(valid.FileEvidence, item => Assert.Equal(ThemeIntegrityEvidenceStatus.Verified, item.Status));

        ThemePackageContentEntryV1 payloadEntry = vector.Entries.Single(entry => entry.LogicalPath == "assets/payload.bin");
        ThemeIntegrityVerificationResultV2 wrongLength = ThemeIntegrityContractConformanceOracle.Evaluate(
            vector.Request,
            vector.Entries.Select(entry => entry == payloadEntry ? entry with { LengthBytes = entry.LengthBytes + 1 } : entry).ToArray(),
            vector.Contents);
        Assert.Contains(ThemeIntegrityDiagnosticCodes.FileLengthMismatch, wrongLength.Diagnostics.Select(item => item.Code));

        Dictionary<string, byte[]> wrongHashContents = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        wrongHashContents[payloadEntry.LogicalPath] = Encoding.UTF8.GetBytes("PAYLOAD");
        ThemeIntegrityVerificationResultV2 wrongHash = ThemeIntegrityContractConformanceOracle.Evaluate(
            vector.Request, vector.Entries, wrongHashContents);
        Assert.Contains(ThemeIntegrityDiagnosticCodes.FileHashMismatch, wrongHash.Diagnostics.Select(item => item.Code));
    }

    [Fact]
    public void CanonicalPackageTreeHashMatchesDeterministicVectorAndExactExclusions()
    {
        TestPackageEntry[] payload =
        [
            Entry("theme.json", "{\"id\":\"theme\"}"),
            Entry("assets/A.txt", "Hello"),
            Entry("assets/b.txt", "world"),
        ];

        string forward = ComputePackageHash(payload);
        string reverse = ComputePackageHash(payload.Reverse());

        Assert.Equal("9986788c05d8024b349fe4259559b4f41ed233500659df99a6f9363cc9ad0370", forward);
        Assert.Equal(forward, reverse);

        TestPackageEntry[] withReserved =
        [
            .. payload,
            Entry(IntegrityManifestPath, "self-reference"),
            Entry(SignatureEnvelopePath, "detached-signature"),
        ];
        Assert.Equal(forward, ComputePackageHash(withReserved));

        TestPackageEntry[] withSimilarName = [.. withReserved, Entry($"{IntegrityManifestPath}.bak", "included")];
        Assert.NotEqual(forward, ComputePackageHash(withSimilarName));
    }

    [Fact]
    public void CanonicalPackageTreeHashIsIndependentOfRawZipRepresentation()
    {
        (string Path, byte[] Bytes)[] entries =
        [
            ("theme.json", Encoding.UTF8.GetBytes("{\"id\":\"theme\"}")),
            ("assets/A.txt", Encoding.UTF8.GetBytes("Hello")),
            ("assets/b.txt", Encoding.UTF8.GetBytes("world")),
            (IntegrityManifestPath, Encoding.UTF8.GetBytes("integrity")),
            (SignatureEnvelopePath, Encoding.UTF8.GetBytes("signature")),
        ];
        byte[] firstZip = CreateZip(entries, CompressionLevel.NoCompression, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        byte[] secondZip = CreateZip(entries.Reverse().ToArray(), CompressionLevel.Optimal, new DateTimeOffset(2026, 2, 2, 0, 0, 0, TimeSpan.Zero));

        Assert.NotEqual(Convert.ToHexString(firstZip), Convert.ToHexString(secondZip));
        Assert.Equal(ComputePackageHash(ReadZip(firstZip)), ComputePackageHash(ReadZip(secondZip)));
        Assert.Equal("9986788c05d8024b349fe4259559b4f41ed233500659df99a6f9363cc9ad0370", ComputePackageHash(ReadZip(firstZip)));
    }

    [Fact]
    public void ManifestEvidenceHashesUseActualRawBytesWithoutJsonReserialization()
    {
        byte[] compactThemeManifest = Encoding.UTF8.GetBytes("{\"theme_id\":\"com.example.theme\"}");
        byte[] formattedThemeManifest = Encoding.UTF8.GetBytes("{ \"theme_id\" : \"com.example.theme\" }");
        byte[] compactIntegrityManifest = Encoding.UTF8.GetBytes("{\"schema_version\":\"2.0\"}");
        byte[] formattedIntegrityManifest = Encoding.UTF8.GetBytes("{\n  \"schema_version\": \"2.0\"\n}");

        string themeManifestHash = ThemeIntegrityContractConformanceOracle.Sha256Hex(compactThemeManifest);
        string integrityManifestHash = ThemeIntegrityContractConformanceOracle.Sha256Hex(compactIntegrityManifest);

        Assert.NotEqual(themeManifestHash, ThemeIntegrityContractConformanceOracle.Sha256Hex(formattedThemeManifest));
        Assert.NotEqual(integrityManifestHash, ThemeIntegrityContractConformanceOracle.Sha256Hex(formattedIntegrityManifest));
        Assert.Matches("^[a-f0-9]{64}$", themeManifestHash);
        Assert.Matches("^[a-f0-9]{64}$", integrityManifestHash);
    }

    [Fact]
    public void SignatureCanonicalPayloadMatchesUtf8WithoutBomDeterministicVector()
    {
        byte[] payload = ThemeIntegrityContractConformanceOracle.BuildSignaturePayload(
            "com.example.theme",
            "1.2.3",
            new string('a', 64),
            new string('b', 64),
            new string('c', 64),
            "publisher.example",
            "key-2026-01");

        Assert.Equal(
            "5443432d5448454d452d5041434b4147452d5349474e41545552452d563100636f6d2e6578616d706c652e7468656d6500312e322e33006161616161616161616161616161616161616161616161616161616161616161616161616161616161616161616161616161616161616161616161616161616100626262626262626262626262626262626262626262626262626262626262626262626262626262626262626262626262626262626262626262626262626262620063636363636363636363636363636363636363636363636363636363636363636363636363636363636363636363636363636363636363636363636363636363007075626c69736865722e6578616d706c65006b65792d323032362d3031",
            Convert.ToHexString(payload).ToLowerInvariant());
        Assert.False(payload.AsSpan().StartsWith(Encoding.UTF8.GetPreamble()));
    }

    [Fact]
    public void EcdsaP256P1363VectorAcceptsValidAndRejectsInvalidOrWrongKey()
    {
        byte[] payload = ThemeIntegrityContractConformanceOracle.BuildSignaturePayload(
            "com.example.theme",
            "1.2.3",
            new string('a', 64),
            new string('b', 64),
            new string('c', 64),
            "publisher.example",
            "key-2026-01");
        byte[] publicKey = Convert.FromBase64String(
            "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEeT68e7qekZ59xr4povRF6e9gB8M4GErSkxdIsmwVDfRVmfmXa9duEKfoOCulrcN/bSeXbd4M+2OMcMKmEsXmfw==");
        byte[] signature = Convert.FromBase64String(
            "pyTXo1K40x+l8IHxodTZ30zgKa2pAv4sW4QP/9j7ILZrEtHLbhSp9qfbcm8IYcv0fx7RaeRhk0gb9RnBfFSMaw==");

        using ECDsa verifier = ECDsa.Create();
        verifier.ImportSubjectPublicKeyInfo(publicKey, out int bytesRead);
        Assert.Equal(publicKey.Length, bytesRead);
        Assert.Equal(64, signature.Length);
        Assert.True(verifier.VerifyData(
            payload,
            signature,
            HashAlgorithmName.SHA256,
            DSASignatureFormat.IeeeP1363FixedFieldConcatenation));

        byte[] invalidSignature = (byte[])signature.Clone();
        invalidSignature[0] ^= 0x01;
        Assert.False(verifier.VerifyData(
            payload,
            invalidSignature,
            HashAlgorithmName.SHA256,
            DSASignatureFormat.IeeeP1363FixedFieldConcatenation));

        using ECDsa wrongKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        Assert.False(wrongKey.VerifyData(
            payload,
            signature,
            HashAlgorithmName.SHA256,
            DSASignatureFormat.IeeeP1363FixedFieldConcatenation));
    }

    [Fact]
    public void TrustSnapshotDistinguishesTrustedRevokedAndUnknownSigner()
    {
        OracleVector vector = CreateValidOracleVector();
        ThemeTrustedSignerV1 trusted = Assert.Single(vector.Request.TrustSnapshot.TrustedSigners);
        Assert.Equal(
            ThemeSignatureVerificationStatus.Valid,
            ThemeIntegrityContractConformanceOracle.Evaluate(vector.Request, vector.Entries, vector.Contents).SignatureStatus);

        ThemeIntegrityVerificationRequestV2 revoked = vector.Request with
        {
            TrustSnapshot = vector.Request.TrustSnapshot with
            {
                TrustedSigners = [trusted with { TrustState = ThemeTrustState.Revoked }],
            },
        };
        Assert.Equal(
            ThemeSignatureVerificationStatus.RevokedSigner,
            ThemeIntegrityContractConformanceOracle.Evaluate(revoked, vector.Entries, vector.Contents).SignatureStatus);

        foreach ((string publisherId, string keyId) in new[]
                 {
                     (trusted.PublisherId, "missing-key"),
                     ("other-publisher", trusted.KeyId),
                 })
        {
            ThemeIntegrityVerificationRequestV2 unknown = vector.Request with
            {
                SignatureEnvelope = vector.Request.SignatureEnvelope! with
                {
                    PublisherId = publisherId,
                    KeyId = keyId,
                },
            };
            Assert.Equal(
                ThemeSignatureVerificationStatus.UnknownSigner,
                ThemeIntegrityContractConformanceOracle.Evaluate(unknown, vector.Entries, BindMetadata(vector, unknown)).SignatureStatus);
        }
    }

    [Theory]
    [InlineData(ThemeDistributionChannel.Stable, ThemeSignatureRequirement.Required, false, true, true)]
    [InlineData(ThemeDistributionChannel.Stable, ThemeSignatureRequirement.Required, false, false, false)]
    [InlineData(ThemeDistributionChannel.Stable, ThemeSignatureRequirement.Optional, false, true, false)]
    [InlineData(ThemeDistributionChannel.Stable, ThemeSignatureRequirement.Required, true, true, false)]
    [InlineData(ThemeDistributionChannel.Store, ThemeSignatureRequirement.Required, false, true, true)]
    [InlineData(ThemeDistributionChannel.Store, ThemeSignatureRequirement.Required, false, false, false)]
    [InlineData(ThemeDistributionChannel.Store, ThemeSignatureRequirement.Optional, false, true, false)]
    [InlineData(ThemeDistributionChannel.Developer, ThemeSignatureRequirement.Optional, true, false, true)]
    [InlineData(ThemeDistributionChannel.Developer, ThemeSignatureRequirement.Optional, false, false, false)]
    [InlineData(ThemeDistributionChannel.Developer, ThemeSignatureRequirement.Optional, false, true, true)]
    [InlineData(ThemeDistributionChannel.Beta, ThemeSignatureRequirement.Required, false, false, false)]
    [InlineData(ThemeDistributionChannel.Beta, ThemeSignatureRequirement.Required, false, true, true)]
    [InlineData(ThemeDistributionChannel.Beta, ThemeSignatureRequirement.Optional, false, false, true)]
    public void ChannelPolicyRequiresExplicitCallerDecision(
        ThemeDistributionChannel channel,
        ThemeSignatureRequirement requirement,
        bool developerException,
        bool signaturePresent,
        bool expected)
    {
        OracleVector vector = CreateValidOracleVector();
        ThemeIntegrityVerificationPolicyV1 policy = vector.Request.Policy with
        {
            DistributionChannel = channel,
            SignatureRequirement = requirement,
            DeveloperExceptionAuthorized = developerException,
        };
        ThemeIntegrityVerificationRequestV2 request = vector.Request with
        {
            Policy = policy,
            SignatureEnvelope = signaturePresent ? vector.Request.SignatureEnvelope : null,
            SignatureEnvelopePath = signaturePresent ? vector.Request.SignatureEnvelopePath : null,
        };

        Assert.Equal(
            expected,
            ThemeIntegrityContractConformanceOracle.Evaluate(
                request,
                signaturePresent
                    ? vector.Entries
                    : vector.Entries.Where(entry => entry.LogicalPath != SignatureEnvelopePath).ToArray(),
                vector.Contents).IsVerified);
    }

    [Fact]
    public void DiagnosticsUseUniqueStableP4ICodesAndDeterministicOrdinalOrdering()
    {
        FieldInfo[] codeFields = typeof(ThemeIntegrityDiagnosticCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .ToArray();
        string[] codes = codeFields.Select(field => (string)field.GetRawConstantValue()!).ToArray();

        Assert.NotEmpty(codes);
        Assert.Equal(codes.Length, codes.Distinct(StringComparer.Ordinal).Count());
        Assert.All(codes, code => Assert.Matches("^P4I[0-9]{3}$", code));

        ThemeIntegrityDiagnosticV1[] input =
        [
            new("P4I010", ThemeDiagnosticsSeverity.Error, "b/path", "z-message"),
            new("P4I002", ThemeDiagnosticsSeverity.Error, "z/path", "message"),
            new("P4I010", ThemeDiagnosticsSeverity.Error, "A/path", "message"),
            new("P4I010", ThemeDiagnosticsSeverity.Error, "A/path", "a-message"),
        ];
        string[] first = ThemeIntegrityContractConformanceOracle.OrderDiagnostics(input).Select(FormatDiagnostic).ToArray();
        string[] second = ThemeIntegrityContractConformanceOracle.OrderDiagnostics(input.Reverse()).Select(FormatDiagnostic).ToArray();

        Assert.Equal(first, second);
        Assert.Equal(
            ["P4I002|z/path|message", "P4I010|A/path|a-message", "P4I010|A/path|message", "P4I010|b/path|z-message"],
            first);
    }

    [Fact]
    public void PackageReaderSurfaceIsReadOnlyAndPackageRefRemainsOpaque()
    {
        MethodInfo[] methods = typeof(IThemePackageContentReader).GetMethods().OrderBy(method => method.Name, StringComparer.Ordinal).ToArray();

        Assert.Equal(["EnumerateEntriesAsync", "ReadContentAsync"], methods.Select(method => method.Name));
        Assert.Equal(typeof(ValueTask<IReadOnlyList<ThemePackageContentEntryV1>>), methods[0].ReturnType);
        Assert.Equal(typeof(ValueTask<ReadOnlyMemory<byte>>), methods[1].ReturnType);
        Assert.All(methods, method => Assert.Equal(typeof(ThemePackageRef), method.GetParameters()[0].ParameterType));
        Assert.DoesNotContain(
            methods.SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType == typeof(FileInfo) || parameter.ParameterType == typeof(DirectoryInfo));
        Assert.DoesNotContain(
            methods,
            method => ForbiddenReaderMethodFragments
                .Any(fragment => method.Name.Contains(fragment, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void UnsupportedPhysicalEntryKindsFailClosedAtVerifierBoundary()
    {
        OracleVector vector = CreateValidOracleVector();
        Assert.True(Evaluate(vector).IsVerified);
        foreach (ThemePackageContentEntryKind kind in Enum.GetValues<ThemePackageContentEntryKind>().Where(kind => kind != ThemePackageContentEntryKind.File))
            AssertStructuredFailure(ThemeIntegrityContractConformanceOracle.Evaluate(vector.Request,
                vector.Entries.Select(entry => entry with { EntryKind = kind }).ToArray(), vector.Contents),
                ThemeIntegrityDiagnosticCodes.UnsupportedPackageEntry);
    }

    [Fact]
    public void ResultUsesNullableUnavailableHashesAndContainsNoDecisionTimestamp()
    {
        ThemeIntegrityVerificationResultV2 unavailable = new(
            false,
            null,
            null,
            null,
            [],
            [],
            ThemeSignatureVerificationStatus.NotEvaluated,
            null,
            null,
            [new ThemeIntegrityDiagnosticV1(
                ThemeIntegrityDiagnosticCodes.PackageReadFailure,
                ThemeDiagnosticsSeverity.Error,
                null,
                "Package content was unavailable.")]);
        string json = JsonSerializer.Serialize(unavailable, ThemeContractJson.CreateSerializerOptions());

        Assert.DoesNotContain("\"package_hash\":\"\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"theme_manifest_hash\":\"\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"integrity_manifest_hash\":\"\"", json, StringComparison.Ordinal);
        Assert.Null(typeof(ThemeIntegrityVerificationResultV2).GetProperty("VerifiedAt"));
        Assert.NotNull(typeof(ThemeIntegrityVerificationResultV2).GetProperty("FileEvidence"));
        Assert.NotNull(typeof(ThemeIntegrityVerificationResultV2).GetProperty("AssetEvidence"));
    }

    [Fact]
    public void AmendmentAddsNoVerifierImplementationOrPhaseFiveOrTradingLeakage()
    {
        Type[] productionTypes =
        [
            .. typeof(Tcc.Presentation.Contracts.AssemblyMarker).Assembly.GetTypes(),
            .. typeof(Tcc.Themes.AssemblyMarker).Assembly.GetTypes(),
            .. typeof(Tcc.Features.Themes.AssemblyMarker).Assembly.GetTypes(),
            .. typeof(Tcc.Windows.AssemblyMarker).Assembly.GetTypes(),
        ];

        PhaseThreeScopeBoundaryTests.AssertExactVerifierImplementations(productionTypes);
        Assert.Equal(typeof(PhaseFourContractAmendmentTests).Assembly, typeof(ThemeIntegrityContractConformanceOracle).Assembly);
        Assert.DoesNotContain(
            productionTypes,
            type => string.Equals(type.Name, nameof(ThemeIntegrityContractConformanceOracle), StringComparison.Ordinal));

        string[] amendmentFiles =
        [
            Path.Combine(RepositoryPaths.Root, "src", "Tcc.Presentation.Contracts", "Theme", "ThemeIntegrityContractsV2.cs"),
            Path.Combine(RepositoryPaths.Root, "src", "Tcc.Presentation.Contracts", "Theme", "ThemeIntegrityInterfacesV2.cs"),
            Path.Combine(RepositoryPaths.ThemeContracts, "schemas", "ThemeIntegrity.v2.schema.json"),
            Path.Combine(RepositoryPaths.ThemeContracts, "schemas", "ThemeSignatureEnvelope.v1.schema.json"),
        ];
        string contractText = string.Join('\n', amendmentFiles.Select(File.ReadAllText));

        Assert.DoesNotContain(
            ForbiddenContractFragments,
            fragment => contractText.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    [Trait("Defect", "P4A-VAL-001")]
    public void SignatureSchemaAndContractOracleRejectMalformedOrWrongLengthP1363Values()
    {
        string validSignature = Convert.ToBase64String(Enumerable.Range(0, 64).Select(index => (byte)index).ToArray());
        string[] invalidSignatures =
        [
            "",
            "A",
            "YQ==",
            $"{new string('A', 87)}*",
            Convert.ToBase64String(new byte[63]),
            Convert.ToBase64String(new byte[65]),
        ];

        using JsonDocument schema = ReadSchema("ThemeSignatureEnvelope.v1.schema.json");
        JsonObject baseline = Assert.IsType<JsonObject>(
            JsonSchemaSubsetValidator.CreateMinimumValidInstance(schema.RootElement));
        baseline["signature"] = validSignature;
        using (JsonDocument valid = JsonDocument.Parse(baseline.ToJsonString()))
        {
            Assert.Empty(JsonSchemaSubsetValidator.Validate(schema.RootElement, valid.RootElement));
        }

        OracleVector vector = CreateValidOracleVector();
        foreach (string invalidSignature in invalidSignatures)
        {
            JsonObject mutation = Assert.IsType<JsonObject>(JsonNode.Parse(baseline.ToJsonString()));
            mutation["signature"] = invalidSignature;
            using JsonDocument invalid = JsonDocument.Parse(mutation.ToJsonString());
            Assert.NotEmpty(JsonSchemaSubsetValidator.Validate(schema.RootElement, invalid.RootElement));

            ThemeIntegrityVerificationRequestV2 request = vector.Request with
            {
                SignatureEnvelope = vector.Request.SignatureEnvelope! with { Signature = invalidSignature },
            };
            Assert.Contains(
                ThemeIntegrityDiagnosticCodes.InvalidSignature,
                ThemeIntegrityContractConformanceOracle.Evaluate(request, vector.Entries, vector.Contents)
                    .Diagnostics.Select(diagnostic => diagnostic.Code));
        }
    }

    [Fact]
    [Trait("Defect", "P4A-VAL-001")]
    public void AuthoritativeV2EnumsRejectUnknownNumericAndStringJsonValues()
    {
        JsonSerializerOptions options = ThemeContractJson.CreateSerializerOptions();
        ThemeSignatureEnvelopeV1 envelope = CreateValidOracleVector().Request.SignatureEnvelope!;
        JsonObject baseline = Assert.IsType<JsonObject>(
            JsonNode.Parse(JsonSerializer.Serialize(envelope, options)));

        foreach (JsonNode unknown in new JsonNode[] { JsonValue.Create(999)!, JsonValue.Create("unknown_algorithm")! })
        {
            JsonObject mutation = Assert.IsType<JsonObject>(JsonNode.Parse(baseline.ToJsonString()));
            mutation["algorithm"] = unknown.DeepClone();
            Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<ThemeSignatureEnvelopeV1>(mutation.ToJsonString(), options));
        }

        JsonObject inventory = Assert.IsType<JsonObject>(
            JsonNode.Parse(JsonSerializer.Serialize(CreateV2Manifest(), options)));
        inventory["inventory_mode"] = 999;
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<ThemeIntegrityManifestV2>(inventory.ToJsonString(), options));
    }

    [Fact]
    [Trait("Defect", "P4A-VAL-001")]
    public void UnsupportedSignatureAlgorithmAndEncodingFailClosedInTheOracle()
    {
        OracleVector vector = CreateValidOracleVector();
        AssertOracleDiagnostic(
            vector,
            vector.Request with
            {
                SignatureEnvelope = vector.Request.SignatureEnvelope! with
                {
                    Algorithm = (ThemeSignatureAlgorithm)999,
                },
            },
            ThemeIntegrityDiagnosticCodes.UnsupportedSignatureAlgorithm);
        AssertOracleDiagnostic(
            vector,
            vector.Request with
            {
                SignatureEnvelope = vector.Request.SignatureEnvelope! with
                {
                    SignatureEncoding = (ThemeSignatureEncoding)999,
                },
            },
            ThemeIntegrityDiagnosticCodes.UnsupportedSignatureEncoding);
    }

    [Fact]
    [Trait("Defect", "P4A-VAL-002")]
    public void CanonicalPathContractAndSchemaRejectCompleteWindowsDeviceVocabulary()
    {
        string[] basenames =
        [
            "CON", "PRN", "AUX", "NUL", "CLOCK$", "CONIN$", "CONOUT$",
            .. Enumerable.Range(1, 9).Select(index => $"COM{index}"),
            .. Enumerable.Range(1, 9).Select(index => $"LPT{index}"),
        ];
        using JsonDocument schema = ReadSchema("ThemeIntegrity.v2.schema.json");

        foreach (string basename in basenames)
        {
            foreach (string path in new[] { basename, $"assets/{basename}.json" })
            {
                Assert.False(ThemeCanonicalPath.TryCreate(path, out _));
                JsonObject manifest = Assert.IsType<JsonObject>(
                    JsonSchemaSubsetValidator.CreateMinimumValidInstance(schema.RootElement));
                JsonObject file = Assert.IsType<JsonObject>(Assert.IsType<JsonArray>(manifest["files"])[0]);
                file["canonical_path"] = path;
                using JsonDocument invalid = JsonDocument.Parse(manifest.ToJsonString());
                Assert.NotEmpty(JsonSchemaSubsetValidator.Validate(schema.RootElement, invalid.RootElement));
            }
        }
    }

    [Fact]
    [Trait("Defect", "P4A-VAL-002")]
    public void InventoryOracleRejectsExactCaseAndUnicodeCanonicalCollisions()
    {
        OracleVector vector = CreateValidOracleVector();
        ThemeIntegrityFileV2 declared = vector.Request.IntegrityManifest.Files[1];

        AssertOracleDiagnosticForFiles(
            vector,
            [.. vector.Request.IntegrityManifest.Files, declared with { Sha256 = new string('f', 64) }],
            ThemeIntegrityDiagnosticCodes.DuplicateNormalizedPath);
        AssertOracleDiagnosticForFiles(
            vector,
            [.. vector.Request.IntegrityManifest.Files, declared with { CanonicalPath = "Assets/payload.bin" }],
            ThemeIntegrityDiagnosticCodes.CaseInsensitivePathCollision);
        AssertOracleDiagnosticForFiles(
            vector,
            [.. vector.Request.IntegrityManifest.Files, declared with { CanonicalPath = "assets/e\u0301.png" }, declared with { CanonicalPath = "assets/é.png" }],
            ThemeIntegrityDiagnosticCodes.DuplicateNormalizedPath);
    }

    [Fact]
    [Trait("Defect", "P4A-VAL-003")]
    public void ReservedMetadataPathsAreTypedAndRequestIdentityMustMatchManifestIdentity()
    {
        Assert.Equal(typeof(ThemeCanonicalPath), typeof(ThemeIntegrityVerificationRequestV2).GetProperty("ThemeManifestPath")!.PropertyType);
        Assert.Equal(typeof(ThemeCanonicalPath), typeof(ThemeIntegrityVerificationRequestV2).GetProperty("IntegrityManifestPath")!.PropertyType);
        PropertyInfo signaturePath = typeof(ThemeIntegrityVerificationRequestV2).GetProperty("SignatureEnvelopePath")!;
        Assert.Equal(typeof(ThemeCanonicalPath), signaturePath.PropertyType);
        Assert.Equal(NullabilityState.Nullable, new NullabilityInfoContext().Create(signaturePath).ReadState);
        Assert.False(ThemeCanonicalPath.TryCreate("../metadata/integrity.json", out _));

        OracleVector vector = CreateValidOracleVector();
        JsonSerializerOptions options = ThemeContractJson.CreateSerializerOptions();
        JsonObject invalidPathRequest = Assert.IsType<JsonObject>(
            JsonNode.Parse(JsonSerializer.Serialize(vector.Request, options)));
        invalidPathRequest["integrity_manifest_path"] = "../metadata/integrity.json";
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<ThemeIntegrityVerificationRequestV2>(invalidPathRequest.ToJsonString(), options));

        ThemeIntegrityVerificationResultV2 themeMismatch = ThemeIntegrityContractConformanceOracle.Evaluate(
            vector.Request with { ThemeId = new ThemeId("com.other.theme") },
            vector.Entries,
            vector.Contents);
        Assert.Contains(ThemeIntegrityDiagnosticCodes.ThemeIdentityMismatch, themeMismatch.Diagnostics.Select(item => item.Code));

        ThemeIntegrityVerificationResultV2 versionMismatch = ThemeIntegrityContractConformanceOracle.Evaluate(
            vector.Request with { Version = new ThemeVersion("9.9.9") },
            vector.Entries,
            vector.Contents);
        Assert.Contains(ThemeIntegrityDiagnosticCodes.ThemeVersionMismatch, versionMismatch.Diagnostics.Select(item => item.Code));
    }

    [Fact]
    [Trait("Defect", "P4A-VAL-003")]
    public void PackageRefIsProvenanceOnlyAndDoesNotChangeSignedContentIdentity()
    {
        OracleVector vector = CreateValidOracleVector();
        ThemeIntegrityVerificationResultV2 original = ThemeIntegrityContractConformanceOracle.Evaluate(
            vector.Request,
            vector.Entries,
            vector.Contents);
        ThemeIntegrityVerificationResultV2 relocated = ThemeIntegrityContractConformanceOracle.Evaluate(
            vector.Request with { PackageRef = new ThemePackageRef("transport:relocated/source") },
            vector.Entries,
            vector.Contents);

        Assert.True(original.IsVerified);
        Assert.True(relocated.IsVerified);
        Assert.Equal(original.PackageHash, relocated.PackageHash);
        Assert.Equal(original.ThemeManifestHash, relocated.ThemeManifestHash);
        Assert.Equal(original.IntegrityManifestHash, relocated.IntegrityManifestHash);
        Assert.Equal(ThemeSignatureVerificationStatus.Valid, relocated.SignatureStatus);
    }

    [Fact]
    [Trait("Defect", "P4A-VAL-004")]
    public void TrustSnapshotDuplicateAndExpectedPolicyRulesFailClosedWithoutSingleOrDefault()
    {
        OracleVector vector = CreateValidOracleVector();
        ThemeTrustedSignerV1 signer = Assert.Single(vector.Request.TrustSnapshot.TrustedSigners);

        ThemeTrustSnapshotV1 exactDuplicate = vector.Request.TrustSnapshot with
        {
            TrustedSigners = [signer, signer],
        };
        AssertOracleDiagnostic(
            vector,
            vector.Request with { TrustSnapshot = exactDuplicate },
            ThemeIntegrityDiagnosticCodes.DuplicateTrustedSignerIdentity);

        ThemeTrustSnapshotV1 samePublisherDifferentKey = vector.Request.TrustSnapshot with
        {
            TrustedSigners = [signer, signer with { KeyId = "other-key" }],
        };
        Assert.True(ThemeIntegrityContractConformanceOracle.Evaluate(
            vector.Request with { TrustSnapshot = samePublisherDifferentKey }, vector.Entries, vector.Contents).IsVerified);

        ThemeTrustSnapshotV1 differentPublisherSameKey = vector.Request.TrustSnapshot with
        {
            TrustedSigners = [signer, signer with { PublisherId = "publisher.other" }],
        };
        Assert.True(ThemeIntegrityContractConformanceOracle.Evaluate(
            vector.Request with { TrustSnapshot = differentPublisherSameKey }, vector.Entries, vector.Contents).IsVerified);

        ThemeTrustSnapshotV1 revokedTrustedAmbiguity = vector.Request.TrustSnapshot with
        {
            TrustedSigners = [signer, signer with { TrustState = ThemeTrustState.Revoked }],
        };
        AssertOracleDiagnostic(
            vector,
            vector.Request with { TrustSnapshot = revokedTrustedAmbiguity },
            ThemeIntegrityDiagnosticCodes.DuplicateTrustedSignerIdentity);

        AssertOracleDiagnostic(
            vector,
            vector.Request with
            {
                Policy = vector.Request.Policy with { ExpectedTrustPolicyId = "other-trust" },
            },
            ThemeIntegrityDiagnosticCodes.TrustPolicyMismatch);
        AssertOracleDiagnostic(
            vector,
            vector.Request with
            {
                Policy = vector.Request.Policy with { ExpectedTrustPolicyVersion = "2" },
            },
            ThemeIntegrityDiagnosticCodes.TrustPolicyMismatch);
    }

    [Fact]
    [Trait("Defect", "P4A-VAL-005")]
    public void TestOnlyOracleUsesPublicContractsAndIndependentNegativeMutations()
    {
        OracleVector vector = CreateValidOracleVector();
        ThemeIntegrityVerificationResultV2 valid = ThemeIntegrityContractConformanceOracle.Evaluate(
            vector.Request,
            vector.Entries,
            vector.Contents);
        Assert.True(valid.IsVerified);
        Assert.Empty(valid.Diagnostics);

        Dictionary<string, byte[]> corrupted = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        corrupted["assets/payload.bin"] = Encoding.UTF8.GetBytes("independent mutation");
        ThemeIntegrityVerificationResultV2 hashFailure = ThemeIntegrityContractConformanceOracle.Evaluate(
            vector.Request,
            vector.Entries,
            corrupted);
        Assert.False(hashFailure.IsVerified);
        Assert.Contains(ThemeIntegrityDiagnosticCodes.FileLengthMismatch, hashFailure.Diagnostics.Select(item => item.Code));

        ThemePackageContentEntryV1 extraEntry = Present("assets/undeclared.bin");
        Dictionary<string, byte[]> withExtraContent = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        withExtraContent[extraEntry.LogicalPath] = [0x01];
        ThemeIntegrityVerificationResultV2 inventoryFailure = ThemeIntegrityContractConformanceOracle.Evaluate(
            vector.Request,
            [.. vector.Entries, extraEntry],
            withExtraContent);
        Assert.Contains(ThemeIntegrityDiagnosticCodes.UndeclaredFile, inventoryFailure.Diagnostics.Select(item => item.Code));

        AssertOracleDiagnostic(
            vector,
            vector.Request with
            {
                SignatureEnvelope = vector.Request.SignatureEnvelope! with { KeyId = "unknown-key" },
            },
            ThemeIntegrityDiagnosticCodes.UnknownSigner);
        AssertOracleDiagnostic(
            vector,
            vector.Request with
            {
                Policy = vector.Request.Policy with { SignatureRequirement = ThemeSignatureRequirement.Optional },
            },
            ThemeIntegrityDiagnosticCodes.ChannelPolicyViolation);
        AssertOracleDiagnostic(
            vector,
            vector.Request with { TrustSnapshot = null! },
            ThemeIntegrityDiagnosticCodes.MissingSecurityField);
    }

    [Fact]
    [Trait("Defect", "P4A-VAL-006")]
    public void DiagnosticVocabularyIsMechanicalClosedAndFullyMapped()
    {
        string[] declaredCodes = typeof(ThemeIntegrityDiagnosticCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => Assert.IsType<string>(field.GetRawConstantValue()))
            .Order(StringComparer.Ordinal)
            .ToArray();
        ThemeIntegrityDiagnosticDefinition[] active = ThemeIntegrityDiagnosticVocabulary.ActiveDefinitions.ToArray();

        Assert.Equal(active.Length, active.Select(definition => definition.SemanticRule).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(active.Length, active.Select(definition => definition.Code).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(declaredCodes, active.Select(definition => definition.Code).Order(StringComparer.Ordinal));
        Assert.All(active, definition => Assert.Matches("^P4I[0-9]{3}$", definition.Code));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ThemeIntegrityDiagnosticV1("P4I999", ThemeDiagnosticsSeverity.Error, null, "unknown"));

        JsonSerializerOptions options = ThemeContractJson.CreateSerializerOptions();
        JsonObject serialized = Assert.IsType<JsonObject>(JsonNode.Parse(JsonSerializer.Serialize(
            new ThemeIntegrityDiagnosticV1(
                ThemeIntegrityDiagnosticCodes.InvalidSignature,
                ThemeDiagnosticsSeverity.Error,
                null,
                "invalid signature"),
            options)));
        serialized["code"] = "P4I999";
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            JsonSerializer.Deserialize<ThemeIntegrityDiagnosticV1>(serialized.ToJsonString(), options));
    }

    [Fact]
    [Trait("Defect", "P4A-VAL-006")]
    public void ContractReferenceVectorIsIdenticalAcrossFiftyRuns()
    {
        OracleVector vector = CreateValidOracleVector();
        ThemeIntegrityVerificationRequestV2 request = vector.Request with
        {
            Policy = vector.Request.Policy with { SignatureRequirement = ThemeSignatureRequirement.Optional },
        };
        ThemePackageContentEntryV1 extra = Present("assets/undeclared.bin");
        Dictionary<string, byte[]> contents = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        contents[extra.LogicalPath] = [0x01];
        contents["assets/payload.bin"] = Encoding.UTF8.GetBytes("PAYLOAD");
        JsonSerializerOptions options = ThemeContractJson.CreateSerializerOptions();
        ThemeIntegrityVerificationResultV2 first = ThemeIntegrityContractConformanceOracle.Evaluate(
            request,
            [.. vector.Entries, extra],
            contents);
        string expected = JsonSerializer.Serialize(first, options);
        Assert.False(first.IsVerified);
        Assert.NotEmpty(first.Diagnostics);
        Assert.NotNull(first.PackageHash);
        Assert.NotNull(first.ThemeManifestHash);
        Assert.NotNull(first.IntegrityManifestHash);
        Assert.NotEmpty(first.FileEvidence);
        Assert.NotEmpty(first.AssetEvidence);
        Assert.Equal(ThemeSignatureVerificationStatus.Invalid, first.SignatureStatus);
        string validExpected = JsonSerializer.Serialize(Evaluate(vector), options);

        for (int iteration = 0; iteration < 50; iteration++)
        {
            ThemeIntegrityVerificationResultV2 repeated = ThemeIntegrityContractConformanceOracle.Evaluate(
                request,
                new[] { extra }.Concat(vector.Entries.Reverse()).ToArray(),
                contents);
            Assert.Equal(expected, JsonSerializer.Serialize(repeated, options));
            Assert.Equal(first.IsVerified, repeated.IsVerified);
            Assert.Equal(first.PackageHash, repeated.PackageHash);
            Assert.Equal(first.ThemeManifestHash, repeated.ThemeManifestHash);
            Assert.Equal(first.IntegrityManifestHash, repeated.IntegrityManifestHash);
            Assert.Equal(first.Diagnostics.Count, repeated.Diagnostics.Count);
            Assert.Equal(first.Diagnostics.Select(item => item.Code), repeated.Diagnostics.Select(item => item.Code));
            Assert.Equal(first.FileEvidence.Select(item => item.CanonicalPath), repeated.FileEvidence.Select(item => item.CanonicalPath));
            Assert.Equal(first.Diagnostics.Select(item => item.Message), repeated.Diagnostics.Select(item => item.Message));
            ThemeIntegrityVerificationResultV2 validRepeated = Evaluate(vector);
            Assert.True(validRepeated.IsVerified);
            Assert.Equal(validExpected, JsonSerializer.Serialize(validRepeated, options));
        }
    }

    [Fact]
    [Trait("Defect", "P4A-REVAL-002")]
    public void EveryActiveDiagnosticRuleIsActuallyEmittedAndNoUnknownOrUnusedCodesRemain()
    {
        OracleVector vector = CreateValidOracleVector();
        ThemeIntegrityVerificationRequestV2 request = vector.Request;
        ThemeTrustedSignerV1 signer = request.TrustSnapshot.TrustedSigners[0];
        HashSet<string> exercised = new(StringComparer.Ordinal);
        void Observe(string code, ThemeIntegrityVerificationRequestV2? mutation = null,
            IReadOnlyList<ThemePackageContentEntryV1>? entries = null, IReadOnlyDictionary<string, byte[]>? contents = null)
        {
            ThemeIntegrityVerificationRequestV2 input = mutation ?? request;
            ThemeIntegrityVerificationResultV2 result = ThemeIntegrityContractConformanceOracle.Evaluate(
                input, entries ?? vector.Entries, contents ?? BindMetadata(vector, input));
            AssertStructuredFailure(result, code);
            exercised.Add(code);
        }
        Dictionary<string, byte[]> Without(string path) => vector.Contents.Where(pair => pair.Key != path)
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        Observe(ThemeIntegrityDiagnosticCodes.UnsupportedSchemaVersion,
            request with { IntegrityManifest = request.IntegrityManifest with { SchemaVersion = "9.0" } });
        Observe(ThemeIntegrityDiagnosticCodes.InvalidCanonicalPath,
            request with { IntegrityManifest = request.IntegrityManifest with { Files = [request.IntegrityManifest.Files[0] with { CanonicalPath = "../escape" }] } });
        Observe(ThemeIntegrityDiagnosticCodes.DuplicateNormalizedPath,
            request with { IntegrityManifest = request.IntegrityManifest with { Files = [.. request.IntegrityManifest.Files, request.IntegrityManifest.Files[0]] } });
        Observe(ThemeIntegrityDiagnosticCodes.CaseInsensitivePathCollision,
            request with { IntegrityManifest = request.IntegrityManifest with { Files = [.. request.IntegrityManifest.Files, request.IntegrityManifest.Files[0] with { CanonicalPath = "Theme.json" }] } });
        Observe(ThemeIntegrityDiagnosticCodes.MissingRequiredFile,
            entries: vector.Entries.Where(entry => entry.LogicalPath != "assets/payload.bin").ToArray(), contents: Without("assets/payload.bin"));
        Dictionary<string, byte[]> extra = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value);
        extra["extra.bin"] = [1];
        Observe(ThemeIntegrityDiagnosticCodes.UndeclaredFile, entries: [.. vector.Entries, Present("extra.bin")], contents: extra);
        Observe(ThemeIntegrityDiagnosticCodes.FileLengthMismatch,
            entries: vector.Entries.Select(entry => entry with { LengthBytes = entry.LengthBytes + 1 }).ToArray());
        Dictionary<string, byte[]> corrupt = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value);
        corrupt["assets/payload.bin"] = Encoding.UTF8.GetBytes("PAYLOAD");
        Observe(ThemeIntegrityDiagnosticCodes.FileHashMismatch, contents: corrupt);
        Observe(ThemeIntegrityDiagnosticCodes.PackageHashMismatch,
            request with { IntegrityManifest = request.IntegrityManifest with { PackageHash = new string('f', 64) } });
        Dictionary<string, byte[]> changedTheme = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value);
        changedTheme["theme.json"] = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(changedTheme["theme.json"]).Replace("Example Theme", "Changed Theme", StringComparison.Ordinal));
        ThemeIntegrityVerificationRequestV2 changedLength = request with { IntegrityManifest = request.IntegrityManifest with
        {
            Files = request.IntegrityManifest.Files.Select(file => file.CanonicalPath == "theme.json"
                ? file with { LengthBytes = changedTheme["theme.json"].LongLength } : file).ToArray(),
        } };
        changedTheme[IntegrityManifestPath] = JsonSerializer.SerializeToUtf8Bytes(changedLength.IntegrityManifest, ThemeContractJson.CreateSerializerOptions());
        Observe(ThemeIntegrityDiagnosticCodes.ThemeManifestHashMismatch, changedLength,
            vector.Entries.Select(entry => entry.LogicalPath == "theme.json" ? entry with { LengthBytes = changedTheme["theme.json"].LongLength } : entry).ToArray(), changedTheme);
        Observe(ThemeIntegrityDiagnosticCodes.IntegrityManifestHashUnavailable, contents: Without(IntegrityManifestPath));
        Observe(ThemeIntegrityDiagnosticCodes.MissingSignature, request with { SignatureEnvelope = null },
            vector.Entries.Where(entry => entry.LogicalPath != SignatureEnvelopePath).ToArray(), Without(SignatureEnvelopePath));
        Observe(ThemeIntegrityDiagnosticCodes.InvalidSignature, request with { SignatureEnvelope = request.SignatureEnvelope! with { Signature = "YQ==" } });
        Observe(ThemeIntegrityDiagnosticCodes.UnknownSigner, request with { SignatureEnvelope = request.SignatureEnvelope! with { KeyId = "unknown" } });
        Observe(ThemeIntegrityDiagnosticCodes.RevokedSigner, request with { TrustSnapshot = request.TrustSnapshot with { TrustedSigners = [signer with { TrustState = ThemeTrustState.Revoked }] } });
        Observe(ThemeIntegrityDiagnosticCodes.UnsupportedSignatureAlgorithm, request with { SignatureEnvelope = request.SignatureEnvelope! with { Algorithm = (ThemeSignatureAlgorithm)999 } });
        Observe(ThemeIntegrityDiagnosticCodes.ChannelPolicyViolation, request with { Policy = request.Policy with { SignatureRequirement = ThemeSignatureRequirement.Optional } });
        Observe(ThemeIntegrityDiagnosticCodes.UnauthorizedDeveloperException, request with { Policy = request.Policy with { DeveloperExceptionAuthorized = true } });
        Observe(ThemeIntegrityDiagnosticCodes.UnsupportedPackageEntry, entries: [.. vector.Entries, Present("link.bin") with { EntryKind = ThemePackageContentEntryKind.SymbolicLink }]);
        Observe(ThemeIntegrityDiagnosticCodes.MissingSecurityField, request with { IntegrityManifest = request.IntegrityManifest with { Files = [null!] } });
        Observe(ThemeIntegrityDiagnosticCodes.PackageReadFailure, contents: Without("assets/payload.bin"));
        Observe(ThemeIntegrityDiagnosticCodes.DuplicateTrustedSignerIdentity, request with { TrustSnapshot = request.TrustSnapshot with { TrustedSigners = [signer, signer] } });
        Observe(ThemeIntegrityDiagnosticCodes.TrustPolicyMismatch, request with { Policy = request.Policy with { ExpectedTrustPolicyId = "other" } });
        Observe(ThemeIntegrityDiagnosticCodes.ThemeIdentityMismatch, request with { ThemeId = new("com.other.theme") });
        Observe(ThemeIntegrityDiagnosticCodes.ThemeVersionMismatch, request with { Version = new("9.9.9") });
        Observe(ThemeIntegrityDiagnosticCodes.UnsupportedSignatureEncoding, request with { SignatureEnvelope = request.SignatureEnvelope! with { SignatureEncoding = (ThemeSignatureEncoding)999 } });
        Dictionary<string, byte[]> invalidRaw = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value);
        invalidRaw[IntegrityManifestPath] = Encoding.UTF8.GetBytes("{}");
        Observe(ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument, contents: invalidRaw);
        Observe(ThemeIntegrityDiagnosticCodes.MetadataDtoMismatch,
            request with { IntegrityManifest = request.IntegrityManifest with { PackageHash = new string('f', 64) } }, contents: vector.Contents);
        Observe(ThemeIntegrityDiagnosticCodes.InvalidSecurityValue,
            request with { IntegrityManifest = request.IntegrityManifest with { Files = [request.IntegrityManifest.Files[0] with { EntryKind = (ThemeIntegrityEntryKind)999 }] } });
        Assert.Equal(ThemeIntegrityDiagnosticVocabulary.ActiveDefinitions.Select(item => item.Code).Order(StringComparer.Ordinal), exercised.Order(StringComparer.Ordinal));
    }

    [Theory]
    [Trait("Defect", "P4A-REVAL-001")]
    [InlineData("1.2.840.10045.3.1.7", true)]
    [InlineData("1.3.132.0.10", false)]
    [InlineData("1.3.36.3.3.2.8.1.1.7", false)]
    [InlineData("1.3.132.0.34", false)]
    public void ExactCurveIdentityControlsCryptographicallyValidSignatures(string oid, bool approved)
    {
        using ECDsa key = ECDsa.Create(ECCurve.CreateFromValue(oid));
        Assert.Equal(oid, key.ExportParameters(false).Curve.Oid.Value);
        OracleVector vector = CreateValidOracleVector();
        vector = IndependentlySignRaw(vector, key, vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value));
        ThemeIntegrityVerificationResultV2 result = Evaluate(vector);
        Assert.Equal(approved, result.IsVerified);
        if (approved) Assert.Equal(ThemeSignatureVerificationStatus.Valid, result.SignatureStatus);
        else Assert.Contains(result.Diagnostics, item => item.Code ==
            (key.KeySize == 256 ? ThemeIntegrityDiagnosticCodes.UnsupportedSignatureAlgorithm : ThemeIntegrityDiagnosticCodes.InvalidSignature));
    }

    [Fact]
    [Trait("Defect", "P4A-REVAL-001")]
    public void UnknownExplicitMalformedAndTrailingSpkiFailClosed()
    {
        OracleVector vector = CreateValidOracleVector();
        ThemeTrustedSignerV1 signer = vector.Request.TrustSnapshot.TrustedSigners[0];
        byte[] approved = Convert.FromBase64String(signer.PublicKey);
        byte[] unknown = (byte[])approved.Clone();
        // The final octet of the named-curve OID, not a friendly name or key-size label.
        byte[] curveOid = [0x06, 0x08, 0x2a, 0x86, 0x48, 0xce, 0x3d, 0x03, 0x01, 0x07];
        int offset = unknown.AsSpan().IndexOf(curveOid);
        Assert.True(offset >= 0);
        unknown[offset + curveOid.Length - 1] = 0x7f;
        using ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        byte[] explicitSpki = CreateExplicitSpki(key.ExportExplicitParameters(false));
        foreach (byte[] invalid in new byte[][] { [], [0x30, 0xff], unknown, explicitSpki, [.. approved, 0] })
        {
            AssertOracleDiagnostic(vector, vector.Request with
            {
                TrustSnapshot = vector.Request.TrustSnapshot with
                {
                    TrustedSigners = [signer with { PublicKey = Convert.ToBase64String(invalid) }],
                },
            }, string.IsNullOrEmpty(Convert.ToBase64String(invalid))
                ? ThemeIntegrityDiagnosticCodes.InvalidSecurityValue : ThemeIntegrityDiagnosticCodes.UnsupportedSignatureAlgorithm);
        }
        OracleVector explicitlyEncoded = IndependentlySignRaw(vector, key,
            vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value));
        Assert.True(Evaluate(explicitlyEncoded).IsVerified);
        AssertOracleDiagnostic(explicitlyEncoded, explicitlyEncoded.Request with
        {
            TrustSnapshot = explicitlyEncoded.Request.TrustSnapshot with
            {
                TrustedSigners = [explicitlyEncoded.Request.TrustSnapshot.TrustedSigners[0] with { PublicKey = Convert.ToBase64String(explicitSpki) }],
            },
        }, ThemeIntegrityDiagnosticCodes.UnsupportedSignatureAlgorithm);
        byte[] malformedPoint = (byte[])approved.Clone();
        malformedPoint.AsSpan(malformedPoint.Length - 64).Clear();
        foreach (string invalidKey in new[] { "not-base64", Convert.ToBase64String(malformedPoint) })
            AssertOracleDiagnostic(vector, vector.Request with
            {
                TrustSnapshot = vector.Request.TrustSnapshot with { TrustedSigners = [signer with { PublicKey = invalidKey }] },
            }, ThemeIntegrityDiagnosticCodes.InvalidSignature);
    }

    [Theory]
    [Trait("Defect", "P4A-REVAL-002")]
    [InlineData("theme_id", "INVALID")]
    [InlineData("version", "not-semver")]
    public void IndependentlyResignedInvalidRawIdentityFailsSchemaAndSemanticValidation(string field, string invalid)
    {
        OracleVector vector = CreateValidOracleVector();
        ThemeIntegrityManifestV2 manifest = field == "theme_id"
            ? vector.Request.IntegrityManifest with { ThemeId = invalid }
            : vector.Request.IntegrityManifest with { Version = invalid };
        vector = vector with { Request = vector.Request with
        {
            IntegrityManifest = manifest, ThemeId = new(manifest.ThemeId), Version = new(manifest.Version),
        } };
        Dictionary<string, byte[]> contents = BindMetadata(vector, vector.Request);
        JsonNode theme = JsonNode.Parse(contents["theme.json"])!;
        theme["package"]![field] = invalid;
        contents["theme.json"] = Encoding.UTF8.GetBytes(theme.ToJsonString());
        // Keep every hash/length valid so the identity grammar is the rejecting stage.
        manifest = manifest with
        {
            Files = manifest.Files.Select(file => file.CanonicalPath == "theme.json"
                ? file with { Sha256 = IndependentHash(contents["theme.json"]), LengthBytes = contents["theme.json"].LongLength } : file).ToArray(),
            PackageHash = IndependentPackageHash(contents),
        };
        vector = vector with { Request = vector.Request with { IntegrityManifest = manifest } };
        contents[IntegrityManifestPath] = JsonSerializer.SerializeToUtf8Bytes(manifest, ThemeContractJson.CreateSerializerOptions());
        using ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        vector = IndependentlySignRaw(vector, key, contents);
        AssertStructuredFailure(Evaluate(vector), ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument);
        AssertStructuredFailure(Evaluate(vector), ThemeIntegrityDiagnosticCodes.InvalidSecurityValue);
    }

    [Fact]
    [Trait("Defect", "P4A-REVAL-002")]
    public void SignedEmptyRawIntegrityCannotBeReplacedByValidCallerDto()
    {
        OracleVector vector = CreateValidOracleVector();
        Dictionary<string, byte[]> contents = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value);
        contents[IntegrityManifestPath] = Encoding.UTF8.GetBytes("{}");
        using ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        vector = IndependentlySignRaw(vector, key, contents);
        AssertStructuredFailure(Evaluate(vector), ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument);
    }

    [Fact]
    [Trait("Defect", "P4A-REVAL-002")]
    public void RawDtoBindingRejectsBothMetadataSubstitutionsAndAcceptsEquivalentFormatting()
    {
        OracleVector vector = CreateValidOracleVector();
        AssertStructuredFailure(Evaluate(vector with { Request = vector.Request with
        {
            IntegrityManifest = vector.Request.IntegrityManifest with
            {
                Files = vector.Request.IntegrityManifest.Files.Select(file => file with { Required = false }).ToArray(),
            },
        } }), ThemeIntegrityDiagnosticCodes.MetadataDtoMismatch);

        Dictionary<string, byte[]> substituted = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value);
        substituted[SignatureEnvelopePath] = JsonSerializer.SerializeToUtf8Bytes(
            vector.Request.SignatureEnvelope! with { Signature = Convert.ToBase64String(new byte[64]) },
            ThemeContractJson.CreateSerializerOptions());
        AssertStructuredFailure(ThemeIntegrityContractConformanceOracle.Evaluate(vector.Request, vector.Entries, substituted),
            ThemeIntegrityDiagnosticCodes.MetadataDtoMismatch);
        AssertStructuredFailure(Evaluate(vector with { Request = vector.Request with
        {
            SignatureEnvelope = vector.Request.SignatureEnvelope! with { KeyId = "substituted" },
        } }), ThemeIntegrityDiagnosticCodes.MetadataDtoMismatch);

        Dictionary<string, byte[]> contents = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value);
        contents[IntegrityManifestPath] = Encoding.UTF8.GetBytes("\n " + Encoding.UTF8.GetString(contents[IntegrityManifestPath]) + " \n");
        using ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        vector = IndependentlySignRaw(vector, key, contents);
        ThemeIntegrityVerificationResultV2 result = Evaluate(vector);
        Assert.True(result.IsVerified);
        Assert.Equal(IndependentHash(contents[IntegrityManifestPath]), result.IntegrityManifestHash);
        Assert.Equal(IndependentHash(contents["theme.json"]), result.ThemeManifestHash);
    }

    [Theory]
    [Trait("Defect", "P4A-REVAL-002")]
    [InlineData("ThemeIntegrity.v2.schema.json", "schema_version")]
    [InlineData("ThemeSignatureEnvelope.v1.schema.json", "envelope_version")]
    public void ActualRepositorySchemaMutationChangesOracleOutcome(string schemaName, string property)
    {
        OracleVector vector = CreateValidOracleVector();
        Assert.True(Evaluate(vector).IsVerified);
        int mutations = 0;
        ThemeIntegrityVerificationResultV2 result = ThemeIntegrityContractConformanceOracle.Evaluate(
            vector.Request, vector.Entries, vector.Contents, (name, actualSchema) =>
            {
                if (name != schemaName) return actualSchema;
                mutations++;
                JsonNode mutation = JsonNode.Parse(actualSchema.GetRawText())!;
                mutation["properties"]![property]!["const"] = "mutation-rejects-valid-document";
                using JsonDocument changed = JsonDocument.Parse(mutation.ToJsonString());
                return changed.RootElement.Clone();
            });
        Assert.Equal(1, mutations);
        AssertStructuredFailure(result, ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument);
        Assert.True(Evaluate(vector).IsVerified);
    }

    [Fact]
    [Trait("Defect", "P4A-REVAL-002")]
    public void AuthoritativeDeserializerAndSealedManifestSemanticsAreExecutablePipelineStages()
    {
        OracleVector vector = CreateValidOracleVector();
        Dictionary<string, byte[]> contents = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value);
        JsonNode envelope = JsonNode.Parse(contents[SignatureEnvelopePath])!;
        envelope["algorithm"] = 0;
        contents[SignatureEnvelopePath] = Encoding.UTF8.GetBytes(envelope.ToJsonString());
        ThemeIntegrityVerificationResultV2 serializationFailure = ThemeIntegrityContractConformanceOracle.Evaluate(
            vector.Request, vector.Entries, contents, (name, actual) =>
            {
                if (name != "ThemeSignatureEnvelope.v1.schema.json") return actual;
                JsonNode changed = JsonNode.Parse(actual.GetRawText())!;
                changed["properties"]!["algorithm"] = new JsonObject { ["type"] = "integer" };
                using JsonDocument schema = JsonDocument.Parse(changed.ToJsonString());
                using JsonDocument raw = JsonDocument.Parse(contents[SignatureEnvelopePath]);
                Assert.Empty(JsonSchemaSubsetValidator.Validate(schema.RootElement, raw.RootElement));
                return schema.RootElement.Clone();
            });
        AssertStructuredFailure(serializationFailure, ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument);
        Assert.Contains(serializationFailure.Diagnostics, item => item.Message.Contains("serializer", StringComparison.Ordinal));

        contents = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value);
        JsonNode theme = JsonNode.Parse(contents["theme.json"])!;
        theme["variants"]![0]!["default"] = false;
        contents["theme.json"] = Encoding.UTF8.GetBytes(theme.ToJsonString());
        AssertSchemaValid("ThemeManifest.schema.json", Encoding.UTF8.GetString(contents["theme.json"]));
        ThemeIntegrityVerificationResultV2 semanticFailure = ThemeIntegrityContractConformanceOracle.Evaluate(vector.Request, vector.Entries, contents);
        AssertStructuredFailure(semanticFailure, ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument);
        Assert.Contains(semanticFailure.Diagnostics, item => item.Message.Contains("semantic", StringComparison.Ordinal));
    }

    [Fact]
    [Trait("Defect", "P4A-REVAL-003")]
    public void HostileRawMetadataParsingNeverEscapesAndCannotBecomeUnsignedAbsence()
    {
        OracleVector vector = CreateValidOracleVector();
        foreach (string path in new[] { "theme.json", IntegrityManifestPath, SignatureEnvelopePath })
        {
            foreach (byte[] bytes in new byte[][] { [], [0xff], Encoding.UTF8.GetBytes("{"), Encoding.UTF8.GetBytes("null"), Encoding.UTF8.GetBytes("{}") })
            {
                Dictionary<string, byte[]> contents = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value);
                contents[path] = bytes;
                ThemeIntegrityVerificationRequestV2 request = vector.Request with
                {
                    Policy = vector.Request.Policy with { DistributionChannel = ThemeDistributionChannel.Developer,
                        SignatureRequirement = ThemeSignatureRequirement.Optional, DeveloperExceptionAuthorized = true },
                };
                AssertStructuredFailure(ThemeIntegrityContractConformanceOracle.Evaluate(request, vector.Entries, contents),
                    ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument);
            }
        }
        Dictionary<string, byte[]> raw = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value);
        raw[IntegrityManifestPath] = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(raw[IntegrityManifestPath])
            .Replace("\"schema_version\":\"2.0\"", "\"schema_version\":\"2.0\",\"schema_version\":\"2.0\"", StringComparison.Ordinal));
        AssertStructuredFailure(ThemeIntegrityContractConformanceOracle.Evaluate(vector.Request, vector.Entries, raw),
            ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument);
    }

    [Fact]
    [Trait("Defect", "P4A-REVAL-002")]
    public void DirectUndefinedSecurityEnumsCannotBypassTheJsonBoundary()
    {
        OracleVector vector = CreateValidOracleVector();
        ThemeIntegrityVerificationRequestV2 request = vector.Request;
        ThemeTrustedSignerV1 signer = request.TrustSnapshot.TrustedSigners[0];
        ThemeIntegrityVerificationRequestV2[] mutations =
        [
            request with { IntegrityManifest = request.IntegrityManifest with { Files = request.IntegrityManifest.Files.Select(file => file with { EntryKind = (ThemeIntegrityEntryKind)999 }).ToArray() } },
            request with { IntegrityManifest = request.IntegrityManifest with { InventoryMode = (ThemeIntegrityInventoryMode)999 } },
            request with { IntegrityManifest = request.IntegrityManifest with { HashAlgorithm = "unknown" } },
            request with { SignatureEnvelope = request.SignatureEnvelope! with { Algorithm = (ThemeSignatureAlgorithm)999 } },
            request with { SignatureEnvelope = request.SignatureEnvelope! with { SignatureEncoding = (ThemeSignatureEncoding)999 } },
            request with { SignatureEnvelope = request.SignatureEnvelope! with { SignedPayloadType = (ThemeSignedPayloadType)999 } },
            request with { TrustSnapshot = request.TrustSnapshot with { TrustedSigners = [signer with { TrustState = (ThemeTrustState)999 }] } },
            request with { TrustSnapshot = request.TrustSnapshot with { TrustedSigners = [signer with { Algorithm = (ThemeSignatureAlgorithm)999 }] } },
            request with { TrustSnapshot = request.TrustSnapshot with { TrustedSigners = [signer with { PublicKeyEncoding = (ThemePublicKeyEncoding)999 }] } },
            request with { Policy = request.Policy with { DistributionChannel = (ThemeDistributionChannel)999 } },
            request with { Policy = request.Policy with { SignatureRequirement = (ThemeSignatureRequirement)999 } },
            request with { ThemeId = new("INVALID") },
            request with { Version = new("not-semver") },
        ];
        foreach (ThemeIntegrityVerificationRequestV2 mutation in mutations)
            AssertStructuredFailure(ThemeIntegrityContractConformanceOracle.Evaluate(mutation, vector.Entries, vector.Contents));
        AssertStructuredFailure(ThemeIntegrityContractConformanceOracle.Evaluate(mutations[0], vector.Entries, vector.Contents),
            ThemeIntegrityDiagnosticCodes.InvalidSecurityValue);
        AssertStructuredFailure(ThemeIntegrityContractConformanceOracle.Evaluate(request,
            [.. vector.Entries, Present("other.bin") with { EntryKind = (ThemePackageContentEntryKind)999 }], vector.Contents),
            ThemeIntegrityDiagnosticCodes.UnsupportedPackageEntry);

        Dictionary<string, byte[]> raw = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value);
        JsonNode document = JsonNode.Parse(raw[IntegrityManifestPath])!;
        document["files"]![0]!["entry_kind"] = 999;
        raw[IntegrityManifestPath] = Encoding.UTF8.GetBytes(document.ToJsonString());
        AssertStructuredFailure(ThemeIntegrityContractConformanceOracle.Evaluate(request, vector.Entries, raw),
            ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument);
    }

    [Fact]
    [Trait("Defect", "P4A-REVAL-003")]
    public void MalformedUnicodeProducesStructuredDeterministicRejection()
    {
        OracleVector vector = CreateValidOracleVector();
        foreach (string path in new[] { "assets/\uD800.bin", "assets/\uDC00.bin", "assets/\uD800x.bin" })
        {
            Assert.False(ThemeCanonicalPath.TryCreate(path, out _));
            ThemeIntegrityVerificationRequestV2 request = vector.Request with
            {
                IntegrityManifest = vector.Request.IntegrityManifest with
                {
                    Files = [vector.Request.IntegrityManifest.Files[0] with { CanonicalPath = path }],
                },
            };
            string? first = null;
            for (int run = 0; run < 50; run++)
            {
                ThemeIntegrityVerificationResultV2 result = ThemeIntegrityContractConformanceOracle.Evaluate(
                    request, [.. vector.Entries, Present(path)], vector.Contents);
                AssertStructuredFailure(result, ThemeIntegrityDiagnosticCodes.InvalidCanonicalPath);
                string serialized = JsonSerializer.Serialize(result, ThemeContractJson.CreateSerializerOptions());
                first ??= serialized;
                Assert.Equal(first, serialized);
            }
        }
    }

    [Fact]
    [Trait("Defect", "P4A-REVAL-003")]
    public void NullSecurityCollectionsAndItemsAreContained()
    {
        OracleVector vector = CreateValidOracleVector();
        ThemeIntegrityVerificationRequestV2 request = vector.Request;
        foreach (ThemeIntegrityVerificationRequestV2 malformed in new ThemeIntegrityVerificationRequestV2[]
        {
            null!, request with { IntegrityManifest = null! },
            request with { IntegrityManifest = request.IntegrityManifest with { Files = null! } },
            request with { IntegrityManifest = request.IntegrityManifest with { Files = [null!] } },
            request with { TrustSnapshot = request.TrustSnapshot with { TrustedSigners = [null!] } },
            request with { TrustSnapshot = request.TrustSnapshot with { TrustedSigners = default } },
        })
            AssertStructuredFailure(ThemeIntegrityContractConformanceOracle.Evaluate(malformed, vector.Entries, vector.Contents),
                ThemeIntegrityDiagnosticCodes.MissingSecurityField);
        AssertStructuredFailure(ThemeIntegrityContractConformanceOracle.Evaluate(request, [null!], vector.Contents),
            ThemeIntegrityDiagnosticCodes.MissingSecurityField);
        AssertStructuredFailure(ThemeIntegrityContractConformanceOracle.Evaluate(request, null!, vector.Contents));
        AssertStructuredFailure(ThemeIntegrityContractConformanceOracle.Evaluate(request, vector.Entries, null!));
        Assert.Equal(ThemeIntegrityDiagnosticCodes.MissingSecurityField,
            Assert.Single(ThemeIntegrityContractConformanceOracle.OrderDiagnostics([null!])).Code);
        Dictionary<string, byte[]> contents = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value);
        contents["assets/payload.bin"] = null!;
        AssertStructuredFailure(ThemeIntegrityContractConformanceOracle.Evaluate(request, vector.Entries, contents),
            ThemeIntegrityDiagnosticCodes.PackageReadFailure);
    }

    [Fact]
    [Trait("Defect", "P4A-REVAL-003")]
    public void NullAndMalformedSignatureNeverBecomeDeveloperAbsence()
    {
        OracleVector vector = CreateValidOracleVector();
        foreach (ThemeDistributionChannel channel in new[] { ThemeDistributionChannel.Stable, ThemeDistributionChannel.Developer })
        {
            foreach (string? signature in new string?[] { null, "", "not-base64", "YQ==" })
            {
                ThemeIntegrityVerificationRequestV2 request = vector.Request with
                {
                    Policy = vector.Request.Policy with { DistributionChannel = channel,
                        SignatureRequirement = ThemeSignatureRequirement.Optional, DeveloperExceptionAuthorized = channel == ThemeDistributionChannel.Developer },
                    SignatureEnvelope = vector.Request.SignatureEnvelope! with { Signature = signature! },
                };
                AssertStructuredFailure(ThemeIntegrityContractConformanceOracle.Evaluate(request, vector.Entries, BindMetadata(vector, request)),
                    ThemeIntegrityDiagnosticCodes.InvalidSignature);
            }
        }
    }

    [Theory]
    [Trait("Defect", "P4A-REVAL2-001")]
    [InlineData("1e1000", false)]
    [InlineData("-1e1000", false)]
    [InlineData("79228162514264337593543950336", false)]
    [InlineData("-79228162514264337593543950336", false)]
    [InlineData("1e-1000", false)]
    [InlineData("-1e-1000", false)]
    [InlineData("1.00000000000000000000000000001", false)]
    [InlineData("-79228162514264337593543950335", false)]
    [InlineData("1", true)]
    [InlineData("1e0", true)]
    [InlineData("1.2500", true)]
    [InlineData("1.0000000000000000000000000001", true)]
    [InlineData("79228162514264337593543950335", true)]
    public void HostileAndBoundaryDecimalRawMetadataProducesDeterministicOracleDecisions(string literal, bool valid)
    {
        OracleVector vector = CreateNumericRawVector(literal);
        JsonSerializerOptions options = ThemeContractJson.CreateSerializerOptions();
        string? first = null;
        for (int run = 0; run < 50; run++)
        {
            ThemeIntegrityVerificationResultV2 result = Evaluate(vector);
            Assert.Equal(valid, result.IsVerified);
            if (valid)
            {
                Assert.Empty(result.Diagnostics);
                Assert.Equal(ThemeSignatureVerificationStatus.Valid, result.SignatureStatus);
                Assert.Equal(IndependentHash(vector.Contents["theme.json"]), result.ThemeManifestHash);
                Assert.Equal(IndependentHash(vector.Contents[IntegrityManifestPath]), result.IntegrityManifestHash);
            }
            else
            {
                ThemeIntegrityDiagnosticV1 diagnostic = Assert.Single(result.Diagnostics);
                Assert.Equal(ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument, diagnostic.Code);
                Assert.Equal("theme.json", diagnostic.CanonicalPath);
                Assert.Equal("Raw metadata does not conform to its authoritative schema.", diagnostic.Message);
            }

            string serialized = JsonSerializer.Serialize(result, options);
            first ??= serialized;
            Assert.Equal(first, serialized);
        }
    }

    [Fact]
    [Trait("Defect", "P4A-REVAL2-001")]
    public void DecimalSchemaConversionRejectsUnderflowAndRoundingInsteadOfAcceptingRoundedValues()
    {
        using JsonDocument schema = JsonDocument.Parse("{\"type\":\"number\"}");
        foreach (string literal in new[] { "1e-1000", "-1e-1000", "1.00000000000000000000000000001", "1e1000" })
        {
            using JsonDocument number = JsonDocument.Parse(literal);
            string error = Assert.Single(JsonSchemaSubsetValidator.Validate(schema.RootElement, number.RootElement));
            Assert.Contains("cannot be represented exactly", error, StringComparison.Ordinal);
        }
        foreach (string literal in new[] { "1e-28", "-1e-28", "0", "-0", "1.2500", "79228162514264337593543950335", "-79228162514264337593543950335" })
        {
            using JsonDocument number = JsonDocument.Parse(literal);
            Assert.Empty(JsonSchemaSubsetValidator.Validate(schema.RootElement, number.RootElement));
        }
    }

    [Fact]
    [Trait("Defect", "P4A-REVAL2-001")]
    public void TargetIntegerOverflowIsContainedByActualSchemaValidation()
    {
        OracleVector vector = CreateValidOracleVector();
        Dictionary<string, byte[]> contents = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value);
        JsonNode theme = JsonNode.Parse(contents["theme.json"])!;
        // The actual public field is Int64; its first overflowing integer must reject.
        theme["assets"]!["total_declared_size_bytes"] = JsonNode.Parse("9223372036854775808");
        contents["theme.json"] = Encoding.UTF8.GetBytes(theme.ToJsonString());
        AssertStructuredFailure(ThemeIntegrityContractConformanceOracle.Evaluate(vector.Request, vector.Entries, contents),
            ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument);
    }

    [Theory]
    [Trait("Defect", "P4A-REVAL2-001")]
    [InlineData("minimum")]
    [InlineData("maximum")]
    public void UnrepresentableActualSchemaNumericLimitsFailClosed(string keyword)
    {
        OracleVector vector = CreateNumericRawVector("1.25");
        Assert.True(Evaluate(vector).IsVerified);
        ThemeIntegrityVerificationResultV2 result = ThemeIntegrityContractConformanceOracle.Evaluate(
            vector.Request, vector.Entries, vector.Contents, (name, actual) =>
            {
                if (name != "ThemeManifest.schema.json") return actual;
                JsonNode mutation = JsonNode.Parse(actual.GetRawText())!;
                mutation["properties"]!["compatibility"]!["properties"]!["minimum_dpi_scale"]![keyword] = JsonNode.Parse("1e1000");
                using JsonDocument schema = JsonDocument.Parse(mutation.ToJsonString());
                return schema.RootElement.Clone();
            });
        AssertStructuredFailure(result, ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument);
    }

    private static OracleVector CreateNumericRawVector(string literal)
    {
        OracleVector vector = CreateValidOracleVector();
        Dictionary<string, byte[]> contents = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value);
        JsonNode theme = JsonNode.Parse(contents["theme.json"])!;
        theme["compatibility"]!["minimum_dpi_scale"] = JsonNode.Parse(literal);
        theme["compatibility"]!["maximum_tested_dpi_scale"] = JsonNode.Parse(literal);
        contents["theme.json"] = Encoding.UTF8.GetBytes(theme.ToJsonString());
        // Every raw mutation is re-hashed and independently signed. Numeric validation
        // is the only changed decision input, not a stale hash or signature fixture.
        ThemeIntegrityManifestV2 manifest = vector.Request.IntegrityManifest with
        {
            Files = vector.Request.IntegrityManifest.Files.Select(file => file.CanonicalPath == "theme.json"
                ? file with { Sha256 = IndependentHash(contents["theme.json"]), LengthBytes = contents["theme.json"].LongLength } : file).ToArray(),
            PackageHash = IndependentPackageHash(contents),
        };
        contents[IntegrityManifestPath] = JsonSerializer.SerializeToUtf8Bytes(manifest, ThemeContractJson.CreateSerializerOptions());
        vector = vector with { Request = vector.Request with { IntegrityManifest = manifest } };
        using ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        return IndependentlySignRaw(vector, key, contents);
    }

    private static ThemeIntegrityVerificationResultV2 Evaluate(OracleVector vector) =>
        ThemeIntegrityContractConformanceOracle.Evaluate(vector.Request, vector.Entries, vector.Contents);

    private static void AssertStructuredFailure(ThemeIntegrityVerificationResultV2 result, string? code = null)
    {
        Assert.False(result.IsVerified);
        Assert.NotEmpty(result.Diagnostics);
        Assert.All(result.Diagnostics, item => Assert.True(ThemeIntegrityDiagnosticVocabulary.IsKnownCode(item.Code)));
        if (code is not null) Assert.Contains(code, result.Diagnostics.Select(item => item.Code));
    }

    private static string IndependentHash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static string IndependentPackageHash(Dictionary<string, byte[]> contents)
    {
        string tree = string.Concat(contents.Where(pair => pair.Key != IntegrityManifestPath && pair.Key != SignatureEnvelopePath)
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => pair.Key + "\0" + pair.Value.LongLength.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + "\0" + IndependentHash(pair.Value) + "\n"));
        return IndependentHash(Encoding.UTF8.GetBytes(tree));
    }

    private static OracleVector IndependentlySignRaw(OracleVector vector, ECDsa key, Dictionary<string, byte[]> contents)
    {
        // Deliberately independent from both oracle hash and payload helpers.
        ThemeSignatureEnvelopeV1 envelope = vector.Request.SignatureEnvelope!;
        byte[] payload = Encoding.UTF8.GetBytes(string.Join('\0', "TCC-THEME-PACKAGE-SIGNATURE-V1",
            vector.Request.ThemeId.Value, vector.Request.Version.Value, IndependentPackageHash(contents),
            IndependentHash(contents["theme.json"]), IndependentHash(contents[IntegrityManifestPath]), envelope.PublisherId, envelope.KeyId));
        byte[] signature = key.SignData(payload, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        Assert.True(key.VerifyData(payload, signature, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation));
        envelope = envelope with { Signature = Convert.ToBase64String(signature) };
        contents[SignatureEnvelopePath] = JsonSerializer.SerializeToUtf8Bytes(envelope, ThemeContractJson.CreateSerializerOptions());
        return vector with
        {
            Request = vector.Request with
            {
                SignatureEnvelope = envelope,
                TrustSnapshot = vector.Request.TrustSnapshot with
                {
                    TrustedSigners = [vector.Request.TrustSnapshot.TrustedSigners[0] with { PublicKey = Convert.ToBase64String(key.ExportSubjectPublicKeyInfo()) }],
                },
            },
            Entries = contents.Select(pair => new ThemePackageContentEntryV1(pair.Key, pair.Value.LongLength, ThemePackageContentEntryKind.File, null)).ToArray(),
            Contents = contents,
        };
    }

    private static byte[] CreateExplicitSpki(ECParameters parameters)
    {
        System.Formats.Asn1.AsnWriter writer = new(System.Formats.Asn1.AsnEncodingRules.DER);
        using (writer.PushSequence())
        {
            using (writer.PushSequence())
            {
                writer.WriteObjectIdentifier("1.2.840.10045.2.1");
                using (writer.PushSequence())
                {
                    writer.WriteInteger(1);
                    using (writer.PushSequence())
                    {
                        writer.WriteObjectIdentifier("1.2.840.10045.1.1");
                        writer.WriteIntegerUnsigned(parameters.Curve.Prime!);
                    }
                    using (writer.PushSequence())
                    {
                        writer.WriteOctetString(parameters.Curve.A!);
                        writer.WriteOctetString(parameters.Curve.B!);
                    }
                    writer.WriteOctetString([4, .. parameters.Curve.G.X!, .. parameters.Curve.G.Y!]);
                    writer.WriteIntegerUnsigned(parameters.Curve.Order!);
                    writer.WriteIntegerUnsigned(parameters.Curve.Cofactor!);
                }
            }
            writer.WriteBitString([4, .. parameters.Q.X!, .. parameters.Q.Y!]);
        }
        return writer.Encode();
    }

    private static OracleVector CreateValidOracleVector()
    {
        ThemeManifest themeDocument = new(ContractVersions.Schema, ContractVersions.ThemeApi,
            new ThemePackageIdentity("com.example.theme", "com.example.theme.package", "Example Theme", "Example Publisher",
                "publisher.example", "1.2.3", "stable", "Presentation-only theme.", null, null, "private", ["deep"]),
            new ThemeCompatibilityDeclaration(">=1.0.0 <2.0.0", ">=1.0.0 <2.0.0", ">=1.1.0 <2.0.0", ["windows"], true, 1m, 3m),
            [new ThemeVariant("deep", "deep", "Deep", true, "tokens/deep.json", [], null)],
            ["presentation.tokens"], new ThemeFeatureFlags([]),
            new ThemeDegradedModeDeclaration(true, ["none", "reduced_decoration", "minimal_decoration", "safe_presentation_only"],
                ["safety_core", "critical_alerts", "keyboard", "screen_reader", "contrast", "risk_permission_semantics", "confirmation_semantics", "accessibility"],
                "safe_presentation_only"),
            new ThemeAccessibilityDeclaration(true, true, true, true, true, true, true, true, true, true, true, true),
            new ThemeAssetDeclaration("assets/index.json", ["tier0"], 0, "sha256"), null,
            new ThemeMotionDeclaration("motion/profiles.json", "motion/reduced.json", true, true, true), null,
            new ThemeIntegrityDeclaration(IntegrityManifestPath, SignatureEnvelopePath, "stable_or_store", "sha256"), null);
        byte[] themeManifestBytes = JsonSerializer.SerializeToUtf8Bytes(themeDocument, ThemeContractJson.CreateSerializerOptions());
        byte[] payloadBytes = Encoding.UTF8.GetBytes("payload");
        ThemeIntegrityFileV2 themeManifestFile = new(
            "theme.json",
            ThemeIntegrityEntryKind.ThemeManifest,
            ThemeIntegrityContractConformanceOracle.Sha256Hex(themeManifestBytes),
            themeManifestBytes.LongLength,
            true);
        ThemeIntegrityFileV2 payloadFile = new(
            "assets/payload.bin",
            ThemeIntegrityEntryKind.Payload,
            ThemeIntegrityContractConformanceOracle.Sha256Hex(payloadBytes),
            payloadBytes.LongLength,
            true);
        string packageHash = ThemeIntegrityContractConformanceOracle.ComputePackageHash(
        [
            (themeManifestFile.CanonicalPath, themeManifestBytes),
            (payloadFile.CanonicalPath, payloadBytes),
        ]);
        ThemeIntegrityManifestV2 manifest = new(
            ContractVersions.ThemeIntegritySchemaV2,
            ThemeIntegrityInventoryMode.ExhaustiveAllowedPayload,
            "com.example.theme",
            "1.2.3",
            ThemeIntegrityContractSemantics.HashAlgorithm,
            [themeManifestFile, payloadFile],
            packageHash);
        JsonSerializerOptions options = ThemeContractJson.CreateSerializerOptions();
        byte[] integrityManifestBytes = JsonSerializer.SerializeToUtf8Bytes(manifest, options);

        using ECDsa signer = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        string publisherId = "publisher.example";
        string keyId = "key-2026-01";
        byte[] signaturePayload = ThemeIntegrityContractConformanceOracle.BuildSignaturePayload(
            manifest.ThemeId,
            manifest.Version,
            packageHash,
            ThemeIntegrityContractConformanceOracle.Sha256Hex(themeManifestBytes),
            ThemeIntegrityContractConformanceOracle.Sha256Hex(integrityManifestBytes),
            publisherId,
            keyId);
        byte[] signature = signer.SignData(
            signaturePayload,
            HashAlgorithmName.SHA256,
            DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        ThemeSignatureEnvelopeV1 envelope = new(
            ContractVersions.ThemeSignatureEnvelopeSchemaV1,
            ThemeSignatureAlgorithm.EcdsaP256Sha256,
            publisherId,
            keyId,
            ThemeSignatureEncoding.IeeeP1363FixedFieldConcatenation,
            Convert.ToBase64String(signature),
            ThemeSignedPayloadType.TccThemePackageSignatureV1);
        byte[] signatureEnvelopeBytes = JsonSerializer.SerializeToUtf8Bytes(envelope, options);
        ThemeIntegrityVerificationPolicyV1 policy = new(
            "stable-channel-policy",
            "1",
            "default-trust",
            "1",
            ThemeDistributionChannel.Stable,
            ThemeSignatureRequirement.Required,
            false);
        ThemeTrustSnapshotV1 trust = new(
            "default-trust",
            "1",
            [new ThemeTrustedSignerV1(
                publisherId,
                keyId,
                ThemeSignatureAlgorithm.EcdsaP256Sha256,
                ThemePublicKeyEncoding.SubjectPublicKeyInfo,
                Convert.ToBase64String(signer.ExportSubjectPublicKeyInfo()),
                ThemeTrustState.Trusted)]);
        ThemeIntegrityVerificationRequestV2 request = new(
            new ThemeId(manifest.ThemeId),
            new ThemeVersion(manifest.Version),
            new ThemePackageRef("transport:original/source"),
            ThemeCanonicalPath.Parse("theme.json"),
            ThemeCanonicalPath.Parse(IntegrityManifestPath),
            ThemeCanonicalPath.Parse(SignatureEnvelopePath),
            manifest,
            envelope,
            policy,
            trust);
        ThemePackageContentEntryV1[] entries =
        [
            new(themeManifestFile.CanonicalPath, themeManifestBytes.LongLength, ThemePackageContentEntryKind.File, "application/json"),
            new(payloadFile.CanonicalPath, payloadBytes.LongLength, ThemePackageContentEntryKind.File, "application/octet-stream"),
            new(IntegrityManifestPath, integrityManifestBytes.LongLength, ThemePackageContentEntryKind.File, "application/json"),
            new(SignatureEnvelopePath, signatureEnvelopeBytes.LongLength, ThemePackageContentEntryKind.File, "application/json"),
        ];
        Dictionary<string, byte[]> contents = new(StringComparer.Ordinal)
        {
            [themeManifestFile.CanonicalPath] = themeManifestBytes,
            [payloadFile.CanonicalPath] = payloadBytes,
            [IntegrityManifestPath] = integrityManifestBytes,
            [SignatureEnvelopePath] = signatureEnvelopeBytes,
        };
        return new OracleVector(request, entries, contents);
    }

    private static void AssertOracleDiagnosticForFiles(
        OracleVector vector,
        IReadOnlyList<ThemeIntegrityFileV2> files,
        string expectedCode)
    {
        ThemeIntegrityVerificationRequestV2 request = vector.Request with
        {
            IntegrityManifest = vector.Request.IntegrityManifest with { Files = files },
        };
        AssertOracleDiagnostic(vector, request, expectedCode);
    }

    private static void AssertOracleDiagnostic(
        OracleVector vector,
        ThemeIntegrityVerificationRequestV2 request,
        string expectedCode)
    {
        ThemeIntegrityVerificationResultV2 result = ThemeIntegrityContractConformanceOracle.Evaluate(
            request,
            vector.Entries,
            BindMetadata(vector, request));
        Assert.False(result.IsVerified);
        Assert.Contains(expectedCode, result.Diagnostics.Select(item => item.Code));
    }

    private static Dictionary<string, byte[]> BindMetadata(OracleVector vector, ThemeIntegrityVerificationRequestV2 request)
    {
        Dictionary<string, byte[]> contents = vector.Contents.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        // Deliberately permissive fixture writer can represent hostile enum/null values.
        // The oracle still reads these bytes with the authoritative strict serializer.
        JsonSerializerOptions options = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
        // Negative semantic vectors mutate the raw document too; no helper changes the oracle binding rule.
        if (request.IntegrityManifest is not null)
        {
            contents[IntegrityManifestPath] = JsonSerializer.SerializeToUtf8Bytes(request.IntegrityManifest, options);
        }
        if (request.SignatureEnvelope is not null)
        {
            contents[SignatureEnvelopePath] = JsonSerializer.SerializeToUtf8Bytes(request.SignatureEnvelope, options);
        }
        return contents;
    }

    private sealed record OracleVector(
        ThemeIntegrityVerificationRequestV2 Request,
        IReadOnlyList<ThemePackageContentEntryV1> Entries,
        Dictionary<string, byte[]> Contents);

    private static ThemeIntegrityManifestV2 CreateV2Manifest() => new(
        ContractVersions.ThemeIntegritySchemaV2,
        ThemeIntegrityInventoryMode.ExhaustiveAllowedPayload,
        "com.example.theme",
        "1.2.3",
        ThemeIntegrityContractSemantics.HashAlgorithm,
        [new ThemeIntegrityFileV2("theme.json", ThemeIntegrityEntryKind.ThemeManifest, new string('a', 64), 42, true)],
        new string('b', 64));

    private static ThemeIntegrityFileV2 Declared(string path, bool required) =>
        new(path, ThemeIntegrityEntryKind.Payload, new string('a', 64), 1, required);

    private static ThemePackageContentEntryV1 Present(string path) =>
        new(path, 1, ThemePackageContentEntryKind.File, "application/octet-stream");

    private static ThemeTrustedSignerV1 Signer(string publisherId, string keyId, ThemeTrustState state) =>
        new(
            publisherId,
            keyId,
            ThemeSignatureAlgorithm.EcdsaP256Sha256,
            ThemePublicKeyEncoding.SubjectPublicKeyInfo,
            "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEeT68e7qekZ59xr4povRF6e9gB8M4GErSkxdIsmwVDfRVmfmXa9duEKfoOCulrcN/bSeXbd4M+2OMcMKmEsXmfw==",
            state);

    private static void AssertRecordProperties<T>(params (string Name, Type Type)[] expected)
    {
        (string Name, Type Type)[] actual = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(property => (property.Name, property.PropertyType))
            .ToArray();
        Assert.Equal(expected, actual);
    }

    private static void AssertSchemaValid(string schemaFile, string json)
    {
        using JsonDocument schema = ReadSchema(schemaFile);
        using JsonDocument instance = JsonDocument.Parse(json);
        Assert.Empty(JsonSchemaSubsetValidator.Validate(schema.RootElement, instance.RootElement));
    }

    private static JsonDocument ReadSchema(string schemaFile) =>
        JsonDocument.Parse(File.ReadAllText(Path.Combine(RepositoryPaths.ThemeContracts, "schemas", schemaFile)));

    private static TestPackageEntry Entry(string path, string content) =>
        new(path, Encoding.UTF8.GetBytes(content));

    private static string ComputePackageHash(IEnumerable<TestPackageEntry> entries)
    {
        return ThemeIntegrityContractConformanceOracle.ComputePackageHash(
            entries
                .Where(entry => !string.Equals(entry.Path, IntegrityManifestPath, StringComparison.Ordinal)
                                && !string.Equals(entry.Path, SignatureEnvelopePath, StringComparison.Ordinal))
                .Select(entry => (ThemeCanonicalPath.Parse(entry.Path).Value, entry.Bytes)));
    }

    private static byte[] CreateZip(
        IReadOnlyList<(string Path, byte[] Bytes)> entries,
        CompressionLevel compression,
        DateTimeOffset timestamp)
    {
        using MemoryStream output = new();
        using (ZipArchive archive = new(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach ((string path, byte[] bytes) in entries)
            {
                ZipArchiveEntry entry = archive.CreateEntry(path, compression);
                entry.LastWriteTime = timestamp;
                using Stream stream = entry.Open();
                stream.Write(bytes);
            }
        }

        return output.ToArray();
    }

    private static TestPackageEntry[] ReadZip(byte[] archiveBytes)
    {
        using MemoryStream input = new(archiveBytes, writable: false);
        using ZipArchive archive = new(input, ZipArchiveMode.Read);
        return archive.Entries.Select(entry =>
        {
            using Stream stream = entry.Open();
            using MemoryStream content = new();
            stream.CopyTo(content);
            return new TestPackageEntry(entry.FullName, content.ToArray());
        }).ToArray();
    }

    private static string FormatDiagnostic(ThemeIntegrityDiagnosticV1 diagnostic) =>
        $"{diagnostic.Code}|{diagnostic.CanonicalPath}|{diagnostic.Message}";

    private sealed record TestPackageEntry(string Path, byte[] Bytes);
}
