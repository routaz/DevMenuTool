using UnityEngine;
using DevMenuTool.Core.Attributes;
using DevMenuTool.Core.Interfaces;

namespace DevMenuTool.Sample
{
    // Drop this component on an empty GameObject and enter Play Mode.
    // Every method marked with [DevMenu] shows up in the dev menu and can be
    // triggered by its hotkey or from the UI. It exercises each capability the
    // tool offers: plain actions, typed parameters, validation, and change events.
    public class DevMenuSample : MonoBehaviour, IParameterChangeHandler
    {
        public enum Weather { Clear, Rain, Snow, Fog }

        private int _gold;
        private string _playerName = "Hero";

        // --- Plain action, no parameters -------------------------------------

        [DevMenu(KeyCode.F1, "General")]
        private void LogState()
        {
            Debug.Log($"[Sample] Gold={_gold}, Player={_playerName}, TimeScale={Time.timeScale}");
        }

        // --- Every supported parameter type ----------------------------------

        [DevMenu(KeyCode.F2, "General")]
        private void SetEverything(
            int level = 1,
            float speed = 5f,
            bool invincible = false,
            string label = "default",
            Weather weather = Weather.Clear)
        {
            Debug.Log($"[Sample] level={level}, speed={speed}, invincible={invincible}, " +
                      $"label={label}, weather={weather}");
        }

        // --- Validation: numeric range ---------------------------------------

        [DevMenu(KeyCode.G, "Cheats")]
        private void AddGold([ParameterRange(0, 9999)] int amount = 100)
        {
            _gold += amount;
            Debug.Log($"[Sample] Added {amount} gold (total {_gold}).");
        }

        // --- Validation: string length / required ----------------------------

        [DevMenu(KeyCode.N, "Player")]
        private void SetPlayerName(
            [StringValidation(minLength: 2, maxLength: 16)] string newName = "Hero")
        {
            _playerName = newName;
            Debug.Log($"[Sample] Player renamed to {newName}.");
        }

        [DevMenu(KeyCode.I, "Player")]
        private void GiveItem([Required] string itemId, [ParameterRange(1, 99)] int quantity = 1)
        {
            Debug.Log($"[Sample] Gave {quantity}x '{itemId}'.");
        }

        // --- Validation: float range -----------------------------------------

        [DevMenu(KeyCode.T, "World")]
        private void SetTimeScale([ParameterRange(0.1, 2.0)] float scale = 1f)
        {
            Time.timeScale = scale;
            Debug.Log($"[Sample] TimeScale set to {scale}.");
        }

        // --- Parameter change events -----------------------------------------
        // Implementing IParameterChangeHandler lets you react whenever a value is
        // edited in the menu (e.g. to mirror it into live game state or a HUD).

        public void OnParameterChanged(object sender, ParameterChangeEventArgs e)
        {
            Debug.Log($"[Sample] {e.MethodName}.{e.ParameterName}: {e.OldValue} -> {e.NewValue}");
        }
    }
}
