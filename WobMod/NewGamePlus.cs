using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace WobMod {
    internal class NewGamePlus {

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTING DEFAULTS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        private record BurdenInfo( string Config, string Name, bool Scaling, int MaxLevel, int Weight, int StatGain, float StatScaler, string StatName );
		private static readonly Dictionary<BurdenType, BurdenInfo> burdenInfo = new() {
            { BurdenType.EnemyDamage,      new( "Power",            "Burden of Power - Enemies deal more damage",                                            true,  5, 1, 8,   0.01f, "Damage %"          ) },
            { BurdenType.EnemyHealth,      new( "Vitality",         "Burden of Vitality - Enemies have more health",                                         true,  5, 1, 8,   0.01f, "Health %"          ) },
            { BurdenType.EnemyEvolve,      new( "Evolution",        "Burden of Evolution - Enemies become stronger variant",                                 true,  5, 1, 10,  0.01f, "Chance %"          ) },
            { BurdenType.EnemyLifesteal,   new( "Blood",            "Burden of Blood - Enemies have lifesteal",                                              true,  5, 1, 120, 0.01f, "Lifesteal %"       ) },
            { BurdenType.EnemyArmorShred,  new( "Metal",            "Burden of Metal - Enemies have armor shred",                                            true,  5, 1, 1,   0.01f, "Armor reduction %" ) },
            { BurdenType.EnemyAdapt,       new( "Adaptation",       "Burden of Adaptation - Enemies have commander buff chance",                             true,  5, 1, 3,   0.01f, "Chance %"          ) },
            { BurdenType.EnemyAggression,  new( "Aggression",       "Burden of Aggression - Enemies attack more frequently",                                 true,  5, 1, 10,  0.01f, "Aggression %"      ) },
            { BurdenType.EnemySpeed,       new( "Mobility",         "Burden of Mobility - Enemies are faster",                                               true,  5, 1, 7,   0.1f,  "Movement speed +"  ) },
            { BurdenType.EnemyProjectiles, new( "Flame",            "Burden of Flame - Enemy projectiles are faster",                                        true,  5, 1, 7,   0.01f, "Speed %"           ) },
            { BurdenType.RoomThreat,       new( "Ruin",             "Burden of Ruin - Hazards deal more damage",                                             true,  5, 1, 30,  0.01f, "Damage %"          ) },
            { BurdenType.RoomCount,        new( "Scale",            "Burden of Scale - World size increase",                                                 true,  3, 1, 10,  0.01f, "Size %"            ) },
            { BurdenType.CommanderTraits,  new( "Command",          "Burden of Command - Commanders gain additional buffs",                                  false, 2, 2, 1,   0f,    ""                  ) },
            { BurdenType.LifestealReduc,   new( "Drain",            "Burden of Drain - All forms of life-drain from Runes, Relics, and Traits are reduced",  true,  5, 1, 15,  0.01f, "Lifesteal %"       ) },
            { BurdenType.DeathMark,        new( "BlackRoot",        "Burden of Black Root - You have been afflicted with the Black Root Poison",             false, 3, 2, -1,  0f,    ""                  ) },

            { BurdenType.CastleBossUp,     new( "AgarthaBoss",      "Burden of Lamech - Fight Lamech prior to being wounded during the Rebellion",           false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.CastleEnemyUp,    new( "AgarthaEnemies",   "Burden of Agartha's Royal Guard - The Royal Guards return to Citadel Agartha",          false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.CastleBiomeUp,    new( "AgarthaHazards",   "Burden of Agartha's Fortification - Flaming Pinwheels become Flaming Triwheels",        false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.BridgeBossUp,     new( "AxisMundiBoss",    "Burden of the Beast - Fight the True Form of the Beasts on the Bridge",                 false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.BridgeEnemyUp,    new( "AxisMundiEnemies", "Burden of Mundi's Crossing - Denizens from the Sun Tower and Plateau begin to migrate", false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.BridgeBiomeUp,    new( "AxisMundiHazards", "Burden of Mundi's Flagship - The flagship Preserver of Life begins its assault",        false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.ForestBossUp,     new( "KerguelenBoss",    "Burden of Naamah - Fight Naamah before the famine",                                     false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.ForestEnemyUp,    new( "KerguelenEnemies", "Burden of Kerguelen's Sacrifice - New monsters emerge from the frost",                  false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.ForestBiomeUp,    new( "KerguelenHazards", "Burden of Kerguelen's Frost - The Kerguelen Plateau becomes colder",                    false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.StudyBossUp,      new( "StudyBoss",        "Burden of Enoch - Fight Enoch before he succumbs to the poison",                        false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.StudyEnemyUp,     new( "StudyEnemies",     "Burden of the Study's Scholars - The Elementalists return",                             false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.StudyBiomeUp,     new( "StudyHazards",     "Burden of the High Scholar - The Stygian Study becomes more dangerous",                 false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.TowerBossUp,      new( "TowerBoss",        "Burden of Irad - Fight Irad after the metamorphosis is complete",                       false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.TowerEnemyUp,     new( "TowerEnemies",     "Burden of Irad's Calling - New horrors ascend the Tower",                               false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.TowerBiomeUp,     new( "TowerHazards",     "Burden of Irad's Torment - Spectral Dragon Lancers haunt the Sun Tower",                false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.CaveBossUp,       new( "PishonBoss",       "Burden of Tubal - Fight Tubal before the madness sinks in",                             false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.CaveEnemyUp,      new( "PishonEnemies",    "Burden of Pishon's Mining - New creatures emerge from the Earth",                       false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.CaveBiomeUp,      new( "PishonHazards",    "Burden of Pishon's Uprising - The automaton army awakens",                              false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.FinalBossUp,      new( "FinalBoss",        "Burden of Cain - Fight Cain before the guilt",                                          false, 1, 1, 1,   0f,    ""                  ) },
        };

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTINGS READ/WRITE
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        internal static readonly WobSettings.KeyHelper<BurdenType> burdenKeys = new( "Burden", 2 );

        internal static void RunSetup() {
            WobMod.configFiles.Add( "NewGamePlus", "NewGamePlus" );
            foreach( BurdenType burdenType in burdenInfo.Keys ) {
                burdenKeys.Add( burdenType, burdenInfo[burdenType].Config );
                if( burdenInfo[burdenType].Scaling ) {
                    WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "NewGamePlus" ), burdenKeys.Get( burdenType, "MaxLevel" ), "Max burden level for " + burdenInfo[burdenType].Name, burdenInfo[burdenType].MaxLevel, bounds: (1, 100) ) );
                }
                WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "NewGamePlus" ), burdenKeys.Get( burdenType, "Weight" ), "Weight per level for " + burdenInfo[burdenType].Name, burdenInfo[burdenType].Weight, bounds: (1, 10) ) );
                if( burdenInfo[burdenType].Scaling ) {
                    WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "NewGamePlus" ), burdenKeys.Get( burdenType, "StatGain" ), burdenInfo[burdenType].StatName + " per level for " + burdenInfo[burdenType].Name, burdenInfo[burdenType].StatGain, burdenInfo[burdenType].StatScaler, bounds: (0, 1000000) ) );
                }
            }
            // Settings for resetting keys, etc.
            WobSettings.Add( new List<WobSettings.Entry> {
                new WobSettings.Boolean(  WobMod.configFiles.Get( "NewGamePlus" ), "ResetState",   "Reset_AgarthaDoor",     "When starting a new thread, require hitting the lamps to unlock Lamech's door",                          true  ),
                new WobSettings.Boolean(  WobMod.configFiles.Get( "NewGamePlus" ), "ResetState",   "Reset_AxisMundiGate",   "When starting a new thread, require beating the Void Beasts to raise the gate",                          true  ),
                new WobSettings.Boolean(  WobMod.configFiles.Get( "NewGamePlus" ), "ResetState",   "Reset_KerguelenLilies", "When starting a new thread, require gathering Lily of the Valley to unlock Namaah's door",               true  ),
                new WobSettings.Boolean(  WobMod.configFiles.Get( "NewGamePlus" ), "ResetState",   "Reset_PishonKeys",      "When starting a new thread, require collecting the Onyx and Pearl Keys to access the Pishon minibosses", true  ),
                new WobSettings.Boolean(  WobMod.configFiles.Get( "NewGamePlus" ), "ResetState",   "Reset_NamelessKnight",  "When starting a new thread, respawn the Nameless Knight in various places with repeated dialogue",       true  ),
                new WobSettings.Boolean(  WobMod.configFiles.Get( "NewGamePlus" ), "ResetState",   "Reset_Teleporters",     "When starting a new thread, require paying to unlock teleporters in each biome",                         true  ),
                new WobSettings.Boolean(  WobMod.configFiles.Get( "NewGamePlus" ), "MiscSettings", "UnlockBurdens",         "Unlock all burdens to allow selection from NG+1",                                                        false ),
                new WobSettings.Boolean(  WobMod.configFiles.Get( "NewGamePlus" ), "MiscSettings", "UnlockLevels",          "Unlock all NG+ level to allow selection after completing the true ending",                               false ),
                new WobSettings.Num<int>( WobMod.configFiles.Get( "NewGamePlus" ), "MiscSettings", "NGLevel1",              "Require 2 burdens per NG level up to this level, and 1 burden after",                                    10, bounds: ( 0, NewGamePlus_EV.MAX_NG_PLUS ) ),
                new WobSettings.Num<int>( WobMod.configFiles.Get( "NewGamePlus" ), "MiscSettings", "NGLevel2",              "Require no additional burdens per NG level after this level",                                            25, bounds: ( 0, NewGamePlus_EV.MAX_NG_PLUS ) ),
            } );
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - BURDENS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Unlock all burdens
        [HarmonyPatch( typeof( BurdenManager ), nameof( BurdenManager.IsBurdenUnlocked ) )]
        internal static class BurdenManager_IsBurdenUnlocked_Patch {
            internal static void Postfix( BurdenType burdenType, ref bool __result ) {
                if( WobSettings.Get( "MiscSettings", "UnlockBurdens", false ) ) {
                    BurdenObj burden = BurdenManager.GetBurden( burdenType );
                    __result = burden != null && !burden.BurdenData.Disabled;
                }
            }
        }

        // Unlock all NG+ Levels
        [HarmonyPatch( typeof( NewGamePlusOmniUIEquipButton ), "GetHighestAllowedNGPlus" )]
        internal static class NewGamePlusOmniUIEquipButton_GetHighestAllowedNGPlus_Patch {
            internal static void Postfix( ref int __result ) {
                if( SaveManager.PlayerSaveData.GetFlag( PlayerSaveFlag.SeenTrueEnding_FirstTime ) && WobSettings.Get( "MiscSettings", "UnlockLevels", false ) ) {
                    __result = NewGamePlus_EV.MAX_NG_PLUS;
                }
            }
        }

        // Set burden max levels, weights, and stat gains
        [HarmonyPatch( typeof( BurdenType_RL ), nameof( BurdenType_RL.TypeArray ), MethodType.Getter )]
        internal static class BurdenType_RL_TypeArray_Patch {
            private static bool runOnce = false;
            internal static void Prefix() {
                // Only need to run this once, as the new settings are written into the burden data for the session
                if( !runOnce ) {
                    // Get the list of burdens from the private field
                    BurdenType[] m_typeArray = (BurdenType[])Traverse.Create( typeof( BurdenType_RL ) ).Field( "m_typeArray" ).GetValue();
                    // Go through each type in the array
                    foreach( BurdenType burdenType in m_typeArray ) {
                        BurdenData burdenData = BurdenLibrary.GetBurdenData( burdenType );
                        if( burdenData != null ) {
                            //WobPlugin.Log( "[NewGamePlus] " + burdenType + ", " + burdenData.MaxBurdenLevel + ", " + burdenData.InitialBurdenCost + ", " + burdenData.StatsGain );
                            if( burdenKeys.Exists( burdenType ) ) {
                                burdenData.InitialBurdenCost = WobSettings.Get( burdenKeys.Get( burdenType, "Weight" ), burdenData.InitialBurdenCost );
                                if( burdenInfo[burdenType].Scaling ) {
                                    burdenData.MaxBurdenLevel = WobSettings.Get( burdenKeys.Get( burdenType, "MaxLevel" ), burdenData.MaxBurdenLevel );
                                    burdenData.StatsGain = WobSettings.Get( burdenKeys.Get( burdenType, "StatGain" ), burdenData.StatsGain );
                                }
                            }
                        }
                    }
                    runOnce = true;
                }
            }
        }

        // Set burden weight requirement
        [HarmonyPatch( typeof( NewGamePlus_EV ), nameof( NewGamePlus_EV.GetBurdensRequiredForNG ) )]
        internal static class NewGamePlus_EV_GetBurdensRequiredForNG_Patch {
            internal static void Postfix( int ngLevel, ref int __result ) {
                int scalingTrigger = WobSettings.Get( "MiscSettings", "NGLevel1", 10 );
                int scalingCap = WobSettings.Get( "MiscSettings", "NGLevel2", 25 );
                ngLevel = Mathf.Min( ngLevel, scalingCap );
                int burdensRequired = ( ngLevel <= 0 ) ? 0 : ( 2 * Mathf.Min( scalingTrigger, ngLevel ) + Mathf.Max( 0, ngLevel - scalingTrigger ) );
                if( SaveManager.PlayerSaveData.SpecialModeType == SpecialModeType.Thanatwophobia ) {
                    burdensRequired += 23;
                } else if( SaveManager.PlayerSaveData.SpecialModeType == SpecialModeType.TrueRogue ) {
                    burdensRequired += 5;
                }
                if( SaveManager.PlayerSaveData.EnableHouseRules ) {
                    burdensRequired = Mathf.FloorToInt( burdensRequired * SaveManager.PlayerSaveData.Assist_BurdenRequirementsMod );
                }
                __result = Mathf.Clamp( burdensRequired, 0, BurdenManager.GetTotalBurdenMaxLevel() );
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - STATE FLAGS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Patch the method that sets save data flags to prevent resets in NG+
        [HarmonyPatch( typeof( PlayerSaveData ), nameof( PlayerSaveData.SetFlag ) )]
        internal static class PlayerSaveData_SetFlag_Patch {
            // These are the flags for Pishon Dry Lake minibosses so they do not require keys
            private static readonly List<PlayerSaveFlag> keyFlags = new() {
                PlayerSaveFlag.CaveMiniboss_WhiteDoor_Opened,
                PlayerSaveFlag.CaveMiniboss_BlackDoor_Opened,
            };

            // These are the flags for the repeated dialogues with Jonah
            private static readonly List<PlayerSaveFlag> jonahFlags = new() {
                PlayerSaveFlag.Johan_First_Death_Intro,
                PlayerSaveFlag.Johan_Getting_Memory_Heirloom,
                PlayerSaveFlag.Johan_Entering_Secret_Tower,
                PlayerSaveFlag.Johan_After_Beating_Castle_Boss,
                PlayerSaveFlag.Johan_Finding_On_Bridge,
                PlayerSaveFlag.Johan_After_Beating_Bridge_Boss,
                PlayerSaveFlag.Johan_Sitting_At_Far_Shore,
                PlayerSaveFlag.Johan_After_Beating_Forest_Boss,
                PlayerSaveFlag.Johan_Reaching_Sun_Tower_Top,
                PlayerSaveFlag.Johan_After_Beating_Study_Boss,
                PlayerSaveFlag.Johan_After_Beating_Tower_Boss,
                PlayerSaveFlag.Johan_After_Getting_Heirloom_Lantern,
            };

            // Run before the orginal method to intercept and change parameters before it runs
            internal static void Prefix( PlayerSaveData __instance, PlayerSaveFlag flag, ref bool value ) {
                // Check the value is being set to false
                if( !value ) {
                    if( keyFlags.Contains( flag ) && !WobSettings.Get( "ResetState", "Reset_PishonKeys", true ) ) {
                        // Change the parameter to the flag's current value to prevent changes
                        value = __instance.GetFlag( flag );
                    }
                    if( jonahFlags.Contains( flag ) && !WobSettings.Get( "ResetState", "Reset_NamelessKnight", true ) ) {
                        // Change the parameter to the flag's current value to prevent changes
                        value = __instance.GetFlag( flag );
                    }
                }
            }
        }

        // Patch the method that sets insight states to prevent resets in NG+
        [HarmonyPatch( typeof( PlayerSaveData ), nameof( PlayerSaveData.SetInsightState ) )]
        internal static class PlayerSaveData_SetInsightState_Patch {
            // Run before the orginal method to intercept and change parameters before it runs
            internal static void Prefix( PlayerSaveData __instance, InsightType insightType, InsightState insightState, ref bool forceOverride ) {
                // Check if the door is being locked after previously being unlocked
                if( forceOverride && insightState == InsightState.Undiscovered && __instance.GetInsightState( insightType ) > InsightState.Undiscovered && (
                        ( insightType == InsightType.CastleBoss_DoorOpened && !WobSettings.Get( "ResetState", "Reset_AgarthaDoor", true ) ) ||
                        ( insightType == InsightType.BridgeBoss_GateRaised && !WobSettings.Get( "ResetState", "Reset_AxisMundiGate", true ) ) ||
                        ( insightType == InsightType.ForestBoss_DoorOpened && !WobSettings.Get( "ResetState", "Reset_KerguelenLilies", true ) ) ) ) {
                    // Remove the force override flag, which means the original method won't reset the insight to a previous state
                    forceOverride = false;
                }
            }
        }

        // Patch the gatehouse room to respect the insight state to decide if the gate should be open, rather than just using the boss defeated flag
        [HarmonyPatch( typeof( BridgeBossEntranceRoomController ), "OnPlayerEnterRoom" )]
        internal static class BridgeBossEntranceRoomController_OnPlayerEnterRoom_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "BridgeBossEntranceRoomController.OnPlayerEnterRoom" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Call, name: "OnPlayerEnterRoom" ), // base.OnPlayerEnterRoom(sender, eventArgs);
                        /*  1 */ new( OpCodeSet.Brfalse                       ), // if (num)
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_Insert( 1, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => ShowGateAnim( false ) ) ),
                    }, expected: 1 );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Call, name: "get_IsRoomComplete" ), // base.IsRoomComplete
                        /*  1 */ new( OpCodeSet.Brfalse                        ), // if (base.IsRoomComplete)
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_Insert( 1, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => ShowGateOpen( false ) ) ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool ShowGateAnim( bool flag ) {
                return flag && ( SaveManager.PlayerSaveData.GetInsightState( InsightType.BridgeBoss_GateRaised ) < InsightState.ResolvedButNotViewed );
            }

            private static bool ShowGateOpen( bool roomComplete ) {
                return roomComplete || ( SaveManager.PlayerSaveData.GetInsightState( InsightType.BridgeBoss_GateRaised ) >= InsightState.ResolvedButNotViewed );
            }
        }

        // Patch the method that sets teleporter unlock states to prevent resets in NG+
        [HarmonyPatch( typeof( PlayerSaveData ), nameof( PlayerSaveData.SetTeleporterIsUnlocked ) )]
        internal static class PlayerSaveData_SetTeleporterIsUnlocked_Patch {
            // Run before the orginal method to intercept and change parameters before it runs
            internal static void Prefix( PlayerSaveData __instance, BiomeType biomeType, ref bool state ) {
                // Check if the teleporter is being locked after previously being unlocked
                if( !state && __instance.TeleporterUnlockTable.TryGetValue( biomeType, out bool unlocked ) && !WobSettings.Get( "ResetState", "Reset_Teleporters", true ) ) {
                    state = unlocked;
                }
            }
        }

    }
}
