using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_SeenState {
    [BepInPlugin( "Wob.SeenState", "Seen State Mod", "0.1.0" )]
    public partial class SeenState : BaseUnityPlugin {
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

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

    }
}