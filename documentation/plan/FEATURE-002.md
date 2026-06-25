# FEATURE-002 — Avalonia MSI Builder UI

**Status:** DONE

## Objective

Deliver a desktop GUI that drives the existing `MsiBuilder` fluent API (see `README.md` /
`src/ConsoleApp1`) to produce Windows Installer (`.msi`) packages. The user fills a form — including
free-text ProductId / UpgradeCode GUIDs each with a "generate" button — and builds an MSI.

## Key constraint & chosen architecture

`MsiBuilder` targets **`net472`** and its WixSharp `ManagedProject`/`ManagedUI` build path is only stable
on .NET Framework. Avalonia 12 dropped `net4x`/`netstandard2.0` (min .NET 8), and a `net10` project
**cannot** project-reference a `net472` one (NuGet restore fails, NU1201).

Resolution chosen by the customer: **out-of-process**.

- The UI is fully House-Convention compliant: **Avalonia 12 / net10 / `IHost` / explicit MVVM**.
- A small **`net472` worker exe** holds the actual reference to `MsiBuilder` and runs the build.
- A shared **`netstandard2.0` contracts library** (the only TFM both `net10` and `net472` consume) carries
  the request/result DTOs so they aren't duplicated, and doubles as the Save/Load profile format.

**Runtime is Windows-only** (WixSharp + MSI + `net472`). WSL/Linux is the dev shell; full build of the
`net472` projects and actual MSI generation require Windows + the `wix` global tool.

## Scope

In scope: three new production projects + two test projects (below), wired into `MsiBuilder.slnx`.
Out of scope: any change to `src/MsiBuilder` or `src/ConsoleApp1` (both stay as-is).

```
src/MsiBuilder.Contracts/  netstandard2.0   DTOs + mirror enums + JSON serializer
src/MsiBuilder.Worker/     net472           console exe; refs MsiBuilder + Contracts
src/MsiBuilderUI/          net10 / Avalonia 12   GUI; refs Contracts only (NOT MsiBuilder)
tests/MsiBuilderUI.Tests/        net10      VM logic, validation, contracts round-trip
tests/MsiBuilder.Worker.Tests/   net472     DTO→WixSharp mapping helpers
```

All new projects: `Nullable=enable`, `LangVersion=14`, `ImplicitUsings=disable`,
`WarningsAsErrors=nullable`.

## Customer-confirmed decisions

- Out-of-process architecture (UI ⇒ worker via JSON request/result files; UI does not reference MsiBuilder).
- Shortcuts: **dynamic add/remove list**.
- Path inputs: **textbox + Browse button** (Avalonia `StorageProvider`).
- **Expose ManagedUI**: WUI selection + Install/Modify dialog checklists.
- **Save/Load JSON profiles** (profile == serialized `MsiBuildRequest`).
- **Live build-log panel** + final result banner.

## PHASE01 — Contracts library (`src/MsiBuilder.Contracts`, netstandard2.0)

**Status:** IN PROGRESS

- DTOs: `MsiBuildRequest`, `ShortcutDto`, `ManagedUiDto`, `MsiBuildResult`.
- Mirror enums (WixSharp-free): `InstallScopeOption`, `CompressionLevelOption`, `WuiOption`, `DialogOption`.
- `MsiContractSerializer` — shared `System.Text.Json` wrapper (camelCase, `JsonStringEnumConverter`,
  indented); identical options on both sides. `System.Text.Json` package ref (needed on
  netstandard2.0/net472).
- Round-trip tests live in `tests/MsiBuilderUI.Tests` (net10 consumes netstandard2.0).

## PHASE02 — Worker exe (`src/MsiBuilder.Worker`, net472)

**Status:** TODO

- CLI: `MsiBuilder.Worker --request <req.json> --result <res.json>`; exit `0` success / `1` build-failure /
  `2` bad-args/exception; WixSharp console output flows to stdout/stderr (the UI's live log).
- `BuilderConfigurator` maps `MsiBuildRequest` → the `WixMsiBuilder` chain from the README, then
  `BuildMsi()`. Optional ControlPanelInfo fields skipped when null/empty; `ManagedUi == null` ⇒ builder
  default UI.
- Pure mapping helpers (net472-unit-tested): `MapScope`, `MapCompression`, `MapWui`, `MapDialog(DialogOption)
  → WixSharp `Dialogs.X` type. **Pin exact WixSharp 2.12 member names** (esp. `License`/`Licence`) during
  implementation. Unknown enum ⇒ throw; bad GUID ⇒ failure result.
- **Deviation:** no `IHost` — stateless one-shot CLI adapter (justified; UI uses `IHost` fully).

## PHASE03 — Avalonia UI (`src/MsiBuilderUI`, net10 / Avalonia 12)

**Status:** DONE

- Bootstrapping: `IHost` first (`Host.CreateApplicationBuilder`) → register services/VMs →
  `BuildAvaloniaApp(host.Services).StartWithClassicDesktopLifetime`; `App` gets `IServiceProvider` via
  `.AfterSetup`, resolves `MainWindow` in `OnFrameworkInitializationCompleted`. No service instantiation in
  code-behind.
- DI: `MainWindowViewModel`; `IMsiBuildService → WorkerMsiBuildService`; `IStoragePickerService →
  AvaloniaStoragePickerService`; `IProfileService → ProfileService`; `WorkerOptions` via `IOptions<T>`.
- MVVM (CommunityToolkit.Mvvm, explicit `field` + `SetProperty`, commands as get-only `…Command` props, no
  source generators): `MainWindowViewModel`, `ShortcutViewModel`, `DialogSelectionViewModel`. Generate-GUID
  commands, add/remove shortcut, browse commands, `BuildCommand` (`CanExecute = IsValid && !IsBuilding`),
  Save/Load profile. `ToRequest()` / `LoadFrom()`.
- Views: scrollable `MainWindow.axaml` (General · Install dir · Identity w/ Generate buttons · Control-Panel
  info · Shortcuts · Managed UI · Output · Compression · Build + busy · live log · result banner; File menu
  Save/Load Profile).
- Build-order + worker copy: UI csproj references the worker with `ReferenceOutputAssembly="false"
  SkipGetTargetFrameworkProperties="true"` + an `AfterBuild` target copying worker output into
  `$(OutDir)worker\`. `WorkerOptions.WorkerPath` is the dev override.
- Tests (`tests/MsiBuilderUI.Tests`, net10): VM validation, Generate*, add/remove, ToRequest/LoadFrom
  round-trip, BuildCommand gating, build delegates to mocked `IMsiBuildService`. Mock the three services.

## Tests (xUnit v3) — summary

- `tests/MsiBuilderUI.Tests` (net10): contracts round-trip + VM logic (mocked services).
- `tests/MsiBuilder.Worker.Tests` (net472): `BuilderConfigurator` mapping helpers. Real `BuildMsi()` is
  integration-only (Windows + wix), covered by manual e2e.
- Mocking lib: **NSubstitute**.

## Acceptance criteria

- [x] All seven projects build with 0 warnings / 0 errors via `dotnet build MsiBuilder.slnx` (net472 projects
      build on this WSL box too); all new projects listed in `MsiBuilder.slnx`.
- [x] `src/MsiBuilder` and `src/ConsoleApp1` unchanged.
- [x] UI references only `MsiBuilder.Contracts`; worker references `MsiBuilder` + `MsiBuilder.Contracts`.
- [x] Worker is built and copied to `MsiBuilderUI/bin/.../worker/MsiBuilder.Worker.exe` by the build.
- [x] `MsiBuilderUI.Tests` green: 12 passing (contracts round-trip + ViewModel logic). `MsiBuilder.Worker.Tests`
      compiles (validates every WixSharp mapping); runs on Windows (no Mono on the dev shell).
- [ ] **Manual e2e (Windows + wix tool, user):** fill the README example values → Build → `.msi` at the output
      path, log streams, success banner; Save then Load profile round-trips; Generate buttons emit fresh GUIDs.

## Deviations from House Conventions (explicit)

1. UI does not directly reference `MsiBuilder` — by design (out-of-process); the worker holds the reference.
2. Worker has no `IHost` — stateless one-shot CLI adapter.
3. Whole runtime stack is Windows-only (WixSharp/MSI + net472).
