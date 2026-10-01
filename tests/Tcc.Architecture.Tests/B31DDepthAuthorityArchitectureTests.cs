using System.Text.Json;

namespace Tcc.Architecture.Tests;

public sealed class B31DDepthAuthorityArchitectureTests
{
    private static readonly string Root = FindRoot();
    private static readonly string Work = Path.Combine(Root, "automation", "tcc_master_pipeline", "work", "m1_4_6_b3_1d");
    private static readonly string[] ExpectedDepthClasses = ["D0", "D1", "D2", "D3", "D4", "D5", "D6"];
    private static readonly string[] ExpectedPolicyValues =
    [
        "HARD_FOREGROUND_ALLOWED", "SOFT_FOREGROUND_ALLOWED", "GLASS_FADE_ONLY",
        "BACKGROUND_ONLY", "FORBIDDEN", "UNKNOWN",
    ];

    [Fact]
    public void HistoricalB31RuntimeCandidatesStayExcludedAfterReferenceMasterSupersession()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(B31DDepthAuthorityArchitectureTests), nameof(HistoricalB31RuntimeCandidatesStayExcludedAfterReferenceMasterSupersession));
    }

    [Fact]
    public void SceneDepthAuthoritySeparatesDepthFromFusionPolicy()
    {
        using JsonDocument depth = ReadJson("b3_1d_scene_depth_authority.json");
        using JsonDocument policy = ReadJson("b3_1d_ui_fusion_policy.json");

        string[] classes = depth.RootElement.GetProperty("classes").EnumerateArray()
            .Select(item => item.GetProperty("depth_class").GetString()!)
            .ToArray();
        Assert.Equal(ExpectedDepthClasses, classes);
        Assert.Equal("spatial depth is independent from UI fusion policy", depth.RootElement.GetProperty("principle").GetString());
        string[] values = policy.RootElement.GetProperty("allowed_values").EnumerateArray()
            .Select(item => item.GetString()!)
            .ToArray();
        Assert.Equal(ExpectedPolicyValues, values);
    }

    [Fact]
    public void CharacterCoreUsesSilhouetteGlassFadeWithoutRadialHalo()
    {
        using JsonDocument field = ReadJson("b3_1d_character_distance_field.json");
        using JsonDocument fade = ReadJson("b3_1d_glass_fade_policy.json");
        using JsonDocument relationships = ReadJson("b3_1d_occlusion_relationships.json");

        Assert.False(field.RootElement.GetProperty("radial_gradient_used").GetBoolean());
        Assert.True(fade.RootElement.GetProperty("silhouette_based").GetBoolean());
        Assert.False(fade.RootElement.GetProperty("radial_gradient_used").GetBoolean());
        Assert.Equal(5, fade.RootElement.GetProperty("synchronized_channels").GetArrayLength());
        JsonElement character = relationships.RootElement.GetProperty("relationships").EnumerateArray()
            .Single(item => item.GetProperty("element_id").GetString() == "B3SC011");
        Assert.Equal("GLASS_FADE_ONLY", character.GetProperty("design_policy").GetString());
    }

    [Fact]
    public void PlumPolicyRequiresWholeSourceContinuousBranchesAndProtectsContent()
    {
        using JsonDocument plum = ReadJson("b3_1d_plum_foreground_policy.json");
        Assert.False(plum.RootElement.GetProperty("fragment_only_allowed").GetBoolean());
        Assert.Equal(0, plum.RootElement.GetProperty("critical_content_overlap_pixels").GetInt32());
        foreach (JsonElement element in plum.RootElement.GetProperty("elements").EnumerateArray())
        {
            Assert.True(element.GetProperty("main_branch").GetBoolean());
            Assert.True(element.GetProperty("secondary_branches").GetBoolean());
            Assert.True(element.GetProperty("blossoms").GetBoolean());
            Assert.True(element.GetProperty("source_coordinate").GetBoolean());
        }
    }

    [Fact]
    public void IsolatedCandidatePreviewsUseCanonicalViewportAndRemainUnselected()
    {
        string[] candidates =
        {
            "B3_1D_CHARACTER_FADE_A.png", "B3_1D_CHARACTER_FADE_B.png", "B3_1D_CHARACTER_FADE_C.png",
            "B3_1D_COMBINED_A.png", "B3_1D_COMBINED_B.png", "B3_1D_COMBINED_C.png",
            "B3_1D_FINAL_PRODUCT_PREVIEW.png",
        };
        foreach (string candidate in candidates)
        {
            byte[] header = File.ReadAllBytes(Path.Combine(Work, candidate))[..24];
            int width = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(16, 4));
            int height = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(20, 4));
            Assert.Equal((2142, 1196), (width, height));
        }

        using JsonDocument fade = ReadJson("b3_1d_glass_fade_policy.json");
        Assert.False(fade.RootElement.GetProperty("production_applied").GetBoolean());
        Assert.Equal("PENDING", fade.RootElement.GetProperty("supervisor_selection").GetString());
    }

    private static JsonDocument ReadJson(string name) => JsonDocument.Parse(File.ReadAllText(Path.Combine(Work, name)));

    private static string FindRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "Tcc.slnx")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
