using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MoreMountains.CorgiEngine;
using UnityEngine;
using Wob_Common;

namespace WobMod {
    internal class Relics {

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTING DEFAULTS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        internal record RelicInfo( string Config, string Name, string Description, bool Spawn, int Resolve, int Stack );
        internal static readonly Dictionary<RelicType, RelicInfo> relicInfo = new() {
            // Standard stackable relics
            { RelicType.MaxHealthStatBonus,         new( "AchillesShield",          "Achilles' Shield",           "While ABOVE 50% Health, deal 10% more Spell and Weapon damage.",                                    true,   25, 5  ) }, // +10% dmg
            { RelicType.DoubleJumpRelic,            new( "AethersGrace",            "Aether's Grace",             "Gain 1 additional Double Jump.",                                                                    true,   35, 5  ) }, // +1 jump
            { RelicType.WeaponsBurnAdd,             new( "AmaterasusSun",           "Amaterasu's Sun",            "Your Weapon applies (or extends) Burn for 2 seconds.",                                              true,   55, 5  ) }, // +2s
            { RelicType.ExtendInvuln,               new( "Ambrosia",                "Ambrosia",                   "After taking damage you are Invincible for an additional 1.25 seconds.",                            true,   25, 5  ) }, // +1.25s
            { RelicType.DashRelic,                  new( "AnankesGrace",            "Ananke's Grace",             "Gain 1 additional Air Dash.",                                                                       true,   35, 5  ) }, // +1 dash
            { RelicType.MagicDamageEnemyCount,      new( "Antikythera",             "Antikythera",                "Gain 10% more Magic Damage per enemy defeated. Bonus lost when hit (max 50%).",                     true,   35, 5  ) }, // +50% max
            { RelicType.FreeCastSpell,              new( "ArcaneNecklace",          "Arcane Necklace",            "Cast 3 Spells to charge the necklace, and make the next cast free.",                                true,   25, 3  ) }, // -1 spell
            { RelicType.AllCritDamageUp,            new( "AtroposScissors",         "Atropos' Scissors",          "Critical damage from Spells and Weapons increased by 20%.",                                         true,   25, 5  ) }, // +20%
            { RelicType.ManaShield,                 new( "AzureAegis",              "Azure Aegis",                "10% of incoming damage is absorbed by your Mana.",                                                  true,   35, 5  ) }, // +10%
            { RelicType.EnemiesDropMeat,            new( "BodyBuffet",              "Body Buffet",                "Defeated enemies have a 8% chance of dropping a Health Drop.",                                      true,   35, 5  ) }, // +8%
            { RelicType.WeaponsComboAdd,            new( "BoxingBell",              "Boxing Bell",                "Your Weapon applies Combo.",                                                                        true,   45, 5  ) }, // +1 stack
            { RelicType.DamageBuffStatusEffect,     new( "Catalyst",                "Catalyst",                   "Deal 20% bonus damage to enemies with a Status Effect.",                                            true,   25, 5  ) }, // +20%
            { RelicType.ImmuneStill,                new( "CloakOfEventide",         "Cloak of Eventide",          "Standing still for 1.2 second(s) grants you Cloak.",                                                true,   45, 5  ) }, // -0.2s
            { RelicType.AllCritChanceUp,            new( "ClothosSpindle",          "Clotho's Spindle",           "Critical chance for Spells and Weapons increased by 10%.",                                          true,   25, 5  ) }, // +10%
            { RelicType.FreeHitRegenerate,          new( "CoeusShell",              "Coeus' Shell",               "Defeat 6 enemies to prevent the next source of damage.",                                            true,   45, 5  ) }, // -1 kill
            { RelicType.FoodHealsMore,              new( "Cornucopia",              "Cornucopia",                 "Health Drops restore an additional 8% of your Max Health.",                                         true,   25, 5  ) }, // +8%
            { RelicType.ProjectileDashStart,        new( "CorruptingReagent",       "Corrupting Reagent",         "Dashing leaves a poisonous cloud behind for 1 second.",                                             true,   35, 5  ) }, // +1s
            { RelicType.ManaRestoreOnHurt,          new( "CosmicInsight",           "Cosmic Insight",             "Gain 100 Mana when hurt. Mana gained can Overcharge.",                                              true,   25, 5  ) }, // +100 mana
            { RelicType.SpellKillMaxMana,           new( "Dreamcatcher",            "Dreamcatcher",               "Defeat enemies while at Max HP to gain 5 Max Mana (Max 200).",                                      true,   35, 5  ) }, // +200 max
            { RelicType.ReplacementRelic,           new( "EmptyVessel",             "Empty Vessel",               "Gain 10% bonus Health, Weapon, and Magic Damage.",                                                  false,  25, 5  ) }, // +10%
            { RelicType.FreeEnemyKill,              new( "FatesDie",                "Fate's Die",                 "Defeat 6 enemies to load the die and instantly defeat the next enemy.",                             true,   25, 5  ) }, // -1 kill
            { RelicType.TalentCharge,               new( "FeatheredCap",            "Feathered Cap",              "Casting your Talent grants you Charged for 1.5 seconds.",                                           true,   35, 5  ) }, // +1.5s
            { RelicType.ChestHealthRestore,         new( "FreonsReward",            "Freon's Reward",             "Opening chests restores Health (120% of INT.).",                                                    true,   25, 5  ) }, // +120% INT
            { RelicType.BonusDamageOnNextHit,       new( "GlowingEmber",            "Glowing Ember",              "Deal 75% more damage on every 6th hit. Counter resets if you take damage.",                         true,   25, 5  ) }, // +75% dmg
            { RelicType.MeatMaxHealth,              new( "GnawedBone",              "Gnawed Bone",                "Eating Health Drops while at Full Health increases Max HP by 10% (Max 3 stacks).",                  true,   25, 5  ) }, // +3 max
            { RelicType.FatalBlowDodge,             new( "GraveBell",               "Grave Bell",                 "You have a 25% chance of avoiding a Fatal Blow.",                                                   true,   35, 3  ) }, // +25%
            { RelicType.LowHealthStatBonus,         new( "HectorsHelm",             "Hector's Helm",              "While BELOW 50% Health, deal 20% more Spell and Weapon damage.",                                    true,   25, 5  ) }, // +20% dmg
            { RelicType.LowResolveMagicDamage,      new( "HeronsRing",              "Heron's Ring",               "For every point of Resolve below 125%, deal an additional 1% bonus Magic damage.",                  true,   35, 5  ) }, // +1% dmg
            { RelicType.RangeDamageBonusCurse,      new( "IncandescentTelescope",   "Incandescent Telescope",     "Deal 12.5% more damage to enemies far away.",                                                       true,   25, 5  ) }, // +12.5% dmg
            { RelicType.GroundDamageBonus,          new( "IvyRoots",                "Ivy Roots",                  "Deal 12.5% bonus damage while on the ground.",                                                      true,   25, 5  ) }, // +12.5% dmg
            { RelicType.CritKillsHeal,              new( "LachesisMeasure",         "Lachesis' Measure",          "Enemies defeated with a critical hit restore your Health (50% of INT).",                            true,   55, 5  ) }, // +50%
            { RelicType.SkillCritBonus,             new( "LamechsWhetstone",        "Lamech's Whetstone",         "Weapon Skill Crits now apply Magic Break for 3 seconds.",                                           true,   35, 5  ) }, // +3s
            { RelicType.BonusMana,                  new( "LotusPetal",              "Lotus Petal",                "Increases your total Mana Pool by 50. Deal 8% more Magic Damage.",                                  true,   25, 5  ) }, // +50 mana, +8% dmg
            { RelicType.ManaDamageReduction,        new( "LotusStem",               "Lotus Stem",                 "Blocks up to 2 attacks. Consumes 150 Mana per block. Mana potions restore charges.",                true,   45, 5  ) }, // +2 blocks
            { RelicType.CurseRandomRelics,          new( "MammonsBounty",           "Mammon's Bounty",            "Gain 3 random Relic(s) and 30 Bonus Resolve per Mammon's Bounty.",                                  true,   0,  5  ) },
            { RelicType.LandShockwave,              new( "MarbleStatue",            "Marble Statue",              "Landing creates a small shockwave that destroys Mid-sized Projectiles and deals 75% Magic damage.", true,   25, 5  ) }, // +75% dmg
            { RelicType.FreeRerolls,                new( "MonkeysPaw",              "Monkey's Paw",               "Gain 5 bonus Re-roll(s) per Monkey's Paw, and disables Room Re-roll Cap.",                          true,   25, 5  ) }, // +5 rerolls
            { RelicType.SuperCritChanceUp,          new( "Obelisk",                 "Obelisk",                    "Your Skill Crits have an extra 20% chance of becoming Super Crits.",                                true,   25, 5  ) }, // +20%
            { RelicType.InvulnDamageBuff,           new( "RageTincture",            "Rage Tincture",              "After taking damage, deal 100% more damage during the Invincibility window.",                       true,   25, 5  ) }, // +100% dmg
            { RelicType.LowResolveWeaponDamage,     new( "RavensRing",              "Raven's Ring",               "For every point of Resolve below 125%, deal an additional 1% bonus Weapon damage.",                 true,   35, 5  ) }, // +1% dmg
            { RelicType.NoAttackDamageBonus,        new( "RedSandHourglass",        "Red Sand Hourglass",         "Every 5 seconds your next Weapon attack deals 75% bonus damage.",                                   true,   25, 5  ) }, // -1s
            { RelicType.WeaponsPoisonAdd,           new( "SerqetsStinger",          "Serqet's Stinger",           "Your Weapon applies 1 stack of Poison.",                                                            true,   55, 5  ) }, // +1 stack
            { RelicType.DanceStacks,                new( "SilkSlippers",            "Silk Slippers",              "Your Spin Kicks grant you 1 stack(s) of Dance.",                                                    true,   35, 5  ) }, // +1 stack
            { RelicType.OnHitAreaDamage,            new( "SoulTether",              "Soul Tether",                "Every 5 seconds, your next Weapon attack deals 150% bonus Magic damage to all nearby enemies.",     true,   45, 5  ) }, // -1s, +75% dmg
            { RelicType.TalentDamageBolt,           new( "StarSling",               "Star Sling",                 "Casting your Talent or Spell will fire 2 Star Bolt(s).",                                            true,   25, 6  ) }, // +2 bolts
            { RelicType.CurseHazard,                new( "TatteredRope",            "Tattered Rope",              "Gain 50 Resolve per Tattered Rope. Take 160% increased damage from Hazards.",                       true,   0,  5  ) }, // +20 resolve, +160% dmg
            { RelicType.ContrarianCurse,            new( "Transmogrificator",       "Transmogrificator",          "Randomize your Weapon, Spell and Talent. Deal 7% more damage, and gain 20 bonus Resolve.",          true,   0,  99 ) }, // +7% dmg, +20 resolve
            { RelicType.DashStrikeDamageUp,         new( "VanguardsBanner",         "Vanguard's Banner",          "Dashing creates a wave that destroys Mid-sized Projectiles. Waves travel 2 units.",                 true,   45, 5  ) }, // +2 units
            { RelicType.DamageAuraOnHit,            new( "VoltaicCirclet",          "Voltaic Circlet",            "Hitting an enemy with your Weapon will generate a damage aura around you for 1.5 seconds.",         true,   35, 5  ) }, // +1.5s
            { RelicType.SpinKickDamageBonus,        new( "WeightedAnklet",          "Weighted Anklet",            "Your Spin Kicks deal 60% more damage.",                                                             true,   25, 5  ) }, // +40%
            { RelicType.ManaBurn,                   new( "WhiteBeard",              "White Beard",                "Attacks apply Mana Leech for 2 second(s).",                                                         true,   45, 5  ) }, // +2s
            { RelicType.MaxManaDamage,              new( "ZealotsRing",             "Zealot's Ring",              "Spells cast while at Max MP deal an additional 25% damage.",                                        true,   25, 5  ) }, // +25% dmg
            // Non-stackable relics
            { RelicType.NoGoldXPBonus,              new( "DiogenesBargain",         "Diogenes' Bargain",          "No more gold. All Gold Bonuses are converted into an XP Bonus (X%) instead.",                       true,   15, 1  ) },
            { RelicType.GoldDeathCurse,             new( "FutureSuccessorsBargain", "Future Successor's Bargain", "Die. Increase your current gold by 35%.",                                                           true,   15, 1  ) },
            { RelicType.AttackCooldown,             new( "HeavyStoneBargain",       "Heavy Stone Bargain",        "Your Weapon deals 100% more damage, but has a 1.25 second cooldown.",                               true,   15, 1  ) },
            { RelicType.NoSpikeDamage,              new( "HermesBoots",             "Hermes' Boots",              "You are Immune to static spikes.",                                                                  true,   25, 1  ) },
            { RelicType.FlightBonusCurse,           new( "IcarusWingsBargain",      "Icarus' Wings Bargain",      "Jumping in the air enables Flight, but you take 75% extra damage.",                                 true,   15, 1  ) },
            { RelicType.PlatformOnAerial,           new( "IvySeed",                 "Ivy Seed",                   "Create an Ivy Canopy every time you do an Aerial Recovery.",                                        true,   25, 1  ) },
            { RelicType.RemoveVisuals,              new( "NerdyGlasses",            "Nerdy Glasses",              "Removes all Visual Ailments.",                                                                      true,   45, 1  ) },
            { RelicType.BonusDamageCurse,           new( "SerratedHandlesBargain",  "Serrated Handle's Bargain",  "Deal and take 100% more damage.",                                                                   true,   15, 1  ) },
            { RelicType.SpinKickArmorBreak,         new( "Steel-ToedBoots",         "Steel-Toed Boots",           "Spin Kicks now apply Armor Break for 3.5 seconds.",                                                 true,   35, 1  ) },
            { RelicType.RelicAmountDamageUp,        new( "WarDrum",                 "War Drum",                   "Every unique Relic increases your damage by 4%.",                                                   true,   45, 1  ) },
            { RelicType.SporeburstKillAdd,          new( "WeirdMushrooms",          "Weird Mushrooms",            "Defeated enemies have Spore Burst applied to them.",                                                true,   35, 1  ) },
            // Relics that purify
            { RelicType.GoldCombatChallenge,        new( "CharonsTrial",            "Charon's Trial",             "Defeat 15 enemies to purify it and gain a 20% Gold, Ore, and Aether bonus.",                        true,   0,  1  ) },
            { RelicType.GoldCombatChallengeUsed,    new( "CharonsTrial",            "Charon's Reward",            "Defeat 15 enemies to purify it and gain a 20% Gold, Ore, and Aether bonus.",                        false,  0,  1  ) },
            { RelicType.FoodChallenge,              new( "DemetersTrial",           "Demeter's Trial",            "Collect 1 food/potion to purify this Relic and gain 30% Max Health and 50 Mana.",                   true,   0,  1  ) },
            { RelicType.FoodChallengeUsed,          new( "DemetersTrial",           "Demeter's Reward",           "Collect 1 food/potion to purify this Relic and gain 30% Max Health and 50 Mana.",                   false,  0,  1  ) },
            { RelicType.ResolveCombatChallenge,     new( "PandorasTrial",           "Pandora's Trial",            "Defeat 10 enemies to purify this relic and gain 50 Resolve.",                                       true,   0,  1  ) },
            { RelicType.ResolveCombatChallengeUsed, new( "PandorasTrial",           "Pandora's Reward",           "Defeat 10 enemies to purify this relic and gain 50 Resolve.",                                       false, -50, 1  ) },
            // Relics that break
            { RelicType.DamageNoHitChallenge,       new( "AitesSword",              "Aite's Sword",               "You deal 150% more damage, but this Relic is fragile.",                                             true,   35, 1  ) },
            { RelicType.DamageNoHitChallengeUsed,   new( "AitesSword",              "Aite's Broken Sword",        "You deal 150% more damage, but this Relic is fragile.",                                             false,  35, 1  ) },
            { RelicType.ExtraLife_Unity,            new( "AncestralSoul",           "Ancestral Soul",             "Revive from fatal blows and regain 50% of your HP.",                                                false,  0,  99 ) }, // Given by Kin 100 unity
            { RelicType.ExtraLife_UnityUsed,        new( "AncestralSoul",           "Ancestral Dust",             "Revive from fatal blows and regain 50% of your HP.",                                                false,  0,  99 ) },
            { RelicType.ExtraLife,                  new( "HyperionsRing",           "Hyperion's Ring",            "Revive from fatal blows and regain 50% of your HP.",                                                true,   55, 5  ) }, // +1 use
            { RelicType.ExtraLifeUsed,              new( "HyperionsRing",           "Hyperion's Broken Ring",     "Revive from fatal blows and regain 50% of your HP.",                                                false,  55, 99 ) },
            { RelicType.FreeFairyChest,             new( "SkeletonKey",             "Skeleton Key",               "Open locked or melted Fairy Chests. Breaks after use.",                                             true,   35, 5  ) }, // +1 use
            { RelicType.FreeFairyChestUsed,         new( "SkeletonKey",             "Broken Key",                 "Open locked or melted Fairy Chests. Breaks after use.",                                             false,  35, 5  ) },
            // Relics required for accessing Estuaries
            { RelicType.Lily1,                      new( "LilyOfTheValley",         "Lily of the Valley",         "A lily of the valley. (For access to Estuary Namaah)",                                              false,  10, 1  ) },
            { RelicType.Lily2,                      new( "LilyOfTheValley",         "Lily of the Valley",         "A lily of the valley. (For access to Estuary Namaah)",                                              false,  10, 1  ) },
            { RelicType.Lily3,                      new( "LilyOfTheValley",         "Lily of the Valley",         "A lily of the valley. (For access to Estuary Namaah)",                                              false,  10, 1  ) },
            { RelicType.DragonKeyBlack,             new( "DragonKey",               "Onyx Key/Pearl Key",         "A black/white key. (For access to Pishon Dry Lake minibosses)",                                     false,  45, 5  ) },
            { RelicType.DragonKeyWhite,             new( "DragonKey",               "Onyx Key/Pearl Key",         "A black/white key. (For access to Pishon Dry Lake minibosses)",                                     false,  45, 5  ) },
            // Ability swap blessings
            { RelicType.WeaponSwap,                 new( "BlessingOfStrength",      "Blessing of Strength",       "Deal 7% more Weapon Damage.",                                                                       false,  0,  99 ) }, // +7% dmg
            { RelicType.SpellSwap,                  new( "BlessingOfWisdom",        "Blessing of Wisdom",         "Deal 7% more Magic Damage.",                                                                        false,  0,  99 ) }, // +7% dmg
            { RelicType.TalentSwap,                 new( "BlessingOfTalent",        "Blessing of Talent",         "Gain 20 Resolve.",                                                                                  false, -20, 99 ) }, // +20 resolve
        };

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTINGS READ/WRITE
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        internal static readonly WobSettings.KeyHelper<RelicType> relicKeys = new( "Relic" );

        internal static void RunSetup() {
            WobMod.configFiles.Add( "Relics", "Relics" );
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Num<float>( WobMod.configFiles.Get( "Relics" ), "TwinRelics",   "SpawnChance",    "Set the chance to spawn each relic as a twin relic to this percent",                           10f, 0.01f, bounds: (0f, 100f   ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Relics" ), "TwinRelics",   "MinResolve",     "Always spawn twin relics if you have over this amount of current resolve (use -1 to disable)", -1,  0.01f, bounds: (-1, 1000000) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Relics" ), "Resolve",      "ResolveMinimum", "Resolve minimum percent",                                                                      0,   0.01f, bounds: (0, 1000)     ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Relics" ), "Resolve",      "ResolveBonus",   "Resolve bonus percent",                                                                        0,   0.01f, bounds: (0, 1000)     ),
            } );
            foreach( RelicType relicType in relicInfo.Keys ) {
                relicKeys.Add( relicType, relicInfo[relicType].Config );
                if( !WobSettings.Exists( relicKeys.Get( relicType, "Enabled" ) ) && relicInfo[relicType].Spawn ) {
                    WobSettings.Add( new WobSettings.Boolean( WobMod.configFiles.Get( "Relics" ), relicKeys.Get( relicType, "Enabled" ), "Enable random spawn for " + relicInfo[relicType].Name + " - " + relicInfo[relicType].Description, relicInfo[relicType].Spawn ) );
                }
                if( relicInfo[relicType].Resolve < 0 ) {
                    if( !WobSettings.Exists( relicKeys.Get( relicType, "ResolveBonus" ) ) ) {
                        WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "Relics" ), relicKeys.Get( relicType, "ResolveBonus" ), "Resolve bonus percent for " + relicInfo[relicType].Name + " - " + relicInfo[relicType].Description, -relicInfo[relicType].Resolve, -0.01f, bounds: (0, 1000) ) );
                    }
                } else {
                    if( !WobSettings.Exists( relicKeys.Get( relicType, "ResolveCost" ) ) && ( relicInfo[relicType].Spawn || relicInfo[relicType].Resolve > 0 ) ) {
                        WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "Relics" ), relicKeys.Get( relicType, "ResolveCost" ), "Resolve cost percent for " + relicInfo[relicType].Name + " - " + relicInfo[relicType].Description, relicInfo[relicType].Resolve, 0.01f, bounds: (0, 1000) ) );
                    }
                }
            }
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Relics" ), relicKeys.Get( RelicType.DamageNoHitChallenge, "ResolveBroken" ), "Resolve cost percent after breaking for " + relicInfo[RelicType.DamageNoHitChallenge].Name + " - " + relicInfo[RelicType.DamageNoHitChallenge].Description,                    relicInfo[RelicType.DamageNoHitChallenge].Resolve, 0.01f, bounds: (0,  1000    ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Relics" ), relicKeys.Get( RelicType.ExtraLife,            "ResolveBroken" ), "Resolve cost percent after breaking for " + relicInfo[RelicType.ExtraLife].Name + " - " + relicInfo[RelicType.ExtraLife].Description,                                          relicInfo[RelicType.ExtraLife].Resolve,            0.01f, bounds: (0,  1000    ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Relics" ), relicKeys.Get( RelicType.FreeFairyChest,       "ResolveBroken" ), "Resolve cost percent after breaking for " + relicInfo[RelicType.FreeFairyChest].Name + " - " + relicInfo[RelicType.FreeFairyChest].Description,                                relicInfo[RelicType.FreeFairyChest].Resolve,       0.01f, bounds: (0,  1000    ) ),
                new WobSettings.Boolean(    WobMod.configFiles.Get( "Relics" ), relicKeys.Get( RelicType.FreeFairyChest,       "InfiniteUses"  ), "Infinite uses without breaking for " + relicInfo[RelicType.FreeFairyChest].Name + " - " + relicInfo[RelicType.FreeFairyChest].Description,                                     false                                                                            ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "Relics" ), relicKeys.Get( RelicType.AttackCooldown,       "Cooldown"      ), "Set the attack cooldown to this number of seconds for " + relicInfo[RelicType.AttackCooldown].Name + " - " + relicInfo[RelicType.AttackCooldown].Description,                  1.25f,                                                    bounds: (0f, 1000000f) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "Relics" ), relicKeys.Get( RelicType.AttackCooldown,       "CooldownGun"   ), "Set the attack delay multiplier on the revolver to this number for " + relicInfo[RelicType.AttackCooldown].Name + " - " + relicInfo[RelicType.AttackCooldown].Description,     0.1f,                                                     bounds: (0f, 1000000f) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "Relics" ), relicKeys.Get( RelicType.AttackCooldown,       "CooldownCrow"  ), "Set the attack tick rate addition on the crow storm to this number for " + relicInfo[RelicType.AttackCooldown].Name + " - " + relicInfo[RelicType.AttackCooldown].Description, 0.5f,                                                     bounds: (0f, 1000000f) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Relics" ), relicKeys.Get( RelicType.AttackCooldown,       "SlowHammer"    ), "Set the move speed penalty on the hammer to this percent for " + relicInfo[RelicType.AttackCooldown].Name + " - " + relicInfo[RelicType.AttackCooldown].Description,           30,                                               -0.01f, bounds: (0,  1000000 ) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "Relics" ), relicKeys.Get( RelicType.FlightBonusCurse,     "DamageTaken"   ), "Set the additional damage taken to this percent for " + relicInfo[RelicType.FlightBonusCurse].Name + " - " + relicInfo[RelicType.FlightBonusCurse].Description,                75f,                                               0.01f, bounds: (0f, 1000000f) ),
                new WobSettings.Boolean(    WobMod.configFiles.Get( "Relics" ), relicKeys.Get( RelicType.FlightBonusCurse,     "FlightButton"  ), "Use the flight assist button rather than air jump to activate " + relicInfo[RelicType.FlightBonusCurse].Name + " - " + relicInfo[RelicType.FlightBonusCurse].Description,      false                                                                            ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "Relics" ), relicKeys.Get( RelicType.RemoveVisuals,        "SpawnChance"   ), "Set the chance to spawn in each relic room to this percent for " + relicInfo[RelicType.RemoveVisuals].Name + " - " + relicInfo[RelicType.RemoveVisuals].Description,           12.5f,                                             0.01f, bounds: (0f, 100f    ) ),
            } );
            RelicRoomPropController_RollRelics_Patch.removeVisualsChance = WobSettings.Get( relicKeys.Get( RelicType.RemoveVisuals, "SpawnChance" ), Relic_EV.REMOVE_VISUALS_INCREASED_ODDS_CHANCE );
            RelicRoomPropController_RollRelics_Patch.twinRelicChance = WobSettings.Get( "TwinRelics", "SpawnChance", 0.1f );
            RelicRoomPropController_RollRelics_Patch.twinRelicResolve = WobSettings.Get( "TwinRelics", "MinResolve", -1f );
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - RELIC ROLLS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        [HarmonyPatch( typeof( RelicRoomPropController ), nameof( RelicRoomPropController.RollRelics ) )]
        internal static class RelicRoomPropController_RollRelics_Patch {
            internal static float removeVisualsChance = Relic_EV.REMOVE_VISUALS_INCREASED_ODDS_CHANCE;
            internal static float twinRelicChance = 0.1f;
            internal static float twinRelicResolve = -1f;

            private record RelicRoll( RelicType RelicType, RelicModType RelicModType );

			internal static bool Prefix( RelicRoomPropController __instance, int numRolls, bool addToTotalRoomRolls, bool rollMods ) {
                Coroutine m_leftTwinSpinCoroutine = Traverse.Create( __instance ).Field( "m_leftTwinSpinCoroutine" ).GetValue<Coroutine>();
                Coroutine m_rightTwinSpinCoroutine = Traverse.Create( __instance ).Field( "m_rightTwinSpinCoroutine" ).GetValue<Coroutine>();
                List<RelicType> m_exclusionList = Traverse.Create( typeof( RelicRoomPropController ) ).Field( "m_exclusionList" ).GetValue<List<RelicType>>();
                if( m_leftTwinSpinCoroutine != null ) {
                    __instance.StopCoroutine( m_leftTwinSpinCoroutine );
                }
                if( m_rightTwinSpinCoroutine != null ) {
                    __instance.StopCoroutine( m_rightTwinSpinCoroutine );
                }
                if( addToTotalRoomRolls ) {
                    Traverse<int> m_totalRoomRolls = Traverse.Create( __instance ).Field<int>( "m_totalRoomRolls" );
                    m_totalRoomRolls.Value += numRolls;
                    Traverse.Create( __instance ).Method( "SetRoomMiscData", new System.Type[] { typeof( string ), typeof( string ) } ).GetValue( new object[] { "TotalRelicRolls", m_totalRoomRolls.Value.ToString() } );
                }
                Prop component = __instance.GetComponent<Prop>();
                RelicPropTypeOverride relicPropTypeOverride = null;
                if( component && component.PropSpawnController ) {
                    relicPropTypeOverride = component.PropSpawnController.gameObject.GetComponent<RelicPropTypeOverride>();
                }
				RelicRoll relic1 = new( RelicType.None, RelicModType.None );
				RelicRoll relic2 = new( RelicType.None, RelicModType.None );
                for( int i = 0; i < numRolls; i++ ) {
                    RngID rngIDToUse = GameUtility.IsInLevelEditor ? RngID.None : RngID.SpecialProps_RoomSeed;
                    CheckExclusionList( __instance, m_exclusionList );
                    relic1 = RollRelic( ( Global_EV.RELIC_ROOM_TEST_RELICS.x != 0 ? (RelicType)Global_EV.RELIC_ROOM_TEST_RELICS.x : ( relicPropTypeOverride?.Relic1Override ?? RelicType.None ) ), RelicType.None, m_exclusionList, rngIDToUse, false, true, rollMods );
                    relic2 = RollRelic( ( Global_EV.RELIC_ROOM_TEST_RELICS.y != 0 ? (RelicType)Global_EV.RELIC_ROOM_TEST_RELICS.y : ( relicPropTypeOverride?.Relic2Override ?? RelicType.None ) ), relic1.RelicType, m_exclusionList, rngIDToUse, true, __instance.RightIcon.gameObject.activeSelf, rollMods );
                }
                Traverse.Create( __instance ).Field( "m_relicTypes" ).SetValue( new Vector2Int( (int)relic1.RelicType, (int)relic2.RelicType ) );
                Traverse.Create( __instance ).Field( "m_relicModTypes" ).SetValue( new Vector2Int( (int)relic1.RelicModType, (int)relic2.RelicModType ) );
                __instance.InitializeTextBox( true );
                __instance.InitializeTextBox( false );
                // Prevent the original method from running
                return false;
            }

            // These relics are never allowed to be twin relics, even if they pass the normal checks
            private static readonly HashSet<RelicType> noTwinRelics = new() { RelicType.None, RelicType.ReplacementRelic, RelicType.ContrarianCurse, RelicType.RemoveVisuals };

            // Method to roll a random relic and set whether it is a twin relic
            private static RelicRoll RollRelic( RelicType overrideRelic, RelicType otherRelic, List<RelicType> exclusionList, RngID rngIDToUse, bool isRight, bool isRightActive, bool rollMods ) {
                // Check if there is a right relic - if not, always use the Empty Vessel
                if( isRight && !isRightActive ) {
                    WobPlugin.Log( "[Relics] RollRelic - single relic room, skipping right relic" );
                    // Return the Empty Vessel, which is never a twin
                    return new( RelicType.ReplacementRelic, RelicModType.None );
                }
                // Check if this is a Kerguelen Lily room, and if the door has been opened
                if( ( overrideRelic == RelicType.Lily1 || overrideRelic == RelicType.Lily2 || overrideRelic == RelicType.Lily3 ) && SaveManager.PlayerSaveData.GetInsightState( InsightType.ForestBoss_DoorOpened ) < InsightState.ResolvedButNotViewed ) {
                    WobPlugin.Log( "[Relics] RollRelic - override applied: " + overrideRelic );
                    // Return the lily, which is never a twin
                    return new( overrideRelic, RelicModType.None );
                }
                // Check if this is a Pishon Key room, and if the door has been opened
                if( ( overrideRelic == RelicType.DragonKeyWhite && !SaveManager.PlayerSaveData.GetFlag( PlayerSaveFlag.CaveMiniboss_WhiteDoor_Opened ) ) || ( overrideRelic == RelicType.DragonKeyBlack && !SaveManager.PlayerSaveData.GetFlag( PlayerSaveFlag.CaveMiniboss_BlackDoor_Opened ) ) ) {
                    WobPlugin.Log( "[Relics] RollRelic - override applied: " + overrideRelic );
                    // Return the key, which is never a twin
                    return new( overrideRelic, RelicModType.None );
                }
                // Check for the Nerdy Glasses override: can only be on the left side, only if you don't already have it, only if you have a visual affecting trait, and only if randomly rolled to the special odds
                if( !isRight && SaveManager.PlayerSaveData.GetRelic( RelicType.RemoveVisuals ).Level <= 0 && TraitManager.ActiveTraitTypeList.Intersect( Relic_EV.REMOVE_VISUALS_TRAIT_ARRAY ).Count() > 0
                        && RNGManager.GetRandomNumber( RngID.SpecialProps_RoomSeed, "RelicRoomPropController.RollRemoveVisualsOdds", 0f, 1f ) <= removeVisualsChance ) {
                    WobPlugin.Log( "[Relics] RollRelic - Nerdy Glasses rolled" );
                    // Return the Nerdy Glasses, which is never a twin
                    return new( RelicType.RemoveVisuals, RelicModType.None );
                }
				// Otherwise, roll a random relic
				// Variables for the random relic to be returned from this method, and whether it is a twin relic
				RelicType relicType = RelicType.None;
                RelicModType relicModType = RelicModType.None;
                // Variable to track the number of rolls to ensure we don't get trapped in an infinite loop
                int rollAttempts = 0;
                // Roll until a unique relic is found, or we've tried all relics
                while( ( relicType == otherRelic || relicType == RelicType.ReplacementRelic || relicType == RelicType.None ) && rollAttempts < RelicType_RL.TypeArray.Length ) {
                    // Use the game's method to get a relic - note that the second parameter is different to the original method, so that it won't return relics that you already have the maximum stack for
                    relicType = RelicLibrary.GetRandomRelic( rngIDToUse, true, exclusionList );
                    // Increment the attempts for escaping an infinite loop
                    rollAttempts++;
                }
                // Check if we gave up after an infinite loop or if we somehow have not set a relic
                if( rollAttempts >= RelicType_RL.TypeArray.Length || relicType == RelicType.None ) {
                    WobPlugin.Log( "[Relics] RollRelic - WARNING: relic could not be rolled", WobPlugin.ERROR );
                    // Return the Empty Vessel, which is never a twin
                    return new( RelicType.ReplacementRelic, RelicModType.None );
                }
                // A random relic has been rolled
                // Add the relic to the exclusion list for future rolls
                exclusionList.Add( relicType );
                // Check if the room allows twin relics and that the relic is not one of the specific exceptions that doesn't allow twin relics
                if( rollMods && !noTwinRelics.Contains( relicType ) ) {
                    // Look up the relic base data so we can check its max stack size
                    RelicData relicData = RelicLibrary.GetRelicData( relicType );
                    // Null check, then check if more than one of the relic can be added to the current character without exceeding the max stack
                    if( relicData && relicData.MaxStack - SaveManager.PlayerSaveData.GetRelic( relicType ).Level > 1 ) {
                        // Normal checks for twin relics - character has Hoarder trait, or random roll
                        if( TraitManager.IsTraitActive( TraitType.TwinRelics ) || rngIDToUse == RngID.None || RNGManager.GetRandomNumber( rngIDToUse, "RelicRoomPropController.RollRelicMods()", 0f, 1f ) <= twinRelicChance ) {
							// Set the relic to be a twin
							relicModType = RelicModType.DoubleRelic;
                        } else {
                            // Normal game method would have returned as no twin, but the mod has an exception in the settings
                            // Read the character's current resolve
                            float currentResolve = PlayerManager.GetPlayerController().ActualResolve;
                            // Check that the setting is enabled (not -1) and compare the player resolve to the setting 
                            if( twinRelicResolve >= 0f && currentResolve >= twinRelicResolve ) {
								// Set the relic to be a twin
								relicModType = RelicModType.DoubleRelic;
                                WobPlugin.Log( "[Relics] Forcing twin relic - current resolve: " + currentResolve + ", config setting: " + twinRelicResolve );
                            }
                        }
                    }
                }
                WobPlugin.Log( "[Relics] RollRelic - random relic: " + relicType + ", " + relicModType );
                // Return the random relic
                return new( relicType, relicModType );
            }

            // Check the exclusion list to ensure that it is possible to roll relics, and reset the list if too many are excluded, to prevent game freeze if rerolling relics too many times
            private static void CheckExclusionList( RelicRoomPropController __instance, List<RelicType> exclusionList ) {
                // List for the relics that are able to be rolled
                List<RelicType> allowedTypes = new();
                //string allowedNames = "";
                // Loop through all relics in the game
                foreach( RelicType relicType in RelicType_RL.TypeArray ) {
                    // Look up the relic base data so we can check its max stack size
                    RelicData relicData = RelicLibrary.GetRelicData( relicType );
                    // Check if it can be used by the player, if it is currently excluded, and is not the special Empty Vessel
                    if( RelicLibrary.IsRelicAllowed( relicType ) && !exclusionList.Contains( relicType ) && relicType != RelicType.ReplacementRelic && relicData && SaveManager.PlayerSaveData.GetRelic( relicType ).Level < relicData.MaxStack ) {
                        // Add it to the list of rollable relics
                        allowedTypes.Add( relicType );
                        //allowedNames += relicType + ", ";
                    }
                }
                //WobPlugin.Log( "[Relics] Remaining relics: " + allowedNames );
                // Check that there are enough relics remaining in the pool to fill the current room
                if( allowedTypes.Count < ( __instance.RightIcon.gameObject.activeSelf ? 2 : 1 ) ) {
                    // Reset the exclusion list to how it was when the room was first created
                    // Empty the list
                    exclusionList.Clear();
                    // If in a challenge, add relics not allowed in challenges
                    if( ChallengeManager.IsInChallenge || __instance.Room.BiomeType == BiomeType.Garden ) {
                        exclusionList.AddRange( Challenge_EV.RELIC_EXCLUSION_ARRAY );
                    }
                    // If in one of the True Rogue, etc. modes, add any relics not allowed in that mode
                    if( SaveManager.PlayerSaveData.SpecialModeType != SpecialModeType.None ) {
                        foreach( Vector2Int vector2Int in SpecialMode_EV.SPECIAL_MODE_RELIC_EXCLUSION_ARRAY ) {
                            if( vector2Int.x == (int)SaveManager.PlayerSaveData.SpecialModeType ) {
                                exclusionList.Add( (RelicType)vector2Int.y );
                            }
                        }
                    }
                    // Add the Nerdy Glasses - this has a special override roll if relevant
                    exclusionList.Add( RelicType.RemoveVisuals );
                    // Check heirloom state for if related relics should be allowed
                    if( SaveManager.PlayerSaveData.DoubleJumpHeirloomUnlockedThisRun || SaveManager.PlayerSaveData.GetHeirloomLevel( HeirloomType.UnlockDoubleJump ) <= 0 ) {
                        exclusionList.Add( RelicType.DoubleJumpRelic );
                    }
                    // Check heirloom state for if related relics should be allowed
                    if( SaveManager.PlayerSaveData.AirDashHeirloomUnlockedThisRun || SaveManager.PlayerSaveData.GetHeirloomLevel( HeirloomType.UnlockAirDash ) <= 0 ) {
                        exclusionList.Add( RelicType.DashRelic );
                    }
                    WobPlugin.Log( "[Relics] Relic reroll list reset" );
                }
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - RELIC GENERAL VALUES
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Enable or disable a relic for random spawn and change the resolve costs
        [HarmonyPatch( typeof( RelicLibrary ), "Instance", MethodType.Getter )]
        internal static class RelicLibrary_Instance_Getter_Patch {
            private static bool runOnce = false;
            internal static void Postfix( RelicLibrary __result ) {
                if( !runOnce ) {
                    RelicTypeRelicDataDictionary m_relicLibrary = Traverse.Create( __result ).Field<RelicTypeRelicDataDictionary>( "m_relicLibrary" ).Value;
                    foreach( KeyValuePair<RelicType, RelicData> relic in m_relicLibrary ) {
                        WobPlugin.Log( "[Relics] " + relic.Key + ", " + relic.Value.Name + ", " + relicKeys.Exists( relic.Key ) + ", " + relic.Value.Rarity + ", " + relic.Value.CostAmount + ", " + relic.Value.MaxStack );
                        if( relicKeys.Exists( relic.Key ) ) {
                            // Check that the rarity is allowing spawn, and get the value of the setting that has the same name as the internal name of the relic
                            if( relic.Value.Rarity == 1 && !WobSettings.Get( relicKeys.Get( relic.Key, "Enabled" ), true ) ) {
                                // The game seems to use a value of 99 for the rarity of diabled relics, so I will stick to this convention
                                relic.Value.Rarity = 99;
                                WobPlugin.Log( "[Relics] Disabling spawn for " + relic.Value.Name );
                            }
                            if( relic.Key == RelicType.FreeFairyChest && WobSettings.Get( relicKeys.Get( RelicType.FreeFairyChest, "InfiniteUses" ), false ) ) {
                                relic.Value.MaxStack = 1;
                                WobPlugin.Log( "[Relics] Setting infinite uses for " + relic.Value.Name );
                            }
                            if( WobSettings.Exists( relicKeys.Get( relic.Key, "ResolveBonus" ) ) ) {
                                float resolve = WobSettings.Get( relicKeys.Get( relic.Key, "ResolveBonus" ), relic.Value.CostAmount );
                                WobPlugin.Log( "[Relics] Changing resolve bonus for " + relic.Value.Name + " " + -relic.Value.CostAmount + " -> " + -resolve );
                                relic.Value.CostAmount = resolve;
                            } else {
                                if( relic.Key is not RelicType.WeaponSwap and not RelicType.SpellSwap and not RelicType.ExtraLife_Unity and not RelicType.ExtraLife_UnityUsed ) {
                                    float resolve;
                                    if( relic.Key is RelicType.DamageNoHitChallengeUsed or RelicType.ExtraLifeUsed or RelicType.FreeFairyChestUsed ) {
                                        resolve = WobSettings.Get( relicKeys.Get( relic.Key, "ResolveBroken" ), relic.Value.CostAmount );
                                    } else {
                                        resolve = WobSettings.Get( relicKeys.Get( relic.Key, "ResolveCost" ), relic.Value.CostAmount );
                                    }
                                    WobPlugin.Log( "[Relics] Changing resolve cost for " + relic.Value.Name + " " + relic.Value.CostAmount + " -> " + resolve );
                                    relic.Value.CostAmount = resolve;
                                }
                            }
                        }
                    }
                    runOnce = true;
                }
            }
        }

        // Correct the total resolve cost calculation to prevent negative costs from reduced relic costs or increased effect of Archeology Camp (Relic_Cost_Down)
        [HarmonyPatch( typeof( PlayerSaveData ), nameof( PlayerSaveData.GetTotalRelicResolveCost ) )]
        internal static class PlayerSaveData_GetTotalRelicResolveCost_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "PlayerSaveData.GetTotalRelicResolveCost" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodeSet.Ldloc ), // num2
                        /*  1 */ new( OpCodeSet.Ldloc ), // relicCostMod
                        /*  2 */ new( OpCodes.Sub     ), // num2 - relicCostMod
                        /*  3 */ new( OpCodeSet.Stloc ), // num2 = num2 - relicCostMod
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_Insert( 3, new List<CodeInstruction> {
                            new( OpCodes.Ldc_R4, 0f ),
                            new( OpCodes.Ldc_R4, float.MaxValue ),
                            new( OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Mathf.Clamp( 0f, 0f, float.MaxValue ) ) ),
                        } ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - RELIC INDIVIDUAL VALUES
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        //// Prevent adding the 'Used' variant of Aite's Sword, Hyperion's Ring, and Skeleton Key
        //[HarmonyPatch( typeof( RelicObj ), nameof( RelicObj.SetLevel ) )]
        //internal static class RelicObj_SetLevel_Patch {
        //    internal static void Prefix( RelicObj __instance, ref int value ) {
        //        RelicType relicType = __instance.RelicType;
        //        if( ( relicType == RelicType.DamageNoHitChallengeUsed || relicType == RelicType.ExtraLifeUsed || relicType == RelicType.FreeFairyChestUsed ) && value > 0 ) {
        //            if( WobSettings.Get( relicKeys.Get( relicType, "RemoveOnBreak" ), false ) ) {
        //                value = 0;
        //            }
        //        }
        //    }
        //}

        // Prevent breaking of Skeleton Key
        [HarmonyPatch( typeof( ChestObj ), "OpenChest" )]
        internal static class ChestObj_OpenChest_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                if( WobSettings.Get( relicKeys.Get( RelicType.FreeFairyChest, "InfiniteUses" ), false ) ) {
                    // Set up the transpiler handler with the instruction list
                    WobTranspiler transpiler = new( instructions, "ChestObj.OpenChest" );
                    // Perform the patching
                    transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            // relic.SetLevel(-1, true, true);
                            // SaveManager.PlayerSaveData.GetRelic(RelicType.FreeFairyChestUsed).SetLevel(1, true, true);
                            /*  0 */ new( OpCodeSet.Ldloc                                   ), // relic
                            /*  1 */ new( OpCodes.Ldc_I4_M1                                 ), // -1
                            /*  2 */ new( OpCodeSet.Ldc_I4_Bool                             ), // true
                            /*  3 */ new( OpCodeSet.Ldc_I4_Bool                             ), // true
                            /*  4 */ new( OpCodes.Callvirt, name: "SetLevel"                ), // relic.SetLevel(-1, true, true)
                            /*  5 */ new( OpCodes.Ldsfld, name: "PlayerSaveData"            ), // SaveManager.PlayerSaveData
                            /*  6 */ new( OpCodes.Ldc_I4, (int)RelicType.FreeFairyChestUsed ), // RelicType.FreeFairyChestUsed
                            /*  7 */ new( OpCodes.Callvirt,  name: "GetRelic"               ), // SaveManager.PlayerSaveData.GetRelic(RelicType.FreeFairyChestUsed)
                            /*  8 */ new( OpCodes.Ldc_I4_1                                  ), // 1
                            /*  9 */ new( OpCodeSet.Ldc_I4_Bool                             ), // true
                            /* 10 */ new( OpCodeSet.Ldc_I4_Bool                             ), // true
                            /* 11 */ new( OpCodes.Callvirt, name: "SetLevel"                ), // SaveManager.PlayerSaveData.GetRelic(RelicType.FreeFairyChestUsed).SetLevel(1, true, true)
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_Remove( 0, 12 ),
                        }, expected: 1 );
                    // Return the modified instructions
                    return transpiler.GetResult();
                } else {
                    return instructions;
                }
            }
        }

        // Add bonus resolve and minimum resolve, by using the method that checks the Leather Unity 100 bonus to minimum resolve
        [HarmonyPatch( typeof( EquipmentManager ), nameof( EquipmentManager.Get_EquipmentSet_BonusTypeStatGain ) )]
        internal static class EquipmentManager_Get_EquipmentSet_BonusTypeStatGain_Patch {
            internal static void Postfix( EquipmentSetBonusType bonusType, ref float __result ) {
                switch( bonusType ) {
                    case EquipmentSetBonusType.MinimumResolve:
                        __result += WobSettings.Get( "Resolve", "ResolveMinimum", 0f );
                        break;
                    case EquipmentSetBonusType.Resolve:
                        __result += WobSettings.Get( "Resolve", "ResolveBonus", 0f );
                        break;
                }
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - HEAVY STONE BARGAIN
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Set the attack cooldown of Heavy Stone Bargain
        [HarmonyPatch( typeof( BaseAbility_RL ), nameof( BaseAbility_RL.ActualCooldownTime ), MethodType.Getter )]
        internal static class BaseAbility_RL_ActualCooldownTime_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "BaseAbility_RL.ActualCooldownTime" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodeSet.Ldloc                                   ), // num
                        /*  1 */ new( OpCodes.Ldc_R4, Relic_EV.ATTACK_COOLDOWN_DURATION ), // 1.25f
                        /*  2 */ new( OpCodeSet.Ldloc                                   ), // level
                        /*  3 */ new( OpCodes.Conv_R4                                   ), // (float)level
                        /*  4 */ new( OpCodes.Mul                                       ), // 1.25f * (float)level
                        /*  5 */ new( OpCodes.Add                                       ), // num + 1.25f * (float)level
                        /*  6 */ new( OpCodeSet.Stloc                                   ), // num = num + 1.25f * (float)level
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( relicKeys.Get( RelicType.AttackCooldown, "Cooldown" ), Relic_EV.ATTACK_COOLDOWN_DURATION ) ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Attack cooldown of Heavy Stone Bargain on revolver
        [HarmonyPatch( typeof( PistolWeapon_Ability ), "ExitAnimExitDelay", MethodType.Getter )]
        internal static class PistolWeapon_Ability_ExitAnimExitDelay_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "PistolWeapon_Ability.ExitAnimExitDelay" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldc_I4, RelicType.AttackCooldown                       ), // RelicType.AttackCooldown
                        /*  1 */ new( OpCodes.Callvirt, name: "GetRelic"                             ), // GetRelic(RelicType.AttackCooldown)
                        /*  2 */ new( OpCodes.Callvirt, name: "get_Level"                            ), // GetRelic(RelicType.AttackCooldown).Level
                        /*  3 */ new( OpCodes.Conv_R4                                                ), // (float)GetRelic(RelicType.AttackCooldown).Level
                        /*  4 */ new( OpCodes.Ldc_R4, Relic_EV.ATTACK_COOLDOWN_PISTOL_EXIT_DELAY_ADD ), // 0.1f
                        /*  5 */ new( OpCodes.Mul                                                    ), // (float)GetRelic(RelicType.AttackCooldown).Level * 0.1f
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 4, WobSettings.Get( relicKeys.Get( RelicType.AttackCooldown, "CooldownGun" ), Relic_EV.ATTACK_COOLDOWN_PISTOL_EXIT_DELAY_ADD ) ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Attack cooldown of Heavy Stone Bargain on blunderbus
        [HarmonyPatch( typeof( Shotgun_Ability ), "ExitAnimExitDelay", MethodType.Getter )]
        internal static class Shotgun_Ability_ExitAnimExitDelay_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "Shotgun_Ability.ExitAnimExitDelay" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldc_I4, RelicType.AttackCooldown                       ), // RelicType.AttackCooldown
                        /*  1 */ new( OpCodes.Callvirt, name: "GetRelic"                             ), // GetRelic(RelicType.AttackCooldown)
                        /*  2 */ new( OpCodes.Callvirt, name: "get_Level"                            ), // GetRelic(RelicType.AttackCooldown).Level
                        /*  3 */ new( OpCodes.Conv_R4                                                ), // (float)GetRelic(RelicType.AttackCooldown).Level
                        /*  4 */ new( OpCodes.Ldc_R4, Relic_EV.ATTACK_COOLDOWN_PISTOL_EXIT_DELAY_ADD ), // 0.1f
                        /*  5 */ new( OpCodes.Mul                                                    ), // (float)GetRelic(RelicType.AttackCooldown).Level * 0.1f
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 4, WobSettings.Get( relicKeys.Get( RelicType.AttackCooldown, "CooldownGun" ), Relic_EV.ATTACK_COOLDOWN_PISTOL_EXIT_DELAY_ADD ) ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Attack cooldown of Heavy Stone Bargain on crow storm
        [HarmonyPatch( typeof( CrowStorm_Ability ), "ChangeAnim" )]
        internal static class CrowStorm_Ability_ChangeAnim_Patch {
            // Find the correct method - this is an implicitly defined method
            // 'ChangeAnim' returns an IEnumerator, and we need to patch the 'MoveNext' method on that
            internal static MethodInfo TargetMethod() {
                // Find the nested class of 'CrowStorm_Ability' that 'ChangeAnim' implicitly created
                System.Type type = AccessTools.FirstInner( typeof( CrowStorm_Ability ), t => t.Name.Contains( "<ChangeAnim>d__" ) );
                // Find the 'MoveNext' method on the nested class
                return AccessTools.FirstMethod( type, method => method.Name.Contains( "MoveNext" ) );
            }

            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "CrowStorm_Ability.ChangeAnim" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldsfld, name: "PlayerSaveData"                           ), // SaveManager.PlayerSaveData
                        /*  1 */ new( OpCodes.Ldc_I4, RelicType.AttackCooldown                         ), // RelicType.AttackCooldown
                        /*  2 */ new( OpCodes.Callvirt, name: "GetRelic"                               ), // GetRelic(RelicType.AttackCooldown)
                        /*  3 */ new( OpCodes.Callvirt, name: "get_Level"                              ), // GetRelic(RelicType.AttackCooldown).Level
                        /*  4 */ new( OpCodeSet.Stloc                                                  ), // int level = GetRelic(RelicType.AttackCooldown).Level
                        /*  5 */ new( OpCodes.Ldarg_0                                                  ), // this
                        /*  6 */ new( OpCodes.Ldarg_0                                                  ), // this
                        /*  7 */ new( OpCodes.Ldfld                                                    ), // ticDuration
                        /*  8 */ new( OpCodes.Ldc_R4, Relic_EV.ATTACK_COOLDOWN_CROWSTORM_TIC_DELAY_ADD ), // 0.5f
                        /*  9 */ new( OpCodeSet.Ldloc                                                  ), // level
                        /* 10 */ new( OpCodes.Conv_R4                                                  ), // (float)level
                        /* 11 */ new( OpCodes.Mul                                                      ), // 0.5f * (float)level
                        /* 12 */ new( OpCodes.Add                                                      ), // ticDuration + ( 0.5f * (float)level )
                        /* 13 */ new( OpCodes.Stfld                                                    ), // ticDuration = ticDuration + ( 0.5f * (float)level )
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 8, WobSettings.Get( relicKeys.Get( RelicType.AttackCooldown, "CooldownCrow" ), Relic_EV.ATTACK_COOLDOWN_CROWSTORM_TIC_DELAY_ADD ) ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Move slow of Heavy Stone Bargain on Hephaestus' Hammer
        [HarmonyPatch( typeof( AxeSpinner_Ability ), "OnEnterAttackLogic" )]
        internal static class AxeSpinner_Ability_OnEnterAttackLogic_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "AxeSpinner_Ability.OnEnterAttackLogic" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Callvirt, name: "get_MovementSpeedMod" ), // MovementSpeedMod
                        /*  1 */ new( OpCodes.Ldc_R4                                 ), // -0.3f
                        /*  2 */ new( OpCodeSet.Ldloc                                ), // level
                        /*  3 */ new( OpCodes.Conv_R4                                ), // (float)level
                        /*  4 */ new( OpCodes.Mul                                    ), // -0.3f * (float)level
                        /*  5 */ new( OpCodes.Add                                    ), // MovementSpeedMod + -0.3f * (float)level
                        /*  6 */ new( OpCodes.Callvirt, name: "set_MovementSpeedMod" ), // MovementSpeedMod = MovementSpeedMod + -0.3f * (float)level
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( relicKeys.Get( RelicType.AttackCooldown, "SlowHammer" ), Relic_EV.ATTACK_COOLDOWN_AXE_SPINNER_MOD ) ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }
        // Move slow of Heavy Stone Bargain on Hephaestus' Hammer
        [HarmonyPatch( typeof( AxeSpinner_Ability ), "StopAbility" )]
        internal static class AxeSpinner_Ability_StopAbility_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "AxeSpinner_Ability.StopAbility" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Callvirt, name: "get_MovementSpeedMod" ), // MovementSpeedMod
                        /*  1 */ new( OpCodes.Ldc_R4                                 ), // -0.3f
                        /*  2 */ new( OpCodeSet.Ldloc                                ), // level
                        /*  3 */ new( OpCodes.Conv_R4                                ), // (float)level
                        /*  4 */ new( OpCodes.Mul                                    ), // -0.3f * (float)level
                        /*  5 */ new( OpCodes.Sub                                    ), // MovementSpeedMod - -0.3f * (float)level
                        /*  6 */ new( OpCodes.Callvirt, name: "set_MovementSpeedMod" ), // MovementSpeedMod = MovementSpeedMod - -0.3f * (float)level
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( relicKeys.Get( RelicType.AttackCooldown, "SlowHammer" ), Relic_EV.ATTACK_COOLDOWN_AXE_SPINNER_MOD ) ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - ICARUS WINGS BUTTON
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Disable standard activation of Icarus' Wings relic
        [HarmonyPatch( typeof( CharacterJump_RL ), "EvaluateJumpConditions" )]
        internal static class CharacterJump_RL_EvaluateJumpConditions_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "CharacterJump_RL.EvaluateJumpConditions" );
                // Perform the patching
                if( WobSettings.Get( relicKeys.Get( RelicType.FlightBonusCurse, "FlightButton" ), false ) ) {
                    transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new( OpCodes.Ldsfld, name: "PlayerSaveData"     ), // SaveManager.PlayerSaveData
                            /*  1 */ new( OpCodes.Ldc_I4, RelicType.FlightBonusCurse ), // RelicType.FlightBonusCurse
                            /*  2 */ new( OpCodes.Callvirt, name: "GetRelic"         ), // SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse)
                            /*  3 */ new( OpCodes.Callvirt, name: "get_Level"        ), // SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse).Level
                            /*  4 */ new( OpCodes.Ldc_I4_0                           ), // 0
                            /*  5 */ new( OpCodeSet.Ble                              ), // if (SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse).Level > 0)
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetInstruction( 4, OpCodes.Ldc_I4, int.MaxValue ), // Change test to Level > int.MaxValue, which is impossible
                        }, expected: 1 );
                }
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Enable assistance flight button press for Icarus' Wings relic
        [HarmonyPatch( typeof( CharacterFlight_RL ), "HandleInput" )]
        internal static class CharacterFlight_RL_HandleInput_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "CharacterFlight_RL.HandleInput" );
                // Perform the patching
                if( WobSettings.Get( relicKeys.Get( RelicType.FlightBonusCurse, "FlightButton" ), false ) ) {
                    transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new( OpCodes.Ldsfld, name: "PlayerSaveData"           ), // SaveManager.PlayerSaveData
                            /*  1 */ new( OpCodes.Ldfld, name: "EnableHouseRules"          ), // SaveManager.PlayerSaveData.EnableHouseRules
                            /*  2 */ new( OpCodeSet.Brfalse                                ), // if (SaveManager.PlayerSaveData.EnableHouseRules)
                            /*  3 */ new( OpCodes.Ldsfld, name: "PlayerSaveData"           ), // SaveManager.PlayerSaveData
                            /*  4 */ new( OpCodes.Ldfld, name: "Assist_EnableFlightToggle" ), // SaveManager.PlayerSaveData.Assist_EnableFlightToggle
                            /*  5 */ new( OpCodeSet.Brfalse                                ), // if (SaveManager.PlayerSaveData.Assist_EnableFlightToggle)
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetInstruction( 0, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => CheckFlightConditions() ) ), // New override method call
                            new WobTranspiler.OpAction_Remove( 1, 4 ), // Remove the rest until the final branch
                        }, expected: 1 );
                }
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool CheckFlightConditions() {
                return ( SaveManager.PlayerSaveData.EnableHouseRules && SaveManager.PlayerSaveData.Assist_EnableFlightToggle ) || ( SaveManager.PlayerSaveData.GetRelic( RelicType.FlightBonusCurse ).Level > 0 );
            }
        }

    }
}
