using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Resources;
using System.Text.Json;
using System.Windows;
using Tcc.DesktopHost.ThemeBootstrap;

namespace Tcc.DesktopHost;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            List<string> bootstrapArguments = new();
            List<string> hostArguments = new();
            for (int index = 0; index < e.Args.Length; index++)
            {
                string argument = e.Args[index];
                if (argument.StartsWith("--startup-variant=", StringComparison.Ordinal)
                    || string.Equals(argument, "--startup-variant", StringComparison.Ordinal)
                    || argument.StartsWith("--startup-mode=", StringComparison.Ordinal)
                    || string.Equals(argument, "--startup-mode", StringComparison.Ordinal)
                    || argument.StartsWith("--portable-data-root=", StringComparison.Ordinal)
                    || string.Equals(argument, "--portable-data-root", StringComparison.Ordinal))
                {
                    bootstrapArguments.Add(argument);
                }
                else
                {
                    hostArguments.Add(argument);
                }
            }

            HostApplicationBuilder builder = Host.CreateApplicationBuilder(hostArguments.ToArray());
            ThemeBootstrap.ThemeBootstrap.Configure(builder, bootstrapArguments.ToArray());

            _host = builder.Build();
            await _host.StartAsync();

            MainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow.Show();
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidOperationException
            or JsonException
            or MissingManifestResourceException)
        {
            MessageBox.Show(
                "內建顯示資源無法載入。應用程式將結束。",
                "Trading Command Center",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
