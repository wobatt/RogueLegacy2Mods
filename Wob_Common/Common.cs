using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Reflection;

namespace Wob_Common {

    public static class WobPlugin {
        // Static reference to the BepInEx logger for debugging
        private static BepInEx.Logging.ManualLogSource bepInExLog;

        // These hold the values read from the config file
        public static bool Enabled { get; private set; }
        private static bool Debug { get; set; }

        // Set up the log for debugging and create/read the basic mod settings
        public static void Initialise( BaseUnityPlugin plugin, BepInEx.Logging.ManualLogSource log ) {
            // Save a reference to the logger to be used later
            bepInExLog = log;

            // Basic settings used in all mods
            Enabled = new ConfigItem<bool>( plugin.Config, "General", "Enabled", "Enable this mod",   true,  new bool[] { true, false } ).Value;
            Debug   = new ConfigItem<bool>( plugin.Config, "General", "IsDebug", "Enable debug logs", false, new bool[] { true, false } ).Value;
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
        public static void Log( string message ) {
            if( Debug ) {
                // Simply pass the message through - the BepInEx logger takes care of adding the mod name, etc.
                bepInExLog.LogMessage( message );
            }
        }
    }

    // Wrapper class for binding config file options with validation
    public class ConfigItem<T> where T : IComparable, IEquatable<T> {
        protected readonly ConfigEntry<T> configEntry;

        // Public constructors with variations for how to restrict/validate config values
        public ConfigItem( ConfigFile config, string section, string key, string desc, T value, T min, T max ) : this( config, section, key, desc, value, new AcceptableValueRange<T>( min, max ), null ) {}
        public ConfigItem( ConfigFile config, string section, string key, string desc, T value, T min, T max, Func<T, T> limits ) : this( config, section, key, desc, value, new AcceptableValueRange<T>( min, max ), limits ) {}
        public ConfigItem( ConfigFile config, string section, string key, string desc, T value, T[] acceptedValues ) : this( config, section, key, desc, value, new AcceptableValueList<T>( acceptedValues ), null ) {}
        public ConfigItem( ConfigFile config, string section, string key, string desc, T value, T[] acceptedValues, Func<T, T> limits ) : this( config, section, key, desc, value, new AcceptableValueList<T>( acceptedValues ), limits ) {}

        // Base constructor that binds the option in the config file and applies the additional limiter function
        private ConfigItem( ConfigFile config, string section, string key, string desc, T value, AcceptableValueBase acceptableValues, Func<T, T> limits ) {
            this.configEntry = config.Bind( section, key, value, new ConfigDescription( desc, acceptableValues ) );
            if( limits != null ) {
                this.configEntry.Value = limits( this.configEntry.Value );
            }
        }

        // Read the current setting value
        public T Value { get => this.configEntry.Value; }
    }

    // Extended config item for calculating skill stat gain float from a scaled int
    public class SkillConfig : ConfigItem<int> {
        public SkillConfig( ConfigFile config, string section, string key, string desc, int value, int min, int max, float scaler ) : base( config, section, key, desc, value, min, max ) {
            this.StatGain = scaler * this.configEntry.Value;
        }
        public float StatGain { get; private set; }
    }

}
