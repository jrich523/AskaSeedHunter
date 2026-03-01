using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using SeedHunter.Utils;

namespace SeedHunter
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class SeedHunterPlugin : BasePlugin
    {
        internal static new ManualLogSource Log;
        internal static SeedHunterPlugin Instance;
        private Harmony _harmony;

        // Configuration entries for persistent settings
        private static ConfigEntry<bool> configMapRevealed;
        private static ConfigEntry<bool> configFastMovement;
        private static ConfigEntry<bool> configNoclip;
        private static ConfigEntry<float> configSpeed;

        public override void Load()
        {
            Instance = this;
            Log = base.Log;

            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loading...");

            // Initialize configuration and load saved settings
            InitializeConfig();

            // Initialize Harmony and apply all patches
            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

            try
            {
                _harmony.PatchAll();

                // Log what was patched
                var patchedMethods = _harmony.GetPatchedMethods();
                int count = 0;
                foreach (var method in patchedMethods)
                {
                    Log.LogInfo($"Patched: {method.DeclaringType?.Name}.{method.Name}");
                    count++;
                }
                Log.LogInfo($"Applied {count} Harmony patches");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"Failed to apply Harmony patches: {ex.Message}");
                Log.LogError($"Stack trace: {ex.StackTrace}");
            }

            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            Log.LogInfo("Hotkeys:");
            Log.LogInfo("  F8  - Toggle continuous map reveal (caves, objectives, locations)");
            Log.LogInfo("  F9  - Toggle fast movement + infinite stamina + god mode");
            Log.LogInfo("  F10 - Toggle noclip/fly mode (Space=up, Ctrl=down) + god mode");
            Log.LogInfo("  F7  - Disable all cheats");
            Log.LogInfo("  +/- - Increase/decrease movement speed (range: 5-100, default: 50)");
            Log.LogInfo("NOTE: God mode (invincibility) is auto-enabled with F9 or F10");

            // Hook into Unity's update loop for hotkey detection
            AddComponent<HotkeyManager>();

            // Note: StatusOverlay will be added by HotkeyManager once player is in-game
            // This prevents load delays during initial startup
        }

        public override bool Unload()
        {
            _harmony?.UnpatchSelf();
            return true;
        }

        /// <summary>
        /// Initialize configuration and load saved settings
        /// </summary>
        private void InitializeConfig()
        {
            // Create config entries
            configMapRevealed = Config.Bind(
                "Features",
                "MapRevealed",
                false,
                "Enable continuous map reveal (caves, objectives, locations)"
            );

            configFastMovement = Config.Bind(
                "Features",
                "FastMovement",
                false,
                "Enable fast movement + infinite stamina + god mode"
            );

            configNoclip = Config.Bind(
                "Features",
                "Noclip",
                false,
                "Enable noclip/fly mode + god mode"
            );

            configSpeed = Config.Bind(
                "Movement",
                "Speed",
                50f,
                new ConfigDescription(
                    "Movement speed multiplier",
                    new AcceptableValueRange<float>(StateManager.MIN_SPEED, StateManager.MAX_SPEED)
                )
            );

            // Load saved settings into StateManager
            StateManager.MapRevealed = configMapRevealed.Value;
            StateManager.FastMovementEnabled = configFastMovement.Value;
            StateManager.NoclipEnabled = configNoclip.Value;
            StateManager.CurrentSpeed = configSpeed.Value;

            Log.LogInfo("Configuration loaded:");
            Log.LogInfo($"  Map Reveal: {StateManager.MapRevealed}");
            Log.LogInfo($"  Fast Movement: {StateManager.FastMovementEnabled}");
            Log.LogInfo($"  Noclip: {StateManager.NoclipEnabled}");
            Log.LogInfo($"  Speed: {StateManager.CurrentSpeed}");
        }

        /// <summary>
        /// Save current settings to config file
        /// </summary>
        internal static void SaveConfig()
        {
            if (Instance == null) return;

            configMapRevealed.Value = StateManager.MapRevealed;
            configFastMovement.Value = StateManager.FastMovementEnabled;
            configNoclip.Value = StateManager.NoclipEnabled;
            configSpeed.Value = StateManager.CurrentSpeed;

            Instance.Config.Save();
        }

        /// <summary>
        /// Manually reveal all caves by directly accessing CavesManager
        /// </summary>
        internal static void RevealAllCaves()
        {
            try
            {
                // Find CavesManager instance
                var cavesManager = UnityEngine.Object.FindObjectOfType<SSSGame.CavesManager>();
                if (cavesManager == null) return;

                // Access _explorationHandlers property
                var handlers = cavesManager._explorationHandlers;
                if (handlers == null || handlers.Count == 0) return;

                int revealed = 0;
                foreach (var kvp in handlers)
                {
                    var handler = kvp.Value;
                    if (handler == null || handler.data == null) continue;
                    if (handler.data.explored) continue;

                    // Set explored state on CaveData
                    handler.data.SetExploredState(true);

                    // Trigger handler's exploration changed callback
                    handler._OnCaveExplorationChanged(true);

                    // Refresh marker state (same as CaveFinder mod)
                    handler._RefreshMarkerState();

                    revealed++;
                }

                // Only log when we actually reveal something new
                if (revealed > 0)
                {
                    SeedHunterPlugin.Log.LogInfo($"✓ Revealed {revealed} new caves!");
                }
            }
            catch (System.Exception ex)
            {
                SeedHunterPlugin.Log.LogError($"Error revealing caves: {ex.Message}");
            }
        }

        /// <summary>
        /// Reveal all objectives and locations on the map
        /// </summary>
        internal static void RevealAllObjectives()
        {
            try
            {
                // TODO: Find ObjectiveManager and LocationManager
                // For now, just a placeholder that won't error

                // This will be implemented once we find the right managers
            }
            catch (System.Exception ex)
            {
                SeedHunterPlugin.Log.LogError($"Error revealing objectives: {ex.Message}");
            }
        }
    } // End of SeedHunterPlugin class

    /// <summary>
    /// MonoBehaviour component for detecting hotkeys
    /// </summary>
    public class HotkeyManager : MonoBehaviour
    {
        private float lastRevealAttempt = 0f;
        private static bool overlayCreated = false;
        private static float lastOverlayCheck = 0f;

        private void Update()
        {
            // Create overlay once player is in-game (prevents load delays)
            if (!overlayCreated && Time.time - lastOverlayCheck > 2f)
            {
                lastOverlayCheck = Time.time;
                var player = UnityEngine.Object.FindObjectOfType<SSSGame.PlayerCharacter>();
                if (player != null && SeedHunterPlugin.Instance != null)
                {
                    try
                    {
                        // Use plugin's AddComponent method for IL2CPP compatibility
                        SeedHunterPlugin.Instance.AddComponent<UI.StatusOverlay>();
                        SeedHunterPlugin.Log.LogInfo("Status overlay created (player in-game)");
                        overlayCreated = true;
                    }
                    catch (System.Exception ex)
                    {
                        SeedHunterPlugin.Log.LogError($"Failed to create overlay: {ex.Message}");
                        overlayCreated = true; // Don't keep trying
                    }
                }
            }

            // Continuously reveal caves/objectives as they load while enabled
            if (StateManager.MapRevealed)
            {
                // Try every 2 seconds (less spam)
                if (Time.time - lastRevealAttempt >= 2f)
                {
                    lastRevealAttempt = Time.time;

                    // Reveal caves
                    SeedHunterPlugin.RevealAllCaves();

                    // Reveal objectives
                    SeedHunterPlugin.RevealAllObjectives();
                }
            }

            // F8 - Reveal map (caves, objectives, locations)
            if (Input.GetKeyDown(KeyCode.F8))
            {
                StateManager.MapRevealed = !StateManager.MapRevealed;
                SeedHunterPlugin.SaveConfig(); // Save setting

                if (StateManager.MapRevealed)
                {
                    SeedHunterPlugin.Log.LogInfo("Map Reveal: ENABLED - Continuously revealing map objects");
                    lastRevealAttempt = 0f; // Try immediately
                }
                else
                {
                    SeedHunterPlugin.Log.LogInfo("Map Reveal: DISABLED");
                }
            }

            // F9 - Fast movement and infinite stamina
            if (Input.GetKeyDown(KeyCode.F9))
            {
                StateManager.FastMovementEnabled = !StateManager.FastMovementEnabled;
                SeedHunterPlugin.SaveConfig(); // Save setting

                if (StateManager.FastMovementEnabled)
                {
                    SeedHunterPlugin.Log.LogInfo($"Fast Movement: ENABLED (Speed: {StateManager.CurrentSpeed:F0}, use +/- to adjust)");
                }
                else
                {
                    SeedHunterPlugin.Log.LogInfo("Fast Movement: DISABLED");
                }
            }

            // F10 - Noclip/fly mode
            if (Input.GetKeyDown(KeyCode.F10))
            {
                StateManager.NoclipEnabled = !StateManager.NoclipEnabled;
                SeedHunterPlugin.SaveConfig(); // Save setting

                if (StateManager.NoclipEnabled)
                {
                    SeedHunterPlugin.Log.LogInfo("Noclip/Fly: ENABLED (SPACE=up, CTRL=down)");
                }
                else
                {
                    SeedHunterPlugin.Log.LogInfo("Noclip/Fly: DISABLED");
                }
            }

            // F7 - Reset all cheats
            if (Input.GetKeyDown(KeyCode.F7))
            {
                StateManager.ResetAll();
                SeedHunterPlugin.SaveConfig(); // Save setting
                SeedHunterPlugin.Log.LogInfo("All cheats DISABLED");
            }

            // + (Equals key) - Increase movement speed
            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                StateManager.IncreaseSpeed();
                SeedHunterPlugin.SaveConfig(); // Save setting
                SeedHunterPlugin.Log.LogInfo($"Movement Speed: {StateManager.CurrentSpeed:F0}");
            }

            // - (Minus key) - Decrease movement speed
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                StateManager.DecreaseSpeed();
                SeedHunterPlugin.SaveConfig(); // Save setting
                SeedHunterPlugin.Log.LogInfo($"Movement Speed: {StateManager.CurrentSpeed:F0}");
            }
        }
    }

    /// <summary>
    /// Plugin metadata - auto-generated or manually defined
    /// </summary>
    internal static class MyPluginInfo
    {
        public const string PLUGIN_GUID = "com.jrich523.askaseedhunter";
        public const string PLUGIN_NAME = "SeedHunter";
        public const string PLUGIN_VERSION = "1.0.0";
    }
}
