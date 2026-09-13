using System.Collections.Immutable;
using System.Text.Json;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Integrity;

namespace Tcc.Themes.Compatibility.Binding;

internal static class ThemeCompatibilityManifestMaterializer
{
    internal const string SchemaResourceName =
        "Tcc.Themes.Schemas.ThemeCompatibility.schema.json";

    internal static ThemeCompatibilityManifest? Materialize(
        ReadOnlyMemory<byte> bytes,
        out ImmutableArray<ThemeCompatibilityFailureV2> failures)
    {
        if (!TryParseStrict(bytes, out JsonDocument? document))
        {
            failures = ImmutableArray.Create(Failure(ThemeCompatibilityFailureKindV2.ManifestSchemaInvalid));
            return null;
        }

        using JsonDocument parsed = document!;
        if (parsed.RootElement.ValueKind != JsonValueKind.Object
            || !parsed.RootElement.TryGetProperty("schema_version", out JsonElement schemaVersion)
            || schemaVersion.ValueKind != JsonValueKind.String)
        {
            failures = ImmutableArray.Create(Failure(ThemeCompatibilityFailureKindV2.ManifestSchemaInvalid));
            return null;
        }

        if (!string.Equals(schemaVersion.GetString(), ContractVersions.Schema, StringComparison.Ordinal))
        {
            failures = ImmutableArray.Create(Failure(ThemeCompatibilityFailureKindV2.UnsupportedManifestSchema));
            return null;
        }

        if (!ThemeMetadataSchemaValidator.Validate(bytes, SchemaResourceName))
        {
            failures = ImmutableArray.Create(Failure(ThemeCompatibilityFailureKindV2.ManifestSchemaInvalid));
            return null;
        }

        try
        {
            ThemeCompatibilityManifest? manifest = JsonSerializer.Deserialize<ThemeCompatibilityManifest>(
                bytes.Span,
                ThemeContractJson.CreateSerializerOptions());
            if (manifest is null)
            {
                failures = ImmutableArray.Create(Failure(ThemeCompatibilityFailureKindV2.ManifestSchemaInvalid));
                return null;
            }

            failures = ImmutableArray<ThemeCompatibilityFailureV2>.Empty;
            return manifest;
        }
        catch (JsonException)
        {
            failures = ImmutableArray.Create(Failure(ThemeCompatibilityFailureKindV2.ManifestSchemaInvalid));
            return null;
        }
        catch (NotSupportedException)
        {
            failures = ImmutableArray.Create(Failure(ThemeCompatibilityFailureKindV2.ManifestSchemaInvalid));
            return null;
        }
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
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    scopes.Add(new HashSet<string>(StringComparer.Ordinal));
                }
                else if (reader.TokenType == JsonTokenType.StartArray)
                {
                    scopes.Add(null);
                }
                else if (reader.TokenType is JsonTokenType.EndObject or JsonTokenType.EndArray)
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

                    string? name;
                    try
                    {
                        name = reader.GetString();
                    }
                    catch (InvalidOperationException)
                    {
                        return false;
                    }

                    if (name is null || !names.Add(name))
                    {
                        return false;
                    }
                }
                else if (reader.TokenType == JsonTokenType.String)
                {
                    try
                    {
                        _ = reader.GetString();
                    }
                    catch (InvalidOperationException)
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

    private static ThemeCompatibilityFailureV2 Failure(ThemeCompatibilityFailureKindV2 kind) =>
        new(kind, ThemeCompatibilityDimensionV2.ManifestSchema, 0, null,
            $"{kind} in {ThemeCompatibilityDimensionV2.ManifestSchema}.");
}
