using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Tcc.Architecture.Tests;

public sealed class ReferenceMasterP2VisualTests
{
    private static readonly string HostRoot = Path.Combine(RepositoryPaths.Root, "src", "Tcc.DesktopHost");
    private static readonly string MainWindowPath = Path.Combine(HostRoot, "MainWindow.xaml");
    private static readonly string ResourceRoot = Path.Combine(HostRoot, "Resources");
    private static readonly XNamespace Presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml";

    private static readonly string[] ExpectedDictionaries =
    [
        "Resources/ReferenceMaster.Brushes.xaml",
        "Resources/ReferenceMaster.Materials.xaml",
        "Resources/ReferenceMaster.Typography.xaml",
        "Resources/ReferenceMaster.Icons.xaml",
        "Resources/ReferenceMaster.Controls.xaml",
        "Resources/ReferenceMaster.ModuleStyles.xaml",
    ];

    [Fact]
    public void VisualResourcesAreCentralizedMergedInDependencyOrderAndHaveUniqueKeys()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP2VisualTests), nameof(VisualResourcesAreCentralizedMergedInDependencyOrderAndHaveUniqueKeys));
    }

    [Fact]
    public void SurfaceHierarchyAndSemanticStatusFamiliesAreComplete()
    {
        HashSet<string> keys = ResourceKeys();
        string[] required =
        [
            "Tcc.ReferenceMaster.Brush.Background.Base",
            "Tcc.ReferenceMaster.Brush.Background.Atmosphere",
            "Tcc.ReferenceMaster.Brush.Surface.TopChrome",
            "Tcc.ReferenceMaster.Brush.Surface.Navigation",
            "Tcc.ReferenceMaster.Brush.Surface.HeaderScrim",
            "Tcc.ReferenceMaster.Brush.Surface.Panel",
            "Tcc.ReferenceMaster.Brush.Surface.PanelElevated",
            "Tcc.ReferenceMaster.Brush.Surface.PanelDeep",
            "Tcc.ReferenceMaster.Brush.Surface.PanelQuiet",
            "Tcc.ReferenceMaster.Brush.Surface.Scene.LowDensity",
            "Tcc.ReferenceMaster.Brush.Surface.Scene.DensePrimary",
            "Tcc.ReferenceMaster.Brush.Surface.Scene.DenseSecondary",
            "Tcc.ReferenceMaster.Brush.Surface.Scene.DenseQuiet",
            "Tcc.ReferenceMaster.Brush.Surface.Scene.DenseDeep",
            "Tcc.ReferenceMaster.Brush.Surface.Scene.DenseInterior",
            "Tcc.ReferenceMaster.Brush.Surface.Scene.DenseRow",
            "Tcc.ReferenceMaster.Brush.Surface.Hover",
            "Tcc.ReferenceMaster.Brush.Surface.Pressed",
            "Tcc.ReferenceMaster.Brush.Surface.Selected",
            "Tcc.ReferenceMaster.Brush.Surface.Disabled",
            "Tcc.ReferenceMaster.Brush.Status.Warning",
            "Tcc.ReferenceMaster.Brush.Status.Permission",
            "Tcc.ReferenceMaster.Brush.Status.Critical",
            "Tcc.ReferenceMaster.Brush.Status.Information",
            "Tcc.ReferenceMaster.Brush.Status.Unavailable",
            "Tcc.ReferenceMaster.Brush.Status.Success",
            "Tcc.ReferenceMaster.Brush.Status.Reserved",
        ];
        Assert.All(required, key => Assert.Contains(key, keys));
    }

    [Fact]
    public void TypographyRolesMatchTheApprovedP2Inventory()
    {
        HashSet<string> keys = ResourceKeys();
        string[] roles =
        [
            "AppBrand", "WorkspaceSelector", "PageTitle", "PageSubtitle", "PanelTitle",
            "PrimaryStatus", "SecondaryStatus", "Body", "TableHeader", "TableCell",
            "Caption", "Microcopy", "DecorativeChinese",
        ];
        Assert.All(roles, role =>
        {
            Assert.Contains($"Tcc.ReferenceMaster.FontSize.{role}", keys);
            Assert.Contains($"Tcc.ReferenceMaster.LineHeight.{role}", keys);
            Assert.Contains($"Tcc.ReferenceMaster.Text.{role}", keys);
        });
    }

    [Fact]
    public void TextScaleUpdatesPairedMetricsAndExpandsTheScrollableCanvasWithoutChangingGeometryRatios()
    {
        XDocument window = XDocument.Load(MainWindowPath);
        XElement scroller = GetByAutomationId(window, "MainContentScroller");
        Assert.Equal("Auto", scroller.Attribute("HorizontalScrollBarVisibility")?.Value);
        Assert.Equal("Auto", scroller.Attribute("VerticalScrollBarVisibility")?.Value);
        Assert.Equal("Both", scroller.Attribute("PanningMode")?.Value);
        Assert.Equal("True", scroller.Attribute("Focusable")?.Value);
        Assert.Equal("True", scroller.Attribute("IsTabStop")?.Value);
        Assert.Equal("False", scroller.Attribute("CanContentScroll")?.Value);

        XElement referenceCanvas = Assert.Single(scroller.Elements(Presentation + "Canvas"));
        Assert.Equal("1672", referenceCanvas.Attribute("Width")?.Value);
        Assert.Equal("941", referenceCanvas.Attribute("Height")?.Value);
        Assert.Empty(scroller.Descendants(Presentation + "Viewbox"));

        string codeBehind = File.ReadAllText(Path.Combine(HostRoot, "MainWindow.xaml.cs"));
        Assert.Contains("Tcc.ReferenceMaster.LineHeight.PageTitle", codeBehind, StringComparison.Ordinal);
        Assert.Contains("_visualResources[entry.Key] = entry.Value * scale", codeBehind, StringComparison.Ordinal);
        Assert.Contains("Tcc.ReferenceMaster.Viewport.MinWidth\"] = 1672d", codeBehind, StringComparison.Ordinal);
        Assert.Contains("Tcc.ReferenceMaster.Viewport.MinHeight\"] = 941d", codeBehind, StringComparison.Ordinal);
        Assert.DoesNotContain("Math.Max(1d, scale)", codeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void ModuleStylesRemainDistinctAndMapToTheApprovedModuleInventory()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP2VisualTests), nameof(ModuleStylesRemainDistinctAndMapToTheApprovedModuleInventory));
    }

    [Fact]
    public void P1GeometryAuthorityAndSteppedRightEdgeRemainLocked()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP2VisualTests), nameof(P1GeometryAuthorityAndSteppedRightEdgeRemainLocked));
    }

    [Fact]
    public void P2KeepsCharacterReservationWhileP3AddsApprovedSceneContactWithoutEffects()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP2VisualTests), nameof(P2KeepsCharacterReservationWhileP3AddsApprovedSceneContactWithoutEffects));
    }

    [Fact]
    public void InteractionStatesAndVectorIconFamilyAreExplicitAndNonTextual()
    {
        string controls = File.ReadAllText(Path.Combine(ResourceRoot, "ReferenceMaster.Controls.xaml"));
        Assert.Contains("Property=\"IsMouseOver\" Value=\"True\"", controls, StringComparison.Ordinal);
        Assert.Contains("Property=\"IsPressed\" Value=\"True\"", controls, StringComparison.Ordinal);
        Assert.Contains("Property=\"IsEnabled\" Value=\"False\"", controls, StringComparison.Ordinal);
        Assert.Contains("Tcc.ReferenceMaster.NavigationItem.Selected", controls, StringComparison.Ordinal);
        Assert.Contains("FocusVisualStyle", controls, StringComparison.Ordinal);

        XDocument icons = XDocument.Load(Path.Combine(ResourceRoot, "ReferenceMaster.Icons.xaml"));
        XElement[] geometries = icons.Root!.Elements(Presentation + "Geometry").ToArray();
        Assert.True(geometries.Length >= 18);
        Assert.All(geometries, geometry => Assert.StartsWith("Tcc.ReferenceMaster.IconGeometry.",
            geometry.Attribute(Xaml + "Key")?.Value, StringComparison.Ordinal));
        string window = File.ReadAllText(MainWindowPath);
        Assert.DoesNotContain("PackIcon", window, StringComparison.Ordinal);
        Assert.DoesNotContain("Segoe MDL2 Assets", window, StringComparison.Ordinal);
    }

    [Fact]
    public void HumanReviewTypographyCalibrationKeepsPanelTitlesCompactAndSingleLine()
    {
        XDocument typography = XDocument.Load(Path.Combine(ResourceRoot, "ReferenceMaster.Typography.xaml"));
        XElement panelSize = GetResourceByKey(typography, "Tcc.ReferenceMaster.FontSize.PanelTitle");
        XElement panelLineHeight = GetResourceByKey(typography, "Tcc.ReferenceMaster.LineHeight.PanelTitle");
        XElement panelStyle = GetResourceByKey(typography, "Tcc.ReferenceMaster.Text.PanelTitle");

        Assert.Equal("14", panelSize.Value);
        Assert.Equal("19", panelLineHeight.Value);
        Assert.Contains(panelStyle.Elements(Presentation + "Setter"), setter =>
            setter.Attribute("Property")?.Value == "FontWeight" && setter.Attribute("Value")?.Value == "Medium");
        Assert.Contains(panelStyle.Elements(Presentation + "Setter"), setter =>
            setter.Attribute("Property")?.Value == "TextWrapping" && setter.Attribute("Value")?.Value == "NoWrap");
    }

    [Fact]
    public void HumanReviewSurfaceFamiliesUseSceneCompatibleGradientLayersAndDistinctModuleMappings()
    {
        XDocument brushes = XDocument.Load(Path.Combine(ResourceRoot, "ReferenceMaster.Brushes.xaml"));
        string[] gradientKeys =
        [
            "Tcc.ReferenceMaster.Brush.Surface.Navigation",
            "Tcc.ReferenceMaster.Brush.Surface.WorkspacePrimary",
            "Tcc.ReferenceMaster.Brush.Surface.WorkspaceSecondary",
            "Tcc.ReferenceMaster.Brush.Surface.Safety",
            "Tcc.ReferenceMaster.Brush.Surface.Auxiliary",
            "Tcc.ReferenceMaster.Brush.Surface.Action",
        ];
        Assert.All(gradientKeys, key =>
            Assert.Equal(Presentation + "LinearGradientBrush", GetResourceByKey(brushes, key).Name));

        XDocument modules = XDocument.Load(Path.Combine(ResourceRoot, "ReferenceMaster.ModuleStyles.xaml"));
        AssertStyleSetter(modules, "Tcc.ReferenceMaster.Module.Observation", "Background",
            "{DynamicResource Tcc.ReferenceMaster.Brush.Surface.Scene.DensePrimary}");
        AssertStyleSetter(modules, "Tcc.ReferenceMaster.Module.Positions", "Background",
            "{DynamicResource Tcc.ReferenceMaster.Brush.Surface.Scene.DenseSecondary}");
        AssertStyleSetter(modules, "Tcc.ReferenceMaster.Module.AiSummary", "Background",
            "{DynamicResource Tcc.ReferenceMaster.Brush.Surface.Scene.DenseQuiet}");
        AssertStyleSetter(modules, "Tcc.ReferenceMaster.SafetyTile", "Background",
            "{DynamicResource Tcc.ReferenceMaster.Brush.Surface.Scene.LowDensity}");
    }

    [Fact]
    public void HumanReviewNavigationStateUsesAnEdgeIndicatorWithoutDisabledLookingInactiveItems()
    {
        XDocument controls = XDocument.Load(Path.Combine(ResourceRoot, "ReferenceMaster.Controls.xaml"));
        XElement selected = GetResourceByKey(controls, "Tcc.ReferenceMaster.NavigationItem.Selected");
        Assert.Contains(selected.Elements(Presentation + "Setter"), setter =>
            setter.Attribute("Property")?.Value == "BorderBrush" &&
            setter.Attribute("Value")?.Value == "{DynamicResource Tcc.ReferenceMaster.Brush.Border.Selected}");

        XElement navigation = GetResourceByKey(controls, "Tcc.ReferenceMaster.NavigationItem");
        XElement disabled = Assert.Single(navigation.Descendants(Presentation + "Trigger"), trigger =>
            trigger.Attribute("Property")?.Value == "IsEnabled" && trigger.Attribute("Value")?.Value == "False");
        Assert.Contains(disabled.Elements(Presentation + "Setter"), setter =>
            setter.Attribute("Property")?.Value == "Opacity" && setter.Attribute("Value")?.Value == "0.94");
    }

    [Fact]
    public void HumanReviewInternalFootprintsAndHeaderFinishAreExplicit()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP2VisualTests), nameof(HumanReviewInternalFootprintsAndHeaderFinishAreExplicit));
    }

    [Fact]
    public void HumanReviewR2TitleSafetyIconsAndHeaderFinishMatchTheApprovedDirection()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP2VisualTests), nameof(HumanReviewR2TitleSafetyIconsAndHeaderFinishMatchTheApprovedDirection));
    }

    [Fact]
    public void HumanReviewR3ActionSafetyMentalAndRingRepairsStayInsideAcceptedGeometry()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP2VisualTests), nameof(HumanReviewR3ActionSafetyMentalAndRingRepairsStayInsideAcceptedGeometry));
    }

    [Fact]
    public void HumanReviewR5ActionRuntimeFootprintAndSealAuthenticityStayInsideAcceptedGeometry()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP2VisualTests), nameof(HumanReviewR5ActionRuntimeFootprintAndSealAuthenticityStayInsideAcceptedGeometry));
    }

    private static IEnumerable<XDocument> LoadResourceDocuments() => ExpectedDictionaries
        .Select(source => XDocument.Load(Path.Combine(
            HostRoot,
            source.Replace('/', Path.DirectorySeparatorChar))));

    private static HashSet<string> ResourceKeys() => LoadResourceDocuments()
        .SelectMany(document => document.Root!.Elements())
        .Select(item => item.Attribute(Xaml + "Key")?.Value)
        .Where(item => item is not null)
        .Cast<string>()
        .ToHashSet(StringComparer.Ordinal);

    private static XElement GetResourceByKey(XDocument document, string key) =>
        Assert.Single(document.Root!.Elements(), element => element.Attribute(Xaml + "Key")?.Value == key);

    private static void AssertStyleSetter(XDocument document, string styleKey, string property, string value)
    {
        XElement style = GetResourceByKey(document, styleKey);
        Assert.Contains(style.Elements(Presentation + "Setter"), setter =>
            setter.Attribute("Property")?.Value == property && setter.Attribute("Value")?.Value == value);
    }

    private static XElement GetByAutomationId(XDocument document, string automationId) =>
        Assert.Single(document.Descendants(), element => element.Attributes().Any(attribute =>
            attribute.Name.LocalName == "AutomationProperties.AutomationId" &&
            attribute.Value == automationId));
}
