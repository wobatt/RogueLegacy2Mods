using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Wob_Common;

namespace Wob_UnlockSkillTree {
    [BepInPlugin( "Wob.UnlockSkillTree", "Unlock Skill Tree Mod", "1.0.0" )]
    public partial class UnlockSkillTree : BaseUnityPlugin {
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobPlugin.Settings.Add( new WobSettings.Entry[] {
                new WobSettings.EntryBool( "UnlockTree",  "Unlock the tree - all skills visible/selectable",    true ),
                new WobSettings.EntryBool( "UnlockLevel", "Unlock the level - remove manor level requirements", true ),
            } );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Patch for the method that gets the level when a skill is unlocked
        [HarmonyPatch( typeof( SkillTreeObj ), nameof( SkillTreeObj.UnlockLevel ), MethodType.Getter )]
        internal static class SkillTreeObj_UnlockLevel_Patch {
            internal static void Postfix( ref int __result ) {
                if( WobPlugin.Settings.Get( "UnlockLevel", false ) ) {
                    // Always return 0 (unlocked)
                    __result = 0;
                }
            }
        }

        // Patch for the method that controls whether a skill is initially hidden or visible
        [HarmonyPatch( typeof( SkillTreeWindowController ), nameof( SkillTreeWindowController.Initialize ) )]
        internal static class SkillTreeWindowController_Initialize_Patch {
            // When the skill tree initialises, it looks for all skills with level > 0 and makes them visible, then all skills they connect to and also makes them visible
            // We are looking for the check of level > 0 and changing it to level >= 0, which makes all skills visible
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                if( WobPlugin.Settings.Get( "UnlockTree", false ) ) {
                    WobPlugin.Log( "SkillTreeWindowController.Initialize Transpiler Patch" );
                    // Set up the transpiler handler with the instruction list
                    WobTranspiler transpiler = new WobTranspiler( instructions );
                    // Perform the patching
                    transpiler.PatchAll(
                                // Define the IL code instructions that should be matched
                                new List<WobTranspiler.OpTest> {
                                /*  0 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_SkillTreeType" ), // skillTreeSlot.SkillTreeType
                                /*  1 */ new WobTranspiler.OpTest( OpCodes.Call, name: "GetSkillObjLevel"      ), // SkillTreeManager.GetSkillObjLevel( skillTreeSlot.SkillTreeType )
                                /*  2 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4_0                            ), // 0
                                /*  3 */ new WobTranspiler.OpTest( OpCodes.Ble                                 ), // >    [inverted]
                            },
                            // Define the actions to take when an occurrence is found
                            new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_SetOpcode( 3, OpCodes.Blt ), // Change the "ble" (Branch Less than or Equal) check to "blt" (Branch Less Than) - only do the 'else' part that hides a skill if the level is less than 0
                            } );
                    // Return the modified instructions
                    return transpiler.GetResult();
                } else {
                    return instructions;
                }
            }
        }
    }
}