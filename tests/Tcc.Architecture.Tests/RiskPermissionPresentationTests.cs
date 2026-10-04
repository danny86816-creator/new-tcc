using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace Tcc.Architecture.Tests;

public sealed class RiskPermissionPresentationTests
{
    private static readonly string Root = RepositoryPaths.Root;
    private static readonly string ViewPath = Path.Combine(Root, "src", "Tcc.DesktopHost", "RiskPermissionView.xaml");
    private static readonly string ViewCodePath = Path.Combine(Root, "src", "Tcc.DesktopHost", "RiskPermissionView.xaml.cs");
    private static readonly string WindowPath = Path.Combine(Root, "src", "Tcc.DesktopHost", "MainWindow.xaml");
    private static readonly string WindowCodePath = Path.Combine(Root, "src", "Tcc.DesktopHost", "MainWindow.xaml.cs");
    private static readonly string ProjectPath = Path.Combine(Root, "src", "Tcc.DesktopHost", "Tcc.DesktopHost.csproj");
    private static readonly string BackgroundPath = Path.Combine(Root, "src", "Tcc.DesktopHost", "Assets", "Risk", "Tcc.RiskPermission.GuardedPass.Background.R1.png");
    private static readonly XNamespace P = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace X = "http://schemas.microsoft.com/winfx/2006/xaml";

    [Fact]
    public void RiskSliceIdentifiesTheApprovedPageAndPermissionModule()
    {
        string xaml = File.ReadAllText(ViewPath);

        Assert.Contains("交易權限總覽", xaml, StringComparison.Ordinal);
        Assert.Contains("UX-RISK-001 · MOD-RISK-PERMISSION", xaml, StringComparison.Ordinal);
        Assert.Contains("目前交易權限", xaml, StringComparison.Ordinal);
        Assert.Contains("不是正式風險判定", xaml, StringComparison.Ordinal);
        Assert.Contains("來源與新鮮度不可用", xaml, StringComparison.Ordinal);
        Assert.Contains("帳戶 / 群組", xaml, StringComparison.Ordinal);
        Assert.Contains("評估時間", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void PermissionSemanticsArePlainFixedAndNotColorOnly()
    {
        XDocument document = XDocument.Load(ViewPath);
        string xaml = File.ReadAllText(ViewPath);

        foreach ((string chinese, string invariant) in new[]
                 {
                     ("可交易", "TRADABLE"),
                     ("警告", "WARNING"),
                     ("封鎖", "BLOCKED"),
                 })
        {
            Assert.Contains($"Text=\"{chinese}\"", xaml, StringComparison.Ordinal);
            Assert.Contains($"Text=\"{invariant}\"", xaml, StringComparison.Ordinal);
        }

        XElement semantics = Assert.Single(document.Descendants(), element =>
            element.Attribute("AutomationProperties.AutomationId")?.Value == "Risk.Semantics");
        Assert.Contains("可交易、警告、封鎖", semantics.Attribute("AutomationProperties.Name")?.Value, StringComparison.Ordinal);
        Assert.Contains("語意不得隨主題改變", semantics.Attribute("AutomationProperties.Name")?.Value, StringComparison.Ordinal);
        Assert.Contains("下列為狀態定義，不是目前帳戶判定", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void RiskSliceKeepsEvidenceLimitsApprovalAndCorrectionVisible()
    {
        string xaml = File.ReadAllText(ViewPath);

        Assert.Contains("判定證據稽核", xaml, StringComparison.Ordinal);
        Assert.Contains("證據與原因", xaml, StringComparison.Ordinal);
        Assert.Contains("目前值 / 限制", xaml, StringComparison.Ordinal);
        Assert.Contains("核准 / 稽核", xaml, StringComparison.Ordinal);
        Assert.Contains("來源 / 新鮮度", xaml, StringComparison.Ordinal);
        Assert.Contains("解除路徑時間軸", xaml, StringComparison.Ordinal);
        foreach (string step in new[] { "舊值", "新值", "風險影響", "明確確認" })
        {
            Assert.Contains($"Text=\"{step}\"", xaml, StringComparison.Ordinal);
        }
        Assert.Contains("套用前必須模擬與安全驗證", xaml, StringComparison.Ordinal);
        Assert.Contains("本階段不提供操作，也沒有略過方式", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void RiskSliceReusesTheCompletedPlanViewportAndPanelRhythm()
    {
        XDocument document = XDocument.Load(ViewPath);
        string xaml = File.ReadAllText(ViewPath);

        XElement contentGrid = Assert.Single(document.Descendants(P + "Grid"), element =>
            element.Attribute("MinWidth")?.Value == "1476" &&
            element.Attribute("MinHeight")?.Value == "817");
        Assert.Equal("34,24,38,30", contentGrid.Attribute("Margin")?.Value);

        Assert.Contains("<ColumnDefinition Width=\"420\" />", xaml, StringComparison.Ordinal);
        Assert.Contains("<ColumnDefinition Width=\"18\" />", xaml, StringComparison.Ordinal);
        Assert.Contains("<RowDefinition Height=\"14\" />", xaml, StringComparison.Ordinal);
        Assert.Contains("Risk.Surface.Timeline", xaml, StringComparison.Ordinal);
        Assert.Contains("Risk.Band.Current", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void RiskBackgroundIsACompiledDecorativeAssetWithHighContrastBoundary()
    {
        byte[] asset = File.ReadAllBytes(BackgroundPath);
        Assert.Equal("754788F608D00D2F140DB309B0696B0C43ACCB27588F0A10D6137C2C0A2B950D",
            Convert.ToHexString(SHA256.HashData(asset)));
        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, asset[..8]);
        Assert.Equal(1672, BinaryPrimitives.ReadInt32BigEndian(asset.AsSpan(16, 4)));
        Assert.Equal(941, BinaryPrimitives.ReadInt32BigEndian(asset.AsSpan(20, 4)));

        XDocument project = XDocument.Load(ProjectPath);
        Assert.Single(project.Descendants("Resource"), resource =>
            resource.Attribute("Include")?.Value == "Assets/Risk/Tcc.RiskPermission.GuardedPass.Background.R1.png");

        XDocument view = XDocument.Load(ViewPath);
        XElement image = Assert.Single(view.Descendants(P + "Image"), candidate =>
            candidate.Attribute("Source")?.Value == "Assets/Risk/Tcc.RiskPermission.GuardedPass.Background.R1.png");
        Assert.Equal("UniformToFill", image.Attribute("Stretch")?.Value);
        Assert.Equal("{DynamicResource Tcc.Strata.SceneOpacity}", image.Attribute("Opacity")?.Value);
        Assert.Equal("False", image.Attribute("IsHitTestVisible")?.Value);
        Assert.Equal("False", image.Attribute("Focusable")?.Value);
        Assert.Null(image.Attribute("AutomationProperties.AutomationId"));
        Assert.Null(image.Attribute("AutomationProperties.Name"));
    }

    [Fact]
    public void RiskRegionsUseDistinctVisualGrammarsInsteadOfRepeatedCardGrids()
    {
        string xaml = File.ReadAllText(ViewPath);

        Assert.Contains("權限判定圖例", xaml, StringComparison.Ordinal);
        Assert.Contains("三種狀態牌", xaml, StringComparison.Ordinal);
        Assert.Contains("判定證據稽核", xaml, StringComparison.Ordinal);
        Assert.Contains("4 項必要輸入 · 0 項可用", xaml, StringComparison.Ordinal);
        Assert.Contains("解除路徑時間軸", xaml, StringComparison.Ordinal);
        Assert.Contains("高影響變更確認帶", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Risk.Surface.Step", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void RiskSliceImplementsRequiredPresentationStatesAndLiveAnnouncement()
    {
        XDocument document = XDocument.Load(ViewPath);
        string xaml = File.ReadAllText(ViewPath);
        string code = File.ReadAllText(ViewCodePath);

        foreach (string state in new[] { "EmptyState", "LoadingState", "OfflineState", "ErrorState", "BlockedState" })
        {
            Assert.Single(document.Descendants(), element => element.Attribute(X + "Name")?.Value == state);
            Assert.Contains($"RiskPermissionPreviewState.{state.Replace("State", string.Empty, StringComparison.Ordinal)}", code, StringComparison.Ordinal);
        }

        XElement stateRegion = Assert.Single(document.Descendants(), element =>
            element.Attribute("AutomationProperties.AutomationId")?.Value == "Risk.StateRegion");
        Assert.Equal("Polite", stateRegion.Attribute("AutomationProperties.LiveSetting")?.Value);
        Assert.Contains("RaiseAutomationEvent(AutomationEvents.LiveRegionChanged)", code, StringComparison.Ordinal);
        Assert.Contains("系統不做推測", xaml + code, StringComparison.Ordinal);
    }

    [Fact]
    public void RiskSliceIsKeyboardScrollableAndReadOnly()
    {
        XDocument document = XDocument.Load(ViewPath);
        XElement scroller = Assert.Single(document.Descendants(P + "ScrollViewer"));

        Assert.Equal("RiskPermissionScroller", scroller.Attribute("AutomationProperties.AutomationId")?.Value);
        Assert.Equal("Auto", scroller.Attribute("HorizontalScrollBarVisibility")?.Value);
        Assert.Equal("Auto", scroller.Attribute("VerticalScrollBarVisibility")?.Value);
        Assert.Equal("False", scroller.Attribute("CanContentScroll")?.Value);
        Assert.Equal("Both", scroller.Attribute("PanningMode")?.Value);
        Assert.Equal("True", scroller.Attribute("Focusable")?.Value);
        Assert.Equal("True", scroller.Attribute("IsTabStop")?.Value);
        Assert.Equal("{StaticResource Risk.ViewportFocusVisual}", scroller.Attribute("FocusVisualStyle")?.Value);

        XElement focusStyle = Assert.Single(document.Descendants(P + "Style"), element =>
            element.Attribute(X + "Key")?.Value == "Risk.ViewportFocusVisual");
        XElement focusBorder = Assert.Single(focusStyle.Descendants(P + "ControlTemplate").Descendants(P + "Border"));
        Assert.Equal("2", focusBorder.Attribute("Margin")?.Value);
        Assert.Equal("2", focusBorder.Attribute("BorderThickness")?.Value);
        Assert.Equal("{DynamicResource Tcc.Strata.Brush.Ice}", focusBorder.Attribute("BorderBrush")?.Value);

        Assert.Empty(document.Descendants(P + "Button"));
        Assert.Empty(document.Descendants(P + "Viewbox"));
        Assert.Empty(document.Descendants(P + "LayoutTransform"));
        Assert.Empty(document.Descendants(P + "ScaleTransform"));
        Assert.DoesNotContain(document.Descendants(), element =>
            element.Attribute("Click") is not null || element.Attribute("Command") is not null);
    }

    [Fact]
    public void HostActivatesRiskAsTheThirdNavigationDestination()
    {
        XDocument window = XDocument.Load(WindowPath);
        string code = File.ReadAllText(WindowCodePath);

        XElement riskSurface = Assert.Single(window.Descendants(), element => element.Name.LocalName == "RiskPermissionView");
        Assert.Equal("Collapsed", riskSurface.Attribute("Visibility")?.Value);
        Assert.Equal("104", riskSurface.Attribute("Canvas.Left")?.Value);
        Assert.Equal("52", riskSurface.Attribute("Canvas.Top")?.Value);

        XElement navRisk = Assert.Single(window.Descendants(P + "Button"), element =>
            element.Attribute(X + "Name")?.Value == "NavRisk");
        Assert.NotEqual("False", navRisk.Attribute("IsEnabled")?.Value);
        Assert.NotEqual("False", navRisk.Attribute("IsTabStop")?.Value);
        Assert.Equal("OnRiskNavigationClick", navRisk.Attribute("Click")?.Value);
        Assert.Equal("Available", navRisk.Attribute("AutomationProperties.ItemStatus")?.Value);
        Assert.Null(navRisk.Attribute("Command"));
        Assert.Equal("Collapsed", Assert.Single(navRisk.Descendants(P + "TextBlock"), element =>
            element.Attribute(X + "Name")?.Value == "NavRiskCurrentState").Attribute("Visibility")?.Value);

        Assert.Contains("private const string RiskVisualFixture = \"RISK_R1\"", code, StringComparison.Ordinal);
        Assert.Contains("private void OnRiskNavigationClick(object sender, RoutedEventArgs e)", code, StringComparison.Ordinal);
        Assert.Contains("private void ApplyShellPage(bool showPlan, bool showRisk, bool announce)", code, StringComparison.Ordinal);
        Assert.Contains("RiskPermissionSurface.Visibility = showRisk ? Visibility.Visible : Visibility.Collapsed", code, StringComparison.Ordinal);
        Assert.Contains("NavRisk.Style = (Style)FindResource(showRisk ? \"Strata.NavButton.Selected\" : \"Strata.NavButton\")", code, StringComparison.Ordinal);
        Assert.Contains("NavRiskCurrentState.Visibility = showRisk ? Visibility.Visible : Visibility.Collapsed", code, StringComparison.Ordinal);
        Assert.Contains("RiskPermissionSurface.ShowPreviewState(RiskPermissionPreviewState.Offline)", code, StringComparison.Ordinal);
        Assert.Contains("TCC — 交易權限總覽", code, StringComparison.Ordinal);
    }

    [Fact]
    public void RiskSlicePreservesOptionalGptAndReadOnlyConnectorTruth()
    {
        string combined = File.ReadAllText(ViewPath) + File.ReadAllText(ViewCodePath);

        Assert.Contains("連接器 · 僅限唯讀同步／對帳／監控 · 沒有下單或執行路徑", combined, StringComparison.Ordinal);
        Assert.Contains("GPT · 可選 · 非正式評分", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("HttpClient", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("DbContext", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectReference", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("ExecuteOrder", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("PlaceOrder", combined, StringComparison.Ordinal);
    }
}
