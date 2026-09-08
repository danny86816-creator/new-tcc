using System.Diagnostics;
using System.Reflection;
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
            || !string.Equals(
                evaluator.FullName,
                "Tcc.Themes.Integrity.ThemePackageInventoryEvaluator",
                StringComparison.Ordinal)
            || !type.IsNestedPrivate
            || !type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)
            || !typeof(IAsyncStateMachine).IsAssignableFrom(type))
        {
            return false;
        }

        MethodInfo? method = evaluator.GetMethod(
            "EvaluateAsync",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        return method?.DeclaringType == evaluator
            && method.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType == type;
    }

    private static bool IsApprovedSealedBaselineCompilerArtifact(Type type) =>
        type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false)
        && ApprovedSealedBaselineCompilerArtifacts.Contains(type.FullName, StringComparer.Ordinal);

    private static string NormalizeSymbol(string symbol) =>
        new(symbol.Where(char.IsLetterOrDigit).ToArray());

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
                        new XElement("ImplicitUsings", "enable")),
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
