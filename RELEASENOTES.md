# MsiBuilderUI v1.0.0 Release Notes

First branded, versioned release of the **MsiBuilderUI** desktop application — the Avalonia front-end
for the MsiBuilder library. (The `MsiBuilder` WixSharp NuGet library is versioned and released
separately; this note covers the app only.)

## New Features

- **First public release** of the MsiBuilderUI desktop app, versioned `1.0.0`.
- **Application icon** — a multi-resolution (16/32/48/256 px) app icon based on the Phosphor
  "Package" glyph on a Carbon-blue tile, shown in Explorer and in the app's title bar / taskbar.
- **Carbon-based UI** — the redesigned interface built on Carbon.Avalonia.Desktop (SettingsCards,
  icons, InfoBar, theme toggle), delivered in FEATURE-004.
- **MSI installer profile** — a ready-to-load build profile
  (`msiProfiles/MsiBuilderUI.msiprofile.1.0.0.json`) that the app itself can open to produce a
  Windows Installer package for MsiBuilderUI.

## Compatibility

- **Windows only.** The app builds and packages Windows Installer (MSI) output.
- **Requires the [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0).**
  This release ships **framework-dependent** — the target machine must have the .NET 10 Desktop
  Runtime installed. (A self-contained build that removes this prerequisite is under consideration
  for a future release.)
- The bundled **net472 worker** (which invokes WixSharp) requires **.NET Framework 4.7.2**, which is
  built into supported versions of Windows.

## Version

- **1.0.0**
