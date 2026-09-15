using System.Globalization;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using Tcc.Themes.Fallback;

namespace Tcc.DesktopHost.EmbeddedSafeTheme;

internal static class SafeThemeResourceAdapter
{
    internal static ResourceDictionary Create(BuiltInThemePresentationSnapshot presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);

        ResourceDictionary resources = new();
        foreach (KeyValuePair<string, Tcc.Presentation.Contracts.Theme.ThemeTokenValue> entry
            in presentation.Tokens.Tokens)
        {
            resources.Add(entry.Key, ConvertValue(entry.Key, entry.Value.Type, entry.Value.Value));
        }

        return resources;
    }

    private static object ConvertValue(string key, string type, JsonElement value)
    {
        if (string.Equals(type, "color", StringComparison.Ordinal))
        {
            Color color = SystemParameters.HighContrast
                ? GetHighContrastColor(key)
                : ParseColor(value.GetString());
            SolidColorBrush brush = new(color);
            brush.Freeze();
            return brush;
        }

        if (string.Equals(type, "font", StringComparison.Ordinal))
        {
            return new FontFamily(value.GetString() ?? throw new JsonException("Font token is null."));
        }

        if (string.Equals(type, "duration", StringComparison.Ordinal))
        {
            if (!string.Equals(value.GetString(), "0ms", StringComparison.Ordinal))
            {
                throw new JsonException("Only the approved zero-duration token is supported.");
            }

            return TimeSpan.Zero;
        }

        if (string.Equals(type, "dimension", StringComparison.Ordinal)
            || string.Equals(type, "opacity", StringComparison.Ordinal))
        {
            double number = value.GetDouble();
            if (!double.IsFinite(number))
            {
                throw new JsonException("Numeric presentation tokens must be finite.");
            }

            return number;
        }

        throw new JsonException($"Unsupported built-in token type '{type}'.");
    }

    private static Color ParseColor(string? text)
    {
        if (text is null
            || text.Length != 7
            || text[0] != '#'
            || !byte.TryParse(text.AsSpan(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte red)
            || !byte.TryParse(text.AsSpan(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte green)
            || !byte.TryParse(text.AsSpan(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte blue))
        {
            throw new JsonException("Built-in color token is malformed.");
        }

        return Color.FromArgb(255, red, green, blue);
    }

    private static Color GetHighContrastColor(string key)
    {
        if (string.Equals(key, "focus.ring.color", StringComparison.Ordinal))
        {
            return SystemColors.HighlightColor;
        }

        if (key.StartsWith("color.background.", StringComparison.Ordinal))
        {
            return SystemColors.WindowColor;
        }

        return SystemColors.WindowTextColor;
    }
}
