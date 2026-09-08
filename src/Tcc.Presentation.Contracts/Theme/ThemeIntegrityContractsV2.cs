using System.Collections.Immutable;
using System.Text;
using System.Text.Json.Serialization;

namespace Tcc.Presentation.Contracts.Theme;

/// <summary>
/// Stable identifiers for the additive Phase 4 integrity contract amendment.
/// </summary>
public static class ThemeIntegrityContractSemantics
{
    public const string CanonicalPathVersion = "TCC Package Canonical Path v1";
    public const string PackageTreeHashVersion = "Canonical Package Tree Hash v1";
    public const string InventoryMode = "exhaustive_allowed_payload";
    public const string HashAlgorithm = "sha256";
    public const string SignaturePayloadDomain = "TCC-THEME-PACKAGE-SIGNATURE-V1";
    public const string SignatureAlgorithm = "ecdsa_p256_sha256";
    public const string SignatureEncoding = "ieee_p1363_fixed_field_concatenation";
    public const string PublicKeyEncoding = "subject_public_key_info";
    public const string SignedPayloadType = "tcc_theme_package_signature_v1";
}

/// <summary>
/// Stable Phase 4 integrity diagnostic codes. Ordering is Code, CanonicalPath, then Message using ordinal comparison.
/// </summary>
public static class ThemeIntegrityDiagnosticCodes
{
    public const string UnsupportedSchemaVersion = "P4I001";
    public const string InvalidCanonicalPath = "P4I002";
    public const string DuplicateNormalizedPath = "P4I003";
    public const string CaseInsensitivePathCollision = "P4I004";
    public const string MissingRequiredFile = "P4I005";
    public const string UndeclaredFile = "P4I006";
    public const string FileLengthMismatch = "P4I007";
    public const string FileHashMismatch = "P4I008";
    public const string PackageHashMismatch = "P4I009";
    public const string ThemeManifestHashMismatch = "P4I010";
    public const string IntegrityManifestHashUnavailable = "P4I011";
    public const string MissingSignature = "P4I012";
    public const string InvalidSignature = "P4I013";
    public const string UnknownSigner = "P4I014";
    public const string RevokedSigner = "P4I015";
    public const string UnsupportedSignatureAlgorithm = "P4I016";
    public const string ChannelPolicyViolation = "P4I017";
    public const string UnauthorizedDeveloperException = "P4I018";
    public const string UnsupportedPackageEntry = "P4I019";
    public const string MissingSecurityField = "P4I020";
    public const string PackageReadFailure = "P4I021";
    public const string DuplicateTrustedSignerIdentity = "P4I022";
    public const string TrustPolicyMismatch = "P4I023";
    public const string ThemeIdentityMismatch = "P4I024";
    public const string ThemeVersionMismatch = "P4I025";
    public const string UnsupportedSignatureEncoding = "P4I026";
    public const string InvalidMetadataDocument = "P4I027";
    public const string MetadataDtoMismatch = "P4I028";
    public const string InvalidSecurityValue = "P4I029";
}

/// <summary>
/// Authoritative one-to-one registry of active Phase 4 integrity rules and diagnostic codes.
/// </summary>
public static class ThemeIntegrityDiagnosticVocabulary
{
    public static ImmutableArray<ThemeIntegrityDiagnosticDefinition> ActiveDefinitions { get; } =
    [
        new("schema.unsupported_version", ThemeIntegrityDiagnosticCodes.UnsupportedSchemaVersion),
        new("path.invalid_canonical_form", ThemeIntegrityDiagnosticCodes.InvalidCanonicalPath),
        new("path.duplicate_normalized", ThemeIntegrityDiagnosticCodes.DuplicateNormalizedPath),
        new("path.case_insensitive_collision", ThemeIntegrityDiagnosticCodes.CaseInsensitivePathCollision),
        new("inventory.missing_required_file", ThemeIntegrityDiagnosticCodes.MissingRequiredFile),
        new("inventory.undeclared_file", ThemeIntegrityDiagnosticCodes.UndeclaredFile),
        new("file.length_mismatch", ThemeIntegrityDiagnosticCodes.FileLengthMismatch),
        new("file.hash_mismatch", ThemeIntegrityDiagnosticCodes.FileHashMismatch),
        new("package.hash_mismatch", ThemeIntegrityDiagnosticCodes.PackageHashMismatch),
        new("theme_manifest.hash_mismatch", ThemeIntegrityDiagnosticCodes.ThemeManifestHashMismatch),
        new("integrity_manifest.hash_unavailable", ThemeIntegrityDiagnosticCodes.IntegrityManifestHashUnavailable),
        new("signature.missing", ThemeIntegrityDiagnosticCodes.MissingSignature),
        new("signature.invalid", ThemeIntegrityDiagnosticCodes.InvalidSignature),
        new("signer.unknown", ThemeIntegrityDiagnosticCodes.UnknownSigner),
        new("signer.revoked", ThemeIntegrityDiagnosticCodes.RevokedSigner),
        new("signature.algorithm_unsupported", ThemeIntegrityDiagnosticCodes.UnsupportedSignatureAlgorithm),
        new("channel.policy_violation", ThemeIntegrityDiagnosticCodes.ChannelPolicyViolation),
        new("channel.developer_exception_unauthorized", ThemeIntegrityDiagnosticCodes.UnauthorizedDeveloperException),
        new("package.entry_kind_unsupported", ThemeIntegrityDiagnosticCodes.UnsupportedPackageEntry),
        new("security.required_field_missing", ThemeIntegrityDiagnosticCodes.MissingSecurityField),
        new("package.content_read_failure", ThemeIntegrityDiagnosticCodes.PackageReadFailure),
        new("trust.duplicate_signer_identity", ThemeIntegrityDiagnosticCodes.DuplicateTrustedSignerIdentity),
        new("trust.policy_identity_mismatch", ThemeIntegrityDiagnosticCodes.TrustPolicyMismatch),
        new("identity.theme_id_mismatch", ThemeIntegrityDiagnosticCodes.ThemeIdentityMismatch),
        new("identity.version_mismatch", ThemeIntegrityDiagnosticCodes.ThemeVersionMismatch),
        new("signature.encoding_unsupported", ThemeIntegrityDiagnosticCodes.UnsupportedSignatureEncoding),
        new("metadata.document_nonconforming", ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument),
        new("metadata.dto_binding_mismatch", ThemeIntegrityDiagnosticCodes.MetadataDtoMismatch),
        new("security.value_invalid", ThemeIntegrityDiagnosticCodes.InvalidSecurityValue),
    ];

    public static bool IsKnownCode(string? code) =>
        code is not null && ActiveDefinitions.Any(definition => string.Equals(definition.Code, code, StringComparison.Ordinal));
}

public sealed record ThemeIntegrityDiagnosticDefinition(string SemanticRule, string Code);

/// <summary>
/// A validated canonical logical package path. It never represents a filesystem location.
/// </summary>
public sealed record ThemeCanonicalPath
{
    private static readonly string[] ReservedDeviceNames =
    [
        "CON", "PRN", "AUX", "NUL", "CLOCK$", "CONIN$", "CONOUT$",
    ];

    private ThemeCanonicalPath(string value) => Value = value;

    public string Value { get; }

    public static bool TryCreate(string? candidate, out ThemeCanonicalPath? path)
    {
        path = null;
        if (string.IsNullOrEmpty(candidate)
            || candidate.StartsWith('/')
            || candidate.StartsWith('\\')
            || candidate.Contains('\\', StringComparison.Ordinal)
            || candidate.Contains(':', StringComparison.Ordinal)
            || candidate.Any(character => character == '\0' || char.IsControl(character)))
        {
            return false;
        }

        string normalized;
        try
        {
            normalized = candidate.Normalize(NormalizationForm.FormC);
        }
        catch (ArgumentException)
        {
            return false;
        }
        string[] segments = normalized.Split('/');
        if (segments.Any(segment => segment.Length == 0
                                    || segment is "." or ".."
                                    || segment.EndsWith('.')
                                    || segment.EndsWith(' ')
                                    || IsWindowsDeviceName(segment)))
        {
            return false;
        }

        path = new ThemeCanonicalPath(normalized);
        return true;
    }

    public static ThemeCanonicalPath Parse(string candidate) =>
        TryCreate(candidate, out ThemeCanonicalPath? path)
            ? path!
            : throw new ArgumentException("Value must satisfy TCC Package Canonical Path v1.", nameof(candidate));

    public override string ToString() => Value;

    private static bool IsWindowsDeviceName(string segment)
    {
        string basename = segment.Split('.')[0];
        if (ReservedDeviceNames.Contains(basename, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        return basename.Length == 4
            && (basename.StartsWith("COM", StringComparison.OrdinalIgnoreCase)
                || basename.StartsWith("LPT", StringComparison.OrdinalIgnoreCase))
            && basename[3] is >= '1' and <= '9';
    }
}

public enum ThemeIntegrityInventoryMode
{
    ExhaustiveAllowedPayload,
}

public enum ThemeIntegrityEntryKind
{
    Payload,
    ThemeManifest,
}

public enum ThemePackageContentEntryKind
{
    File,
    Directory,
    SymbolicLink,
    ReparsePoint,
    Unsupported,
}

public enum ThemeSignatureAlgorithm
{
    EcdsaP256Sha256,
}

public enum ThemeSignatureEncoding
{
    IeeeP1363FixedFieldConcatenation,
}

public enum ThemePublicKeyEncoding
{
    SubjectPublicKeyInfo,
}

public enum ThemeSignedPayloadType
{
    TccThemePackageSignatureV1,
}

public enum ThemeTrustState
{
    Trusted,
    Revoked,
}

public enum ThemeDistributionChannel
{
    Stable,
    Beta,
    Developer,
    Store,
}

public enum ThemeSignatureRequirement
{
    Required,
    Optional,
}

public enum ThemeSignatureVerificationStatus
{
    NotEvaluated,
    NotRequired,
    DeveloperExceptionAccepted,
    Valid,
    Missing,
    Invalid,
    UnknownSigner,
    RevokedSigner,
    UnsupportedAlgorithm,
}

public enum ThemeIntegrityEvidenceStatus
{
    Verified,
    MissingAllowed,
    MissingRequired,
    HashMismatch,
    LengthMismatch,
    Undeclared,
    UnsupportedEntry,
    Unavailable,
}

public sealed record ThemeIntegrityManifestV2(
    [property: JsonPropertyName("schema_version")] string SchemaVersion,
    [property: JsonPropertyName("inventory_mode")] ThemeIntegrityInventoryMode InventoryMode,
    [property: JsonPropertyName("theme_id")] string ThemeId,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("hash_algorithm")] string HashAlgorithm,
    [property: JsonPropertyName("files")] IReadOnlyList<ThemeIntegrityFileV2> Files,
    [property: JsonPropertyName("package_hash")] string PackageHash);

public sealed record ThemeIntegrityFileV2(
    [property: JsonPropertyName("canonical_path")] string CanonicalPath,
    [property: JsonPropertyName("entry_kind")] ThemeIntegrityEntryKind EntryKind,
    [property: JsonPropertyName("sha256")] string Sha256,
    [property: JsonPropertyName("length_bytes")] long LengthBytes,
    [property: JsonPropertyName("required")] bool Required);

public sealed record ThemeSignatureEnvelopeV1(
    [property: JsonPropertyName("envelope_version")] string EnvelopeVersion,
    [property: JsonPropertyName("algorithm")] ThemeSignatureAlgorithm Algorithm,
    [property: JsonPropertyName("publisher_id")] string PublisherId,
    [property: JsonPropertyName("key_id")] string KeyId,
    [property: JsonPropertyName("signature_encoding")] ThemeSignatureEncoding SignatureEncoding,
    [property: JsonPropertyName("signature")] string Signature,
    [property: JsonPropertyName("signed_payload_type")] ThemeSignedPayloadType SignedPayloadType);

public sealed record ThemeIntegrityVerificationPolicyV1(
    [property: JsonPropertyName("policy_id")] string PolicyId,
    [property: JsonPropertyName("policy_version")] string PolicyVersion,
    [property: JsonPropertyName("expected_trust_policy_id")] string ExpectedTrustPolicyId,
    [property: JsonPropertyName("expected_trust_policy_version")] string ExpectedTrustPolicyVersion,
    [property: JsonPropertyName("distribution_channel")] ThemeDistributionChannel DistributionChannel,
    [property: JsonPropertyName("signature_requirement")] ThemeSignatureRequirement SignatureRequirement,
    [property: JsonPropertyName("developer_exception_authorized")] bool DeveloperExceptionAuthorized);

public sealed record ThemeTrustSnapshotV1(
    [property: JsonPropertyName("policy_id")] string PolicyId,
    [property: JsonPropertyName("policy_version")] string PolicyVersion,
    [property: JsonPropertyName("trusted_signers")] ImmutableArray<ThemeTrustedSignerV1> TrustedSigners);

public sealed record ThemeTrustedSignerV1(
    [property: JsonPropertyName("publisher_id")] string PublisherId,
    [property: JsonPropertyName("key_id")] string KeyId,
    [property: JsonPropertyName("algorithm")] ThemeSignatureAlgorithm Algorithm,
    [property: JsonPropertyName("public_key_encoding")] ThemePublicKeyEncoding PublicKeyEncoding,
    [property: JsonPropertyName("public_key")] string PublicKey,
    [property: JsonPropertyName("trust_state")] ThemeTrustState TrustState);

public sealed record ThemePackageContentEntryV1(
    string LogicalPath,
    long LengthBytes,
    ThemePackageContentEntryKind EntryKind,
    string? ContentType);

public sealed record ThemeIntegrityVerificationRequestV2(
    ThemeId ThemeId,
    ThemeVersion Version,
    ThemePackageRef PackageRef,
    ThemeCanonicalPath ThemeManifestPath,
    ThemeCanonicalPath IntegrityManifestPath,
    ThemeCanonicalPath? SignatureEnvelopePath,
    ThemeIntegrityManifestV2 IntegrityManifest,
    ThemeSignatureEnvelopeV1? SignatureEnvelope,
    ThemeIntegrityVerificationPolicyV1 Policy,
    ThemeTrustSnapshotV1 TrustSnapshot);

public sealed record ThemeIntegrityFileEvidenceV1(
    string CanonicalPath,
    ThemeIntegrityEntryKind EntryKind,
    bool Required,
    bool IsPresent,
    long DeclaredLengthBytes,
    long? ActualLengthBytes,
    string DeclaredSha256,
    string? ActualSha256,
    ThemeIntegrityEvidenceStatus Status);

public sealed record ThemeIntegrityDiagnosticV1
{
    public ThemeIntegrityDiagnosticV1(
        string code,
        ThemeDiagnosticsSeverity severity,
        string? canonicalPath,
        string message)
    {
        if (!ThemeIntegrityDiagnosticVocabulary.IsKnownCode(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code), code, "Unknown Phase 4 integrity diagnostic code.");
        }

        if (!Enum.IsDefined(severity))
        {
            throw new ArgumentOutOfRangeException(nameof(severity), severity, "Unknown diagnostic severity.");
        }

        Code = code;
        Severity = severity;
        CanonicalPath = canonicalPath;
        Message = !string.IsNullOrWhiteSpace(message)
            ? message
            : throw new ArgumentException("Diagnostic message is required.", nameof(message));
    }

    public string Code { get; }
    public ThemeDiagnosticsSeverity Severity { get; }
    public string? CanonicalPath { get; }
    public string Message { get; }
}

public sealed record ThemeIntegrityVerificationResultV2(
    bool IsVerified,
    string? PackageHash,
    string? ThemeManifestHash,
    string? IntegrityManifestHash,
    IReadOnlyList<ThemeIntegrityFileEvidenceV1> FileEvidence,
    IReadOnlyList<ThemeIntegrityFileEvidenceV1> AssetEvidence,
    ThemeSignatureVerificationStatus SignatureStatus,
    string? PublisherId,
    string? KeyId,
    IReadOnlyList<ThemeIntegrityDiagnosticV1> Diagnostics);
