using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_Plus {
    [BepInPlugin( "Wob.NGPlus", "New Game Plus Mod", "0.1.0" )]
    public partial class NGPlus : BaseUnityPlugin {

        private static readonly WobSettings.KeyHelper<BurdenType> keys = new WobSettings.KeyHelper<BurdenType>( "Burden", 2 );

        private static readonly Dictionary<BurdenType,(string Config, string Name, int MaxLevel, int Weight, int StatGain, float StatScaler, string StatName)> BurdenInfo = new Dictionary<BurdenType,(string Config, string Name, int MaxLevel, int Weight, int StatGain, float StatScaler, string StatName)>() {
            { BurdenType.EnemyDamage,      ( "EnemyDamage",      "Burden of Power - Enemies deal more damage",                                  5, 1, 8,   0.01f, "Damage %"          ) },
            { BurdenType.EnemyHealth,      ( "EnemyHealth",      "Burden of Vitality - Enemies have more health",                               5, 1, 8,   0.01f, "Health %"          ) },
            { BurdenType.EnemyEvolve,      ( "EnemyEvolve",      "Burden of Evolution - Enemies become stronger variant",                       5, 1, 10,  0.01f, "Chance %"          ) },
            { BurdenType.EnemyLifesteal,   ( "EnemyLifesteal",   "Burden of Blood - Enemies have lifesteal",                                    5, 1, 120, 0.01f, "Lifesteal %"       ) },
            { BurdenType.EnemyArmorShred,  ( "EnemyArmorShred",  "Burden of Metal - Enemies have armor shred",                                  5, 1, 1,   0.01f, "Armor reduction %" ) },
            { BurdenType.EnemyAdapt,       ( "EnemyAdapt",       "Burden of Adaptation - Enemies have commander buff chance",                   5, 1, 3,   0.01f, "Chance %"          ) },
            { BurdenType.EnemyAggression,  ( "EnemyAggression",  "Burden of Aggression - Enemies attack more frequently",                       5, 1, 10,  0.01f, "Aggression %"      ) },
            { BurdenType.EnemySpeed,       ( "EnemySpeed",       "Burden of Mobility - Enemies are faster",                                     5, 1, 7,   0.1f,  "Movement Speed +"  ) },
            { BurdenType.EnemyProjectiles, ( "EnemyProjectiles", "Burden of Flame - Enemy projectiles are faster",                              5, 1, 7,   0.01f, "Speed %"           ) },
            { BurdenType.RoomThreat,       ( "RoomThreat",       "Burden of Ruin - Hazards deal more damage",                                   5, 1, 30,  0.01f, "Damage %"          ) },
            { BurdenType.RoomCount,        ( "RoomCount",        "Burden of Scale - World size increase",                                       3, 1, 10,  0.01f, "Size %"            ) },
            { BurdenType.CommanderTraits,  ( "CommanderTraits",  "Burden of Command - Commanders gain additional buffs",                        2, 2, 1,   0f,    ""                  ) },

            { BurdenType.CastleBossUp,     ( "CastleBossUp",     "Burden of Lamech - Fight Lamech prior to being wounded during the Rebellion", 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.CastleBiomeUp,    ( "CastleBiomeUp",    "Burden of Agartha's Royal Guard - Citadel Agartha becomes more dangerous",    1, 2, 1,   0f,    ""                  ) },
            { BurdenType.BridgeBossUp,     ( "BridgeBossUp",     "Burden of the Beast - Fight the True Form of the Beasts on the Bridge",       1, 1, 1,   0f,    ""                  ) },
            { BurdenType.BridgeBiomeUp,    ( "BridgeBiomeUp",    "Burden of Mundi's Flagship - Axis Mundi becomes more dangerous",              1, 2, 1,   0f,    ""                  ) },
            { BurdenType.ForestBossUp,     ( "ForestBossUp",     "Burden of Naamah - Fight Naamah before the famine",                           1, 1, 1,   0f,    ""                  ) },
            { BurdenType.ForestBiomeUp,    ( "ForestBiomeUp",    "Burden of Kerguelen's Sorrow - The Kerguelen Plateau becomes more dangerous", 1, 2, 1,   0f,    ""                  ) },
            { BurdenType.StudyBossUp,      ( "StudyBossUp",      "Burden of Enoch - Fight Enoch before he succumbs to the poison",              1, 1, 1,   0f,    ""                  ) },
            { BurdenType.StudyBiomeUp,     ( "StudyBiomeUp",     "Burden of the High Scholar - The Stygian Study becomes more dangerous",       1, 2, 1,   0f,    ""                  ) },
            { BurdenType.TowerBossUp,      ( "TowerBossUp",      "Burden of Irad - Fight Irad after the metamorphosis is complete",             1, 1, 1,   0f,    ""                  ) },
            { BurdenType.TowerBiomeUp,     ( "TowerBiomeUp",     "Burden of Irad's Torment - The Sun Tower becomes more dangerous",             1, 2, 1,   0f,    ""                  ) },
            { BurdenType.CaveBossUp,       ( "CaveBossUp",       "Burden of Tubal - Fight Tubal before the madness sinks in",                   1, 1, 1,   0f,    ""                  ) },
            { BurdenType.CaveBiomeUp,      ( "CaveBiomeUp",      "Burden of Pishon's Uprising - Pishon Dry Lake becomes more dangerous",        1, 2, 1,   0f,    ""                  ) },
            { BurdenType.FinalBossUp,      ( "FinalBossUp",      "Burden of Cain - Fight Cain before the guilt",                                1, 1, 1,   0f,    ""                  ) },
        };

        // These are the flags for Pishon Dry Lake minibosses so they do not require keys
        private static readonly List<PlayerSaveFlag> keyFlags = new List<PlayerSaveFlag> {
            PlayerSaveFlag.CaveMiniboss_WhiteDoor_Opened,
            PlayerSaveFlag.CaveMiniboss_BlackDoor_Opened,
        };
        // These are the flags for the repeated dialogues with Jonah
        private static readonly List<PlayerSaveFlag> jonahFlags = new List<PlayerSaveFlag> {
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

        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            foreach( BurdenType burdenType in BurdenInfo.Keys ) {
                keys.Add( burdenType, BurdenInfo[burdenType].Config );
                WobSettings.Add( new WobSettings.Num<int>( keys.Get( burdenType, "MaxLevel" ), "Max burden level for " + BurdenInfo[burdenType].Name, BurdenInfo[burdenType].MaxLevel, bounds: (1, 100) ) );
                WobSettings.Add( new WobSettings.Num<int>( keys.Get( burdenType, "Weight" ), "Weight per level for " + BurdenInfo[burdenType].Name, BurdenInfo[burdenType].Weight, bounds: (1, 10) ) );
                if( BurdenInfo[burdenType].StatScaler != 0f && BurdenInfo[burdenType].StatName != "" ) {
                    WobSettings.Add( new WobSettings.Num<int>( keys.Get( burdenType, "StatGain" ), BurdenInfo[burdenType].StatName + " per level for " + BurdenInfo[burdenType].Name, BurdenInfo[burdenType].StatGain, BurdenInfo[burdenType].StatScaler, bounds: (0, 1000000) ) );
                }
            }
            // Settings for environmental additions
            WobSettings.Add( new List<WobSettings.Entry> {
                new WobSettings.Boolean( keys.Get( BurdenType.CastleBiomeUp, "Triwheels"   ), "Allow the Citadel Agartha flaming triwheels added by the burden",   true ),
                new WobSettings.Boolean( keys.Get( BurdenType.BridgeBiomeUp, "CannonWaves" ), "Allow the Axis Mundi cannonball waves added by the burden",         true ),
                new WobSettings.Boolean( keys.Get( BurdenType.ForestBiomeUp, "Nightmares"  ), "Allow the Kerguelen Plateau frozen nightmares added by the burden", true ),
                new WobSettings.Boolean( keys.Get( BurdenType.StudyBiomeUp,  "VoidWaves"   ), "Allow the Stygian Study void waves added by the burden",            true ),
                new WobSettings.Boolean( keys.Get( BurdenType.TowerBiomeUp,  "Lancers"     ), "Allow the Sun Tower lancer projectiles added by the burden",        true ),
                new WobSettings.Boolean( keys.Get( BurdenType.CaveBiomeUp,   "HandWaves"   ), "Allow the Pishon Dry Lake automaton waves added by the burden",     true ),
            } );
            // Settings for resetting keys, etc.
            WobSettings.Add( new List<WobSettings.Entry> {
                new WobSettings.Boolean( "ResetState", "Reset_ForestKeys",     "When starting a new thread, require gathering Lily of the Valley to unlock Namaah's door",               true ),
                new WobSettings.Boolean( "ResetState", "Reset_CaveKeys",       "When starting a new thread, require collecting the Onyx and Pearl Keys to access the Pishon minibosses", true ),
                new WobSettings.Boolean( "ResetState", "Reset_NamelessKnight", "When starting a new thread, respawn the Nameless Knight in various places with repeated dialogue",       true ),
            } );
            if( !WobSettings.Get( "ResetState", "Reset_CaveKeys", true ) ) {
                foreach( PlayerSaveFlag flag in keyFlags ) {
                    PlayerSaveData_SetFlag_Patch.flags.Add( flag );
                }
            }
            if( !WobSettings.Get( "ResetState", "Reset_NamelessKnight", true ) ) {
                foreach( PlayerSaveFlag flag in jonahFlags ) {
                    PlayerSaveData_SetFlag_Patch.flags.Add( flag );
                }
            }
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // EnemyManager.Internal_CreateBiomePools  -  Evolve
        // RoomEnemyManager.SetupEnemySpawnControllers  -  Evolve
        // SummonEnemy_SummonRule.RunSummonRule  -  Evolve
        // EnemyUtility.GetAllEnemiesInBiome  -  Upgrade biome

        // Patch the method that sets save data flags to prevent resets in NG+
        [HarmonyPatch( typeof( PlayerSaveData ), nameof( PlayerSaveData.SetFlag ) )]
        internal static class PlayerSaveData_SetFlag_Patch {
            // These are the flags that shouldn't be reset in NG+
            public static readonly HashSet<PlayerSaveFlag> flags = new HashSet<PlayerSaveFlag>();
            // Run before the orginal method to intercept and change parameters before it runs
            internal static void Prefix( PlayerSaveData __instance, PlayerSaveFlag flag, ref bool value ) {
                // Check the value being set, what flag it is for, and what the current value is
                if( !value && flags.Contains( flag ) && __instance.FlagTable.ContainsKey( flag ) && __instance.FlagTable[flag] ) {
                    // Override the value parameter to prevent setting to false
                    value = true;
                }
            }
        }

        // Patch the method that sets insight states to prevent resets in NG+
        [HarmonyPatch( typeof( PlayerSaveData ), nameof( PlayerSaveData.SetInsightState ) )]
        internal static class PlayerSaveData_SetInsightState_Patch {
            // Run before the orginal method to intercept and change parameters before it runs
            internal static void Prefix( PlayerSaveData __instance, InsightType insightType, InsightState insightState, ref bool forceOverride ) {
                // Check if the Namaah door is being locked after previously being unlocked
                if( forceOverride && insightType == InsightType.ForestBoss_DoorOpened && insightState == InsightState.Undiscovered && __instance.GetInsightState( insightType ) > InsightState.Undiscovered && !WobSettings.Get( "ResetState", "Reset_ForestKeys", true ) ) {
                    // Remove the force override flag, which means the original method won't reset the insight to a previous state
                    forceOverride = false;
                }
            }
        }

        // Unlock all burdens
        [HarmonyPatch( typeof( BurdenManager ), nameof( BurdenManager.IsBurdenUnlocked ) )]
        internal static class BurdenManager_IsBurdenUnlocked_Patch {
            internal static void Postfix( BurdenType burdenType, ref bool __result ) {
                BurdenObj burden = BurdenManager.GetBurden( burdenType );
                __result = burden != null && !burden.BurdenData.Disabled;
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
                        if( keys.Exists( burdenType ) ) {
                            BurdenData burdenData = BurdenLibrary.GetBurdenData( burdenType );
                            burdenData.MaxBurdenLevel = WobSettings.Get( keys.Get( burdenType, "MaxLevel" ), burdenData.MaxBurdenLevel );
                            burdenData.InitialBurdenCost = WobSettings.Get( keys.Get( burdenType, "Weight" ), burdenData.InitialBurdenCost );
                            if( BurdenInfo[burdenType].StatScaler != 0f && BurdenInfo[burdenType].StatName != "" ) {
                                burdenData.StatsGain = WobSettings.Get( keys.Get( burdenType, "StatGain" ), burdenData.StatsGain );
                            }
                        }
                    }
                    runOnce = true;
                }
            }
        }

        // Disable Citadel Agartha triple orbiter and Kerguelen Plateau ice nightmares
        [HarmonyPatch( typeof( HazardSpawnControllerBase ), nameof( HazardSpawnControllerBase.SetSpawnType ) )]
        internal static class HazardSpawnControllerBase_SetSpawnType_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "HazardSpawnControllerBase.SetSpawnType Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                if( !WobSettings.Get( keys.Get( BurdenType.CastleBiomeUp, "Triwheels" ), true ) ) {
                    // Perform the patching
                    transpiler.PatchAll(
                            // Define the IL code instructions that should be matched
                            new List<WobTranspiler.OpTest> {
                                // hazardType = HazardType.Triple_Orbiter;
                                /*  0 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4, HazardType.Triple_Orbiter ), // HazardType.Triple_Orbiter
                                /*  1 */ new WobTranspiler.OpTest( OpCodeSet.Stloc                             ), // hazardType = HazardType.Triple_Orbiter
                            },
                            // Define the actions to take when an occurrence is found
                            new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_Remove( 0, 2 ),
                            } );
                    // Return the modified instructions
                }
                if( !WobSettings.Get( keys.Get( BurdenType.ForestBiomeUp, "Nightmares" ), true ) ) {
                    // Perform the patching
                    transpiler.PatchAll(
                            // Define the IL code instructions that should be matched
                            new List<WobTranspiler.OpTest> {
                                // hazardType = HazardType.Triple_Orbiter;
                                /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, HazardType.SentryWithIce ), // HazardType.SentryWithIce
                                /*  1 */ new WobTranspiler.OpTest( OpCodeSet.Stloc                          ), // hazardType = HazardType.SentryWithIce
                            },
                            // Define the actions to take when an occurrence is found
                            new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_Remove( 0, 2 ),
                            } );
                    // Return the modified instructions
                }
                return transpiler.GetResult();
            }
        }

        // Disable Axis Mundi wave projectiles
        [HarmonyPatch( typeof( BridgeBiomeUp_BiomeRule ), "OnPlayerEnterRoom" )]
        internal static class BridgeBiomeUp_BiomeRule_OnPlayerEnterRoom_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "BridgeBiomeUp_BiomeRule.OnPlayerEnterRoom Transpiler Patch" );
                if( !WobSettings.Get( keys.Get( BurdenType.BridgeBiomeUp, "CannonWaves" ), true ) ) {
                    // Set up the transpiler handler with the instruction list
                    WobTranspiler transpiler = new WobTranspiler( instructions );
                    // Perform the patching
                    transpiler.PatchAll(
                            // Define the IL code instructions that should be matched
                            new List<WobTranspiler.OpTest> {
                                // this.m_attackCoroutine = this.m_currentRoom.StartCoroutine(this.Attack_Coroutine());
                                /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                          ), // this
                                /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                          ), // this
                                /*  2 */ new WobTranspiler.OpTest( OpCodes.Ldfld, name: "m_currentRoom"     ), // this.m_currentRoom
                                /*  3 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                          ), // this
                                /*  4 */ new WobTranspiler.OpTest( OpCodes.Call, name: "Attack_Coroutine"   ), // this.Attack_Coroutine()
                                /*  5 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "StartCoroutine" ), // m_currentRoom.StartCoroutine(Attack_Coroutine())
                                /*  6 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "m_attackCoroutine" ), // this.m_attackCoroutine = 
                            },
                            // Define the actions to take when an occurrence is found
                            new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_Remove( 0, 7 ),
                            } );
                    // Return the modified instructions
                    return transpiler.GetResult();
                } else {
                    return instructions;
                }
            }
        }

        // Disable Stygian Study wave projectiles
        [HarmonyPatch( typeof( StudyBiomeUp_BiomeRule ), "OnPlayerEnterRoom" )]
        internal static class StudyBiomeUp_BiomeRule_OnPlayerEnterRoom_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "StudyBiomeUp_BiomeRule.OnPlayerEnterRoom Transpiler Patch" );
                if( !WobSettings.Get( keys.Get( BurdenType.StudyBiomeUp, "VoidWaves" ), true ) ) {
                    // Set up the transpiler handler with the instruction list
                    WobTranspiler transpiler = new WobTranspiler( instructions );
                    // Perform the patching
                    transpiler.PatchAll(
                            // Define the IL code instructions that should be matched
                            new List<WobTranspiler.OpTest> {
                                // this.m_voidAttackCoroutine = this.m_currentRoom.StartCoroutine(this.Void_Attack_Coroutine());
                                /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                              ), // this
                                /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                              ), // this
                                /*  2 */ new WobTranspiler.OpTest( OpCodes.Ldfld, name: "m_currentRoom"         ), // this.m_currentRoom
                                /*  3 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                              ), // this
                                /*  4 */ new WobTranspiler.OpTest( OpCodes.Call, name: "Void_Attack_Coroutine"  ), // this.Void_Attack_Coroutine()
                                /*  5 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "StartCoroutine"     ), // m_currentRoom.StartCoroutine(Void_Attack_Coroutine())
                                /*  6 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "m_voidAttackCoroutine" ), // this.m_voidAttackCoroutine = 
                            },
                            // Define the actions to take when an occurrence is found
                            new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_Remove( 0, 7 ),
                            } );
                    // Return the modified instructions
                    return transpiler.GetResult();
                } else {
                    return instructions;
                }
            }
        }

        // Disable Sun Tower wave projectiles
        [HarmonyPatch( typeof( TowerBiomeUp_BiomeRule ), "OnPlayerEnterRoom" )]
        internal static class TowerBiomeUp_BiomeRule_OnPlayerEnterRoom_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "TowerBiomeUp_BiomeRule.OnPlayerEnterRoom Transpiler Patch" );
                if( !WobSettings.Get( keys.Get( BurdenType.TowerBiomeUp, "Lancers" ), true ) ) {
                    // Set up the transpiler handler with the instruction list
                    WobTranspiler transpiler = new WobTranspiler( instructions );
                    // Perform the patching
                    transpiler.PatchAll(
                            // Define the IL code instructions that should be matched
                            new List<WobTranspiler.OpTest> {
                                // this.m_lancerAttackCoroutine = this.m_currentRoom.StartCoroutine(this.Lancer_Attack_Coroutine());
                                /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                                ), // this
                                /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                                ), // this
                                /*  2 */ new WobTranspiler.OpTest( OpCodes.Ldfld, name: "m_currentRoom"           ), // this.m_currentRoom
                                /*  3 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                                ), // this
                                /*  4 */ new WobTranspiler.OpTest( OpCodes.Call, name: "Lancer_Attack_Coroutine"  ), // this.Lancer_Attack_Coroutine()
                                /*  5 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "StartCoroutine"       ), // m_currentRoom.StartCoroutine(Lancer_Attack_Coroutine())
                                /*  6 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "m_lancerAttackCoroutine" ), // this.m_lancerAttackCoroutine = 
                            },
                            // Define the actions to take when an occurrence is found
                            new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_Remove( 0, 7 ),
                            } );
                    // Return the modified instructions
                    return transpiler.GetResult();
                } else {
                    return instructions;
                }
            }
        }

        // Disable Pishon Dry Lake wave projectiles
        [HarmonyPatch( typeof( CaveBiomeUp_BiomeRule ), "OnPlayerEnterRoom" )]
        internal static class CaveBiomeUp_BiomeRule_OnPlayerEnterRoom_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "CaveBiomeUp_BiomeRule.OnPlayerEnterRoom Transpiler Patch" );
                if( !WobSettings.Get( keys.Get( BurdenType.CaveBiomeUp, "HandWaves" ), true ) ) {
                    // Set up the transpiler handler with the instruction list
                    WobTranspiler transpiler = new WobTranspiler( instructions );
                    // Perform the patching
                    transpiler.PatchAll(
                            // Define the IL code instructions that should be matched
                            new List<WobTranspiler.OpTest> {
                                // this.m_waveAttackCoroutine = this.m_currentRoom.StartCoroutine(this.Wave_Attack_Coroutine());
                                /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                              ), // this
                                /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                              ), // this
                                /*  2 */ new WobTranspiler.OpTest( OpCodes.Ldfld, name: "m_currentRoom"         ), // this.m_currentRoom
                                /*  3 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                              ), // this
                                /*  4 */ new WobTranspiler.OpTest( OpCodes.Call, name: "Wave_Attack_Coroutine"  ), // this.Wave_Attack_Coroutine()
                                /*  5 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "StartCoroutine"     ), // m_currentRoom.StartCoroutine(Wave_Attack_Coroutine())
                                /*  6 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "m_waveAttackCoroutine" ), // this.m_waveAttackCoroutine = 
                            },
                            // Define the actions to take when an occurrence is found
                            new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_Remove( 0, 7 ),
                            } );
                    // Return the modified instructions
                    return transpiler.GetResult();
                } else {
                    return instructions;
                }
            }
        }
    }
}