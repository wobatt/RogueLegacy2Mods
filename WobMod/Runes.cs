using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace WobMod {
    public partial class Runes {

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTING DEFAULTS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        private record RuneInfo( string Config, string Name, string StatName, int Cost, int Weight, float StatGain, bool IsAdd );
        private static readonly Dictionary<RuneType, RuneInfo> runeInfo = new() {
            { RuneType.Lifesteal,            new( "Lifesteal",     "Lifesteal Rune - Restore Health for every enemy defeated (Scales with Strength).",                        "Health gain",  800,  30, 3f,   false ) },
            { RuneType.SoulSteal,            new( "SoulSteal",     "Soulsteal Rune - Restore Health for every enemy defeated (Scales with Intelligence).",                    "Health gain",  800,  30, 3f,   false ) },
            { RuneType.WeaponCritChanceAdd,  new( "Sharpened",     "Sharpened Rune - Increases your Weapon Crit. Chance.",                                                    "Crit chance",  800,  30, 2.5f, false ) },
            { RuneType.WeaponCritDamageAdd,  new( "Might",         "Might Rune - Increases your Weapon Crit. Damage.",                                                        "Crit damage",  800,  30, 5f,   false ) },
            { RuneType.MagicCritChanceAdd,   new( "Focal",         "Focal Rune - Increases your Magic Crit. Chance.",                                                         "Crit chance",  800,  30, 2.5f, false ) },
            { RuneType.MagicCritDamageAdd,   new( "Eldar",         "Eldar Rune - Increases your Magic Crit. Damage.",                                                         "Crit damage",  800,  30, 5f,   false ) },
            { RuneType.SuperCritChanceAdd,   new( "LuckyRoller",   "Lucky Roller Rune - Skill Crits have a bonus chance of becoming Super Crits.",                            "Crit chance",  800,  30, 2.5f, false ) },
            { RuneType.SuperCritDamageAdd,   new( "HighStakes",    "High Stakes Rune - Increases the final damage multiplier from Super Crits.",                              "Crit damage",  800,  30, 5f,   false ) },
            { RuneType.ManaRegen,            new( "Siphon",        "Siphon Rune - Restore more Mana per hit.",                                                                "Mana regen",   800,  30, 5f,   false ) },
            { RuneType.MaxMana,              new( "Capacity",      "Capacity Rune - Increases Max Mana Capacity.",                                                            "Max mana",     500,  20, 20f,  true  ) },
            { RuneType.StatusEffectDuration, new( "Amplification", "Amplification Rune - Increases the duration of status effects.",                                          "Duration",     500,  20, 7.5f, false ) },
            { RuneType.ManaOnSpinKick,       new( "Trick",         "Trick Rune - Gain Mana when you Spin Kick an enemy.",                                                     "Mana regen",   500,  20, 1f,   true  ) },
            { RuneType.ArmorRegen,           new( "Reinforced",    "Reinforced Rune - Increases total Armor by 8%.",                                                          "Armor gain",   1000, 40, 8f,   false ) },
            { RuneType.ArmorMinBlock,        new( "Folded",        "Folded Rune - Increases Armor's Max Block Cap.",                                                          "Max block",    500,  20, 4f,   false ) },
            { RuneType.ArmorHealth,          new( "Quenching",     "Quenching Rune - Mana Potions restore a percentage of your Max Health and Armor (Scales with Vitality).", "Restored",     800,  30, 2.5f, false ) },
            { RuneType.ReturnDamage,         new( "Retaliation",   "Retaliation Rune - Deal damage to enemies that hit you (Scales with Vitality). ",                         "Damage",       800,  30, 60f,  false ) },
            { RuneType.Magnet,               new( "Magnesis",      "Magnesis Rune - Increases the magnetic distance that makes coins fly toward you.",                        "Distance",     300,  10, 5f,   true  ) },
            { RuneType.GoldGain,             new( "Bounty",        "Bounty Rune - Gain more gold.",                                                                           "Gold gain",    500,  20, 10f,  false ) },
            { RuneType.OreGain,              new( "Stone",         "Stone Rune - Gain more Ore.",                                                                             "Ore gain",     500,  20, 10f,  false ) },
            { RuneType.RuneOreGain,          new( "Red",           "Red Rune - Gain more Red Aether.",                                                                        "Aether gain",  500,  20, 10f,  false ) },
            { RuneType.ResolveGain,          new( "Resolve",       "Resolve Rune - Gain bonus Resolve.",                                                                      "Resolve gain", 800,  30, 10f,  false ) },
            { RuneType.Haste,                new( "Haste",         "Haste Rune - Increases movement speed.",                                                                  "Move speed",   500,  20, 10f,  false ) },
            { RuneType.Dash,                 new( "Dash",          "Dash Rune - Gain additional Air Dashes.",                                                                 "Air dashes",   1300, 50, 1f,   true  ) },
            { RuneType.DoubleJump,           new( "Vault",         "Vault Rune - Gain additional Double Jumps.",                                                              "Double jumps", 1500, 60, 1f,   true  ) },
        };

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTINGS READ/WRITE
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        internal static readonly WobSettings.KeyHelper<RuneType> runeKeys = new( "Rune", 2 );

        internal static void RunSetup() {
            WobMod.configFiles.Add( "Runes", "Runes" );
            for( int i = 0; i < Economy_EV.RUNE_LEVEL_GOLD_MOD.Length; i++ ) {
                WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "Runes" ), "ScalingCosts_Rune_Gold", "GoldCostMult_Level" + ( i + 1 ), "Multiply base gold cost by this value to get the rune cost for level " + ( i + 1 ), Economy_EV.RUNE_LEVEL_GOLD_MOD[i], bounds: (1, 1000000) ) );
            }
            for( int i = 0; i < Economy_EV.RUNE_LEVEL_ORE_MOD.Length; i++ ) {
                WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "Runes" ), "ScalingCosts_Rune_Aether", "AetherCostMult_Level" + ( i + 1 ), "Multiply base red aether cost by this value to get the rune cost for level " + ( i + 1 ), Economy_EV.RUNE_LEVEL_ORE_MOD[i], bounds: (1, 1000000) ) );
            }
            foreach( RuneType runeType in runeInfo.Keys ) {
                runeKeys.Add( runeType, runeInfo[runeType].Config );
                WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "Runes" ), runeKeys.Get( runeType, "GoldCost" ), "Base gold cost for " + runeInfo[runeType].Name, runeInfo[runeType].Cost, bounds: (1, 1000000) ) );
                WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "Runes" ), runeKeys.Get( runeType, "AetherCost" ), "Base red aether cost for " + runeInfo[runeType].Name, runeInfo[runeType].Cost, bounds: (1, 1000000) ) );
                WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "Runes" ), runeKeys.Get( runeType, "Weight" ), "Rune weight per level for " + runeInfo[runeType].Name, runeInfo[runeType].Weight, bounds: (1, 1000000) ) );
                if( runeType == RuneType.Magnet ) {
                    WobSettings.Add( new WobSettings.Num<float>( WobMod.configFiles.Get( "Runes" ), runeKeys.Get( runeType, "StatGain" ), runeInfo[runeType].StatName + " added per level for " + runeInfo[runeType].Name, runeInfo[runeType].StatGain, bounds: (0f, 1000000f) ) );
                } else if( runeInfo[runeType].IsAdd ) {
                    WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "Runes" ), runeKeys.Get( runeType, "StatGain" ), runeInfo[runeType].StatName + " added per level for " + runeInfo[runeType].Name, Mathf.FloorToInt( runeInfo[runeType].StatGain ), bounds: (0, 1000000) ) );
                } else {
                    WobSettings.Add( new WobSettings.Num<float>( WobMod.configFiles.Get( "Runes" ), runeKeys.Get( runeType, "StatGain" ), runeInfo[runeType].StatName + " percent per level for " + runeInfo[runeType].Name, runeInfo[runeType].StatGain, 0.01f, bounds: (0f, 1000000f) ) );
                }
                WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "Runes" ), runeKeys.Get( runeType, "AddRunes" ), "Additional effective levels to always be added for " + runeInfo[runeType].Name, 0, bounds: (0, 1000) ) );
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - RUNES
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Patch to change scaling gold costs
        [HarmonyPatch( typeof( RuneObj ), nameof( RuneObj.GoldCostToUpgrade ), MethodType.Getter )]
        internal static class RuneObj_GoldCostToUpgrade_Patch {
            private static bool runOnce = false;
            internal static void Prefix() {
                if( !runOnce ) {
                    for( int i = 0; i < Economy_EV.RUNE_LEVEL_GOLD_MOD.Length; i++ ) {
                        Economy_EV.RUNE_LEVEL_GOLD_MOD[i] = WobSettings.Get( "ScalingCosts_Rune_Gold", "GoldCostMult_Level" + ( i + 1 ), Economy_EV.RUNE_LEVEL_GOLD_MOD[i] );
                    }
                    runOnce = true;
                }
            }
            internal static void Postfix( RuneObj __instance, ref int __result ) {
                if( WobSettings.Get( "Resources", "ConvertOres", false ) ) {
                    if( __instance.RuneData.Disabled ) { return; }
                    int blackStoneCost = __instance.RuneData.BlackStoneCost;
                    int newLevel = Mathf.Clamp( __instance.ClampedUpgradeLevel, 0, Economy_EV.RUNE_LEVEL_ORE_MOD.Length - 1 );
                    __result += blackStoneCost * Economy_EV.RUNE_LEVEL_ORE_MOD[newLevel];
                }
            }
        }

        // Patch to change scaling ore costs
        [HarmonyPatch( typeof( RuneObj ), nameof( RuneObj.OreCostToUpgrade ), MethodType.Getter )]
        internal static class RuneObj_OreCostToUpgrade_Patch {
            private static bool runOnce = false;
            internal static void Prefix() {
                if( !runOnce ) {
                    for( int i = 0; i < Economy_EV.RUNE_LEVEL_ORE_MOD.Length; i++ ) {
                        Economy_EV.RUNE_LEVEL_ORE_MOD[i] = WobSettings.Get( "ScalingCosts_Rune_Aether", "AetherCostMult_Level" + ( i + 1 ), Economy_EV.RUNE_LEVEL_ORE_MOD[i] );
                    }
                    runOnce = true;
                }
            }
            internal static void Postfix( ref int __result ) {
                if( WobSettings.Get( "Resources", "ConvertOres", false ) ) {
                    __result = 0;
                }
            }
        }

        // Patch to edit basic rune stats
        [HarmonyPatch( typeof( RuneType_RL ), nameof( RuneType_RL.TypeArray ), MethodType.Getter )]
        internal static class TraitType_RL_TypeArray_Patch {
            private static bool runOnce = false;
            internal static void Prefix() {
                // Only need to run this once, as the new settings are written into the rune data for the session
                if( !runOnce ) {
                    // Get the list of runes from the private field
                    RuneType[] m_typeArray = Traverse.Create( typeof( TraitType_RL ) ).Field( "m_typeArray" ).GetValue<RuneType[]>();
                    // Go through each type in the array
                    foreach( RuneType runeType in m_typeArray ) {
                        // Get the rune data that includes rarity info
                        RuneData runeData = RuneLibrary.GetRuneData( runeType );
                        // Check that it has writable stats that are in the options
                        if( runeData != null && !runeData.Disabled && runeKeys.Exists( runeType ) ) {
                            // Write the values from config into the game data
                            runeData.GoldCost = WobSettings.Get( runeKeys.Get( runeType, "GoldCost" ), runeData.GoldCost );
                            runeData.BlackStoneCost = WobSettings.Get( runeKeys.Get( runeType, "AetherCost" ), runeData.BlackStoneCost );
                            runeData.BaseWeight = WobSettings.Get( runeKeys.Get( runeType, "Weight" ), runeData.BaseWeight );
                            runeData.ScalingWeight = runeData.BaseWeight;
                            runeData.StatMod01 = WobSettings.Get( runeKeys.Get( runeType, "StatGain" ), runeData.StatMod01 );
                            runeData.ScalingStatMod01 = runeData.StatMod01;
                        }
                    }
                    runOnce = true;
                }
            }
        }

        // Patch to make the extra double jumps rune use its stat gain rather than equipped level
        [HarmonyPatch( typeof( RuneLogicHelper ), nameof( RuneLogicHelper.GetExtraJumps ) )]
        internal static class RuneLogicHelper_GetExtraJumps_Patch {
            internal static void Postfix( ref int __result ) {
                __result = Mathf.FloorToInt( RuneManager.GetRune( RuneType.DoubleJump ).CurrentStatModTotal_1 );
            }
        }

        // Patch to make the extra air dashes rune use its stat gain rather than equipped level
        [HarmonyPatch( typeof( RuneLogicHelper ), nameof( RuneLogicHelper.GetExtraDashes ) )]
        internal static class RuneLogicHelper_GetExtraDashes_Patch {
            internal static void Postfix( ref int __result ) {
                __result = Mathf.FloorToInt( RuneManager.GetRune( RuneType.Dash ).CurrentStatModTotal_1 );
            }
        }

        // Patch to add extra runes
        [HarmonyPatch( typeof( RuneObj ), nameof( RuneObj.CurrentStatModTotal_1 ), MethodType.Getter )]
        internal static class RuneObj_CurrentStatModTotal_1_Patch {
            internal static void Postfix( RuneObj __instance, ref float __result ) {
                if( !__instance.RuneData.Disabled ) {
                    // Do not add extra runes for air dash or double jump if the respective heirloom has not been unlocked yet
                    if( __instance.RuneType == RuneType.Dash && SaveManager.PlayerSaveData.GetHeirloomLevel( HeirloomType.UnlockAirDash ) <= 0 ) { return; }
                    if( __instance.RuneType == RuneType.DoubleJump && SaveManager.PlayerSaveData.GetHeirloomLevel( HeirloomType.UnlockDoubleJump ) <= 0 ) { return; }
                    // Add the extra levels to the equipped rune level
                    int effectiveLevel = __instance.EquippedLevel + WobSettings.Get( runeKeys.Get( __instance.RuneType, "AddRunes" ), 0 );
                    // Check that the level is non-zero
                    if( effectiveLevel > 0 ) {
                        // Calculate the new stat gain and override the method return value
                        __result = __instance.RuneData.StatMod01 + ( ( effectiveLevel - 1 ) * ( __instance.RuneData.ScalingStatMod01 ) );
                    }
                }
            }
        }

    }
}
