using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Wob_Common;

namespace Wob_UpgradeStats {
    [BepInPlugin( "Wob.UpgradeStats", "Upgrade Stat Gains Mod", "1.0.2" )]
    public partial class UpgradeStats : BaseUnityPlugin {
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Num<float>( "Upgrades", "Architect_Cost_Down",       "Drill Store - Reduce the Architect finder's fee. +X% fee reduction per rank",                                                                            2f,   0.01f, bounds: (0.1f, 20f     ) ),
                new WobSettings.Num<int>(   "Upgrades", "Armor_Up",                  "Foundry - Gain bonus Armor. Each point in Armor blocks one point of damage. (Max block cap: 35%). +X Armor per rank",                                    2,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Armor_Up2",                 "Blast Furnace - Gain bonus Armor. Each point in Armor blocks one point of damage. (Max block cap: 35%). +X Armor per rank",                              2,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Armor_Up3",                 "Some Kind of Kiln - Gain bonus Armor. Each point in Armor blocks one point of damage. (Max block cap: 35%). +X Armor per rank",                          2,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Attack_Up",                 "Arsenal - Increases Strength, raising Weapon Damage. +X Strength per rank",                                                                              1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Attack_Up2",                "Sauna - Increases Strength, raising Weapon Damage. +X Strength per rank",                                                                                1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Attack_Up3",                "Rock Climbing Wall - Increases Strength, raising Weapon Damage. +X Strength per rank",                                                                   1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<float>( "Upgrades", "Boss_Health_Restore",       "Meditation Studies - Restore Health and Mana when entering a Boss Chamber. +X% Health and Mana restored per rank",                                       20f,  0.01f, bounds: (0.1f, 20f     ) ),
                new WobSettings.Num<float>( "Upgrades", "Crit_Chance_Flat_Up",       "The Dicer's Den - Increases the chance of a random Weapon Crit. Also raises the chance for Skill Crits to become Super Crits. +X% Crit Chance per rank", 1f,   0.01f, bounds: (0.1f, 4f      ) ),
                new WobSettings.Num<float>( "Upgrades", "Crit_Damage_Up",            "The Laundromat - Increases damage from Weapon Crits. +X% Crit Damage per rank",                                                                          2f,   0.01f, bounds: (0.1f, 1000000f) ),
                new WobSettings.Num<float>( "Upgrades", "Dash_Strike_Up",            "Jousting Studies - You take reduced damage while Dashing. -X% damage taken per rank",                                                                    5f,   0.01f, bounds: (0.1f, 20f     ) ),
                new WobSettings.Num<int>(   "Upgrades", "Dexterity_Up1",             "Gym - Increases Dexterity, raising damage on Weapon Crits. +X Dexterity per rank",                                                                       1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Dexterity_Up2",             "Yoga Class - Increases Dexterity, raising damage on Weapon Crits. +X Dexterity per rank",                                                                1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Dexterity_Up3",             "Flower Shop - Increases Dexterity, raising damage on Weapon Crits. +X Dexterity per rank",                                                               1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<float>( "Upgrades", "Down_Strike_Up",            "Bamboo Garden - Spin Kick damage also scales with your INT. stat. +X% Intelligence Scaling per rank",                                                    10f,  0.01f, bounds: (0.1f, 20f     ) ),
                new WobSettings.Num<int>(   "Upgrades", "Equip_Up",                  "Fashion Chambers - Increases Max Weight Capacity. +X Equip Weight per rank",                                                                             10,          bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Equip_Up2",                 "Tailors - Increases Max Weight Capacity. +X Equip Weight per rank",                                                                                      10,          bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Equip_Up3",                 "Artisan - Increases Max Weight Capacity. +X Equip Weight per rank",                                                                                      10,          bounds: (1,    1000000 ) ),
                new WobSettings.Num<float>( "Upgrades", "Equipment_Ore_Gain_Up",     "Jeweler - Increase Ore gain. +X% Ore gain per rank",                                                                                                     5f,   0.01f, bounds: (0.1f, 1000000f) ),
                new WobSettings.Num<int>(   "Upgrades", "Focus_Up1",                 "Library - Increases Focus, raising damage on Spell Crits. +X Focus per rank",                                                                            1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Focus_Up2",                 "Hall of Wisdom - Increases Focus, raising damage on Spell Crits. +X Focus per rank",                                                                     1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Focus_Up3",                 "Court of the Wise - Increases Focus, raising damage on Spell Crits. +X Focus per rank",                                                                  1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<float>( "Upgrades", "Gold_Gain_Up",              "Massive Vault - Increase Gold Gain. +X% Gold gain per rank",                                                                                             5f,   0.01f, bounds: (0.1f, 1000000f) ),
                new WobSettings.Num<float>( "Upgrades", "Gold_Saved_Amount_Saved",   "Scribe's Office - Increase Living Safe's Gold Conversion. +X% Gold per rank",                                                                            4f,   0.01f, bounds: (0.1f, 4.5f    ) ),
                new WobSettings.Num<int>(   "Upgrades", "Gold_Saved_Cap_Up",         "Courthouse - Increase Living Safe's Max Gold Capacity. +X Gold per rank",                                                                                1500,        bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Health_Up",                 "Mess Hall - Increases Vitality, raising Max HP. +X Vitality per rank",                                                                                   1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Health_Up2",                "Fruit Juice Bar - Increases Vitality, raising Max HP. +X Vitality per rank",                                                                             1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Health_Up3",                "Meteora Gym - Increases Vitality, raising Max HP. +X Vitality per rank",                                                                                 1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Magic_Attack_Up",           "Study Hall - Increases Intelligence, raising Spell and Talent Damage. Increases Health Gain from Health Drops. +X Intelligence per rank",                1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Magic_Attack_Up2",          "Math Club - Increases Intelligence, raising Spell and Talent Damage. Increases Health Gain from Health Drops. +X Intelligence per rank",                 1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Magic_Attack_Up3",          "University - Increases Intelligence, raising Spell and Talent Damage. Increases Health Gain from Health Drops. +X Intelligence per rank",                1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<float>( "Upgrades", "Magic_Crit_Chance_Flat_Up", "The Quantum Observatory - Increases the chance of random Magic Crits. Adds a chance for Magic Crits to become Super Crits. +X% Crit Chance per rank",    1f,   0.01f, bounds: (0.1f, 4f      ) ),
                new WobSettings.Num<float>( "Upgrades", "Magic_Crit_Damage_Up",      "The Lodge - Increases damage from Spell Crits. +X% Crit Damage per rank",                                                                                2f,   0.01f, bounds: (0.1f, 1000000f) ),
                new WobSettings.Num<float>( "Upgrades", "Ore_Find_Up",               "Geologist's Camp - Chance to randomly find Ore in Breakables. +X% chance per rank",                                                                      1f,   0.01f, bounds: (0.1f, 20f     ) ),
                new WobSettings.Num<float>( "Upgrades", "Potion_Up",                 "Institute of Gastronomy - Improve INT. scaling from Health Drops. +X% Health gain per rank",                                                             4f,   0.01f, bounds: (0.1f, 1000000f) ),
                new WobSettings.Num<int>(   "Upgrades", "Randomize_Children",        "Career Center - Allows you to re-roll your characters. +X re-rolls per rank",                                                                            1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Relic_Cost_Down",           "Archeology Camp - Relics cost less Resolve. -X% Resolve cost per rank",                                                                                  1,    0.01f, bounds: (1,    20      ) ),
                new WobSettings.Num<int>(   "Upgrades", "Reroll_Relic",              "Medieval Forgery - You can now re-roll Relics and Curios found in the Kingdom. +X re-rolls per rank",                                                    1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Reroll_Relic_Room_Cap_L1",  "The Bizarre Bazaar - You can now re-roll Relics and Curios in the same location multiple times. +X re-rolls for 1st rank",                               1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Reroll_Relic_Room_Cap_L2",  "The Bizarre Bazaar - You can now re-roll Relics and Curios in the same location multiple times. +X re-rolls for 2nd rank",                               1,           bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Resolve_Up",                "Psychiatrist - Increase your starting Resolve. +X% Resolve per rank",                                                                                    1,    0.01f, bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Rune_Equip_Up",             "Etching Chambers - Increases Max Rune Weight. +X Rune Weight per rank",                                                                                  10,          bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Rune_Equip_Up2",            "Pillow Mill - Increases Max Rune Weight. +X Rune Weight per rank",                                                                                       10,          bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Rune_Equip_Up3",            "Bed Mill - Increases Max Rune Weight. +X Rune Weight per rank",                                                                                          10,          bounds: (1,    1000000 ) ),
                new WobSettings.Num<float>( "Upgrades", "Rune_Ore_Find_Up",          "Dowsing Center - Chance to randomly find Red Aether in Breakables. +X% chance per rank",                                                                 1f,   0.01f, bounds: (0.1f, 20f     ) ),
                new WobSettings.Num<float>( "Upgrades", "Rune_Ore_Gain_Up",          "Buried Tomb - Increase Red Aether gain. +X% Aether gain per rank",                                                                                       5f,   0.01f, bounds: (0.1f, 1000000f) ),
                new WobSettings.Num<int>(   "Upgrades", "Traits_Give_Gold_Mod",      "Repurposed Mining Shaft - Increases Gold Gain for certain Traits. +X% Gold granted by Trait bonus per rank",                                             10,   0.01f, bounds: (1,    1000000 ) ),
                new WobSettings.Num<int>(   "Upgrades", "Weight_CD_Reduce",          "Aerobics Classroom - Increases Encumbrance Limits. *Your Weight Class will now change every X%. +X% Encumbrance per rank",                               1,    0.01f, bounds: (1,    8       ) ),
                new WobSettings.Num<float>( "Upgrades", "XP_Up",                     "Trophy Room - Increase XP Gain. +X% XP per rank",                                                                                                        2.5f, 0.01f, bounds: (0.1f, 1000000f) ),
            } );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        // Change the stat gain just before the firest level stat gain is read
        [HarmonyPatch( typeof( SkillTreeObj ), nameof( SkillTreeObj.FirstLevelStatGain ), MethodType.Getter )]
        internal static class SkillTreeObj_FirstLevelStatGain_Patch {
            internal static void Prefix( SkillTreeObj __instance ) {
                // Putting this in a variable just to shorten the name and make the rest of this easier to read
                SkillTreeData skill = __instance.SkillTreeData;
                if( __instance.SkillTreeType == SkillTreeType.Reroll_Relic_Room_Cap ) {
                    // Search the config for a setting that has the same name as the internal name of the skill
                    float statGain1 = WobSettings.Get( "Upgrades", skill.Name + "_L1", skill.FirstLevelStatGain );
                    float statGain2 = WobSettings.Get( "Upgrades", skill.Name + "_L2", skill.AdditionalLevelStatGain );
                    // Check if the stat gain is already what we want it to be
                    if( statGain1 != skill.FirstLevelStatGain || statGain2 != skill.AdditionalLevelStatGain ) {
                        // Report the change to the log
                        WobPlugin.Log( "Changing bonus for " + skill.Name + " from " + skill.FirstLevelStatGain + "/" + skill.AdditionalLevelStatGain + " to " + statGain1 + "/" + statGain2 );
                        // Set the stat gain for all ranks
                        skill.FirstLevelStatGain = statGain1;
                        skill.AdditionalLevelStatGain = statGain2;
                    }
                } else {
                    // Search the config for a setting that has the same name as the internal name of the skill
                    float statGain = WobSettings.Get( "Upgrades", skill.Name, skill.FirstLevelStatGain );
                    // Check if the stat gain is already what we want it to be
                    if( statGain != skill.FirstLevelStatGain || statGain != skill.AdditionalLevelStatGain ) {
                        // Report the change to the log
                        WobPlugin.Log( "Changing bonus for " + skill.Name + " from " + skill.FirstLevelStatGain + "/" + skill.AdditionalLevelStatGain + " to " + statGain );
                        // Set the stat gain for all ranks
                        skill.FirstLevelStatGain = statGain;
                        skill.AdditionalLevelStatGain = statGain;
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        // Reroll for heirs seems to ignore the stat gain and just use the upgrade level - this is to fix that
        // Patch for the method that controls initial setup of the UI elements for character selection, including reroll
        [HarmonyPatch( typeof( LineageWindowController ), "OnOpen" )]
        internal static class LineageWindowController_OnOpen_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "LineageWindowController.OnOpen Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Call, name: "GetSkillObjLevel" ), // SkillTreeManager.GetSkillObjLevel( SkillTreeType.Randomize_Children )
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 0, SymbolExtensions.GetMethodInfo( () => SkillTreeManager.GetSkillTreeObj( SkillTreeType.None ) ) ), // Replace the call with one that gets the upgrade itself
                            new WobTranspiler.OpAction_Insert( 1, new List<CodeInstruction>() {
                                new CodeInstruction( OpCodes.Callvirt, AccessTools.Method( typeof( SkillTreeObj ), "get_CurrentStatGain" ) ), // Add a call to the upgrade's CurrentStatGain getter
                                new CodeInstruction( OpCodes.Conv_I4 ), // Cast the float return value to int
                            } ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        // Correct the total resolve cost calculation to prevent negative costs from increased effect of Archeology Camp (Relic_Cost_Down)
        [HarmonyPatch( typeof( PlayerSaveData ), nameof( PlayerSaveData.GetTotalRelicResolveCost ) )]
        internal static class PlayerSaveData_GetTotalRelicResolveCost_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "PlayerSaveData.GetTotalRelicResolveCost Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc ), // num2
                            /*  1 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc ), // relicCostMod
                            /*  2 */ new WobTranspiler.OpTest( OpCodes.Sub     ), // num2 - relicCostMod
                            /*  3 */ new WobTranspiler.OpTest( OpCodeSet.Stloc ), // num2 = num2 - relicCostMod
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_Insert( 3, new List<CodeInstruction> {
                                new CodeInstruction( OpCodes.Ldc_R4, 0f ),
                                new CodeInstruction( OpCodes.Ldc_R4, float.MaxValue ),
                                new CodeInstruction( OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Mathf.Clamp( 0f, 0f, float.MaxValue ) ) ),
                            } ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        // Prevent game freeze if rerolling relics too many times
        [HarmonyPatch( typeof( RelicRoomPropController ), nameof( RelicRoomPropController.RollRelics ) )]
        internal static class RelicRoomPropController_RollRelics_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "RelicRoomPropController.RollRelics Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                         ), // this
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldflda                          ), // this.m_relicTypes
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc                         ), // rngIDToUse
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4_0                        ), // false
                            /*  4 */ new WobTranspiler.OpTest( OpCodes.Ldsfld, name: "m_exclusionList" ), // m_exclusionList
                            /*  5 */ new WobTranspiler.OpTest( OpCodes.Call, name: "GetRandomRelic"    ), // RelicLibrary.GetRandomRelic(rngIDToUse, false, m_exclusionList)
                            /*  6 */ new WobTranspiler.OpTest( OpCodes.Call, name: "set_x"             ), // this.m_relicTypes.x = 
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_Insert( 5, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => CheckExclusionListX( null ) ) ),
                        } );
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                         ), // this
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldflda                          ), // this.m_relicTypes
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc                         ), // rngIDToUse
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4_0                        ), // false
                            /*  4 */ new WobTranspiler.OpTest( OpCodes.Ldsfld, name: "m_exclusionList" ), // m_exclusionList
                            /*  5 */ new WobTranspiler.OpTest( OpCodes.Call, name: "GetRandomRelic"    ), // RelicLibrary.GetRandomRelic(rngIDToUse, false, m_exclusionList)
                            /*  6 */ new WobTranspiler.OpTest( OpCodes.Call, name: "set_y"             ), // this.m_relicTypes.y = 
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_Insert( 5, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => CheckExclusionListY( null ) ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            // Reset exclusions if there are no unseen relics left to roll
            private static List<RelicType> CheckExclusionListX( List<RelicType> exclusionList ) { return CheckExclusionList( exclusionList, 0 ); }
            // Reset exclusions if there is 1 unseen relics left to roll, as that one will be in X and duplicates are not allowed
            private static List<RelicType> CheckExclusionListY( List<RelicType> exclusionList ) { return CheckExclusionList( exclusionList, 1 ); }
            // Check the exclusion list to ensure that it is possible to roll relics, and reset the list if too many are excluded
            private static List<RelicType> CheckExclusionList( List<RelicType> exclusionList, int resetAt ) {
                List<RelicType> allowedTypes = new List<RelicType>();
                foreach( RelicType relicType in RelicType_RL.TypeArray ) {
                    if( RelicLibrary.IsRelicAllowed( relicType ) ) {
                        //RelicData relicData = RelicLibrary.GetRelicData( relicType );
                        //RelicObj relicObj = SaveManager.PlayerSaveData.GetRelic( relicType );
                        //if( relicData != null && relicObj != null && relicObj.Level <= relicData.MaxStack ) {
                        if( !exclusionList.Contains( relicType ) ) {
                            allowedTypes.Add( relicType );
                        }
                        //}
                    }
                }
                if( allowedTypes.Count <= resetAt ) {
                    WobPlugin.Log( "Relic reroll list reset" );
                    exclusionList.Clear();
                    if( ChallengeManager.IsInChallenge ) {
                        exclusionList.AddRange( Challenge_EV.RELIC_EXCLUSION_ARRAY );
                    }
                }
                return exclusionList;
            }
        }

        // Reset available abilities list if rerolled through all of them, instead of using the sword weapon ability
        [HarmonyPatch( typeof( SwapAbilityRoomPropController ), nameof( SwapAbilityRoomPropController.RollAbilities ) )]
        internal static class SwapAbilityRoomPropController_RollAbilities_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "SwapAbilityRoomPropController.RollAbilities Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
							// this.m_abilityType = AbilityType.SwordWeapon; break;
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                           ), // this
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4_S, AbilityType.SwordWeapon ), // AbilityType.SwordWeapon
                            /*  2 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "m_abilityType"      ), // this.m_abilityType = AbilityType.SwordWeapon
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Br                                ), // break
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_SetInstruction( 1, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => RepopulateList( null ) ) ),
                                new WobTranspiler.OpAction_Remove( 2, 2 ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static void RepopulateList( SwapAbilityRoomPropController instance ) {
                // Find out if the ability should be a weapon, talent, or spell
                CastAbilityType castAbilityType = Traverse.Create( instance ).Field( "m_castAbilityTypeToSwap" ).GetValue<CastAbilityType>();
                // Call the method that lists available abilities for the category (patched)
                AbilityType[] abilityArray = Traverse.Create( instance ).Method( "GetAbilityArray", new object[] { typeof( CastAbilityType ) } ).GetValue<AbilityType[]>( new object[] { castAbilityType } );
                // Get a reference to the list to be repopulated
                List<AbilityType> m_potentialAbilityList = Traverse.Create( typeof( SwapAbilityRoomPropController ) ).Field( "m_potentialAbilityList" ).GetValue<List<AbilityType>>();
                // Check and copy each ability into the list
                for( int i = 0; i < abilityArray.Length; i++ ) {
                    if( abilityArray[i] != AbilityType.None && AbilityLibrary.GetAbility( abilityArray[i] ) ) {
                        m_potentialAbilityList.Add( abilityArray[i] );
                    }
                }
                // If the list is still somehow empty after attempting to repopulate, fall back to giving a sword as before modifying the method
                if( m_potentialAbilityList.Count == 0 ) {
                    // Add the sword to the list so random generation still works
                    m_potentialAbilityList.Add( AbilityType.SwordWeapon );
                    // Set the rolled ability to the sword so the while loop exits
                    Traverse.Create( instance ).Field( "m_abilityType" ).SetValue( AbilityType.SwordWeapon );
                }
            }
        }
    }
}