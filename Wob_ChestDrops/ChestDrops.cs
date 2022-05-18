using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_ChestDrops {
    [BepInPlugin( "Wob.ChestDrops", "Chest Drops Mod", "0.1.0" )]
    public partial class ChestDrops : BaseUnityPlugin {
        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        [HarmonyPatch( typeof( ChestObj ), "DropRewardFromRegularChest" )]
        static class ChestObj_DropRewardFromRegularChest_Patch {
            // Patch to remove 'this.m_specialItemDropsList.Clear()' from the start of the method so we can add extra items
            static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "ChestObj.DropRewardFromRegularChest Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                               ), // this
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldfld, name: "m_specialItemDropsList" ), // this.m_specialItemDropsList
                            /*  2 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "Clear"               ), // this.m_specialItemDropsList.Clear()
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_Remove( 0, 3 ), // Blank out the found instructions with nop instructions
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        private static readonly List<ISpecialItemDrop> drops = new List<ISpecialItemDrop>();

        [HarmonyPatch( typeof( ChestObj ), "GetSpecialItemTypeToDrop" )]
        static class ChestObj_GetSpecialItemTypeToDrop_Patch {
            // Patch to add the extra drops
            static void Postfix( ChestObj __instance, ref SpecialItemType __result ) {
                if( __instance.ChestType == ChestType.Bronze || __instance.ChestType == ChestType.Silver || __instance.ChestType == ChestType.Gold || __instance.ChestType == ChestType.Fairy ) {
                    WobPlugin.Log( __instance.ChestType + " chest at level " + __instance.Level );
                    drops.Clear();
                    // Try to get a rune and add it to the drop list
                    AddSpecialItem( drops, GetSpecialItem( __instance, SpecialItemType.Rune ) );
                    // Get another if this isn't a basic chest or if we haven't got any items yet
                    if( __instance.ChestType != ChestType.Bronze || drops.Count == 0 ) {
                        // Try to get a blueprint and add it to the drop list
                        AddSpecialItem( drops, GetSpecialItem( __instance, SpecialItemType.Blueprint ) );
                    }
                    // Get another if this isn't a basic chest or if we haven't got any items yet
                    if( __instance.ChestType != ChestType.Bronze || drops.Count == 0 ) {
                        // Try to get a challenge empathy and add it to the drop list
                        AddSpecialItem( drops, GetSpecialItem( __instance, SpecialItemType.Challenge ) );
                    }
                    if( drops.Count > 0 ) {
                        // Change the method's return value
                        __result = drops.Last().SpecialItemType;
                        WobPlugin.Log( "    Returning drop of " + drops.Last().SpecialItemType );
                    } else {
                        __result = ( __instance.ChestType == ChestType.Fairy ? SpecialItemType.Ore : SpecialItemType.Gold );
                        WobPlugin.Log( "    No drops available" );
                    }
                }
            }

            // Helper method to calculate a random item
            private static ISpecialItemDrop GetSpecialItem( ChestObj chest, SpecialItemType specialItemType ) {
                ISpecialItemDrop specialItem = null;
                switch( specialItemType ) {
                    case SpecialItemType.Rune:
                        specialItem = SpecialItemDropUtility.GetRuneDrop( chest.Level );
                        break;
                    case SpecialItemType.Blueprint:
                        specialItem = SpecialItemDropUtility.GetBlueprintDrop( chest.Level, ChestType_RL.GetChestRarity( chest.ChestType ) );
                        break;
                    case SpecialItemType.Challenge:
                        specialItem = SpecialItemDropUtility.GetChallengeDrop();
                        break;
                }
                return specialItem;
            }

            // Helper method to add a drop only if not null
            private static void AddSpecialItem( List<ISpecialItemDrop> dropsList, ISpecialItemDrop specialItem ) {
                if( specialItem != null ) {
                    dropsList.Add( specialItem );
                }
            }
        }

        [HarmonyPatch( typeof( ChestObj ), "CalculateSpecialItemDropObj" )]
        static class ChestObj_CalculateSpecialItemDropObj_Patch {
            // Patch to add the extra drops
            static void Postfix( ChestObj __instance, ref ISpecialItemDrop __result ) {
                // Get a reference to the drops list
                List<ISpecialItemDrop> m_specialItemDropsList = (List<ISpecialItemDrop>)Traverse.Create( __instance ).Field( "m_specialItemDropsList" ).GetValue();
                // Clear the list, to replace the original clear we removed
                m_specialItemDropsList.Clear();
                // Check that at least 1 drop has been found
                if( drops.Count > 0 ) {
                    // Change the method's return value
                    __result = drops[drops.Count - 1];
                    // Remove the drop from the list
                    drops.RemoveAt( drops.Count - 1 );
                    // For all remaining drops in the list...
                    foreach( ISpecialItemDrop drop in drops ) {
                        // Add the drop to the chest's additional drops list
                        m_specialItemDropsList.Add( drop );
                        WobPlugin.Log( "    Extra drop of " + drop.SpecialItemType );
                    }
                }
            }
        }
    }
}