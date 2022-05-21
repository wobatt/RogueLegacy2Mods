using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob__Test {
    [BepInPlugin( "Wob._Test", "Wob's Test Mod", "0.1.0" )]
    public partial class Test : BaseUnityPlugin {
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobPlugin.Settings.Add( new WobSettings.Entry[] {
                new WobSettings.Entry<float>( "AttackCooldown", "Set the attack cooldown of Heavy Stone Bargain to this number of seconds", 2f, bounds: (0f, 1000000f) ),
            } );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // This patch simply dumps skill tree data to the debug log when the Manor skill tree is opened - useful for getting internal names and default values for the upgrades
        /*[HarmonyPatch( typeof( SkillTreeWindowController ), nameof( SkillTreeWindowController.Initialize ) )]
        internal static class SkillTreeWindowController_Initialize_Patch {
            internal static void Postfix() {
                foreach( ClassType classType in ClassType_RL.TypeArray ) {
                    ClassData classData = ClassLibrary.GetClassData( classType );
                    if( classData != null ) {
                        ClassPassiveData classPassiveData = classData.PassiveData;
                        if( classPassiveData != null ) {
                            WobPlugin.Log( "~~ " + classType + "|N=" + classPassiveData.ClassName +
                                "|HP=" + classPassiveData.MaxHPMod + "|MP=" + classPassiveData.MaxManaMod +
                                "|Vit=" + classPassiveData.VitalityMod + "|Arm=" + classPassiveData.ArmorMod +
                                "|Str=" + classPassiveData.StrengthMod + "|Int=" + classPassiveData.IntelligenceMod +
                                "|Dex=" + classPassiveData.DexterityMod + "|Foc=" + classPassiveData.FocusMod +
                                "|WCC=" + classPassiveData.WeaponCritChanceAdd + "|MCC=" + classPassiveData.MagicCritChanceAdd +
                                "|WCD=" + classPassiveData.WeaponCritDamageAdd + "|MCD=" + classPassiveData.MagicCritDamageAdd +
                                "|" + LocalizationManager.GetString( classPassiveData.Title, false, false ) );
                        } else {
                            WobPlugin.Log( "@ No ClassPassiveData for " + classType );
                        }
                    } else {
                        WobPlugin.Log( "@ No ClassData for " + classType );
                    }
                }
            }
        }*/

        [HarmonyPatch( typeof( CaveLanternPostProcessingController ), "DarknessAmountWhenFullyLit", MethodType.Getter )]
        internal static class CaveLanternPostProcessingController_DarknessAmountWhenFullyLit_Patch {
            internal static void Postfix( ref float __result ) {
                __result = 0f;
            }
        }

        // Set the attack cooldown of Heavy Stone Bargain
        [HarmonyPatch( typeof( BaseAbility_RL ), nameof( BaseAbility_RL.ActualCooldownTime ), MethodType.Getter )]
        internal static class BaseAbility_RL_ActualCooldownTime_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "BaseAbility_RL.ActualCooldownTime Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc    ), // num
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4, 2f ), // 2f
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc    ), // level
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Conv_R4    ), // (float)level
                            /*  4 */ new WobTranspiler.OpTest( OpCodes.Mul        ), // 2f * (float)level
                            /*  5 */ new WobTranspiler.OpTest( OpCodes.Add        ), // num + 2f * (float)level
                            /*  6 */ new WobTranspiler.OpTest( OpCodeSet.Stloc    ), // num = num + 2f * (float)level
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 1, WobPlugin.Settings.Get( "AttackCooldown", 2f ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

    }
}