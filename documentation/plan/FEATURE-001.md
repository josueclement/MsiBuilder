# FEATURE-001 — Restructure to `src/` layout and migrate solution to `.slnx`

**Status:** DONE

## Objective

Adopt the House-Conventions repository layout (`src/`, `tests/`, `documentation/`) and the
modern `.slnx` (XML) solution format, without changing any code or build behavior.

## Scope

- Move both production projects under `src/`.
- Replace the legacy `MsiBuilder.sln` with `MsiBuilder.slnx`.
- Scaffold the `tests/` and `documentation/` top-level directories.

Out of scope (deliberately): target-framework changes, project renames, source-code edits.

## Design

### Moves (history preserved via `git mv`)

| From            | To                  |
|-----------------|---------------------|
| `MsiBuilder/`   | `src/MsiBuilder/`   |
| `ConsoleApp1/`  | `src/ConsoleApp1/`  |

### Relative-path adjustments

- `src/MsiBuilder/MsiBuilder.csproj` — packaging includes moved one level deeper:
  `..\README.md` → `..\..\README.md`, `..\LICENSE.md` → `..\..\LICENSE.md`.
- `src/ConsoleApp1/ConsoleApp1.csproj` — **unchanged**: its `..\MsiBuilder\MsiBuilder.csproj`
  reference still resolves because both projects moved together.

### Solution migration

`MsiBuilder.slnx` at the repo root, referencing the projects at their new `src/` paths.
Debug/Release `Any CPU` configurations are implicit defaults in `.slnx`, so no explicit
configuration block is required. The old `MsiBuilder.sln` is removed.

## Acceptance criteria

- [x] Both projects live under `src/`; `git status` records renames, not delete+add.
- [x] `MsiBuilder.slnx` exists and `dotnet sln MsiBuilder.slnx list` lists both projects.
- [x] `dotnet restore MsiBuilder.slnx` resolves the full project + package graph.
- [x] `MsiBuilder.sln` is gone.
- [x] `tests/` and `documentation/` directories exist.

> Note: a full `dotnet build` requires Windows (.NET Framework `net472` targeting pack);
> it is not run on the Linux/WSL development environment.
