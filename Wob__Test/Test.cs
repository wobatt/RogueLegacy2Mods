using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using MoreMountains.CorgiEngine;
using UnityEngine;
using Wob_Common;

namespace Wob__Test {
    [BepInPlugin( "Wob._Test", "Wob's Test Mod", "0.1.0" )]
    public partial class Test : BaseUnityPlugin {
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobSettings.Add( new WobSettings.Num<int>( "TwinRelicResolve", "Always spawn twin relics if you have this amount of total resolve (use -1 to disable)", 500, 0.01f, bounds: (-1, 1000000) ) );

            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }





        // This patch simply dumps skill tree data to the debug log when the Manor skill tree is opened - useful for getting internal names and default values for the upgrades
        /*[HarmonyPatch( typeof( SkillTreeWindowController ), nameof( SkillTreeWindowController.Initialize ) )]
        internal static class SkillTreeWindowController_Initialize_Patch {
            internal static void Postfix() {
                foreach( RuneType runeType in RuneType_RL.TypeArray ) {
                    if( runeType != RuneType.None ) {
                        RuneData runeData = RuneLibrary.GetRuneData( runeType );
                        if( runeData != null && !runeData.Disabled ) {
                            WobPlugin.Log( "~~  " + runeType + "|" + runeData.BaseItemLevel + "|" + runeData.ScalingItemLevel  + "|" + runeData.MaximumLevel
                                + "|" + runeData.GoldCost + "|" + runeData.ScalingGoldCost + "|" + runeData.BlackStoneCost + "|" + runeData.ScalingBlackStoneCost
                                + "|" + runeData.BaseWeight + "|" + runeData.ScalingWeight
                                + "|" + runeData.StatMod01 + "|" + runeData.ScalingStatMod01 + "|" + runeData.StatMod02 + "|" + runeData.ScalingStatMod02 + "|" + runeData.StatMod03 + "|" + runeData.ScalingStatMod03
                                + "|" + LocalizationManager.GetString( runeData.Title, false, false ) + "|" + LocalizationManager.GetString( runeData.Description, false, false ) );
                        }
                    }
                }
            }
        }*/

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        [HarmonyPatch( typeof( PistolWeapon_Ability ), "FireProjectile" )]
        internal static class PistolWeapon_Ability_FireProjectile_Patch {
            internal static void Postfix( PistolWeapon_Ability __instance ) {
                if( __instance.CurrentAmmo <= 0 ) {
                    __instance.CurrentAmmo = __instance.MaxAmmo;
                }
            }
        }
        
        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        [HarmonyPatch( typeof( CaveLanternPostProcessingController ), "DarknessAmountWhenFullyLit", MethodType.Getter )]
        internal static class CaveLanternPostProcessingController_DarknessAmountWhenFullyLit_Patch {
            internal static void Postfix( ref float __result ) {
                __result = 0f;
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        [HarmonyPatch( typeof( RelicObj ), nameof( RelicObj.WasSeen ), MethodType.Getter )]
        internal static class RelicObj_WasSeen_Patch {
            internal static void Postfix( RelicObj __instance, ref bool __result ) {
                if( !__result ) {
                    Traverse.Create( __instance ).Field( "m_wasSeen" ).SetValue( true );
                    __result = true;
                }
            }
        }
        
        [HarmonyPatch( typeof( PlayerSaveData ), nameof( PlayerSaveData.GetSpellSeenState ) )]
        internal static class PlayerSaveData_GetSpellSeenState_Patch {
            internal static void Postfix( PlayerSaveData __instance, AbilityType spellType, ref bool __result ) {
                if( !__result ) {
                    __instance.SetSpellSeenState( spellType, true );
                    __result = true;
                }
            }
        }
        
        [HarmonyPatch( typeof( TraitManager ), nameof( TraitManager.GetTraitSeenState ) )]
        internal static class TraitManager_GetTraitFoundState_Patch {
            internal static void Postfix( TraitType traitType, ref TraitSeenState __result ) {
                if( __result < TraitSeenState.SeenTwice ) {
                    TraitManager.SetTraitSeenState( traitType, TraitSeenState.SeenTwice, false );
                    __result = TraitSeenState.SeenTwice;
                }
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        [HarmonyPatch( typeof( RelicRoomPropController ), "RollRelicMod" )]
        internal static class RelicRoomPropController_RollRelicMod_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "RelicRoomPropController.RollRelicMod Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
							// if (TraitManager.IsTraitActive(TraitType.TwinRelics))
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, TraitType.TwinRelics ), // TraitType.TwinRelics
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Call, name: "IsTraitActive"  ), // TraitManager.IsTraitActive(TraitType.TwinRelics)
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                    ), // if (TraitManager.IsTraitActive(TraitType.TwinRelics))
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_SetOperand( 1, SymbolExtensions.GetMethodInfo( () => IsTraitActiveOrOverride( TraitType.None ) ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool IsTraitActiveOrOverride( TraitType traitType ) {
                float config = WobSettings.Get( "TwinRelicResolve", -1f );
                return TraitManager.IsTraitActive( traitType ) || ( config >= 0 && GetResolveTotal() > config );
            }

            private static float GetResolveTotal() {
                float current = 1f + PlayerManager.GetPlayerController().ResolveAdd;
                float spent = SaveManager.PlayerSaveData.GetTotalRelicResolveCost();
                float total = current + spent;
                WobPlugin.Log( "~~  Resolve = " + current + " + " + spent + " = " + total );
                /*if( !ChallengeManager.IsInChallenge ) {
                    resolve += Equipment_EV.RESOLVE_BONUS_PER_WEIGHT_LEVEL[Mathf.Clamp( EquipmentManager.GetWeightLevel(), 0, Equipment_EV.CRIT_DAMAGE_BONUS_PER_WEIGHT_LEVEL.Length - 1 )];
                }
                resolve += RuneLogicHelper.GetResolveAdd();
                resolve += SkillTreeLogicHelper.GetResolveAdds();
                resolve += EquipmentManager.Get_EquipmentSet_BonusTypeStatGain( EquipmentSetBonusType.Resolve );
                int burdenAdd = BurdenManager.BurdenRequiredForNG(SaveManager.PlayerSaveData.NewGamePlusLevel);
                if( burdenAdd < 0 ) {
                    SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.BurdenOverload);
                    if( !soulShopObj.IsNativeNull() ) {
                        burdenAdd += soulShopObj.CurrentStatGain * (float)Mathf.Abs( num4 );
                    }
                }*/
                return total;
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        // ========== Tutorial Examples ==========

        //[HarmonyPatch( typeof( PlayerSaveData ), nameof( PlayerSaveData.GetSpellSeenState ) )]
        //internal static class PlayerSaveData_GetSpellSeenState_Patch {
        //    internal static void Prefix( PlayerSaveData __instance, AbilityType spellType ) {
        //        // Before the method runs to get the seen state, make sure it is set to true (seen)
        //        __instance.SetSpellSeenState( spellType, true );
        //    }
        //}

        //[HarmonyPatch( typeof( PlayerSaveData ), nameof( PlayerSaveData.GetSpellSeenState ) )]
        //internal static class PlayerSaveData_GetSpellSeenState_Patch {
        //    internal static void Postfix( ref bool __result ) {
        //        // Ignore whether it is recorded as seen or not, just set the return value to true (seen)
        //        __result = true;
        //    }
        //}

        //[HarmonyPatch( typeof( PlayerSaveData ), nameof( PlayerSaveData.GetSpellSeenState ) )]
        //internal static class PlayerSaveData_GetSpellSeenState_Patch {
        //    internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
        //        // Put the instructions in a list for easier manipulation and matching using indexes
        //        List<CodeInstruction> instructionList = new List<CodeInstruction>( instructions );
        //        // Set up variables for looping through the instructions
        //        bool found = false;
        //        int i = 0;
        //        // Loop through the instructions until a match has been found, or the end of the method has been reached
        //        while( !found && i < instructionList.Count ) {
        //            // Check for the desired instructions for 'return value;'
        //            if( instructionList[i].opcode == OpCodes.Ldloc_0 && instructionList[i + 1].opcode == OpCodes.Ret ) {
        //                // Change 'value' to '1' (true/seen);
        //                instructionList[i].opcode = OpCodes.Ldc_I4_1;
        //                // Set the found flag to exit the loop
        //                found = true;
        //            }
        //            // Move to the next instruction
        //            i++;
        //        }
        //        // Return the modified instructions
        //        return instructionList.AsEnumerable();
        //    }
        //}

        //[HarmonyPatch( typeof( PlayerSaveData ), nameof( PlayerSaveData.GetSpellSeenState ) )]
        //internal static class PlayerSaveData_GetSpellSeenState_Patch {
        //    internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
        //        WobPlugin.Log( "PlayerSaveData.GetSpellSeenState Transpiler Patch" );
        //        // Set up the transpiler handler with the instruction list
        //        WobTranspiler transpiler = new WobTranspiler( instructions );
        //        // Perform the patching
        //        transpiler.PatchAll(
        //                // Define the IL code instructions that should be matched
        //                new List<WobTranspiler.OpTest> {
        //                    /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldloc_0 ), // value
        //                    /*  1 */ new WobTranspiler.OpTest( OpCodes.Ret     ), // return value;
        //                },
        //                // Define the actions to take when an occurrence is found
        //                new List<WobTranspiler.OpAction> {
        //                    new WobTranspiler.OpAction_SetOpcode( 0, OpCodes.Ldc_I4_1 ), // Change 'value' to '1' (true/seen);
        //                } );
        //        // Return the modified instructions
        //        return transpiler.GetResult();
        //    }
        //}

        //[HarmonyPatch( typeof( TraitManager ), nameof( TraitManager.GetTraitSeenState ) )]
        //internal static class TraitManager_GetTraitFoundState_Patch {
        //    private static bool runOnce = false;
        //    internal static void Prefix( PlayerSaveData __instance ) {
        //        if( !runOnce ) {
        //            // Loop through all traits
        //            foreach( TraitType traitType in __instance.TraitSeenTable.Keys ) {
        //                // Set the seen state to seen
        //                __instance.TraitSeenTable[traitType] = TraitSeenState.SeenTwice;
        //            }
        //            // Record that this has been done, so no need to run again
        //            runOnce = true;
        //        }
        //    }
        //}

        //[HarmonyPatch( typeof( RelicObj ), nameof( RelicObj.WasSeen ), MethodType.Getter )]
        //internal static class RelicObj_WasSeen_Get_Patch {
        //    internal static bool Prefix( RelicObj __instance, ref bool __result ) {
        //        // Navigate to the private field on the instance object, and set its value to true (seen)
        //        Traverse.Create( __instance ).Field( "m_wasSeen" ).SetValue( true );
        //        // Navigate to the private field on the instance object, get its value, and set the original method's return value
        //        __result = Traverse.Create( __instance ).Field( "m_wasSeen" ).GetValue<bool>();
        //        // Return true to skip running the original method
        //        return true;
        //    }
        //}

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        // Helper to get the UI names of a trait
        /*public static string GetTraitTitles( TraitData traitData ) {
            // Each trait has 4 possible names - scientific/non-scientific and male/female character
            // First get all 4 variants
            string tScientificM = LocalizationManager.GetString( traitData.Title, false, false );
            string tScientificF = LocalizationManager.GetString( traitData.Title, true, false );
            string tNonScientificM = LocalizationManager.GetString( traitData.Title.Replace( "_1", "_2" ), false, false );
            string tNonScientificF = LocalizationManager.GetString( traitData.Title.Replace( "_1", "_2" ), true, false );
            // Build a return string, suppressing variants if they are the same as one already added
            return tScientificM + ( tScientificM == tScientificF ? "" : "/" + tScientificF ) + ( tScientificM == tNonScientificM ? "" : "/" + tNonScientificM ) + ( ( tNonScientificM == tNonScientificF || tScientificF == tNonScientificF ) ? "" : "/" + tNonScientificF );
        }*/
    }
}