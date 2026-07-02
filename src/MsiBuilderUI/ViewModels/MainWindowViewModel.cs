using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Carbon.Avalonia.Desktop.Controls.InfoBar;
using Carbon.Avalonia.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsiBuilder.Contracts;
using MsiBuilderUI.Services;

namespace MsiBuilderUI.ViewModels;

/// <summary>Root view model: collects all build inputs, runs the build, and handles save/load profiles.</summary>
public class MainWindowViewModel : ObservableObject
{
    private readonly IMsiBuildService _buildService;
    private readonly IStoragePickerService _picker;
    private readonly IProfileService _profileService;
    private readonly IInfoBarService _infoBar;

    public MainWindowViewModel(IMsiBuildService buildService, IStoragePickerService picker, IProfileService profileService, IInfoBarService infoBar)
    {
        _buildService = buildService;
        _picker = picker;
        _profileService = profileService;
        _infoBar = infoBar;

        InstallDialogOptions = CreateDialogOptions();
        ModifyDialogOptions = CreateDialogOptions();
        ApplyDefaultManagedUiSelection();

        GenerateProductIdCommand = new RelayCommand(() => ProductId = Guid.NewGuid().ToString());
        GenerateUpgradeCodeCommand = new RelayCommand(() => UpgradeCode = Guid.NewGuid().ToString());
        AddShortcutCommand = new RelayCommand(OnAddShortcut);
        RemoveShortcutCommand = new RelayCommand<ShortcutViewModel>(OnRemoveShortcut);
        BrowseReleasePathCommand = new AsyncRelayCommand(OnBrowseReleasePathAsync);
        BrowseOutputPathCommand = new AsyncRelayCommand(OnBrowseOutputPathAsync);
        BrowseProductIconCommand = new AsyncRelayCommand(OnBrowseProductIconAsync);
        BuildCommand = new AsyncRelayCommand(OnBuildAsync, () => !IsBuilding);
        SaveProfileCommand = new AsyncRelayCommand(OnSaveProfileAsync);
        LoadProfileCommand = new AsyncRelayCommand(OnLoadProfileAsync);
    }

    // --- General ---
    public string AppName { get; set => SetProperty(ref field, value); } = "MyApp";
    public string Version { get; set => SetProperty(ref field, value); } = "1.0.0";
    public InstallScopeOption Scope { get; set => SetProperty(ref field, value); } = InstallScopeOption.PerMachine;

    // --- Install directory ---
    public string InstallPath { get; set => SetProperty(ref field, value); } = "%ProgramFiles%\\MyApp";
    public string ReleasePath { get; set => SetProperty(ref field, value); } = string.Empty;

    // --- Identity ---
    public string ProductId { get; set => SetProperty(ref field, value); } = string.Empty;
    public string UpgradeCode { get; set => SetProperty(ref field, value); } = string.Empty;

    // --- Control Panel info ---
    public string Manufacturer { get; set => SetProperty(ref field, value); } = string.Empty;
    public string ProductIcon { get; set => SetProperty(ref field, value); } = string.Empty;
    public string Comments { get; set => SetProperty(ref field, value); } = string.Empty;
    public string Contact { get; set => SetProperty(ref field, value); } = string.Empty;
    public string HelpLink { get; set => SetProperty(ref field, value); } = string.Empty;
    public string UrlInfoAbout { get; set => SetProperty(ref field, value); } = string.Empty;

    // --- Output ---
    public CompressionLevelOption Compression { get; set => SetProperty(ref field, value); } = CompressionLevelOption.None;
    public string OutputPath { get; set => SetProperty(ref field, value); } = string.Empty;
    public string MsiFilename { get; set => SetProperty(ref field, value); } = "MyInstaller";

    // --- Shortcuts ---
    public ObservableCollection<ShortcutViewModel> Shortcuts { get; } = new();

    // --- Managed UI ---
    public bool UseCustomManagedUi { get; set => SetProperty(ref field, value); }
    public WuiOption SelectedWui { get; set => SetProperty(ref field, value); } = WuiOption.WixUI_InstallDir;
    public ObservableCollection<DialogSelectionViewModel> InstallDialogOptions { get; }
    public ObservableCollection<DialogSelectionViewModel> ModifyDialogOptions { get; }

    // --- Build state ---
    public bool IsBuilding
    {
        get;
        private set
        {
            if (SetProperty(ref field, value))
                BuildCommand.NotifyCanExecuteChanged();
        }
    }

    public string BuildLog { get; private set => SetProperty(ref field, value); } = string.Empty;
    public string StatusMessage { get; private set => SetProperty(ref field, value); } = string.Empty;
    public bool HasResult { get; private set => SetProperty(ref field, value); }
    public bool LastBuildSucceeded { get; private set => SetProperty(ref field, value); }

    // --- Combo option lists ---
    public IReadOnlyList<InstallScopeOption> ScopeOptions { get; } = Enum.GetValues<InstallScopeOption>();
    public IReadOnlyList<CompressionLevelOption> CompressionOptions { get; } = Enum.GetValues<CompressionLevelOption>();
    public IReadOnlyList<WuiOption> WuiOptions { get; } = Enum.GetValues<WuiOption>();

    // --- Commands ---
    public RelayCommand GenerateProductIdCommand { get; }
    public RelayCommand GenerateUpgradeCodeCommand { get; }
    public RelayCommand AddShortcutCommand { get; }
    public RelayCommand<ShortcutViewModel> RemoveShortcutCommand { get; }
    public AsyncRelayCommand BrowseReleasePathCommand { get; }
    public AsyncRelayCommand BrowseOutputPathCommand { get; }
    public AsyncRelayCommand BrowseProductIconCommand { get; }
    public AsyncRelayCommand BuildCommand { get; }
    public AsyncRelayCommand SaveProfileCommand { get; }
    public AsyncRelayCommand LoadProfileCommand { get; }

    private void OnAddShortcut() => Shortcuts.Add(new ShortcutViewModel(_picker));

    private void OnRemoveShortcut(ShortcutViewModel? shortcut)
    {
        if (shortcut is not null)
            Shortcuts.Remove(shortcut);
    }

    private async Task OnBrowseReleasePathAsync()
    {
        string? path = await _picker.PickFolderAsync("Select release folder");
        if (path is not null)
            ReleasePath = path;
    }

    private async Task OnBrowseOutputPathAsync()
    {
        string? path = await _picker.PickFolderAsync("Select output folder");
        if (path is not null)
            OutputPath = path;
    }

    private async Task OnBrowseProductIconAsync()
    {
        string? path = await _picker.PickFileAsync("Select product icon", "Icon", ["ico"]);
        if (path is not null)
            ProductIcon = path;
    }

    private async Task OnSaveProfileAsync()
    {
        string? path = await _picker.PickSaveFileAsync(
            "Save build profile", $"{MsiFilename}.msiprofile.json", "MSI build profile", ["msiprofile.json", "json"]);
        if (path is null)
            return;

        try
        {
            await _profileService.SaveAsync(ToRequest(), path);
            ShowResult(true, $"Profile saved to {path}");
        }
        catch (Exception ex)
        {
            ShowResult(false, $"Failed to save profile: {ex.Message}");
        }
    }

    private async Task OnLoadProfileAsync()
    {
        string? path = await _picker.PickFileAsync("Load build profile", "MSI build profile", ["msiprofile.json", "json"]);
        if (path is null)
            return;

        try
        {
            MsiBuildRequest request = await _profileService.LoadAsync(path);
            LoadFrom(request);
            ShowResult(true, $"Profile loaded from {path}");
        }
        catch (Exception ex)
        {
            ShowResult(false, $"Failed to load profile: {ex.Message}");
        }
    }

    private async Task OnBuildAsync()
    {
        MsiBuildRequest request = ToRequest();

        string? validationError = Validate(request);
        if (validationError is not null)
        {
            ShowResult(false, validationError);
            return;
        }

        IsBuilding = true;
        BuildLog = string.Empty;
        HasResult = false;
        StatusMessage = "Building…";

        var progress = new Progress<string>(AppendLog);
        try
        {
            MsiBuildResult result = await _buildService.BuildAsync(request, progress, CancellationToken.None);
            if (result.Success)
                ShowResult(true, $"MSI created: {result.MsiPath}");
            else
                ShowResult(false, result.Message ?? "Build failed.");
        }
        catch (Exception ex)
        {
            AppendLog(ex.ToString());
            ShowResult(false, $"Build error: {ex.Message}");
        }
        finally
        {
            IsBuilding = false;
        }
    }

    private void AppendLog(string line)
        => BuildLog = BuildLog.Length == 0 ? line : BuildLog + Environment.NewLine + line;

    private void ShowResult(bool success, string message)
    {
        LastBuildSucceeded = success;
        StatusMessage = (success ? "✔ " : "✖ ") + message;
        HasResult = true;

        // Fire-and-forget: the Carbon InfoBar's ShowAsync completes only when the user dismisses the bar.
        // Awaiting it here would keep the build "in progress" (IsBuilding true, command disabled) until the
        // notification is closed, so we start it without blocking the calling flow.
        _ = ShowInfoBarAsync(success, message);
    }

    private async Task ShowInfoBarAsync(bool success, string message)
    {
        try
        {
            await _infoBar.ShowAsync(bar =>
            {
                bar.Title = success ? "Success" : "Error";
                bar.Message = message;
                bar.Severity = success ? InfoBarSeverity.Success : InfoBarSeverity.Error;
            });
        }
        catch (InvalidOperationException)
        {
            // ShowAsync throws only when no InfoBar host has been registered (a startup wiring error);
            // a normally displayed bar's task simply completes when it is dismissed.
        }
    }

    private static string? Validate(MsiBuildRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AppName)) return "Application name is required.";
        if (string.IsNullOrWhiteSpace(request.InstallPath)) return "Install path is required.";
        if (string.IsNullOrWhiteSpace(request.ReleasePath)) return "Release folder is required.";
        if (string.IsNullOrWhiteSpace(request.Version)) return "Version is required.";
        if (!Guid.TryParse(request.ProductId, out _)) return "Product Id must be a valid GUID.";
        if (!Guid.TryParse(request.UpgradeCode, out _)) return "Upgrade code must be a valid GUID.";
        if (string.IsNullOrWhiteSpace(request.Manufacturer)) return "Manufacturer is required.";
        if (string.IsNullOrWhiteSpace(request.OutputPath)) return "Output folder is required.";
        if (string.IsNullOrWhiteSpace(request.MsiFilename)) return "MSI file name is required.";
        return null;
    }

    /// <summary>Builds the contract DTO from the current form state.</summary>
    public MsiBuildRequest ToRequest() => new()
    {
        AppName = AppName,
        InstallPath = InstallPath,
        ReleasePath = ReleasePath,
        Scope = Scope,
        Version = Version,
        ProductId = ProductId,
        UpgradeCode = UpgradeCode,
        Manufacturer = Manufacturer,
        ProductIcon = NullIfBlank(ProductIcon),
        Comments = NullIfBlank(Comments),
        Contact = NullIfBlank(Contact),
        HelpLink = NullIfBlank(HelpLink),
        UrlInfoAbout = NullIfBlank(UrlInfoAbout),
        Compression = Compression,
        OutputPath = OutputPath,
        MsiFilename = MsiFilename,
        Shortcuts = Shortcuts.Select(shortcut => shortcut.ToDto()).ToList(),
        ManagedUi = UseCustomManagedUi ? BuildManagedUi() : null
    };

    /// <summary>Populates the form state from a loaded profile.</summary>
    public void LoadFrom(MsiBuildRequest request)
    {
        AppName = request.AppName;
        InstallPath = request.InstallPath;
        ReleasePath = request.ReleasePath;
        Scope = request.Scope;
        Version = request.Version;
        ProductId = request.ProductId;
        UpgradeCode = request.UpgradeCode;
        Manufacturer = request.Manufacturer;
        ProductIcon = request.ProductIcon ?? string.Empty;
        Comments = request.Comments ?? string.Empty;
        Contact = request.Contact ?? string.Empty;
        HelpLink = request.HelpLink ?? string.Empty;
        UrlInfoAbout = request.UrlInfoAbout ?? string.Empty;
        Compression = request.Compression;
        OutputPath = request.OutputPath;
        MsiFilename = request.MsiFilename;

        Shortcuts.Clear();
        foreach (ShortcutDto dto in request.Shortcuts)
            Shortcuts.Add(ShortcutViewModel.FromDto(_picker, dto));

        if (request.ManagedUi is ManagedUiDto managedUi)
        {
            UseCustomManagedUi = true;
            SelectedWui = managedUi.Wui;
            ApplySelection(InstallDialogOptions, managedUi.InstallDialogs);
            ApplySelection(ModifyDialogOptions, managedUi.ModifyDialogs);
        }
        else
        {
            UseCustomManagedUi = false;
            ApplyDefaultManagedUiSelection();
        }
    }

    private ManagedUiDto BuildManagedUi() => new()
    {
        Wui = SelectedWui,
        InstallDialogs = InstallDialogOptions.Where(d => d.IsSelected).Select(d => d.Option).ToList(),
        ModifyDialogs = ModifyDialogOptions.Where(d => d.IsSelected).Select(d => d.Option).ToList()
    };

    private static ObservableCollection<DialogSelectionViewModel> CreateDialogOptions()
        => new(Enum.GetValues<DialogOption>().Select(option => new DialogSelectionViewModel(option, false)));

    private void ApplyDefaultManagedUiSelection()
    {
        // Mirrors the built-in default dialog sets applied by WixMsiBuilder when no managed UI is configured.
        ApplySelection(InstallDialogOptions, [DialogOption.Welcome, DialogOption.InstallDir, DialogOption.Progress, DialogOption.Exit]);
        ApplySelection(ModifyDialogOptions, [DialogOption.Welcome, DialogOption.MaintenanceType, DialogOption.Progress, DialogOption.Exit]);
    }

    private static void ApplySelection(IEnumerable<DialogSelectionViewModel> options, IReadOnlyCollection<DialogOption> selected)
    {
        foreach (DialogSelectionViewModel option in options)
            option.IsSelected = selected.Contains(option.Option);
    }

    private static string? NullIfBlank(string value)
        => string.IsNullOrWhiteSpace(value) ? null : value;
}
