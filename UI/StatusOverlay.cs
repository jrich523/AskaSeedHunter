using UnityEngine;

namespace SeedHunter.UI
{
    /// <summary>
    /// In-game status overlay using Unity's OnGUI system
    /// Shows current mod status in the top-left corner
    /// </summary>
    public class StatusOverlay : MonoBehaviour
    {
        private GUIStyle _boxStyle;
        private GUIStyle _textStyle;
        private bool _stylesInitialized = false;
        private Rect _windowRect = new Rect(10, 10, 180, 160); // Compact width

        // Cache player reference to avoid expensive FindObjectOfType every frame
        private static SSSGame.PlayerCharacter _cachedPlayer;
        private static float _lastPlayerCheck = 0f;
        private const float PLAYER_CHECK_INTERVAL = 2f; // Check every 2 seconds (was 1)
        private static bool _hasCheckedOnce = false; // Don't check during initial load

        private void OnGUI()
        {
            // Only show overlay when in-game (cached check)
            if (!IsPlayerInGame())
            {
                return; // Not in-game yet, don't show overlay
            }

            if (!_stylesInitialized)
            {
                InitializeStyles();
            }

            // Draw the status box
            GUI.Box(_windowRect, "", _boxStyle);

            // Draw the status text
            GUILayout.BeginArea(new Rect(_windowRect.x + 10, _windowRect.y + 10, _windowRect.width - 20, _windowRect.height - 20));

            GUILayout.Label("═══ SEEDHUNTER ═══", _textStyle);
            GUILayout.Space(5);

            // Map Reveal status with hotkey
            string mapStatus = Utils.StateManager.MapRevealed
                ? "Map Reveal (F8): ON"
                : "Map Reveal (F8): OFF";
            GUILayout.Label(mapStatus, _textStyle);

            // Fast Movement status with hotkey
            string moveStatus = Utils.StateManager.FastMovementEnabled
                ? $"Fast Move (F9): ON"
                : "Fast Move (F9): OFF";
            GUILayout.Label(moveStatus, _textStyle);

            // Noclip status with hotkey
            string noclipStatus = Utils.StateManager.NoclipEnabled
                ? "Noclip (F10): ON"
                : "Noclip (F10): OFF";
            GUILayout.Label(noclipStatus, _textStyle);

            GUILayout.Space(5);

            // Show actual speed: base speed (5) when off, CurrentSpeed when on
            float displaySpeed = Utils.StateManager.FastMovementEnabled
                ? Utils.StateManager.CurrentSpeed
                : Utils.StateManager.MIN_SPEED;
            GUILayout.Label($"Speed: {displaySpeed:F0} (+/- to adjust)", _textStyle);

            GUILayout.EndArea();
        }

        private void InitializeStyles()
        {
            try
            {
                // Box style - semi-transparent black background
                _boxStyle = new GUIStyle(GUI.skin.box);
                _boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.75f));

                // Text style - bright green console-like text
                _textStyle = new GUIStyle(GUI.skin.label);
                _textStyle.normal.textColor = new Color(0.4f, 1f, 0.4f); // Light green
                _textStyle.fontSize = 13;
                // Remove FontStyle and TextAnchor to avoid needing TextRenderingModule

                _stylesInitialized = true;
                SeedHunterPlugin.Log.LogInfo("Status overlay styles initialized");
            }
            catch (System.Exception ex)
            {
                SeedHunterPlugin.Log.LogError($"Error initializing overlay styles: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a simple colored texture for backgrounds
        /// </summary>
        private Texture2D MakeTex(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Efficiently check if player is in-game (cached with periodic refresh)
        /// </summary>
        private bool IsPlayerInGame()
        {
            // Don't check during early game startup (first 5 seconds) to avoid load delays
            if (!_hasCheckedOnce && Time.time < 5f)
            {
                return false;
            }

            // Check if we need to refresh the cached player reference
            if (Time.time - _lastPlayerCheck > PLAYER_CHECK_INTERVAL || (_cachedPlayer == null && _hasCheckedOnce))
            {
                _cachedPlayer = UnityEngine.Object.FindObjectOfType<SSSGame.PlayerCharacter>();
                _lastPlayerCheck = Time.time;
                _hasCheckedOnce = true;
            }

            return _cachedPlayer != null;
        }
    }
}
