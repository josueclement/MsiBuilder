using System;
using System.Threading;
using System.Threading.Tasks;
using Carbon.Avalonia.Desktop.Controls.InfoBar;
using Carbon.Avalonia.Desktop.Services;
using MsiBuilder.Contracts;
using MsiBuilderUI.Services;
using MsiBuilderUI.ViewModels;
using NSubstitute;
using Xunit;

namespace MsiBuilderUI.Tests;

public class MainWindowViewModelTests
{
    private static MainWindowViewModel CreateVm(
        IMsiBuildService? build = null,
        IStoragePickerService? picker = null,
        IProfileService? profile = null,
        IInfoBarService? infoBar = null)
        => new(
            build ?? Substitute.For<IMsiBuildService>(),
            picker ?? Substitute.For<IStoragePickerService>(),
            profile ?? Substitute.For<IProfileService>(),
            infoBar ?? Substitute.For<IInfoBarService>());

    [Fact]
    public void GenerateProductId_ProducesValidGuid()
    {
        MainWindowViewModel vm = CreateVm();
        vm.GenerateProductIdCommand.Execute(null);
        Assert.True(Guid.TryParse(vm.ProductId, out _));
    }

    [Fact]
    public void GenerateUpgradeCode_ProducesValidGuid()
    {
        MainWindowViewModel vm = CreateVm();
        vm.GenerateUpgradeCodeCommand.Execute(null);
        Assert.True(Guid.TryParse(vm.UpgradeCode, out _));
    }

    [Fact]
    public void AddAndRemoveShortcut_MutatesCollection()
    {
        MainWindowViewModel vm = CreateVm();
        Assert.Empty(vm.Shortcuts);

        vm.AddShortcutCommand.Execute(null);
        Assert.Single(vm.Shortcuts);

        ShortcutViewModel added = vm.Shortcuts[0];
        vm.RemoveShortcutCommand.Execute(added);
        Assert.Empty(vm.Shortcuts);
    }

    [Fact]
    public void ToRequest_OmitsManagedUi_WhenNotCustomized()
    {
        MainWindowViewModel vm = CreateVm();
        vm.UseCustomManagedUi = false;
        Assert.Null(vm.ToRequest().ManagedUi);
    }

    [Fact]
    public void ToRequest_IncludesDefaultSelectedDialogs_WhenCustomized()
    {
        MainWindowViewModel vm = CreateVm();
        vm.UseCustomManagedUi = true;

        MsiBuildRequest request = vm.ToRequest();

        Assert.NotNull(request.ManagedUi);
        Assert.Contains(DialogOption.Welcome, request.ManagedUi.InstallDialogs);
        Assert.Contains(DialogOption.Exit, request.ManagedUi.InstallDialogs);
    }

    [Fact]
    public void ToRequest_BlankOptionalFields_BecomeNull()
    {
        MainWindowViewModel vm = CreateVm();
        vm.Comments = "   ";
        vm.ProductIcon = "";

        MsiBuildRequest request = vm.ToRequest();

        Assert.Null(request.Comments);
        Assert.Null(request.ProductIcon);
    }

    [Fact]
    public void LoadFrom_ThenToRequest_RoundTripsCoreFields()
    {
        MainWindowViewModel vm = CreateVm();
        var source = new MsiBuildRequest
        {
            AppName = "Loaded",
            InstallPath = "%ProgramFiles%\\Loaded",
            ReleasePath = "C:\\rel",
            Scope = InstallScopeOption.PerUser,
            Version = "2.3.4",
            ProductId = Guid.NewGuid().ToString(),
            UpgradeCode = Guid.NewGuid().ToString(),
            Manufacturer = "Acme",
            Compression = CompressionLevelOption.High,
            OutputPath = "C:\\out",
            MsiFilename = "Loaded",
            Shortcuts = { new ShortcutDto { ShortcutPath = "%Desktop%", ShortcutName = "L" } }
        };

        vm.LoadFrom(source);
        MsiBuildRequest result = vm.ToRequest();

        Assert.Equal(source.AppName, result.AppName);
        Assert.Equal(source.Scope, result.Scope);
        Assert.Equal(source.Compression, result.Compression);
        Assert.Equal(source.ProductId, result.ProductId);
        Assert.Single(result.Shortcuts);
        Assert.Equal("%Desktop%", result.Shortcuts[0].ShortcutPath);
    }

    [Fact]
    public async Task Build_WithInvalidGuid_DoesNotInvokeBuildService()
    {
        IMsiBuildService build = Substitute.For<IMsiBuildService>();
        MainWindowViewModel vm = CreateVm(build);
        SetValidRequiredFields(vm);
        vm.ProductId = "not-a-guid";

        await vm.BuildCommand.ExecuteAsync(null);

        Assert.True(vm.HasResult);
        Assert.False(vm.LastBuildSucceeded);
        await build.DidNotReceive().BuildAsync(Arg.Any<MsiBuildRequest>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Build_WhenValid_InvokesServiceAndReportsSuccess()
    {
        IMsiBuildService build = Substitute.For<IMsiBuildService>();
        build.BuildAsync(Arg.Any<MsiBuildRequest>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
             .Returns(Task.FromResult(new MsiBuildResult { Success = true, MsiPath = "C:\\out\\MyInstaller.msi" }));
        MainWindowViewModel vm = CreateVm(build);
        SetValidRequiredFields(vm);

        await vm.BuildCommand.ExecuteAsync(null);

        Assert.True(vm.LastBuildSucceeded);
        Assert.Contains("MyInstaller.msi", vm.StatusMessage);
        Assert.False(vm.IsBuilding);
    }

    [Fact]
    public async Task Build_WhenValid_ShowsSuccessInfoBar()
    {
        IMsiBuildService build = Substitute.For<IMsiBuildService>();
        build.BuildAsync(Arg.Any<MsiBuildRequest>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
             .Returns(Task.FromResult(new MsiBuildResult { Success = true, MsiPath = "C:\\out\\MyInstaller.msi" }));
        IInfoBarService infoBar = Substitute.For<IInfoBarService>();
        MainWindowViewModel vm = CreateVm(build, infoBar: infoBar);
        SetValidRequiredFields(vm);

        await vm.BuildCommand.ExecuteAsync(null);

        Assert.True(vm.LastBuildSucceeded);
        await infoBar.Received(1).ShowAsync(Arg.Any<Action<InfoBar>>());
    }

    [Fact]
    public async Task Build_WhenServiceReportsFailure_ShowsErrorInfoBar()
    {
        IMsiBuildService build = Substitute.For<IMsiBuildService>();
        build.BuildAsync(Arg.Any<MsiBuildRequest>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
             .Returns(Task.FromResult(new MsiBuildResult { Success = false, Message = "boom" }));
        IInfoBarService infoBar = Substitute.For<IInfoBarService>();
        MainWindowViewModel vm = CreateVm(build, infoBar: infoBar);
        SetValidRequiredFields(vm);

        await vm.BuildCommand.ExecuteAsync(null);

        Assert.False(vm.LastBuildSucceeded);
        await infoBar.Received(1).ShowAsync(Arg.Any<Action<InfoBar>>());
    }

    [Fact]
    public async Task Build_DoesNotWaitForInfoBarDismissal()
    {
        IMsiBuildService build = Substitute.For<IMsiBuildService>();
        build.BuildAsync(Arg.Any<MsiBuildRequest>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
             .Returns(Task.FromResult(new MsiBuildResult { Success = true, MsiPath = "C:\\out\\MyInstaller.msi" }));
        IInfoBarService infoBar = Substitute.For<IInfoBarService>();
        // Mirror the real Carbon InfoBar: ShowAsync completes only when the bar is dismissed.
        infoBar.ShowAsync(Arg.Any<Action<InfoBar>>()).Returns(new TaskCompletionSource().Task);
        MainWindowViewModel vm = CreateVm(build, infoBar: infoBar);
        SetValidRequiredFields(vm);

        await vm.BuildCommand.ExecuteAsync(null);

        // The build must finish (spinner off, command re-enabled) without waiting for the notification.
        Assert.False(vm.IsBuilding);
        Assert.True(vm.LastBuildSucceeded);
    }

    private static void SetValidRequiredFields(MainWindowViewModel vm)
    {
        vm.AppName = "MyApp";
        vm.InstallPath = "%ProgramFiles%\\MyApp";
        vm.ReleasePath = "C:\\rel";
        vm.Version = "1.0.0";
        vm.ProductId = Guid.NewGuid().ToString();
        vm.UpgradeCode = Guid.NewGuid().ToString();
        vm.Manufacturer = "Acme";
        vm.OutputPath = "C:\\out";
        vm.MsiFilename = "MyInstaller";
    }
}
