using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace Wob_SoulShop {
    [BepInPlugin( "Wob.SoulShop", "Soul Shop Mod", "1.0.0" )]
    public partial class SoulShop : BaseUnityPlugin {
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Num<float>( "SwapScaler", "Multiply Strange Transaction costs by this value", 1f,  bounds: (0f, 100f)   ),
                new WobSettings.Num<int>(   "SwapSouls",  "Number of souls given by Strange Transaction",     150, bounds: (1, 1000000) ),
            } );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        [HarmonyPatch( typeof( Souls_EV ), nameof( Souls_EV.GetSoulSwapCost ) )]
        internal static class Souls_EV_GetSoulSwapCost_Patch {
            private static bool runOnce = false;
            internal static void Prefix() {
                if( !runOnce ) {
                    for( int i = 0; i < Souls_EV.ORE_AETHER_TO_SOUL_COST_LEVELS.Length; i++ ) {
                        Souls_EV.ORE_AETHER_TO_SOUL_COST_LEVELS[i] = Math.Max( 1, Mathf.RoundToInt( Souls_EV.ORE_AETHER_TO_SOUL_COST_LEVELS[i] * WobSettings.Get( "SwapScaler", 1f ) ) );
                    }
                    runOnce = true;
                }
            }
        }

        [HarmonyPatch( typeof( SoulShopOmniUIDescriptionBoxEntry ), "DisplayDescriptionBox" )]
        internal static class SoulShopOmniUIDescriptionBoxEntry_DisplayDescriptionBox_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "SoulShopOmniUIDescriptionBoxEntry.DisplayDescriptionBox Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, Souls_EV.SOULS_PER_LEVEL ), // 150
                            /*  1 */ new WobTranspiler.OpTest( OpCodeSet.Stloc                          ), // int num = 150
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 0, WobSettings.Get( "SwapSouls", 150 ) ), // Set the new number of souls
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        [HarmonyPatch( typeof( SoulShopOmniUIIncrementResourceButton ), "UpdateState" )]
        internal static class SoulShopOmniUIIncrementResourceButton_UpdateState_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "SoulShopOmniUIIncrementResourceButton.UpdateState Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldsfld, name: "SoulTransferLevel" ), // SoulTransferLevel
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, Souls_EV.SOULS_PER_LEVEL  ), // 150
                            /*  2 */ new WobTranspiler.OpTest( OpCodes.Mul                               ), // SoulTransferLevel * 150
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( "SwapSouls", 150 ) ), // Set the new number of souls
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        [HarmonyPatch( typeof( SoulShopOmniUISoulSwapBuyButton ), "ConfirmTransfer" )]
        internal static class SoulShopOmniUISoulSwapBuyButton_ConfirmTransfer_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "SoulShopOmniUISoulSwapBuyButton.ConfirmTransfer Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, Souls_EV.SOULS_PER_LEVEL ), // 150
                            /*  1 */ new WobTranspiler.OpTest( OpCodeSet.Stloc                          ), // int num = 150
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 0, WobSettings.Get( "SwapSouls", 150 ) ), // Set the new number of souls
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        [HarmonyPatch( typeof( SoulShopOmniUISoulSwapBuyButton ), "InitializeConfirmMenu" )]
        internal static class SoulShopOmniUISoulSwapBuyButton_InitializeConfirmMenu_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "SoulShopOmniUISoulSwapBuyButton.InitializeConfirmMenu Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, Souls_EV.SOULS_PER_LEVEL ), // 150
                            /*  1 */ new WobTranspiler.OpTest( OpCodeSet.Stloc                          ), // int num = 150
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 0, WobSettings.Get( "SwapSouls", 150 ) ), // Set the new number of souls
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        [HarmonyPatch( typeof( SoulShopOmniUITransferResourceButton ), "ConfirmTransfer" )]
        internal static class SoulShopOmniUITransferResourceButton_ConfirmTransfer_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "SoulShopOmniUITransferResourceButton.ConfirmTransfer Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldsfld, name: "SoulTransferLevel" ), // SoulTransferLevel
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, Souls_EV.SOULS_PER_LEVEL  ), // 150
                            /*  2 */ new WobTranspiler.OpTest( OpCodes.Mul                               ), // SoulTransferLevel * 150
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( "SwapSouls", 150 ) ), // Set the new number of souls
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        [HarmonyPatch( typeof( SoulShopOmniUITransferResourceButton ), "InitializeConfirmMenu" )]
        internal static class SoulShopOmniUITransferResourceButton_InitializeConfirmMenu_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "SoulShopOmniUITransferResourceButton.InitializeConfirmMenu Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldsfld, name: "SoulTransferLevel" ), // SoulTransferLevel
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, Souls_EV.SOULS_PER_LEVEL  ), // 150
                            /*  2 */ new WobTranspiler.OpTest( OpCodes.Mul                               ), // SoulTransferLevel * 150
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( "SwapSouls", 150 ) ), // Set the new number of souls
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }
    }
}