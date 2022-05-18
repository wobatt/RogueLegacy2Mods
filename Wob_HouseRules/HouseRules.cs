using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Wob_Common;

namespace Wob_HouseRules {
    [BepInPlugin("Wob.HouseRules", "House Rules Mod", "0.1.0")]
    public partial class HouseRules : BaseUnityPlugin {
        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobPlugin.Settings.Add( new WobSettings.Entry<int>( "MaxDifficulty", "Maximum percentage that the enemy health and damage house rules will go up to", 1000, bounds: (200, int.MaxValue), limiter: x => { return (int)( System.Math.Floor( x / 5f ) * 5f ); } ) );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Patch for the method that gets the gold cost for a specific upgrade with labour costs included
        [HarmonyPatch( typeof( ChangeAssistStatModOptionItem ), nameof( ChangeAssistStatModOptionItem.Initialize ) )]
        static class ChangeAssistStatModOptionItem_Initialize_Patch {
            // Change the minimum values of the house rule sliders
            static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "ChangeAssistStatModOptionItem.Initialize Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            // case ChangeAssistStatModOptionItem.StatType.EnemyHealth:
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                         ), // this
                            /*  1 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4                        ), // 50
                            /*  2 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "m_minValue"       ), // this.m_minValue = 50
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                         ), // this
                            /*  4 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4                        ), // 200
                            /*  5 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "m_maxValue"       ), // this.m_maxValue = 200
                            /*  6 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                         ), // this
                            /*  7 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4                        ), // 5
                            /*  8 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "m_incrementValue" ), // this.m_incrementValue = 5
                            /*  9 */ new WobTranspiler.OpTest( OpCodes.Br                              ), // break
                            // case ChangeAssistStatModOptionItem.StatType.EnemyDamage:
                            /* 10 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                         ), // this
                            /* 11 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4                        ), // 50
                            /* 12 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "m_minValue"       ), // this.m_minValue = 50
                            /* 13 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                         ), // this
                            /* 14 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4                        ), // 200
                            /* 15 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "m_maxValue"       ), // this.m_maxValue = 200
                            /* 16 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                         ), // this
                            /* 17 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4                        ), // 5
                            /* 18 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "m_incrementValue" ), // this.m_incrementValue = 5
                            /* 19 */ new WobTranspiler.OpTest( OpCodes.Br                              ), // break
                            // case ChangeAssistStatModOptionItem.StatType.AimTimeSlow:
                            /* 20 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                         ), // this
                            /* 21 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4                        ), // 25
                            /* 22 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "m_minValue"       ), // this.m_minValue = 25
                            /* 23 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                         ), // this
                            /* 24 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4                        ), // 100
                            /* 25 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "m_maxValue"       ), // this.m_maxValue = 100
                            /* 26 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                         ), // this
                            /* 27 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4                        ), // 5
                            /* 28 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "m_incrementValue" ), // this.m_incrementValue = 5
                            /* 29 */ new WobTranspiler.OpTest( OpCodes.Br                              ), // break
                            // case ChangeAssistStatModOptionItem.StatType.BurdenRequirement:
                            /* 30 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                         ), // this
                            /* 31 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4                        ), // 50
                            /* 32 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "m_minValue"       ), // this.m_minValue = 50
                            /* 33 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                         ), // this
                            /* 34 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4                        ), // 200
                            /* 35 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "m_maxValue"       ), // this.m_maxValue = 200
                            /* 36 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                         ), // this
                            /* 37 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4                        ), // 50
                            /* 38 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "m_incrementValue" ), // this.m_incrementValue = 50
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand(  1, (sbyte)5                                       ), // Set minimum enemy health
                            new WobTranspiler.OpAction_SetOperand(  4, WobPlugin.Settings.Get( "MaxDifficulty", 200 ) ), // Set maximum enemy health
                            new WobTranspiler.OpAction_SetOperand( 11, (sbyte)0                                       ), // Set minimum enemy damage
                            new WobTranspiler.OpAction_SetOperand( 14, WobPlugin.Settings.Get( "MaxDifficulty", 200 ) ), // Set maximum enemy damage
                            new WobTranspiler.OpAction_SetOperand( 21, (sbyte)5                                       ), // Set minimum aim time slow
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }
    }
}