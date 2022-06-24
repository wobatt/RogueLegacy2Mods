using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_EquipStats {
    [BepInPlugin( "Wob.EquipStats", "Equipment Stats Mod", "1.0.0" )]
    public partial class EquipStats : BaseUnityPlugin {

        private static readonly WobSettings.FileHelper<EquipmentType> setFiles = new WobSettings.FileHelper<EquipmentType>( "Set", 2 );
        
        private static readonly WobSettings.KeyHelper<EquipmentType> setUnityKeys = new WobSettings.KeyHelper<EquipmentType>( "BonusForUnity" );
        private static readonly WobSettings.KeyHelper<EquipmentType> setChestKeys = new WobSettings.KeyHelper<EquipmentType>( "BlueprintDrop" );
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

        private static readonly WobSettings.KeyHelper<EquipmentCategoryType> slotKeys = new WobSettings.KeyHelper<EquipmentCategoryType>( "Slot", 1 );
        private enum BlueprintRarity { Common = 0, Silver = 1 }
        private static readonly Dictionary<EquipmentCategoryType,(string Config, int UIIndex, string LibraryField, int Unity, BlueprintRarity Rarity, int Level)> SlotInfo = new Dictionary<EquipmentCategoryType,(string Config, int UIIndex, string LibraryField, int Unity, BlueprintRarity Rarity, int Level)>() {
            { EquipmentCategoryType.Weapon,  ( "Weapon",  1, "m_weaponEquipmentLibrary",  5,  BlueprintRarity.Common, 0 ) },
            { EquipmentCategoryType.Head,    ( "Head",    2, "m_headEquipmentLibrary",    5,  BlueprintRarity.Common, 0 ) },
            { EquipmentCategoryType.Chest,   ( "Chest",   3, "m_chestEquipmentLibrary",   5,  BlueprintRarity.Common, 2 ) },
            { EquipmentCategoryType.Cape,    ( "Cape",    4, "m_capeEquipmentLibrary",    5,  BlueprintRarity.Common, 1 ) },
            { EquipmentCategoryType.Trinket, ( "Trinket", 5, "m_trinketEquipmentLibrary", 10, BlueprintRarity.Silver, 4 ) },
        };

        private static readonly WobSettings.KeyHelper<(EquipmentType,EquipmentCategoryType)> itemKeys = new WobSettings.KeyHelper<(EquipmentType,EquipmentCategoryType)>();
        private static readonly Dictionary<(EquipmentType Set,EquipmentCategoryType Slot),(string Name, int GoldCost, int OreCost, int WeightB, int WeightS, int VitB, int VitS, int ArmB, int ArmS, int StrB, int StrS, int DexB, int DexS, int IntB, int IntS, int FocB, int FocS)> ItemInfo = new Dictionary<(EquipmentType Set,EquipmentCategoryType Slot),(string Name, int GoldCost, int OreCost, int WeightB, int WeightS, int VitB, int VitS, int ArmB, int ArmS, int StrB, int StrS, int DexB, int DexS, int IntB, int IntS, int FocB, int FocS)>() {
            //                                                                                            Gold  Ore   Weight   Vit     Arm     Str     Dex     Int     Foc
            { ( EquipmentType.GEAR_BONUS_WEIGHT,  EquipmentCategoryType.Weapon  ), ( "Leather Weapon",    300,  200,  10,  5,  0,  0,  0,  0,  2,  1,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_BONUS_WEIGHT,  EquipmentCategoryType.Head    ), ( "Leather Head",      250,  150,  10,  5,  2,  1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_BONUS_WEIGHT,  EquipmentCategoryType.Chest   ), ( "Leather Chest",     350,  250,  25,  10, 0,  0,  7,  5,  0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_BONUS_WEIGHT,  EquipmentCategoryType.Cape    ), ( "Leather Cape",      275,  200,  10,  5,  0,  0,  0,  0,  0,  0,  0,  0,  2,  1,  0,  0  ) },
            { ( EquipmentType.GEAR_BONUS_WEIGHT,  EquipmentCategoryType.Trinket ), ( "Leather Trinket",   425,  300,  20,  5,  0,  0,  3,  2,  0,  0,  2,  1,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_MAGIC_CRIT,    EquipmentCategoryType.Weapon  ), ( "Scholar Weapon",    675,  500,  20,  5,  0,  0,  0,  0,  4,  2,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_MAGIC_CRIT,    EquipmentCategoryType.Head    ), ( "Scholar Head",      550,  400,  15,  5,  3,  2,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_MAGIC_CRIT,    EquipmentCategoryType.Chest   ), ( "Scholar Chest",     800,  600,  35,  10, 0,  0,  9,  5,  0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_MAGIC_CRIT,    EquipmentCategoryType.Cape    ), ( "Scholar Cape",      600,  450,  25,  10, 0,  0,  0,  0,  0,  0,  0,  0,  5,  3,  0,  0  ) },
            { ( EquipmentType.GEAR_MAGIC_CRIT,    EquipmentCategoryType.Trinket ), ( "Scholar Trinket",   950,  700,  25,  10, 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  3,  2  ) },
            { ( EquipmentType.GEAR_STRENGTH_CRIT, EquipmentCategoryType.Weapon  ), ( "Warden Weapon",     1100, 800,  35,  10, 0,  0,  0,  0,  7,  4,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_STRENGTH_CRIT, EquipmentCategoryType.Head    ), ( "Warden Head",       875,  650,  25,  10, 5,  3,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_STRENGTH_CRIT, EquipmentCategoryType.Chest   ), ( "Warden Chest",      1325, 950,  50,  15, 0,  0,  13, 10, 0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_STRENGTH_CRIT, EquipmentCategoryType.Cape    ), ( "Warden Cape",       1000, 700,  20,  5,  0,  0,  0,  0,  0,  0,  0,  0,  4,  2,  0,  0  ) },
            { ( EquipmentType.GEAR_STRENGTH_CRIT, EquipmentCategoryType.Trinket ), ( "Warden Trinket",    1550, 1100, 30,  10, 0,  0,  0,  0,  0,  0,  2,  1,  0,  0,  2,  1  ) },
            { ( EquipmentType.GEAR_LIFE_STEAL,    EquipmentCategoryType.Weapon  ), ( "Sanguine Weapon",   1600, 1200, 40,  10, 0,  0,  0,  0,  3,  2,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_LIFE_STEAL,    EquipmentCategoryType.Head    ), ( "Sanguine Head",     1275, 950,  35,  10, 3,  2,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_LIFE_STEAL,    EquipmentCategoryType.Chest   ), ( "Sanguine Chest",    1925, 1450, 40,  10, 0,  0,  5,  5,  0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_LIFE_STEAL,    EquipmentCategoryType.Cape    ), ( "Sanguine Cape",     1450, 1100, 35,  10, 0,  0,  0,  0,  0,  0,  0,  0,  3,  2,  0,  0  ) },
            { ( EquipmentType.GEAR_LIFE_STEAL,    EquipmentCategoryType.Trinket ), ( "Sanguine Trinket",  2250, 1700, 50,  15, 0,  0,  0,  0,  3,  2,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_ARMOR,         EquipmentCategoryType.Weapon  ), ( "Ammonite Weapon",   2150, 1600, 50,  15, 0,  0,  7,  4,  5,  3,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_ARMOR,         EquipmentCategoryType.Head    ), ( "Ammonite Head",     1725, 1300, 45,  15, 5,  3,  6,  3,  0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_ARMOR,         EquipmentCategoryType.Chest   ), ( "Ammonite Chest",    2575, 1900, 60,  15, 0,  0,  15, 10, 0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_ARMOR,         EquipmentCategoryType.Cape    ), ( "Ammonite Cape",     1925, 1450, 45,  15, 0,  0,  6,  3,  0,  0,  0,  0,  5,  3,  0,  0  ) },
            { ( EquipmentType.GEAR_ARMOR,         EquipmentCategoryType.Trinket ), ( "Ammonite Trinket",  3000, 2250, 50,  15, 0,  0,  7,  4,  0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_MAGIC_DMG,     EquipmentCategoryType.Weapon  ), ( "Crescent Weapon",   2750, 2000, 55,  15, 0,  0,  0,  0,  11, 6,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_MAGIC_DMG,     EquipmentCategoryType.Head    ), ( "Crescent Head",     2200, 1600, 55,  15, 11, 6,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_MAGIC_DMG,     EquipmentCategoryType.Chest   ), ( "Crescent Chest",    3300, 2400, 70,  20, 0,  0,  9,  5,  0,  0,  0,  0,  7,  5,  0,  0  ) },
            { ( EquipmentType.GEAR_MAGIC_DMG,     EquipmentCategoryType.Cape    ), ( "Crescent Cape",     2475, 1800, 50,  15, 0,  0,  0,  0,  0,  0,  0,  0,  10, 5,  0,  0  ) },
            { ( EquipmentType.GEAR_MAGIC_DMG,     EquipmentCategoryType.Trinket ), ( "Crescent Trinket",  3850, 2800, 70,  20, 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  7,  4  ) },
            { ( EquipmentType.GEAR_MOBILITY,      EquipmentCategoryType.Weapon  ), ( "Drowned Weapon",    3425, 2500, 80,  20, 0,  0,  0,  0,  16, 8,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_MOBILITY,      EquipmentCategoryType.Head    ), ( "Drowned Head",      2750, 2000, 60,  15, 12, 6,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_MOBILITY,      EquipmentCategoryType.Chest   ), ( "Drowned Chest",     4100, 3000, 80,  20, 0,  0,  20, 10, 0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_MOBILITY,      EquipmentCategoryType.Cape    ), ( "Drowned Cape",      3075, 2250, 65,  20, 0,  0,  0,  0,  0,  0,  0,  0,  13, 7,  0,  0  ) },
            { ( EquipmentType.GEAR_MOBILITY,      EquipmentCategoryType.Trinket ), ( "Drowned Trinket",   4800, 3500, 75,  20, 0,  0,  0,  0,  0,  0,  4,  2,  0,  0,  4,  2  ) },
            { ( EquipmentType.GEAR_GOLD,          EquipmentCategoryType.Weapon  ), ( "Gilded Weapon",     4150, 3100, 80,  20, 0,  0,  0,  0,  6,  3,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_GOLD,          EquipmentCategoryType.Head    ), ( "Gilded Head",       3325, 2500, 80,  20, 6,  3,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_GOLD,          EquipmentCategoryType.Chest   ), ( "Gilded Chest",      4975, 3700, 80,  20, 0,  0,  7,  5,  0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_GOLD,          EquipmentCategoryType.Cape    ), ( "Gilded Cape",       3725, 2800, 80,  20, 0,  0,  0,  0,  0,  0,  0,  0,  6,  3,  0,  0  ) },
            { ( EquipmentType.GEAR_GOLD,          EquipmentCategoryType.Trinket ), ( "Gilded Trinket",    5800, 4350, 80,  20, 4,  2,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_RETURN_DMG,    EquipmentCategoryType.Weapon  ), ( "Obsidian Weapon",   4950, 3700, 90,  25, 9,  5,  0,  0,  9,  5,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_RETURN_DMG,    EquipmentCategoryType.Head    ), ( "Obsidian Head",     3950, 2950, 90,  25, 18, 9,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_RETURN_DMG,    EquipmentCategoryType.Chest   ), ( "Obsidian Chest",    5950, 4450, 100, 25, 10, 5,  13, 10, 0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_RETURN_DMG,    EquipmentCategoryType.Cape    ), ( "Obsidian Cape",     4450, 3350, 90,  25, 9,  5,  0,  0,  0,  0,  0,  0,  9,  5,  0,  0  ) },
            { ( EquipmentType.GEAR_RETURN_DMG,    EquipmentCategoryType.Trinket ), ( "Obsidian Trinket",  6925, 5200, 90,  25, 5,  3,  6,  3,  0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_MAG_ON_HIT,    EquipmentCategoryType.Weapon  ), ( "Leviathan Weapon",  5800, 4300, 100, 25, 0,  0,  0,  0,  10, 5,  0,  0,  10, 5,  0,  0  ) },
            { ( EquipmentType.GEAR_MAG_ON_HIT,    EquipmentCategoryType.Head    ), ( "Leviathan Head",    4650, 3450, 95,  25, 10, 5,  0,  0,  0,  0,  0,  0,  10, 5,  0,  0  ) },
            { ( EquipmentType.GEAR_MAG_ON_HIT,    EquipmentCategoryType.Chest   ), ( "Leviathan Chest",   6950, 5150, 100, 25, 0,  0,  25, 15, 0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_MAG_ON_HIT,    EquipmentCategoryType.Cape    ), ( "Leviathan Cape",    5225, 3850, 120, 30, 0,  0,  0,  0,  0,  0,  0,  0,  24, 12, 0,  0  ) },
            { ( EquipmentType.GEAR_MAG_ON_HIT,    EquipmentCategoryType.Trinket ), ( "Leviathan Trinket", 8125, 6000, 100, 25, 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  10, 5  ) },
            { ( EquipmentType.GEAR_LIFE_STEAL_2,  EquipmentCategoryType.Weapon  ), ( "Kin Weapon",        6700, 5000, 120, 30, 0,  0,  0,  0,  24, 12, 0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_LIFE_STEAL_2,  EquipmentCategoryType.Head    ), ( "Kin Head",          5350, 4000, 130, 35, 26, 13, 0,  0,  0,  0,  0,  0,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_LIFE_STEAL_2,  EquipmentCategoryType.Chest   ), ( "Kin Chest",         8050, 6000, 125, 35, 0,  0,  16, 10, 0,  0,  0,  0,  0,  0,  13, 10 ) },
            { ( EquipmentType.GEAR_LIFE_STEAL_2,  EquipmentCategoryType.Cape    ), ( "Kin Cape",          6025, 4500, 135, 35, 14, 7,  0,  0,  0,  0,  14, 7,  0,  0,  0,  0  ) },
            { ( EquipmentType.GEAR_LIFE_STEAL_2,  EquipmentCategoryType.Trinket ), ( "Kin Trinket",       9375, 7000, 125, 35, 0,  0,  0,  0,  0,  0,  7,  4,  0,  0,  7,  4  ) },
        };
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            foreach( EquipmentType setType in SetInfo.Keys ) {
                setFiles.Add( setType, SetInfo[setType].Config );
                setUnityKeys.Add( setType, SetInfo[setType].Config );
                setChestKeys.Add( setType, SetInfo[setType].Config );
                WobSettings.Add( new WobSettings.Entry[] {
                    // Unity stat bonuses
                    new WobSettings.Enum<EquipmentSetBonusType>( setFiles.Get( setType ), setUnityKeys.Get( setType, "U20_BonusType"  ), SetInfo[setType].Config + " set - 20 unity bonus type",  SetInfo[setType].BonusType1                        ),
                    new WobSettings.Num<float>(                  setFiles.Get( setType ), setUnityKeys.Get( setType, "U20_StatGain"   ), SetInfo[setType].Config + " set - 20 unity stat gain",   SetInfo[setType].StatGain1, bounds: (0f, 1000000f) ),
                    new WobSettings.Enum<EquipmentSetBonusType>( setFiles.Get( setType ), setUnityKeys.Get( setType, "U50_BonusType"  ), SetInfo[setType].Config + " set - 50 unity bonus type",  SetInfo[setType].BonusType2                        ),
                    new WobSettings.Num<float>(                  setFiles.Get( setType ), setUnityKeys.Get( setType, "U50_StatGain"   ), SetInfo[setType].Config + " set - 50 unity stat gain",   SetInfo[setType].StatGain2, bounds: (0f, 1000000f) ),
                    new WobSettings.Enum<EquipmentSetBonusType>( setFiles.Get( setType ), setUnityKeys.Get( setType, "U100_BonusType" ), SetInfo[setType].Config + " set - 100 unity bonus type", SetInfo[setType].BonusType3                        ),
                    new WobSettings.Num<float>(                  setFiles.Get( setType ), setUnityKeys.Get( setType, "U100_StatGain"  ), SetInfo[setType].Config + " set - 100 unity stat gain",  SetInfo[setType].StatGain3, bounds: (0f, 1000000f) ),
                    // Chest level requirements
                    new WobSettings.Num<int>( setFiles.Get( setType ), setChestKeys.Get( setType, "BaseLevel"    ), SetInfo[setType].Config + " set - base level for blueprint spawn",    SetInfo[setType].BaseLevel,    bounds: (1, 1000) ),
                    new WobSettings.Num<int>( setFiles.Get( setType ), setChestKeys.Get( setType, "ScalingLevel" ), SetInfo[setType].Config + " set - scaling level for blueprint spawn", SetInfo[setType].ScalingLevel, bounds: (1, 1000) ),
                } );
            }
            foreach( EquipmentCategoryType slotType in SlotInfo.Keys ) {
                slotKeys.Add( slotType, SlotInfo[slotType].Config );
                WobSettings.Add( new WobSettings.Entry[] {
                    new WobSettings.Num<int>( slotKeys.Get( slotType, "Unity" ), SlotInfo[slotType].Config + " base set unity per equipment level", SlotInfo[slotType].Unity, bounds: (0, 100) ),
                    new WobSettings.Enum<BlueprintRarity>( slotKeys.Get( slotType, "Rarity" ), SlotInfo[slotType].Config + " chest rarity for blueprint drops", SlotInfo[slotType].Rarity ),
                    new WobSettings.Num<int>( slotKeys.Get( slotType, "Level" ), SlotInfo[slotType].Config + " chest level added to set base level for blueprint drops", SlotInfo[slotType].Level, bounds: (0, 1000) ),
                } );
            }
            foreach( (EquipmentType Set, EquipmentCategoryType Slot) itemType in ItemInfo.Keys ) {
                itemKeys.Add( itemType, "Item" + SlotInfo[itemType.Slot].UIIndex, SetInfo[itemType.Set].Config + "_" + SlotInfo[itemType.Slot].Config );
                WobSettings.Add( new WobSettings.Entry[] {
                    new WobSettings.Num<int>( setFiles.Get( itemType.Set ), itemKeys.Get( itemType, "GoldCost"       ), ItemInfo[itemType].Name + " - base gold cost",                          ItemInfo[itemType].GoldCost, bounds: (1, 1000000) ),
                    new WobSettings.Num<int>( setFiles.Get( itemType.Set ), itemKeys.Get( itemType, "OreCost"        ), ItemInfo[itemType].Name + " - base ore cost",                           ItemInfo[itemType].OreCost,  bounds: (1, 1000000) ),
                    new WobSettings.Num<int>( setFiles.Get( itemType.Set ), itemKeys.Get( itemType, "Weight_Base"    ), ItemInfo[itemType].Name + " - base weight",                             ItemInfo[itemType].WeightB,  bounds: (1, 1000000) ),
                    new WobSettings.Num<int>( setFiles.Get( itemType.Set ), itemKeys.Get( itemType, "Weight_Scaling" ), ItemInfo[itemType].Name + " - additional weight per level",             ItemInfo[itemType].WeightS,  bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( setFiles.Get( itemType.Set ), itemKeys.Get( itemType, "Vit_Base"       ), ItemInfo[itemType].Name + " - base health bonus",                       ItemInfo[itemType].VitB,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( setFiles.Get( itemType.Set ), itemKeys.Get( itemType, "Vit_Scaling"    ), ItemInfo[itemType].Name + " - additional health bonus per level",       ItemInfo[itemType].VitS,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( setFiles.Get( itemType.Set ), itemKeys.Get( itemType, "Arm_Base"       ), ItemInfo[itemType].Name + " - base armor bonus",                        ItemInfo[itemType].ArmB,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( setFiles.Get( itemType.Set ), itemKeys.Get( itemType, "Arm_Scaling"    ), ItemInfo[itemType].Name + " - additional armor bonus per level",        ItemInfo[itemType].ArmS,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( setFiles.Get( itemType.Set ), itemKeys.Get( itemType, "Str_Base"       ), ItemInfo[itemType].Name + " - base strength bonus",                     ItemInfo[itemType].StrB,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( setFiles.Get( itemType.Set ), itemKeys.Get( itemType, "Str_Scaling"    ), ItemInfo[itemType].Name + " - additional strength bonus per level",     ItemInfo[itemType].StrS,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( setFiles.Get( itemType.Set ), itemKeys.Get( itemType, "Dex_Base"       ), ItemInfo[itemType].Name + " - base dexterity bonus",                    ItemInfo[itemType].DexB,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( setFiles.Get( itemType.Set ), itemKeys.Get( itemType, "Dex_Scaling"    ), ItemInfo[itemType].Name + " - additional dexterity bonus per level",    ItemInfo[itemType].DexS,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( setFiles.Get( itemType.Set ), itemKeys.Get( itemType, "Int_Base"       ), ItemInfo[itemType].Name + " - base intelligence bonus",                 ItemInfo[itemType].IntB,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( setFiles.Get( itemType.Set ), itemKeys.Get( itemType, "Int_Scaling"    ), ItemInfo[itemType].Name + " - additional intelligence bonus per level", ItemInfo[itemType].IntS,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( setFiles.Get( itemType.Set ), itemKeys.Get( itemType, "Foc_Base"       ), ItemInfo[itemType].Name + " - base focus bonus",                        ItemInfo[itemType].FocB,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( setFiles.Get( itemType.Set ), itemKeys.Get( itemType, "Foc_Scaling"    ), ItemInfo[itemType].Name + " - additional focus bonus per level",        ItemInfo[itemType].FocS,     bounds: (0, 1000000) ),
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
                        if( setUnityKeys.Exists( setType ) ) {
                            EquipmentSetData setData = m_equipmentSetLibrary[setType];
                            setData.SetBonus01 = new EquipmentSetBonus( WobSettings.Get( setUnityKeys.Get( setType, "U20_BonusType"  ), setData.SetBonus01.BonusType ), WobSettings.Get( setUnityKeys.Get( setType, "U20_StatGain"  ), setData.SetBonus01.StatGain ) );
                            setData.SetBonus02 = new EquipmentSetBonus( WobSettings.Get( setUnityKeys.Get( setType, "U50_BonusType"  ), setData.SetBonus02.BonusType ), WobSettings.Get( setUnityKeys.Get( setType, "U50_StatGain"  ), setData.SetBonus02.StatGain ) );
                            setData.SetBonus03 = new EquipmentSetBonus( WobSettings.Get( setUnityKeys.Get( setType, "U100_BonusType" ), setData.SetBonus03.BonusType ), WobSettings.Get( setUnityKeys.Get( setType, "U100_StatGain" ), setData.SetBonus03.StatGain ) );
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
                            if( setChestKeys.Exists( equipType ) ) {
                                equipData.ChestLevelRequirement = slotLevel + WobSettings.Get( setChestKeys.Get( equipType, "BaseLevel" ), SetInfo[equipType].BaseLevel );
                                equipData.ScalingItemLevel = WobSettings.Get( setChestKeys.Get( equipType, "ScalingLevel" ), SetInfo[equipType].ScalingLevel );
                            }
                            if( itemKeys.Exists( (equipType, slotType) ) ) {
                                equipData.GoldCost                  = WobSettings.Get( itemKeys.Get( (equipType, slotType), "GoldCost"       ), equipData.GoldCost                  );
                                equipData.OreCost                   = WobSettings.Get( itemKeys.Get( (equipType, slotType), "OreCost"        ), equipData.OreCost                   );
                                equipData.BaseWeight                = WobSettings.Get( itemKeys.Get( (equipType, slotType), "Weight_Base"    ), equipData.BaseWeight                );
                                equipData.ScalingWeight             = WobSettings.Get( itemKeys.Get( (equipType, slotType), "Weight_Scaling" ), equipData.ScalingWeight             );
                                equipData.BaseHealth                = WobSettings.Get( itemKeys.Get( (equipType, slotType), "Vit_Base"       ), equipData.BaseHealth                );
                                equipData.ScalingHealth             = WobSettings.Get( itemKeys.Get( (equipType, slotType), "Vit_Scaling"    ), equipData.ScalingHealth             );
                                equipData.BaseArmor                 = WobSettings.Get( itemKeys.Get( (equipType, slotType), "Arm_Base"       ), equipData.BaseArmor                 );
                                equipData.ScalingArmor              = WobSettings.Get( itemKeys.Get( (equipType, slotType), "Arm_Scaling"    ), equipData.ScalingArmor              );
                                equipData.BaseStrengthDamage        = WobSettings.Get( itemKeys.Get( (equipType, slotType), "Str_Base"       ), equipData.BaseStrengthDamage        );
                                equipData.ScalingStrengthDamage     = WobSettings.Get( itemKeys.Get( (equipType, slotType), "Str_Scaling"    ), equipData.ScalingStrengthDamage     );
                                equipData.BaseStrengthCritChance    = WobSettings.Get( itemKeys.Get( (equipType, slotType), "Dex_Base"       ), equipData.BaseStrengthCritChance    );
                                equipData.ScalingStrengthCritChance = WobSettings.Get( itemKeys.Get( (equipType, slotType), "Dex_Scaling"    ), equipData.ScalingStrengthCritChance );
                                equipData.BaseMagicDamage           = WobSettings.Get( itemKeys.Get( (equipType, slotType), "Int_Base"       ), equipData.BaseMagicDamage           );
                                equipData.ScalingMagicDamage        = WobSettings.Get( itemKeys.Get( (equipType, slotType), "Int_Scaling"    ), equipData.ScalingMagicDamage        );
                                equipData.BaseMagicCritChance       = WobSettings.Get( itemKeys.Get( (equipType, slotType), "Foc_Base"       ), equipData.BaseMagicCritChance       );
                                equipData.ScalingMagicCritChance    = WobSettings.Get( itemKeys.Get( (equipType, slotType), "Foc_Scaling"    ), equipData.ScalingMagicCritChance    );
                            }
                        }
                    }
                    runOnce = true;
                }
            }
        }
    }
}