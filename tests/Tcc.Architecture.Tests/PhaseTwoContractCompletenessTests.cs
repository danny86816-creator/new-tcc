using System.Reflection;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Manifests;

namespace Tcc.Architecture.Tests;

public sealed class PhaseTwoContractCompletenessTests
{
    private static readonly string[] RequiredPhaseTwoSchemaFiles =
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

    private static readonly string[] ApprovedContractAmendmentSchemaFiles =
    [
        "ThemeIntegrity.v2.schema.json",
        "ThemeSignatureEnvelope.v1.schema.json",
    ];

    private static readonly string[] RequiredPhaseTwoInterfaces =
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

    private static readonly string[] ApprovedContractAmendmentInterfaces =
    [
        "IThemeIntegrityVerifierV2",
        "IThemePackageContentReader",
    ];

    [Fact]
    public void SealedPhaseTwoAndApprovedAmendmentSchemaFilesExistExactlyOnce()
    {
        string schemaDirectory = Path.Combine(RepositoryPaths.ThemeContracts, "schemas");
        string[] actual = Directory
            .EnumerateFiles(schemaDirectory, "*.schema.json", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Order(StringComparer.Ordinal)
            .ToArray()!;

        string[] expected = RequiredPhaseTwoSchemaFiles
            .Concat(ApprovedContractAmendmentSchemaFiles)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SealedPhaseTwoAndApprovedAmendmentInterfacesArePublicAndOwnedByPresentationContracts()
    {
        Assembly contractsAssembly = typeof(IThemePackage).Assembly;
        Type[] interfaces = contractsAssembly
            .GetExportedTypes()
            .Where(type => type.IsInterface)
            .ToArray();

        string[] expected = RequiredPhaseTwoInterfaces
            .Concat(ApprovedContractAmendmentInterfaces)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expected, interfaces.Select(type => type.Name).Order(StringComparer.Ordinal));

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
        PhaseThreeScopeBoundaryTests.AssertExactVerifierImplementations(productionAssemblies.SelectMany(assembly => assembly.GetTypes()));
        AssertExactPackageReaderImplementation(productionAssemblies.SelectMany(assembly => assembly.GetTypes()));
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

    [Theory]
    [InlineData("internal abstract class Attack", "")]
    [InlineData("internal sealed class Attack<T>", "")]
    [InlineData("internal struct Attack", "")]
    [InlineData("internal sealed class Outer { internal sealed class Attack", " }")]
    [InlineData("public sealed class ThemeCompatibilityContentSnapshot", "")]
    public void UnauthorizedPackageReaderShapesFailTheImplementationGuard(string declaration, string suffix)
    {
        string source =
            "namespace Tcc.Themes.Compatibility.Binding { " + declaration
            + " : global::Tcc.Presentation.Contracts.Theme.IThemePackageContentReader { "
            + "public global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.IReadOnlyList<global::Tcc.Presentation.Contracts.Theme.ThemePackageContentEntryV1>> EnumerateEntriesAsync(global::Tcc.Presentation.Contracts.Theme.ThemePackageRef packageRef, global::System.Threading.CancellationToken cancellationToken = default) => throw new global::System.NotSupportedException(); "
            + "public global::System.Threading.Tasks.ValueTask<global::System.ReadOnlyMemory<byte>> ReadContentAsync(global::Tcc.Presentation.Contracts.Theme.ThemePackageRef packageRef, string canonicalPath, global::System.Threading.CancellationToken cancellationToken = default) => throw new global::System.NotSupportedException(); }"
            + suffix + " }";
        Assembly fixture = PhaseThreeScopeBoundaryTests.BuildFixtureAssembly(
            null, source, includeCandidateAContracts: false);

        Assert.Contains(FindUnauthorizedImplementationViolations([fixture]), violation =>
            violation.Contains("IThemePackageContentReader", StringComparison.Ordinal));
    }

    [Fact]
    public void WrongNameNamespaceAndSecondPackageReadersFailTheImplementationGuard()
    {
        const string methods = "public global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.IReadOnlyList<global::Tcc.Presentation.Contracts.Theme.ThemePackageContentEntryV1>> EnumerateEntriesAsync(global::Tcc.Presentation.Contracts.Theme.ThemePackageRef packageRef, global::System.Threading.CancellationToken cancellationToken = default) => default; public global::System.Threading.Tasks.ValueTask<global::System.ReadOnlyMemory<byte>> ReadContentAsync(global::Tcc.Presentation.Contracts.Theme.ThemePackageRef packageRef, string canonicalPath, global::System.Threading.CancellationToken cancellationToken = default) => default;";
        Assembly wrongName = PhaseThreeScopeBoundaryTests.BuildFixtureAssembly(
            null,
            "namespace Tcc.Themes.Compatibility.Binding { internal sealed class AlternateReader : global::Tcc.Presentation.Contracts.Theme.IThemePackageContentReader { " + methods + " } }",
            includeCandidateAContracts: false);
        Assembly wrongNamespace = PhaseThreeScopeBoundaryTests.BuildFixtureAssembly(
            null,
            "namespace Tcc.Themes.Compatibility.Attack { internal sealed class ThemeCompatibilityContentSnapshot : global::Tcc.Presentation.Contracts.Theme.IThemePackageContentReader { " + methods + " } }",
            includeCandidateAContracts: false);
        Assembly second = PhaseThreeScopeBoundaryTests.BuildFixtureAssembly(
            null,
            "namespace Tcc.Themes.Compatibility.Binding { internal sealed class AlternateReader : global::Tcc.Presentation.Contracts.Theme.IThemePackageContentReader { " + methods + " } }");

        foreach (Assembly fixture in new[] { wrongName, wrongNamespace, second })
        {
            Assert.Contains(FindUnauthorizedImplementationViolations([fixture]), violation =>
                violation.Contains("IThemePackageContentReader", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void WrongAssemblyPackageReaderFailsTheImplementationGuard()
    {
        Assembly fixture = PhaseThreeScopeBoundaryTests.BuildFixtureAssembly(
            null,
            null,
            includeCandidateAContracts: true,
            assemblyName: "Tcc.Themes.ReaderSubstitute");

        Assert.Contains(FindUnauthorizedImplementationViolations([fixture]), violation =>
            violation.Contains("IThemePackageContentReader", StringComparison.Ordinal));
    }

    [Fact]
    public void ExactPackageReaderFixtureIsAccepted()
    {
        Assembly fixture = PhaseThreeScopeBoundaryTests.BuildFixtureAssembly(
            null,
            "namespace Tcc.Themes.Compatibility.Binding { internal sealed class ThemeCompatibilityContentSnapshot : global::Tcc.Presentation.Contracts.Theme.IThemePackageContentReader { "
            + "public global::System.Threading.Tasks.ValueTask<global::System.Collections.Generic.IReadOnlyList<global::Tcc.Presentation.Contracts.Theme.ThemePackageContentEntryV1>> EnumerateEntriesAsync(global::Tcc.Presentation.Contracts.Theme.ThemePackageRef packageRef, global::System.Threading.CancellationToken cancellationToken = default) => throw new global::System.NotSupportedException(); "
            + "public global::System.Threading.Tasks.ValueTask<global::System.ReadOnlyMemory<byte>> ReadContentAsync(global::Tcc.Presentation.Contracts.Theme.ThemePackageRef packageRef, string canonicalPath, global::System.Threading.CancellationToken cancellationToken = default) => throw new global::System.NotSupportedException(); } }",
            includeCandidateAContracts: false);

        Assert.DoesNotContain(FindUnauthorizedImplementationViolations([fixture]), violation =>
            violation.Contains("IThemePackageContentReader", StringComparison.Ordinal));
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
        string[] guardedInterfaceNames = RequiredPhaseTwoInterfaces
            .Concat(ApprovedContractAmendmentInterfaces)
            .ToArray();
        Type[] requiredInterfaces = typeof(IThemePackage).Assembly
            .GetExportedTypes()
            .Where(type => type.IsInterface && guardedInterfaceNames.Contains(type.Name, StringComparer.Ordinal))
            .ToArray();
        List<string> violations = [];

        foreach (Type contractInterface in requiredInterfaces)
        {
            Type[] implementations = contractInterface == typeof(IThemePackageContentReader)
                ? productionTypes.Where(type => !type.IsInterface && type.GetInterfaces().Contains(contractInterface)).ToArray()
                : productionTypes.Where(type => type.IsClass && !type.IsAbstract && contractInterface.IsAssignableFrom(type)).ToArray();

            foreach (Type implementation in implementations)
            {
                bool isApprovedManifestValidator = contractInterface == typeof(IThemeManifestValidator)
                    && implementation == typeof(ThemeManifestValidator);
                bool isApprovedVerifier = contractInterface == typeof(IThemeIntegrityVerifierV2)
                    && implementation == typeof(Tcc.Themes.Integrity.ThemeIntegrityVerifier)
                    && PhaseThreeScopeBoundaryTests.IsExactPublicVerifier(implementation);
                bool isApprovedPackageReader = contractInterface == typeof(IThemePackageContentReader)
                    && IsExactPackageReader(implementation);
                if (!isApprovedManifestValidator && !isApprovedVerifier && !isApprovedPackageReader)
                {
                    violations.Add(
                        $"{implementation.FullName} is an unauthorized production implementation of {contractInterface.FullName}.");
                }
            }
        }

        return violations.Order(StringComparer.Ordinal).ToArray();
    }

    private static void AssertExactPackageReaderImplementation(IEnumerable<Type> productionTypes)
    {
        Type reader = Assert.Single(productionTypes, type => !type.IsInterface
            && type.GetInterfaces().Contains(typeof(IThemePackageContentReader)));
        Assert.True(IsExactPackageReader(reader));
    }

    private static bool IsExactPackageReader(Type type) =>
        string.Equals(type.FullName,
            "Tcc.Themes.Compatibility.Binding.ThemeCompatibilityContentSnapshot",
            StringComparison.Ordinal)
        && string.Equals(type.Assembly.GetName().Name, "Tcc.Themes", StringComparison.Ordinal)
        && type.IsNotPublic && !type.IsVisible && type.IsClass && type.IsSealed && !type.IsAbstract
        && !type.IsGenericType && !type.IsNested
        && !type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false)
        && type.GetInterfaces().SequenceEqual([typeof(IThemePackageContentReader)]);
}
