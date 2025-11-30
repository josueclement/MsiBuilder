using System.Linq;
using System;
using WixSharp.CommonTasks;
using WixSharp.Forms;
using WixSharp;

namespace MsiBuilder;

/// <summary>
/// Builder class for creating Windows Installer (MSI) packages using WixSharp.
/// Provides a fluent API for configuring and building MSI installers with various options.
/// </summary>
public class WixMsiBuilder(string appName)
{
    //dotnet tool install --global wix
    private readonly ManagedProject _project = new(appName);
    private bool _installDirSet;
    private bool _installScopeSet;
    private bool _versionSet;
    private bool _productIdSet;
    private bool _upgradeCodeSet;
    private bool _managedUiSet;
    private bool _controlPanelInfoSet;
    private bool _compressionLevelSet;
    private bool _outputSet;

    /// <summary>
    /// Sets the installation directory for the application.
    /// Uses Windows environment variables like %ProgramFiles% in the installPath to determine
    /// the target installation folder (e.g., "%ProgramFiles%\MyApp" resolves to "C:\Program Files\MyApp").
    /// </summary>
    /// <param name="installPath">The target installation path, which can include environment variables like %ProgramFiles%</param>
    /// <param name="releasePath">The local path containing the files to be included in the installer</param>
    /// <returns>The builder instance for method chaining</returns>
    public WixMsiBuilder SetInstallDir(string installPath, string releasePath)
    {
        var installDir = new InstallDir(installPath, new Files($@"{releasePath}\*.*"));
        _project.AddDir(installDir);
        _installDirSet = true;
        return this;
    }
    
    /// <summary>
    /// Adds a shortcut to the installer package.
    /// Supports special Windows folders like %Desktop% and %ProgramMenu% in the shortcutPath.
    /// For example, "%Desktop%" creates a shortcut on the user's desktop, and "%ProgramMenu%" 
    /// creates a shortcut in the Start Menu.
    /// </summary>
    /// <param name="shortcutPath">The location where the shortcut will be created (e.g., "%Desktop%" or "%ProgramMenu%")</param>
    /// <param name="shortcutName">The display name of the shortcut</param>
    /// <param name="targetPath">The path to the executable file that the shortcut will launch</param>
    /// <param name="iconPath">The path to the icon file for the shortcut</param>
    /// <param name="arguments">Optional command-line arguments to pass to the executable</param>
    /// <returns>The builder instance for method chaining</returns>
    public WixMsiBuilder AddShortcut(
        string shortcutPath,
        string shortcutName,
        string targetPath,
        string iconPath,
        string arguments = "")
    {
        var shortcutDir = new Dir(shortcutPath,
            new ExeFileShortcut(shortcutName, targetPath, arguments: arguments)
            {
                WorkingDirectory = "[INSTALLDIR]",
                IconFile = iconPath
            }
        );
        _project.AddDir(shortcutDir);
        return this;
    }

    /// <summary>
    /// Sets the installation scope (per-user or per-machine).
    /// </summary>
    /// <param name="installScope">The scope of the installation</param>
    /// <returns>The builder instance for method chaining</returns>
    public WixMsiBuilder SetInstallScope(InstallScope installScope)
    {
        _project.Scope = installScope;
        _installScopeSet = true;
        return this;
    }

    /// <summary>
    /// Sets the version number of the application.
    /// </summary>
    /// <param name="version">The version string (e.g., "1.0.0.0")</param>
    /// <returns>The builder instance for method chaining</returns>
    public WixMsiBuilder SetAppVersion(string version)
    {
        _project.Version = new Version(version);
        _versionSet = true;
        return this;
    }

    /// <summary>
    /// Sets the unique product ID for this installer.
    /// </summary>
    /// <param name="productId">A GUID that uniquely identifies this product version</param>
    /// <returns>The builder instance for method chaining</returns>
    public WixMsiBuilder SetProductId(Guid productId)
    {
        _project.ProductId = productId;
        _productIdSet = true;
        return this;
    }

    /// <summary>
    /// Sets the upgrade code for the installer.
    /// The upgrade code should remain constant across different versions of the same product
    /// to enable proper upgrade detection.
    /// </summary>
    /// <param name="upgradeCode">A GUID that identifies the product line for upgrades</param>
    /// <returns>The builder instance for method chaining</returns>
    public WixMsiBuilder SetUpgradeCode(Guid upgradeCode)
    {
        _project.UpgradeCode = upgradeCode;
        _upgradeCodeSet = true;
        return this;
    }

    /// <summary>
    /// Configures the managed user interface for the installer.
    /// </summary>
    /// <param name="wui">The WixSharp UI type to use</param>
    /// <param name="configure">An action to configure the managed UI dialogs</param>
    /// <returns>The builder instance for method chaining</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public WixMsiBuilder SetManagedUi(WUI wui, Action<IManagedUI> configure)
    {
        _project.UI = wui;
        _project.ManagedUI = new ManagedUI();
        configure(_project.ManagedUI);
        _managedUiSet = true;
        return this;
    }

    /// <summary>
    /// Configures the product information that appears in Windows Control Panel's
    /// Programs and Features section.
    /// </summary>
    /// <param name="productInfo">An action to configure the product information</param>
    /// <returns>The builder instance for method chaining</returns>
    public WixMsiBuilder SetControlPanelInfo(Action<ProductInfo> productInfo)
    {
        productInfo(_project.ControlPanelInfo);
        _controlPanelInfoSet = true;
        return this;
    }

    /// <summary>
    /// Sets the compression level for the MSI package files.
    /// </summary>
    /// <param name="compressionLevel">The compression level to apply</param>
    /// <returns>The builder instance for method chaining</returns>
    public WixMsiBuilder SetCompressionLevel(CompressionLevel compressionLevel)
    {
        _project.Media.FirstOrDefault()?.CompressionLevel = compressionLevel;
        _compressionLevelSet = true;
        return this;
    }
    
    /// <summary>
    /// Sets the output path and filename for the generated MSI file.
    /// </summary>
    /// <param name="outputPath">The directory where the MSI file will be created</param>
    /// <param name="msiFilename">The name of the MSI file (without path and without extension)</param>
    /// <returns>The builder instance for method chaining</returns>
    public WixMsiBuilder SetOutput(string outputPath, string msiFilename)
    {
        _outputSet = true;
        _project.OutDir = outputPath;
        _project.OutFileName = msiFilename;
        return this;
    }

    /// <summary>
    /// Builds the MSI installer package after validating that all required configurations are set.
    /// If the managed UI is not explicitly configured, a default UI with standard dialogs is applied.
    /// </summary>
    /// <returns>
    /// A tuple containing:
    /// - success: Whether the build was successful
    /// - msiPath: The path to the generated MSI file (null if build failed)
    /// - message: An error message if validation failed (null if successful)
    /// </returns>
    public (bool success, string? msiPath, string? message) BuildMsi()
    {
        if (!_installDirSet)
            return (false, null, "InstallDir must be set before building the MSI");
        if (!_installScopeSet)
            return (false, null, "InstallScope must be set before building the MSI");
        if (!_versionSet)
            return (false, null, "Version must be set before building the MSI");
        if (!_productIdSet)
            return (false, null, "ProductId must be set before building the MSI");
        if (!_upgradeCodeSet)
            return (false, null, "UpgradeCode must be set before building the MSI");
        if (!_controlPanelInfoSet)
            return (false, null, "ControlPanelInfo must be set before building the MSI");
        if (!_compressionLevelSet)
            return (false, null, "CompressionLevel must be set before building the MSI");
        if (!_outputSet)
            return (false, null, "Output must be set before building the MSI");

        if (!_managedUiSet)
        {
            SetManagedUi(WUI.WixUI_InstallDir, ui =>
            {
                ui.InstallDialogs
                    .Add(Dialogs.Welcome)
                    .Add(Dialogs.InstallDir)
                    .Add(Dialogs.Progress)
                    .Add(Dialogs.Exit);
        
                ui.ModifyDialogs
                    .Add(Dialogs.Welcome)
                    .Add(Dialogs.MaintenanceType)
                    .Add(Dialogs.Progress)
                    .Add(Dialogs.Exit); 
            });
        }
        
        return (true, _project.BuildMsi(), null);
    }
}