using System.Collections.Generic;
using MsiBuilder.Contracts;
using Xunit;

namespace MsiBuilderUI.Tests;

public class ContractsSerializationTests
{
    [Fact]
    public void Request_RoundTrips_PreservingAllFields()
    {
        var original = new MsiBuildRequest
        {
            AppName = "MyApp",
            InstallPath = "%ProgramFiles%\\MyApp",
            ReleasePath = "C:\\Temp\\net472",
            Scope = InstallScopeOption.PerMachine,
            Version = "1.1.0",
            ProductId = "f9af10ed-ea36-424b-973e-a0e651203370",
            UpgradeCode = "e74fb99c-5bf1-454d-802a-c7c6f9cc219f",
            Manufacturer = "YourCompany",
            ProductIcon = "C:\\Temp\\logo.ico",
            Comments = "comments",
            Compression = CompressionLevelOption.Medium,
            OutputPath = "C:\\Temp",
            MsiFilename = "MyInstaller",
            Shortcuts = new List<ShortcutDto>
            {
                new()
                {
                    ShortcutPath = "%Desktop%",
                    ShortcutName = "MyApp",
                    TargetPath = "[INSTALLDIR]\\MyApp.exe",
                    IconPath = "C:\\Temp\\logo.ico",
                    Arguments = "--x"
                }
            },
            ManagedUi = new ManagedUiDto
            {
                Wui = WuiOption.WixUI_InstallDir,
                InstallDialogs = new List<DialogOption> { DialogOption.Welcome, DialogOption.InstallDir, DialogOption.Progress, DialogOption.Exit },
                ModifyDialogs = new List<DialogOption> { DialogOption.Welcome, DialogOption.MaintenanceType }
            }
        };

        string json = MsiContractSerializer.Serialize(original);
        MsiBuildRequest? clone = MsiContractSerializer.Deserialize<MsiBuildRequest>(json);

        Assert.NotNull(clone);
        Assert.Equal(original.AppName, clone.AppName);
        Assert.Equal(original.Scope, clone.Scope);
        Assert.Equal(original.Compression, clone.Compression);
        Assert.Equal(original.ProductId, clone.ProductId);
        Assert.Equal(original.UpgradeCode, clone.UpgradeCode);
        Assert.Single(clone.Shortcuts);
        Assert.Equal("%Desktop%", clone.Shortcuts[0].ShortcutPath);
        Assert.Equal("--x", clone.Shortcuts[0].Arguments);
        Assert.NotNull(clone.ManagedUi);
        Assert.Equal(WuiOption.WixUI_InstallDir, clone.ManagedUi.Wui);
        Assert.Equal(4, clone.ManagedUi.InstallDialogs.Count);
        Assert.Equal(DialogOption.MaintenanceType, clone.ManagedUi.ModifyDialogs[1]);
    }

    [Fact]
    public void Enums_AreSerializedAsStrings()
    {
        var request = new MsiBuildRequest
        {
            Compression = CompressionLevelOption.MsZip,
            Scope = InstallScopeOption.PerUserOrMachine
        };

        string json = MsiContractSerializer.Serialize(request);

        Assert.Contains("MsZip", json);
        Assert.Contains("PerUserOrMachine", json);
    }

    [Fact]
    public void NullOptionalFields_AreOmitted()
    {
        var request = new MsiBuildRequest { AppName = "x" };

        string json = MsiContractSerializer.Serialize(request);

        Assert.DoesNotContain("productIcon", json);
        Assert.DoesNotContain("managedUi", json);
    }
}
