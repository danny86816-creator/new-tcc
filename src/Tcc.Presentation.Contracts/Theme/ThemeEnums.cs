namespace Tcc.Presentation.Contracts.Theme;

public enum TradingPermissionPresentationState
{
    Tradable,
    Warning,
    Blocked,
}

public enum ThemeCompatibilityStatus
{
    Compatible,
    CompatibleWithDegradation,
    CompatibleUntestedDpiWithScalableFallback,
    IncompatibleCoreVersion,
    IncompatibleThemeApiVersion,
    IncompatibleUxContractVersion,
    AccessibilityValidationFailed,
    IntegrityFailed,
    SignatureFailed,
    BlockedCapability,
    UnsupportedPlatform,
    SafeModeRequired,
}

public enum ThemeAccessibilityStatus
{
    Unvalidated,
    Validated,
    Failed,
}

public enum ThemeCapabilityDecisionResult
{
    Allowed,
    Blocked,
    Unknown,
}

public enum ThemeOperationOutcome
{
    Succeeded,
    Blocked,
    RolledBack,
    FallbackApplied,
    SafeModeRequired,
    Failed,
}

public enum ThemeApplyOutcome
{
    Applied,
    Blocked,
    RolledBack,
    FallbackApplied,
    SafeModeRequired,
}

public enum ThemeAssetTier
{
    Tier0,
    Tier1,
    Tier2,
    Tier3,
    Audio,
}

public enum ThemeDiagnosticsSeverity
{
    Information,
    Warning,
    Error,
    Critical,
}

public enum ThemeFailureReason
{
    ManifestSchemaInvalid,
    ManifestSchemaUnsupported,
    PackageIdentityConflict,
    ArchiveSafetyFailed,
    PortableUnsupported,
    AssetInventoryInvalid,
    MotionSafetyFailed,
    AudioSafetyFailed,
    ChannelPolicyFailed,
    AccessibilityValidationFailed,
    CapabilityBlocked,
    IntegrityFailed,
    SignatureFailed,
    CoreVersionUnsupported,
    ThemeApiVersionUnsupported,
    UxContractVersionUnsupported,
}
