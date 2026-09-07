using Tcc.Presentation.Contracts.Theme;

namespace Tcc.Themes.Manifests;

/// <summary>
/// Performs deterministic semantic validation of an already materialized Theme manifest.
/// This type deliberately performs no package I/O, integrity verification, or runtime activation.
/// </summary>
public sealed class ThemeManifestValidator : IThemeManifestValidator
{
    public ThemeManifestValidationResult Validate(
        ThemeManifest manifest,
        ThemeManifestValidationContext context)
    {
        List<string> errors = [];

        if (manifest is null)
        {
            errors.Add($"{DiagnosticCodes.ManifestRequired} manifest is required.");
            return Invalid(errors);
        }

        if (context is null)
        {
            errors.Add($"{DiagnosticCodes.ContextRequired} validation context is required.");
            return Invalid(errors);
        }

        ValidateVersions(manifest, context, errors);
        ValidatePackage(manifest.Package, errors);
        ValidateCompatibility(manifest.Compatibility, errors);
        ValidateVariants(manifest.Variants, errors);
        ValidateCapabilities(manifest.Capabilities, context.KnownCapabilities, errors);
        ValidateFeatureFlags(manifest.FeatureFlags, errors);
        ValidateDegradedMode(manifest.DegradedMode, errors);
        ValidateAccessibility(manifest.Accessibility, errors);
        ValidateAssets(manifest.Assets, errors);
        ValidateAudio(manifest.Audio, errors);
        ValidateMotion(manifest.Motion, errors);
        ValidatePersonalization(manifest.Personalization, errors);
        ValidateIntegrity(
            manifest.Integrity,
            manifest.Package?.Channel,
            context.IsStableChannel,
            errors);
        ValidateRollback(manifest.Rollback, manifest.Personalization, errors);

        return new ThemeManifestValidationResult(errors.Count == 0, errors.ToArray(), []);
    }

    private static ThemeManifestValidationResult Invalid(List<string> errors) =>
        new(false, errors.ToArray(), []);

    private static void ValidateVersions(
        ThemeManifest manifest,
        ThemeManifestValidationContext context,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(manifest.SchemaVersion)
            || context.SupportedSchemaVersions is null
            || !context.SupportedSchemaVersions.Contains(manifest.SchemaVersion))
        {
            errors.Add($"{DiagnosticCodes.SchemaUnsupported} schema_version '{Display(manifest.SchemaVersion)}' is unsupported.");
        }

        if (!string.Equals(manifest.ThemeApiVersion, ContractVersions.ThemeApi, StringComparison.Ordinal))
        {
            errors.Add(
                $"{DiagnosticCodes.ThemeApiUnsupported} theme_api_version '{Display(manifest.ThemeApiVersion)}' is unsupported; expected '{ContractVersions.ThemeApi}'.");
        }
    }

    private static void ValidatePackage(
        ThemePackageIdentity package,
        List<string> errors)
    {
        if (package is null)
        {
            errors.Add($"{DiagnosticCodes.PackageRequired} package declaration is required.");
            return;
        }

        if (!IsDottedIdentifier(package.ThemeId, requireSeparator: true))
        {
            errors.Add($"{DiagnosticCodes.ThemeIdMalformed} theme_id '{Display(package.ThemeId)}' is malformed.");
        }

        if (!IsDottedIdentifier(package.PackageId, requireSeparator: true))
        {
            errors.Add($"{DiagnosticCodes.PackageIdMalformed} package_id '{Display(package.PackageId)}' is malformed.");
        }

        ValidateRequiredText(package.Name, "name", DiagnosticCodes.PackageNameRequired, errors);
        ValidateRequiredText(package.Publisher, "publisher", DiagnosticCodes.PackagePublisherRequired, errors);

        if (!IsDottedIdentifier(package.PublisherId, requireSeparator: false))
        {
            errors.Add($"{DiagnosticCodes.PublisherIdMalformed} publisher_id '{Display(package.PublisherId)}' is malformed.");
        }

        if (!IsSemanticVersion(package.Version))
        {
            errors.Add($"{DiagnosticCodes.PackageVersionInvalid} package version '{Display(package.Version)}' must be a strict major.minor.patch version.");
        }

        if (!IsOneOf(package.Channel, "stable", "beta", "developer"))
        {
            errors.Add($"{DiagnosticCodes.PackageChannelInvalid} package channel '{Display(package.Channel)}' is invalid.");
        }

        ValidateRequiredText(package.Description, "description", DiagnosticCodes.PackageDescriptionRequired, errors);
        ValidateRequiredText(package.License, "license", DiagnosticCodes.PackageLicenseRequired, errors);
        ValidateOptionalNonBlank(package.Homepage, "homepage", DiagnosticCodes.PackageHomepageBlank, errors);
        ValidateOptionalNonBlank(package.SupportUrl, "support_url", DiagnosticCodes.PackageSupportUrlBlank, errors);
        ValidateUniqueNonBlankValues(package.Tags, "tag", DiagnosticCodes.PackageTagInvalid, errors);
    }

    private static void ValidateRequiredText(
        string value,
        string field,
        string code,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{code} package {field} is required.");
        }
    }

    private static void ValidateOptionalNonBlank(
        string? value,
        string field,
        string code,
        List<string> errors)
    {
        if (value is not null && string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{code} package {field} cannot be blank when declared.");
        }
    }

    private static void ValidateCompatibility(
        ThemeCompatibilityDeclaration compatibility,
        List<string> errors)
    {
        if (compatibility is null)
        {
            errors.Add($"{DiagnosticCodes.CompatibilityRequired} compatibility declaration is required.");
            return;
        }

        if (!IsVersionRangeDeclaration(compatibility.RequiredCoreVersion)
            || !IsVersionRangeDeclaration(compatibility.RequiredThemeApiVersion)
            || !IsVersionRangeDeclaration(compatibility.RequiredUxContractVersion))
        {
            errors.Add($"{DiagnosticCodes.CompatibilityVersionMalformed} compatibility version declarations are malformed.");
        }

        if (compatibility.SupportedPlatforms is null
            || compatibility.SupportedPlatforms.Count == 0
            || compatibility.SupportedPlatforms.Any(platform =>
                !string.Equals(platform, "windows", StringComparison.Ordinal))
            || HasDuplicates(compatibility.SupportedPlatforms))
        {
            errors.Add($"{DiagnosticCodes.CompatibilityPlatformsInvalid} compatibility supported_platforms must contain unique 'windows' declarations only.");
        }

        if (!compatibility.PortableSupported)
        {
            errors.Add($"{DiagnosticCodes.CompatibilityPortableRequired} compatibility must preserve Portable support.");
        }

        if (compatibility.MinimumDpiScale < 1m
            || compatibility.MaximumTestedDpiScale < compatibility.MinimumDpiScale)
        {
            errors.Add($"{DiagnosticCodes.CompatibilityDpiInvalid} compatibility DPI bounds are invalid.");
        }
    }

    private static void ValidateVariants(IReadOnlyList<ThemeVariant> variants, List<string> errors)
    {
        if (variants is null || variants.Count == 0)
        {
            errors.Add($"{DiagnosticCodes.VariantCollectionRequired} at least one variant is required.");
            return;
        }

        Dictionary<string, ThemeVariant> variantsById = new(StringComparer.Ordinal);
        int defaultCount = 0;

        for (int index = 0; index < variants.Count; index++)
        {
            ThemeVariant? variant = variants[index];
            if (variant is null)
            {
                errors.Add($"{DiagnosticCodes.VariantEntryRequired} variant at index {index} is required.");
                continue;
            }

            if (!IsSimpleIdentifier(variant.VariantId))
            {
                errors.Add($"{DiagnosticCodes.VariantIdMalformed} variant_id '{Display(variant.VariantId)}' is malformed.");
            }
            else if (!variantsById.TryAdd(variant.VariantId, variant))
            {
                errors.Add($"{DiagnosticCodes.VariantIdDuplicate} variant_id '{variant.VariantId}' is duplicated.");
            }

            if (!IsOneOf(variant.Type, "deep", "light", "high_contrast", "custom"))
            {
                errors.Add($"{DiagnosticCodes.VariantTypeInvalid} variant '{Display(variant.VariantId)}' has invalid type '{Display(variant.Type)}'.");
            }

            if (string.IsNullOrWhiteSpace(variant.DisplayName))
            {
                errors.Add($"{DiagnosticCodes.VariantDisplayNameRequired} variant '{Display(variant.VariantId)}' display_name is required.");
            }

            if (!IsSafeDeclarationReference(variant.TokenBundleRef))
            {
                errors.Add($"{DiagnosticCodes.VariantTokenReferenceInvalid} variant '{Display(variant.VariantId)}' token_file reference is invalid.");
            }

            ValidateReferenceList(
                variant.AccessibilityOverrides,
                $"variant '{Display(variant.VariantId)}' accessibility_overrides",
                DiagnosticCodes.VariantAccessibilityOverrideInvalid,
                allowEmpty: true,
                errors);

            if (variant.Default)
            {
                defaultCount++;
            }
        }

        if (defaultCount != 1)
        {
            errors.Add($"{DiagnosticCodes.VariantDefaultCountInvalid} variants must declare exactly one default; found {defaultCount}.");
        }

        foreach (ThemeVariant? variant in variants)
        {
            if (variant is null || string.IsNullOrWhiteSpace(variant.FallbackVariantId))
            {
                continue;
            }

            if (string.Equals(variant.VariantId, variant.FallbackVariantId, StringComparison.Ordinal))
            {
                errors.Add($"{DiagnosticCodes.VariantSelfFallback} variant '{Display(variant.VariantId)}' cannot fall back to itself.");
            }
            else if (!variantsById.ContainsKey(variant.FallbackVariantId))
            {
                errors.Add($"{DiagnosticCodes.VariantFallbackMissing} variant '{Display(variant.VariantId)}' references missing fallback '{variant.FallbackVariantId}'.");
            }
            else if (HasFallbackCycle(variant, variantsById))
            {
                errors.Add($"{DiagnosticCodes.VariantFallbackCycle} variant '{Display(variant.VariantId)}' participates in a fallback cycle.");
            }
        }
    }

    private static bool HasFallbackCycle(
        ThemeVariant start,
        Dictionary<string, ThemeVariant> variantsById)
    {
        HashSet<string> visited = new(StringComparer.Ordinal);
        ThemeVariant current = start;

        while (!string.IsNullOrWhiteSpace(current.FallbackVariantId)
               && variantsById.TryGetValue(current.FallbackVariantId, out ThemeVariant? next))
        {
            if (!visited.Add(current.VariantId))
            {
                return true;
            }

            current = next;
        }

        return false;
    }

    private static void ValidateCapabilities(
        IReadOnlyList<string> capabilities,
        IReadOnlySet<string> knownCapabilities,
        List<string> errors)
    {
        if (capabilities is null || capabilities.Count == 0)
        {
            errors.Add($"{DiagnosticCodes.CapabilityCollectionRequired} at least one presentation capability is required.");
            return;
        }

        HashSet<string> seen = new(StringComparer.Ordinal);
        for (int index = 0; index < capabilities.Count; index++)
        {
            string? capability = capabilities[index];
            if (string.IsNullOrWhiteSpace(capability))
            {
                errors.Add($"{DiagnosticCodes.CapabilityEntryRequired} capability at index {index} is required.");
                continue;
            }

            if (!seen.Add(capability))
            {
                errors.Add($"{DiagnosticCodes.CapabilityDuplicate} capability '{capability}' is duplicated.");
            }

            if (!capability.StartsWith("presentation.", StringComparison.Ordinal))
            {
                errors.Add($"{DiagnosticCodes.CapabilityForbidden} capability '{capability}' is forbidden because Theme capabilities must be presentation-only.");
            }
            else if (knownCapabilities is null || !knownCapabilities.Contains(capability))
            {
                errors.Add($"{DiagnosticCodes.CapabilityUnknown} capability '{capability}' is unknown.");
            }
        }
    }

    private static void ValidateFeatureFlags(ThemeFeatureFlags featureFlags, List<string> errors)
    {
        if (featureFlags?.PresentationOnly is null)
        {
            errors.Add($"{DiagnosticCodes.FeatureFlagCollectionRequired} feature_flags.presentation_only is required.");
            return;
        }

        string[] requiredGuards =
        [
            "risk",
            "permissions",
            "audit",
            "recovery",
            "connector_scope",
            "gpt_ai_required_state",
            "authoritative_data",
            "home_safety_core",
            "critical_alert_priority",
            "confirmation_semantics",
            "accessibility",
        ];
        HashSet<string> seenFlags = new(StringComparer.Ordinal);

        for (int index = 0; index < featureFlags.PresentationOnly.Count; index++)
        {
            ThemePresentationFeatureFlag? flag = featureFlags.PresentationOnly[index];
            if (flag is null)
            {
                errors.Add($"{DiagnosticCodes.FeatureFlagEntryRequired} feature flag at index {index} is required.");
                continue;
            }

            if (!IsPresentationIdentifier(flag.FlagId))
            {
                errors.Add($"{DiagnosticCodes.FeatureFlagIdInvalid} feature flag id '{Display(flag.FlagId)}' must be a presentation-only identifier.");
            }
            else if (!seenFlags.Add(flag.FlagId))
            {
                errors.Add($"{DiagnosticCodes.FeatureFlagIdDuplicate} feature flag id '{flag.FlagId}' is duplicated.");
            }

            if (string.IsNullOrWhiteSpace(flag.Description))
            {
                errors.Add($"{DiagnosticCodes.FeatureFlagDescriptionRequired} feature flag '{Display(flag.FlagId)}' description is required.");
            }

            if (flag.CannotAffect is null)
            {
                foreach (string requiredGuard in requiredGuards)
                {
                    errors.Add($"{DiagnosticCodes.FeatureFlagSafetyGuardMissing} feature flag '{Display(flag.FlagId)}' must declare that it cannot affect '{requiredGuard}'.");
                }

                continue;
            }

            HashSet<string> declaredGuards = new(flag.CannotAffect, StringComparer.Ordinal);
            if (declaredGuards.Count != flag.CannotAffect.Count
                || flag.CannotAffect.Any(string.IsNullOrWhiteSpace))
            {
                errors.Add($"{DiagnosticCodes.FeatureFlagSafetyGuardsInvalid} feature flag '{Display(flag.FlagId)}' cannot_affect entries must be unique and non-blank.");
            }

            foreach (string requiredGuard in requiredGuards)
            {
                if (!declaredGuards.Contains(requiredGuard))
                {
                    errors.Add($"{DiagnosticCodes.FeatureFlagSafetyGuardMissing} feature flag '{Display(flag.FlagId)}' must declare that it cannot affect '{requiredGuard}'.");
                }
            }
        }
    }

    private static void ValidateDegradedMode(
        ThemeDegradedModeDeclaration degradedMode,
        List<string> errors)
    {
        if (degradedMode is null)
        {
            errors.Add($"{DiagnosticCodes.DegradedModeRequired} degraded_mode declaration is required.");
            return;
        }

        if (!degradedMode.Supported)
        {
            errors.Add($"{DiagnosticCodes.DegradedModeSupportedRequired} degraded_mode must be supported.");
        }

        string[] requiredLevels =
            ["none", "reduced_decoration", "minimal_decoration", "safe_presentation_only"];
        ValidateExactSet(
            degradedMode.Levels,
            requiredLevels,
            DiagnosticCodes.DegradedModeLevelsInvalid,
            "degraded_mode levels",
            errors);

        if (!string.Equals(
                degradedMode.DefaultFallbackLevel,
                "safe_presentation_only",
                StringComparison.Ordinal))
        {
            errors.Add($"{DiagnosticCodes.DegradedModeFallbackInvalid} degraded_mode default_fallback_level must be 'safe_presentation_only'.");
        }

        string[] requiredPreservations =
        [
            "safety_core",
            "critical_alerts",
            "keyboard",
            "screen_reader",
            "contrast",
            "risk_permission_semantics",
            "confirmation_semantics",
            "accessibility",
        ];
        IReadOnlyList<string> preservations = degradedMode.Preserves ?? [];
        HashSet<string> declared = new(preservations, StringComparer.Ordinal);

        if (declared.Count != preservations.Count || preservations.Any(string.IsNullOrWhiteSpace))
        {
            errors.Add($"{DiagnosticCodes.DegradedModePreservesInvalid} degraded_mode preserves entries must be unique and non-blank.");
        }

        foreach (string required in requiredPreservations)
        {
            if (!declared.Contains(required))
            {
                errors.Add($"{DiagnosticCodes.DegradedModePreservationMissing} degraded_mode must preserve '{required}'.");
            }
        }
    }

    private static void ValidateAccessibility(
        ThemeAccessibilityDeclaration accessibility,
        List<string> errors)
    {
        if (accessibility is null)
        {
            errors.Add($"{DiagnosticCodes.AccessibilityRequired} accessibility declaration is required.");
            return;
        }

        RequireDeclaration(accessibility.TextScalingSupported, "text_scaling_supported", DiagnosticCodes.AccessibilityTextScalingRequired, errors);
        RequireDeclaration(accessibility.ZoomSupported, "zoom_supported", DiagnosticCodes.AccessibilityZoomRequired, errors);
        RequireDeclaration(accessibility.ContrastSupported, "contrast_supported", DiagnosticCodes.AccessibilityContrastRequired, errors);
        RequireDeclaration(accessibility.ColorVisionModesSupported, "color_vision_modes_supported", DiagnosticCodes.AccessibilityColorVisionRequired, errors);
        RequireDeclaration(accessibility.ReducedMotionSupported, "reduced_motion_supported", DiagnosticCodes.AccessibilityReducedMotionRequired, errors);
        RequireDeclaration(accessibility.ReducedTransparencySupported, "reduced_transparency_supported", DiagnosticCodes.AccessibilityReducedTransparencyRequired, errors);
        RequireDeclaration(accessibility.SoundControlsSupported, "sound_controls_supported", DiagnosticCodes.AccessibilitySoundControlsRequired, errors);
        RequireDeclaration(accessibility.KeyboardFocusVisible, "keyboard_focus_visible", DiagnosticCodes.AccessibilityKeyboardFocusRequired, errors);
        RequireDeclaration(accessibility.FullKeyboardOperation, "full_keyboard_operation", DiagnosticCodes.AccessibilityFullKeyboardRequired, errors);
        RequireDeclaration(accessibility.ScreenReaderLabelsPreserved, "screen_reader_labels_preserved", DiagnosticCodes.AccessibilityScreenReaderRequired, errors);
        RequireDeclaration(accessibility.CriticalAlertRedundancySupported, "critical_alert_redundancy_supported", DiagnosticCodes.AccessibilityCriticalAlertRedundancyRequired, errors);
    }

    private static void RequireDeclaration(bool value, string field, string code, List<string> errors)
    {
        if (!value)
        {
            errors.Add($"{code} accessibility.{field} must be declared true.");
        }
    }

    private static void ValidateAssets(ThemeAssetDeclaration assets, List<string> errors)
    {
        if (assets is null)
        {
            errors.Add($"{DiagnosticCodes.AssetsRequired} assets declaration is required.");
            return;
        }

        if (!IsSafeDeclarationReference(assets.Inventory))
        {
            errors.Add($"{DiagnosticCodes.AssetsInventoryInvalid} assets.inventory reference is invalid.");
        }

        ValidateAllowedSet(
            assets.Tiers,
            ["tier0", "tier1", "tier2", "tier3", "audio"],
            DiagnosticCodes.AssetsTiersInvalid,
            "assets tiers",
            errors);

        if (assets.TotalDeclaredSizeBytes < 0)
        {
            errors.Add($"{DiagnosticCodes.AssetsSizeInvalid} assets.total_declared_size_bytes cannot be negative.");
        }

        if (!string.Equals(assets.HashAlgorithm, "sha256", StringComparison.Ordinal))
        {
            errors.Add($"{DiagnosticCodes.AssetsHashInvalid} assets.hash_algorithm must be 'sha256'.");
        }
    }

    private static void ValidateAudio(ThemeAudioDeclaration? audio, List<string> errors)
    {
        if (audio is null)
        {
            return;
        }

        ValidateReference(audio.SoundPack, "audio.sound_pack", DiagnosticCodes.AudioSoundPackInvalid, errors);
        ValidateReference(audio.Bgm, "audio.bgm", DiagnosticCodes.AudioBgmInvalid, errors);
        ValidateReference(audio.Ambient, "audio.ambient", DiagnosticCodes.AudioAmbientInvalid, errors);

        if (!audio.Optional)
        {
            errors.Add($"{DiagnosticCodes.AudioOptionalRequired} audio must remain optional.");
        }

        if (!audio.OneClickDisableSupported)
        {
            errors.Add($"{DiagnosticCodes.AudioOneClickDisableRequired} audio must preserve one-click disable support.");
        }

        if (!audio.NightModeSupported)
        {
            errors.Add($"{DiagnosticCodes.AudioNightModeRequired} audio must preserve night-mode support.");
        }

        if (!audio.DuckingSupported)
        {
            errors.Add($"{DiagnosticCodes.AudioDuckingRequired} audio must preserve ducking support.");
        }
    }

    private static void ValidateMotion(ThemeMotionDeclaration motion, List<string> errors)
    {
        if (motion is null)
        {
            errors.Add($"{DiagnosticCodes.MotionRequired} motion declaration is required.");
            return;
        }

        ValidateReference(motion.Profiles, "motion.profiles", DiagnosticCodes.MotionProfilesInvalid, errors);
        ValidateReference(motion.ReducedMotion, "motion.reduced_motion", DiagnosticCodes.MotionReducedMotionInvalid, errors);

        if (!motion.InterruptibleTransitions)
        {
            errors.Add($"{DiagnosticCodes.MotionInterruptibleRequired} motion must preserve interruptible transitions.");
        }

        if (!motion.SkippableTransitions)
        {
            errors.Add($"{DiagnosticCodes.MotionSkippableRequired} motion must preserve skippable transitions.");
        }

        if (!motion.ParallaxStaticFallback)
        {
            errors.Add($"{DiagnosticCodes.MotionParallaxFallbackRequired} motion must preserve the parallax static fallback.");
        }
    }

    private static void ValidatePersonalization(
        ThemePersonalizationDeclaration? personalization,
        List<string> errors)
    {
        if (personalization is null)
        {
            return;
        }

        ValidateReference(personalization.Presets, "personalization.presets", DiagnosticCodes.PersonalizationPresetsInvalid, errors);
        ValidateReference(personalization.SafeRanges, "personalization.safe_ranges", DiagnosticCodes.PersonalizationSafeRangesInvalid, errors);
        ValidateReference(personalization.Migrations, "personalization.migrations", DiagnosticCodes.PersonalizationMigrationsInvalid, errors);

        if (!personalization.ResetSupported)
        {
            errors.Add($"{DiagnosticCodes.PersonalizationResetRequired} personalization must preserve reset support.");
        }
    }

    private static void ValidateIntegrity(
        ThemeIntegrityDeclaration integrity,
        string? packageChannel,
        bool isStableChannel,
        List<string> errors)
    {
        if (integrity is null)
        {
            errors.Add($"{DiagnosticCodes.IntegrityRequired} integrity declaration is required.");
            return;
        }

        ValidateReference(
            integrity.IntegrityManifest,
            "integrity.integrity_manifest",
            DiagnosticCodes.IntegrityManifestInvalid,
            errors);
        if (integrity.Signature is not null)
        {
            ValidateReference(
                integrity.Signature,
                "integrity.signature",
                DiagnosticCodes.IntegritySignatureInvalid,
                errors);
        }

        if (!string.Equals(integrity.SignatureRequiredForChannel, "stable_or_store", StringComparison.Ordinal))
        {
            errors.Add($"{DiagnosticCodes.IntegritySignaturePolicyInvalid} integrity.signature_required_for_channel must be 'stable_or_store'.");
        }

        if (!string.Equals(integrity.HashAlgorithm, "sha256", StringComparison.Ordinal))
        {
            errors.Add($"{DiagnosticCodes.IntegrityHashInvalid} integrity.hash_algorithm must be 'sha256'.");
        }

        bool signatureRequired = isStableChannel
                                 || string.Equals(packageChannel, "stable", StringComparison.Ordinal);
        if (signatureRequired && string.IsNullOrWhiteSpace(integrity.Signature))
        {
            errors.Add($"{DiagnosticCodes.IntegritySignatureRequired} a signature declaration is required for Stable package or distribution policy.");
        }
    }

    private static void ValidateRollback(
        ThemeRollbackDeclaration? rollback,
        ThemePersonalizationDeclaration? personalization,
        List<string> errors)
    {
        if (rollback is null)
        {
            return;
        }

        ValidateReference(
            rollback.RollbackManifest,
            "rollback.rollback_manifest",
            DiagnosticCodes.RollbackManifestInvalid,
            errors);
        ValidateVersionRanges(rollback.PreviousVersionCompatibility, errors);

        if (rollback.StateMigrationSupported && personalization is null)
        {
            errors.Add($"{DiagnosticCodes.RollbackMigrationInvalid} rollback cannot declare state migration support without a personalization migration declaration.");
        }
    }

    private static void ValidateReference(
        string reference,
        string field,
        string code,
        List<string> errors)
    {
        if (!IsSafeDeclarationReference(reference))
        {
            errors.Add($"{code} {field} reference is invalid.");
        }
    }

    private static void ValidateReferenceList(
        IReadOnlyList<string> references,
        string field,
        string code,
        bool allowEmpty,
        List<string> errors)
    {
        if (references is null || (!allowEmpty && references.Count == 0))
        {
            errors.Add($"{code} {field} is required.");
            return;
        }

        HashSet<string> seen = new(StringComparer.Ordinal);
        for (int index = 0; index < references.Count; index++)
        {
            string? reference = references[index];
            if (!IsSafeDeclarationReference(reference) || !seen.Add(reference!))
            {
                errors.Add($"{code} {field} contains an invalid or duplicate reference at index {index}.");
            }
        }
    }

    private static void ValidateUniqueNonBlankValues(
        IReadOnlyList<string> values,
        string field,
        string code,
        List<string> errors)
    {
        if (values is null)
        {
            errors.Add($"{code} {field} collection is required.");
            return;
        }

        HashSet<string> seen = new(StringComparer.Ordinal);
        for (int index = 0; index < values.Count; index++)
        {
            string? value = values[index];
            if (string.IsNullOrWhiteSpace(value) || !seen.Add(value))
            {
                errors.Add($"{code} {field} value at index {index} is blank or duplicated.");
            }
        }
    }

    private static void ValidateVersionRanges(
        IReadOnlyList<string> ranges,
        List<string> errors)
    {
        if (ranges is null)
        {
            errors.Add($"{DiagnosticCodes.RollbackVersionCollectionRequired} rollback previous_version_compatibility collection is required.");
            return;
        }

        HashSet<string> seen = new(StringComparer.Ordinal);
        for (int index = 0; index < ranges.Count; index++)
        {
            string? range = ranges[index];
            if (!IsVersionRangeDeclaration(range) || !seen.Add(range!))
            {
                errors.Add($"{DiagnosticCodes.RollbackVersionEntryInvalid} rollback previous_version_compatibility value at index {index} is malformed or duplicated.");
            }
        }
    }

    private static void ValidateExactSet(
        IReadOnlyList<string> actual,
        string[] required,
        string code,
        string field,
        List<string> errors)
    {
        if (actual is null
            || actual.Count != required.Length
            || HasDuplicates(actual)
            || required.Any(requiredValue => !actual.Contains(requiredValue, StringComparer.Ordinal)))
        {
            errors.Add($"{code} {field} must contain the complete approved set exactly once.");
        }
    }

    private static void ValidateAllowedSet(
        IReadOnlyList<string> actual,
        IReadOnlyList<string> allowed,
        string code,
        string field,
        List<string> errors)
    {
        if (actual is null
            || actual.Count == 0
            || HasDuplicates(actual)
            || actual.Any(value => !allowed.Contains(value, StringComparer.Ordinal)))
        {
            errors.Add($"{code} {field} must contain unique approved values.");
        }
    }

    private static bool HasDuplicates(IReadOnlyList<string> values) =>
        values.Count != new HashSet<string>(values, StringComparer.Ordinal).Count;

    private static bool IsDottedIdentifier(string? value, bool requireSeparator)
    {
        if (string.IsNullOrWhiteSpace(value)
            || (requireSeparator && !value.Contains('.', StringComparison.Ordinal)))
        {
            return false;
        }

        string[] segments = value.Split(['.', '-'], StringSplitOptions.None);
        return segments.All(segment => segment.Length > 0 && segment.All(IsLowerAlphaNumeric));
    }

    private static bool IsSimpleIdentifier(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !IsLowerAlphaNumeric(value[0]))
        {
            return false;
        }

        return value.All(character =>
            IsLowerAlphaNumeric(character) || character is '.' or '_' or '-');
    }

    private static bool IsPresentationIdentifier(string? value) =>
        value is not null
        && value.StartsWith("presentation.", StringComparison.Ordinal)
        && IsSimpleIdentifier(value);

    private static bool IsLowerAlphaNumeric(char value) =>
        value is >= 'a' and <= 'z' or >= '0' and <= '9';

    private static bool IsSemanticVersion(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string[] parts = value.Split('.', StringSplitOptions.None);
        return parts.Length == 3 && parts.All(IsCanonicalNumericPart);
    }

    private static bool IsCanonicalNumericPart(string value) =>
        value.Length > 0
        && (value.Length == 1 || value[0] != '0')
        && value.All(character => character is >= '0' and <= '9');

    private static bool IsVersionRangeDeclaration(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string[] clauses = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return clauses.Length > 0 && clauses.All(IsVersionRangeClause);
    }

    private static bool IsVersionRangeClause(string clause)
    {
        string version = clause.StartsWith(">=", StringComparison.Ordinal)
                         || clause.StartsWith("<=", StringComparison.Ordinal)
            ? clause[2..]
            : clause.StartsWith('>') || clause.StartsWith('<') || clause.StartsWith('=')
                ? clause[1..]
                : clause;
        return IsSemanticVersion(version);
    }

    private static bool IsSafeDeclarationReference(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Contains('\\', StringComparison.Ordinal)
            || value.Contains(':', StringComparison.Ordinal)
            || value.StartsWith('/')
            || value.EndsWith('/'))
        {
            return false;
        }

        string[] segments = value.Split('/', StringSplitOptions.None);
        return segments.All(segment => segment.Length > 0 && segment is not "." and not "..");
    }

    private static class DiagnosticCodes
    {
        public const string ManifestRequired = "P3M000";
        public const string ContextRequired = "P3M001";
        public const string SchemaUnsupported = "P3M002";
        public const string ThemeApiUnsupported = "P3M003";

        public const string PackageRequired = "P3M010";
        public const string ThemeIdMalformed = "P3M011";
        public const string PackageIdMalformed = "P3M012";
        public const string PackageNameRequired = "P3M013";
        public const string PackagePublisherRequired = "P3M014";
        public const string PublisherIdMalformed = "P3M015";
        public const string PackageVersionInvalid = "P3M016";
        public const string PackageChannelInvalid = "P3M017";
        public const string PackageDescriptionRequired = "P3M018";
        public const string PackageLicenseRequired = "P3M019";
        public const string PackageHomepageBlank = "P3M020";
        public const string PackageSupportUrlBlank = "P3M021";
        public const string PackageTagInvalid = "P3M022";

        public const string CompatibilityRequired = "P3M030";
        public const string CompatibilityVersionMalformed = "P3M031";
        public const string CompatibilityPlatformsInvalid = "P3M032";
        public const string CompatibilityPortableRequired = "P3M033";
        public const string CompatibilityDpiInvalid = "P3M034";

        public const string VariantCollectionRequired = "P3M040";
        public const string VariantEntryRequired = "P3M041";
        public const string VariantIdMalformed = "P3M042";
        public const string VariantIdDuplicate = "P3M043";
        public const string VariantTypeInvalid = "P3M044";
        public const string VariantDisplayNameRequired = "P3M045";
        public const string VariantTokenReferenceInvalid = "P3M046";
        public const string VariantAccessibilityOverrideInvalid = "P3M047";
        public const string VariantDefaultCountInvalid = "P3M048";
        public const string VariantSelfFallback = "P3M049";
        public const string VariantFallbackMissing = "P3M050";
        public const string VariantFallbackCycle = "P3M051";

        public const string CapabilityCollectionRequired = "P3M060";
        public const string CapabilityEntryRequired = "P3M061";
        public const string CapabilityDuplicate = "P3M062";
        public const string CapabilityForbidden = "P3M063";
        public const string CapabilityUnknown = "P3M064";

        public const string FeatureFlagCollectionRequired = "P3M070";
        public const string FeatureFlagEntryRequired = "P3M071";
        public const string FeatureFlagIdInvalid = "P3M072";
        public const string FeatureFlagIdDuplicate = "P3M073";
        public const string FeatureFlagDescriptionRequired = "P3M074";
        public const string FeatureFlagSafetyGuardMissing = "P3M075";
        public const string FeatureFlagSafetyGuardsInvalid = "P3M076";

        public const string DegradedModeRequired = "P3M080";
        public const string DegradedModeSupportedRequired = "P3M081";
        public const string DegradedModeLevelsInvalid = "P3M082";
        public const string DegradedModeFallbackInvalid = "P3M083";
        public const string DegradedModePreservesInvalid = "P3M084";
        public const string DegradedModePreservationMissing = "P3M085";

        public const string AccessibilityRequired = "P3M090";
        public const string AccessibilityTextScalingRequired = "P3M091";
        public const string AccessibilityZoomRequired = "P3M092";
        public const string AccessibilityContrastRequired = "P3M093";
        public const string AccessibilityColorVisionRequired = "P3M094";
        public const string AccessibilityReducedMotionRequired = "P3M095";
        public const string AccessibilityReducedTransparencyRequired = "P3M096";
        public const string AccessibilitySoundControlsRequired = "P3M097";
        public const string AccessibilityKeyboardFocusRequired = "P3M098";
        public const string AccessibilityFullKeyboardRequired = "P3M099";
        public const string AccessibilityScreenReaderRequired = "P3M100";
        public const string AccessibilityCriticalAlertRedundancyRequired = "P3M101";

        public const string AssetsRequired = "P3M110";
        public const string AssetsInventoryInvalid = "P3M111";
        public const string AssetsTiersInvalid = "P3M112";
        public const string AssetsSizeInvalid = "P3M113";
        public const string AssetsHashInvalid = "P3M114";

        public const string AudioSoundPackInvalid = "P3M120";
        public const string AudioBgmInvalid = "P3M121";
        public const string AudioAmbientInvalid = "P3M122";
        public const string AudioOptionalRequired = "P3M123";
        public const string AudioOneClickDisableRequired = "P3M124";
        public const string AudioNightModeRequired = "P3M125";
        public const string AudioDuckingRequired = "P3M126";

        public const string MotionRequired = "P3M130";
        public const string MotionProfilesInvalid = "P3M131";
        public const string MotionReducedMotionInvalid = "P3M132";
        public const string MotionInterruptibleRequired = "P3M133";
        public const string MotionSkippableRequired = "P3M134";
        public const string MotionParallaxFallbackRequired = "P3M135";

        public const string PersonalizationPresetsInvalid = "P3M140";
        public const string PersonalizationSafeRangesInvalid = "P3M141";
        public const string PersonalizationMigrationsInvalid = "P3M142";
        public const string PersonalizationResetRequired = "P3M143";

        public const string IntegrityRequired = "P3M150";
        public const string IntegrityManifestInvalid = "P3M151";
        public const string IntegritySignatureInvalid = "P3M152";
        public const string IntegritySignaturePolicyInvalid = "P3M153";
        public const string IntegrityHashInvalid = "P3M154";
        public const string IntegritySignatureRequired = "P3M155";

        public const string RollbackManifestInvalid = "P3M160";
        public const string RollbackVersionCollectionRequired = "P3M161";
        public const string RollbackVersionEntryInvalid = "P3M162";
        public const string RollbackMigrationInvalid = "P3M163";
    }

    private static bool IsOneOf(string? value, params string[] allowed) =>
        value is not null && allowed.Contains(value, StringComparer.Ordinal);

    private static string Display(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "<blank>" : value;
}
