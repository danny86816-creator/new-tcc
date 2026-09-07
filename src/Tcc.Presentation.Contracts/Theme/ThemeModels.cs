using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tcc.Presentation.Contracts.Theme;

public sealed record ThemeManifest(
    [property: JsonPropertyName("schema_version")] string SchemaVersion,
    [property: JsonPropertyName("theme_api_version")] string ThemeApiVersion,
    [property: JsonPropertyName("package")] ThemePackageIdentity Package,
    [property: JsonPropertyName("compatibility")] ThemeCompatibilityDeclaration Compatibility,
    [property: JsonPropertyName("variants")] IReadOnlyList<ThemeVariant> Variants,
    [property: JsonPropertyName("capabilities")] IReadOnlyList<string> Capabilities,
    [property: JsonPropertyName("feature_flags")] ThemeFeatureFlags FeatureFlags,
    [property: JsonPropertyName("degraded_mode")] ThemeDegradedModeDeclaration DegradedMode,
    [property: JsonPropertyName("accessibility")] ThemeAccessibilityDeclaration Accessibility,
    [property: JsonPropertyName("assets")] ThemeAssetDeclaration Assets,
    [property: JsonPropertyName("audio")] ThemeAudioDeclaration? Audio,
    [property: JsonPropertyName("motion")] ThemeMotionDeclaration Motion,
    [property: JsonPropertyName("personalization")] ThemePersonalizationDeclaration? Personalization,
    [property: JsonPropertyName("integrity")] ThemeIntegrityDeclaration Integrity,
    [property: JsonPropertyName("rollback")] ThemeRollbackDeclaration? Rollback);

public sealed record ThemePackageIdentity(
    [property: JsonPropertyName("theme_id")] string ThemeId,
    [property: JsonPropertyName("package_id")] string PackageId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("publisher")] string Publisher,
    [property: JsonPropertyName("publisher_id")] string PublisherId,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("channel")] string Channel,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("homepage")] string? Homepage,
    [property: JsonPropertyName("support_url")] string? SupportUrl,
    [property: JsonPropertyName("license")] string License,
    [property: JsonPropertyName("tags")] IReadOnlyList<string> Tags);

public sealed record ThemeCompatibilityDeclaration(
    [property: JsonPropertyName("required_core_version")] string RequiredCoreVersion,
    [property: JsonPropertyName("required_theme_api_version")] string RequiredThemeApiVersion,
    [property: JsonPropertyName("required_ux_contract_version")] string RequiredUxContractVersion,
    [property: JsonPropertyName("supported_platforms")] IReadOnlyList<string> SupportedPlatforms,
    [property: JsonPropertyName("portable_supported")] bool PortableSupported,
    [property: JsonPropertyName("minimum_dpi_scale")] decimal MinimumDpiScale,
    [property: JsonPropertyName("maximum_tested_dpi_scale")] decimal MaximumTestedDpiScale);

public sealed record ThemeVariant(
    [property: JsonPropertyName("variant_id")] string VariantId,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("display_name")] string DisplayName,
    [property: JsonPropertyName("default")] bool Default,
    [property: JsonPropertyName("token_file")] string TokenBundleRef,
    [property: JsonPropertyName("accessibility_overrides")] IReadOnlyList<string> AccessibilityOverrides,
    [property: JsonPropertyName("fallback_variant_id")] string? FallbackVariantId);

public sealed record ThemeAccessibilityDeclaration(
    [property: JsonPropertyName("q93_compliant_claim")] bool Q93CompliantClaim,
    [property: JsonPropertyName("text_scaling_supported")] bool TextScalingSupported,
    [property: JsonPropertyName("zoom_supported")] bool ZoomSupported,
    [property: JsonPropertyName("contrast_supported")] bool ContrastSupported,
    [property: JsonPropertyName("color_vision_modes_supported")] bool ColorVisionModesSupported,
    [property: JsonPropertyName("reduced_motion_supported")] bool ReducedMotionSupported,
    [property: JsonPropertyName("reduced_transparency_supported")] bool ReducedTransparencySupported,
    [property: JsonPropertyName("sound_controls_supported")] bool SoundControlsSupported,
    [property: JsonPropertyName("keyboard_focus_visible")] bool KeyboardFocusVisible,
    [property: JsonPropertyName("full_keyboard_operation")] bool FullKeyboardOperation,
    [property: JsonPropertyName("screen_reader_labels_preserved")] bool ScreenReaderLabelsPreserved,
    [property: JsonPropertyName("critical_alert_redundancy_supported")] bool CriticalAlertRedundancySupported);

public sealed record ThemeAssetDeclaration(
    [property: JsonPropertyName("inventory")] string Inventory,
    [property: JsonPropertyName("tiers")] IReadOnlyList<string> Tiers,
    [property: JsonPropertyName("total_declared_size_bytes")] long TotalDeclaredSizeBytes,
    [property: JsonPropertyName("hash_algorithm")] string HashAlgorithm);

public sealed record ThemeFeatureFlags(
    [property: JsonPropertyName("presentation_only")] IReadOnlyList<ThemePresentationFeatureFlag> PresentationOnly);

public sealed record ThemePresentationFeatureFlag(
    [property: JsonPropertyName("flag_id")] string FlagId,
    [property: JsonPropertyName("default_enabled")] bool DefaultEnabled,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("cannot_affect")] IReadOnlyList<string> CannotAffect);

public sealed record ThemeDegradedModeDeclaration(
    [property: JsonPropertyName("supported")] bool Supported,
    [property: JsonPropertyName("levels")] IReadOnlyList<string> Levels,
    [property: JsonPropertyName("preserves")] IReadOnlyList<string> Preserves,
    [property: JsonPropertyName("default_fallback_level")] string DefaultFallbackLevel);

public sealed record ThemeAudioDeclaration(
    [property: JsonPropertyName("sound_pack")] string SoundPack,
    [property: JsonPropertyName("bgm")] string Bgm,
    [property: JsonPropertyName("ambient")] string Ambient,
    [property: JsonPropertyName("optional")] bool Optional,
    [property: JsonPropertyName("one_click_disable_supported")] bool OneClickDisableSupported,
    [property: JsonPropertyName("night_mode_supported")] bool NightModeSupported,
    [property: JsonPropertyName("ducking_supported")] bool DuckingSupported);

public sealed record ThemeMotionDeclaration(
    [property: JsonPropertyName("profiles")] string Profiles,
    [property: JsonPropertyName("reduced_motion")] string ReducedMotion,
    [property: JsonPropertyName("interruptible_transitions")] bool InterruptibleTransitions,
    [property: JsonPropertyName("skippable_transitions")] bool SkippableTransitions,
    [property: JsonPropertyName("parallax_static_fallback")] bool ParallaxStaticFallback);

public sealed record ThemePersonalizationDeclaration(
    [property: JsonPropertyName("presets")] string Presets,
    [property: JsonPropertyName("safe_ranges")] string SafeRanges,
    [property: JsonPropertyName("migrations")] string Migrations,
    [property: JsonPropertyName("reset_supported")] bool ResetSupported);

public sealed record ThemeIntegrityDeclaration(
    [property: JsonPropertyName("integrity_manifest")] string IntegrityManifest,
    [property: JsonPropertyName("signature")] string? Signature,
    [property: JsonPropertyName("signature_required_for_channel")] string SignatureRequiredForChannel,
    [property: JsonPropertyName("hash_algorithm")] string HashAlgorithm);

public sealed record ThemeRollbackDeclaration(
    [property: JsonPropertyName("rollback_manifest")] string RollbackManifest,
    [property: JsonPropertyName("state_migration_supported")] bool StateMigrationSupported,
    [property: JsonPropertyName("previous_version_compatibility")] IReadOnlyList<string> PreviousVersionCompatibility);

public sealed record ThemeIntegrityManifest(
    string SchemaVersion,
    string ThemeId,
    string Version,
    string HashAlgorithm,
    IReadOnlyList<ThemeIntegrityFile> Files,
    string PackageHash,
    DateTimeOffset CreatedAt);

public sealed record ThemeIntegrityFile(string Path, string Sha256, long SizeBytes, bool Required);

public sealed record ThemeCompatibilityManifest(
    string SchemaVersion,
    string ThemeId,
    string Version,
    ThemeVersionRange Core,
    ThemeVersionRange ThemeApi,
    ThemeVersionRange UxContract,
    ThemeWindowsCompatibility Windows,
    IReadOnlyDictionary<string, string> AccessibilityMatrix);

public sealed record ThemeVersionRange(string Required, IReadOnlyList<string> Tested);

public sealed record ThemeWindowsCompatibility(
    bool InstallerSupported,
    bool PortableSupported,
    bool MultiMonitorSupported,
    IReadOnlyList<string> DpiRangesTested);

public sealed record ThemeRollbackManifest(
    string SchemaVersion,
    string ThemeId,
    string Version,
    bool RollbackSupported,
    IReadOnlyList<ThemeRollbackTarget> RollbackTargets,
    string NonMigratableStatePolicy);

public sealed record ThemeRollbackTarget(
    string FromVersion,
    string ToVersionRange,
    string StateMigration,
    IReadOnlyList<string> CacheInvalidation);

public sealed record ThemeAssetInventory(
    string SchemaVersion,
    string ThemeId,
    string Version,
    IReadOnlyList<ThemeAssetEntry> Assets,
    ThemeAssetBudgets AssetBudgets);

public sealed record ThemeAssetEntry(
    string AssetId,
    string Path,
    string Type,
    ThemeAssetTier Tier,
    IReadOnlyList<string> VariantScope,
    IReadOnlyList<string> SurfaceScope,
    bool Required,
    string FallbackAssetId,
    string Sha256,
    long SizeBytes,
    ThemeAssetDimensions? Dimensions,
    string DpiPolicy,
    string LoadPolicy,
    string DisposePolicy);

public sealed record ThemeAssetDimensions(int Width, int Height);

public sealed record ThemeAssetBudgets(
    long MaxTotalPackageBytes,
    long MaxExtractedPackageBytes,
    long MaxTier0Bytes,
    long MaxTier0DecodedMemoryBytes,
    long MaxTier1Bytes,
    long MaxTier2Bytes,
    long MaxTier3Bytes,
    long MaxAudioTotalBytes,
    long MaxBgmTotalBytes,
    long MaxPreviewAssetsBytes,
    long MaxSingleAssetBytes,
    int MaxSingleAssetWidthPx,
    int MaxSingleAssetHeightPx,
    long MaxSingleAssetDecodedMemoryBytes,
    long MaxGpuTextureMemoryBytes,
    long MaxCacheBytesPerTheme,
    long MaxCacheBytesGlobal);

public sealed record ThemeTokenBundle(
    string SchemaVersion,
    IReadOnlyDictionary<string, ThemeTokenValue> Tokens,
    IReadOnlyDictionary<string, ThemeSemanticBinding> SemanticBindings);

public sealed record ThemeTokenValue(string Type, JsonElement Value);

public sealed record ThemeSemanticBinding(
    string Token,
    bool RequiresTextLabel,
    bool RequiresIconOrStructure);

public sealed record ThemeLayoutAdapter(
    string SchemaVersion,
    string SurfaceId,
    IReadOnlyList<ThemeZonePlacement> Zones,
    ThemeKeyboardAndFocusContract KeyboardAndFocus);

public sealed record ThemeZonePlacement(
    string ZoneId,
    string Placement,
    string Visibility,
    string Rearrangeability,
    bool CollapseAllowed);

public sealed record ThemeComponentAdapter(
    string SchemaVersion,
    string ComponentId,
    IReadOnlyList<string> SupportedSurfaceIds,
    IReadOnlyList<string> PreservedSemantics,
    ThemeKeyboardAndFocusContract KeyboardAndFocus);

public sealed record ThemeKeyboardAndFocusContract(
    string FocusOrderPolicy,
    string KeyboardActivationPolicy,
    bool ModalFocusTrapPreserved,
    string FocusRestorationTarget,
    string ShortcutConflictPolicy,
    string CustomCompositeKeyboardPattern,
    string HiddenElementFocusPolicy,
    bool PreviewKeyboardExitRequired,
    string EscapeCancelBehavior);

public sealed record ThemeCopyResources(
    string SchemaVersion,
    string Locale,
    IReadOnlyDictionary<string, string> NonCriticalResources,
    IReadOnlyDictionary<string, string> ApprovedCriticalFallbacks);

public sealed record ThemeCriticalCopyRules(
    string SchemaVersion,
    IReadOnlyList<string> CoreOwnedKeys,
    IReadOnlyList<string> PlainLanguageRequiredCategories,
    bool RejectAmbiguousCriticalCopy,
    bool RejectMissingCriticalFallback);

public sealed record ThemeFocusStyles(
    string SchemaVersion,
    decimal MinimumContrastRatio,
    bool VisibleOnAllInteractiveControls,
    bool ProgrammaticOrderPreserved,
    bool ModalFocusTrapPreserved,
    bool FocusRestorationRequired);

public sealed record ThemeStatePresentation(
    string SchemaVersion,
    string StateId,
    ThemeRequiredSemantics RequiredSemantics,
    ThemeRequiredRedundancy RequiredRedundancy,
    ThemeStateFreedom ThemeFreedom);

public sealed record ThemeRequiredSemantics(
    string Meaning,
    bool MustShowReason,
    bool MustShowExitOrResolutionPath,
    bool MayOfferCorrectiveAction);

public sealed record ThemeRequiredRedundancy(
    bool Text,
    bool IconOrStructure,
    bool ProgrammaticState,
    bool ScreenReaderAnnouncement,
    bool ColorOnlyForbidden,
    bool MotionOnlyForbidden,
    bool SoundOnlyForbidden,
    bool TransparencyOnlyForbidden);

public sealed record ThemeStateFreedom(string Color, string Animation, string Sound, string Metaphor);

public sealed record CopyResourceBundle(
    string Locale,
    IReadOnlyDictionary<string, string> Resources,
    IReadOnlyList<string> CoreOwnedKeys);

public sealed record CopyFallbackResult(
    string SchemaVersion,
    string Key,
    string Locale,
    string Value,
    string Source,
    bool IsCritical,
    bool UsedCoreFallback,
    bool IsValid);

public sealed record ThemeMotionProfile(
    string SchemaVersion,
    IReadOnlyList<ThemeMotionProfileEntry> Profiles,
    ThemeReducedMotionProfile ReducedMotion);

public sealed record ThemeMotionProfileEntry(
    string ProfileId,
    string Intensity,
    bool TransitionsInterruptible,
    bool TransitionsSkippable,
    bool ParallaxEnabled,
    string ParallaxReducedFallback,
    bool CriticalAlertMotionSupplementaryOnly,
    bool LossCelebrationForbidden);

public sealed record ThemeReducedMotionProfile(
    bool DisableNonessentialMotion,
    string ReplaceTransitionsWith,
    bool PreserveStateTextually,
    bool DisableParallax,
    bool DisableLoopingDecorativeMotion);

public sealed record ThemeAudioSoundPack(
    string SchemaVersion,
    string SoundPackId,
    string Version,
    ThemeSoundControls Controls,
    IReadOnlyList<ThemeSoundEvent> Events,
    IReadOnlyList<ThemeBgmTrack> Bgm,
    IReadOnlyList<ThemeAmbientTrack> Ambient);

public sealed record ThemeSoundControls(
    bool OneClickDisable,
    bool IndependentBgm,
    bool IndependentAmbient,
    bool IndependentUi,
    bool IndependentAlertSupplement,
    bool NightMode,
    bool Ducking);

public sealed record ThemeSoundEvent(
    string EventId,
    string AssetId,
    string Priority,
    bool SupplementaryOnly,
    bool DuckBgm,
    bool RespectsSoundDisabled,
    bool RequiresVisualAndTextAlert,
    bool Celebratory);

public sealed record ThemeBgmTrack(
    string TrackId,
    string Path,
    bool Loop,
    bool Contextual,
    bool RespectsOneClickDisable,
    bool RespectsNightMode);

public sealed record ThemeAmbientTrack(
    string AmbientId,
    string Path,
    bool Loop,
    bool Contextual,
    IReadOnlyList<string> SurfaceScope,
    bool RespectsOneClickDisable,
    bool RespectsNightMode,
    bool Duckable,
    decimal DefaultVolume);

public sealed record ThemePersonalizationSafeRanges(
    string SchemaVersion,
    string ThemeId,
    IReadOnlyList<ThemePersonalizationControl> Controls);

public sealed record ThemePersonalizationControl(
    string ControlId,
    string DisplayName,
    string Type,
    JsonElement Default,
    decimal? Min,
    decimal? Max,
    decimal? Step,
    IReadOnlyDictionary<string, JsonElement> AccessibilityOverrides,
    IReadOnlyList<string> CannotAffect);

public sealed record ThemeRuntimeState(
    string SchemaVersion,
    string StateId,
    string UserId,
    string DeviceId,
    string ThemeId,
    string ThemeVersion,
    string VariantId,
    ThemeWorkspaceState? WorkspaceState,
    ThemePersonalizationState Personalization,
    ThemeAudioState Audio,
    ThemeMotionState Motion,
    ThemeAutomationState Automation,
    DateTimeOffset UpdatedAt);

public sealed record ThemeWorkspaceState(
    string WorkspaceId,
    string LayoutRef,
    string MonitorBinding,
    ThemeWindowBounds WindowBounds);

public sealed record ThemeWindowBounds(decimal X, decimal Y, decimal Width, decimal Height, decimal DpiScale);

public sealed record ThemePersonalizationState(
    string PresetId,
    IReadOnlyDictionary<string, JsonElement> Values);

public sealed record ThemeAudioCategoryState(bool Enabled, decimal Volume);

public sealed record ThemeAudioState(
    bool MasterThemeAudioEnabled,
    decimal MasterThemeAudioVolume,
    bool BgmEnabled,
    decimal BgmVolume,
    bool AmbientEnabled,
    decimal AmbientVolume,
    bool UiSoundEnabled,
    decimal UiSoundVolume,
    bool AlertSupplementEnabled,
    decimal AlertSupplementVolume,
    bool NightMode,
    bool DuckingEnabled);

public sealed record ThemePreviewAudioState(
    string PreviewSessionId,
    bool IsolatedFromActiveThemeState,
    ThemeAudioState State);

public sealed record ThemeMotionState(decimal Intensity, bool ParallaxEnabled);

public sealed record ThemeAutomationState(
    bool TimeOfDaySceneEnabled,
    bool TradingStateOverlayEnabled,
    string AutomationPinnedOrDisabled);

public sealed record ThemeCompatibilityFailure(ThemeFailureReason Reason, string Message, string? Remediation);

public sealed record ThemeCompatibilityResult(
    ThemeCompatibilityStatus Status,
    string SelectedCoreVersion,
    string SelectedThemeApiVersion,
    string SelectedUxContractVersion,
    string SelectedManifestSchemaVersion,
    IReadOnlyList<string> EnabledCapabilities,
    IReadOnlyList<string> DisabledCapabilities,
    IReadOnlyList<string> DegradationReasons,
    IReadOnlyList<string> BlockedReasons,
    IReadOnlyList<ThemeCompatibilityFailure> FailureReasons,
    bool MigrationRequired,
    bool RollbackRequiredOrAvailable,
    ThemeAccessibilityStatus AccessibilityValidationStatus,
    string SafetyInvariantsStatus);

public sealed record ThemeInstallResult(ThemeOperationOutcome Outcome, ThemeId ThemeId, IReadOnlyList<string> DiagnosticsRefs);

public sealed record ThemeApplyOptions(
    ThemeId ThemeId,
    ThemeVariantId? VariantId,
    bool PreviewFirst,
    bool PreserveWorkspaceState,
    string AccessibilityProfileId,
    ThemeCorrelationId CorrelationId);

public sealed record ThemeApplyResult(
    ThemeApplyOutcome Result,
    ThemeId ActiveThemeId,
    ThemeVariantId? ActiveVariantId,
    ThemeCompatibilityStatus CompatibilityStatus,
    ThemeAccessibilityStatus AccessibilityStatus,
    IReadOnlyList<string> DiagnosticsRefs,
    bool RollbackAvailable);

public sealed record ThemeRollbackResult(
    ThemeOperationOutcome Outcome,
    ThemeId ActiveThemeId,
    ThemeVersion ActiveVersion,
    IReadOnlyList<string> DiagnosticsRefs);

public sealed record ThemeDiagnosticsPrivacy(
    bool ContainsTradingData,
    bool ContainsPrivateData,
    bool ContainsTeamData);

public sealed record ThemeDiagnosticsEvent(
    string SchemaVersion,
    string EventId,
    string EventType,
    ThemeId ThemeId,
    ThemeVersion ThemeVersion,
    ThemeVariantId? VariantId,
    UxSurfaceId? SurfaceId,
    ThemeDiagnosticsSeverity Severity,
    string Message,
    ThemeAssetId? AssetId,
    ThemeCorrelationId CorrelationId,
    DateTimeOffset Timestamp,
    ThemeDiagnosticsPrivacy Privacy);

public sealed record ThemeCapabilityDecision(
    string Capability,
    ThemeCapabilityDecisionResult Result,
    string Reason,
    string? RequiredUserNotice);

public sealed record ThemeAccessibilityContract(
    bool SupportsTextScaling,
    bool SupportsZoom,
    bool SupportsContrastModes,
    bool SupportsColorVisionModes,
    bool SupportsReducedMotion,
    bool SupportsReducedTransparency,
    bool SupportsSoundControls,
    bool PreservesKeyboardFocusVisibility,
    bool PreservesFullKeyboardOperation,
    bool PreservesScreenReaderLabels,
    bool PreservesCriticalAlertRedundancy);

public sealed record ThemeAccessibilityValidationResult(
    ThemeAccessibilityStatus Status,
    IReadOnlyList<string> FailedRequirements,
    IReadOnlyList<string> DiagnosticsRefs);
