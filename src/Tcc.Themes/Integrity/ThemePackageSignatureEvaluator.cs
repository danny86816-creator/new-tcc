using System.Buffers;
using System.Formats.Asn1;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Tcc.Presentation.Contracts.Theme;
using Codes = Tcc.Presentation.Contracts.Theme.ThemeIntegrityDiagnosticCodes;
using Status = Tcc.Presentation.Contracts.Theme.ThemeSignatureVerificationStatus;

namespace Tcc.Themes.Integrity;

/// <summary>Internal signature/trust/channel stage consuming only validated Phase 4C metadata.</summary>
internal static class ThemePackageSignatureEvaluator
{
    private const string EcPublicKeyOid = "1.2.840.10045.2.1";
    private const string P256Oid = "1.2.840.10045.3.1.7";

    internal static ThemePackageSignatureEvaluation Evaluate(
        ThemePackageMetadataEvaluation metadata,
        ThemeIntegrityVerificationPolicyV1 policy,
        ThemeTrustSnapshotV1 trustSnapshot,
        CancellationToken cancellationToken = default)
    {
        // Stage 0: an internal stage cannot repair unsuccessful or corrupted upstream state.
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(trustSnapshot);
        RequireEntry(metadata, policy, trustSnapshot);

        // Stages 1-4: policy failures precede absence and all signer/cryptographic work.
        if (!StringComparer.Ordinal.Equals(policy.ExpectedTrustPolicyId, trustSnapshot.PolicyId)
            || !StringComparer.Ordinal.Equals(policy.ExpectedTrustPolicyVersion, trustSnapshot.PolicyVersion))
        {
            return Failure(Codes.TrustPolicyMismatch);
        }

        if (policy.DeveloperExceptionAuthorized
            && (policy.DistributionChannel != ThemeDistributionChannel.Developer
                || policy.SignatureRequirement == ThemeSignatureRequirement.Required))
        {
            return Failure(Codes.UnauthorizedDeveloperException);
        }

        if (policy.DistributionChannel is ThemeDistributionChannel.Stable or ThemeDistributionChannel.Store
            && policy.SignatureRequirement != ThemeSignatureRequirement.Required)
        {
            return Failure(Codes.ChannelPolicyViolation);
        }

        ThemeSignatureEnvelopeV1? envelope = metadata.SignatureEnvelope;
        if (envelope is null)
        {
            if (policy.SignatureRequirement == ThemeSignatureRequirement.Required)
            {
                return Failure(Codes.MissingSignature);
            }

            if (policy.DistributionChannel == ThemeDistributionChannel.Developer)
            {
                return policy.DeveloperExceptionAuthorized
                    ? Success(Status.DeveloperExceptionAccepted, null, null, cancellationToken)
                    : Failure(Codes.UnauthorizedDeveloperException, missing: true);
            }

            return Success(Status.NotRequired, null, null, cancellationToken);
        }

        // Stages 5-7: exact identities and framing, before consuming the trust registry.
        if (!IsSigningId(envelope.PublisherId) || !IsSigningId(envelope.KeyId))
        {
            return Failure(Codes.InvalidSecurityValue);
        }

        if (!StringComparer.Ordinal.Equals(metadata.ThemeManifest!.Package.PublisherId, envelope.PublisherId))
        {
            return Failure(Codes.InvalidSecurityValue);
        }

        byte[]? signature = DecodeCanonicalBase64(envelope.Signature, 88, 64);
        if (signature is null)
        {
            return Failure(Codes.InvalidSignature);
        }

        // Stages 8-10 share one complete O(n) scan. Decide only after the scan so
        // a late invalid identity beats an earlier duplicate or matching signer.
        cancellationToken.ThrowIfCancellationRequested();
        Dictionary<string, HashSet<string>> identities = new(StringComparer.Ordinal);
        bool invalidIdentity = false;
        bool duplicate = false;
        ThemeTrustedSignerV1? target = null;
        foreach (ThemeTrustedSignerV1 signer in trustSnapshot.TrustedSigners)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (signer is null
                || !Enum.IsDefined(signer.Algorithm)
                || !Enum.IsDefined(signer.PublicKeyEncoding)
                || !Enum.IsDefined(signer.TrustState))
            {
                throw new InvalidOperationException("The preflight trust snapshot invariant is invalid.");
            }

            if (!IsSigningId(signer.PublisherId) || !IsSigningId(signer.KeyId))
            {
                invalidIdentity = true;
                continue;
            }

            if (!identities.TryGetValue(signer.PublisherId, out HashSet<string>? keys))
            {
                keys = new HashSet<string>(StringComparer.Ordinal);
                identities.Add(signer.PublisherId, keys);
            }

            duplicate |= !keys.Add(signer.KeyId);
            if (StringComparer.Ordinal.Equals(signer.PublisherId, envelope.PublisherId)
                && StringComparer.Ordinal.Equals(signer.KeyId, envelope.KeyId))
            {
                target = signer;
            }
        }

        if (invalidIdentity) return Failure(Codes.InvalidSecurityValue);
        if (duplicate) return Failure(Codes.DuplicateTrustedSignerIdentity);
        if (target is null) return Failure(Codes.UnknownSigner);

        // Stages 11-14: only the exact Trusted pair may reach key import.
        if (target.TrustState == ThemeTrustState.Revoked) return Failure(Codes.RevokedSigner);
        byte[]? publicKey = DecodeCanonicalBase64(target.PublicKey, 124, 91);
        if (publicKey is null) return Failure(Codes.InvalidSignature);

        cancellationToken.ThrowIfCancellationRequested();
        string? keyFailure = ValidateSpki(publicKey);
        if (keyFailure is not null) return Failure(keyFailure);

        cancellationToken.ThrowIfCancellationRequested();
        using ECDsa verifier = ECDsa.Create();
        int bytesRead;
        try
        {
            verifier.ImportSubjectPublicKeyInfo(publicKey, out bytesRead);
        }
        catch (CryptographicException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Failure(Codes.InvalidSignature);
        }

        if (bytesRead != publicKey.Length) return Failure(Codes.InvalidSignature);
        ECCurve curve = verifier.ExportParameters(false).Curve;
        if (!curve.IsNamed || !StringComparer.Ordinal.Equals(curve.Oid.Value, P256Oid)
            || verifier.KeySize != 256)
        {
            return Failure(Codes.UnsupportedSignatureAlgorithm);
        }

        // Stages 15-19: one SHA-256 VerifyData path; no low-S restriction or normalization.
        byte[] payload = BuildPayload(metadata, envelope);
        cancellationToken.ThrowIfCancellationRequested();
        bool valid;
        try
        {
            valid = verifier.VerifyData(payload, signature, HashAlgorithmName.SHA256,
                DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        }
        catch (CryptographicException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Failure(Codes.InvalidSignature);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return valid
            ? Success(Status.Valid, envelope.PublisherId, envelope.KeyId, cancellationToken)
            : Failure(Codes.InvalidSignature);
    }

    private static void RequireEntry(
        ThemePackageMetadataEvaluation metadata,
        ThemeIntegrityVerificationPolicyV1 policy,
        ThemeTrustSnapshotV1 snapshot)
    {
        if (!metadata.CanContinue || metadata.Diagnostics is null || metadata.Diagnostics.Count != 0
            || metadata.ThemeManifest?.Package is null || metadata.IntegrityManifest is null
            || !IsHash(metadata.PackageHash) || !IsHash(metadata.ThemeManifestHash)
            || !IsHash(metadata.IntegrityManifestHash)
            || string.IsNullOrEmpty(metadata.ThemeManifest.Package.ThemeId)
            || string.IsNullOrEmpty(metadata.ThemeManifest.Package.Version)
            || !StringComparer.Ordinal.Equals(metadata.ThemeManifest.Package.ThemeId, metadata.IntegrityManifest.ThemeId)
            || !StringComparer.Ordinal.Equals(metadata.ThemeManifest.Package.Version, metadata.IntegrityManifest.Version)
            || !StringComparer.Ordinal.Equals(metadata.PackageHash, metadata.IntegrityManifest.PackageHash)
            || !Enum.IsDefined(policy.DistributionChannel) || !Enum.IsDefined(policy.SignatureRequirement)
            || string.IsNullOrEmpty(policy.ExpectedTrustPolicyId) || string.IsNullOrEmpty(policy.ExpectedTrustPolicyVersion)
            || string.IsNullOrEmpty(snapshot.PolicyId) || string.IsNullOrEmpty(snapshot.PolicyVersion)
            || snapshot.TrustedSigners.IsDefault)
        {
            throw new InvalidOperationException("Validated metadata and preflight policy/trust inputs are required.");
        }

        if (metadata.SignatureEnvelope is { } envelope
            && (envelope.Algorithm != ThemeSignatureAlgorithm.EcdsaP256Sha256
                || envelope.SignatureEncoding != ThemeSignatureEncoding.IeeeP1363FixedFieldConcatenation
                || envelope.SignedPayloadType != ThemeSignedPayloadType.TccThemePackageSignatureV1))
        {
            throw new InvalidOperationException("The validated signature envelope invariant is invalid.");
        }
    }

    private static bool IsHash(string? value)
    {
        if (value is null || value.Length != 64) return false;
        foreach (char character in value)
        {
            if (character is not (>= '0' and <= '9') and not (>= 'a' and <= 'f')) return false;
        }

        return true;
    }

    private static bool IsSigningId(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > 256) return false;
        ReadOnlySpan<char> remaining = value.AsSpan();
        int byteCount = 0;
        while (!remaining.IsEmpty)
        {
            if (Rune.DecodeFromUtf16(remaining, out Rune scalar, out int consumed) != OperationStatus.Done
                || scalar.Value <= 0x1f || scalar.Value is >= 0x7f and <= 0x9f)
            {
                return false;
            }

            byteCount += scalar.Utf8SequenceLength;
            if (byteCount > 256) return false;
            remaining = remaining[consumed..];
        }

        return byteCount > 0;
    }

    private static byte[]? DecodeCanonicalBase64(string? value, int characters, int bytes)
    {
        if (value is null || value.Length != characters || value[^2] != '=' || value[^1] != '=') return null;
        for (int index = 0; index < value.Length - 2; index++)
        {
            char character = value[index];
            if (character is not (>= 'A' and <= 'Z') and not (>= 'a' and <= 'z')
                and not (>= '0' and <= '9') and not '+' and not '/') return null;
        }

        byte[] decoded;
        try
        {
            decoded = Convert.FromBase64String(value);
        }
        catch (FormatException)
        {
            return null;
        }

        return decoded.Length == bytes && StringComparer.Ordinal.Equals(Convert.ToBase64String(decoded), value)
            ? decoded : null;
    }

    private static string? ValidateSpki(byte[] encoded)
    {
        byte[] point;
        string algorithmOid;
        string? curveOid = null;
        int unusedBits;
        // Contain only candidate ASN.1 parsing failures, not other internal/provider faults.
        try
        {
            AsnReader reader = new(encoded, AsnEncodingRules.DER);
            AsnReader sequence = reader.ReadSequence();
            AsnReader algorithm = sequence.ReadSequence();
            algorithmOid = algorithm.ReadObjectIdentifier();
            if (algorithm.HasData)
            {
                if (algorithm.PeekTag().HasSameClassAndValue(Asn1Tag.ObjectIdentifier))
                    curveOid = algorithm.ReadObjectIdentifier();
                else
                    algorithm.ReadEncodedValue();
            }

            algorithm.ThrowIfNotEmpty();
            point = sequence.ReadBitString(out unusedBits);
            sequence.ThrowIfNotEmpty();
            reader.ThrowIfNotEmpty();
        }
        catch (AsnContentException)
        {
            return Codes.InvalidSignature;
        }

        if (unusedBits != 0) return Codes.InvalidSignature;
        if (!StringComparer.Ordinal.Equals(algorithmOid, EcPublicKeyOid)
            || !StringComparer.Ordinal.Equals(curveOid, P256Oid)) return Codes.UnsupportedSignatureAlgorithm;
        if (point.Length != 65) return Codes.InvalidSignature;
        if (point[0] != 0x04) return Codes.UnsupportedSignatureAlgorithm;
        return IsAffineP256Point(point) ? null : Codes.InvalidSignature;
    }

    private static bool IsAffineP256Point(ReadOnlySpan<byte> point)
    {
        // Decision016: public fixed-size coordinates only; no secret or scalar multiplication.
        BigInteger prime = new(Convert.FromHexString(
            "FFFFFFFF00000001000000000000000000000000FFFFFFFFFFFFFFFFFFFFFFFF"), isUnsigned: true, isBigEndian: true);
        BigInteger coefficient = new(Convert.FromHexString(
            "5AC635D8AA3A93E7B3EBBD55769886BC651D06B0CC53B0F63BCE3C3E27D2604B"), isUnsigned: true, isBigEndian: true);
        BigInteger x = new(point.Slice(1, 32), isUnsigned: true, isBigEndian: true);
        BigInteger y = new(point.Slice(33, 32), isUnsigned: true, isBigEndian: true);
        if (x >= prime || y >= prime) return false;
        BigInteger right = ((x * x * x - 3 * x + coefficient) % prime + prime) % prime;
        return y * y % prime == right;
    }

    private static byte[] BuildPayload(ThemePackageMetadataEvaluation metadata, ThemeSignatureEnvelopeV1 envelope) =>
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true).GetBytes(
            ThemeIntegrityContractSemantics.SignaturePayloadDomain + "\0"
            + metadata.ThemeManifest!.Package.ThemeId + "\0"
            + metadata.ThemeManifest.Package.Version + "\0"
            + metadata.PackageHash + "\0" + metadata.ThemeManifestHash + "\0"
            + metadata.IntegrityManifestHash + "\0" + envelope.PublisherId + "\0" + envelope.KeyId);

    private static ThemePackageSignatureEvaluation Success(
        Status status, string? publisherId, string? keyId, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return new(true, Array.Empty<ThemeIntegrityDiagnosticV1>(), status, publisherId, keyId);
    }

    private static ThemePackageSignatureEvaluation Failure(string code, bool missing = false)
    {
        (Status status, string message) = code switch
        {
            Codes.MissingSignature => (Status.Missing, "A signature is required by the active theme integrity policy."),
            Codes.InvalidSignature => (Status.Invalid, "The signature or signer public-key material is invalid."),
            Codes.UnknownSigner => (Status.UnknownSigner, "No trusted signer entry matches the envelope publisher/key identity."),
            Codes.RevokedSigner => (Status.RevokedSigner, "The matching signer entry is revoked."),
            Codes.UnsupportedSignatureAlgorithm => (Status.UnsupportedAlgorithm, "The signer public key does not match the approved ECDSA P-256 SPKI profile."),
            Codes.ChannelPolicyViolation => (Status.NotEvaluated, "The signature requirement is inconsistent with the selected theme release channel."),
            Codes.UnauthorizedDeveloperException => (missing ? Status.Missing : Status.NotEvaluated, "The Developer unsigned-signature exception is not authorized for this policy state."),
            Codes.DuplicateTrustedSignerIdentity => (Status.NotEvaluated, "The trust snapshot contains duplicate publisher/key signer identities."),
            Codes.TrustPolicyMismatch => (Status.NotEvaluated, "The supplied trust snapshot does not match the expected trust policy identity or version."),
            Codes.InvalidSecurityValue => (Status.NotEvaluated, "A signing identity field is invalid or inconsistent."),
            _ => throw new InvalidOperationException("Unexpected signature-stage diagnostic code."),
        };
        return new(false, new ThemeIntegrityDiagnosticV1[]
        {
            new(code, ThemeDiagnosticsSeverity.Error, null, message),
        }, status, null, null);
    }
}

/// <summary>Internal stage outcome only; does not authorize package installation or activation.</summary>
internal sealed record ThemePackageSignatureEvaluation(
    bool CanContinue,
    IReadOnlyList<ThemeIntegrityDiagnosticV1> Diagnostics,
    ThemeSignatureVerificationStatus SignatureStatus,
    string? PublisherId,
    string? KeyId);
