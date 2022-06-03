using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace Wob_FairyChests {
    [BepInPlugin( "Wob.FairyChests", "Fairy Chests Mod", "1.0.0" )]
    public partial class FairyChests : BaseUnityPlugin {
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }
        
        // Patch to prevent the room state being set to failed
        [HarmonyPatch( typeof( FairyRoomController ), nameof( FairyRoomController.State ), MethodType.Setter )]
        internal static class FairyRoomController_State_Patch {
            internal static void Prefix( ref FairyRoomState value ) {
                if( value == FairyRoomState.Failed ) {
                    value = FairyRoomState.NotRunning;
                }
            }
        }

        // Patch to reset rules instead of fail them
        [HarmonyPatch( typeof( FairyRoomController ), "SetAllFairyRulesFailed" )]
        internal static class FairyRoomController_SetAllFairyRulesFailed_Patch {
            internal static bool Prefix( FairyRoomController __instance ) {
                Traverse.Create( __instance ).Method( "ResetAllFairyRules" ).GetValue();
                return false;
            }
        }

        // Patch for the method that sets the failure state
        [HarmonyPatch( typeof( FairyRoomController ), "PlayerFailed" )]
        internal static class FairyRoomController_PlayerFailed_Patch {
            internal static bool Prefix( FairyRoomController __instance ) {
                // Reset room state to not running
                Traverse.Create( __instance ).Property( "State" ).SetValue( FairyRoomState.NotRunning );
                // Stop checking the rules
                Traverse.Create( __instance ).Method( "RunAllFairyRules", new System.Type[] { typeof( bool ) } ).GetValue( new object[] { false } );
                // Make the chest invisible and prevent opening
                __instance.Chest.Interactable.SetIsInteractableActive( false );
                __instance.Chest.SetOpacity( 0f );
                // This deactivates enemies in the room, but not completely - commander effects still fire
                //Traverse.Create( __instance ).Method( "DeactivateAllEnemies" ).GetValue();
                // Prevent the original method from running
                return false;
            }
        }

        // Patch for the method that auto-fails the task on exit
        [HarmonyPatch( typeof( FairyRoomController ), "OnPlayerExitRoom" )]
        internal static class FairyRoomController_OnPlayerExitRoom_Patch {
            internal static void Prefix( FairyRoomController __instance ) {
                if( __instance.State != FairyRoomState.Passed ) {
                    // Reset room state to not running
                    Traverse.Create( __instance ).Property( "State" ).SetValue( FairyRoomState.NotRunning );
                    // Reset any failed rules
                    Traverse.Create( __instance ).Method( "ResetAllFairyRules" ).GetValue();
                    // Stop checking the rules
                    Traverse.Create( __instance ).Method( "RunAllFairyRules", new System.Type[] { typeof( bool ) } ).GetValue( new object[] { false } );
                    // Make the chest invisible and prevent opening
                    __instance.Chest.Interactable.SetIsInteractableActive( false );
                    __instance.Chest.SetOpacity( 0f );
                    // Reenable blocking walls if they exist in this room
                    GameObject m_roomTriggerWall = (GameObject)Traverse.Create( __instance ).Field( "m_roomTriggerWall" ).GetValue();
                    if( m_roomTriggerWall ) { m_roomTriggerWall.SetActive( true ); }
                }
            }
        }

        // Remove marking the room complete on save while running
        [HarmonyPatch( typeof( FairyRoomController ), "OnRoomDataSaved" )]
        internal static class FairyRoomController_OnRoomDataSaved_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "FairyRoomController.OnRoomDataSaved Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            // base.IsRoomComplete = true;
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                          ), // base
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4_1                         ), // true
                            /*  2 */ new WobTranspiler.OpTest( OpCodes.Call, name: "set_IsRoomComplete" ), // base.IsRoomComplete = true
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_Remove( 0, 3 ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

    }
}