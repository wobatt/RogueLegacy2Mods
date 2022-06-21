using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace Wob_RelicStats {
    [BepInPlugin( "Wob.RelicStats", "Relic Stats Mod", "1.0.0" )]
    public partial class RelicStats : BaseUnityPlugin {
        private static readonly WobSettings.KeyHelper<RelicType> keys = new WobSettings.KeyHelper<RelicType>( "Relic" );

        private static readonly Dictionary<RelicType,(string Config, string Name, bool Spawn, int Resolve, int Stack)> RelicInfo = new Dictionary<RelicType,(string Config, string Name, bool Spawn, int Resolve, int Stack)>() {
            // Standard stackable relics
            { RelicType.MaxHealthStatBonus,         ( "AchillesShield",          "Achilles' Shield - While ABOVE 50% Health, deal 10% more Spell and Weapon damage.",                                 true,   25, 5  ) }, // +10% dmg
            { RelicType.WeaponsBurnAdd,             ( "AmaterasusSun",           "Amaterasu's Sun - Your Weapon applies (or extends) Burn for 2 seconds.",                                            true,   55, 5  ) }, // +2s
            { RelicType.ExtendInvuln,               ( "Ambrosia",                "Ambrosia - After taking damage you are Invincible for an additional 1.25 seconds.",                                 true,   25, 5  ) }, // +1.25s
            { RelicType.MagicDamageEnemyCount,      ( "Antikythera",             "Antikythera - Gain 10% more Magic Damage per enemy defeated. Bonus lost when hit (max 50%).",                       true,   35, 5  ) }, // +50% max
            { RelicType.FreeCastSpell,              ( "ArcaneNecklace",          "Arcane Necklace - Cast 3 Spells to charge the necklace, and make the next cast free.",                              true,   25, 4  ) }, // -1 spell
            { RelicType.AllCritDamageUp,            ( "AtroposScissors",         "Atropos' Scissors - Critical damage from Spells and Weapons increased by 20%.",                                     true,   25, 5  ) }, // +20%
            { RelicType.EnemiesDropMeat,            ( "BodyBuffet",              "Body Buffet - Defeated enemies have a 8% chance of dropping a Health Drop.",                                        true,   35, 5  ) }, // +8%
            { RelicType.WeaponsComboAdd,            ( "BoxingBell",              "Boxing Bell - Your Weapon applies Combo.",                                                                          true,   45, 5  ) }, // +1 stack
            { RelicType.DamageBuffStatusEffect,     ( "Catalyst",                "Catalyst - Deal 20% bonus damage to enemies with a Status Effect.",                                                 true,   25, 5  ) }, // +20%
            { RelicType.AllCritChanceUp,            ( "ClothosSpindle",          "Clotho's Spindle - Critical chance for Spells and Weapons increased by 10%.",                                       true,   25, 5  ) }, // +10%
            { RelicType.FreeHitRegenerate,          ( "CoeusShell",              "Coeus' Shell - Defeat 6 enemies to prevent the next source of damage.",                                             true,   45, 5  ) }, // -1 kill
            { RelicType.FoodHealsMore,              ( "Cornucopia",              "Cornucopia - Health Drops restore an additional 8% of your Max Health.",                                            true,   25, 5  ) }, // +8%
            { RelicType.ProjectileDashStart,        ( "CorruptingReagent",       "Corrupting Reagent - Dashing leaves a poisonous cloud behind for 1 second.",                                        true,   35, 5  ) }, // +1s
            { RelicType.ManaRestoreOnHurt,          ( "CosmicInsight",           "Cosmic Insight - Gain 100 Mana when hurt. Mana gained can Overcharge.",                                             true,   25, 5  ) }, // +100 mana
            { RelicType.SpellKillMaxMana,           ( "Dreamcatcher",            "Dreamcatcher - Defeat enemies while at Max HP to gain 5 Max Mana (Max 200).",                                       true,   35, 5  ) }, // +200 max
            { RelicType.ReplacementRelic,           ( "EmptyVessel",             "Empty Vessel - Gain 10% bonus Health, Weapon, and Magic Damage.",                                                   false,  25, 5  ) }, // +10%
            { RelicType.FreeEnemyKill,              ( "FatesDie",                "Fate's Die - Defeat 6 enemies to load the die and instantly defeat the next enemy.",                                true,   25, 5  ) }, // -1 kill
            { RelicType.ChestHealthRestore,         ( "FreonsReward",            "Freon's Reward - Opening chests restores Health (100% of INT.).",                                                   true,   25, 5  ) }, // +100% INT
            { RelicType.BonusDamageOnNextHit,       ( "GlowingEmber",            "Glowing Ember - Deal 75% more damage on every 6th hit. Counter resets if you take damage.",                         true,   25, 5  ) }, // +75% dmg
            { RelicType.MeatMaxHealth,              ( "GnawedBone",              "Gnawed Bone - Eating Health Drops while at Full Health increases Max HP by 10% (Max 3 stacks).",                    true,   25, 5  ) }, // +3 max
            { RelicType.FatalBlowDodge,             ( "GraveBell",               "Grave Bell - You have a 25% chance of avoiding a Fatal Blow.",                                                      true,   35, 3  ) }, // +25%
            { RelicType.LowHealthStatBonus,         ( "HectorsHelm",             "Hector's Helm - While BELOW 50% Health, deal 20% more Spell and Weapon damage.",                                    true,   25, 5  ) }, // +20% dmg
            { RelicType.LowResolveMagicDamage,      ( "HeronsRing",              "Heron's Ring - For every point of Resolve below 100%, deal an additional 1% bonus Magic damage.",                   true,   35, 5  ) }, // +1% dmg
            { RelicType.RangeDamageBonusCurse,      ( "IncandescentTelescope",   "Incandescent Telescope - Deal 12.5% more damage to enemies far away.",                                              true,   25, 5  ) }, // +12.5% dmg
            { RelicType.GroundDamageBonus,          ( "IvyRoots",                "Ivy Roots - Deal 12.5% bonus damage while on the ground.",                                                          true,   25, 5  ) }, // +12.5% dmg
            { RelicType.CritKillsHeal,              ( "LachesisMeasure",         "Lachesis' Measure - Enemies defeated with a critical hit restore 6% of your Max Health.",                           true,   55, 5  ) }, // +6%
            { RelicType.BonusMana,                  ( "LotusPetal",              "Lotus Petal - Increases your total Mana Pool by 50. Deal 8% more Magic Damage.",                                    true,   25, 5  ) }, // +50 mana, +8% dmg
            { RelicType.ManaDamageReduction,        ( "LotusStem",               "Lotus Stem - Blocks up to 2 attacks. Consumes 150 Mana per block. Mana potions restore charges.",                   true,   45, 5  ) }, // +2 blocks
            { RelicType.LandShockwave,              ( "MarbleStatue",            "Marble Statue - Landing creates a small shockwave that destroys Mid-sized Projectiles and deals 75% Magic damage.", true,   25, 5  ) }, // +75% dmg
            { RelicType.SuperCritChanceUp,          ( "Obelisk",                 "Obelisk - Your Skill Crits have an extra 20% chance of becoming Super Crits.",                                      true,   25, 5  ) }, // +20%
            { RelicType.InvulnDamageBuff,           ( "RageTincture",            "Rage Tincture - After taking damage, deal 100% more damage during the Invincibility window.",                       true,   25, 5  ) }, // +100% dmg
            { RelicType.LowResolveWeaponDamage,     ( "RavensRing",              "Raven's Ring - For every point of Resolve below 100%, deal an additional 1% bonus Weapon damage.",                  true,   35, 5  ) }, // +1% dmg
            { RelicType.NoAttackDamageBonus,        ( "RedSandHourglass",        "Red Sand Hourglass - Every 5 seconds your next Weapon attack deals 75% bonus damage.",                              true,   25, 5  ) }, // -1s
            { RelicType.WeaponsPoisonAdd,           ( "SerqetsStinger",          "Serqet's Stinger - Your Weapon applies 1 stack of Poison.",                                                         true,   55, 5  ) }, // +1 stack
            { RelicType.OnHitAreaDamage,            ( "SoulTether",              "Soul Tether - Every 5 seconds, your next Weapon attack deals 150% bonus Magic damage to all nearby enemies.",       true,   45, 5  ) }, // -1s, +75% dmg
            { RelicType.DashStrikeDamageUp,         ( "VanguardsBanner",         "Vanguard's Banner - Dashing creates a wave that destroys Mid-sized Projectiles. Waves travel 2 units.",             true,   45, 5  ) }, // +2 units
            { RelicType.DamageAuraOnHit,            ( "VoltaicCirclet",          "Voltaic Circlet - Hitting an enemy with your Weapon will generate a damage aura around you for 1.5 seconds.",       true,   35, 5  ) }, // +1.5s
            { RelicType.RelicAmountDamageUp,        ( "WarDrum",                 "War Drum - Every unique Relic increases your damage by 6%.",                                                        true,   45, 5  ) }, // +6%
            { RelicType.SpinKickDamageBonus,        ( "WeightedAnklet",          "Weighted Anklet - Your Spin Kicks deal 40% more damage.",                                                           true,   25, 5  ) }, // +40%
            { RelicType.MaxManaDamage,              ( "ZealotsRing",             "Zealot's Ring - Spells cast while at Max MP deal an additional 25% damage.",                                        true,   25, 5  ) }, // +25% dmg
            // Non-stackable relics
            { RelicType.NoGoldXPBonus,              ( "DiogenesBargain",         "Diogenes' Bargain - No more gold. All Gold Bonuses are converted into an XP Bonus (X%) instead.",                   true,   15, 1  ) },
            { RelicType.GoldDeathCurse,             ( "FutureSuccessorsBargain", "Future Successor's Bargain - Your heirs shall inherit your fortune.",                                               true,   15, 1  ) },
            { RelicType.AttackCooldown,             ( "HeavyStoneBargain",       "Heavy Stone Bargain - Your Weapon deals 100% more damage, but has a 2 second cooldown.",                            true,   15, 1  ) },
            { RelicType.NoSpikeDamage,              ( "HermesBoots",             "Hermes' Boots - You are Immune to static spikes.",                                                                  true,   25, 1  ) },
            { RelicType.FlightBonusCurse,           ( "IcarusWingsBargain",      "Icarus' Wings Bargain - Jumping in the air enables Flight, but you take 75% extra damage.",                         true,   15, 1  ) },
            { RelicType.PlatformOnAerial,           ( "IvySeed",                 "Ivy Seed - Create an Ivy Canopy every time you do an Aerial Recovery.",                                             true,   25, 1  ) },
            { RelicType.SkillCritBonus,             ( "LamechsWhetstone",        "Lamech's Whetstone - Weapon Skill Crits now apply Magic Break for 2.5 seconds.",                                    true,   35, 1  ) },
            { RelicType.BonusDamageCurse,           ( "SerratedHandlesBargain",  "Serrated Handle's Bargain - Deal and take 100% more damage.",                                                       true,   15, 1  ) },
            { RelicType.SpinKickArmorBreak,         ( "Steel-ToedBoots",         "Steel-Toed Boots - Spin Kicks now apply Armor Break for 3.5 seconds.",                                              true,   35, 1  ) },
            { RelicType.SporeburstKillAdd,          ( "WeirdMushrooms",          "Weird Mushrooms - Defeated enemies have Spore Burst applied to them.",                                              true,   35, 1  ) },
            // Relics that purify
            { RelicType.GoldCombatChallenge,        ( "CharonsTrial",            "Charon's Trial - Defeat 15 enemies to purify it and gain a 20% Gold, Ore, and Aether bonus.",                       true,   0,  1  ) },
            { RelicType.GoldCombatChallengeUsed,    ( "CharonsTrial",            "Charon's Reward - Defeat 15 enemies to purify it and gain a 20% Gold, Ore, and Aether bonus.",                      false,  0,  1  ) },
            { RelicType.FoodChallenge,              ( "DemetersTrial",           "Demeter's Trial - Collect 1 food/potion to purify this Relic and gain 30% Max Health and 50 Mana.",                 true,   0,  1  ) },
            { RelicType.FoodChallengeUsed,          ( "DemetersTrial",           "Demeter's Reward - Collect 1 food/potion to purify this Relic and gain 30% Max Health and 50 Mana.",                false,  0,  1  ) },
            { RelicType.ResolveCombatChallenge,     ( "PandorasTrial",           "Pandora's Trial - Defeat 10 enemies to purify this relic and gain 50 Resolve.",                                     true,   0,  1  ) },
            { RelicType.ResolveCombatChallengeUsed, ( "PandorasTrial",           "Pandora's Reward - Defeat 10 enemies to purify this relic and gain 50 Resolve.",                                    false, -50, 1  ) },
            // Relics that break
            { RelicType.DamageNoHitChallenge,       ( "AitesSword",              "Aite's Sword - You deal 150% more damage, but this Relic is fragile.",                                              true,   20, 1  ) },
            { RelicType.DamageNoHitChallengeUsed,   ( "AitesSword",              "Aite's Broken Sword - You deal 150% more damage, but this Relic is fragile.",                                       false,  20, 1  ) },
            { RelicType.ExtraLife_Unity,            ( "AncestralSoul",           "Ancestral Soul - Revive from fatal blows and regain 50% of your HP.",                                               false,  0,  1  ) }, // Given by Kin 100 unity
            { RelicType.ExtraLife_UnityUsed,        ( "AncestralSoul",           "Ancestral Dust - Revive from fatal blows and regain 50% of your HP.",                                               false,  0,  1  ) },
            { RelicType.ExtraLife,                  ( "HyperionsRing",           "Hyperion's Ring - Revive from fatal blows and regain 50% of your HP.",                                              true,   55, 5  ) }, // +1 use
            { RelicType.ExtraLifeUsed,              ( "HyperionsRing",           "Hyperion's Broken Ring - Revive from fatal blows and regain 50% of your HP.",                                       false,  55, 5  ) },
            { RelicType.FreeFairyChest,             ( "SkeletonKey",             "Skeleton Key - Open locked or melted Fairy Chests. Breaks after use.",                                              true,   35, 5  ) }, // +1 use
            { RelicType.FreeFairyChestUsed,         ( "SkeletonKey",             "Broken Key - Open locked or melted Fairy Chests. Breaks after use.",                                                false,  35, 5  ) },
            // Relics required for accessing Estuaries
            { RelicType.Lily1,                      ( "LilyOfTheValley",         "Lily of the Valley - A lily of the valley. (For access to Estuary Namaah)",                                         false,  10, 1  ) },
            { RelicType.Lily2,                      ( "LilyOfTheValley",         "Lily of the Valley - A lily of the valley. (For access to Estuary Namaah)",                                         false,  10, 1  ) },
            { RelicType.Lily3,                      ( "LilyOfTheValley",         "Lily of the Valley - A lily of the valley. (For access to Estuary Namaah)",                                         false,  10, 1  ) },
            { RelicType.DragonKeyBlack,             ( "DragonKey",               "Onyx Key/Pearl Key - A black/white key. (For access to Pishon Dry Lake minibosses)",                                false,  45, 1  ) },
            { RelicType.DragonKeyWhite,             ( "DragonKey",               "Onyx Key/Pearl Key - A black/white key. (For access to Pishon Dry Lake minibosses)",                                false,  45, 1  ) },
            // Ability swap blessings
            { RelicType.WeaponSwap,                 ( "BlessingOfStrength",      "Blessing of Strength - Deal 7% more Weapon Damage.",                                                                false,  0,  99 ) }, // +7% dmg
            { RelicType.SpellSwap,                  ( "BlessingOfWisdom",        "Blessing of Wisdom - Deal 7% more Magic Damage.",                                                                   false,  0,  99 ) }, // +7% dmg
            { RelicType.TalentSwap,                 ( "BlessingOfTalent",        "Blessing of Talent - Gain 20 Resolve.",                                                                             false, -20, 99 ) }, // +20 resolve
        };

        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            foreach( RelicType relicType in RelicInfo.Keys ) {
                if( !keys.Exists( relicType ) ) {
                    keys.Add( relicType, RelicInfo[relicType].Config );
                    if( !WobSettings.Exists( keys.Get( relicType, "Enabled" ) ) && RelicInfo[relicType].Spawn ) {
                        WobSettings.Add( new WobSettings.Boolean( keys.Get( relicType, "Enabled" ), "Enable random spawn for " + RelicInfo[relicType].Name, RelicInfo[relicType].Spawn ) );
                    }
                    if( !WobSettings.Exists( keys.Get( relicType, "ResolveCost" ) ) && RelicInfo[relicType].Resolve > 0 ) {
                        WobSettings.Add( new WobSettings.Num<int>( keys.Get( relicType, "ResolveCost" ), "Resolve cost percent for " + RelicInfo[relicType].Name, RelicInfo[relicType].Resolve, 0.01f, bounds: (0, 1000) ) );
                    }
                    if( !WobSettings.Exists( keys.Get( relicType, "ResolveBonus" ) ) && RelicInfo[relicType].Resolve < 0 ) {
                        WobSettings.Add( new WobSettings.Num<int>( keys.Get( relicType, "ResolveBonus" ), "Resolve bonus percent for " + RelicInfo[relicType].Name, -RelicInfo[relicType].Resolve, -0.01f, bounds: (0, 1000) ) );
                    }
                }
            }
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Boolean(    keys.Get( RelicType.DamageNoHitChallenge, "RemoveOnBreak" ), "Remove on breaking (refund resolve) for " + RelicInfo[RelicType.DamageNoHitChallenge].Name,                         false                                 ),
                new WobSettings.Boolean(    keys.Get( RelicType.ExtraLife,            "RemoveOnBreak" ), "Remove on breaking (refund resolve) for " + RelicInfo[RelicType.ExtraLife].Name,                                    false                                 ),
                new WobSettings.Boolean(    keys.Get( RelicType.FreeFairyChest,       "RemoveOnBreak" ), "Remove on breaking (refund resolve) for " + RelicInfo[RelicType.FreeFairyChest].Name,                               false                                 ),
                new WobSettings.Boolean(    keys.Get( RelicType.FreeFairyChest,       "InfiniteUses"  ), "Infinite uses without breaking for " + RelicInfo[RelicType.FreeFairyChest].Name,                                    false                                 ),
                new WobSettings.Num<float>( keys.Get( RelicType.AttackCooldown,       "Cooldown"      ), "Set the attack cooldown to this number of seconds for " + RelicInfo[RelicType.AttackCooldown].Name,                 2f,            bounds: (0f, 1000000f) ),
                new WobSettings.Num<float>( keys.Get( RelicType.AttackCooldown,       "CooldownGun"   ), "Set the attack cooldown on the revolver to this number of seconds for " + RelicInfo[RelicType.AttackCooldown].Name, 0.125f,        bounds: (0f, 1000000f) ),
                new WobSettings.Num<int>(   keys.Get( RelicType.AttackCooldown,       "SlowHammer"    ), "Set the move speed penalty on the hammer to this percent for " + RelicInfo[RelicType.AttackCooldown].Name,          30,    -0.01f, bounds: (0, 1000000)   ),
                new WobSettings.Num<float>( keys.Get( RelicType.FlightBonusCurse,     "DamageTaken"   ), "Set the additional damage taken to this percent for " + RelicInfo[RelicType.FlightBonusCurse].Name,                 75f,    0.01f, bounds: (0f, 1000000f) ),
            } );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        // Enable or disable a relic for random spawn
        [HarmonyPatch( typeof( RelicType_RL ), nameof( RelicType_RL.TypeArray ), MethodType.Getter )]
        internal static class RelicType_RL_TypeArray_Patch {
            private static bool runOnce = false;
            internal static void Prefix() {
                // Only need to run this once, as the new settings are written into the trait data for the session
                if( !runOnce ) {
                    // Get the list of traits from the private field
                    RelicType[] m_typeArray = (RelicType[])Traverse.Create( typeof( RelicType_RL ) ).Field( "m_relicTypeArray" ).GetValue();
                    // Go through each type in the array
                    foreach( RelicType relicType in m_typeArray ) {
                        // Get the trait data that includes rarity info
                        RelicData relicData = RelicLibrary.GetRelicData( relicType );
                        if( relicData != null && keys.Exists( relicType ) ) {
                            // Check that the rarity is allowing spawn, and get the value of the setting that has the same name as the internal name of the relic
                            if( relicData.Rarity == 1 && !WobSettings.Get( keys.Get( relicType, "Enabled" ), true ) ) {
                                // The game seems to use a value of 99 for the rarity of diabled relics, so I will stick to this convention
                                relicData.Rarity = 99;
                                WobPlugin.Log( "Banning relic " + relicData.Name );
                            }
                        }
                    }
                    runOnce = true;
                }
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        // Change the resolve costs
        [HarmonyPatch( typeof( RelicLibrary ), "Instance", MethodType.Getter )]
        internal static class RelicLibrary_GetRelicData_Patch {
            private static bool runOnce = false;
            internal static void Postfix( RelicLibrary __result ) {
                if( !runOnce ) {
                    RelicTypeRelicDataDictionary m_relicLibrary = (RelicTypeRelicDataDictionary)Traverse.Create( __result ).Field( "m_relicLibrary" ).GetValue();
                    foreach( KeyValuePair<RelicType, RelicData> relic in m_relicLibrary ) {
                        if( keys.Exists( relic.Key ) && relic.Value.CostAmount != 0f ) {
                            if( relic.Value.CostAmount > 0f ) {
                                float resolve = WobSettings.Get( keys.Get( relic.Key, "ResolveCost" ), relic.Value.CostAmount );
                                WobPlugin.Log( "Changing resolve cost for " + relic.Key + " " + relic.Value.CostAmount + " -> " + resolve );
                                relic.Value.CostAmount = resolve;
                            } else {
                                float resolve = WobSettings.Get( keys.Get( relic.Key, "ResolveBonus" ), relic.Value.CostAmount );
                                WobPlugin.Log( "Changing resolve bonus for " + relic.Key + " " + -relic.Value.CostAmount + " -> " + -resolve );
                                relic.Value.CostAmount = resolve;
                            }
                        }
                    }
                    runOnce = true;
                }
            }
        }

        // Correct the total resolve cost calculation to prevent negative costs from reduced relic costs
        [HarmonyPatch( typeof( PlayerSaveData ), nameof( PlayerSaveData.GetTotalRelicResolveCost ) )]
        internal static class PlayerSaveData_GetTotalRelicResolveCost_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "PlayerSaveData.GetTotalRelicResolveCost Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc ), // num2
                            /*  1 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc ), // relicCostMod
                            /*  2 */ new WobTranspiler.OpTest( OpCodes.Sub     ), // num2 - relicCostMod
                            /*  3 */ new WobTranspiler.OpTest( OpCodeSet.Stloc ), // num2 = num2 - relicCostMod
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_Insert( 3, new List<CodeInstruction> {
                                new CodeInstruction( OpCodes.Ldc_R4, 0f ),
                                new CodeInstruction( OpCodes.Ldc_R4, float.MaxValue ),
                                new CodeInstruction( OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Mathf.Clamp( 0f, 0f, float.MaxValue ) ) ),
                            } ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        // Prevent adding the 'Used' variant of Aite's Sword, Hyperion's Ring, and Skeleton Key
        [HarmonyPatch( typeof( RelicObj ), nameof( RelicObj.SetLevel ) )]
        internal static class RelicObj_SetLevel_Patch {
            internal static void Prefix( RelicObj __instance, ref int value ) {
                RelicType relicType = __instance.RelicType;
                if( ( relicType == RelicType.DamageNoHitChallengeUsed || relicType == RelicType.ExtraLifeUsed || relicType == RelicType.FreeFairyChestUsed ) && value > 0 ) {
                    if( WobSettings.Get( keys.Get( relicType, "RemoveOnBreak" ), false ) ) {
                        value = 0;
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

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

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

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
                            /*  0 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc                                   ), // num
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4, Relic_EV.ATTACK_COOLDOWN_DURATION ), // 2f
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc                                   ), // level
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Conv_R4                                   ), // (float)level
                            /*  4 */ new WobTranspiler.OpTest( OpCodes.Mul                                       ), // 2f * (float)level
                            /*  5 */ new WobTranspiler.OpTest( OpCodes.Add                                       ), // num + 2f * (float)level
                            /*  6 */ new WobTranspiler.OpTest( OpCodeSet.Stloc                                   ), // num = num + 2f * (float)level
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( keys.Get( RelicType.AttackCooldown, "Cooldown" ), Relic_EV.ATTACK_COOLDOWN_DURATION ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Attack cooldown of Heavy Stone Bargain on revolver
        [HarmonyPatch( typeof( PistolWeapon_Ability ), "ExitAnimExitDelay", MethodType.Getter )]
        internal static class PistolWeapon_Ability_ExitAnimExitDelay_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "PistolWeapon_Ability.ExitAnimExitDelay Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, RelicType.AttackCooldown ), // RelicType.AttackCooldown
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "GetRelic"       ), // GetRelic(RelicType.AttackCooldown)
                            /*  2 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_Level"      ), // GetRelic(RelicType.AttackCooldown).Level
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Conv_R4                          ), // (float)GetRelic(RelicType.AttackCooldown).Level
                            /*  4 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4                           ), // 0.125f
                            /*  5 */ new WobTranspiler.OpTest( OpCodes.Mul                              ), // (float)GetRelic(RelicType.AttackCooldown).Level * 0.125f
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 4, WobSettings.Get( keys.Get( RelicType.AttackCooldown, "CooldownGun" ), Relic_EV.ATTACK_COOLDOWN_PISTOL_EXIT_DELAY_ADD ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Move slow of Heavy Stone Bargain on Hephaestus' Hammer
        [HarmonyPatch( typeof( AxeSpinner_Ability ), "OnEnterAttackLogic" )]
        internal static class AxeSpinner_Ability_OnEnterAttackLogic_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "AxeSpinner_Ability.OnEnterAttackLogic Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_MovementSpeedMod" ), // MovementSpeedMod
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4                                 ), // -0.3f
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc                                ), // level
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Conv_R4                                ), // (float)level
                            /*  4 */ new WobTranspiler.OpTest( OpCodes.Mul                                    ), // -0.3f * (float)level
                            /*  5 */ new WobTranspiler.OpTest( OpCodes.Add                                    ), // MovementSpeedMod + -0.3f * (float)level
                            /*  6 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "set_MovementSpeedMod" ), // MovementSpeedMod = MovementSpeedMod + -0.3f * (float)level
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( keys.Get( RelicType.AttackCooldown, "SlowHammer" ), Relic_EV.ATTACK_COOLDOWN_AXE_SPINNER_MOD ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }
        // Move slow of Heavy Stone Bargain on Hephaestus' Hammer
        [HarmonyPatch( typeof( AxeSpinner_Ability ), "StopAbility" )]
        internal static class AxeSpinner_Ability_StopAbility_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "AxeSpinner_Ability.StopAbility Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_MovementSpeedMod" ), // MovementSpeedMod
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4                                 ), // -0.3f
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc                                ), // level
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Conv_R4                                ), // (float)level
                            /*  4 */ new WobTranspiler.OpTest( OpCodes.Mul                                    ), // -0.3f * (float)level
                            /*  5 */ new WobTranspiler.OpTest( OpCodes.Sub                                    ), // MovementSpeedMod - -0.3f * (float)level
                            /*  6 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "set_MovementSpeedMod" ), // MovementSpeedMod = MovementSpeedMod - -0.3f * (float)level
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( keys.Get( RelicType.AttackCooldown, "SlowHammer" ), Relic_EV.ATTACK_COOLDOWN_AXE_SPINNER_MOD ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

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
                            new WobTranspiler.OpAction_SetOperand( 0, WobSettings.Get( keys.Get( RelicType.FlightBonusCurse, "DamageTaken" ), Relic_EV.FLIGHT_BONUS_CURSE_DAMAGE_MOD ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

    }
}