using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using TMPro;
using UnityEngine;
using Wob_Common;

namespace WobMod {
    public partial class CastleSkills {

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTING DEFAULTS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        
        private record SkillTreeInfo( string Config, string Name );
        private static readonly Dictionary<SkillTreeType, SkillTreeInfo> skillTreeInfo = new() {
            { SkillTreeType.Architect_Cost_Down,        new( "ArchitectCost",    "Drill Store - Reduce the Architect finder's fee. +X% fee reduction per rank"                                                                            ) },
            { SkillTreeType.Armor_Up,                   new( "Armor1",           "Foundry - Gain bonus Armor. Each point in Armor blocks one point of damage. (Max block cap: 35%). +X Armor per rank"                                    ) },
            { SkillTreeType.Armor_Up2,                  new( "Armor2",           "Blast Furnace - Gain bonus Armor. Each point in Armor blocks one point of damage. (Max block cap: 35%). +X Armor per rank"                              ) },
            { SkillTreeType.Armor_Up3,                  new( "Armor3",           "Some Kind of Kiln - Gain bonus Armor. Each point in Armor blocks one point of damage. (Max block cap: 35%). +X Armor per rank"                          ) },
            { SkillTreeType.Attack_Up,                  new( "Strength1",        "Arsenal - Increases Strength, raising Weapon Damage. +X Strength per rank"                                                                              ) },
            { SkillTreeType.Attack_Up2,                 new( "Strength2",        "Sauna - Increases Strength, raising Weapon Damage. +X Strength per rank"                                                                                ) },
            { SkillTreeType.Attack_Up3,                 new( "Strength3",        "Rock Climbing Wall - Increases Strength, raising Weapon Damage. +X Strength per rank"                                                                   ) },
            { SkillTreeType.Boss_Health_Restore,        new( "BossHeal",         "Meditation Studies - Restore Health and Mana when entering a Boss Chamber. +X% Health and Mana restored per rank"                                       ) },
            { SkillTreeType.Crit_Chance_Flat_Up,        new( "WeaponCritChance", "The Dicer's Den - Increases the chance of a random Weapon Crit. Also raises the chance for Skill Crits to become Super Crits. +X% Crit Chance per rank" ) },
            { SkillTreeType.Crit_Damage_Up,             new( "WeaponCritDamage", "The Laundromat - Increases damage from Weapon Crits. +X% Crit Damage per rank"                                                                          ) },
            { SkillTreeType.Dash_Strike_Up,             new( "DashDamage",       "Jousting Studies - You take reduced damage while Dashing. -X% damage taken per rank"                                                                    ) },
            { SkillTreeType.Dexterity_Add1,             new( "Dexterity1",       "Gym - Increases Dexterity, raising damage on Weapon Crits. +X Dexterity per rank"                                                                       ) },
            { SkillTreeType.Dexterity_Add2,             new( "Dexterity2",       "Yoga Class - Increases Dexterity, raising damage on Weapon Crits. +X Dexterity per rank"                                                                ) },
            { SkillTreeType.Dexterity_Add3,             new( "Dexterity3",       "Flower Shop - Increases Dexterity, raising damage on Weapon Crits. +X Dexterity per rank"                                                               ) },
            { SkillTreeType.Down_Strike_Up,             new( "SpinKick",         "Bamboo Garden - Spin Kick damage also scales with your INT. stat. +X% Intelligence Scaling per rank"                                                    ) },
            { SkillTreeType.Equip_Up,                   new( "EquipWeight1",     "Fashion Chambers - Increases Max Weight Capacity. +X Equip Weight per rank"                                                                             ) },
            { SkillTreeType.Equip_Up2,                  new( "EquipWeight2",     "Tailors - Increases Max Weight Capacity. +X Equip Weight per rank"                                                                                      ) },
            { SkillTreeType.Equip_Up3,                  new( "EquipWeight3",     "Artisan - Increases Max Weight Capacity. +X Equip Weight per rank"                                                                                      ) },
            { SkillTreeType.Equipment_Ore_Find_Up,      new( "OreChance",        "Geologist's Camp - Chance to randomly find Ore in Breakables. +X% chance per rank"                                                                      ) },
            { SkillTreeType.Equipment_Ore_Gain_Up,      new( "OreGain",          "Jeweler - Increase Ore gain. +X% Ore gain per rank"                                                                                                     ) },
            { SkillTreeType.Focus_Up1,                  new( "Focus1",           "Library - Increases Focus, raising damage on Spell Crits. +X Focus per rank"                                                                            ) },
            { SkillTreeType.Focus_Up2,                  new( "Focus2",           "Hall of Wisdom - Increases Focus, raising damage on Spell Crits. +X Focus per rank"                                                                     ) },
            { SkillTreeType.Focus_Up3,                  new( "Focus3",           "Court of the Wise - Increases Focus, raising damage on Spell Crits. +X Focus per rank"                                                                  ) },
            { SkillTreeType.Gold_Gain_Up,               new( "GoldGain",         "Massive Vault - Increase Gold Gain. +X% Gold gain per rank"                                                                                             ) },
            { SkillTreeType.Gold_Saved_Amount_Saved,    new( "SafeConversion",   "Scribe's Office - Increase Living Safe's Gold Conversion. +X% Gold per rank"                                                                            ) },
            { SkillTreeType.Gold_Saved_Cap_Up,          new( "SafeCapacity",     "Courthouse - Increase Living Safe's Max Gold Capacity. +X Gold"                                                                                         ) },
            { SkillTreeType.Health_Up,                  new( "Vitality1",        "Mess Hall - Increases Vitality, raising Max HP. +X Vitality per rank"                                                                                   ) },
            { SkillTreeType.Health_Up2,                 new( "Vitality2",        "Fruit Juice Bar - Increases Vitality, raising Max HP. +X Vitality per rank"                                                                             ) },
            { SkillTreeType.Health_Up3,                 new( "Vitality3",        "Meteora Gym - Increases Vitality, raising Max HP. +X Vitality per rank"                                                                                 ) },
            { SkillTreeType.Magic_Attack_Up,            new( "Intelligence1",    "Study Hall - Increases Intelligence, raising Spell and Talent Damage. Increases Health Gain from Health Drops. +X Intelligence per rank"                ) },
            { SkillTreeType.Magic_Attack_Up2,           new( "Intelligence2",    "Math Club - Increases Intelligence, raising Spell and Talent Damage. Increases Health Gain from Health Drops. +X Intelligence per rank"                 ) },
            { SkillTreeType.Magic_Attack_Up3,           new( "Intelligence3",    "University - Increases Intelligence, raising Spell and Talent Damage. Increases Health Gain from Health Drops. +X Intelligence per rank"                ) },
            { SkillTreeType.Magic_Crit_Chance_Flat_Up,  new( "MagicCritChance",  "The Quantum Observatory - Increases the chance of random Magic Crits. Adds a chance for Magic Crits to become Super Crits. +X% Crit Chance per rank"    ) },
            { SkillTreeType.Magic_Crit_Damage_Up,       new( "MagicCritDamage",  "The Lodge - Increases damage from Magic Crits. +X% Crit Damage per rank"                                                                                ) },
            { SkillTreeType.Potion_Up,                  new( "FoodHealth",       "Institute of Gastronomy - Improve INT. scaling from Health Drops. +X% Health gain per rank"                                                             ) },
            { SkillTreeType.Randomize_Children,         new( "RerollHeirs",      "Career Center - Allows you to re-roll your characters. +X re-rolls per rank"                                                                            ) },
            { SkillTreeType.Relic_Cost_Down,            new( "RelicCost",        "Archeology Camp - Relics cost less Resolve. -X% Resolve cost per rank"                                                                                  ) },
            { SkillTreeType.Reroll_Relic,               new( "RerollRelicTotal", "Medieval Forgery - You can now re-roll Relics and Curios found in the Kingdom. +X re-rolls per rank"                                                    ) },
            { SkillTreeType.Reroll_Relic_Room_Cap,      new( "RerollRelicRoom",  "The Bizarre Bazaar - You can now re-roll Relics and Curios in the same location multiple times. +X re-rolls"                                            ) },
            { SkillTreeType.Resolve_Up,                 new( "Resolve",          "Psychiatrist - Increase your starting Resolve. +X% Resolve per rank"                                                                                    ) },
            { SkillTreeType.Rune_Equip_Up,              new( "RuneWeight1",      "Etching Chambers - Increases Max Rune Weight. +X Rune Weight per rank"                                                                                  ) },
            { SkillTreeType.Rune_Equip_Up2,             new( "RuneWeight2",      "Pillow Mill - Increases Max Rune Weight. +X Rune Weight per rank"                                                                                       ) },
            { SkillTreeType.Rune_Equip_Up3,             new( "RuneWeight3",      "Bed Mill - Increases Max Rune Weight. +X Rune Weight per rank"                                                                                          ) },
            { SkillTreeType.Rune_Ore_Find_Up,           new( "AetherChance",     "Dowsing Center - Chance to randomly find Red Aether in Breakables. +X% chance per rank"                                                                 ) },
            { SkillTreeType.Rune_Ore_Gain_Up,           new( "AetherGain",       "Buried Tomb - Increase Red Aether gain. +X% Aether gain per rank"                                                                                       ) },
            { SkillTreeType.Traits_Give_Gold_Gain_Mod,  new( "TraitGold",        "Repurposed Mining Shaft - Increases Gold Gain for certain Traits. +X% Gold granted by Trait bonus per rank"                                             ) },
            { SkillTreeType.Weight_CD_Reduce,           new( "EquipWeightClass", "Aerobics Classroom - Increases Encumbrance Limits. *Your Weight Class will now change every X%. +X% Encumbrance per rank"                               ) },
            { SkillTreeType.XP_Up,                      new( "XPGain",           "Trophy Room - Increase XP Gain. +X% XP per rank"                                                                                                        ) },
        };

        private static readonly HashSet<SkillTreeType> noStatTypes = new() {
            // Hidden unlocks
            SkillTreeType.None, SkillTreeType.LabourCosts_Unlocked, SkillTreeType.PizzaGirl_Unlocked,
            // One-off class unlocks
            SkillTreeType.Astro_Class_Unlock, SkillTreeType.Axe_Class_Unlock,     SkillTreeType.Bow_Class_Unlock,    SkillTreeType.BoxingGlove_Class_Unlock, SkillTreeType.DualBlades_Class_Unlock,
            SkillTreeType.Gun_Class_Unlock,   SkillTreeType.Ladle_Class_Unlock,   SkillTreeType.Lancer_Class_Unlock, SkillTreeType.Music_Class_Unlock,       SkillTreeType.Pirate_Class_Unlock,
            SkillTreeType.Saber_Class_Unlock, SkillTreeType.Samurai_Class_Unlock, SkillTreeType.Spear_Class_Unlock,  SkillTreeType.Sword_Class_Unlock,       SkillTreeType.Wand_Class_Unlock,
            // One-off NPC unlocks
            SkillTreeType.Architect, SkillTreeType.Banker, SkillTreeType.Enchantress, SkillTreeType.Gold_Saved_Unlock, SkillTreeType.Smithy, SkillTreeType.Unlock_Dummy, SkillTreeType.Unlock_Totem,
            // One-off special unlocks
            SkillTreeType.Charon_Gold_Stat_Bonus, SkillTreeType.More_Children, SkillTreeType.Potion_Recharge_Talent, SkillTreeType.Potions_Free_Cast_Up, SkillTreeType.Traits_Give_Gold,
        };

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTINGS READ/WRITE
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        internal static readonly WobSettings.KeyHelper<SkillTreeType> skillTreeKeys = new( "Skill" );

        internal static void RunSetup() {
            WobMod.configFiles.Add( "CastleSkills", "CastleSkills" );
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Boolean(    WobMod.configFiles.Get( "CastleSkills" ), "CastleTree", "UnlockTree",  "Unlock the tree - all skills visible/selectable",    false ),
                new WobSettings.Boolean(    WobMod.configFiles.Get( "CastleSkills" ), "CastleTree", "UnlockLevel", "Unlock the level - remove manor level requirements", false ),
            } );
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Num<sbyte>( WobMod.configFiles.Get( "CastleSkills" ), "LabourCosts", "StartLevel", "Level after which labour costs start.",              (sbyte)SkillTree_EV.SKILLS_UNLOCKED_BEFORE_LABOUR_COSTS_START,  bounds: (0, sbyte.MaxValue) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "CastleSkills" ), "LabourCosts", "PerLevel",   "Cost increase per level. Set to 0 to remove labour costs.", SkillTree_EV.GLOBAL_SKILL_COST_APPRECIATION_FLAT,        bounds: (0, 1000000f     ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), "LabourCosts", "RoundTo",    "Round down calculated cost to this significance.",          SkillTree_EV.GLOBAL_SKILL_COST_ROUNDING,                 bounds: (1, 1000000      ) ),
            } );
            foreach( SkillTreeType skillTreeType in skillTreeInfo.Keys ) {
                skillTreeKeys.Add( skillTreeType, skillTreeInfo[skillTreeType].Config );
            }
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Num<float>( WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Architect_Cost_Down,       "PerRank" ), skillTreeInfo[SkillTreeType.Architect_Cost_Down].Name,                   2f,   0.01f, bounds: (0.1f, 20f     ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Armor_Up,                  "PerRank" ), skillTreeInfo[SkillTreeType.Armor_Up].Name,                              2,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Armor_Up2,                 "PerRank" ), skillTreeInfo[SkillTreeType.Armor_Up2].Name,                             2,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Armor_Up3,                 "PerRank" ), skillTreeInfo[SkillTreeType.Armor_Up3].Name,                             2,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Attack_Up,                 "PerRank" ), skillTreeInfo[SkillTreeType.Attack_Up].Name,                             1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Attack_Up2,                "PerRank" ), skillTreeInfo[SkillTreeType.Attack_Up2].Name,                            1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Attack_Up3,                "PerRank" ), skillTreeInfo[SkillTreeType.Attack_Up3].Name,                            1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Boss_Health_Restore,       "PerRank" ), skillTreeInfo[SkillTreeType.Boss_Health_Restore].Name,                   20f,  0.01f, bounds: (0.1f, 20f     ) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Crit_Chance_Flat_Up,       "PerRank" ), skillTreeInfo[SkillTreeType.Crit_Chance_Flat_Up].Name,                   1f,   0.01f, bounds: (0.1f, 4f      ) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Crit_Damage_Up,            "PerRank" ), skillTreeInfo[SkillTreeType.Crit_Damage_Up].Name,                        2f,   0.01f, bounds: (0.1f, 1000000f) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Dash_Strike_Up,            "PerRank" ), skillTreeInfo[SkillTreeType.Dash_Strike_Up].Name,                        5f,   0.01f, bounds: (0.1f, 20f     ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Dexterity_Add1,            "PerRank" ), skillTreeInfo[SkillTreeType.Dexterity_Add1].Name,                        1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Dexterity_Add2,            "PerRank" ), skillTreeInfo[SkillTreeType.Dexterity_Add2].Name,                        1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Dexterity_Add3,            "PerRank" ), skillTreeInfo[SkillTreeType.Dexterity_Add3].Name,                        1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Down_Strike_Up,            "PerRank" ), skillTreeInfo[SkillTreeType.Down_Strike_Up].Name,                        10f,  0.01f, bounds: (0.1f, 20f     ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Equip_Up,                  "PerRank" ), skillTreeInfo[SkillTreeType.Equip_Up].Name,                              10,          bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Equip_Up2,                 "PerRank" ), skillTreeInfo[SkillTreeType.Equip_Up2].Name,                             10,          bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Equip_Up3,                 "PerRank" ), skillTreeInfo[SkillTreeType.Equip_Up3].Name,                             10,          bounds: (1,    1000000 ) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Equipment_Ore_Find_Up,     "PerRank" ), skillTreeInfo[SkillTreeType.Equipment_Ore_Find_Up].Name,                 1f,   0.01f, bounds: (0.1f, 20f     ) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Equipment_Ore_Gain_Up,     "PerRank" ), skillTreeInfo[SkillTreeType.Equipment_Ore_Gain_Up].Name,                 5f,   0.01f, bounds: (0.1f, 1000000f) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Focus_Up1,                 "PerRank" ), skillTreeInfo[SkillTreeType.Focus_Up1].Name,                             1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Focus_Up2,                 "PerRank" ), skillTreeInfo[SkillTreeType.Focus_Up2].Name,                             1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Focus_Up3,                 "PerRank" ), skillTreeInfo[SkillTreeType.Focus_Up3].Name,                             1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Gold_Gain_Up,              "PerRank" ), skillTreeInfo[SkillTreeType.Gold_Gain_Up].Name,                          5f,   0.01f, bounds: (0.1f, 1000000f) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Gold_Saved_Amount_Saved,   "PerRank" ), skillTreeInfo[SkillTreeType.Gold_Saved_Amount_Saved].Name,               4f,   0.01f, bounds: (0.1f, 4.5f    ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Gold_Saved_Cap_Up,         "Rank1"   ), skillTreeInfo[SkillTreeType.Gold_Saved_Cap_Up].Name + " for rank 1",     1500,        bounds: (0,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Gold_Saved_Cap_Up,         "Rank2"   ), skillTreeInfo[SkillTreeType.Gold_Saved_Cap_Up].Name + " for rank 2+",    1500,        bounds: (0,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Health_Up,                 "PerRank" ), skillTreeInfo[SkillTreeType.Health_Up].Name,                             1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Health_Up2,                "PerRank" ), skillTreeInfo[SkillTreeType.Health_Up2].Name,                            1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Health_Up3,                "PerRank" ), skillTreeInfo[SkillTreeType.Health_Up3].Name,                            1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Magic_Attack_Up,           "PerRank" ), skillTreeInfo[SkillTreeType.Magic_Attack_Up].Name,                       1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Magic_Attack_Up2,          "PerRank" ), skillTreeInfo[SkillTreeType.Magic_Attack_Up2].Name,                      1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Magic_Attack_Up3,          "PerRank" ), skillTreeInfo[SkillTreeType.Magic_Attack_Up3].Name,                      1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Magic_Crit_Chance_Flat_Up, "PerRank" ), skillTreeInfo[SkillTreeType.Magic_Crit_Chance_Flat_Up].Name,             1f,   0.01f, bounds: (0.1f, 4f      ) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Magic_Crit_Damage_Up,      "PerRank" ), skillTreeInfo[SkillTreeType.Magic_Crit_Damage_Up].Name,                  2f,   0.01f, bounds: (0.1f, 1000000f) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Potion_Up,                 "PerRank" ), skillTreeInfo[SkillTreeType.Potion_Up].Name,                             5f,   0.01f, bounds: (0.1f, 1000000f) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Randomize_Children,        "PerRank" ), skillTreeInfo[SkillTreeType.Randomize_Children].Name,                    1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Relic_Cost_Down,           "PerRank" ), skillTreeInfo[SkillTreeType.Relic_Cost_Down].Name,                       1,    0.01f, bounds: (1,    20      ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Reroll_Relic,              "PerRank" ), skillTreeInfo[SkillTreeType.Reroll_Relic].Name,                          1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Reroll_Relic_Room_Cap,     "Rank1"   ), skillTreeInfo[SkillTreeType.Reroll_Relic_Room_Cap].Name + " for rank 1", 1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Reroll_Relic_Room_Cap,     "Rank2"   ), skillTreeInfo[SkillTreeType.Reroll_Relic_Room_Cap].Name + " for rank 2", 1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Resolve_Up,                "PerRank" ), skillTreeInfo[SkillTreeType.Resolve_Up].Name,                            1f,   0.01f, bounds: (1f,   1000000f) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Rune_Equip_Up,             "PerRank" ), skillTreeInfo[SkillTreeType.Rune_Equip_Up].Name,                         10,          bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Rune_Equip_Up2,            "PerRank" ), skillTreeInfo[SkillTreeType.Rune_Equip_Up2].Name,                        10,          bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Rune_Equip_Up3,            "PerRank" ), skillTreeInfo[SkillTreeType.Rune_Equip_Up3].Name,                        10,          bounds: (1,    1000000 ) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Rune_Ore_Find_Up,          "PerRank" ), skillTreeInfo[SkillTreeType.Rune_Ore_Find_Up].Name,                      1f,   0.01f, bounds: (0.1f, 20f     ) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Rune_Ore_Gain_Up,          "PerRank" ), skillTreeInfo[SkillTreeType.Rune_Ore_Gain_Up].Name,                      5f,   0.01f, bounds: (0.1f, 1000000f) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Traits_Give_Gold_Gain_Mod, "PerRank" ), skillTreeInfo[SkillTreeType.Traits_Give_Gold_Gain_Mod].Name,             10,   0.01f, bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.Weight_CD_Reduce,          "PerRank" ), skillTreeInfo[SkillTreeType.Weight_CD_Reduce].Name,                      1,    0.01f, bounds: (1,    8       ) ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "CastleSkills" ), skillTreeKeys.Get( SkillTreeType.XP_Up,                     "PerRank" ), skillTreeInfo[SkillTreeType.XP_Up].Name,                                 2.5f, 0.01f, bounds: (0.1f, 1000000f) ),
            } );
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - UNLOCK SKILL TREE
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Patch for the method that gets the level when a skill is unlocked
        [HarmonyPatch( typeof( SkillTreeObj ), nameof( SkillTreeObj.UnlockLevel ), MethodType.Getter )]
        internal static class SkillTreeObj_UnlockLevel_Patch {
            internal static void Postfix( ref int __result ) {
                if( WobSettings.Get( "CastleTree", "UnlockLevel", false ) ) {
                    // Always return 0 (unlocked)
                    __result = 0;
                }
            }
        }

        // Patch for the method that controls whether a skill is initially hidden or visible
        [HarmonyPatch( typeof( SkillTreeWindowController ), nameof( SkillTreeWindowController.Initialize ) )]
        internal static class SkillTreeWindowController_Initialize_Patch {
            // When the skill tree initialises, it looks for all skills with level > 0 and makes them visible, then all skills they connect to and also makes them visible
            // We are looking for the check of level > 0 and changing it to level >= 0, which makes all skills visible
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                if( WobSettings.Get( "CastleTree", "UnlockTree", false ) ) {
                    // Set up the transpiler handler with the instruction list
                    WobTranspiler transpiler = new( instructions, "SkillTreeWindowController.Initialize" );
                    // Perform the patching
                    transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new( OpCodes.Callvirt, name: "get_SkillTreeType" ), // skillTreeSlot.SkillTreeType
                            /*  1 */ new( OpCodes.Call, name: "GetSkillObjLevel"      ), // SkillTreeManager.GetSkillObjLevel( skillTreeSlot.SkillTreeType )
                            /*  2 */ new( OpCodes.Ldc_I4_0                            ), // 0
                            /*  3 */ new( OpCodes.Ble                                 ), // >    [inverted]
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOpcode( 3, OpCodes.Blt ), // Change the "ble" (Branch Less than or Equal) check to "blt" (Branch Less Than) - only do the 'else' part that hides a skill if the level is less than 0
                        }, expected: 1 );
                    // Return the modified instructions
                    return transpiler.GetResult();
                } else {
                    return instructions;
                }
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - LABOUR COSTS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Calculate new labour costs based on config parameters
        private static int NewLabourCost() {
            sbyte startLevel = WobSettings.Get( "LabourCosts", "StartLevel", (sbyte)SkillTree_EV.SKILLS_UNLOCKED_BEFORE_LABOUR_COSTS_START );
            float appreciation = WobSettings.Get( "LabourCosts", "PerLevel", SkillTree_EV.GLOBAL_SKILL_COST_APPRECIATION_FLAT );
            int rounding = WobSettings.Get( "LabourCosts", "RoundTo", SkillTree_EV.GLOBAL_SKILL_COST_ROUNDING );
            int labourLevel = Mathf.Clamp( SkillTreeManager.GetTotalSkillObjLevel() - startLevel, 0, int.MaxValue );
            return (int)( System.Math.Floor( ( labourLevel * appreciation ) / rounding ) * rounding );
        }

        // Patch for the method that gets the gold cost for a specific upgrade with labour costs included
        [HarmonyPatch( typeof( SkillTreeObj ), nameof( SkillTreeObj.GoldCostWithLevelAppreciation ), MethodType.Getter )]
        internal static class SkillTreeObj_GoldCostWithLevelAppreciation_Patch {
            internal static void Postfix( SkillTreeObj __instance, ref int __result ) {
                // Calculate the new cost and overwrite the original return value
                __result = __instance.GoldCost + NewLabourCost();
            }
        }

        // Patch for the method that sets the text on the labour cost UI element in the corner of the castle/skill tree
        [HarmonyPatch( typeof( SkillTreeWindowController ), "UpdateLabourCosts" )]
        internal static class SkillTreeWindowController_UpdateLabourCosts_Patch {
            // Change the text on the box to the new number
            internal static void Postfix( SkillTreeWindowController __instance ) {
                // The text field is private, so grab a reference with reflection
                TMP_Text m_labourCostText = (TMP_Text)Traverse.Create( __instance ).Field( "m_labourCostText" ).GetValue();
                // Calculate the new cost and set the text
                m_labourCostText.text = NewLabourCost().ToString();
            }

            // Change the starting level in the method, which sets whether the labour cost box is displayed
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "SkillTreeWindowController.UpdateLabourCosts" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Call, name: "GetTotalSkillObjLevel" ), // SkillTreeManager.GetTotalSkillObjLevel()
                        /*  1 */ new( OpCodes.Ldc_I4_S                            ), // > 30
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( "LabourCosts", "StartLevel", (sbyte)SkillTree_EV.SKILLS_UNLOCKED_BEFORE_LABOUR_COSTS_START ) ), // Set the operand to the new level from the config file
                    }, expected: 2 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Patch for the method that displays the pop-up when labour costs first unlock
        [HarmonyPatch()]
        public static class SkillTreeWindowController_UnlockLabourCostAnimCoroutine_Patch {
            // Find the correct method - this is an implicitly defined method
            // 'UnlockLabourCostAnimCoroutine' returns an IEnumerator, and we need to patch the 'MoveNext' method on that
            internal static MethodInfo TargetMethod() {
                // Find the nested class of 'SkillTreeWindowController' that 'UnlockLabourCostAnimCoroutine' implicitly created
                System.Type type = AccessTools.FirstInner( typeof( SkillTreeWindowController ), t => t.Name.Contains( "<UnlockLabourCostAnimCoroutine>d__" ) );
                // Find the 'MoveNext' method on the nested class
                return AccessTools.FirstMethod( type, method => method.Name.Contains( "MoveNext" ) );
            }

            // Change the starting level in the method. This sets whether the labour cost box is displayed
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "SkillTreeWindowController.UnlockLabourCostAnimCoroutine" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Call, name: "GetTotalSkillObjLevel" ), // SkillTreeManager.GetTotalSkillObjLevel()
                        /*  1 */ new( OpCodes.Ldc_I4_S                            ), // > 30
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( "LabourCosts", "StartLevel", (sbyte)SkillTree_EV.SKILLS_UNLOCKED_BEFORE_LABOUR_COSTS_START ) ), // Set the operand to the new level from the config file
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - SKILL TREE UPGRADE VALUES
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Change the stat gain just before the firest level stat gain is read
        [HarmonyPatch( typeof( SkillTreeObj ), nameof( SkillTreeObj.FirstLevelStatGain ), MethodType.Getter )]
        internal static class SkillTreeObj_FirstLevelStatGain_Patch {
            internal static void Prefix( SkillTreeObj __instance ) {
                // Putting this in a variable just to shorten the name and make the rest of this easier to read
                SkillTreeData skill = __instance.SkillTreeData;
                if( __instance.SkillTreeType is SkillTreeType.Reroll_Relic_Room_Cap or SkillTreeType.Gold_Saved_Cap_Up ) {
                    // Search the config for a setting that has the same name as the internal name of the skill
                    float statGain1 = WobSettings.Get( skillTreeKeys.Get( __instance.SkillTreeType, "Rank1" ), skill.FirstLevelStatGain );
                    float statGain2 = WobSettings.Get( skillTreeKeys.Get( __instance.SkillTreeType, "Rank2" ), skill.AdditionalLevelStatGain );
                    // Check if the stat gain is already what we want it to be
                    if( statGain1 != skill.FirstLevelStatGain || statGain2 != skill.AdditionalLevelStatGain ) {
                        // Report the change to the log
                        WobPlugin.Log( "[CastleSkills] Changing bonus for " + skill.Name + " from " + skill.FirstLevelStatGain + "/" + skill.AdditionalLevelStatGain + " to " + statGain1 + "/" + statGain2 );
                        // Set the stat gain for all ranks
                        skill.FirstLevelStatGain = statGain1;
                        skill.AdditionalLevelStatGain = statGain2;
                    }
                } else if ( skillTreeKeys.Exists( __instance.SkillTreeType ) ) {
                    // Search the config for a setting that has the same name as the internal name of the skill
                    float statGain = WobSettings.Get( skillTreeKeys.Get( __instance.SkillTreeType, "PerRank" ), skill.FirstLevelStatGain );
                    // Check if the stat gain is already what we want it to be
                    if( statGain != skill.FirstLevelStatGain || statGain != skill.AdditionalLevelStatGain ) {
                        // Report the change to the log
                        WobPlugin.Log( "[CastleSkills] Changing bonus for " + skill.Name + " from " + skill.FirstLevelStatGain + "/" + skill.AdditionalLevelStatGain + " to " + statGain + "/" + statGain );
                        // Set the stat gain for all ranks
                        skill.FirstLevelStatGain = statGain;
                        skill.AdditionalLevelStatGain = statGain;
                    }
                } else if( !noStatTypes.Contains( __instance.SkillTreeType ) ) {
                    WobPlugin.Log( "[CastleSkills] WARNING: No config for " + __instance.SkillTreeType, WobPlugin.ERROR );
                }
            }
        }

        // Reroll for heirs seems to ignore the stat gain and just use the upgrade level - this is to fix that
        // Patch for the method that controls initial setup of the UI elements for character selection, including reroll
        [HarmonyPatch( typeof( LineageWindowController ), "OnOpen" )]
        internal static class LineageWindowController_OnOpen_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "LineageWindowController.OnOpen" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Call, name: "GetSkillObjLevel" ), // SkillTreeManager.GetSkillObjLevel( SkillTreeType.Randomize_Children )
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 0, SymbolExtensions.GetMethodInfo( () => SkillTreeManager.GetSkillTreeObj( SkillTreeType.None ) ) ), // Replace the call with one that gets the upgrade itself
                        new WobTranspiler.OpAction_Insert( 1, new List<CodeInstruction>() {
                            new( OpCodes.Callvirt, AccessTools.Method( typeof( SkillTreeObj ), "get_CurrentStatGain" ) ), // Add a call to the upgrade's CurrentStatGain getter
                            new( OpCodes.Conv_I4 ), // Cast the float return value to int
                        } ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

    }
}
