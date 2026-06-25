using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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
}
