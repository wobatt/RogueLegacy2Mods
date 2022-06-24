using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_PishonLight {
    [BepInPlugin( "Wob.PishonLight", "Pishon Light Mod", "0.1.0" )]
    public partial class PishonLight : BaseUnityPlugin {
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        [HarmonyPatch( typeof( CaveLanternPostProcessingController ), "DarknessAmountWhenFullyLit", MethodType.Getter )]
        internal static class CaveLanternPostProcessingController_DarknessAmountWhenFullyLit_Patch {
            internal static void Postfix( ref float __result ) {
                __result = 0f;
            }
        }
    }
}