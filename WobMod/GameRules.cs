using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using MoreMountains.CorgiEngine;
using UnityEngine;
using Wob_Common;

namespace WobMod {
    internal partial class GameRules {

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTING DEFAULTS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTINGS READ/WRITE
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        internal static void RunSetup() {
            // Separate the settings into a new file
            WobMod.configFiles.Add( "GameRules", "GameRules" );
            // Create each of the settings in the file
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Boolean(          WobMod.configFiles.Get( "GameRules" ), "MiscRules",             "ResetJumpDash",       "Actions that reset double jumps also reset dashes, and vice versa",                                   false                                 ),
                new WobSettings.Enum<DeathReset>( WobMod.configFiles.Get( "GameRules" ), "MiscRules",             "DeathResetsBosses",   "Reset Estuaries and minibosses back to being alive every time you die in the castle",                 DeathReset.None                       ),
                new WobSettings.Num<int>(         WobMod.configFiles.Get( "GameRules" ), "HouseRules",            "MaxBound",            "Maximum percentage that the enemy health and enemy damage house rules will go up to",                 200,         bounds: (200,  1000000 ), limiter: x => { return (int)( System.Math.Floor( x / 5f ) * 5f ); } ),
                new WobSettings.Boolean(          WobMod.configFiles.Get( "GameRules" ), "HouseRules",            "MinBound",            "Reduce the minimum that the aim time slow, enemy health and enemy damage house rules will go up to",  false                                 ),
                new WobSettings.Boolean(          WobMod.configFiles.Get( "GameRules" ), "Display",               "ChromaticAbberation", "Show chromatic abberation effects (e.g. on Nostalgic trait)",                                         true                                  ),
                new WobSettings.Boolean(          WobMod.configFiles.Get( "GameRules" ), "Display",               "KerguelenFog",        "Show fog effects in the Kerguelen Plateau",                                                           true                                  ),
                //new WobSettings.Num<int>(         WobMod.configFiles.Get( "GameRules" ), "Display",               "PishonDarknessDim",   "Darkness percent when in Pishon Dry Lake without the Sun Lantern",                                    73,   0.01f, bounds: (0,    100     ) ),
                //new WobSettings.Num<int>(         WobMod.configFiles.Get( "GameRules" ), "Display",               "PishonDarknessLit",   "Darkness percent when in Pishon Dry Lake with the Sun Lantern",                                       45,   0.01f, bounds: (0,    100     ) ),
                new WobSettings.Boolean(          WobMod.configFiles.Get( "GameRules" ), "Display",               "CheerOnClear",        "Always use the crowd cheering effect from the Diva trait when clearing a room",                       false                                 ),
                //new WobSettings.Boolean(          WobMod.configFiles.Get( "GameRules" ), "Display",               "SpellsKnown",         "Show effects of all unseen spells",                                                                   false                                 ),
                //new WobSettings.Boolean(          WobMod.configFiles.Get( "GameRules" ), "Display",               "RelicsKnown",         "Show effects of all unseen relics",                                                                   false                                 ),
                new WobSettings.Boolean(          WobMod.configFiles.Get( "GameRules" ), "Resources",             "ConvertOres",         "Convert all ore and red aether income and costs into an equal amount of gold",                        false                                 ),
                new WobSettings.Num<int>(         WobMod.configFiles.Get( "GameRules" ), "Resources",             "GoldGain",            "Gain +X% gold on all characters",                                                                     0,    0.01f, bounds: (0,    1000000 ) ),
                new WobSettings.Num<int>(         WobMod.configFiles.Get( "GameRules" ), "Resources",             "OreGain",             "Gain +X% ore",                                                                                        0,    0.01f, bounds: (0,    1000000 ) ),
                new WobSettings.Num<int>(         WobMod.configFiles.Get( "GameRules" ), "Resources",             "AetherGain",          "Gain +X% aether",                                                                                     0,    0.01f, bounds: (0,    1000000 ) ),
                new WobSettings.Num<float>(       WobMod.configFiles.Get( "GameRules" ), "Resources",             "GoldMultiply",        "Multiply gold gain by this after all other bonuses are added",                                        1f,          bounds: (0f,   1000000f) ),
                new WobSettings.Num<float>(       WobMod.configFiles.Get( "GameRules" ), "Resources",             "OreMultiply",         "Multiply ore gain by this after all other bonuses are added",                                         1f,          bounds: (0f,   1000000f) ),
                new WobSettings.Num<float>(       WobMod.configFiles.Get( "GameRules" ), "Resources",             "AetherMultiply",      "Multiply aether gain by this after all other bonuses are added",                                      1f,          bounds: (0f,   1000000f) ),
                new WobSettings.Num<float>(       WobMod.configFiles.Get( "GameRules" ), "BonusDamage",           "NormalBonus",         "Deal this % bonus damage to all normal enemies",                                                      0f,   0.01f, bounds: (-99f, 1000000f) ),
                new WobSettings.Num<float>(       WobMod.configFiles.Get( "GameRules" ), "BonusDamage",           "CommanderBonus",      "Deal this % bonus damage to all commanders",                                                          0f,   0.01f, bounds: (-99f, 1000000f) ),
                new WobSettings.Num<float>(       WobMod.configFiles.Get( "GameRules" ), "BonusDamage",           "MinibossBonus",       "Deal this % bonus damage to all minibosses",                                                          0f,   0.01f, bounds: (-99f, 1000000f) ),
                new WobSettings.Num<float>(       WobMod.configFiles.Get( "GameRules" ), "BonusDamage",           "BossBonus",           "Deal this % bonus damage to all bosses",                                                              0f,   0.01f, bounds: (-99f, 1000000f) ),
                new WobSettings.Num<float>(       WobMod.configFiles.Get( "GameRules" ), "BonusDamage",           "InsightBonus",        "Deal this % bonus damage to a boss for resolving their insight",                                      15f,  0.01f, bounds: (0f,   1000000f) ),
                new WobSettings.Num<float>(       WobMod.configFiles.Get( "GameRules" ), "BonusDamage",           "InsightPrime",        "Deal this % bonus damage to Traitor/Cain for resolving the Prime boss insights",                      5f,   0.01f, bounds: (0f,   1000000f) ),
                new WobSettings.Enum<RegenStat>(  WobMod.configFiles.Get( "GameRules" ), "Regen_HealthConstant",  "HealthRegenStat",     "Use this character stat to calculate health regeneration rate",                                       RegenStat.NoRegen                     ),
                new WobSettings.Num<float>(       WobMod.configFiles.Get( "GameRules" ), "Regen_HealthConstant",  "HealthRegenScale",    "Regenerate this percent of the health regen stat per second",                                         0f,   0.01f, bounds: (0f,   10000f  ) ),
                new WobSettings.Enum<RegenStat>(  WobMod.configFiles.Get( "GameRules" ), "Regen_ManaConstant",    "ManaRegenStat",       "Use this character stat to calculate mana regeneration rate",                                         RegenStat.NoRegen                     ),
                new WobSettings.Num<float>(       WobMod.configFiles.Get( "GameRules" ), "Regen_ManaConstant",    "ManaRegenScale",      "Regenerate this percent of the mana regen stat per second",                                           0f,   0.01f, bounds: (0f,   10000f  ) ),
                new WobSettings.Boolean(          WobMod.configFiles.Get( "GameRules" ), "Regen_ManaConstant",    "ManaRegenDelay",      "Enable the 2 second delay to mana regen after casting a spell",                                       true                                  ),
                new WobSettings.Num<float>(       WobMod.configFiles.Get( "GameRules" ), "Regen_HealthOnHit",     "StealRate",           "Gain health equal to this percent of damage dealt to enemies",                                        0f,   0.01f, bounds: (0f,   10000f  ) ),
                new WobSettings.Boolean(          WobMod.configFiles.Get( "GameRules" ), "Regen_HealthOnHit",     "ApplyBurden",         "Apply Burden of Drain reduction to this lifesteal",                                                   true                                  ),
                new WobSettings.Num<float>(       WobMod.configFiles.Get( "GameRules" ), "Regen_ManaOnHit",       "StealRate",           "Gain mana equal to this percent of damage dealt to enemies",                                          0f,   0.01f, bounds: (0f,   10000f  ) ),
                new WobSettings.Boolean(          WobMod.configFiles.Get( "GameRules" ), "Regen_ManaOnHit",       "ApplyBurden",         "Apply Burden of Drain reduction to this manasteal",                                                   false                                 ),
            } );
            // Cache the damage bonus settings into a dictionary based on the EnemyRankBonus enum
            rankBonus.Add( EnemyRankBonus.Normal,    WobSettings.Get( "BonusDamage", "NormalBonus",    0f ) );
            rankBonus.Add( EnemyRankBonus.Commander, WobSettings.Get( "BonusDamage", "CommanderBonus", 0f ) );
            rankBonus.Add( EnemyRankBonus.Miniboss,  WobSettings.Get( "BonusDamage", "MinibossBonus",  0f ) );
            rankBonus.Add( EnemyRankBonus.Boss,      WobSettings.Get( "BonusDamage", "BossBonus",      0f ) );
            // Read settings for constant regen
            ManaRegen_Update_Patch.healthRegenStat  = WobSettings.Get( "Regen_HealthConstant", "HealthRegenStat",  RegenStat.NoRegen );
            ManaRegen_Update_Patch.healthRegenScale = WobSettings.Get( "Regen_HealthConstant", "HealthRegenScale", 0f                );
            ManaRegen_Update_Patch.manaRegenStat    = WobSettings.Get( "Regen_ManaConstant",   "ManaRegenStat",    RegenStat.NoRegen );
            ManaRegen_Update_Patch.manaRegenScale   = WobSettings.Get( "Regen_ManaConstant",   "ManaRegenScale",   0f                );
            ManaRegen_Update_Patch.manaRegenDelay   = WobSettings.Get( "Regen_ManaConstant",   "ManaRegenDelay",   true              );
            // Read settings for on hit enemy regen
            EnemyHitResponse_CharacterDamaged_Patch.healthStealRate   = WobSettings.Get( "Regen_HealthOnHit", "StealRate",   0f    );
            EnemyHitResponse_CharacterDamaged_Patch.healthApplyBurden = WobSettings.Get( "Regen_HealthOnHit", "ApplyBurden", true  );
            EnemyHitResponse_CharacterDamaged_Patch.healthEnabled     = EnemyHitResponse_CharacterDamaged_Patch.healthStealRate > 0f;
            EnemyHitResponse_CharacterDamaged_Patch.manaStealRate     = WobSettings.Get( "Regen_ManaOnHit",   "StealRate",   0f    );
            EnemyHitResponse_CharacterDamaged_Patch.manaApplyBurden   = WobSettings.Get( "Regen_ManaOnHit",   "ApplyBurden", true  );
            EnemyHitResponse_CharacterDamaged_Patch.manaEnabled       = EnemyHitResponse_CharacterDamaged_Patch.manaStealRate   > 0f;
            // Read settings for resource drops
            Economy_EV_GetItemDropValue_Patch.convertOres    = WobSettings.Get( "Resources", "ConvertOres", false );
            Economy_EV_GetItemDropValue_Patch.goldMultiply   = WobSettings.Get( "Resources", "GoldMultiply",   1f );
            Economy_EV_GetItemDropValue_Patch.oreMultiply    = WobSettings.Get( "Resources", "OreMultiply",    1f );
            Economy_EV_GetItemDropValue_Patch.aetherMultiply = WobSettings.Get( "Resources", "AetherMultiply", 1f );
            // Initialise the cheer effect manager
            Messenger<GameMessenger, GameEvent>.AddListener( GameEvent.WorldCreationComplete, ( sender, args ) => CheerOnClear.GetInstance( true ) );
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - RESOURCE GAIN
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        [HarmonyPatch( typeof( SkillTreeLogicHelper ), nameof( SkillTreeLogicHelper.GetGoldGainMod ) )]
        internal static class SkillTreeLogicHelper_GetGoldGainMod_Patch {
            internal static void Postfix( ref float __result ) {
                __result += WobSettings.Get( "Resources", "GoldGain", 0f );
            }
        }

        [HarmonyPatch( typeof( SkillTreeLogicHelper ), nameof( SkillTreeLogicHelper.GetEquipmentOreMod ) )]
        internal static class SkillTreeLogicHelper_GetEquipmentOreMod_Patch {
            internal static void Postfix( ref float __result ) {
                __result += WobSettings.Get( "Resources", "OreGain", 0f );
            }
        }

        [HarmonyPatch( typeof( SkillTreeLogicHelper ), nameof( SkillTreeLogicHelper.GetRuneOreMod ) )]
        internal static class SkillTreeLogicHelper_GetRuneOreMod_Patch {
            internal static void Postfix( ref float __result ) {
                __result += WobSettings.Get( "Resources", "AetherGain", 0f );
            }
        }

        // Apply gold gain modifiers to ore drops that are converted to gold
        [HarmonyPatch( typeof( Economy_EV ), nameof( Economy_EV.GetItemDropValue ) )]
        internal static class Economy_EV_GetItemDropValue_Patch {
            internal static float goldMultiply, oreMultiply, aetherMultiply;
            internal static bool convertOres;
            internal static void Postfix( ItemDropType itemDrop, ref int __result ) {
                float multiplier = 1f;
                if( itemDrop <= ItemDropType.Coin ) {
                    multiplier *= goldMultiply;
                }
                if( itemDrop == ItemDropType.EquipmentOre ) {
                    multiplier *= oreMultiply;
                    if( convertOres ) {
                        multiplier *= ( 1f + Economy_EV.GetGoldGainMod() ) * NPC_EV.GetArchitectGoldMod( -1 );
                    }
                }
                if( itemDrop == ItemDropType.RuneOre ) {
                    multiplier *= aetherMultiply;
                    if( convertOres ) {
                        multiplier *= ( 1f + Economy_EV.GetGoldGainMod() ) * NPC_EV.GetArchitectGoldMod( -1 );
                    }
                }
                //WobPlugin.Log( "[GameRules] Drop of " + itemDrop + ", base amount " + __result + ", applying multiplier " + multiplier );
                __result = Mathf.Clamp( Mathf.RoundToInt( __result * multiplier ), 0, int.MaxValue );
            }
        }

        [HarmonyPatch( typeof( EquipmentOreDrop ), "Collect" )]
        internal static class EquipmentOreDrop_Collect_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "EquipmentOreDrop.Collect" );
                if( WobSettings.Get( "Resources", "ConvertOres", false ) ) {
                    // Perform the patching
                    transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new( OpCodes.Ldfld, name: "EquipmentOreCollected" ), // SaveManager.PlayerSaveData.EquipmentOreCollected
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 0, typeof( PlayerSaveData ).GetField( "GoldCollected" ) ), // SaveManager.PlayerSaveData.GoldCollected
                        }, expected: 1 );
                    transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new( OpCodes.Stfld, name: "EquipmentOreCollected" ), // SaveManager.PlayerSaveData.EquipmentOreCollected
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 0, typeof( PlayerSaveData ).GetField( "GoldCollected" ) ), // SaveManager.PlayerSaveData.GoldCollected
                        }, expected: 1 );
                    transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new( OpCodeSet.Ldc_I4, (int)TextPopupType.EquipmentOreCollected ), // TextPopupType.EquipmentOreCollected
                            /*  1 */ new( OpCodeSet.Ldloc                                            ), // text
                            /*  2 */ new( OpCodeSet.Ldloc                                            ), // absPos
                            /*  3 */ new( OpCodes.Ldnull                                             ), // null
                            /*  4 */ new( OpCodeSet.Ldc_I4                                           ), // TextAlignmentOptions.Center
                            /*  5 */ new( OpCodes.Call, name: "DisplayTextAtAbsPos"                  ), // TextPopupManager.DisplayTextAtAbsPos(TextPopupType.EquipmentOreCollected, text, absPos, null, TextAlignmentOptions.Center)
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetInstruction( 0, OpCodes.Ldc_I4, (int)TextPopupType.GoldCollected ), // TextPopupManager.DisplayTextAtAbsPos(TextPopupType.GoldCollected, text, absPos, null, TextAlignmentOptions.Center)
                        }, expected: 1 );
                    transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new( OpCodeSet.Ldc_I4, (int)GameEvent.EquipmentOreChanged ), // GameEvent.EquipmentOreChanged
                            /*  1 */ new( OpCodes.Ldarg_0                                      ), // this
                            /*  2 */ new( OpCodes.Ldnull                                       ), // null
                            /*  3 */ new( OpCodes.Call, name: "Broadcast"                      ), // Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.EquipmentOreChanged, this, null)
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetInstruction( 0, OpCodes.Ldc_I4, (int)GameEvent.GoldChanged ), // Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldChanged, this, null)
                        }, expected: 1 );
                }
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        [HarmonyPatch( typeof( RuneOreDrop ), "Collect" )]
        internal static class RuneOreDrop_Collect_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "RuneOreDrop.Collect" );
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
                            /*  0 */ new( OpCodes.Stfld, name: "RuneOreCollected" ), // SaveManager.PlayerSaveData.EquipmentOreCollected
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 0, typeof( PlayerSaveData ).GetField( "GoldCollected" ) ), // SaveManager.PlayerSaveData.GoldCollected
                        }, expected: 1 );
                    transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new( OpCodeSet.Ldc_I4, (int)TextPopupType.RuneOreCollected ), // TextPopupType.RuneOreCollected
                            /*  1 */ new( OpCodeSet.Ldloc                                       ), // text
                            /*  2 */ new( OpCodeSet.Ldloc                                       ), // absPos
                            /*  3 */ new( OpCodes.Ldnull                                        ), // null
                            /*  4 */ new( OpCodeSet.Ldc_I4                                      ), // TextAlignmentOptions.Center
                            /*  5 */ new( OpCodes.Call, name: "DisplayTextAtAbsPos"             ), // TextPopupManager.DisplayTextAtAbsPos(TextPopupType.RuneOreCollected, text, absPos, null, TextAlignmentOptions.Center)
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetInstruction( 0, OpCodes.Ldc_I4, (int)TextPopupType.GoldCollected ), // TextPopupManager.DisplayTextAtAbsPos(TextPopupType.GoldCollected, text, absPos, null, TextAlignmentOptions.Center)
                        }, expected: 1 );
                    transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new( OpCodeSet.Ldc_I4, (int)GameEvent.RuneOreChanged ), // GameEvent.RuneOreChanged
                            /*  1 */ new( OpCodes.Ldarg_0                                 ), // this
                            /*  2 */ new( OpCodes.Ldnull                                  ), // null
                            /*  3 */ new( OpCodes.Call, name: "Broadcast"                 ), // Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.RuneOreChanged, this, null)
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetInstruction( 0, OpCodes.Ldc_I4, (int)GameEvent.GoldChanged ), // Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldChanged, this, null)
                        }, expected: 1 );
                }
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Convert held ores to gold
        [HarmonyPatch( typeof( SkillTreeWindowController ), nameof( SkillTreeWindowController.Initialize ) )]
        internal static class SkillTreeWindowController_Initialize_Patch {
            internal static void Postfix( SkillTreeWindowController __instance ) {
                if( WobSettings.Get( "Resources", "ConvertOres", false ) ) {
                    if( SaveManager.PlayerSaveData.EquipmentOreCollected > 0 ) {
                        SaveManager.PlayerSaveData.GoldCollected += SaveManager.PlayerSaveData.EquipmentOreCollected;
                        SaveManager.PlayerSaveData.EquipmentOreCollected = 0;
                        Messenger<GameMessenger, GameEvent>.Broadcast( GameEvent.GoldChanged, __instance, null );
                        Messenger<GameMessenger, GameEvent>.Broadcast( GameEvent.EquipmentOreChanged, __instance, null );
                    }
                    if( SaveManager.PlayerSaveData.RuneOreCollected > 0 ) {
                        SaveManager.PlayerSaveData.GoldCollected += SaveManager.PlayerSaveData.RuneOreCollected;
                        SaveManager.PlayerSaveData.RuneOreCollected = 0;
                        Messenger<GameMessenger, GameEvent>.Broadcast( GameEvent.GoldChanged, __instance, null );
                        Messenger<GameMessenger, GameEvent>.Broadcast( GameEvent.RuneOreChanged, __instance, null );
                    }
                }
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - BONUS DAMAGE
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Damage bonus enemy categories
        internal enum EnemyRankBonus { Normal, Commander, Miniboss, Boss }
        // Cache of settings for quick lookup against the EnemyRank enum
        private static readonly Dictionary<EnemyRankBonus,float> rankBonus = new();

        // Apply damage dealt modifiers on boss fights
        [HarmonyPatch( typeof( EnemyController ), "GetInsightPlayerDamageMod" )]
        internal static class EnemyController_GetInsightPlayerDamageMod_Patch {
            // Modify the insight damage bonuses
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "EnemyController.GetInsightPlayerDamageMod" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldc_R4, Insight_EV.INSIGHT_PLAYER_DAMAGE_MOD ), // 1.15f
                        /*  1 */ new( OpCodes.Ret                                          ), // return 1.15f
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 0, ( WobSettings.Get( "BonusDamage", "InsightBonus", 0.15f ) + 1f ) ),
                    }, expected: 6 );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodeSet.Ldloc                                                ), // num
                        /*  1 */ new( OpCodes.Ldc_R4, Insight_EV.INSIGHT_FINALBOSS_PLAYER_DAMAGE_MOD ), // 0.05f
                        /*  2 */ new( OpCodes.Add                                                    ), // num + 0.05f
                        /*  3 */ new( OpCodeSet.Stloc                                                ), // num += 0.05f
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( "BonusDamage", "InsightPrime", 0.05f ) ),
                    }, expected: 6 );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            // Add the additional bonus for all
            internal static void Postfix( EnemyController __instance, ref float __result ) {
                if( __instance.IsBoss ) {
                    __result += rankBonus[EnemyRankBonus.Boss];
                } else if( __instance.IsCommander ) {
                    __result += rankBonus[EnemyRankBonus.Commander];
                } else if( __instance.EnemyRank == EnemyRank.Miniboss ) {
                    __result += rankBonus[EnemyRankBonus.Miniboss];
                } else {
                    __result += rankBonus[EnemyRankBonus.Normal];
                }
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - DISPLAY
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        [HarmonyPatch( typeof( PlayerSaveData ), nameof( PlayerSaveData.GetSpellSeenState ) )]
        internal static class PlayerSaveData_GetSpellSeenState_Patch {
            internal static void Postfix( PlayerSaveData __instance, AbilityType spellType, ref bool __result ) {
                if( !__result && WobSettings.Get( "Display", "SpellsKnown", false ) ) {
                    __instance.SetSpellSeenState( spellType, true );
                    __result = true;
                }
            }
        }

        [HarmonyPatch( typeof( RelicObj ), nameof( RelicObj.WasSeen ), MethodType.Getter )]
        internal static class RelicObj_WasSeen_Patch {
            internal static void Postfix( RelicObj __instance, ref bool __result ) {
                if( !__result && WobSettings.Get( "MiscSettings", "RelicsKnown", false ) ) {
                    Traverse.Create( __instance ).Field( "m_wasSeen" ).SetValue( true );
                    __result = true;
                }
            }
        }

        // Patch for the method that applies the chromatic aberation effect
        [HarmonyPatch( typeof( MobilePostProcessing ), "ApplyProfileChromaticAbberation" )]
        internal static class MobilePostProcessing_ApplyProfileChromaticAbberation_Patch {
            internal static void Prefix( MobilePostProcessing __instance, MobilePostProcessingProfile profile ) {
                // Check if the profile has chromatic aberations enabled and there is a setting disabling them
                if( profile.EnableChromaticAbberationEffect && !WobSettings.Get( "Display", "ChromaticAbberation", false ) ) {
                    // Disable the effect and the distortion with it - these values are taken from the ResetChromaticAbberation method
                    profile.EnableChromaticAbberationEffect = false;
                    profile.Offset = 1f;
                    profile.FishEyeDistortion = 0f;
                    // Call the reset method to make certain that the effect is turned off now
                    Traverse.Create( __instance ).Method( "ResetChromaticAbberation" ).GetValue();
                }
            }
        }

        // Patch for the method that applies the fog effect in the Kerguelen Plateau
        [HarmonyPatch( typeof( MobilePostProcessing ), "ApplyProfileMist" )]
        internal static class MobilePostProcessing_ApplyProfileMist_Patch {
            internal static void Prefix( MobilePostProcessing __instance, MobilePostProcessingProfile profile ) {
                // Check if the profile has fog enabled and there is a setting disabling them
                if( profile.EnableMistEffect && !WobSettings.Get( "Display", "KerguelenFog", false ) ) {
                    // Disable the effect
                    profile.EnableMistEffect = false;
                    // Call the reset method to make certain that the effect is turned off now
                    Traverse.Create( __instance ).Method( "ResetMist" ).GetValue();
                }
            }
        }

        //// Patch for a method that applies the darkening effect in the Pishon Dry Lake
        //[HarmonyPatch( typeof( CaveLanternPostProcessingController ), "DarknessAmountWhenFullyLit", MethodType.Getter )]
        //internal static class CaveLanternPostProcessingController_DarknessAmountWhenFullyLit_Patch {
        //    internal static void Postfix( ref float __result ) {
        //        float baseDarkness = WobSettings.Get( "Display", "PishonDarknessLit", Heirloom_EV.CAVE_LANTERN_DARKNESS_AMOUNT_LIT );
        //        float burdenModifier = Mathf.Min( SaveManager.PlayerSaveData.GetBurden(BurdenType.EnemyProjectiles).CurrentLevel * Heirloom_EV.CAVE_LANTERN_BURDEN_VIEW_MOD, Heirloom_EV.CAVE_LANTERN_BURDEN_VIEW_CAP );
        //        __result = baseDarkness / ( 1f + burdenModifier );
        //    }
        //}

        //// Patch for a method that applies the darkening effect in the Pishon Dry Lake
        //[HarmonyPatch( typeof( CaveLanternPostProcessingController ), "OnEnable" )]
        //internal static class CaveLanternPostProcessingController_OnEnable_Patch {
        //    internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
        //        // Set up the transpiler handler with the instruction list
        //        WobTranspiler transpiler = new( instructions, "CaveLanternPostProcessingController.OnEnable" );
        //        // Perform the patching
        //        transpiler.PatchAll(
        //            // Define the IL code instructions that should be matched
        //            new List<WobTranspiler.OpTest> {
        //                /*  0 */ new( OpCodes.Ldarg_0                          ), // this
        //                /*  1 */ new( OpCodes.Ldfld, name:"m_profile"          ), // this.m_profile
        //                /*  2 */ new( OpCodes.Ldc_R4                           ), // 0.73f
        //                /*  3 */ new( OpCodes.Stfld, name:"CircDarknessAmount" ), // this.m_profile.CircDarknessAmount = 0.73f * percent;
        //            },
        //            // Define the actions to take when an occurrence is found
        //            new List<WobTranspiler.OpAction> {
        //                new WobTranspiler.OpAction_SetOperand( 2, WobSettings.Get( "Display", "PishonDarknessDim", Heirloom_EV.CAVE_LANTERN_DARKNESS_AMOUNT_DIM ) )
        //            }, expected: 1 );
        //        // Return the modified instructions
        //        return transpiler.GetResult();
        //    }
        //}

        //// Patch for a method that applies the darkening effect in the Pishon Dry Lake
        //[HarmonyPatch( typeof( CaveLanternPostProcessingController ), "SetDimnessPercent" )]
        //internal static class CaveLanternPostProcessingController_SetDimnessPercent_Patch {
        //    internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
        //        // Set up the transpiler handler with the instruction list
        //        WobTranspiler transpiler = new( instructions, "CaveLanternPostProcessingController.SetDimnessPercent" );
        //        // Perform the patching
        //        transpiler.PatchAll(
        //            // Define the IL code instructions that should be matched
        //            new List<WobTranspiler.OpTest> {
        //                /*  0 */ new( OpCodes.Call, name:"get_Instance"        ), // Instance
        //                /*  1 */ new( OpCodes.Ldfld, name:"m_profile"          ), // Instance.m_profile
        //                /*  2 */ new( OpCodes.Ldc_R4                           ), // 0.73f
        //                /*  3 */ new( OpCodes.Ldarg_0                          ), // percent
        //                /*  4 */ new( OpCodes.Mul                              ), // 0.73f * percent
        //                /*  5 */ new( OpCodes.Stfld, name:"CircDarknessAmount" ), // Instance.m_profile.CircDarknessAmount = 0.73f * percent;
        //            },
        //            // Define the actions to take when an occurrence is found
        //            new List<WobTranspiler.OpAction> {
        //                new WobTranspiler.OpAction_SetOperand( 2, WobSettings.Get( "Display", "PishonDarknessDim", Heirloom_EV.CAVE_LANTERN_DARKNESS_AMOUNT_DIM ) )
        //            }, expected: 1 );
        //        // Return the modified instructions
        //        return transpiler.GetResult();
        //    }
        //}

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - HEALTH & MANA REGEN
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Patch the method that applies damage to enemies to add vampiric HP regen
        [HarmonyPatch( typeof( EnemyHitResponse ), "CharacterDamaged" )]
        internal static class EnemyHitResponse_CharacterDamaged_Patch {
            // Health regen settings
            internal static float healthStealRate;
            internal static bool healthApplyBurden;
            internal static bool healthEnabled;
            // Mana regen settings
            internal static float manaStealRate;
            internal static bool manaApplyBurden;
            internal static bool manaEnabled;

            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "EnemyHitResponse.CharacterDamaged" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodeSet.Ldarg                         ), // otherRootObj
                        /*  1 */ new( OpCodes.Ldstr, "Hazard"                 ), // "Hazard"
                        /*  2 */ new( OpCodes.Callvirt, name:"CompareTag"     ), // otherRootObj.CompareTag("Hazard")
                        /*  3 */ new( OpCodeSet.Brtrue                        ), // if (!otherRootObj.CompareTag("Hazard"))
                        /*  4 */ new( OpCodeSet.Ldarg                         ), // damageObj
                        /*  5 */ new( OpCodes.Callvirt, name:"get_gameObject" ), // damageObj.gameObject
                        /*  6 */ new( OpCodes.Callvirt, name:"GetComponent"   ), // damageObj.gameObject.GetComponent<Projectile_RL>()
                        /*  7 */ new( OpCodeSet.Stloc                         ), // Projectile_RL component = damageObj.gameObject.GetComponent<Projectile_RL>()
                        /*  8 */ new( OpCodeSet.Ldloc                         ), // component
                        /*  9 */ new( OpCodes.Call                            ), // (bool)component
                        /* 10 */ new( OpCodeSet.Brfalse                       ), // if ((bool)component)
                        /* 11 */ new( OpCodeSet.Ldloc                         ), // component
                        /* 12 */ new( OpCodes.Ldstr, "PlayerProjectile"       ), // "PlayerProjectile"
                        /* 13 */ new( OpCodes.Callvirt, name:"CompareTag"     ), // component.CompareTag("PlayerProjectile")
                        /* 14 */ new( OpCodeSet.Brfalse                       ), // if (component.CompareTag("PlayerProjectile"))
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_Insert( 4, new List<CodeInstruction> {
                            new( OpCodes.Ldarg_0 ),                                                              // this
                            new( OpCodes.Ldloc_1 ),                                                              // num   // damage dealt to enemy
                            new( OpCodes.Call, SymbolExtensions.GetMethodInfo( () => ApplyRegen( null, 0f ) ) ), // ApplyRegen(this, num)
                        } ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static void ApplyRegen( EnemyHitResponse enemyHitResponse, float damage ) {
                // Check if applying HP regen, and if the player has trait Hero Complex that prevents healing
                if( healthEnabled && !TraitManager.IsTraitActive( TraitType.MegaHealth ) ) {
                    // Calculate the amount to regen from damage dealt, steal rate, and apply burden reduction
                    int regenAmount = Mathf.CeilToInt( damage * healthStealRate * ( healthApplyBurden ? ( 1f - BurdenManager.GetBurdenStatGain( BurdenType.LifestealReduc ) ) : 1f ) );
                    // Check that at least 1 HP is being added
                    if( regenAmount > 0 ) {
                        // Get the player
                        PlayerController playerController = PlayerManager.GetPlayerController();
                        // Add the regen amount so it shows as text above the player
                        playerController.UpdateFrameAccumulatedLifeSteal( regenAmount );
                        // Add the regen amount to the player's health
                        playerController.SetHealth( regenAmount, true, true );
                        // Get the enemy that was hit
                        EnemyController m_enemyController = Traverse.Create( enemyHitResponse ).Field( "m_enemyController" ).GetValue<EnemyController>();
                        // Show the lifesteal visual effect
                        EffectManager.PlayEffect( m_enemyController.gameObject, m_enemyController.Animator, "LifestealBurst_Effect", Vector3.zero, 0f, EffectStopType.Gracefully, EffectTriggerDirection.None );
                    }
                }
                // Check if applying MP regen, and if the player has trait Masochism that prevents mana regen from hitting enemies
                if( manaEnabled && !TraitManager.IsTraitActive( TraitType.ManaFromHurt ) ) {
                    // Calculate the amount to regen from damage dealt, steal rate, and apply burden reduction
                    int regenAmount = Mathf.CeilToInt( damage * manaStealRate * ( manaApplyBurden ? ( 1f - BurdenManager.GetBurdenStatGain( BurdenType.LifestealReduc ) ) : 1f ) );
                    // Check that at least 1 MP is being added
                    if( regenAmount > 0f ) {
                        // Get the player
                        PlayerController playerController = PlayerManager.GetPlayerController();
                        // Add the regen amount to the player's mana
                        playerController.SetMana( regenAmount, true, true );
                        // Get the enemy that was hit
                        EnemyController m_enemyController = Traverse.Create( enemyHitResponse ).Field( "m_enemyController" ).GetValue<EnemyController>();
                        // Show the manasteal visual effect
                        EffectManager.PlayEffect( m_enemyController.gameObject, m_enemyController.Animator, "ManaRegenBurst_Effect", Vector3.zero, 0f, EffectStopType.Gracefully, EffectTriggerDirection.None );
                    }
                }
            }
        }

        // The character attributes that constant regeneration can be based on
        internal enum RegenStat { NoRegen, MaxHealth, MaxMana, Vitality, Strength, Dexterity, Intelligence, Focus, Constant100 }

        // Patch to the method that controls mana regen
        [HarmonyPatch( typeof( ManaRegen ), "Update" )]
        internal static class ManaRegen_Update_Patch {
            // Health regen settings
            internal static RegenStat healthRegenStat;
            internal static float healthRegenScale;
            // Health regen fractional running total
            private static float healthRegenTotal = 0f;
            // Mana regen settings
            internal static RegenStat manaRegenStat;
            internal static float manaRegenScale;
            internal static bool manaRegenDelay;
            // Mana regen fractional running total
            private static float manaRegenTotal = 0f;

            internal static void Prefix( ManaRegen __instance ) {
                // Get a reference to the private field where current player info is stored
                PlayerController m_playerController = Traverse.Create( __instance ).Field( "m_playerController" ).GetValue<PlayerController>();
                // Continue if we are enabling health regen
                if( healthRegenScale > 0f ) {
                    // Get the current health state
                    float actualMaxHealth = m_playerController.ActualMaxHealth;
                    float currentHealth = m_playerController.CurrentHealth;
                    // Check that health is missing
                    if( currentHealth < actualMaxHealth ) {
                        // Add current frame's regen to the running total
                        healthRegenTotal += GetRegenStatValue( m_playerController, healthRegenStat ) * healthRegenScale * Time.deltaTime;
                        // After the total is over 1 do the actual regeneration
                        if( healthRegenTotal > 1f ) {
                            // The regen to add is the integer portion of the total
                            float regenNow = Mathf.FloorToInt( healthRegenTotal );
                            // Subtract the regen from the running total so it won't be added twice
                            healthRegenTotal -= regenNow;
                            // Check if the regen will take the current health over the maximum
                            if( regenNow + currentHealth >= actualMaxHealth ) {
                                // Set health to maximum
                                m_playerController.SetHealth( actualMaxHealth, false, true );
                            } else {
                                // Add regen amount to current health
                                m_playerController.SetHealth( regenNow, true, true );
                            }
                        }
                    }
                }
                // Continue if we are enabling mana regen
                if( manaRegenScale > 0f ) {
                    // Get the current mana state
                    float actualMaxMana = m_playerController.ActualMaxMana;
                    float currentMana = m_playerController.CurrentMana;
                    // Check that mana is missing
                    if( currentMana < actualMaxMana && !( manaRegenDelay && __instance.IsManaRegenDelayed ) ) {
                        // Add current frame's regen to the running total
                        manaRegenTotal += GetRegenStatValue( m_playerController, manaRegenStat ) * manaRegenScale * Time.deltaTime;
                        // After the total is over 1 do the actual regeneration
                        if( manaRegenTotal > 1f ) {
                            // The regen to add is the integer portion of the total
                            float regenNow = Mathf.FloorToInt( manaRegenTotal );
                            // Subtract the regen from the running total so it won't be added twice
                            manaRegenTotal -= regenNow;
                            // Check if the regen will take the current mana over the maximum
                            if( regenNow + currentMana >= actualMaxMana ) {
                                // Set mana to maximum
                                m_playerController.SetMana( actualMaxMana, false, true );
                            } else {
                                // Add regen amount to current mana
                                m_playerController.SetMana( regenNow, true, true );
                            }
                        }
                    }
                }
            }

            private static float GetRegenStatValue( PlayerController player, RegenStat regenStat ) {
                return regenStat switch {
                    RegenStat.MaxHealth    => player.ActualMaxHealth,
                    RegenStat.MaxMana      => player.ActualMaxMana,
                    RegenStat.Vitality     => player.ActualVitality,
                    RegenStat.Strength     => player.ActualStrength,
                    RegenStat.Dexterity    => player.ActualDexterity,
                    RegenStat.Intelligence => player.ActualMagic,
                    RegenStat.Focus        => player.ActualFocus,
                    RegenStat.Constant100  => 100f,
                    _                      => 0f,
                };
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - HOUSE RULE LIMITS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Patch for the method that sets the limits on House Rules options
        [HarmonyPatch( typeof( ChangeAssistStatModOptionItem ), nameof( ChangeAssistStatModOptionItem.Initialize ) )]
        internal static class ChangeAssistStatModOptionItem_Initialize_Patch {
            // Change the minimum values of the house rule sliders
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "ChangeAssistStatModOptionItem.Initialize" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        // case ChangeAssistStatModOptionItem.StatType.EnemyHealth:
                        /*  0 */ new( OpCodes.Ldarg_0                                                 ), // this
                        /*  1 */ new( OpCodeSet.Ldc_I4, AssistMode_EV.ENEMY_HEALTH_MIN_AMOUNT         ), // 25
                        /*  2 */ new( OpCodes.Stfld, name: "m_minValue"                               ), // this.m_minValue = 25
                        /*  3 */ new( OpCodes.Ldarg_0                                                 ), // this
                        /*  4 */ new( OpCodeSet.Ldc_I4, AssistMode_EV.ENEMY_HEALTH_MAX_AMOUNT         ), // 200
                        /*  5 */ new( OpCodes.Stfld, name: "m_maxValue"                               ), // this.m_maxValue = 200
                        /*  6 */ new( OpCodes.Ldarg_0                                                 ), // this
                        /*  7 */ new( OpCodeSet.Ldc_I4, AssistMode_EV.ENEMY_HEALTH_SCALE_AMOUNT       ), // 5
                        /*  8 */ new( OpCodes.Stfld, name: "m_incrementValue"                         ), // this.m_incrementValue = 5
                        /*  9 */ new( OpCodes.Br                                                      ), // break
                        // case ChangeAssistStatModOptionItem.StatType.EnemyDamage:
                        /* 10 */ new( OpCodes.Ldarg_0                                                 ), // this
                        /* 11 */ new( OpCodeSet.Ldc_I4, AssistMode_EV.ENEMY_DAMAGE_MIN_AMOUNT         ), // 25
                        /* 12 */ new( OpCodes.Stfld, name: "m_minValue"                               ), // this.m_minValue = 25
                        /* 13 */ new( OpCodes.Ldarg_0                                                 ), // this
                        /* 14 */ new( OpCodeSet.Ldc_I4, AssistMode_EV.ENEMY_DAMAGE_MAX_AMOUNT         ), // 200
                        /* 15 */ new( OpCodes.Stfld, name: "m_maxValue"                               ), // this.m_maxValue = 200
                        /* 16 */ new( OpCodes.Ldarg_0                                                 ), // this
                        /* 17 */ new( OpCodeSet.Ldc_I4, AssistMode_EV.ENEMY_DAMAGE_SCALE_AMOUNT       ), // 5
                        /* 18 */ new( OpCodes.Stfld, name: "m_incrementValue"                         ), // this.m_incrementValue = 5
                        /* 19 */ new( OpCodes.Br                                                      ), // break
                        // case ChangeAssistStatModOptionItem.StatType.AimTimeSlow:
                        /* 20 */ new( OpCodes.Ldarg_0                                                 ), // this
                        /* 21 */ new( OpCodeSet.Ldc_I4, AssistMode_EV.AIM_TIME_SLOW_MIN_AMOUNT        ), // 25
                        /* 22 */ new( OpCodes.Stfld, name: "m_minValue"                               ), // this.m_minValue = 25
                        /* 23 */ new( OpCodes.Ldarg_0                                                 ), // this
                        /* 24 */ new( OpCodeSet.Ldc_I4, AssistMode_EV.AIM_TIME_SLOW_MAX_AMOUNT        ), // 100
                        /* 25 */ new( OpCodes.Stfld, name: "m_maxValue"                               ), // this.m_maxValue = 100
                        /* 26 */ new( OpCodes.Ldarg_0                                                 ), // this
                        /* 27 */ new( OpCodeSet.Ldc_I4, AssistMode_EV.AIM_TIME_SLOW_SCALE_AMOUNT      ), // 5
                        /* 28 */ new( OpCodes.Stfld, name: "m_incrementValue"                         ), // this.m_incrementValue = 5
                        /* 29 */ new( OpCodes.Br                                                      ), // break
                        // case ChangeAssistStatModOptionItem.StatType.BurdenRequirement:
                        /* 30 */ new( OpCodes.Ldarg_0                                                 ), // this
                        /* 31 */ new( OpCodeSet.Ldc_I4, AssistMode_EV.BURDEN_REQUIREMENT_MIN_AMOUNT   ), // 0
                        /* 32 */ new( OpCodes.Stfld, name: "m_minValue"                               ), // this.m_minValue = 0
                        /* 33 */ new( OpCodes.Ldarg_0                                                 ), // this
                        /* 34 */ new( OpCodeSet.Ldc_I4, AssistMode_EV.BURDEN_REQUIREMENT_MAX_AMOUNT   ), // 200
                        /* 35 */ new( OpCodes.Stfld, name: "m_maxValue"                               ), // this.m_maxValue = 200
                        /* 36 */ new( OpCodes.Ldarg_0                                                 ), // this
                        /* 37 */ new( OpCodeSet.Ldc_I4, AssistMode_EV.BURDEN_REQUIREMENT_SCALE_AMOUNT ), // 50
                        /* 38 */ new( OpCodes.Stfld, name: "m_incrementValue"                         ), // this.m_incrementValue = 50
                        /* 39 */ new( OpCodes.Br                                                      ), // break
                        // case ChangeAssistStatModOptionItem.StatType.ResolveCost:
                        /* 40 */ new( OpCodes.Ldarg_0                                                 ), // this
                        /* 41 */ new( OpCodeSet.Ldc_I4, AssistMode_EV.RESOLVE_COST_MIN_AMOUNT         ), // 50
                        /* 42 */ new( OpCodes.Stfld, name: "m_minValue"                               ), // this.m_minValue = 50
                        /* 43 */ new( OpCodes.Ldarg_0                                                 ), // this
                        /* 44 */ new( OpCodeSet.Ldc_I4, AssistMode_EV.RESOLVE_COST_MAX_AMOUNT         ), // 200
                        /* 45 */ new( OpCodes.Stfld, name: "m_maxValue"                               ), // this.m_maxValue = 200
                        /* 46 */ new( OpCodes.Ldarg_0                                                 ), // this
                        /* 47 */ new( OpCodeSet.Ldc_I4, AssistMode_EV.RESOLVE_COST_SCALE_AMOUNT       ), // 10
                        /* 48 */ new( OpCodes.Stfld, name: "m_incrementValue"                         ), // this.m_incrementValue = 10
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetInstruction(  1, OpCodes.Ldc_I4, WobSettings.Get( "HouseRules", "MinBound", false ) ? 5 : AssistMode_EV.ENEMY_HEALTH_MIN_AMOUNT  ), // Set minimum enemy health
                        new WobTranspiler.OpAction_SetInstruction(  4, OpCodes.Ldc_I4, WobSettings.Get( "HouseRules", "MaxBound", AssistMode_EV.ENEMY_HEALTH_MAX_AMOUNT )              ), // Set maximum enemy health
                        new WobTranspiler.OpAction_SetInstruction( 11, OpCodes.Ldc_I4, WobSettings.Get( "HouseRules", "MinBound", false ) ? 0 : AssistMode_EV.ENEMY_DAMAGE_MIN_AMOUNT  ), // Set minimum enemy damage
                        new WobTranspiler.OpAction_SetInstruction( 14, OpCodes.Ldc_I4, WobSettings.Get( "HouseRules", "MaxBound", AssistMode_EV.ENEMY_DAMAGE_MAX_AMOUNT )              ), // Set maximum enemy damage
                        new WobTranspiler.OpAction_SetInstruction( 21, OpCodes.Ldc_I4, WobSettings.Get( "HouseRules", "MinBound", false ) ? 5 : AssistMode_EV.AIM_TIME_SLOW_MIN_AMOUNT ), // Set minimum aim time slow
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - JUMP & DASH RESET
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Patch for the method that resets dashes and make it also reset jumps
        [HarmonyPatch( typeof( CharacterDash_RL ), nameof( CharacterDash_RL.ResetNumberOfDashes ) )]
        internal static class CharacterDash_RL_ResetNumberOfDashes_Patch {
            internal static void Postfix() {
                if( WobSettings.Get( "MiscRules", "ResetJumpDash", false ) ) {
                    CharacterJump_RL characterJump = PlayerManager.GetPlayerController().CharacterJump;
                    // Can't just call ResetNumberOfJumps() without creating an infinite loop with the other patch, so need to set the value manually
                    // Use a Traverse to get around the access restriction on the setter
                    Traverse.Create( characterJump ).Property( "NumberOfJumpsLeft" ).SetValue( characterJump.NumberOfJumps );
                }
            }
        }

        // Patch for the method that resets jumps and make it also reset dashes
        [HarmonyPatch( typeof( CharacterJump ), nameof( CharacterJump.ResetNumberOfJumps ) )]
        internal static class CharacterJump_ResetNumberOfJumps_Patch {
            internal static void Postfix() {
                if( WobSettings.Get( "MiscRules", "ResetJumpDash", false ) ) {
                    CharacterDash_RL characterDash = PlayerManager.GetPlayerController().CharacterDash;
                    // Can't just call ResetNumberOfDashes() without creating an infinite loop with the other patch, so need to set the value manually
                    // Use Traverses to get around the access restriction on the fileds
                    Traverse.Create( characterDash ).Field( "m_numDashesAvailable" ).SetValue( Traverse.Create( characterDash ).Field( "m_totalDashesAllowed" ).GetValue<int>() );
                }
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - RESET BOSSES
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Things to be reset each death
        internal enum DeathReset { None, Bosses, BossesMinibosses, BossesMinibossesKeys }

        // Patch for the method that runs on character death to add boss and miniboss flag resets
        [HarmonyPatch( typeof( PlayerDeathWindowController ), nameof( PlayerDeathWindowController.GlobalCharacterDeathResetLogic ) )]
		internal static class PlayerDeathWindowController_GlobalCharacterDeathResetLogic_Patch {
			internal static void Postfix() {
                if( !SaveManager.PlayerSaveData.GetFlag( PlayerSaveFlag.GardenBoss_Defeated ) ) {
                    DeathReset setting = WobSettings.Get( "MiscRules", "DeathResetsBosses", DeathReset.None );
					if( setting >= DeathReset.Bosses ) {
                        WobPlugin.Log( "[GameRules] DEATH! Bosses reset" );
                        // Reset bosses back to undefeated
                        SaveManager.PlayerSaveData.SetFlag( PlayerSaveFlag.CastleBoss_Defeated, false ); // Lamech
                        SaveManager.PlayerSaveData.SetFlag( PlayerSaveFlag.BridgeBoss_Defeated, false ); // Pirates
                        SaveManager.PlayerSaveData.SetFlag( PlayerSaveFlag.ForestBoss_Defeated, false ); // Naamah
                        SaveManager.PlayerSaveData.SetFlag( PlayerSaveFlag.StudyBoss_Defeated,  false ); // Enoch
                        SaveManager.PlayerSaveData.SetFlag( PlayerSaveFlag.TowerBoss_Defeated,  false ); // Irad
                        SaveManager.PlayerSaveData.SetFlag( PlayerSaveFlag.CaveBoss_Defeated,   false ); // Tubal
                    }
                    if( setting >= DeathReset.BossesMinibosses ) {
                        WobPlugin.Log( "[GameRules] DEATH! Minibosses reset" );
                        // Reset minibosses back to undefeated
                        SaveManager.PlayerSaveData.SetFlag( PlayerSaveFlag.StudyMiniboss_SpearKnight_Defeated, false ); // Gongheads
                        SaveManager.PlayerSaveData.SetFlag( PlayerSaveFlag.StudyMiniboss_SwordKnight_Defeated, false ); // Murmur
                        SaveManager.PlayerSaveData.SetFlag( PlayerSaveFlag.CaveMiniboss_White_Defeated, false ); // Hammer + Sword
                        SaveManager.PlayerSaveData.SetFlag( PlayerSaveFlag.CaveMiniboss_Black_Defeated, false ); // Axe + Shield
                        // Close the doors to require beating the minibosses
                        SaveManager.PlayerSaveData.SetInsightState( InsightType.StudyBoss_DoorOpened, InsightState.Undiscovered, true );
                        SaveManager.PlayerSaveData.SetInsightState( InsightType.CaveBoss_DoorOpened, InsightState.Undiscovered, true );
                        // Reset the dragon
                        SaveManager.PlayerSaveData.SetFlag( PlayerSaveFlag.DragonDialogue_BossDoorOpen, false );
                        SaveManager.PlayerSaveData.SetFlag( PlayerSaveFlag.DragonDialogue_AfterDefeatingTubal, false );
                        SaveManager.PlayerSaveData.SetFlag( PlayerSaveFlag.DragonDialogue_Sleep, false );
                    }
                    if( setting >= DeathReset.BossesMinibossesKeys ) {
                        WobPlugin.Log( "[GameRules] DEATH! Keys reset" );
                        // Reset doors that require keys, offerings, or other actions to open
                        SaveManager.PlayerSaveData.SetInsightState( InsightType.CastleBoss_DoorOpened, InsightState.Undiscovered, true ); // Lamech's door lamps
                        SaveManager.PlayerSaveData.SetInsightState( InsightType.BridgeBoss_GateRaised, InsightState.Undiscovered, true ); // Axis Mundi Gatehouse portcullis
                        SaveManager.PlayerSaveData.SetInsightState( InsightType.ForestBoss_DoorOpened, InsightState.Undiscovered, true ); // Naamah's door lily offerings
                        SaveManager.PlayerSaveData.SetFlag( PlayerSaveFlag.CaveMiniboss_WhiteDoor_Opened, false ); // Pearl key
                        SaveManager.PlayerSaveData.SetFlag( PlayerSaveFlag.CaveMiniboss_BlackDoor_Opened, false ); // Onyx key
                    }
                }
            }
		}

    }
}
