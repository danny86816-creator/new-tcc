using System.Security.Cryptography;
using System.Xml.Linq;

namespace Tcc.Architecture.Tests;

public sealed class ReferenceMasterP3BackgroundSceneTests
{
    private const string ExpectedBackgroundHash = "4384664A6016564F6C47E56FA554F6B10B1BA855499742ECFEF000267A4E2771";
    private const string ProductionAssetRelativePath = "Assets/Scene/TCC_P3_SCENE_BACKGROUND_PLATE.png";
    private static readonly XNamespace Presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly string HostRoot = Path.Combine(RepositoryPaths.Root, "src", "Tcc.DesktopHost");
    private static readonly string MainWindowPath = Path.Combine(HostRoot, "MainWindow.xaml");
    private static readonly string ProjectPath = Path.Combine(HostRoot, "Tcc.DesktopHost.csproj");
    private static readonly string ResourceRoot = Path.Combine(HostRoot, "Resources");
    private static readonly string ProductionAssetPath = Path.Combine(HostRoot, ProductionAssetRelativePath.Replace('/', Path.DirectorySeparatorChar));
    private static readonly string AcceptedAssetPath = Path.Combine(
        RepositoryPaths.Root, "uiux_cleanroom", "reference_master_rebase", "wpf_p3", "assets", "scene",
        "TCC_P3_SCENE_BACKGROUND_PLATE_V1_CANDIDATE_02.png");

    [Fact]
    public void ProductionBackgroundIsTheByteIdenticalAcceptedAssetAndACompiledResource()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3BackgroundSceneTests), nameof(ProductionBackgroundIsTheByteIdenticalAcceptedAssetAndACompiledResource));
    }

    [Fact]
    public void SceneBackgroundIsDecorativeUniformToFillAndBehindTheAcceptedUi()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3BackgroundSceneTests), nameof(SceneBackgroundIsDecorativeUniformToFillAndBehindTheAcceptedUi));
    }

    [Fact]
    public void SceneTopologyIntegratesAuthorityBackedContactAndPreservesDepthPlanes()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3BackgroundSceneTests), nameof(SceneTopologyIntegratesAuthorityBackedContactAndPreservesDepthPlanes));
    }

    [Fact]
    public void AcceptedP1GeometryAndP2ResourceOrderRemainUnchanged()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3BackgroundSceneTests), nameof(AcceptedP1GeometryAndP2ResourceOrderRemainUnchanged));
    }

    [Fact]
    public void BackgroundStageUsesOneStaticBitmapAndNoRuntimeVisualEffects()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3BackgroundSceneTests), nameof(BackgroundStageUsesOneStaticBitmapAndNoRuntimeVisualEffects));
    }

    [Fact]
    public void SceneCompatibilityUsesDistinctLowDensityDenseAndInnerAttenuationTiers()
    {
        XDocument brushes = XDocument.Load(Path.Combine(ResourceRoot, "ReferenceMaster.Brushes.xaml"));
        XDocument materials = XDocument.Load(Path.Combine(ResourceRoot, "ReferenceMaster.Materials.xaml"));
        XDocument controls = XDocument.Load(Path.Combine(ResourceRoot, "ReferenceMaster.Controls.xaml"));
        XDocument modules = XDocument.Load(Path.Combine(ResourceRoot, "ReferenceMaster.ModuleStyles.xaml"));

        string low = "Tcc.ReferenceMaster.Brush.Surface.Scene.LowDensity";
        string densePrimary = "Tcc.ReferenceMaster.Brush.Surface.Scene.DensePrimary";
        string denseSecondary = "Tcc.ReferenceMaster.Brush.Surface.Scene.DenseSecondary";
        string denseQuiet = "Tcc.ReferenceMaster.Brush.Surface.Scene.DenseQuiet";
        string denseDeep = "Tcc.ReferenceMaster.Brush.Surface.Scene.DenseDeep";
        string denseInterior = "Tcc.ReferenceMaster.Brush.Surface.Scene.DenseInterior";
        string denseRow = "Tcc.ReferenceMaster.Brush.Surface.Scene.DenseRow";
        string lowerDenseInterior = "Tcc.ReferenceMaster.Brush.Surface.Scene.LowerDenseInterior";
        string prioritiesInterior = "Tcc.ReferenceMaster.Brush.Surface.Scene.PrioritiesInterior";
        string mentalInterior = "Tcc.ReferenceMaster.Brush.Surface.Scene.MentalInterior";

        string[] gradientTiers =
        [
            low, densePrimary, denseSecondary, denseQuiet, denseDeep, denseInterior,
            lowerDenseInterior, prioritiesInterior, mentalInterior,
        ];
        Assert.All(gradientTiers, key =>
            Assert.Equal(Presentation + "LinearGradientBrush", GetResourceByKey(brushes, key).Name));
        Assert.Equal(Presentation + "SolidColorBrush", GetResourceByKey(brushes, denseRow).Name);
        Assert.Equal(gradientTiers.Length, gradientTiers
            .Select(key => GetResourceByKey(brushes, key).ToString(SaveOptions.DisableFormatting))
            .Distinct(StringComparer.Ordinal).Count());

        double lowAverageAlpha = AverageGradientAlpha(GetResourceByKey(brushes, low));
        Assert.All([densePrimary, denseSecondary, denseQuiet, denseDeep], key =>
            Assert.True(AverageGradientAlpha(GetResourceByKey(brushes, key)) > lowAverageAlpha));

        AssertStyleBackground(modules, "Tcc.ReferenceMaster.SafetyTile", low);
        AssertStyleBackground(modules, "Tcc.ReferenceMaster.Module.RiskTerrain", low);
        AssertStyleBackground(modules, "Tcc.ReferenceMaster.Module.Observation", densePrimary);
        AssertStyleBackground(modules, "Tcc.ReferenceMaster.Module.MarketAnchor", densePrimary);
        AssertStyleBackground(modules, "Tcc.ReferenceMaster.Module.Positions", denseSecondary);
        AssertStyleBackground(modules, "Tcc.ReferenceMaster.Module.Priorities", denseSecondary);
        AssertStyleBackground(modules, "Tcc.ReferenceMaster.Module.MentalState", denseQuiet);
        AssertStyleBackground(modules, "Tcc.ReferenceMaster.Module.AiSummary", denseQuiet);
        AssertStyleBackground(modules, "Tcc.ReferenceMaster.Module.Activity", denseDeep);
        AssertStyleBackground(modules, "Tcc.ReferenceMaster.TableHeaderBand", denseInterior);
        AssertStyleBackground(modules, "Tcc.ReferenceMaster.TableRow", denseRow);
        AssertStyleBackground(modules, "Tcc.ReferenceMaster.Content.LowerDense", lowerDenseInterior);
        AssertStyleBackground(modules, "Tcc.ReferenceMaster.Content.Priorities", prioritiesInterior);
        AssertStyleBackground(modules, "Tcc.ReferenceMaster.Content.Mental", mentalInterior);
        AssertStyleBackground(materials, "Tcc.ReferenceMaster.Material.ChartPane", denseInterior);
        AssertStyleBackground(materials, "Tcc.ReferenceMaster.Material.StatusBand", denseInterior);
        AssertStyleBackground(controls, "Tcc.ReferenceMaster.AiSummaryInner", denseInterior);
    }

    [Fact]
    public void HumanReviewR2SuppressesOnlyObservationRowsAndLowerContentZones()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP3BackgroundSceneTests), nameof(HumanReviewR2SuppressesOnlyObservationRowsAndLowerContentZones));
    }

    private static string Sha256(string path) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    private static int ReadBigEndianInt32(byte[] bytes, int offset) =>
        (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];

    private static XElement GetResourceByKey(XDocument document, string key) =>
        Assert.Single(document.Root!.Elements(), element => element.Attribute(Xaml + "Key")?.Value == key);

    private static double AverageGradientAlpha(XElement brush) => brush
        .Elements(Presentation + "GradientStop")
        .Select(stop => Convert.ToByte(stop.Attribute("Color")!.Value.Substring(1, 2), 16))
        .Average(value => value);

    private static void AssertStyleBackground(XDocument document, string styleKey, string brushKey)
    {
        XElement style = GetResourceByKey(document, styleKey);
        Assert.Contains(style.Elements(Presentation + "Setter"), setter =>
            setter.Attribute("Property")?.Value == "Background" &&
            setter.Attribute("Value")?.Value == $"{{DynamicResource {brushKey}}}");
    }

    private static void AssertContentZone(
        XDocument window,
        string moduleId,
        string styleKey,
        string gridRow,
        string? margin)
    {
        XElement module = GetByAutomationId(window, moduleId);
        XElement zone = Assert.Single(module.Descendants(Presentation + "Border"), item =>
            item.Attribute("Style")?.Value == $"{{StaticResource {styleKey}}}");
        Assert.Equal(gridRow, zone.Attribute("Grid.Row")?.Value);
        Assert.Equal(margin, zone.Attribute("Margin")?.Value);
        Assert.Null(zone.Attribute("Width"));
        Assert.Null(zone.Attribute("Height"));
    }

    private static XElement GetByTag(XDocument document, string tag) =>
        Assert.Single(document.Descendants(), element => element.Attribute("Tag")?.Value == tag);

    private static XElement GetByAutomationId(XDocument document, string automationId) =>
        Assert.Single(document.Descendants(), element => element.Attributes().Any(attribute =>
            attribute.Name.LocalName == "AutomationProperties.AutomationId" && attribute.Value == automationId));
}
