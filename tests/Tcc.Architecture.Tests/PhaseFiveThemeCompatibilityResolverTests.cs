using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Compatibility.V2;

namespace Tcc.Architecture.Tests;

public sealed class PhaseFiveThemeCompatibilityResolverTests
{
    [Fact]
    public void NullRequestThrowsAndMissingStructuralInputsRefuse()
    {
        ThemeCompatibilityResolverV2 resolver = new();
        Assert.Throws<ArgumentNullException>(() => resolver.Resolve(null!));

        ThemeCompatibilityResultV2 missingContext = resolver.Resolve(new(null, Environment(), null));
        Assert.Equal(ThemeCompatibilityStatusV2.RefusedPrecondition, missingContext.Status);
        Assert.Equal(ThemeCompatibilityFailureKindV2.MissingTrustedContext, Assert.Single(missingContext.Failures).Kind);

        ThemeCompatibilityResultV2 missingEnvironment = resolver.Resolve(new(Context(), null, null));
        Assert.Equal(ThemeCompatibilityStatusV2.RefusedPrecondition, missingEnvironment.Status);
        Assert.Equal(ThemeCompatibilityFailureKindV2.InvalidRuntimeContext, Assert.Single(missingEnvironment.Failures).Kind);
        Assert.All(new[] { missingContext, missingEnvironment }, result =>
            Assert.Equal(ThemeCompatibilityEvaluationStateV2.RefusedPrecondition, result.EvaluationState));
    }

    [Fact]
    public void EligibleMissingEvidenceProducesCanonicalSixOwnerResultAndRoundTrips()
    {
        ThemeCompatibilityContextV2 context = Context();
        ThemeCompatibilityEnvironmentV2 environment = Environment();
        ThemeCompatibilityResultV2 result = new ThemeCompatibilityResolverV2().Resolve(
            new ThemeCompatibilityRequestV2(context, environment, null));

        Assert.Equal(ThemeCompatibilityStatusV2.EvidenceUnavailable, result.Status);
        Assert.Equal(ThemeCompatibilityEvaluationStateV2.Evaluated, result.EvaluationState);
        Assert.Equal(new[]
        {
            ThemeCompatibilityDimensionV2.Runtime,
            ThemeCompatibilityDimensionV2.Accessibility,
            ThemeCompatibilityDimensionV2.Safety,
            ThemeCompatibilityDimensionV2.Capability,
            ThemeCompatibilityDimensionV2.Migration,
            ThemeCompatibilityDimensionV2.Rollback,
        }, result.Failures.Select(failure => failure.Dimension));
        Assert.All(result.Failures, failure => Assert.Equal(ThemeCompatibilityFailureKindV2.EvidenceMissing, failure.Kind));
        Assert.DoesNotContain(result.Failures, failure => failure.Dimension == ThemeCompatibilityDimensionV2.Precondition);
        Assert.Equal(Enumerable.Range(0, 6), result.Failures.Select(failure => failure.Sequence));
        Assert.Equal("1.0.0", result.SelectedCoreVersion);
        Assert.Equal("1.0.0", result.SelectedThemeApiVersion);
        Assert.Equal("1.1.0", result.SelectedUxContractVersion);
        Assert.Equal("1.0", result.SelectedManifestSchemaVersion);
        Assert.Equal(new ThemeId("com.example.theme"), result.ThemeId);
        Assert.Equal(new ThemeVersion("1.2.3"), result.ThemeVersion);
        Assert.Equal(new string('a', 64), result.PackageHash);

        string first = ThemeCompatibilityJsonV2.SerializeResult(result);
        ThemeCompatibilityResultV2 transported = ThemeCompatibilityJsonV2.DeserializeResult(first);
        Assert.Equal(first, ThemeCompatibilityJsonV2.SerializeResult(transported));
    }

    [Fact]
    public void CompleteOwnerEvidenceProducesCompatibleResultWithoutManufacturingFacts()
    {
        ThemeCompatibilityContextV2 context = Context();
        ThemeCompatibilityEnvironmentV2 environment = Environment();
        ThemeCompatibilityEvidenceV2 evidence = Evidence(context, environment);

        ThemeCompatibilityResultV2 result = new ThemeCompatibilityResolverV2().Resolve(new(context, environment, evidence));

        Assert.Equal(ThemeCompatibilityStatusV2.Compatible, result.Status);
        Assert.Empty(result.Failures);
        Assert.Empty(result.Notices);
        Assert.Equal(ThemeCompatibilityEvidenceStatusV2.Passed, result.RuntimeValidationStatus);
        Assert.Equal(ThemeCompatibilityEvidenceStatusV2.Passed, result.CapabilityEvidenceStatus);
        Assert.Equal(ThemeCompatibilityAccessibilityStatusV2.Validated, result.AccessibilityValidationStatus);
        Assert.Equal(ThemeCompatibilitySafetyStatusV2.Passed, result.SafetyInvariantsStatus);
        Assert.False(result.MigrationRequired);
        Assert.Null(result.MigrationReady);
        Assert.False(result.RollbackRequired);
        Assert.Null(result.RollbackAvailable);
        _ = ThemeCompatibilityJsonV2.SerializeResult(result);
    }

    [Theory]
    [InlineData(0, ThemeCompatibilityDimensionV2.Runtime)]
    [InlineData(1, ThemeCompatibilityDimensionV2.Accessibility)]
    [InlineData(2, ThemeCompatibilityDimensionV2.Safety)]
    [InlineData(3, ThemeCompatibilityDimensionV2.Capability)]
    [InlineData(4, ThemeCompatibilityDimensionV2.Migration)]
    [InlineData(5, ThemeCompatibilityDimensionV2.Rollback)]
    public void EveryMissingOwnerFamilyIsReportedInItsOwnDimension(int missingSlot, ThemeCompatibilityDimensionV2 expected)
    {
        ThemeCompatibilityContextV2 context = Context();
        ThemeCompatibilityEnvironmentV2 environment = Environment();
        ThemeCompatibilityResultV2 result = new ThemeCompatibilityResolverV2().Resolve(
            new(context, environment, Evidence(context, environment, missingSlot)));

        ThemeCompatibilityFailureV2 failure = Assert.Single(result.Failures);
        Assert.Equal(ThemeCompatibilityFailureKindV2.EvidenceMissing, failure.Kind);
        Assert.Equal(expected, failure.Dimension);
        Assert.Equal(ThemeCompatibilityStatusV2.EvidenceUnavailable, result.Status);
    }

    [Theory]
    [InlineData(0, ThemeCompatibilityDimensionV2.Runtime, ThemeCompatibilityFailureKindV2.RuntimeValidationFailed)]
    [InlineData(1, ThemeCompatibilityDimensionV2.Accessibility, ThemeCompatibilityFailureKindV2.AccessibilityValidationFailed)]
    [InlineData(2, ThemeCompatibilityDimensionV2.Safety, ThemeCompatibilityFailureKindV2.SafetyValidationFailed)]
    [InlineData(3, ThemeCompatibilityDimensionV2.Capability, ThemeCompatibilityFailureKindV2.CapabilityBlocked)]
    [InlineData(4, ThemeCompatibilityDimensionV2.Migration, ThemeCompatibilityFailureKindV2.MigrationNotReady)]
    [InlineData(5, ThemeCompatibilityDimensionV2.Rollback, ThemeCompatibilityFailureKindV2.RollbackUnavailable)]
    public void EveryOwnerFamilyComposesNotEvaluatedFailedMalformedAndPassed(
        int ownerSlot,
        ThemeCompatibilityDimensionV2 dimension,
        ThemeCompatibilityFailureKindV2 failedKind)
    {
        ThemeCompatibilityContextV2 context = Context();
        ThemeCompatibilityEnvironmentV2 environment = Environment();
        ThemeCompatibilityResolverV2 resolver = new();

        ThemeCompatibilityResultV2 notEvaluated = resolver.Resolve(
            new(context, environment, OwnerEvidence(context, environment, ownerSlot, 0)));
        ThemeCompatibilityFailureV2 unavailable = Assert.Single(notEvaluated.Failures);
        Assert.Equal(ThemeCompatibilityFailureKindV2.EvidenceNotEvaluated, unavailable.Kind);
        Assert.Equal(dimension, unavailable.Dimension);

        ThemeCompatibilityResultV2 failed = resolver.Resolve(
            new(context, environment, OwnerEvidence(context, environment, ownerSlot, 1)));
        ThemeCompatibilityFailureV2 ownerFailure = Assert.Single(failed.Failures);
        Assert.Equal(failedKind, ownerFailure.Kind);
        Assert.Equal(dimension, ownerFailure.Dimension);

        ThemeCompatibilityResultV2 malformed = resolver.Resolve(
            new(context, environment, OwnerEvidence(context, environment, ownerSlot, 2)));
        ThemeCompatibilityFailureV2 malformedFailure = Assert.Single(malformed.Failures);
        Assert.Equal(ThemeCompatibilityFailureKindV2.EvidenceMalformed, malformedFailure.Kind);
        Assert.Equal(dimension, malformedFailure.Dimension);
        Assert.Equal(ThemeCompatibilityStatusV2.RefusedPrecondition, malformed.Status);

        ThemeCompatibilityResultV2 passed = resolver.Resolve(
            new(context, environment, OwnerEvidence(context, environment, ownerSlot, 3)));
        Assert.Equal(ThemeCompatibilityStatusV2.Compatible, passed.Status);
        Assert.Empty(passed.Failures);
    }

    [Theory]
    [InlineData(0, "Runtime", ThemeCompatibilityDimensionV2.Runtime)]
    [InlineData(1, "Accessibility", ThemeCompatibilityDimensionV2.Accessibility)]
    [InlineData(2, "Safety", ThemeCompatibilityDimensionV2.Safety)]
    [InlineData(3, "Capability", ThemeCompatibilityDimensionV2.Capability)]
    [InlineData(4, "Migration", ThemeCompatibilityDimensionV2.Migration)]
    [InlineData(5, "Rollback", ThemeCompatibilityDimensionV2.Rollback)]
    public void EveryOwnerReceiptScopeMismatchFailsClosed(
        int ownerSlot,
        string propertyName,
        ThemeCompatibilityDimensionV2 dimension)
    {
        ThemeCompatibilityContextV2 context = Context();
        ThemeCompatibilityContextV2 foreignContext = Context();
        ThemeCompatibilityEnvironmentV2 environment = Environment();
        ThemeCompatibilityEvidenceV2 evidence = OwnerEvidence(context, environment, ownerSlot, 3);
        ThemeCompatibilityEvidenceV2 foreign = OwnerEvidence(foreignContext, environment, ownerSlot, 3);
        object foreignReceipt = typeof(ThemeCompatibilityEvidenceV2)
            .GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(foreign)!;
        typeof(ThemeCompatibilityEvidenceV2)
            .GetField($"<{propertyName}>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(evidence, foreignReceipt);

        ThemeCompatibilityResultV2 result = new ThemeCompatibilityResolverV2().Resolve(
            new(context, environment, evidence));

        ThemeCompatibilityFailureV2 failure = Assert.Single(result.Failures);
        Assert.Equal(ThemeCompatibilityFailureKindV2.EvidenceScopeMismatch, failure.Kind);
        Assert.Equal(dimension, failure.Dimension);
    }

    [Fact]
    public void ContextAndOperationMismatchesRefuseBeforeNegotiation()
    {
        ThemeCompatibilityContextV2 first = Context();
        ThemeCompatibilityContextV2 second = Context();
        ThemeCompatibilityEnvironmentV2 environment = Environment();
        ThemeCompatibilityEvidenceV2 evidence = Evidence(first, environment);
        ThemeCompatibilityResultV2 mismatch = new ThemeCompatibilityResolverV2().Resolve(
            new(second, environment, evidence));
        Assert.Equal(ThemeCompatibilityFailureKindV2.ContextBindingMismatch, Assert.Single(mismatch.Failures).Kind);

        ThemeCompatibilityEnvironmentV2 invalidTransition = Environment(
            operation: ThemeCompatibilityOperationV2.TransitionEvaluation);
        ThemeCompatibilityResultV2 operation = new ThemeCompatibilityResolverV2().Resolve(
            new(first, invalidTransition, null));
        Assert.Equal(ThemeCompatibilityFailureKindV2.InvalidRuntimeContext, Assert.Single(operation.Failures).Kind);
    }

    [Fact]
    public void ReceiptStructuralPreconditionsExecuteBeforePhaseFiveANegotiation()
    {
        Assembly fixture = PhaseThreeScopeBoundaryTests.BuildInstrumentedResolverFixtureAssembly();
        ThemeCompatibilityEnvironmentV2 environment = Environment();
        object context = FixtureContext(fixture);

        Assert.Equal(1, ExecuteFixtureResolver(fixture, context, environment, null));

        object notEvaluated = FixtureNew(fixture,
            "Tcc.Themes.Compatibility.V2.ThemeCompatibilityRuntimeEvidenceV2",
            context, environment, ThemeCompatibilityEvidenceStatusV2.NotEvaluated,
            Array.Empty<ThemeCompatibilityNoticeV2>(), Array.Empty<ThemeCompatibilityFailureV2>());
        Assert.Equal(1, ExecuteFixtureResolver(
            fixture, context, environment, FixtureEvidence(fixture, context, environment, runtime: notEvaluated)));

        object failed = FixtureNew(fixture,
            "Tcc.Themes.Compatibility.V2.ThemeCompatibilityRuntimeEvidenceV2",
            context, environment, ThemeCompatibilityEvidenceStatusV2.Failed,
            Array.Empty<ThemeCompatibilityNoticeV2>(),
            new[] { Failure(ThemeCompatibilityFailureKindV2.RuntimeValidationFailed, ThemeCompatibilityDimensionV2.Runtime, 0) });
        Assert.Equal(1, ExecuteFixtureResolver(
            fixture, context, environment, FixtureEvidence(fixture, context, environment, runtime: failed)));

        object malformedRuntime = FixtureNew(fixture,
            "Tcc.Themes.Compatibility.V2.ThemeCompatibilityRuntimeEvidenceV2",
            context, environment, ThemeCompatibilityEvidenceStatusV2.NotApplicable,
            Array.Empty<ThemeCompatibilityNoticeV2>(), Array.Empty<ThemeCompatibilityFailureV2>());
        Assert.Equal(0, ExecuteFixtureResolver(
            fixture, context, environment, FixtureEvidence(fixture, context, environment, runtime: malformedRuntime)));

        object otherContext = FixtureContext(fixture);
        object validEvidence = FixtureEvidence(fixture, context, environment, runtime: notEvaluated);
        Assert.Equal(0, ExecuteFixtureResolver(fixture, otherContext, environment, validEvidence));
        ThemeCompatibilityEnvironmentV2 otherEnvironment = Environment();
        Assert.Equal(0, ExecuteFixtureResolver(fixture, context, otherEnvironment, validEvidence));

        object foreignContextRuntime = FixtureNew(fixture,
            "Tcc.Themes.Compatibility.V2.ThemeCompatibilityRuntimeEvidenceV2",
            otherContext, environment, ThemeCompatibilityEvidenceStatusV2.NotEvaluated,
            Array.Empty<ThemeCompatibilityNoticeV2>(), Array.Empty<ThemeCompatibilityFailureV2>());
        object receiptContextMismatch = FixtureEvidence(fixture, context, environment, runtime: notEvaluated);
        SetFixtureReceipt(receiptContextMismatch, "Runtime", foreignContextRuntime);
        Assert.Equal(0, ExecuteFixtureResolver(fixture, context, environment, receiptContextMismatch));

        object foreignEnvironmentRuntime = FixtureNew(fixture,
            "Tcc.Themes.Compatibility.V2.ThemeCompatibilityRuntimeEvidenceV2",
            context, otherEnvironment, ThemeCompatibilityEvidenceStatusV2.NotEvaluated,
            Array.Empty<ThemeCompatibilityNoticeV2>(), Array.Empty<ThemeCompatibilityFailureV2>());
        object receiptEnvironmentMismatch = FixtureEvidence(fixture, context, environment, runtime: notEvaluated);
        SetFixtureReceipt(receiptEnvironmentMismatch, "Runtime", foreignEnvironmentRuntime);
        Assert.Equal(0, ExecuteFixtureResolver(fixture, context, environment, receiptEnvironmentMismatch));

        object malformedMigration = FixtureNew(fixture,
            "Tcc.Themes.Compatibility.V2.ThemeCompatibilityMigrationEvidenceV2",
            context, environment, ThemeCompatibilityEvidenceStatusV2.Failed, false, null,
            Array.Empty<ThemeCompatibilityFailureV2>());
        Assert.Equal(0, ExecuteFixtureResolver(
            fixture, context, environment, FixtureEvidence(fixture, context, environment, migration: malformedMigration)));

        object malformedRollback = FixtureNew(fixture,
            "Tcc.Themes.Compatibility.V2.ThemeCompatibilityRollbackEvidenceV2",
            context, environment, ThemeCompatibilityEvidenceStatusV2.Failed, false, true,
            Array.Empty<ThemeCompatibilityFailureV2>());
        Assert.Equal(0, ExecuteFixtureResolver(
            fixture, context, environment, FixtureEvidence(fixture, context, environment, rollback: malformedRollback)));

        object malformedSafety = FixtureNew(fixture,
            "Tcc.Themes.Compatibility.V2.ThemeCompatibilitySafetyEvidenceV2",
            context, environment, ThemeCompatibilitySafetyStatusV2.Passed,
            Array.Empty<ThemeCompatibilitySafetyStatusV2>(),
            ThemeCompatibilityEvidenceStatusV2.Failed,
            ThemeCompatibilityEvidenceStatusV2.Passed,
            ThemeCompatibilityEvidenceStatusV2.NotApplicable,
            Array.Empty<ThemeCompatibilityFailureV2>());
        Assert.Equal(0, ExecuteFixtureResolver(
            fixture, context, environment, FixtureEvidence(fixture, context, environment, safety: malformedSafety)));
    }

    [Theory]
    [InlineData(null, ThemeCompatibilityFailureKindV2.InvalidSchemaSupportInput, ThemeCompatibilityStatusV2.RefusedPrecondition)]
    [InlineData("", ThemeCompatibilityFailureKindV2.UnsupportedManifestSchema, ThemeCompatibilityStatusV2.UnsupportedManifestSchema)]
    [InlineData("1.0.0", ThemeCompatibilityFailureKindV2.InvalidSchemaSupportInput, ThemeCompatibilityStatusV2.RefusedPrecondition)]
    [InlineData("2.0", ThemeCompatibilityFailureKindV2.UnsupportedManifestSchema, ThemeCompatibilityStatusV2.UnsupportedManifestSchema)]
    public void SchemaSupportIsFailClosed(string? value, ThemeCompatibilityFailureKindV2 kind, ThemeCompatibilityStatusV2 status)
    {
        IEnumerable<string?>? support = value switch
        {
            null => null,
            "" => Array.Empty<string?>(),
            _ => new[] { value },
        };
        ThemeCompatibilityResultV2 result = new ThemeCompatibilityResolverV2().Resolve(
            new(Context(), Environment(schemaSupport: support, nullSchemaSupport: value is null), null));
        Assert.Equal(kind, Assert.Single(result.Failures).Kind);
        Assert.Equal(status, result.Status);
        Assert.Null(result.SelectedManifestSchemaVersion);
    }

    [Theory]
    [InlineData("bad", ThemeCompatibilityFailureKindV2.InvalidVersionInput, ThemeCompatibilityStatusV2.InvalidVersionInput)]
    [InlineData("2.0.0", ThemeCompatibilityFailureKindV2.CoreVersionIncompatible, ThemeCompatibilityStatusV2.IncompatibleCoreVersion)]
    public void PhaseFiveAFailuresMapWithoutInventingCoreSelection(
        string coreVersion,
        ThemeCompatibilityFailureKindV2 expected,
        ThemeCompatibilityStatusV2 status)
    {
        ThemeCompatibilityResultV2 result = new ThemeCompatibilityResolverV2().Resolve(
            new(Context(), Environment(coreVersion: coreVersion), null));
        Assert.Equal(expected, result.Failures[0].Kind);
        Assert.Equal(status, result.Status);
        Assert.Null(result.SelectedCoreVersion);
        Assert.Equal("1.0", result.SelectedManifestSchemaVersion);
    }

    [Theory]
    [InlineData(0, ThemeCompatibilityFailureKindV2.InvalidVersionInput)]
    [InlineData(1, ThemeCompatibilityFailureKindV2.UnsatisfiableVersionRange)]
    [InlineData(2, ThemeCompatibilityFailureKindV2.UnsupportedManifestSchema)]
    [InlineData(3, ThemeCompatibilityFailureKindV2.ConflictingVersionDeclaration)]
    [InlineData(4, ThemeCompatibilityFailureKindV2.CoreVersionIncompatible)]
    [InlineData(5, ThemeCompatibilityFailureKindV2.ThemeApiNoCompatibleVersion)]
    [InlineData(6, ThemeCompatibilityFailureKindV2.UxContractNoCompatibleVersion)]
    public void EveryPhaseFiveAFailureIsMappedDeterministically(
        int scenario,
        ThemeCompatibilityFailureKindV2 expected)
    {
        ThemeManifest manifest = Manifest();
        ThemeCompatibilityManifest compatibility = Compatibility();
        ThemeCompatibilityEnvironmentV2 environment = Environment();
        switch (scenario)
        {
            case 0: environment = Environment(coreVersion: "bad"); break;
            case 1: compatibility = compatibility with { Core = new(">=2.0.0 <1.0.0", ["1.0.0"]) }; break;
            case 2: compatibility = compatibility with { SchemaVersion = "2.0" }; break;
            case 3: manifest = manifest with { Compatibility = manifest.Compatibility with { RequiredThemeApiVersion = ">=1.1.0 <2.0.0" } }; break;
            case 4: environment = Environment(coreVersion: "2.0.0"); break;
            case 5: environment = Environment(themeApiVersions: ["9.0.0"]); break;
            case 6: environment = Environment(uxContractVersions: ["9.0.0"]); break;
        }

        ThemeCompatibilityResultV2 result = new ThemeCompatibilityResolverV2().Resolve(
            new(Context(manifest, compatibility), environment, null));

        Assert.Equal(expected, result.Failures[0].Kind);
        Assert.Equal(0, result.Failures[0].Sequence);
    }

    [Fact]
    public void OwnerNoticeOrderAndFallbackPrecedenceAreStable()
    {
        ThemeCompatibilityContextV2 context = Context();
        ThemeCompatibilityEnvironmentV2 environment = Environment();
        ThemeCompatibilityNoticeV2 runtimeNotice = Notice(
            ThemeCompatibilityNoticeKindV2.UntestedDpiScalableFallback,
            ThemeCompatibilityDimensionV2.Runtime,
            0);
        ThemeCompatibilityNoticeV2 capabilityNotice = Notice(
            ThemeCompatibilityNoticeKindV2.OptionalCapabilityDisabled,
            ThemeCompatibilityDimensionV2.Capability,
            0);
        ThemeCompatibilityEvidenceV2 evidence = Evidence(
            context,
            environment,
            runtime: new(context, environment, ThemeCompatibilityEvidenceStatusV2.Passed,
                [runtimeNotice], []),
            capability: new(context, environment, ThemeCompatibilityEvidenceStatusV2.Passed,
                ["presentation.tokens"], ["presentation.tokens"], [], [capabilityNotice], []));

        ThemeCompatibilityResultV2 result = new ThemeCompatibilityResolverV2().Resolve(new(context, environment, evidence));
        Assert.Equal(ThemeCompatibilityStatusV2.CompatibleUntestedDpiWithScalableFallback, result.Status);
        Assert.Collection(result.Notices,
            notice => { Assert.Equal(runtimeNotice.Kind, notice.Kind); Assert.Equal(0, notice.Sequence); },
            notice => { Assert.Equal(capabilityNotice.Kind, notice.Kind); Assert.Equal(1, notice.Sequence); });
    }

    [Fact]
    public void BlockingCapabilityFailureDominatesNotices()
    {
        ThemeCompatibilityContextV2 context = Context();
        ThemeCompatibilityEnvironmentV2 environment = Environment();
        ThemeCompatibilityFailureV2 blocked = Failure(
            ThemeCompatibilityFailureKindV2.CapabilityBlocked,
            ThemeCompatibilityDimensionV2.Capability,
            0);
        ThemeCompatibilityCapabilityEvidenceV2 capability = new(
            context,
            environment,
            ThemeCompatibilityEvidenceStatusV2.Failed,
            ["presentation.tokens"],
            [],
            ["presentation.tokens"],
            [Notice(ThemeCompatibilityNoticeKindV2.OptionalCapabilityDisabled,
                ThemeCompatibilityDimensionV2.Capability, 0)],
            [blocked]);
        ThemeCompatibilityResultV2 result = new ThemeCompatibilityResolverV2().Resolve(
            new(context, environment, Evidence(context, environment, capability: capability)));

        Assert.Equal(ThemeCompatibilityStatusV2.BlockedCapability, result.Status);
        Assert.Equal(ThemeCompatibilityFailureKindV2.CapabilityBlocked, Assert.Single(result.Failures).Kind);
    }

    [Fact]
    public void MigrationAndRollbackStatusFactMatricesMatchCanonicalTransport()
    {
        ThemeCompatibilityEvidenceStatusV2[] statuses = Enum.GetValues<ThemeCompatibilityEvidenceStatusV2>();
        bool?[] facts = [null, false, true];
        foreach (ThemeCompatibilityEvidenceStatusV2 status in statuses)
        foreach (bool? required in facts)
        foreach (bool? value in facts)
        {
            ThemeCompatibilityContextV2 migrationContext = Context();
            ThemeCompatibilityEnvironmentV2 migrationEnvironment = Environment();
            ThemeCompatibilityMigrationEvidenceV2 migration = new(
                migrationContext, migrationEnvironment, status, required, value, []);
            ThemeCompatibilityResultV2 migrationResult = new ThemeCompatibilityResolverV2().Resolve(new(
                migrationContext, migrationEnvironment,
                Evidence(migrationContext, migrationEnvironment, migration: migration)));
            AssertMatrixOutcome(
                migrationResult,
                IsValidMigrationTuple(status, required, value),
                ThemeCompatibilityDimensionV2.Migration);

            ThemeCompatibilityContextV2 rollbackContext = Context();
            ThemeCompatibilityEnvironmentV2 rollbackEnvironment = Environment();
            ThemeCompatibilityRollbackEvidenceV2 rollback = new(
                rollbackContext, rollbackEnvironment, status, required, value, []);
            ThemeCompatibilityResultV2 rollbackResult = new ThemeCompatibilityResolverV2().Resolve(new(
                rollbackContext, rollbackEnvironment,
                Evidence(rollbackContext, rollbackEnvironment, rollback: rollback)));
            AssertMatrixOutcome(
                rollbackResult,
                IsValidRollbackTuple(status, required, value),
                ThemeCompatibilityDimensionV2.Rollback);
        }
    }

    [Theory]
    [InlineData(true, false, null)]
    [InlineData(true, true, true)]
    [InlineData(false, false, null)]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    public void SixIndependentTransitionContradictionsAreRefused(
        bool migration,
        bool required,
        bool? value)
    {
        ThemeCompatibilityContextV2 context = Context();
        ThemeCompatibilityEnvironmentV2 environment = Environment();
        ThemeCompatibilityEvidenceV2 evidence = migration
            ? Evidence(context, environment, migration: new(
                context, environment, ThemeCompatibilityEvidenceStatusV2.Failed, required, value, []))
            : Evidence(context, environment, rollback: new(
                context, environment, ThemeCompatibilityEvidenceStatusV2.Failed, required, value, []));

        ThemeCompatibilityResultV2 result = new ThemeCompatibilityResolverV2().Resolve(new(context, environment, evidence));

        ThemeCompatibilityFailureV2 failure = Assert.Single(result.Failures);
        Assert.Equal(ThemeCompatibilityFailureKindV2.EvidenceMalformed, failure.Kind);
        Assert.Equal(
            migration ? ThemeCompatibilityDimensionV2.Migration : ThemeCompatibilityDimensionV2.Rollback,
            failure.Dimension);
        Assert.Equal(ThemeCompatibilityStatusV2.RefusedPrecondition, result.Status);
    }

    [Fact]
    public void SafetyDetailsAndAllExplicitFailedSubstatusesArePreservedInCanonicalOrder()
    {
        ThemeCompatibilityContextV2 context = Context();
        ThemeCompatibilityEnvironmentV2 environment = Environment();
        ThemeCompatibilitySafetyEvidenceV2 safety = new(
            context,
            environment,
            ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore,
            [ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore],
            ThemeCompatibilityEvidenceStatusV2.Failed,
            ThemeCompatibilityEvidenceStatusV2.Failed,
            ThemeCompatibilityEvidenceStatusV2.Failed,
            [Failure(ThemeCompatibilityFailureKindV2.SafetyValidationFailed, ThemeCompatibilityDimensionV2.Safety, 0)]);

        ThemeCompatibilityResultV2 result = new ThemeCompatibilityResolverV2().Resolve(new(
            context, environment, Evidence(context, environment, safety: safety)));

        Assert.Equal(new[]
        {
            ThemeCompatibilityFailureKindV2.SafetyValidationFailed,
            ThemeCompatibilityFailureKindV2.AssetInventoryInvalid,
            ThemeCompatibilityFailureKindV2.MotionSafetyFailed,
            ThemeCompatibilityFailureKindV2.AudioSafetyFailed,
        }, result.Failures.Select(failure => failure.Kind));
        AssertCanonicalRoundTrip(result);
    }

    [Fact]
    public void SafetySubstatusCrossProductPreservesFailuresAndRejectsContradictions()
    {
        var scenarios = new[]
        {
            new { Status = ThemeCompatibilitySafetyStatusV2.Passed, SafetyFailures = Array.Empty<ThemeCompatibilitySafetyStatusV2>(), Asset = ThemeCompatibilityEvidenceStatusV2.Passed, Motion = ThemeCompatibilityEvidenceStatusV2.Passed, Audio = ThemeCompatibilityEvidenceStatusV2.Passed, Details = Array.Empty<ThemeCompatibilityFailureV2>(), Valid = true },
            new { Status = ThemeCompatibilitySafetyStatusV2.Passed, SafetyFailures = Array.Empty<ThemeCompatibilitySafetyStatusV2>(), Asset = ThemeCompatibilityEvidenceStatusV2.Passed, Motion = ThemeCompatibilityEvidenceStatusV2.Passed, Audio = ThemeCompatibilityEvidenceStatusV2.NotApplicable, Details = Array.Empty<ThemeCompatibilityFailureV2>(), Valid = true },
            new { Status = ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore, SafetyFailures = new[] { ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore }, Asset = ThemeCompatibilityEvidenceStatusV2.Failed, Motion = ThemeCompatibilityEvidenceStatusV2.Passed, Audio = ThemeCompatibilityEvidenceStatusV2.NotApplicable, Details = Array.Empty<ThemeCompatibilityFailureV2>(), Valid = true },
            new { Status = ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore, SafetyFailures = new[] { ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore }, Asset = ThemeCompatibilityEvidenceStatusV2.Failed, Motion = ThemeCompatibilityEvidenceStatusV2.Failed, Audio = ThemeCompatibilityEvidenceStatusV2.NotApplicable, Details = Array.Empty<ThemeCompatibilityFailureV2>(), Valid = true },
            new { Status = ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore, SafetyFailures = new[] { ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore }, Asset = ThemeCompatibilityEvidenceStatusV2.Failed, Motion = ThemeCompatibilityEvidenceStatusV2.Failed, Audio = ThemeCompatibilityEvidenceStatusV2.Failed, Details = Array.Empty<ThemeCompatibilityFailureV2>(), Valid = true },
            new { Status = ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore, SafetyFailures = new[] { ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore }, Asset = ThemeCompatibilityEvidenceStatusV2.Failed, Motion = ThemeCompatibilityEvidenceStatusV2.Failed, Audio = ThemeCompatibilityEvidenceStatusV2.Failed, Details = new[] { Failure(ThemeCompatibilityFailureKindV2.SafetyValidationFailed, ThemeCompatibilityDimensionV2.Safety, 0) }, Valid = true },
            new { Status = ThemeCompatibilitySafetyStatusV2.Passed, SafetyFailures = Array.Empty<ThemeCompatibilitySafetyStatusV2>(), Asset = ThemeCompatibilityEvidenceStatusV2.Failed, Motion = ThemeCompatibilityEvidenceStatusV2.Passed, Audio = ThemeCompatibilityEvidenceStatusV2.NotApplicable, Details = Array.Empty<ThemeCompatibilityFailureV2>(), Valid = false },
            new { Status = ThemeCompatibilitySafetyStatusV2.NotEvaluated, SafetyFailures = Array.Empty<ThemeCompatibilitySafetyStatusV2>(), Asset = ThemeCompatibilityEvidenceStatusV2.Passed, Motion = ThemeCompatibilityEvidenceStatusV2.NotEvaluated, Audio = ThemeCompatibilityEvidenceStatusV2.NotEvaluated, Details = Array.Empty<ThemeCompatibilityFailureV2>(), Valid = false },
            new { Status = ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore, SafetyFailures = new[] { ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore }, Asset = ThemeCompatibilityEvidenceStatusV2.NotApplicable, Motion = ThemeCompatibilityEvidenceStatusV2.Passed, Audio = ThemeCompatibilityEvidenceStatusV2.NotApplicable, Details = Array.Empty<ThemeCompatibilityFailureV2>(), Valid = false },
            new { Status = ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore, SafetyFailures = new[] { ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore }, Asset = ThemeCompatibilityEvidenceStatusV2.Passed, Motion = ThemeCompatibilityEvidenceStatusV2.NotApplicable, Audio = ThemeCompatibilityEvidenceStatusV2.NotApplicable, Details = Array.Empty<ThemeCompatibilityFailureV2>(), Valid = false },
            new { Status = ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore, SafetyFailures = new[] { ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore }, Asset = ThemeCompatibilityEvidenceStatusV2.Passed, Motion = ThemeCompatibilityEvidenceStatusV2.Passed, Audio = ThemeCompatibilityEvidenceStatusV2.NotApplicable, Details = new[] { Failure(ThemeCompatibilityFailureKindV2.AssetInventoryInvalid, ThemeCompatibilityDimensionV2.Safety, 0) }, Valid = false },
        };

        foreach (var scenario in scenarios)
        {
            ThemeCompatibilityContextV2 context = Context();
            ThemeCompatibilityEnvironmentV2 environment = Environment();
            ThemeCompatibilitySafetyEvidenceV2 safety = new(
                context, environment, scenario.Status, scenario.SafetyFailures,
                scenario.Asset, scenario.Motion, scenario.Audio, scenario.Details);
            ThemeCompatibilityResultV2 result = new ThemeCompatibilityResolverV2().Resolve(new(
                context, environment, Evidence(context, environment, safety: safety)));
            if (scenario.Valid)
            {
                Assert.NotEqual(ThemeCompatibilityStatusV2.RefusedPrecondition, result.Status);
                AssertCanonicalRoundTrip(result);
                if (scenario.Asset == ThemeCompatibilityEvidenceStatusV2.Failed)
                    Assert.Contains(result.Failures, failure => failure.Kind == ThemeCompatibilityFailureKindV2.AssetInventoryInvalid);
                if (scenario.Motion == ThemeCompatibilityEvidenceStatusV2.Failed)
                    Assert.Contains(result.Failures, failure => failure.Kind == ThemeCompatibilityFailureKindV2.MotionSafetyFailed);
                if (scenario.Audio == ThemeCompatibilityEvidenceStatusV2.Failed)
                    Assert.Contains(result.Failures, failure => failure.Kind == ThemeCompatibilityFailureKindV2.AudioSafetyFailed);
            }
            else
            {
                ThemeCompatibilityFailureV2 failure = Assert.Single(result.Failures);
                Assert.Equal(ThemeCompatibilityFailureKindV2.EvidenceMalformed, failure.Kind);
                Assert.Equal(ThemeCompatibilityDimensionV2.Safety, failure.Dimension);
            }
        }
    }

    [Fact]
    public async Task RepeatedConcurrentAndCultureSpecificCallsAreEquivalent()
    {
        ThemeCompatibilityContextV2 context = Context();
        ThemeCompatibilityEnvironmentV2 environment = Environment();
        ThemeCompatibilityFailureV2 runtimeFailure = Failure(
            ThemeCompatibilityFailureKindV2.RuntimeValidationFailed,
            ThemeCompatibilityDimensionV2.Runtime,
            0);
        ThemeCompatibilityFailureV2 capabilityFailure = Failure(
            ThemeCompatibilityFailureKindV2.CapabilityBlocked,
            ThemeCompatibilityDimensionV2.Capability,
            0);
        ThemeCompatibilitySafetyEvidenceV2 safetyFailure = new(
            context,
            environment,
            ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore,
            [ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore],
            ThemeCompatibilityEvidenceStatusV2.Failed,
            ThemeCompatibilityEvidenceStatusV2.Failed,
            ThemeCompatibilityEvidenceStatusV2.Failed,
            [Failure(ThemeCompatibilityFailureKindV2.SafetyValidationFailed, ThemeCompatibilityDimensionV2.Safety, 0)]);
        ThemeCompatibilityRequestV2[] requests =
        [
            new(context, environment, Evidence(context, environment)),
            new(context, environment, Evidence(
                context,
                environment,
                runtime: new(context, environment, ThemeCompatibilityEvidenceStatusV2.Failed, [], [runtimeFailure]),
                capability: new(context, environment, ThemeCompatibilityEvidenceStatusV2.Failed,
                    ["presentation.tokens"], [], ["presentation.tokens"], [], [capabilityFailure]))),
            new(context, environment, null),
            new(context, environment, Evidence(context, environment, safety: safetyFailure)),
            new(context, environment, Evidence(context, environment, migration: new(
                context, environment, ThemeCompatibilityEvidenceStatusV2.Failed, true, false, []))),
            new(context, environment, Evidence(context, environment, rollback: new(
                context, environment, ThemeCompatibilityEvidenceStatusV2.Failed, true, false, []))),
        ];
        ThemeCompatibilityResolverV2 resolver = new();
        foreach (ThemeCompatibilityRequestV2 request in requests)
        {
            string baseline = ThemeCompatibilityJsonV2.SerializeResult(resolver.Resolve(request));
            for (int index = 0; index < 10; index++)
                Assert.Equal(baseline, ThemeCompatibilityJsonV2.SerializeResult(resolver.Resolve(request)));

            string[] concurrent = await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => Task.Run(() =>
                ThemeCompatibilityJsonV2.SerializeResult(resolver.Resolve(request)))));
            Assert.All(concurrent, value => Assert.Equal(baseline, value));

            CultureInfo original = CultureInfo.CurrentCulture;
            try
            {
                foreach (string culture in new[] { "en-US", "tr-TR", "zh-TW" })
                {
                    CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                    Assert.Equal(baseline, ThemeCompatibilityJsonV2.SerializeResult(resolver.Resolve(request)));
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }
    }

    private static ThemeCompatibilityContextV2 Context(
        ThemeManifest? manifest = null,
        ThemeCompatibilityManifest? compatibility = null)
    {
        ThemeIntegrityVerificationResultV2 integrity = new(
            true, new string('a', 64), new string('c', 64), new string('d', 64),
            [], [], ThemeSignatureVerificationStatus.NotRequired, null, null, []);
        return new ThemeCompatibilityContextV2(
            new("opaque:resolver"), integrity, manifest ?? Manifest(), compatibility ?? Compatibility(), new string('b', 64), []);
    }

    private static ThemeCompatibilityEnvironmentV2 Environment(
        string? coreVersion = "1.0.0",
        IEnumerable<string?>? schemaSupport = null,
        bool nullSchemaSupport = false,
        ThemeCompatibilityOperationV2 operation = ThemeCompatibilityOperationV2.PackageEvaluation,
        IEnumerable<string>? themeApiVersions = null,
        IEnumerable<string>? uxContractVersions = null) =>
        new(
            coreVersion,
            themeApiVersions ?? ["1.0.0"],
            uxContractVersions ?? ["1.1.0"],
            nullSchemaSupport ? null : schemaSupport ?? ["1.0"],
            "windows",
            "x64",
            ThemeCompatibilityInstallationModeV2.Installer,
            "build",
            "profile",
            [new("main", new("deep"), new("home"), 1m, 1280m, 720m,
                new("standard", 1m, 1m, false, null, false, false, true, false, false))],
            operation,
            null,
            null,
            null);

    private static ThemeCompatibilityEvidenceV2 Evidence(
        ThemeCompatibilityContextV2 context,
        ThemeCompatibilityEnvironmentV2 environment,
        int missingSlot = -1,
        ThemeCompatibilityRuntimeEvidenceV2? runtime = null,
        ThemeCompatibilityCapabilityEvidenceV2? capability = null,
        ThemeCompatibilityAccessibilityEvidenceV2? accessibility = null,
        ThemeCompatibilitySafetyEvidenceV2? safety = null,
        ThemeCompatibilityMigrationEvidenceV2? migration = null,
        ThemeCompatibilityRollbackEvidenceV2? rollback = null)
    {
        ThemeCompatibilityRuntimeEvidenceV2 runtimeValue = runtime
            ?? new(context, environment, ThemeCompatibilityEvidenceStatusV2.Passed, [], []);
        ThemeCompatibilityCapabilityEvidenceV2 capabilityValue = capability
            ?? new(context, environment, ThemeCompatibilityEvidenceStatusV2.Passed,
                ["presentation.tokens"], ["presentation.tokens"], [], [], []);
        ThemeCompatibilityAccessibilityEvidenceV2 accessibilityValue = accessibility ?? new(
            context, environment, ThemeCompatibilityAccessibilityStatusV2.Validated, []);
        ThemeCompatibilitySafetyEvidenceV2 safetyValue = safety ?? new(
            context, environment, ThemeCompatibilitySafetyStatusV2.Passed, [],
            ThemeCompatibilityEvidenceStatusV2.Passed,
            ThemeCompatibilityEvidenceStatusV2.Passed,
            ThemeCompatibilityEvidenceStatusV2.NotApplicable,
            []);
        ThemeCompatibilityMigrationEvidenceV2 migrationValue = migration ?? new(
            context, environment, ThemeCompatibilityEvidenceStatusV2.Passed, false, null, []);
        ThemeCompatibilityRollbackEvidenceV2 rollbackValue = rollback ?? new(
            context, environment, ThemeCompatibilityEvidenceStatusV2.Passed, false, null, []);
        return new ThemeCompatibilityEvidenceV2(
            context,
            environment,
            missingSlot == 0 ? null : runtimeValue,
            missingSlot == 3 ? null : capabilityValue,
            missingSlot == 1 ? null : accessibilityValue,
            missingSlot == 2 ? null : safetyValue,
            missingSlot == 4 ? null : migrationValue,
            missingSlot == 5 ? null : rollbackValue);
    }

    private static bool IsValidMigrationTuple(
        ThemeCompatibilityEvidenceStatusV2 status,
        bool? required,
        bool? ready) => status switch
        {
            ThemeCompatibilityEvidenceStatusV2.NotEvaluated => required is null && ready is null,
            ThemeCompatibilityEvidenceStatusV2.Passed => required == false && ready is null
                || required == true && ready == true,
            ThemeCompatibilityEvidenceStatusV2.Failed => required == true && ready == false,
            ThemeCompatibilityEvidenceStatusV2.NotApplicable => required == false && ready is null,
            _ => false,
        };

    private static bool IsValidRollbackTuple(
        ThemeCompatibilityEvidenceStatusV2 status,
        bool? required,
        bool? available) => status switch
        {
            ThemeCompatibilityEvidenceStatusV2.NotEvaluated => required is null && available is null,
            ThemeCompatibilityEvidenceStatusV2.Passed => required == false || required == true && available == true,
            ThemeCompatibilityEvidenceStatusV2.Failed => required == true && available == false,
            ThemeCompatibilityEvidenceStatusV2.NotApplicable => required == false,
            _ => false,
        };

    private static void AssertMatrixOutcome(
        ThemeCompatibilityResultV2 result,
        bool valid,
        ThemeCompatibilityDimensionV2 dimension)
    {
        if (valid)
        {
            Assert.NotEqual(ThemeCompatibilityStatusV2.RefusedPrecondition, result.Status);
            AssertCanonicalRoundTrip(result);
            return;
        }

        ThemeCompatibilityFailureV2 failure = Assert.Single(result.Failures);
        Assert.Equal(ThemeCompatibilityFailureKindV2.EvidenceMalformed, failure.Kind);
        Assert.Equal(dimension, failure.Dimension);
        Assert.Equal(ThemeCompatibilityStatusV2.RefusedPrecondition, result.Status);
    }

    private static void AssertCanonicalRoundTrip(ThemeCompatibilityResultV2 result)
    {
        string first = ThemeCompatibilityJsonV2.SerializeResult(result);
        ThemeCompatibilityResultV2 transported = ThemeCompatibilityJsonV2.DeserializeResult(first);
        Assert.Equal(first, ThemeCompatibilityJsonV2.SerializeResult(transported));
    }

    private static object FixtureContext(Assembly fixture)
    {
        ThemeIntegrityVerificationResultV2 integrity = new(
            true, new string('a', 64), new string('c', 64), new string('d', 64),
            [], [], ThemeSignatureVerificationStatus.NotRequired, null, null, []);
        return FixtureNew(
            fixture,
            "Tcc.Themes.Compatibility.V2.ThemeCompatibilityContextV2",
            new ThemePackageRef("opaque:resolver-fixture"),
            integrity,
            Manifest(),
            Compatibility(),
            new string('b', 64),
            Array.Empty<ThemeCompatibilityFailureV2>());
    }

    private static object FixtureEvidence(
        Assembly fixture,
        object context,
        ThemeCompatibilityEnvironmentV2 environment,
        object? runtime = null,
        object? capability = null,
        object? accessibility = null,
        object? safety = null,
        object? migration = null,
        object? rollback = null) =>
        FixtureNew(
            fixture,
            "Tcc.Themes.Compatibility.V2.ThemeCompatibilityEvidenceV2",
            context, environment, runtime, capability, accessibility, safety, migration, rollback);

    private static object FixtureNew(Assembly fixture, string typeName, params object?[] arguments)
    {
        Type type = fixture.GetType(typeName, throwOnError: true)!;
        ConstructorInfo constructor = Assert.Single(type.GetConstructors(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
        return constructor.Invoke(arguments);
    }

    private static void SetFixtureReceipt(object evidence, string propertyName, object receipt) =>
        evidence.GetType().GetField(
            $"<{propertyName}>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(evidence, receipt);

    private static int ExecuteFixtureResolver(
        Assembly fixture,
        object context,
        ThemeCompatibilityEnvironmentV2 environment,
        object? evidence)
    {
        Type counter = fixture.GetType(
            "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiator", throwOnError: true)!;
        FieldInfo calls = counter.GetField("Calls", BindingFlags.NonPublic | BindingFlags.Static)!;
        calls.SetValue(null, 0);
        object request = FixtureNew(
            fixture,
            "Tcc.Themes.Compatibility.V2.ThemeCompatibilityRequestV2",
            context, environment, evidence);
        object resolver = FixtureNew(
            fixture,
            "Tcc.Themes.Compatibility.V2.ThemeCompatibilityResolverV2");
        resolver.GetType().GetMethod("Resolve", BindingFlags.Public | BindingFlags.Instance)!.Invoke(resolver, [request]);
        return (int)calls.GetValue(null)!;
    }

    private static ThemeCompatibilityEvidenceV2 OwnerEvidence(
        ThemeCompatibilityContextV2 context,
        ThemeCompatibilityEnvironmentV2 environment,
        int ownerSlot,
        int scenario)
    {
        ThemeCompatibilityRuntimeEvidenceV2 runtime = new(
            context, environment, ThemeCompatibilityEvidenceStatusV2.Passed, [], []);
        ThemeCompatibilityAccessibilityEvidenceV2 accessibility = new(
            context, environment, ThemeCompatibilityAccessibilityStatusV2.Validated, []);
        ThemeCompatibilitySafetyEvidenceV2 safety = new(
            context, environment, ThemeCompatibilitySafetyStatusV2.Passed, [],
            ThemeCompatibilityEvidenceStatusV2.Passed,
            ThemeCompatibilityEvidenceStatusV2.Passed,
            ThemeCompatibilityEvidenceStatusV2.NotApplicable,
            []);
        ThemeCompatibilityCapabilityEvidenceV2 capability = new(
            context, environment, ThemeCompatibilityEvidenceStatusV2.Passed,
            ["presentation.tokens"], ["presentation.tokens"], [], [], []);
        ThemeCompatibilityMigrationEvidenceV2 migration = new(
            context, environment, ThemeCompatibilityEvidenceStatusV2.Passed, false, null, []);
        ThemeCompatibilityRollbackEvidenceV2 rollback = new(
            context, environment, ThemeCompatibilityEvidenceStatusV2.Passed, false, null, []);

        ThemeCompatibilityFailureV2 ownerFailure = Failure(
            ownerSlot switch
            {
                0 => ThemeCompatibilityFailureKindV2.RuntimeValidationFailed,
                1 => ThemeCompatibilityFailureKindV2.AccessibilityValidationFailed,
                2 => ThemeCompatibilityFailureKindV2.SafetyValidationFailed,
                3 => ThemeCompatibilityFailureKindV2.CapabilityBlocked,
                4 => ThemeCompatibilityFailureKindV2.MigrationNotReady,
                _ => ThemeCompatibilityFailureKindV2.RollbackUnavailable,
            },
            ownerSlot switch
            {
                0 => ThemeCompatibilityDimensionV2.Runtime,
                1 => ThemeCompatibilityDimensionV2.Accessibility,
                2 => ThemeCompatibilityDimensionV2.Safety,
                3 => ThemeCompatibilityDimensionV2.Capability,
                4 => ThemeCompatibilityDimensionV2.Migration,
                _ => ThemeCompatibilityDimensionV2.Rollback,
            },
            0);

        switch (ownerSlot, scenario)
        {
            case (0, 0): runtime = new(context, environment, ThemeCompatibilityEvidenceStatusV2.NotEvaluated, [], []); break;
            case (0, 1): runtime = new(context, environment, ThemeCompatibilityEvidenceStatusV2.Failed, [], [ownerFailure]); break;
            case (0, 2): runtime = new(context, environment, ThemeCompatibilityEvidenceStatusV2.Passed, [], [ownerFailure]); break;
            case (1, 0): accessibility = new(context, environment, ThemeCompatibilityAccessibilityStatusV2.NotEvaluated, []); break;
            case (1, 1): accessibility = new(context, environment, ThemeCompatibilityAccessibilityStatusV2.Failed, [ownerFailure]); break;
            case (1, 2): accessibility = new(context, environment, ThemeCompatibilityAccessibilityStatusV2.Validated, [ownerFailure]); break;
            case (2, 0): safety = new(context, environment, ThemeCompatibilitySafetyStatusV2.NotEvaluated, [], ThemeCompatibilityEvidenceStatusV2.NotEvaluated, ThemeCompatibilityEvidenceStatusV2.NotEvaluated, ThemeCompatibilityEvidenceStatusV2.NotEvaluated, []); break;
            case (2, 1): safety = new(context, environment, ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore, [ThemeCompatibilitySafetyStatusV2.FailedHomeSafetyCore], ThemeCompatibilityEvidenceStatusV2.Passed, ThemeCompatibilityEvidenceStatusV2.Passed, ThemeCompatibilityEvidenceStatusV2.NotApplicable, [ownerFailure]); break;
            case (2, 2): safety = new(context, environment, ThemeCompatibilitySafetyStatusV2.Passed, [], ThemeCompatibilityEvidenceStatusV2.Passed, ThemeCompatibilityEvidenceStatusV2.Passed, ThemeCompatibilityEvidenceStatusV2.NotApplicable, [ownerFailure]); break;
            case (3, 0): capability = new(context, environment, ThemeCompatibilityEvidenceStatusV2.NotEvaluated, [], [], [], [], []); break;
            case (3, 1): capability = new(context, environment, ThemeCompatibilityEvidenceStatusV2.Failed, ["presentation.tokens"], [], ["presentation.tokens"], [], [ownerFailure]); break;
            case (3, 2): capability = new(context, environment, ThemeCompatibilityEvidenceStatusV2.Passed, ["presentation.tokens"], ["presentation.tokens"], [], [], [ownerFailure]); break;
            case (4, 0): migration = new(context, environment, ThemeCompatibilityEvidenceStatusV2.NotEvaluated, null, null, []); break;
            case (4, 1): migration = new(context, environment, ThemeCompatibilityEvidenceStatusV2.Failed, true, false, [ownerFailure]); break;
            case (4, 2): migration = new(context, environment, ThemeCompatibilityEvidenceStatusV2.Passed, null, null, []); break;
            case (5, 0): rollback = new(context, environment, ThemeCompatibilityEvidenceStatusV2.NotEvaluated, null, null, []); break;
            case (5, 1): rollback = new(context, environment, ThemeCompatibilityEvidenceStatusV2.Failed, true, false, [ownerFailure]); break;
            case (5, 2): rollback = new(context, environment, ThemeCompatibilityEvidenceStatusV2.Passed, null, null, []); break;
        }

        return new ThemeCompatibilityEvidenceV2(
            context, environment, runtime, capability, accessibility, safety, migration, rollback);
    }

    private static ThemeCompatibilityNoticeV2 Notice(
        ThemeCompatibilityNoticeKindV2 kind,
        ThemeCompatibilityDimensionV2 dimension,
        int sequence) => new(kind, dimension, sequence, null, $"{kind} in {dimension}.");

    private static ThemeCompatibilityFailureV2 Failure(
        ThemeCompatibilityFailureKindV2 kind,
        ThemeCompatibilityDimensionV2 dimension,
        int sequence) => new(kind, dimension, sequence, null, $"{kind} in {dimension}.");

    private static ThemeCompatibilityManifest Compatibility() => new(
        "1.0", "com.example.theme", "1.2.3",
        new(">=1.0.0 <2.0.0", ["1.0.0"]),
        new(">=1.0.0 <2.0.0", ["1.0.0"]),
        new(">=1.1.0 <2.0.0", ["1.1.0"]),
        new(true, true, true, ["100%"]),
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["deep"] = "pass_required", ["light"] = "pass_required",
            ["reduced_motion"] = "pass_required", ["reduced_transparency"] = "pass_required",
            ["high_contrast"] = "pass_required", ["color_vision"] = "pass_required",
            ["keyboard"] = "pass_required", ["screen_reader"] = "pass_required",
        });

    private static ThemeManifest Manifest() => new(
        "1.0", "1.0.0",
        new("com.example.theme", "com.example.theme.package", "Example", "Publisher", "publisher",
            "1.2.3", "stable", "Example", null, null, "private", ["deep"]),
        new(">=1.0.0 <2.0.0", ">=1.0.0 <2.0.0", ">=1.1.0 <2.0.0", ["windows"], true, 1m, 3m),
        [new("deep", "deep", "Deep", true, "tokens/deep.json", [], null)],
        ["presentation.tokens"], new([]),
        new(true, ["none"], ["safety_core"], "none"),
        new(true, true, true, true, true, true, true, true, true, true, true, true),
        new("assets/index.json", ["tier0"], 0, "sha256"), null,
        new("motion/profiles.json", "motion/reduced.json", true, true, true), null,
        new("integrity.json", null, "stable_or_store", "sha256"), null);
}
