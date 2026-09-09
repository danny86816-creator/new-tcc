using System.Collections.Immutable;
using System.Formats.Asn1;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Integrity;
using Status = Tcc.Presentation.Contracts.Theme.ThemeSignatureVerificationStatus;

namespace Tcc.Architecture.Tests;

public sealed class PhaseFourThemePackageSignatureEvaluatorTests
{
    private const string GoldenKey = "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEeT68e7qekZ59xr4povRF6e9gB8M4GErSkxdIsmwVDfRVmfmXa9duEKfoOCulrcN/bSeXbd4M+2OMcMKmEsXmfw==";
    private const string GoldenSignature = "pyTXo1K40x+l8IHxodTZ30zgKa2pAv4sW4QP/9j7ILZrEtHLbhSp9qfbcm8IYcv0fx7RaeRhk0gb9RnBfFSMaw==";
    private const string GoldenHex = "5443432d5448454d452d5041434b4147452d5349474e41545552452d563100636f6d2e6578616d706c652e7468656d6500312e322e33006161616161616161616161616161616161616161616161616161616161616161616161616161616161616161616161616161616161616161616161616161616100626262626262626262626262626262626262626262626262626262626262626262626262626262626262626262626262626262626262626262626262626262620063636363636363636363636363636363636363636363636363636363636363636363636363636363636363636363636363636363636363636363636363636363007075626c69736865722e6578616d706c65006b65792d323032362d3031";
    private const string FieldPrime = "FFFFFFFF00000001000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFF";
    // SEC 2 / NIST P-256 base point, independent of the sealed signer fixture.
    private const string Generator = "6B17D1F2E12C4247F8BCE6E563A440F277037D812DEB33A0F4A13945D898C2964FE342E2FE1A7F9B8EE7EB4A7C0F9E162BCE33576B315ECECBB6406837BF51F5";

    [Fact]
    public void LiteralGoldenPayloadAndSealedSignatureVerifyThroughProduction()
    {
        Scenario scenario = Golden();
        byte[] actual = (byte[])typeof(ThemePackageSignatureEvaluator)
            .GetMethod("BuildPayload", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, [scenario.Metadata, scenario.Metadata.SignatureEnvelope])!;
        Assert.Equal(Convert.FromHexString(GoldenHex), actual);
        Assert.Equal(7, actual.Count(value => value == 0));
        Assert.NotEqual((byte)0, actual[^1]);
        Assert.DoesNotContain((byte)'\n', actual);
        AssertSuccess(Evaluate(scenario), Status.Valid, "publisher.example", "key-2026-01");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void EverySignedFieldIsCryptographicallyBound(int field)
    {
        Scenario s = Golden();
        ThemePackageMetadataEvaluation m = s.Metadata;
        if (field == 0) m = m with { ThemeManifest = m.ThemeManifest! with { Package = m.ThemeManifest.Package with { ThemeId = "com.changed.theme" } }, IntegrityManifest = m.IntegrityManifest! with { ThemeId = "com.changed.theme" } };
        if (field == 1) m = m with { ThemeManifest = m.ThemeManifest! with { Package = m.ThemeManifest.Package with { Version = "1.2.4" } }, IntegrityManifest = m.IntegrityManifest! with { Version = "1.2.4" } };
        if (field == 2) m = m with { PackageHash = new string('d', 64), IntegrityManifest = m.IntegrityManifest! with { PackageHash = new string('d', 64) } };
        if (field == 3) m = m with { ThemeManifestHash = new string('d', 64) };
        if (field == 4) m = m with { IntegrityManifestHash = new string('d', 64) };
        s = s with { Metadata = m };
        if (field == 5) s = Identity(s, "publisher.changed", "key-2026-01");
        if (field == 6) s = Identity(s, "publisher.example", "key-changed");
        AssertFailure(Evaluate(s), "P4I013");
    }

    [Fact]
    public void BothEquivalentLowAndHighSSignaturesAreAccepted()
    {
        BigInteger order = new(Convert.FromHexString("FFFFFFFF00000000FFFFFFFFFFFFFFFFBCE6FAADA7179E84F3B9CAC2FC632551"), true, true);
        byte[] bytes = Convert.FromBase64String(GoldenSignature);
        BigInteger original = new(bytes.AsSpan(32), true, true);
        BigInteger opposite = order - original;
        Assert.True(BigInteger.Min(original, opposite) <= order / 2);
        Assert.True(BigInteger.Max(original, opposite) > order / 2);
        byte[] changed = (byte[])bytes.Clone();
        Array.Clear(changed, 32, 32);
        byte[] encoded = opposite.ToByteArray(true, true);
        encoded.CopyTo(changed, 64 - encoded.Length);
        AssertSuccess(Evaluate(Golden()), Status.Valid, "publisher.example", "key-2026-01");
        AssertSuccess(Evaluate(WithSignature(Golden(), Convert.ToBase64String(changed))), Status.Valid, "publisher.example", "key-2026-01");
    }

    private static string?[] SigningIds() => [null, "", "\0", "\u001f", " ", "\u007f", "\u009f", "\u00a0",
            new('a',255), new('a',256), new('a',257), new('é',128), new('é',129),
            string.Concat(Enumerable.Repeat("😀",64)), string.Concat(Enumerable.Repeat("😀",65)),
            "\ud800", "\udc00", "a\ud800b", " pub ", "PUB", "é", "e\u0301", "рub"];

    public static IEnumerable<object[]> IdCases()
    {
        string?[] ids = SigningIds();
        bool[] accepted = [false, false, false, false, true, false, false, true, true, true, false, true, false, true, false, false, false, false, true, true, true, true, true];
        for (int position = 0; position < 4; position++)
            for (int index = 0; index < ids.Length; index++)
                yield return [position, index, accepted[index]];
    }

    [Theory]
    [MemberData(nameof(IdCases))]
    public void ExactUtf8ScalarProfileAppliesToEverySigningIdentity(int position, int idIndex, bool accepted)
    {
        // Construct malformed UTF-16 inside the test; runner serialization otherwise replaces
        // isolated surrogates with U+FFFD and collapses distinct invalid vectors into one ID.
        string? id = SigningIds()[idIndex];
        Scenario s = Golden();
        if (position == 0) s = Identity(s, id!, "key-2026-01");
        if (position == 1) s = Identity(s, "publisher.example", id!);
        if (position >= 2)
        {
            ThemeTrustedSignerV1 other = s.Trust.TrustedSigners[0] with { PublisherId = "other", KeyId = "other" };
            other = position == 2 ? other with { PublisherId = id! } : other with { KeyId = id! };
            s = s with { Trust = s.Trust with { TrustedSigners = [s.Trust.TrustedSigners[0], other] } };
        }

        if (!accepted) AssertFailure(Evaluate(s), "P4I029");
        else if (position < 2) AssertFailure(Evaluate(s), "P4I013"); // identity passed; old signature must fail
        else AssertSuccess(Evaluate(s), Status.Valid, "publisher.example", "key-2026-01");
    }

    [Theory]
    [InlineData("pub\0a", "b")]
    [InlineData("pub", "a\0b")]
    public void BothNulFramingCollisionCandidatesAreRejected(string publisher, string key) =>
        AssertFailure(Evaluate(Identity(Golden(), publisher, key)), "P4I029");

    [Theory]
    [InlineData("Publisher.example", "publisher.example")]
    [InlineData("é", "e\u0301")]
    [InlineData("pub ", "pub")]
    [InlineData("pub", "рub")]
    public void ManifestPublisherBindingIsExactWithoutNormalization(string manifest, string envelope)
    {
        Scenario s = Identity(Golden(), envelope, "key");
        s = s with { Metadata = s.Metadata with { ThemeManifest = s.Metadata.ThemeManifest! with { Package = s.Metadata.ThemeManifest.Package with { PublisherId = manifest } } } };
        AssertFailure(Evaluate(s), "P4I029");
    }

    [Theory]
    [InlineData(" ", " ")]
    [InlineData(" pub ", "key-with-no-manifest-binding")]
    [InlineData("é", "e\u0301")]
    [InlineData("PUB", "рub")]
    public void IndependentlySignedExactIdentityBytesAreAccepted(string publisher, string keyId)
    {
        Scenario s = Identity(Golden(), publisher, keyId);
        using ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        byte[] prefix = Convert.FromHexString(GoldenHex)[..^(Encoding.UTF8.GetByteCount("publisher.example\0key-2026-01"))];
        byte[] literalPayload = [.. prefix, .. Encoding.UTF8.GetBytes(publisher + "\0" + keyId)];
        s = WithKey(s, Convert.ToBase64String(key.ExportSubjectPublicKeyInfo()));
        s = WithSignature(s, Convert.ToBase64String(key.SignData(literalPayload, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation)));
        AssertSuccess(Evaluate(s), Status.Valid, publisher, keyId);
    }

    [Theory]
    [InlineData("Publisher.example")]
    [InlineData("publisher.example ")]
    [InlineData("publisher.examplе")]
    public void TrustLookupNeverNormalizesOrMatchesOnlyKey(string publisher)
    {
        Scenario s = Golden();
        s = s with { Trust = s.Trust with { TrustedSigners = [s.Trust.TrustedSigners[0] with { PublisherId = publisher }] } };
        AssertFailure(Evaluate(s), "P4I014");
    }

    public static IEnumerable<object[]> SignatureCases()
    {
        string canonical = GoldenSignature;
        yield return ["whitespace", canonical[..10] + " " + canonical[11..]];
        yield return ["URL-safe", canonical.Replace('+', '-').Replace('/', '_')];
        yield return ["missing padding", canonical[..^2]];
        yield return ["wrong padding", canonical[..^2] + "=A"];
        yield return ["pad bits", canonical[..^3] + "x=="];
        yield return ["63 bytes", Convert.ToBase64String(new byte[63])];
        yield return ["65 bytes", Convert.ToBase64String(new byte[65])];
        yield return ["zero", Convert.ToBase64String(new byte[64])];
        byte[] rZero = Convert.FromBase64String(canonical); Array.Clear(rZero, 0, 32);
        yield return ["r zero", Convert.ToBase64String(rZero)];
        byte[] sZero = Convert.FromBase64String(canonical); Array.Clear(sZero, 32, 32);
        yield return ["s zero", Convert.ToBase64String(sZero)];
        foreach (int offset in new[] { 0, 32 })
        {
            byte[] outOfRange = Convert.FromBase64String(canonical);
            Array.Fill(outOfRange, (byte)255, offset, 32);
            yield return ["component out of range " + offset, Convert.ToBase64String(outOfRange)];
        }
        using ECDsa key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        yield return ["DER", Convert.ToBase64String(key.SignData(Convert.FromHexString(GoldenHex), HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence))];
    }

    [Theory]
    [MemberData(nameof(SignatureCases))]
    public void NonCanonicalOrCryptographicallyInvalidSignatureFails(string description, string signature)
    {
        Assert.NotEmpty(description);
        AssertFailure(Evaluate(WithSignature(Golden(), signature)), "P4I013");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public void PublicKeyCanonicalRepresentationFailsBeforeProfile(int mutation)
    {
        string key = mutation switch
        {
            0 => GoldenKey[..10] + " " + GoldenKey[11..],
            1 => GoldenKey.Replace('+', '-').Replace('/', '_'),
            2 => GoldenKey[..^2],
            3 => GoldenKey[..^3] + "x==",
            4 => GoldenKey + "AAAA",
            5 => Convert.ToBase64String(new byte[90]),
            6 => Convert.ToBase64String(new byte[92]),
            7 => GoldenKey[..^2] + "=A",
            _ => "!" + GoldenKey[1..],
        };
        AssertFailure(Evaluate(WithKey(Golden(), key)), "P4I013");
    }

    public static IEnumerable<object[]> SpkiCases()
    {
        foreach (string kind in new[]{"algorithm","curve","explicit","absent","NULL","trailing","unused",
            "length","prefix","compressed","hybrid","infinity","DER","zero","X=p","Y=p","X=max","Y=max","X bit","Y bit"})
        {
            byte[] bytes = Convert.FromBase64String(GoldenKey);
            string expected = "P4I013";
            switch (kind)
            {
                case "algorithm": bytes[12] = 2; expected = "P4I016"; break;
                case "curve": bytes[22] = 8; expected = "P4I016"; break;
                case "explicit": bytes[13] = 0x30; expected = "P4I016"; break;
                case "absent": bytes = [.. bytes[..13], .. bytes[23..]]; bytes[1] -= 10; bytes[3] -= 10; break;
                case "NULL": bytes = [.. bytes[..13], 5, 0, .. bytes[23..]]; bytes[1] -= 8; bytes[3] -= 8; break;
                case "trailing": bytes = [.. bytes, 0]; break;
                case "unused": bytes[25] = 1; bytes[^1] &= 0xfe; break;
                case "length": bytes = bytes[..^1]; bytes[1]--; bytes[24]--; break;
                case "prefix": bytes[26] = 5; expected = "P4I016"; break;
                case "compressed": bytes = [.. bytes[..59]]; bytes[1] -= 32; bytes[24] -= 32; bytes[26] = 2; break;
                case "hybrid": bytes[26] = 6; expected = "P4I016"; break;
                case "infinity": bytes[26] = 0; expected = "P4I016"; break;
                case "DER": bytes[0] = 0x31; break;
                case "zero": Array.Clear(bytes, 27, 64); break;
                case "X=p": Convert.FromHexString(FieldPrime).CopyTo(bytes, 27); break;
                case "Y=p": Convert.FromHexString(FieldPrime).CopyTo(bytes, 59); break;
                case "X=max": Array.Fill(bytes, (byte)255, 27, 32); break;
                case "Y=max": Array.Fill(bytes, (byte)255, 59, 32); break;
                case "X bit": bytes[58] ^= 1; break;
                case "Y bit": bytes[90] ^= 1; break;
            }
            yield return [kind, Convert.ToBase64String(bytes), expected];
        }
    }

    [Theory]
    [MemberData(nameof(SpkiCases))]
    public void SpkiRepresentationProfileAndAffinePointUseExactPrecedence(string kind, string key, string expected)
    {
        Assert.NotEmpty(kind);
        AssertFailure(Evaluate(WithKey(Golden(), key)), expected);
    }

    [Fact]
    public void IndependentStandardGeneratorPointImportsAndVerifiesRealSignature()
    {
        byte[] coordinates = Convert.FromHexString(Generator);
        byte[] privateScalar = new byte[32]; privateScalar[^1] = 1;
        using ECDsa key = ECDsa.Create(new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q = new ECPoint { X = coordinates[..32], Y = coordinates[32..] },
            D = privateScalar,
        });
        Scenario s = WithKey(Golden(), Convert.ToBase64String(key.ExportSubjectPublicKeyInfo()));
        s = WithSignature(s, Convert.ToBase64String(key.SignData(Convert.FromHexString(GoldenHex), HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation)));
        AssertSuccess(Evaluate(s), Status.Valid, "publisher.example", "key-2026-01");
    }

    [Theory]
    [InlineData("absent", 75)]
    [InlineData("NULL", 73)]
    [InlineData("explicit", 73)]
    public void UnsupportedParametersWithExact91ByteDerReachProfileStage(string parameters, int pointLength)
    {
        AsnWriter writer = new(AsnEncodingRules.DER);
        using (writer.PushSequence())
        {
            using (writer.PushSequence())
            {
                writer.WriteObjectIdentifier("1.2.840.10045.2.1");
                if (parameters == "NULL") writer.WriteNull();
                if (parameters == "explicit") { using (writer.PushSequence()) { } }
            }
            byte[] point = new byte[pointLength]; point[0] = 4;
            writer.WriteBitString(point);
        }
        byte[] encoded = writer.Encode(); Assert.Equal(91, encoded.Length);
        AssertFailure(Evaluate(WithKey(Golden(), Convert.ToBase64String(encoded))), "P4I016");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ExactLengthMalformedDerFailsFullConsumption(int kind)
    {
        byte[] bytes = Convert.FromBase64String(GoldenKey);
        if (kind == 0) bytes[3] += 2; // AlgorithmIdentifier leaves extra data after its curve OID.
        if (kind == 1) bytes[1]--; // Trailing data in outer reader / truncated BIT STRING.
        if (kind == 2) bytes[24]--; // Trailing data inside the SPKI sequence.
        Assert.Equal(91, bytes.Length);
        AssertFailure(Evaluate(WithKey(Golden(), Convert.ToBase64String(bytes))), "P4I013");
    }

    public static IEnumerable<object[]> ChannelCases()
    {
        foreach (ThemeDistributionChannel channel in Enum.GetValues<ThemeDistributionChannel>())
            foreach (ThemeSignatureRequirement required in Enum.GetValues<ThemeSignatureRequirement>())
                foreach (bool exception in new[] { false, true })
                    foreach (string state in new[] { "absent", "valid", "unknown", "invalid", "revoked", "malformed key" })
                        yield return [channel, required, exception, state];
    }

    [Theory]
    [MemberData(nameof(ChannelCases))]
    public void ExhaustiveChannelRequirementExceptionAndEnvelopeMatrix(
        ThemeDistributionChannel channel, ThemeSignatureRequirement requirement, bool exception, string state)
    {
        Scenario s = State(state) with { Policy = Golden().Policy with { DistributionChannel = channel, SignatureRequirement = requirement, DeveloperExceptionAuthorized = exception } };
        // Independent decision table indexed by channel/requirement/flag. Entries specify policy
        // failure first; legal presence then uses the independent six-state expected outcome table.
        string?[] policyFailures = channel switch
        {
            ThemeDistributionChannel.Stable or ThemeDistributionChannel.Store => [null, "P4I018", "P4I017", "P4I018"],
            ThemeDistributionChannel.Beta => [null, "P4I018", null, "P4I018"],
            ThemeDistributionChannel.Developer => [null, "P4I018", null, null],
            _ => throw new InvalidOperationException(),
        };
        string? code = policyFailures[(requirement == ThemeSignatureRequirement.Required ? 0 : 2) + (exception ? 1 : 0)];
        if (code is not null) { AssertFailure(Evaluate(s), code); return; }
        if (state == "absent")
        {
            if (requirement == ThemeSignatureRequirement.Required) AssertFailure(Evaluate(s), "P4I012");
            else if (channel == ThemeDistributionChannel.Beta) AssertSuccess(Evaluate(s), Status.NotRequired, null, null);
            else if (exception) AssertSuccess(Evaluate(s), Status.DeveloperExceptionAccepted, null, null);
            else AssertFailure(Evaluate(s), "P4I018", Status.Missing);
            return;
        }
        if (state == "valid") AssertSuccess(Evaluate(s), Status.Valid, "publisher.example", "key-2026-01");
        else AssertFailure(Evaluate(s), state switch { "unknown" => "P4I014", "revoked" => "P4I015", _ => "P4I013" });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    public void MixedFailuresSelectTheEarliestDecision015Stage(int pair)
    {
        Scenario s = Golden();
        ThemeTrustedSignerV1 signer = s.Trust.TrustedSigners[0];
        string code;
        switch (pair)
        {
            case 0: s = State("absent") with { Policy = s.Policy with { ExpectedTrustPolicyId = "wrong" } }; code = "P4I023"; break;
            case 1: s = s with { Policy = s.Policy with { SignatureRequirement = ThemeSignatureRequirement.Optional, DeveloperExceptionAuthorized = true } }; code = "P4I018"; break;
            case 2: s = State("absent") with { Policy = s.Policy with { SignatureRequirement = ThemeSignatureRequirement.Optional } }; code = "P4I017"; break;
            case 3: s = WithSignature(Identity(s, "bad\0id", "key"), "bad"); code = "P4I029"; break;
            case 4: s = WithSignature(s, "bad") with { Trust = s.Trust with { TrustedSigners = [signer with { KeyId = "bad\0id" }] } }; code = "P4I013"; break;
            case 5: s = s with { Trust = s.Trust with { TrustedSigners = [signer, signer, signer with { KeyId = "bad\0id" }] } }; code = "P4I029"; break;
            case 6: s = s with { Trust = s.Trust with { TrustedSigners = [signer with { KeyId = "other" }, signer with { KeyId = "other" }] } }; code = "P4I022"; break;
            case 7: s = s with { Trust = s.Trust with { TrustedSigners = [signer with { PublisherId = "other", PublicKey = "bad" }] } }; code = "P4I014"; break;
            case 8: s = WithKey(State("revoked"), "bad"); code = "P4I015"; break;
            case 9: byte[] wrong = Convert.FromBase64String(GoldenKey); wrong[22] = 8; s = WithKey(s, Convert.ToBase64String(wrong) + "AAAA"); code = "P4I013"; break;
            default: s = WithSignature(State("curve"), Convert.ToBase64String(new byte[64])); code = "P4I016"; break;
        }
        AssertFailure(Evaluate(s), code);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BothTrustPolicyIdentityAndVersionAreBound(bool version)
    {
        Scenario s = Golden();
        s = s with { Trust = version ? s.Trust with { PolicyVersion = "2" } : s.Trust with { PolicyId = "TRUST" } };
        AssertFailure(Evaluate(s), "P4I023");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IdenticalAndConflictingDuplicatePairsFailEvenAfterTarget(bool revoked)
    {
        Scenario s = Golden(); ThemeTrustedSignerV1 signer = s.Trust.TrustedSigners[0];
        s = s with { Trust = s.Trust with { TrustedSigners = [signer, revoked ? signer with { TrustState = ThemeTrustState.Revoked } : signer] } };
        AssertFailure(Evaluate(s), "P4I022");
    }

    [Fact]
    public void ExactPairsPermitSharedPublisherOrKeyAndIgnoreUnrelatedKeyMaterial()
    {
        Scenario s = Golden(); ThemeTrustedSignerV1 signer = s.Trust.TrustedSigners[0];
        s = s with { Trust = s.Trust with { TrustedSigners = [signer with { PublisherId = "other", PublicKey = "bad" }, signer, signer with { KeyId = "other", PublicKey = "bad" }] } };
        AssertSuccess(Evaluate(s), Status.Valid, "publisher.example", "key-2026-01");
        s = s with { Metadata = s.Metadata with { SignatureEnvelope = s.Metadata.SignatureEnvelope! with { KeyId = "unknown" } } };
        AssertFailure(Evaluate(s), "P4I014");
    }

    [Theory]
    [InlineData("é", "e\u0301")]
    [InlineData("pub", " pub")]
    [InlineData("pub", "PUB")]
    public void SnapshotDistinctUnicodeIdentitiesAreNotDuplicatePairs(string first, string second)
    {
        Scenario s = Golden(); ThemeTrustedSignerV1 signer = s.Trust.TrustedSigners[0];
        s = s with { Trust = s.Trust with { TrustedSigners = [signer, signer with { PublisherId = first }, signer with { PublisherId = second }] } };
        AssertSuccess(Evaluate(s), Status.Valid, "publisher.example", "key-2026-01");
        s = Identity(Golden(), first, "key");
        s = s with { Trust = s.Trust with { TrustedSigners = [s.Trust.TrustedSigners[0] with { PublisherId = second }] } };
        AssertFailure(Evaluate(s), "P4I014");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    public void ImpossibleUpstreamStatesPropagateInternalFaults(int kind)
    {
        Scenario s = Golden(); ThemePackageMetadataEvaluation m = s.Metadata;
        m = kind switch
        {
            0 => m with { CanContinue = false },
            1 => m with { ThemeManifest = null },
            2 => m with { IntegrityManifest = null },
            3 => m with { PackageHash = null },
            4 => m with { ThemeManifestHash = "bad" },
            5 => m with { IntegrityManifestHash = null },
            6 => m with { SignatureEnvelope = m.SignatureEnvelope! with { Algorithm = (ThemeSignatureAlgorithm)99 } },
            7 => m with { SignatureEnvelope = m.SignatureEnvelope! with { SignatureEncoding = (ThemeSignatureEncoding)99 } },
            8 => m with { SignatureEnvelope = m.SignatureEnvelope! with { SignedPayloadType = (ThemeSignedPayloadType)99 } },
            _ => m with { Diagnostics = [new("P4I013", ThemeDiagnosticsSeverity.Error, null, "upstream failure")] },
        };
        Assert.Throws<InvalidOperationException>(() => Evaluate(s with { Metadata = m }));
    }

    [Fact]
    public void NullArgumentsAndUnexpectedMetadataAccessFaultsAreNotPackageDiagnostics()
    {
        Scenario s = Golden();
        Assert.Throws<ArgumentNullException>(() => ThemePackageSignatureEvaluator.Evaluate(null!, s.Policy, s.Trust));
        Assert.Throws<ArgumentNullException>(() => ThemePackageSignatureEvaluator.Evaluate(s.Metadata, null!, s.Trust));
        Assert.Throws<ArgumentNullException>(() => ThemePackageSignatureEvaluator.Evaluate(s.Metadata, s.Policy, null!));
        foreach (Exception fault in new Exception[] { new ArgumentException("private"), new InvalidOperationException("private"), new PlatformNotSupportedException("private"), new ObjectDisposedException("private") })
        {
            Exception actual = Assert.ThrowsAny<Exception>(() => Evaluate(s with { Metadata = s.Metadata with { Diagnostics = new FaultingDiagnostics(fault) } }));
            Assert.Same(fault, actual);
        }
    }

    [Fact]
    public void PreCancellationWinsEvenOverEntryFaultAndPreservesOriginalToken()
    {
        using CancellationTokenSource source = new(); source.Cancel(); Scenario s = Golden();
        OperationCanceledException fault = Assert.Throws<OperationCanceledException>(() => ThemePackageSignatureEvaluator.Evaluate(null!, s.Policy, s.Trust, source.Token));
        Assert.Equal(source.Token, fault.CancellationToken);
    }

    [Fact]
    public async Task CancellationDuringLargeTrustScanPropagatesWithoutOutcome()
    {
        Scenario s = Golden(); ThemeTrustedSignerV1 signer = s.Trust.TrustedSigners[0];
        s = s with { Trust = s.Trust with { TrustedSigners = Enumerable.Repeat(signer, 500000).ToImmutableArray() } };
        using CancellationTokenSource source = new();
        using ManualResetEventSlim started = new();
        Task<ThemePackageSignatureEvaluation> work = Task.Run(() => { started.Set(); return Evaluate(s, source.Token); }, CancellationToken.None);
        Assert.True(started.Wait(TimeSpan.FromSeconds(10)));
        source.Cancel();
        OperationCanceledException fault = await Assert.ThrowsAsync<OperationCanceledException>(async () => await work);
        Assert.Equal(source.Token, fault.CancellationToken);
    }

    [Theory]
    [InlineData("valid")]
    [InlineData("beta unsigned")]
    [InlineData("developer unsigned")]
    [InlineData("unknown")]
    [InlineData("revoked")]
    [InlineData("invalid")]
    [InlineData("identity")]
    [InlineData("duplicate")]
    [InlineData("curve")]
    public void FullOutcomeIsDeterministicForCulturesPermutationsAndFiftyRuns(string state)
    {
        Scenario s = State(state); string expected = Serialize(Evaluate(s));
        CultureInfo previous = CultureInfo.CurrentCulture; CultureInfo previousUi = CultureInfo.CurrentUICulture;
        try
        {
            foreach (string culture in new[] { "en-US", "tr-TR", "zh-TW" })
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture); CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
                ImmutableArray<ThemeTrustedSignerV1> signers = s.Trust.TrustedSigners;
                for (int permutation = 0; permutation < 3; permutation++)
                {
                    ImmutableArray<ThemeTrustedSignerV1> order = permutation switch
                    {
                        0 => signers,
                        1 => signers.Reverse().ToImmutableArray(),
                        _ => signers.Skip(1).Concat(signers.Take(1)).ToImmutableArray(),
                    };
                    Scenario permuted = s with { Trust = s.Trust with { TrustedSigners = order } };
                    for (int run = 0; run < 50; run++) Assert.Equal(expected, Serialize(Evaluate(permuted)));
                }
            }
        }
        finally { CultureInfo.CurrentCulture = previous; CultureInfo.CurrentUICulture = previousUi; }
    }

    [Fact]
    public async Task ConcurrentCallsDoNotContaminateSignerOrCryptoState()
    {
        Scenario[] inputs = [Golden(), State("unknown"), State("revoked"), State("invalid"), State("beta unsigned"), State("developer unsigned"), State("curve")];
        string[] expected = inputs.Select(input => Serialize(Evaluate(input))).ToArray();
        await Task.WhenAll(Enumerable.Range(0, 100).Select(index => Task.Run(() => Assert.Equal(expected[index % inputs.Length], Serialize(Evaluate(inputs[index % inputs.Length]))))));
    }

    [Fact]
    public void ProductionBoundaryHasOnlyTwoInternalTypesAndNoReaderOrFinalVerdict()
    {
        Type evaluator = typeof(ThemePackageSignatureEvaluator); Type outcome = typeof(ThemePackageSignatureEvaluation);
        Assert.True(evaluator.IsNotPublic && evaluator.IsAbstract && evaluator.IsSealed);
        Assert.True(outcome.IsNotPublic && outcome.IsSealed);
        Assert.Empty(evaluator.GetInterfaces());
        MethodInfo method = evaluator.GetMethod("Evaluate", BindingFlags.Static | BindingFlags.NonPublic)!;
        Assert.Equal(new[] { typeof(ThemePackageMetadataEvaluation), typeof(ThemeIntegrityVerificationPolicyV1), typeof(ThemeTrustSnapshotV1), typeof(CancellationToken) }, method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        string[] expectedProperties = ["CanContinue", "Diagnostics", "KeyId", "PublisherId", "SignatureStatus"];
        Assert.Equal(expectedProperties, outcome.GetProperties().Select(property => property.Name).Order(StringComparer.Ordinal).ToArray());
        Assert.DoesNotContain(evaluator.GetFields(BindingFlags.Static | BindingFlags.NonPublic), field => !field.IsLiteral);
        PhaseThreeScopeBoundaryTests.AssertExactVerifierImplementations(evaluator.Assembly.GetTypes());
        // Actual production success accompanies the structural dependency guard.
        AssertSuccess(Evaluate(Golden()), Status.Valid, "publisher.example", "key-2026-01");
    }

    private static Scenario Golden()
    {
        ThemeManifest manifest = new(ContractVersions.Schema, ContractVersions.ThemeApi,
            new("com.example.theme", "com.example.theme.package", "Example Theme", "Example Publisher", "publisher.example", "1.2.3", "stable", "Example", null, null, "private", ["deep"]),
            new(">=1.0.0 <2.0.0", ">=1.0.0 <2.0.0", ">=1.1.0 <2.0.0", ["windows"], true, 1m, 3m),
            [new("deep", "deep", "Deep", true, "tokens/deep.json", [], null)], ["presentation.tokens"], new([]),
            new(true, ["none", "reduced_decoration", "minimal_decoration", "safe_presentation_only"], ["safety_core", "critical_alerts", "keyboard", "screen_reader", "contrast", "risk_permission_semantics", "confirmation_semantics", "accessibility"], "safe_presentation_only"),
            new(true, true, true, true, true, true, true, true, true, true, true, true), new("assets/index.json", ["tier0"], 0, "sha256"), null,
            new("motion/profiles.json", "motion/reduced.json", true, true, true), null, new("integrity.json", "signature.sig", "stable_or_store", "sha256"), null);
        ThemeSignatureEnvelopeV1 envelope = new("1.0.0", ThemeSignatureAlgorithm.EcdsaP256Sha256, "publisher.example", "key-2026-01", ThemeSignatureEncoding.IeeeP1363FixedFieldConcatenation, GoldenSignature, ThemeSignedPayloadType.TccThemePackageSignatureV1);
        ThemeTrustedSignerV1 signer = new("publisher.example", "key-2026-01", ThemeSignatureAlgorithm.EcdsaP256Sha256, ThemePublicKeyEncoding.SubjectPublicKeyInfo, GoldenKey, ThemeTrustState.Trusted);
        return new(new(true, [], new('a', 64), new('b', 64), new('c', 64), manifest, new("2.0.0", ThemeIntegrityInventoryMode.ExhaustiveAllowedPayload, "com.example.theme", "1.2.3", "sha256", [], new('a', 64)), envelope),
            new("policy", "1", "trust", "1", ThemeDistributionChannel.Stable, ThemeSignatureRequirement.Required, false), new("trust", "1", [signer]));
    }

    private static Scenario Identity(Scenario s, string publisher, string key) => s with
    {
        Metadata = s.Metadata with { SignatureEnvelope = s.Metadata.SignatureEnvelope! with { PublisherId = publisher, KeyId = key }, ThemeManifest = s.Metadata.ThemeManifest! with { Package = s.Metadata.ThemeManifest.Package with { PublisherId = publisher } } },
        Trust = s.Trust with { TrustedSigners = [s.Trust.TrustedSigners[0] with { PublisherId = publisher, KeyId = key }] },
    };
    private static Scenario WithSignature(Scenario s, string signature) => s with { Metadata = s.Metadata with { SignatureEnvelope = s.Metadata.SignatureEnvelope! with { Signature = signature } } };
    private static Scenario WithKey(Scenario s, string key) => s with { Trust = s.Trust with { TrustedSigners = [s.Trust.TrustedSigners[0] with { PublicKey = key }] } };
    private static Scenario State(string state)
    {
        Scenario s = Golden(); ThemeTrustedSignerV1 signer = s.Trust.TrustedSigners[0];
        s = state switch
        {
            "absent" => s with { Metadata = s.Metadata with { SignatureEnvelope = null } },
            "beta unsigned" => s with { Metadata = s.Metadata with { SignatureEnvelope = null }, Policy = s.Policy with { DistributionChannel = ThemeDistributionChannel.Beta, SignatureRequirement = ThemeSignatureRequirement.Optional } },
            "developer unsigned" => s with { Metadata = s.Metadata with { SignatureEnvelope = null }, Policy = s.Policy with { DistributionChannel = ThemeDistributionChannel.Developer, SignatureRequirement = ThemeSignatureRequirement.Optional, DeveloperExceptionAuthorized = true } },
            "unknown" => s with { Trust = s.Trust with { TrustedSigners = [signer with { KeyId = "other" }] } },
            "revoked" => s with { Trust = s.Trust with { TrustedSigners = [signer with { TrustState = ThemeTrustState.Revoked }] } },
            "invalid" => WithSignature(s, Convert.ToBase64String(new byte[64])),
            "malformed key" => WithKey(s, "bad"),
            "identity" => s with { Trust = s.Trust with { TrustedSigners = [signer, signer with { KeyId = "bad\0id" }] } },
            "duplicate" => s with { Trust = s.Trust with { TrustedSigners = [signer, signer] } },
            _ => s,
        };
        if (state == "curve") { byte[] bytes = Convert.FromBase64String(GoldenKey); bytes[22] = 8; s = WithKey(s, Convert.ToBase64String(bytes)); }
        // Unrelated entries make forward/reverse/rotation tests exercise real different orders.
        return s with { Trust = s.Trust with { TrustedSigners = s.Trust.TrustedSigners.Add(signer with { PublisherId = "unrelated", KeyId = "unrelated", PublicKey = "ignored" }) } };
    }
    private static ThemePackageSignatureEvaluation Evaluate(Scenario s, CancellationToken token = default) => ThemePackageSignatureEvaluator.Evaluate(s.Metadata, s.Policy, s.Trust, token);
    private static string Serialize(ThemePackageSignatureEvaluation result) => JsonSerializer.Serialize(result);
    private static void AssertSuccess(ThemePackageSignatureEvaluation result, Status status, string? publisher, string? key)
    {
        Assert.True(result.CanContinue); Assert.Empty(result.Diagnostics); Assert.Equal(status, result.SignatureStatus);
        Assert.Equal(publisher, result.PublisherId); Assert.Equal(key, result.KeyId);
    }
    private static void AssertFailure(ThemePackageSignatureEvaluation result, string code, Status? status = null)
    {
        Assert.False(result.CanContinue); Assert.Null(result.PublisherId); Assert.Null(result.KeyId);
        ThemeIntegrityDiagnosticV1 diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal(code, diagnostic.Code); Assert.Null(diagnostic.CanonicalPath); Assert.Equal(ThemeDiagnosticsSeverity.Error, diagnostic.Severity);
        (Status expected, string message) = code switch
        {
            "P4I012" => (Status.Missing, "A signature is required by the active theme integrity policy."),
            "P4I013" => (Status.Invalid, "The signature or signer public-key material is invalid."),
            "P4I014" => (Status.UnknownSigner, "No trusted signer entry matches the envelope publisher/key identity."),
            "P4I015" => (Status.RevokedSigner, "The matching signer entry is revoked."),
            "P4I016" => (Status.UnsupportedAlgorithm, "The signer public key does not match the approved ECDSA P-256 SPKI profile."),
            "P4I017" => (Status.NotEvaluated, "The signature requirement is inconsistent with the selected theme release channel."),
            "P4I018" => (Status.NotEvaluated, "The Developer unsigned-signature exception is not authorized for this policy state."),
            "P4I022" => (Status.NotEvaluated, "The trust snapshot contains duplicate publisher/key signer identities."),
            "P4I023" => (Status.NotEvaluated, "The supplied trust snapshot does not match the expected trust policy identity or version."),
            "P4I029" => (Status.NotEvaluated, "A signing identity field is invalid or inconsistent."),
            _ => throw new InvalidOperationException("Unknown test diagnostic"),
        };
        Assert.Equal(status ?? expected, result.SignatureStatus); Assert.Equal(message, diagnostic.Message);
    }
    private sealed record Scenario(ThemePackageMetadataEvaluation Metadata, ThemeIntegrityVerificationPolicyV1 Policy, ThemeTrustSnapshotV1 Trust);
    private sealed class FaultingDiagnostics(Exception fault) : IReadOnlyList<ThemeIntegrityDiagnosticV1>
    {
        public int Count => throw fault;
        public ThemeIntegrityDiagnosticV1 this[int index] => throw fault;
        public IEnumerator<ThemeIntegrityDiagnosticV1> GetEnumerator() => throw fault;
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
