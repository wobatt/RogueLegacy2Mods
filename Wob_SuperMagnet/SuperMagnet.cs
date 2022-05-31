using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_SuperMagnet {
    [BepInPlugin( "Wob.SuperMagnet", "Super Magnet Mod", "0.1.0" )]
    public partial class SuperMagnet : BaseUnityPlugin {
		// Main method that kicks everything off
		protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobSettings.Add( new WobSettings.Num<float>( "DistanceScaler", "Multiply magnet distance by this", 1f, bounds: (0f, 1000000f) ) );
			// Apply the patches if the mod is enabled
			WobPlugin.Patch();
        }

		// Patch for the method that gets the gold cost for a specific upgrade with labour costs included
		[HarmonyPatch( typeof( RuneLogicHelper ), nameof( RuneLogicHelper.GetMagnetDistance ) )]
        internal static class RuneLogicHelper_GetMagnetDistance_Patch {
            internal static void Postfix( ref float __result ) {
                // Calculate the new cost and overwrite the original return value
                __result *= WobSettings.Get( "DistanceScaler", 1f );
            }
        }
    }
}