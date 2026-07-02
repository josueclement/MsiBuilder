# FEATURE-004 — Improve the Avalonia UI with Carbon.Avalonia.Desktop

**Status:** DONE

| Phase   | Title                                                      | Status |
|---------|------------------------------------------------------------|--------|
| PHASE01 | Carbon foundation (package, theme, services, picker swap)  | DONE   |
| PHASE02 | Carbon UI redesign (SettingsCards, icons, InfoBar, toggle) | DONE   |

## Objective

Upgrade the look and UX of `MsiBuilderUI` by adopting the **Carbon.Avalonia.Desktop** control
library (Fluent-based, Dark + Light), without changing what the app does or its data contracts.
The seven hand-rolled `Border.card` sections become Carbon `SettingsCardExpander`s with icons; build
results surface through a Carbon `InfoBar`; file/folder picking routes through Carbon's picker
services; a dark/light theme toggle is added.

**Enabler:** the app already ships the stack Carbon expects — Avalonia **12.0.5** (Carbon needs
≥12.0.2, satisfied, so **no Avalonia upgrade**), .NET 10, `CommunityToolkit.Mvvm`, and a full
`Microsoft.Extensions.Hosting` + DI bootstrap (`App(IServiceProvider)`).

## Decisions

1. **Scope = single-form "card settings."** Keep the one scrolling form; convert each section to a
   `SettingsCardExpander`, preserving its responsive inner field layout. No NavigationView / page split.
2. **Build feedback = InfoBar + keep log.** Result shown via Carbon `InfoBar` (Success/Error); the
   streaming build log and indeterminate progress bar stay. No modal Overlay (would hide the log).
3. **File pickers = swap to Carbon**, but keep the neutral `IStoragePickerService` interface —
   reimplement it as `CarbonStoragePickerService` delegating to Carbon's picker services, so the
   ViewModels and every Browse command are untouched.
4. **Icons = cards + key buttons** (Phosphor, transitive via Carbon).
5. **Theme toggle = yes**, initial app theme **Dark**; implemented as a view concern (top-bar
   `ToggleSwitch` + a small handler in code-behind) — no ViewModel/theme service, no VM test churn.
   The Dark default and the toggle are coupled theme-control changes and both land in **PHASE02**;
   PHASE01 keeps `RequestedThemeVariant="Default"` so the foundation phase stays behavior-neutral
   (Carbon re-skins the controls but the app still follows the OS light/dark setting).
6. **Delivery = two phases** with a working checkpoint after PHASE01.
7. **Only the three used Carbon services are registered** (`IFileDialogService`,
   `IFolderDialogService`, `IInfoBarService`) — not the full six — so no unused hosts are needed.
8. **No changes** to `MsiBuilder.Contracts`, the serializer, `IProfileService`, the worker, or the
   build pipeline; profiles stay backward-compatible. New VM members match the existing style
   (`ObservableObject` + `SetProperty(ref field, …)` + manual `RelayCommand`/`AsyncRelayCommand`).

## PHASE01 — Carbon foundation

Adopt the package, theme, and services with a verified checkpoint: same layout, Carbon-themed,
pickers routed through Carbon.

### Changes
- `src/MsiBuilderUI/MsiBuilderUI.csproj` — add `Carbon.Avalonia.Desktop` `0.2.0`.
- `src/MsiBuilderUI/App.axaml` — add the Carbon `ResourceInclude`
  (`avares://Carbon.Avalonia.Desktop/Themes/Fluent.axaml`) inside `Application.Resources`; keep
  `<FluentTheme/>` and leave `RequestedThemeVariant="Default"` (the Dark default moves to PHASE02
  with the toggle, so this phase changes no runtime behavior beyond the control re-skin).
- `src/MsiBuilderUI/Program.cs` — register `IFileDialogService→FileDialogService` and
  `IFolderDialogService→FolderDialogService` (singletons); switch the `IStoragePickerService`
  registration to `CarbonStoragePickerService`.
- `src/MsiBuilderUI/App.axaml.cs` — in `OnFrameworkInitializationCompleted`, after resolving
  `MainWindow`, call `SetStorageProvider(mainWindow.StorageProvider)` on both picker services.
- `src/MsiBuilderUI/Services/CarbonStoragePickerService.cs` — **new**; implements
  `IStoragePickerService` over Carbon's `IFileDialogService`/`IFolderDialogService` (string-path
  overloads), mapping the existing three methods 1:1.
- `src/MsiBuilderUI/Services/AvaloniaStoragePickerService.cs` — **deleted** (replaced).

### Acceptance criteria
- Solution builds with **zero warnings**; all existing tests pass unchanged.
- App launches Carbon-themed (following the OS light/dark setting) with the current card layout;
  Browse / Save profile / Load profile dialogs work through Carbon's pickers.

## PHASE02 — Carbon UI redesign

### Changes
- `src/MsiBuilderUI/Program.cs` — register `IInfoBarService→InfoBarService` (singleton).
- `src/MsiBuilderUI/App.axaml` — set `RequestedThemeVariant="Dark"` as the initial theme (paired
  with the new toggle below).
- `src/MsiBuilderUI/App.axaml.cs` — `RegisterHost(mainWindow.HostInfoBar)` for the InfoBar service.
- `src/MsiBuilderUI/Views/MainWindow.axaml` — add carbon/infoBar/pia xmlns; wrap the root in a
  `Panel` with an `<infoBar:InfoBar x:Name="HostInfoBar"/>` sibling; convert the seven `Border.card`
  sections to `SettingsCardExpander` (Header/Description/IconData, keeping inner field layout); add
  icons to Build/Browse/Generate/Add/Remove buttons; add a top-bar dark/light `ToggleSwitch`; drop
  the redundant bottom `StatusMessage` `TextBlock` (InfoBar replaces it); keep the progress bar + log.
- `src/MsiBuilderUI/Views/MainWindow.axaml.cs` — theme-toggle handler setting
  `Application.Current.RequestedThemeVariant` (Dark/Light).
- `src/MsiBuilderUI/ViewModels/MainWindowViewModel.cs` — inject `IInfoBarService`; replace the
  synchronous `SetStatus(...)` with `ShowResult(...)` that sets `StatusMessage`/`HasResult`/
  `LastBuildSucceeded` and **fire-and-forgets** the InfoBar (Success/Error) via a `ShowInfoBarAsync`
  helper. Carbon's `InfoBar.ShowAsync` completes only on dismissal, so awaiting it would keep the
  build "in progress" until the user closed the bar (caught by the PHASE02 review — see Notes).
- `tests/MsiBuilderUI.Tests/MainWindowViewModelTests.cs` — substitute `IInfoBarService` in the
  `CreateVm` factory; add InfoBar-severity assertions for build success and failure/invalid-GUID.

### Acceptance criteria
- Builds with **zero warnings**; all tests (existing + new InfoBar tests) pass.
- Sections render as `SettingsCardExpander`s with a leading icon; short fields still reflow 2-up→1-up.
- Dark/light toggle switches the whole app live.
- Build/profile results appear in the `InfoBar` while the progress bar and streaming log remain;
  profiles still round-trip; pickers still work.

## Verification

1. `dotnet build MsiBuilder.slnx` → zero warnings.
2. `dotnet test` → all tests green.
3. `dotnet run --project src/MsiBuilderUI` → confirm Carbon dark theme, `SettingsCardExpander`
   sections with icons, responsive reflow, live theme toggle, working Browse/Generate/Add/Remove,
   Build shows progress + streams the log + ends with a Success/Error InfoBar, and profile save/load
   round-trips.

## Notes
- Pre-existing CRLF↔LF working-tree noise (45 files, zero content change) was parked in a git stash
  to get a clean tree before branching (no `.gitattributes` in the repo — a suggested follow-up).
- PHASE01 review: kept `RequestedThemeVariant="Default"` in PHASE01 (behavior-neutral) and moved the
  Dark default into PHASE02 with the toggle.
- PHASE02 review (decompiled Carbon 0.2.0): `InfoBar.ShowAsync` completes only on user dismissal, so
  the initial `await` would have kept `IsBuilding`/commands blocked until the bar was closed. Fixed by
  making the notification fire-and-forget; guarded by the `Build_DoesNotWaitForInfoBarDismissal` test.
- Environment: the net472 `MsiBuilder.Worker.Tests` need `mono` to run on Linux (absent here) and the
  Avalonia GUI needs `libfontconfig` for Skia (absent here), so visual verification is done on Windows.
