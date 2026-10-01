using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;

namespace Tcc.Architecture.Tests;

public sealed class ReferenceMasterP3CharacterIntegrationTests
{
    private const string ExpectedCharacterHash = "A42A885A2E3E4823E57AE50CADBED91CBDC73C50B6B620416949149000B6385D";
    private const string ExpectedDerivedCharacterHash = "F2ABBD4778C017DED49416EACD4F29F88631362769D8869030C3A54CCD8FDAE4";
    private const string CharacterRelativePath = "Assets/Character/TCC_P3_CHARACTER_WORKING_SOURCE.png";
    private const string DerivedCharacterRelativePath = "Assets/Character/TCC_P3_CHARACTER_DERIVED_INTEGRATED.png";
    private const string EdgeMatteRelativeRoot = "Assets/Character/EdgeMatte/";
    private const string IntegrationRelativeRoot = "Assets/Character/Integration/";
    private static readonly Dictionary<string, string> ExpectedEdgeMatteHashes =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["TCC_P3_CHARACTER_CORE_MASK_V1.png"] = "617A6D8A0FE0D4D062A43E0B0DEB6BFA3555A35F85E33F3BF4348E2018147AFC",
            ["TCC_P3_CHARACTER_EDGE_BAND_MASK_V1.png"] = "65653EE9BF0B2CA0105C5F7BFC9B04B9F3B38F6E2DC8D9A72B036220D2E4986B",
            ["TCC_P3_CHARACTER_HAIR_EDGE_MASK_V1.png"] = "A58C7455C9013F97442589727A0BDAAFD8F488D090E1F9E164D3F79052EF54EE",
            ["TCC_P3_CHARACTER_FUR_EDGE_MASK_V1.png"] = "76A5C6CD57A62F1060BD312033062821501AA434374E2689346FD1AB5D1AD1B7",
            ["TCC_P3_CHARACTER_SOFT_CLOTH_EDGE_MASK_V1.png"] = "C1B76E7649E9C7C09333DD14FBE95173B5E5A5DD2BBEC5922865DB1FE3284E07",
            ["TCC_P3_CHARACTER_HARD_EDGE_MASK_V1.png"] = "95564B1BD4E623C0C526293729B5D3A8B3C0CF4C2C4D48A14ACCA74EEF12FE6E",
        };
    private static readonly Dictionary<string, string> ExpectedIntegrationMaskHashes =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["TCC_P3_CHARACTER_CLIP_TRANSITION_MASK.png"] = "7207CC07AD530F005104E171217D845FA8B40641DC84183F6D73B2A7A051F953",
            ["TCC_P3_CHARACTER_ENVIRONMENT_MASK.png"] = "8019D2981623791CA6712DB14F13DF5B7C78632536DD39E8FFE75EBA90E75DE8",
            ["TCC_P3_CHARACTER_SHADOW_SIDE_MASK.png"] = "C25F02CEE09A6262C922B07DBD4055A4D59828CF8448148A80B8C4FFC8806E6E",
            ["TCC_P3_CHARACTER_MOONLIGHT_EDGE_MASK.png"] = "9DE279A1358D82FBA862CB320567D6594D97099A70CDA8CE7280EC77024796FD",
            ["TCC_P3_CHARACTER_ATMOSPHERE_MASK.png"] = "6ACD61A92B43180F1C56EE286F3C9171FC339C75368A9D17727AE9A3F8593CB0",
        };
    private static readonly XNamespace Presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly string HostRoot = Path.Combine(RepositoryPaths.Root, "src", "Tcc.DesktopHost");
    private static readonly string MainWindowPath = Path.Combine(HostRoot, "MainWindow.xaml");
    private static readonly string ProjectPath = Path.Combine(HostRoot, "Tcc.DesktopHost.csproj");
    private static readonly string ManifestPath = Path.Combine(
        RepositoryPaths.Root, "uiux_cleanroom", "reference_master_rebase", "wpf_p3", "p3_c",
        "derived_asset", "TCC_P3_DERIVED_CHARACTER_ASSET_MANIFEST.json");
    private static readonly string EdgeMatteManifestPath = Path.Combine(
        RepositoryPaths.Root, "uiux_cleanroom", "reference_master_rebase", "wpf_p3", "p3_c",
        "edge_matte", "TCC_WPF_P3_C_EDGE_MATTE_MANIFEST.json");
    private static readonly string IntegrationMaskManifestPath = Path.Combine(
        RepositoryPaths.Root, "uiux_cleanroom", "reference_master_rebase", "wpf_p3", "p3_c",
        "TCC_WPF_P3_C_DERIVED_MASKS.json");

    [Fact]
    public void CharacterAuthorityAssetsRemainFrozenAndP3CRuntimeUsesWorkingSource()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3CharacterIntegrationTests), nameof(CharacterAuthorityAssetsRemainFrozenAndP3CRuntimeUsesWorkingSource));
    }

    [Fact]
    public void AcceptedDerivedCharacterIsAByteIdenticalCompiledResource()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3CharacterIntegrationTests), nameof(AcceptedDerivedCharacterIsAByteIdenticalCompiledResource));
    }

    [Fact]
    public void P3CUsesOneWorkingSourceImageWithStaticMaterialSensitiveLayers()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3CharacterIntegrationTests), nameof(P3CUsesOneWorkingSourceImageWithStaticMaterialSensitiveLayers));
    }

    [Fact]
    public void P3CUsesFullSourceCoordinatesAndExcludesEdgeAndLowerContinuationImages()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3CharacterIntegrationTests), nameof(P3CUsesFullSourceCoordinatesAndExcludesEdgeAndLowerContinuationImages));
    }

    [Fact]
    public void IntegrationMasksAreDeterministicCompiledRgbaAssets()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3CharacterIntegrationTests), nameof(IntegrationMasksAreDeterministicCompiledRgbaAssets));
    }

    [Fact]
    public void SelectiveEdgeMasksAreDeterministicCompiledRgbaAssets()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3CharacterIntegrationTests), nameof(SelectiveEdgeMasksAreDeterministicCompiledRgbaAssets));
    }

    [Fact]
    public void P3CCharacterIntegrationRemainsStaticInsideTheAcceptedSceneDepthTopology()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3CharacterIntegrationTests), nameof(P3CCharacterIntegrationRemainsStaticInsideTheAcceptedSceneDepthTopology));
    }

    private static void AssertDecorative(XElement element)
    {
        Assert.Equal("False", element.Attribute("IsHitTestVisible")?.Value);
        Assert.Equal("False", element.Attribute("Focusable")?.Value);
        Assert.Equal("False", element.Attribute("KeyboardNavigation.IsTabStop")?.Value);
    }

    private static string Sha256(string path) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    private static int ReadBigEndianInt32(byte[] bytes, int offset) =>
        (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];

    private static XElement GetByTag(XDocument document, string tag) =>
        Assert.Single(document.Descendants(), element => element.Attribute("Tag")?.Value == tag);
}
