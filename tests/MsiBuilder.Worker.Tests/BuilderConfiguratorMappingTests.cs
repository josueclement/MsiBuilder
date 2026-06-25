using System;
using MsiBuilder.Contracts;
using WixSharp;
using WixSharp.Forms;
using Xunit;

namespace MsiBuilder.Worker.Tests;

public class BuilderConfiguratorMappingTests
{
    [Theory]
    [InlineData(InstallScopeOption.PerUser, InstallScope.perUser)]
    [InlineData(InstallScopeOption.PerMachine, InstallScope.perMachine)]
    [InlineData(InstallScopeOption.PerUserOrMachine, InstallScope.perUserOrMachine)]
    public void MapScope_MapsAllValues(InstallScopeOption option, InstallScope expected)
        => Assert.Equal(expected, BuilderConfigurator.MapScope(option));

    [Theory]
    [InlineData(CompressionLevelOption.None, CompressionLevel.none)]
    [InlineData(CompressionLevelOption.Low, CompressionLevel.low)]
    [InlineData(CompressionLevelOption.Medium, CompressionLevel.medium)]
    [InlineData(CompressionLevelOption.High, CompressionLevel.high)]
    [InlineData(CompressionLevelOption.MsZip, CompressionLevel.mszip)]
    public void MapCompression_MapsAllValues(CompressionLevelOption option, CompressionLevel expected)
        => Assert.Equal(expected, BuilderConfigurator.MapCompression(option));

    [Theory]
    [InlineData(WuiOption.WixUI_Minimal, WUI.WixUI_Minimal)]
    [InlineData(WuiOption.WixUI_InstallDir, WUI.WixUI_InstallDir)]
    [InlineData(WuiOption.WixUI_FeatureTree, WUI.WixUI_FeatureTree)]
    [InlineData(WuiOption.WixUI_Mondo, WUI.WixUI_Mondo)]
    [InlineData(WuiOption.WixUI_Advanced, WUI.WixUI_Advanced)]
    [InlineData(WuiOption.WixUI_ProgressOnly, WUI.WixUI_ProgressOnly)]
    [InlineData(WuiOption.WixUI_Common, WUI.WixUI_Common)]
    public void MapWui_MapsAllValues(WuiOption option, WUI expected)
        => Assert.Equal(expected, BuilderConfigurator.MapWui(option));

    [Fact]
    public void MapDialog_MapsToWixSharpDialogTypes()
    {
        Assert.Equal(Dialogs.Welcome, BuilderConfigurator.MapDialog(DialogOption.Welcome));
        Assert.Equal(Dialogs.Licence, BuilderConfigurator.MapDialog(DialogOption.Licence));
        Assert.Equal(Dialogs.InstallDir, BuilderConfigurator.MapDialog(DialogOption.InstallDir));
        Assert.Equal(Dialogs.Features, BuilderConfigurator.MapDialog(DialogOption.Features));
        Assert.Equal(Dialogs.SetupType, BuilderConfigurator.MapDialog(DialogOption.SetupType));
        Assert.Equal(Dialogs.Progress, BuilderConfigurator.MapDialog(DialogOption.Progress));
        Assert.Equal(Dialogs.MaintenanceType, BuilderConfigurator.MapDialog(DialogOption.MaintenanceType));
        Assert.Equal(Dialogs.Exit, BuilderConfigurator.MapDialog(DialogOption.Exit));
    }

    [Fact]
    public void Build_WithInvalidProductId_ReturnsFailureWithoutThrowing()
    {
        var request = new MsiBuildRequest { ProductId = "nope", UpgradeCode = Guid.NewGuid().ToString() };

        MsiBuildResult result = BuilderConfigurator.Build(request);

        Assert.False(result.Success);
        Assert.Contains("ProductId", result.Message ?? string.Empty);
    }

    [Fact]
    public void Build_WithInvalidUpgradeCode_ReturnsFailureWithoutThrowing()
    {
        var request = new MsiBuildRequest { ProductId = Guid.NewGuid().ToString(), UpgradeCode = "nope" };

        MsiBuildResult result = BuilderConfigurator.Build(request);

        Assert.False(result.Success);
        Assert.Contains("UpgradeCode", result.Message ?? string.Empty);
    }
}
