using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Integrity;

namespace Tcc.Architecture.Tests;

public sealed class PhaseFourThemePackageMetadataEvaluatorTests
{
    public static IEnumerable<object[]> HostileUnicodeVectors()
    {
        foreach (string path in new[] { "theme.json", "meta/integrity.json", "meta/signature.json" })
        {
            foreach (bool propertyName in new[] { false, true })
            {
                foreach (string vector in new[]
                {
                    "continuation", "overlong", "truncated", "surrogate-scalar", "out-of-range",
                    "invalid-leading-byte", "escaped-high", "escaped-low", "escaped-high-ascii",
                    "escaped-reversed-pair",
                })
                {
                    yield return [path, propertyName, vector];
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(HostileUnicodeVectors))]
    public async Task HostileUnicodeRejectsEveryRawTokenBeforeSchemaWithExactDiagnostic(
        string path, bool propertyName, string vector)
    {
        Scenario scenario = CreateScenario(includeSignature: true);
        Assert.True((await Evaluate(CreateScenario(includeSignature: true))).CanContinue);
        byte[] source = path switch
        {
            "theme.json" => scenario.ThemeBytes,
            "meta/integrity.json" => scenario.IntegrityBytes,
            _ => JsonSerializer.SerializeToUtf8Bytes(
                scenario.Request.SignatureEnvelope, ThemeContractJson.CreateSerializerOptions()),
        };
        string resource = path switch
        {
            "theme.json" => ThemeMetadataSchemaValidator.ThemeManifestSchemaResource,
            "meta/integrity.json" => ThemeMetadataSchemaValidator.IntegrityManifestSchemaResource,
            _ => ThemeMetadataSchemaValidator.SignatureEnvelopeSchemaResource,
        };
        Assert.True(ThemeMetadataSchemaValidator.Validate(source, resource));
        string marker = propertyName
            ? path == "meta/signature.json" ? "envelope_version" : "schema_version"
            : path switch
            {
                "theme.json" => "Presentation-only example theme.",
                "meta/integrity.json" => "assets/icon.png",
                _ => "example.publisher",
            };
        byte[] replacement = vector switch
        {
            "continuation" => [0x80],
            "overlong" => [0xc0, 0xaf],
            "truncated" => [0xe2, 0x82],
            "surrogate-scalar" => [0xed, 0xa0, 0x80],
            "out-of-range" => [0xf4, 0x90, 0x80, 0x80],
            "invalid-leading-byte" => [0xff],
            "escaped-high" => Encoding.ASCII.GetBytes("\\uD800"),
            "escaped-low" => Encoding.ASCII.GetBytes("\\uDC00"),
            "escaped-high-ascii" => Encoding.ASCII.GetBytes("\\uD800x"),
            _ => Encoding.ASCII.GetBytes("\\uDC00\\uD800"),
        };
        byte[] raw = ReplaceRawMarker(source, marker, replacement);

        // The empty schema imposes no constraints: rejection here can only come
        // from strict raw parsing, never a missing required field or schema type.
        Assert.False(ThemeMetadataSchemaValidator.ValidateAgainstSchemaBytes(raw, "{}"u8.ToArray()));
        Assert.False(ThemeMetadataSchemaValidator.Validate(raw, resource));
        if (path == "theme.json")
        {
            scenario = RebindThemeEvidence(scenario, raw);
        }
        else
        {
            scenario.Reader.SetBytes(path, raw);
        }

        ThemePackageMetadataEvaluation result = await Evaluate(scenario);
        AssertDiagnostic(result, ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument, path);
        Assert.Equal(path, scenario.Reader.ReadPaths[^1]);
        AssertNoHashesOrMetadata(result);
    }

    [Theory]
    [InlineData("\"\\uD83D\\uDE00\"")]
    [InlineData("\"😀\"")]
    [InlineData("\"�\"")]
    [InlineData("{\"\\uD83D\\uDE00\":\"ok\"}")]
    [InlineData("{\"😀\":\"ok\"}")]
    public void ValidUnicodeIsNotConfusedWithMalformedTokenRecovery(string document) =>
        Assert.True(ValidateSchema(document, "{}"));

    [Theory]
    [InlineData("theme.json")]
    [InlineData("meta/integrity.json")]
    [InlineData("meta/signature.json")]
    public async Task ReaderProgrammerFaultStillPropagatesAtEveryMetadataRead(string path)
    {
        Scenario scenario = CreateScenario(includeSignature: true);
        InvalidOperationException failure = new("reader programmer fault");
        scenario.Reader.SetFailure(path, failure);
        Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(async () => await Evaluate(scenario)));
    }

    [Theory]
    [InlineData("0", "0.1", false)]
    [InlineData("-0", "0.1", false)]
    [InlineData("0.1", "0", true)]
    [InlineData("0", "1e-1000", false)]
    [InlineData("1e-1000", "0", true)]
    [InlineData("0", "-0.1", true)]
    [InlineData("-0", "-0.1", true)]
    [InlineData("-0.1", "0", false)]
    [InlineData("0", "-1e-1000", true)]
    [InlineData("-1e-1000", "0", false)]
    public void ExactMinimumOrdersZeroAndFractions(string instance, string minimum, bool expected) =>
        AssertSchema(instance, "{\"minimum\":" + minimum + "}", expected);

    [Fact]
    public void ExactNumericOrderingHasCanonicalZeroSymmetrySignAndTransitivity()
    {
        // Independent, explicitly ordered mathematical equivalence classes.
        // Includes exponents beyond Int64 without allocating their powers of ten.
        string[][] ordered =
        [
            ["-1e999999999999999999999999"],
            ["-9223372036854775808"], ["-1"], ["-0.1"], ["-1e-1000"],
            ["-1e-999999999999999999999999"],
            ["0", "-0", "0.0", "-0.0", "0e1000", "-0e-1000"],
            ["1e-999999999999999999999999"], ["1e-1000"], ["0.1"],
            ["1", "1.0", "1e0"],
            ["1.00000000000000000000000000000000000000000000000000000001"],
            ["9223372036854775807"], ["9223372036854775808"],
            ["123456789012345678901234567890123456789012345678901234567890"],
            ["1e1000"], ["1e999999999999999999999999"],
        ];
        List<(string Raw, int Rank)> numbers = [];
        for (int rank = 0; rank < ordered.Length; rank++)
        {
            foreach (string raw in ordered[rank])
            {
                numbers.Add((raw, rank));
            }
        }

        int[,] comparisons = new int[numbers.Count, numbers.Count];
        for (int left = 0; left < numbers.Count; left++)
        {
            for (int right = 0; right < numbers.Count; right++)
            {
                string a = numbers[left].Raw;
                string b = numbers[right].Raw;
                bool atLeast = ValidateSchema(a, "{\"minimum\":" + b + "}");
                bool equal = ValidateSchema(a, "{\"const\":" + b + "}");
                int actual = equal ? 0 : atLeast ? 1 : -1;
                comparisons[left, right] = actual;
                int expected = Math.Sign(numbers[left].Rank.CompareTo(numbers[right].Rank));
                Assert.Equal(expected >= 0, atLeast);
                Assert.Equal(expected == 0, equal);
                Assert.Equal(expected, actual);
            }
        }

        for (int a = 0; a < numbers.Count; a++)
        {
            for (int b = 0; b < numbers.Count; b++)
            {
                Assert.Equal(comparisons[a, b], -comparisons[b, a]);
                for (int c = 0; c < numbers.Count; c++)
                {
                    if (comparisons[a, b] <= 0 && comparisons[b, c] <= 0)
                    {
                        Assert.True(comparisons[a, c] <= 0);
                    }
                }
            }
        }
    }

    [Theory]
    [InlineData("[\"A\",\"A\"]")]
    [InlineData("[1]")]
    [InlineData("[\"A\",1]")]
    [InlineData("\"foo\"")]
    [InlineData("null")]
    [InlineData("[\"A\",\"\\u0041\"]")]
    public void MalformedRequiredFailsSchemaDefinitionBeforeAnyDocumentValidation(string required)
    {
        string schema = "{\"required\":" + required + "}";
        foreach (string document in new[] { "{\"A\":1}", "0", "{" })
        {
            Assert.Throws<InvalidOperationException>(() => ValidateSchema(document, schema));
        }

        foreach (string resource in new[]
        {
            ThemeMetadataSchemaValidator.ThemeManifestSchemaResource,
            ThemeMetadataSchemaValidator.IntegrityManifestSchemaResource,
            ThemeMetadataSchemaValidator.SignatureEnvelopeSchemaResource,
        })
        {
            System.Text.Json.Nodes.JsonNode schemaNode = System.Text.Json.Nodes.JsonNode.Parse(
                ThemeMetadataSchemaValidator.ReadSchemaResource(resource))!;
            schemaNode["required"] = System.Text.Json.Nodes.JsonNode.Parse(required);
            Assert.Throws<InvalidOperationException>(() => ValidateSchema("{", schemaNode.ToJsonString()));
        }
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("[\"A\"]")]
    [InlineData("[\"A\",\"B\"]")]
    [InlineData("[\"A\",\"a\"]")]
    [InlineData("[\"é\",\"e\\u0301\"]")]
    public void ValidRequiredNamesUseDecodedOrdinalIdentity(string required)
    {
        string schema = "{\"required\":" + required + "}";
        Assert.True(ValidateSchema("{\"A\":1,\"B\":2,\"a\":3,\"é\":4,\"e\\u0301\":5}", schema));
        Assert.False(ValidateSchema("{", schema));
    }

    [Theory]
    [InlineData("{\"properties\":[]}")]
    [InlineData("{\"properties\":{\"x\":true}}")]
    [InlineData("{\"items\":[]}")]
    [InlineData("{\"items\":true}")]
    [InlineData("{\"items\":{\"required\":[1]}}")]
    [InlineData("{\"properties\":{\"x\":{\"required\":[\"A\",\"A\"]}}}")]
    [InlineData("{\"type\":1}")]
    [InlineData("{\"type\":\"bogus\"}")]
    [InlineData("{\"type\":[]}")]
    [InlineData("{\"type\":[\"string\",\"string\"]}")]
    [InlineData("{\"type\":[\"string\",1]}")]
    [InlineData("{\"enum\":null}")]
    [InlineData("{\"additionalProperties\":{}}")]
    [InlineData("{\"uniqueItems\":1}")]
    [InlineData("{\"minItems\":-1}")]
    [InlineData("{\"minItems\":0.1}")]
    [InlineData("{\"minLength\":null}")]
    [InlineData("{\"minLength\":-1}")]
    [InlineData("{\"maxLength\":\"1\"}")]
    [InlineData("{\"maxLength\":0.1}")]
    [InlineData("{\"minimum\":\"0\"}")]
    [InlineData("{\"pattern\":1}")]
    [InlineData("{\"$schema\":1}")]
    [InlineData("{\"$id\":1}")]
    [InlineData("{\"title\":null}")]
    [InlineData("{\"description\":false}")]
    public void SupportedKeywordMalformedShapesFailBeforeDocumentParsing(string schema) =>
        Assert.Throws<InvalidOperationException>(() => ValidateSchema("{", schema));

    [Theory]
    [InlineData("{\"properties\":{},\"items\":{},\"required\":[],\"enum\":[]}")]
    [InlineData("{\"type\":[\"null\",\"string\"],\"additionalProperties\":false,\"uniqueItems\":true}")]
    [InlineData("{\"minItems\":0,\"minLength\":0,\"maxLength\":1,\"minimum\":1e-1000,\"pattern\":\"x\"}")]
    [InlineData("{\"$schema\":\"https://json-schema.org/draft/2020-12/schema\",\"$id\":\"x\",\"title\":\"x\",\"description\":\"x\"}")]
    [InlineData("{\"const\":null}")]
    [InlineData("{\"const\":{\"x\":[1,true,\"value\",null]}}")]
    public void SupportedKeywordValidShapesReachDocumentParsing(string schema) =>
        Assert.False(ValidateSchema("{", schema));

    [Theory]
    [InlineData("1.0", true, false)]
    [InlineData("1e0", true, false)]
    [InlineData("0", true, true)]
    [InlineData("1", true, true)]
    [InlineData("9223372036854775807", true, true)]
    [InlineData("9223372036854775808", true, false)]
    [InlineData("-1", false, false)]
    [InlineData("1e-1000", false, false)]
    public async Task SealedIntegerSchemaAndExactInt64MaterializationRemainSeparate(
        string length, bool schemaValid, bool int64Valid)
    {
        Scenario scenario = CreateScenario(includeSignature: true);
        string originalLength = scenario.Request.IntegrityManifest.Files[0].LengthBytes.ToString(CultureInfo.InvariantCulture);
        byte[] raw = ReplaceRawMarker(scenario.IntegrityBytes,
            "\"length_bytes\":" + originalLength,
            Encoding.ASCII.GetBytes("\"length_bytes\":" + length));
        Assert.Equal(schemaValid, ThemeMetadataSchemaValidator.Validate(raw,
            ThemeMetadataSchemaValidator.IntegrityManifestSchemaResource));
        if (int64Valid)
        {
            ThemeIntegrityManifestV2 dto = JsonSerializer.Deserialize<ThemeIntegrityManifestV2>(
                raw, ThemeContractJson.CreateSerializerOptions())!;
            Assert.Equal(long.Parse(length, CultureInfo.InvariantCulture), dto.Files[0].LengthBytes);
        }
        else
        {
            scenario.Reader.SetBytes("meta/integrity.json", raw);
            AssertDiagnostic(await Evaluate(scenario), ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument,
                "meta/integrity.json");
        }

        Assert.True(ThemeMetadataSchemaValidator.Validate(scenario.ThemeBytes,
            ThemeMetadataSchemaValidator.ThemeManifestSchemaResource));
        Assert.True(ThemeMetadataSchemaValidator.Validate(JsonSerializer.SerializeToUtf8Bytes(
            scenario.Request.SignatureEnvelope, ThemeContractJson.CreateSerializerOptions()),
            ThemeMetadataSchemaValidator.SignatureEnvelopeSchemaResource));
    }

    private static byte[] ReplaceRawMarker(byte[] source, string marker, byte[] replacement)
    {
        byte[] markerBytes = Encoding.UTF8.GetBytes(marker);
        int offset = source.AsSpan().IndexOf(markerBytes);
        Assert.True(offset >= 0, "The valid source must contain the intended token marker.");
        return [.. source.AsSpan(0, offset), .. replacement, .. source.AsSpan(offset + markerBytes.Length)];
    }

    [Theory]
    [InlineData("0", false)]
    [InlineData("0.1", false)]
    [InlineData("1e-1000", false)]
    [InlineData("-0.1", false)]
    [InlineData("1.0", true)]
    [InlineData("1e0", true)]
    [InlineData("1.0000000000000000000000000000000000000001", true)]
    public void SealedThemeMinimumStillUsesExactNumberOrdering(string value, bool expected)
    {
        Scenario scenario = CreateScenario();
        byte[] raw = ReplaceRawMarker(scenario.ThemeBytes, "\"minimum_dpi_scale\":1",
            Encoding.ASCII.GetBytes("\"minimum_dpi_scale\":" + value));
        Assert.Equal(expected, ThemeMetadataSchemaValidator.Validate(raw,
            ThemeMetadataSchemaValidator.ThemeManifestSchemaResource));
    }

    [Theory]
    [InlineData("{\"title\":\"\\uD800\"}")]
    [InlineData("{\"\\uD800\":1}")]
    [InlineData("{\"const\":[\"\\uDC00\"]}")]
    public void MalformedSchemaUnicodeRemainsDeploymentFault(string schema) =>
        Assert.Throws<InvalidOperationException>(() => ValidateSchema("{", schema));

    [Fact]
    public async Task ValidRawMetadataProducesOnlyValidatedDeterministicInputs()
    {
        Scenario scenario = CreateScenario();

        ThemePackageMetadataEvaluation result = await Evaluate(scenario);

        Assert.True(result.CanContinue);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(GoldenTreeHash(scenario.Inventory.FileEvidence), result.PackageHash);
        Assert.Equal(Hash(scenario.ThemeBytes), result.ThemeManifestHash);
        Assert.Equal(Hash(scenario.IntegrityBytes), result.IntegrityManifestHash);
        Assert.NotNull(result.ThemeManifest);
        Assert.True(IntegrityManifestsEqual(scenario.Request.IntegrityManifest, result.IntegrityManifest!));
        Assert.Null(result.SignatureEnvelope);
        Assert.Equal(["theme.json", "meta/integrity.json"], scenario.Reader.ReadPaths);
        Assert.DoesNotContain("assets/icon.png", scenario.Reader.ReadPaths);
    }

    [Fact]
    public async Task FailedInventoryIsReturnedWithoutAnyPhaseFourCWork()
    {
        Scenario scenario = CreateScenario();
        ThemeIntegrityDiagnosticV1 diagnostic = new(
            ThemeIntegrityDiagnosticCodes.FileHashMismatch,
            ThemeDiagnosticsSeverity.Error,
            "assets/icon.png",
            "sealed Phase 4B finding");
        scenario = scenario with
        {
            Inventory = new ThemePackageInventoryEvaluation(false, scenario.Inventory.FileEvidence, [diagnostic]),
        };

        ThemePackageMetadataEvaluation result = await Evaluate(scenario);

        Assert.False(result.CanContinue);
        Assert.Equal([diagnostic], result.Diagnostics);
        AssertNoHashesOrMetadata(result);
        Assert.Empty(scenario.Reader.ReadPaths);
    }

    [Theory]
    [InlineData(ThemeIntegrityEvidenceStatus.MissingAllowed)]
    [InlineData(ThemeIntegrityEvidenceStatus.MissingRequired)]
    [InlineData(ThemeIntegrityEvidenceStatus.LengthMismatch)]
    [InlineData(ThemeIntegrityEvidenceStatus.HashMismatch)]
    [InlineData(ThemeIntegrityEvidenceStatus.Unavailable)]
    [InlineData(ThemeIntegrityEvidenceStatus.UnsupportedEntry)]
    public async Task NonVerifiedThemeEvidenceFailsTheZeroReadEntryGate(
        ThemeIntegrityEvidenceStatus status)
    {
        Scenario scenario = CreateScenario();
        ThemeIntegrityFileEvidenceV1 theme = scenario.Inventory.FileEvidence[0] with { Status = status };
        scenario = WithEvidence(scenario, [theme, scenario.Inventory.FileEvidence[1]]);

        ThemePackageMetadataEvaluation result = await Evaluate(scenario);

        AssertEntryGateFailure(result, scenario);
    }

    [Fact]
    public async Task MissingAndUnrelatedVerifiedEvidenceFailTheZeroReadEntryGate()
    {
        Scenario missing = WithEvidence(CreateScenario(), []);
        Scenario unrelated = CreateScenario();
        unrelated = WithEvidence(unrelated, [unrelated.Inventory.FileEvidence[1]]);

        AssertEntryGateFailure(await Evaluate(missing), missing);
        AssertEntryGateFailure(await Evaluate(unrelated), unrelated);
    }

    [Fact]
    public async Task InvalidThemeEvidenceFieldsFailTheZeroReadEntryGate()
    {
        Scenario baseline = CreateScenario();
        ThemeIntegrityFileEvidenceV1 theme = baseline.Inventory.FileEvidence[0];
        ThemeIntegrityFileEvidenceV1[] invalid =
        [
            theme with { IsPresent = false },
            theme with { ActualLengthBytes = null },
            theme with { ActualSha256 = null },
            theme with { ActualSha256 = new string('a', 63) },
            theme with { ActualSha256 = new string('A', 64) },
            theme with { ActualSha256 = new string('g', 64) },
        ];

        foreach (ThemeIntegrityFileEvidenceV1 item in invalid)
        {
            Scenario scenario = WithEvidence(CreateScenario(), [item]);
            AssertEntryGateFailure(await Evaluate(scenario), scenario);
        }
    }

    [Fact]
    public async Task CaseDifferentEvidenceDoesNotMatchAndMultipleMatchesAreRejected()
    {
        Scenario caseDifferent = CreateScenario();
        ThemeIntegrityFileEvidenceV1 theme = caseDifferent.Inventory.FileEvidence[0];
        caseDifferent = WithEvidence(caseDifferent, [theme with { CanonicalPath = "Theme.json" }]);
        AssertEntryGateFailure(await Evaluate(caseDifferent), caseDifferent);

        Scenario multiple = CreateScenario();
        ThemeIntegrityFileEvidenceV1 exact = multiple.Inventory.FileEvidence[0];
        multiple = WithEvidence(multiple, [exact, exact]);
        AssertEntryGateFailure(await Evaluate(multiple), multiple);
    }

    [Fact]
    public async Task NfcEquivalentEvidenceIdentityMatchesWithCasePreserved()
    {
        const string decomposed = "meta/e\u0301.json";
        string canonical = decomposed.Normalize(NormalizationForm.FormC);
        Scenario scenario = CreateScenario(canonical);
        ThemeIntegrityFileEvidenceV1 evidence = scenario.Inventory.FileEvidence[0] with
        {
            CanonicalPath = decomposed,
        };
        scenario = WithEvidence(scenario, [evidence, scenario.Inventory.FileEvidence[1]]);

        ThemePackageMetadataEvaluation result = await Evaluate(scenario);

        Assert.True(result.CanContinue);
        Assert.Equal(canonical, scenario.Request.ThemeManifestPath.Value);
        Assert.Equal(canonical, scenario.Reader.ReadPaths[0]);
    }

    [Fact]
    public async Task ThemeSecondReadLengthOrHashIncoherenceStopsBeforeRawJson()
    {
        Scenario length = CreateScenario();
        length.Reader.SetBytes("theme.json", [1]);
        ThemePackageMetadataEvaluation lengthResult = await Evaluate(length);
        AssertDiagnostic(lengthResult, ThemeIntegrityDiagnosticCodes.ThemeManifestHashMismatch, "theme.json");
        Assert.Equal(["theme.json"], length.Reader.ReadPaths);

        Scenario sameLength = CreateScenario();
        sameLength.Reader.SetBytes("theme.json", new byte[sameLength.ThemeBytes.Length]);
        ThemePackageMetadataEvaluation hashResult = await Evaluate(sameLength);
        AssertDiagnostic(hashResult, ThemeIntegrityDiagnosticCodes.ThemeManifestHashMismatch, "theme.json");
        Assert.Equal(["theme.json"], sameLength.Reader.ReadPaths);

        Scenario malformed = CreateScenario();
        byte[] malformedBytes = Encoding.UTF8.GetBytes(new string(' ', malformed.ThemeBytes.Length));
        malformed.Reader.SetBytes("theme.json", malformedBytes);
        ThemePackageMetadataEvaluation malformedResult = await Evaluate(malformed);
        AssertDiagnostic(malformedResult, ThemeIntegrityDiagnosticCodes.ThemeManifestHashMismatch, "theme.json");
        Assert.NotEqual(ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument,
            Assert.Single(malformedResult.Diagnostics).Code);
    }

    [Fact]
    public async Task ThemeExpectedHashAndTreeHashUseTheirExactDiagnostics()
    {
        Scenario themeHash = CreateScenario();
        ThemeIntegrityManifestV2 rawThemeHashMismatch = themeHash.Request.IntegrityManifest with
        {
            Files =
            [
                themeHash.Request.IntegrityManifest.Files[0] with { Sha256 = new string('b', 64) },
                themeHash.Request.IntegrityManifest.Files[1],
            ],
        };
        themeHash = WithRawAndCallerIntegrity(themeHash, rawThemeHashMismatch, rawThemeHashMismatch);
        AssertDiagnostic(
            await Evaluate(themeHash),
            ThemeIntegrityDiagnosticCodes.ThemeManifestHashMismatch,
            "theme.json");

        Scenario tree = CreateScenario();
        ThemeIntegrityManifestV2 wrongPackageHash = tree.Request.IntegrityManifest with
        {
            PackageHash = new string('b', 64),
        };
        tree = WithRawAndCallerIntegrity(tree, wrongPackageHash, wrongPackageHash);
        AssertDiagnostic(await Evaluate(tree), ThemeIntegrityDiagnosticCodes.PackageHashMismatch, null);
    }

    [Fact]
    public async Task ThemeIdentityAndVersionBindingsUseP4I024AndP4I025()
    {
        Scenario identity = CreateScenario();
        identity = identity with
        {
            Request = identity.Request with { ThemeId = new ThemeId("com.example.other") },
        };
        AssertDiagnostic(
            await Evaluate(identity),
            ThemeIntegrityDiagnosticCodes.ThemeIdentityMismatch,
            null);

        Scenario version = CreateScenario();
        version = version with
        {
            Request = version.Request with { Version = new ThemeVersion("1.2.4") },
        };
        AssertDiagnostic(
            await Evaluate(version),
            ThemeIntegrityDiagnosticCodes.ThemeVersionMismatch,
            null);
    }

    [Fact]
    public async Task RawThemeJsonFailuresMapToP4I027AfterCoherence()
    {
        string[] mutations =
        [
            "{\"schema_version\":\"1.0\",\"schema_version\":\"1.0\"}",
            "{\"package\":{\"name\":\"x\",\"name\":\"y\"}}",
            "{\"items\":[{\"x\":1,\"\\u0078\":2}]}",
            "{}{}",
            "{} trailing",
            "{\"Schema_version\":\"1.0\"}",
            "{\"schema_version\":null}",
            "{\"x\":\"\\uD800\"}",
        ];

        foreach (string json in mutations)
        {
            Scenario scenario = CreateScenario();
            byte[] raw = Encoding.UTF8.GetBytes(json);
            scenario = RebindThemeEvidence(scenario, raw);
            ThemePackageMetadataEvaluation result = await Evaluate(scenario);
            AssertDiagnostic(result, ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument, "theme.json");
            Assert.Equal(["theme.json"], scenario.Reader.ReadPaths);
        }

        Scenario utf8 = CreateScenario();
        utf8 = RebindThemeEvidence(utf8, [0xff, 0xfe, 0xfd]);
        AssertDiagnostic(
            await Evaluate(utf8),
            ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument,
            "theme.json");
    }

    [Fact]
    public void EmbeddedSchemasHaveExactNamesAndSourceBytes()
    {
        (string Resource, string File, string Hash)[] expected =
        [
            (ThemeMetadataSchemaValidator.ThemeManifestSchemaResource,
                "ThemeManifest.schema.json", "E2B13A0EA1CBC076C844ED0D2AB2890B1AD1FA383FF1AFF559517B55893B875D"),
            (ThemeMetadataSchemaValidator.IntegrityManifestSchemaResource,
                "ThemeIntegrity.v2.schema.json", "B2C96379F01C98F6154495B463DCFD24CFF5E9F43EE50D755F558EF93BA83686"),
            (ThemeMetadataSchemaValidator.SignatureEnvelopeSchemaResource,
                "ThemeSignatureEnvelope.v1.schema.json", "A6A29757F00D395D4FDB065DA75BA7EE5D230272174D26083C78E145D06D264D"),
        ];

        string schemaRoot = Path.Combine(RepositoryPaths.ThemeContracts, "schemas");
        string[] actualNames = typeof(ThemeMetadataSchemaValidator).Assembly.GetManifestResourceNames();
        foreach ((string resource, string file, string expectedHash) in expected)
        {
            Assert.Contains(resource, actualNames);
            byte[] source = File.ReadAllBytes(Path.Combine(schemaRoot, file));
            byte[] embedded = ThemeMetadataSchemaValidator.ReadSchemaResource(resource);
            Assert.Equal(source, embedded);
            Assert.Equal(expectedHash, Convert.ToHexString(SHA256.HashData(source)));
            Assert.Equal(expectedHash, Convert.ToHexString(SHA256.HashData(embedded)));
        }

        Assert.Throws<InvalidOperationException>(() =>
            ThemeMetadataSchemaValidator.ReadSchemaResource("Tcc.Themes.Schemas.Missing.schema.json"));
    }

    [Fact]
    public void SchemaRuntimeExecutesEverySealedKeywordSemantic()
    {
        AssertSchema("{\"x\":1}", "{\"$schema\":\"https://json-schema.org/draft/2020-12/schema\",\"type\":\"object\",\"properties\":{\"x\":{\"type\":\"integer\",\"minimum\":0}},\"required\":[\"x\"],\"additionalProperties\":false}", true);
        AssertSchema("{\"x\":-1}", "{\"type\":\"object\",\"properties\":{\"x\":{\"type\":\"integer\",\"minimum\":0}},\"required\":[\"x\"],\"additionalProperties\":false}", false);
        AssertSchema("{\"X\":1}", "{\"type\":\"object\",\"properties\":{\"x\":{\"type\":\"integer\"}},\"required\":[\"x\"],\"additionalProperties\":false}", false);
        AssertSchema("null", "{\"type\":[\"string\",\"null\"]}", true);
        AssertSchema("true", "{\"type\":\"boolean\"}", true);
        AssertSchema("1.5", "{\"type\":\"number\"}", true);
        AssertSchema("1.0", "{\"type\":\"integer\"}", true);
        AssertSchema("[1,2]", "{\"type\":\"array\",\"items\":{\"type\":\"integer\"},\"minItems\":2,\"uniqueItems\":true}", true);
        AssertSchema("[{\"a\":1},{\"a\":1.0}]", "{\"type\":\"array\",\"uniqueItems\":true}", false);
        AssertSchema("\"ok\"", "{\"type\":\"string\",\"enum\":[\"ok\",\"yes\"]}", true);
        AssertSchema("{\"b\":2,\"a\":1}", "{\"const\":{\"a\":1.0,\"b\":2}}", true);
        AssertSchema("\"😀\"", "{\"type\":\"string\",\"minLength\":1,\"maxLength\":1}", true);
        AssertSchema("\"aa\"", "{\"type\":\"string\",\"maxLength\":1}", false);
        AssertSchema("\"abc\"", "{\"type\":\"string\",\"pattern\":\"^(?!x)[a-z]+$\"}", true);
    }

    [Fact]
    public void SchemaRuntimeRejectsMalformedUnsupportedOrDuplicateSchemasClosed()
    {
        Assert.Throws<InvalidOperationException>(() => ValidateSchema("{}", "{"));
        Assert.Throws<InvalidOperationException>(() => ValidateSchema("{", "{"));
        Assert.Throws<InvalidOperationException>(() => ValidateSchema("{}", "{\"oneOf\":[]}"));
        Assert.Throws<InvalidOperationException>(() => ValidateSchema("{}", "{\"type\":\"object\",\"type\":\"object\"}"));
        Assert.Throws<InvalidOperationException>(() => ValidateSchema("{}", "{\"$schema\":\"https://example.invalid/dialect\"}"));
        Assert.Throws<InvalidOperationException>(() => ValidateSchema("{}", "{\"pattern\":\"[\"}"));
    }

    [Fact]
    public void StrictJsonAcceptsWhitespaceAndLineEndingsButRejectsInvalidEscapeAndTrailingData()
    {
        const string schema =
            "{\"type\":\"object\",\"properties\":{\"x\":{\"type\":\"string\"}},\"required\":[\"x\"],\"additionalProperties\":false}";
        Assert.True(ValidateSchema("  { \"x\" : \"ok\" }  ", schema));
        Assert.True(ValidateSchema("{\r\n  \"x\": \"ok\"\r\n}\r\n", schema));
        Assert.True(ValidateSchema("{\n  \"x\": \"ok\"\n}\n", schema));
        Assert.False(ValidateSchema("{\"x\":\"\\q\"}", schema));
        Assert.False(ValidateSchema("{\"x\":\"ok\"}{}", schema));
    }

    [Fact]
    public void SchemaRuntimeUsesExactNumbersAndFinitePatternTimeout()
    {
        AssertSchema("9223372036854775807", "{\"type\":\"integer\",\"minimum\":0}", true);
        AssertSchema("9223372036854775808", "{\"type\":\"integer\",\"minimum\":9223372036854775807}", true);
        AssertSchema("-1", "{\"type\":\"integer\",\"minimum\":0}", false);
        AssertSchema("1e0", "{\"type\":\"integer\",\"minimum\":1.0}", true);
        AssertSchema("1e-1", "{\"type\":\"integer\"}", false);
        AssertSchema("\"1\"", "{\"type\":\"integer\"}", false);
        AssertSchema("null", "{\"type\":\"integer\"}", false);

        string hostile = "\"" + new string('a', 50_000) + "!\"";
        Stopwatch stopwatch = Stopwatch.StartNew();
        Assert.False(ValidateSchema(hostile, "{\"type\":\"string\",\"pattern\":\"^(a+)+$\"}"));
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task IntegrityRawDtoBindingIsExactAndFilesOrderIsSignificant()
    {
        Scenario baseline = CreateScenario();
        ThemeIntegrityManifestV2 source = baseline.Request.IntegrityManifest;
        ThemeIntegrityManifestV2[] mutations =
        [
            source with { SchemaVersion = "2.00" },
            source with { InventoryMode = (ThemeIntegrityInventoryMode)99 },
            source with { ThemeId = "com.example.other" },
            source with { Version = "1.2.4" },
            source with { HashAlgorithm = "SHA256" },
            source with { PackageHash = new string('b', 64) },
            source with { Files = [source.Files[1], source.Files[0]] },
            source with { Files = [source.Files[0] with { CanonicalPath = "Theme.json" }, source.Files[1]] },
            source with { Files = [source.Files[0] with { EntryKind = ThemeIntegrityEntryKind.Payload }, source.Files[1]] },
            source with { Files = [source.Files[0] with { Sha256 = new string('b', 64) }, source.Files[1]] },
            source with { Files = [source.Files[0] with { LengthBytes = source.Files[0].LengthBytes + 1 }, source.Files[1]] },
            source with { Files = [source.Files[0] with { Required = false }, source.Files[1]] },
        ];

        foreach (ThemeIntegrityManifestV2 callerMutation in mutations)
        {
            Scenario scenario = CreateScenario();
            scenario = scenario with { Request = scenario.Request with { IntegrityManifest = callerMutation } };
            AssertDiagnostic(
                await Evaluate(scenario),
                ThemeIntegrityDiagnosticCodes.MetadataDtoMismatch,
                "meta/integrity.json");
        }
    }

    [Fact]
    public async Task IntegrityNumericAndRawJsonFailuresMapToP4I027()
    {
        string[] replacements =
        [
            "9223372036854775808",
            "-1",
            "1.5",
            "1e-1",
            "\"1\"",
            "null",
        ];

        foreach (string replacement in replacements)
        {
            Scenario scenario = CreateScenario();
            string json = Encoding.UTF8.GetString(scenario.IntegrityBytes);
            string rawLength = scenario.Request.IntegrityManifest.Files[0].LengthBytes
                .ToString(CultureInfo.InvariantCulture);
            json = json.Replace(
                "\"length_bytes\":" + rawLength,
                "\"length_bytes\":" + replacement,
                StringComparison.Ordinal);
            scenario.Reader.SetBytes("meta/integrity.json", Encoding.UTF8.GetBytes(json));
            AssertDiagnostic(
                await Evaluate(scenario),
                ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument,
                "meta/integrity.json");
        }
    }

    [Fact]
    public async Task SignatureEnvelopeIsOptionalAndAllSevenCallerFieldsBindExactly()
    {
        Scenario unsigned = CreateScenario();
        ThemePackageMetadataEvaluation unsignedResult = await Evaluate(unsigned);
        Assert.True(unsignedResult.CanContinue);
        Assert.Null(unsignedResult.SignatureEnvelope);
        Assert.DoesNotContain("meta/signature.json", unsigned.Reader.ReadPaths);

        Scenario baseline = CreateScenario(includeSignature: true);
        ThemePackageMetadataEvaluation valid = await Evaluate(baseline);
        Assert.True(valid.CanContinue);
        Assert.Equal(baseline.Request.SignatureEnvelope, valid.SignatureEnvelope);

        ThemeSignatureEnvelopeV1 source = baseline.Request.SignatureEnvelope!;
        ThemeSignatureEnvelopeV1[] mutations =
        [
            source with { EnvelopeVersion = "1.00" },
            source with { Algorithm = (ThemeSignatureAlgorithm)99 },
            source with { PublisherId = "other.publisher" },
            source with { KeyId = "other-key" },
            source with { SignatureEncoding = (ThemeSignatureEncoding)99 },
            source with { Signature = Convert.ToBase64String(new byte[63]) },
            source with { SignedPayloadType = (ThemeSignedPayloadType)99 },
        ];

        foreach (ThemeSignatureEnvelopeV1 callerMutation in mutations)
        {
            Scenario scenario = CreateScenario(includeSignature: true);
            scenario = scenario with { Request = scenario.Request with { SignatureEnvelope = callerMutation } };
            AssertDiagnostic(await Evaluate(scenario), ThemeIntegrityDiagnosticCodes.MetadataDtoMismatch, null);
        }
    }

    [Theory]
    [InlineData("theme.json", 0, ThemeIntegrityDiagnosticCodes.PackageReadFailure)]
    [InlineData("meta/integrity.json", 0, ThemeIntegrityDiagnosticCodes.IntegrityManifestHashUnavailable)]
    [InlineData("meta/signature.json", 0, ThemeIntegrityDiagnosticCodes.MissingSignature)]
    [InlineData("theme.json", 1, ThemeIntegrityDiagnosticCodes.PackageReadFailure)]
    [InlineData("meta/integrity.json", 1, ThemeIntegrityDiagnosticCodes.PackageReadFailure)]
    [InlineData("meta/signature.json", 1, ThemeIntegrityDiagnosticCodes.PackageReadFailure)]
    [InlineData("theme.json", 2, ThemeIntegrityDiagnosticCodes.PackageReadFailure)]
    [InlineData("meta/integrity.json", 2, ThemeIntegrityDiagnosticCodes.PackageReadFailure)]
    [InlineData("meta/signature.json", 2, ThemeIntegrityDiagnosticCodes.PackageReadFailure)]
    [InlineData("theme.json", 3, ThemeIntegrityDiagnosticCodes.PackageReadFailure)]
    [InlineData("meta/integrity.json", 3, ThemeIntegrityDiagnosticCodes.PackageReadFailure)]
    [InlineData("meta/signature.json", 3, ThemeIntegrityDiagnosticCodes.PackageReadFailure)]
    public async Task ApprovedReaderFailuresUseTheExactDiagnosticMatrix(
        string path,
        int failureKind,
        string expectedCode)
    {
        Scenario scenario = CreateScenario(includeSignature: true);
        scenario.Reader.SetFailure(path, ReaderFailure(failureKind));

        ThemePackageMetadataEvaluation result = await Evaluate(scenario);

        string? expectedPath = expectedCode == ThemeIntegrityDiagnosticCodes.PackageHashMismatch
            ? null
            : path;
        AssertDiagnostic(result, expectedCode, expectedPath);
        Assert.Equal(1, scenario.Reader.ReadPaths.Count(item => string.Equals(item, path, StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task UnrelatedReaderExceptionsPropagate(int kind)
    {
        Scenario scenario = CreateScenario();
        Exception failure = kind switch
        {
            0 => new InvalidOperationException("unrelated"),
            1 => new ArgumentException("unrelated"),
            _ => new NotSupportedException("unrelated"),
        };
        scenario.Reader.SetFailure("theme.json", failure);

        Exception actual = await Assert.ThrowsAsync(failure.GetType(), async () => await Evaluate(scenario));
        Assert.Same(failure, actual);
    }

    [Fact]
    public async Task CancellationPropagatesAtEntryAndEveryMetadataBoundary()
    {
        using CancellationTokenSource preCancelled = new();
        preCancelled.Cancel();
        Scenario entry = CreateScenario();
        OperationCanceledException entryException = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await ThemePackageMetadataEvaluator.EvaluateAsync(
                entry.Request,
                entry.Reader,
                entry.Inventory,
                preCancelled.Token));
        Assert.Equal(preCancelled.Token, entryException.CancellationToken);
        Assert.Empty(entry.Reader.ReadPaths);

        foreach (string path in new[] { "theme.json", "meta/integrity.json", "meta/signature.json" })
        {
            using CancellationTokenSource source = new();
            Scenario scenario = CreateScenario(includeSignature: true);
            scenario.Reader.BeforeRead = (readPath, token) =>
            {
                if (string.Equals(readPath, path, StringComparison.Ordinal))
                {
                    source.Cancel();
                }
            };
            OperationCanceledException exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => await ThemePackageMetadataEvaluator.EvaluateAsync(
                    scenario.Request,
                    scenario.Reader,
                    scenario.Inventory,
                    source.Token));
            Assert.Equal(source.Token, exception.CancellationToken);
        }
    }

    [Fact]
    public async Task CancellationWinsAnIoFailureRace()
    {
        using CancellationTokenSource source = new();
        Scenario scenario = CreateScenario();
        scenario.Reader.BeforeRead = (path, token) =>
        {
            source.Cancel();
            scenario.Reader.SetFailure(path, new IOException("private"));
        };

        OperationCanceledException exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await ThemePackageMetadataEvaluator.EvaluateAsync(
                scenario.Request,
                scenario.Reader,
                scenario.Inventory,
                source.Token));

        Assert.Equal(source.Token, exception.CancellationToken);
    }

    [Fact]
    public async Task TreeHashGoldenVectorsProveFramingOrderingExclusionsAndEntryKindIndependence()
    {
        Scenario baseline = CreateScenario();
        string expected = GoldenTreeHash(baseline.Inventory.FileEvidence);

        Scenario reversed = WithEvidence(
            CreateScenario(),
            [baseline.Inventory.FileEvidence[1], baseline.Inventory.FileEvidence[0]]);
        Assert.Equal(expected, (await Evaluate(reversed)).PackageHash);

        ThemeIntegrityFileEvidenceV1 reserved = baseline.Inventory.FileEvidence[1] with
        {
            CanonicalPath = "meta/integrity.json",
        };
        Scenario excludedReserved = WithEvidence(
            CreateScenario(),
            [baseline.Inventory.FileEvidence[0], reserved, baseline.Inventory.FileEvidence[1]]);
        Assert.Equal(expected, (await Evaluate(excludedReserved)).PackageHash);

        ThemeIntegrityFileEvidenceV1 optionalMissing = baseline.Inventory.FileEvidence[1] with
        {
            IsPresent = false,
            ActualLengthBytes = null,
            ActualSha256 = null,
            Status = ThemeIntegrityEvidenceStatus.MissingAllowed,
        };
        Scenario omitted = WithEvidence(CreateScenario(), [baseline.Inventory.FileEvidence[0], optionalMissing]);
        ThemeIntegrityManifestV2 omittedManifest = omitted.Request.IntegrityManifest with
        {
            Files = [omitted.Request.IntegrityManifest.Files[0], omitted.Request.IntegrityManifest.Files[1] with { Required = false }],
            PackageHash = GoldenTreeHash(omitted.Inventory.FileEvidence),
        };
        omitted = WithRawAndCallerIntegrity(omitted, omittedManifest, omittedManifest);
        Assert.True((await Evaluate(omitted)).CanContinue);

        ThemeIntegrityFileEvidenceV1 changedKind = baseline.Inventory.FileEvidence[1] with
        {
            EntryKind = ThemeIntegrityEntryKind.ThemeManifest,
        };
        Assert.Equal(expected, GoldenTreeHash([baseline.Inventory.FileEvidence[0], changedKind]));

        Assert.NotEqual(expected, GoldenTreeHash([
            baseline.Inventory.FileEvidence[0] with { CanonicalPath = "Theme.json" },
            baseline.Inventory.FileEvidence[1],
        ]));
        Assert.NotEqual(expected, GoldenTreeHash([
            baseline.Inventory.FileEvidence[0],
            baseline.Inventory.FileEvidence[1] with { CanonicalPath = "assets/other.png" },
        ]));
        Assert.NotEqual(expected, GoldenTreeHash([
            baseline.Inventory.FileEvidence[0],
            baseline.Inventory.FileEvidence[1] with { ActualLengthBytes = 4 },
        ]));
        Assert.NotEqual(expected, GoldenTreeHash([
            baseline.Inventory.FileEvidence[0],
            baseline.Inventory.FileEvidence[1] with { ActualSha256 = new string('b', 64) },
        ]));

        byte[] exactRecord = Encoding.UTF8.GetBytes(
            "theme.json\0" + baseline.ThemeBytes.LongLength.ToString(CultureInfo.InvariantCulture)
            + "\0" + Hash(baseline.ThemeBytes) + "\n");
        Assert.False(exactRecord.AsSpan().StartsWith(Encoding.UTF8.GetPreamble()));
        Assert.Contains((byte)0, exactRecord);
        Assert.Equal((byte)10, exactRecord[^1]);
    }

    [Fact]
    public async Task CompleteOutcomeIsIdenticalAcrossCulturesOrdersAndFiftyRuns()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            ThemePackageMetadataEvaluation? expected = null;
            foreach (string cultureName in new[] { "en-US", "tr-TR", "zh-TW" })
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
                for (int reverse = 0; reverse < 2; reverse++)
                {
                    for (int run = 0; run < 50; run++)
                    {
                        Scenario scenario = CreateScenario();
                        if (reverse == 1)
                        {
                            scenario = WithEvidence(
                                scenario,
                                [scenario.Inventory.FileEvidence[1], scenario.Inventory.FileEvidence[0]]);
                        }

                        ThemePackageMetadataEvaluation actual = await Evaluate(scenario);
                        expected ??= actual;
                        AssertOutcomeEqual(expected, actual);
                    }
                }
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    private static async Task<ThemePackageMetadataEvaluation> Evaluate(Scenario scenario) =>
        await ThemePackageMetadataEvaluator.EvaluateAsync(
            scenario.Request,
            scenario.Reader,
            scenario.Inventory);

    private static Scenario CreateScenario(
        string themePath = "theme.json",
        bool includeSignature = false)
    {
        ThemeManifest theme = CreateValidTheme();
        byte[] themeBytes = JsonSerializer.SerializeToUtf8Bytes(
            theme,
            ThemeContractJson.CreateSerializerOptions());
        byte[] payloadBytes = [1, 2, 3];
        ThemeIntegrityFileV2 themeFile = new(
            themePath,
            ThemeIntegrityEntryKind.ThemeManifest,
            Hash(themeBytes),
            themeBytes.LongLength,
            true);
        ThemeIntegrityFileV2 payloadFile = new(
            "assets/icon.png",
            ThemeIntegrityEntryKind.Payload,
            Hash(payloadBytes),
            payloadBytes.LongLength,
            true);
        ThemeIntegrityFileEvidenceV1 themeEvidence = Evidence(themeFile, themeBytes);
        ThemeIntegrityFileEvidenceV1 payloadEvidence = Evidence(payloadFile, payloadBytes);
        ThemeIntegrityFileEvidenceV1[] evidence = [themeEvidence, payloadEvidence];
        string packageHash = GoldenTreeHash(evidence);
        ThemeIntegrityManifestV2 integrity = new(
            ContractVersions.ThemeIntegritySchemaV2,
            ThemeIntegrityInventoryMode.ExhaustiveAllowedPayload,
            "com.example.theme",
            "1.2.3",
            ThemeIntegrityContractSemantics.HashAlgorithm,
            [themeFile, payloadFile],
            packageHash);
        byte[] integrityBytes = JsonSerializer.SerializeToUtf8Bytes(
            integrity,
            ThemeContractJson.CreateSerializerOptions());

        ThemeSignatureEnvelopeV1? envelope = includeSignature
            ? new ThemeSignatureEnvelopeV1(
                "1.0",
                ThemeSignatureAlgorithm.EcdsaP256Sha256,
                "example.publisher",
                "key-1",
                ThemeSignatureEncoding.IeeeP1363FixedFieldConcatenation,
                Convert.ToBase64String(new byte[64]),
                ThemeSignedPayloadType.TccThemePackageSignatureV1)
            : null;
        ThemeCanonicalPath? envelopePath = includeSignature
            ? ThemeCanonicalPath.Parse("meta/signature.json")
            : null;
        ThemeIntegrityVerificationRequestV2 request = new(
            new ThemeId("com.example.theme"),
            new ThemeVersion("1.2.3"),
            new ThemePackageRef("opaque:test-package"),
            ThemeCanonicalPath.Parse(themePath),
            ThemeCanonicalPath.Parse("meta/integrity.json"),
            envelopePath,
            integrity,
            envelope,
            new ThemeIntegrityVerificationPolicyV1(
                "beta",
                "1",
                "trust",
                "1",
                ThemeDistributionChannel.Beta,
                ThemeSignatureRequirement.Optional,
                false),
            new ThemeTrustSnapshotV1("trust", "1", []));

        Dictionary<string, byte[]> bytes = new(StringComparer.Ordinal)
        {
            [themePath] = themeBytes,
            ["meta/integrity.json"] = integrityBytes,
        };
        if (envelope is not null)
        {
            bytes["meta/signature.json"] = JsonSerializer.SerializeToUtf8Bytes(
                envelope,
                ThemeContractJson.CreateSerializerOptions());
        }

        return new Scenario(
            request,
            new ThemePackageInventoryEvaluation(true, evidence, []),
            new RecordingReader(bytes),
            themeBytes,
            integrityBytes);
    }

    private static ThemeManifest CreateValidTheme() => new(
        ContractVersions.Schema,
        ContractVersions.ThemeApi,
        new ThemePackageIdentity(
            "com.example.theme",
            "com.example.theme.package",
            "Example Theme",
            "Example Publisher",
            "example-publisher",
            "1.2.3",
            "stable",
            "Presentation-only example theme.",
            null,
            null,
            "private",
            ["deep"]),
        new ThemeCompatibilityDeclaration(
            ">=1.0.0 <2.0.0",
            ">=1.0.0 <2.0.0",
            ">=1.1.0 <2.0.0",
            ["windows"],
            true,
            1m,
            3m),
        [new ThemeVariant("deep", "deep", "Deep", true, "tokens/deep.json", [], null)],
        ["presentation.tokens"],
        new ThemeFeatureFlags([]),
        new ThemeDegradedModeDeclaration(
            true,
            ["none", "reduced_decoration", "minimal_decoration", "safe_presentation_only"],
            [
                "safety_core",
                "critical_alerts",
                "keyboard",
                "screen_reader",
                "contrast",
                "risk_permission_semantics",
                "confirmation_semantics",
                "accessibility",
            ],
            "safe_presentation_only"),
        new ThemeAccessibilityDeclaration(true, true, true, true, true, true, true, true, true, true, true, true),
        new ThemeAssetDeclaration("assets/index.json", ["tier0"], 0, "sha256"),
        null,
        new ThemeMotionDeclaration("motion/profiles.json", "motion/reduced.json", true, true, true),
        null,
        new ThemeIntegrityDeclaration("integrity.json", "signature.sig", "stable_or_store", "sha256"),
        null);

    private static Scenario WithEvidence(
        Scenario scenario,
        IReadOnlyList<ThemeIntegrityFileEvidenceV1> evidence) =>
        scenario with
        {
            Inventory = new ThemePackageInventoryEvaluation(true, evidence, []),
        };

    private static Scenario RebindThemeEvidence(Scenario scenario, byte[] raw)
    {
        ThemeIntegrityFileEvidenceV1 rebound = scenario.Inventory.FileEvidence[0] with
        {
            ActualLengthBytes = raw.LongLength,
            ActualSha256 = Hash(raw),
        };
        scenario.Reader.SetBytes(scenario.Request.ThemeManifestPath.Value, raw);
        return WithEvidence(scenario, [rebound, scenario.Inventory.FileEvidence[1]]) with { ThemeBytes = raw };
    }

    private static Scenario WithRawAndCallerIntegrity(
        Scenario scenario,
        ThemeIntegrityManifestV2 raw,
        ThemeIntegrityManifestV2 caller)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(raw, ThemeContractJson.CreateSerializerOptions());
        scenario.Reader.SetBytes("meta/integrity.json", bytes);
        return scenario with
        {
            Request = scenario.Request with { IntegrityManifest = caller },
            IntegrityBytes = bytes,
        };
    }

    private static ThemeIntegrityFileEvidenceV1 Evidence(ThemeIntegrityFileV2 file, byte[] bytes) =>
        new(
            file.CanonicalPath,
            file.EntryKind,
            file.Required,
            true,
            file.LengthBytes,
            bytes.LongLength,
            file.Sha256,
            Hash(bytes),
            ThemeIntegrityEvidenceStatus.Verified);

    private static string GoldenTreeHash(IReadOnlyList<ThemeIntegrityFileEvidenceV1> source)
    {
        ThemeIntegrityFileEvidenceV1[] evidence = source
            .Where(item => item.IsPresent && item.Status == ThemeIntegrityEvidenceStatus.Verified)
            .OrderBy(item => item.CanonicalPath.Normalize(NormalizationForm.FormC), StringComparer.Ordinal)
            .ToArray();
        using MemoryStream bytes = new();
        foreach (ThemeIntegrityFileEvidenceV1 item in evidence)
        {
            byte[] record = Encoding.UTF8.GetBytes(
                item.CanonicalPath.Normalize(NormalizationForm.FormC)
                + "\0"
                + item.ActualLengthBytes!.Value.ToString(CultureInfo.InvariantCulture)
                + "\0"
                + item.ActualSha256
                + "\n");
            bytes.Write(record);
        }

        return Hash(bytes.ToArray());
    }

    private static bool ValidateSchema(string document, string schema) =>
        ThemeMetadataSchemaValidator.ValidateAgainstSchemaBytes(
            Encoding.UTF8.GetBytes(document),
            Encoding.UTF8.GetBytes(schema));

    private static void AssertSchema(string document, string schema, bool expected) =>
        Assert.Equal(expected, ValidateSchema(document, schema));

    private static void AssertEntryGateFailure(
        ThemePackageMetadataEvaluation result,
        Scenario scenario)
    {
        AssertDiagnostic(result, ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument,
            scenario.Request.ThemeManifestPath.Value);
        Assert.Equal(
            "Theme Manifest must be present as verified package content before metadata validation.",
            Assert.Single(result.Diagnostics).Message);
        AssertNoHashesOrMetadata(result);
        Assert.Empty(scenario.Reader.ReadPaths);
    }

    private static void AssertDiagnostic(
        ThemePackageMetadataEvaluation result,
        string code,
        string? path)
    {
        Assert.False(result.CanContinue);
        ThemeIntegrityDiagnosticV1 diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(code, diagnostic.Code);
        Assert.Equal(path, diagnostic.CanonicalPath);
        Assert.Equal(ThemeDiagnosticsSeverity.Error, diagnostic.Severity);
    }

    private static void AssertNoHashesOrMetadata(ThemePackageMetadataEvaluation result)
    {
        Assert.Null(result.PackageHash);
        Assert.Null(result.ThemeManifestHash);
        Assert.Null(result.IntegrityManifestHash);
        Assert.Null(result.ThemeManifest);
        Assert.Null(result.IntegrityManifest);
        Assert.Null(result.SignatureEnvelope);
    }

    private static void AssertOutcomeEqual(
        ThemePackageMetadataEvaluation expected,
        ThemePackageMetadataEvaluation actual)
    {
        Assert.Equal(expected.CanContinue, actual.CanContinue);
        Assert.Equal(expected.Diagnostics.ToArray(), actual.Diagnostics.ToArray());
        Assert.Equal(expected.PackageHash, actual.PackageHash);
        Assert.Equal(expected.ThemeManifestHash, actual.ThemeManifestHash);
        Assert.Equal(expected.IntegrityManifestHash, actual.IntegrityManifestHash);
        Assert.Equal(
            JsonSerializer.Serialize(expected.ThemeManifest, ThemeContractJson.CreateSerializerOptions()),
            JsonSerializer.Serialize(actual.ThemeManifest, ThemeContractJson.CreateSerializerOptions()));
        Assert.True(IntegrityManifestsEqual(expected.IntegrityManifest!, actual.IntegrityManifest!));
        Assert.Equal(expected.SignatureEnvelope, actual.SignatureEnvelope);
    }

    private static bool IntegrityManifestsEqual(
        ThemeIntegrityManifestV2 left,
        ThemeIntegrityManifestV2 right) =>
        JsonSerializer.Serialize(left, ThemeContractJson.CreateSerializerOptions())
        == JsonSerializer.Serialize(right, ThemeContractJson.CreateSerializerOptions());

    private static Exception ReaderFailure(int kind) => kind switch
    {
        0 => new FileNotFoundException("private"),
        1 => new IOException("private"),
        2 => new InvalidDataException("private"),
        _ => new UnauthorizedAccessException("private"),
    };

    private static string Hash(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexStringLower(SHA256.HashData(bytes));

    private sealed record Scenario(
        ThemeIntegrityVerificationRequestV2 Request,
        ThemePackageInventoryEvaluation Inventory,
        RecordingReader Reader,
        byte[] ThemeBytes,
        byte[] IntegrityBytes);

    private sealed class RecordingReader(Dictionary<string, byte[]> bytes) : IThemePackageContentReader
    {
        private readonly Dictionary<string, byte[]> content = new(bytes, StringComparer.Ordinal);
        private readonly Dictionary<string, Exception> failures = new(StringComparer.Ordinal);

        public List<string> ReadPaths { get; } = [];
        public Action<string, CancellationToken>? BeforeRead { get; set; }

        public ValueTask<IReadOnlyList<ThemePackageContentEntryV1>> EnumerateEntriesAsync(
            ThemePackageRef packageRef,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Phase 4C must not enumerate package content.");

        public ValueTask<ReadOnlyMemory<byte>> ReadContentAsync(
            ThemePackageRef packageRef,
            string canonicalPath,
            CancellationToken cancellationToken = default)
        {
            Assert.Equal(new ThemePackageRef("opaque:test-package"), packageRef);
            Assert.Equal(canonicalPath.Normalize(NormalizationForm.FormC), canonicalPath);
            Assert.DoesNotContain(canonicalPath, ReadPaths);
            ReadPaths.Add(canonicalPath);
            BeforeRead?.Invoke(canonicalPath, cancellationToken);
            if (failures.TryGetValue(canonicalPath, out Exception? failure))
            {
                throw failure;
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (!content.TryGetValue(canonicalPath, out byte[]? value))
            {
                throw new FileNotFoundException("private");
            }

            return ValueTask.FromResult<ReadOnlyMemory<byte>>(value);
        }

        public void SetBytes(string path, byte[] value) => content[path] = value;

        public void SetFailure(string path, Exception failure) => failures[path] = failure;
    }
}
