using System;
using Avalonia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MsiBuilderUI.Options;
using MsiBuilderUI.Services;
using MsiBuilderUI.ViewModels;
using MsiBuilderUI.Views;

namespace MsiBuilderUI;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection(WorkerOptions.SectionName));
        builder.Services.AddSingleton<IMsiBuildService, WorkerMsiBuildService>();
        builder.Services.AddSingleton<IStoragePickerService, AvaloniaStoragePickerService>();
        builder.Services.AddSingleton<IProfileService, ProfileService>();
        builder.Services.AddSingleton<MainWindowViewModel>();
        builder.Services.AddTransient<MainWindow>();

        using IHost host = builder.Build();
        host.Start();
        try
        {
            BuildAvaloniaApp(host.Services).StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            host.StopAsync().GetAwaiter().GetResult();
        }
    }

    public static AppBuilder BuildAvaloniaApp(IServiceProvider services)
        => AppBuilder.Configure(() => new App(services))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    // Parameterless entry point used by the Avalonia XAML previewer/designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
