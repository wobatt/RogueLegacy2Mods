using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;

namespace Wob_LabourCosts {
    [BepInPlugin("Wob.LabourCosts", "Labour Costs Mod", "0.1.0")]
    public partial class BepInExPlugin : BaseUnityPlugin
    {
        // Static reference to the BepInEx logger for debugging
        private static BepInEx.Logging.ManualLogSource debugLog;

        // Configuration file entries, globally accessible for patches
        public static ConfigEntry<bool> configModEnabled;
        public static ConfigEntry<bool> configDebugLogs;
        public static ConfigEntry<sbyte> configStartLevel;
        public static ConfigEntry<float> configPerLevel;
        public static ConfigEntry<int> configRoundTo;

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
            configStartLevel = this.Config.Bind( "Options", "StartLevel", (sbyte)18, new ConfigDescription( "Level after which labour costs start.", new AcceptableValueRange<sbyte>( 0, sbyte.MaxValue ) ) );
            configPerLevel = this.Config.Bind( "Options", "PerLevel", 14f, new ConfigDescription( "Cost increase per level. Set to 0 to remove labour costs.", new AcceptableValueRange<float>( 0, float.MaxValue ) ) );
            configRoundTo = this.Config.Bind( "Options", "RoundTo", 5, new ConfigDescription( "Round down calculated cost to this significance.", new AcceptableValueRange<int>( 1, int.MaxValue ) ) );
            
            // Check if the mod has been disabled in the options
            if( configModEnabled.Value ) {
                // Perform the patching
                Harmony.CreateAndPatchAll( Assembly.GetExecutingAssembly(), null );
                DebugLog( "Plugin awake" );
            } else {
                DebugLog( "Plugin disabled" );
            }
        }

        // Calculate new labour costs based on config parameters
        private static int NewLabourCost() {
            int labourLevel = Mathf.Clamp( SkillTreeManager.GetTotalSkillObjLevel() - configStartLevel.Value, 0, int.MaxValue );
            return (int)( System.Math.Floor( ( labourLevel * configPerLevel.Value ) / configRoundTo.Value ) * configRoundTo.Value );
        }

        // Patch for the method that sets the text on the labour cost UI element in the corner of the castle/skill tree
        [HarmonyPatch( typeof( SkillTreeWindowController ), "UpdateLabourCosts" )]
        static class SkillTreeWindowController_UpdateLabourCosts_Patch {
            // Change the text on the box to the new number
            static void Postfix( SkillTreeWindowController __instance ) {
                // The text field is private, so grab a reference with reflection
                TMP_Text m_labourCostText = (TMP_Text)Traverse.Create( __instance ).Field( "m_labourCostText" ).GetValue();
                // Calculate the new cost and set the text
                m_labourCostText.text = NewLabourCost().ToString();
            }

            // Change the starting level in the method, which sets whether the labour cost box is displayed
            static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Put the instructions into a list for easier manipulation
                List<CodeInstruction> codes = new List<CodeInstruction>( instructions );
                DebugLog( "Searching opcodes" );
                // Iterate through the instruction codes
                for( int i = 0; i < codes.Count; i++ ) {
                    // Search for instruction 'ldc.i4.s' which pushes an int8 onto the stack
                    if( codes[i].opcode == OpCodes.Ldc_I4_S ) {
                        DebugLog( "Found matching opcode at " + i + ": " + codes[i].ToString() );
                        // Check if the operand is correct for level 18
                        if( (sbyte)codes[i].operand == 18 ) {
                            DebugLog( "Correct operand - patching" );
                            // Set the operand to the new value from the config file
                            codes[i].operand = configStartLevel.Value;
                        }
                    }
                }
                // Return the modified instructions to complete the patch
                return codes.AsEnumerable();
            }
        }

        // Patch for the method that gets the gold cost for a specific upgrade with labour costs included
        [HarmonyPatch(typeof( SkillTreeObj ), nameof( SkillTreeObj.GoldCostWithLevelAppreciation ), MethodType.Getter )]
        static class SkillTreeObj_GoldCostWithLevelAppreciation_Patch {
            static void Postfix( SkillTreeObj __instance, ref int __result ) {
                // Calculate the new cost and overwrite the original return value
                __result = __instance.GoldCost + NewLabourCost();
            }
        }

        // Patch for the method that displays the pop-up when labour costs first unlock
        [HarmonyPatch()]
        public static class SkillTreeWindowController_UnlockLabourCostAnimCoroutine_Patch {
            // Find the correct method - this is an implicitly defined method
            // 'UnlockLabourCostAnimCoroutine' returns an IEnumerator, and we need to patch the 'MoveNext' method on that
            static MethodInfo TargetMethod() {
                // Find the subclass of 'SkillTreeWindowController' that 'UnlockLabourCostAnimCoroutine' implicitly created
                System.Type type = AccessTools.FirstInner( typeof( SkillTreeWindowController ), t => t.Name.Contains( "<UnlockLabourCostAnimCoroutine>d__" ) );
                // Find the 'MoveNext' method on the subclass
                return AccessTools.FirstMethod( type, method => method.Name.Contains( "MoveNext" ) );
            }

            // Change the starting level in the method. This sets whether the labour cost box is displayed
            static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Put the instructions into a list for easier manipulation
                List<CodeInstruction> codes = new List<CodeInstruction>( instructions );
                DebugLog( "Searching opcodes" );
                // Iterate through the instruction codes
                for( int i = 0; i < codes.Count; i++ ) {
                    // Search for instruction 'ldc.i4.s' which pushes an int8 onto the stack
                    if( codes[i].opcode == OpCodes.Ldc_I4_S ) {
                        DebugLog( "Found matching opcode at " + i + ": " + codes[i].ToString() );
                        // Check if the operand is correct for level 18
                        if( (sbyte)codes[i].operand == 18 ) {
                            DebugLog( "Correct operand - patching" );
                            // Set the operand to the new value from the config file
                            codes[i].operand = configStartLevel.Value;
                        }
                    }
                }
                // Return the modified instructions to complete the patch
                return codes.AsEnumerable();
            }
        }
    }
}