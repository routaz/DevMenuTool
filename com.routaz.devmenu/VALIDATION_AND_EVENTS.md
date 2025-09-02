# Parameter Validation & Change Events

## Overview

The DevMenu tool now includes **parameter validation** and **change events** to provide better control and monitoring of parameter values. This system allows you to:

- **Validate parameter values** before they are applied
- **Respond to parameter changes** with custom logic
- **Prevent invalid values** from being stored
- **Monitor parameter usage** for debugging and analytics

## Parameter Validation

### **Built-in Validators**

#### **RangeValidator**
Validates numeric parameters within a specified range.

```csharp
// Add range validation (0-100, inclusive)
var validator = new RangeValidator(0, 100);
devMethod.AddValidator(0, validator);

// Add range validation (0-100, exclusive)
var validator = new RangeValidator(0, 100, inclusive: false);
devMethod.AddValidator(0, validator);
```

#### **StringValidator**
Validates string parameters with length and pattern constraints.

```csharp
// String with length constraints
var validator = new StringValidator(minLength: 2, maxLength: 20);
devMethod.AddValidator(0, validator);

// String with regex pattern
var validator = new StringValidator(pattern: @"^[A-Za-z0-9_]+$");
devMethod.AddValidator(0, validator);

// Required string (no empty/null)
var validator = new StringValidator(allowEmpty: false);
devMethod.AddValidator(0, validator);
```

### **Using Validation Attributes**

The easiest way to add validation is using attributes on your method parameters:

```csharp
[DevMenu(KeyCode.G, "Cheats")]
private void AddGold(
    [ParameterRange(0, 999)] int amount = 100,
    [StringValidation(minLength: 2, maxLength: 20)] string reason = "Debug")
{
    Debug.Log($"Added {amount} gold for reason: {reason}");
}

[DevMenu(KeyCode.H, "Player")]
private void SetPlayerStats(
    [ParameterRange(0, 100)] int health = 100,
    [ParameterRange(0.1, 10.0)] float speed = 5.0f,
    [Required] string playerName = "Player")
{
    Debug.Log($"Set {playerName}: Health={health}, Speed={speed}");
}
```

### **Custom Validators**

You can create custom validators by implementing `IParameterValidator`:

```csharp
public class CustomValidator : IParameterValidator
{
    public ParameterValidationResult Validate(string parameterName, object value, Type parameterType)
    {
        if (value == null)
            return ParameterValidationResult.Success();

        // Your custom validation logic here
        if (value is int intValue && intValue < 0)
        {
            return ParameterValidationResult.Failure($"Parameter '{parameterName}' cannot be negative.");
        }

        return ParameterValidationResult.Success();
    }
}

// Usage
var validator = new CustomValidator();
devMethod.AddValidator(0, validator);
```

### **Validation Management**

```csharp
// Add a validator
devMethod.AddValidator(parameterIndex, validator);

// Remove a specific validator
devMethod.RemoveValidator(parameterIndex, validator);

// Remove all validators for a parameter
devMethod.ClearValidators(parameterIndex);

// Validate a value manually
var result = devMethod.ValidateParameter(parameterIndex, value);
if (!result.IsValid)
{
    Debug.LogWarning(result.ErrorMessage);
}
```

## Parameter Change Events

### **Event System Overview**

The parameter change event system allows you to respond to parameter value changes:

```csharp
public class GameManager : MonoBehaviour, IParameterChangeHandler
{
    public void OnParameterChanged(object sender, ParameterChangeEventArgs e)
    {
        Debug.Log($"Parameter changed: {e.MethodName}.{e.ParameterName} = {e.OldValue} → {e.NewValue}");
        
        // Custom logic based on parameter changes
        if (e.MethodName == "SetPlayerHealth" && e.ParameterName == "health")
        {
            UpdateHealthUI((int)e.NewValue);
        }
    }
}
```

### **Event Arguments**

The `ParameterChangeEventArgs` contains:

- **`MethodName`**: Name of the method that owns the parameter
- **`ParameterName`**: Name of the parameter that changed
- **`OldValue`**: Previous parameter value
- **`NewValue`**: New parameter value
- **`ParameterType`**: Type of the parameter

### **Event Subscription**

You can subscribe to parameter change events in multiple ways:

#### **1. Implement IParameterChangeHandler (Recommended)**
```csharp
public class MyComponent : MonoBehaviour, IParameterChangeHandler
{
    public void OnParameterChanged(object sender, ParameterChangeEventArgs e)
    {
        // Handle parameter changes
    }
}
```

#### **2. Subscribe to the Event**
```csharp
devMethod.ParameterChanged += OnParameterChanged;

private void OnParameterChanged(object sender, ParameterChangeEventArgs e)
{
    // Handle parameter changes
}
```

#### **3. Add/Remove Handlers Programmatically**
```csharp
var handler = new MyParameterChangeHandler();
devMethod.AddParameterChangeHandler(handler);
devMethod.RemoveParameterChangeHandler(handler);
```

### **Automatic Registration**

Components that implement `IParameterChangeHandler` are automatically registered when their methods are scanned by the `MethodScanner`.

## Usage Examples

### **Complete Example with Validation and Events**

```csharp
using UnityEngine;
using DevMenuTool.Core.Attributes;
using DevMenuTool.Core.Interfaces;

public class GameDebugger : MonoBehaviour, IParameterChangeHandler
{
    [Header("Game State")]
    public int playerHealth = 100;
    public float gameSpeed = 1.0f;
    public string currentLevel = "Level1";

    #region DevMenu Methods with Validation

    [DevMenu(KeyCode.F1, "Health Management")]
    private void SetPlayerHealth(
        [ParameterRange(0, 100)] int health = 100,
        [ParameterRange(0, 10)] float regenerationRate = 1.0f)
    {
        playerHealth = health;
        Debug.Log($"Set player health to {health} with regeneration rate {regenerationRate}");
    }

    [DevMenu(KeyCode.F2, "Game Settings")]
    private void SetGameSettings(
        [ParameterRange(0.1, 2.0)] float timeScale = 1.0f,
        [ParameterRange(0, 100)] int difficulty = 50,
        [StringValidation(pattern: @"^[A-Za-z0-9_]+$")] string saveSlot = "slot1")
    {
        Time.timeScale = timeScale;
        Debug.Log($"Set game: TimeScale={timeScale}, Difficulty={difficulty}, SaveSlot={saveSlot}");
    }

    [DevMenu(KeyCode.F3, "Add Item")]
    private void AddItemToInventory(
        [Required] string itemName,
        [ParameterRange(1, 999)] int quantity = 1,
        [ParameterRange(0, 100)] float durability = 100.0f)
    {
        Debug.Log($"Added {quantity}x {itemName} with {durability}% durability");
    }

    #endregion

    #region Parameter Change Event Handler

    public void OnParameterChanged(object sender, ParameterChangeEventArgs e)
    {
        Debug.Log($"Parameter changed: {e.MethodName}.{e.ParameterName} = {e.OldValue} → {e.NewValue}");
        
        // Custom logic based on parameter changes
        switch (e.MethodName)
        {
            case "SetPlayerHealth" when e.ParameterName == "health":
                UpdateHealthUI((int)e.NewValue);
                break;
                
            case "SetGameSettings" when e.ParameterName == "timeScale":
                UpdateTimeScaleUI((float)e.NewValue);
                break;
                
            case "AddItemToInventory" when e.ParameterName == "itemName":
                UpdateInventoryUI((string)e.NewValue);
                break;
        }
    }

    #endregion

    #region UI Update Methods

    private void UpdateHealthUI(int newHealth)
    {
        // Update health UI elements
        Debug.Log($"Health UI updated to: {newHealth}");
    }

    private void UpdateTimeScaleUI(float newTimeScale)
    {
        // Update time scale UI elements
        Debug.Log($"Time scale UI updated to: {newTimeScale}");
    }

    private void UpdateInventoryUI(string newItemName)
    {
        // Update inventory UI elements
        Debug.Log($"Inventory UI updated for item: {newItemName}");
    }

    #endregion
}
```

### **Advanced Validation Example**

```csharp
[DevMenu(KeyCode.F4, "Complex Validation")]
private void ComplexValidationExample(
    [ParameterRange(0, 100)] int health = 100,
    [StringValidation(minLength: 3, maxLength: 15, pattern: @"^[A-Za-z\s]+$")] string playerName = "Player",
    [ParameterRange(0.1, 5.0)] float speed = 1.0f,
    [Required] string itemName = "Sword")
{
    Debug.Log($"Complex validation passed: {playerName} has {health} health, {speed} speed, wielding {itemName}");
}
```

## Benefits

### **✅ Validation Benefits**

1. **Data Integrity**: Prevents invalid values from being stored
2. **User Feedback**: Clear error messages for invalid inputs
3. **Type Safety**: Ensures parameters meet expected constraints
4. **Debugging**: Helps identify parameter-related issues early

### **✅ Event Benefits**

1. **Reactive Programming**: Respond to parameter changes automatically
2. **UI Updates**: Keep UI in sync with parameter changes
3. **Analytics**: Track parameter usage and changes
4. **Debugging**: Monitor parameter changes for debugging

### **✅ Integration Benefits**

1. **Automatic Registration**: Handlers are registered automatically
2. **Attribute-Based**: Easy validation with attributes
3. **Extensible**: Custom validators and handlers
4. **Backward Compatible**: Existing code works without changes

## Best Practices

### **Validation Best Practices**

1. **Use Attributes When Possible**: They're declarative and easy to understand
2. **Provide Clear Error Messages**: Help users understand what went wrong
3. **Validate Early**: Catch issues before they cause problems
4. **Keep Validators Simple**: Each validator should have a single responsibility

### **Event Best Practices**

1. **Handle Exceptions**: Wrap event handlers in try-catch blocks
2. **Keep Handlers Lightweight**: Don't do heavy work in event handlers
3. **Use Switch Statements**: For complex parameter change logic
4. **Unsubscribe Properly**: Clean up event subscriptions when done

### **Performance Considerations**

1. **Validation Caching**: Validators are created once and reused
2. **Event Optimization**: Events are only triggered when values actually change
3. **Memory Management**: Handlers are automatically cleaned up
4. **Lazy Loading**: Validation only occurs when needed

## Testing

The validation and events system includes comprehensive tests:

```csharp
[Test]
public void ValidateParameter_WithValidValue_ShouldReturnSuccess()
{
    var validator = new RangeValidator(0, 100);
    _devMethod.AddValidator(0, validator);
    
    var result = _devMethod.ValidateParameter(0, 50);
    Assert.That(result.IsValid, Is.True);
}

[Test]
public void SetParameter_ShouldTriggerParameterChangedEvent()
{
    _devMethod.SetParameter(0, 50);
    
    Assert.That(_eventTriggered, Is.True);
    Assert.That(_lastEventArgs.NewValue, Is.EqualTo(50));
}
```

## Conclusion

The parameter validation and change events system provides powerful tools for ensuring data integrity and responding to parameter changes. It integrates seamlessly with the existing DevMenu system while adding robust validation and event capabilities.

For more information about the enhanced parameter system, see [ENHANCED_PARAMETER_SYSTEM.md](ENHANCED_PARAMETER_SYSTEM.md). 