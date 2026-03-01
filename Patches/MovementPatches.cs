using HarmonyLib;
using System;
using UnityEngine;
using SSSGame;
using SSSGame.Controllers;

namespace SeedHunter.Patches
{
    /// <summary>
    /// Harmony patches for fast movement and infinite stamina
    /// </summary>
    [HarmonyPatch]
    public class MovementPatches
    {
        private static float cachedOriginalSpeed = -1f;

        /// <summary>
        /// Block stamina drain when fast movement is enabled
        /// Based on "Echoes of the Seer Creative Mode" mod approach
        /// </summary>
        [HarmonyPatch(typeof(SSSGame.Character), "DrainStamina")]
        [HarmonyPrefix]
        public static bool BlockStaminaDrain(SSSGame.Character __instance)
        {
            // Only block for player characters when fast movement is enabled
            if (!Utils.StateManager.FastMovementEnabled)
                return true; // Allow normal stamina drain

            // Check if this is the player
            if (__instance.IsPlayer())
            {
                return false; // Block stamina drain for player
            }

            return true; // Allow stamina drain for NPCs/villagers
        }

        /// <summary>
        /// Increase player movement speed when fast movement is enabled
        /// Path: PlayerCharacter → GetCharacterMovement() → parameters.speed
        /// Uses CurrentSpeed which can be adjusted with +/- keys
        /// </summary>
        [HarmonyPatch(typeof(SSSGame.PlayerCharacter), "Update")]
        [HarmonyPostfix]
        public static void ModifyPlayerSpeed(SSSGame.PlayerCharacter __instance)
        {
            try
            {
                // Get CharacterMovement component
                var characterMovement = __instance.GetCharacterMovement();
                if (characterMovement == null || characterMovement.parameters == null)
                    return;

                var movementStats = characterMovement.parameters;

                // Cache original speed on first access
                if (cachedOriginalSpeed < 0)
                {
                    cachedOriginalSpeed = movementStats.speed;
                    SeedHunterPlugin.Log.LogInfo($"Cached original movement speed: {cachedOriginalSpeed}");
                }

                if (!Utils.StateManager.FastMovementEnabled)
                {
                    // Restore original speed if disabled
                    if (Math.Abs(movementStats.speed - cachedOriginalSpeed) > 0.1f)
                    {
                        movementStats.speed = cachedOriginalSpeed;
                        characterMovement.parameters = movementStats; // Reassign in case it's a struct
                    }
                    return;
                }

                // Apply custom speed (adjustable with +/- keys)
                float targetSpeed = Utils.StateManager.CurrentSpeed;
                if (Math.Abs(movementStats.speed - targetSpeed) > 0.1f)
                {
                    movementStats.speed = targetSpeed;
                    characterMovement.parameters = movementStats; // Reassign in case it's a struct
                }
            }
            catch (Exception ex)
            {
                SeedHunterPlugin.Log.LogError($"Error in ModifyPlayerSpeed: {ex.Message}");
            }
        }
    }
}
