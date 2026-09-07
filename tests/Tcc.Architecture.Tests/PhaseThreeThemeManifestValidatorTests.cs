using System.Reflection;
using System.Text.Json;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Manifests;

namespace Tcc.Architecture.Tests;

public sealed class PhaseThreeThemeManifestValidatorTests
{
    private static readonly string[] KnownCapabilities =
    [
        "presentation.tokens",
        "presentation.layout",
        "presentation.components",
        "presentation.icons",
        "presentation.copy.noncritical",
        "presentation.motion",
        "presentation.parallax",
        "presentation.cursor.showcase_limited",
        "presentation.audio.ui",
        "presentation.audio.bgm",
        "presentation.audio.ambient",
        "presentation.loading_empty_error",
        "presentation.time_of_day_scene",
        "presentation.trading_state_overlay",
        "presentation.workspace_appearance",
        "presentation.floating_workspace_appearance",
        "presentation.personalization",
    ];

    private static readonly string[] RequiredFeatureFlagGuards =
    [
        "risk",
        "permissions",
        "audit",
        "recovery",
        "connector_scope",
        "gpt_ai_required_state",
        "authoritative_data",
        "home_safety_core",
        "critical_alert_priority",
        "confirmation_semantics",
        "accessibility",
    ];

    private static readonly string[] RequiredDegradedModePreservations =
    [
        "safety_core",
        "critical_alerts",
        "keyboard",
        "screen_reader",
        "contrast",
        "risk_permission_semantics",
        "confirmation_semantics",
        "accessibility",
    ];

    private readonly ThemeManifestValidator validator = new();

    [Fact]
    public void ProductionTypeImplementsTheSealedPhaseTwoContract()
    {
        Assert.IsAssignableFrom<IThemeManifestValidator>(validator);
    }

    [Fact]
    public void MinimumValidManifestPasses()
    {
        ThemeManifestValidationResult result = Validate(CreateValidManifest());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void FullyPopulatedValidManifestPasses()
    {
        ThemeManifest manifest = CreateValidManifest() with
        {
            Package = CreateValidManifest().Package with
            {
                Homepage = "https://example.invalid/theme",
                SupportUrl = "https://example.invalid/support",
                Tags = ["deep", "fintech"],
            },
            Variants =
            [
                new ThemeVariant("deep", "deep", "Deep", true, "tokens/deep.json", ["tokens/high-contrast.json"], null),
                new ThemeVariant("light", "light", "Light", false, "tokens/light.json", [], "deep"),
            ],
            Capabilities = KnownCapabilities,
            FeatureFlags = new ThemeFeatureFlags(
            [
                new ThemePresentationFeatureFlag(
                    "presentation.decorative_scene",
                    false,
                    "Optional decorative scene.",
                    RequiredFeatureFlagGuards),
            ]),
            Audio = new ThemeAudioDeclaration("audio/sound-pack.json", "audio/bgm.json", "audio/ambient.json", true, true, true, true),
            Personalization = new ThemePersonalizationDeclaration(
                "personalization/presets.json",
                "personalization/safe-ranges.json",
                "personalization/migrations.json",
                true),
            Rollback = new ThemeRollbackDeclaration("rollback.json", true, [">=0.9.0 <1.0.0"]),
        };

        ThemeManifestValidationResult result = Validate(manifest);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void UnsupportedSchemaAndApiVersionsFailClosedWithOrderedDiagnostics()
    {
        ThemeManifest manifest = CreateValidManifest() with
        {
            SchemaVersion = "2.0",
            ThemeApiVersion = "2.0.0",
        };

        ThemeManifestValidationResult result = Validate(manifest);

        Assert.False(result.IsValid);
        Assert.Equal(
        [
            "P3M002 schema_version '2.0' is unsupported.",
            "P3M003 theme_api_version '2.0.0' is unsupported; expected '1.0.0'.",
        ], result.Errors);
    }

    [Theory]
    [InlineData("blank-name")]
    [InlineData("malformed-package-id")]
    [InlineData("invalid-version")]
    [InlineData("invalid-channel")]
    public void InvalidPackageIdentityFailsWithRelevantDiagnostic(string mutation)
    {
        ThemeManifest manifest = CreateValidManifest();
        ThemePackageIdentity package = mutation switch
        {
            "blank-name" => manifest.Package with { Name = " " },
            "malformed-package-id" => manifest.Package with { PackageId = "Invalid Package" },
            "invalid-version" => manifest.Package with { Version = "01.0.0" },
            "invalid-channel" => manifest.Package with { Channel = "store" },
            _ => throw new ArgumentOutOfRangeException(nameof(mutation)),
        };

        ThemeManifestValidationResult result = Validate(manifest with { Package = package });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.StartsWith("P3M01", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("version-range", "P3M031")]
    [InlineData("platform", "P3M032")]
    [InlineData("portable", "P3M033")]
    [InlineData("dpi", "P3M034")]
    public void InvalidCompatibilityDeclarationFailsClosed(string mutation, string expectedCode)
    {
        ThemeManifest manifest = CreateValidManifest();
        ThemeCompatibilityDeclaration compatibility = mutation switch
        {
            "version-range" => manifest.Compatibility with { RequiredThemeApiVersion = "latest" },
            "platform" => manifest.Compatibility with { SupportedPlatforms = ["linux"] },
            "portable" => manifest.Compatibility with { PortableSupported = false },
            "dpi" => manifest.Compatibility with { MinimumDpiScale = 2m, MaximumTestedDpiScale = 1m },
            _ => throw new ArgumentOutOfRangeException(nameof(mutation)),
        };

        ThemeManifestValidationResult result = Validate(manifest with { Compatibility = compatibility });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.StartsWith(expectedCode, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("zero-default", "P3M048")]
    [InlineData("multiple-defaults", "P3M048")]
    [InlineData("duplicate-id", "P3M043")]
    [InlineData("missing-fallback", "P3M050")]
    [InlineData("self-fallback", "P3M049")]
    [InlineData("fallback-cycle", "P3M051")]
    public void InvalidVariantGraphFailsClosed(string mutation, string expectedCode)
    {
        ThemeVariant deep = new("deep", "deep", "Deep", true, "tokens/deep.json", [], null);
        ThemeVariant light = new("light", "light", "Light", false, "tokens/light.json", [], "deep");
        IReadOnlyList<ThemeVariant> variants = mutation switch
        {
            "zero-default" => [deep with { Default = false }, light],
            "multiple-defaults" => [deep, light with { Default = true }],
            "duplicate-id" => [deep, light with { VariantId = "deep" }],
            "missing-fallback" => [deep, light with { FallbackVariantId = "missing" }],
            "self-fallback" => [deep, light with { FallbackVariantId = "light" }],
            "fallback-cycle" => [deep with { FallbackVariantId = "light" }, light],
            _ => throw new ArgumentOutOfRangeException(nameof(mutation)),
        };

        ThemeManifestValidationResult result = Validate(CreateValidManifest() with { Variants = variants });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.StartsWith(expectedCode, StringComparison.Ordinal));
    }

    [Fact]
    public void ValidFallbackReferencePasses()
    {
        ThemeManifest manifest = CreateValidManifest() with
        {
            Variants =
            [
                new ThemeVariant("deep", "deep", "Deep", true, "tokens/deep.json", [], null),
                new ThemeVariant("light", "light", "Light", false, "tokens/light.json", [], "deep"),
            ],
        };

        Assert.True(Validate(manifest).IsValid);
    }

    [Fact]
    public void UnknownCapabilityFailsClosed()
    {
        ThemeManifestValidationResult result = Validate(CreateValidManifest() with
        {
            Capabilities = ["presentation.tokens", "presentation.future"],
        });

        Assert.False(result.IsValid);
        Assert.Contains("P3M064 capability 'presentation.future' is unknown.", result.Errors);
    }

    [Fact]
    public void ForbiddenCapabilityFailsEvenWhenContextCallsItKnown()
    {
        ThemeManifest manifest = CreateValidManifest() with { Capabilities = ["risk.mutate"] };
        ThemeManifestValidationContext context = CreateContext() with
        {
            KnownCapabilities = new HashSet<string>(["risk.mutate"], StringComparer.Ordinal),
        };

        ThemeManifestValidationResult result = validator.Validate(manifest, context);

        Assert.False(result.IsValid);
        Assert.Contains("P3M063 capability 'risk.mutate' is forbidden because Theme capabilities must be presentation-only.", result.Errors);
    }

    [Theory]
    [MemberData(nameof(FeatureFlagGuardCases))]
    public void FeatureFlagMustDeclareEverySafetyBoundary(string omittedGuard)
    {
        ThemePresentationFeatureFlag flag = new(
            "presentation.example",
            false,
            "Example",
            RequiredFeatureFlagGuards.Where(value => value != omittedGuard).ToArray());
        ThemeManifest manifest = CreateValidManifest() with
        {
            FeatureFlags = new ThemeFeatureFlags([flag]),
        };

        ThemeManifestValidationResult result = Validate(manifest);

        Assert.False(result.IsValid);
        Assert.Contains(
            $"P3M075 feature flag 'presentation.example' must declare that it cannot affect '{omittedGuard}'.",
            result.Errors);
    }

    public static TheoryData<string> FeatureFlagGuardCases => new()
    {
        "risk",
        "permissions",
        "audit",
        "recovery",
        "connector_scope",
        "gpt_ai_required_state",
        "authoritative_data",
        "home_safety_core",
        "critical_alert_priority",
        "confirmation_semantics",
        "accessibility",
    };

    [Theory]
    [MemberData(nameof(DegradedModePreservationCases))]
    public void DegradedModeCannotRemoveSafetyOrAccessibilitySemantics(string omittedPreservation)
    {
        ThemeManifest manifest = CreateValidManifest();
        manifest = manifest with
        {
            DegradedMode = manifest.DegradedMode with
            {
                Preserves = RequiredDegradedModePreservations
                    .Where(value => value != omittedPreservation)
                    .ToArray(),
            },
        };

        ThemeManifestValidationResult result = Validate(manifest);

        Assert.False(result.IsValid);
        Assert.Contains(
            $"P3M085 degraded_mode must preserve '{omittedPreservation}'.",
            result.Errors);
    }

    public static TheoryData<string> DegradedModePreservationCases => new()
    {
        "safety_core",
        "critical_alerts",
        "keyboard",
        "screen_reader",
        "contrast",
        "risk_permission_semantics",
        "confirmation_semantics",
        "accessibility",
    };

    [Fact]
    public void Q93ClaimRemainsADeclarationAndDoesNotNeedToClaimCompliance()
    {
        ThemeManifest manifest = CreateValidManifest();
        manifest = manifest with
        {
            Accessibility = manifest.Accessibility with { Q93CompliantClaim = false },
        };

        ThemeManifestValidationResult result = Validate(manifest);

        Assert.True(result.IsValid);
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("text_scaling", "P3M091")]
    [InlineData("reduced_motion", "P3M095")]
    [InlineData("keyboard", "P3M099")]
    [InlineData("screen_reader", "P3M100")]
    [InlineData("critical_alerts", "P3M101")]
    public void MandatoryAccessibilityDeclarationsCannotBeDisabled(string mutation, string expectedCode)
    {
        ThemeManifest manifest = CreateValidManifest();
        ThemeAccessibilityDeclaration accessibility = mutation switch
        {
            "text_scaling" => manifest.Accessibility with { TextScalingSupported = false },
            "reduced_motion" => manifest.Accessibility with { ReducedMotionSupported = false },
            "keyboard" => manifest.Accessibility with { FullKeyboardOperation = false },
            "screen_reader" => manifest.Accessibility with { ScreenReaderLabelsPreserved = false },
            "critical_alerts" => manifest.Accessibility with { CriticalAlertRedundancySupported = false },
            _ => throw new ArgumentOutOfRangeException(nameof(mutation)),
        };

        ThemeManifestValidationResult result = Validate(manifest with { Accessibility = accessibility });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.StartsWith(expectedCode, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("asset-reference", "P3M111")]
    [InlineData("duplicate-tier", "P3M112")]
    [InlineData("audio-reference", "P3M120")]
    [InlineData("audio-required", "P3M123")]
    [InlineData("motion-reference", "P3M131")]
    [InlineData("mandatory-motion", "P3M134")]
    [InlineData("personalization-reference", "P3M141")]
    [InlineData("personalization-reset", "P3M143")]
    public void InvalidPresentationDeclarationsFailClosed(string mutation, string expectedCode)
    {
        ThemeManifest manifest = CreateValidManifest() with
        {
            Audio = new ThemeAudioDeclaration("audio/sound-pack.json", "audio/bgm.json", "audio/ambient.json", true, true, true, true),
            Personalization = new ThemePersonalizationDeclaration(
                "personalization/presets.json", "personalization/ranges.json", "personalization/migrations.json", true),
        };
        manifest = mutation switch
        {
            "asset-reference" => manifest with { Assets = manifest.Assets with { Inventory = "../assets.json" } },
            "duplicate-tier" => manifest with { Assets = manifest.Assets with { Tiers = ["tier0", "tier0"] } },
            "audio-reference" => manifest with { Audio = manifest.Audio! with { SoundPack = "C:\\audio.json" } },
            "audio-required" => manifest with { Audio = manifest.Audio! with { Optional = false } },
            "motion-reference" => manifest with { Motion = manifest.Motion with { Profiles = "https://example.invalid/motion.json" } },
            "mandatory-motion" => manifest with { Motion = manifest.Motion with { SkippableTransitions = false } },
            "personalization-reference" => manifest with { Personalization = manifest.Personalization! with { SafeRanges = "" } },
            "personalization-reset" => manifest with { Personalization = manifest.Personalization! with { ResetSupported = false } },
            _ => throw new ArgumentOutOfRangeException(nameof(mutation)),
        };

        ThemeManifestValidationResult result = Validate(manifest);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.StartsWith(expectedCode, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("invalid-integrity-reference", "P3M151")]
    [InlineData("invalid-hash", "P3M154")]
    [InlineData("missing-stable-signature", "P3M155")]
    [InlineData("invalid-rollback-reference", "P3M160")]
    [InlineData("migration-without-personalization", "P3M163")]
    [InlineData("invalid-rollback-version-range", "P3M162")]
    public void InvalidIntegrityOrRollbackDeclarationFailsClosed(string mutation, string expectedCode)
    {
        ThemeManifest manifest = CreateValidManifest() with
        {
            Rollback = new ThemeRollbackDeclaration("rollback.json", false, []),
        };
        manifest = mutation switch
        {
            "invalid-integrity-reference" => manifest with
            {
                Integrity = manifest.Integrity with { IntegrityManifest = "../integrity.json" },
            },
            "invalid-hash" => manifest with { Integrity = manifest.Integrity with { HashAlgorithm = "md5" } },
            "missing-stable-signature" => manifest with { Integrity = manifest.Integrity with { Signature = null } },
            "invalid-rollback-reference" => manifest with
            {
                Rollback = manifest.Rollback! with { RollbackManifest = "/rollback.json" },
            },
            "migration-without-personalization" => manifest with
            {
                Rollback = manifest.Rollback! with { StateMigrationSupported = true },
                Personalization = null,
            },
            "invalid-rollback-version-range" => manifest with
            {
                Rollback = manifest.Rollback! with { PreviousVersionCompatibility = ["previous"] },
            },
            _ => throw new ArgumentOutOfRangeException(nameof(mutation)),
        };

        ThemeManifestValidationResult result = Validate(manifest);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.StartsWith(expectedCode, StringComparison.Ordinal));
    }

    [Fact]
    public void NonStableValidationContextAllowsUnsignedDeveloperManifest()
    {
        ThemeManifest manifest = CreateValidManifest() with
        {
            Package = CreateValidManifest().Package with { Channel = "developer" },
            Integrity = CreateValidManifest().Integrity with { Signature = null },
        };
        ThemeManifestValidationContext context = CreateContext() with { IsStableChannel = false };

        ThemeManifestValidationResult result = validator.Validate(manifest, context);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("stable", true, true, true)]
    [InlineData("stable", true, false, false)]
    [InlineData("stable", false, false, false)]
    [InlineData("stable", false, true, true)]
    [InlineData("beta", false, false, true)]
    [InlineData("developer", false, false, true)]
    [InlineData("beta", true, false, false)]
    [InlineData("developer", true, false, false)]
    public void SignatureDeclarationPolicyCannotBeBypassedByChannelContext(
        string packageChannel,
        bool isStableContext,
        bool signaturePresent,
        bool expectedValid)
    {
        ThemeManifest baseline = CreateValidManifest();
        ThemeManifest manifest = baseline with
        {
            Package = baseline.Package with { Channel = packageChannel },
            Integrity = baseline.Integrity with
            {
                Signature = signaturePresent ? "signature.sig" : null,
            },
        };
        ThemeManifestValidationContext context = CreateContext() with
        {
            IsStableChannel = isStableContext,
        };

        ThemeManifestValidationResult result = validator.Validate(manifest, context);

        Assert.Equal(expectedValid, result.IsValid);
        if (!expectedValid)
        {
            Assert.Contains(result.Errors, error => error.Contains("signature declaration", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void DifferentDiagnosticSemanticRulesEmitDifferentCodes()
    {
        ThemeManifest baseline = CreateValidManifest();
        (string Rule, string ExpectedCode, string ExpectedSeverity, bool ExpectedValid, ThemeManifestValidationResult Result)[] cases =
        [
            ("manifest required", "P3M000", "Error", false, validator.Validate(null!, CreateContext())),
            ("context required", "P3M001", "Error", false, validator.Validate(baseline, null!)),
            ("compatibility required", "P3M030", "Error", false, Validate(baseline with { Compatibility = null! })),
            ("compatibility version", "P3M031", "Error", false, Validate(baseline with
            {
                Compatibility = baseline.Compatibility with { RequiredCoreVersion = "latest" },
            })),
            ("compatibility platform", "P3M032", "Error", false, Validate(baseline with
            {
                Compatibility = baseline.Compatibility with { SupportedPlatforms = ["linux"] },
            })),
            ("compatibility Portable declaration", "P3M033", "Error", false, Validate(baseline with
            {
                Compatibility = baseline.Compatibility with { PortableSupported = false },
            })),
            ("compatibility DPI declaration", "P3M034", "Error", false, Validate(baseline with
            {
                Compatibility = baseline.Compatibility with { MinimumDpiScale = 2m, MaximumTestedDpiScale = 1m },
            })),
            ("full keyboard operation", "P3M099", "Error", false, Validate(baseline with
            {
                Accessibility = baseline.Accessibility with { FullKeyboardOperation = false },
            })),
            ("screen reader labels", "P3M100", "Error", false, Validate(baseline with
            {
                Accessibility = baseline.Accessibility with { ScreenReaderLabelsPreserved = false },
            })),
            ("critical alert redundancy", "P3M101", "Error", false, Validate(baseline with
            {
                Accessibility = baseline.Accessibility with { CriticalAlertRedundancySupported = false },
            })),
        ];

        Assert.All(
            cases,
            semanticCase =>
            {
                Assert.False(semanticCase.ExpectedValid);
                Assert.Equal(semanticCase.ExpectedValid, semanticCase.Result.IsValid);
                Assert.Equal("Error", semanticCase.ExpectedSeverity);
                Assert.Empty(semanticCase.Result.Warnings);
                Assert.Equal(semanticCase.ExpectedCode, GetSingleDiagnosticCode(semanticCase.Result));
            });
        Assert.Equal(cases.Length, cases.Select(semanticCase => semanticCase.ExpectedCode).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void DiagnosticSemanticCodeTableContainsNinetySixUniqueWellFormedCodes()
    {
        Type diagnosticCodes = typeof(ThemeManifestValidator).GetNestedType(
            "DiagnosticCodes",
            BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ThemeManifestValidator.DiagnosticCodes was not found.");
        string[] codes = diagnosticCodes
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(field => Assert.IsType<string>(field.GetRawConstantValue()))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(96, codes.Length);
        Assert.Equal(codes.Length, codes.Distinct(StringComparer.Ordinal).Count());
        Assert.All(
            codes,
            code => Assert.True(
                code.Length == 6
                && code.StartsWith("P3M", StringComparison.Ordinal)
                && code[3..].All(char.IsAsciiDigit),
                $"Diagnostic code '{code}' must use P3M plus exactly three digits."));
    }

    [Fact]
    public void EveryDiagnosticSemanticRuleHasExpectedCodeSeverityAndOutcome()
    {
        ThemeManifest baseline = CreateValidManifest();
        List<(string ExpectedCode, string ExpectedSeverity, bool ExpectedValid, ThemeManifestValidationResult Result)> cases = [];

        void Add(ThemeManifestValidationResult result, params string[] expectedCodes)
        {
            cases.AddRange(expectedCodes.Select(code => (code, "Error", false, result)));
        }

        Add(validator.Validate(null!, CreateContext()), "P3M000");
        Add(validator.Validate(baseline, null!), "P3M001");
        Add(Validate(baseline with { SchemaVersion = "2.0", ThemeApiVersion = "2.0.0" }), "P3M002", "P3M003");

        Add(Validate(baseline with { Package = null! }), "P3M010");
        Add(
            Validate(baseline with
            {
                Package = new ThemePackageIdentity(
                    "",
                    "",
                    "",
                    "",
                    "",
                    "invalid",
                    "store",
                    "",
                    " ",
                    " ",
                    "",
                    ["", ""]),
            }),
            "P3M011", "P3M012", "P3M013", "P3M014", "P3M015", "P3M016",
            "P3M017", "P3M018", "P3M019", "P3M020", "P3M021", "P3M022");

        Add(Validate(baseline with { Compatibility = null! }), "P3M030");
        Add(
            Validate(baseline with
            {
                Compatibility = new ThemeCompatibilityDeclaration(
                    "latest", ">=1.0.0 <2.0.0", ">=1.1.0 <2.0.0", ["linux", "linux"], false, 2m, 1m),
            }),
            "P3M031", "P3M032", "P3M033", "P3M034");

        Add(Validate(baseline with { Variants = null! }), "P3M040");
        Add(Validate(baseline with { Variants = [null!] }), "P3M041", "P3M048");
        Add(
            Validate(baseline with
            {
                Variants = [new ThemeVariant("", "invalid", "", true, "../tokens.json", ["/override.json"], null)],
            }),
            "P3M042", "P3M044", "P3M045", "P3M046", "P3M047");
        Add(
            Validate(baseline with
            {
                Variants =
                [
                    new ThemeVariant("deep", "deep", "Deep", true, "tokens/deep.json", [], null),
                    new ThemeVariant("deep", "light", "Light", false, "tokens/light.json", [], null),
                ],
            }),
            "P3M043");
        Add(
            Validate(baseline with
            {
                Variants = [new ThemeVariant("deep", "deep", "Deep", true, "tokens/deep.json", [], "deep")],
            }),
            "P3M049");
        Add(
            Validate(baseline with
            {
                Variants = [new ThemeVariant("deep", "deep", "Deep", true, "tokens/deep.json", [], "missing")],
            }),
            "P3M050");
        Add(
            Validate(baseline with
            {
                Variants =
                [
                    new ThemeVariant("deep", "deep", "Deep", true, "tokens/deep.json", [], "light"),
                    new ThemeVariant("light", "light", "Light", false, "tokens/light.json", [], "deep"),
                ],
            }),
            "P3M051");

        Add(Validate(baseline with { Capabilities = null! }), "P3M060");
        Add(Validate(baseline with { Capabilities = [null!] }), "P3M061");
        Add(Validate(baseline with { Capabilities = ["presentation.tokens", "presentation.tokens"] }), "P3M062");
        Add(Validate(baseline with { Capabilities = ["risk.mutate"] }), "P3M063");
        Add(Validate(baseline with { Capabilities = ["presentation.unknown"] }), "P3M064");

        ThemePresentationFeatureFlag validFlag = new(
            "presentation.example", false, "Example", RequiredFeatureFlagGuards);
        Add(Validate(baseline with { FeatureFlags = null! }), "P3M070");
        Add(Validate(baseline with { FeatureFlags = new ThemeFeatureFlags([null!]) }), "P3M071");
        Add(
            Validate(baseline with
            {
                FeatureFlags = new ThemeFeatureFlags(
                    [new ThemePresentationFeatureFlag("risk.example", false, "", RequiredFeatureFlagGuards)]),
            }),
            "P3M072", "P3M074");
        Add(
            Validate(baseline with { FeatureFlags = new ThemeFeatureFlags([validFlag, validFlag]) }),
            "P3M073");
        Add(
            Validate(baseline with
            {
                FeatureFlags = new ThemeFeatureFlags(
                    [validFlag with { CannotAffect = RequiredFeatureFlagGuards[1..] }]),
            }),
            "P3M075");
        Add(
            Validate(baseline with
            {
                FeatureFlags = new ThemeFeatureFlags(
                    [validFlag with { CannotAffect = [.. RequiredFeatureFlagGuards, RequiredFeatureFlagGuards[0]] }]),
            }),
            "P3M076");

        Add(Validate(baseline with { DegradedMode = null! }), "P3M080");
        Add(
            Validate(baseline with
            {
                DegradedMode = new ThemeDegradedModeDeclaration(
                    false,
                    ["none"],
                    [.. RequiredDegradedModePreservations, RequiredDegradedModePreservations[0]],
                    "none"),
            }),
            "P3M081", "P3M082", "P3M083", "P3M084");
        Add(
            Validate(baseline with
            {
                DegradedMode = baseline.DegradedMode with { Preserves = RequiredDegradedModePreservations[1..] },
            }),
            "P3M085");

        Add(Validate(baseline with { Accessibility = null! }), "P3M090");
        Add(
            Validate(baseline with
            {
                Accessibility = new ThemeAccessibilityDeclaration(
                    false, false, false, false, false, false, false, false, false, false, false, false),
            }),
            "P3M091", "P3M092", "P3M093", "P3M094", "P3M095", "P3M096",
            "P3M097", "P3M098", "P3M099", "P3M100", "P3M101");

        Add(Validate(baseline with { Assets = null! }), "P3M110");
        Add(
            Validate(baseline with
            {
                Assets = new ThemeAssetDeclaration("../assets.json", ["tier0", "tier0", "invalid"], -1, "md5"),
            }),
            "P3M111", "P3M112", "P3M113", "P3M114");

        Add(
            Validate(baseline with
            {
                Audio = new ThemeAudioDeclaration("../sound.json", "/bgm.json", "C:\\ambient.json", false, false, false, false),
            }),
            "P3M120", "P3M121", "P3M122", "P3M123", "P3M124", "P3M125", "P3M126");

        Add(Validate(baseline with { Motion = null! }), "P3M130");
        Add(
            Validate(baseline with
            {
                Motion = new ThemeMotionDeclaration("../profiles.json", "/reduced.json", false, false, false),
            }),
            "P3M131", "P3M132", "P3M133", "P3M134", "P3M135");

        Add(
            Validate(baseline with
            {
                Personalization = new ThemePersonalizationDeclaration(
                    "../presets.json", "/ranges.json", "C:\\migrations.json", false),
            }),
            "P3M140", "P3M141", "P3M142", "P3M143");

        Add(Validate(baseline with { Integrity = null! }), "P3M150");
        Add(
            Validate(baseline with
            {
                Integrity = new ThemeIntegrityDeclaration(
                    "../integrity.json", "/signature.sig", "never", "md5"),
            }),
            "P3M151", "P3M152", "P3M153", "P3M154");
        Add(Validate(baseline with { Integrity = baseline.Integrity with { Signature = null } }), "P3M155");

        Add(
            Validate(baseline with
            {
                Rollback = new ThemeRollbackDeclaration("/rollback.json", false, null!),
            }),
            "P3M160", "P3M161");
        Add(
            Validate(baseline with
            {
                Rollback = new ThemeRollbackDeclaration("rollback.json", false, ["previous"]),
            }),
            "P3M162");
        Add(
            Validate(baseline with
            {
                Rollback = new ThemeRollbackDeclaration("rollback.json", true, []),
                Personalization = null,
            }),
            "P3M163");

        Type diagnosticCodes = typeof(ThemeManifestValidator).GetNestedType(
            "DiagnosticCodes",
            BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ThemeManifestValidator.DiagnosticCodes was not found.");
        string[] mappedCodes = diagnosticCodes
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(field => Assert.IsType<string>(field.GetRawConstantValue()))
            .Order(StringComparer.Ordinal)
            .ToArray();
        string[] expectedCodes = cases
            .Select(semanticCase => semanticCase.ExpectedCode)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(mappedCodes, expectedCodes);
        Assert.All(
            cases,
            semanticCase =>
            {
                Assert.Equal("Error", semanticCase.ExpectedSeverity);
                Assert.False(semanticCase.ExpectedValid);
                Assert.Equal(semanticCase.ExpectedValid, semanticCase.Result.IsValid);
                Assert.Empty(semanticCase.Result.Warnings);
                Assert.Contains(
                    semanticCase.Result.Errors,
                    error => error.StartsWith($"{semanticCase.ExpectedCode} ", StringComparison.Ordinal));
            });
    }

    [Fact]
    public void RepeatedValidationProducesIdenticalOrderedDiagnostics()
    {
        ThemeManifest invalid = CreateValidManifest() with
        {
            SchemaVersion = "2.0",
            ThemeApiVersion = "2.0.0",
            Package = CreateValidManifest().Package with { Name = "" },
            Capabilities = ["unknown.capability"],
        };

        ThemeManifestValidationResult first = Validate(invalid);

        for (int iteration = 0; iteration < 20; iteration++)
        {
            ThemeManifestValidationResult repeated = Validate(invalid);
            Assert.Equal(first.IsValid, repeated.IsValid);
            Assert.Equal(first.Errors, repeated.Errors);
            Assert.Equal(first.Warnings, repeated.Warnings);
        }
    }

    [Fact]
    public void ValidationDoesNotMutateTheInputGraph()
    {
        ThemeManifest manifest = CreateValidManifest() with
        {
            Capabilities = ["presentation.tokens", "unknown.capability"],
        };
        JsonSerializerOptions options = ThemeContractJson.CreateSerializerOptions();
        string before = JsonSerializer.Serialize(manifest, options);

        _ = Validate(manifest);

        string after = JsonSerializer.Serialize(manifest, options);
        Assert.Equal(before, after);
    }

    private ThemeManifestValidationResult Validate(ThemeManifest manifest) =>
        validator.Validate(manifest, CreateContext());

    private static string GetSingleDiagnosticCode(ThemeManifestValidationResult result)
    {
        Assert.False(result.IsValid);
        Assert.Empty(result.Warnings);
        string error = Assert.Single(result.Errors);
        return error.Split(' ', 2, StringSplitOptions.None)[0];
    }

    private static ThemeManifestValidationContext CreateContext() => new(
        new HashSet<string>([ContractVersions.Schema], StringComparer.Ordinal),
        new HashSet<string>(KnownCapabilities, StringComparer.Ordinal),
        true);

    private static ThemeManifest CreateValidManifest() => new(
        ContractVersions.Schema,
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
            ">=1.0.0 <2.0.0",
            ">=1.0.0 <2.0.0",
            ">=1.1.0 <2.0.0",
            ["windows"],
            true,
            1m,
            3m),
        [new ThemeVariant("deep", "deep", "Deep", true, "tokens/deep.json", [], null)],
        ["presentation.tokens"],
        new ThemeFeatureFlags([]),
        new ThemeDegradedModeDeclaration(
            true,
            ["none", "reduced_decoration", "minimal_decoration", "safe_presentation_only"],
            RequiredDegradedModePreservations,
            "safe_presentation_only"),
        new ThemeAccessibilityDeclaration(true, true, true, true, true, true, true, true, true, true, true, true),
        new ThemeAssetDeclaration("assets/index.json", ["tier0"], 0, "sha256"),
        null,
        new ThemeMotionDeclaration("motion/profiles.json", "motion/reduced.json", true, true, true),
        null,
        new ThemeIntegrityDeclaration("integrity.json", "signature.sig", "stable_or_store", "sha256"),
        null);
}
