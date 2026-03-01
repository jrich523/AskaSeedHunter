using UnityEngine;

namespace SeedHunter.Utils
{
    /// <summary>
    /// Centralized state management for all SeedHunter features
    /// Tracks feature toggles and original values for restoration
    /// </summary>
    public static class StateManager
    {
        // Feature toggle flags
        public static bool MapRevealed { get; set; } = false;
        public static bool FastMovementEnabled { get; set; } = false;
        public static bool NoclipEnabled { get; set; } = false;

        // Speed control - adjustable with +/- keys
        public const float MIN_SPEED = 5f;      // Default base speed (don't go below this)
        public const float MAX_SPEED = 100f;    // Maximum allowed speed
        public const float SPEED_INCREMENT = 5f; // How much +/- keys adjust speed
        private static float _currentSpeed = 50f; // Default to 10x (5 base * 10)

        public static float CurrentSpeed
        {
            get => _currentSpeed;
            set => _currentSpeed = Mathf.Clamp(value, MIN_SPEED, MAX_SPEED);
        }

        // Movement constants
        public const float JUMP_MULTIPLIER = 2f;
        public const float NOCLIP_VERTICAL_SPEED = 25f; // Base vertical flying speed
        public const float SPEED_MULTIPLIER = 10f; // Used for vertical flying speed boost

        // Original values for restoration
        public static float OriginalMoveSpeed { get; set; } = -1f;
        public static float OriginalJumpForce { get; set; } = -1f;
        public static float OriginalGravity { get; set; } = -9.81f;
        public static float OriginalColliderRadius { get; set; } = -1f;
        public static float OriginalColliderHeight { get; set; } = -1f;

        /// <summary>
        /// Increase movement speed by increment, clamped to max
        /// </summary>
        public static void IncreaseSpeed()
        {
            CurrentSpeed += SPEED_INCREMENT;
        }

        /// <summary>
        /// Decrease movement speed by increment, clamped to min (base speed)
        /// </summary>
        public static void DecreaseSpeed()
        {
            CurrentSpeed -= SPEED_INCREMENT;
        }

        /// <summary>
        /// Reset all features to their original state
        /// </summary>
        public static void ResetAll()
        {
            MapRevealed = false;
            FastMovementEnabled = false;
            NoclipEnabled = false;

            // Note: CurrentSpeed is NOT reset - it persists across toggles
            // This allows the user to keep their preferred speed setting
        }

        /// <summary>
        /// Check if any cheat features are currently active
        /// </summary>
        public static bool AnyFeatureActive()
        {
            return FastMovementEnabled || NoclipEnabled;
        }
    }
}
