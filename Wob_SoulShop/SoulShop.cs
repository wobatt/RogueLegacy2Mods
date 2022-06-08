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

        private static readonly HashSet<SoulShopType> specialShopItems = new HashSet<SoulShopType> { SoulShopType.SoulSwap, SoulShopType.OreAetherSwap, SoulShopType.AetherOreSwap };

        private static readonly WobSettings.KeyHelper<SoulShopType> keys = new WobSettings.KeyHelper<SoulShopType>( "ShopItem", 2 );

        private static readonly Dictionary<SoulShopType,(string Config, string Name, int BaseCost, int ScalingCost, int MaxScaling, int MaxLevel, int MaxOverload, int UnlockLevel)> SSInfo = new Dictionary<SoulShopType,(string Config, string Name, int BaseCost, int ScalingCost, int MaxScalingLevel, int MaxLevel, int MaxOverload, int UnlockLevel)> {
            { SoulShopType.MaxEquipmentDrops,     ( "MaxEquipmentLevel",   "Embroidered Investments",   50,  200, 4, 4,  9,  0  ) },
            { SoulShopType.MaxRuneDrops,          ( "MaxRuneLevel",        "Runic Horizons",            50,  150, 4, 4,  9,  1  ) },
            { SoulShopType.BaseStats01MaxUp,      ( "UpgradeManor1",       "Unbreakable Will",          100, 50,  4, 10, 50, 2  ) },
            { SoulShopType.BaseStats02MaxUp,      ( "UpgradeManor2",       "Absolute Strength",         100, 50,  4, 10, 50, 2  ) },
            { SoulShopType.BaseStats03MaxUp,      ( "UpgradeManor3",       "Infinite Knowledge",        100, 50,  4, 10, 50, 2  ) },
            { SoulShopType.BaseStats04MaxUp,      ( "UpgradeManor4",       "Master Smith",              100, 50,  4, 10, 50, 2  ) },
            { SoulShopType.MaxMasteryFlat,        ( "MasteryLevel",        "Limitless Potential",       100, 50,  4, 10, 35, 4  ) },
            { SoulShopType.MaxCharonDonationFlat, ( "CharonDonationLimit", "Insatiable Greed",          100, 50,  4, 10, 25, 10 ) },
            { SoulShopType.ArcherVariant,         ( "BallisticArchers",    "Ballistic Archer",          300, 150, 0, 1,  0,  6  ) },
            { SoulShopType.AxeVariant,            ( "Fighters",            "Furious Fighter",           300, 150, 0, 1,  0,  6  ) },
            { SoulShopType.BoxerVariant,          ( "EnkindledBoxers",     "Enkindled Boxer",           300, 150, 0, 1,  0,  7  ) },
            { SoulShopType.LadleVariant,          ( "Waiters",             "Stoic Waiter",              300, 150, 0, 1,  0,  7  ) },
            { SoulShopType.SwordVariant,          ( "PizzaDelivery",       "Pizza Delivery",            300, 150, 0, 1,  0,  8  ) },
            { SoulShopType.LuteVariant,           ( "RockStar",            "Rock Star",                 300, 150, 0, 1,  0,  9  ) },
            { SoulShopType.WandVariant,           ( "Reaper",              "Hallowed Reaper",           300, 150, 0, 1,  0,  9  ) },
            { SoulShopType.ChooseYourClass,       ( "LockClass",           "Preferential Treatment",    200, 100, 0, 1,  0,  4  ) },
            { SoulShopType.ChooseYourSpell,       ( "LockSpell",           "Existential Appeasement",   300, 150, 0, 1,  0,  4  ) },
            { SoulShopType.ForceRandomizeKit,     ( "LockContrarian",      "Quintessential Resentment", 100, 50,  0, 1,  0,  15 ) },
            { SoulShopType.UnlockJukebox,         ( "Jukebox",             "Music Box",                 100, 50,  0, 1,  0,  2  ) },
            { SoulShopType.UnlockOverload,        ( "Overload",            "Cosmic Overload",           200, 100, 0, 1,  0,  65 ) },
        };

        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            foreach( SoulShopType sType in SSInfo.Keys ) {
                keys.Add( sType, SSInfo[sType].Config );
                WobSettings.Add( new WobSettings.Num<int>( keys.Get( sType, "BaseCost"    ), "Cost at level 1 for "         + SSInfo[sType].Name, SSInfo[sType].BaseCost,    bounds: (1, 1000000) ) );
                if( SSInfo[sType].MaxLevel > 1 ) {
                    int maxBound = ( sType == SoulShopType.MaxEquipmentDrops || sType == SoulShopType.MaxRuneDrops ) ? 9 : 1000000;
                    WobSettings.Add( new WobSettings.Num<int>( keys.Get( sType, "ScalingCost" ), "Cost increase per level for "         + SSInfo[sType].Name, SSInfo[sType].ScalingCost, bounds: (0, 1000000)  ) );
                    WobSettings.Add( new WobSettings.Num<int>( keys.Get( sType, "MaxScaling"  ), "Stop scaling cost at this level for " + SSInfo[sType].Name, SSInfo[sType].MaxScaling,  bounds: (1, maxBound) ) );
                    WobSettings.Add( new WobSettings.Num<int>( keys.Get( sType, "MaxLevel"    ), "Max level without overload for "      + SSInfo[sType].Name, SSInfo[sType].MaxLevel,    bounds: (1, maxBound) ) );
                    WobSettings.Add( new WobSettings.Num<int>( keys.Get( sType, "MaxOverload" ), "Max level with overload for "         + SSInfo[sType].Name, SSInfo[sType].MaxOverload, bounds: (1, maxBound) ) );
                }
                WobSettings.Add( new WobSettings.Num<int>( keys.Get( sType, "UnlockLevel" ), "Upgrades needed to unlock for " + SSInfo[sType].Name, SSInfo[sType].UnlockLevel, bounds: (0, 1000000) ) );
            }
            keys.Add( SoulShopType.OreAetherSwap, "OreToAether" );
            keys.Add( SoulShopType.AetherOreSwap, "AetherToOre" );
            keys.Add( SoulShopType.SoulSwap, "StrangeTransaction" );
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Num<float>( keys.Get( SoulShopType.SoulSwap, "CostScaler" ), "Multiply costs by this for Strange Transaction", 1f,  bounds: (0f, 100f)   ),
                new WobSettings.Num<int>(   keys.Get( SoulShopType.SoulSwap, "GainSouls"  ), "Number of souls given for Strange Transaction",  150, bounds: (1, 1000000) ),
                new WobSettings.Num<int>(   keys.Get( SoulShopType.SoulSwap, "MaxLevel"   ), "Max level for Strange Transaction",              100, bounds: (1, 1000000) ),
            } );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        [HarmonyPatch( typeof( Souls_EV ), nameof( Souls_EV.GetTotalSoulsCollected ) )]
        internal static class Souls_EV_GetTotalSoulsCollected_Patch {
            private static bool runOnce = false;
            internal static void Prefix() {
                if( !runOnce ) {
                    foreach( SoulShopType soulShopType in SoulShopType_RL.TypeArray ) {
                        SoulShopData soulShopData = SoulShopLibrary.GetSoulShopData( soulShopType );
                        if( soulShopData != null && !soulShopData.Disabled ) {
                            if( !specialShopItems.Contains( soulShopType ) ) {
                                soulShopData.BaseCost = WobSettings.Get( keys.Get( soulShopType, "BaseCost" ), soulShopData.BaseCost );
                                if( soulShopData.MaxLevel > 1 ) {
                                    soulShopData.ScalingCost = WobSettings.Get( keys.Get( soulShopType, "ScalingCost" ), soulShopData.ScalingCost );
                                    soulShopData.MaxLevelScalingCap = soulShopData.ScalingCost == 0 ? 1 : WobSettings.Get( keys.Get( soulShopType, "MaxScaling" ), soulShopData.MaxLevelScalingCap );
                                    soulShopData.MaxLevel = WobSettings.Get( keys.Get( soulShopType, "MaxLevel" ), soulShopData.MaxLevel );
                                    soulShopData.OverloadMaxLevel = Mathf.Max( soulShopData.MaxLevel, WobSettings.Get( keys.Get( soulShopType, "MaxOverload" ), soulShopData.OverloadMaxLevel ) );
                                } else {
                                    soulShopData.ScalingCost = 0;
                                    soulShopData.MaxLevelScalingCap = 1;
                                }
                                soulShopData.MaxSoulCostCap = soulShopData.BaseCost + ( soulShopData.ScalingCost * Mathf.Min( soulShopData.MaxLevelScalingCap, soulShopData.MaxLevel ) );
                                soulShopData.UnlockLevel = WobSettings.Get( keys.Get( soulShopType, "UnlockLevel" ), soulShopData.UnlockLevel );
                            } else {
                                if( soulShopType == SoulShopType.SoulSwap ) {
                                    soulShopData.MaxLevel = WobSettings.Get( keys.Get( soulShopType, "MaxLevel" ), soulShopData.MaxLevel );
                                }
                            }
                        }
                    }
                    runOnce = true;
                }
            }
        }

        [HarmonyPatch( typeof( Souls_EV ), nameof( Souls_EV.GetSoulSwapCost ) )]
        internal static class Souls_EV_GetSoulSwapCost_Patch {
            private static bool runOnce = false;
            internal static void Prefix() {
                if( !runOnce ) {
                    for( int i = 0; i < Souls_EV.ORE_AETHER_TO_SOUL_COST_LEVELS.Length; i++ ) {
                        Souls_EV.ORE_AETHER_TO_SOUL_COST_LEVELS[i] = Math.Max( 1, Mathf.RoundToInt( Souls_EV.ORE_AETHER_TO_SOUL_COST_LEVELS[i] * WobSettings.Get( keys.Get( SoulShopType.SoulSwap, "CostScaler" ), 1f ) ) );
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
                            new WobTranspiler.OpAction_SetOperand( 0, WobSettings.Get( keys.Get( SoulShopType.SoulSwap, "GainSouls" ), 150 ) ), // Set the new number of souls
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
                            new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( keys.Get( SoulShopType.SoulSwap, "GainSouls" ), 150 ) ), // Set the new number of souls
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
                            new WobTranspiler.OpAction_SetOperand( 0, WobSettings.Get( keys.Get( SoulShopType.SoulSwap, "GainSouls" ), 150 ) ), // Set the new number of souls
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
                            new WobTranspiler.OpAction_SetOperand( 0, WobSettings.Get( keys.Get( SoulShopType.SoulSwap, "GainSouls" ), 150 ) ), // Set the new number of souls
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
                            new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( keys.Get( SoulShopType.SoulSwap, "GainSouls" ), 150 ) ), // Set the new number of souls
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
                            new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( keys.Get( SoulShopType.SoulSwap, "GainSouls" ), 150 ) ), // Set the new number of souls
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }
    }
}