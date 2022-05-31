using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_ResourceGain {
    [BepInPlugin( "Wob.ResourceGain", "Resource Gain Mod", "1.0.0" )]
    public partial class GoldGain : BaseUnityPlugin {
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Num<int>( "GoldGain",   "Gain +X% gold on all characters", 0, 0.01f, bounds: (0, 1000000) ),
                new WobSettings.Num<int>( "OreGain",    "Gain +X% ore",                    0, 0.01f, bounds: (0, 1000000) ),
                new WobSettings.Num<int>( "AetherGain", "Gain +X% aether",                 0, 0.01f, bounds: (0, 1000000) ),
            } );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        [HarmonyPatch( typeof( SkillTreeLogicHelper ), nameof( SkillTreeLogicHelper.GetGoldGainMod ) )]
        internal static class SkillTreeLogicHelper_GetGoldGainMod_Patch {
            internal static void Postfix( ref float __result ) {
                __result += WobSettings.Get( "GoldGain", 0f );
            }
        }

        [HarmonyPatch( typeof( SkillTreeLogicHelper ), nameof( SkillTreeLogicHelper.GetEquipmentOreMod ) )]
        internal static class SkillTreeLogicHelper_GetEquipmentOreMod_Patch {
            internal static void Postfix( ref float __result ) {
                __result += WobSettings.Get( "OreGain", 0f );
            }
        }

        [HarmonyPatch( typeof( SkillTreeLogicHelper ), nameof( SkillTreeLogicHelper.GetRuneOreMod ) )]
        internal static class SkillTreeLogicHelper_GetRuneOreMod_Patch {
            internal static void Postfix( ref float __result ) {
                __result += WobSettings.Get( "AetherGain", 0f );
            }
        }
    }
}