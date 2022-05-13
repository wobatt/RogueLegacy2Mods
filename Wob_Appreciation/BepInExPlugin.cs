using System;
using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_Appreciation {
    [BepInPlugin( "Wob.Appreciation", "Upgrade Appreciation Mod", "0.1.0" )]
    public partial class BepInExPlugin : BaseUnityPlugin {
        // Configuration file entries, globally accessible for patches
        public static ConfigItem<float> configAppreciation;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configAppreciation = new ConfigItem<float>( this.Config, "Options", "Appreciation", "Multiply level cost appreciation by this", 1f, 0f, float.MaxValue );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        [HarmonyPatch( typeof( SkillTreeObj ), nameof( SkillTreeObj.GoldCost ), MethodType.Getter )]
        static class SkillTreeObj_GoldCost_Patch {
            static void Postfix( SkillTreeObj __instance, ref int __result ) {
                __result = __instance.SkillTreeData.BaseCost + (int)Math.Floor( __instance.SkillTreeData.Appreciation * configAppreciation.Value ) * __instance.Level;
            }
        }
    }
}