﻿using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using Wob_Common;

namespace Wob_LabourCosts {
    [BepInPlugin("Wob.LabourCosts", "Labour Costs Mod", "0.2.0")]
    public partial class LabourCosts : BaseUnityPlugin {
        // Configuration file entries, globally accessible for patches
        public static ConfigItem<sbyte> configStartLevel;
        public static ConfigItem<float> configPerLevel;
        public static ConfigItem<int>   configRoundTo;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configStartLevel = new ConfigItem<sbyte>( this.Config, "Options", "StartLevel", "Level after which labour costs start.",                     20,  0, sbyte.MaxValue );
            configPerLevel   = new ConfigItem<float>( this.Config, "Options", "PerLevel",   "Cost increase per level. Set to 0 to remove labour costs.", 14f, 0, float.MaxValue );
            configRoundTo    = new ConfigItem<int>(   this.Config, "Options", "RoundTo",    "Round down calculated cost to this significance.",          5,   1, int.MaxValue   );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
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
                WobPlugin.Log( "SkillTreeWindowController.UpdateLabourCosts Transpiler Patch" );
                // Set up the transpiler handler's parameters
                WobTranspiler transpiler = new WobTranspiler( instructions,
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTestLine> {
                            /*  0 */ new WobTranspiler.OpTestLine( OpCodes.Call, name: "GetTotalSkillObjLevel" ), // SkillTreeManager.GetTotalSkillObjLevel()
                            /*  1 */ new WobTranspiler.OpTestLine( OpCodes.Ldc_I4_S                            ), // > 20
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 1, configStartLevel.Value ), // Set the operand to the new level from the config file
                        } );
                // Perform the patching and return the modified instructions
                return transpiler.PatchAll();
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
                WobPlugin.Log( "SkillTreeWindowController.UnlockLabourCostAnimCoroutine Transpiler Patch" );
                // Set up the transpiler handler's parameters
                WobTranspiler transpiler = new WobTranspiler( instructions,
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTestLine> {
                            /*  0 */ new WobTranspiler.OpTestLine( OpCodes.Call, name: "GetTotalSkillObjLevel" ), // SkillTreeManager.GetTotalSkillObjLevel()
                            /*  1 */ new WobTranspiler.OpTestLine( OpCodes.Ldc_I4_S                            ), // > 20
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 1, configStartLevel.Value ), // Set the operand to the new level from the config file
                        } );
                // Perform the patching and return the modified instructions
                return transpiler.PatchAll();
            }
        }
    }
}