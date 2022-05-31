using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_Accessibility {
    [BepInPlugin( "Wob.Accessibility", "Accessibility Mod", "1.0.0" )]
    public class Accessibility : BaseUnityPlugin {
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobSettings.Add( new WobSettings.Boolean( "ChromaticAbberation", "Allow chromatic abberation effects", false ) );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Patch for the method that gets the gold increase for a trait
        [HarmonyPatch( typeof( MobilePostProcessing ), "ApplyProfileChromaticAbberation" )]
        internal static class MobilePostProcessing_ApplyProfileChromaticAbberation_Patch {
            internal static void Prefix( MobilePostProcessing __instance, MobilePostProcessingProfile profile ) {
                // Check if the profile has chromatic aberations enabled and there is a setting disabling them
                if( profile.EnableChromaticAbberationEffect && !WobSettings.Get( "ChromaticAbberation", false ) ) {
                    // Disable the effect and the distortion with it - these values are taken from the ResetChromaticAbberation method
                    profile.EnableChromaticAbberationEffect = false;
                    profile.Offset = 1f;
                    profile.FishEyeDistortion = 0f;
                    // Call the reset method to make certain that the effect is turned off now
                    Traverse.Create( __instance ).Method( "ResetChromaticAbberation" ).GetValue();
                }
            }
        }
    }
}