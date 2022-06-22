using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace Wob_EquipStats {
    [BepInPlugin( "Wob.EquipStats", "Equipment Stats Mod", "0.1.0" )]
    public partial class EquipStats : BaseUnityPlugin {

        private static readonly WobSettings.KeyHelper<EquipmentType> setKeys = new WobSettings.KeyHelper<EquipmentType>( "EquipSet" );
        private static readonly Dictionary<EquipmentType,(string Config, EquipmentSetBonusType BonusType1, float StatGain1, EquipmentSetBonusType BonusType2, float StatGain2, EquipmentSetBonusType BonusType3, float StatGain3, int BaseLevel, int ScalingLevel)> SetInfo = new Dictionary<EquipmentType,(string Config, EquipmentSetBonusType BonusType1, float StatGain1, EquipmentSetBonusType BonusType2, float StatGain2, EquipmentSetBonusType BonusType3, float StatGain3, int BaseLevel, int ScalingLevel)>() {
            { EquipmentType.GEAR_BONUS_WEIGHT,  ( "Leather",   EquipmentSetBonusType.Resolve,       0.2f, EquipmentSetBonusType.Resolve,       0.3f,  EquipmentSetBonusType.MinimumResolve,       0.3f,  1,  12 ) },
            { EquipmentType.GEAR_MAGIC_CRIT,    ( "Scholar",   EquipmentSetBonusType.Focus_Add,     10f,  EquipmentSetBonusType.Focus_Add,     20f,   EquipmentSetBonusType.FlatMagicCritChance,  0.2f,  5,  16 ) },
            { EquipmentType.GEAR_STRENGTH_CRIT, ( "Warden",    EquipmentSetBonusType.Dexterity_Add, 10f,  EquipmentSetBonusType.Dexterity_Add, 20f,   EquipmentSetBonusType.FlatWeaponCritChance, 0.2f,  12, 20 ) },
            { EquipmentType.GEAR_LIFE_STEAL,    ( "Sanguine",  EquipmentSetBonusType.LifeSteal,     2f,   EquipmentSetBonusType.LifeSteal,     3f,    EquipmentSetBonusType.ReturnDamage,         3.75f, 16, 24 ) },
            { EquipmentType.GEAR_ARMOR,         ( "Ammonite",  EquipmentSetBonusType.Armor,         30f,  EquipmentSetBonusType.Armor,         60f,   EquipmentSetBonusType.ArmorMod,             0.4f,  19, 28 ) },
            { EquipmentType.GEAR_MAGIC_DMG,     ( "Crescent",  EquipmentSetBonusType.MagicAdd,      15f,  EquipmentSetBonusType.MagicAdd,      30f,   EquipmentSetBonusType.MagicMod,             0.2f,  25, 32 ) },
            { EquipmentType.GEAR_MOBILITY,      ( "Drowned",   EquipmentSetBonusType.StrengthAdd,   15f,  EquipmentSetBonusType.StrengthAdd,   30f,   EquipmentSetBonusType.StrengthMod,          0.2f,  32, 36 ) },
            { EquipmentType.GEAR_GOLD,          ( "Gilded",    EquipmentSetBonusType.OreAetherGain, 0.1f, EquipmentSetBonusType.OreAetherGain, 0.2f,  EquipmentSetBonusType.GoldGain,             0.5f,  40, 40 ) },
            { EquipmentType.GEAR_RETURN_DMG,    ( "Obsidian",  EquipmentSetBonusType.VitalityAdd,   15f,  EquipmentSetBonusType.VitalityAdd,   30f,   EquipmentSetBonusType.VitalityMod,          0.2f,  48, 44 ) },
            { EquipmentType.GEAR_MAG_ON_HIT,    ( "Leviathan", EquipmentSetBonusType.ManaRegenMod,  0.1f, EquipmentSetBonusType.ManaRegenMod,  0.15f, EquipmentSetBonusType.SoulSteal,            5f,    54, 48 ) },
            { EquipmentType.GEAR_LIFE_STEAL_2,  ( "Kin",       EquipmentSetBonusType.ReturnDamage,  1.5f, EquipmentSetBonusType.MaxMana,       50f,   EquipmentSetBonusType.Revives,              1f,    60, 52 ) },
        };

        private static readonly WobSettings.KeyHelper<EquipmentCategoryType> slotKeys = new WobSettings.KeyHelper<EquipmentCategoryType>( "EquipSlot" );
        private enum BlueprintRarity { Common = 0, Silver = 1 }
        private static readonly Dictionary<EquipmentCategoryType,(string Config, string LibraryField, int Unity, BlueprintRarity Rarity, int Level)> SlotInfo = new Dictionary<EquipmentCategoryType,(string Config, string LibraryField, int Unity, BlueprintRarity Rarity, int Level)>() {
            { EquipmentCategoryType.Weapon,  ( "Weapon",  "m_weaponEquipmentLibrary",  5,  BlueprintRarity.Common, 0 ) },
            { EquipmentCategoryType.Head,    ( "Head",    "m_headEquipmentLibrary",    5,  BlueprintRarity.Common, 0 ) },
            { EquipmentCategoryType.Chest,   ( "Chest",   "m_chestEquipmentLibrary",   5,  BlueprintRarity.Common, 2 ) },
            { EquipmentCategoryType.Cape,    ( "Cape",    "m_capeEquipmentLibrary",    5,  BlueprintRarity.Common, 1 ) },
            { EquipmentCategoryType.Trinket, ( "Trinket", "m_trinketEquipmentLibrary", 10, BlueprintRarity.Silver, 4 ) },
        };

        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            foreach( EquipmentType setType in SetInfo.Keys ) {
                setKeys.Add( setType, SetInfo[setType].Config );
                WobSettings.Add( new WobSettings.Entry[] {
                    // Unity stat bonuses
                    new WobSettings.Enum<EquipmentSetBonusType>( setKeys.Get( setType, "U20_BonusType"  ), SetInfo[setType].Config + " set 20 unity bonus type",  SetInfo[setType].BonusType1                        ),
                    new WobSettings.Num<float>(                  setKeys.Get( setType, "U20_StatGain"   ), SetInfo[setType].Config + " set 20 unity stat gain",   SetInfo[setType].StatGain1, bounds: (0f, 1000000f) ),
                    new WobSettings.Enum<EquipmentSetBonusType>( setKeys.Get( setType, "U50_BonusType"  ), SetInfo[setType].Config + " set 50 unity bonus type",  SetInfo[setType].BonusType2                        ),
                    new WobSettings.Num<float>(                  setKeys.Get( setType, "U50_StatGain"   ), SetInfo[setType].Config + " set 50 unity stat gain",   SetInfo[setType].StatGain1, bounds: (0f, 1000000f) ),
                    new WobSettings.Enum<EquipmentSetBonusType>( setKeys.Get( setType, "U100_BonusType" ), SetInfo[setType].Config + " set 100 unity bonus type", SetInfo[setType].BonusType3                        ),
                    new WobSettings.Num<float>(                  setKeys.Get( setType, "U100_StatGain"  ), SetInfo[setType].Config + " set 100 unity stat gain",  SetInfo[setType].StatGain1, bounds: (0f, 1000000f) ),
                    // Chest level requirements
                    new WobSettings.Num<int>( setKeys.Get( setType, "BaseLevel"    ), SetInfo[setType].Config + " set base level for blueprint spawn",    SetInfo[setType].BaseLevel,    bounds: (1, 1000) ),
                    new WobSettings.Num<int>( setKeys.Get( setType, "ScalingLevel" ), SetInfo[setType].Config + " set scaling level for blueprint spawn", SetInfo[setType].ScalingLevel, bounds: (1, 1000) ),
                } );
            }
            foreach( EquipmentCategoryType slotType in SlotInfo.Keys ) {
                slotKeys.Add( slotType, SlotInfo[slotType].Config );
                WobSettings.Add( new WobSettings.Entry[] {
                    new WobSettings.Num<int>( slotKeys.Get( slotType, "Unity"  ), SlotInfo[slotType].Config + " base set unity per equipment level", SlotInfo[slotType].Unity, bounds: (0, 100) ),
                    new WobSettings.Enum<BlueprintRarity>( slotKeys.Get( slotType, "Rarity" ), SlotInfo[slotType].Config + " chest rarity for blueprint drops", SlotInfo[slotType].Rarity ),
                    new WobSettings.Num<int>( slotKeys.Get( slotType, "Level" ), SlotInfo[slotType].Config + " chest level added to set base level for blueprint drops", SlotInfo[slotType].Level, bounds: (0, 1000) ),
                } );
            }
            for( int i = 0; i < Economy_EV.EQUIPMENT_LEVEL_GOLD_MOD.Length; i++ ) {
                WobSettings.Add( new WobSettings.Num<int>( "ScalingGoldCost", "GoldCostMult_Level" + ( i + 1 ), "Multiply base gold cost by this value to get the equipment cost for level " + ( i + 1 ), Economy_EV.EQUIPMENT_LEVEL_GOLD_MOD[i], bounds: (1, 1000000) ) );
            }
            for( int i = 0; i < Economy_EV.EQUIPMENT_LEVEL_ORE_MOD.Length; i++ ) {
                WobSettings.Add( new WobSettings.Num<int>( "ScalingOreCost", "OreCostMult_Level" + ( i + 1 ), "Multiply base ore cost by this value to get the equipment cost for level " + ( i + 1 ), Economy_EV.EQUIPMENT_LEVEL_ORE_MOD[i], bounds: (1, 1000000) ) );
            }
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Patch to change scaling gold costs
        [HarmonyPatch( typeof( EquipmentObj ), nameof( EquipmentObj.GoldCostToUpgrade ), MethodType.Getter )]
        internal static class EquipmentObj_GoldCostToUpgrade_Patch {
            private static bool runOnce = false;
            internal static void Prefix() {
                if( !runOnce ) {
                    for( int i = 0; i < Economy_EV.EQUIPMENT_LEVEL_GOLD_MOD.Length; i++ ) {
                        Economy_EV.EQUIPMENT_LEVEL_GOLD_MOD[i] = WobSettings.Get( "ScalingGoldCost", "GoldCostMult_Level" + ( i + 1 ), Economy_EV.EQUIPMENT_LEVEL_GOLD_MOD[i] );
                    }
                    runOnce = true;
                }
            }
        }

        // Patch to change scaling ore costs
        [HarmonyPatch( typeof( EquipmentObj ), nameof( EquipmentObj.OreCostToUpgrade ), MethodType.Getter )]
        internal static class EquipmentObj_OreCostToUpgrade_Patch {
            private static bool runOnce = false;
            internal static void Prefix() {
                if( !runOnce ) {
                    for( int i = 0; i < Economy_EV.EQUIPMENT_LEVEL_ORE_MOD.Length; i++ ) {
                        Economy_EV.EQUIPMENT_LEVEL_ORE_MOD[i] = WobSettings.Get( "ScalingOreCost", "OreCostMult_Level" + ( i + 1 ), Economy_EV.EQUIPMENT_LEVEL_ORE_MOD[i] );
                    }
                    runOnce = true;
                }
            }
        }

        [HarmonyPatch( typeof( EquipmentSetLibrary ), "Instance", MethodType.Getter )]
        internal static class EquipmentSetLibrary_Instance_Patch {
            private static bool runOnce = false;
            internal static void Postfix( EquipmentSetLibrary __result ) {
                if( !runOnce ) {
                    EquipmentTypeEquipmentSetDataDictionary m_equipmentSetLibrary = Traverse.Create( __result ).Field( "m_equipmentSetLibrary" ).GetValue<EquipmentTypeEquipmentSetDataDictionary>();
                    foreach( EquipmentType setType in m_equipmentSetLibrary.Keys ) {
                        if( setKeys.Exists( setType ) ) {
                            EquipmentSetData setData = m_equipmentSetLibrary[setType];
                            setData.SetBonus01 = new EquipmentSetBonus( WobSettings.Get( setKeys.Get( setType, "U20_BonusType" ), setData.SetBonus01.BonusType ), WobSettings.Get( setKeys.Get( setType, "U20_StatGain" ), setData.SetBonus01.StatGain ) );
                            setData.SetBonus02 = new EquipmentSetBonus( WobSettings.Get( setKeys.Get( setType, "U50_BonusType" ), setData.SetBonus02.BonusType ), WobSettings.Get( setKeys.Get( setType, "U50_StatGain" ), setData.SetBonus02.StatGain ) );
                            setData.SetBonus03 = new EquipmentSetBonus( WobSettings.Get( setKeys.Get( setType, "U100_BonusType" ), setData.SetBonus03.BonusType ), WobSettings.Get( setKeys.Get( setType, "U100_StatGain" ), setData.SetBonus03.StatGain ) );
                        }
                    }
                    runOnce = true;
                }
            }
        }

        [HarmonyPatch( typeof( EquipmentLibrary ), "Instance", MethodType.Getter )]
        internal static class EquipmentLibrary_Instance_Patch {
            private static bool runOnce = false;
            internal static void Postfix( EquipmentLibrary __result ) {
                if( !runOnce ) {
                    foreach( EquipmentCategoryType slotType in SlotInfo.Keys ) {
                        EquipmentTypeEquipmentDataDictionary equipmentLibrary  = Traverse.Create( __result ).Field( SlotInfo[slotType].LibraryField ).GetValue<EquipmentTypeEquipmentDataDictionary>();
                        int unity = WobSettings.Get( slotKeys.Get( slotType, "Unity" ), SlotInfo[slotType].Unity );
                        int rarity = (int)WobSettings.Get( slotKeys.Get( slotType, "Rarity" ), SlotInfo[slotType].Rarity );
                        int slotLevel = WobSettings.Get( slotKeys.Get( slotType, "Level" ), SlotInfo[slotType].Level );
                        foreach( EquipmentType equipType in equipmentLibrary.Keys ) {
                            EquipmentData equipData = equipmentLibrary[equipType];
                            equipData.BaseEquipmentSetLevel = unity;
                            equipData.ScalingEquipmentSetLevel = unity;
                            equipData.ChestRarityRequirement = rarity;
                            if( setKeys.Exists( equipType ) ) {
                                equipData.ChestLevelRequirement = slotLevel + WobSettings.Get( setKeys.Get( equipType, "BaseLevel" ), SetInfo[equipType].BaseLevel );
                                equipData.ScalingItemLevel = WobSettings.Get( setKeys.Get( equipType, "ScalingLevel" ), SetInfo[equipType].ScalingLevel );
                            }
                        }
                    }
                    runOnce = true;
                }
            }
        }
    }
}