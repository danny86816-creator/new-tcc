namespace Tcc.Presentation.Contracts.Theme;

public interface IThemePackage
{
    ThemeManifest GetManifest();

    ThemeTokenBundle GetPresentationTokens(ThemeTokenRequest request);

    ThemeTokenBundle GetVariantTokens(ThemeVariantId variantId);

    IReadOnlyList<ThemeLayoutAdapter> GetLayoutAdapters(ThemeLayoutAdapterRequest request);

    IReadOnlyList<ThemeComponentAdapter> GetComponentAdapters(ThemeComponentAdapterRequest request);

    ThemeMotionProfile GetMotionProfile(ThemeMotionProfileRequest request);

    ThemeAudioSoundPack GetSoundPacks(ThemeSoundPackRequest request);

    CopyResourceBundle GetCopyResources(ThemeCopyResourceRequest request);

    ThemeAssetInventory GetAssetIndex(ThemeAssetIndexRequest request);
}

public interface IThemeRuntime
{
    ValueTask<ThemeLoadResult> LoadThemeAsync(
        ThemeId themeId,
        ThemeLoadOptions options,
        CancellationToken cancellationToken = default);

    ValueTask<ThemePreviewSession> PreviewThemeAsync(
        ThemeId themeId,
        ThemePreviewOptions options,
        CancellationToken cancellationToken = default);

    ValueTask<ThemeApplyResult> ApplyThemeAsync(
        ThemeApplyOptions options,
        CancellationToken cancellationToken = default);

    ValueTask<ThemeRollbackResult> RollbackThemeAsync(
        ThemeRollbackRequest request,
        CancellationToken cancellationToken = default);

    ValueTask<ThemeDisableResult> DisableThemeAsync(
        ThemeId themeId,
        ThemeDisableReason reason,
        CancellationToken cancellationToken = default);

    ThemeTokenBundle ResolveTokens(ThemeResolutionContext context);

    ThemeSurfacePresentation ResolveSurfacePresentation(
        UxSurfaceId surfaceId,
        ThemeResolutionContext context);

    ValueTask<ThemeDisposalResult> DisposeThemeAsync(
        ThemeId themeId,
        CancellationToken cancellationToken = default);
}

public interface IThemeManifestValidator
{
    ThemeManifestValidationResult Validate(
        ThemeManifest manifest,
        ThemeManifestValidationContext context);
}

public interface IThemeIntegrityVerifier
{
    ValueTask<ThemeIntegrityVerificationResult> VerifyAsync(
        ThemeIntegrityVerificationRequest request,
        CancellationToken cancellationToken = default);
}

public interface IThemeCompatibilityResolver
{
    ThemeCompatibilityResult Resolve(ThemeCompatibilityRequest request);
}

public interface IThemeCapabilityGate
{
    IReadOnlyList<ThemeCapabilityDecision> Evaluate(ThemeCapabilityRequest request);
}

public interface IThemeAssetLoader
{
    ValueTask<ThemeAssetLoadResult> LoadTierAsync(
        ThemeId themeId,
        ThemeAssetTier tier,
        ThemeAssetLoadContext context,
        CancellationToken cancellationToken = default);

    ValueTask<ThemeResolvedAsset> LoadAssetAsync(
        ThemeAssetId assetId,
        ThemeAssetLoadPolicy policy,
        CancellationToken cancellationToken = default);

    ValueTask<ThemeAssetPreloadResult> PreloadAssetsAsync(
        ThemeAssetPreloadRequest request,
        CancellationToken cancellationToken = default);

    ValueTask<ThemeAssetUnloadResult> UnloadAssetsAsync(
        ThemeAssetUnloadRequest request,
        CancellationToken cancellationToken = default);
}

public interface IThemeAssetCache
{
    ValueTask<ThemeCacheLookupResult> TryGetAsync(
        ThemeCacheKey key,
        CancellationToken cancellationToken = default);

    ValueTask<ThemeCacheWriteResult> StoreAsync(
        ThemeCacheWriteRequest request,
        CancellationToken cancellationToken = default);

    ValueTask<ThemeCacheClearResult> ClearThemeCacheAsync(
        ThemeId themeId,
        CancellationToken cancellationToken = default);

    ValueTask<ThemeCacheValidationResult> ValidateAsync(
        ThemeCacheValidationRequest request,
        CancellationToken cancellationToken = default);
}

public interface IThemePreviewSandbox
{
    ValueTask<ThemePreviewSession> BeginPreviewAsync(
        ThemePreviewRequest request,
        CancellationToken cancellationToken = default);

    ThemePreviewReadModel BuildReadOnlySnapshot(ThemePreviewSnapshotRequest request);

    ValueTask<ThemeApplyResult> ApplyPreviewAsync(
        ThemePreviewSessionId sessionId,
        ThemeApplyOptions options,
        CancellationToken cancellationToken = default);

    ValueTask EndPreviewAsync(
        ThemePreviewSessionId sessionId,
        ThemePreviewEndReason reason,
        CancellationToken cancellationToken = default);
}

public interface ILiveThemeSwitchCoordinator
{
    ValueTask<ThemeSwitchResult> SwitchAsync(
        ThemeSwitchRequest request,
        CancellationToken cancellationToken = default);

    ValueTask<ThemeSwitchInterruptionResult> InterruptAsync(
        ThemeCorrelationId correlationId,
        ThemeSwitchInterruptionReason reason,
        CancellationToken cancellationToken = default);
}

public interface IThemePersonalizationEngine
{
    ThemePersonalizationValidationResult Validate(
        ThemePersonalizationRequest request,
        ThemeAccessibilityProfile accessibilityProfile);

    ThemePersonalizationResult ClampToSafeRanges(
        ThemePersonalizationRequest request,
        ThemeAccessibilityProfile accessibilityProfile);

    ThemePersonalizationResetResult Reset(ThemePersonalizationResetRequest request);
}

public interface IThemeMotionManager
{
    ThemeMotionSpec ResolveMotion(
        UxSurfaceId surfaceId,
        GlobalStateId stateId,
        ThemeMotionContext context);

    ThemeMotionSpec ApplyReducedMotion(
        ThemeMotionSpec profile,
        ThemeAccessibilityProfile accessibilityProfile);

    void InterruptMotion(string motionId);

    void SkipTransition(string transitionId);
}

public interface IThemeAudioRouter
{
    ValueTask<ThemeAudioState> GetAudioStateAsync(
        ThemeAudioContext context,
        CancellationToken cancellationToken = default);

    ValueTask<ThemeAudioApplyResult> ApplyAudioStateAsync(
        ThemeAudioState state,
        ThemeAudioContext context,
        CancellationToken cancellationToken = default);

    ValueTask SetThemeAudioAsync(ThemeAudioCategoryChange change, CancellationToken cancellationToken = default);

    ValueTask SetBgmAsync(ThemeAudioCategoryChange change, CancellationToken cancellationToken = default);

    ValueTask SetAmbientAsync(ThemeAudioCategoryChange change, CancellationToken cancellationToken = default);

    ValueTask SetUiSoundAsync(ThemeAudioCategoryChange change, CancellationToken cancellationToken = default);

    ValueTask SetAlertSupplementAsync(ThemeAudioCategoryChange change, CancellationToken cancellationToken = default);

    ValueTask SetOneClickThemeAudioDisabledAsync(
        bool disabled,
        ThemeAudioContext context,
        CancellationToken cancellationToken = default);

    ValueTask ApplyNightModeAsync(
        bool enabled,
        ThemeAudioContext context,
        CancellationToken cancellationToken = default);

    ValueTask SetDuckingEnabledAsync(
        bool enabled,
        ThemeAudioContext context,
        CancellationToken cancellationToken = default);

    ValueTask<ThemeAudioPlaybackResult> StartBgmAsync(
        string trackId,
        ThemeAudioContext context,
        CancellationToken cancellationToken = default);

    ValueTask StopBgmAsync(string reason, ThemeAudioContext context, CancellationToken cancellationToken = default);

    ValueTask<ThemeAudioPlaybackResult> StartAmbientAsync(
        string ambientId,
        ThemeAudioContext context,
        CancellationToken cancellationToken = default);

    ValueTask StopAmbientAsync(string reason, ThemeAudioContext context, CancellationToken cancellationToken = default);

    ValueTask<ThemeAudioPlaybackResult> PlayUiSoundAsync(
        string eventId,
        ThemeAudioContext context,
        CancellationToken cancellationToken = default);

    ValueTask<ThemeAudioPlaybackResult> PlayAlertSupplementAsync(
        string eventId,
        ThemeAudioContext context,
        CancellationToken cancellationToken = default);

    ValueTask<ThemeAudioDuckingResult> DuckBgmAsync(
        ThemeAudioDuckingRequest request,
        CancellationToken cancellationToken = default);

    ValueTask<ThemeAudioDuckingResult> DuckAmbientAsync(
        ThemeAudioDuckingRequest request,
        CancellationToken cancellationToken = default);

    ValueTask<ThemePreviewAudioState> GetPreviewAudioStateAsync(
        ThemePreviewSessionId previewSessionId,
        CancellationToken cancellationToken = default);

    ValueTask<ThemeAudioApplyResult> ApplyPreviewAudioStateAsync(
        ThemePreviewSessionId previewSessionId,
        ThemePreviewAudioState state,
        CancellationToken cancellationToken = default);

    ValueTask<ThemeAudioPreviewResult> PreviewAudioAsync(
        ThemeAudioPreviewRequest request,
        CancellationToken cancellationToken = default);

    ValueTask StopPreviewAudioAsync(
        ThemePreviewSessionId previewSessionId,
        string reason,
        CancellationToken cancellationToken = default);
}

public interface IThemeAccessibilityValidator
{
    ThemeAccessibilityValidationResult Validate(ThemeAccessibilityValidationRequest request);
}

public interface IThemeRollbackManager
{
    ValueTask<IReadOnlyList<ThemeRollbackOption>> GetVerifiedOptionsAsync(
        ThemeId themeId,
        CancellationToken cancellationToken = default);

    ValueTask<ThemeRollbackResult> RollbackAsync(
        ThemeRollbackRequest request,
        CancellationToken cancellationToken = default);
}

public interface IThemeRecoveryHooks
{
    ThemeFailureDecision DetectThemeFailure(ThemeFailureInput input);

    ValueTask DisableThemeForSafeModeAsync(
        ThemeId themeId,
        string reason,
        CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<ThemeRollbackOption>> BuildThemeRollbackOptionsAsync(
        ThemeId themeId,
        CancellationToken cancellationToken = default);

    ValueTask ClearThemeCacheAsync(ThemeId themeId, CancellationToken cancellationToken = default);
}

public interface IThemeDiagnosticsEmitter
{
    ValueTask EmitAsync(ThemeDiagnosticsEvent diagnosticsEvent, CancellationToken cancellationToken = default);
}

public sealed record ThemeTokenRequest(ThemeId ThemeId, ThemeVariantId? VariantId, ThemeResolutionContext Context);

public sealed record ThemeLayoutAdapterRequest(ThemeId ThemeId, IReadOnlyList<UxSurfaceId> SurfaceIds);

public sealed record ThemeComponentAdapterRequest(ThemeId ThemeId, IReadOnlyList<UxSurfaceId> SurfaceIds);

public sealed record ThemeMotionProfileRequest(ThemeId ThemeId, ThemeVariantId? VariantId);

public sealed record ThemeSoundPackRequest(ThemeId ThemeId, ThemeVariantId? VariantId);

public sealed record ThemeCopyResourceRequest(ThemeId ThemeId, string Locale);

public sealed record ThemeAssetIndexRequest(ThemeId ThemeId, ThemeVersion Version);

public sealed record ThemeResolutionContext(
    ThemeId ThemeId,
    ThemeVersion Version,
    ThemeVariantId VariantId,
    string AccessibilityProfileId,
    string WorkspaceId,
    decimal DpiScale,
    IReadOnlyList<GlobalStateId> ActiveStates);

public sealed record ThemeLoadOptions(bool AllowSafeDegradation, bool IsPortableMode, ThemeCorrelationId CorrelationId);

public sealed record ThemePreviewOptions(
    ThemeVariantId? VariantId,
    bool FullScreen,
    bool AudioEnabled,
    string AccessibilityProfileId,
    ThemeCorrelationId CorrelationId);

public sealed record ThemeLoadResult(
    ThemeOperationOutcome Outcome,
    ThemeId ThemeId,
    ThemeCompatibilityResult Compatibility,
    IReadOnlyList<string> DiagnosticsRefs);

public sealed record ThemePreviewSession(
    ThemePreviewSessionId SessionId,
    ThemeId ThemeId,
    ThemeVariantId VariantId,
    bool UsesReadOnlySnapshot,
    bool IsolatedAudioState,
    DateTimeOffset StartedAt);

public sealed record ThemeRollbackRequest(
    ThemeId ThemeId,
    ThemeVersion FromVersion,
    ThemeVersion ToVersion,
    string VerifiedPackageRef,
    string VerifiedPackageHash,
    ThemeCorrelationId CorrelationId);

public sealed record ThemeDisableReason(string Code, string Description, bool SafeModeInitiated);

public sealed record ThemeDisableResult(ThemeOperationOutcome Outcome, ThemeId ThemeId, IReadOnlyList<string> DiagnosticsRefs);

public sealed record ThemeSurfacePresentation(
    UxSurfaceId SurfaceId,
    ThemeTokenBundle Tokens,
    ThemeLayoutAdapter Layout,
    IReadOnlyList<ThemeComponentAdapter> Components,
    ThemeStatePresentation StatePresentation);

public sealed record ThemeDisposalResult(ThemeOperationOutcome Outcome, ThemeId ThemeId, IReadOnlyList<string> DiagnosticsRefs);

public sealed record ThemeManifestValidationContext(
    IReadOnlySet<string> SupportedSchemaVersions,
    IReadOnlySet<string> KnownCapabilities,
    bool IsStableChannel);

public sealed record ThemeManifestValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings);

public sealed record ThemeIntegrityVerificationRequest(
    ThemeId ThemeId,
    ThemeVersion Version,
    string PackageRef,
    ThemeIntegrityManifest Manifest,
    bool SignatureRequired);

public sealed record ThemeIntegrityVerificationResult(
    bool IsVerified,
    bool SignatureVerified,
    string PackageHash,
    string ManifestHash,
    IReadOnlyList<string> Errors);

public sealed record ThemeCompatibilityRequest(
    ThemeManifest Manifest,
    ThemeCompatibilityManifest Compatibility,
    string CoreVersion,
    IReadOnlySet<string> SupportedThemeApiVersions,
    IReadOnlySet<string> SupportedUxContractVersions,
    IReadOnlySet<string> SupportedManifestSchemaVersions,
    string Platform,
    bool IsPortableMode,
    ThemeAccessibilityValidationResult AccessibilityValidation);

public sealed record ThemeCapabilityRequest(
    ThemeId ThemeId,
    IReadOnlyList<string> DeclaredCapabilities,
    IReadOnlySet<string> AllowedCapabilities);

public sealed record ThemeAssetLoadContext(
    ThemeVersion Version,
    ThemeVariantId VariantId,
    UxSurfaceId? SurfaceId,
    decimal DpiScale,
    bool IsPreview,
    bool IsPortableMode);

public sealed record ThemeAssetLoadPolicy(bool VerifyIntegrity, bool AllowFallback, bool IsSafetyCritical);

public sealed record ThemeAssetLoadResult(
    ThemeOperationOutcome Outcome,
    ThemeAssetTier Tier,
    IReadOnlyList<ThemeResolvedAsset> Assets,
    IReadOnlyList<string> DiagnosticsRefs);

public sealed record ThemeResolvedAsset(
    ThemeAssetId AssetId,
    string ResolvedRef,
    string Hash,
    bool UsedFallback,
    bool IsCoreFallback);

public sealed record ThemeAssetPreloadRequest(
    ThemeId ThemeId,
    IReadOnlyList<ThemeAssetId> AssetIds,
    ThemeAssetLoadContext Context);

public sealed record ThemeAssetPreloadResult(ThemeOperationOutcome Outcome, IReadOnlyList<ThemeAssetId> LoadedAssets);

public sealed record ThemeAssetUnloadRequest(ThemeId ThemeId, IReadOnlyList<ThemeAssetId> AssetIds, string Reason);

public sealed record ThemeAssetUnloadResult(ThemeOperationOutcome Outcome, IReadOnlyList<ThemeAssetId> ReleasedAssets);

public sealed record ThemeCacheKey(
    ThemeId ThemeId,
    ThemeVersion Version,
    ThemeVariantId? VariantId,
    ThemeAssetId AssetId,
    string Hash,
    string DpiBucket);

public sealed record ThemeCacheLookupResult(bool Found, bool IntegrityValid, string? ResolvedRef);

public sealed record ThemeCacheWriteRequest(ThemeCacheKey Key, string SourceRef, long SizeBytes);

public sealed record ThemeCacheWriteResult(ThemeOperationOutcome Outcome, ThemeCacheKey Key);

public sealed record ThemeCacheClearResult(ThemeOperationOutcome Outcome, ThemeId ThemeId, long ReleasedBytes);

public sealed record ThemeCacheValidationRequest(ThemeId ThemeId, ThemeVersion? Version);

public sealed record ThemeCacheValidationResult(bool IsValid, IReadOnlyList<ThemeCacheKey> CorruptEntries);

public sealed record ThemePreviewRequest(ThemeId ThemeId, ThemePreviewOptions Options, IReadOnlyList<UxSurfaceId> SurfaceIds);

public sealed record ThemePreviewSnapshotRequest(
    ThemePreviewSessionId SessionId,
    IReadOnlyList<UxSurfaceId> SurfaceIds,
    string PermissionFilteredSnapshotRef);

public sealed record ThemePreviewReadModel(
    ThemePreviewSessionId SessionId,
    IReadOnlyList<UxSurfaceId> SurfaceIds,
    bool IsNonAuthoritative,
    bool IsPermissionFiltered);

public enum ThemePreviewEndReason
{
    Applied,
    Cancelled,
    Exited,
    AppLocked,
    SafeModeEntered,
    Interrupted,
    Failed,
}

public sealed record ThemeSwitchRequest(
    ThemeId FromThemeId,
    ThemeId ToThemeId,
    ThemeApplyOptions ApplyOptions,
    bool PreserveSafetyPresentationContinuity);

public sealed record ThemeSwitchResult(
    ThemeOperationOutcome Outcome,
    ThemeId ActiveThemeId,
    bool SafetyPresentationContinuityPreserved,
    IReadOnlyList<string> DiagnosticsRefs);

public enum ThemeSwitchInterruptionReason
{
    CriticalAlert,
    AppLock,
    SafeMode,
    Recovery,
    UserCancellation,
    AccessibilityModeChanged,
}

public sealed record ThemeSwitchInterruptionResult(bool Interrupted, ThemeId ActiveThemeId, bool FallbackApplied);

public sealed record ThemePersonalizationRequest(
    ThemeId ThemeId,
    ThemeVariantId VariantId,
    IReadOnlyDictionary<string, System.Text.Json.JsonElement> Values,
    IReadOnlyList<UxSurfaceId> AffectedSurfaces);

public sealed record ThemeAccessibilityProfile(
    string ProfileId,
    decimal TextScale,
    decimal Zoom,
    bool HighContrast,
    string? ColorVisionMode,
    bool ReducedMotion,
    bool ReducedTransparency,
    bool ThemeAudioDisabled,
    bool KeyboardOnly,
    bool ScreenReaderActive);

public sealed record ThemePersonalizationValidationResult(bool IsValid, IReadOnlyList<string> Errors);

public sealed record ThemePersonalizationResult(
    IReadOnlyDictionary<string, System.Text.Json.JsonElement> Values,
    IReadOnlyList<string> ClampedControlIds,
    IReadOnlyList<string> Explanations);

public sealed record ThemePersonalizationResetRequest(ThemeId ThemeId, ThemeVariantId? VariantId, string Scope);

public sealed record ThemePersonalizationResetResult(ThemeOperationOutcome Outcome, string Scope);

public sealed record ThemeMotionContext(
    ThemeId ThemeId,
    ThemeVariantId VariantId,
    ThemeAccessibilityProfile AccessibilityProfile,
    bool IsCriticalAlertActive,
    bool IsNegativeOutcome);

public sealed record ThemeMotionSpec(
    string MotionId,
    string PriorityClass,
    TimeSpan Duration,
    bool Interruptible,
    bool Skippable,
    bool SupplementaryOnly,
    bool IsStaticFallback);

public sealed record ThemeAudioContext(
    ThemeId ThemeId,
    ThemeVariantId VariantId,
    ThemePreviewSessionId? PreviewSessionId,
    bool IsCriticalAlert,
    ThemeCorrelationId CorrelationId);

public sealed record ThemeAudioCategoryChange(bool Enabled, decimal Volume, ThemeAudioContext Context);

public sealed record ThemeAudioApplyResult(ThemeOperationOutcome Outcome, ThemeAudioState EffectiveState);

public sealed record ThemeAudioPlaybackResult(ThemeOperationOutcome Outcome, string AssetOrEventId, bool SuppressedByPolicy);

public sealed record ThemeAudioDuckingRequest(
    ThemeAudioContext Context,
    decimal TargetVolume,
    TimeSpan Duration,
    string Priority);

public sealed record ThemeAudioDuckingResult(ThemeOperationOutcome Outcome, decimal EffectiveVolume);

public sealed record ThemeAudioPreviewRequest(
    ThemePreviewSessionId PreviewSessionId,
    string AssetOrEventId,
    string Category,
    ThemeAudioState PreviewState);

public sealed record ThemeAudioPreviewResult(ThemeOperationOutcome Outcome, bool IsolatedFromActiveThemeState);

public sealed record ThemeAccessibilityValidationRequest(
    ThemeManifest Manifest,
    ThemeAccessibilityContract Contract,
    IReadOnlyList<ThemeVariantId> Variants,
    IReadOnlyList<ThemeAccessibilityProfile> RequiredProfiles,
    IReadOnlyList<UxSurfaceId> RequiredSurfaces);

public sealed record ThemeRollbackOption(
    ThemeId ThemeId,
    ThemeVersion Version,
    string PackageRef,
    string PackageHash,
    string ManifestHash,
    bool CurrentValidationPassed);

public sealed record ThemeFailureInput(
    ThemeId ThemeId,
    ThemeVersion Version,
    string FailureClass,
    int SessionFailureCount,
    bool OccurredDuringStartup);

public sealed record ThemeFailureDecision(
    bool DisableTheme,
    bool UseFallback,
    bool EnterSafeMode,
    string Reason);
