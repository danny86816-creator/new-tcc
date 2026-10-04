using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace Tcc.DesktopHost;

internal enum PlanDashboardPreviewState
{
    Empty,
    Loading,
    Offline,
    Error,
    Blocked,
}

public sealed partial class PlanDashboardView : UserControl
{
    public PlanDashboardView()
    {
        InitializeComponent();
    }

    internal void ShowPreviewState(PlanDashboardPreviewState state)
    {
        EmptyState.Visibility = Visibility.Collapsed;
        LoadingState.Visibility = Visibility.Collapsed;
        OfflineState.Visibility = Visibility.Collapsed;
        ErrorState.Visibility = Visibility.Collapsed;
        BlockedState.Visibility = Visibility.Collapsed;

        string announcement = state switch
        {
            PlanDashboardPreviewState.Empty => Show(EmptyState, "空白。尚無草稿計畫。"),
            PlanDashboardPreviewState.Loading => Show(LoadingState, "正在載入計畫索引。草稿與正式紀錄維持區隔。"),
            PlanDashboardPreviewState.Offline => Show(OfflineState, "離線。計畫資料無法使用，系統不推測任何計畫。"),
            PlanDashboardPreviewState.Error => Show(ErrorState, "錯誤。無法讀取計畫索引；資料未被變更。"),
            PlanDashboardPreviewState.Blocked => Show(BlockedState, "已阻擋。計畫存取已被阻擋，且不提供略過方式。"),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "不支援的計畫儀表板預覽狀態。"),
        };

        AutomationProperties.SetName(PlanStateRegion, announcement);
        UIElementAutomationPeer.FromElement(PlanStateRegion)?.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
    }

    private static string Show(FrameworkElement element, string announcement)
    {
        element.Visibility = Visibility.Visible;
        return announcement;
    }
}
