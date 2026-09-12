using System.Collections.Immutable;
using Tcc.Presentation.Contracts.Theme;

namespace Tcc.Themes.Compatibility.V2;

public sealed class ThemeCompatibilityEvidenceV2
{
    internal ThemeCompatibilityEvidenceV2(
        ThemeCompatibilityContextV2 context,
        ThemeCompatibilityEnvironmentV2 environment,
        ThemeCompatibilityRuntimeEvidenceV2? runtime,
        ThemeCompatibilityCapabilityEvidenceV2? capability,
        ThemeCompatibilityAccessibilityEvidenceV2? accessibility,
        ThemeCompatibilitySafetyEvidenceV2? safety,
        ThemeCompatibilityMigrationEvidenceV2? migration,
        ThemeCompatibilityRollbackEvidenceV2? rollback)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(environment);
        if (runtime is not null && (!ReferenceEquals(runtime.Context, context) || !ReferenceEquals(runtime.Environment, environment)))
        {
            throw new ArgumentException("Receipt binding must match the exact context and environment.", nameof(runtime));
        }
        if (capability is not null && (!ReferenceEquals(capability.Context, context) || !ReferenceEquals(capability.Environment, environment)))
        {
            throw new ArgumentException("Receipt binding must match the exact context and environment.", nameof(capability));
        }
        if (accessibility is not null && (!ReferenceEquals(accessibility.Context, context) || !ReferenceEquals(accessibility.Environment, environment)))
        {
            throw new ArgumentException("Receipt binding must match the exact context and environment.", nameof(accessibility));
        }
        if (safety is not null && (!ReferenceEquals(safety.Context, context) || !ReferenceEquals(safety.Environment, environment)))
        {
            throw new ArgumentException("Receipt binding must match the exact context and environment.", nameof(safety));
        }
        if (migration is not null && (!ReferenceEquals(migration.Context, context) || !ReferenceEquals(migration.Environment, environment)))
        {
            throw new ArgumentException("Receipt binding must match the exact context and environment.", nameof(migration));
        }
        if (rollback is not null && (!ReferenceEquals(rollback.Context, context) || !ReferenceEquals(rollback.Environment, environment)))
        {
            throw new ArgumentException("Receipt binding must match the exact context and environment.", nameof(rollback));
        }
        Context = context;
        Environment = environment;
        Runtime = runtime;
        Capability = capability;
        Accessibility = accessibility;
        Safety = safety;
        Migration = migration;
        Rollback = rollback;
    }

    internal ThemeCompatibilityContextV2 Context { get; }
    internal ThemeCompatibilityEnvironmentV2 Environment { get; }
    internal ThemeCompatibilityRuntimeEvidenceV2? Runtime { get; }
    internal ThemeCompatibilityCapabilityEvidenceV2? Capability { get; }
    internal ThemeCompatibilityAccessibilityEvidenceV2? Accessibility { get; }
    internal ThemeCompatibilitySafetyEvidenceV2? Safety { get; }
    internal ThemeCompatibilityMigrationEvidenceV2? Migration { get; }
    internal ThemeCompatibilityRollbackEvidenceV2? Rollback { get; }
}
internal sealed class ThemeCompatibilityRuntimeEvidenceV2
{
    internal ThemeCompatibilityRuntimeEvidenceV2(
        ThemeCompatibilityContextV2 context,
        ThemeCompatibilityEnvironmentV2 environment,
        ThemeCompatibilityEvidenceStatusV2 status,
        IEnumerable<ThemeCompatibilityNoticeV2> notices,
        IEnumerable<ThemeCompatibilityFailureV2> failures)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(environment);
        Context = context;
        Environment = environment;
        Status = status;
        ArgumentNullException.ThrowIfNull(notices);
        Notices = notices.ToImmutableArray();
        ArgumentNullException.ThrowIfNull(failures);
        Failures = failures.ToImmutableArray();
    }

    internal ThemeCompatibilityContextV2 Context { get; }
    internal ThemeCompatibilityEnvironmentV2 Environment { get; }
    internal ThemeCompatibilityEvidenceStatusV2 Status { get; }
    internal ImmutableArray<ThemeCompatibilityNoticeV2> Notices { get; }
    internal ImmutableArray<ThemeCompatibilityFailureV2> Failures { get; }
}

internal sealed class ThemeCompatibilityCapabilityEvidenceV2
{
    internal ThemeCompatibilityCapabilityEvidenceV2(
        ThemeCompatibilityContextV2 context,
        ThemeCompatibilityEnvironmentV2 environment,
        ThemeCompatibilityEvidenceStatusV2 status,
        IEnumerable<string> authorizedCapabilities,
        IEnumerable<string> enabledCapabilities,
        IEnumerable<string> disabledCapabilities,
        IEnumerable<ThemeCompatibilityNoticeV2> notices,
        IEnumerable<ThemeCompatibilityFailureV2> failures)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(environment);
        Context = context;
        Environment = environment;
        Status = status;
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
    }

    internal ThemeCompatibilityContextV2 Context { get; }
    internal ThemeCompatibilityEnvironmentV2 Environment { get; }
    internal ThemeCompatibilityEvidenceStatusV2 Status { get; }
    internal ImmutableArray<string> AuthorizedCapabilities { get; }
    internal ImmutableArray<string> EnabledCapabilities { get; }
    internal ImmutableArray<string> DisabledCapabilities { get; }
    internal ImmutableArray<ThemeCompatibilityNoticeV2> Notices { get; }
    internal ImmutableArray<ThemeCompatibilityFailureV2> Failures { get; }
}

internal sealed class ThemeCompatibilityAccessibilityEvidenceV2
{
    internal ThemeCompatibilityAccessibilityEvidenceV2(
        ThemeCompatibilityContextV2 context,
        ThemeCompatibilityEnvironmentV2 environment,
        ThemeCompatibilityAccessibilityStatusV2 status,
        IEnumerable<ThemeCompatibilityFailureV2> failures)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(environment);
        Context = context;
        Environment = environment;
        Status = status;
        ArgumentNullException.ThrowIfNull(failures);
        Failures = failures.ToImmutableArray();
    }

    internal ThemeCompatibilityContextV2 Context { get; }
    internal ThemeCompatibilityEnvironmentV2 Environment { get; }
    internal ThemeCompatibilityAccessibilityStatusV2 Status { get; }
    internal ImmutableArray<ThemeCompatibilityFailureV2> Failures { get; }
}

internal sealed class ThemeCompatibilitySafetyEvidenceV2
{
    internal ThemeCompatibilitySafetyEvidenceV2(
        ThemeCompatibilityContextV2 context,
        ThemeCompatibilityEnvironmentV2 environment,
        ThemeCompatibilitySafetyStatusV2 status,
        IEnumerable<ThemeCompatibilitySafetyStatusV2> failures,
        ThemeCompatibilityEvidenceStatusV2 assetInventoryStatus,
        ThemeCompatibilityEvidenceStatusV2 motionSafetyStatus,
        ThemeCompatibilityEvidenceStatusV2 audioSafetyStatus,
        IEnumerable<ThemeCompatibilityFailureV2> details)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(environment);
        Context = context;
        Environment = environment;
        Status = status;
        ArgumentNullException.ThrowIfNull(failures);
        Failures = failures.ToImmutableArray();
        AssetInventoryStatus = assetInventoryStatus;
        MotionSafetyStatus = motionSafetyStatus;
        AudioSafetyStatus = audioSafetyStatus;
        ArgumentNullException.ThrowIfNull(details);
        Details = details.ToImmutableArray();
    }

    internal ThemeCompatibilityContextV2 Context { get; }
    internal ThemeCompatibilityEnvironmentV2 Environment { get; }
    internal ThemeCompatibilitySafetyStatusV2 Status { get; }
    internal ImmutableArray<ThemeCompatibilitySafetyStatusV2> Failures { get; }
    internal ThemeCompatibilityEvidenceStatusV2 AssetInventoryStatus { get; }
    internal ThemeCompatibilityEvidenceStatusV2 MotionSafetyStatus { get; }
    internal ThemeCompatibilityEvidenceStatusV2 AudioSafetyStatus { get; }
    internal ImmutableArray<ThemeCompatibilityFailureV2> Details { get; }
}

internal sealed class ThemeCompatibilityMigrationEvidenceV2
{
    internal ThemeCompatibilityMigrationEvidenceV2(
        ThemeCompatibilityContextV2 context,
        ThemeCompatibilityEnvironmentV2 environment,
        ThemeCompatibilityEvidenceStatusV2 status,
        bool? migrationRequired,
        bool? migrationReady,
        IEnumerable<ThemeCompatibilityFailureV2> failures)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(environment);
        Context = context;
        Environment = environment;
        Status = status;
        MigrationRequired = migrationRequired;
        MigrationReady = migrationReady;
        ArgumentNullException.ThrowIfNull(failures);
        Failures = failures.ToImmutableArray();
    }

    internal ThemeCompatibilityContextV2 Context { get; }
    internal ThemeCompatibilityEnvironmentV2 Environment { get; }
    internal ThemeCompatibilityEvidenceStatusV2 Status { get; }
    internal bool? MigrationRequired { get; }
    internal bool? MigrationReady { get; }
    internal ImmutableArray<ThemeCompatibilityFailureV2> Failures { get; }
}

internal sealed class ThemeCompatibilityRollbackEvidenceV2
{
    internal ThemeCompatibilityRollbackEvidenceV2(
        ThemeCompatibilityContextV2 context,
        ThemeCompatibilityEnvironmentV2 environment,
        ThemeCompatibilityEvidenceStatusV2 status,
        bool? rollbackRequired,
        bool? rollbackAvailable,
        IEnumerable<ThemeCompatibilityFailureV2> failures)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(environment);
        Context = context;
        Environment = environment;
        Status = status;
        RollbackRequired = rollbackRequired;
        RollbackAvailable = rollbackAvailable;
        ArgumentNullException.ThrowIfNull(failures);
        Failures = failures.ToImmutableArray();
    }

    internal ThemeCompatibilityContextV2 Context { get; }
    internal ThemeCompatibilityEnvironmentV2 Environment { get; }
    internal ThemeCompatibilityEvidenceStatusV2 Status { get; }
    internal bool? RollbackRequired { get; }
    internal bool? RollbackAvailable { get; }
    internal ImmutableArray<ThemeCompatibilityFailureV2> Failures { get; }
}
