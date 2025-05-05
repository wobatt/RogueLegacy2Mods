using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace WobMod {
    internal class Traits {

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTING DEFAULTS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        public enum TraitName {
            Random,                  Aerodynamic,             Alexithymia,             Algesia,                 Antique,                 AssociativeAgnosia,      Bookish,                 CIP,                     Cartographer,            Charismatic,
			ChronicFatigueSyndrome,  Clownanthropy,           Clumsy,                  Colorblind,              Combative,               CompulsiveGambling,      CompulsiveHoarder,       Contrarian,              CripplingIntellect,      Disattuned,
			Diva,                    Dwarfism,                Dyspraxia,               Ectomorph,               EmotionalDysregularity,  Endomorph,               ExplodingCasketSyndrome, FMFFan,                  FND,                     Festive,
			Forgetful,               FunGuyFunGal,            Gigantism,               HeroComplex,             Histrionic,              HollowBones,             Hypercoagulation,        Hypergonadism,           IBS,                     IIBMuscleFibers,
			Interdimensional,        Kanganthropy,            Limitless,               Masochism,               Medium,                  Methemoglobinemia,       Minimalist,              MuscleWeakness,          Nature,                  Nostalgic,
			OneHitWonder,            Pacifist,                PanicAttacks,            Paranoid,                Perfectionist,           Puritan,                 Spelunker,               SuperIBS,                Superfluid,              Synesthesia,
			Vampirism,               Vegan,
        }

        private record TraitInfo( string Config, string Name, bool Spawn, int Gold, TraitName TraitName, bool AllowAlwaysActive );
		private static readonly Dictionary<TraitType, TraitInfo> traitInfo = new() {
            { TraitType.Antique,             new( "Antique",                 "Antique - Heir starts with a random relic.",                                                                                      true,  0,   TraitName.Antique,                 false ) },
            { TraitType.BlurOnHit,           new( "PanicAttacks",            "Panic Attacks/Stressed - Getting hit darkens the screen.",                                                                        true,  50,  TraitName.PanicAttacks,            false ) },
            { TraitType.BonusChestGold,      new( "CompulsiveGambling",      "Compulsive Gambling/Lootbox Addict - Only chests drop gold and chest values swing wildly!",                                       true,  50,  TraitName.CompulsiveGambling,      true  ) },
            { TraitType.BonusMagicStrength,  new( "CripplingIntellect",      "Crippling Intellect - -50% Health and Weapon Damage. Mana regenerates over time.",                                                true,  0,   TraitName.CripplingIntellect,      true  ) },
            { TraitType.BounceTerrain,       new( "Clownanthropy",           "Clownanthropy - -30% Health, but you can Spin Kick off terrain.",                                                                 true,  0,   TraitName.Clownanthropy,           true  ) },
            { TraitType.BreakPropsForMana,   new( "Minimalist",              "Minimalist/Breaker - Breaking things restores Mana.",                                                                             true,  0,   TraitName.Minimalist,              false ) },
            { TraitType.CanNowAttack,        new( "Pacifier",                "Pacifier - -60% Health and you love to fight!",                                                                                   false, 150, 0,                                 false ) },
            { TraitType.CantAttack,          new( "Pacifist",                "Pacifist - -60% Health and you can't deal damage.",                                                                               true,  150, TraitName.Pacifist,                false ) },
            { TraitType.CheerOnKills,        new( "Diva",                    "Diva - Everyone gets a spotlight but all eyes are on you.",                                                                       true,  75,  TraitName.Diva,                    false ) },
            { TraitType.ChristmasHoliday,    new( "Festive",                 "Festive - Hats are back. (No effect)",                                                                                            true,  0,   TraitName.Festive,                 false ) },
            { TraitType.CoinTimer,           new( "Forgetful",               "Forgetful - Dropped items will disappear after a short period.",                                                                  true,  25,  TraitName.Forgetful,               false ) },
            { TraitType.ColorTrails,         new( "Synesthesia",             "Synesthesia - Everything leaves behind color.",                                                                                   true,  25,  TraitName.Synesthesia,             false ) },
            { TraitType.DamageBoost,         new( "Combative",               "Combative - +50% Weapon Damage, -25% Health.",                                                                                    true,  0,   TraitName.Combative,               true  ) },
            { TraitType.DisarmOnHurt,        new( "FND",                     "FND/Shocked - Taking damage inflicts Disarmed, meaning the player cannot attack or use spells for 2 seconds.",                    true,  50,  TraitName.FND,                     true  ) },
            { TraitType.EasyBreakables,      new( "Clumsy",                  "Clumsy - Objects break on touch.",                                                                                                true,  0,   TraitName.Clumsy,                  false ) },
            { TraitType.EnemiesBlackFill,    new( "AssociativeAgnosia",      "Associative Agnosia - Enemies are blacked out.",                                                                                  true,  25,  TraitName.AssociativeAgnosia,      true  ) },
            { TraitType.EnemiesCensored,     new( "Puritan",                 "Puritan - Enemies are censored.",                                                                                                 true,  25,  TraitName.Puritan,                 false ) },
            { TraitType.EnemyKnockedFar,     new( "Hypergonadism",           "Hypergonadism - Enemies are knocked far away.",                                                                                   true,  0,   TraitName.Hypergonadism,           true  ) },
            { TraitType.EnemyKnockedLow,     new( "MuscleWeakness",          "Muscle Weakness - Enemies barely flinch when hit.",                                                                               true,  25,  TraitName.MuscleWeakness,          true  ) },
            { TraitType.ExplosiveChests,     new( "Paranoid",                "Paranoid/Explosive Chests - Chests drop an explosive surprise.",                                                                  true,  25,  TraitName.Paranoid,                true  ) },
            { TraitType.ExplosiveEnemies,    new( "ExplodingCasketSyndrome", "Exploding Casket Syndrome/Explosive Enemies - Enemies drop an explosive surprise.",                                               true,  50,  TraitName.ExplodingCasketSyndrome, true  ) },
            { TraitType.FakeSelfDamage,      new( "Histrionic",              "Histrionic - Numbers are exaggerated.",                                                                                           true,  0,   TraitName.Histrionic,               false ) },
            { TraitType.Fart,                new( "IBS",                     "IBS - Sometimes fart when jumping or dashing.",                                                                                   true,  0,   TraitName.IBS,                      false ) },
            { TraitType.FMFFan,              new( "FMFFan",                  "FMF Fan - You're probably Korean. (No effect)",                                                                                   true,  25,  TraitName.FMFFan,                  false ) },
            { TraitType.GainDownStrike,      new( "Aerodynamic",             "Aerodynamic - Your Spinkick is replaced with Downstrike.",                                                                        true,  0,   TraitName.Aerodynamic,              false ) },
            { TraitType.Disposition,         new( "Nature",                  "Nature - Being true to being you. (No effect)",                                                                                   true,  0,   TraitName.Nature,                  false ) },
            { TraitType.HalloweenHoliday,    new( "Medium",                  "Medium - Everything is spooky! (No effect)",                                                                                      true,  0,   TraitName.Medium,                  false ) },
            { TraitType.HighJump,            new( "IIBMuscleFibers",         "IIB Muscle Fibers/High Jumper - Hold [Jump] to Super Jump.",                                                                      true,  0,   TraitName.IIBMuscleFibers,          false ) },
            { TraitType.ItemsGoFlying,       new( "Dyspraxia",               "Dyspraxia/Butter Fingers - Items go flying!",                                                                                     true,  25,  TraitName.Dyspraxia,                false ) },
            { TraitType.LongerCD,            new( "ChronicFatigueSyndrome",  "Chronic Fatigue Syndrome/Exhausted - All Spells and Talents have a cooldown.",                                                    true,  25,  TraitName.ChronicFatigueSyndrome,   false ) },
            { TraitType.LowerGravity,        new( "HollowBones",             "Hollow Bones - You fall slowly.",                                                                                                 true,  0,   TraitName.HollowBones,              false ) },
            { TraitType.LowerStorePrice,     new( "Charismatic",             "Charismatic - 15% gold discount from all shopkeeps.",                                                                             true,  0,   TraitName.Charismatic,              false ) },
            { TraitType.MagicBoost,          new( "Bookish",                 "Bookish - +50% Magic Damage and +50 Mana Capacity. -25% Health.",                                                                 true,  0,   TraitName.Bookish,                  false ) },
            { TraitType.ManaCostAndDamageUp, new( "EmotionalDysregularity",  "Emotional Dysregularity/Overcompensation - Mana costs and spell damage are increased by 100%.",                                   true,  0,   TraitName.EmotionalDysregularity,   false ) },
            { TraitType.ManaFromHurt,        new( "Masochism",               "Masochism - Gain 50% of your mana when hit, but can't regain mana from attacks.",                                                 true,  25,  TraitName.Masochism,                false ) },
            { TraitType.MapReveal,           new( "Cartographer",            "Cartographer - Map is revealed but you have no position marker.",                                                                 true,  25,  TraitName.Cartographer,             false ) },
            { TraitType.MegaHealth,          new( "HeroComplex",             "Hero Complex - +100% Health but you can't heal, ever.",                                                                           true,  0,   TraitName.HeroComplex,              false ) },
            { TraitType.MushroomGrow,        new( "FunGuyFunGal",            "Fun-Guy/Gal - You really like mushrooms.",                                                                                        true,  0,   TraitName.FunGuyFunGal,             false ) },
            { TraitType.NoColor,             new( "Colorblind",              "Colorblind - You can't see colors.",                                                                                              true,  25,  TraitName.Colorblind,              false ) },
            { TraitType.NoEnemyHealthBar,    new( "Alexithymia",             "Alexithymia/Unempathetic - Can't see damage dealt.",                                                                              true,  25,  TraitName.Alexithymia,              false ) },
            { TraitType.NoHealthBar,         new( "CIP",                     "C.I.P - Can't see your health.",                                                                                                  true,  25,  TraitName.CIP,                      false ) },
            { TraitType.NoImmunityWindow,    new( "Algesia",                 "Algesia - No immunity window after taking damage.",                                                                               true,  50,  TraitName.Algesia,                  false ) },
            { TraitType.NoManaCap,           new( "Limitless",               "Limitless - Your Mana has infinite Overcharge. While Overcharged you take +50% more damage.",                                     true,  0,   TraitName.Limitless,                false ) },
            { TraitType.NoMeat,              new( "Vegan",                   "Vegan - Eating food hurts you.",                                                                                                  true,  75,  TraitName.Vegan,                    false ) },
            { TraitType.OldYellowTint,       new( "Nostalgic",               "Nostalgic - Everything is old-timey tinted.",                                                                                     true,  25,  TraitName.Nostalgic,                false ) },
            { TraitType.OmniDash,            new( "Superfluid",              "Superfluid - -20% Health, but you can dash in ANY direction.",                                                                    true,  0,   TraitName.Superfluid,               false ) },
            { TraitType.OneHitDeath,         new( "OneHitWonder",            "One-Hit Wonder/Fragile - You die in one hit.",                                                                                    true,  200, TraitName.OneHitWonder,             false ) },
            { TraitType.PlayerKnockedFar,    new( "Ectomorph",               "Ectomorph - Taking damage knocks you far away.",                                                                                  true,  25,  TraitName.Ectomorph,                false ) },
            { TraitType.PlayerKnockedLow,    new( "Endomorph",               "Endomorph - You barely flinch when enemies hit you.",                                                                             true,  0,   TraitName.Endomorph,                false ) },
            { TraitType.ProjectilesNoWalls,  new( "Interdimensional",        "Inter-dimensional - Most projectiles pass through walls.",                                                                        true,  50,  TraitName.Interdimensional,         false ) },
            { TraitType.RandomizeKit,        new( "Contrarian",              "Contrarian/Innovator - Your Weapon and Talent are randomized.",                                                                   true,  25,  TraitName.Contrarian,               false ) },
            { TraitType.RevealAllChests,     new( "Spelunker",               "Spelunker - -10% Health but you can see all chests on the map!",                                                                  true,  0,   TraitName.Spelunker,                false ) },
            { TraitType.SkillCritsOnly,      new( "Perfectionist",           "Perfectionist - Only Skill Crits and Spin Kicks deal damage.",                                                                    true,  50,  TraitName.Perfectionist,            false ) },
            { TraitType.SmallHitbox,         new( "Disattuned",              "Disattuned/Only Heart - -25% health, but you can only be hit in the heart.",                                                      true,  0,   TraitName.Disattuned,               false ) },
            { TraitType.SuperFart,           new( "SuperIBS",                "Super IBS - Super Fart Talent that releases a cloud that inflicts 3 seconds of Burn on enemies and launches the player upwards.", true,  0,   TraitName.SuperIBS,                 false ) },
            { TraitType.SuperHealer,         new( "Hypercoagulation",        "Hypercoagulation/Super Healer - HP regenerates, but you lose some Max HP when hit.",                                              true,  0,   TraitName.Hypercoagulation,         false ) },
            { TraitType.SuperSpinKick,       new( "Kanganthropy",            "Kanganthropy - Your Spin Kicks deal 150% more damage, but you bounce higher.",                                                    true,  0,   TraitName.Kanganthropy,             false ) },
            { TraitType.TwinRelics,          new( "CompulsiveHoarder",       "Compulsive Hoarder - All Relics are Twin Relics (when possible).",                                                                true,  0,   TraitName.CompulsiveHoarder,        false ) },
            { TraitType.Vampire,             new( "Vampirism",               "Vampirism - Gain 20% of your Weapon Damage as Health, but you take 125% more damage.",                                            true,  0,   TraitName.Vampirism,                false ) },
            { TraitType.YouAreBlue,          new( "Methemoglobinemia",       "Methemoglobinemia/Blue - You are blue. (No effect)",                                                                              true,  0,   TraitName.Methemoglobinemia,       false ) },
            { TraitType.YouAreLarge,         new( "Gigantism",               "Gigantism - You are gigantic.",                                                                                                   true,  25,  TraitName.Gigantism,               false ) },
            { TraitType.YouAreSmall,         new( "Dwarfism",                "Dwarfism - You are Tiny.",                                                                                                        true,  25,  TraitName.Dwarfism,                false ) },
        };

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTINGS READ/WRITE
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        internal static readonly WobSettings.KeyHelper<TraitType> traitKeys = new( "Trait" );

        internal static void RunSetup() {
            WobMod.configFiles.Add( "Traits", "Traits" );
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Num<float>(      WobMod.configFiles.Get( "Traits" ), "NewCharacters", "TraitChance1",      "Chance to generate first trait",                                                 Trait_EV.TRAIT_ODDS_OF_GETTING_FIRST_TRAIT  * 100f, 0.01f, bounds: (0f,  100f    ) ),
                new WobSettings.Num<float>(      WobMod.configFiles.Get( "Traits" ), "NewCharacters", "TraitChance2",      "Chance to generate second trait",                                                Trait_EV.TRAIT_ODDS_OF_GETTING_SECOND_TRAIT * 100f, 0.01f, bounds: (0f,  100f    ) ),
                new WobSettings.Num<float>(      WobMod.configFiles.Get( "Traits" ), "NewCharacters", "AntiqueChance",     "Additional chance for a trait to be an antique (even if disabled below)",        Trait_EV.ANTIQUE_CHANCE                     * 100f, 0.01f, bounds: (0f,  100f    ) ),
                new WobSettings.Enum<TraitName>( WobMod.configFiles.Get( "Traits" ), "NewCharacters", "TraitType1",        "Name of first trait to always use",                                              TraitName.Random                                                                   ),
                new WobSettings.Enum<TraitName>( WobMod.configFiles.Get( "Traits" ), "NewCharacters", "TraitType2",        "Name of second trait to always use",                                             TraitName.Random                                                                   ),
                new WobSettings.Boolean(         WobMod.configFiles.Get( "Traits" ), "NewCharacters", "PreventDuplicates", "Try to prevent each trait appearing more than once in each set of heir options", false                                                                              ),
                new WobSettings.Boolean(         WobMod.configFiles.Get( "Traits" ), "MiscSettings",  "TraitsKnown",       "Show effects of all unseen traits",                                              false                                                                              ),
            } );
            foreach( TraitType traitType in traitInfo.Keys ) {
                traitKeys.Add( traitType, traitInfo[traitType].Config );
                if( traitInfo[traitType].Spawn ) {
                    WobSettings.Add( new WobSettings.Boolean( WobMod.configFiles.Get( "Traits" ), traitKeys.Get( traitType, "Enabled" ), "Allow random spawn for " + traitInfo[traitType].Name, traitInfo[traitType].Spawn ) );
                }
                WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "Traits" ), traitKeys.Get( traitType, "GoldBonus" ), "Gold bonus for " + traitInfo[traitType].Name, traitInfo[traitType].Gold, 0.01f, bounds: (0, 1000000) ) );
                if( traitType != TraitType.CanNowAttack ) {
                    CharacterCreator_GetRandomTraits_Patch.traitIDPairs.Add( traitInfo[traitType].TraitName, traitType );
                }
            }
            WobSettings.Add( new WobSettings.Entry[] {
                // Positive health modifiers
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.MegaHealth,          "Health"       ), "Health modifier for " + traitInfo[TraitType.MegaHealth].Name,                   100,   0.01f, bounds: (-99, 1000000 ) ),
                // Negative health modifiers
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.BonusMagicStrength,  "Health"       ), "Health modifier for " + traitInfo[TraitType.BonusMagicStrength].Name,          -50,    0.01f, bounds: (-99, 1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.BounceTerrain,       "Health"       ), "Health modifier for " + traitInfo[TraitType.BounceTerrain].Name,               -30,    0.01f, bounds: (-99, 1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.CanNowAttack,        "Health"       ), "Health modifier for " + traitInfo[TraitType.CanNowAttack].Name,                -60,    0.01f, bounds: (-99, 1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.CantAttack,          "Health"       ), "Health modifier for " + traitInfo[TraitType.CantAttack].Name,                  -60,    0.01f, bounds: (-99, 1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.DamageBoost,         "Health"       ), "Health modifier for " + traitInfo[TraitType.DamageBoost].Name,                 -25,    0.01f, bounds: (-99, 1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.MagicBoost,          "Health"       ), "Health modifier for " + traitInfo[TraitType.MagicBoost].Name,                  -25,    0.01f, bounds: (-99, 1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.OmniDash,            "Health"       ), "Health modifier for " + traitInfo[TraitType.OmniDash].Name,                    -20,    0.01f, bounds: (-99, 1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.RevealAllChests,     "Health"       ), "Health modifier for " + traitInfo[TraitType.RevealAllChests].Name,             -10,    0.01f, bounds: (-99, 1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.SmallHitbox,         "Health"       ), "Health modifier for " + traitInfo[TraitType.SmallHitbox].Name,                 -25,    0.01f, bounds: (-99, 1000000 ) ),
                // Health loss per hit modifiers
                new WobSettings.Num<float>( WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.SuperHealer,         "LossPerHit"   ), "Max health percent lost per hit for " + traitInfo[TraitType.SuperHealer].Name,  6.25f, 0.01f, bounds: ( 0f, 100f    ) ),
                // Max mana modifiers
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.MagicBoost,          "MaxMana"      ), "Max mana modifier for " + traitInfo[TraitType.MagicBoost].Name,                 50,    0.01f, bounds: (-99, 1000000 ) ),
                // Mana from taking damage modifiers
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.ManaFromHurt,        "ManaRegen"    ), "Mana gain from damage for " + traitInfo[TraitType.ManaFromHurt].Name,           50,    0.01f, bounds: ( 0,  100     ) ),
                // Vampiric regen modifiers
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.Vampire,             "DamageRegen"  ), "Health from damage modifier for " + traitInfo[TraitType.Vampire].Name,          20,    0.01f, bounds: ( 0,  1000000 ) ),
                // Damage taken modifiers
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.NoManaCap,           "DamageTaken"  ), "Damage taken modifier for " + traitInfo[TraitType.NoManaCap].Name,              50,    0.01f, bounds: (-99, 1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.Vampire,             "DamageTaken"  ), "Damage taken modifier for " + traitInfo[TraitType.Vampire].Name,                125,   0.01f, bounds: (-99, 1000000 ) ),
                // Weapon damage modifiers
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.BonusMagicStrength,  "WeaponDamage" ), "Weapon damage modifier for " + traitInfo[TraitType.BonusMagicStrength].Name,   -50,    0.01f, bounds: (-99, 1000000 ) ),
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.DamageBoost,         "WeaponDamage" ), "Weapon damage modifier for " + traitInfo[TraitType.DamageBoost].Name,           50,    0.01f, bounds: (-99, 1000000 ) ),
                // Magic damage modifiers
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.MagicBoost,          "MagicDamage"  ), "Magic damage modifier for " + traitInfo[TraitType.MagicBoost].Name,             50,    0.01f, bounds: (-99, 1000000 ) ),
                // Spell damage modifiers
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.ManaCostAndDamageUp, "SpellDamage"  ), "Spell damage modifier for " + traitInfo[TraitType.ManaCostAndDamageUp].Name,    100,   0.01f, bounds: (-99, 1000000 ) ),
                // Spell cost modifiers
                new WobSettings.Num<int>(   WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.ManaCostAndDamageUp, "SpellCost"    ), "Spell cost modifier for " + traitInfo[TraitType.ManaCostAndDamageUp].Name,      100,   0.01f, bounds: (-99, 1000000 ) ),
                // Disarm time
                new WobSettings.Num<float>( WobMod.configFiles.Get( "Traits" ), traitKeys.Get( TraitType.DisarmOnHurt,        "DisarmTime"   ), "Seconds of being disarmed for " + traitInfo[TraitType.DisarmOnHurt].Name,       2f,           bounds: ( 0f, 60f     ) ),
            } );
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - SEEN STATE
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        [HarmonyPatch( typeof( TraitManager ), nameof( TraitManager.GetTraitSeenState ) )]
        internal static class TraitManager_GetTraitFoundState_Patch {
            internal static void Postfix( TraitType traitType, ref TraitSeenState __result ) {
                if( __result < TraitSeenState.SeenTwice && WobSettings.Get( "MiscSettings", "TraitsKnown", false ) ) {
                    TraitManager.SetTraitSeenState( traitType, TraitSeenState.SeenTwice, false );
                    __result = TraitSeenState.SeenTwice;
                }
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - NEW CHARACTERS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        [HarmonyPatch( typeof( CharacterCreator ), nameof( CharacterCreator.GenerateRandomCharacter ) )]
        internal static class CharacterCreator_GenerateRandomCharacter_Patch {
            internal static void Prefix( ref bool forceRandomizeKit ) {
                WobPlugin.Log( "[Abilities] GenerateRandomCharacter.Prefix called" );
                if( WobSettings.Get( SoulShop.soulShopKeys.Get( SoulShopType.ForceRandomizeKit, "AllSlots" ), false ) ) {
                    SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj( SoulShopType.ForceRandomizeKit );
                    if( soulShopObj != null && soulShopObj.CurrentEquippedLevel > 0 ) {
                        WobPlugin.Log( "[SoulShop] CharacterCreator.GenerateRandomCharacter: Setting forceRandomizeKit to false" );
                        forceRandomizeKit = false; // This flag forces the trait to be added - we are randomising without the trait, so override to false
                    }
                }
            }

            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "CharacterCreator.GenerateRandomCharacter" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldloc_1                        ), // characterData
                        /*  1 */ new( OpCodes.Ldfld, name: "TraitOne"        ), // characterData.TraitOne
                        /*  2 */ new( OpCodes.Ldc_I4, TraitType.RandomizeKit ), // TraitType.RandomizeKit   990
                        /*  3 */ new( OpCodeSet.Beq                          ), // if (characterData.TraitOne == TraitType.RandomizeKit || 
                        /*  4 */ new( OpCodes.Ldloc_1                        ), // characterData
                        /*  5 */ new( OpCodes.Ldfld, name: "TraitTwo"        ), // characterData.TraitTwo
                        /*  6 */ new( OpCodes.Ldc_I4, TraitType.RandomizeKit ), // TraitType.RandomizeKit   990
                        /*  7 */ new( OpCodeSet.Bne_Un                       ), // if (characterData.TraitTwo == TraitType.RandomizeKit)
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetInstruction( 1, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => CheckRandomizeKit( null ) ) ),
                        new WobTranspiler.OpAction_Remove( 3, 4 ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            // Original method will compare the return value of this to TraitType.RandomizeKit, and apply the randomize if they are equal
            private static TraitType CheckRandomizeKit( CharacterData characterData ) {
                WobPlugin.Log( "[Abilities] CheckRandomizeKit called" );
                if( WobSettings.Get( SoulShop.soulShopKeys.Get( SoulShopType.ForceRandomizeKit, "AllSlots" ), false ) ) {
                    SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj( SoulShopType.ForceRandomizeKit );
                    if( !soulShopObj.IsNativeNull() && soulShopObj.CurrentEquippedLevel > 0 ) {
                        WobPlugin.Log( "[SoulShop] CharacterCreator.GenerateRandomCharacter: Randomizing kit" );
                        return TraitType.RandomizeKit;
                    }
                }
                // Either the setting is not enabled or item is switched off - use the standard condition of checking the character's traits
                return ( characterData.TraitOne == TraitType.RandomizeKit || characterData.TraitTwo == TraitType.RandomizeKit ) ? TraitType.RandomizeKit : TraitType.None;
            }
        }

        [HarmonyPatch( typeof( LineageWindowController ), "CreateRandomCharacters" )]
        internal static class LineageWindowController_CreateRandomCharacters_Patch {
            // While a new set of heirs is being generated, traits used are tracked to prevent duplicates - this patch reads settings
            private static bool runOnce = false;
            internal static void Prefix() {
                WobPlugin.Log( "[Abilities] CreateRandomCharacters.Prefix called" );
                if( !runOnce ) {
                    CharacterCreator_GetRandomTraits_Patch.LoadSettings();
                    runOnce = true;
                }
            }

            // While a new set of heirs is being generated, traits used are tracked to prevent duplicates - this patch clears the list after creating a set of heirs
            internal static void Postfix() {
                WobPlugin.Log( "[Traits] Clear used traits; " + CharacterCreator_GetRandomTraits_Patch.usedTraits.Count );
                CharacterCreator_GetRandomTraits_Patch.usedTraits.Clear();
            }
        }

        // New implmentation of GetRandomTraits as a total replacement of the original, allowing control of trait spawn
        [HarmonyPatch( typeof( CharacterCreator ), nameof( CharacterCreator.GetRandomTraits ) )]
        internal static class CharacterCreator_GetRandomTraits_Patch {
            private static readonly TraitType[] fixedTraits = new TraitType[2];
            private static bool allSlotsContrarian = false;
            private static readonly float[] spawnChance = new float[3];
            private static bool preventDuplicates = false;
            internal static List<TraitType> usedTraits = new();
            // Dictionary to translate from the name in config to the game's trait type enum
            internal static Dictionary<TraitName, TraitType> traitIDPairs = new();

            internal static void LoadSettings() {
                // Initialise the traits to those in the config file, or 'TraitType.None' if we should randomly generate
                fixedTraits[0] = DecodeTraitSetting( "TraitType1" );
                fixedTraits[1] = DecodeTraitSetting( "TraitType2" );
                // Make sure that the traits are different
                if( fixedTraits[0] == fixedTraits[1] && fixedTraits[0] != TraitType.None ) {
                    WobPlugin.Log( "[Traits] CONFIG ERROR: Traits in config file should not be the same - " + fixedTraits[0] + ", " + fixedTraits[1], WobPlugin.ERROR );
                    fixedTraits[1] = TraitType.None;
                }
                // Make sure the traits are compatible
                if( !CharacterCreator_AreTraitsCompatible( fixedTraits[0], fixedTraits[1] ) ) {
                    WobPlugin.Log( "[Traits] CONFIG ERROR: Traits in config file are not compatible - " + fixedTraits[0] + ", " + fixedTraits[1], WobPlugin.ERROR );
                    fixedTraits[1] = TraitType.None;
                }
                // Check for guaranteed spawn of contrarian trait from parameter, all slot lock setting, or fixed spawn settings
                allSlotsContrarian = WobSettings.Get( SoulShop.soulShopKeys.Get( SoulShopType.ForceRandomizeKit, "AllSlots" ), false );
                // Get the trait roll chances from config
                spawnChance[0] = WobSettings.Get( "NewCharacters", "TraitChance1", Trait_EV.TRAIT_ODDS_OF_GETTING_FIRST_TRAIT );
                spawnChance[1] = WobSettings.Get( "NewCharacters", "TraitChance2", Trait_EV.TRAIT_ODDS_OF_GETTING_SECOND_TRAIT );
                spawnChance[2] = WobSettings.Get( "NewCharacters", "AntiqueChance", Trait_EV.ANTIQUE_CHANCE );
                // Check if we should attempt to prevent duplicate traits on a set of heir options
                preventDuplicates = WobSettings.Get( "NewCharacters", "PreventDuplicates", false );
            }

            internal static bool Prefix( bool forceRandomizeKit, ref Vector2Int __result ) {
                WobPlugin.Log( "[Abilities] GetRandomTraits.Prefix called" );
                //WobPlugin.Log( "[Traits] GetRandomTraits Prefix called" );
                __result = NewGetRandomTraits( forceRandomizeKit );
                // Do not run the original method
                return false;
            }

            private static Vector2Int NewGetRandomTraits( bool forceRandomizeKit ) {
                // Initialise the traits to those in the config file, or 'TraitType.None' if we should randomly generate
                TraitType[] traitTypes = (TraitType[])fixedTraits.Clone();
                // List for the available traits to choose from
                List<TraitType> traitTypesList = GetAllowedTraits();
                // Check for guaranteed spawn of contrarian trait from parameter, all slot lock setting, or fixed spawn settings
                if( allSlotsContrarian ) {
                    // Remove the trait from the fixed spawn items
                    if( traitTypes[0] == TraitType.RandomizeKit ) { traitTypes[0] = TraitType.None; }
                    if( traitTypes[1] == TraitType.RandomizeKit ) { traitTypes[1] = TraitType.None; }
                } else {
                    // Add fixed spawn of contrarian trait
                    if( forceRandomizeKit ) { traitTypes[0] = TraitType.RandomizeKit; }
                }
                // Do for each of the 2 trait slots
                for( int j = 0; j < 2; j++ ) {
                    // We only want to randomly generate a trait if the slot is empty
                    if( traitTypes[j] == TraitType.None ) {
                        WobPlugin.Log( "[Traits] About to roll for trait " + j + ", traits currently are " + traitTypes[0] + ", " + traitTypes[1] );
                        float roll = RNGManager.GetRandomNumber( RngID.Lineage, "GetRandomTraitSpawnChance", 0f, 1f );
                        WobPlugin.Log( "[Traits] Roll is " + roll + ", comparator is " + spawnChance[j] + ", so " + ( roll <= spawnChance[j] ) );
                        // Roll for whether to generate a trait
                        if( roll <= spawnChance[j] ) {
                            WobPlugin.Log( "[Traits] About to roll for antique" );
                            float rollA = RNGManager.GetRandomNumber( RngID.Lineage, "GetAntiqueSpawnChance", 0f, 1f );
                            WobPlugin.Log( "[Traits] Roll is " + rollA + ", comparator is " + spawnChance[2] + ", so " + ( rollA <= spawnChance[2] ) );
                            // Roll for whether the spawned trait should be an antique
                            if( rollA < spawnChance[2] ) {
                                traitTypes[j] = TraitType.Antique;
                            } else {
                                // Check if random traits are disabled in House Rules
                                if( !( SaveManager.PlayerSaveData.EnableHouseRules && SaveManager.PlayerSaveData.Assist_DisableTraits ) ) {
                                    // Generate a random trait that is compatible with the trait in the other slot
                                    traitTypes[j] = GetCompatibleTrait( traitTypesList, traitTypes[( j == 0 ? 1 : 0 )] );
                                    WobPlugin.Log( "[Traits] Trait " + j + " is " + traitTypes[j] );
                                }
                            }
                        }
                    }
                }
                // If only the second slot is a trait, move it to the first
                if( traitTypes[0] == TraitType.None ) {
                    (traitTypes[0], traitTypes[1]) = (traitTypes[1], traitTypes[0]);
                }
                // Antique should always be displayed last
                if( traitTypes[0] == TraitType.Antique && traitTypes[1] != TraitType.Antique && traitTypes[1] != TraitType.None ) {
                    (traitTypes[0], traitTypes[1]) = (traitTypes[1], traitTypes[0]);
                }
                // Check if we should attempt to prevent duplicate traits on a set of heir options
                if( preventDuplicates ) {
                    // Add the new traits to the list so it won't be selected again
                    if( traitTypes[0] is not TraitType.None and not TraitType.Antique ) { usedTraits.Add( traitTypes[0] ); }
                    if( traitTypes[1] is not TraitType.None and not TraitType.Antique ) { usedTraits.Add( traitTypes[1] ); }
                }
                WobPlugin.Log( "[Traits] GetRandomTraits complete, returning " + traitTypes[0] + ", " + traitTypes[1] );
                // Return the traits
                return new Vector2Int( (int)traitTypes[0], (int)traitTypes[1] );
            }

            // Get a list of the traits that can be randomly added to an heir
            private static List<TraitType> GetAllowedTraits() {
                // List for the available traits to choose from
                List<TraitType> traitTypesList = new();
                // Go through all of traits in the game
                foreach( TraitType traitType in TraitType_RL.TypeArray ) {
                    // Exclude the non-trait and special event traits
                    if( traitType == TraitType.None ||
                            ( traitType == TraitType.HalloweenHoliday && !HolidayLookController.IsHoliday( HolidayType.Halloween ) ) ||
                            ( traitType == TraitType.ChristmasHoliday && !HolidayLookController.IsHoliday( HolidayType.Christmas ) ) ||
                            ( SpecialMode_EV.TRAIT_EXCEPTION_ARRAY.Contains( traitType ) && ( SaveManager.PlayerSaveData.SpecialModeType > SpecialModeType.None ) ) ||
                            usedTraits.Contains( traitType ) ) {
                        WobPlugin.Log( "[Traits] Excluding trait " + traitType );
                    } else {
                        // Get the data holding rarity info for the trait
                        TraitData traitData = TraitLibrary.GetTraitData( traitType );
                        // Only use traits with rarity of 1
                        if( traitData != null && traitData.Rarity == 1 ) {
                            // Add the trait to the list for random generation
                            traitTypesList.Add( traitType );
                        }
                        // The original method also defines lists for rarities 2 and 3, and rolls for which to use, but the range of the rolled value means it only ever selects the list for rarity 1 - I have left this out as redundant
                    }
                }
                // If the lock contrarian setting is enabled, it takes effect regardless of the trait being on a character, so disable spawn
                if( WobSettings.Get( SoulShop.soulShopKeys.Get( SoulShopType.ForceRandomizeKit, "AllSlots" ), false ) ) {
                    traitTypesList.Remove( TraitType.RandomizeKit );
                }
                WobPlugin.Log( "[Traits] Trait array contains " + traitTypesList.Count );
                return traitTypesList;
            }

            // Randomly generate a trait that is compatible with an existing trait (all traits are compatible with TraitType.None)
            private static TraitType GetCompatibleTrait( List<TraitType> traitTypesList, TraitType existingTrait ) {
                // Variable for the return value
                TraitType newTrait = TraitType.None;
                // Keep track of the number of attempts to prevent an infinite loop
                int attempts = 0;
                // Continue generating new traits until one is found or we run out of attempts
                while( newTrait == TraitType.None && attempts < 100 ) {
                    // Roll for the trait index, and get it from the list
                    newTrait = traitTypesList[RNGManager.GetRandomNumber( RngID.Lineage, "GetRandomTraitChoiceRoll - Trait #?", 0, traitTypesList.Count )];
                    if( newTrait != TraitType.None ) {
                        // Check that the traits are different and are compatible
                        if( newTrait == existingTrait || !CharacterCreator_AreTraitsCompatible( existingTrait, newTrait ) ) {
                            // Reset the trait to None if the combination is invalid
                            newTrait = TraitType.None;
                        }
                        // Record the attempt
                        attempts++;
                    }
                }
                // Return the new trait, or 'None' if no compatible trait could be found in 100 attempts
                return newTrait;
            }

            // Method to access private method CharacterCreator.AreTraitsCompatible
            private static bool CharacterCreator_AreTraitsCompatible( TraitType traitType1, TraitType traitType2 ) {
                return Traverse.Create( typeof( CharacterCreator ) ).Method( "AreTraitsCompatible", new System.Type[] { typeof( TraitType ), typeof( TraitType ) } ).GetValue<bool>( new object[] { traitType1, traitType2 } );
            }

            // Method to fetch the config value, and translate it to a TraitType
            private static TraitType DecodeTraitSetting( string settingName ) {
                // Variable for the return value, with default of 'None', which means randomly generate
                TraitType traitType = TraitType.None;
                // Get the value from config
                TraitName traitName = WobSettings.Get( "NewCharacters", settingName, TraitName.Random );
                // Only need to change the trait type if it isn't randomly generated
                if( traitName != TraitName.Random ) {
                    // Get the trait type for the name
                    if( !traitIDPairs.TryGetValue( traitName, out traitType ) ) {
                        WobPlugin.Log( "[Traits] WARNING: Could not find TraitType for " + traitName );
                        traitType = TraitType.None;
                    }
                }
                // Return the fixed spawn, or 'None' for random generation
                return traitType;
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - GENERAL
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Enable or disable a trait for random generation
        [HarmonyPatch( typeof( TraitType_RL ), nameof( TraitType_RL.TypeArray ), MethodType.Getter )]
        internal static class TraitType_RL_TypeArray_Patch {
            private static bool runOnce = false;
            internal static void Prefix() {
                // Only need to run this once, as the new settings are written into the trait data for the session
                if( !runOnce ) {
                    // Get the list of traits from the private field
                    TraitType[] m_typeArray = (TraitType[])Traverse.Create( typeof( TraitType_RL ) ).Field( "m_typeArray" ).GetValue();
                    // Go through each type in the array
                    foreach( TraitType traitType in m_typeArray ) {
                        //WobPlugin.Log( "[Traits] Checking trait " + traitType );
                        // Get the trait data that includes rarity info
                        TraitData traitData = TraitLibrary.GetTraitData( traitType );
                        if( traitData != null ) {
                            // Check that the rarity is within the range looked at during character generation
                            // Get the value of the setting that has the same name as the internal name of the trait
                            if( ( traitData.Rarity >= 1 && traitData.Rarity <= 3 ) && !WobSettings.Get( traitKeys.Get( traitType, "Enabled" ), true ) ) {
                                // The game seems to use values of 91, 92 and 93 for the rarity of diabled traits, so I will stick to this convention, though any value > 3 would work
                                traitData.Rarity += 90;
                                WobPlugin.Log( "[Traits] Banning trait " + traitType );
                            }
                        }
                    }
                    runOnce = true;
                }
            }
        }

        // Patch for the method that gets the gold increase for a trait
        [HarmonyPatch( typeof( TraitManager ), nameof( TraitManager.GetActualTraitGoldGain ) )]
        internal static class TraitManager_GetActualTraitGoldGain_Patch {
            internal static void Prefix( TraitType traitType ) {
                // Get the data for the trait being looked at - this is from the original method parameter
                TraitData traitData = TraitLibrary.GetTraitData( traitType );
                if( traitData != null ) {
                    // Get the value of the setting that has the same name as the internal name of the trait
                    float goldBonus = WobSettings.Get( traitKeys.Get( traitType, "GoldBonus" ), traitData.GoldBonus );
                    if( goldBonus != traitData.GoldBonus ) {
                        WobPlugin.Log( "[Traits] Changing gold bonus for " + traitType + " from " + traitData.GoldBonus + " to " + goldBonus );
                        traitData.GoldBonus = goldBonus;
                    }
                }
            }

            //internal static void Postfix( TraitType traitType, ref float __result ) {
            //    // Check that traits should be giving gold
            //    if( SkillTreeManager.GetSkillObjLevel( SkillTreeType.Traits_Give_Gold ) > 0 ) {
            //        // Get the data for the trait being looked at - this is from the original method parameter
            //        TraitData traitData = TraitLibrary.GetTraitData( traitType );
            //        if( traitData != null ) {
            //            // Get the value of the setting that has the same name as the internal name of the trait
            //            float goldBonus = WobSettings.Get( traitKeys.Get( traitType, "GoldBonus" ), traitData.GoldBonus );
            //            if( goldBonus != traitData.GoldBonus ) {
            //                WobPlugin.Log( "[Traits] Changing bonus for " + traitData.Name + " from " + traitData.GoldBonus + " to " + goldBonus );
            //                traitData.GoldBonus = goldBonus;
            //                // If a matching config setting has been found, calculate the new gold gain using the file value rather than the game value, and overwite the method return value
            //                __result = goldBonus * ( 1f + SkillTreeManager.GetSkillTreeObj( SkillTreeType.Traits_Give_Gold_Gain_Mod ).CurrentStatGain );
            //            //} else {
            //                //WobPlugin.Log( "[Traits] Same bonus for " + traitData.Name + " of " + traitData.GoldBonus );
            //            }
            //        }
            //    }
            //    // If any of the 'if' statements fail, don't change the return value that the original method decided on
            //}
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - STAT SPECIFIC
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Method that checks if a trait is active on the current heir, and gets the new modifier from settings if it is
        private static float GetActiveMod( TraitType traitType, string modType, float defaultMod ) {
            float modifier = 0f;
            if( TraitManager.IsTraitActive( traitType ) ) {
                modifier = WobSettings.Get( traitKeys.Get( traitType, modType ), defaultMod );
                if( modifier != defaultMod ) {
                    WobPlugin.Log( "[Traits] Changing " + traitType + " " + modType + " mod to " + modifier );
                }
            }
            return modifier;
        }

        // Apply max health modifiers
        [HarmonyPatch( typeof( PlayerController ), "InitializeTraitHealthMods" )]
        internal static class PlayerController_InitializeTraitHealthMods_Patch {
            internal static void Postfix( PlayerController __instance ) {
                float healthMod = 0f;
                // I have no idea what this trait is, but it is in the original method so I'm including it here
                if( TraitManager.IsTraitActive( TraitType.BonusHealth ) ) { healthMod += 0.1f; }
                // Unused trait
                if( TraitManager.IsTraitActive( TraitType.InvulnDash ) ) { healthMod += -0.5f; }
                // Positive modifiers
                healthMod += GetActiveMod( TraitType.MegaHealth, "Health", 1f );
                // Negative modifiers
                healthMod += GetActiveMod( TraitType.BonusMagicStrength, "Health", -0.5f );
                healthMod += GetActiveMod( TraitType.BounceTerrain, "Health", -0.3f );
                healthMod += GetActiveMod( TraitType.CanNowAttack, "Health", -0.6f );
                healthMod += GetActiveMod( TraitType.CantAttack, "Health", -0.6f );
                healthMod += GetActiveMod( TraitType.DamageBoost, "Health", -0.25f );
                healthMod += GetActiveMod( TraitType.MagicBoost, "Health", -0.25f );
                healthMod += GetActiveMod( TraitType.OmniDash, "Health", -0.2f );
                healthMod += GetActiveMod( TraitType.RevealAllChests, "Health", -0.1f );
                healthMod += GetActiveMod( TraitType.SmallHitbox, "Health", -0.25f );
                // Return the calculated value, overriding the original method
                __instance.TraitMaxHealthMod = healthMod;
            }
        }

        // Apply max mana modifiers
        [HarmonyPatch( typeof( PlayerController ), "InitializeTraitMaxManaMods" )]
        internal static class PlayerController_InitializeTraitMaxManaMods_Patch {
            internal static void Postfix( PlayerController __instance ) {
                float manaMod = 0f;
                // Positive modifiers
                manaMod += GetActiveMod( TraitType.MagicBoost, "MaxMana", 0.5f );
                // Return the calculated value, overriding the original method
                __instance.TraitMaxManaMod = manaMod;
            }
        }

        // Apply disarm time
        [HarmonyPatch( typeof( StatusEffectController ), nameof( StatusEffectController.StartStatusEffect ) )]
        internal static class StatusEffectController_StartStatusEffect_Patch {
            internal static bool Prefix( StatusEffectType statusEffectType, ref float duration, IDamageObj caster ) {
                if( statusEffectType == StatusEffectType.Player_Disarmed && caster == null ) {
                    duration = GetActiveMod( TraitType.DisarmOnHurt, "DisarmTime", 2f );
                    return duration > 0;
                }
                return true;
            }
        }

        // Apply weapon and magic damage modifiers
        [HarmonyPatch( typeof( ProjectileManager ), nameof( ProjectileManager.CalculateStrengthDamageMod ) )]
        internal static class ProjectileManager_CalculateStrengthDamageMod_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "ProjectileManager.CalculateStrengthDamageMod" );
                // Perform the patching for weapon damage modifiers
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldc_I4, TraitType.BonusMagicStrength ), // TraitType.BonusMagicStrength
                        /*  1 */ new( OpCodes.Call, name: "IsTraitActive"          ), // TraitManager.IsTraitActive(TraitType.BonusMagicStrength)
                        /*  2 */ new( OpCodeSet.Brfalse                            ), // if (TraitManager.IsTraitActive(TraitType.BonusMagicStrength))
                        /*  3 */ new( OpCodeSet.Ldloc                              ), // num
                        /*  4 */ new( OpCodes.Ldc_R4                               ), // -0.5f
                        /*  5 */ new( OpCodes.Add                                  ), // num + -0.5f
                        /*  6 */ new( OpCodeSet.Stloc                              ), // num = num + -0.5f

                        /*  7 */ new( OpCodes.Ldc_I4, TraitType.DamageBoost        ), // TraitType.DamageBoost
                        /*  8 */ new( OpCodes.Call, name: "IsTraitActive"          ), // TraitManager.IsTraitActive(TraitType.DamageBoost)
                        /*  9 */ new( OpCodeSet.Brfalse                            ), // if (TraitManager.IsTraitActive(TraitType.DamageBoost))
                        /* 10 */ new( OpCodeSet.Ldloc                              ), // num
                        /* 11 */ new( OpCodes.Ldc_R4                               ), // 0.5f
                        /* 12 */ new( OpCodes.Add                                  ), // num + 0.5f
                        /* 13 */ new( OpCodeSet.Stloc                              ), // num = num + 0.5f
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 4,  WobSettings.Get( traitKeys.Get( TraitType.BonusMagicStrength, "WeaponDamage" ), -0.5f ) ),
                        new WobTranspiler.OpAction_SetOperand( 11, WobSettings.Get( traitKeys.Get( TraitType.DamageBoost,        "WeaponDamage" ),  0.5f ) ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Apply weapon and magic damage modifiers
        [HarmonyPatch( typeof( ProjectileManager ), nameof( ProjectileManager.CalculateMagicDamageMod ) )]
        internal static class ProjectileManager_CalculateMagicDamageMod_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "ProjectileManager.CalculateMagicDamageMod" );
                // Perform the patching for magic damage modifiers
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldc_I4, TraitType.BonusMagicStrength ), // TraitType.BonusMagicStrength
                        /*  1 */ new( OpCodes.Call, name: "IsTraitActive"          ), // TraitManager.IsTraitActive(TraitType.BonusMagicStrength)
                        /*  2 */ new( OpCodeSet.Brfalse                            ), // if (TraitManager.IsTraitActive(TraitType.BonusMagicStrength))
                        /*  3 */ new( OpCodeSet.Ldloc                              ), // num
                        /*  4 */ new( OpCodes.Ldc_R4                               ), // 0.0f
                        /*  5 */ new( OpCodes.Add                                  ), // num + 0.0f
                        /*  6 */ new( OpCodeSet.Stloc                              ), // num = num + 0.0f

                        /*  7 */ new( OpCodes.Ldc_I4, TraitType.MagicBoost         ), // TraitType.MagicBoost
                        /*  8 */ new( OpCodes.Call, name: "IsTraitActive"          ), // TraitManager.IsTraitActive(TraitType.MagicBoost)
                        /*  9 */ new( OpCodeSet.Brfalse                            ), // if (TraitManager.IsTraitActive(TraitType.MagicBoost))
                        /* 10 */ new( OpCodeSet.Ldloc                              ), // num
                        /* 11 */ new( OpCodes.Ldc_R4                               ), // 0.5f
                        /* 12 */ new( OpCodes.Add                                  ), // num + 0.5f
                        /* 13 */ new( OpCodeSet.Stloc                              ), // num = num + 0.5f
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        //new WobTranspiler.OpAction_SetOperand( 4,  WobSettings.Get( keys.Get( TraitType.BonusMagicStrength, "MagicDamage" ), 0.0f ) ),
                        new WobTranspiler.OpAction_SetOperand( 11, WobSettings.Get( traitKeys.Get( TraitType.MagicBoost, "MagicDamage" ), 0.5f ) ),
                    }, expected: 1 );
                // Perform the patching for spell damage modifiers
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldc_I4, TraitType.ManaCostAndDamageUp ), // TraitType.ManaCostAndDamageUp
                        /*  1 */ new( OpCodes.Call, name: "IsTraitActive"           ), // TraitManager.IsTraitActive(TraitType.ManaCostAndDamageUp)
                        /*  2 */ new( OpCodeSet.Brfalse                             ), // if (TraitManager.IsTraitActive(TraitType.ManaCostAndDamageUp))

                        /*  3 */ new( OpCodeSet.Ldarg                               ), // projectile
                        /*  4 */ new( OpCodes.Ldstr                                 ), // "PlayerProjectile"
                        /*  5 */ new( OpCodes.Callvirt                              ), // projectile.CompareTag("PlayerProjectile")
                        /*  6 */ new( OpCodeSet.Brfalse                             ), // if (projectile.CompareTag("PlayerProjectile"))

                        /*  7 */ new( OpCodeSet.Ldarg                               ), // playerController
                        /*  8 */ new( OpCodes.Callvirt                              ), // playerController.CastAbility
                        /*  9 */ new( OpCodeSet.Ldloc                               ), // lastCastAbilityTypeCasted
                        /* 10 */ new( OpCodeSet.Ldc_I4                              ), // false
                        /* 11 */ new( OpCodes.Callvirt                              ), // playerController.CastAbility.GetAbility(lastCastAbilityTypeCasted)
                        /* 12 */ new( OpCodeSet.Stloc                               ), // BaseAbility_RL ability = playerController.CastAbility.GetAbility(lastCastAbilityTypeCasted)
                            
                        /* 13 */ new( OpCodeSet.Ldloc                               ), // ability
                        /* 14 */ new( OpCodes.Call                                  ), // (bool)ability
                        /* 15 */ new( OpCodeSet.Brfalse                             ), // if ((bool)ability)

                        /* 16 */ new( OpCodeSet.Ldloc                               ), // ability
                        /* 17 */ new( OpCodes.Callvirt                              ), // ability.BaseCost
                        /* 18 */ new( OpCodeSet.Ldc_I4                              ), // 0
                        /* 19 */ new( OpCodeSet.Ble                                 ), // if (ability.BaseCost > 0)

                        /* 20 */ new( OpCodeSet.Ldloc                               ), // num
                        /* 21 */ new( OpCodes.Ldc_R4                                ), // 1f
                        /* 22 */ new( OpCodes.Add                                   ), // num + 1f
                        /* 23 */ new( OpCodeSet.Stloc                               ), // num = num + 1f
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 21, WobSettings.Get( traitKeys.Get( TraitType.ManaCostAndDamageUp, "SpellDamage" ), 1f ) ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Apply damage taken modifiers
        [HarmonyPatch( typeof( EnemyHitResponse ), "CharacterDamaged" )]
        internal static class EnemyHitResponse_CharacterDamaged_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "EnemyHitResponse.CharacterDamaged" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        // if (TraitManager.IsTraitActive(TraitType.Vampire) && (this.m_enemyController.EnemyType != EnemyType.Dummy || (this.m_enemyController.EnemyType == EnemyType.Dummy && this.m_enemyController.EnemyRank == EnemyRank.Miniboss)) && this.m_enemyController.EnemyType != EnemyType.Eggplant)
                        /*  0 */ new( OpCodes.Ldc_I4, TraitType.Vampire   ), // TraitType.Vampire
                        /*  1 */ new( OpCodes.Call, name: "IsTraitActive" ), // TraitManager.IsTraitActive(TraitType.Vampire)
                        /*  2 */ new( OpCodeSet.Brfalse                   ), // if (TraitManager.IsTraitActive(TraitType.Vampire))

                        /*  3 */ new( OpCodeSet.Ldarg                     ), // this
                        /*  4 */ new( OpCodes.Ldfld                       ), // this.m_enemyController
                        /*  5 */ new( OpCodes.Callvirt                    ), // this.m_enemyController.EnemyType
                        /*  6 */ new( OpCodeSet.Ldc_I4                    ), // EnemyType.Dummy
                        /*  7 */ new( OpCodeSet.Bne_Un                    ), // this.m_enemyController.EnemyType != EnemyType.Dummy

                        /*  8 */ new( OpCodeSet.Ldarg                     ), // this
                        /*  9 */ new( OpCodes.Ldfld                       ), // this.m_enemyController
                        /* 10 */ new( OpCodes.Callvirt                    ), // this.m_enemyController.EnemyType
                        /* 11 */ new( OpCodeSet.Ldc_I4                    ), // EnemyType.Dummy
                        /* 12 */ new( OpCodeSet.Bne_Un                    ), // this.m_enemyController.EnemyType == EnemyType.Dummy

                        /* 13 */ new( OpCodeSet.Ldarg                     ), // this
                        /* 14 */ new( OpCodes.Ldfld                       ), // this.m_enemyController
                        /* 15 */ new( OpCodes.Callvirt                    ), // this.m_enemyController.EnemyRank
                        /* 16 */ new( OpCodeSet.Ldc_I4                    ), // EnemyRank.Miniboss
                        /* 17 */ new( OpCodeSet.Bne_Un                    ), // this.m_enemyController.EnemyRank == EnemyRank.Miniboss

                        /* 18 */ new( OpCodeSet.Ldarg                     ), // this
                        /* 19 */ new( OpCodes.Ldfld                       ), // this.m_enemyController
                        /* 20 */ new( OpCodes.Callvirt                    ), // this.m_enemyController.EnemyType
                        /* 21 */ new( OpCodeSet.Ldc_I4                    ), // EnemyType.Eggplant
                        /* 22 */ new( OpCodeSet.Beq                       ), // this.m_enemyController.EnemyType != EnemyType.Eggplant

                        /* 23 */ new( OpCodeSet.Ldloc                     ), // num3
                        /* 24 */ new( OpCodeSet.Ldloc                     ), // num
                        /* 25 */ new( OpCodes.Ldc_R4                      ), // 0.2f
                        /* 26 */ //new( OpCodes.Mul                         ), // num * 0.2f
                        /* 27 */ //new( OpCodes.Add                         ), // num3 + num * 0.2f
                        /* 28 */ //new( OpCodes.Stloc                       ), // num3 = num3 + num * 0.2f
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 25, WobSettings.Get( traitKeys.Get( TraitType.Vampire, "DamageRegen" ), 0.2f ) ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Apply damage taken modifiers
        [HarmonyPatch( typeof( PlayerController ), nameof( PlayerController.CalculateDamageTaken ) )]
        internal static class PlayerController_CalculateDamageTaken_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "PlayerController.CalculateDamageTaken" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldc_I4, TraitType.Vampire   ), // TraitType.Vampire
                        /*  1 */ new( OpCodes.Call, name: "IsTraitActive" ), // TraitManager.IsTraitActive(TraitType.Vampire)
                        /*  2 */ new( OpCodeSet.Brfalse                   ), // if (TraitManager.IsTraitActive(TraitType.Vampire))
                        /*  3 */ new( OpCodeSet.Ldloc                     ), // num
                        /*  4 */ new( OpCodes.Ldc_R4                      ), // 1.25f
                        /*  5 */ new( OpCodes.Add                         ), // num + 1.25f
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 4, WobSettings.Get( traitKeys.Get( TraitType.Vampire, "DamageTaken" ), 1.25f ) ),
                    }, expected: 1 );
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldc_I4, TraitType.NoManaCap     ), // TraitType.NoManaCap
                        /*  1 */ new( OpCodes.Call, name: "IsTraitActive"     ), // TraitManager.IsTraitActive(TraitType.NoManaCap)
                        /*  2 */ new( OpCodeSet.Brfalse                       ), // if (TraitManager.IsTraitActive(TraitType.NoManaCap))
                        /*  3 */ new( OpCodes.Ldarg_0                         ), // this
                        /*  4 */ new( OpCodes.Call, name: "get_CurrentMana"   ), // this.CurrentMana
                        /*  5 */ new( OpCodes.Ldarg_0                         ), // this
                        /*  6 */ new( OpCodes.Call, name: "get_ActualMaxMana" ), // this.ActualMaxMana
                        /*  7 */ new( OpCodes.Conv_R4                         ), // (float)this.ActualMaxMana
                        /*  8 */ new( OpCodeSet.Ble_Un                        ), // if( this.CurrentMana > (float)this.ActualMaxMana )
                        /*  9 */ new( OpCodeSet.Ldloc                         ), // num
                        /* 10 */ new( OpCodes.Ldc_R4                          ), // 0.5f
                        /* 11 */ new( OpCodes.Add                             ), // num + 0.5f
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 10, WobSettings.Get( traitKeys.Get( TraitType.NoManaCap, "DamageTaken" ), 0.5f ) ),
                    }, expected: 1 );
                // Set the damage taken for Icarus' Wings Bargain
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        // num += 0.75f * (float)SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse).Level;
                        /*  0 */ new( OpCodes.Ldc_R4                                  ), // 0.75f
                        /*  1 */ new( OpCodes.Ldsfld, name: "PlayerSaveData"          ), // SaveManager.PlayerSaveData
                        /*  2 */ new( OpCodes.Ldc_I4, (int)RelicType.FlightBonusCurse ), // RelicType.FlightBonusCurse
                        /*  3 */ new( OpCodes.Callvirt, name: "GetRelic"              ), // SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse)
                        /*  4 */ new( OpCodes.Callvirt, name: "get_Level"             ), // SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse).Level
                        /*  5 */ new( OpCodes.Conv_R4                                 ), // (float)SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse).Level
                        /*  6 */ new( OpCodes.Mul                                     ), // 0.75f * (float)SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse).Level
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 0, WobSettings.Get( Relics.relicKeys.Get( RelicType.FlightBonusCurse, "DamageTaken" ), Relic_EV.FLIGHT_BONUS_CURSE_DAMAGE_MOD ) ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Apply damage taken modifiers
        [HarmonyPatch( typeof( ManaRegen ), "OnPlayerHit" )]
        internal static class ManaRegen_OnPlayerHit_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "ManaRegen.OnPlayerHit" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Callvirt, name: "get_ActualMaxMana" ), // PlayerManager.GetPlayerController().ActualMaxMana
                        /*  1 */ new( OpCodes.Conv_R4                             ), // (float)PlayerManager.GetPlayerController().ActualMaxMana
                        /*  2 */ new( OpCodes.Ldc_R4                              ), // 0.5f
                        /*  3 */ new( OpCodes.Mul                                 ), // (float)PlayerManager.GetPlayerController().ActualMaxMana * 0.5f
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 2, WobSettings.Get( traitKeys.Get( TraitType.ManaFromHurt, "ManaRegen" ), 0.5f ) ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Apply max health loss per hit modifiers
        [HarmonyPatch( typeof( SuperHealer_Trait ), "OnPlayerHit" )]
        internal static class SuperHealer_Trait_OnPlayerHit_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "SuperHealer_Trait.OnPlayerHit" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldfld, name: "TemporaryMaxHealthMods" ), // TemporaryMaxHealthMods
                        /*  1 */ new( OpCodes.Ldc_R4                                ), // 0.0625f
                        /*  2 */ new( OpCodes.Sub                                   ), // TemporaryMaxHealthMods - 0.0625f
                        /*  3 */ new( OpCodes.Stfld, name: "TemporaryMaxHealthMods" ), // TemporaryMaxHealthMods = TemporaryMaxHealthMods - 0.0625f
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( traitKeys.Get( TraitType.SuperHealer, "LossPerHit" ), 0.0625f ) ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Apply spell cost modifiers
        [HarmonyPatch( typeof( BaseAbility_RL ), nameof( BaseAbility_RL.ActualCost ), MethodType.Getter )]
        internal static class BaseAbility_RL_ActualCost_Patch {
            // Multiply the cost by the modifier + 1 (100% + additional % from config)
            internal static void Postfix( ref int __result ) {
                __result = Mathf.RoundToInt( __result * ( 1f + GetActiveMod( TraitType.ManaCostAndDamageUp, "SpellCost", 1f ) ) );
            }
            // Patch to set the multiplier in the original method to 1, effectively removing it so we can apply a new modifier in the postfix patch
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "BaseAbility_RL.ActualCost" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodeSet.Ldloc ), // baseCost
                        /*  1 */ new( OpCodes.Conv_R4 ), // (float)baseCost
                        /*  2 */ new( OpCodes.Ldc_R4  ), // 2f
                        /*  3 */ new( OpCodes.Mul     ), // (float)baseCost * 2f
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 2, 1f ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

    }
}
