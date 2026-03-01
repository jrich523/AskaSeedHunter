using HarmonyLib;
using System;
using UnityEngine;
using SSSGame;

namespace SeedHunter.Patches
{
    /// <summary>
    /// Direct position manipulation for noclip flying
    /// Working version: fly up, float down
    /// </summary>
    [HarmonyPatch]
    public class NoclipPatches_DirectPosition
    {
        private static bool collisionsDisabled = false;
        private static float baseVerticalSpeed = 25f;
        // Note: _velocityY is a public property in IL2CPP, no reflection needed

        /// <summary>
        /// Handle flying by directly modifying position
        /// This approach bypasses physics entirely
        /// </summary>
        [HarmonyPatch(typeof(SSSGame.PlayerCharacter), "Update")]
        [HarmonyPostfix]
        public static void PlayerCharacter_Update_Postfix(SSSGame.PlayerCharacter __instance)
        {
            try
            {
                if (!Utils.StateManager.NoclipEnabled)
                {
                    // Restore collisions when disabled
                    if (collisionsDisabled)
                    {
                        var controller = __instance.GetComponent<CharacterController>();
                        if (controller != null)
                        {
                            controller.detectCollisions = true;
                            collisionsDisabled = false;
                        }
                    }
                    return;
                }

                // Disable collisions (do this once)
                if (!collisionsDisabled)
                {
                    var controller = __instance.GetComponent<CharacterController>();
                    if (controller != null)
                    {
                        controller.detectCollisions = false;
                        collisionsDisabled = true;
                    }
                }

                // Get movement component and counteract gravity
                var movement = __instance.GetCharacterMovement();
                if (movement != null)
                {
                    movement._velocityY = 0f;
                }

                // Direct position manipulation for vertical movement
                var transform = __instance.transform;
                Vector3 currentPos = transform.position;
                float deltaY = 0f;

                // Use CurrentSpeed for vertical movement
                float verticalSpeed = Utils.StateManager.FastMovementEnabled
                    ? Utils.StateManager.CurrentSpeed
                    : baseVerticalSpeed;

                if (Input.GetKey(KeyCode.Space))
                {
                    // Fly up - apply upward position change
                    deltaY = verticalSpeed * Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    // Fly down
                    deltaY = -verticalSpeed * Time.deltaTime;
                }

                // Apply vertical movement
                if (Math.Abs(deltaY) > 0.001f)
                {
                    Vector3 newPos = new Vector3(currentPos.x, currentPos.y + deltaY, currentPos.z);
                    transform.position = newPos;
                }
            }
            catch (Exception ex)
            {
                SeedHunterPlugin.Log.LogError($"Error in PlayerCharacter_Update_Postfix (DirectPosition): {ex.Message}");
            }
        }
    }
}
