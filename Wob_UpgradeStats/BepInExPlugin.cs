using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using Wob_Common;

namespace Wob_UpgradeStats {
    [BepInPlugin( "Wob.UpgradeStats", "Upgrade Stat Gains Mod", "0.1" )]
    public partial class BepInExPlugin : BaseUnityPlugin {
        // Static reference to the config item collection so it can be searched in the patch
        public static Dictionary<string, SkillConfig> configSkills;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configSkills = new Dictionary<string, SkillConfig> {
                { "Architect_Cost_Down", new SkillConfig( this.Config, "Upgrades", "Architect_Cost_Down", "Drill Store - percent Architect fee reduction per rank",             2,  1, 20,           0.01f ) },
                { "Dash_Strike_Up",      new SkillConfig( this.Config, "Upgrades", "Dash_Strike_Up",      "Jousting Studies - percent damage reduction while dashing per rank", 5,  1, 20,           0.01f ) },
                { "Down_Strike_Up",      new SkillConfig( this.Config, "Upgrades", "Down_Strike_Up",      "Bamboo Garden - percent INT scaling per rank",                       10, 1, 20,           0.01f ) },
                { "Ore_Find_Up",         new SkillConfig( this.Config, "Upgrades", "Ore_Find_Up",         "Geologist's Camp - percent chance of finding ore per rank",          1,  1, 20,           0.01f ) },
                { "Relic_Cost_Down",     new SkillConfig( this.Config, "Upgrades", "Relic_Cost_Down",     "Archaeology Camp - percent decrease in relic resolve cost per rank", 1,  1, 20,           0.01f ) },
                { "Resolve_Up",          new SkillConfig( this.Config, "Upgrades", "Resolve_Up",          "Psychiatrist - percent increase in starting resolve per rank",       1,  1, int.MaxValue, 0.01f ) },
                { "Rune_Ore_Find_Up",    new SkillConfig( this.Config, "Upgrades", "Rune_Ore_Find_Up",    "Dowsing Center - percent chance of finding aether per rank",         1,  1, 20,           0.01f ) },
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
                SkillConfig skillConfig;
                // Search the config for a setting that has the same name as the internal name of the skill
                if( configSkills.TryGetValue( skill.Name, out skillConfig ) ) {
                    // Check if the stat gain is already what we want it to be
                    if( skillConfig.StatGain != skill.FirstLevelStatGain || skillConfig.StatGain != skill.AdditionalLevelStatGain ) {
                        // Set the stat gain for all ranks
                        skill.FirstLevelStatGain = skillConfig.StatGain;
                        skill.AdditionalLevelStatGain = skillConfig.StatGain;
                        // Report the change to the log
                        WobPlugin.Log( "Changing bonus for " + skill.Name + "  from " + skill.FirstLevelStatGain + "/" + skill.AdditionalLevelStatGain + " to " + skillConfig.StatGain );
                    }
                }
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