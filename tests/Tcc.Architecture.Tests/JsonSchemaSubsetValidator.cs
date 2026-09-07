using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Tcc.Architecture.Tests;

internal static class JsonSchemaSubsetValidator
{
    private static readonly HashSet<string> SupportedSchemaKeywords = new(StringComparer.Ordinal)
    {
        "$schema", "$id", "$ref", "$defs", "definitions", "title", "description", "default",
        "type", "required", "properties", "additionalProperties", "enum", "const", "pattern", "format",
        "items", "minItems", "maxItems", "uniqueItems", "minimum", "maximum", "minLength", "maxLength",
        "anyOf", "oneOf", "allOf",
    };

    public static IReadOnlyList<string> Validate(JsonElement schema, JsonElement instance)
    {
        AssertSchemaKeywordsSupported(schema);
        List<string> errors = [];
        ValidateNode(schema, schema, instance, "$", errors);
        return errors;
    }

    public static JsonNode CreateMinimumValidInstance(JsonElement schema)
    {
        AssertSchemaKeywordsSupported(schema);
        return CreateValidInstance(schema, schema, includeOptionalProperties: false);
    }

    public static JsonNode CreateCompleteValidInstance(JsonElement schema)
    {
        AssertSchemaKeywordsSupported(schema);
        return CreateValidInstance(schema, schema, includeOptionalProperties: true);
    }

    public static void AssertSchemaKeywordsSupported(JsonElement schema) =>
        AssertSchemaKeywordsSupported(schema, "$schema");

    private static JsonNode CreateValidInstance(
        JsonElement schema,
        JsonElement rootSchema,
        bool includeOptionalProperties)
    {
        if (schema.TryGetProperty("$ref", out JsonElement reference))
        {
            return CreateValidInstance(
                ResolveReference(rootSchema, reference.GetString()),
                rootSchema,
                includeOptionalProperties);
        }

        foreach (string combinator in new[] { "allOf", "anyOf", "oneOf" })
        {
            if (schema.TryGetProperty(combinator, out JsonElement alternatives)
                && alternatives.GetArrayLength() > 0)
            {
                return CreateValidInstance(alternatives[0], rootSchema, includeOptionalProperties);
            }
        }

        if (schema.TryGetProperty("const", out JsonElement constant))
        {
            return JsonNode.Parse(constant.GetRawText())
                ?? throw new InvalidOperationException("Schema const produced null JSON.");
        }

        if (schema.TryGetProperty("enum", out JsonElement enumValues)
            && enumValues.GetArrayLength() > 0)
        {
            return JsonNode.Parse(enumValues[0].GetRawText())
                ?? throw new InvalidOperationException("Schema enum produced null JSON.");
        }

        string type = GetPreferredType(schema);
        return type switch
        {
            "object" => CreateObject(schema, rootSchema, includeOptionalProperties),
            "array" => CreateArray(schema, rootSchema, includeOptionalProperties),
            "boolean" => JsonValue.Create(false),
            "integer" => JsonValue.Create(GetMinimum(schema)),
            "number" => JsonValue.Create(GetMinimum(schema)),
            "null" => JsonNode.Parse("null")!,
            _ => JsonValue.Create(CreateSampleString(schema)),
        };
    }

    private static JsonObject CreateObject(
        JsonElement schema,
        JsonElement rootSchema,
        bool includeOptionalProperties)
    {
        JsonObject result = [];
        if (!schema.TryGetProperty("properties", out JsonElement properties))
        {
            return result;
        }

        IEnumerable<string> propertyNames = includeOptionalProperties
            ? properties.EnumerateObject().Select(property => property.Name)
            : schema.TryGetProperty("required", out JsonElement required)
                ? required.EnumerateArray().Select(item => item.GetString()!)
                : [];

        foreach (string name in propertyNames)
        {
            result[name] = CreateValidInstance(
                properties.GetProperty(name),
                rootSchema,
                includeOptionalProperties);
        }

        return result;
    }

    private static JsonArray CreateArray(
        JsonElement schema,
        JsonElement rootSchema,
        bool includeOptionalProperties)
    {
        JsonArray result = [];
        int minimum = schema.TryGetProperty("minItems", out JsonElement minItems)
            ? minItems.GetInt32()
            : 0;
        if (includeOptionalProperties
            && (!schema.TryGetProperty("maxItems", out JsonElement maxItems) || maxItems.GetInt32() > 0))
        {
            minimum = Math.Max(minimum, 1);
        }
        JsonElement items = schema.TryGetProperty("items", out JsonElement itemSchema)
            ? itemSchema
            : default;

        for (int index = 0; index < minimum; index++)
        {
            if (items.ValueKind == JsonValueKind.Undefined)
            {
                result.Add(JsonValue.Create($"value-{index}"));
            }
            else if (schema.TryGetProperty("uniqueItems", out JsonElement unique)
                     && unique.ValueKind == JsonValueKind.True
                     && items.TryGetProperty("enum", out JsonElement enumValues)
                     && enumValues.GetArrayLength() > index)
            {
                result.Add(JsonNode.Parse(enumValues[index].GetRawText()));
            }
            else
            {
                result.Add(CreateValidInstance(items, rootSchema, includeOptionalProperties));
            }
        }

        return result;
    }

    private static void ValidateNode(
        JsonElement rootSchema,
        JsonElement schema,
        JsonElement instance,
        string path,
        ICollection<string> errors)
    {
        if (schema.TryGetProperty("$ref", out JsonElement reference))
        {
            ValidateNode(rootSchema, ResolveReference(rootSchema, reference.GetString()), instance, path, errors);
        }

        if (schema.TryGetProperty("allOf", out JsonElement allOf))
        {
            foreach (JsonElement branch in allOf.EnumerateArray())
            {
                ValidateNode(rootSchema, branch, instance, path, errors);
            }
        }

        if (schema.TryGetProperty("anyOf", out JsonElement anyOf)
            && CountMatchingBranches(rootSchema, anyOf, instance, path) == 0)
        {
            errors.Add($"{path}: value does not match anyOf.");
        }

        if (schema.TryGetProperty("oneOf", out JsonElement oneOf)
            && CountMatchingBranches(rootSchema, oneOf, instance, path) != 1)
        {
            errors.Add($"{path}: value does not match exactly one oneOf branch.");
        }

        if (schema.TryGetProperty("const", out JsonElement constant)
            && !JsonElement.DeepEquals(constant, instance))
        {
            errors.Add($"{path}: value does not match const.");
        }

        if (schema.TryGetProperty("enum", out JsonElement enumValues)
            && !enumValues.EnumerateArray().Any(value => JsonElement.DeepEquals(value, instance)))
        {
            errors.Add($"{path}: value is not in enum.");
        }

        if (!MatchesDeclaredType(schema, instance))
        {
            errors.Add($"{path}: type mismatch.");
            return;
        }

        switch (instance.ValueKind)
        {
            case JsonValueKind.Object:
                ValidateObject(rootSchema, schema, instance, path, errors);
                break;
            case JsonValueKind.Array:
                ValidateArray(rootSchema, schema, instance, path, errors);
                break;
            case JsonValueKind.String:
                ValidateString(schema, instance.GetString() ?? string.Empty, path, errors);
                break;
            case JsonValueKind.Number:
                ValidateNumber(schema, instance.GetDecimal(), path, errors);
                break;
        }
    }

    private static void ValidateObject(
        JsonElement rootSchema,
        JsonElement schema,
        JsonElement instance,
        string path,
        ICollection<string> errors)
    {
        if (schema.TryGetProperty("properties", out JsonElement properties))
        {
            _ = properties.ValueKind;
        }

        if (schema.TryGetProperty("required", out JsonElement required))
        {
            foreach (JsonElement requiredName in required.EnumerateArray())
            {
                string name = requiredName.GetString() ?? string.Empty;
                if (!instance.TryGetProperty(name, out _))
                {
                    errors.Add($"{path}: missing required property '{name}'.");
                }
            }
        }

        JsonElement additionalProperties = schema.TryGetProperty(
            "additionalProperties",
            out JsonElement additional)
            ? additional
            : default;

        foreach (JsonProperty property in instance.EnumerateObject())
        {
            if (properties.ValueKind == JsonValueKind.Object
                && properties.TryGetProperty(property.Name, out JsonElement propertySchema))
            {
                ValidateNode(rootSchema, propertySchema, property.Value, $"{path}.{property.Name}", errors);
                continue;
            }

            if (additionalProperties.ValueKind == JsonValueKind.False)
            {
                errors.Add($"{path}: unknown property '{property.Name}'.");
            }
            else if (additionalProperties.ValueKind == JsonValueKind.Object)
            {
                ValidateNode(rootSchema, additionalProperties, property.Value, $"{path}.{property.Name}", errors);
            }
        }
    }

    private static void ValidateArray(
        JsonElement rootSchema,
        JsonElement schema,
        JsonElement instance,
        string path,
        ICollection<string> errors)
    {
        int count = instance.GetArrayLength();
        if (schema.TryGetProperty("minItems", out JsonElement minimum)
            && count < minimum.GetInt32())
        {
            errors.Add($"{path}: array has fewer than {minimum.GetInt32()} items.");
        }

        if (schema.TryGetProperty("maxItems", out JsonElement maximum)
            && count > maximum.GetInt32())
        {
            errors.Add($"{path}: array has more than {maximum.GetInt32()} items.");
        }

        if (schema.TryGetProperty("uniqueItems", out JsonElement unique)
            && unique.ValueKind == JsonValueKind.True)
        {
            string[] values = instance.EnumerateArray().Select(item => item.GetRawText()).ToArray();
            if (values.Length != values.Distinct(StringComparer.Ordinal).Count())
            {
                errors.Add($"{path}: array items are not unique.");
            }
        }

        if (schema.TryGetProperty("items", out JsonElement itemSchema))
        {
            int index = 0;
            foreach (JsonElement item in instance.EnumerateArray())
            {
                ValidateNode(rootSchema, itemSchema, item, $"{path}[{index}]", errors);
                index++;
            }
        }
    }

    private static void ValidateString(
        JsonElement schema,
        string value,
        string path,
        ICollection<string> errors)
    {
        if (schema.TryGetProperty("minLength", out JsonElement minimum)
            && value.Length < minimum.GetInt32())
        {
            errors.Add($"{path}: string is shorter than {minimum.GetInt32()}.");
        }

        if (schema.TryGetProperty("maxLength", out JsonElement maximum)
            && value.Length > maximum.GetInt32())
        {
            errors.Add($"{path}: string is longer than {maximum.GetInt32()}.");
        }

        if (schema.TryGetProperty("pattern", out JsonElement pattern)
            && !Regex.IsMatch(value, pattern.GetString() ?? string.Empty, RegexOptions.CultureInvariant))
        {
            errors.Add($"{path}: string does not match pattern.");
        }

        if (schema.TryGetProperty("format", out JsonElement format)
            && format.GetString() == "date-time"
            && !DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _))
        {
            errors.Add($"{path}: string is not an RFC 3339 date-time.");
        }
    }

    private static void ValidateNumber(
        JsonElement schema,
        decimal value,
        string path,
        ICollection<string> errors)
    {
        if (schema.TryGetProperty("minimum", out JsonElement minimum)
            && value < minimum.GetDecimal())
        {
            errors.Add($"{path}: number is below minimum.");
        }

        if (schema.TryGetProperty("maximum", out JsonElement maximum)
            && value > maximum.GetDecimal())
        {
            errors.Add($"{path}: number is above maximum.");
        }
    }

    private static bool MatchesDeclaredType(JsonElement schema, JsonElement instance)
    {
        if (!schema.TryGetProperty("type", out JsonElement type))
        {
            return true;
        }

        return type.ValueKind switch
        {
            JsonValueKind.String => MatchesType(type.GetString() ?? string.Empty, instance),
            JsonValueKind.Array => type.EnumerateArray().Any(
                candidate => MatchesType(candidate.GetString() ?? string.Empty, instance)),
            _ => true,
        };
    }

    private static bool MatchesType(string type, JsonElement instance) => type switch
    {
        "object" => instance.ValueKind == JsonValueKind.Object,
        "array" => instance.ValueKind == JsonValueKind.Array,
        "string" => instance.ValueKind == JsonValueKind.String,
        "boolean" => instance.ValueKind is JsonValueKind.True or JsonValueKind.False,
        "number" => instance.ValueKind == JsonValueKind.Number,
        "integer" => instance.ValueKind == JsonValueKind.Number && instance.TryGetInt64(out _),
        "null" => instance.ValueKind == JsonValueKind.Null,
        _ => true,
    };

    private static string GetPreferredType(JsonElement schema)
    {
        if (!schema.TryGetProperty("type", out JsonElement type))
        {
            return "string";
        }

        return type.ValueKind == JsonValueKind.Array
            ? type.EnumerateArray().Select(item => item.GetString()).First(candidate => candidate != "null") ?? "string"
            : type.GetString() ?? "string";
    }

    private static decimal GetMinimum(JsonElement schema) =>
        schema.TryGetProperty("minimum", out JsonElement minimum) ? minimum.GetDecimal() : 0;

    private static string CreateSampleString(JsonElement schema)
    {
        string? pattern = schema.TryGetProperty("pattern", out JsonElement patternElement)
            ? patternElement.GetString()
            : null;
        string? format = schema.TryGetProperty("format", out JsonElement formatElement)
            ? formatElement.GetString()
            : null;

        if (format == "date-time") return "2026-09-07T00:00:00Z";
        if (pattern is null) return "value";
        if (pattern.Contains("{64}", StringComparison.Ordinal)) return new string('a', 64);
        if (pattern.Contains("UX-", StringComparison.Ordinal)) return "UX-HOME-001";
        if (pattern.Contains("STATE-", StringComparison.Ordinal)) return "STATE-NORMAL";
        if (pattern.Contains("ZONE-", StringComparison.Ordinal)) return "ZONE-SAFETY-CORE";
        if (pattern.Contains("WS-", StringComparison.Ordinal)) return "WS-TRADING";
        if (pattern.Contains("theme\\.", StringComparison.Ordinal)) return "theme.test";
        if (pattern.Contains("presentation\\.", StringComparison.Ordinal)) return "presentation.test";
        if (pattern.Contains("[0-9]+%", StringComparison.Ordinal)) return "100%";
        if (pattern.Contains("A-Za-z", StringComparison.Ordinal)) return "en-US";
        if (pattern.Contains("\\.", StringComparison.Ordinal) && pattern.Contains("[0-9]", StringComparison.Ordinal)) return "1.0.0";
        if (pattern.Contains("a-z0-9", StringComparison.Ordinal)) return "com.example.theme";
        return "file.json";
    }

    private static int CountMatchingBranches(
        JsonElement rootSchema,
        JsonElement branches,
        JsonElement instance,
        string path)
    {
        int matches = 0;
        foreach (JsonElement branch in branches.EnumerateArray())
        {
            List<string> branchErrors = [];
            ValidateNode(rootSchema, branch, instance, path, branchErrors);
            if (branchErrors.Count == 0)
            {
                matches++;
            }
        }

        return matches;
    }

    private static JsonElement ResolveReference(JsonElement rootSchema, string? reference)
    {
        if (reference is null || !reference.StartsWith("#/", StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Only local JSON Schema references are supported: '{reference}'.");
        }

        JsonElement current = rootSchema;
        foreach (string rawSegment in reference[2..].Split('/'))
        {
            string segment = rawSegment.Replace("~1", "/", StringComparison.Ordinal)
                .Replace("~0", "~", StringComparison.Ordinal);
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out JsonElement next))
            {
                throw new InvalidDataException($"JSON Schema reference could not be resolved: '{reference}'.");
            }

            current = next;
        }

        return current;
    }

    private static void AssertSchemaKeywordsSupported(JsonElement schema, string path)
    {
        if (schema.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidDataException($"{path}: schema node must be an object.");
        }

        foreach (JsonProperty keyword in schema.EnumerateObject())
        {
            if (!SupportedSchemaKeywords.Contains(keyword.Name))
            {
                throw new InvalidDataException($"{path}: unsupported JSON Schema keyword '{keyword.Name}'.");
            }

            switch (keyword.Name)
            {
                case "properties":
                case "$defs":
                case "definitions":
                    foreach (JsonProperty child in keyword.Value.EnumerateObject())
                    {
                        AssertSchemaKeywordsSupported(child.Value, $"{path}.{keyword.Name}.{child.Name}");
                    }

                    break;
                case "items":
                    AssertSchemaKeywordsSupported(keyword.Value, $"{path}.items");
                    break;
                case "additionalProperties" when keyword.Value.ValueKind == JsonValueKind.Object:
                    AssertSchemaKeywordsSupported(keyword.Value, $"{path}.additionalProperties");
                    break;
                case "anyOf":
                case "oneOf":
                case "allOf":
                    int index = 0;
                    foreach (JsonElement branch in keyword.Value.EnumerateArray())
                    {
                        AssertSchemaKeywordsSupported(branch, $"{path}.{keyword.Name}[{index}]");
                        index++;
                    }

                    break;
            }
        }
    }
}
