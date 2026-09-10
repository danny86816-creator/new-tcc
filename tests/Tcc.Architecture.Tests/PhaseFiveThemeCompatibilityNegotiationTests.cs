using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Compatibility;

namespace Tcc.Architecture.Tests;

public sealed class PhaseFiveThemeCompatibilityNegotiationTests
{
    private static readonly string[] ExpectedCompatibilityTypeNames =
    [
        "Tcc.Themes.Compatibility.ThemeCompatibilityNegotiationFailureKind",
        "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiation",
        "Tcc.Themes.Compatibility.ThemeCompatibilityVersionNegotiator",
    ];

    private static readonly string[] ExpectedOutcomePropertyNames =
        ["CanContinue", "Failures", "SelectedThemeApiVersion", "SelectedUxContractVersion"];

    private static readonly string[] ExpectedFailureKindNames =
    [
        "InvalidVersionInput",
        "UnsatisfiableVersionRange",
        "UnsupportedManifestSchema",
        "ConflictingVersionDeclaration",
        "CoreVersionIncompatible",
        "ThemeApiNoCompatibleVersion",
        "UxContractNoCompatibleVersion",
    ];

    [Fact]
    public void NullRequestIsAProgrammingPreconditionViolation()
    {
        Assert.Throws<ArgumentNullException>(() => ThemeCompatibilityVersionNegotiator.Negotiate(null!));
    }

    [Fact]
    public void ExactSchemaAndAuthorizedVersionsProduceSuccessfulNegotiation()
    {
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest());

        Assert.True(result.CanContinue);
        Assert.Equal(ContractVersions.ThemeApi, result.SelectedThemeApiVersion);
        Assert.Equal(ContractVersions.UxContract, result.SelectedUxContractVersion);
        Assert.Empty(result.Failures);
    }

    [Theory]
    [InlineData("1.0.0", "1.0")]
    [InlineData("1.0", "1.0.0")]
    [InlineData(null, "1.0")]
    [InlineData("1.0", null)]
    public void UnsupportedSchemaHardShortCircuitsAllVersionEvaluation(string? themeSchema, string? compatibilitySchema)
    {
        ThemeCompatibilityRequest request = CreateRequest(
            themeSchema: themeSchema,
            compatibilitySchema: compatibilitySchema,
            themeCoreRange: "not-a-range",
            compatibilityCoreRange: "still-not-a-range",
            coreVersion: "also-invalid");

        ThemeCompatibilityVersionNegotiation result = Negotiate(request);

        Assert.False(result.CanContinue);
        Assert.Null(result.SelectedThemeApiVersion);
        Assert.Null(result.SelectedUxContractVersion);
        Assert.Equal(
            new[] { ThemeCompatibilityNegotiationFailureKind.UnsupportedManifestSchema },
            result.Failures);
    }

    [Theory]
    [InlineData("01.0.0")]
    [InlineData("1.0")]
    [InlineData("v1.0.0")]
    [InlineData("-1.0.0")]
    [InlineData("1.0.0-alpha")]
    [InlineData("1.0.0+build")]
    [InlineData("1.0.0 ")]
    [InlineData("１.0.0")]
    public void NonCanonicalExactMachineVersionsAreRejected(string version)
    {
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest(coreVersion: version));

        Assert.False(result.CanContinue);
        Assert.Equal(new[] { ThemeCompatibilityNegotiationFailureKind.InvalidVersionInput }, result.Failures);
    }

    [Theory]
    [InlineData("0.9.9", false)]
    [InlineData("1.0.0", true)]
    [InlineData("1.9.9", true)]
    [InlineData("2.0.0", false)]
    public void CoreRangeUsesExactHalfOpenBoundaries(string coreVersion, bool expected)
    {
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest(coreVersion: coreVersion));

        Assert.Equal(expected, result.CanContinue);
        Assert.Equal(
            expected ? Array.Empty<ThemeCompatibilityNegotiationFailureKind>()
                : new[] { ThemeCompatibilityNegotiationFailureKind.CoreVersionIncompatible },
            result.Failures);
    }

    [Theory]
    [InlineData(">=2.0.0 <1.0.0")]
    [InlineData(">1.0.0 <1.0.1")]
    [InlineData("<0.0.0")]
    [InlineData("=1.0.0 =2.0.0")]
    [InlineData("=1.0.0 >1.0.0")]
    public void GrammarValidEmptyCoreRangesAreUnsatisfiable(string range)
    {
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest(
            themeCoreRange: range,
            compatibilityCoreRange: range));

        Assert.False(result.CanContinue);
        Assert.Equal(new[] { ThemeCompatibilityNegotiationFailureKind.UnsatisfiableVersionRange }, result.Failures);
    }

    [Theory]
    [InlineData(">=1.0.0 <2.0.0", "<2.0.0 >=1.0.0")]
    [InlineData(">1.0.0", ">=1.0.1")]
    [InlineData("1.0.0", "=1.0.0")]
    [InlineData("=1.0.0", ">=1.0.0 <=1.0.0")]
    [InlineData("  >=1.0.0   <2.0.0  ", "<2.0.0 >=1.0.0")]
    public void CompleteNormalizedAcceptedSetsDetermineSemanticEquality(string themeRange, string compatibilityRange)
    {
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest(
            themeCoreRange: themeRange,
            compatibilityCoreRange: compatibilityRange,
            coreVersion: themeRange.StartsWith(">1.0.0", StringComparison.Ordinal) ? "1.0.1" : "1.0.0"));

        Assert.DoesNotContain(ThemeCompatibilityNegotiationFailureKind.ConflictingVersionDeclaration, result.Failures);
        Assert.True(result.CanContinue);
    }

    [Fact]
    public void ValidButNonEquivalentDeclarationsConflictInsteadOfBeingNarrowed()
    {
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest(
            themeCoreRange: ">=1.0.0 <2.0.0",
            compatibilityCoreRange: ">=1.5.0 <2.0.0"));

        Assert.False(result.CanContinue);
        Assert.Equal(new[] { ThemeCompatibilityNegotiationFailureKind.ConflictingVersionDeclaration }, result.Failures);
    }

    [Fact]
    public void MalformedDeclarationPreventsConflictClassification()
    {
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest(
            themeCoreRange: "latest",
            compatibilityCoreRange: ">=1.5.0 <2.0.0"));

        Assert.Equal(new[] { ThemeCompatibilityNegotiationFailureKind.InvalidVersionInput }, result.Failures);
    }

    [Theory]
    [InlineData("theme-core")]
    [InlineData("compatibility-core")]
    [InlineData("theme-api")]
    [InlineData("compatibility-api")]
    [InlineData("theme-ux")]
    [InlineData("compatibility-ux")]
    public void AllSixRequiredRangeFieldsUseTheSameStrictGrammar(string target)
    {
        ThemeCompatibilityRequest baseline = CreateRequest();
        ThemeCompatibilityDeclaration themeDeclaration = baseline.Manifest.Compatibility;
        ThemeCompatibilityManifest compatibility = baseline.Compatibility;
        ThemeCompatibilityRequest request = target switch
        {
            "theme-core" => baseline with
            {
                Manifest = baseline.Manifest with
                {
                    Compatibility = themeDeclaration with { RequiredCoreVersion = "^1.0.0" },
                },
            },
            "compatibility-core" => baseline with
            {
                Compatibility = compatibility with { Core = compatibility.Core with { Required = "^1.0.0" } },
            },
            "theme-api" => baseline with
            {
                Manifest = baseline.Manifest with
                {
                    Compatibility = themeDeclaration with { RequiredThemeApiVersion = "^1.0.0" },
                },
            },
            "compatibility-api" => baseline with
            {
                Compatibility = compatibility with { ThemeApi = compatibility.ThemeApi with { Required = "^1.0.0" } },
            },
            "theme-ux" => baseline with
            {
                Manifest = baseline.Manifest with
                {
                    Compatibility = themeDeclaration with { RequiredUxContractVersion = "^1.0.0" },
                },
            },
            "compatibility-ux" => baseline with
            {
                Compatibility = compatibility with { UxContract = compatibility.UxContract with { Required = "^1.0.0" } },
            },
            _ => throw new ArgumentOutOfRangeException(nameof(target)),
        };

        ThemeCompatibilityVersionNegotiation result = Negotiate(request);

        Assert.False(result.CanContinue);
        Assert.Equal(new[] { ThemeCompatibilityNegotiationFailureKind.InvalidVersionInput }, result.Failures);
    }

    [Fact]
    public void MalformedCoreDoesNotHideIndependentApiAndUxFailures()
    {
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest(
            themeCoreRange: "latest",
            compatibilityCoreRange: "latest",
            supportedThemeApiVersions: new HashSet<string>(["9.0.0"], StringComparer.Ordinal),
            supportedUxContractVersions: new HashSet<string>(["9.0.0"], StringComparer.Ordinal)));

        Assert.Equal(
            new[]
            {
                ThemeCompatibilityNegotiationFailureKind.InvalidVersionInput,
                ThemeCompatibilityNegotiationFailureKind.ThemeApiNoCompatibleVersion,
                ThemeCompatibilityNegotiationFailureKind.UxContractNoCompatibleVersion,
            },
            result.Failures);
    }

    [Fact]
    public void ConflictingApiDoesNotHideIndependentCoreAndUxFailures()
    {
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest(
            themeApiRange: ">=1.0.0 <2.0.0",
            compatibilityThemeApiRange: ">=1.5.0 <2.0.0",
            coreVersion: "9.0.0",
            supportedUxContractVersions: new HashSet<string>(["9.0.0"], StringComparer.Ordinal)));

        Assert.Equal(
            new[]
            {
                ThemeCompatibilityNegotiationFailureKind.ConflictingVersionDeclaration,
                ThemeCompatibilityNegotiationFailureKind.CoreVersionIncompatible,
                ThemeCompatibilityNegotiationFailureKind.UxContractNoCompatibleVersion,
            },
            result.Failures);
    }

    [Fact]
    public void ThemeApiCallerClaimsCannotWidenSealedProductionAuthority()
    {
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest(
            supportedThemeApiVersions: new HashSet<string>(["9.0.0", "1.0.0"], StringComparer.Ordinal)));

        Assert.True(result.CanContinue);
        Assert.Equal("1.0.0", result.SelectedThemeApiVersion);
        Assert.NotEqual("9.0.0", result.SelectedThemeApiVersion);
    }

    [Fact]
    public void UnsupportedThemeApiCallerClaimProducesNoCompatibleVersion()
    {
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest(
            supportedThemeApiVersions: new HashSet<string>(["9.0.0"], StringComparer.Ordinal)));

        Assert.Null(result.SelectedThemeApiVersion);
        Assert.Equal(new[] { ThemeCompatibilityNegotiationFailureKind.ThemeApiNoCompatibleVersion }, result.Failures);
    }

    [Fact]
    public void CanonicalUxMachineVersionIsSelectedWhenAllowed()
    {
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest());

        Assert.Equal("1.1.0", result.SelectedUxContractVersion);
    }

    [Theory]
    [InlineData("v1.1")]
    [InlineData("1.1")]
    public void UxDisplayTokensAreNotNormalizedInsideNegotiation(string displayToken)
    {
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest(
            supportedUxContractVersions: new HashSet<string>([displayToken], StringComparer.Ordinal)));

        Assert.Null(result.SelectedUxContractVersion);
        Assert.Equal(new[] { ThemeCompatibilityNegotiationFailureKind.InvalidVersionInput }, result.Failures);
    }

    [Fact]
    public void TestedVersionsCannotRescueMissingAuthorizedCallerSupport()
    {
        ThemeCompatibilityRequest baseline = CreateRequest(
            supportedThemeApiVersions: new HashSet<string>(["9.0.0"], StringComparer.Ordinal));
        ThemeCompatibilityRequest request = baseline with
        {
            Compatibility = baseline.Compatibility with
            {
                ThemeApi = baseline.Compatibility.ThemeApi with { Tested = ["1.0.0", "9.0.0"] },
            },
        };

        ThemeCompatibilityVersionNegotiation result = Negotiate(request);

        Assert.Null(result.SelectedThemeApiVersion);
        Assert.Equal(new[] { ThemeCompatibilityNegotiationFailureKind.ThemeApiNoCompatibleVersion }, result.Failures);
    }

    [Fact]
    public void IndependentCompatibilityFailuresUseFixedCoreApiUxOrder()
    {
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest(
            coreVersion: "9.0.0",
            supportedThemeApiVersions: new HashSet<string>(["9.0.0"], StringComparer.Ordinal),
            supportedUxContractVersions: new HashSet<string>(["9.0.0"], StringComparer.Ordinal)));

        Assert.Equal(
            new[]
            {
                ThemeCompatibilityNegotiationFailureKind.CoreVersionIncompatible,
                ThemeCompatibilityNegotiationFailureKind.ThemeApiNoCompatibleVersion,
                ThemeCompatibilityNegotiationFailureKind.UxContractNoCompatibleVersion,
            },
            result.Failures);
    }

    [Fact]
    public void BigIntegerComponentsCompareWithoutArtificialMaximum()
    {
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest(
            themeCoreRange: ">18446744073709551615.0.0",
            compatibilityCoreRange: ">=18446744073709551615.0.1",
            coreVersion: "18446744073709551616.0.0"));

        Assert.True(result.CanContinue);
    }

    [Fact]
    public void NumericComparisonDoesNotUseLexicalVersionOrdering()
    {
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest(
            themeCoreRange: ">1.9.0",
            compatibilityCoreRange: ">=1.9.1",
            coreVersion: "1.10.0"));

        Assert.True(result.CanContinue);
    }

    [Theory]
    [InlineData(">=1.0.0\t<2.0.0")]
    [InlineData(">=1.0.0\n<2.0.0")]
    [InlineData(">=1.0.0\u00a0<2.0.0")]
    [InlineData(">= 1.0.0")]
    [InlineData(">=1.0.0,<2.0.0")]
    [InlineData(">=1.0.0 || <2.0.0")]
    [InlineData("1.0.0 - 2.0.0")]
    [InlineData("^1.0.0")]
    [InlineData("~1.0.0")]
    [InlineData("1.*")]
    public void OnlyAsciiSpaceClauseSeparationIsAccepted(string range)
    {
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest(
            themeCoreRange: range,
            compatibilityCoreRange: range));

        Assert.Equal(new[] { ThemeCompatibilityNegotiationFailureKind.InvalidVersionInput }, result.Failures);
    }

    [Fact]
    public void ClauseAndCallerEnumerationOrderDoNotChangeOutcome()
    {
        ThemeCompatibilityVersionNegotiation first = Negotiate(CreateRequest(
            themeCoreRange: ">=1.0.0 <2.0.0 >=1.0.0",
            compatibilityCoreRange: "<2.0.0 >=1.0.0",
            supportedThemeApiVersions: new HashSet<string>(["9.0.0", "1.0.0"], StringComparer.Ordinal)));
        ThemeCompatibilityVersionNegotiation second = Negotiate(CreateRequest(
            themeCoreRange: "<2.0.0 >=1.0.0",
            compatibilityCoreRange: ">=1.0.0 <2.0.0",
            supportedThemeApiVersions: new HashSet<string>(["1.0.0", "9.0.0"], StringComparer.Ordinal)));

        AssertEquivalent(first, second);
    }

    [Fact]
    public void CultureDoesNotChangeCompleteOutcome()
    {
        ThemeCompatibilityRequest request = CreateRequest(
            themeCoreRange: ">18446744073709551615.1.9",
            compatibilityCoreRange: ">=18446744073709551615.1.10",
            coreVersion: "18446744073709551616.0.0");
        ThemeCompatibilityVersionNegotiation? expected = null;
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            foreach (string cultureName in new[] { "en-US", "tr-TR", "zh-TW" })
            {
                CultureInfo.CurrentCulture = new CultureInfo(cultureName);
                CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
                ThemeCompatibilityVersionNegotiation actual = Negotiate(request);
                if (expected is null)
                {
                    expected = actual;
                }
                else
                {
                    AssertEquivalent(expected, actual);
                }
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void CompletedOutcomeIsImmutableAfterCallerCollectionMutation()
    {
        HashSet<string> themeApi = new(["1.0.0"], StringComparer.Ordinal);
        HashSet<string> ux = new(["1.1.0"], StringComparer.Ordinal);
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest(
            supportedThemeApiVersions: themeApi,
            supportedUxContractVersions: ux));

        themeApi.Clear();
        themeApi.Add("9.0.0");
        ux.Clear();
        ux.Add("v1.1");

        Assert.True(result.CanContinue);
        Assert.Equal("1.0.0", result.SelectedThemeApiVersion);
        Assert.Equal("1.1.0", result.SelectedUxContractVersion);
        Assert.True(result.Failures.IsEmpty);
        Assert.IsType<ImmutableArray<ThemeCompatibilityNegotiationFailureKind>>(result.Failures);
    }

    [Fact]
    public async Task OverlappingCallsDoNotLeakStateAcrossSuccessAndFailureInputs()
    {
        ThemeCompatibilityRequest success = CreateRequest();
        ThemeCompatibilityRequest otherSuccess = CreateRequest(
            themeCoreRange: ">1.9.0",
            compatibilityCoreRange: ">=1.9.1",
            coreVersion: "1.10.0");
        ThemeCompatibilityRequest failure = CreateRequest(coreVersion: "9.0.0");
        Task<ThemeCompatibilityVersionNegotiation>[] calls = new Task<ThemeCompatibilityVersionNegotiation>[300];
        for (int index = 0; index < calls.Length; index++)
        {
            ThemeCompatibilityRequest request = (index % 3) switch
            {
                0 => success,
                1 => otherSuccess,
                _ => failure,
            };
            calls[index] = Task.Run(() => Negotiate(request));
        }

        ThemeCompatibilityVersionNegotiation[] results = await Task.WhenAll(calls);

        for (int index = 0; index < results.Length; index++)
        {
            if (index % 3 == 2)
            {
                Assert.Equal(new[] { ThemeCompatibilityNegotiationFailureKind.CoreVersionIncompatible }, results[index].Failures);
            }
            else
            {
                Assert.True(results[index].CanContinue);
                Assert.Empty(results[index].Failures);
            }
        }
    }

    [Fact]
    public void EveryCallerSupportedTokenIsValidatedBeforeAuthorityIntersection()
    {
        ThemeCompatibilityVersionNegotiation result = Negotiate(CreateRequest(
            supportedThemeApiVersions: new HashSet<string>(["1.0.0", "not-a-version", "9.0.0"], StringComparer.Ordinal)));

        Assert.Null(result.SelectedThemeApiVersion);
        Assert.Equal(new[] { ThemeCompatibilityNegotiationFailureKind.InvalidVersionInput }, result.Failures);
    }

    [Fact]
    public void CompiledPhaseFiveSurfaceIsInternalSynchronousAndStateless()
    {
        Assembly assembly = typeof(Tcc.Themes.AssemblyMarker).Assembly;
        Type[] compatibilityTypes = assembly.GetTypes()
            .Where(type => string.Equals(type.Namespace, "Tcc.Themes.Compatibility", StringComparison.Ordinal))
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(ExpectedCompatibilityTypeNames, compatibilityTypes.Select(type => type.FullName).ToArray());
        Assert.All(compatibilityTypes, type => Assert.False(type.IsVisible));
        Assert.Empty(typeof(ThemeCompatibilityVersionNegotiator).GetFields(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance));
        MethodInfo negotiate = typeof(ThemeCompatibilityVersionNegotiator).GetMethod(
            "Negotiate",
            BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Phase5A Negotiate method is missing.");
        Assert.Equal(typeof(ThemeCompatibilityVersionNegotiation), negotiate.ReturnType);
        Assert.False(typeof(Task).IsAssignableFrom(negotiate.ReturnType));
        Assert.False(negotiate.ReturnType.IsGenericType
            && negotiate.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>));
        Assert.DoesNotContain(
            assembly.GetTypes().Where(type => !type.IsInterface),
            type => typeof(IThemeCompatibilityResolver).IsAssignableFrom(type));
    }

    [Fact]
    public void OutcomeAndFailureEnumExposeOnlyTheAuthorizedInternalShape()
    {
        PropertyInfo[] properties = typeof(ThemeCompatibilityVersionNegotiation)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Equal(ExpectedOutcomePropertyNames,
            properties.Select(property => property.Name).Order(StringComparer.Ordinal).ToArray());
        Assert.Equal(typeof(ImmutableArray<ThemeCompatibilityNegotiationFailureKind>),
            Assert.Single(properties, property => property.Name == "Failures").PropertyType);
        Assert.Equal(ExpectedFailureKindNames, Enum.GetNames<ThemeCompatibilityNegotiationFailureKind>());
    }

    private static ThemeCompatibilityVersionNegotiation Negotiate(ThemeCompatibilityRequest request) =>
        ThemeCompatibilityVersionNegotiator.Negotiate(request);

    private static void AssertEquivalent(
        ThemeCompatibilityVersionNegotiation expected,
        ThemeCompatibilityVersionNegotiation actual)
    {
        Assert.Equal(expected.CanContinue, actual.CanContinue);
        Assert.Equal(expected.SelectedThemeApiVersion, actual.SelectedThemeApiVersion);
        Assert.Equal(expected.SelectedUxContractVersion, actual.SelectedUxContractVersion);
        Assert.Equal(expected.Failures, actual.Failures);
    }

    private static ThemeCompatibilityRequest CreateRequest(
        string? themeSchema = ContractVersions.Schema,
        string? compatibilitySchema = ContractVersions.Schema,
        string themeCoreRange = ">=1.0.0 <2.0.0",
        string? compatibilityCoreRange = null,
        string themeApiRange = ">=1.0.0 <2.0.0",
        string? compatibilityThemeApiRange = null,
        string uxRange = ">=1.1.0 <2.0.0",
        string? compatibilityUxRange = null,
        string coreVersion = "1.5.0",
        IReadOnlySet<string>? supportedThemeApiVersions = null,
        IReadOnlySet<string>? supportedUxContractVersions = null)
    {
        ThemeManifest manifest = CreateManifest(
            themeSchema!,
            themeCoreRange,
            themeApiRange,
            uxRange);
        ThemeCompatibilityManifest compatibility = new(
            compatibilitySchema!,
            "com.example.theme",
            "1.0.0",
            new ThemeVersionRange(compatibilityCoreRange ?? themeCoreRange, []),
            new ThemeVersionRange(compatibilityThemeApiRange ?? themeApiRange, []),
            new ThemeVersionRange(compatibilityUxRange ?? uxRange, []),
            new ThemeWindowsCompatibility(true, true, true, []),
            new Dictionary<string, string>(StringComparer.Ordinal));
        return new ThemeCompatibilityRequest(
            manifest,
            compatibility,
            coreVersion,
            supportedThemeApiVersions ?? new HashSet<string>([ContractVersions.ThemeApi], StringComparer.Ordinal),
            supportedUxContractVersions ?? new HashSet<string>([ContractVersions.UxContract], StringComparer.Ordinal),
            new HashSet<string>([ContractVersions.Schema], StringComparer.Ordinal),
            "windows",
            false,
            new ThemeAccessibilityValidationResult(ThemeAccessibilityStatus.Validated, [], []));
    }

    private static ThemeManifest CreateManifest(
        string schema,
        string coreRange,
        string themeApiRange,
        string uxRange) => new(
        schema,
        ContractVersions.ThemeApi,
        new ThemePackageIdentity(
            "com.example.theme",
            "com.example.theme.package",
            "Example Theme",
            "Example Publisher",
            "example-publisher",
            "1.0.0",
            "stable",
            "Presentation-only example theme.",
            null,
            null,
            "private",
            ["deep"]),
        new ThemeCompatibilityDeclaration(
            coreRange,
            themeApiRange,
            uxRange,
            ["windows"],
            true,
            1m,
            3m),
        [new ThemeVariant("deep", "deep", "Deep", true, "tokens/deep.json", [], null)],
        ["presentation.tokens"],
        new ThemeFeatureFlags([]),
        new ThemeDegradedModeDeclaration(true, ["safe_presentation_only"], ["home_safety_core"], "safe_presentation_only"),
        new ThemeAccessibilityDeclaration(true, true, true, true, true, true, true, true, true, true, true, true),
        new ThemeAssetDeclaration("assets/index.json", ["tier0"], 0, "sha256"),
        null,
        new ThemeMotionDeclaration("motion/profiles.json", "motion/reduced.json", true, true, true),
        null,
        new ThemeIntegrityDeclaration("integrity.json", "signature.sig", "stable_or_store", "sha256"),
        null);
}
