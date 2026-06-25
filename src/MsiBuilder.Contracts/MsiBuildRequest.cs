using System.Collections.Generic;

namespace MsiBuilder.Contracts;

/// <summary>
/// The full set of inputs needed to build an MSI, mirroring the <c>WixMsiBuilder</c> fluent API.
/// Serialized to JSON to drive the worker process and also reused as the on-disk build-profile format.
/// </summary>
public class MsiBuildRequest
{
    /// <summary>Application name passed to the <c>WixMsiBuilder</c> constructor.</summary>
    public string AppName { get; set; } = string.Empty;

    /// <summary>Install directory, may contain environment variables, e.g. <c>%ProgramFiles%\MyApp</c>.</summary>
    public string InstallPath { get; set; } = string.Empty;

    /// <summary>Local folder whose contents are packaged into the installer.</summary>
    public string ReleasePath { get; set; } = string.Empty;

    /// <summary>Per-user vs per-machine installation scope.</summary>
    public InstallScopeOption Scope { get; set; } = InstallScopeOption.PerMachine;

    /// <summary>Product version string, e.g. <c>1.1.0</c>.</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Product id GUID (string form). The worker parses it with <c>Guid.Parse</c>.</summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>Upgrade code GUID (string form), constant across versions of the same product.</summary>
    public string UpgradeCode { get; set; } = string.Empty;

    /// <summary>Control Panel "Programs and Features" manufacturer.</summary>
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>Optional product icon shown in Control Panel.</summary>
    public string? ProductIcon { get; set; }

    /// <summary>Optional comments shown in Control Panel.</summary>
    public string? Comments { get; set; }

    /// <summary>Optional support contact shown in Control Panel.</summary>
    public string? Contact { get; set; }

    /// <summary>Optional help link shown in Control Panel.</summary>
    public string? HelpLink { get; set; }

    /// <summary>Optional "about" URL shown in Control Panel.</summary>
    public string? UrlInfoAbout { get; set; }

    /// <summary>Cabinet compression level for the packaged files.</summary>
    public CompressionLevelOption Compression { get; set; } = CompressionLevelOption.None;

    /// <summary>Directory where the generated MSI is written.</summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>MSI file name without path or extension.</summary>
    public string MsiFilename { get; set; } = string.Empty;

    /// <summary>Shortcuts to create.</summary>
    public List<ShortcutDto> Shortcuts { get; set; } = new();

    /// <summary>Optional managed-UI configuration; null lets the builder apply its default UI.</summary>
    public ManagedUiDto? ManagedUi { get; set; }
}
