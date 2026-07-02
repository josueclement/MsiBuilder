using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Carbon.Avalonia.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;
using MsiBuilderUI.Views;

namespace MsiBuilderUI;

public partial class App : Application
{
    private readonly IServiceProvider? _services;

    /// <summary>Parameterless constructor used by the Avalonia XAML previewer/designer.</summary>
    public App()
    {
    }

    public App(IServiceProvider services)
    {
        _services = services;
    }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && _services is not null)
        {
            MainWindow mainWindow = _services.GetRequiredService<MainWindow>();

            // Carbon's file/folder pickers resolve paths through the window's storage provider,
            // which must be handed to them once at startup.
            _services.GetRequiredService<IFileDialogService>().SetStorageProvider(mainWindow.StorageProvider);
            _services.GetRequiredService<IFolderDialogService>().SetStorageProvider(mainWindow.StorageProvider);

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
