using System.Collections.Generic;

namespace MsiBuilder.Contracts;

/// <summary>
/// Optional managed-UI configuration. When a <see cref="MsiBuildRequest.ManagedUi"/> is null, the worker
/// lets <c>WixMsiBuilder</c> apply its built-in default UI.
/// </summary>
public class ManagedUiDto
{
    /// <summary>The built-in WixUI dialog set to use.</summary>
    public WuiOption Wui { get; set; } = WuiOption.WixUI_InstallDir;

    /// <summary>Ordered dialogs shown during a fresh install.</summary>
    public List<DialogOption> InstallDialogs { get; set; } = new();

    /// <summary>Ordered dialogs shown during modify/repair/uninstall.</summary>
    public List<DialogOption> ModifyDialogs { get; set; } = new();
}
