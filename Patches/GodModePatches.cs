using HarmonyLib;
using System;
using SSSGame;
using SSSGame.Controllers;

namespace SeedHunter.Patches
{
    /// <summary>
    /// Harmony patches for god mode (invincibility, no damage)
    /// </summary>
    [HarmonyPatch]
    public class GodModePatches
    {
        // Note: _velocityY is a public property in IL2CPP, no reflection needed

        /// <summary>
        /// Block all damage (including fall damage) when fast movement or noclip is enabled
        /// </summary>
        [HarmonyPatch(typeof(SSSGame.Character), "TakeDamage")]
        [HarmonyPrefix]
        public static bool BlockAllDamage(SSSGame.Character __instance)
        {
            // Block damage if any cheat is active and this is the player
            if ((Utils.StateManager.FastMovementEnabled || Utils.StateManager.NoclipEnabled) && __instance.IsPlayer())
            {
                return false; // Block all damage
            }
            return true; // Allow damage for NPCs or when cheats are off
        }

        /// <summary>
        /// Intercept CurrentHealth setter to prevent health from decreasing
        /// This catches fall damage which bypasses TakeDamage()
        /// </summary>
        [HarmonyPatch(typeof(SSSGame.Character), "CurrentHealth", MethodType.Setter)]
        [HarmonyPrefix]
        public static bool PreventHealthDecrease(SSSGame.Character __instance, ref float value)
        {
            // Exit immediately if god mode isn't active
            if (!Utils.StateManager.FastMovementEnabled && !Utils.StateManager.NoclipEnabled)
                return true;

            // Only for player
            if (!__instance.IsPlayer())
                return true;

            try
            {
                // Get current health
                float currentHealth = __instance.CurrentHealth;

                // If new value is less than current (taking damage), block it
                if (value < currentHealth)
                {
                    value = currentHealth; // Keep health the same
                    return true; // Still call setter but with unchanged value
                }

                // Allow health increases (healing, food, etc)
                return true;
            }
            catch (Exception ex)
            {
                SeedHunterPlugin.Log.LogError($"Error in PreventHealthDecrease: {ex.Message}");
                return true;
            }
        }

        /// <summary>
        /// Prevent fall damage by clamping velocity before grounding check
        /// Fall damage is calculated based on how fast you're falling when you hit the ground
        /// </summary>
        [HarmonyPatch(typeof(SSSGame.Controllers.CharacterMovement), "CheckGrounded")]
        [HarmonyPrefix]
        public static void PreventFallDamage(SSSGame.Controllers.CharacterMovement __instance)
        {
            try
            {
                // Only for player and when cheats are active
                if (!Utils.StateManager.FastMovementEnabled && !Utils.StateManager.NoclipEnabled)
                    return;

                // Only if this is the player's CharacterMovement
                if (!__instance.TryGetComponent<SSSGame.PlayerCharacter>(out var playerChar))
                    return;

                // Clamp downward velocity to prevent fall damage calculation
                float currentVelocity = __instance._velocityY;
                if (currentVelocity < -5f)
                {
                    __instance._velocityY = -5f; // Safe landing speed
                }
            }
            catch (Exception ex)
            {
                SeedHunterPlugin.Log.LogError($"Error in PreventFallDamage: {ex.Message}");
            }
        }
    }
}
