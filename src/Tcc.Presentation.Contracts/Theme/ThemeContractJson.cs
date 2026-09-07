using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tcc.Presentation.Contracts.Theme;

/// <summary>
/// Canonical serializer settings for deterministic Theme public-contract JSON.
/// </summary>
public static class ThemeContractJson
{
    public static JsonSerializerOptions CreateSerializerOptions(bool writeIndented = false)
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DictionaryKeyPolicy = null,
            PropertyNameCaseInsensitive = false,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            RespectNullableAnnotations = true,
            WriteIndented = writeIndented,
        };

        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
        options.Converters.Add(new StringValueObjectJsonConverter<ThemeId>(value => new(value), value => value.Value));
        options.Converters.Add(new StringValueObjectJsonConverter<ThemeVersion>(value => new(value), value => value.Value));
        options.Converters.Add(new StringValueObjectJsonConverter<ThemeApiVersion>(value => new(value), value => value.Value));
        options.Converters.Add(new StringValueObjectJsonConverter<UxContractVersion>(value => new(value), value => value.Value));
        options.Converters.Add(new StringValueObjectJsonConverter<UxSurfaceId>(value => new(value), value => value.Value));
        options.Converters.Add(new StringValueObjectJsonConverter<GlobalStateId>(value => new(value), value => value.Value));
        options.Converters.Add(new StringValueObjectJsonConverter<ThemeVariantId>(value => new(value), value => value.Value));
        options.Converters.Add(new StringValueObjectJsonConverter<ThemeAssetId>(value => new(value), value => value.Value));
        options.Converters.Add(new StringValueObjectJsonConverter<ThemePreviewSessionId>(value => new(value), value => value.Value));
        options.Converters.Add(new StringValueObjectJsonConverter<ThemeCorrelationId>(value => new(value), value => value.Value));
        return options;
    }

    private sealed class StringValueObjectJsonConverter<T>(
        Func<string, T> factory,
        Func<T, string> valueSelector) : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            factory(reader.GetString() ?? throw new JsonException($"{typeof(T).Name} must be a JSON string."));

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            writer.WriteStringValue(valueSelector(value));
    }
}
