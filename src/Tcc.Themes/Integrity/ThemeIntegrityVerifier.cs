using System.Collections.Immutable;
using Tcc.Presentation.Contracts.Theme;

namespace Tcc.Themes.Integrity;

/// <summary>Composes the sealed package integrity stages into the public V2 verdict.</summary>
public sealed class ThemeIntegrityVerifier : IThemeIntegrityVerifierV2
{
    public async ValueTask<ThemeIntegrityVerificationResultV2> VerifyAsync(
        ThemeIntegrityVerificationRequestV2 request,
        IThemePackageContentReader contentReader,
        CancellationToken cancellationToken = default)
    {
        bool accepted = ThemeIntegrityRequestBoundary.TryAcceptForNextStage(
            request, contentReader, cancellationToken, out IReadOnlyList<ThemeIntegrityDiagnosticV1> boundaryDiagnostics);
        List<ThemeIntegrityDiagnosticV1> diagnostics = new(boundaryDiagnostics);
        ImmutableArray<ThemeIntegrityFileEvidenceV1> files = ImmutableArray<ThemeIntegrityFileEvidenceV1>.Empty;
        ImmutableArray<ThemeIntegrityFileEvidenceV1> assets = ImmutableArray<ThemeIntegrityFileEvidenceV1>.Empty;
        ThemePackageMetadataEvaluation? metadata = null;
        ThemePackageSignatureEvaluation? signature = null;

        if (accepted)
        {
            ThemePackageInventoryEvaluation inventory = await ThemePackageInventoryEvaluator
                .EvaluateAsync(request, contentReader, cancellationToken).ConfigureAwait(false);
            diagnostics.AddRange(inventory.Diagnostics);
            files = ImmutableArray.CreateRange(inventory.FileEvidence);
            assets = ProjectAssets(files);
            if (inventory.CanContinue)
            {
                metadata = await ThemePackageMetadataEvaluator
                    .EvaluateAsync(request, contentReader, inventory, cancellationToken).ConfigureAwait(false);
                diagnostics.AddRange(metadata.Diagnostics);
                if (metadata.CanContinue)
                {
                    signature = ThemePackageSignatureEvaluator.Evaluate(
                        metadata, request.Policy, request.TrustSnapshot, cancellationToken);
                    diagnostics.AddRange(signature.Diagnostics);
                }
            }
        }

        // D is reachable only after A, B and C accepted. Legal unsigned D success
        // is a verified result too; diagnostics or SignatureStatus.Valid are not the verdict.
        bool isVerified = signature?.CanContinue == true;
        diagnostics.Sort(new Comparison<ThemeIntegrityDiagnosticV1>(CompareDiagnostics));
        ThemeIntegrityVerificationResultV2 result = new(
            isVerified,
            metadata?.CanContinue == true ? metadata.PackageHash : null,
            metadata?.CanContinue == true ? metadata.ThemeManifestHash : null,
            metadata?.CanContinue == true ? metadata.IntegrityManifestHash : null,
            files,
            assets,
            signature?.SignatureStatus ?? ThemeSignatureVerificationStatus.NotEvaluated,
            isVerified ? signature!.PublisherId : null,
            isVerified ? signature!.KeyId : null,
            ImmutableArray.CreateRange(diagnostics));
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }

    private static ImmutableArray<ThemeIntegrityFileEvidenceV1> ProjectAssets(
        ImmutableArray<ThemeIntegrityFileEvidenceV1> files)
    {
        ImmutableArray<ThemeIntegrityFileEvidenceV1>.Builder assets =
            ImmutableArray.CreateBuilder<ThemeIntegrityFileEvidenceV1>();
        foreach (ThemeIntegrityFileEvidenceV1 item in files)
        {
            if (item.CanonicalPath.StartsWith("assets/", StringComparison.Ordinal))
            {
                assets.Add(item);
            }
        }

        return assets.ToImmutable();
    }

    private static int CompareDiagnostics(ThemeIntegrityDiagnosticV1 left, ThemeIntegrityDiagnosticV1 right)
    {
        int comparison = StringComparer.Ordinal.Compare(left.Code, right.Code);
        if (comparison == 0)
        {
            comparison = StringComparer.Ordinal.Compare(left.CanonicalPath, right.CanonicalPath);
        }

        return comparison == 0 ? StringComparer.Ordinal.Compare(left.Message, right.Message) : comparison;
    }
}
