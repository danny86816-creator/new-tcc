using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;

namespace Tcc.Architecture.Tests;

public sealed class PlanDashboardPresentationTests
{
    private static readonly string Root = RepositoryPaths.Root;
    private static readonly string ViewPath = Path.Combine(Root, "src", "Tcc.DesktopHost", "PlanDashboardView.xaml");
    private static readonly string ViewCodePath = Path.Combine(Root, "src", "Tcc.DesktopHost", "PlanDashboardView.xaml.cs");
    private static readonly string WindowPath = Path.Combine(Root, "src", "Tcc.DesktopHost", "MainWindow.xaml");
    private static readonly string WindowCodePath = Path.Combine(Root, "src", "Tcc.DesktopHost", "MainWindow.xaml.cs");
    private static readonly string ProjectPath = Path.Combine(Root, "src", "Tcc.DesktopHost", "Tcc.DesktopHost.csproj");
    private static readonly string BackgroundPath = Path.Combine(Root, "src", "Tcc.DesktopHost", "Assets", "Plan", "Tcc.PlanDashboard.HomeStyle.Background.R1.png");
    private static readonly string ContractPath = Path.Combine(Root, "contracts", "theme", "page-contracts", "UX-PLAN-001.json");
    private static readonly XNamespace P = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace X = "http://schemas.microsoft.com/winfx/2006/xaml";

    [Fact]
    public void PlanSliceTracesTheApprovedUxPlanAuthority()
    {
        using JsonDocument contract = JsonDocument.Parse(File.ReadAllText(ContractPath));
        JsonElement root = contract.RootElement;

        Assert.Equal("UX-PLAN-001", root.GetProperty("surface_id").GetString());
        Assert.Equal("Trade Plan Dashboard", root.GetProperty("surface_name").GetString());
        Assert.Contains("Draft and formal distinction preserved",
            root.GetProperty("safety_constraints").EnumerateArray().Select(value => value.GetString()));

        string phase = File.ReadAllText(Path.Combine(Root, "docs", "design", "TCC_PLAN_DASHBOARD_WPF_FIRST_VISIBLE_SLICE_R1.md"));
        Assert.Contains("IMPLEMENTED / VALIDATED / HUMAN VISUAL ACCEPTED / LOCAL COMMIT EXISTS", phase, StringComparison.Ordinal);
        Assert.Contains("UX-PLAN-001", phase, StringComparison.Ordinal);
        Assert.Contains("MOD-PLAN-WORKBENCH", phase, StringComparison.Ordinal);
        Assert.Contains("Real navigation or enabling the existing left-rail Plan button", phase, StringComparison.Ordinal);

        string visualAlignment = File.ReadAllText(Path.Combine(Root, "docs", "design", "TCC_PLAN_DASHBOARD_WPF_VISUAL_ALIGNMENT_R2.md"));
        Assert.Contains("IMPLEMENTED / VALIDATED / HUMAN VISUAL ACCEPTED / LOCAL COMMIT EXISTS", visualAlignment, StringComparison.Ordinal);
    }

    [Fact]
    public void PlanSliceKeepsDraftFormalAndSafetySemanticsVisible()
    {
        string xaml = File.ReadAllText(ViewPath);

        Assert.Contains("計畫工作台", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"草稿\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"正式快照\"", xaml, StringComparison.Ordinal);
        Assert.Contains("草稿計畫與正式快照維持明確區隔。", xaml, StringComparison.Ordinal);
        Assert.Contains("草稿 ≠ 正式 · 操作前先確認安全", xaml, StringComparison.Ordinal);

        Assert.Contains("AutomationProperties.AutomationId=\"Plan.SafetyCore\"", xaml, StringComparison.Ordinal);
        Assert.Contains("交易權限", xaml, StringComparison.Ordinal);
        Assert.Contains("總風險", xaml, StringComparison.Ordinal);
        Assert.Contains("持倉 / 警示", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"01\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"目前\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"02\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"建議\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"03\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"風險影響\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"04\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"確認\"", xaml, StringComparison.Ordinal);
        Assert.Contains("正式化前請先檢視風險", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"交易計畫儀表板唯讀展示\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"離線。計畫資料無法使用，系統不推測任何計畫。\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("PLAN WORKBENCH", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("OFFLINE · READ ONLY", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("UNKNOWN", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("UNAVAILABLE", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void PlanVisualAlignmentUsesOneNativeWorkbenchAndFourReusableSurfaceTypes()
    {
        XDocument document = XDocument.Load(ViewPath);
        string xaml = File.ReadAllText(ViewPath);

        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        string[] surfaceKeys = document.Descendants(presentation + "Style")
            .Select(style => style.Attribute(X + "Key")?.Value)
            .Where(key => key?.StartsWith("Plan.Surface.", StringComparison.Ordinal) == true)
            .Cast<string>()
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
        [
            "Plan.Surface.Action",
            "Plan.Surface.Primary",
            "Plan.Surface.Secondary",
            "Plan.Surface.Step",
        ], surfaceKeys);
        Assert.Contains("首頁安全核心", xaml, StringComparison.Ordinal);
        Assert.Contains("計畫工作台", xaml, StringComparison.Ordinal);
        Assert.Contains("安全檢視", xaml, StringComparison.Ordinal);
        Assert.Contains("資料來源 / 更新狀態", xaml, StringComparison.Ordinal);
        Assert.Contains("下一步", xaml, StringComparison.Ordinal);
        XElement safetyCore = Assert.Single(document.Descendants(), element =>
            element.Attribute("AutomationProperties.AutomationId")?.Value == "Plan.SafetyCore");
        Assert.Equal(presentation + "TextBlock", safetyCore.Name);
        Assert.DoesNotContain("#", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("CornerRadius=\"8", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("CornerRadius=\"12", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void PlanBackgroundUsesAcceptedHomeStyleAssetAndHighContrastBoundary()
    {
        byte[] asset = File.ReadAllBytes(BackgroundPath);
        Assert.Equal("19BC4132539F7614843F5B0955364DE8FC717B903F6C227651D9F786A40F3F05",
            Convert.ToHexString(SHA256.HashData(asset)));
        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, asset[..8]);
        Assert.Equal(1672, BinaryPrimitives.ReadInt32BigEndian(asset.AsSpan(16, 4)));
        Assert.Equal(941, BinaryPrimitives.ReadInt32BigEndian(asset.AsSpan(20, 4)));

        XDocument project = XDocument.Load(ProjectPath);
        Assert.Single(project.Descendants("Resource"), resource =>
            resource.Attribute("Include")?.Value == "Assets/Plan/Tcc.PlanDashboard.HomeStyle.Background.R1.png");

        XDocument view = XDocument.Load(ViewPath);
        XElement image = Assert.Single(view.Descendants(P + "Image"), candidate =>
            candidate.Attribute("Source")?.Value == "Assets/Plan/Tcc.PlanDashboard.HomeStyle.Background.R1.png");
        Assert.Equal("UniformToFill", image.Attribute("Stretch")?.Value);
        Assert.Equal("{DynamicResource Tcc.Strata.SceneOpacity}", image.Attribute("Opacity")?.Value);
        Assert.Equal("False", image.Attribute("IsHitTestVisible")?.Value);
        Assert.Equal("False", image.Attribute("Focusable")?.Value);
        Assert.Null(image.Attribute("AutomationProperties.AutomationId"));
        Assert.Null(image.Attribute("AutomationProperties.Name"));
        Assert.Null(image.Attribute(X + "Name"));

        string hostCode = File.ReadAllText(WindowCodePath);
        Assert.Contains("_visualResources[\"Tcc.Strata.SceneOpacity\"] = 0d;", hostCode, StringComparison.Ordinal);
    }

    [Fact]
    public void PlanSliceImplementsRequiredPresentationStatesAndLiveAnnouncement()
    {
        XDocument document = XDocument.Load(ViewPath);
        string code = File.ReadAllText(ViewCodePath);

        foreach (string state in new[] { "EmptyState", "LoadingState", "OfflineState", "ErrorState", "BlockedState" })
        {
            Assert.Single(document.Descendants(), element => element.Attribute(X + "Name")?.Value == state);
            Assert.Contains($"PlanDashboardPreviewState.{state.Replace("State", string.Empty, StringComparison.Ordinal)}", code, StringComparison.Ordinal);
        }

        XElement stateRegion = Assert.Single(document.Descendants(), element =>
            element.Attribute("AutomationProperties.AutomationId")?.Value == "Plan.StateRegion");
        Assert.Equal("Polite", stateRegion.Attribute("AutomationProperties.LiveSetting")?.Value);
        Assert.Contains("RaiseAutomationEvent(AutomationEvents.LiveRegionChanged)", code, StringComparison.Ordinal);
    }

    [Fact]
    public void PlanSliceIsKeyboardScrollableAndDoesNotRasterScale()
    {
        XDocument document = XDocument.Load(ViewPath);
        XElement scroller = Assert.Single(document.Descendants(P + "ScrollViewer"));

        Assert.Equal("PlanDashboardScroller", scroller.Attribute("AutomationProperties.AutomationId")?.Value);
        Assert.Equal("Auto", scroller.Attribute("HorizontalScrollBarVisibility")?.Value);
        Assert.Equal("Auto", scroller.Attribute("VerticalScrollBarVisibility")?.Value);
        Assert.Equal("False", scroller.Attribute("CanContentScroll")?.Value);
        Assert.Equal("Both", scroller.Attribute("PanningMode")?.Value);
        Assert.Equal("True", scroller.Attribute("Focusable")?.Value);
        Assert.Equal("True", scroller.Attribute("IsTabStop")?.Value);
        Assert.Equal("{StaticResource Plan.ViewportFocusVisual}", scroller.Attribute("FocusVisualStyle")?.Value);
        XElement planFocusStyle = Assert.Single(document.Descendants(P + "Style"), element =>
            element.Attribute(X + "Key")?.Value == "Plan.ViewportFocusVisual");
        AssertViewportSafeFocusStyle(planFocusStyle);
        Assert.Equal(
            "唯讀交易計畫儀表板。文字放大時，請在此計畫面板按 PageDown。視窗較小時，按 Tab 移至外層頁面捲動區，再按 PageDown 前往計畫下方區域；可用向左鍵或向右鍵檢視完整寬度。",
            scroller.Attribute("AutomationProperties.HelpText")?.Value);

        XDocument window = XDocument.Load(WindowPath);
        XElement outerScroller = Assert.Single(window.Descendants(P + "ScrollViewer"), element =>
            element.Attribute("AutomationProperties.AutomationId")?.Value == "MainContentScroller");
        Assert.Equal("Auto", outerScroller.Attribute("HorizontalScrollBarVisibility")?.Value);
        Assert.Equal("Auto", outerScroller.Attribute("VerticalScrollBarVisibility")?.Value);
        Assert.Equal("True", outerScroller.Attribute("Focusable")?.Value);
        Assert.Equal("True", outerScroller.Attribute("IsTabStop")?.Value);
        Assert.Equal("{x:Null}", outerScroller.Attribute("FocusVisualStyle")?.Value);
        Assert.DoesNotContain(window.Descendants(P + "Style"), element =>
            element.Attribute(X + "Key")?.Value == "Tcc.Strata.ViewportFocusVisual");
        XElement outerFocusTrigger = Assert.Single(window.Descendants(P + "DataTrigger"), element =>
            element.Attribute("Binding")?.Value ==
                "{Binding Children[0].IsKeyboardFocused, RelativeSource={RelativeSource AncestorType={x:Type Grid}}}" &&
            element.Attribute("Value")?.Value == "True");
        XElement outerFocusStyle = Assert.IsType<XElement>(outerFocusTrigger.Parent?.Parent);
        XElement outerFocusBorder = Assert.IsType<XElement>(outerFocusStyle.Parent?.Parent);
        Assert.Equal(P + "Border", outerFocusBorder.Name);
        AssertViewportSafeFocusBorder(outerFocusBorder);
        Assert.Equal("False", outerFocusBorder.Attribute("Focusable")?.Value);
        Assert.Equal("False", outerFocusBorder.Attribute("IsHitTestVisible")?.Value);
        Assert.Equal("100", outerFocusBorder.Attribute("Panel.ZIndex")?.Value);
        Assert.Equal("True", outerFocusBorder.Attribute("SnapsToDevicePixels")?.Value);
        Assert.Contains(outerFocusStyle.Elements(P + "Setter"), element =>
            element.Attribute("Property")?.Value == "Visibility" && element.Attribute("Value")?.Value == "Collapsed");
        Assert.Contains(outerFocusTrigger.Elements(P + "Setter"), element =>
            element.Attribute("Property")?.Value == "Visibility" && element.Attribute("Value")?.Value == "Visible");
        Assert.DoesNotContain("IsKeyboardFocusWithin", window.ToString(SaveOptions.DisableFormatting), StringComparison.Ordinal);
        Assert.Empty(document.Descendants(P + "Viewbox"));
        Assert.Empty(document.Descendants(P + "LayoutTransform"));
        Assert.Empty(document.Descendants(P + "ScaleTransform"));

        XElement[] enabledButtons = document.Descendants(P + "Button")
            .Where(button => button.Attribute("IsEnabled")?.Value != "False")
            .ToArray();
        Assert.Empty(enabledButtons);
    }

    [Fact]
    public void NextActionIsExplicitlyReadOnlyDirectionRatherThanFalseControl()
    {
        XDocument document = XDocument.Load(ViewPath);
        XElement nextAction = Assert.Single(document.Descendants(P + "Border"), element =>
            element.Attribute("AutomationProperties.Name")?.Value.StartsWith("下一步。", StringComparison.Ordinal) == true);

        Assert.Equal("False", nextAction.Attribute("Focusable")?.Value);
        Assert.Equal("False", nextAction.Attribute("IsHitTestVisible")?.Value);
        Assert.Empty(nextAction.Descendants(P + "Button"));
        Assert.DoesNotContain(nextAction.Descendants(), element =>
            element.Attribute("Click") is not null || element.Attribute("Command") is not null);

        Assert.Single(nextAction.Descendants(P + "TextBlock"), element =>
            element.Attribute("Text")?.Value.Contains("本階段無法執行", StringComparison.Ordinal) == true);
        XElement directionMarker = Assert.Single(nextAction.Descendants(P + "TextBlock"), element =>
            element.Attribute("Text")?.Value == "→");
        Assert.Equal("False", directionMarker.Attribute("IsHitTestVisible")?.Value);
        Assert.Equal("{DynamicResource Tcc.Strata.Brush.Gold}", directionMarker.Attribute("Foreground")?.Value);
    }

    [Fact]
    public void HostActivatesOnlyHomeAndPlanNavigationWithSynchronizedPageState()
    {
        XDocument window = XDocument.Load(WindowPath);
        string code = File.ReadAllText(WindowCodePath);

        XElement plan = Assert.Single(window.Descendants(), element => element.Name.LocalName == "PlanDashboardView");
        Assert.Equal("Collapsed", plan.Attribute("Visibility")?.Value);
        Assert.Equal("104", plan.Attribute("Canvas.Left")?.Value);
        Assert.Equal("52", plan.Attribute("Canvas.Top")?.Value);

        XElement navHome = Assert.Single(window.Descendants(P + "Button"), element =>
            element.Attribute(X + "Name")?.Value == "NavHome");
        Assert.NotEqual("False", navHome.Attribute("IsEnabled")?.Value);
        Assert.NotEqual("False", navHome.Attribute("IsTabStop")?.Value);
        Assert.Equal("OnHomeNavigationClick", navHome.Attribute("Click")?.Value);
        Assert.Null(navHome.Attribute("Command"));
        XElement homeLabel = Assert.Single(navHome.Descendants(P + "TextBlock"), element =>
            element.Attribute("Text")?.Value == "HOME");
        Assert.Equal("{StaticResource Strata.NavLabel.Selected}", homeLabel.Attribute("Style")?.Value);
        Assert.Equal(
            "{Binding Foreground, ElementName=NavHome}",
            homeLabel.Attribute("Foreground")?.Value);

        XElement navPlanning = Assert.Single(window.Descendants(P + "Button"), element =>
            element.Attribute(X + "Name")?.Value == "NavPlanning");
        Assert.NotEqual("False", navPlanning.Attribute("IsEnabled")?.Value);
        Assert.NotEqual("False", navPlanning.Attribute("IsTabStop")?.Value);
        Assert.Equal("OnPlanNavigationClick", navPlanning.Attribute("Click")?.Value);
        Assert.Null(navPlanning.Attribute("Command"));
        XElement planLabel = Assert.Single(navPlanning.Descendants(P + "TextBlock"), element =>
            element.Attribute("Text")?.Value == "PLAN");
        Assert.Equal("{StaticResource Strata.NavLabel}", planLabel.Attribute("Style")?.Value);
        Assert.Equal(
            "{Binding Foreground, ElementName=NavPlanning}",
            planLabel.Attribute("Foreground")?.Value);
        Assert.Equal("Collapsed", Assert.Single(navPlanning.Descendants(P + "TextBlock"), element =>
            element.Attribute(X + "Name")?.Value == "NavPlanningCurrentState").Attribute("Visibility")?.Value);

        Assert.Contains("private const string PlanVisualFixture = \"PLAN_R1\"", code, StringComparison.Ordinal);
        Assert.Contains("private void OnHomeNavigationClick(object sender, RoutedEventArgs e)", code, StringComparison.Ordinal);
        Assert.Contains("private void OnPlanNavigationClick(object sender, RoutedEventArgs e)", code, StringComparison.Ordinal);
        Assert.Contains("private void ApplyShellPage(bool showPlan, bool announce)", code, StringComparison.Ordinal);
        Assert.Contains("HomePageLayer.Visibility = showPlan ? Visibility.Collapsed : Visibility.Visible", code, StringComparison.Ordinal);
        Assert.Contains("PlanDashboardSurface.Visibility = showPlan ? Visibility.Visible : Visibility.Collapsed", code, StringComparison.Ordinal);
        Assert.Contains("NavHome.Style = (Style)FindResource(showPlan ? \"Strata.NavButton\" : \"Strata.NavButton.Selected\")", code, StringComparison.Ordinal);
        Assert.Contains("NavHomeCurrentState.Visibility = showPlan ? Visibility.Collapsed : Visibility.Visible", code, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.SetName(NavHome, showPlan ? \"Open Home\" : \"Home selected, current page\")", code, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.SetItemStatus(NavHome, showPlan ? \"Available\" : \"Current page\")", code, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.SetHelpText(NavHome, showPlan ? \"Navigates to the Home page.\" : \"Current page.\")", code, StringComparison.Ordinal);
        Assert.Contains("NavHome.ToolTip = showPlan ? \"Open Home\" : \"Current page\"", code, StringComparison.Ordinal);
        Assert.Contains("NavPlanning.Style = (Style)FindResource(showPlan ? \"Strata.NavButton.Selected\" : \"Strata.NavButton\")", code, StringComparison.Ordinal);
        Assert.Contains("NavPlanningCurrentState.Visibility = showPlan ? Visibility.Visible : Visibility.Collapsed", code, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.SetName(NavPlanning, showPlan ? \"Plan selected, current page\" : \"Open Plan\")", code, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.SetItemStatus(NavPlanning, showPlan ? \"Current page\" : \"Available\")", code, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.SetHelpText(NavPlanning, showPlan ? \"Current page.\" : \"Navigates to the read-only Plan page.\")", code, StringComparison.Ordinal);
        Assert.Contains("NavPlanning.ToolTip = showPlan ? \"Current page\" : \"Open Plan\"", code, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.SetName(MainContentScroller, showPlan ? \"交易計畫儀表板\" : \"Strata Observatory HOME\")", code, StringComparison.Ordinal);
        Assert.Contains("_baseTitle = showPlan ? \"TCC — 交易計畫儀表板\" : \"TCC — Strata Observatory\"", code, StringComparison.Ordinal);
        Assert.Contains("PlanDashboardSurface.ShowPreviewState(PlanDashboardPreviewState.Offline)", code, StringComparison.Ordinal);
        Assert.Contains("AutomationEvents.LiveRegionChanged", code, StringComparison.Ordinal);
        Assert.Contains("交易計畫儀表板，視覺測試", code, StringComparison.Ordinal);
        Assert.Contains("TCC — 交易計畫儀表板", code, StringComparison.Ordinal);
        Assert.DoesNotContain("ICommand", code, StringComparison.Ordinal);
        Assert.DoesNotContain("DbContext", code, StringComparison.Ordinal);
    }

    [Fact]
    public void PlanSlicePreservesOptionalGptReadOnlyConnectorAndManualConflictTruth()
    {
        string xaml = File.ReadAllText(ViewPath);
        string code = File.ReadAllText(ViewCodePath);

        Assert.Contains("連接器 · 僅限唯讀範圍", xaml, StringComparison.Ordinal);
        Assert.Contains("GPT · 可選 · 未使用 · 非正式評分", xaml, StringComparison.Ordinal);
        Assert.Contains("衝突仍須手動處理；目前沒有下單或執行路徑。", xaml, StringComparison.Ordinal);

        string combined = xaml + code;
        Assert.DoesNotContain("HttpClient", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("DbContext", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectReference", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("ExecuteOrder", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("PlaceOrder", combined, StringComparison.Ordinal);
    }

    private static void AssertViewportSafeFocusStyle(XElement style)
    {
        XElement template = Assert.Single(style.Descendants(P + "ControlTemplate"));
        XElement border = Assert.Single(template.Descendants(P + "Border"));
        AssertViewportSafeFocusBorder(border);
    }

    private static void AssertViewportSafeFocusBorder(XElement border)
    {
        Assert.Equal("2", border.Attribute("Margin")?.Value);
        Assert.Equal("2", border.Attribute("BorderThickness")?.Value);
        Assert.Equal("{DynamicResource Tcc.Strata.Brush.Ice}", border.Attribute("BorderBrush")?.Value);
        Assert.DoesNotContain("-", border.Attribute("Margin")!.Value, StringComparison.Ordinal);
    }
}
