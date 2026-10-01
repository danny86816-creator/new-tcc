using System.Globalization;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace Tcc.Architecture.Tests;

public sealed class ReferenceMasterRebuildFoundationR2Tests
{
    private static readonly XNamespace P = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly string HostRoot = Path.Combine(RepositoryPaths.Root, "src", "Tcc.DesktopHost");
    private static readonly string XamlPath = Path.Combine(HostRoot, "MainWindow.xaml");
    private static readonly string RebuildAssetRoot = Path.Combine(HostRoot, "Assets", "RebuildR2");
    private static readonly string CharacterAssetRoot = Path.Combine(HostRoot, "Assets", "Character");

    [Fact]
    public void RebuildUsesOnlyTheNewFoundationResourceDictionary()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterRebuildFoundationR2Tests), nameof(RebuildUsesOnlyTheNewFoundationResourceDictionary));
    }

    [Fact]
    public void ProjectCompilesExactlyFourRuntimeRasterResources()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterRebuildFoundationR2Tests), nameof(ProjectCompilesExactlyFourRuntimeRasterResources));
    }

    [Fact]
    public void CharacterAuthorityIsByteLockedToTheUserSelectedAsset()
    {
        string path = Path.Combine(CharacterAssetRoot, "TCC_P3_CHARACTER_WORKING_SOURCE.png");
        Assert.Equal("A42A885A2E3E4823E57AE50CADBED91CBDC73C50B6B620416949149000B6385D", Hash(path));
        Assert.Equal((1024, 1536, 6), ReadPngHeader(path));
    }

    [Fact]
    public void SceneAndForegroundAssetsHaveTheRequiredCanvasAndAlpha()
    {
        string scene = Path.Combine(RebuildAssetRoot, "TCC_WPF_REBUILD_SCENE_BASE_R2.png");
        string left = Path.Combine(RebuildAssetRoot, "TCC_WPF_REBUILD_FOREGROUND_LEFT_R4.png");
        string right = Path.Combine(RebuildAssetRoot, "TCC_WPF_REBUILD_FOREGROUND_RIGHT_R5.png");
        Assert.Equal((1672, 941, 2), ReadPngHeader(scene));
        Assert.Equal((1672, 941, 6), ReadPngHeader(left));
        Assert.Equal((1672, 941, 6), ReadPngHeader(right));
        Assert.Equal("D27895EB7DD3AB07864A0C043C7E71735472A8B0F6194C136AD5D9E3CE1D0E54", Hash(scene));
        Assert.Equal("5A992FA8718BABE3AD25107BC9398D7152360D09B31051CAD50E09796CD8DDC4", Hash(left));
        Assert.Equal("1F164687D977962DB30D8ACC74A37E79350EE710663F151283F635C3757A060F", Hash(right));
    }

    [Fact]
    public void CharacterUsesAspectSafeUniformScalingWithoutClipOrOpacityMask()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterRebuildFoundationR2Tests), nameof(CharacterUsesAspectSafeUniformScalingWithoutClipOrOpacityMask));
    }

    [Fact]
    public void CardShellGeometryMatchesTheSelectedMasterFoundation()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterRebuildFoundationR2Tests), nameof(CardShellGeometryMatchesTheSelectedMasterFoundation));
    }

    [Fact]
    public void RightEdgePreservesTheMastersThreeStepComposition()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterRebuildFoundationR2Tests), nameof(RightEdgePreservesTheMastersThreeStepComposition));
    }

    [Fact]
    public void RiskTerrainUsesUpperSplitContentAndAReleasedLowerRightShell()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterRebuildFoundationR2Tests), nameof(RiskTerrainUsesUpperSplitContentAndAReleasedLowerRightShell));
    }

    [Fact]
    public void CardMaterialUsesPerCardMasterCalibratedGradients()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterRebuildFoundationR2Tests), nameof(CardMaterialUsesPerCardMasterCalibratedGradients));
    }

    [Fact]
    public void ChromeNavigationHeaderAndNestedSurfacesUseAuditableMasterCalibratedBrushes()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterRebuildFoundationR2Tests), nameof(ChromeNavigationHeaderAndNestedSurfacesUseAuditableMasterCalibratedBrushes));
    }

    [Fact]
    public void CardJunctionsRetainTheMastersLowAlphaGlassContinuity()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterRebuildFoundationR2Tests), nameof(CardJunctionsRetainTheMastersLowAlphaGlassContinuity));
    }

    [Fact]
    public void EvidenceBackedCardsUseDistinctInternalDepthPlanesWithoutChangingSafetyCards()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterRebuildFoundationR2Tests), nameof(EvidenceBackedCardsUseDistinctInternalDepthPlanesWithoutChangingSafetyCards));
    }

    [Fact]
    public void SceneUiAndForegroundDepthOrderIsExplicit()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterRebuildFoundationR2Tests), nameof(SceneUiAndForegroundDepthOrderIsExplicit));
    }

    [Fact]
    public void ForegroundUsesTwoCleanIndependentSourcesWithoutRuntimeClipping()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterRebuildFoundationR2Tests), nameof(ForegroundUsesTwoCleanIndependentSourcesWithoutRuntimeClipping));
    }

    [Fact]
    public void RequiredModulesAndUiAutomationRemainAvailable()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterRebuildFoundationR2Tests), nameof(RequiredModulesAndUiAutomationRemainAvailable));
    }

    [Fact]
    public void FoundationAvoidsRasterUiAndUnsafeVisualEffects()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterRebuildFoundationR2Tests), nameof(FoundationAvoidsRasterUiAndUnsafeVisualEffects));
    }

    private static double Right(XElement element) =>
        double.Parse(element.Attribute("Canvas.Left")!.Value, CultureInfo.InvariantCulture) +
        double.Parse(element.Attribute("Width")!.Value, CultureInfo.InvariantCulture);

    private static XElement ByAutomationId(XDocument document, string id) =>
        Assert.Single(document.Descendants(), item => item.Attributes().Any(attribute =>
            attribute.Name.LocalName == "AutomationProperties.AutomationId" && attribute.Value == id));

    private static void AssertImageZ(XDocument document, string tag, string expected) =>
        Assert.Equal(expected, Assert.Single(document.Descendants(P + "Image"),
            item => item.Attribute("Tag")?.Value == tag).Attribute("Panel.ZIndex")?.Value);

    private static string Geometry(XElement element) => string.Join(',',
        element.Attribute("Canvas.Left")?.Value,
        element.Attribute("Canvas.Top")?.Value,
        element.Attribute("Width")?.Value,
        element.Attribute("Height")?.Value);

    private static string Hash(string path) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    private static (int Width, int Height, int ColorType) ReadPngHeader(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        Assert.True(bytes.Length > 26);
        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, bytes[..8]);
        int width = (bytes[16] << 24) | (bytes[17] << 16) | (bytes[18] << 8) | bytes[19];
        int height = (bytes[20] << 24) | (bytes[21] << 16) | (bytes[22] << 8) | bytes[23];
        return (width, height, bytes[25]);
    }
}
