using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsiBuilder.Contracts;
using MsiBuilderUI.Services;

namespace MsiBuilderUI.ViewModels;

/// <summary>Editable row for a single shortcut in the dynamic shortcut list.</summary>
public class ShortcutViewModel : ObservableObject
{
    private readonly IStoragePickerService _picker;

    public string ShortcutPath { get; set => SetProperty(ref field, value); } = "%Desktop%";
    public string ShortcutName { get; set => SetProperty(ref field, value); } = string.Empty;
    public string TargetPath { get; set => SetProperty(ref field, value); } = "[INSTALLDIR]\\MyApp.exe";
    public string IconPath { get; set => SetProperty(ref field, value); } = string.Empty;
    public string Arguments { get; set => SetProperty(ref field, value); } = string.Empty;

    public AsyncRelayCommand BrowseIconCommand { get; }

    public ShortcutViewModel(IStoragePickerService picker)
    {
        _picker = picker;
        BrowseIconCommand = new AsyncRelayCommand(OnBrowseIconAsync);
    }

    private async Task OnBrowseIconAsync()
    {
        string? path = await _picker.PickFileAsync("Select shortcut icon", "Icon", ["ico"]);
        if (path is not null)
            IconPath = path;
    }

    public ShortcutDto ToDto() => new()
    {
        ShortcutPath = ShortcutPath,
        ShortcutName = ShortcutName,
        TargetPath = TargetPath,
        IconPath = IconPath,
        Arguments = Arguments
    };

    public static ShortcutViewModel FromDto(IStoragePickerService picker, ShortcutDto dto) => new(picker)
    {
        ShortcutPath = dto.ShortcutPath,
        ShortcutName = dto.ShortcutName,
        TargetPath = dto.TargetPath,
        IconPath = dto.IconPath,
        Arguments = dto.Arguments
    };
}
