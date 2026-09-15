using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using Tcc.DesktopHost.EmbeddedSafeTheme;
using Tcc.Windows.Monitors;

namespace Tcc.DesktopHost;

public partial class MainWindow : Window
{
    private const int DisplayChangeMessage = 0x007E;
    private const string MonitorUnavailableSuffix = " — 顯示器／DPI 資訊暫不可用";
    private readonly MainWindowViewModel _viewModel;
    private readonly WindowMonitorAdapter _monitorAdapter;
    private string _baseTitle = string.Empty;
    private HwndSource? _windowSource;
    private bool _isEnsuringVisible;

    public MainWindow(
        MainWindowViewModel viewModel,
        WindowMonitorAdapter monitorAdapter)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _monitorAdapter = monitorAdapter ?? throw new ArgumentNullException(nameof(monitorAdapter));
        InitializeComponent();
        _baseTitle = Title;
        Resources = SafeThemeResourceAdapter.Create(viewModel.Presentation);
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
            Resources = SafeThemeResourceAdapter.Create(_viewModel.Presentation);
        }
    }

    private nint OnWindowMessage(nint hwnd, int message, nint wParam, nint lParam, ref bool handled)
    {
        if (message == DisplayChangeMessage)
        {
            EnsureMonitorVisibility();
        }

        return 0;
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
