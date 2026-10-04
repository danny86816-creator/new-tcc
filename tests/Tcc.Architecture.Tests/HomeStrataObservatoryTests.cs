using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Tcc.Architecture.Tests;

public sealed class HomeStrataObservatoryTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string MainWindowPath = Path.Combine(RepositoryRoot, "src", "Tcc.DesktopHost", "MainWindow.xaml");
    private static readonly string DesignPath = Path.Combine(RepositoryRoot, "src", "Tcc.DesktopHost", "Resources", "StrataObservatory.Design.xaml");
    private static readonly string IconographyPath = Path.Combine(RepositoryRoot, "src", "Tcc.DesktopHost", "Resources", "StrataObservatory.Iconography.xaml");
    private static readonly string CodeBehindPath = Path.Combine(RepositoryRoot, "src", "Tcc.DesktopHost", "MainWindow.xaml.cs");
    private static readonly string RuntimeCapturePath = Path.Combine(RepositoryRoot, "automation", "capture_strata_matrix_r1.ps1");
    private static readonly string ProjectPath = Path.Combine(RepositoryRoot, "src", "Tcc.DesktopHost", "Tcc.DesktopHost.csproj");
    private static readonly string AppIconPath = Path.Combine(RepositoryRoot, "src", "Tcc.DesktopHost", "Assets", "Brand", "Tcc.NocturneMeridian.AppIcon.ico");
    private static readonly string AppIconWindowPath = Path.Combine(RepositoryRoot, "src", "Tcc.DesktopHost", "Assets", "Brand", "Tcc.NocturneMeridian.AppIcon.Window.png");
    private static readonly string AppIconSvgPath = Path.Combine(RepositoryRoot, "src", "Tcc.DesktopHost", "Assets", "Brand", "Tcc.NocturneMeridian.AppIcon.svg");
    private static readonly string AppIconSvg32Path = Path.Combine(RepositoryRoot, "src", "Tcc.DesktopHost", "Assets", "Brand", "Tcc.NocturneMeridian.AppIcon.32.svg");
    private static readonly string AppIconSvg16Path = Path.Combine(RepositoryRoot, "src", "Tcc.DesktopHost", "Assets", "Brand", "Tcc.NocturneMeridian.AppIcon.16.svg");
    private static readonly string ScenePath = Path.Combine(RepositoryRoot, "src", "Tcc.DesktopHost", "Assets", "StrataObservatory", "StrataObservatory.TwoSource.R4.png");
    private static readonly string ForegroundPlumPath = Path.Combine(RepositoryRoot, "src", "Tcc.DesktopHost", "Assets", "StrataObservatory", "StrataObservatory.ForegroundPlum.R1.png");
    private static readonly string MentalStateSuitePath = Path.Combine(RepositoryRoot, "src", "Tcc.DesktopHost", "Assets", "StrataObservatory", "StrataObservatory.MentalStateSuite.R1.png");
    [Fact]
    public void HomeUsesOnlyTheStrataObservatoryVisualPackage()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(HomeStrataObservatoryTests), nameof(HomeUsesOnlyTheStrataObservatoryVisualPackage));
    }

    [Fact]
    public void ReferenceGeometryMatchesTheApprovedTargetFrame()
    {
        string xaml = File.ReadAllText(MainWindowPath);

        Assert.Contains("Canvas.Left=\"6\" Canvas.Top=\"52\" Width=\"98\" Height=\"889\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Canvas.Left=\"104\" Canvas.Top=\"52\" Width=\"1154\" Height=\"186\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Canvas.Left=\"126\" Canvas.Top=\"239\" Width=\"870\" Height=\"404\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Canvas.Left=\"1001\" Canvas.Top=\"239\" Width=\"251\" Height=\"404\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Canvas.Left=\"126\" Canvas.Top=\"654\" Width=\"363\" Height=\"260\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Canvas.Left=\"492\" Canvas.Top=\"654\" Width=\"382\" Height=\"260\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Canvas.Left=\"879\" Canvas.Top=\"654\" Width=\"373\" Height=\"260\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void TypographyTransparencyBordersAndLogoAreLocked()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(HomeStrataObservatoryTests), nameof(TypographyTransparencyBordersAndLogoAreLocked));
    }

    [Fact]
    public void MentalStateUsesTheApprovedSuiteWithPresentationOnlySelectionAndPreservesLiveText()
    {
        string xaml = File.ReadAllText(MainWindowPath);
        string design = File.ReadAllText(DesignPath);
        string codeBehind = File.ReadAllText(CodeBehindPath);
        string project = File.ReadAllText(ProjectPath);

        Assert.True(File.Exists(MentalStateSuitePath));
        byte[] png = File.ReadAllBytes(MentalStateSuitePath);
        Assert.True(png.Length > 33);
        Assert.Equal(1774, BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)));
        Assert.Equal(887, BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)));
        Assert.Equal(6, png[25]);
        Assert.Equal("EA15A6E24FEE80A3395DDE775E52F340FC2A64AFF6DA40EC1C08C2FCBDFE20C1", Convert.ToHexString(SHA256.HashData(png)));

        const string source = "Assets/StrataObservatory/StrataObservatory.MentalStateSuite.R1.png";
        Assert.True(Regex.Count(project, Regex.Escape(source), RegexOptions.CultureInvariant) == 1);
        Assert.Equal(1, Regex.Count(xaml, Regex.Escape(source), RegexOptions.CultureInvariant));
        Assert.DoesNotContain("StrataObservatory.MentalState.NotSetGlyph.R2.png", project, StringComparison.Ordinal);
        Assert.DoesNotContain("StrataObservatory.MentalState.NotSetGlyph.R2.png", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("StrataObservatory.MentalState.NotSetGlyph.R1.png", project, StringComparison.Ordinal);
        Assert.DoesNotContain("StrataObservatory.MentalState.NotSetGlyph.R1.png", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("StrataObservatory.MentalStateBrushRing.R1.png", project, StringComparison.Ordinal);
        Assert.DoesNotContain("StrataObservatory.MentalStateBrushRing.R1.png", xaml, StringComparison.Ordinal);

        XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);
        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        (string Key, string SourceRect)[] cells =
        [
            ("Tcc.Strata.MentalStateGlyph.NotSet", "0,0,444,444"),
            ("Tcc.Strata.MentalStateGlyph.Calm", "444,0,443,444"),
            ("Tcc.Strata.MentalStateGlyph.Focused", "887,0,444,444"),
            ("Tcc.Strata.MentalStateGlyph.Cautious", "1331,0,443,444"),
            ("Tcc.Strata.MentalStateGlyph.Stressed", "0,444,444,443"),
            ("Tcc.Strata.MentalStateGlyph.Fatigued", "444,444,443,443"),
            ("Tcc.Strata.MentalStateGlyph.Impulsive", "887,444,444,443"),
        ];
        XElement atlas = document.Descendants(presentation + "BitmapImage")
            .Single(element => element.Attribute(x + "Key")?.Value == "Tcc.Strata.MentalStateAtlas.R1");
        Assert.Equal(source, atlas.Attribute("UriSource")?.Value);
        XElement[] glyphSources = document.Descendants(presentation + "CroppedBitmap").
            Where(element => element.Attribute(x + "Key")?.Value.StartsWith("Tcc.Strata.MentalStateGlyph.", StringComparison.Ordinal) == true).
            ToArray();
        Assert.Equal(cells.Length, glyphSources.Length);
        Assert.All(cells, cell =>
        {
            XElement glyphSource = Assert.Single(glyphSources, element => element.Attribute(x + "Key")?.Value == cell.Key);
            Assert.Equal("{StaticResource Tcc.Strata.MentalStateAtlas.R1}", glyphSource.Attribute("Source")?.Value);
            Assert.Equal(cell.SourceRect, glyphSource.Attribute("SourceRect")?.Value);
        });

        XElement glyph = document.Descendants(presentation + "Image")
            .Single(element => element.Attribute(x + "Name")?.Value == "MentalStateGlyph");
        Assert.Equal("145", glyph.Attribute("Width")?.Value);
        Assert.Equal("145", glyph.Attribute("Height")?.Value);
        Assert.Equal("{DynamicResource Tcc.Strata.MentalStateGlyph.NotSet}", glyph.Attribute("Source")?.Value);
        Assert.Equal("Uniform", glyph.Attribute("Stretch")?.Value);
        Assert.Equal("{DynamicResource Tcc.Strata.MentalStateGlyphOpacity}", glyph.Attribute("Opacity")?.Value);
        Assert.Equal("HighQuality", glyph.Attribute("RenderOptions.BitmapScalingMode")?.Value);
        Assert.Equal("False", glyph.Attribute("Focusable")?.Value);
        Assert.Equal("False", glyph.Attribute("IsHitTestVisible")?.Value);
        Assert.Null(glyph.Attribute("AutomationProperties.AutomationId"));

        Assert.DoesNotContain("MentalStateGlyphKind", codeBehind, StringComparison.Ordinal);
        Assert.Contains("MentalStateValue.Text = \"CALM\";", codeBehind, StringComparison.Ordinal);
        Assert.Contains("MentalStateGlyph.SetValue(", codeBehind, StringComparison.Ordinal);
        Assert.Contains("System.Windows.Controls.Image.SourceProperty", codeBehind, StringComparison.Ordinal);
        Assert.Contains("(ImageSource)FindResource(\"Tcc.Strata.MentalStateGlyph.Calm\")", codeBehind, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"MentalStateValue\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"MentalStateHelper\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Tcc.Strata.Brush.MentalRing.", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Tcc.Strata.Brush.MentalRing.", design, StringComparison.Ordinal);
        Assert.DoesNotContain("Tcc.Strata.Brush.MentalRing.", codeBehind, StringComparison.Ordinal);
        Assert.DoesNotContain("M18,101 C9,76 16,49 34,28", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"NOT SET\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"NO SELF-CHECK\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Tcc.Strata.MentalStateGlyphOpacity\"", design, StringComparison.Ordinal);
        Assert.Contains("_visualResources.Remove(\"Tcc.Strata.MentalStateGlyphOpacity\")", codeBehind, StringComparison.Ordinal);
        Assert.Contains("_visualResources[\"Tcc.Strata.MentalStateGlyphOpacity\"] = 0d", codeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void R26OfflinePlotRetainsAQuietButLegibleAnalyticalScaffold()
    {
        string design = File.ReadAllText(DesignPath);
        string xaml = File.ReadAllText(MainWindowPath);
        string offlineMarket = xaml[
            xaml.IndexOf("x:Name=\"OfflineMarketBody\"", StringComparison.Ordinal)..
            xaml.IndexOf("x:Name=\"OfflineKpiBand\"", StringComparison.Ordinal)];

        Assert.Contains(
            "x:Key=\"Tcc.Strata.Brush.ChartGrid\" Color=\"#345D7481\"",
            design,
            StringComparison.Ordinal);
        Assert.Equal(5, Regex.Count(offlineMarket, "Stroke=\"\\{DynamicResource Tcc\\.Strata\\.Brush\\.ChartGrid\\}\" StrokeThickness=\"1\""));
        Assert.Contains("NO SERIES TO DISPLAY", offlineMarket, StringComparison.Ordinal);
        Assert.Contains("PRICES · DATES · TRENDS REMAIN BLANK", offlineMarket, StringComparison.Ordinal);
        Assert.DoesNotContain("43,287.62", offlineMarket, StringComparison.Ordinal);
    }

    [Fact]
    public void HomeUiPhase2MarketOverviewHasAnHonestReadableHierarchy()
    {
        string design = File.ReadAllText(DesignPath);
        string xaml = File.ReadAllText(MainWindowPath);
        string market = xaml[
            xaml.IndexOf("<!-- Market overview -->", StringComparison.Ordinal)..
            xaml.IndexOf("<!-- Watchlist -->", StringComparison.Ordinal)];
        string offlineMarket = market[
            market.IndexOf("x:Name=\"OfflineMarketBody\"", StringComparison.Ordinal)..
            market.IndexOf("x:Name=\"OfflineKpiBand\"", StringComparison.Ordinal)];

        Assert.Contains("READ-ONLY ANALYSIS", market, StringComparison.Ordinal);
        Assert.Contains("Text=\"SYMBOL\"", market, StringComparison.Ordinal);
        Assert.Contains("Text=\"RANGE\"", market, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.MarketTabButton\"", design, StringComparison.Ordinal);
        Assert.Equal(10, Regex.Count(market, "Style=\"\\{StaticResource Strata\\.MarketTabButton(?:\\.Selected)?\\}\" IsEnabled=\"False\""));
        Assert.Contains("NO VALUES ARE INFERRED", offlineMarket, StringComparison.Ordinal);
        Assert.Contains("PRICES · DATES · TRENDS REMAIN BLANK", offlineMarket, StringComparison.Ordinal);
        Assert.Equal(5, Regex.Count(offlineMarket, "Stroke=\"\\{DynamicResource Tcc\\.Strata\\.Brush\\.ChartGrid\\}\" StrokeThickness=\"1\""));
        Assert.Equal(4, Regex.Count(offlineMarket, "Stroke=\"\\{DynamicResource Tcc\\.Strata\\.Brush\\.BorderQuiet\\}\" StrokeThickness=\"1\" Opacity=\"0\\.32\""));
        Assert.Contains("X1=\"48\" Y1=\"12\" X2=\"48\" Y2=\"176\"", offlineMarket, StringComparison.Ordinal);
        Assert.DoesNotContain("43,287.62", offlineMarket, StringComparison.Ordinal);
        Assert.DoesNotContain("Jan 10", offlineMarket, StringComparison.Ordinal);
    }

    [Fact]
    public void FourthRefinementUsesHonestEmptyStateAndNativeVectorCraft()
    {
        string design = File.ReadAllText(DesignPath);
        string iconography = File.ReadAllText(IconographyPath);
        string xaml = File.ReadAllText(MainWindowPath);
        string codeBehind = File.ReadAllText(CodeBehindPath);

        Assert.Contains("NO SERIES TO DISPLAY", xaml, StringComparison.Ordinal);
        Assert.Contains("NO VALUES ARE INFERRED", xaml, StringComparison.Ordinal);
        Assert.Contains("PRICES · DATES · TRENDS REMAIN BLANK", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource Strata.ChartOfflineGlyph}\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Data=\"M48,148 C120,148", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Content=\"—\" Style=\"{StaticResource Strata.WindowButton}\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Content=\"□\" Style=\"{StaticResource Strata.WindowButton}\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Content=\"×\" Style=\"{StaticResource Strata.WindowButton}\"", xaml, StringComparison.Ordinal);

        string[] requiredStyles =
        [
            "Strata.WindowGlyph.Minimize",
            "Strata.WindowGlyph.Maximize",
            "Strata.WindowGlyph.Close",
            "Strata.NavGlyph.Home",
            "Strata.NavGlyph.Market",
            "Strata.NavGlyph.Plan",
            "Strata.NavGlyph.Risk",
            "Strata.NavGlyph.Positions",
            "Strata.NavGlyph.Review",
            "Strata.NavGlyph.Settings",
            "Strata.ChartOfflineGlyph",
            "Strata.CraftFrame",
        ];

        Assert.All(requiredStyles[..^1], key => Assert.Contains($"x:Key=\"{key}\"", iconography, StringComparison.Ordinal));
        Assert.Contains($"x:Key=\"{requiredStyles[^1]}\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Tcc.Strata.Brush.EditorialScrim\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Tcc.Strata.Brush.LowerPanelScrim\"", design, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource Strata.CraftFrame}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Fill=\"{DynamicResource Tcc.Strata.Brush.EditorialScrim}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Fill=\"{DynamicResource Tcc.Strata.Brush.LowerPanelScrim}\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("StrokeDashArray=", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("<Ellipse Margin=\"13\"", xaml, StringComparison.Ordinal);
        Assert.Contains("\"Tcc.Strata.Brush.EditorialScrim\"", codeBehind, StringComparison.Ordinal);
        Assert.Contains("\"Tcc.Strata.Brush.LowerPanelScrim\"", codeBehind, StringComparison.Ordinal);
        Assert.Contains("SetVisualBrush(\"Tcc.Strata.Brush.EditorialScrim\", Colors.Transparent)", codeBehind, StringComparison.Ordinal);
        Assert.Contains("SetVisualBrush(\"Tcc.Strata.Brush.LowerPanelScrim\", Colors.Transparent)", codeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void FifthRefinementUsesLuxuryHierarchyInkBleedAndContinuousBrushwork()
    {
        string design = File.ReadAllText(DesignPath);
        string xaml = File.ReadAllText(MainWindowPath);

        Assert.Contains("P5 H5 E5 S5 R5 V5 · macrostructure: observatory workbench continuation", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Tcc.Strata.Brush.SelectedSurface\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.FooterMotto\"", design, StringComparison.Ordinal);
        Assert.Contains("StrokeThickness=\"2.65\"", design, StringComparison.Ordinal);
        Assert.Contains("<TranslateTransform X=\"0.28\" Y=\"-0.18\" />", design, StringComparison.Ordinal);
        Assert.Contains("Text=\"OFFLINE\" Style=\"{StaticResource Strata.Caption}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"NO LIVE MARKET DATA\" Style=\"{StaticResource Strata.DisplayCaption}\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Text=\"NO LIVE MARKET DATA\" Style=\"{StaticResource Strata.StatusValue}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Width=\"360\" Height=\"92\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource Strata.FooterMotto}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"MentalStateGlyph\"", xaml, StringComparison.Ordinal);
        Assert.Contains("StrataObservatory.MentalStateSuite.R1.png", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("StrokeDashArray=", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void SixthRefinementRemovesFalseDirectionAndDormantAffordances()
    {
        string design = File.ReadAllText(DesignPath);
        string iconography = File.ReadAllText(IconographyPath);
        string xaml = File.ReadAllText(MainWindowPath);

        Assert.Contains("foreground refinement: moonlit luminance and lower-right plum depth", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.UnsetStatusMarker\"", iconography, StringComparison.Ordinal);
        Assert.Equal(16, Regex.Count(xaml, "Style=\\\"\\{StaticResource Strata\\.UnsetStatusMarker\\}\\\""));
        Assert.DoesNotContain("M0,0 L8,0 L4,6 Z", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("M0,6 L8,6 L4,0 Z", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"READ THE TERRAIN\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"MANAGE RISK\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"EXECUTE WITH DISCIPLINE\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("R E A D   T H E   T E R R A I N", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Tcc.ReferenceMaster.FontSize.PageSubtitle\">7.5", design, StringComparison.Ordinal);
        Assert.Contains("Padding=\"10,0\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"TradingPermissionHelper\" Grid.Row=\"2\" Text=\"Read only · Not tradable\" Style=\"{StaticResource Strata.Caption}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"ProductionPriorityRows\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"No priorities loaded\" Style=\"{StaticResource Strata.PriorityLabel}\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Background=\"{DynamicResource Tcc.Strata.Brush.PanelQuiet}\" BorderBrush=\"{DynamicResource Tcc.Strata.Brush.BorderQuiet}\" BorderThickness=\"1\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Width=\"2\" Background=\"{DynamicResource Tcc.Strata.Brush.Gold}\"", xaml, StringComparison.Ordinal);

        XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);
        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        XElement home = document.Descendants(presentation + "Button")
            .Single(button => button.Attribute(x + "Name")?.Value == "NavHome");
        Assert.NotEqual("False", home.Attribute("IsEnabled")?.Value);
        Assert.Equal("OnHomeNavigationClick", home.Attribute("Click")?.Value);
        Assert.Equal("Home selected, current page", home.Attribute("AutomationProperties.Name")?.Value);
    }

    [Fact]
    public void HomeUiPhase2TopChromePreservesIdentityAndGeometryWithReadableHierarchy()
    {
        string design = File.ReadAllText(DesignPath);
        string xaml = File.ReadAllText(MainWindowPath);
        string chrome = xaml[
            xaml.IndexOf("<!-- Top chrome -->", StringComparison.Ordinal)..
            xaml.IndexOf("<!-- Left rail -->", StringComparison.Ordinal)];

        Assert.Contains("x:Name=\"TopChromeRegion\" Canvas.Left=\"0\" Canvas.Top=\"0\" Width=\"1672\" Height=\"52\"", chrome, StringComparison.Ordinal);
        Assert.Contains("CaptionHeight=\"52\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ResizeBorderThickness=\"8\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"TopChromeBrandMark\" Width=\"36\" Height=\"36\"", chrome, StringComparison.Ordinal);
        Assert.Contains("Text=\"TRADING COMMAND\" Style=\"{StaticResource Strata.ChromeBrandName}\"", chrome, StringComparison.Ordinal);
        Assert.Contains("Text=\"STRATA OBSERVATORY\" Style=\"{StaticResource Strata.ChromeBrandSubtitle}\"", chrome, StringComparison.Ordinal);
        Assert.Contains("Text=\"DISCIPLINE TRADES A LONGER TOMORROW\"", chrome, StringComparison.Ordinal);
        Assert.DoesNotContain("D I S C I P L I N E", chrome, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.ChromeBrandName\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.ChromeBrandSubtitle\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.ChromeMotto\"", design, StringComparison.Ordinal);
        Assert.Equal(2, Regex.Count(chrome, "Height=\\\"24\\\" HorizontalAlignment=\\\"(Right|Left)\\\" VerticalAlignment=\\\"Center\\\""));
    }

    [Fact]
    public void HomeUiPhase2TopChromeKeepsOfflineSearchStaticAndHonest()
    {
        string design = File.ReadAllText(DesignPath);
        string xaml = File.ReadAllText(MainWindowPath);
        string chrome = xaml[
            xaml.IndexOf("<!-- Top chrome -->", StringComparison.Ordinal)..
            xaml.IndexOf("<!-- Left rail -->", StringComparison.Ordinal)];

        Assert.Contains("x:Name=\"TopChromeOfflineStatus\"", chrome, StringComparison.Ordinal);
        Assert.Contains("Text=\"DATA OFFLINE\" Style=\"{StaticResource Strata.ChromeStatusLabel}\"", chrome, StringComparison.Ordinal);
        Assert.Contains("Text=\"READ ONLY\" Style=\"{StaticResource Strata.ChromeStatusDetail}\"", chrome, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"SearchUnavailableSurface\"", chrome, StringComparison.Ordinal);
        Assert.Contains("IsHitTestVisible=\"False\" Focusable=\"False\"", chrome, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Search unavailable while data is offline\"", chrome, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"Read-only presentation; search and command palette are unavailable.\"", chrome, StringComparison.Ordinal);
        Assert.Contains("Text=\"SEARCH UNAVAILABLE\" Style=\"{StaticResource Strata.ChromeSearchLabel}\"", chrome, StringComparison.Ordinal);
        Assert.Contains("Text=\"OFFLINE\" Style=\"{StaticResource Strata.ChromeSearchState}\"", chrome, StringComparison.Ordinal);
        Assert.DoesNotContain("Command=", chrome[..chrome.IndexOf("x:Name=\"WindowMinimizeButton\"", StringComparison.Ordinal)], StringComparison.Ordinal);
        Assert.DoesNotContain("Click=", chrome[..chrome.IndexOf("x:Name=\"WindowMinimizeButton\"", StringComparison.Ordinal)], StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.ChromeStatusLabel\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.ChromeSearchLabel\"", design, StringComparison.Ordinal);
    }

    [Fact]
    public void HomeUiPhase2TopChromeRetainsThreeAccessibleRealWindowControls()
    {
        string xaml = File.ReadAllText(MainWindowPath);
        string codeBehind = File.ReadAllText(CodeBehindPath);
        string chrome = xaml[
            xaml.IndexOf("<!-- Top chrome -->", StringComparison.Ordinal)..
            xaml.IndexOf("<!-- Left rail -->", StringComparison.Ordinal)];

        (string Name, string AutomationId, string AccessibleName, string Handler, string Tooltip)[] controls =
        [
            ("WindowMinimizeButton", "WindowMinimizeButton", "Minimize window", "OnMinimizeWindowClick", "Minimize"),
            ("WindowMaximizeRestoreWindowButton", "WindowMaximizeRestoreButton", "Maximize or restore window", "OnMaximizeRestoreWindowClick", "Maximize or restore"),
            ("WindowCloseButton", "WindowCloseButton", "Close window", "OnCloseWindowClick", "Close")
        ];

        foreach ((string name, string automationId, string accessibleName, string handler, string tooltip) in controls)
        {
            Assert.Contains($"x:Name=\"{name}\"", chrome, StringComparison.Ordinal);
            Assert.Contains($"AutomationProperties.AutomationId=\"{automationId}\"", chrome, StringComparison.Ordinal);
            Assert.Contains($"AutomationProperties.Name=\"{accessibleName}\"", chrome, StringComparison.Ordinal);
            Assert.Contains($"ToolTip=\"{tooltip}\"", chrome, StringComparison.Ordinal);
            Assert.Contains($"Click=\"{handler}\"", chrome, StringComparison.Ordinal);
            Assert.Contains($"void {handler}", codeBehind, StringComparison.Ordinal);
        }

        Assert.Equal(3, Regex.Count(chrome, "Style=\\\"\\{StaticResource Strata\\.WindowButton\\}\\\""));
    }

    [Fact]
    public void HomeUiPhase2HeaderIdentityAlignsWithTheLockedSafetyGeometry()
    {
        string xaml = File.ReadAllText(MainWindowPath);
        int headerStart = xaml.IndexOf("<!-- Header field -->", StringComparison.Ordinal);
        string header = xaml[
            headerStart..
            xaml.IndexOf("<!-- Market overview -->", headerStart, StringComparison.Ordinal)];

        Assert.Contains("Canvas.Left=\"104\" Canvas.Top=\"52\" Width=\"1154\" Height=\"186\"", header, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"HeaderIdentityRegion\" Width=\"452\" Height=\"101\" Margin=\"44,70,0,0\"", header, StringComparison.Ordinal);
        Assert.Contains("Margin=\"0,70,6,0\" Width=\"640\" Height=\"101\"", header, StringComparison.Ordinal);
        Assert.True(
            header.IndexOf("x:Name=\"HeaderIdentityRegion\"", StringComparison.Ordinal) <
            header.IndexOf("Style=\"{StaticResource Strata.Panel.Safety}\"", StringComparison.Ordinal),
            "Header identity must remain before the locked Safety Core in reading order.");
        Assert.Contains("Grid.RowSpan=\"3\" Width=\"1\" Height=\"65\"", header, StringComparison.Ordinal);
    }

    [Fact]
    public void HomeUiPhase2HeaderIdentityUsesAnOrderedEditorialHierarchy()
    {
        string design = File.ReadAllText(DesignPath);
        string xaml = File.ReadAllText(MainWindowPath);
        string identity = xaml[
            xaml.IndexOf("x:Name=\"HeaderIdentityRegion\"", StringComparison.Ordinal)..
            xaml.IndexOf("<Border HorizontalAlignment=\"Right\" VerticalAlignment=\"Top\" Margin=\"0,70,6,0\"", StringComparison.Ordinal)];

        Assert.Contains("x:Key=\"Strata.HeaderEyebrow\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.HeaderTitle\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.HeaderPrinciple\"", design, StringComparison.Ordinal);

        string[] orderedCopy = ["HOME / OBSERVATORY", "Market Observatory", "READ THE TERRAIN", "MANAGE RISK", "EXECUTE WITH DISCIPLINE"];
        int prior = -1;
        foreach (string copy in orderedCopy)
        {
            int current = identity.IndexOf($"Text=\"{copy}\"", StringComparison.Ordinal);
            Assert.True(current > prior, $"Header identity hierarchy drifted at '{copy}'.");
            prior = current;
        }

        Assert.Contains("x:Name=\"HeaderIdentitySeal\"", identity, StringComparison.Ordinal);
        Assert.Equal(1, Regex.Count(identity, "Style=\\\"\\{StaticResource Strata\\.Seal\\.Vertical\\}\\\""));
        Assert.Equal(3, Regex.Count(identity, "Style=\\\"\\{StaticResource Strata\\.HeaderPrinciple\\}\\\""));
        Assert.Equal(2, Regex.Count(identity, "Width=\\\"1\\\" Height=\\\"10\\\" Margin=\\\"11,0\\\""));
    }

    [Fact]
    public void HomeUiPhase2HeaderIdentityIsAccessibleAndNonInteractive()
    {
        string xaml = File.ReadAllText(MainWindowPath);
        string identity = xaml[
            xaml.IndexOf("x:Name=\"HeaderIdentityRegion\"", StringComparison.Ordinal)..
            xaml.IndexOf("<Border HorizontalAlignment=\"Right\" VerticalAlignment=\"Top\" Margin=\"0,70,6,0\"", StringComparison.Ordinal)];

        Assert.Contains("IsHitTestVisible=\"False\" Focusable=\"False\"", identity, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"HeaderIdentityLabel\" Text=\"Market Observatory\"", identity, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Region.HeaderIdentity\"", identity, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Market Observatory\"", identity, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"Home workspace. Read the terrain, manage risk, and execute with discipline.\"", identity, StringComparison.Ordinal);
        Assert.DoesNotContain("<Button", identity, StringComparison.Ordinal);
        Assert.DoesNotContain("<TextBox", identity, StringComparison.Ordinal);
        Assert.DoesNotContain("Command=", identity, StringComparison.Ordinal);
        Assert.DoesNotContain("Click=", identity, StringComparison.Ordinal);
        Assert.DoesNotContain("account", identity, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("date", identity, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HomeUiPhase2RightEditorialMarksPreserveTheScenicStripBoundary()
    {
        string xaml = File.ReadAllText(MainWindowPath);
        int editorialStart = xaml.IndexOf("<!-- Right-side editorial marks -->", StringComparison.Ordinal);
        string editorial = xaml[
            editorialStart..
            xaml.IndexOf("<TextBlock Visibility=\"Collapsed\"", editorialStart, StringComparison.Ordinal)];

        Assert.Contains("x:Name=\"RightEditorialMarksRegion\" Canvas.Left=\"1538\" Canvas.Top=\"102\" Width=\"130\" Height=\"190\"", editorial, StringComparison.Ordinal);
        Assert.Contains("<Rectangle Fill=\"{DynamicResource Tcc.Strata.Brush.EditorialScrim}\" />", editorial, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"RightEditorialCadenceLine\" Width=\"1\" Height=\"140\" Margin=\"19,24,0,0\"", editorial, StringComparison.Ordinal);
        Assert.Contains("Width=\"96\" Height=\"154\" Margin=\"20,20,14,16\"", editorial, StringComparison.Ordinal);
        Assert.DoesNotContain("Canvas.Left=\"1565\"", editorial, StringComparison.Ordinal);
    }

    [Fact]
    public void HomeUiPhase2RightEditorialMarksUseTwoLevelCadenceAndAcceptedSeal()
    {
        string design = File.ReadAllText(DesignPath);
        string xaml = File.ReadAllText(MainWindowPath);
        int editorialStart = xaml.IndexOf("<!-- Right-side editorial marks -->", StringComparison.Ordinal);
        string editorial = xaml[
            editorialStart..
            xaml.IndexOf("<TextBlock Visibility=\"Collapsed\"", editorialStart, StringComparison.Ordinal)];

        string[] orderedCopy = ["M A R K E T S", "C H A N G E", "D I S C I P L I N E", "E N D U R E S"];
        int prior = -1;
        foreach (string copy in orderedCopy)
        {
            int current = editorial.IndexOf($"Text=\"{copy}\"", StringComparison.Ordinal);
            Assert.True(current > prior, $"Right editorial cadence drifted at '{copy}'.");
            prior = current;
        }

        Assert.Contains("x:Key=\"Strata.EditorialEmphasis\"", design, StringComparison.Ordinal);
        Assert.Equal(2, Regex.Count(editorial, "Style=\\\"\\{StaticResource Strata\\.EditorialCaption\\}\\\""));
        Assert.Equal(2, Regex.Count(editorial, "Style=\\\"\\{StaticResource Strata\\.EditorialEmphasis\\}\\\""));
        Assert.Contains("x:Name=\"RightEditorialPairRule\"", editorial, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"RightEditorialSeal\" Grid.Row=\"6\" Width=\"24\" Height=\"24\"", editorial, StringComparison.Ordinal);
        Assert.Equal(1, Regex.Count(editorial, "Style=\\\"\\{StaticResource Strata\\.Seal\\.Square\\}\\\""));
    }

    [Fact]
    public void HomeUiPhase2RightEditorialMarksAreAccessibleAndNonInteractive()
    {
        string xaml = File.ReadAllText(MainWindowPath);
        int editorialStart = xaml.IndexOf("<!-- Right-side editorial marks -->", StringComparison.Ordinal);
        string editorial = xaml[
            editorialStart..
            xaml.IndexOf("<TextBlock Visibility=\"Collapsed\"", editorialStart, StringComparison.Ordinal)];

        Assert.Contains("IsHitTestVisible=\"False\" Focusable=\"False\"", editorial, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"RightEditorialRegionLabel\"", editorial, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.AutomationId=\"Editorial.RightMaxim\"", editorial, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Markets change. Discipline endures.\"", editorial, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"Decorative editorial maxim in the right scenic margin.\"", editorial, StringComparison.Ordinal);
        Assert.Equal(4, Regex.Count(editorial, "AutomationProperties\\.Name=\\\"\\\""));
        Assert.DoesNotContain("<Button", editorial, StringComparison.Ordinal);
        Assert.DoesNotContain("<TextBox", editorial, StringComparison.Ordinal);
        Assert.DoesNotContain("Command=", editorial, StringComparison.Ordinal);
        Assert.DoesNotContain("Click=", editorial, StringComparison.Ordinal);
    }

    [Fact]
    public void HomeUiPhase2LeftNavigationSeparatesCurrentUnavailableAndUtilityStates()
    {
        string design = File.ReadAllText(DesignPath);
        string xaml = File.ReadAllText(MainWindowPath);
        XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);
        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";

        Assert.Contains("x:Key=\"Strata.NavLabel\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.NavLabel.Selected\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.NavState.Current\"", design, StringComparison.Ordinal);
        Assert.Contains("Property=\"ToolTipService.ShowOnDisabled\" Value=\"True\"", design, StringComparison.Ordinal);
        Assert.Contains("Canvas.Left=\"6\" Canvas.Top=\"52\" Width=\"98\" Height=\"889\"", xaml, StringComparison.Ordinal);

        XElement home = document.Descendants(presentation + "Button")
            .Single(button => button.Attribute(x + "Name")?.Value == "NavHome");
        Assert.NotEqual("False", home.Attribute("IsEnabled")?.Value);
        Assert.NotEqual("False", home.Attribute("IsTabStop")?.Value);
        Assert.Equal("Current page", home.Attribute("AutomationProperties.ItemStatus")?.Value);
        Assert.Equal("Current page", home.Attribute("ToolTip")?.Value);
        Assert.Contains(home.Descendants(presentation + "TextBlock"), label =>
            label.Attribute("Text")?.Value == "CURRENT" &&
            label.Attribute("Style")?.Value == "{StaticResource Strata.NavState.Current}");
        Assert.Contains(home.Descendants(presentation + "TextBlock"), label =>
            label.Attribute("Text")?.Value == "HOME" &&
            label.Attribute("Style")?.Value == "{StaticResource Strata.NavLabel.Selected}" &&
            label.Attribute("Foreground")?.Value == "{Binding Foreground, ElementName=NavHome}");
        Assert.DoesNotContain("Command=", home.ToString(SaveOptions.DisableFormatting), StringComparison.Ordinal);
        Assert.Equal("OnHomeNavigationClick", home.Attribute("Click")?.Value);

        XElement plan = document.Descendants(presentation + "Button")
            .Single(button => button.Attribute(x + "Name")?.Value == "NavPlanning");
        Assert.NotEqual("False", plan.Attribute("IsEnabled")?.Value);
        Assert.NotEqual("False", plan.Attribute("IsTabStop")?.Value);
        Assert.Equal("Available", plan.Attribute("AutomationProperties.ItemStatus")?.Value);
        Assert.Equal("Open Plan", plan.Attribute("ToolTip")?.Value);
        Assert.Equal("OnPlanNavigationClick", plan.Attribute("Click")?.Value);
        Assert.DoesNotContain("Command=", plan.ToString(SaveOptions.DisableFormatting), StringComparison.Ordinal);
        Assert.Contains(plan.Descendants(presentation + "TextBlock"), label =>
            label.Attribute("Text")?.Value == "PLAN" &&
            label.Attribute("Style")?.Value == "{StaticResource Strata.NavLabel}" &&
            label.Attribute("Foreground")?.Value == "{Binding Foreground, ElementName=NavPlanning}");

        string[] unavailableNavIds = ["NavMarkets", "NavRisk", "NavPositions", "NavReview", "NavSettings"];
        foreach (string navId in unavailableNavIds)
        {
            XElement nav = document.Descendants(presentation + "Button")
                .Single(button => button.Attribute(x + "Name")?.Value == navId);
            Assert.Equal("False", nav.Attribute("IsEnabled")?.Value);
            Assert.Equal("Unavailable", nav.Attribute("AutomationProperties.ItemStatus")?.Value);
            Assert.False(string.IsNullOrWhiteSpace(nav.Attribute("AutomationProperties.Name")?.Value));
            Assert.False(string.IsNullOrWhiteSpace(nav.Attribute("AutomationProperties.HelpText")?.Value));
            Assert.False(string.IsNullOrWhiteSpace(nav.Attribute("ToolTip")?.Value));
            Assert.DoesNotContain("Command=", nav.ToString(SaveOptions.DisableFormatting), StringComparison.Ordinal);
            Assert.DoesNotContain("Click=", nav.ToString(SaveOptions.DisableFormatting), StringComparison.Ordinal);
            Assert.Single(nav.Descendants(presentation + "TextBlock"), label =>
                label.Attribute("Style")?.Value == "{StaticResource Strata.NavLabel}");
        }

        XElement settings = document.Descendants(presentation + "Button")
            .Single(button => button.Attribute(x + "Name")?.Value == "NavSettings");
        Assert.Equal("7", settings.Attribute("Grid.Row")?.Value);
        Assert.Contains(document.Descendants(presentation + "Border"), border =>
            border.Attribute("Grid.Row")?.Value == "6" &&
            border.Attribute("BorderThickness")?.Value == "0,0,0,1");
        Assert.Contains("<StackPanel Grid.Row=\"8\" Margin=\"17,0\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\">", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void HomeUiPhase2WatchlistKeepsOfflineRowsNeutralAndFixtureIsolated()
    {
        string design = File.ReadAllText(DesignPath);
        string xaml = File.ReadAllText(MainWindowPath);
        string watchlist = xaml[
            xaml.IndexOf("<!-- Watchlist -->", StringComparison.Ordinal)..
            xaml.IndexOf("<!-- Bottom row -->", StringComparison.Ordinal)];
        string header = watchlist[..watchlist.IndexOf("x:Name=\"ProductionWatchlistRows\"", StringComparison.Ordinal)];
        string production = watchlist[
            watchlist.IndexOf("x:Name=\"ProductionWatchlistRows\"", StringComparison.Ordinal)..
            watchlist.IndexOf("x:Name=\"FixtureWatchlistRows\"", StringComparison.Ordinal)];
        string fixture = watchlist[
            watchlist.IndexOf("x:Name=\"FixtureWatchlistRows\"", StringComparison.Ordinal)..
            watchlist.IndexOf("x:Name=\"ProductionWatchlistFooter\"", StringComparison.Ordinal)];

        Assert.Contains("x:Key=\"Strata.WatchlistTitle\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.WatchlistSymbol\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.WatchlistPlaceholder\"", design, StringComparison.Ordinal);
        Assert.Contains("Canvas.Left=\"1001\" Canvas.Top=\"239\" Width=\"251\" Height=\"404\"", watchlist, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Watchlist, five symbols, prices and 24 hour changes unavailable offline\"", header, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"Read-only symbol list; no market values are inferred.\"", header, StringComparison.Ordinal);
        Assert.Contains("Text=\"READ ONLY\"", header, StringComparison.Ordinal);
        Assert.DoesNotContain("Top Movers", header, StringComparison.Ordinal);
        Assert.DoesNotContain("Strata.KebabGlyph", header, StringComparison.Ordinal);

        string[] symbols = ["BTCUSDT", "ETHUSDT", "SOLUSDT", "BNBUSDT", "XRPUSDT"];
        int previousIndex = -1;
        foreach (string symbol in symbols)
        {
            int index = production.IndexOf($"Text=\"{symbol}\"", StringComparison.Ordinal);
            Assert.True(index > previousIndex, $"Production symbol order drifted at {symbol}.");
            previousIndex = index;
        }

        Assert.Equal(5, Regex.Count(production, "Style=\"\\{StaticResource Strata\\.WatchlistSymbol\\}\""));
        Assert.Equal(10, Regex.Count(production, "Text=\"—\" Style=\"\\{StaticResource Strata\\.WatchlistPlaceholder\\}\""));
        Assert.Equal(5, Regex.Count(production, "Style=\"\\{StaticResource Strata\\.UnsetStatusMarker\\}\""));
        Assert.DoesNotContain("Tcc.Strata.Brush.SelectedSurface", production, StringComparison.Ordinal);
        Assert.DoesNotContain("Tcc.Strata.Brush.Danger", production, StringComparison.Ordinal);
        Assert.DoesNotContain("Tcc.Strata.Brush.Success", production, StringComparison.Ordinal);
        Assert.DoesNotContain("Text=\"OFF\"", production, StringComparison.Ordinal);
        Assert.DoesNotContain("Text=\"▲\"", production, StringComparison.Ordinal);
        Assert.DoesNotContain("Text=\"▼\"", production, StringComparison.Ordinal);
        Assert.DoesNotContain("43,287.62", production, StringComparison.Ordinal);

        Assert.Contains("Visibility=\"Collapsed\"", fixture, StringComparison.Ordinal);
        Assert.Contains("43,287.62", fixture, StringComparison.Ordinal);
        Assert.Contains("+1.26%", fixture, StringComparison.Ordinal);
        Assert.Contains("Text=\"DATA UNAVAILABLE · READ ONLY\"", watchlist, StringComparison.Ordinal);
    }

    [Fact]
    public void HomeUiPhase2PrioritiesKeepsNoneLoadedSlotsNeutralAndNonInteractive()
    {
        string design = File.ReadAllText(DesignPath);
        string xaml = File.ReadAllText(MainWindowPath);
        string codeBehind = File.ReadAllText(CodeBehindPath);
        string priorities = xaml[
            xaml.IndexOf("<!-- Bottom row -->", StringComparison.Ordinal)..
            xaml.IndexOf("Canvas.Left=\"492\" Canvas.Top=\"654\"", StringComparison.Ordinal)];
        string production = priorities[
            priorities.IndexOf("x:Name=\"ProductionPriorityRows\"", StringComparison.Ordinal)..];

        Assert.Contains("x:Key=\"Strata.PriorityIndex\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.PriorityLabel\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.PriorityPlaceholder\"", design, StringComparison.Ordinal);
        Assert.Contains("Canvas.Left=\"126\" Canvas.Top=\"654\" Width=\"363\" Height=\"260\"", priorities, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Priorities, no items loaded, not set\"", priorities, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"Read-only priority slots; editing and completion are unavailable.\"", priorities, StringComparison.Ordinal);
        Assert.Contains("Text=\"NOT SET\"", priorities, StringComparison.Ordinal);

        string[] indices = ["01", "02", "03", "04"];
        int previousIndex = -1;
        foreach (string slot in indices)
        {
            int index = production.IndexOf($"Text=\"{slot}\"", StringComparison.Ordinal);
            Assert.True(index > previousIndex, $"Production priority order drifted at {slot}.");
            previousIndex = index;
        }

        Assert.Equal(4, Regex.Count(production, "Style=\"\\{StaticResource Strata\\.PriorityIndex\\}\""));
        Assert.Equal(4, Regex.Count(production, "Style=\"\\{StaticResource Strata\\.UnsetStatusMarker\\}\""));
        Assert.Equal(3, Regex.Count(production, "Text=\"—\" Style=\"\\{StaticResource Strata\\.PriorityPlaceholder\\}\""));
        Assert.Equal(1, Regex.Count(production, "Text=\"No priorities loaded\""));
        Assert.DoesNotContain("Tcc.Strata.Brush.SelectedSurface", production, StringComparison.Ordinal);
        Assert.DoesNotContain("Tcc.Strata.Brush.Gold", production, StringComparison.Ordinal);
        Assert.DoesNotContain("<CheckBox", production, StringComparison.Ordinal);
        Assert.DoesNotContain("Command=", priorities, StringComparison.Ordinal);
        Assert.DoesNotContain("Click=", priorities, StringComparison.Ordinal);
        Assert.Contains("Text=\"SMALL STEPS · COMPOUNDED\"", priorities, StringComparison.Ordinal);

        Assert.Contains("PriorityFirstRow.BorderBrush", codeBehind, StringComparison.Ordinal);
        Assert.Contains("PriorityText1.Text = \"Define market bias\"", codeBehind, StringComparison.Ordinal);
        Assert.Contains("PriorityText4.Text = \"Wait for quality\"", codeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void HomeUiPhase2MentalStateMakesLiveStatePrimaryAndUnknownFieldsAligned()
    {
        string design = File.ReadAllText(DesignPath);
        string xaml = File.ReadAllText(MainWindowPath);
        string codeBehind = File.ReadAllText(CodeBehindPath);
        string mental = xaml[
            xaml.IndexOf("Canvas.Left=\"492\" Canvas.Top=\"654\"", StringComparison.Ordinal)..
            xaml.IndexOf("Canvas.Left=\"879\" Canvas.Top=\"654\"", StringComparison.Ordinal)];
        string production = mental[
            mental.IndexOf("x:Name=\"ProductionMentalRows\"", StringComparison.Ordinal)..
            mental.IndexOf("x:Name=\"FixtureMentalRows\"", StringComparison.Ordinal)];
        string fixture = mental[mental.IndexOf("x:Name=\"FixtureMentalRows\"", StringComparison.Ordinal)..];

        Assert.Contains("x:Key=\"Strata.MentalStateValue\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.MentalStateFieldLabel\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.MentalStateFieldValue\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.MentalStateHelper\"", design, StringComparison.Ordinal);
        Assert.Contains("Canvas.Left=\"492\" Canvas.Top=\"654\" Width=\"382\" Height=\"260\"", mental, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"MentalStateRegionLabel\"", mental, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Mental state, not set, no self-check recorded\"", mental, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"Read-only self-check summary; no mental-state input is available.\"", mental, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"MentalStateValue\" Text=\"NOT SET\" Style=\"{StaticResource Strata.MentalStateValue}\"", mental, StringComparison.Ordinal);
        Assert.True(
            mental.IndexOf("x:Name=\"MentalStateValue\"", StringComparison.Ordinal) < mental.IndexOf("Grid.Row=\"1\"", StringComparison.Ordinal),
            "The authoritative live state must remain in the Mental State header.");
        Assert.Contains("Width=\"145\" Height=\"145\"", mental, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"MentalStateHelper\"", mental, StringComparison.Ordinal);
        Assert.Contains("Text=\"NO SELF-CHECK\"", mental, StringComparison.Ordinal);

        string[] fields = ["Emotion", "Plan", "Patience", "Readiness"];
        int previousIndex = -1;
        foreach (string field in fields)
        {
            int index = production.IndexOf($"Text=\"{field}\"", StringComparison.Ordinal);
            Assert.True(index > previousIndex, $"Production Mental State field order drifted at {field}.");
            previousIndex = index;
        }

        Assert.Equal(4, Regex.Count(production, "Style=\"\\{StaticResource Strata\\.MentalStateFieldLabel\\}\""));
        Assert.Equal(4, Regex.Count(production, "Text=\"—\" Style=\"\\{StaticResource Strata\\.MentalStateFieldValue\\}\""));
        Assert.Equal(4, Regex.Count(production, "Style=\"\\{StaticResource Strata\\.UnsetStatusMarker\\}\""));
        Assert.DoesNotContain("Tcc.Strata.Brush.Success", production, StringComparison.Ordinal);
        Assert.DoesNotContain("Tcc.Strata.Brush.Danger", production, StringComparison.Ordinal);
        Assert.DoesNotContain("<Button", mental, StringComparison.Ordinal);
        Assert.DoesNotContain("<TextBox", mental, StringComparison.Ordinal);
        Assert.DoesNotContain("Command=", mental, StringComparison.Ordinal);
        Assert.DoesNotContain("Click=", mental, StringComparison.Ordinal);
        Assert.Contains("Text=\"A CLEAR MIND · SEES FURTHER\"", mental, StringComparison.Ordinal);

        Assert.Contains("Visibility=\"Collapsed\"", fixture, StringComparison.Ordinal);
        Assert.Equal(5, Regex.Count(fixture, "Fill=\"\\{DynamicResource Tcc\\.Strata\\.Brush\\.Ice\\}\""));
        Assert.Contains("MentalStateValue.Text = \"CALM\";", codeBehind, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.SetName(MentalStateRegionLabel, \"Mental state, calm, focused and disciplined\");", codeBehind, StringComparison.Ordinal);
        Assert.Contains("MentalStateHelper.Text = \"FOCUSED\\nDISCIPLINED\";", codeBehind, StringComparison.Ordinal);
        Assert.Contains("Tcc.Strata.MentalStateGlyph.Calm", codeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void HomeUiPhase2RecentActivityIsAnHonestReadOnlyLocalTimeline()
    {
        string design = File.ReadAllText(DesignPath);
        string xaml = File.ReadAllText(MainWindowPath);
        string codeBehind = File.ReadAllText(CodeBehindPath);
        string activity = xaml[
            xaml.IndexOf("Canvas.Left=\"879\" Canvas.Top=\"654\"", StringComparison.Ordinal)..
            xaml.IndexOf("<!-- Right-side editorial marks -->", StringComparison.Ordinal)];
        string production = activity[..activity.IndexOf("x:Name=\"RecentRow5\"", StringComparison.Ordinal)];
        string fixtureRow = activity[activity.IndexOf("x:Name=\"RecentRow5\"", StringComparison.Ordinal)..];

        Assert.Contains("x:Key=\"Strata.ActivityColumnLabel\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.ActivityTime\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.ActivityEvent\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.ActivityMarker.Current\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.ActivityMarker.Alert\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.ActivityMarker.Neutral\"", design, StringComparison.Ordinal);
        Assert.Contains("Canvas.Left=\"879\" Canvas.Top=\"654\" Width=\"373\" Height=\"260\"", activity, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"RecentActivityRegionLabel\"", activity, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Recent activity, four local session facts, timestamps unavailable\"", activity, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"Read-only local session facts; timestamps are unavailable and no historical activity is inferred.\"", activity, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"RecentActivityMode\" Text=\"LOCAL EVENTS\"", activity, StringComparison.Ordinal);
        Assert.Contains("Text=\"TIME\" Style=\"{StaticResource Strata.ActivityColumnLabel}\"", activity, StringComparison.Ordinal);
        Assert.Contains("Text=\"EVENT\" Style=\"{StaticResource Strata.ActivityColumnLabel}\"", activity, StringComparison.Ordinal);

        string[] events = ["Workspace opened", "Market data offline", "Read-only mode active", "No execution API"];
        int previousIndex = -1;
        foreach (string eventText in events)
        {
            int index = production.IndexOf($"Text=\"{eventText}\"", StringComparison.Ordinal);
            Assert.True(index > previousIndex, $"Production activity order drifted at {eventText}.");
            previousIndex = index;
        }

        Assert.Equal(4, Regex.Count(production, "Text=\"—\" Style=\"\\{StaticResource Strata\\.ActivityTime\\}\""));
        Assert.Equal(4, Regex.Count(production, "Style=\"\\{StaticResource Strata\\.ActivityEvent\\}\""));
        Assert.Equal(1, Regex.Count(production, "Strata.ActivityMarker.Current"));
        Assert.Equal(1, Regex.Count(production, "Strata.ActivityMarker.Alert"));
        Assert.Equal(2, Regex.Count(production, "Strata.ActivityMarker.Neutral"));
        Assert.DoesNotContain("Tcc.Strata.Brush.Success", activity, StringComparison.Ordinal);
        Assert.DoesNotContain("<Button", activity, StringComparison.Ordinal);
        Assert.DoesNotContain("<TextBox", activity, StringComparison.Ordinal);
        Assert.DoesNotContain("Command=", activity, StringComparison.Ordinal);
        Assert.DoesNotContain("Click=", activity, StringComparison.Ordinal);
        Assert.DoesNotContain("⌄", activity, StringComparison.Ordinal);
        Assert.Contains("Text=\"JOURNAL TODAY · A BETTER TOMORROW\"", activity, StringComparison.Ordinal);

        Assert.Contains("Visibility=\"Collapsed\"", fixtureRow, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"RecentTimelineFixtureExtension\"", activity, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.SetName(RecentActivityRegionLabel, \"Recent activity fixture, five reference events with timestamps\");", codeBehind, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.SetHelpText(RecentActivityRegionLabel, \"Reference fixture activity; filtering remains unavailable.\");", codeBehind, StringComparison.Ordinal);
        Assert.Contains("RecentActivityMode.Text = \"ALL ACTIVITY\";", codeBehind, StringComparison.Ordinal);
        Assert.Contains("RecentTimelineFixtureExtension.Visibility = Visibility.Visible;", codeBehind, StringComparison.Ordinal);
        Assert.Contains("ApplyRecentFixtureRow(RecentTime5, RecentText5, \"08:03\", \"Checked risk parameters\");", codeBehind, StringComparison.Ordinal);
        Assert.DoesNotContain("All Activities⌄", codeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void ForegroundPlumAndMoonlitLuminanceAreExplicitAndAccessible()
    {
        CurrentHomeAuthorityContract.AssertReconciled(nameof(HomeStrataObservatoryTests), nameof(ForegroundPlumAndMoonlitLuminanceAreExplicitAndAccessible));
    }

    [Fact]
    public void MasterCardMaterialUsesSubtleRadiusWithoutDecorativeIceOverlays()
    {
        string design = File.ReadAllText(DesignPath);
        string xaml = File.ReadAllText(MainWindowPath);
        string codeBehind = File.ReadAllText(CodeBehindPath);
        string project = File.ReadAllText(ProjectPath);

        Assert.Contains("<CornerRadius x:Key=\"Tcc.Strata.CardCornerRadius\">2</CornerRadius>", design, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"CornerRadius\" Value=\"{StaticResource Tcc.Strata.CardCornerRadius}\" />", design, StringComparison.Ordinal);
        Assert.DoesNotContain("EdgeMaterial", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("EdgeMaterial", project, StringComparison.Ordinal);
        Assert.DoesNotContain("EdgeMaterialOpacity", design, StringComparison.Ordinal);
        Assert.DoesNotContain("EdgeMaterialOpacity", codeBehind, StringComparison.Ordinal);
    }

    [Fact]
    public void CardMaterialCorrectionPreservesEveryApprovedCardRectangle()
    {
        string xaml = File.ReadAllText(MainWindowPath);

        Assert.Contains("Canvas.Left=\"126\" Canvas.Top=\"239\" Width=\"870\" Height=\"404\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Canvas.Left=\"1001\" Canvas.Top=\"239\" Width=\"251\" Height=\"404\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Canvas.Left=\"126\" Canvas.Top=\"654\" Width=\"363\" Height=\"260\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Canvas.Left=\"492\" Canvas.Top=\"654\" Width=\"382\" Height=\"260\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Canvas.Left=\"879\" Canvas.Top=\"654\" Width=\"373\" Height=\"260\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("EdgeMaterial", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void SemanticRegionsAndCardLayerContractAreExplicitAndCorrectlyBound()
    {
        XDocument document = XDocument.Load(MainWindowPath, LoadOptions.PreserveWhitespace);
        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
        string[] expectedRegionIds =
        [
            "Region.HeaderIdentity",
            "Region.HomeSafetyCore",
            "Region.MarketOverview",
            "Region.Watchlist",
            "Region.Priorities",
            "Region.MentalState",
            "Region.RecentActivity"
        ];

        XElement[] regions = document.Descendants(presentation + "TextBlock")
            .Where(element => element.Attribute("AutomationProperties.AutomationId")?.Value.StartsWith("Region.", StringComparison.Ordinal) == true)
            .ToArray();
        Assert.Equal(expectedRegionIds.Order(StringComparer.Ordinal), regions.Select(region => region.Attribute("AutomationProperties.AutomationId")!.Value).Order(StringComparer.Ordinal));
        Assert.All(regions, region => Assert.False(string.IsNullOrWhiteSpace(region.Attribute("AutomationProperties.Name")?.Value)));
        XElement safetyRegion = regions.Single(region => region.Attribute("AutomationProperties.AutomationId")?.Value == "Region.HomeSafetyCore");
        XElement marketRegion = regions.Single(region => region.Attribute("AutomationProperties.AutomationId")?.Value == "Region.MarketOverview");
        XElement headerRegion = regions.Single(region => region.Attribute("AutomationProperties.AutomationId")?.Value == "Region.HeaderIdentity");
        Assert.Equal("Market Observatory", headerRegion.Attribute("Text")?.Value);
        Assert.Equal("Trading Permission", safetyRegion.Attribute("Text")?.Value);
        Assert.Equal("Market Overview", marketRegion.Attribute("Text")?.Value);

        Assert.DoesNotContain(document.Descendants(presentation + "Image"), image => image.Attribute(x + "Name")?.Value?.Contains("EdgeMaterial", StringComparison.Ordinal) == true);
    }

    [Fact]
    public void RuntimeCaptureInventoryMatchesAllCurrentHomeAutomationIds()
    {
        XDocument document = XDocument.Load(MainWindowPath, LoadOptions.PreserveWhitespace);
        string[] sourceIds = document.Root!.DescendantsAndSelf()
            .SelectMany(element => element.Attributes())
            .Where(attribute => attribute.Name.LocalName == "AutomationProperties.AutomationId")
            .Select(attribute => attribute.Value)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(29, sourceIds.Length);
        Assert.Equal(29, sourceIds.Distinct(StringComparer.Ordinal).Count());

        string capture = File.ReadAllText(RuntimeCapturePath);
        Match expectedBlock = Regex.Match(
            capture,
            @"\$expectedIds\s*=\s*@\((?<ids>[\s\S]*?)\)",
            RegexOptions.CultureInvariant);
        Assert.True(expectedBlock.Success, "Runtime capture must declare one explicit expected UIA inventory.");
        string[] captureIds = Regex.Matches(expectedBlock.Groups["ids"].Value, "\"([^\"]+)\"")
            .Select(match => match.Groups[1].Value)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(sourceIds, captureIds);
        Assert.Contains("keyboard_focusable", capture, StringComparison.Ordinal);
        Assert.Contains("control_type", capture, StringComparison.Ordinal);
        Assert.Contains("focusable_unnamed_button_count", capture, StringComparison.Ordinal);
        Assert.Contains("unnamed_application_button_count", capture, StringComparison.Ordinal);
        Assert.Contains("framework_scroll_part_button_count", capture, StringComparison.Ordinal);
        Assert.Contains("scroll_pattern", capture, StringComparison.Ordinal);
        Assert.Contains("[System.Windows.SystemParameters]::HighContrast", capture, StringComparison.Ordinal);
        Assert.Contains("Region.HeaderIdentity", captureIds);
        Assert.Contains("Editorial.RightMaxim", captureIds);
    }

    [Fact]
    public void HomeSafetyCoreRemainsVisibleAndHonest()
    {
        XDocument document = XDocument.Load(MainWindowPath, LoadOptions.PreserveWhitespace);
        string xaml = document.ToString(SaveOptions.DisableFormatting);

        Assert.Contains("Trading Permission", xaml, StringComparison.Ordinal);
        Assert.Contains("Total Risk", xaml, StringComparison.Ordinal);
        Assert.Contains("Current Positions", xaml, StringComparison.Ordinal);
        Assert.Contains("Major Alerts", xaml, StringComparison.Ordinal);
        Assert.Contains("OFFLINE", xaml, StringComparison.Ordinal);
        Assert.Contains("UNKNOWN", xaml, StringComparison.Ordinal);
        Assert.Contains("Read only · Not tradable", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("ENABLED", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("All systems nominal", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void HomeUiPhase2SafetyCoreUsesOnePrimaryAlertAndThreeNeutralUnknownStates()
    {
        XDocument document = XDocument.Load(MainWindowPath, LoadOptions.PreserveWhitespace);
        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";

        string[] safetyCellNames = ["TradingPermissionCell", "TotalRiskCell", "PositionsCell", "AlertsCell"];
        XElement[] safetyCells = safetyCellNames
            .Select(name => document.Descendants(presentation + "Border")
                .Single(element => element.Attribute(x + "Name")?.Value == name))
            .ToArray();
        Assert.Equal(4, safetyCells.Length);
        Assert.Equal("190", safetyCells[0].Parent!
            .Element(presentation + "Grid.ColumnDefinitions")!
            .Elements(presentation + "ColumnDefinition")
            .First()
            .Attribute("Width")?.Value);

        string[] labels = ["Trading Permission", "Total Risk", "Current Positions", "Major Alerts"];
        XElement[] labelElements = labels
            .Select(label => document.Descendants(presentation + "TextBlock")
                .Single(element => element.Attribute("Text")?.Value == label))
            .ToArray();
        Assert.Equal("{StaticResource Strata.SafetyLabel.Primary}", labelElements[0].Attribute("Style")?.Value);
        Assert.All(labelElements.Skip(1), label => Assert.Equal("{StaticResource Strata.SafetyLabel}", label.Attribute("Style")?.Value));
        Assert.All(labelElements, label => Assert.Null(label.Attribute("TextDecorations")));

        XElement primaryCell = safetyCells[0];
        Assert.Equal(3, primaryCell.Descendants().Count(element => element.Attributes().Any(attribute => attribute.Value.Contains("Tcc.Strata.Brush.Danger", StringComparison.Ordinal))));
        Assert.All(safetyCells.Skip(1), cell => Assert.DoesNotContain(cell.Descendants().Attributes(), attribute => attribute.Value.Contains("Tcc.Strata.Brush.Danger", StringComparison.Ordinal)));
        Assert.Equal(3, safetyCells.Skip(1).SelectMany(cell => cell.Descendants(presentation + "TextBlock")).Count(element => element.Attribute("Text")?.Value == "UNKNOWN"));
    }

    [Fact]
    public void SeventhRoundUsesOneAlertFocusAndAQuietResultOrientedChartEmptyState()
    {
        string design = File.ReadAllText(DesignPath);
        XDocument document = XDocument.Load(MainWindowPath, LoadOptions.PreserveWhitespace);
        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        Assert.Contains("x:Key=\"Strata.StatusUnknown\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Strata.EmptyStateTitle\"", design, StringComparison.Ordinal);

        XElement[] unknownLabels = document.Descendants(presentation + "TextBlock")
            .Where(element => element.Attribute("Text")?.Value == "UNKNOWN")
            .ToArray();
        Assert.Equal(3, unknownLabels.Length);
        Assert.All(unknownLabels, label =>
        {
            Assert.Equal("{StaticResource Strata.StatusUnknown}", label.Attribute("Style")?.Value);
            XElement? marker = label.Parent?.Elements(presentation + "Control").SingleOrDefault();
            Assert.NotNull(marker);
            Assert.Equal("{StaticResource Strata.UnsetStatusMarker}", marker.Attribute("Style")?.Value);
        });

        XElement chartEmptyTitle = document.Descendants(presentation + "TextBlock")
            .Single(element => element.Attribute("Text")?.Value == "NO SERIES TO DISPLAY");
        Assert.Equal("{StaticResource Strata.EmptyStateTitle}", chartEmptyTitle.Attribute("Style")?.Value);
        Assert.Single(document.Descendants(presentation + "TextBlock"), element => element.Attribute("Text")?.Value == "PRICES · DATES · TRENDS REMAIN BLANK");
        Assert.DoesNotContain(document.Descendants(presentation + "TextBlock"), element => element.Attribute("Text")?.Value == "MARKET FEED OFFLINE");
    }

    [Fact]
    public void EighthRoundUsesOpticallyCalibratedNativeVectorGraphicsWithoutChangingProductTruth()
    {
        string xaml = File.ReadAllText(MainWindowPath);
        string design = File.ReadAllText(DesignPath);
        string iconography = File.ReadAllText(IconographyPath);
        XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);
        XDocument resources = XDocument.Parse(design, LoadOptions.PreserveWhitespace);
        XDocument icons = XDocument.Parse(iconography, LoadOptions.PreserveWhitespace);
        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";

        Assert.Contains("eighth graphics refinement R1", design, StringComparison.Ordinal);
        Assert.Contains("Nocturne Meridian R7", iconography, StringComparison.Ordinal);
        Assert.Contains("Tcc.NocturneMeridian.Home.24", iconography, StringComparison.Ordinal);
        Assert.Contains("Brush=\"{DynamicResource Tcc.Strata.Brush.Text}\"", iconography, StringComparison.Ordinal);
        Assert.Contains("Thickness=\"1.48\"", iconography, StringComparison.Ordinal);

        XElement navHome = document.Descendants(presentation + "Button")
            .Single(element => element.Attribute(x + "Name")?.Value == "NavHome");
        XElement homeGlyph = navHome.Descendants(presentation + "Control").Single();
        Assert.Equal("23", homeGlyph.Attribute("Width")?.Value);
        Assert.Equal("23", homeGlyph.Attribute("Height")?.Value);

        string[] secondaryNavIds = ["NavMarkets", "NavPlanning", "NavRisk", "NavPositions", "NavReview", "NavSettings"];
        foreach (string navId in secondaryNavIds)
        {
            XElement nav = document.Descendants(presentation + "Button")
                .Single(element => element.Attribute(x + "Name")?.Value == navId);
            XElement glyph = nav.Descendants(presentation + "Control").Single();
            Assert.Equal("22", glyph.Attribute("Width")?.Value);
            Assert.Equal("22", glyph.Attribute("Height")?.Value);
        }

        Assert.DoesNotContain("Text=\"⋮\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Style=\"{StaticResource Strata.KebabGlyph}\"", xaml, StringComparison.Ordinal);
        XElement kebabStyle = icons.Descendants(presentation + "Style")
            .Single(style => style.Attribute(x + "Key")?.Value == "Strata.KebabGlyph");
        Assert.Single(kebabStyle.Descendants(presentation + "Image"));
        Assert.Contains("Tcc.NocturneMeridian.More.16", iconography, StringComparison.Ordinal);

        XElement mentalLabel = document.Descendants(presentation + "TextBlock")
            .Single(element => element.Attribute(x + "Name")?.Value == "MentalStateValue");
        Assert.Equal("NOT SET", mentalLabel.Attribute("Text")?.Value);
        Assert.Equal("{StaticResource Strata.MentalStateValue}", mentalLabel.Attribute("Style")?.Value);
        XElement brushImage = document.Descendants(presentation + "Image")
            .Single(element => element.Attribute(x + "Name")?.Value == "MentalStateGlyph");
        Assert.Equal("MentalStateGlyph", brushImage.Attribute(x + "Name")?.Value);
        Assert.Equal("{DynamicResource Tcc.Strata.MentalStateGlyph.NotSet}", brushImage.Attribute("Source")?.Value);
        XElement mentalStage = Assert.IsType<XElement>(brushImage.Parent);
        Assert.Empty(mentalStage.Elements(presentation + "Path"));
        XElement mentalHelper = mentalStage.Elements(presentation + "TextBlock")
            .Single(element => element.Attribute(x + "Name")?.Value == "MentalStateHelper");
        Assert.Equal("NO SELF-CHECK", mentalHelper.Attribute("Text")?.Value);

        XElement verticalSeal = resources.Descendants(presentation + "Style")
            .Single(style => style.Attribute(x + "Key")?.Value == "Strata.Seal.Vertical");
        XElement squareSeal = resources.Descendants(presentation + "Style")
            .Single(style => style.Attribute(x + "Key")?.Value == "Strata.Seal.Square");
        Assert.Contains(verticalSeal.Elements(presentation + "Setter"), setter => setter.Attribute("Property")?.Value == "Opacity" && setter.Attribute("Value")?.Value == "0.96");
        Assert.Contains(squareSeal.Elements(presentation + "Setter"), setter => setter.Attribute("Property")?.Value == "Opacity" && setter.Attribute("Value")?.Value == "0.92");

        Assert.Contains("Text=\"SEARCH UNAVAILABLE\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"SearchKeyHint\" Text=\"OFFLINE\"", xaml, StringComparison.Ordinal);
        Assert.Contains("NO SERIES TO DISPLAY", xaml, StringComparison.Ordinal);
        Assert.Contains("UNKNOWN", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("ENABLED", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void ThemeIconographyUsesNocturneMeridianOpticalMastersWithoutReferenceMasterDependency()
    {
        string xaml = File.ReadAllText(MainWindowPath);
        string design = File.ReadAllText(DesignPath);
        string iconography = File.ReadAllText(IconographyPath);
        XDocument icons = XDocument.Parse(iconography, LoadOptions.PreserveWhitespace);
        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";

        Assert.Contains("Nocturne Meridian R7", iconography, StringComparison.Ordinal);
        Assert.Contains("no reference-image tracing", iconography, StringComparison.Ordinal);
        Assert.DoesNotContain("ReferenceMaster", iconography, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("mother", iconography, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotMatch("#[0-9A-Fa-f]{6,8}", iconography);
        Assert.DoesNotContain("FontFamily=", iconography, StringComparison.Ordinal);
        Assert.Contains("{DynamicResource Tcc.Strata.Brush.Background}", iconography, StringComparison.Ordinal);
        Assert.Contains("{DynamicResource Tcc.Strata.Brush.Text}", iconography, StringComparison.Ordinal);
        Assert.Contains("{DynamicResource Tcc.Strata.Brush.Gold}", iconography, StringComparison.Ordinal);
        Assert.Contains("{DynamicResource Tcc.Strata.Brush.Ice}", iconography, StringComparison.Ordinal);

        string[] requiredStyles =
        [
            "Strata.ThemeIcon.Base",
            "Strata.WindowGlyph.Minimize",
            "Strata.WindowGlyph.Maximize",
            "Strata.WindowGlyph.Close",
            "Strata.NavGlyph.Home",
            "Strata.NavGlyph.Market",
            "Strata.NavGlyph.Plan",
            "Strata.NavGlyph.Risk",
            "Strata.NavGlyph.Positions",
            "Strata.NavGlyph.Review",
            "Strata.NavGlyph.Settings",
            "Strata.CommandGlyph.Search",
            "Strata.CommandGlyph.Add",
            "Strata.KebabGlyph",
            "Strata.ChartOfflineGlyph",
            "Strata.UnsetStatusMarker",
        ];

        string[] requiredIcons =
        [
            "Home", "Market", "Plan", "Risk", "Positions", "Review", "Settings",
            "Search", "Add", "More", "Minimize", "Maximize", "Close", "Unavailable", "Unset",
        ];

        string[] styleKeys = icons.Descendants(presentation + "Style")
            .Select(style => style.Attribute(x + "Key")?.Value)
            .OfType<string>()
            .ToArray();
        string[] geometryKeys = icons.Descendants(presentation + "StreamGeometry")
            .Select(geometry => geometry.Attribute(x + "Key")?.Value)
            .OfType<string>()
            .ToArray();
        string[] drawingKeys = icons.Descendants(presentation + "DrawingImage")
            .Select(drawing => drawing.Attribute(x + "Key")?.Value)
            .OfType<string>()
            .ToArray();

        Assert.Equal(styleKeys.Length, styleKeys.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(geometryKeys.Length, geometryKeys.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(drawingKeys.Length, drawingKeys.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(requiredStyles.Length, styleKeys.Length);
        Assert.Equal(75, geometryKeys.Length);
        Assert.Equal(30, drawingKeys.Length);
        Assert.All(requiredStyles, key => Assert.Contains(key, styleKeys));
        Assert.All(requiredIcons, icon =>
        {
            Assert.Contains($"Tcc.NocturneMeridian.{icon}.24.Core", geometryKeys);
            Assert.Contains($"Tcc.NocturneMeridian.{icon}.24.Calibration", geometryKeys);
            Assert.Contains($"Tcc.NocturneMeridian.{icon}.24.Index", geometryKeys);
            Assert.Contains($"Tcc.NocturneMeridian.{icon}.16.Core", geometryKeys);
            Assert.Contains($"Tcc.NocturneMeridian.{icon}.16.Index", geometryKeys);
            Assert.Contains($"Tcc.NocturneMeridian.{icon}.24", drawingKeys);
            Assert.Contains($"Tcc.NocturneMeridian.{icon}.16", drawingKeys);
        });

        Assert.Contains("Style=\"{StaticResource Strata.CommandGlyph.Search}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource Strata.CommandGlyph.Add}\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Style=\"{StaticResource Strata.KebabGlyph}\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Content=\"＋\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("M8,1 A7,7 0 1 0", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("x:Key=\"Strata.NavGlyph.", design, StringComparison.Ordinal);
        Assert.DoesNotContain("x:Key=\"Strata.WindowGlyph.", design, StringComparison.Ordinal);
    }

    [Fact]
    public void OptionalAiAndConnectorBoundariesRemainHonest()
    {
        string xaml = File.ReadAllText(MainWindowPath);

        Assert.Contains("NO LIVE MARKET DATA", xaml, StringComparison.Ordinal);
        Assert.Contains("DATA UNAVAILABLE · READ ONLY", xaml, StringComparison.Ordinal);
        Assert.Contains("No execution API", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void ApplicationIdentityUsesDedicatedNocturneMeridianOpticalMasters()
    {
        string xaml = File.ReadAllText(MainWindowPath);
        string project = File.ReadAllText(ProjectPath);
        string codeBehind = File.ReadAllText(CodeBehindPath);

        Assert.Contains("<ApplicationIcon>Assets/Brand/Tcc.NocturneMeridian.AppIcon.ico</ApplicationIcon>", project, StringComparison.Ordinal);
        Assert.Contains("<Resource Include=\"Assets/Brand/Tcc.NocturneMeridian.AppIcon.ico\" />", project, StringComparison.Ordinal);
        Assert.Contains("<Resource Include=\"Assets/Brand/Tcc.NocturneMeridian.AppIcon.Window.png\" />", project, StringComparison.Ordinal);
        Assert.DoesNotContain(" Icon=", xaml, StringComparison.Ordinal);
        Assert.Contains("BitmapFrame.Create(new Uri(\"pack://application:,,,/Assets/Brand/Tcc.NocturneMeridian.AppIcon.Window.png\"", codeBehind, StringComparison.Ordinal);
        Assert.True(File.Exists(AppIconPath));
        Assert.True(File.Exists(AppIconWindowPath));
        byte[] windowIcon = File.ReadAllBytes(AppIconWindowPath);
        byte[] pngSignature = [137, 80, 78, 71, 13, 10, 26, 10];
        Assert.True(windowIcon.AsSpan(0, pngSignature.Length).SequenceEqual(pngSignature));
        Assert.Equal(256, (windowIcon[16] << 24) | (windowIcon[17] << 16) | (windowIcon[18] << 8) | windowIcon[19]);
        Assert.Equal(256, (windowIcon[20] << 24) | (windowIcon[21] << 16) | (windowIcon[22] << 8) | windowIcon[23]);

        string[] svgPaths = [AppIconSvgPath, AppIconSvg32Path, AppIconSvg16Path];
        Assert.All(svgPaths, path =>
        {
            Assert.True(File.Exists(path));
            string svg = File.ReadAllText(path);
            XDocument document = XDocument.Parse(svg, LoadOptions.PreserveWhitespace);
            Assert.Equal("svg", document.Root?.Name.LocalName);
            Assert.Contains("Nocturne Meridian", svg, StringComparison.Ordinal);
            Assert.DoesNotContain("<text", svg, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("<title", svg, StringComparison.Ordinal);
            Assert.Contains("<desc", svg, StringComparison.Ordinal);
        });

        byte[] icon = File.ReadAllBytes(AppIconPath);
        Assert.True(icon.Length > 6 + (16 * 9));
        Assert.Equal((ushort)0, BitConverter.ToUInt16(icon, 0));
        Assert.Equal((ushort)1, BitConverter.ToUInt16(icon, 2));
        Assert.Equal((ushort)9, BitConverter.ToUInt16(icon, 4));

        int[] expectedSizes = [16, 20, 24, 32, 40, 48, 64, 128, 256];
        List<int> actualSizes = [];
        for (int index = 0; index < expectedSizes.Length; index++)
        {
            int entry = 6 + (index * 16);
            int width = icon[entry] == 0 ? 256 : icon[entry];
            int height = icon[entry + 1] == 0 ? 256 : icon[entry + 1];
            int payloadLength = BitConverter.ToInt32(icon, entry + 8);
            int payloadOffset = BitConverter.ToInt32(icon, entry + 12);

            Assert.Equal(width, height);
            Assert.Equal((ushort)1, BitConverter.ToUInt16(icon, entry + 4));
            Assert.Equal((ushort)32, BitConverter.ToUInt16(icon, entry + 6));
            Assert.InRange(payloadOffset, 6 + (16 * 9), icon.Length - pngSignature.Length);
            Assert.InRange(payloadLength, pngSignature.Length, icon.Length - payloadOffset);
            Assert.True(icon.AsSpan(payloadOffset, pngSignature.Length).SequenceEqual(pngSignature));
            actualSizes.Add(width);
        }

        Assert.Equal(expectedSizes, actualSizes);
    }

    [Fact]
    public void TwoSourceBackgroundIdentityIsLocked()
    {
        Assert.True(File.Exists(ScenePath));
        string sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(ScenePath)));
        Assert.Equal("A5279D85EB7324982703BD757556938D3DFE8267E62AC9E893963156878A0804", sha256);
    }

    [Fact]
    public void EveryVisibleTypographyAndSurfaceRoleParticipatesInScalingAndAccessibility()
    {
        string xaml = File.ReadAllText(MainWindowPath);
        string design = File.ReadAllText(DesignPath);
        string codeBehind = File.ReadAllText(CodeBehindPath);

        Assert.DoesNotMatch(new Regex("#[0-9A-Fa-f]{6,8}", RegexOptions.CultureInvariant), xaml);
        Assert.DoesNotMatch(new Regex("FontSize=\"[0-9]", RegexOptions.CultureInvariant), xaml);
        Assert.DoesNotContain("FontFamily=\"Georgia\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("FontFamily=\"Microsoft JhengHei\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Tcc.Strata.Font.Cjk\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Tcc.Strata.Brush.SceneVeil\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Tcc.Strata.Brush.WorkspaceScrim\"", design, StringComparison.Ordinal);
        Assert.Contains("x:Key=\"Tcc.Strata.MentalStateGlyphOpacity\"", design, StringComparison.Ordinal);
        Assert.Contains("\"Tcc.Strata.Brush.Seal\"", codeBehind, StringComparison.Ordinal);
        Assert.Contains("SetVisualBrush(\"Tcc.Strata.Brush.Seal\", SystemColors.HighlightColor)", codeBehind, StringComparison.Ordinal);
        Assert.Contains("[\"Tcc.ReferenceMaster.FontSize.SectionTitle\"]", codeBehind, StringComparison.Ordinal);
        Assert.Contains("[\"Tcc.ReferenceMaster.FontSize.MarketValue\"]", codeBehind, StringComparison.Ordinal);
        Assert.Contains("[\"Tcc.ReferenceMaster.FontSize.Editorial\"]", codeBehind, StringComparison.Ordinal);
        Assert.Contains("[\"Tcc.ReferenceMaster.LineHeight.Editorial\"]", codeBehind, StringComparison.Ordinal);
        Assert.Contains("SetVisualBrush(\"Tcc.Strata.Brush.WorkspaceScrim\", Colors.Transparent)", codeBehind, StringComparison.Ordinal);
        Assert.Contains("_visualResources.Remove(\"Tcc.Strata.SceneOpacity\")", codeBehind, StringComparison.Ordinal);
        Assert.Contains("HorizontalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
        Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", xaml, StringComparison.Ordinal);
        Assert.Contains("PanningMode=\"Both\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Focusable=\"True\"", xaml, StringComparison.Ordinal);
        Assert.Contains("IsTabStop=\"True\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("<Viewbox Stretch=\"Uniform\" StretchDirection=\"Both\">", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"SEARCH UNAVAILABLE\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"SearchKeyHint\" Text=\"OFFLINE\"", xaml, StringComparison.Ordinal);
        Assert.Contains("LOCAL EVENTS", xaml, StringComparison.Ordinal);

        XDocument document = XDocument.Parse(xaml, LoadOptions.PreserveWhitespace);
        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XElement[] buttons = document.Descendants(presentation + "Button").ToArray();
        Assert.NotEmpty(buttons);
        Assert.All(buttons, button => Assert.False(
            string.IsNullOrWhiteSpace(button.Attribute("AutomationProperties.AutomationId")?.Value),
            $"Button '{button.Attribute("Content")?.Value}' must expose an AutomationId."));
        Assert.All(buttons, button => Assert.False(
            string.IsNullOrWhiteSpace(button.Attribute("AutomationProperties.Name")?.Value),
            $"Button '{button.Attribute("AutomationProperties.AutomationId")?.Value}' must expose an accessible name."));
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Tcc.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
