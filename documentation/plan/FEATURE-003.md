# FEATURE-003 — Main form layout redesign (cards + responsive paired fields)

**Status:** DONE

## Objective

Improve the main window (`src/MsiBuilderUI/Views/MainWindow.axaml`) layout. The form currently
renders every input as a single vertical column of `160px label + full-width control` rows, so
short values (Version, Install scope, Compression, GUIDs, MSI name…) stretch across the whole
window, wasting horizontal space and making the form needlessly tall.

Restructure the scalar sections into bordered "cards", pair short fields side-by-side, and make
the field arrangement responsive to window width. **No data, binding, ViewModel, or behavior
changes** — pure XAML layout/styling.

## Decisions

1. **Layout:** Cards + paired fields. Each section becomes a bordered card; short fields sit
   side-by-side; long paths/URLs stay full width.
2. **Responsiveness:** Responsive reflow + wider default window. Short fields reflow 2-up on a
   wide window, 1-up when narrow. Default window widened (820 → 980).
3. **Scope:** Scalar sections only (General, Install directory, Identity, Control Panel info,
   Output). Shortcuts and Managed UI keep their internal structure — only wrapped in the new
   card styling for visual consistency.

## Design

### Responsive mechanism (no code-behind)
Within each card, short fields go inside a `WrapPanel`. Each field is a fixed-width "cell"
(`Grid Classes="field"`, `Width=430`, internal `ColumnDefinitions="130,*"`). The WrapPanel
reflows cells based on available width: 2 per row at the default ~980px window, 1 per row when
narrowed. Long fields (paths, URLs, GUIDs with buttons) remain full-width `Grid Classes="row"`
rows spanning the card.

### Styles (`Window.Styles`) and resources (`Window.Resources`)
- `CardBorder` brush resource (`#33808080`) — shared by cards, build log, build bar (replaces
  the inline hardcoded color).
- `Border.card` — `BorderThickness=1`, `CornerRadius=6`, `Padding=12`, `Margin=0,0,0,12`.
- `Grid.field` — short-field cell: `Width=430`, `Margin=0,3,16,3`.
- `TextBlock.section` — card header (`Margin=0,0,0,8`).
- `Grid.row`, `TextBlock.label` — kept; label column standardized 160 → 130.

### Per-card field assignment
- **General:** cells → App name, Version, Install scope.
- **Install directory:** full-width rows → Install path; Release folder (+ Browse).
- **Identity:** full-width rows → Product Id (+ Generate); Upgrade code (+ Generate).
- **Control Panel info:** cells → Manufacturer, Contact, Comments; full-width rows → Product
  icon (+ Browse), Help link, About URL.
- **Output:** cells → Compression, MSI file name; full-width row → Output folder (+ Browse).
- **Shortcuts:** card wrapper only; `ItemsControl` + per-shortcut template internals unchanged;
  shortcut item border re-skinned to `CardBorder`.
- **Managed UI:** card wrapper only; checkbox `WrapPanel`s + conditional panel unchanged.

### Window
- `Width` 820 → 980; `Height=900`; `MinWidth=640` / `MinHeight=500` unchanged.
- DockPanel shell unchanged: Menu (top), Build log + Build bar (bottom), ScrollViewer form (fill).

## Files modified

- `src/MsiBuilderUI/Views/MainWindow.axaml` — only code file changed.

No changes to: `MainWindowViewModel.cs`, any other ViewModel, `App.axaml`, contracts, worker.

## Acceptance criteria

- Sections render as cards; short fields sit 2-up at the default width and reflow to 1-up when
  the window is narrowed; paths/URLs stay full width and stretch.
- All bindings, Browse/Generate commands, and Save/Load profile behavior are unchanged.
- `dotnet build` of the UI project succeeds (XAML compiles); existing tests stay green.

## Verification

1. `dotnet build src/MsiBuilderUI/MsiBuilderUI.csproj` (XAML is compiled at build).
2. `dotnet test` — VM/contract tests stay green (untouched by this change).
3. Run `dotnet run --project src/MsiBuilderUI` and visually confirm cards, reflow, and that
   buttons/profile round-trip still work.
