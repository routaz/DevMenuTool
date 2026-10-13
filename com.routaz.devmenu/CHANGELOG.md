# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Added
- MIT `LICENSE.md` in the package (and `LICENSE` at the repository root); `package.json` now declares `"license": "MIT"`.

### Changed
- README rewritten: accurate supported parameter types and Edit Mode behaviour, git URL install, hotkey rebinding, and the legacy Input Manager requirement for hotkeys.

### Removed
- Redundant code comments in package sources and tests (XML API docs are unchanged).
- Unity template leftovers (`TutorialInfo/`, `Readme.asset`) from the development project.
- Committed IDE settings (`.idea/`), Test Runner result exports and an imported copy of the 0.1.0 sample; these are now git-ignored.
- Stray Unity project at the repository root (`Packages/`, `ProjectSettings/`). `DevMenuTool/` is the only development project.

## [0.2.0] - 2026-09-18

### Fixed
- **Player builds no longer fail to compile.** Editor code now lives in a separate
  `DevMenuTool.Editor` assembly (`Editor/DevMenuTool.Editor.asmdef`, Editor platform only),
  so `UnityEditor` references are no longer compiled into players.
- `MethodScanner.ScanScene()` now finds `[DevMenu]` methods on objects hidden with `HideFlags`
  (e.g. `HideAndDontSave`), which `FindObjectsOfType` silently skipped. Inactive and
  `DontDestroyOnLoad` objects are still included; prefabs and other assets are excluded.
- The `DevMenu > Open Dev Menu` window now rescans when entering or exiting Play Mode (it
  already rebuilds after domain reloads), so it no longer shows stale methods or disabled Run buttons.

### Added
- **Refresh** toolbar button in the editor window to rescan loaded scenes on demand.
- `[DevMenu(string groupName)]` constructor for menu-only methods without a hotkey
  (same as passing `KeyCode.None`).
- Parameter values and hotkey rebinds persist in player builds via `PlayerPrefs`
  (the editor keeps using `EditorPrefs`, so existing saved values are preserved).

### Changed
- **Breaking:** `DevMenuParameterPrefs` and `DevMenuKeybinds` moved from the `DevMenuTool.Editor`
  namespace to `DevMenuTool.Runtime.Persistence` (files moved to `Runtime/Persistence/`). They are
  runtime persistence, not editor tooling. Update `using` directives if you call them directly.

## [0.1.0] - 2026-06-01

Initial release.

### Added
- `[DevMenu(KeyCode, string)]` attribute to expose any method in the runtime dev menu,
  with an optional hotkey and group name.
- Automatic scene scanning that discovers decorated methods and builds a UI Toolkit menu
  at runtime.
- Parameter editing for `int`, `float`, `bool`, `enum`, and `string` parameters, with
  per-parameter persistence so edited values survive across play sessions.
- Declarative validation attributes (`[ParameterRange]`, `[StringValidation]`,
  `[Required]`); invalid edits are rejected and the last valid value is kept.
- Configurable hotkeys per method, editable in the editor and remembered between sessions.
- Editor window for browsing and configuring dev methods outside of play mode.
- Extensible parameter handling through `IParameterHandlerRegistry` for parsing and
  formatting custom types.
- Importable **DevMenu Sample** (via Package Manager) demonstrating actions, typed
  parameters, validation, and change events.
