# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

## [0.3.0] - 2026-10-15

### Added
- A runtime overlay for Play Mode and development builds. F1 toggles a panel on the right with commands grouped by name, editable parameters, Run buttons and the last 5 log lines. The game keeps running.
- `DevMenuOverlayInput.IsOpen`, `IsPointerOverOverlay` and `IsTextInputFocused`, so games can keep their own input out of the way.
- Optional Input System support: when `com.unity.inputsystem` is installed, the toggle key and pointer are read through it.
- MIT `LICENSE.md` in the package (and `LICENSE` at the repository root); `package.json` now declares `"license": "MIT"`.
- Background command index (`DevMenuMethodIndex`, exposed as `DevMenuContainer.MethodIndex`): the list of `[DevMenu]` methods is built once, from `TypeCache` in the Editor (on project open and after every recompile) and on a background thread at startup in player builds (only assemblies that reference `DevMenuTool` are searched). WebGL has no threads, so there the index is built synchronously on the first scan.
- **DevMenu > Rescan Commands** menu item: rebuilds the index, rescans the loaded scenes, refreshes open Dev Menu windows and logs the command and type counts.
- `IMethodScanner.RebuildIndex()`: rebuilds the index from the loaded assemblies, then rescans.
- `DevMenuOverlay.Instance`: the one active overlay. A second overlay logs a warning and destroys its own component.
- `DevMenuInput.EffectiveToggleKey(KeyCode)`: the key the overlay actually reads (F1 for keys the Input System path doesn't support). The overlay's hint label shows it.

### Changed
- The hotkey listener and the overlay are only spawned in the Editor and development builds. Release builds get neither.
- The hotkey listener's legacy `Input` calls are compiled only when the legacy Input Manager is enabled. In Input-System-only projects, per-command hotkeys are inactive, and commands are run from the overlay or the editor window instead.
- Command hotkeys don't fire while a text or number field in the overlay has focus, so typing into a parameter no longer runs the commands bound to those letters.
- Closing the overlay removes keyboard focus from its fields.
- The overlay runs early (`DefaultExecutionOrder(-1000)`), so `DevMenuOverlayInput` is current before game scripts read it, and it destroys its `PanelSettings` when it is destroyed.
- The Play Mode injector skips spawning an overlay when one already exists in the scene.
- The background command index is only started in the Editor and development builds.
- The sample's `LogState` command is bound to F3, since F1 toggles the overlay.
- README rewritten: accurate supported parameter types and Edit Mode behaviour, git URL install, hotkey rebinding, and the legacy Input Manager requirement for hotkeys.
- Opening the menu no longer reflects over every `MonoBehaviour` in the scene, which froze the Editor in large projects. `MethodScanner.ScanScene()` now only looks up live instances of the indexed types, so its cost is proportional to the number of command-bearing types. Inactive, `DontDestroyOnLoad` and `HideFlags`-hidden objects are still found.
- `MethodScanner`'s constructor now takes an `IDevMenuMethodIndex` as a third argument.
- A `[DevMenu]` method declared private on a base class now also appears on subclass instances (previously only public and protected base methods did). Methods declared on open generic classes are no longer listed.

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
