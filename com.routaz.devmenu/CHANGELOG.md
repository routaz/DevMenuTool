# Changelog

All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Fixed
- Resolved a compile error in `DevMethod` caused by accessing validator-collection
  methods through `IParameterValidator`, which only declares `Validate`. Introduced
  `IParameterValidatorManager` for the collection role, keeping `IParameterValidator`
  as the single-validator strategy (Interface Segregation).
- Runtime hotkeys now work in builds: `DevMenuHotkeyListener` scans the scene on enable
  and rescans on scene load, so `CachedMethods` is populated even without the runtime UI.
  The play-mode injector now runs `AfterSceneLoad` so scene objects exist when scanning.
- `DevMenuParameterPrefs` keys registry is now serialized with a concrete type instead of
  an anonymous object (which `JsonUtility` cannot serialize), so `ClearAll()` works.
- Numeric parameters parse and format with `InvariantCulture`, fixing round-tripping on
  locales that use a comma decimal separator.
- Parameter editor fields now persist through a single path; the redundant editor-time
  write no longer diverges from the stored-value format.

### Added
- Declarative validation attributes (`[ParameterRange]`, `[StringValidation]`,
  `[Required]`) are now registered automatically when a `DevMethod` is created.

### Changed
- Entering an invalid parameter value now rejects the edit and keeps the last valid
  value, instead of overwriting it with the parameter default.
- Resolved hotkeys are cached per method signature to avoid reading `EditorPrefs`
  every frame.
- `IDevMethod` is now composed from focused role interfaces (`IDevMethodInfo`,
  `IDevMethodExecutor`, `IParameterManager`, `IParameterValidatorManager`,
  `IParameterChangeNotifier`) instead of re-declaring a flat member list, satisfying
  Interface Segregation. The public member surface is unchanged.
- `DevMethod` no longer constructs its collaborators with `new`; the parameter manager,
  validator manager, and event manager are injected through the constructor (Dependency
  Inversion). `DevMethodFactory` is now the single place that wires them.
- `ParameterManager`, `ParameterValidator`, and `ParameterEventManager` depend on the
  minimal data they need (`ParameterInfo[]`, method key/name, `IParameterHandlerRegistry`)
  rather than back-referencing the whole `IDevMethod`, removing a construction cycle.
- Parameter handler lookups are now resolved through the `IParameterHandlerRegistry`
  abstraction injected into `ParameterManager`, `DevMethodFactory`, and `UIBuilder`,
  instead of reaching into the static `DevMenuContainer` (no more hidden service-locator
  dependencies). `DevMenuContainer` remains the composition root that wires them together.
- `DevMethod` no longer throws when constructed with a null target instance, allowing the
  factory to build methods for static targets.

### Documentation
- Corrected all `[DevMenu(...)]` examples to match the attribute signature
  `[DevMenu(KeyCode, string)]`.
