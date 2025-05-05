using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace WobMod {
    public partial class SoulShop {

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTING DEFAULTS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        private record SoulShopInfo( string Config, string Name, int BaseCost, int ScalingCost, int MaxScaling, int MaxLevel, int MaxOverload, int UnlockLevel );
		private static readonly Dictionary<SoulShopType, SoulShopInfo> soulShopInfo = new() {
            { SoulShopType.MaxEquipmentDrops,     new( "MaxEquipmentLevel",      "Embroidered Investments",   50,  200, 4, 4,  9,  0  ) },
            { SoulShopType.MaxRuneDrops,          new( "MaxRuneLevel",           "Runic Horizons",            50,  150, 4, 4,  9,  1  ) },
            { SoulShopType.BaseStats01MaxUp,      new( "UpgradeManor1",          "Unbreakable Will",          100, 50,  4, 10, 60, 2  ) },
            { SoulShopType.BaseStats02MaxUp,      new( "UpgradeManor2",          "Absolute Strength",         100, 50,  4, 10, 60, 2  ) },
            { SoulShopType.BaseStats03MaxUp,      new( "UpgradeManor3",          "Infinite Knowledge",        100, 50,  4, 10, 60, 2  ) },
            { SoulShopType.BaseStats04MaxUp,      new( "UpgradeManor4",          "Master Smith",              100, 50,  4, 10, 60, 2  ) },
            { SoulShopType.MaxMasteryFlat,        new( "MaxMasteryLevel",        "Limitless Potential",       100, 50,  4, 10, 35, 4  ) },
            { SoulShopType.MaxCharonDonationFlat, new( "MaxCharonDonation",      "Insatiable Greed",          100, 50,  4, 10, 25, 10 ) },
            { SoulShopType.ArcherVariant,         new( "Class_BallisticArchers", "Ballistic Archer",          300, 150, 0, 1,  0,  6  ) },
            { SoulShopType.AxeVariant,            new( "Class_Fighters",         "Furious Fighter",           300, 150, 0, 1,  0,  6  ) },
            { SoulShopType.BoxerVariant,          new( "Class_EnkindledBoxers",  "Enkindled Boxer",           300, 150, 0, 1,  0,  7  ) },
            { SoulShopType.LadleVariant,          new( "Class_Waiters",          "Stoic Waiter",              300, 150, 0, 1,  0,  7  ) },
            { SoulShopType.SwordVariant,          new( "Class_PizzaDelivery",    "Pizza Delivery",            300, 150, 0, 1,  0,  8  ) },
            { SoulShopType.LuteVariant,           new( "Class_RockStar",         "Rock Star",                 300, 150, 0, 1,  0,  9  ) },
            { SoulShopType.WandVariant,           new( "Class_Reaper",           "Hallowed Reaper",           300, 150, 0, 1,  0,  8  ) },
            { SoulShopType.DualBladesVariant,     new( "Class_Spies",            "Shadow Spies",              300, 150, 0, 1,  0,  9  ) },
            { SoulShopType.LancerVariant,         new( "Class_Vikings",          "Royal Vikings",             300, 150, 0, 1,  0,  12 ) },
            { SoulShopType.SamuraiVariant,        new( "Class_Tamer",            "Otherworldly Tamer",        300, 150, 0, 1,  0,  11 ) },
            { SoulShopType.SpearVariant,          new( "Class_Shapeshifters",    "Serpentine Shapeshifters",  300, 150, 0, 1,  0,  12 ) },
            { SoulShopType.GunslingerVariant,     new( "Class_Brigands",         "Blasting Brigands",         300, 150, 0, 1,  0,  11 ) },
            { SoulShopType.AstromancerVariant,    new( "Class_Lich",             "Eldritch Lich",             300, 150, 0, 1,  0,  10 ) },
            { SoulShopType.PirateVariant,         new( "Class_Surfers",          "Sick Surfers",              300, 150, 0, 1,  0,  12 ) },
            { SoulShopType.DuelistVariant,        new( "Class_Snipers",          "Trigonometric Snipers",     300, 150, 0, 1,  0,  10 ) },
            { SoulShopType.ChooseYourClass,       new( "LockClass",              "Preferential Treatment",    200, 100, 0, 1,  0,  4  ) },
            { SoulShopType.ChooseYourSpell,       new( "LockSpell",              "Existential Appeasement",   300, 150, 0, 1,  0,  4  ) },
            { SoulShopType.ForceRandomizeKit,     new( "LockContrarian",         "Quintessential Resentment", 100, 50,  0, 1,  0,  5  ) },
            { SoulShopType.UnlockJukebox,         new( "Jukebox",                "Music Box",                 100, 50,  0, 1,  0,  2  ) },
            { SoulShopType.UnlockOverload,        new( "Overload",               "Cosmic Overload",           200, 100, 0, 1,  0,  65 ) },
        };

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTINGS READ/WRITE
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        internal static readonly WobSettings.KeyHelper<SoulShopType> soulShopKeys = new( "ShopItem" );

        internal static void RunSetup() {
            WobMod.configFiles.Add( "SoulShop", "SoulShop" );
            WobSettings.Add( new WobSettings.Boolean( WobMod.configFiles.Get( "SoulShop" ), "DriftHouse", "UnlockDriftHouse", "Force the building containing the soul shop to spawn on the docks (NOTE: This cannot be undone!)", false ) );
            foreach( SoulShopType sType in soulShopInfo.Keys ) {
                soulShopKeys.Add( sType, soulShopInfo[sType].Config );
                WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "SoulShop" ), soulShopKeys.Get( sType, "BaseCost" ), "Cost at level 1 for " + soulShopInfo[sType].Name, soulShopInfo[sType].BaseCost, bounds: (1, 1000000) ) );
                if( soulShopInfo[sType].MaxLevel > 1 ) {
                    int maxBound = 1000000;
                    switch( sType ) {
                        case SoulShopType.MaxEquipmentDrops:
                        case SoulShopType.MaxRuneDrops:
                            maxBound = 9;
                            break;
                        case SoulShopType.MaxMasteryFlat:
                            maxBound = 200; // Max mastery XP int overflow at 263
                            break;
                        case SoulShopType.MaxCharonDonationFlat:
                            maxBound = 100; // Max Charon level = 25 + 3 * 100 = 325, int overflow at 335
                            break;
                    }
                    WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "SoulShop" ), soulShopKeys.Get( sType, "ScalingCost" ), "Cost increase per level for " + soulShopInfo[sType].Name, soulShopInfo[sType].ScalingCost, bounds: (0, 1000000) ) );
                    WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "SoulShop" ), soulShopKeys.Get( sType, "MaxScaling" ), "Stop scaling cost at this level for " + soulShopInfo[sType].Name, soulShopInfo[sType].MaxScaling, bounds: (1, maxBound) ) );
                    WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "SoulShop" ), soulShopKeys.Get( sType, "MaxLevel" ), "Max level without overload for " + soulShopInfo[sType].Name, soulShopInfo[sType].MaxLevel, bounds: (1, maxBound) ) );
                    WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "SoulShop" ), soulShopKeys.Get( sType, "MaxOverload" ), "Max level with overload for " + soulShopInfo[sType].Name, soulShopInfo[sType].MaxOverload, bounds: (1, maxBound) ) );
                }
                WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "SoulShop" ), soulShopKeys.Get( sType, "UnlockLevel" ), "Upgrades needed to unlock for " + soulShopInfo[sType].Name, soulShopInfo[sType].UnlockLevel, bounds: (0, 1000000) ) );
            }
            soulShopKeys.Add( SoulShopType.OreAetherSwap, "OreToAether" );
            soulShopKeys.Add( SoulShopType.AetherOreSwap, "AetherToOre" );
            soulShopKeys.Add( SoulShopType.SoulSwap, "StrangeTransaction" );
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Num<float>( WobMod.configFiles.Get( "SoulShop" ), soulShopKeys.Get( SoulShopType.SoulSwap,          "CostScaler"        ), "Multiply costs by this for Strange Transaction",                                                                            1f,  bounds: ( 0f, 100f   ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "SoulShop" ), soulShopKeys.Get( SoulShopType.SoulSwap,          "GainSouls"         ), "Number of souls given for Strange Transaction",                                                                             150, bounds: ( 1,  1000000) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "SoulShop" ), soulShopKeys.Get( SoulShopType.SoulSwap,          "MaxLevel"          ), "Max level for Strange Transaction",                                                                                         100, bounds: ( 1,  1000000) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "SoulShop" ), soulShopKeys.Get( SoulShopType.BaseStats04MaxUp,  "EquipOverloadCap"  ), "Apply a hard cap to equip weight upgrade levels at this number of additional overload levels (-1 to disable)",              -1,  bounds: (-1,  1000000) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "SoulShop" ), soulShopKeys.Get( SoulShopType.BaseStats04MaxUp,  "RuneOverloadCap"   ), "Apply a hard cap to rune weight upgrade levels at this number of additional overload levels (-1 to disable)",               -1,  bounds: (-1,  1000000) ),
                new WobSettings.Boolean(    WobMod.configFiles.Get( "SoulShop" ), soulShopKeys.Get( SoulShopType.ChooseYourClass,   "UseMinMastery"     ), "When generating characters, set the locked class to the one with lowest mastery level",                                     false                       ),
                new WobSettings.Boolean(    WobMod.configFiles.Get( "SoulShop" ), soulShopKeys.Get( SoulShopType.ChooseYourClass,   "AllSlots"          ), "Locking a class in the Soul Shop affects all slots, not just the last",                                                     false                       ),
                new WobSettings.Boolean(    WobMod.configFiles.Get( "SoulShop" ), soulShopKeys.Get( SoulShopType.ChooseYourSpell,   "AllSlots"          ), "Locking a spell in the Soul Shop affects all slots, not just the last",                                                     false                       ),
                new WobSettings.Boolean(    WobMod.configFiles.Get( "SoulShop" ), soulShopKeys.Get( SoulShopType.ForceRandomizeKit, "AllSlots"          ), "Locking contrarian in the Soul Shop affects all slots, not just the first, and takes effect without the trait",             false                       ),
                new WobSettings.Boolean(    WobMod.configFiles.Get( "SoulShop" ), soulShopKeys.Get( SoulShopType.ForceRandomizeKit, "PreventDuplicates" ), "Try to prevent each weapon, talent, and spell appearing more than once in each set of heir options",                        false                       ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "SoulShop" ), soulShopKeys.Get( SoulShopType.MaxMasteryFlat,    "BaseMaxMastery"    ), "Maximum mastery level per class without soul shop upgrades",                                                                15,  bounds: ( 1,  50     ) ),
            } );
            if( WobPlugin.Enabled ) {
                GenerateLeveledArrays();
                SkillTreeObj_MaxLevel_Patch.equipOverloadCap = WobSettings.Get( soulShopKeys.Get( SoulShopType.BaseStats04MaxUp, "EquipOverloadCap" ), -1 );
                SkillTreeObj_MaxLevel_Patch.runeOverloadCap  = WobSettings.Get( soulShopKeys.Get( SoulShopType.BaseStats04MaxUp, "RuneOverloadCap"  ), -1 );
            }
        }

        private static void GenerateLeveledArrays() {
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

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - DOCK
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Patch the method that checks save data flags to override the soul shop and challenge shop unlock flag
        [HarmonyPatch( typeof( PlayerSaveData ), nameof( PlayerSaveData.GetFlag ) )]
        internal static class PlayerSaveData_GetFlag_Patch {
            // Run before the orginal method to intercept and change parameters before it runs
            internal static void Postfix( PlayerSaveData __instance, PlayerSaveFlag flag, ref bool __result ) {
                // Check which flag is requested and what its value is
                if( flag == PlayerSaveFlag.DriftHouseUnlocked && !__result ) {
                    if( WobSettings.Get( "DriftHouse", "UnlockDriftHouse", false ) ) {
                        WobPlugin.Log( "[SoulShop] " + "Unlocking drift house" );
                        // Set the flag to true
                        __instance.SetFlag( PlayerSaveFlag.DriftHouseUnlocked, true );
                        __result = true;
                    }
                }
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - SOUL SHOP
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        [HarmonyPatch( typeof( Souls_EV ), nameof( Souls_EV.GetTotalSoulsCollected ) )]
        internal static class Souls_EV_GetTotalSoulsCollected_Patch {
            private static bool runOnce = false;
            private static readonly HashSet<SoulShopType> soulShopSpecialItems = new() { SoulShopType.SoulSwap, SoulShopType.OreAetherSwap, SoulShopType.AetherOreSwap };
            internal static void Prefix() {
                if( !runOnce ) {
                    foreach( SoulShopType soulShopType in SoulShopType_RL.TypeArray ) {
                        SoulShopData soulShopData = SoulShopLibrary.GetSoulShopData( soulShopType );
                        if( soulShopData != null && !soulShopData.Disabled ) {
                            if( !soulShopSpecialItems.Contains( soulShopType ) ) {
                                //WobPlugin.Log( "[SoulShop] " + "SoulShopData before edit: " + soulShopType + ", " + soulShopData.BaseCost + ", " + soulShopData.ScalingCost + ", " + soulShopData.MaxLevelScalingCap + ", " + soulShopData.MaxLevel + ", " + soulShopData.OverloadMaxLevel + ", " + soulShopData.UnlockLevel );
                                soulShopData.BaseCost = WobSettings.Get( soulShopKeys.Get( soulShopType, "BaseCost" ), soulShopData.BaseCost );
                                if( soulShopData.MaxLevel > 1 ) {
                                    soulShopData.ScalingCost = WobSettings.Get( soulShopKeys.Get( soulShopType, "ScalingCost" ), soulShopData.ScalingCost );
                                    soulShopData.MaxLevelScalingCap = soulShopData.ScalingCost == 0 ? 1 : WobSettings.Get( soulShopKeys.Get( soulShopType, "MaxScaling" ), soulShopData.MaxLevelScalingCap );
                                    soulShopData.MaxLevel = WobSettings.Get( soulShopKeys.Get( soulShopType, "MaxLevel" ), soulShopData.MaxLevel );
                                    soulShopData.OverloadMaxLevel = Mathf.Max( soulShopData.MaxLevel, WobSettings.Get( soulShopKeys.Get( soulShopType, "MaxOverload" ), soulShopData.OverloadMaxLevel ) );
                                } else {
                                    soulShopData.ScalingCost = 0;
                                    soulShopData.MaxLevelScalingCap = 1;
                                }
                                soulShopData.MaxSoulCostCap = soulShopData.BaseCost + ( soulShopData.ScalingCost * Mathf.Min( soulShopData.MaxLevelScalingCap, soulShopData.MaxLevel ) );
                                soulShopData.UnlockLevel = WobSettings.Get( soulShopKeys.Get( soulShopType, "UnlockLevel" ), soulShopData.UnlockLevel );
                            } else {
                                if( soulShopType == SoulShopType.SoulSwap ) {
                                    soulShopData.MaxLevel = WobSettings.Get( soulShopKeys.Get( soulShopType, "MaxLevel" ), soulShopData.MaxLevel );
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
                        Souls_EV.ORE_AETHER_TO_SOUL_COST_LEVELS[i] = System.Math.Max( 1, Mathf.RoundToInt( Souls_EV.ORE_AETHER_TO_SOUL_COST_LEVELS[i] * WobSettings.Get( soulShopKeys.Get( SoulShopType.SoulSwap, "CostScaler" ), 1f ) ) );
                    }
                    runOnce = true;
                }
            }
        }

        [HarmonyPatch( typeof( SoulShopOmniUIDescriptionBoxEntry ), "DisplayDescriptionBox" )]
        internal static class SoulShopOmniUIDescriptionBoxEntry_DisplayDescriptionBox_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "SoulShopOmniUIDescriptionBoxEntry.DisplayDescriptionBox" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldc_I4, Souls_EV.SOULS_PER_LEVEL ), // 150
                        /*  1 */ new( OpCodeSet.Stloc                          ), // int num = 150
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 0, WobSettings.Get( soulShopKeys.Get( SoulShopType.SoulSwap, "GainSouls" ), 150 ) ), // Set the new number of souls
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        [HarmonyPatch( typeof( SoulShopOmniUIIncrementResourceButton ), "UpdateState" )]
        internal static class SoulShopOmniUIIncrementResourceButton_UpdateState_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "SoulShopOmniUIIncrementResourceButton.UpdateState" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldsfld, name: "SoulTransferLevel" ), // SoulTransferLevel
                        /*  1 */ new( OpCodes.Ldc_I4, Souls_EV.SOULS_PER_LEVEL  ), // 150
                        /*  2 */ new( OpCodes.Mul                               ), // SoulTransferLevel * 150
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( soulShopKeys.Get( SoulShopType.SoulSwap, "GainSouls" ), 150 ) ), // Set the new number of souls
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Ore to gold costs for Strange Transaction
        [HarmonyPatch( typeof( SoulShopOmniUISoulSwapBuyButton ), nameof( SoulShopOmniUISoulSwapBuyButton.UpdateState ) )]
        internal static class SoulShopOmniUISoulSwapBuyButton_UpdateState_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "SoulShopOmniUISoulSwapBuyButton.UpdateState" );
                if( WobSettings.Get( "Resources", "ConvertOres", false ) ) {
                    // Perform the patching
                    transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new( OpCodes.Ldfld, name: "RuneOreCollected" ), // SaveManager.PlayerSaveData.EquipmentOreCollected
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 0, typeof( PlayerSaveData ).GetField( "GoldCollected" ) ), // SaveManager.PlayerSaveData.GoldCollected
                        }, expected: 1 );
                    transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new( OpCodes.Ldfld, name: "EquipmentOreCollected" ), // SaveManager.PlayerSaveData.EquipmentOreCollected
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 0, typeof( PlayerSaveData ).GetField( "GoldCollected" ) ), // SaveManager.PlayerSaveData.GoldCollected
                        }, expected: 1 );
                }
                // Return the modified instructions
                return transpiler.GetResult();
            }

            internal static void Postfix() {
                if( WobSettings.Get( "Resources", "ConvertOres", false ) ) {
                    if( SaveManager.PlayerSaveData.EquipmentOreCollected > 0 ) {
                        SaveManager.PlayerSaveData.GoldCollected += SaveManager.PlayerSaveData.EquipmentOreCollected;
                        SaveManager.PlayerSaveData.EquipmentOreCollected = 0;
                    }
                    if( SaveManager.PlayerSaveData.RuneOreCollected > 0 ) {
                        SaveManager.PlayerSaveData.GoldCollected += SaveManager.PlayerSaveData.RuneOreCollected;
                        SaveManager.PlayerSaveData.RuneOreCollected = 0;
                    }
                }
            }
        }

        // Ore to gold costs for Strange Transaction
        [HarmonyPatch( typeof( SoulShopOmniUISoulSwapBuyButton ), nameof( SoulShopOmniUISoulSwapBuyButton.OnConfirmButtonPressed ) )]
        internal static class SoulShopOmniUISoulSwapBuyButton_OnConfirmButtonPressed_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "SoulShopOmniUISoulSwapBuyButton.OnConfirmButtonPressed" );
                if( WobSettings.Get( "Resources", "ConvertOres", false ) ) {
                    // Perform the patching
                    transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new( OpCodes.Ldfld, name: "RuneOreCollected" ), // SaveManager.PlayerSaveData.EquipmentOreCollected
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 0, typeof( PlayerSaveData ).GetField( "GoldCollected" ) ), // SaveManager.PlayerSaveData.GoldCollected
                        }, expected: 1 );
                    transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new( OpCodes.Ldfld, name: "EquipmentOreCollected" ), // SaveManager.PlayerSaveData.EquipmentOreCollected
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 0, typeof( PlayerSaveData ).GetField( "GoldCollected" ) ), // SaveManager.PlayerSaveData.GoldCollected
                        }, expected: 1 );
                }
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Ore to gold costs for Strange Transaction, and souls gained
        [HarmonyPatch( typeof( SoulShopOmniUISoulSwapBuyButton ), "ConfirmTransfer" )]
        internal static class SoulShopOmniUISoulSwapBuyButton_ConfirmTransfer_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "SoulShopOmniUISoulSwapBuyButton.ConfirmTransfer" );
                if( WobSettings.Get( "Resources", "ConvertOres", false ) ) {
                    // Perform the patching
                    transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new( OpCodes.Ldsfld, name: "PlayerSaveData"       ), // PlayerSaveData
                            /*  1 */ new( OpCodes.Dup                                  ), // PlayerSaveData
                            /*  2 */ new( OpCodes.Ldfld, name: "RuneOreCollected"      ), // PlayerSaveData.RuneOreCollected
                            /*  3 */ new( OpCodeSet.Ldloc                              ), // soulSwapCost
                            /*  4 */ new( OpCodes.Sub                                  ), // PlayerSaveData.RuneOreCollected - soulSwapCost
                            /*  5 */ new( OpCodes.Stfld, name: "RuneOreCollected"      ), // PlayerSaveData.RuneOreCollected = PlayerSaveData.RuneOreCollected - soulSwapCost
                            /*  6 */ new( OpCodes.Ldsfld, name: "PlayerSaveData"       ), // PlayerSaveData
                            /*  7 */ new( OpCodes.Dup                                  ), // PlayerSaveData
                            /*  8 */ new( OpCodes.Ldfld, name: "EquipmentOreCollected" ), // PlayerSaveData.EquipmentOreCollected
                            /*  9 */ new( OpCodeSet.Ldloc                              ), // soulSwapCost
                            /* 10 */ new( OpCodes.Sub                                  ), // PlayerSaveData.EquipmentOreCollected - soulSwapCost
                            /* 11 */ new( OpCodes.Stfld, name: "EquipmentOreCollected" ), // PlayerSaveData.EquipmentOreCollected = PlayerSaveData.EquipmentOreCollected - soulSwapCost
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            // Change the cost from ore to gold
                            new WobTranspiler.OpAction_SetOperand( 2, typeof( PlayerSaveData ).GetField( "GoldCollected" ) ), // PlayerSaveData.GoldCollected
                            new WobTranspiler.OpAction_SetOperand( 5, typeof( PlayerSaveData ).GetField( "GoldCollected" ) ), // PlayerSaveData.GoldCollected
                            // Add a call that causes the UI to update gold totals and save the purchase into the save data
                            new WobTranspiler.OpAction_SetInstruction( 6, OpCodes.Ldc_I4, (int)GameEvent.GoldChanged ), // GameEvent.GoldChanged
                            new WobTranspiler.OpAction_SetInstruction( 7, OpCodes.Ldarg_0, null                      ), // this
                            new WobTranspiler.OpAction_SetInstruction( 8, OpCodes.Ldnull, null                       ), // null
                            new WobTranspiler.OpAction_SetInstruction( 9, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Messenger<GameMessenger,GameEvent>.Broadcast( GameEvent.GoldChanged, null, null ) ) ), // Messenger<GameMessenger,GameEvent>.Broadcast( GameEvent.GoldChanged, this, null )
                            // Remove last 2 unnecessary instructions
                            new WobTranspiler.OpAction_Remove( 10, 2 ),
                        }, expected: 1 );
                }
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldc_I4, Souls_EV.SOULS_PER_LEVEL ), // 150
                        /*  1 */ new( OpCodeSet.Stloc                          ), // int num = 150
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 0, WobSettings.Get( soulShopKeys.Get( SoulShopType.SoulSwap, "GainSouls" ), 150 ) ), // Set the new number of souls
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        [HarmonyPatch( typeof( SoulShopOmniUISoulSwapBuyButton ), "InitializeConfirmMenu" )]
        internal static class SoulShopOmniUISoulSwapBuyButton_InitializeConfirmMenu_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "SoulShopOmniUISoulSwapBuyButton.InitializeConfirmMenu" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldc_I4, Souls_EV.SOULS_PER_LEVEL ), // 150
                        /*  1 */ new( OpCodeSet.Stloc                          ), // int num = 150
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 0, WobSettings.Get( soulShopKeys.Get( SoulShopType.SoulSwap, "GainSouls" ), 150 ) ), // Set the new number of souls
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        [HarmonyPatch( typeof( SoulShopOmniUITransferResourceButton ), "ConfirmTransfer" )]
        internal static class SoulShopOmniUITransferResourceButton_ConfirmTransfer_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "SoulShopOmniUITransferResourceButton.ConfirmTransfer" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldsfld, name: "SoulTransferLevel" ), // SoulTransferLevel
                        /*  1 */ new( OpCodes.Ldc_I4, Souls_EV.SOULS_PER_LEVEL  ), // 150
                        /*  2 */ new( OpCodes.Mul                               ), // SoulTransferLevel * 150
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( soulShopKeys.Get( SoulShopType.SoulSwap, "GainSouls" ), 150 ) ), // Set the new number of souls
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        [HarmonyPatch( typeof( SoulShopOmniUITransferResourceButton ), "InitializeConfirmMenu" )]
        internal static class SoulShopOmniUITransferResourceButton_InitializeConfirmMenu_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "SoulShopOmniUITransferResourceButton.InitializeConfirmMenu" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldsfld, name: "SoulTransferLevel" ), // SoulTransferLevel
                        /*  1 */ new( OpCodes.Ldc_I4, Souls_EV.SOULS_PER_LEVEL  ), // 150
                        /*  2 */ new( OpCodes.Mul                               ), // SoulTransferLevel * 150
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( soulShopKeys.Get( SoulShopType.SoulSwap, "GainSouls" ), 150 ) ), // Set the new number of souls
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Patch to apply a cap on equip weight and rune weight upgrades overload levels
        [HarmonyPatch( typeof( SkillTreeObj ), nameof( SkillTreeObj.MaxLevel ), MethodType.Getter )]
        internal static class SkillTreeObj_MaxLevel_Patch {
            // These are the upgrades to be edited for equip weight and rune weight
            private static readonly SkillTreeType[] equipUpgrades = new SkillTreeType[] { SkillTreeType.Equip_Up, SkillTreeType.Equip_Up2, SkillTreeType.Equip_Up3 };
            private static readonly SkillTreeType[] runeUpgrades = new SkillTreeType[] { SkillTreeType.Rune_Equip_Up, SkillTreeType.Rune_Equip_Up2, SkillTreeType.Rune_Equip_Up3 };
            // Only need to edit once
            private static bool runOnce = false;
            internal static int equipOverloadCap = 0;
            internal static int runeOverloadCap = 0;

            // The patch itself - call the calculation methods
            internal static void Prefix() {
                if( !runOnce ) {
                    UpdateCaps();
                    runOnce = true;
                }
            }

            internal static void UpdateCaps() {
                if( WobPlugin.Debug ) { GetMaximumWeights(); }
                SetMaxLevel( equipUpgrades, equipOverloadCap );
                SetMaxLevel( runeUpgrades, runeOverloadCap );
            }

            private static void SetMaxLevel( SkillTreeType[] skillTreeTypes, int overloadLevels ) {
                if( overloadLevels < 0 ) { return; }
                // Get the Soul Shop item that governs weight upgrades
                SoulShopData soulShopData = SaveManager.ModeSaveData.GetSoulShopObj( SoulShopType.BaseStats04MaxUp ).SoulShopData;
                // Loop through the upgrades
                foreach( SkillTreeType skillTreeType in skillTreeTypes ) {
                    // Get the upgrade item
                    SkillTreeObj skillTreeObj = SkillTreeManager.GetSkillTreeObj( skillTreeType );
                    // Calculate the new cap for the skill
                    int overloadLevelCap = skillTreeObj.SkillTreeData.MaxLevel + ( System.Math.Min( soulShopData.MaxLevel + overloadLevels, soulShopData.OverloadMaxLevel ) * skillTreeObj.SkillTreeData.AdditiveSoulShopLevels );
                    // Make sure we are not setting the cap to below the current purchased level
                    overloadLevelCap = Mathf.Max( SkillTreeManager.GetSkillTreeObj( skillTreeType ).Level, overloadLevelCap );
                    // Set the cap on the skill
                    skillTreeObj.SkillTreeData.OverloadLevelCap = overloadLevelCap;
                    WobPlugin.Log( "[SoulShop] " + skillTreeType + ": " + ( skillTreeObj.SkillTreeData.MaxLevel + ( soulShopData.MaxLevel * skillTreeObj.SkillTreeData.AdditiveSoulShopLevels ) ) + " base, " 
                            + overloadLevelCap + " overload, for total weight " + ( skillTreeObj.FirstLevelStatGain + skillTreeObj.AdditionalLevelStatGain * ( overloadLevelCap - 1 ) ) );
                }
            }

            // Calculate the maximum weight of equipment and runea that can be on a character at once, and scale so this is within the lightest category for maximum resolve
            private static void GetMaximumWeights() {
                // Running maximums of the weights of max level equipment in each category
                Dictionary<EquipmentCategoryType, int> maxWeights = new();
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
                WobPlugin.Log( "[SoulShop] Max equipment weight: " + totalEquipWeight + ", capacity for lightest category: " + Mathf.CeilToInt( totalEquipWeight / weightCat ) );
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
                WobPlugin.Log( "[SoulShop] Max rune weight: " + totalRuneWeight );
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
            private static readonly HashSet<SkillTreeType> upgrades = new() { SkillTreeType.Equip_Up, SkillTreeType.Equip_Up2, SkillTreeType.Equip_Up3, SkillTreeType.Rune_Equip_Up, SkillTreeType.Rune_Equip_Up2, SkillTreeType.Rune_Equip_Up3 };
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

        [HarmonyPatch( typeof( Mastery_EV ), nameof( Mastery_EV.GetMaxMasteryRank ) )]
        internal static class Mastery_EV_GetMaxMasteryRank_Patch {
            internal static void Postfix( ref int __result ) {
                int num = WobSettings.Get( soulShopKeys.Get( SoulShopType.MaxMasteryFlat, "BaseMaxMastery" ), 15 ) - 1;
                SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.MaxMasteryFlat);
                if( !soulShopObj.IsNativeNull() ) {
                    num += Mathf.RoundToInt( soulShopObj.CurrentStatGain );
                }
                __result = Mathf.Clamp( num, 0, Mastery_EV.XP_REQUIRED.Length );
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - NEW CHARACTERS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        [HarmonyPatch( typeof( CharacterCreator ), nameof( CharacterCreator.GenerateRandomCharacter ) )]
        internal static class CharacterCreator_GenerateRandomCharacter_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "CharacterCreator.GenerateRandomCharacter" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Call, name: "GetAvailableClasses" ), // CharacterCreator.GetAvailableClasses()
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetInstruction( 0, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => GetAvailableClasses() ) ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static List<ClassType> GetAvailableClasses() {
                WobPlugin.Log( "[Abilities] GenerateRandomCharacter.GetAvailableClasses called" );
                List<ClassType> classes = CharacterCreator.GetAvailableClasses();
                SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj( SoulShopType.ChooseYourClass );
                if( soulShopObj != null && soulShopObj.CurrentEquippedLevel > 0 ) {
                    if( WobSettings.Get( soulShopKeys.Get( SoulShopType.ChooseYourClass, "UseMinMastery" ), false ) ) {
                        ClassType lowestLevelClass = SaveManager.ModeSaveData.SoulShopClassChosen;
                        int lowestMasteryRank = SaveManager.PlayerSaveData.GetClassMasteryRank( lowestLevelClass );
                        foreach( ClassType classType in classes ) {
                            int classMasteryRank = SaveManager.PlayerSaveData.GetClassMasteryRank( classType );
                            if( classMasteryRank < lowestMasteryRank ) {
                                lowestMasteryRank = classMasteryRank;
                                lowestLevelClass = classType;
                            }
                        }
                        if( lowestLevelClass != SaveManager.ModeSaveData.SoulShopClassChosen ) {
                            WobPlugin.Log( "[SoulShop] Changing selected class from " + SaveManager.ModeSaveData.SoulShopClassChosen + " to " + lowestLevelClass );
                            SaveManager.ModeSaveData.SoulShopClassChosen = lowestLevelClass;
                        }
                    }
                    if( WobSettings.Get( soulShopKeys.Get( SoulShopType.ChooseYourClass, "AllSlots" ), false ) ) {
                        ClassType soulShopClassChosen = SaveManager.ModeSaveData.SoulShopClassChosen;
                        if( soulShopClassChosen != ClassType.None ) {
                            WobPlugin.Log( "[SoulShop] CharacterCreator.GetAvailableClasses: Replacing classes" );
                            classes = new List<ClassType> { soulShopClassChosen };
                        }
                    } 
                }
                WobPlugin.Log( "[Abilities] GenerateRandomCharacter.GetAvailableClasses complete" );
                return classes;
            }
        }

        [HarmonyPatch( typeof( LineageWindowController ), "CreateRandomCharacters" )]
        internal static class LineageWindowController_CreateRandomCharacters_Patch {
            // This patch fixes an issue where the always contrarian option is overridden by locked class weapon and talent.
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "LineageWindowController.CreateRandomCharacters" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldsfld, name: "ModeSaveData" ),       // SaveManager.ModeSaveData
                        /*  1 */ new( OpCodes.Ldfld, name: "SoulShopClassChosen" ), // SaveManager.ModeSaveData.SoulShopClassChosen
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetInstruction( 0, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => GetChosenClass() ) ),
                        new WobTranspiler.OpAction_Remove( 1, 1  ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            // Method to replace field read, so it can include a setting check and value override
            private static ClassType GetChosenClass() {
                // Check if the locked class is being applied to all heirs
                if( WobSettings.Get( soulShopKeys.Get( SoulShopType.ForceRandomizeKit, "AllSlots" ), false ) ) {
                    // Setting class is done in the CharacterCreator.GenerateRandomCharacter patch, so override it here - otherwise it overwrites the always contrarian patch on the right slot
                    return ClassType.None;
                } else {
                    // Return the result of the original code we patched out
                    return SaveManager.ModeSaveData.SoulShopClassChosen;
                }
            }
        }

    }
}
