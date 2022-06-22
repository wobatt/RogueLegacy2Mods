using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_Accessibility {
    [BepInPlugin( "Wob.Accessibility", "Accessibility Mod", "1.1.0" )]
    public class Accessibility : BaseUnityPlugin {
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobSettings.Add( new WobSettings.Boolean( "ChromaticAbberation", "Allow chromatic abberation effects", true ) );
            WobSettings.Add( new WobSettings.Boolean( "Fog",                 "Allow fog effects",                  true ) );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Patch for the method that applies the chromatic aberation effect
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

        // Patch for the method that applies the fog effect
        [HarmonyPatch( typeof( MobilePostProcessing ), "ApplyProfileMist" )]
        internal static class MobilePostProcessing_ApplyProfileMist_Patch {
            internal static void Prefix( MobilePostProcessing __instance, MobilePostProcessingProfile profile ) {
                // Check if the profile has fog enabled and there is a setting disabling them
                if( profile.EnableMistEffect && !WobSettings.Get( "Fog", false ) ) {
                    // Disable the effect
                    profile.EnableMistEffect = false;
                    // Call the reset method to make certain that the effect is turned off now
                    Traverse.Create( __instance ).Method( "ResetMist" ).GetValue();
                }
            }
        }
    }
}