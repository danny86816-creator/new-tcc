from __future__ import annotations

import json
import subprocess
import tempfile
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
WORK = ROOT / "automation" / "tcc_master_pipeline" / "work" / "m1_4_6_b3_1d"


PROGRAM = r'''
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Tcc.DesktopHost;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Fallback;
using Tcc.Windows.Monitors;

internal static class Program
{
    private const double DesignWidth = 2142;
    private const double DesignHeight = 1196;

    private static FrameworkElement Find(DependencyObject root, string automationId)
    {
        if (root is FrameworkElement element && AutomationProperties.GetAutomationId(element) == automationId)
        {
            return element;
        }

        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            try
            {
                return Find(VisualTreeHelper.GetChild(root, index), automationId);
            }
            catch (KeyNotFoundException)
            {
            }
        }

        throw new KeyNotFoundException(automationId);
    }

    private static bool HasAncestor(DependencyObject? candidate, DependencyObject expected)
    {
        for (DependencyObject? current = candidate; current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (ReferenceEquals(current, expected))
            {
                return true;
            }
        }

        return false;
    }

    private static (double X, double Y) DeclaredPosition(FrameworkElement element, FrameworkElement viewport)
    {
        double x = 0;
        double y = 0;
        for (DependencyObject? current = element; current is not null && !ReferenceEquals(current, viewport); current = VisualTreeHelper.GetParent(current))
        {
            if (current is FrameworkElement item)
            {
                double left = Canvas.GetLeft(item);
                double top = Canvas.GetTop(item);
                if (!double.IsNaN(left)) x += left;
                if (!double.IsNaN(top)) y += top;
            }
        }

        return (x, y);
    }

    private static void Save(FrameworkElement viewport, string path, double scale)
    {
        int width = (int)Math.Round(DesignWidth * scale);
        int height = (int)Math.Round(DesignHeight * scale);
        RenderTargetBitmap bitmap = new(width, height, 96 * scale, 96 * scale, PixelFormats.Pbgra32);
        bitmap.Render(viewport);
        PngBitmapEncoder encoder = new();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using FileStream output = File.Create(path);
        encoder.Save(output);
    }

    [STAThread]
    private static int Main(string[] args)
    {
        string output = args[0];
        Application application = new() { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        BuiltInThemePresentationSnapshot presentation = new BuiltInThemePresentationSource()
            .GetPresentation(new ThemeVariantId("deep"));
        MainWindow window = new(new MainWindowViewModel(presentation, "B3.1 runtime validation"), new WindowMonitorAdapter())
        {
            Width = 2142,
            Height = 1196,
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false,
            Opacity = 0.01,
        };
        window.Show();
        window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

        FrameworkElement viewport = Find(window, "Region.Viewport");
        viewport.UpdateLayout();

        Dictionary<string, string> geometryIds = new(StringComparer.Ordinal)
        {
            ["Viewport"] = "Region.Viewport",
            ["Safety Core"] = "Region.SafetyCore",
            ["Main Workspace"] = "Region.MainWorkspace",
            ["Global Top Bar"] = "Region.GlobalTopBar",
            ["Left Navigation Rail"] = "Region.LeftNavigationRail",
            ["Command Center Header"] = "Region.CommandCenterHeader",
            ["Workspace Selector"] = "Region.WorkspaceSelector",
            ["New Trade Plan"] = "Region.NewTradePlan",
            ["Review Checklist"] = "Region.ReviewChecklist",
            ["Trading Permission"] = "Region.TradingPermission",
            ["Total Risk"] = "Region.TotalRisk",
            ["Current Positions"] = "Region.CurrentPositions",
            ["Major Alerts"] = "Region.MajorAlerts",
            ["Market Overview"] = "Region.MarketOverview",
            ["Mental State"] = "Region.MentalState",
            ["Today's Priorities"] = "Region.TodaysPriorities",
            ["Risk Allocation"] = "Region.RiskAllocation",
            ["Open Positions"] = "Region.OpenPositions",
            ["Activity"] = "Region.Activity",
            ["Character Zone"] = "Region.CharacterZone",
            ["Application Chrome"] = "Region.ApplicationChrome",
        };
        List<object> geometry = new();
        foreach ((string name, string id) in geometryIds)
        {
            FrameworkElement element = Find(viewport, id);
            (double x, double y) = DeclaredPosition(element, viewport);
            geometry.Add(new { component_id = name, automation_id = id, x, y, width = element.Width, height = element.Height });
        }

        string[] hitIds =
        [
            "Region.NewTradePlan", "Region.ReviewChecklist", "Region.GlobalTopBar", "Region.Search", "NavHome",
            "Region.TradingPermission", "Region.TotalRisk", "Region.CurrentPositions", "Region.MajorAlerts",
            "Region.MarketOverview", "Region.MentalState", "Region.TodaysPriorities", "Region.OpenPositions", "Region.Activity",
        ];
        FrameworkElement characterLayer = Find(viewport, "Artwork.Home.B3.CharacterEdge");
        FrameworkElement plumLayer = Find(viewport, "Artwork.Home.B3.PlumForeground");
        FrameworkElement snowLayer = Find(viewport, "Artwork.Home.B3.NearestStaticSnow");
        List<object> hits = new();
        foreach (string id in hitIds)
        {
            FrameworkElement target = Find(viewport, id);
            Point center = target.TranslatePoint(new Point(target.ActualWidth / 2, target.ActualHeight / 2), viewport);
            DependencyObject? result = viewport.InputHitTest(center) as DependencyObject;
            bool passed = HasAncestor(result, target)
                && !HasAncestor(result, characterLayer)
                && !HasAncestor(result, plumLayer)
                && !HasAncestor(result, snowLayer);
            hits.Add(new { automation_id = id, passed, point = new[] { center.X, center.Y } });
        }

        Save(viewport, Path.Combine(output, "B3_1D_PRODUCTION_ROLLBACK_RUNTIME.png"), 1.0);
        Save(viewport, Path.Combine(output, "B3_1D_PRODUCTION_100_DPI.png"), 1.0);
        Save(viewport, Path.Combine(output, "B3_1D_PRODUCTION_125_DPI.png"), 1.25);
        Save(viewport, Path.Combine(output, "B3_1D_PRODUCTION_150_DPI.png"), 1.5);

        object report = new
        {
            runtime = "compiled production MainWindow WPF visual tree",
            product_assembly = typeof(MainWindow).Assembly.Location,
            viewport = new { width = viewport.ActualWidth, height = viewport.ActualHeight },
            geometry,
            art_layers = new[]
            {
                new { id = "Artwork.Home.B3.CharacterEdge", is_hit_test_visible = characterLayer.IsHitTestVisible, z_index = Panel.GetZIndex(characterLayer) },
                new { id = "Artwork.Home.B3.PlumForeground", is_hit_test_visible = plumLayer.IsHitTestVisible, z_index = Panel.GetZIndex(plumLayer) },
                new { id = "Artwork.Home.B3.NearestStaticSnow", is_hit_test_visible = snowLayer.IsHitTestVisible, z_index = Panel.GetZIndex(snowLayer) },
            },
            hit_test = hits,
        };
        File.WriteAllText(Path.Combine(output, "b3_1d_production_runtime.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        bool passedAll = hits.All(item => (bool)item.GetType().GetProperty("passed")!.GetValue(item)!)
            && !characterLayer.IsHitTestVisible && !plumLayer.IsHitTestVisible && !snowLayer.IsHitTestVisible;
        window.Close();
        application.Shutdown();
        return passedAll ? 0 : 2;
    }
}
'''


def main() -> int:
    WORK.mkdir(parents=True, exist_ok=True)
    project = ROOT / "src" / "Tcc.DesktopHost" / "Tcc.DesktopHost.csproj"
    with tempfile.TemporaryDirectory(prefix="tcc_b3_1d_runtime_") as temp:
        temp_path = Path(temp)
        project_ref = str(project).replace("&", "&amp;")
        (temp_path / "Harness.csproj").write_text(
            '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType>'
            '<TargetFramework>net10.0-windows</TargetFramework><UseWPF>true</UseWPF>'
            '<EnableDefaultApplicationDefinition>false</EnableDefaultApplicationDefinition>'
            '<ImplicitUsings>disable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup>'
            f'<ItemGroup><ProjectReference Include="{project_ref}" /></ItemGroup></Project>',
            encoding="utf-8",
        )
        (temp_path / "Program.cs").write_text(PROGRAM, encoding="utf-8")
        process = subprocess.run(
            ["dotnet", "run", "--project", str(temp_path / "Harness.csproj"), "-c", "Release", "--", str(WORK)],
            cwd=temp_path,
            text=True,
            capture_output=True,
            timeout=240,
        )
        (WORK / "b3_1d_production_runtime_console.txt").write_text(process.stdout + "\n" + process.stderr, encoding="utf-8")
        if process.returncode != 0:
            raise SystemExit(f"Production WPF runtime validation failed ({process.returncode}): {process.stderr[-3000:]}")
    report = json.loads((WORK / "b3_1d_production_runtime.json").read_text(encoding="utf-8"))
    print(json.dumps({
        "runtime": report["runtime"],
        "geometry_count": len(report["geometry"]),
        "hit_test": f'{sum(1 for item in report["hit_test"] if item["passed"])}/{len(report["hit_test"])}',
        "art_layers": report["art_layers"],
    }, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
