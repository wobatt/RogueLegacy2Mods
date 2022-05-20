using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_ResourceGain {
    [BepInPlugin( "Wob.ResourceGain", "Resource Gain Mod", "0.1.0" )]
    public partial class GoldGain : BaseUnityPlugin {
        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobPlugin.Settings.Add( new WobSettings.Entry[] {
                new WobSettings.Scaled<int>( "GoldGain",   "Gain +X% gold on all characters", 0, 0.01f, bounds: (0, 1000000) ),
                new WobSettings.Scaled<int>( "OreGain",    "Gain +X% ore",                    0, 0.01f, bounds: (0, 1000000) ),
                new WobSettings.Scaled<int>( "AetherGain", "Gain +X% aether",                 0, 0.01f, bounds: (0, 1000000) ),
            } );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        [HarmonyPatch( typeof( SkillTreeLogicHelper ), nameof( SkillTreeLogicHelper.GetGoldGainMod ) )]
        static class SkillTreeLogicHelper_GetGoldGainMod_Patch {
            static void Postfix( ref float __result ) {
                __result += WobPlugin.Settings.Get( "GoldGain", 0f );
            }
        }

        [HarmonyPatch( typeof( SkillTreeLogicHelper ), nameof( SkillTreeLogicHelper.GetEquipmentOreMod ) )]
        static class SkillTreeLogicHelper_GetEquipmentOreMod_Patch {
            static void Postfix( ref float __result ) {
                __result += WobPlugin.Settings.Get( "OreGain", 0f );
            }
        }

        [HarmonyPatch( typeof( SkillTreeLogicHelper ), nameof( SkillTreeLogicHelper.GetRuneOreMod ) )]
        static class SkillTreeLogicHelper_GetRuneOreMod_Patch {
            static void Postfix( ref float __result ) {
                __result += WobPlugin.Settings.Get( "AetherGain", 0f );
            }
        }
    }
}