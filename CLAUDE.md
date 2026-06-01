# CLAUDE.md

Behavioral guidelines to reduce common LLM coding mistakes. Merge with project-specific instructions as needed.

## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

## 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

---

**These guidelines are working if:** fewer unnecessary changes in diffs, fewer rewrites due to overcomplication, and clarifying questions come before implementation rather than after mistakes.

## Project Overview

The DevToolMenu is the prototype for development tool used in UNITY game engine. The project is intended to be shared as UnityPackage.

## Coding Rules

1. Write clean code.
2. Follow SOLID principles
3. Ask before implementing.
4. Always write/Update README.MD with latest changes/feature
5. Keep CHANGELOG.md up to date.


# DevMenu Tool - Enhanced Unity Debug Menu System

A powerful, modular in-game debug menu system for Unity that uses attributes, reflection, and UI Toolkit to create developer-friendly debug commands.

## 🚀 Features

### **Enhanced Parameter System** ⭐ **NEW**
- **Parameter Storage**: Each method stores its own parameter values
- **Automatic Persistence**: Values persist between Unity sessions via EditorPrefs
- **Reset Functionality**: "Reset to Defaults" button for each method
- **Smart Parameter Resolution**: UI updates stored values, hotkeys use stored values

### **Core Features**
- **Attribute-Based**: Simple `[DevMenu]` attribute to mark debug methods
- **Hotkey Support**: Assign keyboard shortcuts to debug methods
- **UI Toolkit Integration**: Modern, responsive UI built with UI Toolkit
- **Parameter Support**: Full support for all parameter types (int, float, bool, enum, string)
- **Editor Integration**: Works in both Play Mode and Edit Mode
- **Project Agnostic**: Designed as a Unity package for easy distribution

## 📦 Installation

1. **Download the package** from the releases page
2. **Import into Unity** via Package Manager or Asset Store
3. **Add the DevMenu component** to any GameObject in your scene
4. **Start using** with the `[DevMenu]` attribute!

## 🎯 Quick Start

### **Basic Usage**

```csharp
using DevMenuTool.Core.Attributes;
using UnityEngine;

public class GameDebugger : MonoBehaviour
{
    [DevMenu(KeyCode.G, "Cheats")]
    private void AddGold(int amount = 100)
    {
        Debug.Log($"Added {amount} gold!");
        // This method will appear in the DevMenu with a hotkey
    }

    [DevMenu(KeyCode.H, "Player")]
    private void SetPlayerHealth(int health = 100, bool invincible = false)
    {
        Debug.Log($"Set health to {health}, invincible: {invincible}");
        // Parameters are stored and remembered between sessions
    }
}
```

### **Opening the Menu**

- **Runtime**: Press the configured hotkey or use the UI
- **Editor**: Go to `DevMenu > Open Dev Menu` in the menu bar

## 🔧 Enhanced Parameter System

The DevMenu tool now features an **enhanced parameter system** that provides better control and persistence:

### **How It Works**

1. **Parameter Storage**: Each `DevMethod` stores its own parameter values
2. **Automatic Persistence**: Values are saved to EditorPrefs automatically
3. **Smart Resolution**: UI updates stored values, hotkeys use stored values
4. **Reset Functionality**: Easy reset to default values

### **Parameter Flow**

```mermaid
graph TD
    A[DevMethod Created] --> B[Load from EditorPrefs]
    B --> C{Values Found?}
    C -->|Yes| D[Use Stored Values]
    C -->|No| E[Use Default Values]
    D --> F[UI Shows Current Values]
    E --> F
    F --> G[User Modifies Values]
    G --> H[Update Stored Parameters]
    H --> I[Save to EditorPrefs]
    I --> J[Method Invocation]
    J --> K[Use Stored Values]
```

### **Button Click vs Hotkey**

| Action | Parameter Source | Behavior |
|--------|------------------|----------|
| **UI Button** | Current UI field values | Updates stored parameters, then invokes |
| **Hotkey** | Stored parameter values | Uses last-saved values, no UI interaction |

## 🎮 Usage Examples

### **Basic Debug Methods**

```csharp
[DevMenu(KeyCode.F1, "Debug")]
private void LogPlayerPosition()
{
    var player = GameObject.FindGameObjectWithTag("Player");
    if (player != null)
        Debug.Log($"Player position: {player.transform.position}");
}

[DevMenu(KeyCode.F2, "Debug")]
private void ShowGameStats()
{
    Debug.Log($"FPS: {1.0f / Time.deltaTime:F1}");
    Debug.Log($"Memory: {System.GC.GetTotalMemory(false) / 1024 / 1024}MB");
}
```

### **Parameterized Methods**

```csharp
[DevMenu(KeyCode.G, "Cheats")]
private void AddGold(int amount = 100)
{
    // amount will be stored and remembered
    PlayerGold += amount;
    Debug.Log($"Added {amount} gold! Total: {PlayerGold}");
}

[DevMenu(KeyCode.H, "Player")]
private void SetPlayerStats(int health = 100, float speed = 5.0f, bool invincible = false)
{
    // All parameters are stored independently
    playerHealth = health;
    playerSpeed = speed;
    playerInvincible = invincible;
    Debug.Log($"Set stats: Health={health}, Speed={speed}, Invincible={invincible}");
}
```

### **Enum Parameters**

```csharp
public enum GameState { Menu, Playing, Paused, GameOver }

[DevMenu(KeyCode.J, "Game")]
private void SetGameState(GameState state = GameState.Playing)
{
    currentGameState = state;
    Debug.Log($"Game state set to: {state}");
}
```

## 🏗️ Architecture

The DevMenu tool follows **SOLID principles** and **Clean Code** practices:

### **Core Components**

- **`DevMethod`**: Represents a debug method with parameter storage
- **`MethodScanner`**: Discovers methods with `[DevMenu]` attributes
- **`MethodInvoker`**: Handles method invocation with parameter resolution
- **`UIBuilder`**: Creates UI elements for method interaction
- **`DevMenuContainer`**: Simple DI container for service management

### **Design Patterns**

- **Factory Pattern**: `DevMethodFactory` creates `DevMethod` instances
- **Strategy Pattern**: `ParameterHandlerRegistry` handles different parameter types
- **Dependency Injection**: `DevMenuContainer` manages service instances

## 🧪 Testing

The tool includes comprehensive unit tests:

```bash
# Run tests in Unity
Window > General > Test Runner
```

### **Test Categories**

- **Core Tests**: Method scanning, caching, and factory tests
- **Runtime Tests**: Parameter handling and method invocation tests
- **Integration Tests**: End-to-end workflow tests
- **Enhanced Tests**: Parameter storage and persistence tests

## 📁 Project Structure

```
com.routaz.devmenu/
├── Core/                    # Core interfaces and implementations
│   ├── Interfaces/         # Interface definitions
│   ├── DevMethod.cs       # Enhanced DevMethod with parameter storage
│   ├── MethodScanner.cs   # Method discovery
│   └── DevMenuContainer.cs # DI container
├── Runtime/                # Runtime functionality
│   ├── MethodInvoker.cs   # Method invocation
│   ├── Handlers/          # Parameter type handlers
│   └── DevMenuHotkeyListener.cs
├── UI/                     # UI Toolkit components
│   ├── UIBuilder.cs       # UI construction
│   └── DevMenuUIController.cs
├── Editor/                 # Editor-specific functionality
│   └── DevMenuEditorWindow.cs
├── Demo/                   # Example implementations
│   └── EnhancedDevMenuDemo.cs
└── Tests/                  # Unit tests
    └── EnhancedDevMethodTests.cs
```

## 🔄 Migration from Previous Versions

The enhanced parameter system is **fully backward compatible**:

- ✅ Existing code works without changes
- ✅ Parameter persistence still works
- ✅ UI behavior is improved but familiar
- ✅ Hotkey behavior is more predictable

## 🚀 Advanced Features

### **Custom Parameter Types**

The system supports custom parameter types through the `ParameterHandlerRegistry`:

```csharp
// Custom parameter handler example
public class Vector3ParameterHandler : IParameterHandler
{
    public bool CanHandle(Type parameterType) => parameterType == typeof(Vector3);
    
    public object Parse(string input, Type targetType)
    {
        // Parse "x,y,z" format
        var parts = input.Split(',');
        return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
    }
    
    public string ToString(object value)
    {
        var vector = (Vector3)value;
        return $"{vector.x},{vector.y},{vector.z}";
    }
}
```

### **Parameter Validation**

You can add validation to your parameters:

```csharp
[DevMenu(KeyCode.G, "Cheats")]
private void AddGold(int amount = 100)
{
    if (amount < 0)
    {
        Debug.LogError("Amount cannot be negative!");
        return;
    }
    
    PlayerGold += amount;
    Debug.Log($"Added {amount} gold!");
}
```

## 🤝 Contributing

1. **Fork the repository**
2. **Create a feature branch**
3. **Make your changes**
4. **Add tests for new functionality**
5. **Submit a pull request**

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- **Unity UI Toolkit** for modern UI components
- **NUnit** for comprehensive testing framework
- **SOLID Principles** for clean architecture guidance

---

**Made with ❤️ for Unity developers**

For more information about the enhanced parameter system, see [ENHANCED_PARAMETER_SYSTEM.md](ENHANCED_PARAMETER_SYSTEM.md). 