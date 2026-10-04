using System.Security.Cryptography;
using System.Xml.Linq;

namespace Tcc.Architecture.Tests;

internal static class CurrentHomeAuthorityContract
{
    private static readonly XNamespace P = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace X = "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly string Root = RepositoryPaths.Root;
    private static readonly string Host = Path.Combine(Root, "src", "Tcc.DesktopHost");
    private static readonly string XamlPath = Path.Combine(Host, "MainWindow.xaml");
    private static readonly Lazy<string[]> CustodyViolations = new(FindCustodyViolations);

    private static readonly string[] CurrentAutomationIds =
    [
        "Editorial.RightMaxim", "MainContentScroller", "NavHome", "NavMarkets", "NavPlanning",
        "NavPositions", "NavReview", "NavRisk", "NavSettings", "Region.HeaderIdentity",
        "Region.HomeSafetyCore", "Region.MarketOverview", "Region.MentalState", "Region.Priorities",
        "Region.RecentActivity", "Region.Watchlist", "TabAddSymbol", "TabBnbUsdt", "TabBtcUsdt",
        "TabEthUsdt", "TabSolUsdt", "TabXrpUsdt", "Timeframe1D", "Timeframe1H", "Timeframe1W",
        "Timeframe4H", "WindowCloseButton", "WindowMaximizeRestoreButton", "WindowMinimizeButton",
    ];

    public static void AssertReconciled(string legacySuite, string legacyTest)
    {
        AssertCurrentSupersessionChain(legacySuite, legacyTest);
        AssertCurrentStructure();
        AssertCurrentSceneAndAssets();
        AssertCurrentMaterials();
        AssertCurrentAccessibility();
        AssertCurrentProductTruth();

        if (ContainsAny(legacyTest, "Asset", "Byte", "Resource", "Hash", "Custody", "Raster", "Compile",
                "Scene", "Background", "Character", "Foreground", "Terrain", "Architecture", "Material"))
        {
            Assert.Empty(CustodyViolations.Value);
        }
    }

    private static void AssertCurrentSupersessionChain(string legacySuite, string legacyTest)
    {
        Assert.False(string.IsNullOrWhiteSpace(legacySuite));
        Assert.False(string.IsNullOrWhiteSpace(legacyTest));
        string decisions = File.ReadAllText(Path.Combine(Root, "docs", "CODEX_DECISIONS.md"));
        Assert.Contains("## TCC-DEC-2026-09-27-160", decisions, StringComparison.Ordinal);
        Assert.Contains("## TCC-DEC-2026-09-27-161", decisions, StringComparison.Ordinal);
        Assert.Contains("## TCC-DEC-2026-09-29-199", decisions, StringComparison.Ordinal);
        Assert.Contains("## TCC-DEC-2026-10-01-222", decisions, StringComparison.Ordinal);
        Assert.Contains("Decision151–159", decisions, StringComparison.Ordinal);
        Assert.Contains("被此決策取代的歷史視覺／asset custody contracts", decisions, StringComparison.Ordinal);
    }

    private static void AssertCurrentStructure()
    {
        XDocument document = XDocument.Load(XamlPath);
        XElement window = document.Root!;
        Assert.Equal("1672", window.Attribute("Width")?.Value);
        Assert.Equal("941", window.Attribute("Height")?.Value);
        Assert.Equal("1280", window.Attribute("MinWidth")?.Value);
        Assert.Equal("720", window.Attribute("MinHeight")?.Value);

        XElement scroller = ByAutomationId(document, "MainContentScroller");
        Assert.Equal("Auto", scroller.Attribute("HorizontalScrollBarVisibility")?.Value);
        Assert.Equal("Auto", scroller.Attribute("VerticalScrollBarVisibility")?.Value);
        Assert.Equal("False", scroller.Attribute("CanContentScroll")?.Value);
        Assert.Equal("Both", scroller.Attribute("PanningMode")?.Value);
        Assert.Equal("True", scroller.Attribute("Focusable")?.Value);
        Assert.Equal("True", scroller.Attribute("IsTabStop")?.Value);

        XElement canvas = Assert.Single(document.Descendants(P + "Canvas"), element =>
            element.Attribute(X + "Name")?.Value == "ReferenceCompositionCanvas");
        Assert.Equal("1672", canvas.Attribute("Width")?.Value);
        Assert.Equal("941", canvas.Attribute("Height")?.Value);
        Assert.Empty(document.Descendants(P + "Viewbox"));
        Assert.Empty(document.Descendants(P + "LayoutTransform"));
        Assert.DoesNotContain(document.Descendants(P + "ScaleTransform"), element =>
            element.Attribute("ScaleX")?.Value == element.Attribute("ScaleY")?.Value);

        AssertGeometry(ByName(document, "TopChromeRegion"), "0", "0", "1672", "52");
        AssertGeometry(AncestorCanvasBorder(ByAutomationId(document, "Region.MarketOverview")), "126", "239", "870", "404");
        AssertGeometry(AncestorCanvasBorder(ByAutomationId(document, "Region.Watchlist")), "1001", "239", "251", "404");
        AssertGeometry(AncestorCanvasBorder(ByAutomationId(document, "Region.Priorities")), "126", "654", "363", "260");
        AssertGeometry(AncestorCanvasBorder(ByAutomationId(document, "Region.MentalState")), "492", "654", "382", "260");
        AssertGeometry(AncestorCanvasBorder(ByAutomationId(document, "Region.RecentActivity")), "879", "654", "373", "260");
        AssertGeometry(ByName(document, "RightEditorialMarksRegion"), "1538", "102", "130", "190");
    }

    private static void AssertCurrentSceneAndAssets()
    {
        XDocument document = XDocument.Load(XamlPath);
        XElement scene = Assert.Single(document.Descendants(P + "Image"), image =>
            image.Attribute(X + "Name")?.Value == "MasterFidelitySceneImage");
        Assert.Equal("Assets/MasterFidelity/StrataObservatory.MasterR1.Scene.R26.png", scene.Attribute("Source")?.Value);
        Assert.Equal("UniformToFill", scene.Attribute("Stretch")?.Value);
        Assert.Equal("False", scene.Attribute("IsHitTestVisible")?.Value);
        Assert.Equal("B4C4B7225D75C5A3408E8BF683EEDA7756C6E4398560021BC1F4FE2996C385C2",
            Hash(Path.Combine(Host, "Assets", "MasterFidelity", "StrataObservatory.MasterR1.Scene.R26.png")));
        Assert.Equal("EA15A6E24FEE80A3395DDE775E52F340FC2A64AFF6DA40EC1C08C2FCBDFE20C1",
            Hash(Path.Combine(Host, "Assets", "StrataObservatory", "StrataObservatory.MentalStateSuite.R1.png")));

        string source = File.ReadAllText(XamlPath);
        string[] forbiddenRuntimeMarkers =
        [
            "Assets/RebuildR1/", "Assets/RebuildR2/", "Assets/Scene/", "Assets/Character/", "Assets/Home/",
            "Resources/RebuildR2.Foundation.xaml", "Resources/AutonomousR1.Design.xaml",
            "SceneBackgroundImage", "SceneCharacterImage", "ForegroundLeftBank", "ForegroundRightAccent",
            "Region.ReferenceMasterViewport", "Region.CharacterReserved",
        ];
        Assert.All(forbiddenRuntimeMarkers, marker => Assert.DoesNotContain(marker, source, StringComparison.Ordinal));

        XDocument project = XDocument.Load(Path.Combine(Host, "Tcc.DesktopHost.csproj"));
        string[] resources = project.Descendants().Where(element => element.Name.LocalName == "Resource")
            .Select(element => element.Attribute("Include")?.Value).OfType<string>().ToArray();
        Assert.Equal([
            "Assets/Brand/Tcc.NocturneMeridian.AppIcon.ico",
            "Assets/Brand/Tcc.NocturneMeridian.AppIcon.Window.png",
            "Assets/MasterFidelity/StrataObservatory.MasterR1.Scene.R26.png",
            "Assets/Plan/Tcc.PlanDashboard.HomeStyle.Background.R1.png",
            "Assets/Risk/Tcc.RiskPermission.GuardedPass.Background.R1.png",
            "Assets/StrataObservatory/StrataObservatory.MentalStateSuite.R1.png",
        ], resources);
    }

    private static void AssertCurrentMaterials()
    {
        XDocument document = XDocument.Load(XamlPath);
        string[] sources = document.Descendants(P + "ResourceDictionary")
            .Select(item => item.Attribute("Source")?.Value).OfType<string>().ToArray();
        Assert.Equal([
            "Resources/StrataObservatory.Design.xaml",
            "Resources/StrataObservatory.Iconography.xaml",
        ], sources);

        XElement[] resources = sources.Select(source => XDocument.Load(Path.Combine(
                Host, source.Replace('/', Path.DirectorySeparatorChar))))
            .SelectMany(dictionary => dictionary.Root!.Elements()).ToArray();
        string[] keys = resources.Select(element => element.Attribute(X + "Key")?.Value)
            .OfType<string>().ToArray();
        Assert.Equal(keys.Length, keys.Distinct(StringComparer.Ordinal).Count());
        Assert.Contains("Tcc.Strata.Brush.Background", keys);
        Assert.Contains("Tcc.Strata.Brush.Panel.Safety", keys);
        Assert.Contains("Strata.Panel.Primary", keys);
        Assert.Contains("Strata.NavGlyph.Home", keys);
        Assert.Contains("Strata.Seal.Vertical", keys);
        Assert.Contains("Strata.Seal.Square", keys);
        Assert.DoesNotContain("DropShadowEffect", document.ToString(SaveOptions.DisableFormatting), StringComparison.Ordinal);
        Assert.DoesNotContain("BlurEffect", document.ToString(SaveOptions.DisableFormatting), StringComparison.Ordinal);
    }

    private static void AssertCurrentAccessibility()
    {
        XDocument document = XDocument.Load(XamlPath);
        XElement[] automated = document.Descendants().Where(element =>
            element.Attributes().Any(attribute => attribute.Name.LocalName == "AutomationProperties.AutomationId")).ToArray();
        string[] ids = automated.Select(element => Attribute(element, "AutomationProperties.AutomationId")!)
            .Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(CurrentAutomationIds, ids);
        Assert.All(automated, element => Assert.False(string.IsNullOrWhiteSpace(
            Attribute(element, "AutomationProperties.Name"))));

        foreach (string id in new[] { "WindowMinimizeButton", "WindowMaximizeRestoreButton", "WindowCloseButton" })
        {
            XElement button = ByAutomationId(document, id);
            Assert.False(string.IsNullOrWhiteSpace(button.Attribute("Click")?.Value));
            Assert.NotEqual("False", button.Attribute("IsEnabled")?.Value);
        }
        foreach (string id in new[] { "NavHome", "NavPlanning", "NavRisk" })
        {
            XElement navigation = ByAutomationId(document, id);
            Assert.NotEqual("False", navigation.Attribute("IsEnabled")?.Value);
            Assert.NotEqual("False", navigation.Attribute("IsTabStop")?.Value);
            Assert.False(string.IsNullOrWhiteSpace(navigation.Attribute("Click")?.Value));
        }
    }

    private static void AssertCurrentProductTruth()
    {
        string source = File.ReadAllText(XamlPath);
        Assert.Contains("DATA OFFLINE", source, StringComparison.Ordinal);
        Assert.Contains("READ ONLY", source, StringComparison.Ordinal);
        Assert.Contains("UNKNOWN", source, StringComparison.Ordinal);
        Assert.Contains("No live data", source, StringComparison.Ordinal);
        Assert.Contains("No execution API", source, StringComparison.Ordinal);
        Assert.Contains("No priorities loaded", source, StringComparison.Ordinal);
        Assert.Contains("timestamps unavailable", source, StringComparison.Ordinal);
        Assert.DoesNotContain("automatic trading", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("broker execution", source, StringComparison.OrdinalIgnoreCase);
    }

    private static string[] FindCustodyViolations()
    {
        List<string> violations = [];
        string manifest = Path.Combine(Root, "tests", "Tcc.Architecture.Tests", "HomeLegacyVisualCustody.sha256");
        foreach (string line in File.ReadAllLines(manifest).Where(line =>
                     !string.IsNullOrWhiteSpace(line) && !line.StartsWith('#')))
        {
            string[] fields = line.Split("  ", 2, StringSplitOptions.None);
            if (fields.Length != 2)
            {
                violations.Add("malformed custody line: " + line);
                continue;
            }
            string path = Path.Combine(Root, fields[1].Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path)) violations.Add("missing: " + fields[1]);
            else if (!StringComparer.Ordinal.Equals(fields[0], Hash(path))) violations.Add("hash: " + fields[1]);
        }
        return violations.ToArray();
    }

    private static XElement ByAutomationId(XDocument document, string id) => Assert.Single(
        document.Descendants(), element =>
            Attribute(element, "AutomationProperties.AutomationId") == id);

    private static XElement ByName(XDocument document, string name) => Assert.Single(
        document.Descendants(), element => element.Attribute(X + "Name")?.Value == name);

    private static XElement AncestorCanvasBorder(XElement element) => Assert.Single(
        element.Ancestors(P + "Border"), ancestor => ancestor.Attribute("Canvas.Left") is not null);

    private static string? Attribute(XElement element, string localName) => element.Attributes()
        .SingleOrDefault(attribute => attribute.Name.LocalName == localName)?.Value;

    private static void AssertGeometry(XElement element, string left, string top, string width, string height)
    {
        Assert.Equal(left, element.Attribute("Canvas.Left")?.Value);
        Assert.Equal(top, element.Attribute("Canvas.Top")?.Value);
        Assert.Equal(width, element.Attribute("Width")?.Value);
        Assert.Equal(height, element.Attribute("Height")?.Value);
    }

    private static bool ContainsAny(string value, params string[] terms) => terms.Any(term =>
        value.Contains(term, StringComparison.OrdinalIgnoreCase));

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
}
