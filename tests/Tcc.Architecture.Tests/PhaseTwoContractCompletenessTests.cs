using System.Reflection;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Manifests;

namespace Tcc.Architecture.Tests;

public sealed class PhaseTwoContractCompletenessTests
{
    private static readonly string[] RequiredSchemaFiles =
    [
        "ThemeManifest.schema.json",
        "ThemeIntegrity.schema.json",
        "ThemeCompatibility.schema.json",
        "ThemeRollback.schema.json",
        "ThemeAssets.schema.json",
        "ThemeTokens.schema.json",
        "ThemeLayoutAdapter.schema.json",
        "ThemeComponentAdapter.schema.json",
        "ThemeCopyResources.schema.json",
        "ThemeCriticalCopyRules.schema.json",
        "ThemeFocusStyles.schema.json",
        "ThemeStatePresentation.schema.json",
        "ThemeMotionProfile.schema.json",
        "ThemeSoundPack.schema.json",
        "ThemePersonalizationSafeRanges.schema.json",
        "ThemeRuntimeState.schema.json",
        "UxSurfaceContract.schema.json",
        "FunctionalZoneContract.schema.json",
        "GlobalStatePresentation.schema.json",
        "ThemeDiagnosticsEvent.schema.json",
        "CopyFallbackResult.schema.json",
    ];

    private static readonly string[] RequiredInterfaces =
    [
        "IThemePackage",
        "IThemeRuntime",
        "IThemeManifestValidator",
        "IThemeIntegrityVerifier",
        "IThemeCompatibilityResolver",
        "IThemeCapabilityGate",
        "IThemeAssetLoader",
        "IThemeAssetCache",
        "IThemePreviewSandbox",
        "ILiveThemeSwitchCoordinator",
        "IThemePersonalizationEngine",
        "IThemeMotionManager",
        "IThemeAudioRouter",
        "IThemeAccessibilityValidator",
        "IThemeRollbackManager",
        "IThemeRecoveryHooks",
        "IThemeDiagnosticsEmitter",
    ];

    [Fact]
    public void AllRequiredSectionThirtyThreeSchemaFilesExistExactlyOnce()
    {
        string schemaDirectory = Path.Combine(RepositoryPaths.ThemeContracts, "schemas");
        string[] actual = Directory
            .EnumerateFiles(schemaDirectory, "*.schema.json", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Order(StringComparer.Ordinal)
            .ToArray()!;

        Assert.Equal(RequiredSchemaFiles.Order(StringComparer.Ordinal), actual);
    }

    [Fact]
    public void AllRequiredSectionThirtyThreeInterfacesArePublicAndOwnedByPresentationContracts()
    {
        Assembly contractsAssembly = typeof(IThemePackage).Assembly;
        Type[] interfaces = contractsAssembly
            .GetExportedTypes()
            .Where(type => type.IsInterface)
            .ToArray();

        Assert.Equal(
            RequiredInterfaces.Order(StringComparer.Ordinal),
            interfaces.Select(type => type.Name).Order(StringComparer.Ordinal));

        Assert.All(
            interfaces,
            contractInterface =>
            {
                Assert.Equal("Tcc.Presentation.Contracts.Theme", contractInterface.Namespace);
                Assert.Equal("Tcc.Presentation.Contracts", contractInterface.Assembly.GetName().Name);
                Assert.True(contractInterface.IsPublic);
            });
    }

    [Fact]
    public void RequiredInterfacesHaveOnlyPhaseApprovedProductionImplementations()
    {
        Assembly[] productionAssemblies =
        [
            typeof(Tcc.Presentation.Contracts.AssemblyMarker).Assembly,
            typeof(Tcc.Themes.AssemblyMarker).Assembly,
            typeof(Tcc.Features.Themes.AssemblyMarker).Assembly,
            typeof(Tcc.Windows.AssemblyMarker).Assembly,
        ];

        Assert.Empty(FindUnauthorizedImplementationViolations(productionAssemblies));
    }

    [Fact]
    public void UnauthorizedPhaseTwoInterfaceImplementationFailsTheImplementationGuard()
    {
        Assembly fixture = PhaseThreeScopeBoundaryTests.BuildFixtureAssembly(
            markerAdditionalSource:
                """
                public sealed class ThemeIntegrityVerifier :
                    global::Tcc.Presentation.Contracts.Theme.IThemeIntegrityVerifier
                {
                    public global::System.Threading.Tasks.ValueTask<global::Tcc.Presentation.Contracts.Theme.ThemeIntegrityVerificationResult> VerifyAsync(
                        global::Tcc.Presentation.Contracts.Theme.ThemeIntegrityVerificationRequest request,
                        global::System.Threading.CancellationToken cancellationToken = default) =>
                        global::System.Threading.Tasks.ValueTask.FromResult(
                            new global::Tcc.Presentation.Contracts.Theme.ThemeIntegrityVerificationResult(
                                false,
                                false,
                                string.Empty,
                                string.Empty,
                                []));
                }
                """,
            otherSource: null);

        string[] violations = FindUnauthorizedImplementationViolations([fixture]);

        Assert.Contains(
            violations,
            violation => violation.Contains("IThemeIntegrityVerifier", StringComparison.Ordinal)
                && violation.Contains("ThemeIntegrityVerifier", StringComparison.Ordinal));
    }

    [Fact]
    public void RequiredGeneratedUxContractSurfacesExist()
    {
        string[] requiredFiles =
        [
            "ux-contract.v1.1.json",
            "accessibility-contract.v1.json",
            "motion-contract.v1.json",
            "audio-contract.v1.json",
            "theme-manifest.schema.json",
        ];

        Assert.All(
            requiredFiles,
            file => Assert.True(File.Exists(Path.Combine(RepositoryPaths.ThemeContracts, file)), file));

        Assert.Equal(71, Directory.EnumerateFiles(Path.Combine(RepositoryPaths.ThemeContracts, "page-contracts"), "*.json").Count());
        Assert.Equal(41, Directory.EnumerateFiles(Path.Combine(RepositoryPaths.ThemeContracts, "module-contracts"), "*.json").Count());
        Assert.Equal(17, Directory.EnumerateFiles(Path.Combine(RepositoryPaths.ThemeContracts, "zone-contracts"), "*.json").Count());
        Assert.Equal(19, Directory.EnumerateFiles(Path.Combine(RepositoryPaths.ThemeContracts, "state-contracts"), "*.json").Count());
    }

    private static string[] FindUnauthorizedImplementationViolations(IEnumerable<Assembly> productionAssemblies)
    {
        Type[] productionTypes = productionAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .ToArray();
        Type[] requiredInterfaces = typeof(IThemePackage).Assembly
            .GetExportedTypes()
            .Where(type => type.IsInterface && RequiredInterfaces.Contains(type.Name, StringComparer.Ordinal))
            .ToArray();
        List<string> violations = [];

        foreach (Type contractInterface in requiredInterfaces)
        {
            Type[] implementations = productionTypes
                .Where(type => type.IsClass && !type.IsAbstract && contractInterface.IsAssignableFrom(type))
                .ToArray();

            foreach (Type implementation in implementations)
            {
                bool isApprovedManifestValidator = contractInterface == typeof(IThemeManifestValidator)
                    && implementation == typeof(ThemeManifestValidator);
                if (!isApprovedManifestValidator)
                {
                    violations.Add(
                        $"{implementation.FullName} is an unauthorized production implementation of {contractInterface.FullName}.");
                }
            }
        }

        return violations.Order(StringComparer.Ordinal).ToArray();
    }
}
