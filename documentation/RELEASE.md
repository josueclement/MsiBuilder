# Release runbook — MsiBuilderUI (app)

Reusable checklist for cutting a new version of the **MsiBuilderUI** desktop app. MsiBuilderUI is an
app, not a NuGet package, so there are **no `dotnet pack` / `dotnet nuget push` steps** — the
deliverable is the app build plus a Windows Installer (MSI) produced from its build profile.

> The **MsiBuilder** WixSharp library in this repo is published to NuGet on its **own** cadence with
> **bare** `X.Y.Z` tags. This runbook is for the app only; its tags are namespaced `msibuilderui/X.Y.Z`
> so the two never collide.

Replace `X.Y.Z` with the version being released (e.g. `1.0.0`) throughout. The app version lives in
`src/MsiBuilderUI/MsiBuilderUI.csproj` (`<Version>`).

## 1. Pre-release checks

Run from the repository root, on the branch that will be merged:

- [ ] `<Version>X.Y.Z</Version>` set in `src/MsiBuilderUI/MsiBuilderUI.csproj`.
- [ ] `RELEASENOTES.md` has a top `MsiBuilderUI vX.Y.Z` section (newest-first), documenting the
      **.NET 10 Desktop Runtime** prerequisite.
- [ ] `src/MsiBuilderUI/Assets/MsiBuilderUI.ico` exists and is multi-resolution;
      `<ApplicationIcon>` and the `Window.Icon` reference it.
- [ ] `msiProfiles/MsiBuilderUI.msiprofile.X.Y.Z.json` exists, with the **same `upgradeCode`** as prior
      versions and a **new `productId`** for this version.
- [ ] Clean, warning-free build (`WarningsAsErrors=nullable`):
      ```bash
      dotnet build MsiBuilder.slnx -c Release
      ```
- [ ] Full test suite green:
      ```bash
      dotnet test MsiBuilder.slnx -c Release
      ```
      > The net472 worker and the Skia-backed UI tests are **Windows-only** — run the build/test
      > pre-flight (and a visual smoke test of the app) on Windows.

## 2. Merge to the default branch

Merge the release branch into the default (published) branch — `main` — then check it out locally:

```bash
git switch main
git pull
```

## 3. Tag the release

App tags are **namespaced** (the library uses bare `X.Y.Z`). Tag the merge commit and push:

```bash
git tag msibuilderui/X.Y.Z
git push origin msibuilderui/X.Y.Z
```

## 4. Build the MSI (Windows)

The app builds its own installer by loading the committed profile:

1. `dotnet build MsiBuilder.slnx -c Release` (populates `src\MsiBuilderUI\bin\Release\net10.0`, the
   profile's `releasePath`).
2. Launch MsiBuilderUI and **load** `msiProfiles/MsiBuilderUI.msiprofile.X.Y.Z.json`.
3. Build the MSI; it is written to the profile's `outputPath` (`artifacts`) as
   `MsiBuilderUI.msi`.

> The profile's committed paths are **repo-relative**; substitute absolute paths when building from a
> different working directory. Because the release is **framework-dependent**, the target machine needs
> the **.NET 10 Desktop Runtime** installed.

## 5. Post-release verification

- [ ] `MsiBuilderUI.exe` in the Release output shows the app icon in Explorer, and the running app
      shows it in the title bar / taskbar.
- [ ] The generated `MsiBuilderUI.msi` installs, creates the Desktop + Start-menu shortcuts, and the
      installed app launches.
- [ ] The `msibuilderui/X.Y.Z` tag is present and its notes match `RELEASENOTES.md`.
