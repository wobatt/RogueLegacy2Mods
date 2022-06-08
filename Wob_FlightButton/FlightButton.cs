using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using MoreMountains.CorgiEngine;
using Wob_Common;

namespace Wob_FlightButton {
    [BepInPlugin( "Wob.FlightButton", "Icarus' Wings Activation Mod", "1.0.0" )]
    public partial class FlightButton : BaseUnityPlugin {
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Disable standard activation of Icarus' Wings relic
        [HarmonyPatch( typeof( CharacterJump_RL ), "EvaluateJumpConditions" )]
        internal static class CharacterJump_RL_EvaluateJumpConditions_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "CharacterJump_RL.EvaluateJumpConditions Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldsfld, name: "PlayerSaveData"     ), // SaveManager.PlayerSaveData
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, RelicType.FlightBonusCurse ), // RelicType.FlightBonusCurse
                            /*  2 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "GetRelic"         ), // SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse)
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_Level"        ), // SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse).Level
                            /*  4 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4_0                           ), // 0
                            /*  5 */ new WobTranspiler.OpTest( OpCodeSet.Ble                              ), // if (SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse).Level > 0)
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetInstruction( 4, OpCodes.Ldc_I4, int.MaxValue ), // Change test to Level > int.MaxValue, which is impossible
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Enable assistance flight button press for Icarus' Wings relic
        [HarmonyPatch( typeof( CharacterFlight_RL ), "HandleInput" )]
        internal static class CharacterFlight_RL_HandleInput_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "CharacterFlight_RL.HandleInput Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldsfld, name: "PlayerSaveData"           ), // SaveManager.PlayerSaveData
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldfld, name: "EnableHouseRules"          ), // SaveManager.PlayerSaveData.EnableHouseRules
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                                ), // if (SaveManager.PlayerSaveData.EnableHouseRules)
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Ldsfld, name: "PlayerSaveData"           ), // SaveManager.PlayerSaveData
                            /*  4 */ new WobTranspiler.OpTest( OpCodes.Ldfld, name: "Assist_EnableFlightToggle" ), // SaveManager.PlayerSaveData.Assist_EnableFlightToggle
                            /*  5 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                                ), // if (SaveManager.PlayerSaveData.Assist_EnableFlightToggle)
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetInstruction( 0, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => CheckFlightConditions() ) ), // New override method call
                            new WobTranspiler.OpAction_Remove( 1, 4 ), // Remove the rest until the final branch
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool CheckFlightConditions() {
                return ( SaveManager.PlayerSaveData.EnableHouseRules && SaveManager.PlayerSaveData.Assist_EnableFlightToggle ) || ( SaveManager.PlayerSaveData.GetRelic( RelicType.FlightBonusCurse ).Level > 0 );
            }
        }
    }
}