# DevMenu runtime overlay: design spec

Date: 2026-10-14
Status: approved in conversation, awaiting written-spec review
Package: `com.routaz.devmenu` (0.2.0 → 0.3.0)
First consumer: TheLongBurn (Input System only, standalone development builds for playtests)

## Goal

Make `[DevMenu]` commands usable in **standalone development builds**, where the editor window doesn't exist.
A tester presses **F1**, sees every command grouped by category, edits parameters, clicks **Run**, sees the
result in a log strip, and presses F1 again to close. The game keeps running throughout.

Success: in a development build of TheLongBurn, F1 opens the overlay. "Carriers → Depart In Seconds" runs with
its default of 30, its log line appears in the overlay, and clicking the overlay doesn't also click the game
behind it.

## Decisions (from the conversation)

- The overlay lives in this package, not in a game. It is generic and reusable.
- Toggle key: **F1**.
- The game **keeps running** while the overlay is open; there is no pause.
- The overlay shows the **last few log lines**.
- UI Toolkit, reusing `UIBuilder`. An IMGUI version was rejected because it would duplicate the parameter-field
  code.

## Out of scope

- Per-command hotkeys under the Input System (KeyCode → Key mapping). Commands are run from the overlay instead.
- Rebinding the toggle key in-game. F1 is a serialized default on the component, so a project can change it in code.
- Search or filtering, favourites, gamepad or touch navigation.
- Pausing the game.

## 1. Availability

- New compile guard: `UNITY_EDITOR || DEVELOPMENT_BUILD`. Everything the overlay adds lives behind it.
- `DevMenuPlayModeInjector`, which today spawns the hotkey listener in **every** build including release, is
  wrapped in the same guard. In a release build neither the listener nor the overlay exists. This fixes an
  existing behaviour: release builds no longer carry a live dev-command listener.
- The injector spawns the overlay next to the hotkey listener, as one `DontDestroyOnLoad` object called
  `__DevMenuOverlay`. That object must **not** use `HideFlags.HideAndDontSave`. `MethodScanner` uses
  `FindObjectsOfType`, which skips hidden objects (a gotcha observed in TheLongBurn).

## 2. Input

- An optional dependency on the Input System, through `versionDefines` in `DevMenuTool.asmdef`:
  `com.unity.inputsystem` defines `DEVMENU_INPUT_SYSTEM`.
- `DevMenuInput.WasTogglePressed()` (internal static):
  - With `DEVMENU_INPUT_SYSTEM` and `ENABLE_INPUT_SYSTEM`: `Keyboard.current?.f1Key.wasPressedThisFrame`.
  - Otherwise, if `ENABLE_LEGACY_INPUT_MANAGER`: `Input.GetKeyDown(KeyCode.F1)`.
  - Otherwise: false.
  - The key is a field (default F1). The Input System branch maps the configured KeyCode to `Key` through a small
    lookup that covers F1-F12 and backquote. If the KeyCode isn't in the lookup, it falls back to F1 with a
    warning.
- `DevMenuHotkeyListener.Update` wraps its `Input.GetKeyDown` calls in `#if ENABLE_LEGACY_INPUT_MANAGER`. Without
  that guard, a project with only the Input System throws as soon as any command has a hotkey. Under the Input
  System alone, per-command hotkeys are inactive, and the README says so.

## 3. Overlay component

`DevMenuOverlay : MonoBehaviour` (namespace `DevMenuTool.UI`):

- **Construction.** On the first open it builds its own `UIDocument` with a runtime-created `PanelSettings`:
  - scale mode constant pixel size, scale 1, reference resolution 1920×1080;
  - `sortingOrder` 10000, so it sits above game UI.
  - The theme is a small `DevMenuRuntimeTheme.tss` shipped in `UI/Resources`, containing
    `@import url("unity-theme://default");`, so standard controls render styled.
  - The tree comes from a new `DevMenuOverlay.uxml` plus the existing `DevMenu.uss` and a few overlay rules.
- **Layout.**
  - A panel docked to the **right edge**, 420 px wide, full height, semi-opaque.
  - Header: "DEV MENU" and an "F1 to close" hint.
  - A `ScrollView` of **collapsible category foldouts**, one per `GroupName` and sorted by name. Inside each, one
    `UIBuilder.BuildMethodUI(method)` per command, sorted by display name.
  - At the bottom, the **log strip**.
- **Lifecycle.**
  - Hidden at start.
  - F1 toggles `IsOpen`.
  - On open: rescan (`MethodScanner.ScanScene()`) and rebuild the list, so commands from scenes loaded since the
    last open appear. This is cheap because it relies on the command index from commits 75a32f1 and 4ce7629. That
    index is built once: from `TypeCache` in the Editor, and on a background thread at startup in players. With
    it, `ScanScene` only looks up live instances of the few command-bearing types, with no reflection per scene
    object, so opening the overlay causes no lag spike.
  - On close: the panel is hidden (`display: none`). It is not destroyed, so foldout state persists for the session.
- **Statics for games.**
  - `public static bool IsOpen { get; }`
  - `public static bool IsPointerOverOverlay { get; }`: true while the pointer is over the visible panel's
    world bound. It is computed from `panel.Pick` / `RuntimePanelUtils.ScreenToPanel` of the current pointer
    position, read from whichever input backend is active.
  - A game uses these to keep its own click handling and hotkeys out of the way (section 5).

## 4. Log strip

- `DevMenuLogBuffer` is a plain C# class and unit-tested. It is a ring buffer of the **last 5** entries, holding
  type, message (the first line only) and time.
- The overlay subscribes to `Application.logMessageReceived` from spawn time, not from first open, so lines logged
  just before opening still show.
- Entries render newest last. Warnings and errors get a colour class (`devmenu-log--warning`/`--error`).
- The buffer runs whether the panel is open or closed, at negligible cost: 5 entries, no allocation per frame.

## 5. Consumer integration (TheLongBurn, separate change in that repo)

- World picking and HUD clicks are ignored while `DevMenuOverlay.IsPointerOverOverlay`.
- Game hotkeys are ignored while `DevMenuOverlay.IsOpen` **and** an overlay text field has focus. This keeps typing
  in parameter fields from firing game keys. Otherwise, game keys keep working while the overlay is open, because
  the game keeps running.
- The TheLongBurn plan names the exact files. This package spec only guarantees the two statics.

## 6. Package hygiene

- `package.json` goes to 0.3.0, with `com.unity.inputsystem` not added as a hard dependency (it is optional).
- CHANGELOG: an Added entry for the overlay and log strip, and a Changed entry for the release-build guard on the
  injector and the guarded legacy `Input` in the listener.
- README: a "Using the menu in builds" section covering F1, development builds only, the Input System notes, and the
  two statics for games.

## 7. Testing

EditMode tests in the development project, `DevMenuTool/Assets/Tests`. Running them needs that project open in
Unity.

- `DevMenuLogBuffer`: keeps only the last 5 entries; first-line trimming; type preserved; ordering.
- Category grouping: a pure helper that takes the cached methods and returns groups sorted by name, with commands
  sorted inside each group.
- Toggle state: the overlay's open/close logic, driven through an injectable "toggle pressed" function. No real
  keyboard is needed.
- KeyCode → Key lookup: F1-F12 and backquote map correctly; an unknown key falls back to F1.

Manual: a development build of TheLongBurn using the success scenario under Goal, plus a release build to confirm the
overlay and listener are absent.

## Risks

- **Runtime `PanelSettings` without a theme renders unstyled controls.** The shipped `.tss` covers this. Verify it
  in a build, not only in the Editor.
- **`unity-theme://default` availability varies by Unity version.** The package declares 2021.3. Confirm the
  import resolves in TheLongBurn's Unity version. If it doesn't, ship a copy of the default runtime theme.
- **Input System versions:** `wasPressedThisFrame` and `Keyboard.current` are stable from 1.0 onward.
