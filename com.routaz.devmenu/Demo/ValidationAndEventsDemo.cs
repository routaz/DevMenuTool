using UnityEngine;
using DevMenuTool.Core.Attributes;
using DevMenuTool.Core.Interfaces;

namespace DevMenuTool.Demo
{
    /// <summary>
    /// Demo class showcasing parameter validation and change events.
    /// This demonstrates how to add validation rules and respond to parameter changes.
    /// </summary>
    public class ValidationAndEventsDemo : MonoBehaviour, IParameterChangeHandler
    {
        [Header("Demo Settings")]
        public int currentHealth = 100;
        public string playerName = "Player";

        private void Start()
        {
            // Register this class as a parameter change handler
            // This will be done automatically when methods are scanned
        }

        #region DevMenu Methods with Validation

        [DevMenu(KeyCode.F1, "Health Management")]
        private void SetPlayerHealth(
            [ParameterRange(0, 100)] int health = 100,
            [ParameterRange(0, 10)] float regenerationRate = 1.0f)
        {
            currentHealth = health;
            Debug.Log($"Set player health to {health} with regeneration rate {regenerationRate}");
        }

        [DevMenu(KeyCode.F2, "Player Settings")]
        private void SetPlayerSettings(
            [StringValidation(minLength: 2, maxLength: 20)] string name = "Player",
            [ParameterRange(1, 100)] int level = 1,
            [ParameterRange(0.1, 10.0)] float speed = 5.0f)
        {
            playerName = name;
            Debug.Log($"Set player: {name}, Level: {level}, Speed: {speed}");
        }

        [DevMenu(KeyCode.F3, "Inventory Management")]
        private void AddItemToInventory(
            [Required] string itemName,
            [ParameterRange(1, 999)] int quantity = 1,
            [ParameterRange(0, 100)] float durability = 100.0f)
        {
            Debug.Log($"Added {quantity}x {itemName} with {durability}% durability");
        }

        [DevMenu(KeyCode.F4, "Game Settings")]
        private void SetGameSettings(
            [ParameterRange(0.1, 2.0)] float timeScale = 1.0f,
            [ParameterRange(0, 100)] int difficulty = 50,
            [StringValidation(pattern: @"^[A-Za-z0-9_]+$")] string saveSlot = "slot1")
        {
            Time.timeScale = timeScale;
            Debug.Log($"Set game: TimeScale={timeScale}, Difficulty={difficulty}, SaveSlot={saveSlot}");
        }

        #endregion

        #region Parameter Change Event Handler

        public void OnParameterChanged(object sender, ParameterChangeEventArgs e)
        {
            Debug.Log($"Parameter changed: {e.MethodName}.{e.ParameterName} = {e.OldValue} → {e.NewValue}");
            
            // You can add custom logic here, such as:
            // - Updating UI elements
            // - Saving to different storage
            // - Triggering other game events
            // - Logging for debugging
            
            // Example: Update UI when health changes
            if (e.MethodName == "SetPlayerHealth" && e.ParameterName == "health")
            {
                UpdateHealthUI((int)e.NewValue);
            }
        }

        private void UpdateHealthUI(int newHealth)
        {
            // This would update the health UI in a real game
            Debug.Log($"Health UI updated to: {newHealth}");
        }

        #endregion

        #region Helper Methods

        [DevMenu(KeyCode.F5, "Test Validation")]
        private void TestValidation()
        {
            Debug.Log("Try changing parameter values in the UI to see validation in action!");
            Debug.Log("Invalid values will show warnings and won't be applied.");
        }

        [DevMenu(KeyCode.F6, "Show Current State")]
        private void ShowCurrentState()
        {
            Debug.Log($"Current state: Health={currentHealth}, Player={playerName}");
        }

        #endregion
    }
} 