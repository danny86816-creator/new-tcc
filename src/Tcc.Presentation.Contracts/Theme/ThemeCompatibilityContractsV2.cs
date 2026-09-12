using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Tcc.Presentation.Contracts.Theme;

public enum ThemeCompatibilityEvaluationStateV2
{
    [JsonStringEnumMemberName("not_evaluated")]
    NotEvaluated = 0,
    [JsonStringEnumMemberName("refused_precondition")]
    RefusedPrecondition = 1,
    [JsonStringEnumMemberName("evaluated")]
    Evaluated = 2,
}
public enum ThemeCompatibilityStatusV2
{
    [JsonStringEnumMemberName("not_evaluated")]
    NotEvaluated = 0,
    [JsonStringEnumMemberName("refused_precondition")]
    RefusedPrecondition = 1,
    [JsonStringEnumMemberName("trusted_evidence_failure")]
    TrustedEvidenceFailure = 2,
    [JsonStringEnumMemberName("unsupported_manifest_schema")]
    UnsupportedManifestSchema = 3,
    [JsonStringEnumMemberName("manifest_schema_invalid")]
    ManifestSchemaInvalid = 4,
    [JsonStringEnumMemberName("invalid_version_input")]
    InvalidVersionInput = 5,
    [JsonStringEnumMemberName("unsatisfiable_version_range")]
    UnsatisfiableVersionRange = 6,
    [JsonStringEnumMemberName("conflicting_version_declaration")]
    ConflictingVersionDeclaration = 7,
    [JsonStringEnumMemberName("incompatible_core_version")]
    IncompatibleCoreVersion = 8,
    [JsonStringEnumMemberName("incompatible_theme_api_version")]
    IncompatibleThemeApiVersion = 9,
    [JsonStringEnumMemberName("incompatible_ux_contract_version")]
    IncompatibleUxContractVersion = 10,
    [JsonStringEnumMemberName("unsupported_platform")]
    UnsupportedPlatform = 11,
    [JsonStringEnumMemberName("unsupported_installation_mode")]
    UnsupportedInstallationMode = 12,
    [JsonStringEnumMemberName("runtime_validation_failed")]
    RuntimeValidationFailed = 13,
    [JsonStringEnumMemberName("accessibility_validation_failed")]
    AccessibilityValidationFailed = 14,
    [JsonStringEnumMemberName("safety_validation_failed")]
    SafetyValidationFailed = 15,
    [JsonStringEnumMemberName("blocked_capability")]
    BlockedCapability = 16,
    [JsonStringEnumMemberName("migration_not_ready")]
    MigrationNotReady = 17,
    [JsonStringEnumMemberName("rollback_unavailable")]
    RollbackUnavailable = 18,
    [JsonStringEnumMemberName("evidence_unavailable")]
    EvidenceUnavailable = 19,
    [JsonStringEnumMemberName("compatible")]
    Compatible = 20,
    [JsonStringEnumMemberName("compatible_with_degradation")]
    CompatibleWithDegradation = 21,
    [JsonStringEnumMemberName("compatible_untested_dpi_with_scalable_fallback")]
    CompatibleUntestedDpiWithScalableFallback = 22,
}

public enum ThemeCompatibilityDimensionV2
{
    [JsonStringEnumMemberName("precondition")]
    Precondition = 0,
    [JsonStringEnumMemberName("integrity")]
    Integrity = 1,
    [JsonStringEnumMemberName("manifest_schema")]
    ManifestSchema = 2,
    [JsonStringEnumMemberName("version_declarations")]
    VersionDeclarations = 3,
    [JsonStringEnumMemberName("core_version")]
    CoreVersion = 4,
    [JsonStringEnumMemberName("theme_api_version")]
    ThemeApiVersion = 5,
    [JsonStringEnumMemberName("ux_contract_version")]
    UxContractVersion = 6,
    [JsonStringEnumMemberName("platform_mode")]
    PlatformMode = 7,
    [JsonStringEnumMemberName("runtime")]
    Runtime = 8,
    [JsonStringEnumMemberName("accessibility")]
    Accessibility = 9,
    [JsonStringEnumMemberName("safety")]
    Safety = 10,
    [JsonStringEnumMemberName("capability")]
    Capability = 11,
    [JsonStringEnumMemberName("migration")]
    Migration = 12,
    [JsonStringEnumMemberName("rollback")]
    Rollback = 13,
}

public enum ThemeCompatibilityFailureKindV2
{
    [JsonStringEnumMemberName("missing_trusted_context")]
    MissingTrustedContext = 0,
    [JsonStringEnumMemberName("invalid_runtime_context")]
    InvalidRuntimeContext = 1,
    [JsonStringEnumMemberName("context_binding_mismatch")]
    ContextBindingMismatch = 2,
    [JsonStringEnumMemberName("content_snapshot_unavailable")]
    ContentSnapshotUnavailable = 3,
    [JsonStringEnumMemberName("integrity_not_verified")]
    IntegrityNotVerified = 4,
    [JsonStringEnumMemberName("content_evidence_mismatch")]
    ContentEvidenceMismatch = 5,
    [JsonStringEnumMemberName("unsupported_manifest_schema")]
    UnsupportedManifestSchema = 6,
    [JsonStringEnumMemberName("manifest_schema_invalid")]
    ManifestSchemaInvalid = 7,
    [JsonStringEnumMemberName("invalid_schema_support_input")]
    InvalidSchemaSupportInput = 8,
    [JsonStringEnumMemberName("invalid_version_input")]
    InvalidVersionInput = 9,
    [JsonStringEnumMemberName("unsatisfiable_version_range")]
    UnsatisfiableVersionRange = 10,
    [JsonStringEnumMemberName("conflicting_version_declaration")]
    ConflictingVersionDeclaration = 11,
    [JsonStringEnumMemberName("core_version_incompatible")]
    CoreVersionIncompatible = 12,
    [JsonStringEnumMemberName("theme_api_no_compatible_version")]
    ThemeApiNoCompatibleVersion = 13,
    [JsonStringEnumMemberName("ux_contract_no_compatible_version")]
    UxContractNoCompatibleVersion = 14,
    [JsonStringEnumMemberName("platform_unsupported")]
    PlatformUnsupported = 15,
    [JsonStringEnumMemberName("installation_mode_unsupported")]
    InstallationModeUnsupported = 16,
    [JsonStringEnumMemberName("runtime_validation_failed")]
    RuntimeValidationFailed = 17,
    [JsonStringEnumMemberName("accessibility_validation_failed")]
    AccessibilityValidationFailed = 18,
    [JsonStringEnumMemberName("safety_validation_failed")]
    SafetyValidationFailed = 19,
    [JsonStringEnumMemberName("asset_inventory_invalid")]
    AssetInventoryInvalid = 20,
    [JsonStringEnumMemberName("motion_safety_failed")]
    MotionSafetyFailed = 21,
    [JsonStringEnumMemberName("audio_safety_failed")]
    AudioSafetyFailed = 22,
    [JsonStringEnumMemberName("capability_blocked")]
    CapabilityBlocked = 23,
    [JsonStringEnumMemberName("migration_not_ready")]
    MigrationNotReady = 24,
    [JsonStringEnumMemberName("rollback_unavailable")]
    RollbackUnavailable = 25,
    [JsonStringEnumMemberName("evidence_missing")]
    EvidenceMissing = 26,
    [JsonStringEnumMemberName("evidence_not_evaluated")]
    EvidenceNotEvaluated = 27,
    [JsonStringEnumMemberName("evidence_scope_mismatch")]
    EvidenceScopeMismatch = 28,
    [JsonStringEnumMemberName("evidence_malformed")]
    EvidenceMalformed = 29,
}

public enum ThemeCompatibilityEvidenceStatusV2
{
    [JsonStringEnumMemberName("not_evaluated")]
    NotEvaluated = 0,
    [JsonStringEnumMemberName("passed")]
    Passed = 1,
    [JsonStringEnumMemberName("failed")]
    Failed = 2,
    [JsonStringEnumMemberName("not_applicable")]
    NotApplicable = 3,
}

public enum ThemeCompatibilityAccessibilityStatusV2
{
    [JsonStringEnumMemberName("not_evaluated")]
    NotEvaluated = 0,
    [JsonStringEnumMemberName("validated")]
    Validated = 1,
    [JsonStringEnumMemberName("failed")]
    Failed = 2,
}

public enum ThemeCompatibilitySafetyStatusV2
{
    [JsonStringEnumMemberName("not_evaluated")]
    NotEvaluated = 0,
    [JsonStringEnumMemberName("passed")]
    Passed = 1,
    [JsonStringEnumMemberName("failed_home_safety_core")]
    FailedHomeSafetyCore = 2,
    [JsonStringEnumMemberName("failed_critical_alerts")]
    FailedCriticalAlerts = 3,
    [JsonStringEnumMemberName("failed_risk_permission_clarity")]
    FailedRiskPermissionClarity = 4,
    [JsonStringEnumMemberName("failed_confirmation_semantics")]
    FailedConfirmationSemantics = 5,
    [JsonStringEnumMemberName("failed_accessibility")]
    FailedAccessibility = 6,
}

public enum ThemeCompatibilityInstallationModeV2
{
    [JsonStringEnumMemberName("not_specified")]
    NotSpecified = 0,
    [JsonStringEnumMemberName("installer")]
    Installer = 1,
    [JsonStringEnumMemberName("portable")]
    Portable = 2,
}

public enum ThemeCompatibilityOperationV2
{
    [JsonStringEnumMemberName("not_specified")]
    NotSpecified = 0,
    [JsonStringEnumMemberName("package_evaluation")]
    PackageEvaluation = 1,
    [JsonStringEnumMemberName("transition_evaluation")]
    TransitionEvaluation = 2,
}

public enum ThemeCompatibilityNoticeKindV2
{
    [JsonStringEnumMemberName("not_specified")]
    NotSpecified = 0,
    [JsonStringEnumMemberName("optional_capability_disabled")]
    OptionalCapabilityDisabled = 1,
    [JsonStringEnumMemberName("decorative_presentation_degradation")]
    DecorativePresentationDegradation = 2,
    [JsonStringEnumMemberName("untested_dpi_scalable_fallback")]
    UntestedDpiScalableFallback = 3,
}

public sealed record ThemeCompatibilityRuntimeTargetV2
{
    public ThemeCompatibilityRuntimeTargetV2(
        string targetId,
        ThemeVariantId variant,
        UxSurfaceId surface,
        decimal dpiScale,
        decimal viewportWidthDip,
        decimal viewportHeightDip,
        ThemeAccessibilityProfile accessibilityProfile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId);
        ArgumentNullException.ThrowIfNull(accessibilityProfile);
        if (dpiScale <= 0 || viewportWidthDip <= 0 || viewportHeightDip <= 0
            || accessibilityProfile.TextScale <= 0 || accessibilityProfile.Zoom <= 0)
        {
            throw new ArgumentException("Runtime target dimensions and scales must be positive.");
        }
        TargetId = targetId;
        Variant = variant;
        Surface = surface;
        DpiScale = dpiScale;
        ViewportWidthDip = viewportWidthDip;
        ViewportHeightDip = viewportHeightDip;
        AccessibilityProfile = accessibilityProfile with { };
    }

    public string TargetId { get; }
    public ThemeVariantId Variant { get; }
    public UxSurfaceId Surface { get; }
    public decimal DpiScale { get; }
    public decimal ViewportWidthDip { get; }
    public decimal ViewportHeightDip { get; }
    public ThemeAccessibilityProfile AccessibilityProfile { get; }
}

public sealed class ThemeCompatibilityEnvironmentV2
{
    public ThemeCompatibilityEnvironmentV2(
        string? coreVersion,
        IEnumerable<string?>? supportedThemeApiVersions,
        IEnumerable<string?>? supportedUxContractVersions,
        IEnumerable<string?>? supportedManifestSchemaVersions,
        string? platform,
        string? architecture,
        ThemeCompatibilityInstallationModeV2 installationMode,
        string runtimeBuildId,
        string validationProfileVersion,
        IEnumerable<ThemeCompatibilityRuntimeTargetV2> runtimeTargets,
        ThemeCompatibilityOperationV2 operation,
        ThemeId? currentThemeId,
        ThemeVersion? currentThemeVersion,
        string? themeStateRevision)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runtimeBuildId);
        ArgumentException.ThrowIfNullOrWhiteSpace(validationProfileVersion);
        ArgumentNullException.ThrowIfNull(runtimeTargets);
        if (!Enum.IsDefined(installationMode) || !Enum.IsDefined(operation)
            || currentThemeId.HasValue != currentThemeVersion.HasValue)
        {
            throw new ArgumentException("Invalid environment description.");
        }
        if (operation == ThemeCompatibilityOperationV2.PackageEvaluation
            && (currentThemeId is not null || currentThemeVersion is not null || themeStateRevision is not null))
        {
            throw new ArgumentException("Package evaluation has no source state.");
        }
        ImmutableArray<ThemeCompatibilityRuntimeTargetV2>.Builder targets = ImmutableArray.CreateBuilder<ThemeCompatibilityRuntimeTargetV2>();
        HashSet<string> identifiers = new(StringComparer.Ordinal);
        foreach (ThemeCompatibilityRuntimeTargetV2 target in runtimeTargets)
        {
            ArgumentNullException.ThrowIfNull(target);
            if (!identifiers.Add(target.TargetId))
            {
                throw new ArgumentException("Runtime target identifiers must be unique.");
            }
            targets.Add(new(target.TargetId, target.Variant, target.Surface, target.DpiScale,
                target.ViewportWidthDip, target.ViewportHeightDip, target.AccessibilityProfile));
        }
        if (targets.Count == 0)
        {
            throw new ArgumentException("At least one runtime target is required.");
        }
        CoreVersion = coreVersion;
        SupportedThemeApiVersions = supportedThemeApiVersions?.ToImmutableArray();
        SupportedUxContractVersions = supportedUxContractVersions?.ToImmutableArray();
        SupportedManifestSchemaVersions = supportedManifestSchemaVersions?.ToImmutableArray();
        Platform = platform;
        Architecture = architecture;
        InstallationMode = installationMode;
        RuntimeBuildId = runtimeBuildId;
        ValidationProfileVersion = validationProfileVersion;
        RuntimeTargets = targets.ToImmutable();
        Operation = operation;
        CurrentThemeId = currentThemeId;
        CurrentThemeVersion = currentThemeVersion;
        ThemeStateRevision = themeStateRevision;
    }

    public string? CoreVersion { get; }
    public ImmutableArray<string?>? SupportedThemeApiVersions { get; }
    public ImmutableArray<string?>? SupportedUxContractVersions { get; }
    public ImmutableArray<string?>? SupportedManifestSchemaVersions { get; }
    public string? Platform { get; }
    public string? Architecture { get; }
    public ThemeCompatibilityInstallationModeV2 InstallationMode { get; }
    public string RuntimeBuildId { get; }
    public string ValidationProfileVersion { get; }
    public ImmutableArray<ThemeCompatibilityRuntimeTargetV2> RuntimeTargets { get; }
    public ThemeCompatibilityOperationV2 Operation { get; }
    public ThemeId? CurrentThemeId { get; }
    public ThemeVersion? CurrentThemeVersion { get; }
    public string? ThemeStateRevision { get; }
}

public sealed record ThemeCompatibilityFailureV2
{
    public ThemeCompatibilityFailureV2(
        ThemeCompatibilityFailureKindV2 kind,
        ThemeCompatibilityDimensionV2 dimension,
        int sequence,
        string? diagnosticCode,
        string message)
    {
        ThemeCompatibilityJsonV2.ValidateFailure(kind, dimension, sequence, diagnosticCode, message);
        Kind = kind;
        Dimension = dimension;
        Sequence = sequence;
        DiagnosticCode = diagnosticCode;
        Message = message;
    }

    public ThemeCompatibilityFailureKindV2 Kind { get; }
    public ThemeCompatibilityDimensionV2 Dimension { get; }
    public int Sequence { get; }
    public string? DiagnosticCode { get; }
    public string Message { get; }
}

public sealed record ThemeCompatibilityNoticeV2
{
    public ThemeCompatibilityNoticeV2(
        ThemeCompatibilityNoticeKindV2 kind,
        ThemeCompatibilityDimensionV2 dimension,
        int sequence,
        string? diagnosticCode,
        string message)
    {
        ThemeCompatibilityJsonV2.ValidateNotice(kind, dimension, sequence, diagnosticCode, message);
        Kind = kind;
        Dimension = dimension;
        Sequence = sequence;
        DiagnosticCode = diagnosticCode;
        Message = message;
    }

    public ThemeCompatibilityNoticeKindV2 Kind { get; }
    public ThemeCompatibilityDimensionV2 Dimension { get; }
    public int Sequence { get; }
    public string? DiagnosticCode { get; }
    public string Message { get; }
}

public sealed class ThemeCompatibilityResultV2
{
    public ThemeCompatibilityResultV2(
        string contractVersion,
        ThemeCompatibilityStatusV2 status,
        ThemeCompatibilityEvaluationStateV2 evaluationState,
        ThemeId? themeId,
        ThemeVersion? themeVersion,
        string? packageHash,
        string? selectedCoreVersion,
        string? selectedThemeApiVersion,
        string? selectedUxContractVersion,
        string? selectedManifestSchemaVersion,
        ThemeCompatibilityEvidenceStatusV2 runtimeValidationStatus,
        ThemeCompatibilityEvidenceStatusV2 capabilityEvidenceStatus,
        IEnumerable<string> authorizedCapabilities,
        IEnumerable<string> enabledCapabilities,
        IEnumerable<string> disabledCapabilities,
        IEnumerable<ThemeCompatibilityNoticeV2> notices,
        IEnumerable<ThemeCompatibilityFailureV2> failures,
        ThemeCompatibilityAccessibilityStatusV2 accessibilityValidationStatus,
        ThemeCompatibilitySafetyStatusV2 safetyInvariantsStatus,
        IEnumerable<ThemeCompatibilitySafetyStatusV2> safetyFailures,
        ThemeCompatibilityEvidenceStatusV2 migrationEvidenceStatus,
        bool? migrationRequired,
        bool? migrationReady,
        ThemeCompatibilityEvidenceStatusV2 rollbackEvidenceStatus,
        bool? rollbackRequired,
        bool? rollbackAvailable)
    {
        ContractVersion = contractVersion;
        Status = status;
        EvaluationState = evaluationState;
        ThemeId = themeId;
        ThemeVersion = themeVersion;
        PackageHash = packageHash;
        SelectedCoreVersion = selectedCoreVersion;
        SelectedThemeApiVersion = selectedThemeApiVersion;
        SelectedUxContractVersion = selectedUxContractVersion;
        SelectedManifestSchemaVersion = selectedManifestSchemaVersion;
        RuntimeValidationStatus = runtimeValidationStatus;
        CapabilityEvidenceStatus = capabilityEvidenceStatus;
        ArgumentNullException.ThrowIfNull(authorizedCapabilities);
        AuthorizedCapabilities = authorizedCapabilities.ToImmutableArray();
        ArgumentNullException.ThrowIfNull(enabledCapabilities);
        EnabledCapabilities = enabledCapabilities.ToImmutableArray();
        ArgumentNullException.ThrowIfNull(disabledCapabilities);
        DisabledCapabilities = disabledCapabilities.ToImmutableArray();
        ArgumentNullException.ThrowIfNull(notices);
        Notices = notices.ToImmutableArray();
        ArgumentNullException.ThrowIfNull(failures);
        Failures = failures.ToImmutableArray();
        AccessibilityValidationStatus = accessibilityValidationStatus;
        SafetyInvariantsStatus = safetyInvariantsStatus;
        ArgumentNullException.ThrowIfNull(safetyFailures);
        SafetyFailures = safetyFailures.ToImmutableArray();
        MigrationEvidenceStatus = migrationEvidenceStatus;
        MigrationRequired = migrationRequired;
        MigrationReady = migrationReady;
        RollbackEvidenceStatus = rollbackEvidenceStatus;
        RollbackRequired = rollbackRequired;
        RollbackAvailable = rollbackAvailable;
    }

    public string ContractVersion { get; }
    public ThemeCompatibilityStatusV2 Status { get; }
    public ThemeCompatibilityEvaluationStateV2 EvaluationState { get; }
    public ThemeId? ThemeId { get; }
    public ThemeVersion? ThemeVersion { get; }
    public string? PackageHash { get; }
    public string? SelectedCoreVersion { get; }
    public string? SelectedThemeApiVersion { get; }
    public string? SelectedUxContractVersion { get; }
    public string? SelectedManifestSchemaVersion { get; }
    public ThemeCompatibilityEvidenceStatusV2 RuntimeValidationStatus { get; }
    public ThemeCompatibilityEvidenceStatusV2 CapabilityEvidenceStatus { get; }
    public ImmutableArray<string> AuthorizedCapabilities { get; }
    public ImmutableArray<string> EnabledCapabilities { get; }
    public ImmutableArray<string> DisabledCapabilities { get; }
    public ImmutableArray<ThemeCompatibilityNoticeV2> Notices { get; }
    public ImmutableArray<ThemeCompatibilityFailureV2> Failures { get; }
    public ThemeCompatibilityAccessibilityStatusV2 AccessibilityValidationStatus { get; }
    public ThemeCompatibilitySafetyStatusV2 SafetyInvariantsStatus { get; }
    public ImmutableArray<ThemeCompatibilitySafetyStatusV2> SafetyFailures { get; }
    public ThemeCompatibilityEvidenceStatusV2 MigrationEvidenceStatus { get; }
    public bool? MigrationRequired { get; }
    public bool? MigrationReady { get; }
    public ThemeCompatibilityEvidenceStatusV2 RollbackEvidenceStatus { get; }
    public bool? RollbackRequired { get; }
    public bool? RollbackAvailable { get; }
}
