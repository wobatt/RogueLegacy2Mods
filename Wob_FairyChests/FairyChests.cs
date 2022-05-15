using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_FairyChests {
    [BepInPlugin( "Wob.FairyChests", "Fairy Chests Mod", "0.1.0" )]
    public partial class FairyChests : BaseUnityPlugin {
        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        /*[HarmonyPatch( typeof( FairyRoomController ), nameof( FairyRoomController.State ), MethodType.Setter )]
        static class FairyRoomController_State_Patch {
            static void Prefix( FairyRoomController __instance, ref FairyRoomState value ) {
                if( value == FairyRoomState.Failed ) {
                    value = FairyRoomState.NotRunning;
                }
            }
        }*/

        /*[HarmonyPatch( typeof( FairyRoomController ), "PlayerFailed" )]
        static class FairyRoomController_PlayerFailed_Patch {
            static void Prefix( FairyRoomController __instance, ref bool setRoomComplete ) {
                setRoomComplete = false;
            }
        }*/

        [HarmonyPatch( typeof( ChestObj ), nameof( ChestObj.SetChestLockState ) )]
        static class ChestObj_SetChestLockState_Patch {
            static void Prefix( ChestObj __instance, ref ChestLockState lockState ) {
                if( lockState == ChestLockState.Failed ) {
                    lockState = ChestLockState.Unlocked;
                }
            }
        }

        // Patch for the method that auto-fails the task on exit
        /*[HarmonyPatch( typeof( FairyRoomController ), "OnPlayerExitRoom" )]
        static class FairyRoomController_OnPlayerExitRoom_Patch {
            static void Prefix( FairyRoomController __instance ) {
                if( __instance.State == FairyRoomState.Running || __instance.State == FairyRoomState.Failed ) {
                    Traverse.Create( __instance ).Property( "State" ).SetValue( FairyRoomState.NotRunning );
                    Traverse.Create( __instance ).Method( "RunAllFairyRules", new System.Type[] { typeof( bool ) } ).GetValue( new object[] { false } );
                    Traverse.Create( __instance ).Method( "ResetAllFairyRules" ).GetValue();
                    __instance.Chest.SetChestLockState( __instance.FairyRoomRuleEntries.Any( ( FairyRoomRuleEntry entry ) => entry.FairyRule.LockChestAtStart ) ? ChestLockState.Locked : ChestLockState.Unlocked );
                    GameObject m_roomTriggerWall = (GameObject)Traverse.Create( __instance ).Field( "m_roomTriggerWall" ).GetValue();
                    if( m_roomTriggerWall ) { m_roomTriggerWall.SetActive( false ); }
                }
            }
        }*/
    }
}