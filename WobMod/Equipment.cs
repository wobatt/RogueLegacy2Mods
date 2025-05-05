using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace WobMod {
    internal class Equipment {

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTING DEFAULTS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        private record EquipSetInfo( string Config, EquipmentSetBonusType BonusType1, float StatGain1, EquipmentSetBonusType BonusType2, float StatGain2, EquipmentSetBonusType BonusType3, float StatGain3, int BaseLevel, int ScalingLevel );
		private static readonly Dictionary<EquipmentType, EquipSetInfo> equipSetInfo = new() {
            { EquipmentType.GEAR_BONUS_WEIGHT,  new( "Leather",   EquipmentSetBonusType.Resolve,              0.2f,  EquipmentSetBonusType.Resolve,         0.3f,  EquipmentSetBonusType.MinimumResolve,       0.3f,  1,  12 ) },
            { EquipmentType.GEAR_MAGIC_CRIT,    new( "Scholar",   EquipmentSetBonusType.Focus_Add,            10f,   EquipmentSetBonusType.Focus_Add,       20f,   EquipmentSetBonusType.FlatMagicCritChance,  0.2f,  5,  16 ) },
            { EquipmentType.GEAR_STRENGTH_CRIT, new( "Warden",    EquipmentSetBonusType.Dexterity_Add,        10f,   EquipmentSetBonusType.Dexterity_Add,   20f,   EquipmentSetBonusType.FlatWeaponCritChance, 0.2f,  12, 20 ) },
            { EquipmentType.GEAR_LIFE_STEAL,    new( "Sanguine",  EquipmentSetBonusType.LifeSteal,            2f,    EquipmentSetBonusType.LifeSteal,       3f,    EquipmentSetBonusType.ReturnDamage,         5f,    16, 24 ) },
            { EquipmentType.GEAR_ARMOR,         new( "Ammonite",  EquipmentSetBonusType.Armor,                30f,   EquipmentSetBonusType.Armor,           60f,   EquipmentSetBonusType.ArmorMod,             0.4f,  19, 27 ) },
            { EquipmentType.GEAR_MAGIC_DMG,     new( "Crescent",  EquipmentSetBonusType.MagicAdd,             15f,   EquipmentSetBonusType.MagicAdd,        30f,   EquipmentSetBonusType.MagicMod,             0.2f,  25, 31 ) },
            { EquipmentType.GEAR_MOBILITY,      new( "Drowned",   EquipmentSetBonusType.StrengthAdd,          15f,   EquipmentSetBonusType.StrengthAdd,     30f,   EquipmentSetBonusType.StrengthMod,          0.2f,  32, 34 ) },
            { EquipmentType.GEAR_GOLD,          new( "Gilded",    EquipmentSetBonusType.OreAetherGain,        0.1f,  EquipmentSetBonusType.OreAetherGain,   0.2f,  EquipmentSetBonusType.GoldGain,             0.5f,  40, 38 ) },
            { EquipmentType.GEAR_RETURN_DMG,    new( "Obsidian",  EquipmentSetBonusType.VitalityAdd,          15f,   EquipmentSetBonusType.VitalityAdd,     30f,   EquipmentSetBonusType.VitalityMod,          0.2f,  48, 41 ) },
            { EquipmentType.GEAR_MAG_ON_HIT,    new( "Leviathan", EquipmentSetBonusType.ManaRegenMod,         0.1f,  EquipmentSetBonusType.ManaRegenMod,    0.15f, EquipmentSetBonusType.SoulSteal,            5f,    54, 48 ) },
            { EquipmentType.GEAR_LIFE_STEAL_2,  new( "Kin",       EquipmentSetBonusType.ReturnDamage,         2f,    EquipmentSetBonusType.MaxMana,         50f,   EquipmentSetBonusType.Revives,              1f,    60, 48 ) },
            { EquipmentType.GEAR_REVIVE,        new( "WhiteWood", EquipmentSetBonusType.FlatMagicCritChance,  0.05f, EquipmentSetBonusType.MagicCritDamage, 0.2f,  EquipmentSetBonusType.DexterityMod,         0.2f,  68, 51 ) },
            { EquipmentType.GEAR_FINAL_BOSS,    new( "BlackRoot", EquipmentSetBonusType.FlatWeaponCritChance, 0.05f, EquipmentSetBonusType.CritDamage,      0.2f,  EquipmentSetBonusType.FocusMod,             0.2f,  68, 51 ) },
        };

        private enum BlueprintRarity { Bronze = 0, Silver = 1 }
        private record EquipSlotInfo( string Config, int UIIndex, string LibraryField, int Unity, BlueprintRarity Rarity, int Level );
		private static readonly Dictionary<EquipmentCategoryType, EquipSlotInfo> equipSlotInfo = new() {
            { EquipmentCategoryType.Weapon,  new( "Weapon",  1, "m_weaponEquipmentLibrary",  5,  BlueprintRarity.Bronze, 0 ) },
            { EquipmentCategoryType.Head,    new( "Head",    2, "m_headEquipmentLibrary",    5,  BlueprintRarity.Bronze, 0 ) },
            { EquipmentCategoryType.Chest,   new( "Chest",   3, "m_chestEquipmentLibrary",   5,  BlueprintRarity.Bronze, 2 ) },
            { EquipmentCategoryType.Cape,    new( "Cape",    4, "m_capeEquipmentLibrary",    5,  BlueprintRarity.Bronze, 1 ) },
            { EquipmentCategoryType.Trinket, new( "Trinket", 5, "m_trinketEquipmentLibrary", 10, BlueprintRarity.Silver, 4 ) },
        };

		private record EquipItemInfo( string Name, int GoldCost, int OreCost, int WeightB, int WeightS, int VitB, int VitS, int ArmB, int ArmS, int StrB, int StrS, int DexB, int DexS, int IntB, int IntS, int FocB, int FocS );
		private static readonly Dictionary<EquipItemKeys, EquipItemInfo> equipItemInfo = new() {
            //                                                                                                  Gold   Ore   Weight    Vit       Arm       Str       Dex       Int       Foc
            { new( EquipmentType.GEAR_BONUS_WEIGHT,  EquipmentCategoryType.Weapon  ), new( "Leather Weapon",    300,   200,  10,  5,   0,   0,   0,   0,   2,   1,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_BONUS_WEIGHT,  EquipmentCategoryType.Head    ), new( "Leather Head",      250,   150,  10,  5,   2,   1,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_BONUS_WEIGHT,  EquipmentCategoryType.Chest   ), new( "Leather Chest",     350,   250,  25,  10,  0,   0,   7,   5,   0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_BONUS_WEIGHT,  EquipmentCategoryType.Cape    ), new( "Leather Cape",      275,   200,  10,  5,   0,   0,   0,   0,   0,   0,   0,   0,   2,   1,   0,   0   ) },
            { new( EquipmentType.GEAR_BONUS_WEIGHT,  EquipmentCategoryType.Trinket ), new( "Leather Trinket",   425,   300,  20,  5,   0,   0,   3,   2,   0,   0,   2,   1,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_MAGIC_CRIT,    EquipmentCategoryType.Weapon  ), new( "Scholar Weapon",    675,   500,  20,  5,   0,   0,   0,   0,   4,   2,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_MAGIC_CRIT,    EquipmentCategoryType.Head    ), new( "Scholar Head",      550,   400,  15,  5,   3,   2,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_MAGIC_CRIT,    EquipmentCategoryType.Chest   ), new( "Scholar Chest",     800,   600,  35,  10,  0,   0,   9,   5,   0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_MAGIC_CRIT,    EquipmentCategoryType.Cape    ), new( "Scholar Cape",      600,   450,  25,  10,  0,   0,   0,   0,   0,   0,   0,   0,   5,   3,   0,   0   ) },
            { new( EquipmentType.GEAR_MAGIC_CRIT,    EquipmentCategoryType.Trinket ), new( "Scholar Trinket",   950,   700,  25,  10,  0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   3,   2   ) },
            { new( EquipmentType.GEAR_STRENGTH_CRIT, EquipmentCategoryType.Weapon  ), new( "Warden Weapon",     1100,  800,  35,  10,  0,   0,   0,   0,   7,   4,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_STRENGTH_CRIT, EquipmentCategoryType.Head    ), new( "Warden Head",       875,   650,  25,  10,  5,   3,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_STRENGTH_CRIT, EquipmentCategoryType.Chest   ), new( "Warden Chest",      1325,  950,  50,  15,  0,   0,   13,  10,  0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_STRENGTH_CRIT, EquipmentCategoryType.Cape    ), new( "Warden Cape",       1000,  700,  20,  5,   0,   0,   0,   0,   0,   0,   0,   0,   4,   2,   0,   0   ) },
            { new( EquipmentType.GEAR_STRENGTH_CRIT, EquipmentCategoryType.Trinket ), new( "Warden Trinket",    1550,  1100, 30,  10,  0,   0,   0,   0,   0,   0,   2,   1,   0,   0,   2,   1   ) },
            { new( EquipmentType.GEAR_LIFE_STEAL,    EquipmentCategoryType.Weapon  ), new( "Sanguine Weapon",   1600,  1200, 40,  10,  0,   0,   0,   0,   3,   2,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_LIFE_STEAL,    EquipmentCategoryType.Head    ), new( "Sanguine Head",     1275,  950,  35,  10,  3,   2,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_LIFE_STEAL,    EquipmentCategoryType.Chest   ), new( "Sanguine Chest",    1925,  1450, 40,  10,  0,   0,   5,   5,   0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_LIFE_STEAL,    EquipmentCategoryType.Cape    ), new( "Sanguine Cape",     1450,  1100, 35,  10,  0,   0,   0,   0,   0,   0,   0,   0,   3,   2,   0,   0   ) },
            { new( EquipmentType.GEAR_LIFE_STEAL,    EquipmentCategoryType.Trinket ), new( "Sanguine Trinket",  2250,  1700, 50,  15,  0,   0,   0,   0,   3,   2,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_ARMOR,         EquipmentCategoryType.Weapon  ), new( "Ammonite Weapon",   2150,  1600, 50,  15,  0,   0,   7,   4,   5,   3,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_ARMOR,         EquipmentCategoryType.Head    ), new( "Ammonite Head",     1725,  1300, 45,  15,  5,   3,   6,   3,   0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_ARMOR,         EquipmentCategoryType.Chest   ), new( "Ammonite Chest",    2575,  1900, 60,  15,  0,   0,   15,  10,  0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_ARMOR,         EquipmentCategoryType.Cape    ), new( "Ammonite Cape",     1925,  1450, 45,  15,  0,   0,   6,   3,   0,   0,   0,   0,   5,   3,   0,   0   ) },
            { new( EquipmentType.GEAR_ARMOR,         EquipmentCategoryType.Trinket ), new( "Ammonite Trinket",  3000,  2250, 50,  15,  0,   0,   7,   4,   0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_MAGIC_DMG,     EquipmentCategoryType.Weapon  ), new( "Crescent Weapon",   2750,  2000, 55,  15,  0,   0,   0,   0,   11,  6,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_MAGIC_DMG,     EquipmentCategoryType.Head    ), new( "Crescent Head",     2200,  1600, 55,  15,  11,  6,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_MAGIC_DMG,     EquipmentCategoryType.Chest   ), new( "Crescent Chest",    3300,  2400, 70,  20,  0,   0,   9,   5,   0,   0,   0,   0,   7,   5,   0,   0   ) },
            { new( EquipmentType.GEAR_MAGIC_DMG,     EquipmentCategoryType.Cape    ), new( "Crescent Cape",     2475,  1800, 50,  15,  0,   0,   0,   0,   0,   0,   0,   0,   10,  5,   0,   0   ) },
            { new( EquipmentType.GEAR_MAGIC_DMG,     EquipmentCategoryType.Trinket ), new( "Crescent Trinket",  3850,  2800, 70,  20,  0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   7,   4   ) },
            { new( EquipmentType.GEAR_MOBILITY,      EquipmentCategoryType.Weapon  ), new( "Drowned Weapon",    3425,  2500, 80,  20,  0,   0,   0,   0,   16,  8,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_MOBILITY,      EquipmentCategoryType.Head    ), new( "Drowned Head",      2750,  2000, 60,  15,  12,  6,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_MOBILITY,      EquipmentCategoryType.Chest   ), new( "Drowned Chest",     4100,  3000, 80,  20,  0,   0,   20,  10,  0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_MOBILITY,      EquipmentCategoryType.Cape    ), new( "Drowned Cape",      3075,  2250, 65,  20,  0,   0,   0,   0,   0,   0,   0,   0,   13,  7,   0,   0   ) },
            { new( EquipmentType.GEAR_MOBILITY,      EquipmentCategoryType.Trinket ), new( "Drowned Trinket",   4800,  3500, 75,  20,  0,   0,   0,   0,   0,   0,   4,   2,   0,   0,   4,   2   ) },
            { new( EquipmentType.GEAR_GOLD,          EquipmentCategoryType.Weapon  ), new( "Gilded Weapon",     4150,  3100, 80,  20,  0,   0,   0,   0,   6,   3,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_GOLD,          EquipmentCategoryType.Head    ), new( "Gilded Head",       3325,  2500, 80,  20,  6,   3,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_GOLD,          EquipmentCategoryType.Chest   ), new( "Gilded Chest",      4975,  3700, 80,  20,  0,   0,   7,   5,   0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_GOLD,          EquipmentCategoryType.Cape    ), new( "Gilded Cape",       3725,  2800, 80,  20,  0,   0,   0,   0,   0,   0,   0,   0,   6,   3,   0,   0   ) },
            { new( EquipmentType.GEAR_GOLD,          EquipmentCategoryType.Trinket ), new( "Gilded Trinket",    5800,  4350, 80,  20,  4,   2,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_RETURN_DMG,    EquipmentCategoryType.Weapon  ), new( "Obsidian Weapon",   4950,  3700, 90,  25,  9,   5,   0,   0,   9,   5,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_RETURN_DMG,    EquipmentCategoryType.Head    ), new( "Obsidian Head",     3950,  2950, 90,  25,  18,  9,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_RETURN_DMG,    EquipmentCategoryType.Chest   ), new( "Obsidian Chest",    5950,  4450, 100, 25,  10,  5,   13,  10,  0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_RETURN_DMG,    EquipmentCategoryType.Cape    ), new( "Obsidian Cape",     4450,  3350, 90,  25,  9,   5,   0,   0,   0,   0,   0,   0,   9,   5,   0,   0   ) },
            { new( EquipmentType.GEAR_RETURN_DMG,    EquipmentCategoryType.Trinket ), new( "Obsidian Trinket",  6925,  5200, 90,  25,  5,   3,   6,   3,   0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_MAG_ON_HIT,    EquipmentCategoryType.Weapon  ), new( "Leviathan Weapon",  5800,  4300, 100, 25,  0,   0,   0,   0,   10,  5,   0,   0,   10,  5,   0,   0   ) },
            { new( EquipmentType.GEAR_MAG_ON_HIT,    EquipmentCategoryType.Head    ), new( "Leviathan Head",    4650,  3450, 95,  25,  10,  5,   0,   0,   0,   0,   0,   0,   10,  5,   0,   0   ) },
            { new( EquipmentType.GEAR_MAG_ON_HIT,    EquipmentCategoryType.Chest   ), new( "Leviathan Chest",   6950,  5150, 100, 25,  0,   0,   25,  15,  0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_MAG_ON_HIT,    EquipmentCategoryType.Cape    ), new( "Leviathan Cape",    5225,  3850, 120, 30,  0,   0,   0,   0,   0,   0,   0,   0,   24,  12,  0,   0   ) },
            { new( EquipmentType.GEAR_MAG_ON_HIT,    EquipmentCategoryType.Trinket ), new( "Leviathan Trinket", 8125,  6000, 100, 25,  0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   10,  5   ) },
            { new( EquipmentType.GEAR_LIFE_STEAL_2,  EquipmentCategoryType.Weapon  ), new( "Kin Weapon",        6700,  5000, 120, 30,  0,   0,   0,   0,   24,  12,  0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_LIFE_STEAL_2,  EquipmentCategoryType.Head    ), new( "Kin Head",          5350,  4000, 130, 35,  26,  13,  0,   0,   0,   0,   0,   0,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_LIFE_STEAL_2,  EquipmentCategoryType.Chest   ), new( "Kin Chest",         8050,  6000, 125, 35,  0,   0,   16,  10,  0,   0,   0,   0,   0,   0,   13,  10  ) },
            { new( EquipmentType.GEAR_LIFE_STEAL_2,  EquipmentCategoryType.Cape    ), new( "Kin Cape",          6025,  4500, 135, 35,  14,  7,   0,   0,   0,   0,   14,  7,   0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_LIFE_STEAL_2,  EquipmentCategoryType.Trinket ), new( "Kin Trinket",       9375,  7000, 125, 35,  0,   0,   0,   0,   0,   0,   7,   4,   0,   0,   7,   4   ) },
            { new( EquipmentType.GEAR_REVIVE,        EquipmentCategoryType.Weapon  ), new( "WhiteWood Weapon",  7675,  5700, 135, 35,  -7,  -3,  -9,  -4,  27,  14,  0,   0,   0,   0,   27,  14  ) },
            { new( EquipmentType.GEAR_REVIVE,        EquipmentCategoryType.Head    ), new( "WhiteWood Head",    6150,  4550, 125, 35,  -7,  -3,  -8,  -4,  25,  13,  0,   0,   0,   0,   25,  13  ) },
            { new( EquipmentType.GEAR_REVIVE,        EquipmentCategoryType.Chest   ), new( "WhiteWood Chest",   9200,  6850, 150, 40,  -8,  -4,  -10, -5,  0,   0,   0,   0,   0,   0,   45,  23  ) },
            { new( EquipmentType.GEAR_REVIVE,        EquipmentCategoryType.Cape    ), new( "WhiteWood Cape",    6900,  5150, 165, 45,  -9,  -4,  -11, -5,  0,   0,   0,   0,   33,  17,  33,  17  ) },
            { new( EquipmentType.GEAR_REVIVE,        EquipmentCategoryType.Trinket ), new( "WhiteWood Trinket", 10750, 8000, 125, 35,  -7,  -3,  -8,  -4,  0,   0,   0,   0,   0,   0,   25,  13  ) },
            { new( EquipmentType.GEAR_FINAL_BOSS,    EquipmentCategoryType.Weapon  ), new( "BlackRoot Weapon",  7675,  5700, 165, 45,  -9,  -4,  -11, -5,  33,  17,  33,  17,  0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_FINAL_BOSS,    EquipmentCategoryType.Head    ), new( "BlackRoot Head",    6150,  4550, 125, 35,  -7,  -3,  -8,  -4,  0,   0,   25,  13,  0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_FINAL_BOSS,    EquipmentCategoryType.Chest   ), new( "BlackRoot Chest",   9200,  6850, 150, 40,  -8,  -4,  -10, -5,  0,   0,   45,  23,  0,   0,   0,   0   ) },
            { new( EquipmentType.GEAR_FINAL_BOSS,    EquipmentCategoryType.Cape    ), new( "BlackRoot Cape",    6900,  5150, 135, 35,  -7,  -3,  -9,  -4,  0,   0,   27,  14,  27,  14,  0,   0   ) },
            { new( EquipmentType.GEAR_FINAL_BOSS,    EquipmentCategoryType.Trinket ), new( "BlackRoot Trinket", 10750, 8000, 125, 35,  -7,  -3,  -8,  -4,  0,   0,   25,  13,  0,   0,   0,   0   ) },
        };

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTINGS READ/WRITE
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        private static readonly WobSettings.FileHelper<EquipmentType> equipSetFiles = new( "EquipSet", 2 );

        internal static readonly WobSettings.KeyHelper<EquipmentType> equipSetUnityKeys = new( "BonusForUnity" );
        internal static readonly WobSettings.KeyHelper<EquipmentType> equipSetChestKeys = new( "BlueprintDrop" );
        internal static readonly WobSettings.KeyHelper<EquipmentCategoryType> equipSlotKeys = new( "Slot", 1 );
        internal record EquipItemKeys( EquipmentType Set, EquipmentCategoryType Slot );
        internal static readonly WobSettings.KeyHelper<EquipItemKeys> equipItemKeys = new();

        internal static void RunSetup() {
            WobMod.configFiles.Add( "Equipment", "Equipment" );
            for( int i = 0; i < Economy_EV.EQUIPMENT_LEVEL_GOLD_MOD.Length; i++ ) {
                WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "Equipment" ), "ScalingCosts_Equipment_Gold", "GoldCostMult_Level" + ( i + 1 ), "Multiply base gold cost by this value to get the armor item cost for level " + ( i + 1 ), Economy_EV.EQUIPMENT_LEVEL_GOLD_MOD[i], bounds: (1, 1000000) ) );
            }
            for( int i = 0; i < Economy_EV.EQUIPMENT_LEVEL_ORE_MOD.Length; i++ ) {
                WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "Equipment" ), "ScalingCosts_Equipment_Ore", "OreCostMult_Level" + ( i + 1 ), "Multiply base ore cost by this value to get the armor item cost for level " + ( i + 1 ), Economy_EV.EQUIPMENT_LEVEL_ORE_MOD[i], bounds: (1, 1000000) ) );
            }
            foreach( EquipmentCategoryType slotType in equipSlotInfo.Keys ) {
                equipSlotKeys.Add( slotType, equipSlotInfo[slotType].Config );
                WobSettings.Add( new WobSettings.Entry[] {
                    new WobSettings.Num<int>( WobMod.configFiles.Get( "Equipment" ), equipSlotKeys.Get( slotType, "Unity" ), equipSlotInfo[slotType].Config + " base set unity per armor item level", equipSlotInfo[slotType].Unity, bounds: (0, 100) ),
                    new WobSettings.Enum<BlueprintRarity>( WobMod.configFiles.Get( "Equipment" ), equipSlotKeys.Get( slotType, "Rarity" ), equipSlotInfo[slotType].Config + " chest rarity for blueprint drops", equipSlotInfo[slotType].Rarity ),
                    new WobSettings.Num<int>( WobMod.configFiles.Get( "Equipment" ), equipSlotKeys.Get( slotType, "Level" ), equipSlotInfo[slotType].Config + " chest level added to set base level for blueprint drops", equipSlotInfo[slotType].Level, bounds: (0, 1000) ),
                } );
            }
            foreach( EquipmentType setType in equipSetInfo.Keys ) {
                equipSetFiles.Add( setType, equipSetInfo[setType].Config );
                equipSetUnityKeys.Add( setType, equipSetInfo[setType].Config );
                equipSetChestKeys.Add( setType, equipSetInfo[setType].Config );
                WobSettings.Add( new WobSettings.Entry[] {
                    // Unity stat bonuses
                    new WobSettings.Enum<EquipmentSetBonusType>( equipSetFiles.Get( setType ), equipSetUnityKeys.Get( setType, "U20_BonusType"  ), equipSetInfo[setType].Config + " set - 20 unity bonus type",  equipSetInfo[setType].BonusType1                        ),
                    new WobSettings.Num<float>(                  equipSetFiles.Get( setType ), equipSetUnityKeys.Get( setType, "U20_StatGain"   ), equipSetInfo[setType].Config + " set - 20 unity stat gain",   equipSetInfo[setType].StatGain1, bounds: (0f, 1000000f) ),
                    new WobSettings.Enum<EquipmentSetBonusType>( equipSetFiles.Get( setType ), equipSetUnityKeys.Get( setType, "U50_BonusType"  ), equipSetInfo[setType].Config + " set - 50 unity bonus type",  equipSetInfo[setType].BonusType2                        ),
                    new WobSettings.Num<float>(                  equipSetFiles.Get( setType ), equipSetUnityKeys.Get( setType, "U50_StatGain"   ), equipSetInfo[setType].Config + " set - 50 unity stat gain",   equipSetInfo[setType].StatGain2, bounds: (0f, 1000000f) ),
                    new WobSettings.Enum<EquipmentSetBonusType>( equipSetFiles.Get( setType ), equipSetUnityKeys.Get( setType, "U100_BonusType" ), equipSetInfo[setType].Config + " set - 100 unity bonus type", equipSetInfo[setType].BonusType3                        ),
                    new WobSettings.Num<float>(                  equipSetFiles.Get( setType ), equipSetUnityKeys.Get( setType, "U100_StatGain"  ), equipSetInfo[setType].Config + " set - 100 unity stat gain",  equipSetInfo[setType].StatGain3, bounds: (0f, 1000000f) ),
                    // Chest level requirements
                    new WobSettings.Num<int>( equipSetFiles.Get( setType ), equipSetChestKeys.Get( setType, "BaseLevel"    ), equipSetInfo[setType].Config + " set - base level for blueprint spawn",    equipSetInfo[setType].BaseLevel,    bounds: (1, 1000) ),
                    new WobSettings.Num<int>( equipSetFiles.Get( setType ), equipSetChestKeys.Get( setType, "ScalingLevel" ), equipSetInfo[setType].Config + " set - scaling level for blueprint spawn", equipSetInfo[setType].ScalingLevel, bounds: (1, 1000) ),
                } );
            }
            foreach( EquipItemKeys itemType in equipItemInfo.Keys ) {
                equipItemKeys.Add( itemType, "Item" + equipSlotInfo[itemType.Slot].UIIndex, equipSetInfo[itemType.Set].Config + "_" + equipSlotInfo[itemType.Slot].Config );
                WobSettings.Add( new WobSettings.Entry[] {
                    new WobSettings.Num<int>( equipSetFiles.Get( itemType.Set ), equipItemKeys.Get( itemType, "GoldCost"       ), equipItemInfo[itemType].Name + " - base gold cost",                          equipItemInfo[itemType].GoldCost, bounds: (1, 1000000) ),
                    new WobSettings.Num<int>( equipSetFiles.Get( itemType.Set ), equipItemKeys.Get( itemType, "OreCost"        ), equipItemInfo[itemType].Name + " - base ore cost",                           equipItemInfo[itemType].OreCost,  bounds: (1, 1000000) ),
                    new WobSettings.Num<int>( equipSetFiles.Get( itemType.Set ), equipItemKeys.Get( itemType, "Weight_Base"    ), equipItemInfo[itemType].Name + " - base weight",                             equipItemInfo[itemType].WeightB,  bounds: (1, 1000000) ),
                    new WobSettings.Num<int>( equipSetFiles.Get( itemType.Set ), equipItemKeys.Get( itemType, "Weight_Scaling" ), equipItemInfo[itemType].Name + " - additional weight per level",             equipItemInfo[itemType].WeightS,  bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( equipSetFiles.Get( itemType.Set ), equipItemKeys.Get( itemType, "Vit_Base"       ), equipItemInfo[itemType].Name + " - base health bonus",                       equipItemInfo[itemType].VitB,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( equipSetFiles.Get( itemType.Set ), equipItemKeys.Get( itemType, "Vit_Scaling"    ), equipItemInfo[itemType].Name + " - additional health bonus per level",       equipItemInfo[itemType].VitS,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( equipSetFiles.Get( itemType.Set ), equipItemKeys.Get( itemType, "Arm_Base"       ), equipItemInfo[itemType].Name + " - base armor bonus",                        equipItemInfo[itemType].ArmB,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( equipSetFiles.Get( itemType.Set ), equipItemKeys.Get( itemType, "Arm_Scaling"    ), equipItemInfo[itemType].Name + " - additional armor bonus per level",        equipItemInfo[itemType].ArmS,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( equipSetFiles.Get( itemType.Set ), equipItemKeys.Get( itemType, "Str_Base"       ), equipItemInfo[itemType].Name + " - base strength bonus",                     equipItemInfo[itemType].StrB,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( equipSetFiles.Get( itemType.Set ), equipItemKeys.Get( itemType, "Str_Scaling"    ), equipItemInfo[itemType].Name + " - additional strength bonus per level",     equipItemInfo[itemType].StrS,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( equipSetFiles.Get( itemType.Set ), equipItemKeys.Get( itemType, "Dex_Base"       ), equipItemInfo[itemType].Name + " - base dexterity bonus",                    equipItemInfo[itemType].DexB,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( equipSetFiles.Get( itemType.Set ), equipItemKeys.Get( itemType, "Dex_Scaling"    ), equipItemInfo[itemType].Name + " - additional dexterity bonus per level",    equipItemInfo[itemType].DexS,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( equipSetFiles.Get( itemType.Set ), equipItemKeys.Get( itemType, "Int_Base"       ), equipItemInfo[itemType].Name + " - base intelligence bonus",                 equipItemInfo[itemType].IntB,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( equipSetFiles.Get( itemType.Set ), equipItemKeys.Get( itemType, "Int_Scaling"    ), equipItemInfo[itemType].Name + " - additional intelligence bonus per level", equipItemInfo[itemType].IntS,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( equipSetFiles.Get( itemType.Set ), equipItemKeys.Get( itemType, "Foc_Base"       ), equipItemInfo[itemType].Name + " - base focus bonus",                        equipItemInfo[itemType].FocB,     bounds: (0, 1000000) ),
                    new WobSettings.Num<int>( equipSetFiles.Get( itemType.Set ), equipItemKeys.Get( itemType, "Foc_Scaling"    ), equipItemInfo[itemType].Name + " - additional focus bonus per level",        equipItemInfo[itemType].FocS,     bounds: (0, 1000000) ),
                } );
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - EQUIPMENT
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Patch to change scaling gold costs
        [HarmonyPatch( typeof( EquipmentObj ), nameof( EquipmentObj.GoldCostToUpgrade ), MethodType.Getter )]
        internal static class EquipmentObj_GoldCostToUpgrade_Patch {
            private static bool runOnce = false;
            internal static void Prefix() {
                if( !runOnce ) {
                    for( int i = 0; i < Economy_EV.EQUIPMENT_LEVEL_GOLD_MOD.Length; i++ ) {
                        Economy_EV.EQUIPMENT_LEVEL_GOLD_MOD[i] = WobSettings.Get( "ScalingCosts_Equipment_Gold", "GoldCostMult_Level" + ( i + 1 ), Economy_EV.EQUIPMENT_LEVEL_GOLD_MOD[i] );
                    }
                    runOnce = true;
                }
            }
            internal static void Postfix( EquipmentObj __instance, ref int __result ) {
                if( WobSettings.Get( "Resources", "ConvertOres", false ) ) {
                    if( __instance.EquipmentData.Disabled ) { return; }
                    int oreCost = __instance.EquipmentData.OreCost;
                    int newLevel = Mathf.Clamp( __instance.UpgradeLevel + 1, 0, Economy_EV.EQUIPMENT_LEVEL_ORE_MOD.Length - 1 );
                    __result += oreCost * Economy_EV.EQUIPMENT_LEVEL_ORE_MOD[newLevel];
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
                        Economy_EV.EQUIPMENT_LEVEL_ORE_MOD[i] = WobSettings.Get( "ScalingCosts_Equipment_Ore", "OreCostMult_Level" + ( i + 1 ), Economy_EV.EQUIPMENT_LEVEL_ORE_MOD[i] );
                    }
                    runOnce = true;
                }
            }
            internal static void Postfix( ref int __result ) {
                if( WobSettings.Get( "Resources", "ConvertOres", false ) ) {
                    __result = 0;
                }
            }
        }

        // Patch to change set unity bonuses
        [HarmonyPatch( typeof( EquipmentSetLibrary ), "Instance", MethodType.Getter )]
        internal static class EquipmentSetLibrary_Instance_Patch {
            private static bool runOnce = false;
            internal static void Postfix( EquipmentSetLibrary __result ) {
                if( !runOnce ) {
                    EquipmentTypeEquipmentSetDataDictionary m_equipmentSetLibrary = Traverse.Create( __result ).Field( "m_equipmentSetLibrary" ).GetValue<EquipmentTypeEquipmentSetDataDictionary>();
                    foreach( EquipmentType setType in m_equipmentSetLibrary.Keys ) {
                        if( setType is EquipmentType.None or EquipmentType.GEAR_EMPTY_1 or EquipmentType.GEAR_EMPTY_2 ) { continue; }
                        EquipmentSetData setData = m_equipmentSetLibrary[setType];
                        //WobPlugin.Log( "[Equipment] " + setType + "; U20: " + setData.SetBonus01.BonusType + ", " + setData.SetBonus01.StatGain + "; U50: " + setData.SetBonus02.BonusType + ", " + setData.SetBonus02.StatGain + "; U100: " + setData.SetBonus03.BonusType + ", " + setData.SetBonus03.StatGain );
                        if( equipSetUnityKeys.Exists( setType ) ) {
                            setData.SetBonus01 = new EquipmentSetBonus( WobSettings.Get( equipSetUnityKeys.Get( setType, "U20_BonusType" ), setData.SetBonus01.BonusType ), WobSettings.Get( equipSetUnityKeys.Get( setType, "U20_StatGain" ), setData.SetBonus01.StatGain ) );
                            setData.SetBonus02 = new EquipmentSetBonus( WobSettings.Get( equipSetUnityKeys.Get( setType, "U50_BonusType" ), setData.SetBonus02.BonusType ), WobSettings.Get( equipSetUnityKeys.Get( setType, "U50_StatGain" ), setData.SetBonus02.StatGain ) );
                            setData.SetBonus03 = new EquipmentSetBonus( WobSettings.Get( equipSetUnityKeys.Get( setType, "U100_BonusType" ), setData.SetBonus03.BonusType ), WobSettings.Get( equipSetUnityKeys.Get( setType, "U100_StatGain" ), setData.SetBonus03.StatGain ) );
                        } else {
                            WobPlugin.Log( "[Equipment] equipSetUnityKeys missing " + setType + "; U20: " + setData.SetBonus01.BonusType + ", " + setData.SetBonus01.StatGain + "; U50: " + setData.SetBonus02.BonusType + ", " + setData.SetBonus02.StatGain + "; U100: " + setData.SetBonus03.BonusType + ", " + setData.SetBonus03.StatGain );
                        }
                    }
                    runOnce = true;
                }
            }
        }

        // Patch to change basic item stats
        [HarmonyPatch( typeof( EquipmentLibrary ), "Instance", MethodType.Getter )]
        internal static class EquipmentLibrary_Instance_Patch {
            private static bool runOnce = false;
            internal static void Postfix( EquipmentLibrary __result ) {
                if( !runOnce ) {
                    foreach( EquipmentCategoryType slotType in equipSlotInfo.Keys ) {
                        EquipmentTypeEquipmentDataDictionary equipmentLibrary  = Traverse.Create( __result ).Field( equipSlotInfo[slotType].LibraryField ).GetValue<EquipmentTypeEquipmentDataDictionary>();
                        int unity = WobSettings.Get( equipSlotKeys.Get( slotType, "Unity" ), equipSlotInfo[slotType].Unity );
                        int rarity = (int)WobSettings.Get( equipSlotKeys.Get( slotType, "Rarity" ), equipSlotInfo[slotType].Rarity );
                        int slotLevel = WobSettings.Get( equipSlotKeys.Get( slotType, "Level" ), equipSlotInfo[slotType].Level );
                        foreach( EquipmentType setType in equipmentLibrary.Keys ) {
                            if( setType is EquipmentType.None or EquipmentType.GEAR_EMPTY_1 or EquipmentType.GEAR_EMPTY_2 ) { continue; }
                            EquipmentData equipData = equipmentLibrary[setType];
                            equipData.BaseEquipmentSetLevel = unity;
                            equipData.ScalingEquipmentSetLevel = unity;
                            equipData.ChestRarityRequirement = rarity;
                            if( equipSetChestKeys.Exists( setType ) ) {
                                equipData.ChestLevelRequirement = slotLevel + WobSettings.Get( equipSetChestKeys.Get( setType, "BaseLevel" ), equipSetInfo[setType].BaseLevel );
                                equipData.ScalingItemLevel = WobSettings.Get( equipSetChestKeys.Get( setType, "ScalingLevel" ), equipSetInfo[setType].ScalingLevel );
                            } else {
                                WobPlugin.Log( "[Equipment] equipSetChestKeys missing " + setType + "; BaseLevel: " + equipData.ChestLevelRequirement + "; ScalingLevel: " + equipData.ScalingItemLevel );
                            }
                            if( equipItemKeys.Exists( new(setType, slotType) ) ) {
                                equipData.GoldCost                  = WobSettings.Get( equipItemKeys.Get( new( setType, slotType), "GoldCost"       ), equipData.GoldCost                  );
                                equipData.OreCost                   = WobSettings.Get( equipItemKeys.Get( new( setType, slotType), "OreCost"        ), equipData.OreCost                   );
                                equipData.BaseWeight                = WobSettings.Get( equipItemKeys.Get( new( setType, slotType), "Weight_Base"    ), equipData.BaseWeight                );
                                equipData.ScalingWeight             = WobSettings.Get( equipItemKeys.Get( new( setType, slotType), "Weight_Scaling" ), equipData.ScalingWeight             );
                                equipData.BaseHealth                = WobSettings.Get( equipItemKeys.Get( new( setType, slotType), "Vit_Base"       ), equipData.BaseHealth                );
                                equipData.ScalingHealth             = WobSettings.Get( equipItemKeys.Get( new( setType, slotType), "Vit_Scaling"    ), equipData.ScalingHealth             );
                                equipData.BaseArmor                 = WobSettings.Get( equipItemKeys.Get( new( setType, slotType), "Arm_Base"       ), equipData.BaseArmor                 );
                                equipData.ScalingArmor              = WobSettings.Get( equipItemKeys.Get( new( setType, slotType), "Arm_Scaling"    ), equipData.ScalingArmor              );
                                equipData.BaseStrengthDamage        = WobSettings.Get( equipItemKeys.Get( new( setType, slotType), "Str_Base"       ), equipData.BaseStrengthDamage        );
                                equipData.ScalingStrengthDamage     = WobSettings.Get( equipItemKeys.Get( new( setType, slotType), "Str_Scaling"    ), equipData.ScalingStrengthDamage     );
                                equipData.BaseStrengthCritChance    = WobSettings.Get( equipItemKeys.Get( new( setType, slotType), "Dex_Base"       ), equipData.BaseStrengthCritChance    );
                                equipData.ScalingStrengthCritChance = WobSettings.Get( equipItemKeys.Get( new( setType, slotType), "Dex_Scaling"    ), equipData.ScalingStrengthCritChance );
                                equipData.BaseMagicDamage           = WobSettings.Get( equipItemKeys.Get( new( setType, slotType), "Int_Base"       ), equipData.BaseMagicDamage           );
                                equipData.ScalingMagicDamage        = WobSettings.Get( equipItemKeys.Get( new( setType, slotType), "Int_Scaling"    ), equipData.ScalingMagicDamage        );
                                equipData.BaseMagicCritChance       = WobSettings.Get( equipItemKeys.Get( new( setType, slotType), "Foc_Base"       ), equipData.BaseMagicCritChance       );
                                equipData.ScalingMagicCritChance    = WobSettings.Get( equipItemKeys.Get( new( setType, slotType), "Foc_Scaling"    ), equipData.ScalingMagicCritChance    );
                            } else {
                                WobPlugin.Log( "[Equipment] equipItemKeys missing " + setType + ", " + slotType
                                    + "; Cost: " + equipData.GoldCost + ", " + equipData.OreCost
                                    + "; Weight: " + equipData.BaseWeight + ", " + equipData.ScalingWeight
                                    + "; Vit: " + equipData.BaseHealth + ", " + equipData.ScalingHealth
                                    + "; Arm: " + equipData.BaseArmor + ", " + equipData.ScalingArmor
                                    + "; Str: " + equipData.BaseStrengthDamage + ", " + equipData.ScalingStrengthDamage
                                    + "; Dex: " + equipData.BaseStrengthCritChance + ", " + equipData.ScalingStrengthCritChance
                                    + "; Int: " + equipData.BaseMagicDamage + ", " + equipData.ScalingMagicDamage
                                    + "; Foc: " + equipData.BaseMagicCritChance + ", " + equipData.ScalingMagicCritChance );
                            }
                        }
                    }
                    runOnce = true;
                }
            }
        }

    }
}
