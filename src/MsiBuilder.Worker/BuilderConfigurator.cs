using System;
using MsiBuilder.Contracts;
using WixSharp;
using WixSharp.Forms;

namespace MsiBuilder.Worker;

/// <summary>
/// Translates a <see cref="MsiBuildRequest"/> into the <see cref="WixMsiBuilder"/> fluent chain
/// (mirroring README/ConsoleApp1) and runs the build. The pure value-mapping helpers are exposed so they
/// can be unit-tested without invoking the real <c>BuildMsi</c> (which needs Windows + the wix tool).
/// </summary>
public static class BuilderConfigurator
{
    /// <summary>Maps the request onto the builder and runs the build.</summary>
    public static MsiBuildResult Build(MsiBuildRequest request)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (!Guid.TryParse(request.ProductId, out Guid productId))
            return Fail($"ProductId is not a valid GUID: '{request.ProductId}'");
        if (!Guid.TryParse(request.UpgradeCode, out Guid upgradeCode))
            return Fail($"UpgradeCode is not a valid GUID: '{request.UpgradeCode}'");

        WixMsiBuilder builder = new WixMsiBuilder(request.AppName)
            .SetInstallDir(request.InstallPath, request.ReleasePath);

        foreach (ShortcutDto shortcut in request.Shortcuts)
        {
            builder.AddShortcut(
                shortcutPath: shortcut.ShortcutPath,
                shortcutName: shortcut.ShortcutName,
                targetPath: shortcut.TargetPath,
                iconPath: shortcut.IconPath,
                arguments: shortcut.Arguments);
        }

        builder
            .SetInstallScope(MapScope(request.Scope))
            .SetAppVersion(request.Version)
            .SetProductId(productId)
            .SetUpgradeCode(upgradeCode)
            .SetControlPanelInfo(info =>
            {
                info.Manufacturer = request.Manufacturer;
                if (!string.IsNullOrWhiteSpace(request.ProductIcon))
                    info.ProductIcon = request.ProductIcon;
                if (!string.IsNullOrWhiteSpace(request.Comments))
                    info.Comments = request.Comments;
                if (!string.IsNullOrWhiteSpace(request.Contact))
                    info.Contact = request.Contact;
                if (!string.IsNullOrWhiteSpace(request.HelpLink))
                    info.HelpLink = request.HelpLink;
                if (!string.IsNullOrWhiteSpace(request.UrlInfoAbout))
                    info.UrlInfoAbout = request.UrlInfoAbout;
            })
            .SetCompressionLevel(MapCompression(request.Compression));

        if (request.ManagedUi is ManagedUiDto managedUiDto)
        {
            builder.SetManagedUi(MapWui(managedUiDto.Wui), ui =>
            {
                foreach (DialogOption dialog in managedUiDto.InstallDialogs)
                    ui.InstallDialogs.Add(MapDialog(dialog));
                foreach (DialogOption dialog in managedUiDto.ModifyDialogs)
                    ui.ModifyDialogs.Add(MapDialog(dialog));
            });
        }

        builder.SetOutput(request.OutputPath, request.MsiFilename);

        (bool success, string? msiPath, string? message) = builder.BuildMsi();
        return new MsiBuildResult { Success = success, MsiPath = msiPath, Message = message };
    }

    /// <summary>Maps the mirror scope to WixSharp's <see cref="InstallScope"/>.</summary>
    public static InstallScope MapScope(InstallScopeOption scope) => scope switch
    {
        InstallScopeOption.PerUser => InstallScope.perUser,
        InstallScopeOption.PerMachine => InstallScope.perMachine,
        InstallScopeOption.PerUserOrMachine => InstallScope.perUserOrMachine,
        _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, "Unsupported install scope")
    };

    /// <summary>Maps the mirror compression level to WixSharp's <see cref="CompressionLevel"/>.</summary>
    public static CompressionLevel MapCompression(CompressionLevelOption level) => level switch
    {
        CompressionLevelOption.None => CompressionLevel.none,
        CompressionLevelOption.Low => CompressionLevel.low,
        CompressionLevelOption.Medium => CompressionLevel.medium,
        CompressionLevelOption.High => CompressionLevel.high,
        CompressionLevelOption.MsZip => CompressionLevel.mszip,
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unsupported compression level")
    };

    /// <summary>Maps the mirror WUI option to WixSharp's <see cref="WUI"/>.</summary>
    public static WUI MapWui(WuiOption wui) => wui switch
    {
        WuiOption.WixUI_Minimal => WUI.WixUI_Minimal,
        WuiOption.WixUI_InstallDir => WUI.WixUI_InstallDir,
        WuiOption.WixUI_FeatureTree => WUI.WixUI_FeatureTree,
        WuiOption.WixUI_Mondo => WUI.WixUI_Mondo,
        WuiOption.WixUI_Advanced => WUI.WixUI_Advanced,
        WuiOption.WixUI_ProgressOnly => WUI.WixUI_ProgressOnly,
        WuiOption.WixUI_Common => WUI.WixUI_Common,
        _ => throw new ArgumentOutOfRangeException(nameof(wui), wui, "Unsupported WUI")
    };

    /// <summary>
    /// Maps the mirror dialog option to the WixSharp managed-dialog <see cref="Type"/> exposed by the
    /// <c>Dialogs</c> helper. Member names are pinned to WixSharp 2.12 (note British 'Licence').
    /// </summary>
    public static Type MapDialog(DialogOption dialog) => dialog switch
    {
        DialogOption.Welcome => Dialogs.Welcome,
        DialogOption.Licence => Dialogs.Licence,
        DialogOption.InstallDir => Dialogs.InstallDir,
        DialogOption.Features => Dialogs.Features,
        DialogOption.SetupType => Dialogs.SetupType,
        DialogOption.Progress => Dialogs.Progress,
        DialogOption.MaintenanceType => Dialogs.MaintenanceType,
        DialogOption.Exit => Dialogs.Exit,
        _ => throw new ArgumentOutOfRangeException(nameof(dialog), dialog, "Unsupported dialog")
    };

    private static MsiBuildResult Fail(string message)
        => new MsiBuildResult { Success = false, MsiPath = null, Message = message };
}
