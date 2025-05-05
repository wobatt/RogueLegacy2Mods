using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using RL_Windows;
using RLAudio;
using UnityEngine;
using Wob_Common;

namespace Wob__Test {
    [BepInPlugin( "Wob._Test", "Wob's Test Mod", "0.2.0" )]
    public partial class Test : BaseUnityPlugin {
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options

            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        [HarmonyPatch( typeof( RelicRoomPropController ), nameof( RelicRoomPropController.RollRelics ) )]
        internal static class RelicRoomPropController_RollRelics_Patch {
            internal static void Postfix() {
                RelicObj relic = SaveManager.PlayerSaveData.GetRelic( RelicType.ManaShield );
                if( relic.Level > 0 ) {
                    SaveManager.PlayerSaveData.GetRelic( RelicType.DamageBuffStatusEffect ).SetLevel( relic.Level, true, true );
                    relic.SetLevel( 0, false, true );
                }
            }
        }

        // Make twin relics triple relics if it would complete the stack
        [HarmonyPatch( typeof( RelicObj ), nameof( RelicObj.SetLevel ) )]
        internal static class RelicObj_SetLevel_Patch {
            internal static void Prefix( RelicObj __instance, ref int value, bool additive ) {
                if( additive && value == 2 && __instance.RelicType != RelicType.CurseRandomRelics ) {
                    RelicData relicData = RelicLibrary.GetRelicData( __instance.RelicType );
                    if( relicData != null && ( relicData.MaxStack - __instance.Level ) == 3 ) {
                        value = 3;
                    }
                }
            }
        }

        // Enable or disable a relic for random spawn and change the resolve costs
        [HarmonyPatch( typeof( RelicLibrary ), "Instance", MethodType.Getter )]
        internal static class RelicLibrary_Instance_Getter_Patch {
            private static bool runOnce = false;
            internal static void Postfix( RelicLibrary __result ) {
                if( !runOnce ) {
                    RelicTypeRelicDataDictionary m_relicLibrary = Traverse.Create( __result ).Field<RelicTypeRelicDataDictionary>( "m_relicLibrary" ).Value;
                    RelicData relicData = m_relicLibrary[RelicType.CurseRandomRelics];
                    if( relicData != null ) {
                        relicData.MaxStack = 2;
                    }
                    runOnce = true;
                }
            }
        }

        [HarmonyPatch( typeof( RelicRoomPropController ), "ChooseRelic" )]
        internal static class RelicRoomPropController_ChooseRelic_Patch {
            // Find the 'MoveNext' method on the nested class of 'RelicRoomPropController' that 'ChooseRelic' implicitly created
            internal static MethodInfo TargetMethod() {
                return AccessTools.FirstMethod( AccessTools.FirstInner( typeof( RelicRoomPropController ), t => t.Name.Contains( "<ChooseRelic>d__" ) ), method => method.Name.Contains( "MoveNext" ) );
            }
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "RelicRoomPropController.ChooseRelic" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldc_I4_3                           ), // 3
                        /*  1 */ new( OpCodeSet.Stloc                            ), // int num = 3
                        /*  2 */ new( OpCodes.Ldarg_0                            ), // this
                        /*  3 */ new( OpCodes.Ldfld, name: "relicModType"        ), // this.relicModType
                        /*  4 */ new( OpCodeSet.Ldc_I4, RelicModType.DoubleRelic ), // RelicModType.DoubleRelic     // 10
                        /*  5 */ new( OpCodeSet.Bne_Un                           ), // if( this.relicModType == RelicModType.DoubleRelic )
                        /*  6 */ new( OpCodeSet.Ldloc                            ), // num
                        /*  7 */ new( OpCodes.Ldc_I4_2                           ), // 2
                        /*  8 */ new( OpCodes.Mul                                ), // num * 2
                        /*  9 */ new( OpCodeSet.Stloc                            ), // num = num * 2
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetInstruction( 4, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => GetRandomRelics( 0 ) ) ),
                        new WobTranspiler.OpAction_Remove( 5, 4 ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static readonly HashSet<RelicType> curseRelics = new() { RelicType.GoldCombatChallenge, RelicType.FoodChallenge, RelicType.ResolveCombatChallenge };

            private static int GetRandomRelics( RelicModType relicModType ) {
                WobPlugin.Log( "[TEST] Mammon's Bounty random relics called, " + relicModType );
                List<RelicType> mandatoryDrops = new();
                int relicCount = relicModType == RelicModType.DoubleRelic ? 6 : 3;
                // Loop through all relics in the game
                foreach( RelicType relicType in RelicType_RL.TypeArray ) {
                    // Look up the relic base data so we can check its max stack size
                    RelicData relicData = RelicLibrary.GetRelicData( relicType );
                    // Check if it can be used by the player, if it is currently excluded, and is not the special Empty Vessel
                    if( relicData != null && RelicLibrary.IsRelicAllowed( relicType ) && relicData.MaxStack - SaveManager.PlayerSaveData.GetRelic( relicType ).Level == 1 ) {
                        if( !curseRelics.Contains( relicType ) && ( relicType != RelicType.RemoveVisuals || TraitManager.ActiveTraitTypeList.Intersect( Relic_EV.REMOVE_VISUALS_TRAIT_ARRAY ).Count() > 0 ) ) {
                            WobPlugin.Log( "[TEST]      Add relic: " + relicType );
                            mandatoryDrops.Add( relicType );
                        } else {
                            WobPlugin.Log( "[TEST]      Skip relic: " + relicType );
                        }
                    }
                }
                if( mandatoryDrops.Count < relicCount && SaveManager.PlayerSaveData.GetRelic( RelicType.ExtraLife ).Level < 5 ) {
                    mandatoryDrops.Add( RelicType.ExtraLife );
                }
                if( mandatoryDrops.Count < relicCount ) {
                    mandatoryDrops.Add( RelicType.ReplacementRelic );
                }
                WobPlugin.Log( "[TEST]      Relics in initial list: " + mandatoryDrops.Count );
                while( mandatoryDrops.Count > relicCount ) {
                    mandatoryDrops.RemoveAt( UnityEngine.Random.Range( 0, mandatoryDrops.Count ) );
                }
                WobPlugin.Log( "[TEST]      Relics in final list: " + mandatoryDrops.Count );
                SpecialItemDropWindowController specialItemDropWindowController = WindowManager.GetWindowController(WindowID.SpecialItemDrop) as SpecialItemDropWindowController;
                List<RelicType> m_cursedRandomRelicHelper = Traverse.Create( typeof( RelicRoomPropController ) ).Field( "m_cursedRandomRelicHelper" ).GetValue<List<RelicType>>();
                foreach( RelicType relicType in mandatoryDrops ) {
                    WobPlugin.Log( "[TEST]      Drop relic: " + relicType );
                    RelicDrop relicDrop = new( relicType, RelicModType.None );
                    specialItemDropWindowController.AddSpecialItemDrop( relicDrop );
                    m_cursedRandomRelicHelper.Add( relicType );
                }
                WobPlugin.Log( "[TEST] Mammon's Bounty random relics complete, randoms remaining: " + ( relicCount - mandatoryDrops.Count ) );
                return relicCount - mandatoryDrops.Count;
            }
        }

        // This patch simply dumps data to the debug log when the castle skill tree is opened - useful for getting internal names and default values for patches
        [HarmonyPatch( typeof( SkillTreeWindowController ), nameof( SkillTreeWindowController.Initialize ) )]
        internal static class SkillTreeWindowController_Initialize_Patch {
            internal static void Postfix() {
                //foreach( ChestType chestType in new[] { ChestType.Bronze, ChestType.Silver, ChestType.Gold, ChestType.Fairy } ) {
                //    Vector2[] chestItemTypeOdds = Economy_EV.GetChestItemTypeOdds( chestType );
                //    foreach( Vector2 chestItem in chestItemTypeOdds ) {
                //        WobPlugin.Log( "[DataDump] [ChestDrops] " + chestType + "," + (SpecialItemType)chestItem.x + "," + chestItem.y );
                //    }
                //    Vector2Int goldAmt = Economy_EV.BASE_GOLD_DROP_AMOUNT[chestType];
                //    float goldMod = Economy_EV.CHEST_TYPE_GOLD_MOD[chestType];
                //    WobPlugin.Log( "[DataDump] [ChestGold] " + chestType + "," + goldAmt.x + "," + goldAmt.y + "," + goldMod );
                //}
                //foreach( AbilityType abilityType in AbilityType_RL.TypeArray ) {
                //    BaseAbility_RL ability = AbilityLibrary.GetAbility( abilityType );
                //    if( ability != null ) {
                //        AbilityData abilityData = ability.AbilityData;
                //        if( abilityData != null ) {
                //            WobPlugin.Log( "[DataDump] [Abilities] " + abilityType + "," + abilityData.CooldownDecreaseOverTime + "," + abilityData.CooldownDecreasePerHit
                //                + "," + abilityData.CooldownTime + "," + abilityData.LockoutTime + "," + abilityData.MaxAmmo + "," + abilityData.CooldownRefreshesAllAmmo + "," + abilityData.BaseCost );
                //        }
                //    }
                //}
                //foreach( ProjectileEntry projectileEntry in ProjectileLibrary.ProjectileEntryArray ) {
                //    Projectile_RL projectile = projectileEntry.ProjectilePrefab;
                //    if( projectile.ProjectileData != null ) {
                //        WobPlugin.Log( "[DataDump] [Projectiles] " + projectile.ProjectileData.Name + "," + projectile.ProjectileData.DamageType
                //            + "," + projectile.ProjectileData.StrengthScale + "," + projectile.ProjectileData.MagicScale + "," + projectile.ProjectileData.ManaGainPerHit + "," + projectile.ProjectileData.KnockbackStrength
                //            + "," + projectile.ProjectileData.LifeSpan + "," + projectile.ProjectileData.Speed + "," + projectile.ProjectileData.TurnSpeed + "," + projectile.ProjectileData.CooldownReductionPerHit
                //            + "," + (int)projectile.CanCollideWithFlags + ",\"" + projectile.CanCollideWithFlags + "\"," + (int)projectile.CollisionFlags + ",\"" + projectile.CollisionFlags + "\"," + projectile.CanHitWall + "," + projectile.CanHitOwner );
                //    }
                //}
                //foreach( SkillTreeType skillTreeType in SkillTreeType_RL.TypeArray ) {
                //    SkillTreeObj skillTreeObj = SkillTreeManager.GetSkillTreeObj( skillTreeType );
                //    if( skillTreeObj != null ) {
                //        SkillTreeData skillTreeData = skillTreeObj.SkillTreeData;
                //        if( skillTreeData != null ) {
                //            WobPlugin.Log( "[DataDump] [CastleSkills] " + skillTreeType + "," + skillTreeData.FirstLevelStatGain + "," + skillTreeData.AdditionalLevelStatGain );
                //        }
                //    }
                //}
                //foreach( ClassType classType in ClassType_RL.TypeArray ) {
                //    ClassData classData = ClassLibrary.GetClassData( classType );
                //    if( classData != null ) {
                //        ClassPassiveData classPassiveData = classData.PassiveData;
                //        if( classPassiveData != null ) {
                //            WobPlugin.Log( "[DataDump] [Classes] " + classType
                //                + "," + classPassiveData.MaxHPMod + "," + classPassiveData.MaxManaMod + "," + classPassiveData.ArmorMod + "," + classPassiveData.VitalityMod
                //                + "," + classPassiveData.StrengthMod + "," + classPassiveData.IntelligenceMod + "," + classPassiveData.DexterityMod + "," + classPassiveData.FocusMod
                //                + "," + classPassiveData.WeaponCritChanceAdd + "," + classPassiveData.MagicCritChanceAdd + "," + classPassiveData.WeaponCritDamageAdd + "," + classPassiveData.MagicCritDamageAdd
                //                + "," + classPassiveData.ManaRegenType + "," + classPassiveData.Special );
                //        }
                //    }
                //}
                //foreach( EquipmentType setType in EquipmentType_RL.TypeArray ) {
                //    if( setType is EquipmentType.None or EquipmentType.GEAR_EMPTY_1 or EquipmentType.GEAR_EMPTY_2 ) { continue; }
                //    EquipmentSetData setData = EquipmentSetLibrary.GetEquipmentSetData( setType );
                //    if( setData != null ) {
                //        WobPlugin.Log( "[DataDump] [Equipment.Set] " + setType + "," + setData.SetBonus01.BonusType + "," + setData.SetBonus01.StatGain + "," + setData.SetBonus02.BonusType + "," + setData.SetBonus02.StatGain + "," + setData.SetBonus03.BonusType + "," + setData.SetBonus03.StatGain );
                //    }
                //}
                //foreach( EquipmentType setType in EquipmentType_RL.TypeArray ) {
                //    if( setType is EquipmentType.None or EquipmentType.GEAR_EMPTY_1 or EquipmentType.GEAR_EMPTY_2 ) { continue; }
                //    foreach( EquipmentCategoryType slotType in EquipmentType_RL.CategoryTypeArray ) {
                //        EquipmentData equipData = EquipmentLibrary.GetEquipmentData( slotType, setType );
                //        if( equipData != null ) {
                //            WobPlugin.Log( "[DataDump] [Equipment.Item] " + setType + "," + slotType
                //                + "," + equipData.ChestLevelRequirement + "," + equipData.ScalingItemLevel
                //                + "," + equipData.BaseEquipmentSetLevel + "," + equipData.ScalingEquipmentSetLevel
                //                + "," + equipData.ChestRarityRequirement
                //                + "," + equipData.GoldCost + "," + equipData.OreCost
                //                + "," + equipData.BaseWeight + "," + equipData.ScalingWeight
                //                + "," + equipData.BaseHealth + "," + equipData.ScalingHealth
                //                + "," + equipData.BaseArmor + "," + equipData.ScalingArmor
                //                + "," + equipData.BaseStrengthDamage + "," + equipData.ScalingStrengthDamage
                //                + "," + equipData.BaseStrengthCritChance + "," + equipData.ScalingStrengthCritChance
                //                + "," + equipData.BaseMagicDamage + "," + equipData.ScalingMagicDamage
                //                + "," + equipData.BaseMagicCritChance + "," + equipData.ScalingMagicCritChance );
                //        }
                //    }
                //}
                //foreach( BurdenType burdenType in BurdenType_RL.TypeArray ) {
                //    BurdenData burdenData = BurdenLibrary.GetBurdenData( burdenType );
                //    if( burdenData != null ) {
                //        WobPlugin.Log( "[DataDump] [NewGamePlus] " + burdenType + "," + burdenData.MaxBurdenLevel + "," + burdenData.InitialBurdenCost + "," + burdenData.StatsGain );
                //    }
                //}
                //foreach( RelicType relicType in RelicType_RL.TypeArray ) {
                //    RelicData relicData = RelicLibrary.GetRelicData( relicType );
                //    if( relicData != null ) {
                //        WobPlugin.Log( "[DataDump] [Relics] " + relicType + "," + relicData.Rarity + "," + relicData.CostAmount + "," + relicData.MaxStack );
                //    }
                //}
                //foreach( RuneType runeType in RuneType_RL.TypeArray ) {
                //    RuneData runeData = RuneLibrary.GetRuneData( runeType );
                //    if( runeData != null && !runeData.Disabled ) {
                //        WobPlugin.Log( "[DataDump] [Runes] " + runeType + "," + runeData.Disabled
                //            + "," + runeData.GoldCost + "," + runeData.BlackStoneCost
                //            + "," + runeData.BaseWeight + "," + runeData.ScalingWeight
                //            + "," + runeData.StatMod01 + "," + runeData.ScalingStatMod01 );
                //    }
                //}
                //foreach( SoulShopType soulShopType in SoulShopType_RL.TypeArray ) {
                //    SoulShopData soulShopData = SoulShopLibrary.GetSoulShopData( soulShopType );
                //    if( soulShopData != null && !soulShopData.Disabled ) {
                //        WobPlugin.Log( "[DataDump] [SoulShop] " + soulShopType + "," + soulShopData.Disabled + "," + soulShopData.BaseCost + "," + soulShopData.ScalingCost
                //            + "," + soulShopData.MaxLevelScalingCap + "," + soulShopData.MaxLevel + "," + soulShopData.OverloadMaxLevel + "," + soulShopData.UnlockLevel );
                //    }
                //}
                //foreach( TraitType traitType in TraitType_RL.TypeArray ) {
                //    TraitData traitData = TraitLibrary.GetTraitData( traitType );
                //    if( traitData != null ) {
                //        WobPlugin.Log( "[DataDump] [Traits] " + traitType + "," + traitData.Rarity + "," + traitData.GoldBonus );
                //    }
                //}
            }
        }

    }
}