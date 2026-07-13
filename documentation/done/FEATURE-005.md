# FEATURE-005 — Release MsiBuilderUI v1.0.0 (icon, metadata, MSI profile)

**Status:** DONE

Single-phase work item. First branded, versioned release of the **MsiBuilderUI** Avalonia app, cut via
the `dotnet-release` skill's app path. Scope was the app only — the `MsiBuilder` WixSharp library and its
NuGet package were left untouched.

## Summary

- Added a multi-resolution **application icon** and wired it as both the Win32 exe icon
  (`<ApplicationIcon>`) and the runtime **Window/taskbar icon**.
- Added **release metadata** to the app csproj (`<Version>1.0.0>`, `<Authors>`, `<Copyright>`).
- Wrote **`RELEASENOTES.md`** (v1.0.0 section) and an app-focused **`documentation/RELEASE.md`** runbook.
- Generated the committed **MSI build profile** `msiProfiles/MsiBuilderUI.msiprofile.1.0.0.json`, verified
  to round-trip through the app's own `MsiContractSerializer` into `MsiBuildRequest`.

## GUIDs (record for future releases)

| Field | Value | Rule |
|-------|-------|------|
| `upgradeCode` | `02647720-D980-48A9-BD11-F017A9D1BE56` | **Stable app identity — reuse verbatim in every future MsiBuilderUI profile.** |
| `productId` (v1.0.0) | `744B3EC3-6A5B-475A-9F83-E6F52B7D967C` | New GUID per version — do not reuse for v1.0.1+. |

## Files/modules touched

**Created**
- `src/MsiBuilderUI/Assets/MsiBuilderUI.ico` — multi-res (16/32/48/256 px) icon; white Phosphor
  "Package" glyph on a Carbon Blue 60 (`#0F62FE`) rounded tile.
- `RELEASENOTES.md` — MsiBuilderUI v1.0.0 release notes (New Features · Compatibility · Version).
- `documentation/RELEASE.md` — app release runbook (no pack/push; MSI built by loading the profile).
- `msiProfiles/MsiBuilderUI.msiprofile.1.0.0.json` — the committed MSI build profile.

**Modified**
- `src/MsiBuilderUI/MsiBuilderUI.csproj` — added `<Version>`, `<Authors>`, `<Copyright>`,
  `<ApplicationIcon>`, and an `<AvaloniaResource Include="Assets/**" />` item.
- `src/MsiBuilderUI/Views/MainWindow.axaml` — added `Icon="/Assets/MsiBuilderUI.ico"`.
- `documentation/roadmap.md`, `documentation/plan/FEATURE-005.md` — status flips (TODO → IN PROGRESS → DONE).

## Deviations & follow-ups

- **Icon toolchain (deviation from plan).** The plan's recipe assumed ImageMagick; this environment has
  no rasterizer (no ImageMagick/Pillow/rsvg/inkscape/icotool). Per the user's decision, the `.ico` was
  instead generated with a throwaway **SkiaSharp** program (cached locally): it parses the MIT-licensed
  Phosphor "Package" SVG path, renders the glyph on the Carbon-blue tile at each size, and packs the four
  PNG frames into an ICO container. The generator lives in the session scratchpad (not committed); only
  the resulting `.ico` is in the repo. The exact Phosphor "Package" glyph was used (not the monogram
  fallback).
- **README handled via the doc-freshness sweep.** Per the plan ("Final call runs through the build-flow
  doc-freshness sweep"), the README "Desktop application (MsiBuilderUI)" note was decided during the
  sweep rather than pre-applied. See the commit message for whether it was included.
- **net472 worker tests not executed here (platform).** `MsiBuilder.Worker.Tests` (net472) build clean but
  cannot run on this Linux/WSL box (no Mono host); they are unchanged by this dev and must be run on
  Windows. All net10 `MsiBuilderUI.Tests` pass (15/15).
- **Windows-only visual checks pending.** "`MsiBuilderUI.exe` embeds the icon (Explorer)" and the taskbar
  icon are Windows-verified — the Linux build emits `MsiBuilderUI.dll`, not the Windows apphost. Build +
  XAML compile of the icon wiring succeeded here.
- **CRLF (recommendation only).** The working tree carried pre-existing whole-file CRLF churn (100% EOL
  diff, no content change) before this dev, and the repo has no `.gitattributes`. Recommend adding
  `.gitattributes` with `* text=auto eol=lf` and running `git add --renormalize .` in a separate change.
  Not touched by this dev per house rules.
- **Self-contained build (future).** This release is framework-dependent (needs the .NET 10 Desktop
  Runtime). A self-contained build to drop that prerequisite is deferred to a future release.

## Build/test evidence

- `dotnet build MsiBuilder.slnx -c Release` → **Build succeeded. 0 Warning(s), 0 Error(s)** (full
  solution, including the net472 worker; `WarningsAsErrors=nullable`).
- `dotnet test MsiBuilder.slnx -c Release` → `MsiBuilderUI.Tests` **Passed! 15/15** (0 failed, 0 skipped).
  `MsiBuilder.Worker.Tests` (net472) aborted with "Could not find 'mono' host" — a platform limitation,
  not a failure; run on Windows.
- MSI profile round-trip: a throwaway net10 program using the repo's `MsiContractSerializer` deserialized
  `MsiBuilderUI.msiprofile.1.0.0.json` into `MsiBuildRequest` with all fields correct (`Scope=PerMachine`,
  `Compression=High`, both GUIDs parse) and re-serialized stably.
- `.ico` validated: `file` reports "MS Windows icon resource - 4 icons"; container holds 16/32/48/256 PNG
  frames at 32-bit RGBA.
