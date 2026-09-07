using System.Text.Json;
using System.Text.Json.Serialization;
using Tcc.Presentation.Contracts.Theme;

namespace Tcc.Architecture.Tests;

public sealed class PhaseTwoSerializationTests
{
    public static TheoryData<string, Type> DtoSchemaPairs => new()
    {
        { "ThemeManifest.schema.json", typeof(ThemeManifest) },
        { "ThemeIntegrity.schema.json", typeof(ThemeIntegrityManifest) },
        { "ThemeCompatibility.schema.json", typeof(ThemeCompatibilityManifest) },
        { "ThemeRollback.schema.json", typeof(ThemeRollbackManifest) },
        { "ThemeAssets.schema.json", typeof(ThemeAssetInventory) },
        { "ThemeTokens.schema.json", typeof(ThemeTokenBundle) },
        { "ThemeLayoutAdapter.schema.json", typeof(ThemeLayoutAdapter) },
        { "ThemeComponentAdapter.schema.json", typeof(ThemeComponentAdapter) },
        { "ThemeCopyResources.schema.json", typeof(ThemeCopyResources) },
        { "ThemeCriticalCopyRules.schema.json", typeof(ThemeCriticalCopyRules) },
        { "ThemeFocusStyles.schema.json", typeof(ThemeFocusStyles) },
        { "ThemeStatePresentation.schema.json", typeof(ThemeStatePresentation) },
        { "ThemeMotionProfile.schema.json", typeof(ThemeMotionProfile) },
        { "ThemeSoundPack.schema.json", typeof(ThemeAudioSoundPack) },
        { "ThemePersonalizationSafeRanges.schema.json", typeof(ThemePersonalizationSafeRanges) },
        { "ThemeRuntimeState.schema.json", typeof(ThemeRuntimeState) },
        { "ThemeDiagnosticsEvent.schema.json", typeof(ThemeDiagnosticsEvent) },
        { "CopyFallbackResult.schema.json", typeof(CopyFallbackResult) },
    };

    [Theory]
    [MemberData(nameof(DtoSchemaPairs))]
    public void EveryDtoSchemaCounterpartPreservesSchemaValidWireShape(string schemaFile, Type dtoType)
    {
        using JsonDocument schema = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            RepositoryPaths.ThemeContracts,
            "schemas",
            schemaFile)));
        JsonSerializerOptions options = ThemeContractJson.CreateSerializerOptions();
        AssertDtoMatchesSchema(dtoType, schema.RootElement, "$", new HashSet<string>(StringComparer.Ordinal));

        AssertSchemaGeneratedWireShapeRoundTrips(
            JsonSchemaSubsetValidator.CreateMinimumValidInstance(schema.RootElement).ToJsonString(),
            schema.RootElement,
            dtoType,
            options);
        AssertSchemaGeneratedWireShapeRoundTrips(
            JsonSchemaSubsetValidator.CreateCompleteValidInstance(schema.RootElement).ToJsonString(),
            schema.RootElement,
            dtoType,
            options);
    }

    [Fact]
    public void ThemeManifestRoundTripsThroughActualSchema()
    {
        ThemeManifest value = new(
            "1.0",
            "1.0.0",
            new ThemePackageIdentity(
                "com.example.theme", "com.example.theme", "Example", "Example Publisher", "example-publisher",
                "1.0.0", "stable", "Example theme", null, null, "private", ["deep"]),
            new ThemeCompatibilityDeclaration(
                ">=1.0.0 <2.0.0", ">=1.0.0 <2.0.0", ">=1.1.0 <2.0.0", ["windows"], true, 1m, 3m),
            [new ThemeVariant("deep", "deep", "Deep", true, "tokens/deep.json", [], null)],
            ["presentation.tokens"],
            new ThemeFeatureFlags([]),
            new ThemeDegradedModeDeclaration(
                true,
                ["none", "reduced_decoration", "minimal_decoration", "safe_presentation_only"],
                ["safety_core"],
                "safe_presentation_only"),
            new ThemeAccessibilityDeclaration(true, true, true, true, true, true, true, true, true, true, true, true),
            new ThemeAssetDeclaration("assets/index.json", ["tier0"], 0, "sha256"),
            new ThemeAudioDeclaration("audio/sound-pack.json", "audio/bgm.json", "audio/ambient.json", true, true, true, true),
            new ThemeMotionDeclaration("motion/profiles.json", "motion/reduced.json", true, true, true),
            new ThemePersonalizationDeclaration(
                "personalization/presets.json", "personalization/ranges.json", "personalization/migrations.json", true),
            new ThemeIntegrityDeclaration("integrity.json", null, "stable_or_store", "sha256"),
            new ThemeRollbackDeclaration("rollback.json", false, []));

        AssertSchemaRoundTrip(value, "ThemeManifest.schema.json");
    }

    [Fact]
    public void ThemeRuntimeStateRoundTripsThroughActualSchema()
    {
        ThemeRuntimeState value = new(
            "1.0",
            "state-001",
            "user-001",
            "device-001",
            "com.example.theme",
            "1.0.0",
            "deep",
            new ThemeWorkspaceState("WS-TRADING", "layout-001", "monitor-001", new ThemeWindowBounds(0, 0, 1600, 1000, 1.5m)),
            new ThemePersonalizationState("default", new Dictionary<string, JsonElement>()),
            new ThemeAudioState(
                true,
                1m,
                true,
                0.7m,
                true,
                0.7m,
                true,
                0.7m,
                true,
                0.7m,
                false,
                true),
            new ThemeMotionState(0.5m, true),
            new ThemeAutomationState(true, true, "default"),
            new DateTimeOffset(2026, 9, 7, 0, 0, 0, TimeSpan.Zero));

        AssertSchemaRoundTrip(value, "ThemeRuntimeState.schema.json");
    }

    [Fact]
    public void CopyFallbackResultRoundTripsThroughActualSchema()
    {
        CopyFallbackResult value = new(
            "1.0",
            "copy.permission.blocked",
            "en-US",
            "Trading is blocked.",
            "core_fallback",
            true,
            true,
            true);

        AssertSchemaRoundTrip(value, "CopyFallbackResult.schema.json");
    }

    [Fact]
    public void ThemeApplyResultRoundTripsWithDeterministicSnakeCaseJson()
    {
        ThemeApplyResult value = new(
            ThemeApplyOutcome.Applied,
            new ThemeId("com.example.theme"),
            new ThemeVariantId("deep"),
            ThemeCompatibilityStatus.CompatibleWithDegradation,
            ThemeAccessibilityStatus.Validated,
            ["diag-001"],
            true);
        JsonSerializerOptions options = ThemeContractJson.CreateSerializerOptions();

        string first = JsonSerializer.Serialize(value, options);
        string second = JsonSerializer.Serialize(value, options);
        ThemeApplyResult? roundTripped = JsonSerializer.Deserialize<ThemeApplyResult>(first, options);

        Assert.Equal(first, second);
        Assert.Contains("\"result\":\"applied\"", first, StringComparison.Ordinal);
        Assert.Contains("\"compatibility_status\":\"compatible_with_degradation\"", first, StringComparison.Ordinal);
        Assert.NotNull(roundTripped);
        Assert.Equal(value.Result, roundTripped.Result);
        Assert.Equal(value.ActiveThemeId, roundTripped.ActiveThemeId);
        Assert.Equal(value.ActiveVariantId, roundTripped.ActiveVariantId);
        Assert.Equal(value.CompatibilityStatus, roundTripped.CompatibilityStatus);
        Assert.Equal(value.AccessibilityStatus, roundTripped.AccessibilityStatus);
        Assert.Equal(value.DiagnosticsRefs, roundTripped.DiagnosticsRefs);
        Assert.Equal(value.RollbackAvailable, roundTripped.RollbackAvailable);
    }

    [Fact]
    public void FrozenTradingPermissionStatesSerializeWithExactSemantics()
    {
        JsonSerializerOptions options = ThemeContractJson.CreateSerializerOptions();

        Assert.Equal("\"tradable\"", JsonSerializer.Serialize(TradingPermissionPresentationState.Tradable, options));
        Assert.Equal("\"warning\"", JsonSerializer.Serialize(TradingPermissionPresentationState.Warning, options));
        Assert.Equal("\"blocked\"", JsonSerializer.Serialize(TradingPermissionPresentationState.Blocked, options));
    }

    [Fact]
    public void ContractVersionsRemainFrozenAndNormalized()
    {
        Assert.Equal("1.0.0", ContractVersions.ThemeApi);
        Assert.Equal("1.0.0", ContractVersions.ThemeArchitecture);
        Assert.Equal("1.1.0", ContractVersions.UxContract);
        Assert.Equal("v1.1", ContractVersions.UxArchitectureDisplay);
        Assert.Equal("1.0", ContractVersions.Schema);
    }

    private static void AssertSchemaRoundTrip<T>(T value, string schemaFile)
    {
        JsonSerializerOptions options = ThemeContractJson.CreateSerializerOptions();
        string serialized = JsonSerializer.Serialize(value, options);
        using JsonDocument schema = JsonDocument.Parse(File.ReadAllText(Path.Combine(
            RepositoryPaths.ThemeContracts,
            "schemas",
            schemaFile)));
        using JsonDocument instance = JsonDocument.Parse(serialized);

        Assert.Empty(JsonSchemaSubsetValidator.Validate(schema.RootElement, instance.RootElement));

        T? roundTripped = JsonSerializer.Deserialize<T>(serialized, options);
        Assert.NotNull(roundTripped);
        using JsonDocument roundTrippedJson = JsonDocument.Parse(JsonSerializer.Serialize(roundTripped, options));
        Assert.True(JsonElement.DeepEquals(instance.RootElement, roundTrippedJson.RootElement));
    }

    private static void AssertSchemaGeneratedWireShapeRoundTrips(
        string json,
        JsonElement schema,
        Type dtoType,
        JsonSerializerOptions options)
    {
        object? dto = JsonSerializer.Deserialize(json, dtoType, options);
        Assert.NotNull(dto);

        string reserialized = JsonSerializer.Serialize(dto, dtoType, options);
        using JsonDocument wireDocument = JsonDocument.Parse(reserialized);
        Assert.Empty(JsonSchemaSubsetValidator.Validate(schema, wireDocument.RootElement));

        object? semanticRoundTrip = JsonSerializer.Deserialize(reserialized, dtoType, options);
        Assert.NotNull(semanticRoundTrip);
        Assert.Equal(reserialized, JsonSerializer.Serialize(semanticRoundTrip, dtoType, options));
    }

    private static void AssertDtoMatchesSchema(
        Type dtoType,
        JsonElement schema,
        string path,
        ISet<string> visited)
    {
        dtoType = Nullable.GetUnderlyingType(dtoType) ?? dtoType;
        string visitKey = $"{dtoType.FullName}|{schema.GetRawText()}";
        if (!visited.Add(visitKey) || IsDictionary(dtoType) || dtoType == typeof(JsonElement))
        {
            return;
        }

        string? schemaType = GetSchemaType(schema);
        if (schemaType == "array")
        {
            Type? elementType = GetEnumerableElementType(dtoType);
            Assert.True(elementType is not null, $"{path}: DTO type {dtoType.Name} is not an array.");
            AssertDtoMatchesSchema(elementType, schema.GetProperty("items"), $"{path}[]", visited);
            return;
        }

        if (schemaType != "object")
        {
            return;
        }

        JsonElement schemaProperties = schema.GetProperty("properties");
        HashSet<string> required = schema.TryGetProperty("required", out JsonElement requiredElement)
            ? requiredElement.EnumerateArray().Select(item => item.GetString()!).ToHashSet(StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);
        Dictionary<string, System.Reflection.PropertyInfo> dtoProperties = dtoType
            .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .ToDictionary(GetSerializedPropertyName, StringComparer.Ordinal);
        HashSet<string> schemaPropertyNames = schemaProperties
            .EnumerateObject()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.True(
            schemaPropertyNames.SetEquals(dtoProperties.Keys),
            $"{path}: DTO/schema property mismatch. Schema-only: {string.Join(", ", schemaPropertyNames.Except(dtoProperties.Keys))}; DTO-only: {string.Join(", ", dtoProperties.Keys.Except(schemaPropertyNames))}.");

        foreach ((string jsonName, System.Reflection.PropertyInfo property) in dtoProperties)
        {
            bool dtoOptional = IsNullable(property);
            Assert.True(
                required.Contains(jsonName) != dtoOptional,
                $"{path}.{jsonName}: schema required={required.Contains(jsonName)} but DTO nullable={dtoOptional}.");
            AssertDtoMatchesSchema(
                property.PropertyType,
                schemaProperties.GetProperty(jsonName),
                $"{path}.{jsonName}",
                visited);
        }
    }

    private static string GetSerializedPropertyName(System.Reflection.PropertyInfo property) =>
        property.GetCustomAttributes(typeof(JsonPropertyNameAttribute), inherit: false)
            .Cast<JsonPropertyNameAttribute>()
            .SingleOrDefault()?.Name
        ?? JsonNamingPolicy.SnakeCaseLower.ConvertName(property.Name);

    private static bool IsNullable(System.Reflection.PropertyInfo property)
    {
        if (Nullable.GetUnderlyingType(property.PropertyType) is not null)
        {
            return true;
        }

        if (property.PropertyType.IsValueType)
        {
            return false;
        }

        return new System.Reflection.NullabilityInfoContext().Create(property).ReadState
            == System.Reflection.NullabilityState.Nullable;
    }

    private static bool IsDictionary(Type type) =>
        type.GetInterfaces().Append(type).Any(candidate =>
            candidate.IsGenericType
            && candidate.GetGenericTypeDefinition() is Type definition
            && (definition == typeof(IDictionary<,>) || definition == typeof(IReadOnlyDictionary<,>)));

    private static Type? GetEnumerableElementType(Type type) =>
        type.GetInterfaces().Append(type)
            .FirstOrDefault(candidate => candidate.IsGenericType
                                         && candidate.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            ?.GetGenericArguments()[0];

    private static string? GetSchemaType(JsonElement schema)
    {
        if (!schema.TryGetProperty("type", out JsonElement type))
        {
            return null;
        }

        return type.ValueKind == JsonValueKind.Array
            ? type.EnumerateArray().Select(item => item.GetString()).FirstOrDefault(candidate => candidate != "null")
            : type.GetString();
    }
}
