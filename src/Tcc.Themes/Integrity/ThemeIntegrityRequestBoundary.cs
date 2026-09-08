using System.Text.RegularExpressions;
using Tcc.Presentation.Contracts.Theme;

namespace Tcc.Themes.Integrity;

/// <summary>
/// Performs deterministic validation of the inputs required before package integrity stages may run.
/// It does not inspect package contents or produce a package verification result.
/// </summary>
internal static class ThemeIntegrityRequestBoundary
{
    private const string ThemeIdentityPattern = "\\A[a-z0-9]+(?:[.-][a-z0-9]+)+\\z";
    private const string ThemeVersionPattern = "\\A(0|[1-9][0-9]*)\\.(0|[1-9][0-9]*)\\.(0|[1-9][0-9]*)\\z";

    internal static bool TryAcceptForNextStage(
        ThemeIntegrityVerificationRequestV2? request,
        IThemePackageContentReader? contentReader,
        CancellationToken cancellationToken,
        out IReadOnlyList<ThemeIntegrityDiagnosticV1> diagnostics)
    {
        cancellationToken.ThrowIfCancellationRequested();

        List<ThemeIntegrityDiagnosticV1> findings = [];
        if (request is null || contentReader is null)
        {
            Add(
                findings,
                ThemeIntegrityDiagnosticCodes.MissingSecurityField,
                "The verification request and caller-provided package content reader are required.");
            diagnostics = Order(findings);
            return false;
        }

        bool missing = HasMissingRequiredInput(request);
        bool invalid = HasInvalidBasicValue(request);

        if (missing)
        {
            Add(
                findings,
                ThemeIntegrityDiagnosticCodes.MissingSecurityField,
                "A required preflight security object, value, collection, or collection item is absent.");
        }

        if (invalid)
        {
            Add(
                findings,
                ThemeIntegrityDiagnosticCodes.InvalidSecurityValue,
                "A preflight security identity, version, or enum value is invalid.");
        }

        if (request.SignatureEnvelope is { } envelope)
        {
            if (IsBlank(envelope.Signature))
            {
                Add(
                    findings,
                    ThemeIntegrityDiagnosticCodes.InvalidSignature,
                    "A present signature envelope must contain a nonblank signature value.");
            }

            if (!Enum.IsDefined(envelope.Algorithm))
            {
                Add(
                    findings,
                    ThemeIntegrityDiagnosticCodes.UnsupportedSignatureAlgorithm,
                    "The signature algorithm enum value is unsupported.");
            }

            if (!Enum.IsDefined(envelope.SignatureEncoding))
            {
                Add(
                    findings,
                    ThemeIntegrityDiagnosticCodes.UnsupportedSignatureEncoding,
                    "The signature encoding enum value is unsupported.");
            }
        }

        diagnostics = Order(findings);
        return diagnostics.Count == 0;
    }

    private static bool HasMissingRequiredInput(ThemeIntegrityVerificationRequestV2 request)
    {
        if (IsBlank(request.ThemeId.Value)
            || IsBlank(request.Version.Value)
            || IsBlank(request.PackageRef.Value)
            || request.ThemeManifestPath is null
            || IsBlank(request.ThemeManifestPath.Value)
            || request.IntegrityManifestPath is null
            || IsBlank(request.IntegrityManifestPath.Value)
            || request.IntegrityManifest is null
            || request.Policy is null
            || request.TrustSnapshot is null)
        {
            return true;
        }

        ThemeIntegrityManifestV2 manifest = request.IntegrityManifest;
        ThemeIntegrityVerificationPolicyV1 policy = request.Policy;
        ThemeTrustSnapshotV1 trust = request.TrustSnapshot;

        if (IsBlank(manifest.SchemaVersion)
            || IsBlank(manifest.ThemeId)
            || IsBlank(manifest.Version)
            || IsBlank(manifest.HashAlgorithm)
            || IsBlank(manifest.PackageHash)
            || manifest.Files is null
            || manifest.Files.Any(file => file is null)
            || IsBlank(policy.PolicyId)
            || IsBlank(policy.PolicyVersion)
            || IsBlank(policy.ExpectedTrustPolicyId)
            || IsBlank(policy.ExpectedTrustPolicyVersion)
            || IsBlank(trust.PolicyId)
            || IsBlank(trust.PolicyVersion)
            || trust.TrustedSigners.IsDefault
            || trust.TrustedSigners.Any(signer => signer is null))
        {
            return true;
        }

        if (manifest.Files.Any(file => IsBlank(file.CanonicalPath) || IsBlank(file.Sha256))
            || trust.TrustedSigners.Any(signer =>
                IsBlank(signer.PublisherId)
                || IsBlank(signer.KeyId)
                || IsBlank(signer.PublicKey)))
        {
            return true;
        }

        if (request.SignatureEnvelope is { } envelope)
        {
            return request.SignatureEnvelopePath is null
                || IsBlank(request.SignatureEnvelopePath.Value)
                || IsBlank(envelope.EnvelopeVersion)
                || IsBlank(envelope.PublisherId)
                || IsBlank(envelope.KeyId);
        }

        return false;
    }

    private static bool HasInvalidBasicValue(ThemeIntegrityVerificationRequestV2 request)
    {
        ThemeIntegrityManifestV2? manifest = request.IntegrityManifest;
        ThemeIntegrityVerificationPolicyV1? policy = request.Policy;
        ThemeTrustSnapshotV1? trust = request.TrustSnapshot;

        if (IsPresentAndInvalid(request.ThemeId.Value, ThemeIdentityPattern)
            || IsPresentAndInvalid(request.Version.Value, ThemeVersionPattern)
            || manifest is not null
            && (IsPresentAndInvalid(manifest.ThemeId, ThemeIdentityPattern)
                || IsPresentAndInvalid(manifest.Version, ThemeVersionPattern)
                || !Enum.IsDefined(manifest.InventoryMode)
                || manifest.Files is not null
                && manifest.Files.Where(file => file is not null).Any(file => !Enum.IsDefined(file.EntryKind)))
            || policy is not null
            && (!Enum.IsDefined(policy.DistributionChannel)
                || !Enum.IsDefined(policy.SignatureRequirement))
            || trust is not null
            && !trust.TrustedSigners.IsDefault
            && trust.TrustedSigners.Where(signer => signer is not null).Any(signer =>
                !Enum.IsDefined(signer.Algorithm)
                || !Enum.IsDefined(signer.PublicKeyEncoding)
                || !Enum.IsDefined(signer.TrustState)))
        {
            return true;
        }

        return request.SignatureEnvelope is { } envelope
            && !Enum.IsDefined(envelope.SignedPayloadType);
    }

    private static bool IsPresentAndInvalid(string? value, string pattern) =>
        !IsBlank(value)
        && !Regex.IsMatch(
            value!,
            pattern,
            RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    private static bool IsBlank(string? value) => string.IsNullOrWhiteSpace(value);

    private static void Add(
        List<ThemeIntegrityDiagnosticV1> diagnostics,
        string code,
        string message)
    {
        ThemeIntegrityDiagnosticV1 diagnostic = new(
            code,
            ThemeDiagnosticsSeverity.Error,
            null,
            message);
        if (!diagnostics.Contains(diagnostic))
        {
            diagnostics.Add(diagnostic);
        }
    }

    private static ThemeIntegrityDiagnosticV1[] Order(
        IEnumerable<ThemeIntegrityDiagnosticV1> diagnostics) =>
        diagnostics
            .OrderBy(diagnostic => diagnostic.Code, StringComparer.Ordinal)
            .ThenBy(diagnostic => diagnostic.CanonicalPath, StringComparer.Ordinal)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.Ordinal)
            .ToArray();
}
