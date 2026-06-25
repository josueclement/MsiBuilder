namespace MsiBuilder.Contracts;

// WixSharp-free mirrors of the WixSharp enums/dialog set. The net472 worker maps these to the real
// WixSharp types (WixSharp itself targets net472 and cannot be referenced from netstandard2.0/net10).
// Member sets pinned against WixSharp 2.12 source.

/// <summary>Mirror of WixSharp's <c>InstallScope</c>.</summary>
public enum InstallScopeOption
{
    PerUser,
    PerMachine,
    PerUserOrMachine
}

/// <summary>Mirror of WixSharp's <c>CompressionLevel</c>.</summary>
public enum CompressionLevelOption
{
    None,
    Low,
    Medium,
    High,
    MsZip
}

/// <summary>Mirror of WixSharp's <c>WUI</c> (built-in WixUI dialog sets).</summary>
public enum WuiOption
{
    WixUI_Minimal,
    WixUI_InstallDir,
    WixUI_FeatureTree,
    WixUI_Mondo,
    WixUI_Advanced,
    WixUI_ProgressOnly,
    WixUI_Common
}

/// <summary>
/// Mirror of the standard managed-UI dialogs exposed by WixSharp's <c>Dialogs</c> helper.
/// Note the British spelling <c>Licence</c>, matching WixSharp's member name.
/// </summary>
public enum DialogOption
{
    Welcome,
    Licence,
    InstallDir,
    Features,
    SetupType,
    Progress,
    MaintenanceType,
    Exit
}
