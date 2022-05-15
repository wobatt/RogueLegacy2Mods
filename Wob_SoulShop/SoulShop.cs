using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace Wob_SoulShop {
    [BepInPlugin( "Wob.SoulShop", "Soul Shop Mod", "0.1.0" )]
    public partial class SoulShop : BaseUnityPlugin {
        // Configuration file entries, globally accessible for patches
        public static ConfigItem<float> configSwapScaler;
        public static ConfigItem<int> configSwapSouls;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configSwapScaler = new ConfigItem<float>( this.Config, "Options", "SwapScaler", "Multiply Strange Transaction costs by this value", 1f, 0f, 100f );
            configSwapSouls = new ConfigItem<int>( this.Config, "Options", "SwapSouls", "Number of souls given by Strange Transaction", 150, 1, int.MaxValue );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        [HarmonyPatch( typeof( Souls_EV ), nameof( Souls_EV.GetSoulSwapCost ) )]
        static class Souls_EV_GetSoulSwapCost_Patch {
            private static bool runOnce = false;
            static void Prefix() {
                if( !runOnce ) {
                    for( int i = 0; i < Souls_EV.ORE_AETHER_TO_SOUL_COST_LEVELS.Length; i++ ) {
                        Souls_EV.ORE_AETHER_TO_SOUL_COST_LEVELS[i] = Math.Max( 1, Mathf.RoundToInt( Souls_EV.ORE_AETHER_TO_SOUL_COST_LEVELS[i] * configSwapScaler.Value ) );
                    }
                    runOnce = true;
                }
            }
        }

        [HarmonyPatch( typeof( SoulShopOmniUIDescriptionBoxEntry ), "DisplayDescriptionBox" )]
        static class SoulShopOmniUIDescriptionBoxEntry_DisplayDescriptionBox_Patch {
            static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Put the instructions into a list for easier manipulation
                List<CodeInstruction> codes = new List<CodeInstruction>( instructions );
                WobPlugin.Log( "SoulShopOmniUIDescriptionBoxEntry.DisplayDescriptionBox: Searching opcodes" );
                // Iterate through the instruction codes
                for( int i = 0; i < codes.Count; i++ ) {
                    if( codes[i].opcode == OpCodes.Ldc_I4 && (int)codes[i].operand == Souls_EV.SOULS_PER_LEVEL ) {
                        WobPlugin.Log( "Found matching opcode at " + i + ": " + codes[i].ToString() );
                        codes[i].operand = configSwapSouls.Value;
                    }
                }
                // Return the modified instructions to complete the patch
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch( typeof( SoulShopOmniUIIncrementResourceButton ), "UpdateState" )]
        static class SoulShopOmniUIIncrementResourceButton_UpdateState_Patch {
            static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Put the instructions into a list for easier manipulation
                List<CodeInstruction> codes = new List<CodeInstruction>( instructions );
                WobPlugin.Log( "SoulShopOmniUIIncrementResourceButton.UpdateState: Searching opcodes" );
                // Iterate through the instruction codes
                for( int i = 0; i < codes.Count; i++ ) {
                    if( codes[i].opcode == OpCodes.Ldc_I4 && (int)codes[i].operand == Souls_EV.SOULS_PER_LEVEL ) {
                        WobPlugin.Log( "Found matching opcode at " + i + ": " + codes[i].ToString() );
                        codes[i].operand = configSwapSouls.Value;
                    }
                }
                // Return the modified instructions to complete the patch
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch( typeof( SoulShopOmniUISoulSwapBuyButton ), "ConfirmTransfer" )]
        static class SoulShopOmniUISoulSwapBuyButton_ConfirmTransfer_Patch {
            static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Put the instructions into a list for easier manipulation
                List<CodeInstruction> codes = new List<CodeInstruction>( instructions );
                WobPlugin.Log( "SoulShopOmniUISoulSwapBuyButton.ConfirmTransfer: Searching opcodes" );
                // Iterate through the instruction codes
                for( int i = 0; i < codes.Count - 1; i++ ) {
                    if( codes[i].opcode == OpCodes.Ldc_I4 && (int)codes[i].operand == Souls_EV.SOULS_PER_LEVEL ) {
                        WobPlugin.Log( "Found matching opcode at " + i + ": " + codes[i].ToString() );
                        codes[i].operand = configSwapSouls.Value;
                    }
                }
                // Return the modified instructions to complete the patch
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch( typeof( SoulShopOmniUISoulSwapBuyButton ), "InitializeConfirmMenu" )]
        static class SoulShopOmniUISoulSwapBuyButton_InitializeConfirmMenu_Patch {
            static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Put the instructions into a list for easier manipulation
                List<CodeInstruction> codes = new List<CodeInstruction>( instructions );
                WobPlugin.Log( "SoulShopOmniUISoulSwapBuyButton.InitializeConfirmMenu: Searching opcodes" );
                // Iterate through the instruction codes
                for( int i = 0; i < codes.Count - 1; i++ ) {
                    if( codes[i].opcode == OpCodes.Ldc_I4 && (int)codes[i].operand == Souls_EV.SOULS_PER_LEVEL ) {
                        WobPlugin.Log( "Found matching opcode at " + i + ": " + codes[i].ToString() );
                        codes[i].operand = configSwapSouls.Value;
                    }
                }
                // Return the modified instructions to complete the patch
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch( typeof( SoulShopOmniUITransferResourceButton ), "ConfirmTransfer" )]
        static class SoulShopOmniUITransferResourceButton_ConfirmTransfer_Patch {
            static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Put the instructions into a list for easier manipulation
                List<CodeInstruction> codes = new List<CodeInstruction>( instructions );
                WobPlugin.Log( "SoulShopOmniUITransferResourceButton.ConfirmTransfer: Searching opcodes" );
                // Iterate through the instruction codes
                for( int i = 0; i < codes.Count - 1; i++ ) {
                    if( codes[i].opcode == OpCodes.Ldc_I4 && (int)codes[i].operand == Souls_EV.SOULS_PER_LEVEL ) {
                        WobPlugin.Log( "Found matching opcode at " + i + ": " + codes[i].ToString() );
                        codes[i].operand = configSwapSouls.Value;
                    }
                }
                // Return the modified instructions to complete the patch
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch( typeof( SoulShopOmniUITransferResourceButton ), "InitializeConfirmMenu" )]
        static class SoulShopOmniUITransferResourceButton_InitializeConfirmMenu_Patch {
            static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Put the instructions into a list for easier manipulation
                List<CodeInstruction> codes = new List<CodeInstruction>( instructions );
                WobPlugin.Log( "SoulShopOmniUITransferResourceButton.InitializeConfirmMenu: Searching opcodes" );
                // Iterate through the instruction codes
                for( int i = 0; i < codes.Count - 1; i++ ) {
                    if( codes[i].opcode == OpCodes.Ldc_I4 && (int)codes[i].operand == Souls_EV.SOULS_PER_LEVEL ) {
                        WobPlugin.Log( "Found matching opcode at " + i + ": " + codes[i].ToString() );
                        codes[i].operand = configSwapSouls.Value;
                    }
                }
                // Return the modified instructions to complete the patch
                return codes.AsEnumerable();
            }
        }
    }
}