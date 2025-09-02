using UnityEngine;
using DevMenuTool.Core.Attributes;

public class DevTester : MonoBehaviour
{
    [DevMenu(KeyCode.T, "Testing")]
    private void Hello()
    {
        Debug.Log("Hello from DevMenu!");
    }

    [DevMenu(KeyCode.Y, "Testing")]
    private void Goodbye()
    {
        Debug.Log("Goodbye from DevMenu!");
    }

    [DevMenu(KeyCode.U, "Currency")]
    private void AddCurrency(int amount = 1000)
    {
        Debug.Log($"Adding currency...{amount}");
    }

    [DevMenu(KeyCode.I, "Gems")]
    private void AddGems(int amount)
    {
        Debug.Log($"Adding gems...{amount}");
    }

    [DevMenu(KeyCode.O, "Floats")]
    private void AddFloat(float amount)
    {
        Debug.Log($"Adding float...{amount}");
    }

    [DevMenu(KeyCode.P, "Toggles")]
    private void ToggleFeature(bool enabled)
    {
        Debug.Log($"Feature is now: {(enabled ? "Enabled" : "Disabled")}");
    }

    [DevMenu(KeyCode.K, "Enums")]
    private void SetDifficulty(Difficulty difficulty = Difficulty.Medium)
    {
        Debug.Log($"Difficulty set to: {difficulty}");
    }

    [DevMenu(KeyCode.L, "Strings")]
    private void RenamePlayer(string playerName = "Unnamed")
    {
        Debug.Log($"Player renamed to: {playerName}");
    }

    [DevMenu(KeyCode.M, "Multi-Params")]
    private void SpawnEnemy(string type = "Goblin", int count = 3, bool alert = false)
    {
        Debug.Log($"Spawning {count} {type}(s). Alerted? {alert}");
    }
}

public enum Difficulty
{
    Easy,
    Medium,
    Hard,
    Nightmare
}