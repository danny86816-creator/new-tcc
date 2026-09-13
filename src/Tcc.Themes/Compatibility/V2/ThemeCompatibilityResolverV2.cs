using System.Collections.Immutable;
using Tcc.Presentation.Contracts.Theme;

namespace Tcc.Themes.Compatibility.V2;

public sealed class ThemeCompatibilityResolverV2 : IThemeCompatibilityResolverV2
{
    public ThemeCompatibilityResolverV2()
    {
    }

    public ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Context is null)
        {
            return Stopped(ThemeCompatibilityFailureKindV2.MissingTrustedContext,
                ThemeCompatibilityDimensionV2.Precondition);
        }

        if (request.Environment is null)
        {
            return Stopped(ThemeCompatibilityFailureKindV2.InvalidRuntimeContext,
                ThemeCompatibilityDimensionV2.Precondition);
        }

        ThemeCompatibilityContextV2 context = request.Context;
        ThemeCompatibilityEnvironmentV2 environment = request.Environment;
        if (!IsValidOperation(environment))
        {
            return Stopped(ThemeCompatibilityFailureKindV2.InvalidRuntimeContext,
                ThemeCompatibilityDimensionV2.Precondition);
        }

        if (request.Evidence is not null
            && (!ReferenceEquals(request.Evidence.Context, context)
                || !ReferenceEquals(request.Evidence.Environment, environment)))
        {
            return Stopped(ThemeCompatibilityFailureKindV2.ContextBindingMismatch,
                ThemeCompatibilityDimensionV2.Precondition);
        }

        if (request.Evidence is not null
            && TryFindReceiptPrecondition(
                request.Evidence,
                context,
                environment,
                out ThemeCompatibilityFailureKindV2 receiptFailure,
                out ThemeCompatibilityDimensionV2 receiptDimension))
        {
            return Stopped(receiptFailure, receiptDimension);
        }

        ThemeId? trustedThemeId = null;
        ThemeVersion? trustedThemeVersion = null;
        string? trustedPackageHash = null;
        if (!context.BindingFailures.IsEmpty)
        {
            ImmutableArray<ThemeCompatibilityFailureV2> bindingFailures = context.BindingFailures;
            if (!IsValidBindingFailures(bindingFailures))
            {
                return Stopped(ThemeCompatibilityFailureKindV2.ContentEvidenceMismatch,
                    ThemeCompatibilityDimensionV2.Integrity);
            }

            if (TryGetTrustedIdentity(context, out trustedThemeId, out trustedThemeVersion, out trustedPackageHash))
            {
                return Stopped(bindingFailures, trustedThemeId, trustedThemeVersion, trustedPackageHash);
            }

            return Stopped(bindingFailures, null, null, null);
        }

        if (!TryGetTrustedIdentity(context, out trustedThemeId, out trustedThemeVersion, out trustedPackageHash))
        {
            return Stopped(ThemeCompatibilityFailureKindV2.ContentEvidenceMismatch,
                ThemeCompatibilityDimensionV2.Integrity);
        }

        ThemeCompatibilityFailureKindV2? schemaFailure = ValidateSchemaSupport(
            environment.SupportedManifestSchemaVersions,
            context.Manifest!.SchemaVersion,
            context.Compatibility!.SchemaVersion);
        if (schemaFailure is not null)
        {
            return Stopped(
                schemaFailure.Value,
                ThemeCompatibilityDimensionV2.ManifestSchema,
                trustedThemeId,
                trustedThemeVersion,
                trustedPackageHash);
        }

        ThemeCompatibilityVersionNegotiation negotiation = ThemeCompatibilityVersionNegotiator.Negotiate(
            new ThemeCompatibilityRequest(
                context.Manifest,
                context.Compatibility,
                environment.CoreVersion!,
                ToSet(environment.SupportedThemeApiVersions)!,
                ToSet(environment.SupportedUxContractVersions)!,
                ToSet(environment.SupportedManifestSchemaVersions)!,
                environment.Platform!,
                environment.InstallationMode == ThemeCompatibilityInstallationModeV2.Portable,
                new ThemeAccessibilityValidationResult(
                    ThemeAccessibilityStatus.Unvalidated,
                    Array.Empty<string>(),
                    Array.Empty<string>())));

        List<ThemeCompatibilityFailureV2> failures = [];
        foreach (ThemeCompatibilityNegotiationFailureKind failure in negotiation.Failures)
        {
            (ThemeCompatibilityFailureKindV2 kind, ThemeCompatibilityDimensionV2 dimension) = Map(failure);
            Add(failures, kind, dimension);
        }

        if (!IsSupportedPlatform(environment, context.Manifest, context.Compatibility))
        {
            Add(failures, ThemeCompatibilityFailureKindV2.PlatformUnsupported,
                ThemeCompatibilityDimensionV2.PlatformMode);
        }

        if (!IsSupportedInstallationMode(environment, context.Manifest, context.Compatibility))
        {
            Add(failures, ThemeCompatibilityFailureKindV2.InstallationModeUnsupported,
                ThemeCompatibilityDimensionV2.PlatformMode);
        }

        ThemeCompatibilityEvidenceStatusV2 runtimeStatus = ThemeCompatibilityEvidenceStatusV2.NotEvaluated;
        ThemeCompatibilityEvidenceStatusV2 capabilityStatus = ThemeCompatibilityEvidenceStatusV2.NotEvaluated;
        ThemeCompatibilityAccessibilityStatusV2 accessibilityStatus = ThemeCompatibilityAccessibilityStatusV2.NotEvaluated;
        ThemeCompatibilitySafetyStatusV2 safetyStatus = ThemeCompatibilitySafetyStatusV2.NotEvaluated;
        ImmutableArray<ThemeCompatibilitySafetyStatusV2> safetyFailures =
            ImmutableArray<ThemeCompatibilitySafetyStatusV2>.Empty;
        ThemeCompatibilityEvidenceStatusV2 migrationStatus = ThemeCompatibilityEvidenceStatusV2.NotEvaluated;
        bool? migrationRequired = null;
        bool? migrationReady = null;
        ThemeCompatibilityEvidenceStatusV2 rollbackStatus = ThemeCompatibilityEvidenceStatusV2.NotEvaluated;
        bool? rollbackRequired = null;
        bool? rollbackAvailable = null;
        ImmutableArray<string> authorized = ImmutableArray<string>.Empty;
        ImmutableArray<string> enabled = ImmutableArray<string>.Empty;
        ImmutableArray<string> disabled = ImmutableArray<string>.Empty;
        List<ThemeCompatibilityNoticeV2> notices = [];

        if (request.Evidence is null)
        {
            Add(failures, ThemeCompatibilityFailureKindV2.EvidenceMissing, ThemeCompatibilityDimensionV2.Runtime);
            Add(failures, ThemeCompatibilityFailureKindV2.EvidenceMissing, ThemeCompatibilityDimensionV2.Accessibility);
            Add(failures, ThemeCompatibilityFailureKindV2.EvidenceMissing, ThemeCompatibilityDimensionV2.Safety);
            Add(failures, ThemeCompatibilityFailureKindV2.EvidenceMissing, ThemeCompatibilityDimensionV2.Capability);
            Add(failures, ThemeCompatibilityFailureKindV2.EvidenceMissing, ThemeCompatibilityDimensionV2.Migration);
            Add(failures, ThemeCompatibilityFailureKindV2.EvidenceMissing, ThemeCompatibilityDimensionV2.Rollback);
        }
        else
        {
            ThemeCompatibilityEvidenceV2 evidence = request.Evidence;
            ComposeRuntime(evidence.Runtime, failures, notices, out runtimeStatus);
            ComposeAccessibility(evidence.Accessibility, failures, out accessibilityStatus);
            ComposeSafety(evidence.Safety, failures, out safetyStatus, out safetyFailures);
            ComposeCapability(evidence.Capability, failures, notices, out capabilityStatus,
                out authorized, out enabled, out disabled);
            ComposeMigration(evidence.Migration, failures, out migrationStatus,
                out migrationRequired, out migrationReady);
            ComposeRollback(evidence.Rollback, failures, out rollbackStatus,
                out rollbackRequired, out rollbackAvailable);
        }

        bool coreProven = true;
        foreach (ThemeCompatibilityNegotiationFailureKind failure in negotiation.Failures)
        {
            if (failure is ThemeCompatibilityNegotiationFailureKind.InvalidVersionInput
                or ThemeCompatibilityNegotiationFailureKind.UnsatisfiableVersionRange
                or ThemeCompatibilityNegotiationFailureKind.ConflictingVersionDeclaration
                or ThemeCompatibilityNegotiationFailureKind.CoreVersionIncompatible)
            {
                coreProven = false;
            }
        }

        string? selectedCoreVersion = coreProven ? environment.CoreVersion : null;
        ImmutableArray<ThemeCompatibilityFailureV2> completedFailures = RebuildFailures(failures);
        ImmutableArray<ThemeCompatibilityNoticeV2> completedNotices = RebuildNotices(notices);
        bool hasFallback = false;
        foreach (ThemeCompatibilityNoticeV2 notice in completedNotices)
        {
            hasFallback |= notice.Kind == ThemeCompatibilityNoticeKindV2.UntestedDpiScalableFallback;
        }

        ThemeCompatibilityStatusV2 status = completedFailures.IsEmpty
            ? hasFallback
                ? ThemeCompatibilityStatusV2.CompatibleUntestedDpiWithScalableFallback
                : completedNotices.IsEmpty
                    ? ThemeCompatibilityStatusV2.Compatible
                    : ThemeCompatibilityStatusV2.CompatibleWithDegradation
            : Primary(completedFailures[0].Kind);

        return new ThemeCompatibilityResultV2(
            ContractVersions.ThemeCompatibilityContractV2,
            status,
            ThemeCompatibilityEvaluationStateV2.Evaluated,
            trustedThemeId,
            trustedThemeVersion,
            trustedPackageHash,
            selectedCoreVersion,
            negotiation.SelectedThemeApiVersion,
            negotiation.SelectedUxContractVersion,
            ContractVersions.Schema,
            runtimeStatus,
            capabilityStatus,
            authorized,
            enabled,
            disabled,
            completedNotices,
            completedFailures,
            accessibilityStatus,
            safetyStatus,
            safetyFailures,
            migrationStatus,
            migrationRequired,
            migrationReady,
            rollbackStatus,
            rollbackRequired,
            rollbackAvailable);
    }

    private static bool IsValidOperation(ThemeCompatibilityEnvironmentV2 environment) =>
        environment.Operation switch
        {
            ThemeCompatibilityOperationV2.PackageEvaluation => environment.CurrentThemeId is null
                && environment.CurrentThemeVersion is null && environment.ThemeStateRevision is null,
            ThemeCompatibilityOperationV2.TransitionEvaluation => environment.CurrentThemeId is not null
                && environment.CurrentThemeVersion is not null,
            _ => false,
        };

    private static bool TryGetTrustedIdentity(
        ThemeCompatibilityContextV2 context,
        out ThemeId? themeId,
        out ThemeVersion? themeVersion,
        out string? packageHash)
    {
        themeId = null;
        themeVersion = null;
        packageHash = null;
        if (context.Integrity is not { IsVerified: true, PackageHash: { } hash }
            || context.Manifest?.Package is null
            || context.Compatibility is null
            || context.CompatibilityManifestHash is null
            || !IsSha256(hash)
            || !IsSha256(context.CompatibilityManifestHash)
            || !string.Equals(context.Manifest.Package.ThemeId, context.Compatibility.ThemeId, StringComparison.Ordinal)
            || !string.Equals(context.Manifest.Package.Version, context.Compatibility.Version, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(context.Manifest.Package.ThemeId)
            || string.IsNullOrWhiteSpace(context.Manifest.Package.Version))
        {
            return false;
        }

        themeId = new ThemeId(context.Manifest.Package.ThemeId);
        themeVersion = new ThemeVersion(context.Manifest.Package.Version);
        packageHash = hash;
        return true;
    }

    private static ThemeCompatibilityFailureKindV2? ValidateSchemaSupport(
        ImmutableArray<string?>? supported,
        string manifestSchema,
        string compatibilitySchema)
    {
        if (supported is null)
        {
            return ThemeCompatibilityFailureKindV2.InvalidSchemaSupportInput;
        }

        bool supportsCurrent = false;
        foreach (string? value in supported.Value)
        {
            if (!IsSchemaVersion(value))
            {
                return ThemeCompatibilityFailureKindV2.InvalidSchemaSupportInput;
            }

            supportsCurrent |= string.Equals(value, ContractVersions.Schema, StringComparison.Ordinal);
        }

        return supportsCurrent
            && string.Equals(manifestSchema, ContractVersions.Schema, StringComparison.Ordinal)
            && string.Equals(compatibilitySchema, ContractVersions.Schema, StringComparison.Ordinal)
                ? null
                : ThemeCompatibilityFailureKindV2.UnsupportedManifestSchema;
    }

    private static bool IsSchemaVersion(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        int separator = value.IndexOf('.');
        return separator > 0 && separator == value.LastIndexOf('.') && separator < value.Length - 1
            && IsNumber(value.AsSpan(0, separator)) && IsNumber(value.AsSpan(separator + 1));
    }

    private static bool IsNumber(ReadOnlySpan<char> value)
    {
        if (value.Length == 0 || value.Length > 1 && value[0] == '0')
        {
            return false;
        }

        foreach (char character in value)
        {
            if (character is < '0' or > '9')
            {
                return false;
            }
        }

        return true;
    }

    private static HashSet<string>? ToSet(ImmutableArray<string?>? values)
    {
        if (values is null)
        {
            return null;
        }

        HashSet<string> result = new(StringComparer.Ordinal);
        foreach (string? value in values.Value)
        {
            result.Add(value!);
        }

        return result;
    }

    private static bool IsSupportedPlatform(
        ThemeCompatibilityEnvironmentV2 environment,
        ThemeManifest manifest,
        ThemeCompatibilityManifest compatibility) =>
        string.Equals(environment.Platform, "windows", StringComparison.Ordinal)
        && string.Equals(environment.Architecture, "x64", StringComparison.Ordinal)
        && ContainsOrdinal(manifest.Compatibility.SupportedPlatforms, "windows")
        && compatibility.Windows.MultiMonitorSupported;

    private static bool IsSupportedInstallationMode(
        ThemeCompatibilityEnvironmentV2 environment,
        ThemeManifest manifest,
        ThemeCompatibilityManifest compatibility) =>
        environment.InstallationMode switch
        {
            ThemeCompatibilityInstallationModeV2.Installer => compatibility.Windows.InstallerSupported,
            ThemeCompatibilityInstallationModeV2.Portable =>
                manifest.Compatibility.PortableSupported && compatibility.Windows.PortableSupported,
            _ => false,
        };

    private static bool TryFindReceiptPrecondition(
        ThemeCompatibilityEvidenceV2 evidence,
        ThemeCompatibilityContextV2 context,
        ThemeCompatibilityEnvironmentV2 environment,
        out ThemeCompatibilityFailureKindV2 kind,
        out ThemeCompatibilityDimensionV2 dimension)
    {
        if (ReceiptProblem(evidence.Runtime, context, environment, ThemeCompatibilityDimensionV2.Runtime,
                out kind, out dimension)
            || ReceiptProblem(evidence.Accessibility, context, environment, ThemeCompatibilityDimensionV2.Accessibility,
                out kind, out dimension)
            || ReceiptProblem(evidence.Safety, context, environment, ThemeCompatibilityDimensionV2.Safety,
                out kind, out dimension)
            || ReceiptProblem(evidence.Capability, context, environment, ThemeCompatibilityDimensionV2.Capability,
                out kind, out dimension)
            || ReceiptProblem(evidence.Migration, context, environment, ThemeCompatibilityDimensionV2.Migration,
                out kind, out dimension)
            || ReceiptProblem(evidence.Rollback, context, environment, ThemeCompatibilityDimensionV2.Rollback,
                out kind, out dimension))
        {
            return true;
        }

        kind = default;
        dimension = default;
        return false;
    }

    private static bool ReceiptProblem(
        object? receipt,
        ThemeCompatibilityContextV2 context,
        ThemeCompatibilityEnvironmentV2 environment,
        ThemeCompatibilityDimensionV2 owner,
        out ThemeCompatibilityFailureKindV2 kind,
        out ThemeCompatibilityDimensionV2 dimension)
    {
        dimension = owner;
        if (receipt is null)
        {
            kind = default;
            return false;
        }

        ThemeCompatibilityContextV2 receiptContext;
        ThemeCompatibilityEnvironmentV2 receiptEnvironment;
        bool valid;
        switch (receipt)
        {
            case ThemeCompatibilityRuntimeEvidenceV2 runtime:
                receiptContext = runtime.Context; receiptEnvironment = runtime.Environment;
                valid = IsValidRuntime(runtime); break;
            case ThemeCompatibilityCapabilityEvidenceV2 capability:
                receiptContext = capability.Context; receiptEnvironment = capability.Environment;
                valid = IsValidCapability(capability); break;
            case ThemeCompatibilityAccessibilityEvidenceV2 accessibility:
                receiptContext = accessibility.Context; receiptEnvironment = accessibility.Environment;
                valid = IsValidAccessibility(accessibility); break;
            case ThemeCompatibilitySafetyEvidenceV2 safety:
                receiptContext = safety.Context; receiptEnvironment = safety.Environment;
                valid = IsValidSafety(safety); break;
            case ThemeCompatibilityMigrationEvidenceV2 migration:
                receiptContext = migration.Context; receiptEnvironment = migration.Environment;
                valid = IsValidMigration(migration); break;
            case ThemeCompatibilityRollbackEvidenceV2 rollback:
                receiptContext = rollback.Context; receiptEnvironment = rollback.Environment;
                valid = IsValidRollback(rollback); break;
            default:
                kind = ThemeCompatibilityFailureKindV2.EvidenceMalformed;
                return true;
        }

        if (!ReferenceEquals(receiptContext, context) || !ReferenceEquals(receiptEnvironment, environment))
        {
            kind = ThemeCompatibilityFailureKindV2.EvidenceScopeMismatch;
            return true;
        }

        kind = ThemeCompatibilityFailureKindV2.EvidenceMalformed;
        return !valid;
    }

    private static bool IsValidRuntime(ThemeCompatibilityRuntimeEvidenceV2 receipt) =>
        Enum.IsDefined(receipt.Status)
        && receipt.Status != ThemeCompatibilityEvidenceStatusV2.NotApplicable
        && ValidOwnerFailures(receipt.Failures, ThemeCompatibilityDimensionV2.Runtime)
        && ValidNotices(receipt.Notices, ThemeCompatibilityDimensionV2.Runtime)
        && (receipt.Status == ThemeCompatibilityEvidenceStatusV2.Failed || receipt.Failures.IsEmpty)
        && (receipt.Status != ThemeCompatibilityEvidenceStatusV2.NotEvaluated
            || receipt.Failures.IsEmpty && receipt.Notices.IsEmpty);

    private static bool IsValidCapability(ThemeCompatibilityCapabilityEvidenceV2 receipt) =>
        Enum.IsDefined(receipt.Status)
        && receipt.Status != ThemeCompatibilityEvidenceStatusV2.NotApplicable
        && ValidOwnerFailures(receipt.Failures, ThemeCompatibilityDimensionV2.Capability)
        && ValidNotices(receipt.Notices, ThemeCompatibilityDimensionV2.Capability)
        && ValidCapabilities(receipt)
        && (receipt.Status == ThemeCompatibilityEvidenceStatusV2.Failed || receipt.Failures.IsEmpty)
        && (receipt.Status != ThemeCompatibilityEvidenceStatusV2.NotEvaluated
            || receipt.Failures.IsEmpty && receipt.Notices.IsEmpty
            && receipt.AuthorizedCapabilities.IsEmpty && receipt.EnabledCapabilities.IsEmpty
            && receipt.DisabledCapabilities.IsEmpty);

    private static bool IsValidAccessibility(ThemeCompatibilityAccessibilityEvidenceV2 receipt) =>
        Enum.IsDefined(receipt.Status)
        && ValidOwnerFailures(receipt.Failures, ThemeCompatibilityDimensionV2.Accessibility)
        && (receipt.Status == ThemeCompatibilityAccessibilityStatusV2.Failed || receipt.Failures.IsEmpty);

    private static bool IsValidSafety(ThemeCompatibilitySafetyEvidenceV2 receipt)
    {
        if (!Enum.IsDefined(receipt.Status)
            || !Enum.IsDefined(receipt.AssetInventoryStatus)
            || !Enum.IsDefined(receipt.MotionSafetyStatus)
            || !Enum.IsDefined(receipt.AudioSafetyStatus)
            || receipt.AssetInventoryStatus == ThemeCompatibilityEvidenceStatusV2.NotApplicable
            || receipt.MotionSafetyStatus == ThemeCompatibilityEvidenceStatusV2.NotApplicable
            || !ValidOwnerFailures(receipt.Details, ThemeCompatibilityDimensionV2.Safety))
        {
            return false;
        }

        ImmutableArray<ThemeCompatibilitySafetyStatusV2> failures = receipt.Failures;
        HashSet<ThemeCompatibilitySafetyStatusV2> distinct = [];
        ThemeCompatibilitySafetyStatusV2 previous = default;
        for (int index = 0; index < failures.Length; index++)
        {
            ThemeCompatibilitySafetyStatusV2 value = failures[index];
            if (!Enum.IsDefined(value)
                || value is ThemeCompatibilitySafetyStatusV2.NotEvaluated or ThemeCompatibilitySafetyStatusV2.Passed
                || !distinct.Add(value)
                || index > 0 && previous > value)
            {
                return false;
            }

            previous = value;
        }

        if (receipt.Status == ThemeCompatibilitySafetyStatusV2.NotEvaluated)
        {
            return failures.Length == 0 && receipt.Details.IsEmpty
                && receipt.AssetInventoryStatus == ThemeCompatibilityEvidenceStatusV2.NotEvaluated
                && receipt.MotionSafetyStatus == ThemeCompatibilityEvidenceStatusV2.NotEvaluated
                && receipt.AudioSafetyStatus == ThemeCompatibilityEvidenceStatusV2.NotEvaluated;
        }

        if (receipt.Status == ThemeCompatibilitySafetyStatusV2.Passed)
        {
            return failures.Length == 0 && receipt.Details.IsEmpty
                && receipt.AssetInventoryStatus == ThemeCompatibilityEvidenceStatusV2.Passed
                && receipt.MotionSafetyStatus == ThemeCompatibilityEvidenceStatusV2.Passed
                && receipt.AudioSafetyStatus is ThemeCompatibilityEvidenceStatusV2.Passed
                    or ThemeCompatibilityEvidenceStatusV2.NotApplicable;
        }

        return failures.Length > 0 && failures[0] == receipt.Status
            && !HasContradictorySafetyDetail(
                receipt.Details,
                receipt.AssetInventoryStatus,
                receipt.MotionSafetyStatus,
                receipt.AudioSafetyStatus);
    }

    private static bool HasContradictorySafetyDetail(
        ImmutableArray<ThemeCompatibilityFailureV2> details,
        ThemeCompatibilityEvidenceStatusV2 assetInventoryStatus,
        ThemeCompatibilityEvidenceStatusV2 motionSafetyStatus,
        ThemeCompatibilityEvidenceStatusV2 audioSafetyStatus)
    {
        foreach (ThemeCompatibilityFailureV2 detail in details)
        {
            if (detail.Kind == ThemeCompatibilityFailureKindV2.AssetInventoryInvalid
                && assetInventoryStatus != ThemeCompatibilityEvidenceStatusV2.Failed
                || detail.Kind == ThemeCompatibilityFailureKindV2.MotionSafetyFailed
                && motionSafetyStatus != ThemeCompatibilityEvidenceStatusV2.Failed
                || detail.Kind == ThemeCompatibilityFailureKindV2.AudioSafetyFailed
                && audioSafetyStatus != ThemeCompatibilityEvidenceStatusV2.Failed)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsValidMigration(ThemeCompatibilityMigrationEvidenceV2 receipt) =>
        ValidTransition(receipt.Status, receipt.MigrationRequired, receipt.MigrationReady, true)
        && ValidOwnerFailures(receipt.Failures, ThemeCompatibilityDimensionV2.Migration)
        && (receipt.Status == ThemeCompatibilityEvidenceStatusV2.Failed || receipt.Failures.IsEmpty);

    private static bool IsValidRollback(ThemeCompatibilityRollbackEvidenceV2 receipt) =>
        ValidTransition(receipt.Status, receipt.RollbackRequired, receipt.RollbackAvailable, false)
        && ValidOwnerFailures(receipt.Failures, ThemeCompatibilityDimensionV2.Rollback)
        && (receipt.Status == ThemeCompatibilityEvidenceStatusV2.Failed || receipt.Failures.IsEmpty);

    private static bool ValidTransition(
        ThemeCompatibilityEvidenceStatusV2 status,
        bool? required,
        bool? ready,
        bool migration)
    {
        return status switch
        {
            ThemeCompatibilityEvidenceStatusV2.NotEvaluated => required is null && ready is null,
            ThemeCompatibilityEvidenceStatusV2.Passed => required == true && ready == true
                || required == false && (!migration || ready is null),
            ThemeCompatibilityEvidenceStatusV2.Failed => required == true && ready == false,
            ThemeCompatibilityEvidenceStatusV2.NotApplicable => required == false && (!migration || ready is null),
            _ => false,
        };
    }

    private static bool ValidCapabilities(ThemeCompatibilityCapabilityEvidenceV2 receipt)
    {
        if (!ValidStringSet(receipt.AuthorizedCapabilities)
            || !ValidStringSet(receipt.EnabledCapabilities)
            || !ValidStringSet(receipt.DisabledCapabilities)) return false;

        foreach (string value in receipt.EnabledCapabilities)
        {
            if (!ContainsOrdinal(receipt.AuthorizedCapabilities, value)
                || ContainsOrdinal(receipt.DisabledCapabilities, value))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValidOwnerFailures(
        ImmutableArray<ThemeCompatibilityFailureV2> failures,
        ThemeCompatibilityDimensionV2 dimension)
    {
        for (int index = 0; index < failures.Length; index++)
        {
            ThemeCompatibilityFailureV2? failure = failures[index];
            if (failure is null || failure.Dimension != dimension || failure.Sequence != index
                || !IsAllowedOwnerFailure(failure.Kind, dimension) || failure.DiagnosticCode is not null
                || !string.Equals(failure.Message, $"{failure.Kind} in {dimension}.", StringComparison.Ordinal)
                || index > 0 && failures[index - 1].Kind > failure.Kind)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsAllowedOwnerFailure(
        ThemeCompatibilityFailureKindV2 kind,
        ThemeCompatibilityDimensionV2 dimension) => dimension switch
        {
            ThemeCompatibilityDimensionV2.Runtime => kind == ThemeCompatibilityFailureKindV2.RuntimeValidationFailed,
            ThemeCompatibilityDimensionV2.Accessibility => kind == ThemeCompatibilityFailureKindV2.AccessibilityValidationFailed,
            ThemeCompatibilityDimensionV2.Safety => kind is ThemeCompatibilityFailureKindV2.SafetyValidationFailed
                or ThemeCompatibilityFailureKindV2.AssetInventoryInvalid
                or ThemeCompatibilityFailureKindV2.MotionSafetyFailed
                or ThemeCompatibilityFailureKindV2.AudioSafetyFailed,
            ThemeCompatibilityDimensionV2.Capability => kind == ThemeCompatibilityFailureKindV2.CapabilityBlocked,
            ThemeCompatibilityDimensionV2.Migration => kind == ThemeCompatibilityFailureKindV2.MigrationNotReady,
            ThemeCompatibilityDimensionV2.Rollback => kind == ThemeCompatibilityFailureKindV2.RollbackUnavailable,
            _ => false,
        };

    private static bool ValidStringSet(ImmutableArray<string> values)
    {
        HashSet<string> distinct = new(StringComparer.Ordinal);
        foreach (string value in values)
        {
            if (string.IsNullOrWhiteSpace(value) || !distinct.Add(value))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValidNotices(
        ImmutableArray<ThemeCompatibilityNoticeV2> notices,
        ThemeCompatibilityDimensionV2 dimension)
    {
        for (int index = 0; index < notices.Length; index++)
        {
            ThemeCompatibilityNoticeV2? notice = notices[index];
            bool allowed = dimension switch
            {
                ThemeCompatibilityDimensionV2.Runtime => notice?.Kind is
                    ThemeCompatibilityNoticeKindV2.DecorativePresentationDegradation
                    or ThemeCompatibilityNoticeKindV2.UntestedDpiScalableFallback,
                ThemeCompatibilityDimensionV2.Capability => notice?.Kind is
                    ThemeCompatibilityNoticeKindV2.OptionalCapabilityDisabled
                    or ThemeCompatibilityNoticeKindV2.DecorativePresentationDegradation,
                _ => false,
            };
            if (!allowed || notice!.Dimension != dimension || notice.Sequence != index
                || notice.DiagnosticCode is not null
                || !string.Equals(notice.Message, $"{notice.Kind} in {dimension}.", StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidBindingFailures(ImmutableArray<ThemeCompatibilityFailureV2> failures)
    {
        if (failures.Length == 0)
        {
            return false;
        }

        for (int index = 0; index < failures.Length; index++)
        {
            ThemeCompatibilityFailureV2 failure = failures[index];
            if (failure is null || failure.Sequence != index || failure.DiagnosticCode is not null
                || failure.Dimension is not (ThemeCompatibilityDimensionV2.Integrity
                    or ThemeCompatibilityDimensionV2.ManifestSchema)
                || !string.Equals(failure.Message, $"{failure.Kind} in {failure.Dimension}.", StringComparison.Ordinal))
            {
                return false;
            }
        }

        foreach (ThemeCompatibilityFailureV2 failure in failures)
        {
            if (failure.Dimension != failures[0].Dimension)
            {
                return false;
            }
        }

        return true;
    }

    private static void ComposeRuntime(
        ThemeCompatibilityRuntimeEvidenceV2? receipt,
        List<ThemeCompatibilityFailureV2> failures,
        List<ThemeCompatibilityNoticeV2> notices,
        out ThemeCompatibilityEvidenceStatusV2 status)
    {
        if (receipt is null)
        {
            status = ThemeCompatibilityEvidenceStatusV2.NotEvaluated;
            Add(failures, ThemeCompatibilityFailureKindV2.EvidenceMissing, ThemeCompatibilityDimensionV2.Runtime);
            return;
        }

        status = receipt.Status;
        notices.AddRange(receipt.Notices);
        if (status == ThemeCompatibilityEvidenceStatusV2.NotEvaluated)
            Add(failures, ThemeCompatibilityFailureKindV2.EvidenceNotEvaluated, ThemeCompatibilityDimensionV2.Runtime);
        else if (status == ThemeCompatibilityEvidenceStatusV2.Failed)
            AppendOrDefault(failures, receipt.Failures, ThemeCompatibilityFailureKindV2.RuntimeValidationFailed,
                ThemeCompatibilityDimensionV2.Runtime);
    }

    private static void ComposeAccessibility(
        ThemeCompatibilityAccessibilityEvidenceV2? receipt,
        List<ThemeCompatibilityFailureV2> failures,
        out ThemeCompatibilityAccessibilityStatusV2 status)
    {
        if (receipt is null)
        {
            status = ThemeCompatibilityAccessibilityStatusV2.NotEvaluated;
            Add(failures, ThemeCompatibilityFailureKindV2.EvidenceMissing, ThemeCompatibilityDimensionV2.Accessibility);
            return;
        }

        status = receipt.Status;
        if (status == ThemeCompatibilityAccessibilityStatusV2.NotEvaluated)
            Add(failures, ThemeCompatibilityFailureKindV2.EvidenceNotEvaluated, ThemeCompatibilityDimensionV2.Accessibility);
        else if (status == ThemeCompatibilityAccessibilityStatusV2.Failed)
            AppendOrDefault(failures, receipt.Failures, ThemeCompatibilityFailureKindV2.AccessibilityValidationFailed,
                ThemeCompatibilityDimensionV2.Accessibility);
    }

    private static void ComposeSafety(
        ThemeCompatibilitySafetyEvidenceV2? receipt,
        List<ThemeCompatibilityFailureV2> failures,
        out ThemeCompatibilitySafetyStatusV2 status,
        out ImmutableArray<ThemeCompatibilitySafetyStatusV2> safetyFailures)
    {
        if (receipt is null)
        {
            status = ThemeCompatibilitySafetyStatusV2.NotEvaluated;
            safetyFailures = ImmutableArray<ThemeCompatibilitySafetyStatusV2>.Empty;
            Add(failures, ThemeCompatibilityFailureKindV2.EvidenceMissing, ThemeCompatibilityDimensionV2.Safety);
            return;
        }

        status = receipt.Status;
        safetyFailures = receipt.Failures;
        if (status == ThemeCompatibilitySafetyStatusV2.NotEvaluated)
        {
            Add(failures, ThemeCompatibilityFailureKindV2.EvidenceNotEvaluated, ThemeCompatibilityDimensionV2.Safety);
        }
        else if (status != ThemeCompatibilitySafetyStatusV2.Passed)
        {
            List<ThemeCompatibilityFailureV2> safety = [];
            foreach (ThemeCompatibilityFailureV2 detail in receipt.Details)
            {
                safety.Add(detail);
            }
            if (safety.Count == 0)
                Add(safety, ThemeCompatibilityFailureKindV2.SafetyValidationFailed,
                    ThemeCompatibilityDimensionV2.Safety);
            AddSafetySubstatusFailure(
                safety,
                receipt.AssetInventoryStatus,
                ThemeCompatibilityFailureKindV2.AssetInventoryInvalid);
            AddSafetySubstatusFailure(
                safety,
                receipt.MotionSafetyStatus,
                ThemeCompatibilityFailureKindV2.MotionSafetyFailed);
            AddSafetySubstatusFailure(
                safety,
                receipt.AudioSafetyStatus,
                ThemeCompatibilityFailureKindV2.AudioSafetyFailed);
            failures.AddRange(safety);
        }
    }

    private static void AddSafetySubstatusFailure(
        List<ThemeCompatibilityFailureV2> failures,
        ThemeCompatibilityEvidenceStatusV2 status,
        ThemeCompatibilityFailureKindV2 kind)
    {
        if (status != ThemeCompatibilityEvidenceStatusV2.Failed)
        {
            return;
        }

        for (int index = 0; index < failures.Count; index++)
        {
            if (failures[index].Kind == kind)
            {
                return;
            }

            if (failures[index].Kind > kind)
            {
                failures.Insert(index, new ThemeCompatibilityFailureV2(
                    kind,
                    ThemeCompatibilityDimensionV2.Safety,
                    index,
                    null,
                    $"{kind} in {ThemeCompatibilityDimensionV2.Safety}."));
                return;
            }
        }

        Add(failures, kind, ThemeCompatibilityDimensionV2.Safety);
    }

    private static void ComposeCapability(
        ThemeCompatibilityCapabilityEvidenceV2? receipt,
        List<ThemeCompatibilityFailureV2> failures,
        List<ThemeCompatibilityNoticeV2> notices,
        out ThemeCompatibilityEvidenceStatusV2 status,
        out ImmutableArray<string> authorized,
        out ImmutableArray<string> enabled,
        out ImmutableArray<string> disabled)
    {
        if (receipt is null)
        {
            status = ThemeCompatibilityEvidenceStatusV2.NotEvaluated;
            authorized = ImmutableArray<string>.Empty;
            enabled = ImmutableArray<string>.Empty;
            disabled = ImmutableArray<string>.Empty;
            Add(failures, ThemeCompatibilityFailureKindV2.EvidenceMissing, ThemeCompatibilityDimensionV2.Capability);
            return;
        }

        status = receipt.Status;
        authorized = receipt.AuthorizedCapabilities;
        enabled = receipt.EnabledCapabilities;
        disabled = receipt.DisabledCapabilities;
        notices.AddRange(receipt.Notices);
        if (status == ThemeCompatibilityEvidenceStatusV2.NotEvaluated)
            Add(failures, ThemeCompatibilityFailureKindV2.EvidenceNotEvaluated, ThemeCompatibilityDimensionV2.Capability);
        else if (status == ThemeCompatibilityEvidenceStatusV2.Failed)
            AppendOrDefault(failures, receipt.Failures, ThemeCompatibilityFailureKindV2.CapabilityBlocked,
                ThemeCompatibilityDimensionV2.Capability);
    }

    private static void ComposeMigration(
        ThemeCompatibilityMigrationEvidenceV2? receipt,
        List<ThemeCompatibilityFailureV2> failures,
        out ThemeCompatibilityEvidenceStatusV2 status,
        out bool? required,
        out bool? ready)
    {
        if (receipt is null)
        {
            status = ThemeCompatibilityEvidenceStatusV2.NotEvaluated;
            required = null;
            ready = null;
            Add(failures, ThemeCompatibilityFailureKindV2.EvidenceMissing, ThemeCompatibilityDimensionV2.Migration);
            return;
        }

        status = receipt.Status;
        required = receipt.MigrationRequired;
        ready = receipt.MigrationReady;
        if (status == ThemeCompatibilityEvidenceStatusV2.NotEvaluated)
            Add(failures, ThemeCompatibilityFailureKindV2.EvidenceNotEvaluated, ThemeCompatibilityDimensionV2.Migration);
        else if (status == ThemeCompatibilityEvidenceStatusV2.Failed || required == true && ready != true)
            AppendOrDefault(failures, receipt.Failures, ThemeCompatibilityFailureKindV2.MigrationNotReady,
                ThemeCompatibilityDimensionV2.Migration);
    }

    private static void ComposeRollback(
        ThemeCompatibilityRollbackEvidenceV2? receipt,
        List<ThemeCompatibilityFailureV2> failures,
        out ThemeCompatibilityEvidenceStatusV2 status,
        out bool? required,
        out bool? available)
    {
        if (receipt is null)
        {
            status = ThemeCompatibilityEvidenceStatusV2.NotEvaluated;
            required = null;
            available = null;
            Add(failures, ThemeCompatibilityFailureKindV2.EvidenceMissing, ThemeCompatibilityDimensionV2.Rollback);
            return;
        }

        status = receipt.Status;
        required = receipt.RollbackRequired;
        available = receipt.RollbackAvailable;
        if (status == ThemeCompatibilityEvidenceStatusV2.NotEvaluated)
            Add(failures, ThemeCompatibilityFailureKindV2.EvidenceNotEvaluated, ThemeCompatibilityDimensionV2.Rollback);
        else if (status == ThemeCompatibilityEvidenceStatusV2.Failed || required == true && available != true)
            AppendOrDefault(failures, receipt.Failures, ThemeCompatibilityFailureKindV2.RollbackUnavailable,
                ThemeCompatibilityDimensionV2.Rollback);
    }

    private static void AppendOrDefault(
        List<ThemeCompatibilityFailureV2> destination,
        ImmutableArray<ThemeCompatibilityFailureV2> source,
        ThemeCompatibilityFailureKindV2 fallback,
        ThemeCompatibilityDimensionV2 dimension)
    {
        if (source.IsEmpty) Add(destination, fallback, dimension);
        else destination.AddRange(source);
    }

    private static void Add(
        List<ThemeCompatibilityFailureV2> failures,
        ThemeCompatibilityFailureKindV2 kind,
        ThemeCompatibilityDimensionV2 dimension) =>
        failures.Add(new ThemeCompatibilityFailureV2(kind, dimension, failures.Count, null, $"{kind} in {dimension}."));

    private static ImmutableArray<ThemeCompatibilityFailureV2> RebuildFailures(
        IEnumerable<ThemeCompatibilityFailureV2> failures)
    {
        ImmutableArray<ThemeCompatibilityFailureV2>.Builder result = ImmutableArray.CreateBuilder<ThemeCompatibilityFailureV2>();
        foreach (ThemeCompatibilityFailureV2 failure in failures)
        {
            result.Add(new ThemeCompatibilityFailureV2(
                failure.Kind, failure.Dimension, result.Count, null, $"{failure.Kind} in {failure.Dimension}."));
        }

        return result.ToImmutable();
    }

    private static ImmutableArray<ThemeCompatibilityNoticeV2> RebuildNotices(
        IEnumerable<ThemeCompatibilityNoticeV2> notices)
    {
        ImmutableArray<ThemeCompatibilityNoticeV2>.Builder result = ImmutableArray.CreateBuilder<ThemeCompatibilityNoticeV2>();
        foreach (ThemeCompatibilityNoticeV2 notice in notices)
        {
            result.Add(new ThemeCompatibilityNoticeV2(
                notice.Kind, notice.Dimension, result.Count, null, $"{notice.Kind} in {notice.Dimension}."));
        }

        return result.ToImmutable();
    }

    private static (ThemeCompatibilityFailureKindV2 Kind, ThemeCompatibilityDimensionV2 Dimension) Map(
        ThemeCompatibilityNegotiationFailureKind failure) => failure switch
        {
            ThemeCompatibilityNegotiationFailureKind.InvalidVersionInput =>
                (ThemeCompatibilityFailureKindV2.InvalidVersionInput, ThemeCompatibilityDimensionV2.VersionDeclarations),
            ThemeCompatibilityNegotiationFailureKind.UnsatisfiableVersionRange =>
                (ThemeCompatibilityFailureKindV2.UnsatisfiableVersionRange, ThemeCompatibilityDimensionV2.VersionDeclarations),
            ThemeCompatibilityNegotiationFailureKind.UnsupportedManifestSchema =>
                (ThemeCompatibilityFailureKindV2.UnsupportedManifestSchema, ThemeCompatibilityDimensionV2.ManifestSchema),
            ThemeCompatibilityNegotiationFailureKind.ConflictingVersionDeclaration =>
                (ThemeCompatibilityFailureKindV2.ConflictingVersionDeclaration, ThemeCompatibilityDimensionV2.VersionDeclarations),
            ThemeCompatibilityNegotiationFailureKind.CoreVersionIncompatible =>
                (ThemeCompatibilityFailureKindV2.CoreVersionIncompatible, ThemeCompatibilityDimensionV2.CoreVersion),
            ThemeCompatibilityNegotiationFailureKind.ThemeApiNoCompatibleVersion =>
                (ThemeCompatibilityFailureKindV2.ThemeApiNoCompatibleVersion, ThemeCompatibilityDimensionV2.ThemeApiVersion),
            ThemeCompatibilityNegotiationFailureKind.UxContractNoCompatibleVersion =>
                (ThemeCompatibilityFailureKindV2.UxContractNoCompatibleVersion, ThemeCompatibilityDimensionV2.UxContractVersion),
            _ => throw new InvalidOperationException("Unknown sealed Phase5A failure."),
        };

    private static ThemeCompatibilityStatusV2 Primary(ThemeCompatibilityFailureKindV2 kind) => kind switch
    {
        ThemeCompatibilityFailureKindV2.MissingTrustedContext or ThemeCompatibilityFailureKindV2.InvalidRuntimeContext
            or ThemeCompatibilityFailureKindV2.ContextBindingMismatch or ThemeCompatibilityFailureKindV2.InvalidSchemaSupportInput
            or ThemeCompatibilityFailureKindV2.EvidenceScopeMismatch or ThemeCompatibilityFailureKindV2.EvidenceMalformed =>
            ThemeCompatibilityStatusV2.RefusedPrecondition,
        ThemeCompatibilityFailureKindV2.ContentSnapshotUnavailable or ThemeCompatibilityFailureKindV2.IntegrityNotVerified
            or ThemeCompatibilityFailureKindV2.ContentEvidenceMismatch => ThemeCompatibilityStatusV2.TrustedEvidenceFailure,
        ThemeCompatibilityFailureKindV2.UnsupportedManifestSchema => ThemeCompatibilityStatusV2.UnsupportedManifestSchema,
        ThemeCompatibilityFailureKindV2.ManifestSchemaInvalid => ThemeCompatibilityStatusV2.ManifestSchemaInvalid,
        ThemeCompatibilityFailureKindV2.InvalidVersionInput => ThemeCompatibilityStatusV2.InvalidVersionInput,
        ThemeCompatibilityFailureKindV2.UnsatisfiableVersionRange => ThemeCompatibilityStatusV2.UnsatisfiableVersionRange,
        ThemeCompatibilityFailureKindV2.ConflictingVersionDeclaration => ThemeCompatibilityStatusV2.ConflictingVersionDeclaration,
        ThemeCompatibilityFailureKindV2.CoreVersionIncompatible => ThemeCompatibilityStatusV2.IncompatibleCoreVersion,
        ThemeCompatibilityFailureKindV2.ThemeApiNoCompatibleVersion => ThemeCompatibilityStatusV2.IncompatibleThemeApiVersion,
        ThemeCompatibilityFailureKindV2.UxContractNoCompatibleVersion => ThemeCompatibilityStatusV2.IncompatibleUxContractVersion,
        ThemeCompatibilityFailureKindV2.PlatformUnsupported => ThemeCompatibilityStatusV2.UnsupportedPlatform,
        ThemeCompatibilityFailureKindV2.InstallationModeUnsupported => ThemeCompatibilityStatusV2.UnsupportedInstallationMode,
        ThemeCompatibilityFailureKindV2.RuntimeValidationFailed => ThemeCompatibilityStatusV2.RuntimeValidationFailed,
        ThemeCompatibilityFailureKindV2.AccessibilityValidationFailed => ThemeCompatibilityStatusV2.AccessibilityValidationFailed,
        ThemeCompatibilityFailureKindV2.SafetyValidationFailed or ThemeCompatibilityFailureKindV2.AssetInventoryInvalid
            or ThemeCompatibilityFailureKindV2.MotionSafetyFailed or ThemeCompatibilityFailureKindV2.AudioSafetyFailed =>
            ThemeCompatibilityStatusV2.SafetyValidationFailed,
        ThemeCompatibilityFailureKindV2.CapabilityBlocked => ThemeCompatibilityStatusV2.BlockedCapability,
        ThemeCompatibilityFailureKindV2.MigrationNotReady => ThemeCompatibilityStatusV2.MigrationNotReady,
        ThemeCompatibilityFailureKindV2.RollbackUnavailable => ThemeCompatibilityStatusV2.RollbackUnavailable,
        _ => ThemeCompatibilityStatusV2.EvidenceUnavailable,
    };

    private static bool ContainsOrdinal(IEnumerable<string> values, string expected)
    {
        foreach (string value in values)
        {
            if (string.Equals(value, expected, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSha256(string value)
    {
        if (value.Length != 64)
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

    private static ThemeCompatibilityResultV2 Stopped(
        ThemeCompatibilityFailureKindV2 kind,
        ThemeCompatibilityDimensionV2 dimension,
        ThemeId? themeId = null,
        ThemeVersion? themeVersion = null,
        string? packageHash = null) =>
        Stopped(
            new[] { new ThemeCompatibilityFailureV2(kind, dimension, 0, null, $"{kind} in {dimension}.") },
            themeId,
            themeVersion,
            packageHash);

    private static ThemeCompatibilityResultV2 Stopped(
        IEnumerable<ThemeCompatibilityFailureV2> failures,
        ThemeId? themeId,
        ThemeVersion? themeVersion,
        string? packageHash)
    {
        ImmutableArray<ThemeCompatibilityFailureV2> completed = RebuildFailures(failures);
        ThemeCompatibilityStatusV2 status = Primary(completed[0].Kind);
        ThemeCompatibilityEvaluationStateV2 evaluationState = status is ThemeCompatibilityStatusV2.RefusedPrecondition
            or ThemeCompatibilityStatusV2.TrustedEvidenceFailure
                ? ThemeCompatibilityEvaluationStateV2.RefusedPrecondition
                : ThemeCompatibilityEvaluationStateV2.Evaluated;
        return new ThemeCompatibilityResultV2(
            ContractVersions.ThemeCompatibilityContractV2,
            status,
            evaluationState,
            themeId,
            themeVersion,
            packageHash,
            null,
            null,
            null,
            null,
            ThemeCompatibilityEvidenceStatusV2.NotEvaluated,
            ThemeCompatibilityEvidenceStatusV2.NotEvaluated,
            ImmutableArray<string>.Empty,
            ImmutableArray<string>.Empty,
            ImmutableArray<string>.Empty,
            ImmutableArray<ThemeCompatibilityNoticeV2>.Empty,
            completed,
            ThemeCompatibilityAccessibilityStatusV2.NotEvaluated,
            ThemeCompatibilitySafetyStatusV2.NotEvaluated,
            ImmutableArray<ThemeCompatibilitySafetyStatusV2>.Empty,
            ThemeCompatibilityEvidenceStatusV2.NotEvaluated,
            null,
            null,
            ThemeCompatibilityEvidenceStatusV2.NotEvaluated,
            null,
            null);
    }
}
