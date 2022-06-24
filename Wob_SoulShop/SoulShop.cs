using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace Wob_SoulShop {
    [BepInPlugin( "Wob.SoulShop", "Soul Shop Mod", "1.1.2" )]
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
                    int maxBound = 1000000;
                    switch( sType ) {
                        case SoulShopType.MaxEquipmentDrops:
                        case SoulShopType.MaxRuneDrops:
                            maxBound = 9;
                            break;
                        case SoulShopType.MaxMasteryFlat:
                            maxBound = 235; // Max mastery XP level = 15 + 235 = 250, int overflow at 263
                            break;
                        case SoulShopType.MaxCharonDonationFlat:
                            maxBound = 100; // Max Charon level = 25 + 3 * 100 = 325, int overflow at 335
                            break;
                    }
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
                new WobSettings.Boolean(    keys.Get( SoulShopType.BaseStats04MaxUp, "CapWeights" ), "Apply a hard cap to equip and rune weight upgrade levels when they would exceed the maximum weight of all equipable items", true ),
            } );
            if( WobPlugin.Enabled ) {
                GenerateLeveledArrays();
                SkillTreeObj_MaxLevel_Patch.capWeights = WobSettings.Get( keys.Get( SoulShopType.BaseStats04MaxUp, "CapWeights" ), true );
            }
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        private static void GenerateLeveledArrays( ) {
            int[] xpArray1 = new int[261]; // int overflow at 263
            for( int i = 0; i < xpArray1.Length; i++ ) {
                xpArray1[i] = Mathf.RoundToInt( ( ( 700f / 6f ) * i * i * i ) + ( 450f * i * i ) + ( ( 5800f / 3f ) * i ) );
            }
            Mastery_EV.XP_REQUIRED = xpArray1;
            int[] xpArray2 = new int[261]; // Match length of array above
            for( int i = 0; i < xpArray2.Length; i++ ) {
                xpArray2[i] = Mathf.RoundToInt( ( ( 50f / 3f ) * i * i * i ) + ( 850f * i * i ) + ( ( 2650f / 3f ) * i ) );
            }
            Mastery_EV.DRIFTING_WORLDS_XP_REQUIRED = xpArray2;
            int[] charonGoldArray = new int[331]; // int overflow at 335
            for( int i = 0; i < charonGoldArray.Length; i++ ) {
                charonGoldArray[i] = Mathf.RoundToInt( ( 50f * i * i * i ) + ( 2425f * i * i ) + ( 5025f * i ) );
            }
            SkillTree_EV.CHARON_GOLD_STAT_BONUS_MILESTONES = charonGoldArray;
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

        [HarmonyPatch( typeof( SoulShopObj ), nameof( SoulShopObj.SetEquippedLevel ) )]
        internal static class SoulShopObj_SetEquippedLevel_Patch {
            internal static void Postfix( SoulShopObj __instance ) {
                if( __instance.SoulShopType == SoulShopType.BaseStats04MaxUp ) {
                    SkillTreeObj_MaxLevel_Patch.UpdateCaps();
                }
            }
        }
        
        [HarmonyPatch( typeof( SkillTreeManager ), nameof( SkillTreeManager.SetSkillObjLevel ) )]
        internal static class SkillTreeManager_SetSkillObjLevel_Patch {
            private static readonly HashSet<SkillTreeType> upgrades = new HashSet<SkillTreeType>() { SkillTreeType.Equip_Up, SkillTreeType.Equip_Up2, SkillTreeType.Equip_Up3, SkillTreeType.Rune_Equip_Up, SkillTreeType.Rune_Equip_Up2, SkillTreeType.Rune_Equip_Up3 };
            internal static void Postfix( SkillTreeType skillTreeType ) {
                if( upgrades.Contains( skillTreeType ) ) {
                    SkillTreeObj_MaxLevel_Patch.UpdateCaps();
                }
            }
        }
        
        [HarmonyPatch( typeof( EquipmentSaveData ), nameof( EquipmentSaveData.Initialize ) )]
        internal static class EquipmentSaveData_Initialize_Patch {
            internal static void Postfix() {
                SkillTreeObj_MaxLevel_Patch.UpdateCaps();
            }
        }

        // Patch to apply a cap on equip weight and rune weight upgrades overload levels based on their stat gains and the maximum weight of equipable items
        [HarmonyPatch( typeof( SkillTreeObj ), nameof( SkillTreeObj.MaxLevel ), MethodType.Getter )]
        internal static class SkillTreeObj_MaxLevel_Patch {
            // These are the upgrades to be edited for equip weight and rune weight
            private static readonly SkillTreeType[] equipUpgrades = new SkillTreeType[] { SkillTreeType.Equip_Up, SkillTreeType.Equip_Up2, SkillTreeType.Equip_Up3 };
            private static readonly SkillTreeType[] runeUpgrades = new SkillTreeType[] { SkillTreeType.Rune_Equip_Up, SkillTreeType.Rune_Equip_Up2, SkillTreeType.Rune_Equip_Up3 };
            // Only need to edit once
            private static bool runOnce = false;
            internal static bool capWeights;

            // The patch itself - call the calculation methods
            internal static void Prefix() {
                if( !runOnce ) {
                    UpdateCaps();
                    runOnce = true;
                }
            }

            internal static void UpdateCaps() {
                if( capWeights ) {
                    CalcMaxLevel( equipUpgrades, GetTotalEquipWeight() );
                    CalcMaxLevel( runeUpgrades, GetTotalRuneWeight() );
                }
            }

            // Calculate the maximum weight of equipment that can be on a character at once, and scale so this is within the lightest category for maximum resolve
            private static int GetTotalEquipWeight() {
                // Running maximums of the weights of max level equipment in each category
                Dictionary<EquipmentCategoryType, int> maxWeights = new Dictionary<EquipmentCategoryType, int> {  };
                // Get the extra levels that could be gained from overload
                int equipOverloadMaxLevel = SaveManager.ModeSaveData.GetSoulShopObj( SoulShopType.MaxEquipmentDrops ).SoulShopData.OverloadMaxLevel;
                // Loop through all equipment categories (equipment slot types)
                foreach( EquipmentCategoryType equipmentCatType in EquipmentType_RL.CategoryTypeArray ) {
                    if( equipmentCatType != EquipmentCategoryType.None ) {
                        // Add the category to the max weights dictionary
                        maxWeights.Add( equipmentCatType, 0 );
                        // Loop through each item in the category, one for each set
                        foreach( EquipmentType equipmentType in EquipmentType_RL.TypeArray ) {
                            if( equipmentType != EquipmentType.None ) {
                                // Get the data for the equipment
                                EquipmentData equipmentData = EquipmentLibrary.GetEquipmentData( equipmentCatType, equipmentType );
                                if( equipmentData != null && !equipmentData.Disabled ) {
                                    // Calculate its weight at max level
                                    int weight = equipmentData.BaseWeight + equipmentData.ScalingWeight * ( equipmentData.MaximumLevel + equipOverloadMaxLevel - 1 );
                                    // Store the highest found weight for this category
                                    maxWeights[equipmentCatType] = Mathf.Max( maxWeights[equipmentCatType], weight );
                                }
                            }
                        }
                    }
                }
                // Total weight for all 5 slots
                int totalEquipWeight = 0;
                // Add up the maximum weights for each slot
                foreach( EquipmentCategoryType equipmentCatType in maxWeights.Keys ) {
                    totalEquipWeight += maxWeights[equipmentCatType];
                }
                // Apply the weight category reduction upgrade to make sure the max equipped would be in the lightest category
                SkillTreeObj weightCategory = SkillTreeManager.GetSkillTreeObj( SkillTreeType.Weight_CD_Reduce );
                float weightCat = 0.2f + weightCategory.FirstLevelStatGain + weightCategory.AdditionalLevelStatGain * ( weightCategory.SkillTreeData.MaxLevel - 1 );
                totalEquipWeight = Mathf.CeilToInt( totalEquipWeight / weightCat );
                // Return the calculated maximum
                return totalEquipWeight;
            }

            // Calculate the maximum weight of runes that can be on a character at once
            private static int GetTotalRuneWeight() {
                // Running total of the weights of max level runes
                int totalRuneWeight = 0;
                // Get the extra levels that could be gained from overload
                int runeOverloadMaxLevel = SaveManager.ModeSaveData.GetSoulShopObj( SoulShopType.MaxRuneDrops ).SoulShopData.OverloadMaxLevel;
                // Loop through all runes
                foreach( RuneType runeType in RuneType_RL.TypeArray ) {
                    if( runeType != RuneType.None ) {
                        // Get the data for the rune
                        RuneData runeData = RuneLibrary.GetRuneData( runeType );
                        if( runeData != null && !runeData.Disabled ) {
                            // Calculate its maximum weight and add it to the total
                            int weight = runeData.BaseWeight + runeData.ScalingWeight * ( runeData.MaximumLevel + runeOverloadMaxLevel - 1 );
                            totalRuneWeight += weight;
                        }
                    }
                }
                return totalRuneWeight;
            }

            private static void CalcMaxLevel( SkillTreeType[] skillTreeTypes, int maxWeight ) {
                WobPlugin.Log( "Max weight = " + maxWeight );
                // STEP 1: Calculate the total weight capacity from all upgrades, plus soul shop upgrades, without overload
                // Running total of the weight capacity of max level upgrades
                float totalUpgradeWeight = 0f;
                // Get the Soul Shop item that governs weight upgrades
                SoulShopData soulShopData = SaveManager.ModeSaveData.GetSoulShopObj( SoulShopType.BaseStats04MaxUp ).SoulShopData;
                // Create a dictionary of the 3 upgrades
                Dictionary<SkillTreeType, (int MaxLevel, float BaseWeight, int AdditiveLevels, float WeightPerLevel)> UpgradeInfo = new Dictionary<SkillTreeType, (int MaxLevel, float BaseWeight, int AdditiveLevels, float WeightPerLevel)>();
                // Loop through the upgrades
                foreach( SkillTreeType skillTreeType in skillTreeTypes ) {
                    // Get the upgrade item
                    SkillTreeObj skillTreeObj = SkillTreeManager.GetSkillTreeObj( skillTreeType );
                    // Calculate the maximum possible level without overload
                    int maxLevel = skillTreeObj.SkillTreeData.MaxLevel + ( soulShopData.MaxLevel * skillTreeObj.SkillTreeData.AdditiveSoulShopLevels );
                    // Calculate the weight capacity at the maximum level without overload
                    float baseWeight = skillTreeObj.FirstLevelStatGain + skillTreeObj.AdditionalLevelStatGain * ( maxLevel - 1 );
                    // Add to the total
                    totalUpgradeWeight += baseWeight;
                    // Record the stats needed for level cap calculation
                    UpgradeInfo.Add( skillTreeType, (maxLevel, baseWeight, skillTreeObj.SkillTreeData.AdditiveSoulShopLevels, skillTreeObj.SkillTreeData.AdditionalLevelStatGain) );
                }
                // STEP 2: Calculate how many upgrades on top of the standard maximum are needed to equip all items
                // Counter for number of overload level to add
                int extraLevels = 0;
                // Add levels until the overload maximum is reached or there is enough capacity to equip all items
                while( extraLevels <= soulShopData.OverloadMaxLevel && totalUpgradeWeight < maxWeight ) {
                    // Add the weight capacity for a level of each upgrade
                    foreach( SkillTreeType skillTreeType in UpgradeInfo.Keys ) {
                        totalUpgradeWeight += UpgradeInfo[skillTreeType].AdditiveLevels * UpgradeInfo[skillTreeType].WeightPerLevel;
                    }
                    // Increment the number of levels
                    extraLevels++;
                }
                // STEP 3: Round off, and write the level to the max cap
                // Round down to a multiple of 5
                extraLevels = Mathf.CeilToInt( extraLevels / 5f ) * 5;
                WobPlugin.Log( "Overload levels = " + ( extraLevels + 10 ) );
                // Loop through the upgrades
                foreach( SkillTreeType skillTreeType in UpgradeInfo.Keys ) {
                    // Get the current upgrade level, so we don't set the cap to below it
                    int currentLevel = SkillTreeManager.GetSkillTreeObj( skillTreeType ).Level;
                    // Get the upgrade data to be capped
                    SkillTreeData skillTreeData = SkillTreeLibrary.GetSkillTreeData( skillTreeType );
                    // Set the level cap to the higher of the current upgrade level and the calculated maximum
                    skillTreeData.OverloadLevelCap = Mathf.Max( currentLevel, UpgradeInfo[skillTreeType].MaxLevel + ( UpgradeInfo[skillTreeType].AdditiveLevels * extraLevels ) );
                    WobPlugin.Log( skillTreeType + ": " + skillTreeData.OverloadLevelCap + " = " + UpgradeInfo[skillTreeType].MaxLevel + " + " + ( UpgradeInfo[skillTreeType].AdditiveLevels * extraLevels ) + " (" + currentLevel + ") for " + ( UpgradeInfo[skillTreeType].BaseWeight + ( UpgradeInfo[skillTreeType].WeightPerLevel * UpgradeInfo[skillTreeType].AdditiveLevels * extraLevels ) ) );
                }
            }
        }
    }
}