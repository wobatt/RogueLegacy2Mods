using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace Wob_ChestDrops {
    [BepInPlugin( "Wob.ChestDrops", "Chest Drops Mod", "0.2.0" )]
    public partial class ChestDrops : BaseUnityPlugin {
        private static readonly WobSettings.KeyHelper<ChestType> keys = new WobSettings.KeyHelper<ChestType>( "Chest" );
        private enum DefaultDrop {
            Gold = SpecialItemType.Gold,
            Ore  = SpecialItemType.Ore,
        }
        private static readonly Dictionary<ChestType, (string Config, string Name, int Max, DefaultDrop Default)> chests = new Dictionary<ChestType, (string Config, string Name, int Max, DefaultDrop Default)>() {
            { ChestType.Bronze, ("Bronze", "Basic Chests",  1, DefaultDrop.Gold) },
            { ChestType.Silver, ("Silver", "Silver Chests", 5, DefaultDrop.Gold) },
            { ChestType.Gold,   ("Gold",   "Gold Chests",   5, DefaultDrop.Gold) },
            { ChestType.Fairy,  ("Fairy",  "Fairy Chests",  5, DefaultDrop.Ore ) },
        };

        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            foreach( ChestType chestType in chests.Keys ) {
                keys.Add( chestType, chests[chestType].Config );
                WobSettings.Add( new WobSettings.Num<int>(          keys.Get( chestType, "MaxDrops" ), chests[chestType].Name + ": Maximum number of drops", chests[chestType].Max, bounds: (0, 5) ) );
                WobSettings.Add( new WobSettings.Enum<DefaultDrop>( keys.Get( chestType, "Default"  ), chests[chestType].Name + ": Drop type if no items",   chests[chestType].Default             ) );
            }
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        [HarmonyPatch( typeof( ChestObj ), "DropRewardFromRegularChest" )]
        internal static class ChestObj_DropRewardFromRegularChest_Patch {
            // Patch to remove 'this.m_specialItemDropsList.Clear()' from the start of the method so we can add extra items
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
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
        internal static class ChestObj_GetSpecialItemTypeToDrop_Patch {
            // Patch to add the extra drops
            internal static void Postfix( ChestObj __instance, ref SpecialItemType __result ) {
                drops.Clear();
                if( __instance.ChestType == ChestType.Bronze || __instance.ChestType == ChestType.Silver || __instance.ChestType == ChestType.Gold || __instance.ChestType == ChestType.Fairy ) {
                    WobPlugin.Log( __instance.ChestType + " chest at level " + __instance.Level );
                    GetDrops( WobSettings.Get( keys.Get( __instance.ChestType, "MaxDrops" ), chests[__instance.ChestType].Max ) );
                    if( drops.Count > 0 ) {
                        // Change the method's return value
                        __result = drops.Last().SpecialItemType;
                        WobPlugin.Log( "    Returning drop of " + drops.Last().SpecialItemType );
                    } else {
                        __result = (SpecialItemType)WobSettings.Get( keys.Get( __instance.ChestType, "Default" ), chests[__instance.ChestType].Default );
                        WobPlugin.Log( "    No drops" );
                    }
                }
            }

            private static bool runOnce = false;
            private static readonly List<EquipmentObj> equipItems = new List<EquipmentObj>();
            private static readonly List<RuneObj> runeItems = new List<RuneObj>();
            private static readonly List<ChallengeObj> challengeItems = new List<ChallengeObj>();
            private static void InitialiseLists() {
                foreach( EquipmentCategoryType equipmentCategoryType in EquipmentType_RL.CategoryTypeArray ) {
                    if( equipmentCategoryType != EquipmentCategoryType.None ) {
                        foreach( EquipmentType equipmentType in EquipmentType_RL.TypeArray ) {
                            if( equipmentType != EquipmentType.None ) {
                                EquipmentObj equipment = EquipmentManager.GetEquipment( equipmentCategoryType, equipmentType );
                                if( equipment != null && equipment.EquipmentData != null && !equipment.EquipmentData.Disabled ) {
                                    equipItems.Add( equipment );
                                }
                            }
                        }
                    }
                }
                foreach( RuneType runeType in RuneType_RL.TypeArray ) {
                    if( runeType != RuneType.None ) {
                        RuneObj rune = RuneManager.GetRune(runeType);
                        if( rune != null && rune.RuneData != null && !rune.RuneData.Disabled ) {
                            runeItems.Add( rune );
                        }
                    }
                }
                foreach( ChallengeType challengeType in ChallengeType_RL.TypeArray ) {
                    if( challengeType != ChallengeType.None ) {
                        ChallengeObj challenge = ChallengeManager.GetChallenge(challengeType);
                        if( challenge != null && challenge.ChallengeData != null && !challenge.ChallengeData.Disabled ) {
                            challengeItems.Add( challenge );
                        }
                    }
                }
            }

            private static void GetDrops( int maxDrops ) {
                if( !runOnce ) { InitialiseLists(); runOnce = true; }
                if( maxDrops > 0 ) {
                    List<(int Level, ISpecialItemDrop Drop)> levelDrops = new List<(int Level, ISpecialItemDrop Drop)>();
                    int minEquipmentLevelScale = Mathf.CeilToInt( EquipmentManager.GetMinEquipmentLevelScale() );
                    foreach( EquipmentObj equipment in equipItems ) {
                        if( !equipment.HasMaxBlueprints ) {
                            int itemLevel = equipment.EquipmentData.ChestLevelRequirement + minEquipmentLevelScale + ( ( EquipmentManager.GetUpgradeBlueprintsFound( equipment.CategoryType, equipment.EquipmentType, true ) + 1 ) * equipment.EquipmentData.ScalingItemLevel );
                            levelDrops.Add( (itemLevel, new BlueprintDrop( equipment.CategoryType, equipment.EquipmentType )) );
                        }
                    }
                    int minRuneLevelScale = Mathf.CeilToInt( RuneManager.GetMinRuneLevelScale() );
                    foreach( RuneObj rune in runeItems ) {
                        if( !rune.HasMaxBlueprints && ( SpecialItemDropUtility_HasHeirloom( rune.RuneType ) ) ) {
                            int itemLevel = rune.RuneData.BaseItemLevel + minRuneLevelScale + ( ( RuneManager.GetUpgradeBlueprintsFound( rune.RuneType, true ) + 1 ) * rune.RuneData.ScalingItemLevel );
                            levelDrops.Add( (itemLevel, new RuneDrop( rune.RuneType )) );
                        }
                    }
                    List<ISpecialItemDrop> challenges = new List<ISpecialItemDrop>();
                    foreach( ChallengeObj challenge in challengeItems ) {
                        if( !challenge.HasMaxBlueprints && challenge.FoundState != FoundState.NotFound ) {
                            int count = Mathf.Min( maxDrops, challenge.MaxLevel - challenge.MaxEquippableLevel );
                            for( int i = 0; i < count; i++ ) {
                                challenges.Add( new ChallengeDrop( challenge.ChallengeType ) );
                            }
                        }
                    }
                    while( challenges.Count > maxDrops ) {
                        challenges.RemoveAt( UnityEngine.Random.Range( 0, challenges.Count ) );
                    }
                    foreach( ISpecialItemDrop challenge in challenges ) {
                        levelDrops.Add( (0, challenge) );
                    }
                    levelDrops.Sort( ( x, y ) => x.Level.CompareTo( y.Level ) );
                    //WobPlugin.Log( "~ Options:" );
                    //for( int i = 0; i < levelDrops.Count; i++ ) {
                    //    WobPlugin.Log( "~~  " + levelDrops[i].Level + ", " + levelDrops[i].Drop + ( i >= numDrops ? "" : " -> drop" ) );
                    //}
                    WobPlugin.Log( "    Pool of " + levelDrops.Count + " drops" );
                    int dropCount = Mathf.Min( maxDrops, levelDrops.Count );
                    for( int i = 0; i < dropCount; i++ ) {
                        drops.Add( levelDrops[i].Drop );
                    }
                }
            }

            // Method to access private method SpecialItemDropUtility.HasHeirloom
            private static bool SpecialItemDropUtility_HasHeirloom( RuneType runeType ) {
                return Traverse.Create( typeof( SpecialItemDropUtility ) ).Method( "HasHeirloom", new Type[] { typeof( RuneType ) } ).GetValue<bool>( new object[] { runeType } );
            }

        }

        [HarmonyPatch( typeof( ChestObj ), "CalculateSpecialItemDropObj" )]
        internal static class ChestObj_CalculateSpecialItemDropObj_Patch {
            // Patch to add the extra drops
            internal static void Postfix( ChestObj __instance, ref ISpecialItemDrop __result ) {
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