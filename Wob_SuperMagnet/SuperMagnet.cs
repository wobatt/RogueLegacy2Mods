using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace Wob_SuperMagnet {
    [BepInPlugin( "Wob.SuperMagnet", "Super Magnet Mod", "0.1.0" )]
    public partial class SuperMagnet : BaseUnityPlugin {
		// Main method that kicks everything off
		private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobPlugin.Settings.Add( new WobSettings.Entry<float>( "DistanceScaler", "Multiply magnet distance by this", 1f, bounds: (0f, 1000000f) ) );
            WobPlugin.Settings.Add( new WobSettings.Scaled<int>( "BossDamage", "Resolving insights gives bonus damage of this percent", 15, 0.01f, bounds: (0, 1000000) ) );
			// Apply the patches if the mod is enabled
			WobPlugin.Patch();
        }

		// Patch for the method that gets the gold cost for a specific upgrade with labour costs included
		[HarmonyPatch( typeof( RuneLogicHelper ), nameof( RuneLogicHelper.GetMagnetDistance ) )]
        static class RuneLogicHelper_GetMagnetDistance_Patch {
            static void Postfix( ref float __result ) {
                // Calculate the new cost and overwrite the original return value
                __result *= WobPlugin.Settings.Get( "DistanceScaler", 1f );
            }
        }

        [HarmonyPatch( typeof( CaveLanternPostProcessingController ), "DarknessAmountWhenFullyLit", MethodType.Getter )]
        static class CaveLanternPostProcessingController_DarknessAmountWhenFullyLit_Patch {
            static void Postfix( ref float __result ) {
                __result = 0f;
            }
        }

        // Apply damage dealt modifiers on boss fights
        [HarmonyPatch( typeof( EnemyController ), "GetInsightPlayerDamageMod" )]
        static class EnemyController_GetInsightPlayerDamageMod_Patch {
            static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "EnemyController.GetInsightPlayerDamageMod Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4, Insight_EV.INSIGHT_PLAYER_DAMAGE_MOD ), // 1.15f
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ret                                          ), // return 1.15f
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 0, ( WobPlugin.Settings.Get( "BossDamage", 0.15f ) + 1f ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

    }
}