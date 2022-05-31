using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_RelicStats {
    [BepInPlugin( "Wob.RelicStats", "Relic Stats Mod", "0.1.0" )]
    public partial class RelicStats : BaseUnityPlugin {
        private static readonly WobSettings.KeyHelper<RelicType> keys = new WobSettings.KeyHelper<RelicType>( "Relic_" );

        private static readonly Dictionary<RelicType,(string Config, string Name, bool Add, int Resolve)> RelicInfo = new Dictionary<RelicType,(string Config, string Name, bool Add, int resolve)>() {
            { RelicType.MaxHealthStatBonus,         ( "AchillesShield",          "Achilles' Shield",           true,  25 ) },
            { RelicType.WeaponsBurnAdd,             ( "AmaterasusSun",           "Amaterasu's Sun",            true,  55 ) },
            { RelicType.ExtendInvuln,               ( "Ambrosia",                "Ambrosia",                   true,  25 ) },
            { RelicType.MagicDamageEnemyCount,      ( "Antikythera",             "Antikythera",                true,  35 ) },
            { RelicType.FreeCastSpell,              ( "ArcaneNecklace",          "Arcane Necklace",            true,  25 ) },
            { RelicType.AllCritDamageUp,            ( "AtroposScissors",         "Atropos' Scissors",          true,  25 ) },
            { RelicType.EnemiesDropMeat,            ( "BodyBuffet",              "Body Buffet",                true,  35 ) },
            { RelicType.WeaponsComboAdd,            ( "BoxingBell",              "Boxing Bell",                true,  45 ) },
            { RelicType.DamageBuffStatusEffect,     ( "Catalyst",                "Catalyst",                   true,  25 ) },
            { RelicType.AllCritChanceUp,            ( "ClothosSpindle",          "Clotho's Spindle",           true,  25 ) },
            { RelicType.FreeHitRegenerate,          ( "CoeusShell",              "Coeus' Shell",               true,  45 ) },
            { RelicType.FoodHealsMore,              ( "Cornucopia",              "Cornucopia",                 true,  25 ) },
            { RelicType.ProjectileDashStart,        ( "CorruptingReagent",       "Corrupting Reagent",         true,  35 ) },
            { RelicType.ManaRestoreOnHurt,          ( "CosmicInsight",           "Cosmic Insight",             true,  25 ) },
            { RelicType.NoGoldXPBonus,              ( "DiogenesBargain",         "Diogenes' Bargain",          true,  15 ) },
            { RelicType.SpellKillMaxMana,           ( "Dreamcatcher",            "Dreamcatcher",               true,  35 ) },
            { RelicType.FreeEnemyKill,              ( "FatesDie",                "Fate's Die",                 true,  25 ) },
            { RelicType.ChestHealthRestore,         ( "FreonsReward",            "Freon's Reward",             true,  25 ) },
            { RelicType.GoldDeathCurse,             ( "FutureSuccessorsBargain", "Future Successor's Bargain", true,  0  ) },
            { RelicType.BonusDamageOnNextHit,       ( "GlowingEmber",            "Glowing Ember",              true,  25 ) },
            { RelicType.MeatMaxHealth,              ( "GnawedBone",              "Gnawed Bone",                true,  25 ) },
            { RelicType.FatalBlowDodge,             ( "GraveBell",               "Grave Bell",                 true,  35 ) },
            { RelicType.AttackCooldown,             ( "HeavyStoneBargain",       "Heavy Stone Bargain",        true,  15 ) },
            { RelicType.LowHealthStatBonus,         ( "HectorsHelm",             "Hector's Helm",              true,  25 ) },
            { RelicType.NoSpikeDamage,              ( "HermesBoots",             "Hermes' Boots",              true,  25 ) },
            { RelicType.LowResolveMagicDamage,      ( "HeronsRing",              "Heron's Ring",               true,  35 ) },
            { RelicType.FlightBonusCurse,           ( "IcarusWingsBargain",      "Icarus' Wings Bargain",      true,  15 ) },
            { RelicType.RangeDamageBonusCurse,      ( "IncandescentTelescope",   "Incandescent Telescope",     true,  25 ) },
            { RelicType.GroundDamageBonus,          ( "IvyRoots",                "Ivy Roots",                  true,  25 ) },
            { RelicType.PlatformOnAerial,           ( "IvySeed",                 "Ivy Seed",                   true,  25 ) },
            { RelicType.CritKillsHeal,              ( "LachesisMeasure",         "Lachesis' Measure",          true,  55 ) },
            { RelicType.SkillCritBonus,             ( "LamechsWhetstone",        "Lamech's Whetstone",         true,  35 ) },
            { RelicType.BonusMana,                  ( "LotusPetal",              "Lotus Petal",                true,  25 ) },
            { RelicType.ManaDamageReduction,        ( "LotusStem",               "Lotus Stem",                 true,  45 ) },
            { RelicType.LandShockwave,              ( "MarbleStatue",            "Marble Statue",              true,  25 ) },
            { RelicType.SuperCritChanceUp,          ( "Obelisk",                 "Obelisk",                    true,  25 ) },
            { RelicType.InvulnDamageBuff,           ( "RageTincture",            "Rage Tincture",              true,  25 ) },
            { RelicType.LowResolveWeaponDamage,     ( "RavensRing",              "Raven's Ring",               true,  35 ) },
            { RelicType.NoAttackDamageBonus,        ( "RedSandHourglass",        "Red Sand Hourglass",         true,  25 ) },
            { RelicType.WeaponsPoisonAdd,           ( "SerqetsStinger",          "Serqet's Stinger",           true,  55 ) },
            { RelicType.BonusDamageCurse,           ( "SerratedHandlesBargain",  "Serrated Handle's Bargain",  true,  15 ) },
            { RelicType.OnHitAreaDamage,            ( "SoulTether",              "Soul Tether",                true,  45 ) },
            { RelicType.SpinKickArmorBreak,         ( "Steel-ToedBoots",         "Steel-Toed Boots",           true,  35 ) },
            { RelicType.DashStrikeDamageUp,         ( "VanguardsBanner",         "Vanguard's Banner",          true,  45 ) },
            { RelicType.DamageAuraOnHit,            ( "VoltaicCirclet",          "Voltaic Circlet",            true,  35 ) },
            { RelicType.RelicAmountDamageUp,        ( "WarDrum",                 "War Drum",                   true,  45 ) },
            { RelicType.SpinKickDamageBonus,        ( "WeightedAnklet",          "Weighted Anklet",            true,  25 ) },
            { RelicType.SporeburstKillAdd,          ( "WeirdMushrooms",          "Weird Mushrooms",            true,  35 ) },
            { RelicType.MaxManaDamage,              ( "ZealotsRing",             "Zealot's Ring",              true,  25 ) },
            // Relics that purify
            { RelicType.GoldCombatChallenge,        ( "CharonsTrial",            "Charon's Trial",             true,  0  ) }, // Defeat 15 enemies to purify
            { RelicType.GoldCombatChallengeUsed,    ( "CharonsTrial",            "Charon's Trial",             false, 0  ) },       // <- Charon's Reward
            { RelicType.FoodChallenge,              ( "DemetersTrial",           "Demeter's Trial",            true,  0  ) }, // Collect 1 food/potion to purify
            { RelicType.FoodChallengeUsed,          ( "DemetersTrial",           "Demeter's Trial",            false, 0  ) },       // <- Demeter's Reward
            { RelicType.ResolveCombatChallenge,     ( "PandorasTrial",           "Pandora's Trial",            true,  0  ) }, // Defeat 10 enemies to purify
            { RelicType.ResolveCombatChallengeUsed, ( "PandorasTrial",           "Pandora's Trial",            false, 0  ) },       // <- Pandora's Reward
            // Relics that break
            { RelicType.DamageNoHitChallenge,       ( "AitesSword",              "Aite's Sword",               true,  35 ) }, // Breaks on getting hit
            { RelicType.DamageNoHitChallengeUsed,   ( "AitesSword",              "Aite's Sword",               false, 35 ) },       // <- Aite's Broken Sword
            { RelicType.ExtraLife,                  ( "HyperionsRing",           "Hyperion's Ring",            true,  55 ) }, // Breaks on death
            { RelicType.ExtraLifeUsed,              ( "HyperionsRing",           "Hyperion's Ring",            false, 55 ) },       // <- Hyperion's Broken Ring
            { RelicType.ExtraLife_Unity,            ( "AncestralSoul",           "Ancestral Soul",             true,  0  ) }, // Breaks on death
            { RelicType.ExtraLife_UnityUsed,        ( "AncestralSoul",           "Ancestral Soul",             false, 0  ) },       // <- Ancestral Dust
            { RelicType.FreeFairyChest,             ( "SkeletonKey",             "Skeleton Key",               true,  35 ) }, // Breaks on unlock fairy chest
            { RelicType.FreeFairyChestUsed,         ( "SkeletonKey",             "Skeleton Key",               false, 35 ) },       // <- Broken Key
            // Relics required for accessing Estuaries
            { RelicType.Lily1,                      ( "LilyOfTheValley",         "Lily of the Valley",         true,  10 ) },
            { RelicType.Lily2,                      ( "LilyOfTheValley",         "Lily of the Valley",         false, 10 ) },
            { RelicType.Lily3,                      ( "LilyOfTheValley",         "Lily of the Valley",         false, 10 ) },
            { RelicType.DragonKeyBlack,             ( "DragonKey",               "Onyx Key/Pearl Key",         true,  45 ) },
            { RelicType.DragonKeyWhite,             ( "DragonKey",               "Onyx Key/Pearl Key",         false, 45 ) },
        };

        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            foreach( RelicType relicType in RelicInfo.Keys ) {
                if( !keys.Exists( relicType ) ) {
                    keys.Add( relicType, RelicInfo[relicType].Config );
                    if( RelicInfo[relicType].Add && RelicInfo[relicType].Resolve > 0 ) {
                        WobSettings.Add( new WobSettings.Num<int>( keys.Get( relicType, "Resolve" ), RelicInfo[relicType].Name + ": Resolve cost percent", RelicInfo[relicType].Resolve, 0.01f, bounds: (0, 1000000) ) );
                    }
                }
            }
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Num<float>( keys.Get( RelicType.AttackCooldown,       "Cooldown"      ), RelicInfo[RelicType.AttackCooldown].Name       + ": Set the attack cooldown to this number of seconds",  2f,          bounds: (0f, 1000000f) ),
                new WobSettings.Num<float>( keys.Get( RelicType.FlightBonusCurse,     "DamageTaken"   ), RelicInfo[RelicType.FlightBonusCurse].Name     + ": Set the additional damage taken to this percent",    75f,  0.01f, bounds: (0f, 1000000f) ),
                new WobSettings.Boolean(    keys.Get( RelicType.FreeFairyChest,       "InfiniteUses"  ), RelicInfo[RelicType.FreeFairyChest].Name       + ": Infinite uses without breaking",                     false                               ),
                new WobSettings.Boolean(    keys.Get( RelicType.DamageNoHitChallenge, "RemoveOnBreak" ), RelicInfo[RelicType.DamageNoHitChallenge].Name + ": Remove on breaking, refunding resolve",              false                               ),
                new WobSettings.Boolean(    keys.Get( RelicType.ExtraLife,            "RemoveOnBreak" ), RelicInfo[RelicType.ExtraLife].Name            + ": Remove on breaking, refunding resolve",              false                               ),
            } );

            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Change the resolve costs
        [HarmonyPatch( typeof( RelicLibrary ), "Instance", MethodType.Getter )]
        internal static class RelicLibrary_GetRelicData_Patch {
            private static bool runOnce = false;
            internal static void Postfix( RelicLibrary __result ) {
                if( !runOnce ) {
                    RelicTypeRelicDataDictionary m_relicLibrary = (RelicTypeRelicDataDictionary)Traverse.Create( __result ).Field( "m_relicLibrary" ).GetValue();
                    foreach( KeyValuePair<RelicType, RelicData> relic in m_relicLibrary ) {
                        if( keys.Exists( relic.Key ) ) {
                            float resolve = WobSettings.Get( keys.Get( relic.Key, "Resolve" ), relic.Value.CostAmount );
                            WobPlugin.Log( "Changing resolve cost for " + relic.Key + " " + relic.Value.CostAmount + " -> " + resolve );
                            relic.Value.CostAmount = resolve;
                        }
                    }
                    runOnce = true;
                }
            }
        }

        // Prevent adding the 'Used' variant of Aite's Sword and Hyperion's Ring
        [HarmonyPatch( typeof( RelicObj ), nameof( RelicObj.SetLevel ) )]
        internal static class RelicObj_SetLevel_Patch {
            internal static void Prefix( RelicObj __instance, ref int value ) {
                RelicType relicType = __instance.RelicType;
                if( ( relicType == RelicType.DamageNoHitChallengeUsed || relicType == RelicType.ExtraLife ) && value > 0 ) {
                    if( WobSettings.Get( keys.Get( relicType, "RemoveOnBreak" ), false ) ) {
                        value = 0;
                    }
                }
            }
        }

        // Prevent breaking of Skeleton Key
        [HarmonyPatch( typeof( ChestObj ), "OpenChest" )]
        internal static class ChestObj_OpenChest_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "ChestObj.OpenChest Transpiler Patch" );
                if( WobSettings.Get( keys.Get( RelicType.FreeFairyChest, "InfiniteUses" ), false ) ) {
                    // Set up the transpiler handler with the instruction list
                    WobTranspiler transpiler = new WobTranspiler( instructions );
                    // Perform the patching
                    transpiler.PatchAll(
                            // Define the IL code instructions that should be matched
                            new List<WobTranspiler.OpTest> {
                                // relic.SetLevel(-1, true, true);
                                // SaveManager.PlayerSaveData.GetRelic(RelicType.FreeFairyChestUsed).SetLevel(1, true, true);
                                /*  0 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc                                   ), // relic
                                /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4_M1                                 ), // -1
                                /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4_Bool                             ), // true
                                /*  3 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4_Bool                             ), // true
                                /*  4 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "SetLevel"                ), // relic.SetLevel(-1, true, true)
                                /*  5 */ new WobTranspiler.OpTest( OpCodes.Ldsfld, name: "PlayerSaveData"            ), // SaveManager.PlayerSaveData
                                /*  6 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, (int)RelicType.FreeFairyChestUsed ), // RelicType.FreeFairyChestUsed
                                /*  7 */ new WobTranspiler.OpTest( OpCodes.Callvirt,  name: "GetRelic"               ), // SaveManager.PlayerSaveData.GetRelic(RelicType.FreeFairyChestUsed)
                                /*  8 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4_1                                  ), // 1
                                /*  9 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4_Bool                             ), // true
                                /* 10 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4_Bool                             ), // true
                                /* 11 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "SetLevel"                ), // SaveManager.PlayerSaveData.GetRelic(RelicType.FreeFairyChestUsed).SetLevel(1, true, true)
                            },
                            // Define the actions to take when an occurrence is found
                            new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_Remove( 0, 12 ),
                            } );
                    // Return the modified instructions
                    return transpiler.GetResult();
                } else {
                    return instructions;
                }
            }
        }

        // Set the attack cooldown of Heavy Stone Bargain
        [HarmonyPatch( typeof( BaseAbility_RL ), nameof( BaseAbility_RL.ActualCooldownTime ), MethodType.Getter )]
        internal static class BaseAbility_RL_ActualCooldownTime_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "BaseAbility_RL.ActualCooldownTime Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc    ), // num
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4, 2f ), // 2f
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc    ), // level
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Conv_R4    ), // (float)level
                            /*  4 */ new WobTranspiler.OpTest( OpCodes.Mul        ), // 2f * (float)level
                            /*  5 */ new WobTranspiler.OpTest( OpCodes.Add        ), // num + 2f * (float)level
                            /*  6 */ new WobTranspiler.OpTest( OpCodeSet.Stloc    ), // num = num + 2f * (float)level
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( keys.Get( RelicType.AttackCooldown, "Cooldown" ), 2f ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Set the damage taken for Icarus' Wings Bargain
        [HarmonyPatch( typeof( PlayerController ), nameof( PlayerController.CalculateDamageTaken ) )]
        internal static class PlayerController_CalculateDamageTaken_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "PlayerController.CalculateDamageTaken Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            // num += 0.75f * (float)SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse).Level;
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4                                  ), // 0.75f
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldsfld, name: "PlayerSaveData"          ), // SaveManager.PlayerSaveData
                            /*  2 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, (int)RelicType.FlightBonusCurse ), // RelicType.FlightBonusCurse
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "GetRelic"              ), // SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse)
                            /*  4 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_Level"             ), // SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse).Level
                            /*  5 */ new WobTranspiler.OpTest( OpCodes.Conv_R4                                 ), // (float)SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse).Level
                            /*  6 */ new WobTranspiler.OpTest( OpCodes.Mul                                     ), // 0.75f * (float)SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse).Level
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 0, WobSettings.Get( keys.Get( RelicType.FlightBonusCurse, "DamageTaken" ), 0.75f ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

    }
}