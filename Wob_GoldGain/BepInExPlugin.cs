using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_GoldGain {
    [BepInPlugin( "Wob.GoldGain", "Gold Gain Mod", "0.1.0" )]
    public partial class BepInExPlugin : BaseUnityPlugin {
        // Configuration file entries, globally accessible for patches
        public static ScaledConfigItemI configGoldGain;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configGoldGain = new ScaledConfigItemI( this.Config, "Options", "GoldGain", "Gain +X% gold on all characters", 0, 0, int.MaxValue, 0.01f );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        [HarmonyPatch( typeof( SkillTreeLogicHelper ), nameof( SkillTreeLogicHelper.GetGoldGainMod ) )]
        static class SkillTreeLogicHelper_GetGoldGainMod_Patch {
            static void Postfix( ref float __result ) {
                __result += configGoldGain.ScaledValue;
            }
        }
    }
}