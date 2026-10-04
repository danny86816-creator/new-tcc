using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace Tcc.DesktopHost;

internal enum RiskPermissionPreviewState
{
    Empty,
    Loading,
    Offline,
    Error,
    Blocked,
}

public sealed partial class RiskPermissionView : UserControl
{
    public RiskPermissionView()
    {
        InitializeComponent();
    }

    internal void ShowPreviewState(RiskPermissionPreviewState state)
    {
        EmptyState.Visibility = Visibility.Collapsed;
        LoadingState.Visibility = Visibility.Collapsed;
        OfflineState.Visibility = Visibility.Collapsed;
        ErrorState.Visibility = Visibility.Collapsed;
        BlockedState.Visibility = Visibility.Collapsed;

        string announcement = state switch
        {
            RiskPermissionPreviewState.Empty => Show(EmptyState, "空白。尚無交易權限評估；系統不建立替代判定。"),
            RiskPermissionPreviewState.Loading => Show(LoadingState, "正在載入交易權限證據；完成前不顯示判定。"),
            RiskPermissionPreviewState.Offline => Show(OfflineState, "離線。交易權限證據無法使用，系統不做推測。"),
            RiskPermissionPreviewState.Error => Show(ErrorState, "錯誤。無法完成交易權限評估；資料未被變更。"),
            RiskPermissionPreviewState.Blocked => Show(BlockedState, "封鎖。不可交易；正式判定必須揭露原因、限制與解除路徑。"),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "不支援的交易權限預覽狀態。"),
        };

        AutomationProperties.SetName(RiskStateRegion, announcement);
        UIElementAutomationPeer.FromElement(RiskStateRegion)?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
    }

    private static string Show(FrameworkElement element, string announcement)
    {
        element.Visibility = Visibility.Visible;
        return announcement;
    }
}
