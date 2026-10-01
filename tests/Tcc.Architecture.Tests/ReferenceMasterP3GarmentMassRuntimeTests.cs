using System.Security.Cryptography;
using System.Xml.Linq;

namespace Tcc.Architecture.Tests;

public sealed class ReferenceMasterP3GarmentMassRuntimeTests
{
    private const string TerrainRelativePath = "Assets/Scene/TCC_P3_CHARACTER_LOWER_OCCLUSION_TERRAIN_R1_02.png";
    private const string ForegroundRelativePath = "Assets/Scene/TCC_P3_SCENE_FOREGROUND_ACCENT_V1.png";
    private const string CharacterRelativePath = "Assets/Character/TCC_P3_CHARACTER_WORKING_SOURCE.png";
    private const string ExpectedTerrainHash = "D4A657D8A93D2C4B1B262280889E2BC34BF8147A918270361B42FD3795D30E31";
    private static readonly XNamespace Presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly string HostRoot = Path.Combine(RepositoryPaths.Root, "src", "Tcc.DesktopHost");
    private static readonly string MainWindowPath = Path.Combine(HostRoot, "MainWindow.xaml");
    private static readonly string ProjectPath = Path.Combine(HostRoot, "Tcc.DesktopHost.csproj");

    [Fact]
    public void HumanAcceptedCompositionDUsesSeparateCharacterTerrainUiAndForegroundDepthLayers()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3GarmentMassRuntimeTests), nameof(HumanAcceptedCompositionDUsesSeparateCharacterTerrainUiAndForegroundDepthLayers));
    }

    [Fact]
    public void TerrainMatchesCompositionDBottomRightUniformScaleAndFrozenAsset()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3GarmentMassRuntimeTests), nameof(TerrainMatchesCompositionDBottomRightUniformScaleAndFrozenAsset));
    }

    [Fact]
    public void AcceptedForegroundIsNativeScaleAndDoesNotReplaceTerrainDepth()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3GarmentMassRuntimeTests), nameof(AcceptedForegroundIsNativeScaleAndDoesNotReplaceTerrainDepth));
    }

    [Fact]
    public void CharacterAuthorityPlacementAndEffectBudgetRemainFrozen()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3GarmentMassRuntimeTests), nameof(CharacterAuthorityPlacementAndEffectBudgetRemainFrozen));
    }

    private static void AssertDecorative(XElement element)
    {
        Assert.Equal("False", element.Attribute("IsHitTestVisible")?.Value);
        Assert.Equal("False", element.Attribute("Focusable")?.Value);
        Assert.Equal("False", element.Attribute("KeyboardNavigation.IsTabStop")?.Value);
    }

    private static string Sha256(string path) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    private static XElement GetByTag(XDocument document, string tag) =>
        Assert.Single(document.Descendants(), element => element.Attribute("Tag")?.Value == tag);
}
