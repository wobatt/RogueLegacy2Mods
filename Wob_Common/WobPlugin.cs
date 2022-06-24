using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace Wob_Common {
    internal static class WobPlugin {
        /// <summary>
        /// Static reference to the BepInEx logger for debugging
        /// </summary>
        private static ManualLogSource bepInExLog;

        /// <summary>
        /// Reference to the plugin's metadata.
        /// </summary>
        internal static PluginInfo Info { get; private set; }

        /// <summary>
        /// Reference to the plugin's config file.
        /// </summary>
        internal static ConfigFile Config { get; private set; }

        /// <summary>
        /// Property to state if the mod is enabled, set from the value in the config file.
        /// </summary>
        public static bool Enabled { get; private set; }
        
        /// <summary>
        /// Property to state if the mod should print debug info to the log file, set from the value in the config file.
        /// </summary>
        public static bool Debug { get; private set; } = true;

        /// <summary>
        /// Set up the log for debugging and create/read the basic mod settings in the config file.
        /// </summary>
        /// <param name="plugin">Reference to the plugin's main object.</param>
        /// <param name="log">Reference to the BepInEx log for debug output.</param>
        public static void Initialise( BaseUnityPlugin plugin, ManualLogSource log ) {
            // Save a reference to the logger to be used later
            bepInExLog = log;
            // Save a reference to the config file
            Config = plugin.Config;
            // Save a reference to the plugin metadata
            Info = plugin.Info;
            // Create the basic settings used in all mods
            WobSettings.Add( new List<WobSettings.Entry> {
                new WobSettings.Boolean( "Basic", "Enabled", "Enable this mod",   true  ),
                new WobSettings.Boolean( "Basic", "IsDebug", "Enable debug logs", false ),
            } );
            // Read the basic settings and put the values in properties for easier access
            Enabled = WobSettings.Get( "Basic", "Enabled", true );
            Debug   = WobSettings.Get( "Basic", "IsDebug", true );
        }

        /// <summary>
        /// Check if the mod is enabled, and if so tell Harmony to apply the patches.
        /// </summary>
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

        /// <summary>
        /// Send a message to the BepInEx log file.
        /// </summary>
        /// <param name="message">The text to be added to the log file.</param>
        /// <param name="error">If <see langword="true"/>, this overrides the debug flag to print serious errors to the log. Use sparingly.</param>
        public static void Log( string message, bool error = false ) {
            if( Debug || error ) {
                // Simply pass the message through - the BepInEx logger takes care of adding the mod name, etc.
                bepInExLog.LogMessage( message );
            }
        }
        public const bool ERROR = true;
    }
}
