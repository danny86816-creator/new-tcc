using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text.Json;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Compatibility.V2;
using Tcc.Themes.Integrity;

namespace Tcc.Themes.Compatibility.Binding;

internal static class ThemeCompatibilityEvidenceBinder
{
    private const string CompatibilityManifestPath = "compatibility.json";

    internal static async ValueTask<ThemeCompatibilityContextV2> VerifyAndBindAsync(
        ThemeIntegrityVerificationRequestV2 request,
        IThemePackageContentReader contentReader,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(contentReader);
        cancellationToken.ThrowIfCancellationRequested();

        ThemeIntegrityVerificationRequestV2 requestSnapshot = Snapshot(request);
        ThemeCompatibilityContentSnapshot contentSnapshot;
        try
        {
            contentSnapshot = await ThemeCompatibilityContentSnapshot
                .CaptureAsync(requestSnapshot.PackageRef, contentReader, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (IsExpectedAcquisitionFailure(exception))
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Context(
                requestSnapshot.PackageRef,
                null,
                null,
                null,
                null,
                ThemeCompatibilityFailureKindV2.ContentSnapshotUnavailable,
                ThemeCompatibilityDimensionV2.Integrity);
        }

        ThemeIntegrityVerificationResultV2 integrity = await new ThemeIntegrityVerifier()
            .VerifyAsync(requestSnapshot, contentSnapshot, cancellationToken)
            .ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        if (!integrity.IsVerified)
        {
            return Context(
                requestSnapshot.PackageRef,
                integrity,
                null,
                null,
                null,
                ThemeCompatibilityFailureKindV2.IntegrityNotVerified,
                ThemeCompatibilityDimensionV2.Integrity);
        }

        if (!TryGetVerifiedContent(
                contentSnapshot,
                integrity,
                requestSnapshot.ThemeManifestPath.Value,
                ThemeIntegrityEntryKind.ThemeManifest,
                out ReadOnlyMemory<byte> themeBytes,
                out string? themeHash)
            || !string.Equals(integrity.ThemeManifestHash, themeHash, StringComparison.Ordinal))
        {
            return Context(
                requestSnapshot.PackageRef,
                integrity,
                null,
                null,
                null,
                ThemeCompatibilityFailureKindV2.ContentEvidenceMismatch,
                ThemeCompatibilityDimensionV2.Integrity);
        }

        ThemeManifest? manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<ThemeManifest>(
                themeBytes.Span,
                ThemeContractJson.CreateSerializerOptions());
        }
        catch (JsonException)
        {
            manifest = null;
        }
        catch (NotSupportedException)
        {
            manifest = null;
        }

        if (manifest is null
            || !string.Equals(manifest.Package.ThemeId, requestSnapshot.ThemeId.Value, StringComparison.Ordinal)
            || !string.Equals(manifest.Package.Version, requestSnapshot.Version.Value, StringComparison.Ordinal))
        {
            return Context(
                requestSnapshot.PackageRef,
                integrity,
                null,
                null,
                null,
                ThemeCompatibilityFailureKindV2.ContentEvidenceMismatch,
                ThemeCompatibilityDimensionV2.Integrity);
        }

        if (!TryGetVerifiedContent(
                contentSnapshot,
                integrity,
                CompatibilityManifestPath,
                ThemeIntegrityEntryKind.Payload,
                out ReadOnlyMemory<byte> compatibilityBytes,
                out string? compatibilityHash))
        {
            return Context(
                requestSnapshot.PackageRef,
                integrity,
                manifest,
                null,
                null,
                ThemeCompatibilityFailureKindV2.ContentEvidenceMismatch,
                ThemeCompatibilityDimensionV2.Integrity);
        }

        ThemeCompatibilityManifest? compatibility = ThemeCompatibilityManifestMaterializer.Materialize(
            compatibilityBytes,
            out ImmutableArray<ThemeCompatibilityFailureV2> materializationFailures);
        cancellationToken.ThrowIfCancellationRequested();
        if (!materializationFailures.IsEmpty)
        {
            return new ThemeCompatibilityContextV2(
                requestSnapshot.PackageRef,
                integrity,
                manifest,
                null,
                compatibilityHash,
                materializationFailures);
        }

        if (compatibility is null
            || !string.Equals(compatibility.ThemeId, manifest.Package.ThemeId, StringComparison.Ordinal)
            || !string.Equals(compatibility.Version, manifest.Package.Version, StringComparison.Ordinal))
        {
            return Context(
                requestSnapshot.PackageRef,
                integrity,
                manifest,
                compatibility,
                compatibilityHash,
                ThemeCompatibilityFailureKindV2.ContentEvidenceMismatch,
                ThemeCompatibilityDimensionV2.Integrity);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return new ThemeCompatibilityContextV2(
            requestSnapshot.PackageRef,
            integrity,
            manifest,
            compatibility,
            compatibilityHash,
            ImmutableArray<ThemeCompatibilityFailureV2>.Empty);
    }

    private static ThemeIntegrityVerificationRequestV2 Snapshot(ThemeIntegrityVerificationRequestV2 request)
    {
        ImmutableArray<ThemeIntegrityFileV2>.Builder fileBuilder =
            ImmutableArray.CreateBuilder<ThemeIntegrityFileV2>();
        foreach (ThemeIntegrityFileV2 file in request.IntegrityManifest.Files)
        {
            fileBuilder.Add(file with { });
        }

        ImmutableArray<ThemeIntegrityFileV2> files = fileBuilder.ToImmutable();
        ThemeIntegrityManifestV2 integrityManifest = request.IntegrityManifest with { Files = files };
        ImmutableArray<ThemeTrustedSignerV1>.Builder signerBuilder =
            ImmutableArray.CreateBuilder<ThemeTrustedSignerV1>();
        foreach (ThemeTrustedSignerV1 signer in request.TrustSnapshot.TrustedSigners)
        {
            signerBuilder.Add(signer with { });
        }

        ThemeTrustSnapshotV1 trustSnapshot = request.TrustSnapshot with
        {
            TrustedSigners = signerBuilder.ToImmutable(),
        };
        return request with
        {
            ThemeManifestPath = ThemeCanonicalPath.Parse(request.ThemeManifestPath.Value),
            IntegrityManifestPath = ThemeCanonicalPath.Parse(request.IntegrityManifestPath.Value),
            SignatureEnvelopePath = request.SignatureEnvelopePath is null
                ? null
                : ThemeCanonicalPath.Parse(request.SignatureEnvelopePath.Value),
            IntegrityManifest = integrityManifest,
            SignatureEnvelope = request.SignatureEnvelope is null ? null : request.SignatureEnvelope with { },
            Policy = request.Policy with { },
            TrustSnapshot = trustSnapshot,
        };
    }

    private static bool TryGetVerifiedContent(
        ThemeCompatibilityContentSnapshot snapshot,
        ThemeIntegrityVerificationResultV2 integrity,
        string canonicalPath,
        ThemeIntegrityEntryKind entryKind,
        out ReadOnlyMemory<byte> bytes,
        out string? hash)
    {
        bytes = default;
        hash = null;
        ThemeIntegrityFileEvidenceV1? matched = null;
        int matches = 0;
        foreach (ThemeIntegrityFileEvidenceV1 item in integrity.FileEvidence)
        {
            if (string.Equals(item.CanonicalPath, canonicalPath, StringComparison.Ordinal))
            {
                matched = item;
                matches++;
            }
        }

        if (matches != 1
            || matched is null
            || matched.EntryKind != entryKind
            || !matched.IsPresent
            || matched.Status != ThemeIntegrityEvidenceStatus.Verified
            || matched.ActualLengthBytes is null
            || matched.ActualSha256 is null
            || !snapshot.TryRead(canonicalPath, out bytes))
        {
            return false;
        }

        hash = Convert.ToHexStringLower(SHA256.HashData(bytes.Span));
        return bytes.Length == matched.ActualLengthBytes
            && string.Equals(hash, matched.ActualSha256, StringComparison.Ordinal);
    }

    private static ThemeCompatibilityContextV2 Context(
        ThemePackageRef packageRef,
        ThemeIntegrityVerificationResultV2? integrity,
        ThemeManifest? manifest,
        ThemeCompatibilityManifest? compatibility,
        string? compatibilityHash,
        ThemeCompatibilityFailureKindV2 kind,
        ThemeCompatibilityDimensionV2 dimension) =>
        new(
            packageRef,
            integrity,
            manifest,
            compatibility,
            compatibilityHash,
            ImmutableArray.Create(new ThemeCompatibilityFailureV2(
                kind,
                dimension,
                0,
                null,
                $"{kind} in {dimension}.")));

    private static bool IsExpectedAcquisitionFailure(Exception exception) =>
        exception is IOException or InvalidDataException or UnauthorizedAccessException;
}
