using System.Globalization;
using System.Resources;
using System.Text.Json;
using Tcc.Presentation.Contracts.Theme;

namespace Tcc.Themes.Fallback;

public sealed class BuiltInThemePresentationSource
{
    private readonly BuiltInThemePresentationSnapshot _deep;
    private readonly BuiltInThemePresentationSnapshot _light;

    public BuiltInThemePresentationSource()
    {
        _deep = Load("Tcc.Themes.Fallback.Deep", new ThemeVariantId("deep"));
        _light = Load("Tcc.Themes.Fallback.Light", new ThemeVariantId("light"));
    }

    public BuiltInThemePresentationSnapshot GetPresentation(ThemeVariantId variantId)
    {
        if (string.Equals(variantId.Value, "deep", StringComparison.Ordinal))
        {
            return _deep;
        }

        if (string.Equals(variantId.Value, "light", StringComparison.Ordinal))
        {
            return _light;
        }

        throw new ArgumentOutOfRangeException(nameof(variantId));
    }

    private static BuiltInThemePresentationSnapshot Load(string resourceBaseName, ThemeVariantId variantId)
    {
        ResourceManager manager = new(resourceBaseName, typeof(BuiltInThemePresentationSource).Assembly);
        string json = manager.GetString("Tokens", CultureInfo.InvariantCulture)
            ?? throw new MissingManifestResourceException($"Embedded built-in resource '{resourceBaseName}' is missing Tokens.");

        ThemeTokenBundle tokens = JsonSerializer.Deserialize<ThemeTokenBundle>(
            json,
            ThemeContractJson.CreateSerializerOptions())
            ?? throw new JsonException("Embedded built-in tokens cannot be null.");

        ThemeFocusStyles focusStyles = new("1.0", 3m, true, true, true, true);
        ThemeAccessibilityContract accessibility = new(
            true, true, true, true, true, true, true, true, true, true, true);

        return new BuiltInThemePresentationSnapshot(variantId, tokens, focusStyles, accessibility);
    }
}
