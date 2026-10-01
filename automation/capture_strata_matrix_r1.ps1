param(
    [Parameter(Mandatory)][int]$Width,
    [Parameter(Mandatory)][int]$Height,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [Parameter(Mandatory)][string]$ScreenshotName,
    [string]$Phase = "TCC_STRATA_UI_ART_MATRIX_R1",
    [string]$VisualFixture = "",
    [switch]$ScrollToEnd
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$outputDir = Join-Path $root $OutputDirectory
$screenshot = Join-Path $outputDir $ScreenshotName
$evidence = Join-Path $outputDir "runtime-evidence.json"
$exe = Join-Path $root "src/Tcc.DesktopHost/bin/x64/Release/net10.0-windows/Tcc.DesktopHost.exe"
if (-not (Test-Path -LiteralPath $exe)) { throw "Release x64 DesktopHost executable not found." }

New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public static class StrataMatrixNative
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(IntPtr hwnd, IntPtr insertAfter, int x, int y, int width, int height, uint flags);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
    [DllImport("user32.dll")]
    public static extern uint GetDpiForWindow(IntPtr hwnd);
}
"@
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName PresentationFramework

function Capture-Window {
    param([IntPtr]$Hwnd, [string]$Path, $UiaBounds)
    $bitmapWidth = [int][Math]::Round($UiaBounds.Width)
    $bitmapHeight = [int][Math]::Round($UiaBounds.Height)
    if ($bitmapWidth -le 0 -or $bitmapHeight -le 0) { throw "UIA root returned invalid window bounds." }
    $bitmap = [System.Drawing.Bitmap]::new($bitmapWidth, $bitmapHeight, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $hdc = $graphics.GetHdc()
    try {
        if (-not [StrataMatrixNative]::PrintWindow($Hwnd, $hdc, 2)) { throw "PrintWindow failed." }
    }
    finally {
        $graphics.ReleaseHdc($hdc)
        $graphics.Dispose()
    }
    try { $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png) }
    finally { $bitmap.Dispose() }
    return [ordered]@{
        x = [int][Math]::Round($UiaBounds.X)
        y = [int][Math]::Round($UiaBounds.Y)
        width = $bitmapWidth
        height = $bitmapHeight
    }
}

$expectedIds = @(
    "MainContentScroller",
    "Region.HeaderIdentity", "Editorial.RightMaxim",
    "Region.HomeSafetyCore", "Region.MarketOverview", "Region.Watchlist",
    "Region.Priorities", "Region.MentalState", "Region.RecentActivity",
    "WindowMinimizeButton", "WindowMaximizeRestoreButton", "WindowCloseButton",
    "NavHome", "NavMarkets", "NavPlanning", "NavRisk", "NavPositions", "NavReview", "NavSettings",
    "TabBtcUsdt", "TabEthUsdt", "TabSolUsdt", "TabBnbUsdt", "TabXrpUsdt", "TabAddSymbol",
    "Timeframe1H", "Timeframe4H", "Timeframe1D", "Timeframe1W"
)

$app = $null
try {
    if ([string]::IsNullOrWhiteSpace($VisualFixture)) {
        [Environment]::SetEnvironmentVariable("TCC_VISUAL_FIXTURE", $null, "Process")
    }
    else {
        [Environment]::SetEnvironmentVariable("TCC_VISUAL_FIXTURE", $VisualFixture, "Process")
    }
    $app = Start-Process -FilePath $exe -WorkingDirectory (Split-Path $exe) -PassThru
    for ($attempt = 0; $attempt -lt 100 -and $app.MainWindowHandle -eq 0; $attempt++) {
        Start-Sleep -Milliseconds 100
        $app.Refresh()
    }
    if ($app.MainWindowHandle -eq 0) { throw "DesktopHost did not create a main HWND." }
    $hwnd = [IntPtr]$app.MainWindowHandle
    if (-not [StrataMatrixNative]::SetWindowPos($hwnd, [IntPtr]::Zero, 48, 48, $Width, $Height, 0x0014)) {
        throw "SetWindowPos failed."
    }
    Start-Sleep -Milliseconds 500
    $app.Refresh()
    if ($app.HasExited) { throw "DesktopHost exited before capture." }

    $uiaRoot = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
    $uiaWindowBounds = $uiaRoot.Current.BoundingRectangle
    if ([Math]::Abs($uiaWindowBounds.Width - $Width) -gt 1 -or [Math]::Abs($uiaWindowBounds.Height - $Height) -gt 1) {
        $correctedWidth = [int][Math]::Round($Width * $Width / $uiaWindowBounds.Width)
        $correctedHeight = [int][Math]::Round($Height * $Height / $uiaWindowBounds.Height)
        if (-not [StrataMatrixNative]::SetWindowPos($hwnd, [IntPtr]::Zero, 48, 48, $correctedWidth, $correctedHeight, 0x0014)) {
            throw "DPI-corrected SetWindowPos failed."
        }
        Start-Sleep -Milliseconds 750
        $app.Refresh()
        if ($app.HasExited) { throw "DesktopHost exited during DPI-corrected sizing." }
        $uiaRoot = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
        $uiaWindowBounds = $uiaRoot.Current.BoundingRectangle
    }
    if ([Math]::Abs($uiaWindowBounds.Width - $Width) -gt 2 -or [Math]::Abs($uiaWindowBounds.Height - $Height) -gt 2) {
        throw "Window UIA bounds $($uiaWindowBounds.Width)x$($uiaWindowBounds.Height) do not match requested evidence size ${Width}x${Height}."
    }
    Start-Sleep -Milliseconds 500

    $uia = foreach ($id in $expectedIds) {
        $condition = [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::AutomationIdProperty, $id)
        $element = $uiaRoot.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
        $bounds = if ($null -ne $element) { $element.Current.BoundingRectangle } else { $null }
        [ordered]@{
            automation_id = $id
            status = if ($null -ne $element) { "FOUND" } else { "MISSING" }
            name = if ($null -ne $element) { $element.Current.Name } else { $null }
            help_text = if ($null -ne $element) { $element.Current.HelpText } else { $null }
            control_type = if ($null -ne $element) { $element.Current.LocalizedControlType } else { $null }
            enabled = if ($null -ne $element) { $element.Current.IsEnabled } else { $null }
            keyboard_focusable = if ($null -ne $element) { $element.Current.IsKeyboardFocusable } else { $null }
            has_keyboard_focus = if ($null -ne $element) { $element.Current.HasKeyboardFocus } else { $null }
            offscreen = if ($null -ne $element) { $element.Current.IsOffscreen } else { $null }
            bounds = if ($null -ne $bounds) {
                [ordered]@{ x = $bounds.X; y = $bounds.Y; width = $bounds.Width; height = $bounds.Height }
            }
            else { $null }
        }
    }

    $scrollerCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::AutomationIdProperty, "MainContentScroller")
    $scrollerElement = $uiaRoot.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $scrollerCondition)
    $scrollPatternObject = $null
    $hasScrollPattern = $null -ne $scrollerElement -and $scrollerElement.TryGetCurrentPattern(
        [System.Windows.Automation.ScrollPattern]::Pattern, [ref]$scrollPatternObject)
    $scrollPattern = if ($hasScrollPattern) { [System.Windows.Automation.ScrollPattern]$scrollPatternObject } else { $null }
    if ($ScrollToEnd) {
        if (-not $hasScrollPattern -or -not $scrollPattern.Current.HorizontallyScrollable -or -not $scrollPattern.Current.VerticallyScrollable) {
            throw "The requested end-position evidence requires a two-axis ScrollPattern."
        }
        $scrollPattern.SetScrollPercent(100.0, 100.0)
        Start-Sleep -Milliseconds 300
    }
    $scrollEvidence = [ordered]@{
        available = $hasScrollPattern
        horizontally_scrollable = if ($hasScrollPattern) { $scrollPattern.Current.HorizontallyScrollable } else { $null }
        vertically_scrollable = if ($hasScrollPattern) { $scrollPattern.Current.VerticallyScrollable } else { $null }
        horizontal_scroll_percent = if ($hasScrollPattern) { $scrollPattern.Current.HorizontalScrollPercent } else { $null }
        vertical_scroll_percent = if ($hasScrollPattern) { $scrollPattern.Current.VerticalScrollPercent } else { $null }
        horizontal_view_size = if ($hasScrollPattern) { $scrollPattern.Current.HorizontalViewSize } else { $null }
        vertical_view_size = if ($hasScrollPattern) { $scrollPattern.Current.VerticalViewSize } else { $null }
    }

    $buttonCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::Button)
    $buttonElements = $uiaRoot.FindAll([System.Windows.Automation.TreeScope]::Descendants, $buttonCondition)
    $unnamedButtons = @($buttonElements | Where-Object { [string]::IsNullOrWhiteSpace($_.Current.Name) })
    $focusableUnnamedButtons = @($unnamedButtons | Where-Object { $_.Current.IsKeyboardFocusable })
    $applicationButtons = @($buttonElements | Where-Object { $expectedIds -contains $_.Current.AutomationId })
    $unnamedApplicationButtons = @($applicationButtons | Where-Object { [string]::IsNullOrWhiteSpace($_.Current.Name) })
    $bounds = Capture-Window -Hwnd $hwnd -Path $screenshot -UiaBounds $uiaWindowBounds
    $textScale = (Get-ItemProperty -Path "HKCU:\Software\Microsoft\Accessibility" -Name TextScaleFactor).TextScaleFactor
    $highContrastActive = [System.Windows.SystemParameters]::HighContrast

    $data = [ordered]@{
        phase = $Phase
        visual_fixture = if ([string]::IsNullOrWhiteSpace($VisualFixture)) { "NONE" } else { $VisualFixture }
        scroll_position = if ($ScrollToEnd) { "END" } else { "START" }
        captured_at = (Get-Date).ToString("o")
        capture_method = "HWND_BOUND_PRINTWINDOW_PW_RENDERFULLCONTENT"
        capture_coordinate_source = "UIA_ROOT_BOUNDING_RECTANGLE"
        executable = $exe.Substring($root.Length + 1).Replace('\', '/')
        process_id = $app.Id
        hwnd = $hwnd.ToInt64()
        window = $bounds
        dpi = [StrataMatrixNative]::GetDpiForWindow($hwnd)
        text_scale_percent = $textScale
        high_contrast = $highContrastActive
        screenshot = $screenshot.Substring($root.Length + 1).Replace('\', '/')
        screenshot_sha256 = (Get-FileHash -LiteralPath $screenshot -Algorithm SHA256).Hash
        expected_uia = $expectedIds.Count
        found_uia = @($uia | Where-Object status -eq "FOUND").Count
        offscreen_uia = @($uia | Where-Object offscreen -eq $true).Count
        button_count = $buttonElements.Count
        unnamed_button_count = $unnamedButtons.Count
        focusable_unnamed_button_count = $focusableUnnamedButtons.Count
        application_button_count = $applicationButtons.Count
        unnamed_application_button_count = $unnamedApplicationButtons.Count
        framework_scroll_part_button_count = $buttonElements.Count - $applicationButtons.Count
        process_alive_at_capture = -not $app.HasExited
        scroll_pattern = $scrollEvidence
        uia = @($uia)
    }
    $data | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $evidence -Encoding utf8
    $data | ConvertTo-Json -Depth 6
}
finally {
    [Environment]::SetEnvironmentVariable("TCC_VISUAL_FIXTURE", $null, "Process")
    if ($null -ne $app -and -not $app.HasExited) {
        $app.CloseMainWindow() | Out-Null
        if (-not $app.WaitForExit(3000)) { Stop-Process -Id $app.Id -Force }
    }
}
