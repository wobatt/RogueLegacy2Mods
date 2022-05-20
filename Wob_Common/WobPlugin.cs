using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace Wob_Common {
    internal static class WobPlugin {
        public const bool ERROR = true;
        // Static reference to the BepInEx logger for debugging
        private static BepInEx.Logging.ManualLogSource bepInExLog;

        // Static reference to the config file
        public static WobSettings Settings { get; private set; }

        // These hold the values read from the config file
        public static bool Enabled { get; private set; }
        private static bool Debug { get; set; } = true;

        // Set up the log for debugging and create/read the basic mod settings
        public static void Initialise( BaseUnityPlugin plugin, BepInEx.Logging.ManualLogSource log ) {
            // Save a reference to the logger to be used later
            bepInExLog = log;
            // Save a reference to the config file
            Settings = new WobSettings( plugin.Config );
            // Create the basic settings used in all mods
            Settings.Add( new List<WobSettings.Entry> {
                new WobSettings.EntryBool( "General", "Enabled", "Enable this mod",   true  ),
                new WobSettings.EntryBool( "General", "IsDebug", "Enable debug logs", false ),
            } );
            // Read the basic settings and put the values in properties for easier access
            Enabled = Settings.Get( "General", "Enabled", true );
            Debug   = Settings.Get( "General", "IsDebug", true );
        }

        // Check if the mod is enabled, and if so apply the patches
        public static void Patch() {
            if( Enabled ) {
                // Perform the patching
                Harmony.CreateAndPatchAll( Assembly.GetExecutingAssembly(), null );
                // Report success
                Log( "Plugin awake" );
            } else {
                // Report disabled
                Log( "Plugin disabled" );
            }
        }

        // Send a message to the BepInEx log file
        public static void Log( string message, bool error = false ) {
            if( Debug || error ) {
                // Simply pass the message through - the BepInEx logger takes care of adding the mod name, etc.
                bepInExLog.LogMessage( message );
            }
        }

        // Helper to get the UI names of a trait
        public static string GetTraitTitles( TraitData traitData ) {
            // Each trait has 4 possible names - scientific/non-scientific and male/female character
            // First get all 4 variants
            string tScientificM = LocalizationManager.GetString( traitData.Title, false, false );
            string tScientificF = LocalizationManager.GetString( traitData.Title, true, false );
            string tNonScientificM = LocalizationManager.GetString( traitData.Title.Replace( "_1", "_2" ), false, false );
            string tNonScientificF = LocalizationManager.GetString( traitData.Title.Replace( "_1", "_2" ), true, false );
            // Build a return string, suppressing variants if they are the same as one already added
            return tScientificM + ( tScientificM == tScientificF ? "" : "/" + tScientificF ) + ( tScientificM == tNonScientificM ? "" : "/" + tNonScientificM ) + ( ( tNonScientificM == tNonScientificF || tScientificF == tNonScientificF ) ? "" : "/" + tNonScientificF );
        }
    }
}
