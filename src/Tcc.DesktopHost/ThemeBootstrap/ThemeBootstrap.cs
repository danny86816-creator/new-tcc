using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tcc.Presentation.Contracts.Theme;
using Tcc.Themes.Fallback;
using Tcc.Windows.Monitors;
using Tcc.Windows.Startup;

namespace Tcc.DesktopHost.ThemeBootstrap;

internal static class ThemeBootstrap
{
    internal static void Configure(HostApplicationBuilder builder, string[] args)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(args);

        int variantCount = 0;
        string? requestedVariant = null;
        bool invalidVariantShape = false;
        List<string> windowsArguments = new();
        for (int index = 0; index < args.Length; index++)
        {
            string argument = args[index] ?? throw new ArgumentNullException(nameof(args));
            if (argument.StartsWith("--startup-variant=", StringComparison.Ordinal))
            {
                variantCount++;
                requestedVariant = argument["--startup-variant=".Length..];
            }
            else if (string.Equals(argument, "--startup-variant", StringComparison.Ordinal))
            {
                invalidVariantShape = true;
            }
            else
            {
                windowsArguments.Add(argument);
            }
        }

        bool invalidVariant = invalidVariantShape
            || variantCount > 1
            || variantCount == 1 && requestedVariant is not ("deep" or "light");
        ThemeVariantId selectedVariant = new(
            !invalidVariant && string.Equals(requestedVariant, "light", StringComparison.Ordinal)
                ? "light"
                : "deep");

        WindowsStartupPathResolver pathResolver = new();
        WindowsStartupFacts startupFacts = pathResolver.Resolve(windowsArguments);
        BuiltInThemePresentationSource presentationSource = new();
        BuiltInThemePresentationSnapshot presentation = presentationSource.GetPresentation(selectedVariant);

        string startupNotice = selectedVariant.Value == "light"
            ? "內建 Light 顯示模式已啟用。"
            : "內建 Deep 顯示模式已啟用。";
        if (invalidVariant)
        {
            startupNotice += " BOOTSTRAP_VARIANT_INVALID；已使用 Deep。";
        }

        if (startupFacts.DiagnosticCode is not null)
        {
            startupNotice += $" {startupFacts.DiagnosticCode}；儲存路徑目前不可用。";
        }

        builder.Services.AddSingleton(pathResolver);
        builder.Services.AddSingleton(startupFacts);
        builder.Services.AddSingleton(presentationSource);
        builder.Services.AddSingleton(presentation);
        builder.Services.AddTransient<WindowMonitorAdapter>();
        builder.Services.AddSingleton(new MainWindowViewModel(presentation, startupNotice));
        builder.Services.AddSingleton<MainWindow>();
    }
}
