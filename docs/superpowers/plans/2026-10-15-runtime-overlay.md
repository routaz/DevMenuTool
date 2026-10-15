# DevMenu Runtime Overlay Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make `[DevMenu]` commands usable in standalone development builds. F1 toggles a UI Toolkit panel that lists the commands by group, lets you edit their parameters and run them, and shows the last 5 log lines. The game keeps running throughout.

**Architecture:**
- Small pure pieces, each unit-tested:
  - `DevMenuKeyMap`: maps a toggle key to its Input System key name.
  - `DevMenuLogBuffer`: a ring buffer of recent log lines.
  - `DevMenuCommandGroups`: groups and sorts commands.
- They feed one MonoBehaviour, `DevMenuOverlay`. It builds its own `UIDocument` and `PanelSettings` at runtime and reuses the existing `UIBuilder` for each command.
- `DevMenuPlayModeInjector` spawns the overlay in the Editor and development builds only.
- `DevMenuOverlayInput` publishes the overlay's state (open, pointer over it, typing in it) as plain statics, so a game can keep its own input out of the way. TheLongBurn does that in Task 7.

**Tech Stack:**
- Unity 6000.3.26f1 (the package declares 2021.3).
- UI Toolkit runtime panels.
- An optional Input System dependency through asmdef `versionDefines`.
- NUnit tests in the dev project, run in **PlayMode** through UnityMCP on port 7890.

**Spec:** `docs/superpowers/specs/2026-10-14-runtime-overlay-design.md`, approved 2026-10-15.

## Global Constraints

- **Repo layout:** the repo is `D:/GitHubRepos/DevMenuTool`. The package is in `com.routaz.devmenu/`. The dev project is `DevMenuTool/`, whose tests are in `DevMenuTool/Assets/Tests`, assembly `DevMenuTool.Tests`.
- **Running tests:** the user's Unity Editor has the dev project open, with the UnityMCP bridge on port 7890. Load the tools with ToolSearch (`mcp__UnityMCP__unity_advanced_tool`, `mcp__UnityMCP__unity_get_compilation_errors`, `mcp__UnityMCP__unity_select_instance`). After editing, wait for compilation and confirm 0 errors. Run tests with `unity_advanced_tool` → `unity_testing_run_tests` and params `{"mode":"PlayMode"}`. Add `"filter":"<ClassName>"` to run one fixture. Never launch, quit or kill an Editor.
- **Baseline:** 108/108 PlayMode tests pass before Task 1.
- **The dev project uses the legacy Input Manager only** (`activeInputHandler: 0`) and has no Input System package. The Input System branch therefore compiles only in TheLongBurn, which has Input System 1.18 and new-input-only. Task 7 verifies that branch.
- **Toggle key:** F1 by default.
- **Panel layout:** docked to the right edge, 420 px wide, full height. `sortingOrder` 10000. Scale mode constant pixel size, scale 1. The game keeps running, so there is no pause and no timescale change.
- **Log strip:** keeps the last **5** entries, first line of each message only. Warnings and errors get the colour classes `devmenu-log--warning` and `devmenu-log--error`.
- **Guard:** the injector spawns the overlay only under `#if UNITY_EDITOR || DEVELOPMENT_BUILD`, and the hotkey listener sits under the same guard. The legacy `Input.GetKeyDown` in `DevMenuHotkeyListener` is wrapped in `#if ENABLE_LEGACY_INPUT_MANAGER`.
- **Overlay object:** `__DevMenuOverlay`, `DontDestroyOnLoad`, **not** hidden with HideFlags.
- **Statics for games:** `DevMenuOverlayInput.IsOpen` and `DevMenuOverlayInput.IsPointerOverOverlay`, plus `IsTextInputFocused` (see "Clarifications of the spec").
- **Optional Input System dependency:** `versionDefines` maps `com.unity.inputsystem` ≥ 1.0.0 to the define `DEVMENU_INPUT_SYSTEM`. It is **not** a hard dependency in `package.json`.
- **Docs and version:** the version becomes `0.3.0`. Update CHANGELOG `[Unreleased]` (the release header comes with 0.3.0). The repo root `README.md` and `com.routaz.devmenu/README.md` are identical copies, so make the same edit to both.
- **Repo rules:**
  - Never run `git add -A` or `git add .`. The repo root has stray untracked folders (`Packages/`, `ProjectSettings/`, …), and `DevMenuTool/ProjectSettings/*.asset` contains Unity's edits. Neither is ours.
  - Keep line endings: `.cs` files are CRLF, `.meta` files are LF. Write no literal `\r`.
  - End every commit message with a blank line, then `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.

## Clarifications of the spec (decided while planning)

1. **`IsTextInputFocused` is a third static.** Spec section 5 asks the game to ignore its hotkeys while an overlay text field has focus, which only works if the package exposes that state. Spec section 3 lists two statics, so this adds one.
2. **Log entries don't store a time.** Spec section 4 lists a time on each entry, but nothing displays it. Leaving it out is YAGNI.
3. **The guard is on spawning, not on the class.** `DevMenuOverlay` and its helpers compile in every build, and only the injector's spawning is guarded. Tests and `DevMenuOverlayInput` therefore compile everywhere, and a release build never creates the object.
4. **Toggle tests call `Toggle()`, `Open()` and `Close()` directly.** Spec section 7 wants an "injectable toggle-pressed function". Calling the methods directly tests the same state logic without a seam.

## Review Focus

1. **Changing a `bool` parameter inside a group must not collapse the group.** A Toggle's `ChangeEvent<bool>` bubbles up to the parent `Foldout`'s value-changed callback. Test added to Task 5.
2. **Log spam:** hundreds of logs in a frame keep only the last 5 entries, and the strip renders once per frame. Test added to Task 2.
3. **Collapse state survives close and reopen.** Collapsing a group, closing, then reopening keeps it collapsed for the session. Test added to Task 5.
4. **A hidden overlay never claims the pointer or blocks clicks.** When closed, `IsPointerOverOverlay` is false. The full-screen container uses `PickingMode.Ignore`, so the game's own UI panels still receive clicks. Test added to Task 5. The picking-through behaviour is checked manually in Task 7.
5. **Lines logged before the first open are captured.** Logging starts at spawn time, not at the first open. Test added to Task 5.

---

## File structure

| File | Responsibility |
|------|----------------|
| `com.routaz.devmenu/DevMenuTool.asmdef` (modify) | Optional `Unity.InputSystem` reference, plus a `versionDefines` entry that sets `DEVMENU_INPUT_SYSTEM`. |
| `com.routaz.devmenu/Runtime/DevMenuKeyMap.cs` (new) | Pure mapping from `KeyCode` to the Input System `Key` enum name. |
| `com.routaz.devmenu/Runtime/DevMenuInput.cs` (new) | Reads "key pressed this frame" and the pointer position from whichever input backend is active. |
| `com.routaz.devmenu/Runtime/DevMenuHotkeyListener.cs` (modify) | Legacy `Input` read wrapped in `#if ENABLE_LEGACY_INPUT_MANAGER`. |
| `com.routaz.devmenu/Runtime/DevMenuPlayModeInjector.cs` (modify) | Dev-only guard. Spawns `__DevMenuOverlay`. |
| `com.routaz.devmenu/UI/DevMenuLogBuffer.cs` (new) | Pure ring buffer of the last N log lines. |
| `com.routaz.devmenu/UI/DevMenuCommandGroups.cs` (new) | Pure grouping and sorting of commands. |
| `com.routaz.devmenu/UI/DevMenuOverlayInput.cs` (new) | Public read-only statics for games. |
| `com.routaz.devmenu/UI/DevMenuOverlay.cs` (new) | The runtime panel: build, toggle, rebuild the list, render the log, publish input state. |
| `com.routaz.devmenu/UI/Resources/DevMenuOverlay.uxml` (new) | Panel tree. |
| `com.routaz.devmenu/UI/Resources/DevMenuOverlay.uss` (new) | Panel styles. |
| `com.routaz.devmenu/UI/Resources/DevMenuRuntimeTheme.tss` (new) | Runtime theme that imports Unity's default theme. |
| `DevMenuTool/Assets/Tests/Runtime/DevMenuKeyMapTests.cs` (new) | Tests for the key map. |
| `DevMenuTool/Assets/Tests/UI/DevMenuLogBufferTests.cs` (new) | Tests for the log buffer. |
| `DevMenuTool/Assets/Tests/UI/DevMenuCommandGroupsTests.cs` (new) | Tests for command grouping. |
| `DevMenuTool/Assets/Tests/UI/DevMenuOverlayTests.cs` (new) | PlayMode smoke tests of the panel. |
| `README.md`, `com.routaz.devmenu/README.md`, `com.routaz.devmenu/CHANGELOG.md`, `com.routaz.devmenu/package.json` (modify) | Docs and version. |
| TheLongBurn (Task 7): `Assets/Scripts/TheLongBurn.asmdef`, `Assets/Scripts/UI/UIManager.cs`, `Assets/Scripts/UI/PauseMenuUI.cs` | Integration. |

---

### Task 1: Input plumbing and the release-build guard

**Files:**
- Modify: `com.routaz.devmenu/DevMenuTool.asmdef`
- Create: `com.routaz.devmenu/Runtime/DevMenuKeyMap.cs`, `com.routaz.devmenu/Runtime/DevMenuInput.cs`
- Modify: `com.routaz.devmenu/Runtime/DevMenuHotkeyListener.cs` (`Update`), `com.routaz.devmenu/Runtime/DevMenuPlayModeInjector.cs`
- Test: `DevMenuTool/Assets/Tests/Runtime/DevMenuKeyMapTests.cs`

**Interfaces:**
- Produces:
  - `public static class DevMenuKeyMap { public static bool TryGetInputSystemKeyName(KeyCode keyCode, out string keyName); }`
  - `public static class DevMenuInput { public static bool WasPressedThisFrame(KeyCode keyCode); public static bool TryGetPointerPosition(out Vector2 screenPosition); }`
  - Both live in namespace `DevMenuTool.Runtime`.

- [ ] **Step 1: Write the failing test**

`DevMenuTool/Assets/Tests/Runtime/DevMenuKeyMapTests.cs`:
```csharp
using NUnit.Framework;
using UnityEngine;
using DevMenuTool.Runtime;

namespace DevMenuTool.Tests.Runtime
{
    [TestFixture]
    public class DevMenuKeyMapTests
    {
        [TestCase(KeyCode.F1, "F1")]
        [TestCase(KeyCode.F7, "F7")]
        [TestCase(KeyCode.F12, "F12")]
        [TestCase(KeyCode.BackQuote, "Backquote")]
        public void SupportedKeys_MapToInputSystemKeyNames(KeyCode keyCode, string expected)
        {
            Assert.IsTrue(DevMenuKeyMap.TryGetInputSystemKeyName(keyCode, out var name));
            Assert.AreEqual(expected, name);
        }

        [TestCase(KeyCode.A)]
        [TestCase(KeyCode.F13)]
        [TestCase(KeyCode.None)]
        public void UnsupportedKeys_AreRejected(KeyCode keyCode)
        {
            Assert.IsFalse(DevMenuKeyMap.TryGetInputSystemKeyName(keyCode, out var name));
            Assert.IsNull(name);
        }
    }
}
```

- [ ] **Step 2: Run it and confirm it fails**

Run: `unity_testing_run_tests {"mode":"PlayMode","filter":"DevMenuKeyMapTests"}` (port 7890).
Expected: a compile error, because `DevMenuKeyMap` doesn't exist yet.

- [ ] **Step 3: Implement**

`com.routaz.devmenu/Runtime/DevMenuKeyMap.cs`:
```csharp
using UnityEngine;

namespace DevMenuTool.Runtime
{
    /// <summary>
    /// Maps the keys the overlay can be toggled with to Input System <c>Key</c> enum names.
    /// Kept free of any Input System types so it compiles and tests without the package.
    /// </summary>
    public static class DevMenuKeyMap
    {
        public static bool TryGetInputSystemKeyName(KeyCode keyCode, out string keyName)
        {
            if (keyCode >= KeyCode.F1 && keyCode <= KeyCode.F12)
            {
                keyName = keyCode.ToString();
                return true;
            }

            if (keyCode == KeyCode.BackQuote)
            {
                keyName = "Backquote";
                return true;
            }

            keyName = null;
            return false;
        }
    }
}
```

`com.routaz.devmenu/Runtime/DevMenuInput.cs`:
```csharp
using UnityEngine;

namespace DevMenuTool.Runtime
{
    /// <summary>
    /// Reads keys and the pointer from the Input System when the project has it, otherwise from the
    /// legacy Input Manager. Returns false when neither is available.
    /// </summary>
    public static class DevMenuInput
    {
#if DEVMENU_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
        private const string FallbackKeyName = "F1";
        private static bool _warnedUnsupportedKey;
#endif

        public static bool WasPressedThisFrame(KeyCode keyCode)
        {
#if DEVMENU_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return false;

            if (!DevMenuKeyMap.TryGetInputSystemKeyName(keyCode, out var keyName))
            {
                if (!_warnedUnsupportedKey)
                {
                    Debug.LogWarning($"[DevMenu] {keyCode} can't be read through the Input System; using {FallbackKeyName}.");
                    _warnedUnsupportedKey = true;
                }
                keyName = FallbackKeyName;
            }

            var key = (UnityEngine.InputSystem.Key)System.Enum.Parse(typeof(UnityEngine.InputSystem.Key), keyName);
            return keyboard[key].wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(keyCode);
#else
            return false;
#endif
        }

        public static bool TryGetPointerPosition(out Vector2 screenPosition)
        {
#if DEVMENU_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null)
            {
                screenPosition = mouse.position.ReadValue();
                return true;
            }
            screenPosition = default;
            return false;
#elif ENABLE_LEGACY_INPUT_MANAGER
            screenPosition = Input.mousePosition;
            return true;
#else
            screenPosition = default;
            return false;
#endif
        }
    }
}
```
`FallbackKeyName` and `_warnedUnsupportedKey` are declared inside the same `#if` as the Input System branch, which is their only user. That keeps the dev project's compile free of unused-field warnings. The compile must have 0 warnings.

`com.routaz.devmenu/DevMenuTool.asmdef`: change `"references": []` to `"references": ["Unity.InputSystem"]`, and `"versionDefines": []` to:
```json
    "versionDefines": [
        {
            "name": "com.unity.inputsystem",
            "expression": "1.0.0",
            "define": "DEVMENU_INPUT_SYSTEM"
        }
    ],
```
Unity skips a name reference that doesn't resolve when the package is missing, which is the standard optional-dependency pattern. Confirm the dev project still compiles with 0 errors.

`DevMenuHotkeyListener.Update`: wrap the body in `#if ENABLE_LEGACY_INPUT_MANAGER … #endif`, so an Input-System-only project never calls `Input.GetKeyDown`. Leave the rest of the file unchanged.

`DevMenuPlayModeInjector.cs`: wrap the `[RuntimeInitializeOnLoadMethod]` method and its constant in `#if UNITY_EDITOR || DEVELOPMENT_BUILD … #endif`. Task 5 adds the overlay spawn inside the same block.

- [ ] **Step 4: Run the tests and confirm they pass**

1. Run `filter: DevMenuKeyMapTests`. Expected: 7/7 pass.
2. Run the full PlayMode suite. Expected: 115/115, with 0 compile errors and 0 warnings.

- [ ] **Step 5: Commit**

```bash
git add com.routaz.devmenu/DevMenuTool.asmdef com.routaz.devmenu/Runtime/DevMenuKeyMap.cs com.routaz.devmenu/Runtime/DevMenuKeyMap.cs.meta com.routaz.devmenu/Runtime/DevMenuInput.cs com.routaz.devmenu/Runtime/DevMenuInput.cs.meta com.routaz.devmenu/Runtime/DevMenuHotkeyListener.cs com.routaz.devmenu/Runtime/DevMenuPlayModeInjector.cs DevMenuTool/Assets/Tests/Runtime/DevMenuKeyMapTests.cs DevMenuTool/Assets/Tests/Runtime/DevMenuKeyMapTests.cs.meta
git commit -m "feat: input plumbing for the overlay; dev-only injector; guarded legacy input"
```

---

### Task 2: Log buffer

**Files:**
- Create: `com.routaz.devmenu/UI/DevMenuLogBuffer.cs`
- Test: `DevMenuTool/Assets/Tests/UI/DevMenuLogBufferTests.cs`

**Interfaces:**
- Produces, in namespace `DevMenuTool.UI`:
  - `public readonly struct DevMenuLogEntry { LogType Type; string Message; }`
  - `public sealed class DevMenuLogBuffer` with:
    - `const int DefaultCapacity = 5`
    - a constructor `DevMenuLogBuffer(int capacity = DefaultCapacity)`
    - `int Count`
    - `int Version`
    - `void Add(LogType type, string message)`
    - `IReadOnlyList<DevMenuLogEntry> Snapshot()`, oldest first

- [ ] **Step 1: Write the failing test**

`DevMenuTool/Assets/Tests/UI/DevMenuLogBufferTests.cs`:
```csharp
using NUnit.Framework;
using UnityEngine;
using DevMenuTool.UI;

namespace DevMenuTool.Tests.UI
{
    [TestFixture]
    public class DevMenuLogBufferTests
    {
        [Test]
        public void KeepsOnlyTheLastFive_OldestFirst()
        {
            var buffer = new DevMenuLogBuffer();
            for (int i = 1; i <= 7; i++) buffer.Add(LogType.Log, $"line {i}");

            var entries = buffer.Snapshot();
            Assert.AreEqual(5, entries.Count);
            Assert.AreEqual("line 3", entries[0].Message);
            Assert.AreEqual("line 7", entries[4].Message);
        }

        [Test]
        public void KeepsOnlyTheFirstLine()
        {
            var buffer = new DevMenuLogBuffer();
            buffer.Add(LogType.Error, "boom\r\n  at Foo.Bar()\n  at Baz()");
            Assert.AreEqual("boom", buffer.Snapshot()[0].Message);
        }

        [Test]
        public void PreservesTheLogType()
        {
            var buffer = new DevMenuLogBuffer();
            buffer.Add(LogType.Warning, "careful");
            Assert.AreEqual(LogType.Warning, buffer.Snapshot()[0].Type);
        }

        [Test]
        public void NullMessage_BecomesEmpty()
        {
            var buffer = new DevMenuLogBuffer();
            buffer.Add(LogType.Log, null);
            Assert.AreEqual(string.Empty, buffer.Snapshot()[0].Message);
        }

        [Test]
        public void Version_ChangesOnEveryAdd()
        {
            var buffer = new DevMenuLogBuffer();
            int before = buffer.Version;
            buffer.Add(LogType.Log, "a");
            Assert.AreNotEqual(before, buffer.Version);
        }

        [Test]
        public void LogSpam_StaysBoundedAtCapacity()
        {
            var buffer = new DevMenuLogBuffer();
            for (int i = 0; i < 1000; i++) buffer.Add(LogType.Log, $"spam {i}");

            Assert.AreEqual(DevMenuLogBuffer.DefaultCapacity, buffer.Count);
            Assert.AreEqual("spam 999", buffer.Snapshot()[4].Message);
        }
    }
}
```

- [ ] **Step 2: Run it and confirm it fails**

Run with `filter: DevMenuLogBufferTests`. Expected: a compile error, because the type doesn't exist.

- [ ] **Step 3: Implement**

`com.routaz.devmenu/UI/DevMenuLogBuffer.cs`:
```csharp
using System.Collections.Generic;
using UnityEngine;

namespace DevMenuTool.UI
{
    public readonly struct DevMenuLogEntry
    {
        public readonly LogType Type;
        public readonly string Message;

        public DevMenuLogEntry(LogType type, string message)
        {
            Type = type;
            Message = message;
        }
    }

    /// <summary>Fixed-size ring buffer of the most recent log lines (first line of each message only).</summary>
    public sealed class DevMenuLogBuffer
    {
        public const int DefaultCapacity = 5;

        private readonly DevMenuLogEntry[] _entries;
        private int _start;

        public int Count { get; private set; }

        /// <summary>Changes on every <see cref="Add"/>, so a view can redraw only when something new arrived.</summary>
        public int Version { get; private set; }

        public DevMenuLogBuffer(int capacity = DefaultCapacity)
        {
            _entries = new DevMenuLogEntry[capacity];
        }

        public void Add(LogType type, string message)
        {
            var entry = new DevMenuLogEntry(type, FirstLine(message));

            if (Count < _entries.Length)
            {
                _entries[(_start + Count) % _entries.Length] = entry;
                Count++;
            }
            else
            {
                _entries[_start] = entry;
                _start = (_start + 1) % _entries.Length;
            }

            Version++;
        }

        public IReadOnlyList<DevMenuLogEntry> Snapshot()
        {
            var result = new DevMenuLogEntry[Count];
            for (int i = 0; i < Count; i++)
                result[i] = _entries[(_start + i) % _entries.Length];
            return result;
        }

        private static string FirstLine(string message)
        {
            if (string.IsNullOrEmpty(message)) return string.Empty;
            int end = message.IndexOfAny(new[] { '\r', '\n' });
            return end < 0 ? message : message.Substring(0, end);
        }
    }
}
```

- [ ] **Step 4: Run the tests and confirm they pass**

Run with `filter: DevMenuLogBufferTests`. Expected: 6/6 pass, 0 compile warnings.

- [ ] **Step 5: Commit**

Stage the new `.cs` file, its `.meta`, the new `DevMenuTool/Assets/Tests/UI` folder `.meta`, and the test file with its `.meta`. Commit message: `feat: overlay log ring buffer`.

---

### Task 3: Command grouping

**Files:**
- Create: `com.routaz.devmenu/UI/DevMenuCommandGroups.cs`
- Test: `DevMenuTool/Assets/Tests/UI/DevMenuCommandGroupsTests.cs`

**Interfaces:**
- Consumes `IDevMethod`, with `GroupName` and `ButtonLabel`, in namespace `DevMenuTool.Core.Interfaces`.
- Produces, in namespace `DevMenuTool.UI`:
  - `public sealed class DevMenuCommandGroup { string Name; IReadOnlyList<IDevMethod> Methods; }`
  - `public static class DevMenuCommandGroups { public const string DefaultGroupName = "General"; public static IReadOnlyList<DevMenuCommandGroup> Build(IEnumerable<IDevMethod> methods); }`

- [ ] **Step 1: Write the failing test**

`DevMenuTool/Assets/Tests/UI/DevMenuCommandGroupsTests.cs`:
```csharp
using System.Linq;
using NUnit.Framework;
using DevMenuTool.Core.Interfaces;
using DevMenuTool.Tests.TestHelpers;
using DevMenuTool.UI;

namespace DevMenuTool.Tests.UI
{
    [TestFixture]
    public class DevMenuCommandGroupsTests
    {
        [Test]
        public void GroupsAreSortedByName_CaseInsensitive()
        {
            var groups = DevMenuCommandGroups.Build(new IDevMethod[]
            {
                new MockDevMethod("A", "docking"),
                new MockDevMethod("B", "Carriers"),
                new MockDevMethod("C", "economy"),
            });

            CollectionAssert.AreEqual(new[] { "Carriers", "docking", "economy" }, groups.Select(g => g.Name).ToArray());
        }

        [Test]
        public void CommandsAreSortedByLabelWithinTheirGroup()
        {
            var groups = DevMenuCommandGroups.Build(new IDevMethod[]
            {
                new MockDevMethod("Release Pads", "Docking"),
                new MockDevMethod("fill Pads", "Docking"),
            });

            Assert.AreEqual(1, groups.Count);
            CollectionAssert.AreEqual(new[] { "fill Pads", "Release Pads" },
                groups[0].Methods.Select(m => m.ButtonLabel).ToArray());
        }

        [Test]
        public void EmptyGroupName_GoesToGeneral()
        {
            var groups = DevMenuCommandGroups.Build(new IDevMethod[] { new MockDevMethod("X", "") });
            Assert.AreEqual(DevMenuCommandGroups.DefaultGroupName, groups[0].Name);
        }

        [Test]
        public void NoCommands_GivesNoGroups()
        {
            Assert.AreEqual(0, DevMenuCommandGroups.Build(new IDevMethod[0]).Count);
        }
    }
}
```

- [ ] **Step 2: Run it and confirm it fails**

Run with `filter: DevMenuCommandGroupsTests`. Expected: a compile error, because the type doesn't exist.

- [ ] **Step 3: Implement**

`com.routaz.devmenu/UI/DevMenuCommandGroups.cs`:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using DevMenuTool.Core.Interfaces;

namespace DevMenuTool.UI
{
    public sealed class DevMenuCommandGroup
    {
        public string Name { get; }
        public IReadOnlyList<IDevMethod> Methods { get; }

        public DevMenuCommandGroup(string name, IReadOnlyList<IDevMethod> methods)
        {
            Name = name;
            Methods = methods;
        }
    }

    /// <summary>Groups commands by <see cref="IDevMethodInfo.GroupName"/>; groups and their commands are sorted by name.</summary>
    public static class DevMenuCommandGroups
    {
        public const string DefaultGroupName = "General";

        public static IReadOnlyList<DevMenuCommandGroup> Build(IEnumerable<IDevMethod> methods)
        {
            return methods
                .GroupBy(m => string.IsNullOrEmpty(m.GroupName) ? DefaultGroupName : m.GroupName)
                .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .Select(g => new DevMenuCommandGroup(
                    g.Key,
                    g.OrderBy(m => m.ButtonLabel, StringComparer.OrdinalIgnoreCase).ToList()))
                .ToList();
        }
    }
}
```

- [ ] **Step 4: Run the tests and confirm they pass**

Run with `filter: DevMenuCommandGroupsTests`. Expected: 4/4 pass.

- [ ] **Step 5: Commit**

Stage the new `.cs` file, the test file and their `.meta` files. Commit message: `feat: overlay command grouping`.

---

### Task 4: Overlay input statics

**Files:**
- Create: `com.routaz.devmenu/UI/DevMenuOverlayInput.cs`

**Interfaces:**
- Produces `public static class DevMenuOverlayInput`, in namespace `DevMenuTool.UI`, with these members:
  - `public static bool IsOpen { get; }`
  - `public static bool IsPointerOverOverlay { get; }`
  - `public static bool IsTextInputFocused { get; }`
  - `internal static void Set(bool isOpen, bool isPointerOverOverlay, bool isTextInputFocused)`
  - `internal static void Clear()`

This task has no separate test. It holds three properties and two setters, and Task 5's tests check it through the overlay. It is folded into Task 5's commit.

- [ ] **Step 1: Create the file, then commit it together with Task 5**

`com.routaz.devmenu/UI/DevMenuOverlayInput.cs`:
```csharp
namespace DevMenuTool.UI
{
    /// <summary>
    /// What the runtime overlay is doing this frame, so a game can keep its own input out of the way:
    /// skip world clicks while <see cref="IsPointerOverOverlay"/>, skip typed hotkeys while
    /// <see cref="IsTextInputFocused"/>. All false in release builds, where the overlay never spawns.
    /// </summary>
    public static class DevMenuOverlayInput
    {
        public static bool IsOpen { get; private set; }
        public static bool IsPointerOverOverlay { get; private set; }
        public static bool IsTextInputFocused { get; private set; }

        internal static void Set(bool isOpen, bool isPointerOverOverlay, bool isTextInputFocused)
        {
            IsOpen = isOpen;
            IsPointerOverOverlay = isPointerOverOverlay;
            IsTextInputFocused = isTextInputFocused;
        }

        internal static void Clear() => Set(false, false, false);
    }
}
```

---

### Task 5: The overlay panel

**Files:**
- Create:
  - `com.routaz.devmenu/UI/DevMenuOverlay.cs`
  - `com.routaz.devmenu/UI/Resources/DevMenuOverlay.uxml`
  - `com.routaz.devmenu/UI/Resources/DevMenuOverlay.uss`
  - `com.routaz.devmenu/UI/Resources/DevMenuRuntimeTheme.tss`
- Modify: `com.routaz.devmenu/Runtime/DevMenuPlayModeInjector.cs`, to spawn the overlay.
- Test: `DevMenuTool/Assets/Tests/UI/DevMenuOverlayTests.cs`

**Interfaces:**
- Consumes:
  - `DevMenuInput` (Task 1), `DevMenuLogBuffer` (Task 2), `DevMenuCommandGroups` (Task 3), `DevMenuOverlayInput` (Task 4).
  - `UIBuilder(IMethodInvoker, IParameterHandlerRegistry)` and `UIBuilder.BuildMethodUI(IDevMethod)`.
  - `DevMenuContainer.MethodScanner`, `.MethodInvoker` and `.ParameterHandlerRegistry`.
- Produces `public class DevMenuOverlay : MonoBehaviour`, in namespace `DevMenuTool.UI`, with:
  - `bool IsOpen`
  - `KeyCode ToggleKey { get; set; }`
  - `void Open()`, `void Close()`, `void Toggle()`
  - `IReadOnlyList<DevMenuLogEntry> LogEntries`

- [ ] **Step 1: Write the failing tests**

`DevMenuTool/Assets/Tests/UI/DevMenuOverlayTests.cs`:
```csharp
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using DevMenuTool.Core.Attributes;
using DevMenuTool.UI;

namespace DevMenuTool.Tests.UI
{
    public class OverlayTestCommands : MonoBehaviour
    {
        public bool Flag;

        [DevMenu("OverlayTests")]
        private void SetFlag(bool flag = false) => Flag = flag;
    }

    public class DevMenuOverlayTests
    {
        private GameObject _overlayGo;
        private GameObject _commandsGo;
        private DevMenuOverlay _overlay;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _commandsGo = new GameObject("OverlayTestCommands");
            _commandsGo.AddComponent<OverlayTestCommands>();
            _overlayGo = new GameObject("TestOverlay");
            _overlay = _overlayGo.AddComponent<DevMenuOverlay>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(_overlayGo);
            Object.Destroy(_commandsGo);
            yield return null;
        }

        private VisualElement Root => _overlay.GetComponentInChildren<UIDocument>().rootVisualElement;
        private VisualElement Panel => Root.Q<VisualElement>("devmenu-overlay");
        private Foldout Group(string name) => Root.Query<Foldout>().ToList().First(f => f.text == name);

        [UnityTest]
        public IEnumerator Open_ShowsPanelAndGroupedCommands()
        {
            _overlay.Open();
            yield return null;

            Assert.IsTrue(_overlay.IsOpen);
            Assert.IsTrue(DevMenuOverlayInput.IsOpen);
            Assert.AreEqual(DisplayStyle.Flex, Panel.resolvedStyle.display);
            Assert.IsNotNull(Group("OverlayTests"));
        }

        [UnityTest]
        public IEnumerator Close_HidesPanelAndClearsInputState()
        {
            _overlay.Open();
            yield return null;
            _overlay.Close();
            yield return null;

            Assert.IsFalse(_overlay.IsOpen);
            Assert.IsFalse(DevMenuOverlayInput.IsOpen);
            Assert.IsFalse(DevMenuOverlayInput.IsPointerOverOverlay);
            Assert.AreEqual(DisplayStyle.None, Panel.resolvedStyle.display);
        }

        [UnityTest]
        public IEnumerator Toggle_FlipsOpenState()
        {
            _overlay.Toggle();
            yield return null;
            Assert.IsTrue(_overlay.IsOpen);
            _overlay.Toggle();
            yield return null;
            Assert.IsFalse(_overlay.IsOpen);
        }

        [UnityTest]
        public IEnumerator LogLines_AreCapturedBeforeFirstOpen()
        {
            Debug.Log("overlay test line");
            yield return null;

            Assert.AreEqual("overlay test line", _overlay.LogEntries.Last().Message);
        }

        [UnityTest]
        public IEnumerator CollapsedGroup_StaysCollapsedAcrossCloseAndReopen()
        {
            _overlay.Open();
            yield return null;
            Group("OverlayTests").value = false;
            _overlay.Close();
            _overlay.Open();
            yield return null;

            Assert.IsFalse(Group("OverlayTests").value);
        }

        [UnityTest]
        public IEnumerator ChangingBoolParameter_DoesNotCollapseGroup()
        {
            _overlay.Open();
            yield return null;
            var group = Group("OverlayTests");
            var toggle = group.Query<Toggle>().ToList().First(t => t != group.Q<Toggle>(className: Foldout.toggleUssClassName));
            toggle.value = !toggle.value;
            yield return null;

            Assert.IsTrue(group.value);
        }
    }
}
```
Two notes on these tests:
- **`OverlayTestCommands` sits in the test assembly.** The Editor's `[InitializeOnLoad]` bootstrap indexes it through `TypeCache`, so `ScanScene` finds the instance the test creates.
- **Finding the parameter toggle.** `Foldout` has its own internal `Toggle`, the header. The last test picks the parameter toggle by excluding the one that carries `Foldout.toggleUssClassName`. If that name differs in this Unity version, pick the toggle inside the `method-body` element instead (`group.Q("method-body").Q<Toggle>()`), and note the change in the report.

- [ ] **Step 2: Run them and confirm they fail**

Run with `filter: DevMenuOverlayTests`. Expected: a compile error, because `DevMenuOverlay` doesn't exist.

- [ ] **Step 3: Implement**

`com.routaz.devmenu/UI/Resources/DevMenuRuntimeTheme.tss`:
```css
@import url("unity-theme://default");
```

`com.routaz.devmenu/UI/Resources/DevMenuOverlay.uxml`:
```xml
<?xml version="1.0" encoding="utf-8"?>
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="devmenu-overlay" class="devmenu-overlay">
        <ui:VisualElement class="devmenu-overlay__header">
            <ui:Label text="DEV MENU" class="devmenu-overlay__title" />
            <ui:Label name="devmenu-hint" class="devmenu-overlay__hint" />
        </ui:VisualElement>
        <ui:ScrollView name="devmenu-commands" class="devmenu-overlay__commands" />
        <ui:VisualElement name="devmenu-log" class="devmenu-log" />
    </ui:VisualElement>
</ui:UXML>
```

`com.routaz.devmenu/UI/Resources/DevMenuOverlay.uss`:
```css
.devmenu-overlay {
    position: absolute;
    top: 0;
    right: 0;
    bottom: 0;
    width: 420px;
    padding: 8px;
    flex-direction: column;
    background-color: rgba(20, 20, 20, 0.92);
    border-left-width: 1px;
    border-left-color: #444;
}

.devmenu-overlay__header {
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 6px;
}

.devmenu-overlay__title {
    -unity-font-style: bold;
    font-size: 15px;
    color: #eee;
}

.devmenu-overlay__hint {
    font-size: 11px;
    color: #999;
}

.devmenu-overlay__commands {
    flex-grow: 1;
}

.devmenu-overlay__group {
    margin-bottom: 4px;
}

.devmenu-log {
    flex-shrink: 0;
    min-height: 80px;
    margin-top: 6px;
    padding: 4px;
    background-color: rgba(0, 0, 0, 0.6);
    border-top-width: 1px;
    border-top-color: #444;
}

.devmenu-log__line {
    font-size: 11px;
    color: #ccc;
    white-space: normal;
}

.devmenu-log--warning {
    color: #e6c35c;
}

.devmenu-log--error {
    color: #e66a5c;
}
```

`com.routaz.devmenu/UI/DevMenuOverlay.cs`:
```csharp
using System.Collections.Generic;
using DevMenuTool.Core;
using DevMenuTool.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevMenuTool.UI
{
    /// <summary>
    /// Runtime DevMenu panel for Play Mode and development builds: toggled with <see cref="ToggleKey"/>,
    /// lists [DevMenu] commands grouped by name, and shows the last log lines. The game keeps running.
    /// Spawned by <see cref="DevMenuPlayModeInjector"/>.
    /// </summary>
    public class DevMenuOverlay : MonoBehaviour
    {
        public const int SortingOrder = 10000;

        [SerializeField] private KeyCode toggleKey = KeyCode.F1;

        private readonly DevMenuLogBuffer _log = new();
        private readonly HashSet<string> _collapsedGroups = new();
        private int _renderedLogVersion = -1;

        private UIDocument _document;
        private VisualElement _panel;
        private ScrollView _commands;
        private VisualElement _logStrip;
        private Label _hint;
        private UIBuilder _uiBuilder;

        public bool IsOpen { get; private set; }

        public KeyCode ToggleKey
        {
            get => toggleKey;
            set => toggleKey = value;
        }

        public IReadOnlyList<DevMenuLogEntry> LogEntries => _log.Snapshot();

        private void OnEnable() => Application.logMessageReceived += OnLogMessage;

        private void OnDisable()
        {
            Application.logMessageReceived -= OnLogMessage;
            DevMenuOverlayInput.Clear();
        }

        private void OnLogMessage(string message, string stackTrace, LogType type) => _log.Add(type, message);

        private void Update()
        {
            if (DevMenuInput.WasPressedThisFrame(toggleKey))
                Toggle();

            if (IsOpen)
                RenderLog(force: false);

            PublishInputState();
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public void Open()
        {
            EnsureBuilt();
            _hint.text = $"{toggleKey} to close";
            RebuildCommands();
            _panel.style.display = DisplayStyle.Flex;
            IsOpen = true;
            RenderLog(force: true);
            PublishInputState();
        }

        public void Close()
        {
            if (_panel != null)
                _panel.style.display = DisplayStyle.None;
            IsOpen = false;
            PublishInputState();
        }

        private void EnsureBuilt()
        {
            if (_document != null) return;

            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.name = "DevMenuOverlayPanelSettings";
            settings.themeStyleSheet = Resources.Load<ThemeStyleSheet>("DevMenuRuntimeTheme");
            settings.scaleMode = PanelScaleMode.ConstantPixelSize;
            settings.scale = 1f;
            settings.sortingOrder = SortingOrder;

            // Host the UIDocument on an inactive child so OnEnable runs with the panel settings already set.
            var host = new GameObject("DevMenuOverlayDocument");
            host.SetActive(false);
            host.transform.SetParent(transform, false);
            _document = host.AddComponent<UIDocument>();
            _document.panelSettings = settings;
            host.SetActive(true);

            var root = _document.rootVisualElement;
            root.pickingMode = PickingMode.Ignore;
            root.styleSheets.Add(Resources.Load<StyleSheet>("DevMenu"));
            root.styleSheets.Add(Resources.Load<StyleSheet>("DevMenuOverlay"));

            var tree = Resources.Load<VisualTreeAsset>("DevMenuOverlay").CloneTree();
            tree.pickingMode = PickingMode.Ignore;
            tree.style.position = Position.Absolute;
            tree.style.left = 0;
            tree.style.right = 0;
            tree.style.top = 0;
            tree.style.bottom = 0;
            root.Add(tree);

            _panel = root.Q<VisualElement>("devmenu-overlay");
            _commands = root.Q<ScrollView>("devmenu-commands");
            _logStrip = root.Q<VisualElement>("devmenu-log");
            _hint = root.Q<Label>("devmenu-hint");
            _panel.style.display = DisplayStyle.None;

            _uiBuilder = new UIBuilder(DevMenuContainer.MethodInvoker, DevMenuContainer.ParameterHandlerRegistry);
        }

        private void RebuildCommands()
        {
            var scanner = DevMenuContainer.MethodScanner;
            scanner.ScanScene();
            _commands.Clear();

            foreach (var group in DevMenuCommandGroups.Build(scanner.CachedMethods))
            {
                var foldout = new Foldout { text = group.Name, value = !_collapsedGroups.Contains(group.Name) };
                foldout.AddToClassList("devmenu-overlay__group");

                string groupName = group.Name;
                foldout.RegisterValueChangedCallback(evt =>
                {
                    // Parameter fields inside the group raise bool change events that bubble up here.
                    if (evt.target != foldout) return;
                    if (evt.newValue) _collapsedGroups.Remove(groupName);
                    else _collapsedGroups.Add(groupName);
                });

                foreach (var method in group.Methods)
                    foldout.Add(_uiBuilder.BuildMethodUI(method));

                _commands.Add(foldout);
            }
        }

        private void RenderLog(bool force)
        {
            if (_logStrip == null || (!force && _renderedLogVersion == _log.Version)) return;

            _renderedLogVersion = _log.Version;
            _logStrip.Clear();
            foreach (var entry in _log.Snapshot())
            {
                var line = new Label(entry.Message);
                line.AddToClassList("devmenu-log__line");
                if (entry.Type == LogType.Warning)
                    line.AddToClassList("devmenu-log--warning");
                else if (entry.Type is LogType.Error or LogType.Exception or LogType.Assert)
                    line.AddToClassList("devmenu-log--error");
                _logStrip.Add(line);
            }
        }

        private void PublishInputState()
        {
            DevMenuOverlayInput.Set(IsOpen, IsOpen && IsPointerOverPanel(), IsOpen && IsTextInputFocused());
        }

        private bool IsPointerOverPanel()
        {
            if (_panel?.panel == null || !DevMenuInput.TryGetPointerPosition(out var screen)) return false;
            var panelPosition = RuntimePanelUtils.ScreenToPanel(_panel.panel, new Vector2(screen.x, Screen.height - screen.y));
            return _panel.worldBound.Contains(panelPosition);
        }

        private bool IsTextInputFocused()
        {
            for (var element = _panel?.panel?.focusController?.focusedElement as VisualElement; element != null; element = element.parent)
            {
                if (element is TextField || element is IntegerField || element is FloatField
                    || element is LongField || element is DoubleField)
                    return true;
            }
            return false;
        }
    }
}
```

`DevMenuPlayModeInjector.cs`: inside the `#if UNITY_EDITOR || DEVELOPMENT_BUILD` block from Task 1, add a constant `private const string OverlayObjectName = "__DevMenuOverlay";`. At the end of `OnPlayModeStart`, add:
```csharp
            if (GameObject.Find(OverlayObjectName) == null)
            {
                var overlay = new GameObject(OverlayObjectName);
                overlay.AddComponent<DevMenuTool.UI.DevMenuOverlay>();
                Object.DontDestroyOnLoad(overlay);
            }
```
Don't set HideFlags on it.

- [ ] **Step 4: Run the tests and confirm they pass**

1. Run with `filter: DevMenuOverlayTests`. Expected: 6/6 pass.
2. Run the full PlayMode suite. Expected: 131/131 pass, with 0 compile errors and 0 warnings.
3. If the runtime theme import (`unity-theme://default`) logs an error in this Unity version, stop and report it. Don't swap in a different theme.

- [ ] **Step 5: Manual check in the dev project's Play Mode**

The user's Editor is the dev project.
1. Enter Play Mode through UnityMCP (`unity_play_mode`).
2. Use `unity_execute_code` to call `Object.FindFirstObjectByType<DevMenuTool.UI.DevMenuOverlay>().Open()`.
3. Take a screenshot with `unity_screenshot_game` and confirm the panel is styled: real buttons and fields, not unstyled boxes.
4. Exit Play Mode.

Attach the screenshot path to the report.

- [ ] **Step 6: Commit**

Stage:
- `com.routaz.devmenu/UI/DevMenuOverlayInput.cs` (from Task 4)
- `DevMenuOverlay.cs`
- the three Resources files
- `DevMenuPlayModeInjector.cs`
- the test file
- all new `.meta` files

Commit message: `feat: runtime DevMenu overlay (F1)`.

---

### Task 6: Docs and version

**Files:**
- Modify: `README.md`, `com.routaz.devmenu/README.md` (keep them identical), `com.routaz.devmenu/CHANGELOG.md`, `com.routaz.devmenu/package.json`

- [ ] **Step 1: Bump the version**

In `package.json`, change `"version": "0.2.0"` to `"version": "0.3.0"`. Leave `dependencies` alone: the Input System stays optional.

- [ ] **Step 2: Update the CHANGELOG**

Rename `## [Unreleased]` to `## [0.3.0] - 2026-10-15`, and add a fresh empty `## [Unreleased]` above it. Under 0.3.0, add:
- **Added:**
  - A runtime overlay for Play Mode and development builds. F1 toggles a panel on the right with commands grouped by name, editable parameters, Run buttons and the last 5 log lines. The game keeps running.
  - `DevMenuOverlayInput.IsOpen`, `IsPointerOverOverlay` and `IsTextInputFocused`, so games can keep their own input out of the way.
  - Optional Input System support: when `com.unity.inputsystem` is installed, the toggle key and pointer are read through it.
- **Changed:**
  - The hotkey listener and the overlay are only spawned in the Editor and development builds. Release builds get neither.
  - The hotkey listener's legacy `Input` calls are compiled only when the legacy Input Manager is enabled. In Input-System-only projects, per-command hotkeys are inactive, and commands are run from the overlay or the editor window instead.

- [ ] **Step 3: Update the READMEs (both copies)**

- **Requirements section:** replace the hotkey bullet with:
  - Per-command hotkeys need the legacy Input Manager (Active Input Handling *Input Manager (Old)* or *Both*).
  - The runtime overlay works with either input system. It uses the Input System package when present.
- **New section "Using the menu in builds"**, after "Opening the menu". It should explain:
  - F1 toggles the overlay in Play Mode and development builds, and never in release builds.
  - Commands are grouped by their `[DevMenu]` group name.
  - The last 5 log lines are shown.
  - The toggle key can be changed with `DevMenuOverlay.ToggleKey`; F1-F12 and backquote work with the Input System.
  - Games should use `DevMenuOverlayInput` to ignore world clicks while `IsPointerOverOverlay` is true, and typed hotkeys while `IsTextInputFocused` is true. Include a three-line code example.
- **Features list:** replace "Editor window and in-game panel" with "Editor window and an F1 overlay for development builds".
- **Package layout:** add the new files.

- [ ] **Step 4: Check and commit**

Diff the two READMEs (`git diff --no-index README.md com.routaz.devmenu/README.md`). Expected: no output.

Commit message: `docs: overlay usage; 0.3.0`.

---

### Task 7: TheLongBurn integration (other repo)

**Precondition:**
- TheLongBurn's Editor is open, and UnityMCP lists it. Select it by project name, because the DevMenuTool Editor may be on 7890 and TheLongBurn on 7891.
- TheLongBurn picks up the package from disk through `file:../../DevMenuTool/com.routaz.devmenu`, so Tasks 1-6 are already in effect there.

**Files** (in `D:/GitHubRepos/TheLongBurn`):
- Modify: `Assets/Scripts/TheLongBurn.asmdef`, `Assets/Scripts/UI/UIManager.cs` (`IsPointerOverUI`, ~L538), `Assets/Scripts/UI/PauseMenuUI.cs` (~L56)

- [ ] **Step 1: Branch**

The working tree may contain the user's unrelated uncommitted changes, which carry over a branch switch untouched. Don't stash or commit them. If another code agent is running in that checkout, wait for it first.

```bash
git -C /d/GitHubRepos/TheLongBurn switch -c feat/devmenu-overlay main
```

- [ ] **Step 2: Wire it in**

1. In `TheLongBurn.asmdef`, add `"DevMenuTool"` to `references`.
2. At the top of `UIManager.IsPointerOverUI()`, add:
```csharp
            if (DevMenuTool.UI.DevMenuOverlayInput.IsPointerOverOverlay) return true;
```
3. In `PauseMenuUI`, change the escape check at ~L56 so that Escape typed while editing an overlay field doesn't open the pause menu:
```csharp
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame
                && !DevMenuTool.UI.DevMenuOverlayInput.IsTextInputFocused)
```

- [ ] **Step 3: Compile and test in TheLongBurn**

This exercises the Input System branch of `DevMenuInput`.
1. Recompile and confirm 0 errors. A failure here, such as `Unity.InputSystem` not resolving or the `Key` enum names not matching, is a package bug: report it.
2. Run the EditMode suite. Expected: the known baseline, with the only failure being `CombatHudLayoutTests.HudStack_IsCentred_ClearOfTheShipLog`.
3. Commit the three files with the message `feat: keep game input out of the DevMenu overlay`.

- [ ] **Step 4: Manual check**

The user does this, or it's done with their go-ahead. Back up Saves first.
1. In Play Mode, press F1. Expected: the overlay opens with TheLongBurn's command groups (Carriers, Docking, …).
2. Run "Carriers → Depart In Seconds". Expected: its log line shows in the strip.
3. Click a button in the overlay over a station. Expected: the station is not selected.
4. Type in a parameter field and press Escape. Expected: the pause menu doesn't open.
5. Make a development build and repeat 1-2.
6. Make a release build. Expected: F1 does nothing, and no `__DevMenuOverlay` object exists.
