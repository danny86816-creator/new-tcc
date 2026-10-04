using System.ComponentModel;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tcc.DesktopHost.EmbeddedSafeTheme;
using Tcc.Windows.Monitors;

namespace Tcc.DesktopHost;

public partial class MainWindow : Window
{
    private const int DisplayChangeMessage = 0x007E;
    private const int SettingChangeMessage = 0x001A;
    private const string VisualFixtureEnvironmentVariable = "TCC_VISUAL_FIXTURE";
    private const string MasterVisualFixture = "MASTER_R1";
    private const string PlanVisualFixture = "PLAN_R1";
    private const string MonitorUnavailableSuffix = " — 顯示器／DPI 資訊暫不可用";
    private static readonly string[] StrataBrushOverrideKeys =
    [
        "Tcc.Strata.Brush.Background",
        "Tcc.Strata.Brush.SceneVeil",
        "Tcc.Strata.Brush.WorkspaceScrim",
        "Tcc.Strata.Brush.EditorialScrim",
        "Tcc.Strata.Brush.LowerPanelScrim",
        "Tcc.Strata.Brush.Chrome",
        "Tcc.Strata.Brush.Nav",
        "Tcc.Strata.Brush.Panel",
        "Tcc.Strata.Brush.Panel.Secondary",
        "Tcc.Strata.Brush.Panel.Safety",
        "Tcc.Strata.Brush.HeaderField",
        "Tcc.Strata.Brush.ChartInset",
        "Tcc.Strata.Brush.PanelDeep",
        "Tcc.Strata.Brush.PanelQuiet",
        "Tcc.Strata.Brush.SearchSurface",
        "Tcc.Strata.Brush.SelectedSurface",
        "Tcc.Strata.Brush.PressedSurface",
        "Tcc.Strata.Brush.TabSurface",
        "Tcc.Strata.Brush.TabSelectedSurface",
        "Tcc.Strata.Brush.FooterBand",
        "Tcc.Strata.Brush.EdgeGlint",
        "Tcc.Strata.Brush.ChartGrid",
        "Tcc.Strata.Brush.ChartAxis",
        "Tcc.Strata.Brush.RowSeparator",
        "Tcc.Strata.Brush.Border",
        "Tcc.Strata.Brush.BorderQuiet",
        "Tcc.Strata.Brush.BorderBright",
        "Tcc.Strata.Brush.Text",
        "Tcc.Strata.Brush.TextSecondary",
        "Tcc.Strata.Brush.Muted",
        "Tcc.Strata.Brush.Ice",
        "Tcc.Strata.Brush.Gold",
        "Tcc.Strata.Brush.Seal",
        "Tcc.Strata.Brush.Danger",
        "Tcc.Strata.Brush.Success",
    ];
    private readonly MainWindowViewModel _viewModel;
    private readonly WindowMonitorAdapter _monitorAdapter;
    private readonly ResourceDictionary _visualResources;
    private readonly Dictionary<string, double> _baseTypographyMetrics;
    private string _baseTitle = string.Empty;
    private double _textScale = 1d;
    private HwndSource? _windowSource;
    private bool _isEnsuringVisible;

    public MainWindow(
        MainWindowViewModel viewModel,
        WindowMonitorAdapter monitorAdapter)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _monitorAdapter = monitorAdapter ?? throw new ArgumentNullException(nameof(monitorAdapter));
        InitializeComponent();
        Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Assets/Brand/Tcc.NocturneMeridian.AppIcon.Window.png", UriKind.Absolute));
        _baseTitle = Title;
        _visualResources = Resources;
        _baseTypographyMetrics = new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["Tcc.ReferenceMaster.FontSize.AppBrand"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.AppBrand"),
            ["Tcc.ReferenceMaster.FontSize.BrandMark"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.BrandMark"),
            ["Tcc.ReferenceMaster.FontSize.WorkspaceSelector"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.WorkspaceSelector"),
            ["Tcc.ReferenceMaster.FontSize.PageTitle"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.PageTitle"),
            ["Tcc.ReferenceMaster.FontSize.PageSubtitle"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.PageSubtitle"),
            ["Tcc.ReferenceMaster.FontSize.SectionTitle"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.SectionTitle"),
            ["Tcc.ReferenceMaster.FontSize.PanelTitle"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.PanelTitle"),
            ["Tcc.ReferenceMaster.FontSize.PrimaryStatus"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.PrimaryStatus"),
            ["Tcc.ReferenceMaster.FontSize.SecondaryStatus"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.SecondaryStatus"),
            ["Tcc.ReferenceMaster.FontSize.Body"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.Body"),
            ["Tcc.ReferenceMaster.FontSize.TableHeader"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.TableHeader"),
            ["Tcc.ReferenceMaster.FontSize.TableCell"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.TableCell"),
            ["Tcc.ReferenceMaster.FontSize.Caption"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.Caption"),
            ["Tcc.ReferenceMaster.FontSize.Microcopy"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.Microcopy"),
            ["Tcc.ReferenceMaster.FontSize.Editorial"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.Editorial"),
            ["Tcc.ReferenceMaster.FontSize.DecorativeChinese"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.DecorativeChinese"),
            ["Tcc.ReferenceMaster.FontSize.UtilityGlyph"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.UtilityGlyph"),
            ["Tcc.ReferenceMaster.FontSize.MarketValue"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.MarketValue"),
            ["Tcc.ReferenceMaster.FontSize.StatusValue"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.StatusValue"),
            ["Tcc.ReferenceMaster.FontSize.RailFooter"] = (double)FindResource("Tcc.ReferenceMaster.FontSize.RailFooter"),
            ["Tcc.ReferenceMaster.LineHeight.AppBrand"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.AppBrand"),
            ["Tcc.ReferenceMaster.LineHeight.BrandMark"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.BrandMark"),
            ["Tcc.ReferenceMaster.LineHeight.WorkspaceSelector"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.WorkspaceSelector"),
            ["Tcc.ReferenceMaster.LineHeight.PageTitle"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.PageTitle"),
            ["Tcc.ReferenceMaster.LineHeight.PageSubtitle"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.PageSubtitle"),
            ["Tcc.ReferenceMaster.LineHeight.SectionTitle"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.SectionTitle"),
            ["Tcc.ReferenceMaster.LineHeight.PanelTitle"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.PanelTitle"),
            ["Tcc.ReferenceMaster.LineHeight.PrimaryStatus"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.PrimaryStatus"),
            ["Tcc.ReferenceMaster.LineHeight.SecondaryStatus"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.SecondaryStatus"),
            ["Tcc.ReferenceMaster.LineHeight.Body"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.Body"),
            ["Tcc.ReferenceMaster.LineHeight.TableHeader"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.TableHeader"),
            ["Tcc.ReferenceMaster.LineHeight.TableCell"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.TableCell"),
            ["Tcc.ReferenceMaster.LineHeight.Caption"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.Caption"),
            ["Tcc.ReferenceMaster.LineHeight.Microcopy"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.Microcopy"),
            ["Tcc.ReferenceMaster.LineHeight.Editorial"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.Editorial"),
            ["Tcc.ReferenceMaster.LineHeight.DecorativeChinese"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.DecorativeChinese"),
            ["Tcc.ReferenceMaster.LineHeight.UtilityGlyph"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.UtilityGlyph"),
            ["Tcc.ReferenceMaster.LineHeight.MarketValue"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.MarketValue"),
            ["Tcc.ReferenceMaster.LineHeight.StatusValue"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.StatusValue"),
            ["Tcc.ReferenceMaster.LineHeight.RailFooter"] = (double)FindResource("Tcc.ReferenceMaster.LineHeight.RailFooter"),
        };
        ApplyVisualAccessibilityOverrides();
        ApplyThemeResources();
        RefreshTextScale();
        ApplyVisualFixtureIfRequested();
        DataContext = viewModel;

        SourceInitialized += OnSourceInitialized;
        DpiChanged += OnDpiChanged;
        Closed += OnClosed;
        SystemParameters.StaticPropertyChanged += OnSystemParametersChanged;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        nint handle = new WindowInteropHelper(this).Handle;
        _windowSource = HwndSource.FromHwnd(handle);
        _windowSource?.AddHook(OnWindowMessage);
        EnsureMonitorVisibility();
    }

    private void OnDpiChanged(object sender, DpiChangedEventArgs e) => EnsureMonitorVisibility();

    private void OnSystemParametersChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.Equals(e.PropertyName, nameof(SystemParameters.HighContrast), StringComparison.Ordinal))
        {
            ApplyVisualAccessibilityOverrides();
            ApplyThemeResources();
        }
    }

    private void ApplyVisualAccessibilityOverrides()
    {
        ApplyStrataObservatoryPalette(SystemParameters.HighContrast);

        if (!SystemParameters.HighContrast)
        {
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Background.Base");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Surface.TopChrome");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Surface.Navigation");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Surface.HeaderScrim");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Surface.Panel");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Surface.PanelElevated");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Surface.PanelDeep");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Surface.PanelQuiet");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Surface.WorkspacePrimary");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Surface.WorkspaceSecondary");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Surface.Safety");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Surface.Auxiliary");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Surface.Action");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Border.Action");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Border.ActionInner");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Border.Default");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Border.Quiet");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Border.Elevated");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Border.Frost");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Border.Selected");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Border.Primary");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Border.Secondary");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Border.Safety");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Border.Auxiliary");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Text.Primary");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Text.Secondary");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Text.Tertiary");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Text.Disabled");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Text.OnAction");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Accent.Cyan");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Accent.Ice");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Identity.Seal");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Status.Warning");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Status.WarningFill");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Status.Permission");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Status.PermissionFill");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Status.Critical");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Status.CriticalFill");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Status.Information");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Status.InformationFill");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Status.Unavailable");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Status.Success");
            _visualResources.Remove("Tcc.ReferenceMaster.Brush.Status.Reserved");
            return;
        }

        SetVisualBrush("Tcc.ReferenceMaster.Brush.Background.Base", SystemColors.WindowColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Surface.TopChrome", SystemColors.ControlColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Surface.Navigation", SystemColors.ControlColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Surface.HeaderScrim", SystemColors.WindowColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Surface.Panel", SystemColors.ControlColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Surface.PanelElevated", SystemColors.ControlColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Surface.PanelDeep", SystemColors.WindowColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Surface.PanelQuiet", SystemColors.WindowColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Surface.WorkspacePrimary", SystemColors.ControlColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Surface.WorkspaceSecondary", SystemColors.WindowColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Surface.Safety", SystemColors.ControlColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Surface.Auxiliary", SystemColors.WindowColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Surface.Action", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Border.Action", SystemColors.HighlightTextColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Border.ActionInner", SystemColors.HighlightTextColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Border.Default", SystemColors.ActiveBorderColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Border.Quiet", SystemColors.ActiveBorderColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Border.Elevated", SystemColors.ActiveBorderColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Border.Frost", SystemColors.ActiveBorderColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Border.Selected", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Border.Primary", SystemColors.ActiveBorderColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Border.Secondary", SystemColors.ActiveBorderColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Border.Safety", SystemColors.ActiveBorderColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Border.Auxiliary", SystemColors.ActiveBorderColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Text.Primary", SystemColors.WindowTextColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Text.Secondary", SystemColors.ControlTextColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Text.Tertiary", SystemColors.GrayTextColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Text.Disabled", SystemColors.GrayTextColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Text.OnAction", SystemColors.HighlightTextColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Accent.Cyan", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Accent.Ice", SystemColors.HighlightTextColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Identity.Seal", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Status.Warning", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Status.WarningFill", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Status.Permission", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Status.PermissionFill", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Status.Critical", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Status.CriticalFill", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Status.Information", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Status.InformationFill", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Status.Unavailable", SystemColors.GrayTextColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Status.Success", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.ReferenceMaster.Brush.Status.Reserved", SystemColors.HighlightColor);
    }

    private void SetVisualBrush(string key, Color color) =>
        _visualResources[key] = new SolidColorBrush(color);

    private void ApplyStrataObservatoryPalette(bool highContrast)
    {
        if (!highContrast)
        {
            foreach (string key in StrataBrushOverrideKeys)
            {
                _visualResources.Remove(key);
            }

            _visualResources.Remove("Tcc.Strata.SceneOpacity");
            _visualResources.Remove("Tcc.Strata.ForegroundSceneryOpacity");
            _visualResources.Remove("Tcc.Strata.MentalStateGlyphOpacity");
            return;
        }

        SetVisualBrush("Tcc.Strata.Brush.Background", SystemColors.WindowColor);
        SetVisualBrush("Tcc.Strata.Brush.SceneVeil", Colors.Transparent);
        SetVisualBrush("Tcc.Strata.Brush.WorkspaceScrim", Colors.Transparent);
        SetVisualBrush("Tcc.Strata.Brush.EditorialScrim", Colors.Transparent);
        SetVisualBrush("Tcc.Strata.Brush.LowerPanelScrim", Colors.Transparent);
        SetVisualBrush("Tcc.Strata.Brush.Chrome", SystemColors.ControlColor);
        SetVisualBrush("Tcc.Strata.Brush.Nav", SystemColors.ControlColor);
        SetVisualBrush("Tcc.Strata.Brush.Panel", SystemColors.ControlColor);
        SetVisualBrush("Tcc.Strata.Brush.Panel.Secondary", SystemColors.WindowColor);
        SetVisualBrush("Tcc.Strata.Brush.Panel.Safety", SystemColors.ControlColor);
        SetVisualBrush("Tcc.Strata.Brush.HeaderField", SystemColors.WindowColor);
        SetVisualBrush("Tcc.Strata.Brush.ChartInset", SystemColors.WindowColor);
        SetVisualBrush("Tcc.Strata.Brush.PanelDeep", SystemColors.ControlColor);
        SetVisualBrush("Tcc.Strata.Brush.PanelQuiet", SystemColors.WindowColor);
        SetVisualBrush("Tcc.Strata.Brush.SearchSurface", SystemColors.WindowColor);
        SetVisualBrush("Tcc.Strata.Brush.SelectedSurface", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.Strata.Brush.PressedSurface", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.Strata.Brush.TabSurface", SystemColors.ControlColor);
        SetVisualBrush("Tcc.Strata.Brush.TabSelectedSurface", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.Strata.Brush.FooterBand", SystemColors.WindowColor);
        SetVisualBrush("Tcc.Strata.Brush.EdgeGlint", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.Strata.Brush.ChartGrid", SystemColors.ActiveBorderColor);
        SetVisualBrush("Tcc.Strata.Brush.ChartAxis", SystemColors.WindowTextColor);
        SetVisualBrush("Tcc.Strata.Brush.RowSeparator", SystemColors.ActiveBorderColor);
        SetVisualBrush("Tcc.Strata.Brush.Border", SystemColors.ActiveBorderColor);
        SetVisualBrush("Tcc.Strata.Brush.BorderQuiet", SystemColors.ActiveBorderColor);
        SetVisualBrush("Tcc.Strata.Brush.BorderBright", SystemColors.WindowTextColor);
        SetVisualBrush("Tcc.Strata.Brush.Text", SystemColors.WindowTextColor);
        SetVisualBrush("Tcc.Strata.Brush.TextSecondary", SystemColors.ControlTextColor);
        SetVisualBrush("Tcc.Strata.Brush.Muted", SystemColors.GrayTextColor);
        SetVisualBrush("Tcc.Strata.Brush.Ice", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.Strata.Brush.Gold", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.Strata.Brush.Seal", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.Strata.Brush.Danger", SystemColors.HighlightColor);
        SetVisualBrush("Tcc.Strata.Brush.Success", SystemColors.HighlightColor);
        _visualResources["Tcc.Strata.SceneOpacity"] = 0d;
        _visualResources["Tcc.Strata.ForegroundSceneryOpacity"] = 0d;
        _visualResources["Tcc.Strata.MentalStateGlyphOpacity"] = 0d;
    }

    private void ApplyThemeResources()
    {
        ResourceDictionary resources = SafeThemeResourceAdapter.Create(_viewModel.Presentation);
        resources.MergedDictionaries.Add(_visualResources);
        Resources = resources;
    }

    private void RefreshTextScale()
    {
        using RegistryKey? accessibility = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Accessibility");
        object? setting = accessibility?.GetValue("TextScaleFactor");
        double scale = setting is int percentage && percentage > 0
            ? percentage / 100d
            : 1d;
        if (_textScale == scale)
        {
            return;
        }

        _textScale = scale;
        foreach (KeyValuePair<string, double> entry in _baseTypographyMetrics)
        {
            _visualResources[entry.Key] = entry.Value * scale;
        }
        _visualResources["Tcc.ReferenceMaster.Viewport.MinWidth"] = 1672d;
        _visualResources["Tcc.ReferenceMaster.Viewport.MinHeight"] = 941d;
    }

    private void ApplyVisualFixtureIfRequested()
    {
        string? fixture = Environment.GetEnvironmentVariable(VisualFixtureEnvironmentVariable);
        if (TryApplyPlanFixture(fixture))
        {
            return;
        }

        if (!string.Equals(fixture, MasterVisualFixture, StringComparison.Ordinal))
        {
            return;
        }

        AutomationProperties.SetName(MainContentScroller, "Strata Observatory HOME, visual fixture MASTER_R1");
        AutomationProperties.SetName(SafetyRegionLabel, "Home safety core, visual fixture MASTER_R1");

        SearchPlaceholder.Text = "Search symbols, commands...";
        SearchKeyHint.Text = "Ctrl K";

        Brush success = (Brush)FindResource("Tcc.Strata.Brush.Success");
        TradingPermissionMarker.Stroke = success;
        TradingPermissionValue.Text = "ENABLED";
        TradingPermissionValue.Foreground = success;
        TradingPermissionHelper.Text = "All systems nominal";

        ApplyFixtureSafetyValue(TotalRiskUnsetMarker, TotalRiskValue, TotalRiskHelper, "0.8%", "of account equity");
        ApplyFixtureSafetyValue(PositionsUnsetMarker, PositionsValue, PositionsHelper, "0", "No open positions");
        ApplyFixtureSafetyValue(AlertsUnsetMarker, AlertsValue, AlertsHelper, "0", "No active alerts");

        OfflineMarketBody.Visibility = Visibility.Collapsed;
        OfflineKpiBand.Visibility = Visibility.Collapsed;
        FixtureMarketLayer.Visibility = Visibility.Visible;

        ProductionWatchlistRows.Visibility = Visibility.Collapsed;
        ProductionWatchlistFooter.Visibility = Visibility.Collapsed;
        FixtureWatchlistRows.Visibility = Visibility.Visible;

        PriorityFirstRow.BorderBrush = (Brush)FindResource("Tcc.Strata.Brush.Gold");
        PriorityFirstRow.BorderThickness = new Thickness(2, 0, 1, 1);
        PriorityText1.Text = "Define market bias";
        PriorityText2.Text = "Check key levels";
        PriorityText3.Text = "Review allocation";
        PriorityText4.Text = "Wait for quality";

        AutomationProperties.SetName(MentalStateRegionLabel, "Mental state, calm, focused and disciplined");
        MentalStateValue.Text = "CALM";
        MentalStateHelper.Text = "FOCUSED\nDISCIPLINED";
        MentalStateGlyph.SetValue(
            System.Windows.Controls.Image.SourceProperty,
            (ImageSource)FindResource("Tcc.Strata.MentalStateGlyph.Calm"));
        ProductionMentalRows.Visibility = Visibility.Collapsed;
        FixtureMentalRows.Visibility = Visibility.Visible;

        AutomationProperties.SetName(RecentActivityRegionLabel, "Recent activity fixture, five reference events with timestamps");
        AutomationProperties.SetHelpText(RecentActivityRegionLabel, "Reference fixture activity; filtering remains unavailable.");
        RecentActivityMode.Text = "ALL ACTIVITY";
        RecentTimelineFixtureExtension.Visibility = Visibility.Visible;
        ApplyRecentFixtureRow(RecentTime1, RecentText1, "08:42", "Workspace opened");
        ApplyRecentFixtureRow(RecentTime2, RecentText2, "08:41", "Market data connection lost");
        ApplyRecentFixtureRow(RecentTime3, RecentText3, "08:28", "Watched BTCUSDT");
        ApplyRecentFixtureRow(RecentTime4, RecentText4, "08:16", "Reviewed plan");
        RecentRow5.Visibility = Visibility.Visible;
        ApplyRecentFixtureRow(RecentTime5, RecentText5, "08:03", "Checked risk parameters");
    }

    private bool TryApplyPlanFixture(string? fixture)
    {
        PlanDashboardPreviewState? state = fixture switch
        {
            PlanVisualFixture => PlanDashboardPreviewState.Offline,
            "PLAN_R1_EMPTY" => PlanDashboardPreviewState.Empty,
            "PLAN_R1_LOADING" => PlanDashboardPreviewState.Loading,
            "PLAN_R1_ERROR" => PlanDashboardPreviewState.Error,
            "PLAN_R1_BLOCKED" => PlanDashboardPreviewState.Blocked,
            _ => null,
        };
        if (state is null)
        {
            return false;
        }

        ApplyShellPage(showPlan: true, announce: false);
        PlanDashboardSurface.ShowPreviewState(state.Value);

        AutomationProperties.SetName(MainContentScroller, "交易計畫儀表板，視覺測試 " + fixture);
        AutomationProperties.SetHelpText(MainContentScroller, "唯讀計畫展示。可使用左側 HOME 與 PLAN 導覽；文字放大時請使用計畫內容捲動區。");
        return true;
    }

    private void OnHomeNavigationClick(object sender, RoutedEventArgs e)
    {
        if (HomePageLayer.Visibility == Visibility.Visible)
        {
            return;
        }

        ApplyShellPage(showPlan: false, announce: true);
    }

    private void OnPlanNavigationClick(object sender, RoutedEventArgs e)
    {
        if (PlanDashboardSurface.Visibility == Visibility.Visible)
        {
            return;
        }

        ApplyShellPage(showPlan: true, announce: true);
        PlanDashboardSurface.ShowPreviewState(PlanDashboardPreviewState.Offline);
    }

    private void ApplyShellPage(bool showPlan, bool announce)
    {
        HomePageLayer.Visibility = showPlan ? Visibility.Collapsed : Visibility.Visible;
        PlanDashboardSurface.Visibility = showPlan ? Visibility.Visible : Visibility.Collapsed;

        NavHome.Style = (Style)FindResource(showPlan ? "Strata.NavButton" : "Strata.NavButton.Selected");
        NavHomeCurrentState.Visibility = showPlan ? Visibility.Collapsed : Visibility.Visible;
        AutomationProperties.SetName(NavHome, showPlan ? "Open Home" : "Home selected, current page");
        AutomationProperties.SetItemStatus(NavHome, showPlan ? "Available" : "Current page");
        AutomationProperties.SetHelpText(NavHome, showPlan ? "Navigates to the Home page." : "Current page.");
        NavHome.ToolTip = showPlan ? "Open Home" : "Current page";

        NavPlanning.Style = (Style)FindResource(showPlan ? "Strata.NavButton.Selected" : "Strata.NavButton");
        NavPlanningCurrentState.Visibility = showPlan ? Visibility.Visible : Visibility.Collapsed;
        AutomationProperties.SetName(NavPlanning, showPlan ? "Plan selected, current page" : "Open Plan");
        AutomationProperties.SetItemStatus(NavPlanning, showPlan ? "Current page" : "Available");
        AutomationProperties.SetHelpText(NavPlanning, showPlan ? "Current page." : "Navigates to the read-only Plan page.");
        NavPlanning.ToolTip = showPlan ? "Current page" : "Open Plan";

        AutomationProperties.SetName(MainContentScroller, showPlan ? "交易計畫儀表板" : "Strata Observatory HOME");
        AutomationProperties.SetHelpText(
            MainContentScroller,
            showPlan
                ? "唯讀交易計畫儀表板。文字放大或視窗受限時，請使用計畫內容或外層頁面捲動區。"
                : "At constrained window sizes or enlarged text, use the scroll bars or arrow keys to explore the full HOME composition.");
        _baseTitle = showPlan ? "TCC — 交易計畫儀表板" : "TCC — Strata Observatory";
        Title = _baseTitle;

        if (announce)
        {
            UIElementAutomationPeer.FromElement(MainContentScroller)?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
        }
    }


    private void ApplyFixtureSafetyValue(
        FrameworkElement marker,
        System.Windows.Controls.TextBlock value,
        System.Windows.Controls.TextBlock helper,
        string displayValue,
        string helperText)
    {
        marker.Visibility = Visibility.Collapsed;
        value.Margin = new Thickness(0, -3, 0, 0);
        value.Style = (Style)FindResource("Strata.StatusValue");
        value.Text = displayValue;
        helper.Text = helperText;
    }

    private static void ApplyRecentFixtureRow(
        System.Windows.Controls.TextBlock time,
        System.Windows.Controls.TextBlock text,
        string displayTime,
        string displayText)
    {
        time.Text = displayTime;
        text.Text = displayText;
    }

    private nint OnWindowMessage(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
    {
        if (message == DisplayChangeMessage)
        {
            EnsureMonitorVisibility();
        }
        else if (message == SettingChangeMessage)
        {
            Dispatcher.BeginInvoke(new Action(RefreshTextScale));
        }

        return 0;
    }

    private void OnMinimizeWindowClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void OnMaximizeRestoreWindowClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void OnCloseWindowClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void EnsureMonitorVisibility()
    {
        if (_isEnsuringVisible)
        {
            return;
        }

        _isEnsuringVisible = true;
        try
        {
            nint handle = new WindowInteropHelper(this).Handle;
            bool visible = _monitorAdapter.EnsureVisible(handle);
            WindowMonitorFacts? facts = visible ? _monitorAdapter.Capture(handle) : null;
            Title = visible && facts is not null
                ? _baseTitle
                : _baseTitle + MonitorUnavailableSuffix;
        }
        finally
        {
            _isEnsuringVisible = false;
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        SystemParameters.StaticPropertyChanged -= OnSystemParametersChanged;
        if (_windowSource is not null)
        {
            _windowSource.RemoveHook(OnWindowMessage);
            _windowSource = null;
        }
    }
}
