# FEATURE-005 — Release MsiBuilderUI v1.0.0 (icon, metadata, MSI profile)

**Status:** DONE

Single-phase work item. Cuts the first branded, versioned release of the **MsiBuilderUI** desktop
app using the `dotnet-release` skill's app path. Planned by `/interview`; implement with
`/build FEATURE-005`.

## Objective

Ship a distributable v1.0.0 build of the `MsiBuilderUI` Avalonia app (net10.0 `WinExe`): add an app
icon, release metadata, release notes, and a WixSharp **MSI installer profile**. The app currently
has no `<Version>`, `<Authors>`, or `<ApplicationIcon>`, no `.ico`, no `RELEASENOTES.md`, and no
installer profile.

**Scope = the app only.** The repo also ships the **MsiBuilder** WixSharp NuGet library
(`PackageId=MsiBuilder`, v1.0.0 — the existing bare `1.0.0` git tag is the library's). The library
and its published package are **untouched** by this release.

**Enabler / key finding:** the app's on-disk build-profile format (`MsiBuildRequest` in
`src/MsiBuilder.Contracts`, serialized by `MsiContractSerializer` with **camelCase property names +
string enum values**) is byte-compatible with the `dotnet-release` skill's `msiprofile.template.json`.
So the profile we generate is directly loadable by MsiBuilderUI — the app builds its own installer.

## Decisions

1. **Scope = MsiBuilderUI only**; MsiBuilder library left as-is (no NuGet republish).
2. **Version = 1.0.0** (first app release).
3. **Git tag = `msibuilderui/1.0.0`** — namespaced so it doesn't collide with the library's bare
   `1.0.0` tag; library tags stay bare. The skill *prints* `git tag`/push; the user runs them.
4. **Icon = generated from the Phosphor `Package` glyph** (white on a Carbon-blue rounded tile),
   multi-resolution `.ico` (16/32/48/256 px). Fallback: a simple generated monogram tile if the
   Phosphor SVG can't be obtained offline. Phosphor is MIT-licensed (compatible).
5. **Payload = framework-dependent** — the profile packages `src\MsiBuilderUI\bin\Release\net10.0`;
   the target machine needs the **.NET 10 Desktop Runtime**. Documented in RELEASENOTES + runbook so
   end-users aren't surprised. (Self-contained deferred to a future release — see Follow-ups.)
6. **Author/manufacturer = Josué Clément** (mirrors the library csproj).
7. **App release skips `dotnet pack`/`nuget push`** — it is not a NuGet package.

## Changes

1. **App icon** — new `src/MsiBuilderUI/Assets/MsiBuilderUI.ico` (create `Assets/`), multi-res, from
   the Phosphor `Package` glyph. Rasterize glyph → PNG sizes → assemble with ImageMagick
   (`magick`/`convert`). Record the path actually used (glyph vs monogram fallback) in the
   completion doc.
2. **`src/MsiBuilderUI/MsiBuilderUI.csproj`** — add `<Version>1.0.0</Version>`,
   `<Authors>Josué Clément</Authors>`, `<Copyright>Copyright © 2025 Josué Clément</Copyright>`,
   `<ApplicationIcon>Assets/MsiBuilderUI.ico</ApplicationIcon>`. Do **not** add `PackageId`/
   `GeneratePackageOnBuild`.
3. **Window/taskbar icon** — wire the icon as the Avalonia `Window.Icon` (branding for the running
   app), via `App.axaml(.cs)` / `Views/MainWindow.axaml`.
4. **`RELEASENOTES.md`** (new, repo root) — top section `# MsiBuilderUI v1.0.0 Release Notes` with
   *New Features* (first branded/versioned release; app icon; Carbon UI from FEATURE-004; MSI
   installer profile), *Compatibility* (Windows; requires the **.NET 10 Desktop Runtime**; the
   net472 worker needs .NET Framework 4.7.2, built into Windows), *Version* (1.0.0).
5. **`README.md`** (root) — light touch: add a short "Desktop application (MsiBuilderUI)" note
   linking to `RELEASENOTES.md`. **No** app NuGet badge (app isn't on NuGet); library content and
   its `PackageReadmeFile` role stay intact. Final call runs through the build-flow doc-freshness
   sweep.
6. **`msiProfiles/MsiBuilderUI.msiprofile.1.0.0.json`** (new dir, committed) — from
   `templates/msiprofile.template.json`:
   - `upgradeCode`: generated once (stable identity, reused verbatim on all future versions).
   - `productId`: new GUID for this version.
   - GUIDs generated locally at build (`uuidgen` / `cat /proc/sys/kernel/random/uuid`) — never
     fabricated. **Record `upgradeCode` in the completion doc** so future releases reuse it.
   - `appName=MsiBuilderUI`, `installPath=%ProgramFiles%\MsiBuilderUI`,
     `releasePath=src\MsiBuilderUI\bin\Release\net10.0`, `scope=PerMachine`, `version=1.0.0`,
     `manufacturer=Josué Clément`, `productIcon=src\MsiBuilderUI\Assets\MsiBuilderUI.ico`,
     `compression=High`, `outputPath=artifacts`, `msiFilename=MsiBuilderUI`, shortcuts on
     `%Desktop%` + `%ProgramMenu%` → `[INSTALLDIR]\MsiBuilderUI.exe`.
   - Enum values (`PerMachine`, `High`) are strings → loadable by MsiBuilderUI. Committed paths are
     repo-relative; the user may substitute absolute paths when building elsewhere.
7. **`documentation/RELEASE.md`** (offer, create-if-missing) — the skill's runbook template trimmed
   for an app (no pack/push).

## Reuse (do not duplicate)

`MsiBuildRequest`, `ShortcutDto`, `MirrorEnums`, `MsiContractSerializer` in
`src/MsiBuilder.Contracts/` define the exact schema the profile JSON must match.

## Printed runbook (build prints — user runs; nothing outward-facing is executed)

```
dotnet build MsiBuilder.slnx -c Release
dotnet test  MsiBuilder.slnx -c Release
git switch main && git pull
git tag msibuilderui/1.0.0 && git push origin msibuilderui/1.0.0
# then build MsiBuilderUI.msi by loading msiProfiles/MsiBuilderUI.msiprofile.1.0.0.json in the app
```

## Acceptance criteria (verified at `/build`, on Windows)

- `dotnet build MsiBuilder.slnx -c Release` succeeds with **zero warnings** (`WarningsAsErrors=nullable`).
- `MsiBuilderUI.exe` embeds the icon (visible in Explorer); the running app shows it in the taskbar.
- csproj declares `Version=1.0.0`, `Authors`, `ApplicationIcon`; `Assets/MsiBuilderUI.ico` is multi-res.
- `RELEASENOTES.md` has the v1.0.0 section documenting the .NET 10 runtime prerequisite.
- `msiProfiles/MsiBuilderUI.msiprofile.1.0.0.json` is valid JSON, round-trips through
  `MsiContractSerializer` into `MsiBuildRequest`, and carries generated `upgradeCode`/`productId` GUIDs.
- Existing test suite unchanged/passing where the platform allows (net472 worker tests + Skia are
  Windows-only — verify build + visuals on Windows, as with FEATURE-004).
- `documentation/done/FEATURE-005.md` completion record written; runbook printed.

## Follow-ups (out of scope)

- Add `.gitattributes` (`* text=auto eol=lf`) — repo has none, and FEATURE-004 parked CRLF churn in a
  stash. Recommendation only.
- Consider a self-contained build in a future release to drop the .NET 10 runtime prerequisite.
