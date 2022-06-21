using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_Plus {
    [BepInPlugin( "Wob.NGPlus", "New Game Plus Mod", "1.0.0" )]
    public partial class NGPlus : BaseUnityPlugin {

        private static readonly WobSettings.KeyHelper<BurdenType> keys = new WobSettings.KeyHelper<BurdenType>( "Burden", 2 );

        private static readonly Dictionary<BurdenType,(string Config, string Name, bool Scaling, int MaxLevel, int Weight, int StatGain, float StatScaler, string StatName)> BurdenInfo = new Dictionary<BurdenType,(string Config, string Name, bool Scaling, int MaxLevel, int Weight, int StatGain, float StatScaler, string StatName)>() {
            { BurdenType.EnemyDamage,      ( "EnemyDamage",      "Burden of Power - Enemies deal more damage",                                  true,  5, 1, 8,   0.01f, "Damage %"          ) },
            { BurdenType.EnemyHealth,      ( "EnemyHealth",      "Burden of Vitality - Enemies have more health",                               true,  5, 1, 8,   0.01f, "Health %"          ) },
            { BurdenType.EnemyEvolve,      ( "EnemyEvolve",      "Burden of Evolution - Enemies become stronger variant",                       true,  5, 1, 10,  0.01f, "Chance %"          ) },
            { BurdenType.EnemyLifesteal,   ( "EnemyLifesteal",   "Burden of Blood - Enemies have lifesteal",                                    true,  5, 1, 120, 0.01f, "Lifesteal %"       ) },
            { BurdenType.EnemyArmorShred,  ( "EnemyArmorShred",  "Burden of Metal - Enemies have armor shred",                                  true,  5, 1, 1,   0.01f, "Armor reduction %" ) },
            { BurdenType.EnemyAdapt,       ( "EnemyAdapt",       "Burden of Adaptation - Enemies have commander buff chance",                   true,  5, 1, 3,   0.01f, "Chance %"          ) },
            { BurdenType.EnemyAggression,  ( "EnemyAggression",  "Burden of Aggression - Enemies attack more frequently",                       true,  5, 1, 10,  0.01f, "Aggression %"      ) },
            { BurdenType.EnemySpeed,       ( "EnemySpeed",       "Burden of Mobility - Enemies are faster",                                     true,  5, 1, 7,   0.1f,  "Movement Speed +"  ) },
            { BurdenType.EnemyProjectiles, ( "EnemyProjectiles", "Burden of Flame - Enemy projectiles are faster",                              true,  5, 1, 7,   0.01f, "Speed %"           ) },
            { BurdenType.RoomThreat,       ( "RoomThreat",       "Burden of Ruin - Hazards deal more damage",                                   true,  5, 1, 30,  0.01f, "Damage %"          ) },
            { BurdenType.RoomCount,        ( "RoomCount",        "Burden of Scale - World size increase",                                       true,  3, 1, 10,  0.01f, "Size %"            ) },
            { BurdenType.CommanderTraits,  ( "CommanderTraits",  "Burden of Command - Commanders gain additional buffs",                        false, 2, 2, 1,   0f,    ""                  ) },

            { BurdenType.CastleBossUp,     ( "CastleBossUp",     "Burden of Lamech - Fight Lamech prior to being wounded during the Rebellion", false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.CastleBiomeUp,    ( "CastleBiomeUp",    "Burden of Agartha's Royal Guard - Citadel Agartha becomes more dangerous",    false, 2, 1, 1,   0f,    ""                  ) },
            { BurdenType.BridgeBossUp,     ( "BridgeBossUp",     "Burden of the Beast - Fight the True Form of the Beasts on the Bridge",       false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.BridgeBiomeUp,    ( "BridgeBiomeUp",    "Burden of Mundi's Flagship - Axis Mundi becomes more dangerous",              false, 2, 1, 1,   0f,    ""                  ) },
            { BurdenType.ForestBossUp,     ( "ForestBossUp",     "Burden of Naamah - Fight Naamah before the famine",                           false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.ForestBiomeUp,    ( "ForestBiomeUp",    "Burden of Kerguelen's Sorrow - The Kerguelen Plateau becomes more dangerous", false, 2, 1, 1,   0f,    ""                  ) },
            { BurdenType.StudyBossUp,      ( "StudyBossUp",      "Burden of Enoch - Fight Enoch before he succumbs to the poison",              false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.StudyBiomeUp,     ( "StudyBiomeUp",     "Burden of the High Scholar - The Stygian Study becomes more dangerous",       false, 2, 1, 1,   0f,    ""                  ) },
            { BurdenType.TowerBossUp,      ( "TowerBossUp",      "Burden of Irad - Fight Irad after the metamorphosis is complete",             false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.TowerBiomeUp,     ( "TowerBiomeUp",     "Burden of Irad's Torment - The Sun Tower becomes more dangerous",             false, 2, 1, 1,   0f,    ""                  ) },
            { BurdenType.CaveBossUp,       ( "CaveBossUp",       "Burden of Tubal - Fight Tubal before the madness sinks in",                   false, 1, 1, 1,   0f,    ""                  ) },
            { BurdenType.CaveBiomeUp,      ( "CaveBiomeUp",      "Burden of Pishon's Uprising - Pishon Dry Lake becomes more dangerous",        false, 2, 1, 1,   0f,    ""                  ) },
            { BurdenType.FinalBossUp,      ( "FinalBossUp",      "Burden of Cain - Fight Cain before the guilt",                                false, 1, 1, 1,   0f,    ""                  ) },
        };
        private static readonly HashSet<BurdenType> biomeUpBurdens = new HashSet<BurdenType> { BurdenType.CastleBiomeUp, BurdenType.BridgeBiomeUp, BurdenType.ForestBiomeUp, BurdenType.StudyBiomeUp, BurdenType.TowerBiomeUp, BurdenType.CaveBiomeUp };

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
                if( BurdenInfo[burdenType].Scaling ) {
                    WobSettings.Add( new WobSettings.Num<int>( keys.Get( burdenType, "MaxLevel" ), "Max burden level for " + BurdenInfo[burdenType].Name, BurdenInfo[burdenType].MaxLevel, bounds: (1, 100) ) );
                }
                WobSettings.Add( new WobSettings.Num<int>( keys.Get( burdenType, "Weight" ), "Weight per level for " + BurdenInfo[burdenType].Name, BurdenInfo[burdenType].Weight, bounds: (1, 10) ) );
                if( BurdenInfo[burdenType].Scaling ) {
                    WobSettings.Add( new WobSettings.Num<int>( keys.Get( burdenType, "StatGain" ), BurdenInfo[burdenType].StatName + " per level for " + BurdenInfo[burdenType].Name, BurdenInfo[burdenType].StatGain, BurdenInfo[burdenType].StatScaler, bounds: (0, 1000000) ) );
                }
            }
            // Settings for resetting keys, etc.
            WobSettings.Add( new List<WobSettings.Entry> {
                new WobSettings.Boolean( "ResetState", "Reset_CastleDoor",     "When starting a new thread, require hitting the lamps to unlock Lamech's door",                          true ),
                new WobSettings.Boolean( "ResetState", "Reset_BridgeGate",     "When starting a new thread, require beating the Void Beasts to raise the gate",                          true ),
                new WobSettings.Boolean( "ResetState", "Reset_ForestKeys",     "When starting a new thread, require gathering Lily of the Valley to unlock Namaah's door",               true ),
                new WobSettings.Boolean( "ResetState", "Reset_CaveKeys",       "When starting a new thread, require collecting the Onyx and Pearl Keys to access the Pishon minibosses", true ),
                new WobSettings.Boolean( "ResetState", "Reset_NamelessKnight", "When starting a new thread, respawn the Nameless Knight in various places with repeated dialogue",       true ),
                new WobSettings.Boolean( "ResetState", "Reset_Teleporters",    "When starting a new thread, require paying to unlock teleporters in each biome",                         true ),
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
                // Check if the door is being locked after previously being unlocked
                if( forceOverride && insightState == InsightState.Undiscovered && __instance.GetInsightState( insightType ) > InsightState.Undiscovered && (
                        ( insightType == InsightType.CastleBoss_DoorOpened && !WobSettings.Get( "ResetState", "Reset_CastleDoor", true ) ) || 
                        ( insightType == InsightType.BridgeBoss_GateRaised && !WobSettings.Get( "ResetState", "Reset_BridgeGate", true ) ) || 
                        ( insightType == InsightType.ForestBoss_DoorOpened && !WobSettings.Get( "ResetState", "Reset_ForestKeys", true ) ) ) ) {
                    // Remove the force override flag, which means the original method won't reset the insight to a previous state
                    forceOverride = false;
                }
            }
        }

        // Patch the gatehouse room to respect the insight state to decide if the gate should be open, rather than just using the boss defeated flag
        [HarmonyPatch( typeof( BridgeBossEntranceRoomController ), "OnPlayerEnterRoom" )]
        internal static class BridgeBossEntranceRoomController_OnPlayerEnterRoom_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "BridgeBossEntranceRoomController.OnPlayerEnterRoom Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Call, name: "OnPlayerEnterRoom" ), // base.OnPlayerEnterRoom(sender, eventArgs);
                            /*  1 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                       ), // if (num)
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_Insert( 1, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => ShowGateAnim( false ) ) ),
                        } );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Call, name: "get_IsRoomComplete" ), // base.IsRoomComplete
                            /*  1 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                        ), // if (base.IsRoomComplete)
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_Insert( 1, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => ShowGateOpen( false ) ) ),
                        } );
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
                            bool isBiomeUp = biomeUpBurdens.Contains( burdenType );
                            if( isBiomeUp || BurdenInfo[burdenType].Scaling ) {
                                burdenData.MaxBurdenLevel = isBiomeUp ? 2 : WobSettings.Get( keys.Get( burdenType, "MaxLevel" ), burdenData.MaxBurdenLevel );
                            }
                            burdenData.InitialBurdenCost = WobSettings.Get( keys.Get( burdenType, "Weight" ), burdenData.InitialBurdenCost );
                            if( BurdenInfo[burdenType].Scaling ) {
                                burdenData.StatsGain = WobSettings.Get( keys.Get( burdenType, "StatGain" ), burdenData.StatsGain );
                            }
                        }
                    }
                    runOnce = true;
                }
            }
        }

        private static bool IsBiomeUpBurdenActive( BurdenType burdenType ) {
            if( biomeUpBurdens.Contains( burdenType ) ) {
                return BurdenManager.GetBurdenLevel( burdenType ) > 1;
            } else {
                WobPlugin.Log( "ERROR: An invalid burden type has been passed to a patched IsBurdenActive: " + burdenType, WobPlugin.ERROR );
                return BurdenManager.IsBurdenActive( burdenType );
            }
        }

        // Disable Citadel Agartha triple orbiter and Kerguelen Plateau ice nightmares
        [HarmonyPatch( typeof( HazardSpawnControllerBase ), nameof( HazardSpawnControllerBase.SetSpawnType ) )]
        internal static class HazardSpawnControllerBase_SetSpawnType_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "HazardSpawnControllerBase.SetSpawnType Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, BurdenType.CastleBiomeUp ), // BurdenType.CastleBiomeUp
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Call, name: "IsBurdenActive"     ), // BurdenManager.IsBurdenActive(BurdenType.CastleBiomeUp)
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                        ), // if (BurdenManager.IsBurdenActive(BurdenType.CastleBiomeUp))
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 1, SymbolExtensions.GetMethodInfo( () => IsBiomeUpBurdenActive( BurdenType.None ) ) ),
                        } );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, BurdenType.ForestBiomeUp ), // BurdenType.ForestBiomeUp
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Call, name: "IsBurdenActive"     ), // BurdenManager.IsBurdenActive(BurdenType.ForestBiomeUp)
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                        ), // if (BurdenManager.IsBurdenActive(BurdenType.ForestBiomeUp))
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 1, SymbolExtensions.GetMethodInfo( () => IsBiomeUpBurdenActive( BurdenType.None ) ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Disable Axis Mundi wave projectiles
        [HarmonyPatch()]
        internal static class BridgeBiomeUp_BiomeRule_RunRule_Patch {
            // Find the correct method - this is an implicitly defined method
            // BridgeBiomeUp_BiomeRule.RunRule returns an IEnumerator, and we need to patch the MoveNext method on that
            internal static MethodInfo TargetMethod() {
                // Find the nested class the method implicitly created
                System.Type type = AccessTools.FirstInner( typeof( BridgeBiomeUp_BiomeRule ), t => t.Name.Contains( "<RunRule>d__" ) );
                // Find the 'MoveNext' method on the nested class
                return AccessTools.FirstMethod( type, method => method.Name.Contains( "MoveNext" ) );
            }

            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "BridgeBiomeUp_BiomeRule.RunRule Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, BurdenType.BridgeBiomeUp ), // BurdenType.BridgeBiomeUp
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Call, name: "IsBurdenActive"     ), // BurdenManager.IsBurdenActive(BurdenType.BridgeBiomeUp)
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                        ), // if (BurdenManager.IsBurdenActive(BurdenType.BridgeBiomeUp))
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 1, SymbolExtensions.GetMethodInfo( () => IsBiomeUpBurdenActive( BurdenType.None ) ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Disable Stygian Study wave projectiles
        [HarmonyPatch()]
        internal static class StudyBiomeUp_BiomeRule_RunRule_Patch {
            // Find the correct method - this is an implicitly defined method
            // BridgeBiomeUp_BiomeRule.RunRule returns an IEnumerator, and we need to patch the MoveNext method on that
            internal static MethodInfo TargetMethod() {
                // Find the nested class the method implicitly created
                System.Type type = AccessTools.FirstInner( typeof( StudyBiomeUp_BiomeRule ), t => t.Name.Contains( "<RunRule>d__" ) );
                // Find the 'MoveNext' method on the nested class
                return AccessTools.FirstMethod( type, method => method.Name.Contains( "MoveNext" ) );
            }

            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "StudyBiomeUp_BiomeRule.RunRule Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, BurdenType.StudyBiomeUp ), // BurdenType.StudyBiomeUp
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Call, name: "IsBurdenActive"    ), // BurdenManager.IsBurdenActive(BurdenType.StudyBiomeUp)
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                       ), // if (BurdenManager.IsBurdenActive(BurdenType.StudyBiomeUp))
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 1, SymbolExtensions.GetMethodInfo( () => IsBiomeUpBurdenActive( BurdenType.None ) ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Disable Sun Tower wave projectiles
        [HarmonyPatch()]
        internal static class TowerBiomeUp_BiomeRule_RunRule_Patch {
            // Find the correct method - this is an implicitly defined method
            // BridgeBiomeUp_BiomeRule.RunRule returns an IEnumerator, and we need to patch the MoveNext method on that
            internal static MethodInfo TargetMethod() {
                // Find the nested class the method implicitly created
                System.Type type = AccessTools.FirstInner( typeof( TowerBiomeUp_BiomeRule ), t => t.Name.Contains( "<RunRule>d__" ) );
                // Find the 'MoveNext' method on the nested class
                return AccessTools.FirstMethod( type, method => method.Name.Contains( "MoveNext" ) );
            }

            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "TowerBiomeUp_BiomeRule.RunRule Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, BurdenType.TowerBiomeUp ), // BurdenType.TowerBiomeUp
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Call, name: "IsBurdenActive"    ), // BurdenManager.IsBurdenActive(BurdenType.TowerBiomeUp)
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                       ), // if (BurdenManager.IsBurdenActive(BurdenType.TowerBiomeUp))
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 1, SymbolExtensions.GetMethodInfo( () => IsBiomeUpBurdenActive( BurdenType.None ) ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Disable Pishon Dry Lake wave projectiles
        [HarmonyPatch()]
        internal static class CaveBiomeUp_BiomeRule_RunRule_Patch {
            // Find the correct method - this is an implicitly defined method
            // CaveBiomeUp_BiomeRule.RunRule returns an IEnumerator, and we need to patch the MoveNext method on that
            internal static MethodInfo TargetMethod() {
                // Find the nested class the method implicitly created
                System.Type type = AccessTools.FirstInner( typeof( CaveBiomeUp_BiomeRule ), t => t.Name.Contains( "<RunRule>d__" ) );
                // Find the 'MoveNext' method on the nested class
                return AccessTools.FirstMethod( type, method => method.Name.Contains( "MoveNext" ) );
            }

            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "CaveBiomeUp_BiomeRule.RunRule Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, BurdenType.CaveBiomeUp ), // BurdenType.CaveBiomeUp
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Call, name: "IsBurdenActive"   ), // BurdenManager.IsBurdenActive(BurdenType.CaveBiomeUp)
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                      ), // if (BurdenManager.IsBurdenActive(BurdenType.CaveBiomeUp))
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 1, SymbolExtensions.GetMethodInfo( () => IsBiomeUpBurdenActive( BurdenType.None ) ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }
    }
}