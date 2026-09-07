using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tcc.Architecture.Tests;

public sealed class PhaseTwoSchemaTests
{
    public static TheoryData<string> SchemaFiles => new(
        Directory
            .EnumerateFiles(Path.Combine(RepositoryPaths.ThemeContracts, "schemas"), "*.schema.json")
            .Select(Path.GetFileName)
            .Order(StringComparer.Ordinal)
            .ToArray()!);

    [Theory]
    [MemberData(nameof(SchemaFiles))]
    public void SchemaParsesAndDeclaresRequiredFields(string schemaFile)
    {
        using JsonDocument schema = ReadSchema(schemaFile);

        Assert.Equal("https://json-schema.org/draft/2020-12/schema", schema.RootElement.GetProperty("$schema").GetString());
        Assert.Equal("object", schema.RootElement.GetProperty("type").GetString());
        Assert.NotEmpty(schema.RootElement.GetProperty("required").EnumerateArray());
        Assert.Equal(JsonValueKind.Object, schema.RootElement.GetProperty("properties").ValueKind);
        JsonSchemaSubsetValidator.AssertSchemaKeywordsSupported(schema.RootElement);
    }

    [Theory]
    [MemberData(nameof(SchemaFiles))]
    public void MinimumValidDocumentPassesAndEveryMissingRequiredFieldFails(string schemaFile)
    {
        using JsonDocument schema = ReadSchema(schemaFile);
        JsonObject minimum = Assert.IsType<JsonObject>(
            JsonSchemaSubsetValidator.CreateMinimumValidInstance(schema.RootElement));

        using JsonDocument validDocument = JsonDocument.Parse(minimum.ToJsonString());
        Assert.Empty(JsonSchemaSubsetValidator.Validate(schema.RootElement, validDocument.RootElement));

        foreach (JsonElement requiredProperty in schema.RootElement.GetProperty("required").EnumerateArray())
        {
            string requiredName = requiredProperty.GetString()!;
            JsonObject mutation = Assert.IsType<JsonObject>(JsonNode.Parse(minimum.ToJsonString()));
            Assert.True(mutation.Remove(requiredName));

            using JsonDocument invalidDocument = JsonDocument.Parse(mutation.ToJsonString());
            Assert.Contains(
                JsonSchemaSubsetValidator.Validate(schema.RootElement, invalidDocument.RootElement),
                error => error.Contains($"missing required property '{requiredName}'", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void GeneratedPageContractsValidateAgainstUxSurfaceSchema()
    {
        AssertGeneratedDocumentsValidate("UxSurfaceContract.schema.json", "page-contracts");
    }

    [Fact]
    public void GeneratedZoneContractsValidateAgainstFunctionalZoneSchema()
    {
        AssertGeneratedDocumentsValidate("FunctionalZoneContract.schema.json", "zone-contracts");
    }

    [Fact]
    public void GeneratedStateContractsValidateAgainstGlobalStateSchema()
    {
        AssertGeneratedDocumentsValidate("GlobalStatePresentation.schema.json", "state-contracts");
    }

    [Fact]
    public void ForbiddenAndUnknownCapabilitiesFailManifestSchema()
    {
        using JsonDocument schema = ReadSchema("ThemeManifest.schema.json");
        JsonObject manifest = Assert.IsType<JsonObject>(
            JsonSchemaSubsetValidator.CreateMinimumValidInstance(schema.RootElement));
        manifest["capabilities"] = new JsonArray("broker.order");

        using JsonDocument invalidDocument = JsonDocument.Parse(manifest.ToJsonString());
        Assert.Contains(
            JsonSchemaSubsetValidator.Validate(schema.RootElement, invalidDocument.RootElement),
            error => error.Contains("not in enum", StringComparison.Ordinal));
    }

    [Fact]
    public void AccessibilityFieldsAreRequiredAndFailClosed()
    {
        using JsonDocument schema = ReadSchema("ThemeManifest.schema.json");
        JsonObject manifest = Assert.IsType<JsonObject>(
            JsonSchemaSubsetValidator.CreateMinimumValidInstance(schema.RootElement));
        JsonObject accessibility = Assert.IsType<JsonObject>(manifest["accessibility"]);
        accessibility["reduced_motion_supported"] = false;

        using JsonDocument invalidDocument = JsonDocument.Parse(manifest.ToJsonString());
        Assert.Contains(
            JsonSchemaSubsetValidator.Validate(schema.RootElement, invalidDocument.RootElement),
            error => error.Contains("const", StringComparison.Ordinal));
    }

    [Fact]
    public void ThemeApiVersionMutationFailsActualManifestSchema()
    {
        using JsonDocument schema = ReadSchema("ThemeManifest.schema.json");
        JsonObject manifest = Assert.IsType<JsonObject>(
            JsonSchemaSubsetValidator.CreateMinimumValidInstance(schema.RootElement));
        manifest["theme_api_version"] = "2.0.0";

        using JsonDocument mutation = JsonDocument.Parse(manifest.ToJsonString());
        Assert.NotEmpty(JsonSchemaSubsetValidator.Validate(schema.RootElement, mutation.RootElement));
    }

    [Fact]
    public void InventedSurfaceAndStateMutationsFailActualUxSchema()
    {
        using JsonDocument schema = ReadSchema("UxSurfaceContract.schema.json");
        JsonObject page = JsonNode.Parse(File.ReadAllText(Path.Combine(
            RepositoryPaths.ThemeContracts,
            "page-contracts",
            "UX-HOME-001.json")))!.AsObject();

        page["surface_id"] = "UX-INVENTED-999";
        using (JsonDocument surfaceMutation = JsonDocument.Parse(page.ToJsonString()))
        {
            Assert.NotEmpty(JsonSchemaSubsetValidator.Validate(schema.RootElement, surfaceMutation.RootElement));
        }

        page["surface_id"] = "UX-HOME-001";
        page["supported_states"] = new JsonArray("STATE-INVENTED");
        using JsonDocument stateMutation = JsonDocument.Parse(page.ToJsonString());
        Assert.NotEmpty(JsonSchemaSubsetValidator.Validate(schema.RootElement, stateMutation.RootElement));
    }

    [Fact]
    public void ApprovedCustomWorkspacePatternPassesAndInventedWorkspaceFailsActualRuntimeSchema()
    {
        using JsonDocument schema = ReadSchema("ThemeRuntimeState.schema.json");
        JsonElement workspaceSchema = schema.RootElement.GetProperty("properties").GetProperty("workspace_state");
        JsonObject workspace = Assert.IsType<JsonObject>(
            JsonSchemaSubsetValidator.CreateMinimumValidInstance(workspaceSchema));

        workspace["workspace_id"] = "WS-CUSTOM-TRADING-DESK";
        using (JsonDocument approved = JsonDocument.Parse(workspace.ToJsonString()))
        {
            Assert.Empty(JsonSchemaSubsetValidator.Validate(workspaceSchema, approved.RootElement));
        }

        workspace["workspace_id"] = "WS-INVENTED";
        using JsonDocument invented = JsonDocument.Parse(workspace.ToJsonString());
        Assert.NotEmpty(JsonSchemaSubsetValidator.Validate(workspaceSchema, invented.RootElement));
    }

    [Fact]
    public void UnsupportedSchemaKeywordFailsExplicitly()
    {
        using JsonDocument schema = JsonDocument.Parse("""{"type":"string","not":{"const":"forbidden"}}""");
        using JsonDocument instance = JsonDocument.Parse("\"value\"");

        InvalidDataException error = Assert.Throws<InvalidDataException>(
            () => JsonSchemaSubsetValidator.Validate(schema.RootElement, instance.RootElement));

        Assert.Contains("unsupported JSON Schema keyword 'not'", error.Message, StringComparison.Ordinal);
    }

    private static void AssertGeneratedDocumentsValidate(string schemaFile, string directory)
    {
        using JsonDocument schema = ReadSchema(schemaFile);
        string[] documents = Directory
            .EnumerateFiles(Path.Combine(RepositoryPaths.ThemeContracts, directory), "*.json")
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(documents);
        foreach (string path in documents)
        {
            using JsonDocument instance = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Empty(JsonSchemaSubsetValidator.Validate(schema.RootElement, instance.RootElement));
        }
    }

    private static JsonDocument ReadSchema(string schemaFile) =>
        JsonDocument.Parse(File.ReadAllText(Path.Combine(RepositoryPaths.ThemeContracts, "schemas", schemaFile)));
}
