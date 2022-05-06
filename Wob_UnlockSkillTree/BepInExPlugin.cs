using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Wob_Common;

namespace Wob_UnlockSkillTree {
    [BepInPlugin( "Wob.UnlockSkillTree", "Unlock Skill Tree Mod", "0.1.1" )]
    public partial class BepInExPlugin : BaseUnityPlugin {
        // Configuration file entries, globally accessible for patches
        public static ConfigItemBool configUnlockTree;
        public static ConfigItemBool configUnlockLevel;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configUnlockTree =  new ConfigItemBool( this.Config, "Options", "UnlockTree",  "Unlock the tree - all skills visible/selectable",    true );
            configUnlockLevel = new ConfigItemBool( this.Config, "Options", "UnlockLevel", "Unlock the level - remove manor level requirements", true );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Patch for the method that gets the level when a skill is unlocked
        [HarmonyPatch( typeof( SkillTreeObj ), nameof( SkillTreeObj.UnlockLevel ), MethodType.Getter )]
        static class SkillTreeObj_UnlockLevel_Patch {
            static void Postfix( ref int __result ) {
                if( configUnlockLevel.Value ) {
                    // Always return 0 (unlocked)
                    __result = 0;
                }
            }
        }

        // Patch for the method that controls whether a skill is initially hidden or visible
        [HarmonyPatch( typeof( SkillTreeWindowController ), nameof( SkillTreeWindowController.Initialize ) )]
        static class SkillTreeWindowController_Initialize_Patch {
            // When the skill tree initialises, it looks for all skills with level > 0 and makes them visible, then all skills they connect to and also makes them visible
            // We are looking for the check of level > 0 and changing it to level >= 0, which makes all skills visible
            static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Put the instructions into a list for easier manipulation
                List<CodeInstruction> codes = new List<CodeInstruction>( instructions );
                if( configUnlockTree.Value ) {
                    WobPlugin.Log( "Searching opcodes" );
                    // Iterate through the instruction codes
                    for( int i = 3; i < codes.Count; i++ ) {
                        // Find the "ble" (Branch Less than or Equal) with the right method calls before it for the 'if' statement we are looking for
                        if( codes[i].opcode == OpCodes.Ble && codes[i - 1].opcode == OpCodes.Ldc_I4_0
                                && codes[i - 2].opcode == OpCodes.Call && ( codes[i - 2].operand as MethodInfo ).Name == "GetSkillObjLevel"
                                && codes[i - 3].opcode == OpCodes.Callvirt && ( codes[i - 3].operand as MethodInfo ).Name == "get_SkillTreeType" ) {
                            WobPlugin.Log( "Found matching opcode at " + i + ": " + codes[i].ToString() );
                            // Change the check to "blt" (Branch Less Than) - only do the 'else' part that hides a skill if the level is less than 0
                            codes[i].opcode = OpCodes.Blt;
                        }
                    }
                }
                // Return the modified instructions to complete the patch
                return codes.AsEnumerable();
            }
        }
    }
}