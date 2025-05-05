using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Wob_Common;
using static WobMod.Equipment;

namespace WobMod {
    internal class Chests {

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTING DEFAULTS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTINGS READ/WRITE
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        internal static void RunSetup() {
            WobMod.configFiles.Add( "Chests", "Chests" );
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Boolean(  WobMod.configFiles.Get( "Chests" ), "FairyChests", "FairyChestRetry",  "Reset failed fairy chest rooms when you leave, allowing you to retry",                                      false             ),
                new WobSettings.Boolean(  WobMod.configFiles.Get( "Chests" ), "FairyChests", "EmptyDropGold",    "Fairy chests without an item in them drop gold as well as the red aether",                                  false             ),
                new WobSettings.Boolean(  WobMod.configFiles.Get( "Chests" ), "ChestDrops",  "IgnoreChestLevel", "Do not restrict items based on chest/room level (get higher level items sooner)",                           false             ),
                new WobSettings.Boolean(  WobMod.configFiles.Get( "Chests" ), "MultiDrops",  "AllowMultiDrops",  "Use the mod's chest drop method rather than the game's standard method, allowing multiple items per chest," +
                                                                                                                 " and guaranteed drop if there is an unlocked item you don't have",                                          false             ),
                new WobSettings.Num<int>( WobMod.configFiles.Get( "Chests" ), "MultiDrops",  "MaxDrops_Bronze",  "Bronze (normal) chest maximum number of equipment blueprint, rune, or challenge empathy drops",             1, bounds: (0, 5) ),
                new WobSettings.Num<int>( WobMod.configFiles.Get( "Chests" ), "MultiDrops",  "MaxDrops_Silver",  "Silver chest maximum number of equipment blueprint, rune, or challenge empathy drops",                      1, bounds: (0, 5) ),
                new WobSettings.Num<int>( WobMod.configFiles.Get( "Chests" ), "MultiDrops",  "MaxDrops_Gold",    "Gold chest maximum number of equipment blueprint, rune, or challenge empathy drops",                        1, bounds: (0, 5) ),
                new WobSettings.Num<int>( WobMod.configFiles.Get( "Chests" ), "MultiDrops",  "MaxDrops_Fairy",   "Fairy chest maximum number of equipment blueprint, rune, or challenge empathy drops",                       1, bounds: (0, 5) ),
            } );
            ChestObj_GetSpecialItemTypeToDrop_Patch.ignoreLevel = WobSettings.Get( "ChestDrops", "IgnoreChestLevel", false );
            ChestObj_GetSpecialItemTypeToDrop_Patch.patchEnabled = WobSettings.Get( "MultiDrops", "AllowMultiDrops", false );
            ChestObj_GetSpecialItemTypeToDrop_Patch.chestMaxDrops.Add( ChestType.Bronze, WobSettings.Get( "MultiDrops", "MaxDrops_Bronze", 1 ) );
            ChestObj_GetSpecialItemTypeToDrop_Patch.chestMaxDrops.Add( ChestType.Silver, WobSettings.Get( "MultiDrops", "MaxDrops_Silver", 1 ) );
            ChestObj_GetSpecialItemTypeToDrop_Patch.chestMaxDrops.Add( ChestType.Gold,   WobSettings.Get( "MultiDrops", "MaxDrops_Gold",   1 ) );
            ChestObj_GetSpecialItemTypeToDrop_Patch.chestMaxDrops.Add( ChestType.Fairy,  WobSettings.Get( "MultiDrops", "MaxDrops_Fairy",  1 ) );
            ChestObj_DropRewardFromRegularChest_Patch.fairyGold = WobSettings.Get( "FairyChests", "EmptyDropGold", false );
            if( WobPlugin.Enabled && ChestObj_DropRewardFromRegularChest_Patch.fairyGold ) {
                Economy_EV.BASE_GOLD_DROP_AMOUNT[ChestType.Fairy] = Economy_EV.BASE_GOLD_DROP_AMOUNT[ChestType.Bronze]; // Default is (2,3) for Bronze, Silver, and Gold, but (0,0) for Fairy
                Economy_EV.CHEST_TYPE_GOLD_MOD[ChestType.Fairy] = Economy_EV.CHEST_TYPE_GOLD_MOD[ChestType.Bronze]; // Default is 2.5 for Bronze, 5 for Silver, 8 for Gold, and 0 for Fairy
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - CHEST DROPS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        [HarmonyPatch( typeof( ChestObj ), "GetSpecialItemTypeToDrop" )]
        internal static class ChestObj_GetSpecialItemTypeToDrop_Patch {
            // Cached settings values
            internal static bool ignoreLevel = false;
            internal static bool patchEnabled = false;
            internal static readonly Dictionary<ChestType, int> chestMaxDrops = new();

            // List of items that can/will be dropped by a chest, by item level
            private record LevelDrop( int Level, ISpecialItemDrop SpecialItem );
            private static readonly List<LevelDrop> levelDrops = new();

            // Lists of all potential items that are enabled and could be dropped by the game, irrespecive of whether the player has met the unlock conditions
            private static readonly List<EquipItemKeys> validEquipmentItems = new();
            private static readonly List<RuneType> validRunes = new();
            private static readonly List<ChallengeType> validChallenges = new();
            private static bool dropListsInitialised = false;

            // Method to get all special item drops that are enabled and could be dropped by the game, irrespecive of whether the player has met the unlock conditions
            private static void InitialiseLists() {
                // Should only need to do this once
                if( !dropListsInitialised ) {
                    // Loop through all equipment sets
                    foreach( EquipmentType equipmentType in EquipmentType_RL.TypeArray ) {
                        if( equipmentType != EquipmentType.None ) {
                            // Loop through all equipment slots
                            foreach( EquipmentCategoryType equipmentCategoryType in EquipmentType_RL.CategoryTypeArray ) {
                                if( equipmentCategoryType != EquipmentCategoryType.None ) {
                                    // Load the equipment item for this set and slot
                                    EquipmentObj equipment = EquipmentManager.GetEquipment( equipmentCategoryType, equipmentType );
                                    // Check it exists, has defined data, and is not disabled by default or in mod settings
                                    if( equipment != null && equipment.EquipmentData != null && !equipment.EquipmentData.Disabled ) {
                                        // Any that pass the validation checks are added to the list of items that can be dropped by chests
                                        validEquipmentItems.Add( new( equipmentType, equipmentCategoryType ) );
                                    }
                                }
                            }
                        }
                    }
                    //WobPlugin.Log( "[Equipment] Rune Min Level Scale: " + Mathf.CeilToInt( RuneManager.GetMinRuneLevelScale() ) );
                    // Loop through all runes
                    foreach( RuneType runeType in RuneType_RL.TypeArray ) {
                        if( runeType != RuneType.None ) {
                            // Load the rune item for this type
                            RuneObj rune = RuneManager.GetRune( runeType );
                            // Check it exists, has defined data, and is not disabled by default or in mod settings
                            if( rune != null && rune.RuneData != null && !rune.RuneData.Disabled ) {
                                // Any that pass the validation checks are added to the list of items that can be dropped by chests
                                validRunes.Add( runeType );
                                //WobPlugin.Log( "[Equipment] Rune: " + runeType + ", " + rune.RuneData.BaseItemLevel + ", " + rune.RuneData.ScalingItemLevel );
                            }
                        }
                    }
                    // Loop through all challenges
                    foreach( ChallengeType challengeType in ChallengeType_RL.TypeArray ) {
                        if( challengeType != ChallengeType.None ) {
                            // Load the challenge item for this type
                            ChallengeObj challenge = ChallengeManager.GetChallenge( challengeType );
                            // Check it exists, has defined data, and is not disabled by default or in mod settings
                            if( challenge != null && challenge.ChallengeData != null && !challenge.ChallengeData.Disabled ) {
                                // Any that pass the validation checks are added to the list of items that can be dropped by chests
                                validChallenges.Add( challengeType );
                            }
                        }
                    }
                    // Set the flag so this won't run again
                    dropListsInitialised = true;
                }
            }

            // Default drop odds:
            //         Gold  Ore   Rune  Blueprint
            // Bronze  0.85  0     0     0.15
            // Silver  0.01  0     0     0.99
            // Gold    1     0     0     0         NOTE: These odds are ignored for all Gold chests, and the code actually returns Challenge
            // Fairy   0     0     1     0

            // If reward is Rune or Blueprint, and none is found override with
            // Bronze  Gold
            // Silver  Gold
            // Gold    Gold
            // Fairy   Ore

            // Always droped, no matter the roll...
            // Silver  EquipOre * 0.6
            // Gold    EquipOre * 2, RuneOre * 0.5
            // Fairy chests always drop rune ore, but not as the others do. There is an override additional drop of ore only if and item is dropped from the chest, so you get both.

            // Patch to add the extra drops
            internal static void Postfix( ChestObj __instance, ref SpecialItemType __result ) {
                // Check the chest type is a normal one that we want to modify
                if( patchEnabled && __instance.ChestType is ChestType.Bronze or ChestType.Silver or ChestType.Gold or ChestType.Fairy ) {
                    WobPlugin.Log( "[Chests] " + __instance.ChestType + " chest at level " + __instance.Level + ", ignore level: " + ignoreLevel + ", enabled: " + patchEnabled + ", original: " + __result );
                    // Load the lists of all potential items that are enabled and could be dropped by the game, irrespecive of whether the player has met the unlock conditions
                    InitialiseLists();
                    // Clear any previous chest drops remaining in the list from last run
                    levelDrops.Clear();
                    // Check that drops are wanted for this chest type
                    if( chestMaxDrops[__instance.ChestType] > 0 ) {
                        // Get the scaling based on total equipment items unlocked
                        int minEquipmentLevelScale = Mathf.CeilToInt( EquipmentManager.GetMinEquipmentLevelScale() );
                        // Get the rarity level for the current chest to compare to item required rarity levels
                        int chestRarityLevel = ChestType_RL.GetChestRarity( __instance.ChestType );
                        // Loop through the potential items
                        foreach( EquipItemKeys equipmentItem in validEquipmentItems ) {
                            // Get the object recording current unlock progress
                            EquipmentObj equipment = EquipmentManager.GetEquipment( equipmentItem.Slot, equipmentItem.Set );
                            // Check that there are still more to unlock, and that the item is allowed to be dropped from the current chest
                            if( !equipment.HasMaxBlueprints && chestRarityLevel >= equipment.EquipmentData.ChestRarityRequirement ) {
                                // Calculate the scaled item level
                                int itemLevel = equipment.EquipmentData.ChestLevelRequirement + minEquipmentLevelScale + ( ( EquipmentManager.GetUpgradeBlueprintsFound( equipment.CategoryType, equipment.EquipmentType ) + 1 ) * equipment.EquipmentData.ScalingItemLevel );
                                // Check that the chest can drop an item of this level
                                if( ignoreLevel || itemLevel <= __instance.Level ) {
                                    // Add the item to the list of allowed drops
                                    levelDrops.Add( new( itemLevel, new BlueprintDrop( equipment.CategoryType, equipment.EquipmentType ) ) );
                                    WobPlugin.Log( "[Chests] Add possible blueprint drop: " + equipment.CategoryType + " " + equipment.EquipmentType + ", " + EquipmentManager.GetUpgradeBlueprintsFound( equipment.CategoryType, equipment.EquipmentType ) + " / " + equipment.MaxLevel + ", at level " + itemLevel );
                                }
                            }
                        }
                        // Get the scaling based on total rune items unlocked
                        int minRuneLevelScale = Mathf.CeilToInt( RuneManager.GetMinRuneLevelScale() );
                        // Loop through the potential runes
                        foreach( RuneType runeType in validRunes ) {
                            // Get the object recording current unlock progress
                            RuneObj rune = RuneManager.GetRune( runeType );
                            // Check that there are still more to unlock, and that the player has unlocked the required heirloom for dashes and double jump runes
                            if( !rune.HasMaxBlueprints && SpecialItemDropUtility_HasHeirloom( rune.RuneType ) ) {
                                // Calculate the scaled rune level
                                int itemLevel = rune.RuneData.BaseItemLevel + minRuneLevelScale + ( ( RuneManager.GetUpgradeBlueprintsFound( rune.RuneType ) + 1 ) * rune.RuneData.ScalingItemLevel );
                                // Check that the chest can drop a rune of this level
                                if( ignoreLevel || itemLevel <= __instance.Level ) {
                                    // Add the rune to the list of allowed drops
                                    levelDrops.Add( new( itemLevel, new RuneDrop( rune.RuneType ) ) );
                                    WobPlugin.Log( "[Chests] Add possible blueprint drop: " + rune.RuneType + ", " + RuneManager.GetUpgradeBlueprintsFound( rune.RuneType ) + " / " + rune.MaxLevel + ", at level " + itemLevel );
                                }
                            }
                        }
                        // Temporary list of challenge empathies
                        List<ISpecialItemDrop> challenges = new();
                        // Loop through the potential challenge empathies
                        foreach( ChallengeType challengeType in validChallenges ) {
                            // Get the object recording current unlock progress
                            ChallengeObj challenge = ChallengeManager.GetChallenge( challengeType );
                            // Check that there are still more to unlock, and that the player has unlocked the challenge
                            if( !challenge.HasMaxBlueprints && challenge.FoundState != FoundState.NotFound ) {
                                // Calculate how many empathy levels are left to unlock
                                int count = Mathf.Min( chestMaxDrops[__instance.ChestType], challenge.MaxLevel - challenge.MaxEquippableLevel );
                                // Add one instance of the challenge drop for each empathy level left to unlock
                                for( int i = 0; i < count; i++ ) {
                                    challenges.Add( new ChallengeDrop( challenge.ChallengeType ) );
                                }
                                WobPlugin.Log( "[Chests] Add possible blueprint drop: " + challenge.ChallengeType + ", " + challenge.MaxEquippableLevel + " / " + challenge.MaxLevel + ", * " + count );
                            }
                        }
                        // So that we get a random mix of the available challenges, rather than all the same, remove random challenge empathies until we only have as many as can be dropped
                        while( challenges.Count > chestMaxDrops[__instance.ChestType] ) {
                            challenges.RemoveAt( Random.Range( 0, challenges.Count ) );
                        }
                        // Add the remaining challenge empathies to the list of allowed drops, always as the last item after equipment and runes
                        foreach( ISpecialItemDrop challenge in challenges ) {
                            levelDrops.Add( new( int.MaxValue, challenge ) );
                        }
                        // Sort into level order, so lower level items get dropped first
                        levelDrops.Sort( ( x, y ) => x.Level.CompareTo( y.Level ) );
                        //WobPlugin.Log( "[Chests] Drop options:" );
                        //for( int i = 0; i < levelDrops.Count; i++ ) {
                        //    WobPlugin.Log( "[Chests]     " + levelDrops[i].Level + ", " + levelDrops[i].Drop + ( i >= numDrops ? "" : " -> drop" ) );
                        //}
                        WobPlugin.Log( "[Chests]     Pool of " + levelDrops.Count + " drops" );
                        // If there are more dropable items in the list than the current chest can drop, remove the highest level item until at the limit
                        while( levelDrops.Count > chestMaxDrops[__instance.ChestType] ) {
                            levelDrops.RemoveAt( levelDrops.Count - 1 );
                        }
                    }
                    // Check that at least 1 drop has been found
                    if( levelDrops.Count > 0 ) {
                        // Change the method's return value
                        __result = levelDrops.Last().SpecialItem.SpecialItemType;
                        WobPlugin.Log( "[Chests]     Override drop of " + levelDrops.Last().SpecialItem.SpecialItemType );
                    } else {
                        __result = SpecialItemType.None;
                    }
                }
            }

            // Method to access private method SpecialItemDropUtility.HasHeirloom
            private static bool SpecialItemDropUtility_HasHeirloom( RuneType runeType ) {
                return Traverse.Create( typeof( SpecialItemDropUtility ) ).Method( "HasHeirloom", new System.Type[] { typeof( RuneType ) } ).GetValue<bool>( new object[] { runeType } );
            }

            // Method to set the items to be dropped as the return value and additional drops
            internal static ISpecialItemDrop SetDrops( ChestObj __instance, ISpecialItemDrop original ) {
                WobPlugin.Log( "[Chests] CalculateSpecialItemDropObj - " + __instance.ChestType + ", level: " + __instance.Level + ", original: " + original );
                // Get a reference to the drops list
                List<ISpecialItemDrop> m_specialItemDropsList = (List<ISpecialItemDrop>)Traverse.Create( __instance ).Field( "m_specialItemDropsList" ).GetValue();
                // Clear the list, to replace the original clear we removed
                m_specialItemDropsList.Clear();
                // Check that at least 1 drop has been found
                if( patchEnabled && __instance.ChestType is ChestType.Bronze or ChestType.Silver or ChestType.Gold or ChestType.Fairy ) {
                    if( levelDrops.Count > 0 ) {
                        // The last item in the list will be returned as the original method's return value
                        ISpecialItemDrop result = levelDrops[levelDrops.Count - 1].SpecialItem;
                        // Remove the last drop from the list
                        levelDrops.RemoveAt( levelDrops.Count - 1 );
                        // For all remaining drops in the list...
                        foreach( LevelDrop drop in levelDrops ) {
                            // Add the drop to the chest's additional drops list
                            m_specialItemDropsList.Add( drop.SpecialItem );
                            WobPlugin.Log( "[Chests]     Extra drop of " + drop.SpecialItem.SpecialItemType );
                        }
                        // Tidy up - clear the list of all dropped items
                        levelDrops.Clear();
                        // Return the override item
                        return result;
                    } else {
                        WobPlugin.Log( "[Chests] CalculateSpecialItemDropObj - return null" );
                        return null;
                    }
                } else {
                    WobPlugin.Log( "[Chests] CalculateSpecialItemDropObj - return original: " + original );
                    // Return the original method's return value unchanged
                    return original;
                }
            }

        }

        // Patch to add additional item drops to the list
        [HarmonyPatch( typeof( ChestObj ), "CalculateSpecialItemDropObj" )]
        internal static class ChestObj_CalculateSpecialItemDropObj_Patch {
            internal static void Postfix( ChestObj __instance, ref ISpecialItemDrop __result ) {
                __result = ChestObj_GetSpecialItemTypeToDrop_Patch.SetDrops( __instance, __result );
            }
        }

        [HarmonyPatch( typeof( ChestObj ), "DropRewardFromRegularChest" )]
        internal static class ChestObj_DropRewardFromRegularChest_Patch {
            // Patch to remove the clearing of a list of item drops so we can add extras before this method runs
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "ChestObj.DropRewardFromRegularChest" );
                // Patch to remove 'this.m_specialItemDropsList.Clear()' from the start of the method so we can add extra items
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldarg_0                               ), // this
                        /*  1 */ new( OpCodes.Ldfld, name: "m_specialItemDropsList" ), // this.m_specialItemDropsList
                        /*  2 */ new( OpCodes.Callvirt, name: "Clear"               ), // this.m_specialItemDropsList.Clear()
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_Remove( 0, 3 ), // Blank out the found instructions with nop instructions
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            internal static bool fairyGold = false;

            // Patch to add gold drops for fairy chests that don't have a special item in them
            internal static void Prefix( ChestObj __instance, SpecialItemType itemDropType, ISpecialItemDrop specialItemDrop, int chestLevel ) {
                WobPlugin.Log( "[Chests] DropRewardFromRegularChest - " + __instance.ChestType + ", itemDropType: " + itemDropType + ", specialItemDrop: " + specialItemDrop + ", chestLevel: " + chestLevel );
                if( fairyGold && __instance.ChestType == ChestType.Fairy && specialItemDrop == null ) {
                    Vector3 m_dropPosition = Traverse.Create( __instance ).Field<Vector3>( "m_dropPosition" ).Value;
                    ItemDropManager.DropGold( __instance.Gold, m_dropPosition, TraitManager.IsTraitActive( TraitType.ItemsGoFlying ), true, true );
                }
            }
        }

        // Patch to ignore item level restrictions when using the game's standard item drop method
        [HarmonyPatch( typeof( SpecialItemDropUtility ), nameof( SpecialItemDropUtility.GetBlueprintDrop ) )]
        internal static class SpecialItemDropUtility_GetBlueprintDrop_Patch {
            internal static void Prefix( ref int chestLevel ) {
                if( ChestObj_GetSpecialItemTypeToDrop_Patch.ignoreLevel ) { chestLevel = int.MaxValue; }
            }
        }

        // Patch to ignore item level restrictions when using the game's standard item drop method
        [HarmonyPatch( typeof( SpecialItemDropUtility ), nameof( SpecialItemDropUtility.GetRuneDrop ) )]
        internal static class SpecialItemDropUtility_GetRuneDrop_Patch {
            internal static void Prefix( ref int chestLevel ) {
                if( ChestObj_GetSpecialItemTypeToDrop_Patch.ignoreLevel ) { chestLevel = int.MaxValue; }
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - FAIRY CHEST RETRY
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Patch to prevent the room state being set to failed
        [HarmonyPatch( typeof( FairyRoomController ), nameof( FairyRoomController.State ), MethodType.Setter )]
        internal static class FairyRoomController_State_Patch {
            internal static void Prefix( ref FairyRoomState value ) {
                if( value == FairyRoomState.Failed && WobSettings.Get( "FairyChests", "FairyChestRetry", false ) ) {
                    value = FairyRoomState.NotRunning;
                }
            }
        }

        // Patch to reset rules instead of fail them
        [HarmonyPatch( typeof( FairyRoomController ), "SetAllFairyRulesFailed" )]
        internal static class FairyRoomController_SetAllFairyRulesFailed_Patch {
            internal static bool Prefix( FairyRoomController __instance ) {
                if( WobSettings.Get( "FairyChests", "FairyChestRetry", false ) ) {
                    Traverse.Create( __instance ).Method( "ResetAllFairyRules" ).GetValue();
                    return false;
                } else {
                    return true;
                }
            }
        }

        // Patch for the method that sets the failure state
        [HarmonyPatch( typeof( FairyRoomController ), "PlayerFailed" )]
        internal static class FairyRoomController_PlayerFailed_Patch {
            internal static bool Prefix( FairyRoomController __instance ) {
                if( WobSettings.Get( "FairyChests", "FairyChestRetry", false ) ) {
                    // Reset room state to not running
                    Traverse.Create( __instance ).Property( "State" ).SetValue( FairyRoomState.NotRunning );
                    // Stop checking the rules
                    Traverse.Create( __instance ).Method( "RunAllFairyRules", new System.Type[] { typeof( bool ) } ).GetValue( new object[] { false } );
                    // Make the chest invisible and prevent opening
                    __instance.Chest.Interactable.SetIsInteractableActive( false );
                    __instance.Chest.SetOpacity( 0f );
                    // Prevent the original method from running
                    return false;
                } else {
                    return true;
                }
            }
        }

        // Patch for the method that auto-fails the task on exit
        [HarmonyPatch( typeof( FairyRoomController ), "OnPlayerExitRoom" )]
        internal static class FairyRoomController_OnPlayerExitRoom_Patch {
            internal static void Prefix( FairyRoomController __instance ) {
                if( __instance.State != FairyRoomState.Passed && WobSettings.Get( "FairyChests", "FairyChestRetry", false ) ) {
                    // Reset room state to not running
                    Traverse.Create( __instance ).Property( "State" ).SetValue( FairyRoomState.NotRunning );
                    // Reset any failed rules
                    Traverse.Create( __instance ).Method( "ResetAllFairyRules" ).GetValue();
                    // Stop checking the rules
                    Traverse.Create( __instance ).Method( "RunAllFairyRules", new System.Type[] { typeof( bool ) } ).GetValue( new object[] { false } );
                    // Make the chest invisible and prevent opening
                    __instance.Chest.Interactable.SetIsInteractableActive( false );
                    __instance.Chest.SetOpacity( 0f );
                    // Reenable blocking walls if they exist in this room
                    GameObject m_roomTriggerWall = (GameObject)Traverse.Create( __instance ).Field( "m_roomTriggerWall" ).GetValue();
                    if( m_roomTriggerWall ) { m_roomTriggerWall.SetActive( true ); }
                }
            }
        }

        // Remove marking the room complete on save while running
        [HarmonyPatch( typeof( FairyRoomController ), "OnRoomDataSaved" )]
        internal static class FairyRoomController_OnRoomDataSaved_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "FairyRoomController.OnRoomDataSaved" );
                // Perform the patching
                if( WobSettings.Get( "FairyChests", "FairyChestRetry", false ) ) {
                    transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            // base.IsRoomComplete = true;
                            /*  0 */ new( OpCodes.Ldarg_0                          ), // base
                            /*  1 */ new( OpCodes.Ldc_I4_1                         ), // true
                            /*  2 */ new( OpCodes.Call, name: "set_IsRoomComplete" ), // base.IsRoomComplete = true
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_Remove( 0, 3 ),
                        }, expected: 1 );
                }
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

    }
}
