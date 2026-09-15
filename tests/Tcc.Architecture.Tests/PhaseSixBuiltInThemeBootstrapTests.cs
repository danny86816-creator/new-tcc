using System.Collections.Frozen;
using System.Text.Json;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Fallback;

namespace Tcc.Architecture.Tests;

public sealed class PhaseSixBuiltInThemeBootstrapTests
{
    [Fact]
    public void BuiltInSourceExposesStableExactDeepAndLightPresentations()
    {
        BuiltInThemePresentationSource source = new();

        BuiltInThemePresentationSnapshot deep = source.GetPresentation(new ThemeVariantId("deep"));
        BuiltInThemePresentationSnapshot light = source.GetPresentation(new ThemeVariantId("light"));

        Assert.Same(deep, source.GetPresentation(new ThemeVariantId("deep")));
        Assert.Same(light, source.GetPresentation(new ThemeVariantId("light")));
        AssertPresentation(deep, "deep", "#111827", "#F9FAFB", "#FDE047");
        AssertPresentation(light, "light", "#FFFFFF", "#111827", "#1D4ED8");
        Assert.Throws<ArgumentOutOfRangeException>(() => source.GetPresentation(default));
        Assert.Throws<ArgumentOutOfRangeException>(() => source.GetPresentation(new ThemeVariantId("Deep")));
        Assert.Throws<ArgumentOutOfRangeException>(() => source.GetPresentation(new ThemeVariantId("unknown")));
    }

    [Fact]
    public void SnapshotDeepCopiesJsonRecordsAndDictionaries()
    {
        using JsonDocument document = JsonDocument.Parse("\"#111827\"");
        Dictionary<string, ThemeTokenValue> tokens = CreateTokens(document.RootElement);
        Dictionary<string, ThemeSemanticBinding> bindings = CreateBindings();
        ThemeTokenBundle input = new("1.0", tokens, bindings);
        ThemeFocusStyles focus = new("1.0", 3m, true, true, true, true);
        ThemeAccessibilityContract accessibility = new(true, true, true, true, true, true, true, true, true, true, true);

        BuiltInThemePresentationSnapshot snapshot = new(
            new ThemeVariantId("deep"), input, focus, accessibility);
        tokens["color.background.base"] = new ThemeTokenValue("color", JsonDocument.Parse("\"#FFFFFF\"").RootElement);
        bindings["presentation.unknown"] = new ThemeSemanticBinding("wrong", false, false);
        document.Dispose();

        Assert.Equal("#111827", snapshot.Tokens.Tokens["color.background.base"].Value.GetString());
        Assert.Equal("color.state.unknown", snapshot.Tokens.SemanticBindings["presentation.unknown"].Token);
        Assert.IsAssignableFrom<FrozenDictionary<string, ThemeTokenValue>>(snapshot.Tokens.Tokens);
        Assert.IsAssignableFrom<FrozenDictionary<string, ThemeSemanticBinding>>(snapshot.Tokens.SemanticBindings);
        Assert.Throws<NotSupportedException>(() =>
            ((IDictionary<string, ThemeTokenValue>)snapshot.Tokens.Tokens).Add(
                "injected",
                snapshot.Tokens.Tokens["color.background.base"]));

        ThemeTokenBundle alteredCopy = snapshot.Tokens with { SchemaVersion = "changed" };
        ThemeFocusStyles alteredFocus = snapshot.FocusStyles with { MinimumContrastRatio = 0m };
        Assert.Equal("1.0", snapshot.Tokens.SchemaVersion);
        Assert.Equal(3m, snapshot.FocusStyles.MinimumContrastRatio);
        Assert.Equal("changed", alteredCopy.SchemaVersion);
        Assert.Equal(0m, alteredFocus.MinimumContrastRatio);
    }

    [Fact]
    public void SnapshotRejectsNonCanonicalOrIncompleteInputs()
    {
        BuiltInThemePresentationSource source = new();
        BuiltInThemePresentationSnapshot deep = source.GetPresentation(new ThemeVariantId("deep"));
        Dictionary<string, ThemeTokenValue> tokens = new(deep.Tokens.Tokens, StringComparer.Ordinal);
        tokens.Remove("surface.opacity");

        Assert.Throws<ArgumentException>(() => new BuiltInThemePresentationSnapshot(
            new ThemeVariantId("deep"),
            deep.Tokens with { Tokens = tokens },
            deep.FocusStyles,
            deep.AccessibilityRequirements));
        Assert.Throws<ArgumentException>(() => new BuiltInThemePresentationSnapshot(
            new ThemeVariantId("Deep"), deep.Tokens, deep.FocusStyles, deep.AccessibilityRequirements));
        Assert.Throws<ArgumentNullException>(() => new BuiltInThemePresentationSnapshot(
            new ThemeVariantId("deep"), null!, deep.FocusStyles, deep.AccessibilityRequirements));
    }

    [Fact]
    public void ThemeAssemblyContainsExactlyTheTwoBuiltInResources()
    {
        string[] resources = typeof(BuiltInThemePresentationSource).Assembly.GetManifestResourceNames();

        Assert.Contains("Tcc.Themes.Fallback.Deep.resources", resources);
        Assert.Contains("Tcc.Themes.Fallback.Light.resources", resources);
        Assert.Equal(2, resources.Count(name => name.StartsWith("Tcc.Themes.Fallback.", StringComparison.Ordinal)));
    }

    private static void AssertPresentation(
        BuiltInThemePresentationSnapshot presentation,
        string variant,
        string background,
        string text,
        string focus)
    {
        Assert.Equal(variant, presentation.VariantId.Value);
        Assert.Equal("1.0", presentation.Tokens.SchemaVersion);
        Assert.Equal(14, presentation.Tokens.Tokens.Count);
        Assert.Equal(4, presentation.Tokens.SemanticBindings.Count);
        Assert.Equal(background, presentation.Tokens.Tokens["color.background.base"].Value.GetString());
        Assert.Equal(text, presentation.Tokens.Tokens["color.text.primary"].Value.GetString());
        Assert.Equal(focus, presentation.Tokens.Tokens["focus.ring.color"].Value.GetString());
        Assert.Equal("Segoe UI", presentation.Tokens.Tokens["font.family.ui"].Value.GetString());
        Assert.Equal(2d, presentation.Tokens.Tokens["focus.ring.thickness"].Value.GetDouble());
        Assert.Equal(16d, presentation.Tokens.Tokens["font.size.body"].Value.GetDouble());
        Assert.Equal("0ms", presentation.Tokens.Tokens["motion.duration.short"].Value.GetString());
        Assert.Equal(1d, presentation.Tokens.Tokens["surface.opacity"].Value.GetDouble());
        Assert.All(presentation.Tokens.SemanticBindings.Values, binding =>
        {
            Assert.True(binding.RequiresTextLabel);
            Assert.True(binding.RequiresIconOrStructure);
        });
        Assert.Equal(3m, presentation.FocusStyles.MinimumContrastRatio);
        Assert.True(presentation.FocusStyles.VisibleOnAllInteractiveControls);
        Assert.True(presentation.AccessibilityRequirements.SupportsTextScaling);
        Assert.True(presentation.AccessibilityRequirements.SupportsZoom);
        Assert.True(presentation.AccessibilityRequirements.SupportsContrastModes);
        Assert.True(presentation.AccessibilityRequirements.SupportsColorVisionModes);
        Assert.True(presentation.AccessibilityRequirements.SupportsReducedMotion);
        Assert.True(presentation.AccessibilityRequirements.SupportsReducedTransparency);
        Assert.True(presentation.AccessibilityRequirements.SupportsSoundControls);
        Assert.True(presentation.AccessibilityRequirements.PreservesKeyboardFocusVisibility);
        Assert.True(presentation.AccessibilityRequirements.PreservesFullKeyboardOperation);
        Assert.True(presentation.AccessibilityRequirements.PreservesScreenReaderLabels);
        Assert.True(presentation.AccessibilityRequirements.PreservesCriticalAlertRedundancy);
    }

    private static Dictionary<string, ThemeTokenValue> CreateTokens(JsonElement baseColor)
    {
        Dictionary<string, ThemeTokenValue> values = new(StringComparer.Ordinal);
        AddString(values, "color.background.base", "color", baseColor);
        AddString(values, "color.background.surface", "color", "#1F2937");
        AddString(values, "color.text.primary", "color", "#F9FAFB");
        AddString(values, "color.text.secondary", "color", "#D1D5DB");
        AddString(values, "color.state.tradable", "color", "#86EFAC");
        AddString(values, "color.state.warning", "color", "#FDE68A");
        AddString(values, "color.state.blocked", "color", "#FCA5A5");
        AddString(values, "color.state.unknown", "color", "#D1D5DB");
        AddString(values, "focus.ring.color", "color", "#FDE047");
        AddNumber(values, "focus.ring.thickness", "dimension", 2);
        AddString(values, "font.family.ui", "font", "Segoe UI");
        AddNumber(values, "font.size.body", "dimension", 16);
        AddString(values, "motion.duration.short", "duration", "0ms");
        AddNumber(values, "surface.opacity", "opacity", 1);
        return values;
    }

    private static Dictionary<string, ThemeSemanticBinding> CreateBindings() => new(StringComparer.Ordinal)
    {
        ["trading_permission.tradable"] = new("color.state.tradable", true, true),
        ["trading_permission.warning"] = new("color.state.warning", true, true),
        ["trading_permission.blocked"] = new("color.state.blocked", true, true),
        ["presentation.unknown"] = new("color.state.unknown", true, true),
    };

    private static void AddString(
        Dictionary<string, ThemeTokenValue> values,
        string name,
        string type,
        object value)
    {
        JsonElement element = value is JsonElement json
            ? json
            : JsonSerializer.SerializeToElement((string)value);
        values.Add(name, new ThemeTokenValue(type, element));
    }

    private static void AddNumber(
        Dictionary<string, ThemeTokenValue> values,
        string name,
        string type,
        double value) =>
        values.Add(name, new ThemeTokenValue(type, JsonSerializer.SerializeToElement(value)));
}
