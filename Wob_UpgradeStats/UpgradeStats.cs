using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Wob_Common;

namespace Wob_UpgradeStats {
    [BepInPlugin( "Wob.UpgradeStats", "Upgrade Stat Gains Mod", "0.2" )]
    public partial class UpgradeStats : BaseUnityPlugin {
        // Static reference to the config item collection so it can be searched in the patch
        public static Dictionary<string, IScaledConfigItem> configSkills;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configSkills = new Dictionary<string, IScaledConfigItem> {
                { "Architect_Cost_Down",       new ScaledConfigItemF( this.Config, "Upgrades", "Architect_Cost_Down",       "Drill Store - Reduce the Architect finder's fee. +X% fee reduction per rank",                                                                            2f,   0.1f, 20f,            0.01f ) },
                { "Armor_Up",                  new ScaledConfigItemI( this.Config, "Upgrades", "Armor_Up",                  "Foundry - Gain bonus Armor. Each point in Armor blocks one point of damage. (Max block cap: 35%). +X Armor per rank",                                    2,    1,    int.MaxValue,   1f    ) },
                { "Armor_Up2",                 new ScaledConfigItemI( this.Config, "Upgrades", "Armor_Up2",                 "Blast Furnace - Gain bonus Armor. Each point in Armor blocks one point of damage. (Max block cap: 35%). +X Armor per rank",                              2,    1,    int.MaxValue,   1f    ) },
                { "Armor_Up3",                 new ScaledConfigItemI( this.Config, "Upgrades", "Armor_Up3",                 "Some Kind of Kiln - Gain bonus Armor. Each point in Armor blocks one point of damage. (Max block cap: 35%). +X Armor per rank",                          2,    1,    int.MaxValue,   1f    ) },
                { "Attack_Up",                 new ScaledConfigItemI( this.Config, "Upgrades", "Attack_Up",                 "Arsenal - Increases Strength, raising Weapon Damage. +X Strength per rank",                                                                              1,    1,    int.MaxValue,   1f    ) },
                { "Attack_Up2",                new ScaledConfigItemI( this.Config, "Upgrades", "Attack_Up2",                "Sauna - Increases Strength, raising Weapon Damage. +X Strength per rank",                                                                                1,    1,    int.MaxValue,   1f    ) },
                { "Attack_Up3",                new ScaledConfigItemI( this.Config, "Upgrades", "Attack_Up3",                "Rock Climbing Wall - Increases Strength, raising Weapon Damage. +X Strength per rank",                                                                   1,    1,    int.MaxValue,   1f    ) },
                { "Boss_Health_Restore",       new ScaledConfigItemF( this.Config, "Upgrades", "Boss_Health_Restore",       "Meditation Studies - Restore Health and Mana when entering a Boss Chamber. +X% Health and Mana restored per rank",                                       20f,  0.1f, 20f,            0.01f ) },
                { "Crit_Chance_Flat_Up",       new ScaledConfigItemF( this.Config, "Upgrades", "Crit_Chance_Flat_Up",       "The Dicer's Den - Increases the chance of a random Weapon Crit. Also raises the chance for Skill Crits to become Super Crits. +X% Crit Chance per rank", 1f,   0.1f, 4f,             0.01f ) },
                { "Crit_Damage_Up",            new ScaledConfigItemF( this.Config, "Upgrades", "Crit_Damage_Up",            "The Laundromat - Increases damage from Weapon Crits. +X% Crit Damage per rank",                                                                          2f,   0.1f, float.MaxValue, 0.01f ) },
                { "Dash_Strike_Up",            new ScaledConfigItemF( this.Config, "Upgrades", "Dash_Strike_Up",            "Jousting Studies - You take reduced damage while Dashing. -X% damage taken per rank",                                                                    5f,   0.1f, 20f,            0.01f ) },
                { "Dexterity_Up1",             new ScaledConfigItemI( this.Config, "Upgrades", "Dexterity_Up1",             "Gym - Increases Dexterity, raising damage on Weapon Crits. +X Dexterity per rank",                                                                       1,    1,    int.MaxValue,   1f    ) },
                { "Dexterity_Up2",             new ScaledConfigItemI( this.Config, "Upgrades", "Dexterity_Up2",             "Yoga Class - Increases Dexterity, raising damage on Weapon Crits. +X Dexterity per rank",                                                                1,    1,    int.MaxValue,   1f    ) },
                { "Dexterity_Up3",             new ScaledConfigItemI( this.Config, "Upgrades", "Dexterity_Up3",             "Flower Shop - Increases Dexterity, raising damage on Weapon Crits. +X Dexterity per rank",                                                               1,    1,    int.MaxValue,   1f    ) },
                { "Down_Strike_Up",            new ScaledConfigItemF( this.Config, "Upgrades", "Down_Strike_Up",            "Bamboo Garden - Spin Kick damage also scales with your INT. stat. +X% Intelligence Scaling per rank",                                                    10f,  0.1f, 20f,            0.01f ) },
                { "Equip_Up",                  new ScaledConfigItemI( this.Config, "Upgrades", "Equip_Up",                  "Fashion Chambers - Increases Max Weight Capacity. +X Equip Weight per rank",                                                                             10,   1,    int.MaxValue,   1f    ) },
                { "Equip_Up2",                 new ScaledConfigItemI( this.Config, "Upgrades", "Equip_Up2",                 "Tailors - Increases Max Weight Capacity. +X Equip Weight per rank",                                                                                      10,   1,    int.MaxValue,   1f    ) },
                { "Equip_Up3",                 new ScaledConfigItemI( this.Config, "Upgrades", "Equip_Up3",                 "Artisan - Increases Max Weight Capacity. +X Equip Weight per rank",                                                                                      10,   1,    int.MaxValue,   1f    ) },
                { "Equipment_Ore_Gain_Up",     new ScaledConfigItemF( this.Config, "Upgrades", "Equipment_Ore_Gain_Up",     "Jeweler - Increase Ore gain. +X% Ore gain per rank",                                                                                                     5f,   0.1f, float.MaxValue, 0.01f ) },
                { "Focus_Up1",                 new ScaledConfigItemI( this.Config, "Upgrades", "Focus_Up1",                 "Library - Increases Focus, raising damage on Spell Crits. +X Focus per rank",                                                                            1,    1,    int.MaxValue,   1f    ) },
                { "Focus_Up2",                 new ScaledConfigItemI( this.Config, "Upgrades", "Focus_Up2",                 "Hall of Wisdom - Increases Focus, raising damage on Spell Crits. +X Focus per rank",                                                                     1,    1,    int.MaxValue,   1f    ) },
                { "Focus_Up3",                 new ScaledConfigItemI( this.Config, "Upgrades", "Focus_Up3",                 "Court of the Wise - Increases Focus, raising damage on Spell Crits. +X Focus per rank",                                                                  1,    1,    int.MaxValue,   1f    ) },
                { "Gold_Gain_Up",              new ScaledConfigItemF( this.Config, "Upgrades", "Gold_Gain_Up",              "Massive Vault - Increase Gold Gain. +X% Gold gain per rank",                                                                                             5f,   0.1f, float.MaxValue, 0.01f ) },
                { "Gold_Saved_Amount_Saved",   new ScaledConfigItemF( this.Config, "Upgrades", "Gold_Saved_Amount_Saved",   "Scribe's Office - Increase Living Safe's Gold Conversion. +X% Gold per rank",                                                                            4f,   0.1f, 4.5f,           0.01f ) },
                { "Gold_Saved_Cap_Up",         new ScaledConfigItemI( this.Config, "Upgrades", "Gold_Saved_Cap_Up",         "Courthouse - Increase Living Safe's Max Gold Capacity. +X Gold per rank",                                                                                1500, 1,    int.MaxValue,   1f    ) },
                { "Health_Up",                 new ScaledConfigItemI( this.Config, "Upgrades", "Health_Up",                 "Mess Hall - Increases Vitality, raising Max HP. +X Vitality per rank",                                                                                   1,    1,    int.MaxValue,   1f    ) },
                { "Health_Up2",                new ScaledConfigItemI( this.Config, "Upgrades", "Health_Up2",                "Fruit Juice Bar - Increases Vitality, raising Max HP. +X Vitality per rank",                                                                             1,    1,    int.MaxValue,   1f    ) },
                { "Health_Up3",                new ScaledConfigItemI( this.Config, "Upgrades", "Health_Up3",                "Meteora Gym - Increases Vitality, raising Max HP. +X Vitality per rank",                                                                                 1,    1,    int.MaxValue,   1f    ) },
                { "Magic_Attack_Up",           new ScaledConfigItemI( this.Config, "Upgrades", "Magic_Attack_Up",           "Study Hall - Increases Intelligence, raising Spell and Talent Damage. Increases Health Gain from Health Drops. +X Intelligence per rank",                1,    1,    int.MaxValue,   1f    ) },
                { "Magic_Attack_Up2",          new ScaledConfigItemI( this.Config, "Upgrades", "Magic_Attack_Up2",          "Math Club - Increases Intelligence, raising Spell and Talent Damage. Increases Health Gain from Health Drops. +X Intelligence per rank",                 1,    1,    int.MaxValue,   1f    ) },
                { "Magic_Attack_Up3",          new ScaledConfigItemI( this.Config, "Upgrades", "Magic_Attack_Up3",          "University - Increases Intelligence, raising Spell and Talent Damage. Increases Health Gain from Health Drops. +X Intelligence per rank",                1,    1,    int.MaxValue,   1f    ) },
                { "Magic_Crit_Chance_Flat_Up", new ScaledConfigItemF( this.Config, "Upgrades", "Magic_Crit_Chance_Flat_Up", "The Quantum Observatory - Increases the chance of random Magic Crits. Adds a chance for Magic Crits to become Super Crits. +X% Crit Chance per rank",    1f,   0.1f, 4f,             0.01f ) },
                { "Magic_Crit_Damage_Up",      new ScaledConfigItemF( this.Config, "Upgrades", "Magic_Crit_Damage_Up",      "The Lodge - Increases damage from Spell Crits. +X% Crit Damage per rank",                                                                                2f,   0.1f, float.MaxValue, 0.01f ) },
                { "Ore_Find_Up",               new ScaledConfigItemF( this.Config, "Upgrades", "Ore_Find_Up",               "Geologist's Camp - Chance to randomly find Ore in Breakables. +X% chance per rank",                                                                      1f,   0.1f, 20f,            0.01f ) },
                { "Potion_Up",                 new ScaledConfigItemF( this.Config, "Upgrades", "Potion_Up",                 "Institute of Gastronomy - Improve INT. scaling from Health Drops. +X% Health gain per rank",                                                             2f,   0.1f, 4f,             0.01f ) },
                { "Randomize_Children",        new ScaledConfigItemI( this.Config, "Upgrades", "Randomize_Children",        "Career Center - Allows you to re-roll your characters. +X re-rolls per rank",                                                                            1,    1,    int.MaxValue,   1f    ) },
                { "Relic_Cost_Down",           new ScaledConfigItemI( this.Config, "Upgrades", "Relic_Cost_Down",           "Archeology Camp - Relics cost less Resolve. -X% Resolve cost per rank",                                                                                  1,    1,    20,             0.01f ) },
                { "Reroll_Relic",              new ScaledConfigItemI( this.Config, "Upgrades", "Reroll_Relic",              "Medieval Forgery - You can now re-roll Relics and Curios found in the Kingdom. +X re-rolls per rank",                                                    1,    1,    int.MaxValue,   1f    ) },
                { "Reroll_Relic_Room_Cap",     new ScaledConfigItemI( this.Config, "Upgrades", "Reroll_Relic_Room_Cap",     "The Bizarre Bazaar - You can now re-roll Relics and Curios in the same location multiple times. +X re-rolls per rank",                                   1,    1,    int.MaxValue,   1f    ) },
                { "Resolve_Up",                new ScaledConfigItemI( this.Config, "Upgrades", "Resolve_Up",                "Psychiatrist - Increase your starting Resolve. +X% Resolve per rank",                                                                                    1,    1,    int.MaxValue,   0.01f ) },
                { "Rune_Equip_Up",             new ScaledConfigItemI( this.Config, "Upgrades", "Rune_Equip_Up",             "Etching Chambers - Increases Max Rune Weight. +X Rune Weight per rank",                                                                                  10,   1,    int.MaxValue,   1f    ) },
                { "Rune_Equip_Up2",            new ScaledConfigItemI( this.Config, "Upgrades", "Rune_Equip_Up2",            "Pillow Mill - Increases Max Rune Weight. +X Rune Weight per rank",                                                                                       10,   1,    int.MaxValue,   1f    ) },
                { "Rune_Equip_Up3",            new ScaledConfigItemI( this.Config, "Upgrades", "Rune_Equip_Up3",            "Bed Mill - Increases Max Rune Weight. +X Rune Weight per rank",                                                                                          10,   1,    int.MaxValue,   1f    ) },
                { "Rune_Ore_Find_Up",          new ScaledConfigItemF( this.Config, "Upgrades", "Rune_Ore_Find_Up",          "Dowsing Center - Chance to randomly find Red Aether in Breakables. +X% chance per rank",                                                                 1f,   0.1f, 20f,            0.01f ) },
                { "Rune_Ore_Gain_Up",          new ScaledConfigItemF( this.Config, "Upgrades", "Rune_Ore_Gain_Up",          "Buried Tomb - Increase Red Aether gain. +X% Aether gain per rank",                                                                                       5f,   0.1f, float.MaxValue, 0.01f ) },
                { "Traits_Give_Gold_Mod",      new ScaledConfigItemI( this.Config, "Upgrades", "Traits_Give_Gold_Mod",      "Repurposed Mining Shaft - Increases Gold Gain for certain Traits. +X% Gold granted by Trait bonus per rank",                                             10,   1,    int.MaxValue,   0.01f ) },
                { "Weight_CD_Reduce",          new ScaledConfigItemI( this.Config, "Upgrades", "Weight_CD_Reduce",          "Aerobics Classroom - Increases Encumbrance Limits. *Your Weight Class will now change every X%. +X% Encumbrance per rank",                               1,    1,    8,              0.01f ) },
                { "XP_Up",                     new ScaledConfigItemF( this.Config, "Upgrades", "XP_Up",                     "Trophy Room - Increase XP Gain. +X% XP per rank",                                                                                                        2.5f, 0.1f, float.MaxValue, 0.01f ) },
            };
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Change the stat gain just before the firest level stat gain is read
        [HarmonyPatch( typeof( SkillTreeObj ), nameof( SkillTreeObj.FirstLevelStatGain ), MethodType.Getter )]
        static class SkillTreeObj_FirstLevelStatGain_Patch {
            static void Prefix( SkillTreeObj __instance ) {
                // Putting this in a variable just to shorten the name and make the rest of this easier to read
                SkillTreeData skill = __instance.SkillTreeData;
                // Create a variable for the config file element
                IScaledConfigItem skillConfig;
                // Search the config for a setting that has the same name as the internal name of the skill
                if( configSkills.TryGetValue( skill.Name, out skillConfig ) ) {
                    // Check if the stat gain is already what we want it to be
                    if( skillConfig.ScaledValue != skill.FirstLevelStatGain || skillConfig.ScaledValue != skill.AdditionalLevelStatGain ) {
                        // Report the change to the log
                        WobPlugin.Log( "Changing bonus for " + skill.Name + " from " + skill.FirstLevelStatGain + "/" + skill.AdditionalLevelStatGain + " to " + skillConfig.ScaledValue );
                        // Set the stat gain for all ranks
                        skill.FirstLevelStatGain = skillConfig.ScaledValue;
                        skill.AdditionalLevelStatGain = skillConfig.ScaledValue;
                    }
                }
            }
        }

        // Reroll for heirs seems to ignore the stat gain and just use the upgrade level - this is to fix that
        // Patch for the method that controls initial setup of the UI elements for character selection, including reroll
        [HarmonyPatch( typeof( LineageWindowController ), "OnOpen" )]
        static class LineageWindowController_OnOpen_Patch {
            static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Put the instructions into a list for easier manipulation
                List<CodeInstruction> codes = new List<CodeInstruction>( instructions );
                WobPlugin.Log( "Searching opcodes" );
                // Iterate through the instruction codes
                for( int i = 0; i < codes.Count; i++ ) {
                    // Search for the call to get the level of the reroll upgrade
                    if( codes[i].opcode == OpCodes.Call && ( codes[i].operand as MethodInfo ).Name == "GetSkillObjLevel" ) {
                        WobPlugin.Log( "Found matching opcode at " + i + ": " + codes[i].ToString() );
                        // Replace the call with one that gets the upgrade itself
                        codes[i].operand = AccessTools.Method( typeof( SkillTreeManager), "GetSkillTreeObj", new System.Type[] { typeof( SkillTreeType ) } );
                        // Add a call to the upgrade's CurrentStatGain getter
                        codes.Insert( i + 1, new CodeInstruction( OpCodes.Callvirt, AccessTools.Method( typeof( SkillTreeObj ), "get_CurrentStatGain" ) ) );
                        // Cast the float return value to int
                        codes.Insert( i + 2, new CodeInstruction( OpCodes.Conv_I4 ) );
                    }
                }
                // Return the modified instructions to complete the patch
                return codes.AsEnumerable();
            }
        }

        // This patch simply dumps skill tree data to the debug log when the Manor skill tree is opened - useful for getting internal names and default values for the upgrades
        /*[HarmonyPatch( typeof( SkillTreeWindowController ), nameof( SkillTreeWindowController.Initialize ) )]
        static class SkillTreeWindowController_Initialize_Patch {
            static void Postfix( SkillTreeWindowController __instance ) {
                foreach( SkillTreeType skillTreeType in SkillTreeType_RL.TypeArray ) {
                    if( skillTreeType != SkillTreeType.None ) {
                        SkillTreeData skillData = SkillTreeManager.GetSkillTreeObj( skillTreeType ).SkillTreeData;
                        WobPlugin.Log( skillData.Name + "|" + skillData.FirstLevelStatGain + "|" + skillData.AdditionalLevelStatGain + "|" + skillData.MaxLevel + "|" + skillData.OverloadLevelCap + "|" + LocalizationManager.GetString( skillData.Title, false, false ) );
                    }
                }
            }
        }*/
    }
}