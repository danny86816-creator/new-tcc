using System.Collections.Immutable;
using Tcc.Presentation.Contracts.Theme;

namespace Tcc.Themes.Compatibility.V2;

/// <summary>Composes immutable evidence. No implementation or issuance is supplied by Candidate A.</summary>
public interface IThemeCompatibilityResolverV2
{
    ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request);
}

public sealed class ThemeCompatibilityRequestV2
{
    public ThemeCompatibilityRequestV2(
        ThemeCompatibilityContextV2? context,
        ThemeCompatibilityEnvironmentV2? environment,
        ThemeCompatibilityEvidenceV2? evidence)
    {
        Context = context;
        Environment = environment;
        Evidence = evidence;
    }

    public ThemeCompatibilityContextV2? Context { get; }
    public ThemeCompatibilityEnvironmentV2? Environment { get; }
    public ThemeCompatibilityEvidenceV2? Evidence { get; }
}

/// <summary>
/// In-process provenance only. Future production issuance belongs exclusively to the trusted binder.
/// This object neither acquires content nor promotes a caller-created result into verified evidence.
/// Internal callers and the existing test friend are part of the trusted computing base.
/// </summary>
public sealed class ThemeCompatibilityContextV2
{
    internal ThemeCompatibilityContextV2(
        ThemePackageRef packageRef,
        ThemeIntegrityVerificationResultV2? integrity,
        ThemeManifest? manifest,
        ThemeCompatibilityManifest? compatibility,
        string? compatibilityManifestHash,
        IEnumerable<ThemeCompatibilityFailureV2> bindingFailures)
    {
        ArgumentNullException.ThrowIfNull(bindingFailures);
        PackageRef = packageRef;
        Integrity = integrity is null ? null : integrity with
        {
            FileEvidence = integrity.FileEvidence.ToImmutableArray(),
            AssetEvidence = integrity.AssetEvidence.ToImmutableArray(),
            Diagnostics = integrity.Diagnostics.ToImmutableArray(),
        };
        Manifest = Snapshot(manifest);
        Compatibility = compatibility is null ? null : compatibility with
        {
            Core = compatibility.Core with { Tested = compatibility.Core.Tested.ToImmutableArray() },
            ThemeApi = compatibility.ThemeApi with { Tested = compatibility.ThemeApi.Tested.ToImmutableArray() },
            UxContract = compatibility.UxContract with { Tested = compatibility.UxContract.Tested.ToImmutableArray() },
            Windows = compatibility.Windows with { DpiRangesTested = compatibility.Windows.DpiRangesTested.ToImmutableArray() },
            AccessibilityMatrix = compatibility.AccessibilityMatrix.ToImmutableDictionary(StringComparer.Ordinal),
        };
        CompatibilityManifestHash = compatibilityManifestHash;
        BindingFailures = bindingFailures.ToImmutableArray();
    }

    internal ThemePackageRef PackageRef { get; }
    internal ThemeIntegrityVerificationResultV2? Integrity { get; }
    internal ThemeManifest? Manifest { get; }
    internal ThemeCompatibilityManifest? Compatibility { get; }
    internal string? CompatibilityManifestHash { get; }
    internal ImmutableArray<ThemeCompatibilityFailureV2> BindingFailures { get; }

    private static ThemeManifest? Snapshot(ThemeManifest? value)
    {
        if (value is null)
        {
            return null;
        }
        ImmutableArray<ThemeVariant>.Builder variants = ImmutableArray.CreateBuilder<ThemeVariant>();
        foreach (ThemeVariant variant in value.Variants)
        {
            variants.Add(variant with { AccessibilityOverrides = variant.AccessibilityOverrides.ToImmutableArray() });
        }
        ImmutableArray<ThemePresentationFeatureFlag>.Builder flags = ImmutableArray.CreateBuilder<ThemePresentationFeatureFlag>();
        foreach (ThemePresentationFeatureFlag flag in value.FeatureFlags.PresentationOnly)
        {
            flags.Add(flag with { CannotAffect = flag.CannotAffect.ToImmutableArray() });
        }
        return value with
        {
            Package = value.Package with { Tags = value.Package.Tags.ToImmutableArray() },
            Compatibility = value.Compatibility with { SupportedPlatforms = value.Compatibility.SupportedPlatforms.ToImmutableArray() },
            Variants = variants.ToImmutable(),
            Capabilities = value.Capabilities.ToImmutableArray(),
            FeatureFlags = value.FeatureFlags with { PresentationOnly = flags.ToImmutable() },
            DegradedMode = value.DegradedMode with
            {
                Levels = value.DegradedMode.Levels.ToImmutableArray(),
                Preserves = value.DegradedMode.Preserves.ToImmutableArray(),
            },
            Assets = value.Assets with { Tiers = value.Assets.Tiers.ToImmutableArray() },
            Rollback = value.Rollback is null ? null : value.Rollback with
            {
                PreviousVersionCompatibility = value.Rollback.PreviousVersionCompatibility.ToImmutableArray(),
            },
        };
    }
}
