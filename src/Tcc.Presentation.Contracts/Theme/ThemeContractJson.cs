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

        // Phase 4 integrity enums are authoritative security inputs. Register their
        // closed converters before the legacy catch-all converter so numeric and
        // unknown string values fail without changing sealed V1 enum handling.
        options.Converters.Add(new JsonStringEnumConverter<ThemeIntegrityInventoryMode>(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false));
        options.Converters.Add(new JsonStringEnumConverter<ThemeIntegrityEntryKind>(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false));
        options.Converters.Add(new JsonStringEnumConverter<ThemePackageContentEntryKind>(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false));
        options.Converters.Add(new JsonStringEnumConverter<ThemeSignatureAlgorithm>(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false));
        options.Converters.Add(new JsonStringEnumConverter<ThemeSignatureEncoding>(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false));
        options.Converters.Add(new JsonStringEnumConverter<ThemePublicKeyEncoding>(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false));
        options.Converters.Add(new JsonStringEnumConverter<ThemeSignedPayloadType>(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false));
        options.Converters.Add(new JsonStringEnumConverter<ThemeTrustState>(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false));
        options.Converters.Add(new JsonStringEnumConverter<ThemeDistributionChannel>(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false));
        options.Converters.Add(new JsonStringEnumConverter<ThemeSignatureRequirement>(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false));
        options.Converters.Add(new JsonStringEnumConverter<ThemeSignatureVerificationStatus>(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false));
        options.Converters.Add(new JsonStringEnumConverter<ThemeIntegrityEvidenceStatus>(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false));
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
        options.Converters.Add(new StringValueObjectJsonConverter<ThemePackageRef>(value => new(value), value => value.Value));
        options.Converters.Add(new StringValueObjectJsonConverter<ThemeCanonicalPath>(
            value => ThemeCanonicalPath.TryCreate(value, out ThemeCanonicalPath? path)
                ? path!
                : throw new JsonException("ThemeCanonicalPath must satisfy TCC Package Canonical Path v1."),
            value => value.Value));
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
