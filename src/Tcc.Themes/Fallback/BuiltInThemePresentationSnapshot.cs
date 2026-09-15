using System.Collections.Frozen;
using System.Text.Json;
using Tcc.Presentation.Contracts.Theme;

namespace Tcc.Themes.Fallback;

public sealed class BuiltInThemePresentationSnapshot
{
    internal BuiltInThemePresentationSnapshot(
        ThemeVariantId variantId,
        ThemeTokenBundle tokens,
        ThemeFocusStyles focusStyles,
        ThemeAccessibilityContract accessibilityRequirements)
    {
        if (variantId.Value is not ("deep" or "light"))
        {
            throw new ArgumentException("The built-in variant must be canonical.", nameof(variantId));
        }

        ArgumentNullException.ThrowIfNull(tokens);
        ArgumentNullException.ThrowIfNull(focusStyles);
        ArgumentNullException.ThrowIfNull(accessibilityRequirements);

        ValidatePresentation(variantId, tokens, focusStyles, accessibilityRequirements);

        Dictionary<string, ThemeTokenValue> copiedTokens = new(StringComparer.Ordinal);
        foreach (KeyValuePair<string, ThemeTokenValue> entry in tokens.Tokens)
        {
            ThemeTokenValue value = entry.Value
                ?? throw new ArgumentException("Token values cannot be null.", nameof(tokens));
            copiedTokens.Add(entry.Key, new ThemeTokenValue(value.Type, value.Value.Clone()));
        }

        Dictionary<string, ThemeSemanticBinding> copiedBindings = new(StringComparer.Ordinal);
        foreach (KeyValuePair<string, ThemeSemanticBinding> entry in tokens.SemanticBindings)
        {
            ThemeSemanticBinding binding = entry.Value
                ?? throw new ArgumentException("Semantic bindings cannot be null.", nameof(tokens));
            copiedBindings.Add(
                entry.Key,
                new ThemeSemanticBinding(
                    binding.Token,
                    binding.RequiresTextLabel,
                    binding.RequiresIconOrStructure));
        }

        VariantId = new ThemeVariantId(variantId.Value);
        Tokens = new ThemeTokenBundle(
            tokens.SchemaVersion,
            copiedTokens.ToFrozenDictionary(StringComparer.Ordinal),
            copiedBindings.ToFrozenDictionary(StringComparer.Ordinal));
        FocusStyles = new ThemeFocusStyles(
            focusStyles.SchemaVersion,
            focusStyles.MinimumContrastRatio,
            focusStyles.VisibleOnAllInteractiveControls,
            focusStyles.ProgrammaticOrderPreserved,
            focusStyles.ModalFocusTrapPreserved,
            focusStyles.FocusRestorationRequired);
        AccessibilityRequirements = new ThemeAccessibilityContract(
            accessibilityRequirements.SupportsTextScaling,
            accessibilityRequirements.SupportsZoom,
            accessibilityRequirements.SupportsContrastModes,
            accessibilityRequirements.SupportsColorVisionModes,
            accessibilityRequirements.SupportsReducedMotion,
            accessibilityRequirements.SupportsReducedTransparency,
            accessibilityRequirements.SupportsSoundControls,
            accessibilityRequirements.PreservesKeyboardFocusVisibility,
            accessibilityRequirements.PreservesFullKeyboardOperation,
            accessibilityRequirements.PreservesScreenReaderLabels,
            accessibilityRequirements.PreservesCriticalAlertRedundancy);
    }

    public ThemeVariantId VariantId { get; }

    public ThemeTokenBundle Tokens { get; }

    public ThemeFocusStyles FocusStyles { get; }

    public ThemeAccessibilityContract AccessibilityRequirements { get; }

    private static void ValidatePresentation(
        ThemeVariantId variantId,
        ThemeTokenBundle tokens,
        ThemeFocusStyles focusStyles,
        ThemeAccessibilityContract accessibilityRequirements)
    {
        if (!string.Equals(tokens.SchemaVersion, "1.0", StringComparison.Ordinal)
            || tokens.Tokens is null
            || tokens.SemanticBindings is null
            || tokens.Tokens.Count != 14
            || tokens.SemanticBindings.Count != 4)
        {
            throw new ArgumentException("The built-in token bundle is incomplete.", nameof(tokens));
        }

        ValidateToken(tokens, "color.background.base", "color");
        ValidateToken(tokens, "color.background.surface", "color");
        ValidateToken(tokens, "color.text.primary", "color");
        ValidateToken(tokens, "color.text.secondary", "color");
        ValidateToken(tokens, "color.state.tradable", "color");
        ValidateToken(tokens, "color.state.warning", "color");
        ValidateToken(tokens, "color.state.blocked", "color");
        ValidateToken(tokens, "color.state.unknown", "color");
        ValidateToken(tokens, "focus.ring.color", "color");
        ValidateToken(tokens, "focus.ring.thickness", "dimension");
        ValidateToken(tokens, "font.family.ui", "font");
        ValidateToken(tokens, "font.size.body", "dimension");
        ValidateToken(tokens, "motion.duration.short", "duration");
        ValidateToken(tokens, "surface.opacity", "opacity");

        bool deep = string.Equals(variantId.Value, "deep", StringComparison.Ordinal);
        ValidateStringValue(tokens, "color.background.base", deep ? "#111827" : "#FFFFFF");
        ValidateStringValue(tokens, "color.background.surface", deep ? "#1F2937" : "#F3F4F6");
        ValidateStringValue(tokens, "color.text.primary", deep ? "#F9FAFB" : "#111827");
        ValidateStringValue(tokens, "color.text.secondary", deep ? "#D1D5DB" : "#374151");
        ValidateStringValue(tokens, "color.state.tradable", deep ? "#86EFAC" : "#166534");
        ValidateStringValue(tokens, "color.state.warning", deep ? "#FDE68A" : "#854D0E");
        ValidateStringValue(tokens, "color.state.blocked", deep ? "#FCA5A5" : "#991B1B");
        ValidateStringValue(tokens, "color.state.unknown", deep ? "#D1D5DB" : "#374151");
        ValidateStringValue(tokens, "focus.ring.color", deep ? "#FDE047" : "#1D4ED8");
        ValidateNumberValue(tokens, "focus.ring.thickness", 2d);
        ValidateStringValue(tokens, "font.family.ui", "Segoe UI");
        ValidateNumberValue(tokens, "font.size.body", 16d);
        ValidateStringValue(tokens, "motion.duration.short", "0ms");
        ValidateNumberValue(tokens, "surface.opacity", 1d);

        ValidateBinding(tokens, "trading_permission.tradable", "color.state.tradable");
        ValidateBinding(tokens, "trading_permission.warning", "color.state.warning");
        ValidateBinding(tokens, "trading_permission.blocked", "color.state.blocked");
        ValidateBinding(tokens, "presentation.unknown", "color.state.unknown");

        if (!string.Equals(focusStyles.SchemaVersion, "1.0", StringComparison.Ordinal)
            || focusStyles.MinimumContrastRatio != 3m
            || !focusStyles.VisibleOnAllInteractiveControls
            || !focusStyles.ProgrammaticOrderPreserved
            || !focusStyles.ModalFocusTrapPreserved
            || !focusStyles.FocusRestorationRequired)
        {
            throw new ArgumentException("The built-in focus requirements are invalid.", nameof(focusStyles));
        }

        if (!accessibilityRequirements.SupportsTextScaling
            || !accessibilityRequirements.SupportsZoom
            || !accessibilityRequirements.SupportsContrastModes
            || !accessibilityRequirements.SupportsColorVisionModes
            || !accessibilityRequirements.SupportsReducedMotion
            || !accessibilityRequirements.SupportsReducedTransparency
            || !accessibilityRequirements.SupportsSoundControls
            || !accessibilityRequirements.PreservesKeyboardFocusVisibility
            || !accessibilityRequirements.PreservesFullKeyboardOperation
            || !accessibilityRequirements.PreservesScreenReaderLabels
            || !accessibilityRequirements.PreservesCriticalAlertRedundancy)
        {
            throw new ArgumentException("The built-in accessibility requirements are incomplete.", nameof(accessibilityRequirements));
        }
    }

    private static void ValidateToken(ThemeTokenBundle bundle, string name, string expectedType)
    {
        if (!bundle.Tokens.TryGetValue(name, out ThemeTokenValue? value)
            || value is null
            || !string.Equals(value.Type, expectedType, StringComparison.Ordinal)
            || value.Value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            throw new ArgumentException($"Built-in token '{name}' is invalid.", nameof(bundle));
        }
    }

    private static void ValidateBinding(ThemeTokenBundle bundle, string name, string token)
    {
        if (!bundle.SemanticBindings.TryGetValue(name, out ThemeSemanticBinding? binding)
            || binding is null
            || !string.Equals(binding.Token, token, StringComparison.Ordinal)
            || !binding.RequiresTextLabel
            || !binding.RequiresIconOrStructure)
        {
            throw new ArgumentException($"Built-in semantic binding '{name}' is invalid.", nameof(bundle));
        }
    }

    private static void ValidateStringValue(ThemeTokenBundle bundle, string name, string expected)
    {
        JsonElement value = bundle.Tokens[name].Value;
        if (value.ValueKind != JsonValueKind.String
            || !string.Equals(value.GetString(), expected, StringComparison.Ordinal))
        {
            throw new ArgumentException($"Built-in token '{name}' has an unauthorized value.", nameof(bundle));
        }
    }

    private static void ValidateNumberValue(ThemeTokenBundle bundle, string name, double expected)
    {
        JsonElement value = bundle.Tokens[name].Value;
        if (value.ValueKind != JsonValueKind.Number
            || !value.TryGetDouble(out double actual)
            || !double.IsFinite(actual)
            || actual != expected)
        {
            throw new ArgumentException($"Built-in token '{name}' has an unauthorized value.", nameof(bundle));
        }
    }
}
