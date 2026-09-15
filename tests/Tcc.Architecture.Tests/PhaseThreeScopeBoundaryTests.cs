using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Xml.Linq;

namespace Tcc.Architecture.Tests;

public sealed class PhaseThreeScopeBoundaryTests
{
    private static readonly string[] ApprovedTopLevelTypes =
    [
        "Tcc.Themes.AssemblyMarker",
        "Tcc.Themes.Fallback.BuiltInThemePresentationSource",
        "Tcc.Themes.Fallback.BuiltInThemePresentationSnapshot",
        "Tcc.Themes.Compatibility.V2.IThemeCompatibilityResolverV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilityRequestV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilityContextV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilityEvidenceV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilityRuntimeEvidenceV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilityCapabilityEvidenceV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilityAccessibilityEvidenceV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilitySafetyEvidenceV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilityMigrationEvidenceV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilityRollbackEvidenceV2",
        "Tcc.Themes.Compatibility.V2.ThemeCompatibilityResolverV2",

        "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityEvidenceBinder",
        "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityContentSnapshot",
        "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityManifestMaterializer",

        "Tcc.Themes.Integrity.ThemeIntegrityRequestBoundary",
        "Tcc.Themes.Integrity.ThemeIntegrityVerifier",
        "Tcc.Themes.Integrity.ThemePackageInventoryEvaluation",
        "Tcc.Themes.Integrity.ThemePackageInventoryEvaluator",
        "Tcc.Themes.Integrity.ThemeMetadataSchemaValidator",
        "Tcc.Themes.Integrity.ThemePackageMetadataEvaluation",
        "Tcc.Themes.Integrity.ThemePackageMetadataEvaluator",
        "Tcc.Themes.Integrity.ThemePackageSignatureEvaluator",
        "Tcc.Themes.Integrity.ThemePackageSignatureEvaluation",
        "Tcc.Themes.Compatibility.ThemeCompatibilityNegotiationFailureKind",
        "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiation",
        "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiator",
        "Tcc.Themes.Manifests.ThemeManifestValidator",
    ];

    private const string ApprovedNestedType =
        "Tcc.Themes.Manifests.ThemeManifestValidator+DiagnosticCodes";

    private static readonly string[] ApprovedSealedBaselineCompilerArtifacts =
    [
        "<>z__ReadOnlyArray`1",
        "Tcc.Themes.Integrity.ThemeIntegrityRequestBoundary+<>c",
        "Tcc.Themes.Manifests.ThemeManifestValidator+<>O",
        "Tcc.Themes.Manifests.ThemeManifestValidator+<>c",
        "Tcc.Themes.Manifests.ThemeManifestValidator+<>c__DisplayClass24_0",
        "Tcc.Themes.Manifests.ThemeManifestValidator+<>c__DisplayClass25_0",
    ];

    private static readonly string[] ForbiddenCompiledSymbolFragments =
    [
        "TradingAI",
        "AIRecommendation",
        "MarketObservation",
        "TradingRiskPolicy",
        "PositionIntelligence",
        "ShadowTrading",
        "Learning",
        "PreparedOrder",
        "TradeExecution",
    ];

    private static readonly string[] ApprovedAsyncEvaluatorTypes =
    [
        "Tcc.Themes.Integrity.ThemePackageInventoryEvaluator",
        "Tcc.Themes.Integrity.ThemePackageMetadataEvaluator",
    ];

    private static readonly string[] ForbiddenVerifierInterfaces =
    [
        "Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifier",
        "Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifierV2",
    ];

    private const string ForbiddenCompatibilityResolverInterface =
        "Tcc.Presentation.Contracts.Theme.IThemeCompatibilityResolver";

    private static readonly OpCode[] SingleByteOpCodes = BuildOpCodeTable(twoByte: false);
    private static readonly OpCode[] TwoByteOpCodes = BuildOpCodeTable(twoByte: true);

    // Fixed from the approved Phase5A Release IL/signature closure, not learned from
    // the assembly under inspection. Type identity also pins the owning assembly.
    private static readonly HashSet<Type> ApprovedPhaseFiveDependencyTypes =
    [
        typeof(ArgumentNullException), typeof(Array), typeof(bool), typeof(char),
        typeof(EqualityComparer<>), typeof(IEnumerable<>), typeof(IEnumerator<>),
        typeof(IReadOnlyCollection<>), typeof(IReadOnlySet<>), typeof(List<>),
        typeof(System.Collections.IEnumerator),
        typeof(System.Collections.Immutable.ImmutableArray),
        typeof(System.Collections.Immutable.ImmutableArray<>),
        typeof(System.Collections.Immutable.ImmutableArray<>.Builder),
        typeof(Enum), typeof(System.Globalization.CultureInfo), typeof(System.Globalization.NumberStyles),
        typeof(IComparable), typeof(IConvertible), typeof(IDisposable), typeof(IEquatable<>),
        typeof(IFormatProvider), typeof(IFormattable), typeof(int), typeof(ISpanFormattable),
        typeof(MemoryExtensions), typeof(Nullable<>), typeof(System.Numerics.BigInteger),
        typeof(object), typeof(ReadOnlySpan<>), typeof(RuntimeHelpers), typeof(RuntimeTypeHandle),
        typeof(string), typeof(StringComparison), typeof(System.Text.StringBuilder), typeof(Type),
        typeof(ValueTuple<,>), typeof(ValueTuple<,,>), typeof(void),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityDeclaration),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityManifest),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityRequest),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeManifest),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeVersionRange),
    ];

    private static readonly HashSet<Type> ApprovedCandidateBResolverDependencyTypes =
    [
        typeof(ArgumentNullException), typeof(Array), typeof(bool), typeof(char), typeof(Enum),
        typeof(HashSet<>), typeof(IEnumerable<>), typeof(IEnumerator<>), typeof(IEqualityComparer<>),
        typeof(IReadOnlyList<>), typeof(IReadOnlySet<>), typeof(List<>), typeof(System.Collections.IEnumerator),
        typeof(IDisposable), typeof(int), typeof(InvalidOperationException), typeof(MemoryExtensions),
        typeof(Nullable<>), typeof(object), typeof(ReadOnlySpan<>), typeof(string), typeof(StringComparer),
        typeof(StringComparison), typeof(System.Runtime.CompilerServices.DefaultInterpolatedStringHandler),
        typeof(ValueTuple<,>), typeof(void), typeof(System.Collections.Immutable.ImmutableArray),
        typeof(System.Collections.Immutable.ImmutableArray<>), typeof(System.Collections.Immutable.ImmutableArray<>.Builder),
        typeof(System.Collections.Immutable.ImmutableArray<>.Enumerator),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeAccessibilityStatus),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeAccessibilityValidationResult),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityAccessibilityStatusV2),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityDeclaration),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityDimensionV2),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityEnvironmentV2),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityEvaluationStateV2),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityEvidenceStatusV2),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityFailureKindV2),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityFailureV2),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityInstallationModeV2),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityManifest),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityNoticeKindV2),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityNoticeV2),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityOperationV2),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityRequest),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityResultV2),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilitySafetyStatusV2),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityStatusV2),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeId),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeIntegrityVerificationResultV2),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeManifest),
        typeof(Tcc.Presentation.Contracts.Theme.ThemePackageIdentity),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeVersion),
        typeof(Tcc.Presentation.Contracts.Theme.ThemeWindowsCompatibility),
        typeof(Tcc.Themes.Compatibility.ThemeCompatibilityNegotiationFailureKind),
        typeof(Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiation),
        typeof(Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiator),
        typeof(Tcc.Themes.Compatibility.V2.IThemeCompatibilityResolverV2),
        typeof(Tcc.Themes.Compatibility.V2.ThemeCompatibilityAccessibilityEvidenceV2),
        typeof(Tcc.Themes.Compatibility.V2.ThemeCompatibilityCapabilityEvidenceV2),
        typeof(Tcc.Themes.Compatibility.V2.ThemeCompatibilityContextV2),
        typeof(Tcc.Themes.Compatibility.V2.ThemeCompatibilityEvidenceV2),
        typeof(Tcc.Themes.Compatibility.V2.ThemeCompatibilityMigrationEvidenceV2),
        typeof(Tcc.Themes.Compatibility.V2.ThemeCompatibilityRequestV2),
        typeof(Tcc.Themes.Compatibility.V2.ThemeCompatibilityResolverV2),
        typeof(Tcc.Themes.Compatibility.V2.ThemeCompatibilityRollbackEvidenceV2),
        typeof(Tcc.Themes.Compatibility.V2.ThemeCompatibilityRuntimeEvidenceV2),
        typeof(Tcc.Themes.Compatibility.V2.ThemeCompatibilitySafetyEvidenceV2),
    ];

    [Fact]
    public void ActualCompiledThemeAssemblyMatchesExactPhaseThreeTypeSurface()
    {
        string[] violations = GetCompiledSurfaceViolations(typeof(Tcc.Themes.AssemblyMarker).Assembly);

        Assert.Empty(violations);
    }

    [Fact]
    public void CandidateBResolverCallsThePhaseFiveANegotiatorExactlyOnce()
    {
        MethodInfo resolve = typeof(Tcc.Themes.Compatibility.V2.ThemeCompatibilityResolverV2)
            .GetMethod("Resolve", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
        MemberInfo[] calls = GetReferencedMembers(resolve)
            .Where(member => member is MethodInfo method
                && method.Name == "Negotiate"
                && method.DeclaringType?.FullName == PhaseFiveNegotiatorName)
            .ToArray();

        Assert.Single(calls);
    }

    [Fact]
    public void CandidateBResolverCardinalityRejectsZeroAndSecondImplementations()
    {
        Assembly zero = BuildFixtureAssembly(null, null, includeCandidateAContracts: false);
        Assert.Contains(GetCompiledSurfaceViolations(zero), violation =>
            violation.Contains("exactly one legal V2 resolver implementation; found 0", StringComparison.Ordinal));

        Assembly second = BuildFixtureAssembly(
            null,
            "using Tcc.Presentation.Contracts.Theme; namespace Tcc.Themes.Compatibility.V2; public sealed class AlternateResolver : IThemeCompatibilityResolverV2 { public ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request) => null!; }");
        Assert.Contains(GetCompiledSurfaceViolations(second), violation =>
            violation.Contains("exactly one legal V2 resolver implementation; found 2", StringComparison.Ordinal));
    }

    [Fact]
    public void CandidateBSurfaceRejectsWrongAssemblySubstitute()
    {
        Assembly fixture = BuildFixtureAssembly(
            null,
            null,
            includeCandidateAContracts: true,
            assemblyName: "Tcc.Themes.Substitute");

        string[] violations = GetCompiledSurfaceViolations(fixture);
        Assert.Contains(violations, violation =>
            violation.Contains("Illegal V2 resolver implementation shape or identity", StringComparison.Ordinal));
        Assert.Contains(violations, violation =>
            violation.Contains("ThemeCompatibilityContentSnapshot", StringComparison.Ordinal));
    }

    [Fact]
    public void ForgedCandidateBAsyncStateMachineIsRejected()
    {
        AssemblyName name = new($"Tcc.Themes.CandidateBForgery.{Guid.NewGuid():N}");
        AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
        ModuleBuilder module = assembly.DefineDynamicModule(name.Name!);
        TypeBuilder owner = module.DefineType(
            "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityEvidenceBinder",
            TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.NotPublic);
        TypeBuilder artifact = owner.DefineNestedType(
            "<VerifyAndBindAsync>d__1",
            TypeAttributes.NestedPrivate | TypeAttributes.Sealed,
            typeof(ValueType));
        artifact.SetCustomAttribute(CompilerGeneratedAttributeBuilder());
        artifact.AddInterfaceImplementation(typeof(IAsyncStateMachine));
        DefineStateMachineMethod(artifact, nameof(IAsyncStateMachine.MoveNext), Type.EmptyTypes);
        DefineStateMachineMethod(artifact, nameof(IAsyncStateMachine.SetStateMachine), [typeof(IAsyncStateMachine)]);
        artifact.CreateType();
        Type forgedOwner = owner.CreateType()!;

        Assert.Contains(GetCompiledSurfaceViolations(forgedOwner.Assembly), violation =>
            violation.Contains("<VerifyAndBindAsync>d__1", StringComparison.Ordinal));
    }

    [Theory]
    [MemberData(nameof(CandidateBResolverIdentityAttackSources))]
    public void CandidateBResolverIdentityMutantsAreRejected(string caseName, string source)
    {
        Assert.False(string.IsNullOrWhiteSpace(caseName));
        Assembly fixture = BuildFixtureAssembly(null, source, includeCandidateAContracts: false);

        Assert.Contains(GetCompiledSurfaceViolations(fixture), violation =>
            violation.Contains("Illegal V2 resolver implementation shape or identity", StringComparison.Ordinal));
    }

    public static IEnumerable<object[]> CandidateBResolverIdentityAttackSources()
    {
        const string prefix = "using Tcc.Presentation.Contracts.Theme; namespace Tcc.Themes.Compatibility.V2 { public sealed class ThemeCompatibilityRequestV2 { } public interface IThemeCompatibilityResolverV2 { ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request); } ";
        const string suffix = " }";
        yield return ["wrong name", prefix + "public sealed class AlternateResolver : IThemeCompatibilityResolverV2 { public ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request) => null!; }" + suffix];
        yield return ["internal substitute", prefix + "internal sealed class ThemeCompatibilityResolverV2 : IThemeCompatibilityResolverV2 { public ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request) => null!; }" + suffix];
        yield return ["nested substitute", prefix + "public sealed class Owner { public sealed class ThemeCompatibilityResolverV2 : IThemeCompatibilityResolverV2 { public ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request) => null!; } }" + suffix];
        yield return ["generated substitute", prefix + "[System.Runtime.CompilerServices.CompilerGenerated] public sealed class ThemeCompatibilityResolverV2 : IThemeCompatibilityResolverV2 { public ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request) => null!; }" + suffix];
        yield return ["mutable static state", prefix + "public sealed class ThemeCompatibilityResolverV2 : IThemeCompatibilityResolverV2 { private static int state; public ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request) { state++; return null!; } }" + suffix];
        yield return ["wrong namespace", prefix + suffix + " namespace Tcc.Themes.Compatibility.Attack { public sealed class ThemeCompatibilityResolverV2 : global::Tcc.Themes.Compatibility.V2.IThemeCompatibilityResolverV2 { public ThemeCompatibilityResultV2 Resolve(global::Tcc.Themes.Compatibility.V2.ThemeCompatibilityRequestV2 request) => null!; } }"];
        yield return ["dual V1 and V2", prefix + "public sealed class ThemeCompatibilityResolverV2 : IThemeCompatibilityResolverV2, IThemeCompatibilityResolver { public ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request) => null!; public ThemeCompatibilityResult Resolve(ThemeCompatibilityRequest request) => null!; }" + suffix];
    }

    [Theory]
    [MemberData(nameof(CandidateBBindingSurfaceAttackSources))]
    public void CandidateBBindingSurfaceMutantsAreRejected(string caseName, string source, string target)
    {
        Assert.False(string.IsNullOrWhiteSpace(caseName));
        Assembly fixture = BuildFixtureAssembly(null, source, includeCandidateAContracts: false);

        Assert.Contains(GetCompiledSurfaceViolations(fixture), violation =>
            violation.Contains(target, StringComparison.Ordinal));
    }

    public static IEnumerable<object[]> CandidateBBindingSurfaceAttackSources()
    {
        yield return ["public binder", "namespace Tcc.Themes.Compatibility.Binding; public static class ThemeCompatibilityEvidenceBinder { }", "ThemeCompatibilityEvidenceBinder"];
        yield return ["public materializer", "namespace Tcc.Themes.Compatibility.Binding; public static class ThemeCompatibilityManifestMaterializer { }", "ThemeCompatibilityManifestMaterializer"];
        yield return ["mutable binder state", "namespace Tcc.Themes.Compatibility.Binding; internal static class ThemeCompatibilityEvidenceBinder { private static int state; internal static int Read() => state++; }", "mutable or reference-backed static state"];
        yield return ["public snapshot", "namespace Tcc.Themes.Compatibility.Binding; public sealed class ThemeCompatibilityContentSnapshot : global::Tcc.Presentation.Contracts.Theme.IThemePackageContentReader { public global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.IReadOnlyList<global::Tcc.Presentation.Contracts.Theme.ThemePackageContentEntryV1>> EnumerateEntriesAsync(global::Tcc.Presentation.Contracts.Theme.ThemePackageRef packageRef, global::System.Threading.CancellationToken cancellationToken = default) => default; public global::System.Threading.Tasks.ValueTask<global::System.ReadOnlyMemory<byte>> ReadContentAsync(global::Tcc.Presentation.Contracts.Theme.ThemePackageRef packageRef, string canonicalPath, global::System.Threading.CancellationToken cancellationToken = default) => default; }", "ThemeCompatibilityContentSnapshot"];
        yield return ["extra public DTO", "namespace Tcc.Themes.Compatibility.V2; public sealed class CandidateBExtraDto { }", "CandidateBExtraDto"];
        yield return ["extra enum", "namespace Tcc.Themes.Compatibility.V2; public enum CandidateBExtraKind { Value }", "CandidateBExtraKind"];
        yield return ["trust factory", "namespace Tcc.Themes.Compatibility.Binding; public static class ThemeCompatibilityTrustFactory { }", "ThemeCompatibilityTrustFactory"];
    }

    [Theory]
    [InlineData("reader", "_ = typeof(IThemePackageContentReader);")]
    [InlineData("filesystem", "_ = typeof(System.IO.FileInfo);")]
    [InlineData("network", "_ = typeof(System.Net.Http.HttpClient);")]
    [InlineData("crypto", "_ = typeof(System.Security.Cryptography.SHA256);")]
    [InlineData("verifier", "_ = typeof(Tcc.Themes.Integrity.ThemeIntegrityVerifier);")]
    [InlineData("binder and snapshot", "_ = typeof(Tcc.Themes.Compatibility.Binding.ThemeCompatibilityEvidenceBinder); _ = typeof(Tcc.Themes.Compatibility.Binding.ThemeCompatibilityContentSnapshot);")]
    [InlineData("generic hidden", "_ = typeof(System.Collections.Generic.List<System.IO.FileInfo[]>);")]
    [InlineData("typed catch hidden", "try { throw new System.Exception(); } catch (System.IO.FileNotFoundException) { }")]
    public void CandidateBResolverForbiddenDependenciesAreRejected(string caseName, string body)
    {
        Assert.False(string.IsNullOrWhiteSpace(caseName));
        string source = "using Tcc.Presentation.Contracts.Theme; namespace Tcc.Themes.Compatibility.V2 { "
            + "public sealed class ThemeCompatibilityRequestV2 { } public interface IThemeCompatibilityResolverV2 { ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request); } "
            + "public sealed class ThemeCompatibilityResolverV2 : IThemeCompatibilityResolverV2 { public ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request) { "
            + body + " return null!; } } } "
            + "namespace Tcc.Themes.Compatibility.Binding { internal static class ThemeCompatibilityEvidenceBinder { } internal sealed class ThemeCompatibilityContentSnapshot { } } "
            + "namespace Tcc.Themes.Integrity { internal sealed class ThemeIntegrityVerifier { } }";
        Assembly fixture = BuildFixtureAssembly(null, source, includeCandidateAContracts: false);

        Assert.Contains(GetCompiledSurfaceViolations(fixture), violation =>
            violation.Contains("Candidate B resolver dependency outside exact allowlist", StringComparison.Ordinal));
    }

    [Fact]
    public void AssemblyMarkerCannotHideUnauthorizedIntegrityImplementation()
    {
        Assembly fixture = BuildFixtureAssembly(
            markerAdditionalSource: "public sealed class ThemeIntegrityVerifier { }",
            otherSource: null);

        string[] violations = GetCompiledSurfaceViolations(fixture);

        Assert.Contains(violations, violation => violation.Contains("ThemeIntegrityVerifier", StringComparison.Ordinal));
    }

    [Fact]
    public void AnotherProductionSourceCannotHidePhaseFourImplementation()
    {
        Assembly fixture = BuildFixtureAssembly(
            markerAdditionalSource: null,
            otherSource: "namespace Tcc.Themes.Runtime; public sealed class ThemeLoader { }");

        string[] violations = GetCompiledSurfaceViolations(fixture);

        Assert.Contains(violations, violation => violation.Contains("ThemeLoader", StringComparison.Ordinal));
    }

    [Fact]
    public void CompiledTradingIntelligenceSymbolFailsThePhaseThreeBoundary()
    {
        Assembly fixture = BuildFixtureAssembly(
            markerAdditionalSource: null,
            otherSource: "namespace Tcc.Themes.Manifests; public sealed class MarketObservationRuntime { }");

        string[] violations = GetCompiledSurfaceViolations(fixture);

        Assert.Contains(violations, violation => violation.Contains("MarketObservation", StringComparison.Ordinal));
    }

    [Fact]
    public void UnapprovedInternalThemeTypeFailsTheExactCompiledSurfaceBoundary()
    {
        Assembly fixture = BuildFixtureAssembly(
            markerAdditionalSource: null,
            otherSource: "namespace Tcc.Themes.Integrity; internal static class UnapprovedIntegrityStage { }");

        string[] violations = GetCompiledSurfaceViolations(fixture);

        Assert.Contains(
            violations,
            violation => violation.Contains("Tcc.Themes.Integrity.UnapprovedIntegrityStage", StringComparison.Ordinal));
    }

    [Theory]
    [MemberData(nameof(PhaseFiveForbiddenShapeSources))]
    public void PhaseFiveGuardRejectsUnauthorizedCompatibilityShapes(
        string targetType,
        string source,
        string requiredViolation)
    {
        Assembly fixture = BuildFixtureAssembly(markerAdditionalSource: null, otherSource: source);

        AssertTargetedViolation(fixture, targetType, requiredViolation);
    }

    private const string PhaseFiveNegotiatorName = "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiator";
    private const string PhaseFiveOutcomeName = "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiation";

    private static string PhaseFiveFixtureSource(string helper = "", string outcomeMember = "", string extraSource = "") =>
        $$"""
        using Tcc.Presentation.Contracts.Theme;
        using System.Collections.Immutable;
        namespace Tcc.Themes.Compatibility
        {
            internal enum ThemeCompatibilityNegotiationFailureKind { InvalidVersionInput, UnsatisfiableVersionRange, UnsupportedManifestSchema, ConflictingVersionDeclaration, CoreVersionIncompatible, ThemeApiNoCompatibleVersion, UxContractNoCompatibleVersion }
            internal sealed record ThemeCompatibilityVersionNegotiation(bool CanContinue, string? SelectedThemeApiVersion, string? SelectedUxContractVersion, ImmutableArray<ThemeCompatibilityNegotiationFailureKind> Failures)
            {
                {{outcomeMember}}
            }
            internal static class ThemeCompatibilityVersionNegotiator
            {
                internal static ThemeCompatibilityVersionNegotiation Negotiate(ThemeCompatibilityRequest request) => new(false, null, null, ImmutableArray<ThemeCompatibilityNegotiationFailureKind>.Empty);
                {{helper}}
            }
        }
        {{extraSource}}
        """;

    [Fact]
    public void PhaseFiveApprovedCompiledFixtureIncludingGenericCallPasses()
    {
        Assembly fixture = BuildFixtureAssembly(null, PhaseFiveFixtureSource("private static int[] Probe() => Array.Empty<int>();"));
        Assert.DoesNotContain(GetCompiledSurfaceViolations(fixture), violation =>
            violation.Contains("Phase5A", StringComparison.Ordinal)
            || violation.Contains("'Tcc.Themes.Compatibility.", StringComparison.Ordinal));
    }

    [Theory]
    [MemberData(nameof(PhaseFiveExceptionHandlerCases))]
    public void PhaseFiveExceptionHandlerDependenciesAreCollected(
        string caseName,
        string helper,
        string? requiredViolation,
        string? externalSource,
        ExceptionHandlingClauseOptions expectedFlags,
        string? expectedCatchType)
    {
        Assert.False(string.IsNullOrWhiteSpace(caseName));
        Assembly fixture = BuildFixtureAssembly(null, PhaseFiveFixtureSource(helper), externalSource);
        Type negotiator = fixture.GetType(PhaseFiveNegotiatorName, throwOnError: true)!;
        MethodInfo probe = negotiator.GetMethod(
            "Probe",
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
            ?? throw new InvalidOperationException("Compiled exception fixture Probe method is missing.");
        ExceptionHandlingClause clause = Assert.Single(probe.GetMethodBody()!.ExceptionHandlingClauses);
        Assert.Equal(expectedFlags, clause.Flags);
        if (expectedFlags == ExceptionHandlingClauseOptions.Clause)
        {
            Assert.Equal(expectedCatchType, clause.CatchType?.FullName);
        }
        else
        {
            Assert.Null(expectedCatchType);
            Assert.Throws<InvalidOperationException>(() => clause.CatchType);
        }

        string[] violations = GetCompiledSurfaceViolations(fixture);
        if (requiredViolation is null)
        {
            Assert.DoesNotContain(violations, violation =>
                violation.Contains("Phase5A", StringComparison.Ordinal)
                || violation.Contains("'Tcc.Themes.Compatibility.", StringComparison.Ordinal));
        }
        else
        {
            AssertTargetedViolation(fixture, PhaseFiveNegotiatorName, requiredViolation);
        }
    }

    public static IEnumerable<object?[]> PhaseFiveExceptionHandlerCases()
    {
        yield return
        [
            "FileNotFoundException catch-only",
            "private static int Probe(string text) { try { return int.Parse(text); } catch (System.IO.FileNotFoundException) { return -1; } }",
            "System.IO.FileNotFoundException",
            null,
            ExceptionHandlingClauseOptions.Clause,
            "System.IO.FileNotFoundException",
        ];
        yield return
        [
            "neutral external catch-only",
            "private static int Probe(string text) { try { return int.Parse(text); } catch (Remote.Components.ExternalException) { return -1; } }",
            "Remote.Components.ExternalException",
            "namespace Remote.Components { public sealed class ExternalException : System.Exception { } }",
            ExceptionHandlingClauseOptions.Clause,
            "Remote.Components.ExternalException",
        ];
        yield return
        [
            "approved catch",
            "private static int Probe(string text) { try { return int.Parse(text); } catch (System.ArgumentNullException) { return -1; } }",
            null,
            null,
            ExceptionHandlingClauseOptions.Clause,
            "System.ArgumentNullException",
        ];
        yield return
        [
            "finally",
            "private static int Probe(int value) { try { return value; } finally { _ = value; } }",
            null,
            null,
            ExceptionHandlingClauseOptions.Finally,
            null,
        ];
        yield return
        [
            "forbidden filter IL",
            "private static int Probe(string text) { try { return int.Parse(text); } catch (System.ArgumentNullException) when (Remote.Components.FilterProbe.Check()) { return -1; } }",
            "Remote.Components.FilterProbe",
            "namespace Remote.Components { public static class FilterProbe { public static bool Check() => true; } }",
            ExceptionHandlingClauseOptions.Filter,
            null,
        ];
    }

    [Fact]
    public void PhaseFiveFaultClauseHasNoCatchTypeDependency()
    {
        AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName($"PhaseFiveFaultFixture{Guid.NewGuid():N}"),
            AssemblyBuilderAccess.Run);
        TypeBuilder typeBuilder = assembly.DefineDynamicModule("Fixture").DefineType(
            "PhaseFiveFaultFixture",
            TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.Abstract);
        MethodBuilder methodBuilder = typeBuilder.DefineMethod(
            "Probe",
            MethodAttributes.Private | MethodAttributes.Static,
            typeof(void),
            Type.EmptyTypes);
        ILGenerator il = methodBuilder.GetILGenerator();
        il.BeginExceptionBlock();
        il.Emit(OpCodes.Nop);
        il.BeginFaultBlock();
        il.Emit(OpCodes.Nop);
        il.EndExceptionBlock();
        il.Emit(OpCodes.Ret);

        Type fixtureType = typeBuilder.CreateType()!;
        MethodInfo probe = fixtureType.GetMethod(
            "Probe",
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)!;
        ExceptionHandlingClause clause = Assert.Single(probe.GetMethodBody()!.ExceptionHandlingClauses);
        Assert.Equal(ExceptionHandlingClauseOptions.Fault, clause.Flags);
        Assert.Throws<InvalidOperationException>(() => clause.CatchType);
        Assert.DoesNotContain(typeof(Exception), GetMemberDependencyTypes(fixtureType));
    }

    [Theory]
    [MemberData(nameof(PhaseFiveRemediationAttackSources))]
    public void PhaseFiveRemediationAttackIsCompiledAndRejected(
        string caseName, string targetType, string source, string requiredViolation, string? externalSource)
    {
        Assert.False(string.IsNullOrWhiteSpace(caseName));
        Assembly fixture = BuildFixtureAssembly(null, source, externalSource);
        AssertTargetedViolation(fixture, targetType, requiredViolation);
        if (caseName == "generic method FileInfo")
        {
            // Prove FileInfo itself was collected, independently of rejecting Unsafe.
            Assert.Contains(typeof(FileInfo), GetMemberDependencyTypes(fixture.GetType(targetType, true)!));
        }
    }

    public static IEnumerable<object[]> PhaseFiveRemediationAttackSources()
    {
        string source = PhaseFiveFixtureSource();
        yield return ["public negotiator", PhaseFiveNegotiatorName, source.Replace("internal static class ThemeCompatibilityVersionNegotiator", "public static class ThemeCompatibilityVersionNegotiator", StringComparison.Ordinal), "exact internal static Phase5A negotiator", null!];
        yield return ["second negotiator", "Tcc.Themes.Compatibility.SecondNegotiator", source + "namespace Tcc.Themes.Compatibility { internal static class SecondNegotiator { } }", "outside the exact Phase 3 surface", null!];
        yield return ["wrong namespace negotiator", "Tcc.Themes.Wrong.ThemeCompatibilityVersionNegotiator", source.Replace("namespace Tcc.Themes.Compatibility", "namespace Tcc.Themes.Wrong", StringComparison.Ordinal), "outside the exact Phase 3 surface", null!];
        yield return ["public outcome", PhaseFiveOutcomeName, source.Replace("internal sealed record", "public sealed record", StringComparison.Ordinal).Replace("internal enum", "public enum", StringComparison.Ordinal), "exact internal sealed Phase5A outcome record", null!];
        yield return ["public failure enum", "Tcc.Themes.Compatibility.ThemeCompatibilityNegotiationFailureKind", source.Replace("internal enum", "public enum", StringComparison.Ordinal), "exact internal Phase5A failure enum", null!];
        yield return ["resolver implementation", "Tcc.Themes.Compatibility.RogueCompatibilityResolver", source + "namespace Tcc.Themes.Compatibility { internal sealed class RogueCompatibilityResolver : IThemeCompatibilityResolver { public ThemeCompatibilityResult Resolve(ThemeCompatibilityRequest request) => null!; } }", "IThemeCompatibilityResolver", null!];
        yield return ["filesystem signature", PhaseFiveNegotiatorName, PhaseFiveFixtureSource("private static System.IO.FileInfo Probe() => null!;"), "System.IO.FileInfo", null!];
        yield return ["filesystem private body", PhaseFiveNegotiatorName, PhaseFiveFixtureSource("private static bool Probe() => System.IO.File.Exists(\"probe\");"), "System.IO.File", null!];
        yield return ["package reader signature", PhaseFiveNegotiatorName, PhaseFiveFixtureSource("private static void Probe(IThemePackageContentReader reader) { }"), "IThemePackageContentReader", null!];
        yield return ["package reader private body", PhaseFiveNegotiatorName, PhaseFiveFixtureSource("private static void Probe() { IThemePackageContentReader reader = null!; _ = reader.ReadContentAsync(default, \"probe\"); }"), "IThemePackageContentReader", null!];
        yield return ["verifier private body", PhaseFiveNegotiatorName, PhaseFiveFixtureSource("private static void Probe() => Tcc.Themes.Integrity.ThemeIntegrityVerifier.Touch();", extraSource: "namespace Tcc.Themes.Integrity { public sealed class ThemeIntegrityVerifier { public static void Touch() { } } }"), "Tcc.Themes.Integrity.ThemeIntegrityVerifier", null!];
        yield return ["Trading private body", PhaseFiveNegotiatorName, PhaseFiveFixtureSource("private static void Probe() => External.Trading.Position.Execute();"), "External.Trading.Position", "namespace External.Trading { public static class Position { public static void Execute() { } } }"];
        yield return ["AI private body", PhaseFiveNegotiatorName, PhaseFiveFixtureSource("private static void Probe() => External.AI.Intelligence.Evaluate();"), "External.AI.Intelligence", "namespace External.AI { public static class Intelligence { public static void Evaluate() { } } }"];
        yield return ["generic method FileInfo", PhaseFiveNegotiatorName, PhaseFiveFixtureSource("private static int Probe() => System.Runtime.CompilerServices.Unsafe.SizeOf<System.IO.FileInfo>();"), "System.IO.FileInfo", null!];
        yield return ["private Safe property", PhaseFiveOutcomeName, PhaseFiveFixtureSource(outcomeMember: "private bool Safe => true;"), "exact internal sealed Phase5A outcome record", null!];
    }

    [Theory]
    [MemberData(nameof(PhaseFiveAdditionalRemediationSources))]
    public void PhaseFiveAdditionalCompiledRegressionIsRejected(
        string caseName, string targetType, string source, string requiredViolation, string? externalSource)
    {
        Assert.False(string.IsNullOrWhiteSpace(caseName));
        Assembly fixture = BuildFixtureAssembly(null, source, externalSource);
        AssertTargetedViolation(fixture, targetType, requiredViolation);
    }

    public static IEnumerable<object[]> PhaseFiveAdditionalRemediationSources()
    {
        yield return ["renamed external domain", PhaseFiveNegotiatorName, PhaseFiveFixtureSource("private static void Probe() => Remote.Components.Widget.Touch();"), "Remote.Components.Widget", "namespace Remote.Components { public static class Widget { public static void Touch() { } } }"];
        foreach (string member in new[]
        {
            "private int Extra => 1;",
            "private readonly int extra = 1;",
            "private event Action Extra { add { } remove { } }",
            "[System.Runtime.CompilerServices.CompilerGenerated] private int Extra => 1;",
            "private int Extra() => 1;",
        })
        {
            yield return [member, PhaseFiveOutcomeName, PhaseFiveFixtureSource(outcomeMember: member), "exact internal sealed Phase5A outcome record", null!];
        }
        yield return ["nested generic array argument", PhaseFiveNegotiatorName, PhaseFiveFixtureSource("private static int Probe() => System.Runtime.CompilerServices.Unsafe.SizeOf<System.Collections.Generic.List<System.IO.FileInfo[][]>>();"), "System.IO.FileInfo", null!];
        yield return ["generic constructor argument", PhaseFiveNegotiatorName, PhaseFiveFixtureSource("private static void Probe() { _ = new System.Collections.Generic.List<System.IO.FileInfo>(); }"), "System.IO.FileInfo", null!];
        yield return ["generic field argument", PhaseFiveNegotiatorName, PhaseFiveFixtureSource("private static void Probe() { _ = System.Collections.Immutable.ImmutableArray<System.IO.FileInfo>.Empty; }"), "System.IO.FileInfo", null!];
    }

    [Fact]
    public void PhaseFiveTypeTraversalReachesNestedElementAndGenericArguments()
    {
        Type wrapped = typeof(List<FileInfo[][]>).MakeArrayType().MakePointerType().MakeByRefType();
        List<Type> dependencies = [];
        AddTypeAndGenericArguments(dependencies, wrapped);
        Assert.Contains(typeof(FileInfo), dependencies);
        Assert.Contains(typeof(List<FileInfo[][]>), dependencies);
    }

    [Fact]
    public void ApprovedEvaluatorAsyncStateMachineIsTheExactMethodAttributedCompilerArtifact()
    {
        Type evaluator = typeof(Tcc.Themes.Integrity.ThemePackageInventoryEvaluator);
        MethodInfo method = evaluator.GetMethod(
            "EvaluateAsync",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            ?? throw new InvalidOperationException("Approved evaluator method is missing.");
        Type stateMachine = method.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType
            ?? throw new InvalidOperationException("Approved evaluator async state machine is missing.");

        Assert.Same(evaluator, method.DeclaringType);
        Assert.Same(evaluator, stateMachine.DeclaringType);
        Assert.True(stateMachine.IsNestedPrivate);
        Assert.True(stateMachine.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false));
        Assert.True(typeof(IAsyncStateMachine).IsAssignableFrom(stateMachine));
        Assert.True(IsApprovedEvaluatorAsyncStateMachine(stateMachine));
    }

    [Fact]
    public void ApprovedMetadataEvaluatorAsyncStateMachineIsTheExactMethodAttributedCompilerArtifact()
    {
        Type evaluator = typeof(Tcc.Themes.Integrity.ThemePackageMetadataEvaluator);
        MethodInfo method = evaluator.GetMethod(
            "EvaluateAsync",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            ?? throw new InvalidOperationException("Approved metadata evaluator method is missing.");
        Type stateMachine = method.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType
            ?? throw new InvalidOperationException("Approved metadata evaluator async state machine is missing.");

        Assert.Same(evaluator, method.DeclaringType);
        Assert.Same(evaluator, stateMachine.DeclaringType);
        Assert.True(stateMachine.IsNestedPrivate);
        Assert.True(stateMachine.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false));
        Assert.True(typeof(IAsyncStateMachine).IsAssignableFrom(stateMachine));
        Assert.True(IsApprovedEvaluatorAsyncStateMachine(stateMachine));
    }

    [Fact]
    public void OtherGeneratedTypesOnApprovedEvaluatorFailTheCompiledSurfaceBoundary()
    {
        Assembly fixture = BuildFixtureAssembly(
            markerAdditionalSource: null,
            otherSource:
            """
            using System.Runtime.CompilerServices;

            namespace Tcc.Themes.Integrity;

            internal static class ThemeIntegrityRequestBoundary { }
            internal sealed record ThemePackageInventoryEvaluation;
            internal static class ThemePackageInventoryEvaluator
            {
                internal static async System.Threading.Tasks.Task EvaluateAsync()
                {
                    await System.Threading.Tasks.Task.Yield();
                }

                internal static async System.Threading.Tasks.Task UnapprovedAsync()
                {
                    await System.Threading.Tasks.Task.Yield();
                }

                [CompilerGenerated]
                private sealed class GeneratedAttributeOnly { }
            }
            """);

        string[] violations = GetCompiledSurfaceViolations(fixture);

        Assert.DoesNotContain(violations, violation => violation.Contains("<EvaluateAsync>", StringComparison.Ordinal));
        Assert.Contains(violations, violation => violation.Contains("<UnapprovedAsync>", StringComparison.Ordinal));
        Assert.Contains(violations, violation => violation.Contains("GeneratedAttributeOnly", StringComparison.Ordinal));
    }

    [Fact]
    public void CandidateBAsyncStateMachineWithMutableStaticStateIsCompiledAndRejected()
    {
        Assembly fixture = BuildCandidateBAsyncStateMachineWithMutableStaticState();
        Type stateMachine = fixture.GetTypes().Single(type => type.Name.StartsWith(
            "<VerifyAndBindAsync>d__", StringComparison.Ordinal));

        Assert.Equal("HiddenCache", Assert.Single(stateMachine.GetFields(
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)).Name);
        Assert.False(IsApprovedCandidateBAsyncStateMachine(stateMachine));
        Assert.Contains(GetCompiledSurfaceViolations(fixture), violation =>
            violation.Contains(stateMachine.FullName!, StringComparison.Ordinal)
            && violation.Contains("mutable static state", StringComparison.Ordinal));
    }

    [Fact]
    public void AuthorizedCandidateBAsyncStateMachinesHaveZeroMutableStaticFields()
    {
        foreach ((Type owner, string methodName) in new[]
        {
            (typeof(Tcc.Themes.Compatibility.Binding.ThemeCompatibilityContentSnapshot), "CaptureAsync"),
            (typeof(Tcc.Themes.Compatibility.Binding.ThemeCompatibilityEvidenceBinder), "VerifyAndBindAsync"),
        })
        {
            MethodInfo method = owner.GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)!;
            Type stateMachine = GetAsyncStateMachineType(method)!;
            Assert.NotNull(stateMachine);
            Assert.DoesNotContain(
                stateMachine.GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly),
                field => !field.IsLiteral);
            Assert.True(IsApprovedCandidateBAsyncStateMachine(stateMachine));
        }
    }

    [Theory]
    [MemberData(nameof(PhaseFourDForbiddenShapeSources))]
    public void ApprovedPhaseFourDNamesRejectForbiddenVisibilityAndVerifierInterfaces(
        string targetType,
        string declarations,
        string requiredViolation)
    {
        Assembly fixture = BuildApprovedFixtureAssembly(declarations);

        AssertTargetedViolation(fixture, targetType, requiredViolation);
    }

    [Theory]
    [InlineData("IThemeIntegrityVerifier")]
    [InlineData("IThemeIntegrityVerifierV2")]
    public void ExistingApprovedProductionNameCannotImplementVerifierInterface(string interfaceName)
    {
        string method = interfaceName == "IThemeIntegrityVerifier"
            ? "public ValueTask<ThemeIntegrityVerificationResult> VerifyAsync(ThemeIntegrityVerificationRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();"
            : "public ValueTask<ThemeIntegrityVerificationResultV2> VerifyAsync(ThemeIntegrityVerificationRequestV2 request, IThemePackageContentReader contentReader, CancellationToken cancellationToken = default) => throw new NotSupportedException();";
        Assembly fixture = BuildApprovedFixtureAssembly(
            PhaseFourDValidDeclarations,
            $"internal sealed class ThemeIntegrityRequestBoundary : {interfaceName} {{ {method} }}");

        AssertTargetedViolation(
            fixture,
            "Tcc.Themes.Integrity.ThemeIntegrityRequestBoundary",
            $"Tcc.Presentation.Contracts.Theme.{interfaceName}");
    }

    [Theory]
    [MemberData(nameof(AbstractVerifierInterfaceSources))]
    public void AbstractTypesCannotBypassGlobalVerifierInterfaceProhibition(
        string targetType,
        string declarations,
        string requiredInterface)
    {
        Assembly fixture = BuildFixtureAssembly(
            markerAdditionalSource: null,
            otherSource: declarations);

        AssertTargetedViolation(fixture, targetType, requiredInterface);
    }

    [Fact]
    public void ExistingApprovedValueTypeCannotImplementVerifierInterface()
    {
        Assembly fixture = BuildApprovedFixtureAssembly(
            PhaseFourDValidDeclarations,
            $"internal struct ThemeIntegrityRequestBoundary : IThemeIntegrityVerifier {{ {V1VerifierMethod} }}");

        AssertTargetedViolation(
            fixture,
            "Tcc.Themes.Integrity.ThemeIntegrityRequestBoundary",
            "Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifier");
    }

    [Fact]
    public void ExistingApprovedValueTypeCannotImplementV2VerifierInterface()
    {
        Assembly fixture = BuildApprovedFixtureAssembly(
            PhaseFourDValidDeclarations,
            $"internal struct ThemeIntegrityRequestBoundary : IThemeIntegrityVerifierV2 {{ {V2VerifierMethod} }}");

        AssertTargetedViolation(
            fixture,
            "Tcc.Themes.Integrity.ThemeIntegrityRequestBoundary",
            "Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifierV2");
    }

    [Fact]
    public void CompilerGeneratedAttributeAndApprovedArtifactNameDoNotBypassShapeValidation()
    {
        Assembly fixture = BuildForgedClosureArtifact(
            "Tcc.Themes.Integrity.ThemeIntegrityRequestBoundary",
            nestedPrivate: true);

        AssertTargetedViolation(fixture, "ThemeIntegrityRequestBoundary+<>c", "nested production type");
    }

    [Fact]
    public void StandaloneCompilerGeneratedTypeWithApprovedNameDoesNotBypassShapeValidation()
    {
        Assembly fixture = BuildForgedStandaloneGeneratedArtifact();

        AssertTargetedViolation(fixture, "<>z__ReadOnlyArray`1", "outside the exact Phase 3 surface");
    }

    [Fact]
    public void ApprovedLookingGeneratedArtifactUnderWrongOwnerFails()
    {
        Assembly fixture = BuildApprovedFixtureAssembly(
            """
            internal static class ThemePackageSignatureEvaluator
            {
                internal static Func<int, int> Capture(int value) => input => input + value;
            }
            internal sealed record ThemePackageSignatureEvaluation;
            """);

        AssertTargetedViolation(fixture, "ThemePackageSignatureEvaluator+<>c__DisplayClass", "nested production type");
    }

    [Fact]
    public void AsyncArtifactWhoseOwningMethodTargetsAnotherStateMachineFails()
    {
        AssemblyBuilder fixture = BuildForgedWrongAttributeTargetArtifact();
        ResolveEventHandler resolver = (_, eventArgs) =>
            AssemblyName.ReferenceMatchesDefinition(new AssemblyName(eventArgs.Name), fixture.GetName())
                ? fixture
                : null;
        AppDomain.CurrentDomain.AssemblyResolve += resolver;
        try
        {
            AssertTargetedViolation(fixture, "<EvaluateAsync>d__0", "nested production type");
        }
        finally
        {
            AppDomain.CurrentDomain.AssemblyResolve -= resolver;
        }
    }

    [Fact]
    public void AsyncArtifactMissingIAsyncStateMachineFails()
    {
        Assembly fixture = BuildFixtureAssembly(
            markerAdditionalSource: null,
            otherSource:
            """
            using System.Runtime.CompilerServices;

            namespace Tcc.Themes.Integrity;

            internal static class ThemePackageInventoryEvaluator
            {
                [AsyncStateMachine(typeof(ForgedStateMachine))]
                internal static void EvaluateAsync() { }

                [CompilerGenerated]
                private struct ForgedStateMachine { }
            }
            """);

        AssertTargetedViolation(fixture, "ForgedStateMachine", "nested production type");
    }

    [Fact]
    public void IAsyncStateMachineArtifactMissingOwningMethodProvenanceFails()
    {
        Assembly fixture = BuildFixtureAssembly(
            markerAdditionalSource: null,
            otherSource:
            """
            using System.Runtime.CompilerServices;

            namespace Tcc.Themes.Integrity;

            internal static class ThemePackageInventoryEvaluator
            {
                internal static void EvaluateAsync() { }

                [CompilerGenerated]
                private struct ForgedStateMachine : IAsyncStateMachine
                {
                    public void MoveNext() { }
                    public void SetStateMachine(IAsyncStateMachine stateMachine) { }
                }
            }
            """);

        AssertTargetedViolation(fixture, "ForgedStateMachine", "nested production type");
    }

    [Fact]
    public void PhaseFourDEvaluatorCannotIntroduceAnAsyncStateMachine()
    {
        Assembly fixture = BuildApprovedFixtureAssembly(
            """
            internal static class ThemePackageSignatureEvaluator
            {
                internal static async Task EvaluateAsync() => await Task.Yield();
            }
            internal sealed record ThemePackageSignatureEvaluation;
            """);

        AssertTargetedViolation(fixture, "ThemePackageSignatureEvaluator+<EvaluateAsync>", "nested production type");
    }

    [Fact]
    public void PhaseFourDEvaluatorCannotIntroduceADisplayClass()
    {
        Assembly fixture = BuildApprovedFixtureAssembly(
            """
            internal static class ThemePackageSignatureEvaluator
            {
                internal static Func<int, int> Capture(int value) => input => input + value;
            }
            internal sealed record ThemePackageSignatureEvaluation;
            """);

        AssertTargetedViolation(fixture, "DisplayClass", "nested production type");
    }

    [Fact]
    public void PhaseFourDEvaluatorCannotIntroduceAnIteratorArtifact()
    {
        Assembly fixture = BuildApprovedFixtureAssembly(
            """
            internal static class ThemePackageSignatureEvaluator
            {
                internal static IEnumerable<int> Iterate()
                {
                    yield return 1;
                }
            }
            internal sealed record ThemePackageSignatureEvaluation;
            """);

        AssertTargetedViolation(fixture, "ThemePackageSignatureEvaluator+<Iterate>", "nested production type");
    }

    [Fact]
    public void ApprovedSimpleNameInWrongNamespaceFails()
    {
        Assembly fixture = BuildApprovedFixtureAssembly(
            PhaseFourDValidDeclarations,
            extraSource: "namespace Tcc.Themes.Runtime { internal static class ThemePackageSignatureEvaluator { } }");

        AssertTargetedViolation(
            fixture,
            "Tcc.Themes.Runtime.ThemePackageSignatureEvaluator",
            "outside the exact Phase 3 surface");
    }

    [Fact]
    public void ApprovedSimpleNameNestedUnderWrongDeclaringTypeFails()
    {
        Assembly fixture = BuildApprovedFixtureAssembly(
            PhaseFourDValidDeclarations,
            extraSource:
            "namespace Tcc.Themes.Integrity { internal static class WrongOwner { internal sealed record ThemePackageSignatureEvaluation; } }");

        AssertTargetedViolation(
            fixture,
            "WrongOwner+ThemePackageSignatureEvaluation",
            "nested production type");
    }

    public static IEnumerable<object[]> PhaseFiveForbiddenShapeSources()
    {
        yield return
        [
            "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiator",
            "namespace Tcc.Themes.Compatibility; public static class ThemeCompatibilityVersionNegotiator { }",
            "exact internal static Phase5A negotiator",
        ];
        yield return
        [
            "Tcc.Themes.Compatibility.SecondNegotiator",
            "namespace Tcc.Themes.Compatibility; internal static class ThemeCompatibilityVersionNegotiator { } internal static class SecondNegotiator { }",
            "outside the exact Phase 3 surface",
        ];
        yield return
        [
            "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiation",
            "namespace Tcc.Themes.Compatibility; public sealed record ThemeCompatibilityVersionNegotiation;",
            "exact internal sealed Phase5A outcome record",
        ];
        yield return
        [
            "Tcc.Themes.Compatibility.ThemeCompatibilityNegotiationFailureKind",
            "namespace Tcc.Themes.Compatibility; public enum ThemeCompatibilityNegotiationFailureKind { InvalidVersionInput }",
            "exact internal Phase5A failure enum",
        ];
        yield return
        [
            "Tcc.Themes.Compatibility.RogueCompatibilityResolver",
            "using Tcc.Presentation.Contracts.Theme; namespace Tcc.Themes.Compatibility; public sealed class RogueCompatibilityResolver : IThemeCompatibilityResolver { public ThemeCompatibilityResult Resolve(ThemeCompatibilityRequest request) => throw new NotSupportedException(); }",
            "IThemeCompatibilityResolver",
        ];
        yield return
        [
            "Tcc.Themes.Compatibility.FileSystemCompatibilityProbe",
            "namespace Tcc.Themes.Compatibility; internal static class FileSystemCompatibilityProbe { internal static System.IO.Stream Open() => throw new NotSupportedException(); }",
            "System.IO.Stream",
        ];
        yield return
        [
            "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiator",
            """
            using Tcc.Presentation.Contracts.Theme;
            namespace Tcc.Themes.Compatibility
            {
                internal enum ThemeCompatibilityNegotiationFailureKind { InvalidVersionInput, UnsatisfiableVersionRange, UnsupportedManifestSchema, ConflictingVersionDeclaration, CoreVersionIncompatible, ThemeApiNoCompatibleVersion, UxContractNoCompatibleVersion }
                internal sealed record ThemeCompatibilityVersionNegotiation(bool CanContinue, string? SelectedThemeApiVersion, string? SelectedUxContractVersion, System.Collections.Immutable.ImmutableArray<ThemeCompatibilityNegotiationFailureKind> Failures);
                internal static class ThemeCompatibilityVersionNegotiator
                {
                    internal static ThemeCompatibilityVersionNegotiation Negotiate(ThemeCompatibilityRequest request) => throw new NotSupportedException();
                    private static bool ProbeFileSystem() => System.IO.File.Exists("probe");
                }
            }
            """,
            "System.IO.File",
        ];
        yield return
        [
            "Tcc.Themes.Compatibility.PackageReaderCompatibilityProbe",
            "using Tcc.Presentation.Contracts.Theme; namespace Tcc.Themes.Compatibility; internal static class PackageReaderCompatibilityProbe { internal static void Read(IThemePackageContentReader reader) { } }",
            "IThemePackageContentReader",
        ];
        yield return
        [
            "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiator",
            """
            using Tcc.Presentation.Contracts.Theme;
            namespace Tcc.Themes.Integrity
            {
                internal static class ThemePackageSignatureEvaluator { internal static void Evaluate() { } }
                internal sealed record ThemePackageSignatureEvaluation;
            }
            namespace Tcc.Themes.Compatibility
            {
                internal enum ThemeCompatibilityNegotiationFailureKind { InvalidVersionInput, UnsatisfiableVersionRange, UnsupportedManifestSchema, ConflictingVersionDeclaration, CoreVersionIncompatible, ThemeApiNoCompatibleVersion, UxContractNoCompatibleVersion }
                internal sealed record ThemeCompatibilityVersionNegotiation(bool CanContinue, string? SelectedThemeApiVersion, string? SelectedUxContractVersion, System.Collections.Immutable.ImmutableArray<ThemeCompatibilityNegotiationFailureKind> Failures);
                internal static class ThemeCompatibilityVersionNegotiator
                {
                    internal static ThemeCompatibilityVersionNegotiation Negotiate(ThemeCompatibilityRequest request) => throw new NotSupportedException();
                    private static void ProbeIntegrity() => Tcc.Themes.Integrity.ThemePackageSignatureEvaluator.Evaluate();
                }
            }
            """,
            "Tcc.Themes.Integrity.ThemePackageSignatureEvaluator",
        ];
        yield return
        [
            "Tcc.Themes.Wrong.ThemeCompatibilityVersionNegotiator",
            "namespace Tcc.Themes.Wrong; internal static class ThemeCompatibilityVersionNegotiator { }",
            "outside the exact Phase 3 surface",
        ];
        yield return
        [
            "Tcc.Themes.Compatibility.ThemeCompatibilityTradingAI",
            "namespace Tcc.Themes.Compatibility; internal static class ThemeCompatibilityTradingAI { }",
            "TradingAI",
        ];
    }

    public static IEnumerable<object[]> PhaseFourDForbiddenShapeSources()
    {
        yield return
        [
            "Tcc.Themes.Integrity.ThemePackageSignatureEvaluator",
            "public static class ThemePackageSignatureEvaluator { } internal sealed record ThemePackageSignatureEvaluation;",
            "must remain the internal",
        ];
        yield return
        [
            "Tcc.Themes.Integrity.ThemePackageSignatureEvaluation",
            "internal static class ThemePackageSignatureEvaluator { } public sealed record ThemePackageSignatureEvaluation;",
            "must remain the internal",
        ];
        yield return
        [
            "Tcc.Themes.Integrity.ThemePackageSignatureEvaluator",
            $"internal sealed class ThemePackageSignatureEvaluator : IThemeIntegrityVerifier {{ {V1VerifierMethod} }} internal sealed record ThemePackageSignatureEvaluation;",
            "IThemeIntegrityVerifier",
        ];
        yield return
        [
            "Tcc.Themes.Integrity.ThemePackageSignatureEvaluator",
            $"internal sealed class ThemePackageSignatureEvaluator : IThemeIntegrityVerifierV2 {{ {V2VerifierMethod} }} internal sealed record ThemePackageSignatureEvaluation;",
            "IThemeIntegrityVerifierV2",
        ];
        yield return
        [
            "Tcc.Themes.Integrity.ThemePackageSignatureEvaluation",
            $"internal static class ThemePackageSignatureEvaluator {{ }} internal sealed record ThemePackageSignatureEvaluation : IThemeIntegrityVerifier {{ {V1VerifierMethod} }}",
            "IThemeIntegrityVerifier",
        ];
        yield return
        [
            "Tcc.Themes.Integrity.ThemePackageSignatureEvaluation",
            $"internal static class ThemePackageSignatureEvaluator {{ }} internal sealed record ThemePackageSignatureEvaluation : IThemeIntegrityVerifierV2 {{ {V2VerifierMethod} }}",
            "IThemeIntegrityVerifierV2",
        ];
    }

    public static IEnumerable<object[]> AbstractVerifierInterfaceSources()
    {
        yield return
        [
            "Tcc.Themes.Integrity.AbstractVerifierV1",
            VerifierFixtureDeclaration(
                "internal abstract class AbstractVerifierV1",
                "IThemeIntegrityVerifier",
                V1VerifierMethod),
            "Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifier",
        ];
        yield return
        [
            "Tcc.Themes.Integrity.AbstractVerifierV2",
            VerifierFixtureDeclaration(
                "internal abstract class AbstractVerifierV2",
                "IThemeIntegrityVerifierV2",
                V2VerifierMethod),
            "Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifierV2",
        ];
        yield return
        [
            "Tcc.Themes.Integrity.ThemeIntegrityRequestBoundary",
            VerifierFixtureDeclaration(
                "internal abstract class ThemeIntegrityRequestBoundary",
                "IThemeIntegrityVerifier",
                V1VerifierMethod),
            "Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifier",
        ];
        yield return
        [
            "Tcc.Themes.Integrity.ThemeIntegrityRequestBoundary",
            VerifierFixtureDeclaration(
                "internal abstract class ThemeIntegrityRequestBoundary",
                "IThemeIntegrityVerifierV2",
                V2VerifierMethod),
            "Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifierV2",
        ];
        yield return
        [
            "Tcc.Themes.Integrity.NestedVerifierOwner+NestedAbstractVerifierV1",
            VerifierFixtureDeclaration(
                "internal static class NestedVerifierOwner { internal abstract class NestedAbstractVerifierV1",
                "IThemeIntegrityVerifier",
                $"{V1VerifierMethod} }}"),
            "Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifier",
        ];
        yield return
        [
            "Tcc.Themes.Integrity.NestedVerifierOwner+NestedAbstractVerifierV2",
            VerifierFixtureDeclaration(
                "internal static class NestedVerifierOwner { internal abstract class NestedAbstractVerifierV2",
                "IThemeIntegrityVerifierV2",
                $"{V2VerifierMethod} }}"),
            "Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifierV2",
        ];
        yield return
        [
            "Tcc.Themes.Integrity.CompilerGeneratedAbstractVerifierV1",
            VerifierFixtureDeclaration(
                "[CompilerGenerated] internal abstract class CompilerGeneratedAbstractVerifierV1",
                "IThemeIntegrityVerifier",
                V1VerifierMethod),
            "Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifier",
        ];
        yield return
        [
            "Tcc.Themes.Integrity.CompilerGeneratedAbstractVerifierV2",
            VerifierFixtureDeclaration(
                "[CompilerGenerated] internal abstract class CompilerGeneratedAbstractVerifierV2",
                "IThemeIntegrityVerifierV2",
                V2VerifierMethod),
            "Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifierV2",
        ];
    }

    [Theory]
    [InlineData("internal sealed class ThemeIntegrityVerifier", "IThemeIntegrityVerifierV2", false)]
    [InlineData("public abstract class ThemeIntegrityVerifier", "IThemeIntegrityVerifierV2", false)]
    [InlineData("public class ThemeIntegrityVerifier", "IThemeIntegrityVerifierV2", false)]
    [InlineData("public sealed class ThemeIntegrityVerifier<T>", "IThemeIntegrityVerifierV2", false)]
    [InlineData("public struct ThemeIntegrityVerifier", "IThemeIntegrityVerifierV2", false)]
    [InlineData("[CompilerGenerated] public sealed class ThemeIntegrityVerifier", "IThemeIntegrityVerifierV2", false)]
    [InlineData("public sealed class ThemeIntegrityVerifier", "IThemeIntegrityVerifier", false)]
    [InlineData("public sealed class OtherVerifier", "IThemeIntegrityVerifierV2", false)]
    [InlineData("public sealed class ThemeIntegrityVerifier", "IThemeIntegrityVerifierV2", true)]
    public void PublicVerifierAllowanceRejectsEveryLookalike(string declaration, string contract, bool wrongNamespace)
    {
        string method = contract == "IThemeIntegrityVerifier" ? V1VerifierMethod : V2VerifierMethod;
        string source = VerifierFixtureDeclaration(declaration, contract, method);
        if (wrongNamespace) source = source.Replace("namespace Tcc.Themes.Integrity;", "namespace Tcc.Themes.Wrong;", StringComparison.Ordinal);
        Assembly fixture = BuildFixtureAssembly(null, source);
        Type target = Assert.Single(fixture.GetTypes(), type => type.Name.StartsWith("ThemeIntegrityVerifier", StringComparison.Ordinal) || type.Name == "OtherVerifier");
        Assert.False(IsExactPublicVerifier(target));
        Assert.Contains(GetCompiledSurfaceViolations(fixture), violation => violation.Contains(target.FullName!, StringComparison.Ordinal));
    }

    [Fact]
    public void ExactV2AllowanceStillRejectsNestedSecondAndDualInterfaceVerifiers()
    {
        Assembly fixture = BuildFixtureAssembly(null, $$"""
            using Tcc.Presentation.Contracts.Theme;
            namespace Tcc.Themes.Integrity;
            public sealed class ThemeIntegrityVerifier : IThemeIntegrityVerifierV2 { {{V2VerifierMethod}} }
            public static class Owner { public sealed class ThemeIntegrityVerifier : IThemeIntegrityVerifierV2 { {{V2VerifierMethod}} } }
            public sealed class SecondVerifier : IThemeIntegrityVerifierV2 { {{V2VerifierMethod}} }
            public sealed class DualVerifier : IThemeIntegrityVerifierV2, IThemeIntegrityVerifier { {{V2VerifierMethod}} {{V1VerifierMethod}} }
            """);
        Assert.True(IsExactPublicVerifier(fixture.GetType("Tcc.Themes.Integrity.ThemeIntegrityVerifier")!));
        string[] violations = GetCompiledSurfaceViolations(fixture);
        foreach (string name in new[] { "Owner+ThemeIntegrityVerifier", "SecondVerifier", "DualVerifier" })
            Assert.Contains(violations, violation => violation.Contains(name, StringComparison.Ordinal) && violation.Contains("verifier interface", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("wrong owner")]
    [InlineData("wrong target")]
    [InlineData("fake name")]
    [InlineData("no interface")]
    [InlineData("public nested")]
    [InlineData("no attribute")]
    public void PublicVerifierAsyncProvenanceRejectsForgedArtifacts(string attack)
    {
        AssemblyName name = new($"Tcc.Phase4E.Forged.{Guid.NewGuid():N}");
        AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
        ModuleBuilder module = assembly.DefineDynamicModule(name.Name!);
        TypeBuilder owner = module.DefineType(attack == "wrong owner" ? "Tcc.Themes.Integrity.OtherOwner" : "Tcc.Themes.Integrity.ThemeIntegrityVerifier",
            TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class);
        owner.DefineDefaultConstructor(MethodAttributes.Public);
        owner.AddInterfaceImplementation(typeof(Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifierV2));
        TypeBuilder artifact;
        if (attack is "no interface" or "public nested")
        {
            artifact = owner.DefineNestedType("<VerifyAsync>d__7", (attack == "public nested" ? TypeAttributes.NestedPublic : TypeAttributes.NestedPrivate) | TypeAttributes.Sealed, typeof(ValueType));
            artifact.SetCustomAttribute(CompilerGeneratedAttributeBuilder());
            if (attack == "public nested")
            {
                artifact.AddInterfaceImplementation(typeof(IAsyncStateMachine));
                DefineStateMachineMethod(artifact, "MoveNext", Type.EmptyTypes);
                DefineStateMachineMethod(artifact, "SetStateMachine", [typeof(IAsyncStateMachine)]);
            }
        }
        else artifact = DefineForgedStateMachine(owner, attack == "fake name" ? "ForgedStateMachine" : "<VerifyAsync>d__7");
        MethodInfo contractMethod = typeof(Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifierV2).GetMethod("VerifyAsync")!;
        MethodBuilder method = owner.DefineMethod("VerifyAsync", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.NewSlot,
            contractMethod.ReturnType, contractMethod.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        method.DefineParameter(3, ParameterAttributes.Optional | ParameterAttributes.HasDefault, "cancellationToken").SetConstant(null);
        method.GetILGenerator().Emit(OpCodes.Ldnull); method.GetILGenerator().Emit(OpCodes.Throw);
        owner.DefineMethodOverride(method, contractMethod);
        Type target = attack == "wrong target" ? typeof(UnrelatedVerifierStateMachine) : artifact;
        if (attack != "no attribute") method.SetCustomAttribute(new CustomAttributeBuilder(typeof(AsyncStateMachineAttribute).GetConstructor([typeof(Type)])!, [target]));
        Type compiledArtifact = artifact.CreateType()!;
        owner.CreateType();
        ResolveEventHandler resolver = (_, args) => AssemblyName.ReferenceMatchesDefinition(new AssemblyName(args.Name), name) ? assembly : null;
        AppDomain.CurrentDomain.AssemblyResolve += resolver;
        try
        {
            if (attack != "wrong owner") Assert.True(IsExactPublicVerifier(compiledArtifact.DeclaringType!));
            Assert.False(IsApprovedVerifierAsyncStateMachine(compiledArtifact));
            Assert.Contains(GetCompiledSurfaceViolations(assembly), violation => violation.Contains(compiledArtifact.FullName!, StringComparison.Ordinal));
        }
        finally { AppDomain.CurrentDomain.AssemblyResolve -= resolver; }
    }

    private struct UnrelatedVerifierStateMachine : IAsyncStateMachine
    {
        public void MoveNext() { }
        public void SetStateMachine(IAsyncStateMachine stateMachine) { }
    }

    internal static string[] GetCompiledSurfaceViolations(Assembly assembly)
    {
        List<string> violations = [];
        Type[] allTypes = assembly.GetTypes();
        Type[] topLevelTypes = allTypes
            .Where(type => type.DeclaringType is null && !IsApprovedSealedBaselineCompilerArtifact(type))
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        string[] actualTopLevelTypes = topLevelTypes
            .Select(type => type.FullName ?? type.Name)
            .ToArray();

        foreach (string missingType in ApprovedTopLevelTypes.Except(actualTopLevelTypes, StringComparer.Ordinal))
        {
            violations.Add($"Approved Phase 3 type '{missingType}' is missing from the compiled assembly.");
        }

        foreach (string unauthorizedType in actualTopLevelTypes.Except(ApprovedTopLevelTypes, StringComparer.Ordinal))
        {
            violations.Add($"Compiled production type '{unauthorizedType}' is outside the exact Phase 3 surface.");
        }

        foreach (Type candidateBGeneratedArtifact in allTypes.Where(type =>
                     HasCandidateBGeneratedArtifactShape(type)
                     && !HasNoMutableStaticFields(type)))
        {
            violations.Add(
                $"Candidate B generated artifact forbids mutable static state: {candidateBGeneratedArtifact.FullName}.");
        }

        foreach (Type unauthorizedNestedType in allTypes.Where(type =>
                     type.IsNested
                     && !string.Equals(type.FullName, ApprovedNestedType, StringComparison.Ordinal)
                     && !IsApprovedSealedBaselineCompilerArtifact(type)
                     && !IsApprovedEvaluatorAsyncStateMachine(type)
                     && !IsApprovedVerifierAsyncStateMachine(type)
                     && !IsApprovedCandidateBAsyncStateMachine(type)))
        {
            violations.Add(
                $"Compiled nested production type '{unauthorizedNestedType.FullName}' is outside the exact Phase 3 surface.");
        }

        Type? marker = assembly.GetType("Tcc.Themes.AssemblyMarker", throwOnError: false, ignoreCase: false);
        if (marker is not null && (!marker.IsPublic || !marker.IsAbstract || !marker.IsSealed || marker.GetInterfaces().Length != 0))
        {
            violations.Add("Tcc.Themes.AssemblyMarker must remain a public static marker with no implemented interfaces.");
        }

        Type? validator = assembly.GetType(
            "Tcc.Themes.Manifests.ThemeManifestValidator",
            throwOnError: false,
            ignoreCase: false);
        if (validator is not null)
        {
            string[] implementedInterfaces = validator.GetInterfaces()
                .Select(type => type.FullName ?? type.Name)
                .Order(StringComparer.Ordinal)
                .ToArray();
            if (!validator.IsPublic || !validator.IsClass || validator.IsAbstract || !validator.IsSealed
                || !implementedInterfaces.SequenceEqual(
                    ["Tcc.Presentation.Contracts.Theme.IThemeManifestValidator"],
                    StringComparer.Ordinal))
            {
                violations.Add(
                    "Tcc.Themes.Manifests.ThemeManifestValidator must be the public sealed concrete implementation of only IThemeManifestValidator.");
            }
        }

        Type? signatureEvaluator = assembly.GetType(
            "Tcc.Themes.Integrity.ThemePackageSignatureEvaluator",
            throwOnError: false,
            ignoreCase: false);
        if (signatureEvaluator is not null && !IsExactPhaseFourDSignatureEvaluator(signatureEvaluator))
        {
            violations.Add(
                "Tcc.Themes.Integrity.ThemePackageSignatureEvaluator must remain the internal top-level non-generic static Phase 4D evaluator with no implemented interfaces or generated-type shape.");
        }

        Type? signatureEvaluation = assembly.GetType(
            "Tcc.Themes.Integrity.ThemePackageSignatureEvaluation",
            throwOnError: false,
            ignoreCase: false);
        if (signatureEvaluation is not null && !IsExactPhaseFourDSignatureEvaluation(signatureEvaluation))
        {
            violations.Add(
                "Tcc.Themes.Integrity.ThemePackageSignatureEvaluation must remain the internal top-level non-generic sealed Phase 4D record class with only its self IEquatable interface and no generated-type shape.");
        }

        Type? verifier = assembly.GetType("Tcc.Themes.Integrity.ThemeIntegrityVerifier", false, false);
        if (verifier is not null && !IsExactPublicVerifier(verifier))
        {
            violations.Add("Tcc.Themes.Integrity.ThemeIntegrityVerifier must have the exact public sealed V2-only shape.");
        }

        Type? negotiator = assembly.GetType(
            "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiator",
            throwOnError: false,
            ignoreCase: false);
        if (negotiator is not null && !IsExactPhaseFiveNegotiator(negotiator))
        {
            violations.Add(
                "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiator must remain the exact internal static Phase5A negotiator.");
        }

        Type? negotiation = assembly.GetType(
            "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiation",
            throwOnError: false,
            ignoreCase: false);
        if (negotiation is not null && !IsExactPhaseFiveNegotiation(negotiation))
        {
            violations.Add(
                "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiation must remain the exact internal sealed Phase5A outcome record.");
        }

        Type? failureKind = assembly.GetType(
            "Tcc.Themes.Compatibility.ThemeCompatibilityNegotiationFailureKind",
            throwOnError: false,
            ignoreCase: false);
        if (failureKind is not null && !IsExactPhaseFiveFailureKind(failureKind))
        {
            violations.Add(
                "Tcc.Themes.Compatibility.ThemeCompatibilityNegotiationFailureKind must remain the exact internal Phase5A failure enum.");
        }

        Type? binder = assembly.GetType(
            "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityEvidenceBinder", false, false);
        if (binder is null)
        {
            violations.Add("ThemeCompatibilityEvidenceBinder is required by the exact Candidate B surface.");
        }
        else if (!IsExactCandidateBStaticType(binder,
                     "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityEvidenceBinder"))
        {
            violations.Add("ThemeCompatibilityEvidenceBinder must remain the exact internal top-level non-generic static Candidate B binder.");
        }

        Type? materializer = assembly.GetType(
            "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityManifestMaterializer", false, false);
        if (materializer is null)
        {
            violations.Add("ThemeCompatibilityManifestMaterializer is required by the exact Candidate B surface.");
        }
        else if (!IsExactCandidateBStaticType(materializer,
                     "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityManifestMaterializer"))
        {
            violations.Add("ThemeCompatibilityManifestMaterializer must remain the exact internal top-level non-generic static Candidate B materializer.");
        }

        Type? snapshot = assembly.GetType(
            "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityContentSnapshot", false, false);
        if (snapshot is null)
        {
            violations.Add("ThemeCompatibilityContentSnapshot is required by the exact Candidate B surface.");
        }
        else if (!IsExactCandidateBContentSnapshot(snapshot))
        {
            violations.Add("ThemeCompatibilityContentSnapshot must remain the exact internal top-level sealed Candidate B package reader with controlled construction.");
        }

        string[] candidateBTypeNames =
        [
            "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityEvidenceBinder",
            "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityContentSnapshot",
            "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityManifestMaterializer",
            PhaseFiveThemeCompatibilityContractAmendmentTests.ResolverTypeName,
        ];
        foreach (Type candidateBType in candidateBTypeNames
                     .Select(name => assembly.GetType(name, false, false))
                     .OfType<Type>())
        {
            foreach (FieldInfo field in candidateBType.GetFields(
                         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (!field.IsLiteral)
                {
                    violations.Add($"Candidate B forbids mutable or reference-backed static state: {candidateBType.FullName}.{field.Name}.");
                }
            }
        }

        foreach (Type productionType in allTypes)
        {
            foreach (string forbiddenInterface in productionType.GetInterfaces()
                         .Select(contract => contract.FullName ?? contract.Name)
                         .Where(ForbiddenVerifierInterfaces.Contains))
            {
                if (forbiddenInterface == "Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifierV2"
                    && IsExactPublicVerifier(productionType))
                {
                    continue;
                }
                violations.Add(
                    $"Production type '{productionType.FullName ?? productionType.Name}' must not implement Phase 4D-forbidden verifier interface '{forbiddenInterface}'.");
            }
        }

        foreach (Type productionType in allTypes.Where(type => !type.IsInterface
                     && type.GetInterfaces().Any(contract => string.Equals(
                         contract.FullName,
                         ForbiddenCompatibilityResolverInterface,
                         StringComparison.Ordinal))))
        {
            violations.Add(
                $"Production type '{productionType.FullName ?? productionType.Name}' must not implement Phase5A-forbidden interface '{ForbiddenCompatibilityResolverInterface}'.");
        }

        List<Type> v2Resolvers = [];
        foreach (Type productionType in allTypes)
        {
            if (productionType.GetInterfaces().Any(contract => contract.FullName == "Tcc.Themes.Compatibility.V2.IThemeCompatibilityResolverV2"))
                v2Resolvers.Add(productionType);
            violations.AddRange(PhaseFiveThemeCompatibilityContractAmendmentTests.ResolverShapeViolations(productionType));
            if (string.Equals(productionType.FullName,
                    PhaseFiveThemeCompatibilityContractAmendmentTests.ResolverTypeName,
                    StringComparison.Ordinal))
            {
                foreach (Type dependency in GetMemberDependencyTypes(productionType))
                {
                    Type definition = dependency.IsGenericType ? dependency.GetGenericTypeDefinition() : dependency;
                    if (!ApprovedCandidateBResolverDependencyTypes.Contains(definition))
                    {
                        violations.Add($"Candidate B resolver dependency outside exact allowlist: {productionType.FullName} -> {dependency.FullName}.");
                    }
                }
            }
            violations.AddRange(PhaseFiveThemeCompatibilityContractAmendmentTests.RuntimeShapeViolations(productionType));
            if (PhaseFiveThemeCompatibilityContractAmendmentTests.RuntimeTypeNames.Contains(productionType.FullName, StringComparer.Ordinal))
            {
                foreach (Type dependency in GetMemberDependencyTypes(productionType))
                    if (!PhaseFiveThemeCompatibilityContractAmendmentTests.RuntimeDependencyAllowed(dependency, assembly))
                        violations.Add($"Candidate A dependency outside exact allowlist: {productionType.FullName} -> {dependency.FullName}.");
            }
        }


        if (v2Resolvers.Count != 1)
        {
            violations.Add($"Candidate B requires exactly one legal V2 resolver implementation; found {v2Resolvers.Count}.");
        }

        foreach (Type compatibilityType in allTypes.Where(type => string.Equals(
                     type.Namespace,
                     "Tcc.Themes.Compatibility",
                     StringComparison.Ordinal)))
        {
            if (compatibilityType.IsVisible)
            {
                violations.Add(
                    $"Phase5A compatibility type '{compatibilityType.FullName ?? compatibilityType.Name}' must not be public.");
            }

            foreach (Type dependencyType in GetMemberDependencyTypes(compatibilityType))
            {
                string dependencyName = dependencyType.FullName ?? dependencyType.Name;
                if (IsForbiddenPhaseFiveDependency(dependencyType, assembly))
                {
                    violations.Add(
                        $"Phase5A compatibility type '{compatibilityType.FullName ?? compatibilityType.Name}' has forbidden dependency '{dependencyName}'.");
                }
            }

            string[] admissionNames =
            [
                "Compatible", "Verified", "Installable", "Installed", "Enabled", "Active",
                "Activatable", "Safe", "Accessible", "MigrationReady", "RollbackAvailable", "RollbackReady",
            ];
            foreach (MemberInfo member in compatibilityType.GetMembers(
                         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                // The approved failure enum has NoCompatibleVersion members, not admission flags.
                if (!compatibilityType.IsEnum && admissionNames.Any(name =>
                        NormalizeSymbol(member.Name).Contains(name, StringComparison.OrdinalIgnoreCase)))
                {
                    violations.Add($"Phase5A type '{compatibilityType.FullName}' has unauthorized admission member '{member.Name}'.");
                }
            }
        }


        Type? diagnosticCodes = assembly.GetType(ApprovedNestedType, throwOnError: false, ignoreCase: false);
        if (diagnosticCodes is not null
            && (!diagnosticCodes.IsNestedPrivate || !diagnosticCodes.IsAbstract || !diagnosticCodes.IsSealed))
        {
            violations.Add(
                "ThemeManifestValidator.DiagnosticCodes must remain a private static implementation detail.");
        }

        foreach (AssemblyName dependency in assembly.GetReferencedAssemblies()
                     .Where(reference => reference.Name?.StartsWith("Tcc.", StringComparison.Ordinal) == true
                         && !string.Equals(reference.Name, "Tcc.Presentation.Contracts", StringComparison.Ordinal)))
        {
            violations.Add($"Compiled Tcc.Themes dependency '{dependency.Name}' is outside the Phase 3 allowlist.");
        }

        foreach (Type type in allTypes)
        {
            IEnumerable<string> compiledSymbols =
            [
                type.FullName ?? type.Name,
                .. type.GetInterfaces().Select(contract => contract.FullName ?? contract.Name),
                .. type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Select(member => member.Name),
            ];
            foreach (string fragment in ForbiddenCompiledSymbolFragments)
            {
                if (compiledSymbols.Any(symbol => NormalizeSymbol(symbol).Contains(fragment, StringComparison.OrdinalIgnoreCase)))
                {
                    violations.Add(
                        $"Compiled symbol on '{type.FullName ?? type.Name}' contains forbidden Phase 3 fragment '{fragment}'.");
                }
            }
        }

        return violations
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    internal static void AssertExactVerifierImplementations(IEnumerable<Type> productionTypes)
    {
        Type[] types = productionTypes.Where(type => !type.IsInterface).ToArray();
        Assert.DoesNotContain(types, type => typeof(Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifier).IsAssignableFrom(type));
        Type verifier = Assert.Single(types, type => typeof(Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifierV2).IsAssignableFrom(type));
        Assert.Same(typeof(Tcc.Themes.Integrity.ThemeIntegrityVerifier), verifier);
        Assert.True(IsExactPublicVerifier(verifier));
    }

    internal static bool IsExactPublicVerifier(Type type)
    {
        if (type.FullName != "Tcc.Themes.Integrity.ThemeIntegrityVerifier"
            || !type.IsPublic || !type.IsClass || !type.IsSealed || type.IsAbstract
            || type.IsNested || type.IsGenericType || type.BaseType != typeof(object)
            || type.IsDefined(typeof(CompilerGeneratedAttribute), false)
            || !type.GetInterfaces().SequenceEqual(new[] { typeof(Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifierV2) }))
        {
            return false;
        }

        ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        return constructors.Length == 1 && constructors[0].IsPublic && constructors[0].GetParameters().Length == 0
            && type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Length == 0
            && methods.Length == 1 && IsExactVerifyMethod(methods[0], type);
    }

    private static bool IsExactPhaseFiveNegotiator(Type type)
    {
        MethodInfo[] nonPrivateMethods = type.GetMethods(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsPrivate)
            .ToArray();
        if (!string.Equals(type.FullName, "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiator", StringComparison.Ordinal)
            || !type.IsNotPublic || type.IsVisible || !type.IsClass || !type.IsAbstract || !type.IsSealed
            || type.IsNested || type.IsGenericType || type.BaseType != typeof(object)
            || type.GetInterfaces().Length != 0 || type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)
            || type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length != 0
            || nonPrivateMethods.Length != 1)
        {
            return false;
        }

        MethodInfo method = nonPrivateMethods[0];
        ParameterInfo[] parameters = method.GetParameters();
        return method.Name == "Negotiate" && method.IsAssembly && method.IsStatic && !method.IsGenericMethod
            && method.ReturnType.FullName == "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiation"
            && parameters.Length == 1
            && parameters[0].ParameterType == typeof(Tcc.Presentation.Contracts.Theme.ThemeCompatibilityRequest);
    }

    private static bool IsExactPhaseFiveNegotiation(Type type)
    {
        Type? failureKind = type.Assembly.GetType(
            "Tcc.Themes.Compatibility.ThemeCompatibilityNegotiationFailureKind",
            throwOnError: false,
            ignoreCase: false);
        if (failureKind is null)
        {
            return false;
        }

        Dictionary<string, Type> expected = new(StringComparer.Ordinal)
        {
            ["CanContinue"] = typeof(bool),
            ["SelectedThemeApiVersion"] = typeof(string),
            ["SelectedUxContractVersion"] = typeof(string),
            ["Failures"] = typeof(System.Collections.Immutable.ImmutableArray<>).MakeGenericType(failureKind),
        };
        const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        PropertyInfo[] properties = type.GetProperties(declared);
        FieldInfo[] fields = type.GetFields(declared);
        PropertyInfo? equalityContract = properties.SingleOrDefault(property => property.Name == "EqualityContract");
        PropertyInfo[] dataProperties = properties.Where(property => property.Name != "EqualityContract").ToArray();

        return string.Equals(type.FullName, "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiation", StringComparison.Ordinal)
            && type.IsNotPublic && !type.IsVisible && type.IsClass && !type.IsAbstract && type.IsSealed
            && !type.IsNested && !type.IsGenericType && type.BaseType == typeof(object)
            && !type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)
            && type.GetInterfaces().SequenceEqual(new[] { typeof(IEquatable<>).MakeGenericType(type) })
            && properties.Length == expected.Count + 1
            && equalityContract is not null && equalityContract.PropertyType == typeof(Type)
            && equalityContract.IsDefined(typeof(CompilerGeneratedAttribute), false)
            && equalityContract.GetMethod is { IsPrivate: true, IsStatic: false }
            && equalityContract.SetMethod is null && equalityContract.GetIndexParameters().Length == 0
            && dataProperties.Length == expected.Count
            && dataProperties.All(property => property.GetMethod is { IsPublic: true, IsStatic: false }
                && property.SetMethod is { IsPublic: true, IsStatic: false }
                && property.SetMethod.ReturnParameter.GetRequiredCustomModifiers().SequenceEqual(new[] { typeof(IsExternalInit) })
                && property.GetIndexParameters().Length == 0
                && expected.TryGetValue(property.Name, out Type? propertyType)
                && property.PropertyType == propertyType)
            && fields.Length == expected.Count
            && expected.All(pair => fields.Count(field => field.Name == $"<{pair.Key}>k__BackingField"
                && field.FieldType == pair.Value && field.IsPrivate && field.IsInitOnly && !field.IsStatic
                && field.IsDefined(typeof(CompilerGeneratedAttribute), false)) == 1)
            && type.GetEvents(declared).Length == 0
            && HasExactPhaseFiveRecordInfrastructure(type, expected.Values.ToArray());
    }

    private static bool HasExactPhaseFiveRecordInfrastructure(Type type, Type[] dataTypes)
    {
        const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        List<(string Name, Type Result, Type[] Parameters, bool Public, bool Static)> expected =
        [
            ("get_EqualityContract", typeof(Type), [], false, false),
            ("ToString", typeof(string), [], true, false),
            ("PrintMembers", typeof(bool), [typeof(System.Text.StringBuilder)], false, false),
            ("op_Inequality", typeof(bool), [type, type], true, true),
            ("op_Equality", typeof(bool), [type, type], true, true),
            ("GetHashCode", typeof(int), [], true, false),
            ("Equals", typeof(bool), [typeof(object)], true, false),
            ("Equals", typeof(bool), [type], true, false),
            ("<Clone>$", type, [], true, false),
            ("Deconstruct", typeof(void), dataTypes.Select(dataType => dataType.MakeByRefType()).ToArray(), true, false),
        ];
        string[] names = ["CanContinue", "SelectedThemeApiVersion", "SelectedUxContractVersion", "Failures"];
        for (int index = 0; index < names.Length; index++)
        {
            expected.Add(($"get_{names[index]}", dataTypes[index], [], true, false));
            expected.Add(($"set_{names[index]}", typeof(void), [dataTypes[index]], true, false));
        }

        MethodInfo[] methods = type.GetMethods(declared);
        ConstructorInfo[] constructors = type.GetConstructors(declared);
        // CompilerGenerated is necessary only for these exact signatures; it is
        // never a blanket exemption for added fields, properties, or methods.
        return methods.Length == expected.Count && expected.All(shape => methods.Count(method =>
                method.Name == shape.Name && method.ReturnType == shape.Result
                && method.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(shape.Parameters)
                && method.IsPublic == shape.Public && (shape.Public || method.IsPrivate)
                && method.IsStatic == shape.Static && !method.IsGenericMethod && !method.IsAbstract
                && method.IsDefined(typeof(CompilerGeneratedAttribute), false)) == 1)
            && constructors.Length == 2
            && constructors.Count(constructor => constructor.IsPublic && !constructor.IsStatic
                && constructor.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(dataTypes)) == 1
            && constructors.Count(constructor => constructor.IsPrivate && !constructor.IsStatic
                && constructor.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(new[] { type })
                && constructor.IsDefined(typeof(CompilerGeneratedAttribute), false)) == 1;
    }

    private static bool IsExactPhaseFiveFailureKind(Type type) =>
        string.Equals(type.FullName, "Tcc.Themes.Compatibility.ThemeCompatibilityNegotiationFailureKind", StringComparison.Ordinal)
        && type.IsNotPublic
        && !type.IsVisible
        && type.IsEnum
        && Enum.GetUnderlyingType(type) == typeof(int)
        && Enum.GetNames(type).SequenceEqual(
            new[]
            {
                "InvalidVersionInput",
                "UnsatisfiableVersionRange",
                "UnsupportedManifestSchema",
                "ConflictingVersionDeclaration",
                "CoreVersionIncompatible",
                "ThemeApiNoCompatibleVersion",
                "UxContractNoCompatibleVersion",
            },
            StringComparer.Ordinal);

    private static List<Type> GetMemberDependencyTypes(Type type)
    {
        List<Type> dependencies = [];
        if (type.BaseType is not null)
        {
            AddTypeAndGenericArguments(dependencies, type.BaseType);
        }

        foreach (Type implementedInterface in type.GetInterfaces())
        {
            AddTypeAndGenericArguments(dependencies, implementedInterface);
        }

        foreach (FieldInfo field in type.GetFields(
                     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            AddTypeAndGenericArguments(dependencies, field.FieldType);
        }

        foreach (PropertyInfo property in type.GetProperties(
                     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            AddTypeAndGenericArguments(dependencies, property.PropertyType);
            foreach (ParameterInfo parameter in property.GetIndexParameters())
            {
                AddTypeAndGenericArguments(dependencies, parameter.ParameterType);
            }
        }

        foreach (EventInfo eventInfo in type.GetEvents(
                     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            if (eventInfo.EventHandlerType is not null)
            {
                AddTypeAndGenericArguments(dependencies, eventInfo.EventHandlerType);
            }
        }

        MethodBase[] methods =
        [
            .. type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            .. type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance),
        ];
        foreach (MethodBase method in methods)
        {
            MethodBody? methodBody = method.GetMethodBody();
            if (method.IsGenericMethod)
            {
                foreach (Type argument in method.GetGenericArguments())
                {
                    AddTypeAndGenericArguments(dependencies, argument);
                }
            }

            foreach (LocalVariableInfo local in methodBody?.LocalVariables ?? Enumerable.Empty<LocalVariableInfo>())
            {
                AddTypeAndGenericArguments(dependencies, local.LocalType);
            }

            foreach (ExceptionHandlingClause clause in methodBody?.ExceptionHandlingClauses
                         ?? Enumerable.Empty<ExceptionHandlingClause>())
            {
                if (clause.Flags == ExceptionHandlingClauseOptions.Clause
                    && clause.CatchType is Type catchType)
                {
                    AddTypeAndGenericArguments(dependencies, catchType);
                }
            }

            if (method is MethodInfo methodInfo)
            {
                AddTypeAndGenericArguments(dependencies, methodInfo.ReturnType);
            }

            foreach (ParameterInfo parameter in method.GetParameters())
            {
                AddTypeAndGenericArguments(dependencies, parameter.ParameterType);
            }

            foreach (MemberInfo referencedMember in GetReferencedMembers(method))
            {
                if (referencedMember.DeclaringType is not null)
                {
                    AddTypeAndGenericArguments(dependencies, referencedMember.DeclaringType);
                }

                if (referencedMember is MethodInfo referencedMethod)
                {
                    AddTypeAndGenericArguments(dependencies, referencedMethod.ReturnType);
                }

                // MethodBase covers constructors too. Return/parameter types alone
                // do not expose arguments such as Unsafe.SizeOf<FileInfo>().
                if (referencedMember is MethodBase referencedCallable)
                {
                    foreach (ParameterInfo parameter in referencedCallable.GetParameters())
                    {
                        AddTypeAndGenericArguments(dependencies, parameter.ParameterType);
                    }
                    if (referencedCallable.IsGenericMethod)
                    {
                        foreach (Type argument in referencedCallable.GetGenericArguments())
                        {
                            AddTypeAndGenericArguments(dependencies, argument);
                        }
                    }
                }
                else if (referencedMember is FieldInfo referencedField)
                {
                    AddTypeAndGenericArguments(dependencies, referencedField.FieldType);
                }
                else if (referencedMember is Type referencedType)
                {
                    AddTypeAndGenericArguments(dependencies, referencedType);
                }
            }
        }

        return dependencies;
    }

    private static void AddTypeAndGenericArguments(List<Type> dependencies, Type type)
    {
        if (type.HasElementType)
        {
            AddTypeAndGenericArguments(dependencies, type.GetElementType()!);
            return;
        }

        if (dependencies.Contains(type))
        {
            return;
        }

        dependencies.Add(type);
        if (type.IsGenericParameter)
        {
            foreach (Type constraint in type.GetGenericParameterConstraints())
            {
                AddTypeAndGenericArguments(dependencies, constraint);
            }
        }
        foreach (Type argument in type.GetGenericArguments())
        {
            AddTypeAndGenericArguments(dependencies, argument);
        }
    }

    private static List<MemberInfo> GetReferencedMembers(MethodBase method)
    {
        byte[]? il = method.GetMethodBody()?.GetILAsByteArray();
        List<MemberInfo> members = [];
        if (il is null)
        {
            return members;
        }

        int position = 0;
        while (position < il.Length)
        {
            OpCode opcode = il[position++] == 0xfe
                ? TwoByteOpCodes[il[position++]]
                : SingleByteOpCodes[il[position - 1]];
            int operandSize = GetOperandSize(opcode.OperandType, il, position);
            if (opcode.OperandType is OperandType.InlineField
                or OperandType.InlineMethod
                or OperandType.InlineTok
                or OperandType.InlineType)
            {
                int metadataToken = BitConverter.ToInt32(il, position);
                MemberInfo? member = method.Module.ResolveMember(
                    metadataToken,
                    method.DeclaringType?.GetGenericArguments(),
                    method.IsGenericMethod ? method.GetGenericArguments() : null);
                if (member is not null)
                {
                    members.Add(member);
                }
            }

            position += operandSize;
        }

        return members;
    }

    private static int GetOperandSize(OperandType operandType, byte[] il, int operandPosition) =>
        operandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineBrTarget or OperandType.InlineField or OperandType.InlineI
                or OperandType.InlineMethod or OperandType.InlineSig or OperandType.InlineString
                or OperandType.InlineTok or OperandType.InlineType or OperandType.ShortInlineR => 4,
            OperandType.InlineI8 or OperandType.InlineR => 8,
            OperandType.InlineSwitch => 4 + (BitConverter.ToInt32(il, operandPosition) * 4),
            _ => throw new InvalidOperationException($"Unsupported IL operand type '{operandType}'."),
        };

    private static OpCode[] BuildOpCodeTable(bool twoByte)
    {
        OpCode[] table = new OpCode[256];
        foreach (FieldInfo field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.GetValue(null) is not OpCode opcode || opcode.Size != (twoByte ? 2 : 1))
            {
                continue;
            }

            table[unchecked((byte)opcode.Value)] = opcode;
        }

        return table;
    }

    private static bool IsForbiddenPhaseFiveDependency(Type type, Assembly productionAssembly)
    {
        Type definition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
        if (ApprovedPhaseFiveDependencyTypes.Contains(definition))
        {
            return false;
        }

        return type.Assembly != productionAssembly || type.FullName is not
            ("Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiator"
            or "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiation"
            or "Tcc.Themes.Compatibility.ThemeCompatibilityNegotiationFailureKind");
    }

    private static bool IsExactVerifyMethod(MethodInfo method, Type owner)
    {
        ParameterInfo[] parameters = method.GetParameters();
        return method.DeclaringType == owner && method.Name == "VerifyAsync"
            && method.IsPublic && !method.IsStatic && !method.IsAbstract && !method.IsGenericMethod
            && method.ReturnType == typeof(ValueTask<Tcc.Presentation.Contracts.Theme.ThemeIntegrityVerificationResultV2>)
            && parameters.Select(parameter => parameter.ParameterType).SequenceEqual(new[]
            {
                typeof(Tcc.Presentation.Contracts.Theme.ThemeIntegrityVerificationRequestV2),
                typeof(Tcc.Presentation.Contracts.Theme.IThemePackageContentReader), typeof(CancellationToken),
            })
            && parameters[2].HasDefaultValue && parameters[2].DefaultValue is null;
    }

    internal static bool IsApprovedVerifierAsyncStateMachine(Type type)
    {
        Type? owner = type.DeclaringType;
        if (owner is null || !IsExactPublicVerifier(owner) || !type.IsNestedPrivate
            || !type.IsValueType || type.IsGenericType
            || !type.IsDefined(typeof(CompilerGeneratedAttribute), false)
            || !typeof(IAsyncStateMachine).IsAssignableFrom(type)) return false;
        MethodInfo? method = owner.GetMethod("VerifyAsync", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        return method is not null && IsExactVerifyMethod(method, owner)
            && method.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType == type
            && type.Name == $"<VerifyAsync>d__{GetStateMachineOrdinal(type.Name)}";
    }

    private static bool IsExactCandidateBStaticType(Type type, string fullName) =>
        string.Equals(type.FullName, fullName, StringComparison.Ordinal)
        && string.Equals(type.Assembly.GetName().Name, "Tcc.Themes", StringComparison.Ordinal)
        && type.IsNotPublic && !type.IsVisible && type.IsClass && type.IsAbstract && type.IsSealed
        && !type.IsNested && !type.IsGenericType && type.BaseType == typeof(object)
        && type.GetInterfaces().Length == 0
        && !type.IsDefined(typeof(CompilerGeneratedAttribute), false);

    private static bool IsExactCandidateBContentSnapshot(Type type)
    {
        ConstructorInfo[] constructors = type.GetConstructors(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        return string.Equals(type.FullName,
                "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityContentSnapshot",
                StringComparison.Ordinal)
            && type.IsNotPublic && !type.IsVisible && type.IsClass && type.IsSealed && !type.IsAbstract
            && string.Equals(type.Assembly.GetName().Name, "Tcc.Themes", StringComparison.Ordinal)
            && !type.IsNested && !type.IsGenericType && type.BaseType == typeof(object)
            && !type.IsDefined(typeof(CompilerGeneratedAttribute), false)
            && type.GetInterfaces().Length == 1
            && type.GetInterfaces()[0] == typeof(Tcc.Presentation.Contracts.Theme.IThemePackageContentReader)
            && constructors.Length == 1 && constructors[0].IsPrivate && !constructors[0].IsStatic;
    }

    private static bool IsApprovedCandidateBAsyncStateMachine(Type type)
        => HasNoMutableStaticFields(type) && HasApprovedCandidateBAsyncStateMachineIdentity(type);

    private static bool HasCandidateBGeneratedArtifactShape(Type type)
    {
        string? owner = type.DeclaringType?.FullName;
        return owner is "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityContentSnapshot"
                or "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityEvidenceBinder"
            && type.IsNestedPrivate && type.IsValueType && !type.IsGenericType
            && type.IsDefined(typeof(CompilerGeneratedAttribute), false)
            && typeof(IAsyncStateMachine).IsAssignableFrom(type);
    }

    private static bool HasApprovedCandidateBAsyncStateMachineIdentity(Type type)
    {
        Type? owner = type.DeclaringType;
        if (owner is null || !type.IsNestedPrivate || !type.IsValueType || type.IsGenericType
            || !type.IsDefined(typeof(CompilerGeneratedAttribute), false)
            || !typeof(IAsyncStateMachine).IsAssignableFrom(type))
        {
            return false;
        }

        string? methodName = owner.FullName switch
        {
            "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityContentSnapshot" => "CaptureAsync",
            "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityEvidenceBinder" => "VerifyAndBindAsync",
            _ => null,
        };
        if (methodName is null)
        {
            return false;
        }

        MethodInfo? method = owner.GetMethod(
            methodName,
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        return method is not null
            && GetAsyncStateMachineType(method) == type
            && string.Equals(type.Name, $"<{methodName}>d__{GetStateMachineOrdinal(type.Name)}", StringComparison.Ordinal);
    }

    private static bool HasNoMutableStaticFields(Type type) =>
        type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .All(field => field.IsLiteral);

    private static Type? GetAsyncStateMachineType(MethodInfo method)
    {
        CustomAttributeData? attribute = method.CustomAttributes.SingleOrDefault(value =>
            value.AttributeType == typeof(AsyncStateMachineAttribute));
        return attribute?.ConstructorArguments.Count == 1
            ? attribute.ConstructorArguments[0].Value as Type
            : null;
    }

    private static bool IsApprovedEvaluatorAsyncStateMachine(Type type)
    {
        Type? evaluator = type.DeclaringType;
        if (evaluator is null
            || !ApprovedAsyncEvaluatorTypes.Contains(evaluator.FullName, StringComparer.Ordinal)
            || !type.IsNestedPrivate
            || !type.IsValueType
            || type.IsGenericType
            || !type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)
            || !typeof(IAsyncStateMachine).IsAssignableFrom(type))
        {
            return false;
        }

        MethodInfo? method = evaluator.GetMethod(
            "EvaluateAsync",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        return method?.DeclaringType == evaluator
            && method.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType == type
            && string.Equals(type.Name, $"<{method.Name}>d__{GetStateMachineOrdinal(type.Name)}", StringComparison.Ordinal);
    }

    private static bool IsApprovedSealedBaselineCompilerArtifact(Type type)
    {
        if (!type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)
            || !ApprovedSealedBaselineCompilerArtifacts.Contains(type.FullName, StringComparer.Ordinal))
        {
            return false;
        }

        if (string.Equals(type.FullName, "<>z__ReadOnlyArray`1", StringComparison.Ordinal))
        {
            Type[] genericArguments = type.GetGenericArguments();
            FieldInfo? items = type.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            return type.DeclaringType is null
                && type.Namespace is null
                && type.IsNotPublic
                && type.IsClass
                && type.IsSealed
                && !type.IsAbstract
                && type.IsGenericTypeDefinition
                && genericArguments.Length == 1
                && type.BaseType == typeof(object)
                && items is not null
                && items.IsPrivate
                && items.IsInitOnly
                && items.FieldType.IsArray
                && items.FieldType.GetElementType() == genericArguments[0]
                && ImplementsOpenGeneric(type, typeof(IReadOnlyList<>))
                && ImplementsOpenGeneric(type, typeof(IList<>))
                && typeof(System.Collections.IList).IsAssignableFrom(type);
        }

        if (!type.IsNestedPrivate
            || !type.IsClass
            || !type.IsSealed
            || type.IsGenericType
            || type.BaseType != typeof(object)
            || type.GetInterfaces().Length != 0)
        {
            return false;
        }

        FieldInfo[] fields = type.GetFields(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        MethodInfo[] methods = type.GetMethods(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

        if (string.Equals(type.Name, "<>O", StringComparison.Ordinal))
        {
            return type.IsAbstract
                && fields.Length > 0
                && fields.All(field => field.IsStatic && field.Name.Contains(">__", StringComparison.Ordinal))
                && methods.Length == 0;
        }

        if (string.Equals(type.Name, "<>c", StringComparison.Ordinal))
        {
            return !type.IsAbstract
                && fields.Any(field => field.IsStatic && field.Name == "<>9" && field.FieldType == type)
                && methods.Length > 0
                && methods.All(method => method.Name.Contains(">b__", StringComparison.Ordinal));
        }

        if (type.Name.StartsWith("<>c__DisplayClass", StringComparison.Ordinal))
        {
            return !type.IsAbstract
                && fields.Any(field => !field.IsStatic)
                && methods.Length > 0
                && methods.All(method => method.Name.Contains(">b__", StringComparison.Ordinal));
        }

        return false;
    }

    private static bool IsExactPhaseFourDSignatureEvaluator(Type type) =>
        string.Equals(type.FullName, "Tcc.Themes.Integrity.ThemePackageSignatureEvaluator", StringComparison.Ordinal)
        && string.Equals(type.Namespace, "Tcc.Themes.Integrity", StringComparison.Ordinal)
        && type.DeclaringType is null
        && type.IsNotPublic
        && !type.IsVisible
        && type.IsClass
        && type.IsAbstract
        && type.IsSealed
        && !type.IsGenericType
        && type.BaseType == typeof(object)
        && type.GetInterfaces().Length == 0
        && !type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)
        && !IsGeneratedArtifactName(type.Name);

    private static bool IsExactPhaseFourDSignatureEvaluation(Type type)
    {
        Type expectedEquatable = typeof(IEquatable<>).MakeGenericType(type);
        Type[] interfaces = type.GetInterfaces();
        MethodInfo? clone = type.GetMethod(
            "<Clone>$",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        PropertyInfo? equalityContract = type.GetProperty(
            "EqualityContract",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        return string.Equals(type.FullName, "Tcc.Themes.Integrity.ThemePackageSignatureEvaluation", StringComparison.Ordinal)
            && string.Equals(type.Namespace, "Tcc.Themes.Integrity", StringComparison.Ordinal)
            && type.DeclaringType is null
            && type.IsNotPublic
            && !type.IsVisible
            && type.IsClass
            && !type.IsAbstract
            && type.IsSealed
            && !type.IsGenericType
            && type.BaseType == typeof(object)
            && interfaces.Length == 1
            && interfaces[0] == expectedEquatable
            && clone?.ReturnType == type
            && equalityContract?.PropertyType == typeof(Type)
            && !type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)
            && !IsGeneratedArtifactName(type.Name);
    }

    private static bool IsGeneratedArtifactName(string name) =>
        name.StartsWith('<')
        || name.Contains("DisplayClass", StringComparison.Ordinal)
        || name.Contains("Iterator", StringComparison.OrdinalIgnoreCase);

    private static int GetStateMachineOrdinal(string name)
    {
        int marker = name.LastIndexOf("d__", StringComparison.Ordinal);
        return marker >= 0 && int.TryParse(name[(marker + 3)..], out int ordinal) ? ordinal : -1;
    }

    private static bool ImplementsOpenGeneric(Type type, Type openGeneric) =>
        type.GetInterfaces().Any(contract =>
            contract.IsGenericType && contract.GetGenericTypeDefinition() == openGeneric);

    private static string NormalizeSymbol(string symbol) =>
        new(symbol.Where(char.IsLetterOrDigit).ToArray());

    private const string V1VerifierMethod =
        "public ValueTask<ThemeIntegrityVerificationResult> VerifyAsync(ThemeIntegrityVerificationRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();";

    private const string V2VerifierMethod =
        "public ValueTask<ThemeIntegrityVerificationResultV2> VerifyAsync(ThemeIntegrityVerificationRequestV2 request, IThemePackageContentReader contentReader, CancellationToken cancellationToken = default) => throw new NotSupportedException();";

    private const string PhaseFourDValidDeclarations =
        "internal static class ThemePackageSignatureEvaluator { } internal sealed record ThemePackageSignatureEvaluation;";

    private static string VerifierFixtureDeclaration(
        string typeDeclaration,
        string interfaceName,
        string methodDeclaration) =>
        $$"""
        using System.Runtime.CompilerServices;
        using Tcc.Presentation.Contracts.Theme;

        namespace Tcc.Themes.Integrity;

        {{typeDeclaration}} : {{interfaceName}}
        {
            {{methodDeclaration}}
        }
        """;

    private static void AssertTargetedViolation(
        Assembly fixture,
        string targetType,
        string requiredViolation)
    {
        string[] violations = GetCompiledSurfaceViolations(fixture);
        Assert.Contains(
            violations,
            violation => violation.Contains(targetType, StringComparison.Ordinal)
                && violation.Contains(requiredViolation, StringComparison.Ordinal));
    }

    private static Assembly BuildApprovedFixtureAssembly(
        string phaseFourDDeclarations,
        string requestBoundaryDeclaration = "internal static class ThemeIntegrityRequestBoundary { }",
        string? extraSource = null) =>
        BuildFixtureAssembly(
            markerAdditionalSource: null,
            otherSource:
            $$"""
            using Tcc.Presentation.Contracts.Theme;

            namespace Tcc.Themes.Integrity
            {
                {{requestBoundaryDeclaration}}
                internal sealed record ThemePackageInventoryEvaluation;
                internal static class ThemePackageInventoryEvaluator { }
                internal static class ThemeMetadataSchemaValidator { }
                internal sealed record ThemePackageMetadataEvaluation;
                internal static class ThemePackageMetadataEvaluator { }
                {{phaseFourDDeclarations}}
            }

            {{extraSource}}
            """);

    private static AssemblyBuilder BuildForgedClosureArtifact(string ownerFullName, bool nestedPrivate)
    {
        AssemblyName name = new($"Tcc.Themes.ForgedClosure.{Guid.NewGuid():N}");
        AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
        ModuleBuilder module = assembly.DefineDynamicModule(name.Name!);
        TypeBuilder owner = module.DefineType(
            ownerFullName,
            TypeAttributes.NotPublic | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Class);
        TypeAttributes visibility = nestedPrivate ? TypeAttributes.NestedPrivate : TypeAttributes.NestedPublic;
        TypeBuilder artifact = owner.DefineNestedType(
            "<>c",
            visibility | TypeAttributes.Sealed | TypeAttributes.Class);
        artifact.SetCustomAttribute(CompilerGeneratedAttributeBuilder());
        artifact.CreateType();
        owner.CreateType();
        return assembly;
    }

    private static AssemblyBuilder BuildForgedStandaloneGeneratedArtifact()
    {
        AssemblyName name = new($"Tcc.Themes.ForgedStandalone.{Guid.NewGuid():N}");
        AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
        ModuleBuilder module = assembly.DefineDynamicModule(name.Name!);
        TypeBuilder artifact = module.DefineType(
            "<>z__ReadOnlyArray`1",
            TypeAttributes.NotPublic | TypeAttributes.Sealed | TypeAttributes.Class);
        artifact.SetCustomAttribute(CompilerGeneratedAttributeBuilder());
        artifact.CreateType();
        return assembly;
    }

    private static AssemblyBuilder BuildForgedWrongAttributeTargetArtifact()
    {
        AssemblyName name = new($"Tcc.Themes.ForgedAsync.{Guid.NewGuid():N}");
        AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
        ModuleBuilder module = assembly.DefineDynamicModule(name.Name!);
        TypeBuilder owner = module.DefineType(
            "Tcc.Themes.Integrity.ThemePackageInventoryEvaluator",
            TypeAttributes.NotPublic | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Class);
        TypeBuilder expected = DefineForgedStateMachine(owner, "<EvaluateAsync>d__0");
        TypeBuilder wrong = DefineForgedStateMachine(owner, "<OtherAsync>d__1");
        MethodBuilder method = owner.DefineMethod(
            "EvaluateAsync",
            MethodAttributes.Assembly | MethodAttributes.Static | MethodAttributes.HideBySig,
            typeof(void),
            Type.EmptyTypes);
        method.GetILGenerator().Emit(OpCodes.Ret);
        method.SetCustomAttribute(
            new CustomAttributeBuilder(
                typeof(AsyncStateMachineAttribute).GetConstructor([typeof(Type)])!,
                [wrong]));
        expected.CreateType();
        wrong.CreateType();
        owner.CreateType();
        return assembly;
    }

    private static TypeBuilder DefineForgedStateMachine(TypeBuilder owner, string name)
    {
        TypeBuilder stateMachine = owner.DefineNestedType(
            name,
            TypeAttributes.NestedPrivate | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
            typeof(ValueType));
        stateMachine.SetCustomAttribute(CompilerGeneratedAttributeBuilder());
        stateMachine.AddInterfaceImplementation(typeof(IAsyncStateMachine));
        DefineStateMachineMethod(stateMachine, nameof(IAsyncStateMachine.MoveNext), Type.EmptyTypes);
        DefineStateMachineMethod(
            stateMachine,
            nameof(IAsyncStateMachine.SetStateMachine),
            [typeof(IAsyncStateMachine)]);
        return stateMachine;
    }

    private static AssemblyBuilder BuildCandidateBAsyncStateMachineWithMutableStaticState()
    {
        AssemblyName name = new($"Tcc.Themes.CandidateBAsyncStatic.{Guid.NewGuid():N}");
        AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
        ModuleBuilder module = assembly.DefineDynamicModule(name.Name!);
        TypeBuilder owner = module.DefineType(
            "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityEvidenceBinder",
            TypeAttributes.NotPublic | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Class);
        TypeBuilder stateMachine = owner.DefineNestedType(
            "<VerifyAndBindAsync>d__7",
            TypeAttributes.NestedPrivate | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit,
            typeof(ValueType));
        stateMachine.SetCustomAttribute(CompilerGeneratedAttributeBuilder());
        stateMachine.AddInterfaceImplementation(typeof(IAsyncStateMachine));
        FieldBuilder hiddenCache = stateMachine.DefineField(
            "HiddenCache", typeof(object), FieldAttributes.Private | FieldAttributes.Static);

        MethodInfo moveNextContract = typeof(IAsyncStateMachine).GetMethod(nameof(IAsyncStateMachine.MoveNext))!;
        MethodBuilder moveNext = stateMachine.DefineMethod(
            nameof(IAsyncStateMachine.MoveNext),
            MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.Virtual
                | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
            typeof(void), Type.EmptyTypes);
        ILGenerator moveNextIl = moveNext.GetILGenerator();
        moveNextIl.Emit(OpCodes.Newobj, typeof(object).GetConstructor(Type.EmptyTypes)!);
        moveNextIl.Emit(OpCodes.Stsfld, hiddenCache);
        moveNextIl.Emit(OpCodes.Ret);
        stateMachine.DefineMethodOverride(moveNext, moveNextContract);
        DefineStateMachineMethod(
            stateMachine,
            nameof(IAsyncStateMachine.SetStateMachine),
            [typeof(IAsyncStateMachine)]);

        MethodBuilder method = owner.DefineMethod(
            "VerifyAndBindAsync",
            MethodAttributes.Assembly | MethodAttributes.Static | MethodAttributes.HideBySig,
            typeof(ValueTask<Tcc.Themes.Compatibility.V2.ThemeCompatibilityContextV2>),
            [
                typeof(Tcc.Presentation.Contracts.Theme.ThemeIntegrityVerificationRequestV2),
                typeof(Tcc.Presentation.Contracts.Theme.IThemePackageContentReader),
                typeof(CancellationToken),
            ]);
        LocalBuilder result = method.GetILGenerator().DeclareLocal(
            typeof(ValueTask<Tcc.Themes.Compatibility.V2.ThemeCompatibilityContextV2>));
        ILGenerator methodIl = method.GetILGenerator();
        methodIl.Emit(OpCodes.Ldloca_S, result);
        methodIl.Emit(OpCodes.Initobj, typeof(ValueTask<Tcc.Themes.Compatibility.V2.ThemeCompatibilityContextV2>));
        methodIl.Emit(OpCodes.Ldloc_0);
        methodIl.Emit(OpCodes.Ret);
        method.SetCustomAttribute(new CustomAttributeBuilder(
            typeof(AsyncStateMachineAttribute).GetConstructor([typeof(Type)])!, [stateMachine]));

        stateMachine.CreateType();
        owner.CreateType();
        return assembly;
    }

    private static void DefineStateMachineMethod(
        TypeBuilder stateMachine,
        string methodName,
        Type[] parameterTypes)
    {
        MethodInfo contract = typeof(IAsyncStateMachine).GetMethod(methodName)!;
        MethodBuilder method = stateMachine.DefineMethod(
            methodName,
            MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.Virtual
                | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
            typeof(void),
            parameterTypes);
        method.GetILGenerator().Emit(OpCodes.Ret);
        stateMachine.DefineMethodOverride(method, contract);
    }

    private static CustomAttributeBuilder CompilerGeneratedAttributeBuilder() =>
        new(typeof(CompilerGeneratedAttribute).GetConstructor(Type.EmptyTypes)!, []);

    internal static Assembly BuildInstrumentedResolverFixtureAssembly()
    {
        string negotiationProbe =
            "namespace Tcc.Themes.Compatibility { "
            + "internal static class ThemeCompatibilityVersionNegotiator { "
            + "internal static int Calls; "
            + "internal static ThemeCompatibilityVersionNegotiation Negotiate(global::Tcc.Presentation.Contracts.Theme.ThemeCompatibilityRequest request) { "
            + "Calls++; return new ThemeCompatibilityVersionNegotiation(true, \"1.0.0\", \"1.1.0\", global::System.Collections.Immutable.ImmutableArray<ThemeCompatibilityNegotiationFailureKind>.Empty); } } "
            + "internal sealed record ThemeCompatibilityVersionNegotiation(bool CanContinue, string? SelectedThemeApiVersion, string? SelectedUxContractVersion, global::System.Collections.Immutable.ImmutableArray<ThemeCompatibilityNegotiationFailureKind> Failures); "
            + "internal enum ThemeCompatibilityNegotiationFailureKind { InvalidVersionInput, UnsatisfiableVersionRange, UnsupportedManifestSchema, ConflictingVersionDeclaration, CoreVersionIncompatible, ThemeApiNoCompatibleVersion, UxContractNoCompatibleVersion } }";
        return BuildFixtureAssembly(
            markerAdditionalSource: null,
            otherSource: negotiationProbe,
            includeCandidateAContracts: true,
            assemblyName: $"Tcc.Themes.ResolverExecution.{Guid.NewGuid():N}",
            includeCandidateBSurface: false,
            additionalSource: File.ReadAllText(Path.Combine(
                RepositoryPaths.Root,
                "src", "Tcc.Themes", "Compatibility", "V2", "ThemeCompatibilityResolverV2.cs")));
    }

    internal static Assembly BuildFixtureAssembly(string? markerAdditionalSource, string? otherSource, string? externalSource = null,
        bool includeCandidateAContracts = true, string assemblyName = "Tcc.Themes",
        bool includeCandidateBSurface = true, string? additionalSource = null)
    {
        string fixtureRoot = Path.Combine(
            Path.GetTempPath(),
            $"tcc-phase3-boundary-{Guid.NewGuid():N}");

        try
        {
            Directory.CreateDirectory(fixtureRoot);
            string contractsAssembly = typeof(Tcc.Presentation.Contracts.Theme.IThemePackage).Assembly.Location;
            XDocument project = new(
                new XElement(
                    "Project",
                    new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                    new XElement(
                        "PropertyGroup",
                        new XElement("TargetFramework", "net10.0-windows"),
                        new XElement("AssemblyName", assemblyName),
                        new XElement("RootNamespace", "Tcc.Themes"),
                        new XElement("Nullable", "enable"),
                        new XElement("ImplicitUsings", "enable"),
                        new XElement("NuGetAudit", "true")),
                    new XElement(
                        "ItemGroup",
                        new XElement(
                            "Reference",
                            new XAttribute("Include", "Tcc.Presentation.Contracts"),
                            new XElement("HintPath", contractsAssembly),
                            new XElement("Private", "true")))));
            if (externalSource is not null)
            {
                string externalRoot = Path.Combine(fixtureRoot, "External");
                Directory.CreateDirectory(externalRoot);
                string externalName = $"BoundaryComponent{Guid.NewGuid():N}";
                new XDocument(new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                    new XElement("PropertyGroup",
                        new XElement("TargetFramework", "net10.0-windows"),
                        new XElement("AssemblyName", externalName),
                        new XElement("NuGetAudit", "true"))))
                    .Save(Path.Combine(externalRoot, "Fixture.csproj"));
                File.WriteAllText(Path.Combine(externalRoot, "Component.cs"), externalSource);
                BuildFixtureProject(externalRoot);
                string externalPath = Directory.EnumerateFiles(
                    Path.Combine(externalRoot, "bin"), $"{externalName}.dll", SearchOption.AllDirectories).Single();
                project.Root!.Add(new XElement("ItemGroup",
                    new XElement("Compile", new XAttribute("Remove", "External/**")),
                    new XElement("Reference", new XAttribute("Include", externalName),
                        new XElement("HintPath", externalPath))));
                // Load from bytes so fixture cleanup holds no file locks. The
                // unique assembly belongs only to this compiled negative fixture.
                using MemoryStream externalBytes = new(File.ReadAllBytes(externalPath));
                AssemblyLoadContext.Default.LoadFromStream(externalBytes);
            }
            project.Save(Path.Combine(fixtureRoot, "Fixture.csproj"));

            File.WriteAllText(
                Path.Combine(fixtureRoot, "AssemblyMarker.cs"),
                $$"""
                namespace Tcc.Themes
                {
                    public static class AssemblyMarker { }
                    {{markerAdditionalSource}}
                }
                """);
            File.WriteAllText(
                Path.Combine(fixtureRoot, "ThemeManifestValidator.cs"),
                """
                using Tcc.Presentation.Contracts.Theme;

                namespace Tcc.Themes.Manifests;

                public sealed class ThemeManifestValidator : IThemeManifestValidator
                {
                    public ThemeManifestValidationResult Validate(
                        ThemeManifest manifest,
                        ThemeManifestValidationContext context) => new(true, [], []);
                }
                """);
            foreach (string file in new[]
            {
                "BuiltInThemePresentationSource.cs",
                "BuiltInThemePresentationSnapshot.cs",
            })
            {
                File.WriteAllText(
                    Path.Combine(fixtureRoot, file),
                    File.ReadAllText(Path.Combine(
                        RepositoryPaths.Root,
                        "src",
                        "Tcc.Themes",
                        "Fallback",
                        file)));
            }
            if (otherSource is not null)
            {
                File.WriteAllText(Path.Combine(fixtureRoot, "OtherProductionSource.cs"), otherSource);
            }
            if (additionalSource is not null)
            {
                File.WriteAllText(Path.Combine(fixtureRoot, "AdditionalProductionSource.cs"), additionalSource);
            }

            // Preserve the original Phase5A positive assertions: their fixtures now
            // include the independently guarded additive contract family as well.
            // Deliberate V2 shape attacks supply their own declarations instead.
            if (includeCandidateAContracts)
            {
                foreach (string file in new[] { "ThemeCompatibilityApiV2.cs", "ThemeCompatibilityEvidenceV2.cs" })
                {
                    File.WriteAllText(Path.Combine(fixtureRoot, file), File.ReadAllText(Path.Combine(
                        RepositoryPaths.Root, "src", "Tcc.Themes", "Compatibility", "V2", file)));
                }

                if (includeCandidateBSurface)
                {
                    File.WriteAllText(
                        Path.Combine(fixtureRoot, "CandidateBSurface.cs"),
                        "namespace Tcc.Themes.Compatibility.Binding { "
                        + "internal static class ThemeCompatibilityEvidenceBinder { } "
                        + "internal static class ThemeCompatibilityManifestMaterializer { } "
                        + "internal sealed class ThemeCompatibilityContentSnapshot : global::Tcc.Presentation.Contracts.Theme.IThemePackageContentReader { "
                        + "private ThemeCompatibilityContentSnapshot() { } "
                        + "public global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.IReadOnlyList<global::Tcc.Presentation.Contracts.Theme.ThemePackageContentEntryV1>> EnumerateEntriesAsync(global::Tcc.Presentation.Contracts.Theme.ThemePackageRef packageRef, global::System.Threading.CancellationToken cancellationToken = default) => default; "
                        + "public global::System.Threading.Tasks.ValueTask<global::System.ReadOnlyMemory<byte>> ReadContentAsync(global::Tcc.Presentation.Contracts.Theme.ThemePackageRef packageRef, string canonicalPath, global::System.Threading.CancellationToken cancellationToken = default) => default; } } "
                        + "namespace Tcc.Themes.Compatibility.V2 { public sealed class ThemeCompatibilityResolverV2 : IThemeCompatibilityResolverV2 { "
                        + "public ThemeCompatibilityResolverV2() { } public global::Tcc.Presentation.Contracts.Theme.ThemeCompatibilityResultV2 Resolve(ThemeCompatibilityRequestV2 request) => null!; } }");
                }
            }

            BuildFixtureProject(fixtureRoot);

            string assemblyPath = Directory
                .EnumerateFiles(Path.Combine(fixtureRoot, "bin"), $"{assemblyName}.dll", SearchOption.AllDirectories)
                .Single();
            return Assembly.Load(File.ReadAllBytes(assemblyPath));
        }
        finally
        {
            if (Directory.Exists(fixtureRoot))
            {
                Directory.Delete(fixtureRoot, recursive: true);
            }
        }
    }

    private static void BuildFixtureProject(string fixtureRoot)
    {
        ProcessStartInfo startInfo = new("dotnet")
        {
            WorkingDirectory = fixtureRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add("build");
        startInfo.ArgumentList.Add("Fixture.csproj");
        startInfo.ArgumentList.Add("--configuration");
        startInfo.ArgumentList.Add("Release");
        startInfo.ArgumentList.Add("-p:Platform=x64");
        startInfo.ArgumentList.Add("-m:1");
        startInfo.ArgumentList.Add("--nologo");

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start fixture build.");
        string standardOutput = process.StandardOutput.ReadToEnd();
        string standardError = process.StandardError.ReadToEnd();
        process.WaitForExit();
        Assert.True(
            process.ExitCode == 0,
            $"Fixture build failed.{Environment.NewLine}{standardOutput}{Environment.NewLine}{standardError}");
    }
}
