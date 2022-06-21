using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace Wob_RuneStats {
    [BepInPlugin( "Wob.RuneStats", "Rune Stats Mod", "1.0.0" )]
    public partial class RuneStats : BaseUnityPlugin {

        private static readonly WobSettings.KeyHelper<RuneType> keys = new WobSettings.KeyHelper<RuneType>( "Rune" );

        private static readonly Dictionary<RuneType,(string Config, string Name, string StatName, int Cost, int Weight, float StatGain, bool IsAdd)> RuneInfo = new Dictionary<RuneType,(string Config, string Name, string StatName, int Cost, int Weight, float StatGain, bool IsAdd)>() {
            // Percent based stat gains
            { RuneType.ArmorHealth,          ( "ArmorHealth",          "Quenching Rune - Mana Potions restore a percentage of your Max Health and Armor (Scales with Vitality).", "Restored",     800,  30, 2.5f, false ) },
            { RuneType.ArmorMinBlock,        ( "ArmorMinBlock",        "Folded Rune - Increases Armor's Max Block Cap.",                                                          "Max block",    500,  20, 4f,   false ) },
            { RuneType.ArmorRegen,           ( "ArmorRegen",           "Reinforced Rune - Increases total Armor by 8%.",                                                          "Armor gain",   1000, 40, 8f,   false ) },
            { RuneType.GoldGain,             ( "GoldGain",             "Bounty Rune - Gain more gold.",                                                                           "Gold gain",    500,  20, 10f,  false ) },
            { RuneType.Haste,                ( "Haste",                "Haste Rune - Increases movement speed.",                                                                  "Move speed",   500,  20, 10f,  false ) },
            { RuneType.Lifesteal,            ( "Lifesteal",            "Lifesteal Rune - Restore Health for every enemy defeated (Scales with Strength).",                        "Health gain",  800,  30, 4f,   false ) },
            { RuneType.MagicCritChanceAdd,   ( "MagicCritChance",      "Focal Rune - Increases your Magic Crit. Chance.",                                                         "Crit chance",  800,  30, 2.5f, false ) },
            { RuneType.MagicCritDamageAdd,   ( "MagicCritDamage",      "Eldar Rune - Increases your Magic Crit. Damage.",                                                         "Crit damage",  800,  30, 5f,   false ) },
            { RuneType.ManaRegen,            ( "ManaRegen",            "Siphon Rune - Restore more Mana per hit.",                                                                "Mana regen",   800,  30, 5f,   false ) },
            { RuneType.OreGain,              ( "OreGain",              "Stone Rune - Gain more Ore.",                                                                             "Ore gain",     500,  20, 10f,  false ) },
            { RuneType.ResolveGain,          ( "ResolveGain",          "Resolve Rune - Gain bonus Resolve.",                                                                      "Resolve gain", 800,  30, 10f,  false ) },
            { RuneType.ReturnDamage,         ( "ReturnDamage",         "Retaliation Rune - Deal damage to enemies that hit you (Scales with Vitality). ",                         "Damage",       800,  30, 75f,  false ) },
            { RuneType.RuneOreGain,          ( "RuneOreGain",          "Red Rune - Gain more Red Aether.",                                                                        "Aether gain",  500,  20, 10f,  false ) },
            { RuneType.SoulSteal,            ( "SoulSteal",            "Soulsteal Rune - Restore Health for every enemy defeated (Scales with Intelligence).",                    "Health gain",  800,  30, 4f,   false ) },
            { RuneType.StatusEffectDuration, ( "StatusEffectDuration", "Amplification Rune - Increases the duration of status effects.",                                          "Duration",     500,  20, 7.5f, false ) },
            { RuneType.SuperCritChanceAdd,   ( "SuperCritChance",      "Lucky Roller Rune - Skill Crits have a bonus chance of becoming Super Crits.",                            "Crit chance",  800,  30, 2.5f, false ) },
            { RuneType.SuperCritDamageAdd,   ( "SuperCritDamage",      "High Stakes Rune - Increases the final damage multiplier from Super Crits.",                              "Crit damage",  800,  30, 5f,   false ) },
            { RuneType.WeaponCritChanceAdd,  ( "WeaponCritChance",     "Sharpened Rune - Increases your Weapon Crit. Chance.",                                                    "Crit chance",  800,  30, 2.5f, false ) },
            { RuneType.WeaponCritDamageAdd,  ( "WeaponCritDamage",     "Might Rune - Increases your Weapon Crit. Damage.",                                                        "Crit damage",  800,  30, 5f,   false ) },
            // Addition based stat gains
            { RuneType.Dash,                 ( "Dash",                 "Dash Rune - Gain additional Air Dashes.",                                                                 "Air dashes",   1300, 50, 1f,   true  ) },
            { RuneType.DoubleJump,           ( "DoubleJump",           "Vault Rune - Gain additional Double Jumps.",                                                              "Double jumps", 1500, 60, 1f,   true  ) },
            { RuneType.Magnet,               ( "Magnet",               "Magnesis Rune - Increases the magnetic distance that makes coins fly toward you.",                        "Distance",     300,  10, 5f,   true  ) },
            { RuneType.ManaOnSpinKick,       ( "ManaOnSpinKick",       "Trick Rune - Gain Mana when you Spin Kick an enemy.",                                                     "Mana regen",   500,  20, 1f,   true  ) },
            { RuneType.MaxMana,              ( "MaxMana",              "Capacity Rune - Increases Max Mana Capacity.",                                                            "Max mana",     500,  20, 20f,  true  ) },
        };

        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            foreach( RuneType runeType in RuneInfo.Keys ) {
                keys.Add( runeType, RuneInfo[runeType].Config );
                WobSettings.Add( new WobSettings.Num<int>( keys.Get( runeType, "GoldCost" ), "Base gold cost for " + RuneInfo[runeType].Name, RuneInfo[runeType].Cost, bounds: (1, 1000000) ) );
                WobSettings.Add( new WobSettings.Num<int>( keys.Get( runeType, "OreCost" ), "Base red aether cost for " + RuneInfo[runeType].Name, RuneInfo[runeType].Cost, bounds: (1, 1000000) ) );
                WobSettings.Add( new WobSettings.Num<int>( keys.Get( runeType, "Weight" ), "Rune weight per level for " + RuneInfo[runeType].Name, RuneInfo[runeType].Weight, bounds: (1, 1000000) ) );
                if( runeType == RuneType.Magnet ) {
                    WobSettings.Add( new WobSettings.Num<float>( keys.Get( runeType, "StatGain" ), RuneInfo[runeType].StatName + " added per level for " + RuneInfo[runeType].Name, RuneInfo[runeType].StatGain, bounds: (0f, 1000000f) ) );
                } else if( RuneInfo[runeType].IsAdd ) {
                    WobSettings.Add( new WobSettings.Num<int>( keys.Get( runeType, "StatGain" ), RuneInfo[runeType].StatName + " added per level for " + RuneInfo[runeType].Name, Mathf.FloorToInt( RuneInfo[runeType].StatGain ), bounds: (0, 1000000) ) );
                } else {
                    WobSettings.Add( new WobSettings.Num<float>( keys.Get( runeType, "StatGain" ), RuneInfo[runeType].StatName + " percent per level for " + RuneInfo[runeType].Name, RuneInfo[runeType].StatGain, 0.01f, bounds: (0f, 1000000f) ) );
                }
                WobSettings.Add( new WobSettings.Num<int>( keys.Get( runeType, "AddRunes" ), "Additional effective levels to always be added for " + RuneInfo[runeType].Name, 0, bounds: (0, 1000) ) );
            }
            for( int i = 0; i < Economy_EV.RUNE_LEVEL_GOLD_MOD.Length; i++ ) {
                WobSettings.Add( new WobSettings.Num<int>( "ScalingGoldCost", "GoldCostMult_Level" + ( i + 1 ), "Multiply base gold cost by this value to get the rune cost for level " + ( i + 1 ), Economy_EV.RUNE_LEVEL_GOLD_MOD[i], bounds: (1, 1000000) ) );
            }
            for( int i = 0; i < Economy_EV.RUNE_LEVEL_ORE_MOD.Length; i++ ) {
                WobSettings.Add( new WobSettings.Num<int>( "ScalingOreCost", "OreCostMult_Level" + ( i + 1 ), "Multiply base ore cost by this value to get the rune cost for level " + ( i + 1 ), Economy_EV.RUNE_LEVEL_ORE_MOD[i], bounds: (1, 1000000) ) );
            }
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
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
                        if( runeData != null && !runeData.Disabled && keys.Exists( runeType ) ) {
                            // Write the values from config into the game data
                            runeData.GoldCost = WobSettings.Get( keys.Get( runeType, "GoldCost" ), runeData.GoldCost );
                            runeData.BlackStoneCost = WobSettings.Get( keys.Get( runeType, "AetherCost" ), runeData.BlackStoneCost );
                            runeData.BaseWeight = WobSettings.Get( keys.Get( runeType, "Weight" ), runeData.BaseWeight );
                            runeData.ScalingWeight = runeData.BaseWeight;
                            runeData.StatMod01 = WobSettings.Get( keys.Get( runeType, "StatGain" ), runeData.StatMod01 );
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
                    if( __instance.RuneType == RuneType.Dash       && SaveManager.PlayerSaveData.GetHeirloomLevel( HeirloomType.UnlockAirDash    ) <= 0 ) { return; }
                    if( __instance.RuneType == RuneType.DoubleJump && SaveManager.PlayerSaveData.GetHeirloomLevel( HeirloomType.UnlockDoubleJump ) <= 0 ) { return; }
                    // Add the extra levels to the equipped rune level
                    int effectiveLevel = __instance.EquippedLevel + WobSettings.Get( keys.Get( __instance.RuneType, "AddRunes" ), 0 );
                    // Check that the level is non-zero
                    if( effectiveLevel > 0 ) {
                        // Calculate the new stat gain and override the method return value
                        __result = __instance.RuneData.StatMod01 + ( ( effectiveLevel - 1 ) * ( __instance.RuneData.ScalingStatMod01 ) );
                    }
                }
            }
        }

        // Patch to change scaling gold costs
        [HarmonyPatch( typeof( RuneObj ), nameof( RuneObj.GoldCostToUpgrade ), MethodType.Getter )]
        internal static class RuneObj_GoldCostToUpgrade_Patch {
            private static bool runOnce = false;
            internal static void Prefix() {
                if( !runOnce ) {
                    for( int i = 0; i < Economy_EV.RUNE_LEVEL_GOLD_MOD.Length; i++ ) {
                        Economy_EV.RUNE_LEVEL_GOLD_MOD[i] = WobSettings.Get( "ScalingGoldCost", "GoldCostMult_Level" + ( i + 1 ), Economy_EV.RUNE_LEVEL_GOLD_MOD[i] );
                    }
                    runOnce = true;
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
                        Economy_EV.RUNE_LEVEL_ORE_MOD[i] = WobSettings.Get( "ScalingOreCost", "OreCostMult_Level" + ( i + 1 ), Economy_EV.RUNE_LEVEL_ORE_MOD[i] );
                    }
                    runOnce = true;
                }
            }
        }
    }
}