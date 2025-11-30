# MsiBuilder

Builder for creating Windows Installer (MSI) packages using WixSharp.

Provides a fluent API for configuring and building MSI installers with various options.

## Install wix tool

```bash
dotnet tool install --global wix
```

## Usage

```csharp
var builder = new WixMsiBuilder("MyApp")
    .SetInstallDir(
        installPath: Path.Combine("%ProgramFiles%", "MyApp"),
        releasePath: @"C:\Temp\net472\")
    .AddShortcut(
        shortcutPath: "%Desktop%",
        shortcutName: "Myapp",
        targetPath: Path.Combine("[INSTALLDIR]", "MyApp.exe"),
        iconPath: @"C:\Temp\net472\logo.ico")
    .AddShortcut(
        shortcutPath: "%ProgramMenu%",
        shortcutName: "Myapp",
        targetPath: Path.Combine("[INSTALLDIR]", "MyApp.exe"),
        iconPath: @"C:\Temp\net472\logo.ico")
    .SetInstallScope(InstallScope.perMachine)
    .SetAppVersion("1.1.0")
    .SetProductId(Guid.Parse("f9af10ed-ea36-424b-973e-a0e651203370"))
    .SetUpgradeCode(Guid.Parse("e74fb99c-5bf1-454d-802a-c7c6f9cc219f"))
    .SetControlPanelInfo(productInfo =>
    {
        productInfo.Manufacturer = "YourCompany";
        productInfo.ProductIcon = @"C:\Temp\net472\logo.ico";
        productInfo.Comments = "My awesome app blah blah blah";
        productInfo.Contact = "support@yourcompany.com";
        productInfo.HelpLink = "https://www.yourcompany.com/support";
        productInfo.UrlInfoAbout = "https://www.yourcompany.com/about";
    })
    .SetCompressionLevel(CompressionLevel.none)
    .SetOutput(
        outputPath: @"C:\Temp\",
        msiFilename: "MyInstaller");

var (success, path, message) = builder.BuildMsi();
if (success)
    Console.WriteLine($"Installer created at {path}");
else
    Console.WriteLine($"Error building installer: {message}");
```