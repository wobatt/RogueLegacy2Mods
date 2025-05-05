using System.Collections.Generic;
using HarmonyLib;
using Wob_Common;

namespace WobMod {
    internal class Scars {

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTING DEFAULTS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        private record ScarInfo( string Config, string Name, ChallengeScoringType ScoringType );
        private static readonly Dictionary<ChallengeType, ScarInfo> scarInfo = new() {
            { ChallengeType.IntroCombat,       new( "ASimpleStart",          "A Simple Start",         ChallengeScoringType.Battle   ) },
            { ChallengeType.PlatformAxe,       new( "HeavyWeapons",          "Heavy Weapons",          ChallengeScoringType.Platform ) },
            { ChallengeType.TwinMech,          new( "TwoMasters",            "Two Masters",            ChallengeScoringType.Battle   ) },
            { ChallengeType.PlatformRanger,    new( "NarrowPraxis",          "Narrow Praxis",          ChallengeScoringType.Platform ) },
            { ChallengeType.BrotherAndSister,  new( "BladedRose",            "Bladed Rose",            ChallengeScoringType.Battle   ) },
            { ChallengeType.SmallChest,        new( "ClosedSpace",           "Closed Space",           ChallengeScoringType.Battle   ) },
            { ChallengeType.FourHands,         new( "Automatons",            "Automatons",             ChallengeScoringType.Battle   ) },
            { ChallengeType.SubBossBattle,     new( "SpreadingPoison",       "Spreading Poison",       ChallengeScoringType.Battle   ) },
            { ChallengeType.PlatformBoat,      new( "PreserverofLife",       "Preserver of Life",      ChallengeScoringType.Platform ) },
            { ChallengeType.PlatformKatana,    new( "TheRebelsRoad",         "The Rebels Road",        ChallengeScoringType.Platform ) },
            { ChallengeType.TwoLovers,         new( "TheTwoLovers",          "The Two Lovers",         ChallengeScoringType.Battle   ) },
            { ChallengeType.NightmareKhidr,    new( "NightmarePremonitions", "Nightmare Premonitions", ChallengeScoringType.Battle   ) },
            { ChallengeType.PlatformClimb,     new( "AtlantisSpire",         "Atlantis Spire",         ChallengeScoringType.Platform ) },
            { ChallengeType.BigBattle,         new( "TheArmada",             "The Armada",             ChallengeScoringType.Battle   ) },
            { ChallengeType.TwoRebels,         new( "DivergentDimensions",   "Divergent Dimensions",   ChallengeScoringType.Battle   ) },
            { ChallengeType.DragonAspectFight, new( "DragonFlight",          "Dragon Flight",          ChallengeScoringType.Platform ) },
            { ChallengeType.PlatformSurf,      new( "BoogieDays",            "Boogie Days",            ChallengeScoringType.Platform ) },
            { ChallengeType.QuinnFight,        new( "TrainingDaze",          "Training Daze",          ChallengeScoringType.Battle   ) },
        };

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTINGS READ/WRITE
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        internal static readonly WobSettings.KeyHelper<ChallengeType> scarKeys = new( "Scar", 2 );

        internal static void RunSetup() {
            WobMod.configFiles.Add( "Scars", "Scars" );
            foreach( ChallengeType challengeType in scarInfo.Keys ) {
                scarKeys.Add( challengeType, scarInfo[challengeType].Config );
                if( scarInfo[challengeType].ScoringType == ChallengeScoringType.Platform ) {
                    WobSettings.Add( new WobSettings.Num<float>( WobMod.configFiles.Get( "Scars" ), scarKeys.Get( challengeType, "ExtraTime" ), "Extra time in seconds for " + scarInfo[challengeType].Name, 0f, bounds: (0f, 3589f) ) ); // Why 3589? Must be under 1 hour for displayed digits, and bronze target adds 10 seconds, so: (60*60)-11=3589
                } else {
                    WobSettings.Add( new WobSettings.Num<float>( WobMod.configFiles.Get( "Scars" ), scarKeys.Get( challengeType, "DamageMod" ), "Enemy damage modifier percent for " + scarInfo[challengeType].Name, 0f, 0.01f, bounds: (-100f, 1000000f) ) );
                }
            }
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Boolean( WobMod.configFiles.Get( "Scars" ), "ScarDifficulty", "KeepBurdens",      "Do not remove NG+ burdens for scar challenges",           false ),
                new WobSettings.Boolean( WobMod.configFiles.Get( "Scars" ), "ScarDifficulty", "KeepTraits",       "Do not remove traits and antiques for scar challenges",   false ),
                new WobSettings.Boolean( WobMod.configFiles.Get( "Scars" ), "ScarDifficulty", "KeepEquipment",    "Do not remove equipment for scar challenges",             false ),
                new WobSettings.Boolean( WobMod.configFiles.Get( "Scars" ), "ScarDifficulty", "KeepRunes",        "Do not remove runes for scar challenges",                 false ),
                new WobSettings.Boolean( WobMod.configFiles.Get( "Scars" ), "ScarDifficulty", "KeepMastery",      "Do not remove class mastery XP for scar challenges",      false ),
                new WobSettings.Boolean( WobMod.configFiles.Get( "Scars" ), "ScarDifficulty", "KeepCastleSkills", "Do not remove castle skill upgrades for scar challenges", false ),
                new WobSettings.Boolean( WobMod.configFiles.Get( "Scars" ), "ScarDifficulty", "KeepSoulShop",     "Do not remove soul shop upgrades for scar challenges",    false ),
            } );
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - SCAR DATA
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Apply changes to scar challenges when the menu item for it in the scars list is updated
        [HarmonyPatch( typeof( ChallengeManager ), "OnStartGlobalTimer" )]
        internal static class ChallengeManager_OnStartGlobalTimer_Patch {
            internal static void Postfix() {
                if( ChallengeManager.IsInChallenge && ChallengeManager.ActiveChallenge.ChallengeData.ScoringType == ChallengeScoringType.Platform ) {
                    GlobalTimerHUDController.ElapsedTime -= WobSettings.Get( scarKeys.Get( ChallengeManager.ActiveChallenge.ChallengeType, "ExtraTime" ), 0f );
                }
            }
        }
        
        // Apply changes to scar challenges when the menu item for it in the scars list is updated
        [HarmonyPatch( typeof( PlayerController ), "ApplyAssistDamageMods" )]
        internal static class PlayerController_ApplyAssistDamageMods_Patch {
            internal static void Postfix( ref float __result ) {
                if( ChallengeManager.IsInChallenge && ChallengeManager.ActiveChallenge.ChallengeData.ScoringType == ChallengeScoringType.Battle ) {
                    __result *= 1f + WobSettings.Get( scarKeys.Get( ChallengeManager.ActiveChallenge.ChallengeType, "DamageMod" ), 0f );
                }
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - CHARACTER SETUP
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Apply changes to scar challenges when the menu item for it in the scars list is updated
        [HarmonyPatch( typeof( ChallengeManager ), nameof( ChallengeManager.ApplyStatCap ) )]
        internal static class ChallengeManager_ApplyStatCap_Patch {
            internal static void Postfix( float actualStat, ref float __result ) {
                if( WobSettings.Get( "ScarDifficulty", "KeepCastleSkills", false ) ) {
                    __result = actualStat;
                }
            }
        }

        // Apply changes to scar challenges when setting up a character for the challenge
        [HarmonyPatch( typeof( ChallengeManager ), nameof( ChallengeManager.SetupCharacter ) )]
        internal static class ChallengeManager_SetupCharacter_Patch {
            internal static bool Prefix() {
                // Use the total replacement method
                SetupCharacter();
                // Do not run original method
                return false;
            }

            private static void SetupCharacter() {
                Traverse<CharacterData> m_storedCharData = Traverse.Create( ChallengeManager.Instance ).Field<CharacterData>( "m_storedCharData" );
                m_storedCharData.Value = SaveManager.PlayerSaveData.CurrentCharacter.Clone();
                ClassType classType = ChallengeManager.GetChallengeClassOverride(ChallengeManager.ActiveChallenge.ChallengeType);
                if( classType == ClassType.None && ( SaveManager.PlayerSaveData.CurrentCharacter.TraitOne == TraitType.RandomizeKit || SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo == TraitType.RandomizeKit || SaveManager.PlayerSaveData.CurrentCharacter.TraitOne == TraitType.SuperFart || SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo == TraitType.SuperFart ) ) {
                    classType = SaveManager.PlayerSaveData.CurrentCharacter.ClassType;
                }
                if( classType != ClassType.None ) {
                    CharacterCreator.GenerateClass( classType, SaveManager.PlayerSaveData.CurrentCharacter );
                    SaveManager.PlayerSaveData.CurrentCharacter.Spell = m_storedCharData.Value.Spell;
                }
                if( SaveManager.PlayerSaveData.CurrentCharacter.Weapon == AbilityType.PacifistWeapon ) {
                    SaveManager.PlayerSaveData.CurrentCharacter.Weapon = CharacterCreator.GetAvailableWeapons( SaveManager.PlayerSaveData.CurrentCharacter.ClassType )[0];
                }
                if( !WobSettings.Get( "ScarDifficulty", "KeepTraits", false ) ) {
                    SaveManager.PlayerSaveData.CurrentCharacter.TraitOne = TraitType.None;
                    SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo = TraitType.None;
                    Messenger<GameMessenger, GameEvent>.Broadcast( GameEvent.TraitsChanged, ChallengeManager.Instance, new TraitChangedEventArgs( TraitType.None, TraitType.None ) );
                    SaveManager.PlayerSaveData.CurrentCharacter.AntiqueOneOwned = RelicType.None;
                    SaveManager.PlayerSaveData.CurrentCharacter.AntiqueTwoOwned = RelicType.None;
                }
                if( !WobSettings.Get( "ScarDifficulty", "KeepEquipment", false ) ) {
                    SaveManager.PlayerSaveData.CurrentCharacter.CapeEquipmentType = EquipmentType.None;
                    SaveManager.PlayerSaveData.CurrentCharacter.ChestEquipmentType = EquipmentType.None;
                    SaveManager.PlayerSaveData.CurrentCharacter.EdgeEquipmentType = EquipmentType.None;
                    SaveManager.PlayerSaveData.CurrentCharacter.HeadEquipmentType = EquipmentType.None;
                    SaveManager.PlayerSaveData.CurrentCharacter.TrinketEquipmentType = EquipmentType.None;
                }
                Traverse<Dictionary<RuneType, int>> m_storedRuneDict = Traverse.Create( ChallengeManager.Instance ).Field<Dictionary<RuneType,int>>( "m_storedRuneDict" );
                m_storedRuneDict.Value.Clear();
                foreach( KeyValuePair<RuneType, RuneObj> runeLevel in SaveManager.EquipmentSaveData.RuneDict ) {
                    m_storedRuneDict.Value.Add( runeLevel.Key, runeLevel.Value.EquippedLevel );
                    if( !WobSettings.Get( "ScarDifficulty", "KeepRunes", false ) ) {
                        runeLevel.Value.EquippedLevel = 0;
                    }
                }
                Traverse<Dictionary<RelicType, int>> m_storedRelicDict = Traverse.Create( ChallengeManager.Instance ).Field<Dictionary<RelicType,int>>( "m_storedRelicDict" );
                m_storedRelicDict.Value.Clear();
                foreach( KeyValuePair<RelicType, RelicObj> relicLevel in SaveManager.PlayerSaveData.RelicObjTable ) {
                    m_storedRelicDict.Value.Add( relicLevel.Key, relicLevel.Value.Level );
                    if( !WobSettings.Get( "ScarDifficulty", "KeepTraits", false ) ) {
                        relicLevel.Value.SetLevel( 0, false, true );
                    }
                }
                Traverse<Dictionary<ClassType, int>> m_storedMasteryXPTable = Traverse.Create( ChallengeManager.Instance ).Field<Dictionary<ClassType,int>>( "m_storedMasteryXPTable" );
                m_storedMasteryXPTable.Value.Clear();
                foreach( ClassType classType2 in ClassType_RL.TypeArray ) {
                    if( classType2 != ClassType.None ) {
                        m_storedMasteryXPTable.Value.Add( classType2, SaveManager.PlayerSaveData.GetClassXP( classType2 ) );
                        if( !WobSettings.Get( "ScarDifficulty", "KeepMastery", false ) ) {
                            SaveManager.PlayerSaveData.SetClassXP( classType2, 0, false, true, true );
                        }
                    }
                }
                Traverse<Dictionary<SkillTreeType, int>> m_storedSkillTreeDict = Traverse.Create( ChallengeManager.Instance ).Field<Dictionary<SkillTreeType,int>>( "m_storedSkillTreeDict" );
                m_storedSkillTreeDict.Value.Clear();
                foreach( KeyValuePair<SkillTreeType, SkillTreeObj> skillTreeLevel in SaveManager.EquipmentSaveData.SkillTreeDict ) {
                    m_storedSkillTreeDict.Value.Add( skillTreeLevel.Key, skillTreeLevel.Value.Level );
                    if( !WobSettings.Get( "ScarDifficulty", "KeepCastleSkills", false ) ) {
                        SkillTreeManager.SetSkillObjLevel( skillTreeLevel.Key, 0, false, true, false );
                    }
                }
                if( !WobSettings.Get( "ScarDifficulty", "KeepCastleSkills", false ) ) {
                    SaveManager.EquipmentSaveData.SkillTreeDict[SkillTreeType.Relic_Cost_Down].Level = 5;
                    SaveManager.EquipmentSaveData.SkillTreeDict[SkillTreeType.Reroll_Relic].Level = 3;
                    SaveManager.EquipmentSaveData.SkillTreeDict[SkillTreeType.Reroll_Relic_Room_Cap].Level = 1;
                }
                Traverse<Dictionary<BurdenType, int>> m_storedBurdenDict = Traverse.Create( ChallengeManager.Instance ).Field<Dictionary<BurdenType,int>>( "m_storedBurdenDict" );
                m_storedBurdenDict.Value.Clear();
                foreach( KeyValuePair<BurdenType, BurdenObj> burdenLevel in SaveManager.PlayerSaveData.BurdenObjTable ) {
                    m_storedBurdenDict.Value.Add( burdenLevel.Key, burdenLevel.Value.CurrentLevel );
                    if( !WobSettings.Get( "ScarDifficulty", "KeepBurdens", false ) ) {
                        burdenLevel.Value.SetLevel( 0, false, true );
                    }
                }
                Traverse<int> m_storedNGPlusLevel = Traverse.Create( ChallengeManager.Instance ).Field<int>( "m_storedNGPlusLevel" );
                m_storedNGPlusLevel.Value = SaveManager.PlayerSaveData.NewGamePlusLevel;
                if( !WobSettings.Get( "ScarDifficulty", "KeepBurdens", false ) ) {
                    SaveManager.PlayerSaveData.NewGamePlusLevel = 0;
                }
                Messenger<GameMessenger, GameEvent>.Broadcast( GameEvent.NGPlusChanged, null, null );
                Traverse<Dictionary<SoulShopType, int>> m_storedSoulShopDict = Traverse.Create( ChallengeManager.Instance ).Field<Dictionary<SoulShopType,int>>( "m_storedSoulShopDict" );
                m_storedSoulShopDict.Value.Clear();
                foreach( KeyValuePair<SoulShopType, SoulShopObj> soulShopLevel in SaveManager.ModeSaveData.SoulShopTable ) {
                    m_storedSoulShopDict.Value.Add( soulShopLevel.Key, soulShopLevel.Value.CurrentEquippedLevel );
                    if( !WobSettings.Get( "ScarDifficulty", "KeepSoulShop", false ) ) {
                        soulShopLevel.Value.SetEquippedLevel( 0, false, true );
                    }
                }
                Traverse<float> m_storedTemporaryMaxHealthMods = Traverse.Create( ChallengeManager.Instance ).Field<float>( "m_storedTemporaryMaxHealthMods" );
                m_storedTemporaryMaxHealthMods.Value = SaveManager.PlayerSaveData.TemporaryMaxHealthMods;
                SaveManager.PlayerSaveData.TemporaryMaxHealthMods = 0f;
                Traverse<Dictionary<HeirloomType, int>> m_storedHeirloomLevelDict = Traverse.Create( ChallengeManager.Instance ).Field<Dictionary<HeirloomType,int>>( "m_storedHeirloomLevelDict" );
                m_storedHeirloomLevelDict.Value.Clear();
                foreach( KeyValuePair<HeirloomType, int> heirloomLevel in SaveManager.PlayerSaveData.HeirloomLevelTable ) {
                    m_storedHeirloomLevelDict.Value.Add( heirloomLevel.Key, heirloomLevel.Value );
                }
                switch( ChallengeManager.ActiveChallenge.ChallengeType ) {
                    case ChallengeType.PlatformRanger:
                        SaveManager.PlayerSaveData.CurrentCharacter.Spell = AbilityType.BowWeapon;
                        break;
                    case ChallengeType.PlatformBoat:
                        SaveManager.PlayerSaveData.CurrentCharacter.Spell = AbilityType.FireballSpell;
                        break;
                    case ChallengeType.PlatformAxe:
                        SaveManager.PlayerSaveData.CurrentCharacter.Spell = AbilityType.StraightBoltSpell;
                        SaveManager.PlayerSaveData.ResetAllHeirlooms();
                        SaveManager.PlayerSaveData.SetHeirloomLevel( HeirloomType.UnlockAirDash, 1, false, false );
                        SaveManager.PlayerSaveData.SetHeirloomLevel( HeirloomType.UnlockMemory, 1, false, false );
                        break;
                    case ChallengeType.PlatformKatana:
                        SaveManager.PlayerSaveData.CurrentCharacter.Spell = AbilityType.LightningSpell;
                        break;
                    case ChallengeType.PlatformClimb:
                        SaveManager.PlayerSaveData.CurrentCharacter.Spell = AbilityType.GravityWellSpell;
                        break;
                    case ChallengeType.DragonAspectFight:
                        SaveManager.PlayerSaveData.CurrentCharacter.Weapon = AbilityType.DragonAspectWeapon;
                        SaveManager.PlayerSaveData.CurrentCharacter.Spell = AbilityType.AimLaserSpell;
                        if( !WobSettings.Get( "ScarDifficulty", "KeepTraits", false ) ) {
                            SaveManager.PlayerSaveData.CurrentCharacter.AntiqueOneOwned = RelicType.FreeHitRegenerate;
                        }
                        break;
                    case ChallengeType.PlatformSurf:
                        if( !WobSettings.Get( "ScarDifficulty", "KeepTraits", false ) ) {
                            SaveManager.PlayerSaveData.CurrentCharacter.TraitOne = TraitType.OneHitDeath;
                        }
                        SaveManager.PlayerSaveData.CurrentCharacter.Weapon = AbilityType.SurfboardWeapon;
                        SaveManager.PlayerSaveData.CurrentCharacter.Spell = AbilityType.LightningSpell;
                        break;
                }
                LineageWindowController.CharacterLoadedFromLineage = true;
                PlayerManager.GetPlayerController().ResetCharacter();
                LineageWindowController.CharacterLoadedFromLineage = false;
            }
        }

    }
}
