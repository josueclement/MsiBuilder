namespace MsiBuilder.Contracts;

/// <summary>
/// A single shortcut to create, mirroring the arguments of <c>WixMsiBuilder.AddShortcut</c>.
/// </summary>
public class ShortcutDto
{
    /// <summary>Location of the shortcut, e.g. <c>%Desktop%</c> or <c>%ProgramMenu%</c>.</summary>
    public string ShortcutPath { get; set; } = string.Empty;

    /// <summary>Display name of the shortcut.</summary>
    public string ShortcutName { get; set; } = string.Empty;

    /// <summary>Target the shortcut launches, e.g. <c>[INSTALLDIR]\MyApp.exe</c>.</summary>
    public string TargetPath { get; set; } = string.Empty;

    /// <summary>Path to the shortcut's icon file.</summary>
    public string IconPath { get; set; } = string.Empty;

    /// <summary>Optional command-line arguments passed to the target.</summary>
    public string Arguments { get; set; } = string.Empty;
}
