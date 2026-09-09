using System.Numerics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Tcc.Themes.Integrity;

/// <summary>
/// Executes the exact embedded Theme metadata schemas with the constrained BCL-only
/// JSON Schema vocabulary sealed for Phase 4C.
/// </summary>
internal static class ThemeMetadataSchemaValidator
{
    internal const string ThemeManifestSchemaResource =
        "Tcc.Themes.Schemas.ThemeManifest.schema.json";
    internal const string IntegrityManifestSchemaResource =
        "Tcc.Themes.Schemas.ThemeIntegrity.v2.schema.json";
    internal const string SignatureEnvelopeSchemaResource =
        "Tcc.Themes.Schemas.ThemeSignatureEnvelope.v1.schema.json";

    private const string ExpectedDialect = "https://json-schema.org/draft/2020-12/schema";
    private static readonly TimeSpan PatternTimeout = TimeSpan.FromMilliseconds(250);

    internal static bool Validate(ReadOnlyMemory<byte> documentBytes, string schemaResourceName)
    {
        byte[] schemaBytes = ReadSchemaResource(schemaResourceName);
        return ValidateAgainstSchemaBytes(documentBytes, schemaBytes);
    }

    internal static bool ValidateAgainstSchemaBytes(
        ReadOnlyMemory<byte> documentBytes,
        ReadOnlyMemory<byte> schemaBytes)
    {
        if (!TryParseStrict(schemaBytes, out JsonDocument? schema))
        {
            throw new InvalidOperationException("The authoritative embedded metadata schema is malformed.");
        }

        using JsonDocument parsedSchema = schema!;
        EnsureSupportedSchema(parsedSchema.RootElement, true);

        if (!TryParseStrict(documentBytes, out JsonDocument? document))
        {
            return false;
        }

        using JsonDocument parsedDocument = document!;
        return ValidateNode(parsedDocument.RootElement, parsedSchema.RootElement);
    }

    internal static byte[] ReadSchemaResource(string schemaResourceName)
    {
        using Stream? stream = typeof(ThemeMetadataSchemaValidator).Assembly
            .GetManifestResourceStream(schemaResourceName);
        if (stream is null)
        {
            throw new InvalidOperationException(
                $"The authoritative embedded metadata schema '{schemaResourceName}' is unavailable.");
        }

        using MemoryStream buffer = new();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    internal static IReadOnlySet<string> ReadRootArrayItemStringEnum(
        string schemaResourceName,
        string propertyName)
    {
        byte[] schemaBytes = ReadSchemaResource(schemaResourceName);
        if (!TryParseStrict(schemaBytes, out JsonDocument? schema))
        {
            throw new InvalidOperationException("The authoritative embedded metadata schema is malformed.");
        }

        using JsonDocument parsedSchema = schema!;
        EnsureSupportedSchema(parsedSchema.RootElement, true);
        JsonElement values;
        try
        {
            values = parsedSchema.RootElement.GetProperty("properties")
                .GetProperty(propertyName)
                .GetProperty("items")
                .GetProperty("enum");
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOperationException(
                "The authoritative embedded metadata schema has an invalid enum shape.", exception);
        }
        catch (KeyNotFoundException exception)
        {
            throw new InvalidOperationException(
                "The authoritative embedded metadata schema is missing its required enum.", exception);
        }

        HashSet<string> result = new(StringComparer.Ordinal);
        if (values.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                "The authoritative embedded metadata schema has an invalid enum shape.");
        }

        foreach (JsonElement value in values.EnumerateArray())
        {
            if (value.ValueKind != JsonValueKind.String || !result.Add(value.GetString()!))
            {
                throw new InvalidOperationException(
                    "The authoritative embedded metadata schema has an invalid string enum.");
            }
        }

        return result;
    }

    private static bool TryParseStrict(ReadOnlyMemory<byte> bytes, out JsonDocument? document)
    {
        document = null;
        try
        {
            Utf8JsonReader reader = new(
                bytes.Span,
                new JsonReaderOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = 128,
                });
            List<HashSet<string>?> scopes = [];
            while (reader.Read())
            {
                string? decoded = null;
                if (reader.TokenType is JsonTokenType.PropertyName or JsonTokenType.String)
                {
                    // Only decode the current untrusted string token inside this boundary.
                    // GetString rejects malformed UTF-8 and escaped unpaired surrogates;
                    // schema/deployment and unrelated programmer faults remain outside it.
                    try
                    {
                        decoded = reader.GetString();
                    }
                    catch (InvalidOperationException)
                    {
                        return false;
                    }
                }

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    scopes.Add(new HashSet<string>(StringComparer.Ordinal));
                }
                else if (reader.TokenType == JsonTokenType.StartArray)
                {
                    scopes.Add(null);
                }
                else if (reader.TokenType == JsonTokenType.EndObject
                         || reader.TokenType == JsonTokenType.EndArray)
                {
                    if (scopes.Count == 0)
                    {
                        return false;
                    }

                    scopes.RemoveAt(scopes.Count - 1);
                }
                else if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    if (scopes.Count == 0 || scopes[^1] is not { } names)
                    {
                        return false;
                    }

                    if (!names.Add(decoded!))
                    {
                        return false;
                    }
                }
            }

            if (scopes.Count != 0)
            {
                return false;
            }

            document = JsonDocument.Parse(
                bytes,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = 128,
                });
            return true;
        }
        catch (JsonException)
        {
            document?.Dispose();
            document = null;
            return false;
        }
        catch (ArgumentException)
        {
            document?.Dispose();
            document = null;
            return false;
        }
    }

    private static void EnsureSupportedSchema(JsonElement schema, bool isRoot)
    {
        if (schema.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Every authoritative schema node must be an object.");
        }

        foreach (JsonProperty keyword in schema.EnumerateObject())
        {
            switch (keyword.Name)
            {
                case "$schema":
                    if (!isRoot
                        || keyword.Value.ValueKind != JsonValueKind.String
                        || !string.Equals(keyword.Value.GetString(), ExpectedDialect, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException("The authoritative schema dialect is unsupported.");
                    }

                    break;
                case "$id":
                case "title":
                case "description":
                    if (keyword.Value.ValueKind != JsonValueKind.String)
                    {
                        throw new InvalidOperationException($"Schema annotation '{keyword.Name}' must be a string.");
                    }

                    break;
                case "type":
                    EnsureTypeKeyword(keyword.Value);
                    break;
                case "properties":
                    if (keyword.Value.ValueKind != JsonValueKind.Object)
                    {
                        throw new InvalidOperationException("Schema properties must be an object.");
                    }

                    foreach (JsonProperty propertySchema in keyword.Value.EnumerateObject())
                    {
                        EnsureSupportedSchema(propertySchema.Value, false);
                    }

                    break;
                case "items":
                    EnsureSupportedSchema(keyword.Value, false);
                    break;
                case "required":
                    if (keyword.Value.ValueKind != JsonValueKind.Array)
                    {
                        throw new InvalidOperationException("Schema required must be an array of unique strings.");
                    }

                    HashSet<string> requiredNames = new(StringComparer.Ordinal);
                    foreach (JsonElement requiredName in keyword.Value.EnumerateArray())
                    {
                        if (requiredName.ValueKind != JsonValueKind.String
                            || !requiredNames.Add(requiredName.GetString()!))
                        {
                            throw new InvalidOperationException("Schema required must be an array of unique strings.");
                        }
                    }

                    break;
                case "enum":
                    if (keyword.Value.ValueKind != JsonValueKind.Array)
                    {
                        throw new InvalidOperationException($"Schema keyword '{keyword.Name}' must be an array.");
                    }

                    break;
                case "additionalProperties":
                case "uniqueItems":
                    if (keyword.Value.ValueKind is not JsonValueKind.True and not JsonValueKind.False)
                    {
                        throw new InvalidOperationException($"Schema keyword '{keyword.Name}' must be boolean.");
                    }

                    break;
                case "minItems":
                case "minLength":
                case "maxLength":
                    if (keyword.Value.ValueKind != JsonValueKind.Number
                        || !keyword.Value.TryGetInt32(out int integer)
                        || integer < 0)
                    {
                        throw new InvalidOperationException($"Schema keyword '{keyword.Name}' must be a nonnegative integer.");
                    }

                    break;
                case "pattern":
                    if (keyword.Value.ValueKind != JsonValueKind.String)
                    {
                        throw new InvalidOperationException("Schema pattern must be a string.");
                    }

                    try
                    {
                        _ = new Regex(
                            keyword.Value.GetString()!,
                            RegexOptions.CultureInvariant,
                            PatternTimeout);
                    }
                    catch (ArgumentException exception)
                    {
                        throw new InvalidOperationException("The authoritative schema pattern is malformed.", exception);
                    }

                    break;
                case "minimum":
                    if (keyword.Value.ValueKind != JsonValueKind.Number
                        || !TryParseNumber(keyword.Value.GetRawText(), out _, out _, out _))
                    {
                        throw new InvalidOperationException("Schema minimum must be an exact JSON number.");
                    }

                    break;
                case "const":
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Schema keyword '{keyword.Name}' is not supported by the Phase 4C runtime.");
            }
        }
    }

    private static void EnsureTypeKeyword(JsonElement type)
    {
        if (type.ValueKind == JsonValueKind.String)
        {
            EnsureSupportedType(type.GetString());
            return;
        }

        if (type.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Schema type must be a string or string array.");
        }

        HashSet<string> types = new(StringComparer.Ordinal);
        foreach (JsonElement item in type.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String
                || !types.Add(item.GetString()!))
            {
                throw new InvalidOperationException("Schema type union is malformed.");
            }

            EnsureSupportedType(item.GetString());
        }

        if (types.Count != 2 || !types.Contains("string") || !types.Contains("null"))
        {
            throw new InvalidOperationException("Only the sealed string/null type union is supported.");
        }
    }

    private static void EnsureSupportedType(string? type)
    {
        if (type is not ("object" or "array" or "string" or "boolean" or "integer" or "number" or "null"))
        {
            throw new InvalidOperationException($"Schema type '{type}' is unsupported.");
        }
    }

    private static bool ValidateNode(JsonElement instance, JsonElement schema)
    {
        if (schema.TryGetProperty("type", out JsonElement type)
            && !MatchesType(instance, type))
        {
            return false;
        }

        if (schema.TryGetProperty("const", out JsonElement constant)
            && !JsonEquals(instance, constant))
        {
            return false;
        }

        if (schema.TryGetProperty("enum", out JsonElement enumeration))
        {
            bool matched = false;
            foreach (JsonElement candidate in enumeration.EnumerateArray())
            {
                if (JsonEquals(instance, candidate))
                {
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                return false;
            }
        }

        if (instance.ValueKind == JsonValueKind.Object
            && !ValidateObject(instance, schema))
        {
            return false;
        }

        if (instance.ValueKind == JsonValueKind.Array
            && !ValidateArray(instance, schema))
        {
            return false;
        }

        if (instance.ValueKind == JsonValueKind.String
            && !ValidateString(instance.GetString()!, schema))
        {
            return false;
        }

        return instance.ValueKind != JsonValueKind.Number
            || !schema.TryGetProperty("minimum", out JsonElement minimum)
            || CompareNumbers(instance.GetRawText(), minimum.GetRawText()) >= 0;
    }

    private static bool ValidateObject(JsonElement instance, JsonElement schema)
    {
        if (schema.TryGetProperty("required", out JsonElement required))
        {
            foreach (JsonElement requiredName in required.EnumerateArray())
            {
                if (requiredName.ValueKind != JsonValueKind.String
                    || !instance.TryGetProperty(requiredName.GetString()!, out _))
                {
                    return false;
                }
            }
        }

        bool hasProperties = schema.TryGetProperty("properties", out JsonElement properties);
        bool rejectAdditional = schema.TryGetProperty("additionalProperties", out JsonElement additional)
            && additional.ValueKind == JsonValueKind.False;
        foreach (JsonProperty property in instance.EnumerateObject())
        {
            if (hasProperties && properties.TryGetProperty(property.Name, out JsonElement propertySchema))
            {
                if (!ValidateNode(property.Value, propertySchema))
                {
                    return false;
                }
            }
            else if (rejectAdditional)
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValidateArray(JsonElement instance, JsonElement schema)
    {
        int length = instance.GetArrayLength();
        if (schema.TryGetProperty("minItems", out JsonElement minimumItems)
            && length < minimumItems.GetInt32())
        {
            return false;
        }

        if (schema.TryGetProperty("items", out JsonElement itemSchema))
        {
            foreach (JsonElement item in instance.EnumerateArray())
            {
                if (!ValidateNode(item, itemSchema))
                {
                    return false;
                }
            }
        }

        if (schema.TryGetProperty("uniqueItems", out JsonElement unique)
            && unique.ValueKind == JsonValueKind.True)
        {
            for (int left = 0; left < length; left++)
            {
                for (int right = left + 1; right < length; right++)
                {
                    if (JsonEquals(instance[left], instance[right]))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private static bool ValidateString(string value, JsonElement schema)
    {
        int scalarCount = 0;
        foreach (System.Text.Rune _ in value.EnumerateRunes())
        {
            scalarCount++;
        }

        if (schema.TryGetProperty("minLength", out JsonElement minimumLength)
            && scalarCount < minimumLength.GetInt32())
        {
            return false;
        }

        if (schema.TryGetProperty("maxLength", out JsonElement maximumLength)
            && scalarCount > maximumLength.GetInt32())
        {
            return false;
        }

        if (schema.TryGetProperty("pattern", out JsonElement pattern))
        {
            try
            {
                return Regex.IsMatch(
                    value,
                    pattern.GetString()!,
                    RegexOptions.CultureInvariant,
                    PatternTimeout);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchesType(JsonElement instance, JsonElement type)
    {
        if (type.ValueKind == JsonValueKind.String)
        {
            return MatchesSingleType(instance, type.GetString()!);
        }

        foreach (JsonElement candidate in type.EnumerateArray())
        {
            if (MatchesSingleType(instance, candidate.GetString()!))
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesSingleType(JsonElement instance, string type) => type switch
    {
        "object" => instance.ValueKind == JsonValueKind.Object,
        "array" => instance.ValueKind == JsonValueKind.Array,
        "string" => instance.ValueKind == JsonValueKind.String,
        "boolean" => instance.ValueKind is JsonValueKind.True or JsonValueKind.False,
        "integer" => instance.ValueKind == JsonValueKind.Number && IsInteger(instance.GetRawText()),
        "number" => instance.ValueKind == JsonValueKind.Number,
        "null" => instance.ValueKind == JsonValueKind.Null,
        _ => false,
    };

    private static bool JsonEquals(JsonElement left, JsonElement right)
    {
        if (left.ValueKind == JsonValueKind.Number && right.ValueKind == JsonValueKind.Number)
        {
            return CompareNumbers(left.GetRawText(), right.GetRawText()) == 0;
        }

        if (left.ValueKind != right.ValueKind)
        {
            return false;
        }

        switch (left.ValueKind)
        {
            case JsonValueKind.Object:
                int leftCount = 0;
                foreach (JsonProperty property in left.EnumerateObject())
                {
                    leftCount++;
                    if (!right.TryGetProperty(property.Name, out JsonElement other)
                        || !JsonEquals(property.Value, other))
                    {
                        return false;
                    }
                }

                int rightCount = 0;
                foreach (JsonProperty _ in right.EnumerateObject())
                {
                    rightCount++;
                }

                return leftCount == rightCount;
            case JsonValueKind.Array:
                if (left.GetArrayLength() != right.GetArrayLength())
                {
                    return false;
                }

                for (int index = 0; index < left.GetArrayLength(); index++)
                {
                    if (!JsonEquals(left[index], right[index]))
                    {
                        return false;
                    }
                }

                return true;
            case JsonValueKind.String:
                return string.Equals(left.GetString(), right.GetString(), StringComparison.Ordinal);
            case JsonValueKind.True:
            case JsonValueKind.False:
                return left.GetBoolean() == right.GetBoolean();
            case JsonValueKind.Null:
                return true;
            default:
                return false;
        }
    }

    private static bool IsInteger(string raw)
    {
        if (!TryParseNumber(raw, out _, out string digits, out BigInteger exponent))
        {
            return false;
        }

        return digits == "0" || exponent >= BigInteger.Zero;
    }

    private static int CompareNumbers(string left, string right)
    {
        if (!TryParseNumber(left, out bool leftNegative, out string leftDigits, out BigInteger leftExponent)
            || !TryParseNumber(right, out bool rightNegative, out string rightDigits, out BigInteger rightExponent))
        {
            throw new InvalidOperationException("A JSON number could not be compared exactly.");
        }

        if (leftDigits == "0" && rightDigits == "0")
        {
            return 0;
        }

        if (leftDigits == "0")
        {
            return rightNegative ? 1 : -1;
        }

        if (rightDigits == "0")
        {
            return leftNegative ? -1 : 1;
        }

        if (leftNegative != rightNegative)
        {
            return leftNegative ? -1 : 1;
        }

        BigInteger leftOrder = leftExponent + leftDigits.Length;
        BigInteger rightOrder = rightExponent + rightDigits.Length;
        int magnitude = leftOrder.CompareTo(rightOrder);
        if (magnitude == 0)
        {
            int width = Math.Max(leftDigits.Length, rightDigits.Length);
            for (int index = 0; index < width; index++)
            {
                char leftDigit = index < leftDigits.Length ? leftDigits[index] : '0';
                char rightDigit = index < rightDigits.Length ? rightDigits[index] : '0';
                if (leftDigit != rightDigit)
                {
                    magnitude = leftDigit.CompareTo(rightDigit);
                    break;
                }
            }
        }

        return leftNegative ? -magnitude : magnitude;
    }

    private static bool TryParseNumber(
        string raw,
        out bool negative,
        out string significantDigits,
        out BigInteger exponent)
    {
        negative = false;
        significantDigits = string.Empty;
        exponent = BigInteger.Zero;
        if (string.IsNullOrEmpty(raw))
        {
            return false;
        }

        int index = 0;
        if (raw[index] == '-')
        {
            negative = true;
            index++;
        }

        int integerStart = index;
        while (index < raw.Length && raw[index] is >= '0' and <= '9')
        {
            index++;
        }

        if (index == integerStart)
        {
            return false;
        }

        string integer = raw[integerStart..index];
        string fraction = string.Empty;
        if (index < raw.Length && raw[index] == '.')
        {
            int fractionStart = ++index;
            while (index < raw.Length && raw[index] is >= '0' and <= '9')
            {
                index++;
            }

            if (index == fractionStart)
            {
                return false;
            }

            fraction = raw[fractionStart..index];
        }

        BigInteger explicitExponent = BigInteger.Zero;
        if (index < raw.Length && raw[index] is 'e' or 'E')
        {
            int exponentStart = ++index;
            if (index < raw.Length && raw[index] is '+' or '-')
            {
                index++;
            }

            int exponentDigitsStart = index;
            while (index < raw.Length && raw[index] is >= '0' and <= '9')
            {
                index++;
            }

            if (index == exponentDigitsStart
                || !BigInteger.TryParse(
                    raw[exponentStart..index],
                    System.Globalization.NumberStyles.AllowLeadingSign,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out explicitExponent))
            {
                return false;
            }
        }

        if (index != raw.Length)
        {
            return false;
        }

        string digits = integer + fraction;
        int firstNonZero = 0;
        while (firstNonZero < digits.Length && digits[firstNonZero] == '0')
        {
            firstNonZero++;
        }

        if (firstNonZero == digits.Length)
        {
            negative = false;
            significantDigits = "0";
            exponent = BigInteger.Zero;
            return true;
        }

        digits = digits[firstNonZero..];
        exponent = explicitExponent - fraction.Length;
        int trim = digits.Length;
        while (trim > 1 && digits[trim - 1] == '0')
        {
            trim--;
            exponent++;
        }

        significantDigits = digits[..trim];
        return true;
    }
}
