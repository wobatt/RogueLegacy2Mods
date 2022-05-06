using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_Accessibility {
    [BepInPlugin( "Wob.Accessibility", "Accessibility Mod", "0.1" )]
    public class BepInExPlugin : BaseUnityPlugin {
        // Configuration file entries, globally accessible for patches
        public static ConfigItemBool configChromaticAbberation;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configChromaticAbberation = new ConfigItemBool( this.Config, "Options", "ChromaticAbberation", "Allow chromatic abberation effects", false );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Patch for the method that gets the gold increase for a trait
        [HarmonyPatch( typeof( MobilePostProcessing ), "ApplyProfileChromaticAbberation" )]
        static class MobilePostProcessing_ApplyProfileChromaticAbberation_Patch {
            static void Prefix( MobilePostProcessing __instance, MobilePostProcessingProfile profile ) {
                // Check if the profile has chromatic aberations enabled and there is a setting disabling them
                if( profile.EnableChromaticAbberationEffect && !configChromaticAbberation.Value ) {
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