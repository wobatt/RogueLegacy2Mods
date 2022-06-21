using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_ResourceGain {
    [BepInPlugin( "Wob.ResourceGain", "Resource Gain Mod", "1.1.0" )]
    public partial class GoldGain : BaseUnityPlugin {
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Num<int>(   "GoldGain",       "Gain +X% gold on all characters",                                0,  0.01f, bounds: (0,  1000000)  ),
                new WobSettings.Num<int>(   "OreGain",        "Gain +X% ore",                                                   0,  0.01f, bounds: (0,  1000000)  ),
                new WobSettings.Num<int>(   "AetherGain",     "Gain +X% aether",                                                0,  0.01f, bounds: (0,  1000000)  ),
                new WobSettings.Num<float>( "GoldMultiply",   "Multiply gold gain by this after all other bonuses are added",   1f,        bounds: (0f, 1000000f) ),
                new WobSettings.Num<float>( "OreMultiply",    "Multiply ore gain by this after all other bonuses are added",    1f,        bounds: (0f, 1000000f) ),
                new WobSettings.Num<float>( "AetherMultiply", "Multiply aether gain by this after all other bonuses are added", 1f,        bounds: (0f, 1000000f) ),
            } );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        [HarmonyPatch( typeof( SkillTreeLogicHelper ), nameof( SkillTreeLogicHelper.GetGoldGainMod ) )]
        internal static class SkillTreeLogicHelper_GetGoldGainMod_Patch {
            internal static void Postfix( ref float __result ) {
                __result += WobSettings.Get( "GoldGain", 0f );
            }
        }

        [HarmonyPatch( typeof( SkillTreeLogicHelper ), nameof( SkillTreeLogicHelper.GetEquipmentOreMod ) )]
        internal static class SkillTreeLogicHelper_GetEquipmentOreMod_Patch {
            internal static void Postfix( ref float __result ) {
                __result += WobSettings.Get( "OreGain", 0f );
            }
        }

        [HarmonyPatch( typeof( SkillTreeLogicHelper ), nameof( SkillTreeLogicHelper.GetRuneOreMod ) )]
        internal static class SkillTreeLogicHelper_GetRuneOreMod_Patch {
            internal static void Postfix( ref float __result ) {
                __result += WobSettings.Get( "AetherGain", 0f );
            }
        }

        [HarmonyPatch( typeof( Economy_EV ), nameof( Economy_EV.GetOreGainMod ) )]
        internal static class Economy_EV_GetOreGainMod_Patch {
            internal static void Postfix( ref float __result ) {
                __result = ( WobSettings.Get( "OreMultiply", 1f ) * ( __result + 1f ) ) - 1f;
            }
        }

        [HarmonyPatch( typeof( Economy_EV ), nameof( Economy_EV.GetRuneOreGainMod ) )]
        internal static class Economy_EV_GetRuneOreGainMod_Patch {
            internal static void Postfix( ref float __result ) {
                __result = ( WobSettings.Get( "AetherMultiply", 1f ) * ( __result + 1f ) ) - 1f;
            }
        }

        [HarmonyPatch( typeof( CoinDrop ), "Collect" )]
        internal static class CoinDrop_Collect_Patch {
            // Patch to remove 'this.m_specialItemDropsList.Clear()' from the start of the method so we can add extra items
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "CoinDrop.Collect Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Call, name: "GetGoldGainMod" ), // Economy_EV.GetGoldGainMod()
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 0, SymbolExtensions.GetMethodInfo( () => NewGetGoldGainMod() ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static float NewGetGoldGainMod() {
                return ( WobSettings.Get( "GoldMultiply", 1f ) * ( Economy_EV.GetGoldGainMod() + 1f ) ) - 1f;
            }
        }
    }
}