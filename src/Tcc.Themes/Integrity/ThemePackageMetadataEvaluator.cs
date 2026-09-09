using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Manifests;

namespace Tcc.Themes.Integrity;

/// <summary>
/// Validates raw package metadata and derives deterministic non-cryptographic inputs
/// for later integrity stages. This stage does not establish a final package verdict.
/// </summary>
internal static class ThemePackageMetadataEvaluator
{
    private const string EvidenceFailureMessage =
        "Theme Manifest must be present as verified package content before metadata validation.";
    private const string ThemeCoherenceFailureMessage =
        "Theme Manifest raw bytes do not match the content verified by the package inventory stage.";

    internal static async ValueTask<ThemePackageMetadataEvaluation> EvaluateAsync(
        ThemeIntegrityVerificationRequestV2 request,
        IThemePackageContentReader contentReader,
        ThemePackageInventoryEvaluation inventory,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!inventory.CanContinue)
        {
            return new ThemePackageMetadataEvaluation(
                false,
                inventory.Diagnostics,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        cancellationToken.ThrowIfCancellationRequested();
        string themePath = request.ThemeManifestPath.Value;
        ThemeIntegrityFileEvidenceV1? themeEvidence = null;
        int themeEvidenceMatches = 0;
        foreach (ThemeIntegrityFileEvidenceV1 evidence in inventory.FileEvidence)
        {
            if (ThemeCanonicalPath.TryCreate(evidence.CanonicalPath, out ThemeCanonicalPath? canonical)
                && string.Equals(canonical!.Value, themePath, StringComparison.Ordinal))
            {
                themeEvidenceMatches++;
                themeEvidence = evidence;
            }
        }

        if (themeEvidenceMatches != 1
            || themeEvidence is null
            || !themeEvidence.IsPresent
            || themeEvidence.Status != ThemeIntegrityEvidenceStatus.Verified
            || themeEvidence.ActualLengthBytes is null
            || !IsSha256(themeEvidence.ActualSha256))
        {
            return Failure(
                ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument,
                themePath,
                EvidenceFailureMessage);
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (!TryComputePackageHash(request, inventory.FileEvidence, out string? packageHash))
        {
            return Failure(
                ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument,
                themePath,
                "Verified package evidence is not structurally valid for canonical tree hashing.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        ReadOnlyMemory<byte> themeBytes;
        try
        {
            themeBytes = await contentReader
                .ReadContentAsync(request.PackageRef, themePath, cancellationToken)
                .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (Exception exception) when (IsApprovedMetadataReadFailure(exception))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Failure(
                ThemeIntegrityDiagnosticCodes.PackageReadFailure,
                themePath,
                "Theme Manifest bytes are unavailable after package inventory verification.");
        }

        string themeManifestHash = Hash(themeBytes.Span);
        if (themeBytes.Length != themeEvidence.ActualLengthBytes.Value
            || !string.Equals(themeManifestHash, themeEvidence.ActualSha256, StringComparison.Ordinal))
        {
            return Failure(
                ThemeIntegrityDiagnosticCodes.ThemeManifestHashMismatch,
                themePath,
                ThemeCoherenceFailureMessage);
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (!ThemeMetadataSchemaValidator.Validate(
                themeBytes,
                ThemeMetadataSchemaValidator.ThemeManifestSchemaResource))
        {
            return InvalidDocument(themePath, "Raw Theme Manifest violates its authoritative schema.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        ThemeManifest themeManifest;
        try
        {
            themeManifest = JsonSerializer.Deserialize<ThemeManifest>(
                    themeBytes.Span,
                    ThemeContractJson.CreateSerializerOptions())
                ?? throw new JsonException("Theme Manifest materialized to null.");
        }
        catch (Exception exception) when (IsApprovedMaterializationFailure(exception))
        {
            return InvalidDocument(themePath, "Raw Theme Manifest could not be materialized.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlySet<string> knownCapabilities = ThemeMetadataSchemaValidator.ReadRootArrayItemStringEnum(
            ThemeMetadataSchemaValidator.ThemeManifestSchemaResource,
            "capabilities");
        ThemeManifestValidationResult themeSemantics = new ThemeManifestValidator().Validate(
            themeManifest,
            new ThemeManifestValidationContext(
                new HashSet<string>(StringComparer.Ordinal) { ContractVersions.Schema },
                knownCapabilities,
                request.Policy.DistributionChannel is ThemeDistributionChannel.Stable or ThemeDistributionChannel.Store));
        if (!themeSemantics.IsValid)
        {
            return InvalidDocument(
                themePath,
                "Raw Theme Manifest violates the sealed Theme Manifest semantic contract.");
        }

        if (!string.Equals(themeManifest.Package.ThemeId, request.ThemeId.Value, StringComparison.Ordinal))
        {
            return Failure(
                ThemeIntegrityDiagnosticCodes.ThemeIdentityMismatch,
                null,
                "Theme Manifest identity does not match the verification request.");
        }

        if (!string.Equals(themeManifest.Package.Version, request.Version.Value, StringComparison.Ordinal))
        {
            return Failure(
                ThemeIntegrityDiagnosticCodes.ThemeVersionMismatch,
                null,
                "Theme Manifest version does not match the verification request.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        string integrityPath = request.IntegrityManifestPath.Value;
        ReadOnlyMemory<byte> integrityBytes;
        try
        {
            integrityBytes = await contentReader
                .ReadContentAsync(request.PackageRef, integrityPath, cancellationToken)
                .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (FileNotFoundException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Failure(
                ThemeIntegrityDiagnosticCodes.IntegrityManifestHashUnavailable,
                integrityPath,
                "Integrity Manifest bytes are unavailable.");
        }
        catch (Exception exception) when (IsApprovedMetadataReadFailure(exception))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Failure(
                ThemeIntegrityDiagnosticCodes.PackageReadFailure,
                integrityPath,
                "Integrity Manifest bytes could not be read.");
        }

        string integrityManifestHash = Hash(integrityBytes.Span);
        cancellationToken.ThrowIfCancellationRequested();
        if (!ThemeMetadataSchemaValidator.Validate(
                integrityBytes,
                ThemeMetadataSchemaValidator.IntegrityManifestSchemaResource))
        {
            return InvalidDocument(integrityPath, "Raw Integrity Manifest violates its authoritative schema.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        ThemeIntegrityManifestV2 integrityManifest;
        try
        {
            integrityManifest = JsonSerializer.Deserialize<ThemeIntegrityManifestV2>(
                    integrityBytes.Span,
                    ThemeContractJson.CreateSerializerOptions())
                ?? throw new JsonException("Integrity Manifest materialized to null.");
        }
        catch (Exception exception) when (IsApprovedMaterializationFailure(exception))
        {
            return InvalidDocument(integrityPath, "Raw Integrity Manifest could not be materialized.");
        }

        if (!IntegrityManifestsEqual(integrityManifest, request.IntegrityManifest))
        {
            return Failure(
                ThemeIntegrityDiagnosticCodes.MetadataDtoMismatch,
                integrityPath,
                "Raw Integrity Manifest does not exactly match the caller-provided contract.");
        }

        if (!string.Equals(integrityManifest.ThemeId, request.ThemeId.Value, StringComparison.Ordinal))
        {
            return Failure(
                ThemeIntegrityDiagnosticCodes.ThemeIdentityMismatch,
                null,
                "Integrity Manifest identity does not match the verification request.");
        }

        if (!string.Equals(integrityManifest.Version, request.Version.Value, StringComparison.Ordinal))
        {
            return Failure(
                ThemeIntegrityDiagnosticCodes.ThemeVersionMismatch,
                null,
                "Integrity Manifest version does not match the verification request.");
        }

        string? expectedThemeHash = null;
        int expectedThemeMatches = 0;
        foreach (ThemeIntegrityFileV2 file in integrityManifest.Files)
        {
            if (string.Equals(file.CanonicalPath, themePath, StringComparison.Ordinal)
                && file.EntryKind == ThemeIntegrityEntryKind.ThemeManifest)
            {
                expectedThemeMatches++;
                expectedThemeHash = file.Sha256;
            }
        }

        if (expectedThemeMatches != 1 || !IsSha256(expectedThemeHash))
        {
            return InvalidDocument(
                themePath,
                "Integrity Manifest must contain exactly one Theme Manifest hash binding.");
        }

        if (!string.Equals(themeManifestHash, expectedThemeHash, StringComparison.Ordinal))
        {
            return Failure(
                ThemeIntegrityDiagnosticCodes.ThemeManifestHashMismatch,
                themePath,
                "Theme Manifest hash does not match the raw Integrity Manifest binding.");
        }

        if (!string.Equals(packageHash, integrityManifest.PackageHash, StringComparison.Ordinal))
        {
            return Failure(
                ThemeIntegrityDiagnosticCodes.PackageHashMismatch,
                null,
                "Canonical Package Tree Hash does not match the raw Integrity Manifest binding.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        ThemeSignatureEnvelopeV1? signatureEnvelope = null;
        if (request.SignatureEnvelopePath is { } envelopePathValue)
        {
            string envelopePath = envelopePathValue.Value;
            ReadOnlyMemory<byte> envelopeBytes;
            try
            {
                envelopeBytes = await contentReader
                    .ReadContentAsync(request.PackageRef, envelopePath, cancellationToken)
                    .ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (FileNotFoundException)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Failure(
                    ThemeIntegrityDiagnosticCodes.MissingSignature,
                    envelopePath,
                    "Referenced Signature Envelope bytes are unavailable.");
            }
            catch (Exception exception) when (IsApprovedMetadataReadFailure(exception))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Failure(
                    ThemeIntegrityDiagnosticCodes.PackageReadFailure,
                    envelopePath,
                    "Signature Envelope bytes could not be read.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (!ThemeMetadataSchemaValidator.Validate(
                    envelopeBytes,
                    ThemeMetadataSchemaValidator.SignatureEnvelopeSchemaResource))
            {
                return InvalidDocument(envelopePath, "Raw Signature Envelope violates its authoritative schema.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                signatureEnvelope = JsonSerializer.Deserialize<ThemeSignatureEnvelopeV1>(
                        envelopeBytes.Span,
                        ThemeContractJson.CreateSerializerOptions())
                    ?? throw new JsonException("Signature Envelope materialized to null.");
            }
            catch (Exception exception) when (IsApprovedMaterializationFailure(exception))
            {
                return InvalidDocument(envelopePath, "Raw Signature Envelope could not be materialized.");
            }

            if (!SignatureEnvelopesEqual(signatureEnvelope, request.SignatureEnvelope))
            {
                return Failure(
                    ThemeIntegrityDiagnosticCodes.MetadataDtoMismatch,
                    null,
                    "Raw Signature Envelope does not exactly match the caller-provided contract.");
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        return new ThemePackageMetadataEvaluation(
            true,
            Array.Empty<ThemeIntegrityDiagnosticV1>(),
            packageHash,
            themeManifestHash,
            integrityManifestHash,
            themeManifest,
            integrityManifest,
            signatureEnvelope);
    }

    private static bool TryComputePackageHash(
        ThemeIntegrityVerificationRequestV2 request,
        IReadOnlyList<ThemeIntegrityFileEvidenceV1> evidence,
        out string? hash)
    {
        hash = null;
        List<ThemeIntegrityFileEvidenceV1> included = [];
        foreach (ThemeIntegrityFileEvidenceV1 item in evidence)
        {
            if (!item.IsPresent || item.Status != ThemeIntegrityEvidenceStatus.Verified)
            {
                continue;
            }

            if (string.Equals(item.CanonicalPath, request.IntegrityManifestPath.Value, StringComparison.Ordinal)
                || string.Equals(item.CanonicalPath, request.SignatureEnvelopePath?.Value, StringComparison.Ordinal))
            {
                continue;
            }

            if (!ThemeCanonicalPath.TryCreate(item.CanonicalPath, out ThemeCanonicalPath? canonical)
                || item.ActualLengthBytes is null
                || item.ActualLengthBytes.Value < 0
                || !IsSha256(item.ActualSha256))
            {
                return false;
            }

            included.Add(item with { CanonicalPath = canonical!.Value });
        }

        for (int index = 1; index < included.Count; index++)
        {
            ThemeIntegrityFileEvidenceV1 current = included[index];
            int target = index;
            while (target > 0
                   && StringComparer.Ordinal.Compare(
                       included[target - 1].CanonicalPath,
                       current.CanonicalPath) > 0)
            {
                included[target] = included[target - 1];
                target--;
            }

            included[target] = current;
        }

        using MemoryStream records = new();
        UTF8Encoding utf8 = new(false, true);
        foreach (ThemeIntegrityFileEvidenceV1 item in included)
        {
            byte[] path = utf8.GetBytes(item.CanonicalPath);
            byte[] length = Encoding.ASCII.GetBytes(
                item.ActualLengthBytes!.Value.ToString(CultureInfo.InvariantCulture));
            byte[] sha256 = Encoding.ASCII.GetBytes(item.ActualSha256!);
            records.Write(path);
            records.WriteByte(0);
            records.Write(length);
            records.WriteByte(0);
            records.Write(sha256);
            records.WriteByte(10);
        }

        hash = Hash(records.ToArray());
        return true;
    }

    private static bool IntegrityManifestsEqual(
        ThemeIntegrityManifestV2 left,
        ThemeIntegrityManifestV2 right)
    {
        if (!string.Equals(left.SchemaVersion, right.SchemaVersion, StringComparison.Ordinal)
            || left.InventoryMode != right.InventoryMode
            || !string.Equals(left.ThemeId, right.ThemeId, StringComparison.Ordinal)
            || !string.Equals(left.Version, right.Version, StringComparison.Ordinal)
            || !string.Equals(left.HashAlgorithm, right.HashAlgorithm, StringComparison.Ordinal)
            || !string.Equals(left.PackageHash, right.PackageHash, StringComparison.Ordinal)
            || left.Files.Count != right.Files.Count)
        {
            return false;
        }

        for (int index = 0; index < left.Files.Count; index++)
        {
            ThemeIntegrityFileV2 first = left.Files[index];
            ThemeIntegrityFileV2 second = right.Files[index];
            if (!string.Equals(first.CanonicalPath, second.CanonicalPath, StringComparison.Ordinal)
                || first.EntryKind != second.EntryKind
                || !string.Equals(first.Sha256, second.Sha256, StringComparison.Ordinal)
                || first.LengthBytes != second.LengthBytes
                || first.Required != second.Required)
            {
                return false;
            }
        }

        return true;
    }

    private static bool SignatureEnvelopesEqual(
        ThemeSignatureEnvelopeV1 left,
        ThemeSignatureEnvelopeV1? right) =>
        right is not null
        && string.Equals(left.EnvelopeVersion, right.EnvelopeVersion, StringComparison.Ordinal)
        && left.Algorithm == right.Algorithm
        && string.Equals(left.PublisherId, right.PublisherId, StringComparison.Ordinal)
        && string.Equals(left.KeyId, right.KeyId, StringComparison.Ordinal)
        && left.SignatureEncoding == right.SignatureEncoding
        && string.Equals(left.Signature, right.Signature, StringComparison.Ordinal)
        && left.SignedPayloadType == right.SignedPayloadType;

    private static bool IsApprovedMetadataReadFailure(Exception exception) =>
        exception is IOException or InvalidDataException or UnauthorizedAccessException;

    private static bool IsApprovedMaterializationFailure(Exception exception) =>
        exception is JsonException or FormatException or OverflowException;

    private static bool IsSha256(string? value)
    {
        if (value is not { Length: 64 })
        {
            return false;
        }

        foreach (char character in value)
        {
            if (character is not (>= '0' and <= '9') and not (>= 'a' and <= 'f'))
            {
                return false;
            }
        }

        return true;
    }

    private static string Hash(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexStringLower(SHA256.HashData(bytes));

    private static ThemePackageMetadataEvaluation InvalidDocument(string? path, string message) =>
        Failure(ThemeIntegrityDiagnosticCodes.InvalidMetadataDocument, path, message);

    private static ThemePackageMetadataEvaluation Failure(string code, string? path, string message) =>
        new(
            false,
            new ThemeIntegrityDiagnosticV1[]
            {
                new(code, ThemeDiagnosticsSeverity.Error, path, message),
            },
            null,
            null,
            null,
            null,
            null,
            null);
}

/// <summary>Validated deterministic metadata inputs for later Phase 4 stages.</summary>
internal sealed record ThemePackageMetadataEvaluation(
    bool CanContinue,
    IReadOnlyList<ThemeIntegrityDiagnosticV1> Diagnostics,
    string? PackageHash,
    string? ThemeManifestHash,
    string? IntegrityManifestHash,
    ThemeManifest? ThemeManifest,
    ThemeIntegrityManifestV2? IntegrityManifest,
    ThemeSignatureEnvelopeV1? SignatureEnvelope);
