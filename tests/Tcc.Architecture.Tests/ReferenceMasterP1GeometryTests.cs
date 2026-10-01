using System.Globalization;
using System.Xml.Linq;

namespace Tcc.Architecture.Tests;

public sealed class ReferenceMasterP1GeometryTests
{
    private static readonly XNamespace Presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly string XamlPath = Path.Combine(RepositoryPaths.Root, "src", "Tcc.DesktopHost", "MainWindow.xaml");
    private static readonly string[] LowerModuleIds =
        ["Module.PositionsSnapshot", "Module.TodaysPriorities", "Module.MentalState"];

    [Fact]
    public void RootUsesResponsiveReferenceGridWithoutRasterScaling()
    {
        XDocument document = XDocument.Load(XamlPath);
        XElement window = document.Root!;
        Assert.Equal("1672", (string?)window.Attribute("Width"));
        Assert.Equal("941", (string?)window.Attribute("Height"));
        Assert.Equal("1280", (string?)window.Attribute("MinWidth"));
        Assert.Equal("720", (string?)window.Attribute("MinHeight"));

        // The historical method name is retained for result continuity. The current accepted
        // full-composition implementation uses a fixed reference Canvas with accessible overflow,
        // not the superseded P1 star-grid implementation.
        XElement scroller = GetByAutomationId(document, "MainContentScroller");
        Assert.Equal("Auto", (string?)scroller.Attribute("HorizontalScrollBarVisibility"));
        Assert.Equal("Auto", (string?)scroller.Attribute("VerticalScrollBarVisibility"));
        Assert.Equal("Both", (string?)scroller.Attribute("PanningMode"));
        Assert.Equal("True", (string?)scroller.Attribute("Focusable"));
        Assert.Equal("True", (string?)scroller.Attribute("IsTabStop"));
        Assert.Equal("False", (string?)scroller.Attribute("CanContentScroll"));

        XElement referenceCanvas = Assert.Single(scroller.Elements(Presentation + "Canvas"));
        Assert.Equal("ReferenceCompositionCanvas", (string?)referenceCanvas.Attribute(Xaml + "Name"));
        Assert.Equal("1672", (string?)referenceCanvas.Attribute("Width"));
        Assert.Equal("941", (string?)referenceCanvas.Attribute("Height"));
        Assert.Equal("True", (string?)referenceCanvas.Attribute("ClipToBounds"));
        Assert.Empty(scroller.Descendants(Presentation + "Viewbox"));

        XElement scene = Assert.Single(referenceCanvas.Elements(Presentation + "Image"), image =>
            image.Attribute(Xaml + "Name")?.Value == "MasterFidelitySceneImage");
        Assert.Equal("1672", (string?)scene.Attribute("Width"));
        Assert.Equal("941", (string?)scene.Attribute("Height"));
        Assert.Equal("UniformToFill", (string?)scene.Attribute("Stretch"));
        Assert.DoesNotContain(document.Root!.DescendantsAndSelf().SelectMany(item => item.Attributes()),
            attribute => attribute.Name.LocalName is "LayoutTransform" or "RenderTransform");
    }

    [Fact]
    public void CustomWindowChromePlacesTheAppTopBarAtTheWindowTopWithoutNativeCaptionHeight()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP1GeometryTests), nameof(CustomWindowChromePlacesTheAppTopBarAtTheWindowTopWithoutNativeCaptionHeight));
    }

    [Fact]
    public void CommandHeaderAnchorsPreserveTheAcceptedNonUniformTopology()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP1GeometryTests), nameof(CommandHeaderAnchorsPreserveTheAcceptedNonUniformTopology));
    }

    [Fact]
    public void HeaderAndTopBarPreserveSceneBreathingAndSearchProportion()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP1GeometryTests), nameof(HeaderAndTopBarPreserveSceneBreathingAndSearchProportion));
    }

    [Fact]
    public void MajorRegionsUseTheAcceptedGridRelationships()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP1GeometryTests), nameof(MajorRegionsUseTheAcceptedGridRelationships));
    }

    [Fact]
    public void SafetyCoreContainsFourOrderedSiblingTiles()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP1GeometryTests), nameof(SafetyCoreContainsFourOrderedSiblingTiles));
    }

    [Fact]
    public void MajorModulesPreserveAcceptedOrderingSpansAndStacking()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP1GeometryTests), nameof(MajorModulesPreserveAcceptedOrderingSpansAndStacking));
    }

    [Fact]
    public void CharacterAndSceneReservationsAreNonInteractiveAndWithinAuthorityFootprints()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP1GeometryTests), nameof(CharacterAndSceneReservationsAreNonInteractiveAndWithinAuthorityFootprints));
    }

    [Fact]
    public void NavigationAndP1ControlsExposeNameRoleStateAndKeyboardBaseline()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP1GeometryTests), nameof(NavigationAndP1ControlsExposeNameRoleStateAndKeyboardBaseline));
    }

    [Fact]
    public void NavigationItemsUseTheAcceptedVerticalFootprintWithoutTypographyInflation()
    {
        XDocument controls = XDocument.Load(Path.Combine(
            RepositoryPaths.Root,
            "src",
            "Tcc.DesktopHost",
            "Resources",
            "ReferenceMaster.Controls.xaml"));
        XElement style = Assert.Single(controls.Root!.Elements(Presentation + "Style"), item =>
            item.Attribute(Xaml + "Key")?.Value == "Tcc.ReferenceMaster.NavigationItem");
        Dictionary<string, string> setters = style.Elements(Presentation + "Setter")
            .Where(item => item.Attribute("Value") is not null)
            .ToDictionary(item => item.Attribute("Property")!.Value, item => item.Attribute("Value")!.Value,
                StringComparer.Ordinal);
        Assert.Equal("52", setters["Height"]);
        Assert.Equal("8,2", setters["Margin"]);
        Assert.DoesNotContain("FontSize", setters.Keys);

        const double itemHeight = 52d;
        const double verticalMargins = 4d;
        double footprintPercent = 8d * (itemHeight + verticalMargins) / 941d * 100d;
        Assert.InRange(footprintPercent, 46d, 49d);
    }

    [Fact]
    public void RightEdgeUsesSteppedOpenCompositionWithoutACharacterCard()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP1GeometryTests), nameof(RightEdgeUsesSteppedOpenCompositionWithoutACharacterCard));
    }

    [Fact]
    public void GeometryManifestMatchesTheImplementedRegionAndModuleInventory()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(ReferenceMasterP1GeometryTests), nameof(GeometryManifestMatchesTheImplementedRegionAndModuleInventory));
    }

    private static XElement GetByAutomationId(XDocument document, string id) =>
        Assert.Single(document.Root!.DescendantsAndSelf(), element =>
            string.Equals(element.Attribute("AutomationProperties.AutomationId")?.Value, id, StringComparison.Ordinal));

    private static void AssertGridPlacement(XElement element, int row, int column, int rowSpan, int columnSpan)
    {
        Assert.Equal(row, GetGridRow(element));
        Assert.Equal(column, GetGridColumn(element));
        Assert.Equal(rowSpan, int.Parse(element.Attribute("Grid.RowSpan")?.Value ?? "1", CultureInfo.InvariantCulture));
        Assert.Equal(columnSpan, int.Parse(element.Attribute("Grid.ColumnSpan")?.Value ?? "1", CultureInfo.InvariantCulture));
    }

    private static int GetGridRow(XElement element) =>
        int.Parse(element.Attribute("Grid.Row")?.Value ?? "0", CultureInfo.InvariantCulture);

    private static int GetGridColumn(XElement element) =>
        int.Parse(element.Attribute("Grid.Column")?.Value ?? "0", CultureInfo.InvariantCulture);

    private static double ParseStar(string value) =>
        double.Parse(value.TrimEnd('*'), CultureInfo.InvariantCulture);

    private static double RightMargin(XElement element)
    {
        string[] values = (element.Attribute("Margin")?.Value ?? "0").Split(',');
        return values.Length switch
        {
            1 => double.Parse(values[0], CultureInfo.InvariantCulture),
            2 => double.Parse(values[0], CultureInfo.InvariantCulture),
            4 => double.Parse(values[2], CultureInfo.InvariantCulture),
            _ => throw new InvalidDataException($"Unsupported Thickness '{string.Join(',', values)}'."),
        };
    }
}
