using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Wob_HouseRules {
    [BepInPlugin("Wob.HouseRules", "House Rules Mod", "0.1.0")]
    public partial class BepInExPlugin : BaseUnityPlugin
    {
        // Static reference to the BepInEx logger for debugging
        private static BepInEx.Logging.ManualLogSource debugLog;

        // Configuration file entries, globally accessible for patches
        public static ConfigEntry<bool> configModEnabled;
        public static ConfigEntry<bool> configDebugLogs;
        public static ConfigEntry<int> configMaxDifficulty;

        // Send a message to the BepInEx log file
        public static void DebugLog( string message ) {
            if ( configDebugLogs.Value ) {
                debugLog.LogMessage( message );
            }
        }

        // Main method that kicks everything off
        private void Awake() {
            // Set static reference to logger for use in patches
            debugLog = this.Logger;

            // Basic mod settings
            configModEnabled = this.Config.Bind( "General", "Enabled", true, new ConfigDescription( "Enable this mod", new AcceptableValueList<bool>( new bool[] { true, false } ) ) );
            configDebugLogs = this.Config.Bind( "General", "IsDebug", false, new ConfigDescription( "Enable debug logs", new AcceptableValueList<bool>( new bool[] { true, false } ) ) );

            // Create/read the configuration options for how the labour costs should be calculated
            configMaxDifficulty = this.Config.Bind( "Options", "MaxDifficulty", 1000, new ConfigDescription( "Maximum percentage that the enemy health and damage house rules will go up to.", new AcceptableValueRange<int>( 200, int.MaxValue ) ) );
            configMaxDifficulty.Value = (int)( System.Math.Floor( configMaxDifficulty.Value / 5f ) * 5f );

            // Check if the mod has been disabled in the options
            if( configModEnabled.Value ) {
                // Perform the patching
                Harmony.CreateAndPatchAll( Assembly.GetExecutingAssembly(), null );
                DebugLog( "Plugin awake" );
            } else {
                DebugLog( "Plugin disabled" );
            }
        }

        // Patch for the method that gets the gold cost for a specific upgrade with labour costs included
        [HarmonyPatch( typeof( ChangeAssistStatModOptionItem ), nameof( ChangeAssistStatModOptionItem.Initialize ) )]
        static class SkillTreeObj_GoldCostWithLevelAppreciation_Patch {
            // Change the minimum values of the house rule sliders
            static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Put the instructions into a list for easier manipulation
                List<CodeInstruction> codes = new List<CodeInstruction>( instructions );
                DebugLog( "Searching opcodes" );
                // Track how many of the int32 values (maximums) have been found
                byte found_ldc_i4 = 0;
                // Track how many of the int8 values (minimums) have been found
                byte found_ldc_i4_s = 0;
                // Iterate through the instruction codes
                for( int i = 0; i < codes.Count; i++ ) {
                    // Search for instruction 'ldc.i4' which pushes an int32 onto the stack, looking for maximum values
                    if( codes[i].opcode == OpCodes.Ldc_I4 ) {
                        DebugLog( "Found matching opcode at " + i + ": " + codes[i].ToString() );
                        // We are finding 3 specific operand values in order
                        switch( found_ldc_i4 ) {
                            case 0:
                                // Check if the operand is correct for 200% enemy health
                                if( (int)codes[i].operand == 200 ) {
                                    DebugLog( "Enemy Health Max: correct operand - patching" );
                                    // Set the operand to 1000%
                                    codes[i].operand = configMaxDifficulty.Value;
                                }
                                found_ldc_i4++;
                                break;
                            case 1:
                                // Check if the operand is correct for 200% enemy damage
                                if( (int)codes[i].operand == 200 ) {
                                    DebugLog( "Enemy Damage Max: correct operand - patching" );
                                    // Set the operand to 1000%
                                    codes[i].operand = configMaxDifficulty.Value;
                                }
                                found_ldc_i4++;
                                break;
                        }
                    }
                    // Search for instruction 'ldc.i4.s' which pushes an int8 onto the stack, looking for minimum values
                    if( codes[i].opcode == OpCodes.Ldc_I4_S ) {
                        DebugLog( "Found matching opcode at " + i + ": " + codes[i].ToString() );
                        // We are finding 3 specific operand values in order
                        switch( found_ldc_i4_s ) {
                            case 0:
                                // Check if the operand is correct for 50% enemy health
                                if( (sbyte)codes[i].operand == 50 ) {
                                    DebugLog( "Enemy Health Min: correct operand - patching" );
                                    // Set the operand to 5%
                                    codes[i].operand = (sbyte)5;
                                }
                                found_ldc_i4_s++;
                                break;
                            case 1:
                                // Check if the operand is correct for 50% enemy damage
                                if( (sbyte)codes[i].operand == 50 ) {
                                    DebugLog( "Enemy Damage Min: correct operand - patching" );
                                    // Set the operand to 0% (god mode)
                                    codes[i].operand = (sbyte)0;
                                }
                                found_ldc_i4_s++;
                                break;
                            case 2:
                                // Check if the operand is correct for 25% aim time slow
                                if( (sbyte)codes[i].operand == 25 ) {
                                    DebugLog( "Aim Time Slow Min: correct operand - patching" );
                                    // Set the operand to 5% (setting this to 0% will soft lock the game)
                                    codes[i].operand = (sbyte)5;
                                }
                                found_ldc_i4_s++;
                                break;
                        }
                    }
                }
                // Return the modified instructions to complete the patch
                return codes.AsEnumerable();
            }
        }
    }
}