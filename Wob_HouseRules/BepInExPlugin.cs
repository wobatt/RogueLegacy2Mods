using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Wob_Common;

namespace Wob_HouseRules {
    [BepInPlugin("Wob.HouseRules", "House Rules Mod", "0.1.0")]
    public partial class BepInExPlugin : BaseUnityPlugin {
        // Configuration file entries, globally accessible for patches
        public static ConfigItem<int> configMaxDifficulty;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configMaxDifficulty = new ConfigItem<int>( this.Config, "Options", "MaxDifficulty", "Maximum percentage that the enemy health and damage house rules will go up to", 1000, 200, int.MaxValue, x => { return (int)( System.Math.Floor( x / 5f ) * 5f ); } );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Patch for the method that gets the gold cost for a specific upgrade with labour costs included
        [HarmonyPatch( typeof( ChangeAssistStatModOptionItem ), nameof( ChangeAssistStatModOptionItem.Initialize ) )]
        static class SkillTreeObj_GoldCostWithLevelAppreciation_Patch {
            // Change the minimum values of the house rule sliders
            static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Put the instructions into a list for easier manipulation
                List<CodeInstruction> codes = new List<CodeInstruction>( instructions );
                WobPlugin.Log( "Searching opcodes" );
                // Track how many of the int32 values (maximums) have been found
                byte found_ldc_i4 = 0;
                // Track how many of the int8 values (minimums) have been found
                byte found_ldc_i4_s = 0;
                // Iterate through the instruction codes
                for( int i = 0; i < codes.Count; i++ ) {
                    // Search for instruction 'ldc.i4' which pushes an int32 onto the stack, looking for maximum values
                    if( codes[i].opcode == OpCodes.Ldc_I4 ) {
                        WobPlugin.Log( "Found matching opcode at " + i + ": " + codes[i].ToString() );
                        // We are finding 3 specific operand values in order
                        switch( found_ldc_i4 ) {
                            case 0:
                                // Check if the operand is correct for 200% enemy health
                                if( (int)codes[i].operand == 200 ) {
                                    WobPlugin.Log( "Enemy Health Max: correct operand - patching" );
                                    // Set the operand to 1000%
                                    codes[i].operand = configMaxDifficulty.Value;
                                }
                                found_ldc_i4++;
                                break;
                            case 1:
                                // Check if the operand is correct for 200% enemy damage
                                if( (int)codes[i].operand == 200 ) {
                                    WobPlugin.Log( "Enemy Damage Max: correct operand - patching" );
                                    // Set the operand to 1000%
                                    codes[i].operand = configMaxDifficulty.Value;
                                }
                                found_ldc_i4++;
                                break;
                        }
                    }
                    // Search for instruction 'ldc.i4.s' which pushes an int8 onto the stack, looking for minimum values
                    if( codes[i].opcode == OpCodes.Ldc_I4_S ) {
                        WobPlugin.Log( "Found matching opcode at " + i + ": " + codes[i].ToString() );
                        // We are finding 3 specific operand values in order
                        switch( found_ldc_i4_s ) {
                            case 0:
                                // Check if the operand is correct for 50% enemy health
                                if( (sbyte)codes[i].operand == 50 ) {
                                    WobPlugin.Log( "Enemy Health Min: correct operand - patching" );
                                    // Set the operand to 5%
                                    codes[i].operand = (sbyte)5;
                                }
                                found_ldc_i4_s++;
                                break;
                            case 1:
                                // Check if the operand is correct for 50% enemy damage
                                if( (sbyte)codes[i].operand == 50 ) {
                                    WobPlugin.Log( "Enemy Damage Min: correct operand - patching" );
                                    // Set the operand to 0% (god mode)
                                    codes[i].operand = (sbyte)0;
                                }
                                found_ldc_i4_s++;
                                break;
                            case 2:
                                // Check if the operand is correct for 25% aim time slow
                                if( (sbyte)codes[i].operand == 25 ) {
                                    WobPlugin.Log( "Aim Time Slow Min: correct operand - patching" );
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