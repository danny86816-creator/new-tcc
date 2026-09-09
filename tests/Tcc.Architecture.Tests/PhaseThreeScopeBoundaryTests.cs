using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Tcc.Architecture.Tests;

public sealed class PhaseThreeScopeBoundaryTests
{
    private static readonly string[] ApprovedTopLevelTypes =
    [
        "Tcc.Themes.AssemblyMarker",
        "Tcc.Themes.Integrity.ThemeIntegrityRequestBoundary",
        "Tcc.Themes.Integrity.ThemePackageInventoryEvaluation",
        "Tcc.Themes.Integrity.ThemePackageInventoryEvaluator",
        "Tcc.Themes.Integrity.ThemeMetadataSchemaValidator",
        "Tcc.Themes.Integrity.ThemePackageMetadataEvaluation",
        "Tcc.Themes.Integrity.ThemePackageMetadataEvaluator",
        "Tcc.Themes.Integrity.ThemePackageSignatureEvaluator",
        "Tcc.Themes.Integrity.ThemePackageSignatureEvaluation",
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

    [Fact]
    public void ActualCompiledThemeAssemblyMatchesExactPhaseThreeTypeSurface()
    {
        string[] violations = GetCompiledSurfaceViolations(typeof(Tcc.Themes.AssemblyMarker).Assembly);

        Assert.Empty(violations);
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

        foreach (Type unauthorizedNestedType in allTypes.Where(type =>
                     type.IsNested
                     && !string.Equals(type.FullName, ApprovedNestedType, StringComparison.Ordinal)
                     && !IsApprovedSealedBaselineCompilerArtifact(type)
                     && !IsApprovedEvaluatorAsyncStateMachine(type)))
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

        foreach (Type productionType in allTypes)
        {
            foreach (string forbiddenInterface in productionType.GetInterfaces()
                         .Select(contract => contract.FullName ?? contract.Name)
                         .Where(ForbiddenVerifierInterfaces.Contains))
            {
                violations.Add(
                    $"Production type '{productionType.FullName ?? productionType.Name}' must not implement Phase 4D-forbidden verifier interface '{forbiddenInterface}'.");
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

    internal static Assembly BuildFixtureAssembly(string? markerAdditionalSource, string? otherSource)
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
                        new XElement("AssemblyName", "Tcc.Themes"),
                        new XElement("RootNamespace", "Tcc.Themes"),
                        new XElement("Nullable", "enable"),
                        new XElement("ImplicitUsings", "enable"),
                        new XElement("NuGetAudit", "false")),
                    new XElement(
                        "ItemGroup",
                        new XElement(
                            "Reference",
                            new XAttribute("Include", "Tcc.Presentation.Contracts"),
                            new XElement("HintPath", contractsAssembly),
                            new XElement("Private", "true")))));
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
            if (otherSource is not null)
            {
                File.WriteAllText(Path.Combine(fixtureRoot, "OtherProductionSource.cs"), otherSource);
            }

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

            string assemblyPath = Directory
                .EnumerateFiles(Path.Combine(fixtureRoot, "bin"), "Tcc.Themes.dll", SearchOption.AllDirectories)
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
}
