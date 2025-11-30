using System;
using System.IO;
using MsiBuilder;
using WixSharp;

namespace ConsoleApp1;

static class Program
{
    static void Main()
    {
        var builder = new WixMsiBuilder("CameraCalibration")
            .SetInstallDir(
                installPath: Path.Combine("%ProgramFiles%", "CameraCalibration"),
                releasePath: @"C:\Temp\net472\")
            .AddShortcut(
                shortcutPath: "%Desktop%",
                shortcutName: "My Camera Calibration D",
                targetPath: Path.Combine("[INSTALLDIR]", "CameraCalibration.exe"),
                iconPath: @"C:\Temp\net472\logo.ico")
            .AddShortcut(
                shortcutPath: "%ProgramMenu%",
                shortcutName: "My Camera Calibration M",
                targetPath: Path.Combine("[INSTALLDIR]", "CameraCalibration.exe"),
                iconPath: @"C:\Temp\net472\logo.ico")
            .SetInstallScope(InstallScope.perMachine)
            .SetAppVersion("1.1.0")
            .SetProductId(Guid.Parse("f9af10ed-ea36-424b-973e-a0e651203370"))
            .SetUpgradeCode(Guid.Parse("e74fb99c-5bf1-454d-802a-c7c6f9cc219f"))
            .SetControlPanelInfo(productInfo =>
            {
                productInfo.Manufacturer = "JCL";
                productInfo.ProductIcon = @"C:\Temp\net472\logo.ico";
                productInfo.Comments = "Camera Calibration tool blah blah";
                productInfo.Contact = "support@yourcompany.com";
                productInfo.HelpLink = "https://www.yourcompany.com/support";
                productInfo.UrlInfoAbout = "https://www.yourcompany.com/about";
            })
            .SetCompressionLevel(CompressionLevel.low)
            .SetOutput(
                outputPath: @"C:\Temp\", 
                msiFilename: "MyInstaller");
        
        var (success, path, message) = builder.BuildMsi();
        if (success)
            Console.WriteLine($"Installer created at {path}");
        else
            Console.WriteLine($"Error building installer: {message}");
        
    }
}