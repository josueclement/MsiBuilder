using CommunityToolkit.Mvvm.ComponentModel;
using MsiBuilder.Contracts;

namespace MsiBuilderUI.ViewModels;

/// <summary>A selectable managed-UI dialog in one of the dialog checklists.</summary>
public class DialogSelectionViewModel : ObservableObject
{
    public DialogOption Option { get; }

    public string DisplayName => Option.ToString();

    public bool IsSelected { get; set => SetProperty(ref field, value); }

    public DialogSelectionViewModel(DialogOption option, bool isSelected)
    {
        Option = option;
        IsSelected = isSelected;
    }
}
