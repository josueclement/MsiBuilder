using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using MsiBuilderUI.ViewModels;

namespace MsiBuilderUI.Views;

public partial class MainWindow : Window
{
    /// <summary>Parameterless constructor used by the Avalonia XAML previewer/designer.</summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainWindowViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    /// <summary>Switches the application between the Carbon dark and light theme variants.</summary>
    private void OnThemeToggleChanged(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is { } app && sender is ToggleSwitch toggle)
            app.RequestedThemeVariant = toggle.IsChecked == true ? ThemeVariant.Dark : ThemeVariant.Light;
    }
}
