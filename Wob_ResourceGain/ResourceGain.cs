using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_ResourceGain {
    [BepInPlugin( "Wob.ResourceGain", "Resource Gain Mod", "0.1.0" )]
    public partial class GoldGain : BaseUnityPlugin {
        // Configuration file entries, globally accessible for patches
        public static ScaledConfigItemI configGoldGain;
        public static ScaledConfigItemI configOreGain;
        public static ScaledConfigItemI configAetherGain;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configGoldGain = new ScaledConfigItemI( this.Config, "Options", "GoldGain", "Gain +X% gold on all characters", 0, 0, int.MaxValue, 0.01f );
            configOreGain = new ScaledConfigItemI( this.Config, "Options", "OreGain", "Gain +X% ore", 0, 0, int.MaxValue, 0.01f );
            configAetherGain = new ScaledConfigItemI( this.Config, "Options", "AetherGain", "Gain +X% aether", 0, 0, int.MaxValue, 0.01f );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        [HarmonyPatch( typeof( SkillTreeLogicHelper ), nameof( SkillTreeLogicHelper.GetGoldGainMod ) )]
        static class SkillTreeLogicHelper_GetGoldGainMod_Patch {
            static void Postfix( ref float __result ) {
                __result += configGoldGain.ScaledValue;
            }
        }

        [HarmonyPatch( typeof( SkillTreeLogicHelper ), nameof( SkillTreeLogicHelper.GetEquipmentOreMod ) )]
        static class SkillTreeLogicHelper_GetEquipmentOreMod_Patch {
            static void Postfix( ref float __result ) {
                __result += configOreGain.ScaledValue;
            }
        }

        [HarmonyPatch( typeof( SkillTreeLogicHelper ), nameof( SkillTreeLogicHelper.GetRuneOreMod ) )]
        static class SkillTreeLogicHelper_GetRuneOreMod_Patch {
            static void Postfix( ref float __result ) {
                __result += configAetherGain.ScaledValue;
            }
        }
    }
}