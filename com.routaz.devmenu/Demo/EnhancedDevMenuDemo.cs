using UnityEngine;
using DevMenuTool.Core.Attributes;

namespace DevMenuTool.Demo
{
    /// <summary>
    /// Demo class showcasing the enhanced DevMenu parameter system.
    /// This demonstrates how parameters are stored per method and persist between sessions.
    /// </summary>
    public class EnhancedDevMenuDemo : MonoBehaviour
    {
        [Header("Demo Settings")]
        public string demoMessage = "Hello from Enhanced DevMenu!";
        public int demoValue = 42;

        #region DevMenu Methods

        [DevMenu(KeyCode.F1, "Enhanced Demo")]
        private void DemoBasicParameters(int amount = 100, string message = "default")
        {
            Debug.Log($"DemoBasicParameters called with: amount={amount}, message={message}");
            // The parameters will be stored in the DevMethod and persist between sessions
        }

        [DevMenu(KeyCode.F2, "Enhanced Demo")]
        private void DemoMultipleTypes(int health = 100, float speed = 5.5f, bool isActive = true, TestEnum status = TestEnum.Idle)
        {
            Debug.Log($"DemoMultipleTypes called with: health={health}, speed={speed}, isActive={isActive}, status={status}");
            // All parameter types are supported and will be stored
        }

        [DevMenu(KeyCode.F3, "Enhanced Demo")]
        private void DemoNoParameters()
        {
            Debug.Log("DemoNoParameters called - no parameters to store");
            // Methods without parameters work normally
        }

        [DevMenu(KeyCode.F4, "Enhanced Demo")]
        private void DemoComplexParameters(string playerName = "Player", int level = 1, float experience = 0.0f)
        {
            Debug.Log($"DemoComplexParameters called with: playerName={playerName}, level={level}, experience={experience}");
            // These values will be remembered and used for hotkey calls
        }

        #endregion

        #region Helper Methods

        [DevMenu(KeyCode.F5, "Enhanced Demo")]
        private void ResetAllParameters()
        {
            Debug.Log("All parameter values have been reset to defaults!");
            // In the UI, you can click "Reset to Defaults" for individual methods
        }

        [DevMenu(KeyCode.F6, "Enhanced Demo")]
        private void ShowCurrentValues()
        {
            Debug.Log($"Current demo values: message='{demoMessage}', value={demoValue}");
        }

        #endregion

        #region Enums

        public enum TestEnum
        {
            Idle,
            Active,
            Paused,
            Completed
        }

        #endregion
    }
} 