# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

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
