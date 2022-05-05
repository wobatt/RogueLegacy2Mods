using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using Wob_Common;

namespace Wob_TraitGold {
    [BepInPlugin( "Wob.TraitGold", "Trait Gold Bonus Mod", "0.1" )]
    public partial class BepInExPlugin : BaseUnityPlugin {
        // Static reference to the config item collection so it can be searched in the patch
        public static Dictionary<string, ConfigItem<float>> configTraits;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configTraits = new Dictionary<string, ConfigItem<float>> {
                { "Alcoholic",              new ConfigItem<float>( this.Config, "Traits", "Alcoholic",              "Lactose Intolerant",                          0f,    0f, float.MaxValue ) },
                { "AngryOnHit",             new ConfigItem<float>( this.Config, "Traits", "AngryOnHit",             "I.E.D/Quick to Anger",                        0f,    0f, float.MaxValue ) },
                { "Antique",                new ConfigItem<float>( this.Config, "Traits", "Antique",                "Antique",                                     0f,    0f, float.MaxValue ) },
                { "BackwardSpell",          new ConfigItem<float>( this.Config, "Traits", "BackwardSpell",          "Ambilevous/Flippy",                           0f,    0f, float.MaxValue ) },
                { "Bald",                   new ConfigItem<float>( this.Config, "Traits", "Bald",                   "Bald",                                        0f,    0f, float.MaxValue ) },
                { "BiomeBigger",            new ConfigItem<float>( this.Config, "Traits", "BiomeBigger",            "Claustrophobia",                              0f,    0f, float.MaxValue ) },
                { "BiomeSmaller",           new ConfigItem<float>( this.Config, "Traits", "BiomeSmaller",           "Agoraphobia",                                 0f,    0f, float.MaxValue ) },
                { "BlurOnHit",              new ConfigItem<float>( this.Config, "Traits", "BlurOnHit",              "Panic Attacks/Stressed",                      0.5f,  0f, float.MaxValue ) },
                { "BlurryClose",            new ConfigItem<float>( this.Config, "Traits", "BlurryClose",            "Optical Migraine",                            0.1f,  0f, float.MaxValue ) },
                { "BlurryFar",              new ConfigItem<float>( this.Config, "Traits", "BlurryFar",              "Near-sighted",                                0.1f,  0f, float.MaxValue ) },
                { "BonusChestGold",         new ConfigItem<float>( this.Config, "Traits", "BonusChestGold",         "Compulsive Gambling/Lootbox Addict",          0.25f, 0f, float.MaxValue ) },
                { "BonusMagicStrength",     new ConfigItem<float>( this.Config, "Traits", "BonusMagicStrength",     "Crippling Intellect",                         0f,    0f, float.MaxValue ) },
                { "BounceTerrain",          new ConfigItem<float>( this.Config, "Traits", "BounceTerrain",          "Clownanthropy",                               0f,    0f, float.MaxValue ) },
                { "BreakPropsForMana",      new ConfigItem<float>( this.Config, "Traits", "BreakPropsForMana",      "OCD/Breaker",                                 0f,    0f, float.MaxValue ) },
                { "CameraZoomIn",           new ConfigItem<float>( this.Config, "Traits", "CameraZoomIn",           "Macropesia",                                  0f,    0f, float.MaxValue ) },
                { "CameraZoomOut",          new ConfigItem<float>( this.Config, "Traits", "CameraZoomOut",          "Eagle Eye",                                   0f,    0f, float.MaxValue ) },
                { "CanNowAttack",           new ConfigItem<float>( this.Config, "Traits", "CanNowAttack",           "Pacifier",                                    1.5f,  0f, float.MaxValue ) },
                { "CantAttack",             new ConfigItem<float>( this.Config, "Traits", "CantAttack",             "Pacifist",                                    1.5f,  0f, float.MaxValue ) },
                { "CantSeeChildren",        new ConfigItem<float>( this.Config, "Traits", "CantSeeChildren",        "Prosopagnosia",                               0f,    0f, float.MaxValue ) },
                { "CheerOnKills",           new ConfigItem<float>( this.Config, "Traits", "CheerOnKills",           "Diva",                                        0.5f,  0f, float.MaxValue ) },
                { "ChickensAreEnemies",     new ConfigItem<float>( this.Config, "Traits", "ChickensAreEnemies",     "Alektorophobia/Chicken",                      0f,    0f, float.MaxValue ) },
                { "ColorTrails",            new ConfigItem<float>( this.Config, "Traits", "ColorTrails",            "Synesthesia",                                 0.25f, 0f, float.MaxValue ) },
                { "CuteGame",               new ConfigItem<float>( this.Config, "Traits", "CuteGame",               "Optimist",                                    0f,    0f, float.MaxValue ) },
                { "DamageBoost",            new ConfigItem<float>( this.Config, "Traits", "DamageBoost",            "Combative",                                   0f,    0f, float.MaxValue ) },
                { "DarkScreen",             new ConfigItem<float>( this.Config, "Traits", "DarkScreen",             "Glaucoma",                                    0.25f, 0f, float.MaxValue ) },
                { "DisableAttackLock",      new ConfigItem<float>( this.Config, "Traits", "DisableAttackLock",      "Flexible",                                    0f,    0f, float.MaxValue ) },
                { "DisarmOnHurt",           new ConfigItem<float>( this.Config, "Traits", "DisarmOnHurt",           "FND/Shocked",                                 0.5f,  0f, float.MaxValue ) },
                { "Dog",                    new ConfigItem<float>( this.Config, "Traits", "Dog",                    "Lycanthropy",                                 0f,    0f, float.MaxValue ) },
                { "EasyBreakables",         new ConfigItem<float>( this.Config, "Traits", "EasyBreakables",         "Clumsy",                                      0f,    0f, float.MaxValue ) },
                { "EnemiesBlackFill",       new ConfigItem<float>( this.Config, "Traits", "EnemiesBlackFill",       "Associative Agnosia",                         0.25f, 0f, float.MaxValue ) },
                { "EnemiesCensored",        new ConfigItem<float>( this.Config, "Traits", "EnemiesCensored",        "Puritan",                                     0.25f, 0f, float.MaxValue ) },
                { "EnemyKnockedFar",        new ConfigItem<float>( this.Config, "Traits", "EnemyKnockedFar",        "Hypergonadism",                               0f,    0f, float.MaxValue ) },
                { "EnemyKnockedLow",        new ConfigItem<float>( this.Config, "Traits", "EnemyKnockedLow",        "Muscle Weakness",                             0.25f, 0f, float.MaxValue ) },
                { "ExplosiveChests",        new ConfigItem<float>( this.Config, "Traits", "ExplosiveChests",        "Paranoid/Explosive Chests",                   0.25f, 0f, float.MaxValue ) },
                { "ExplosiveEnemies",       new ConfigItem<float>( this.Config, "Traits", "ExplosiveEnemies",       "Exploding Casket Syndrome/Explosive Enemies", 0.5f,  0f, float.MaxValue ) },
                { "FakeEnemies",            new ConfigItem<float>( this.Config, "Traits", "FakeEnemies",            "Hallucinations",                              3f,    0f, float.MaxValue ) },
                { "FakeSelfDamage",         new ConfigItem<float>( this.Config, "Traits", "FakeSelfDamage",         "Histrionic",                                  0f,    0f, float.MaxValue ) },
                { "Fart",                   new ConfigItem<float>( this.Config, "Traits", "Fart",                   "IBS",                                         0f,    0f, float.MaxValue ) },
                { "FastTeleport",           new ConfigItem<float>( this.Config, "Traits", "FastTeleport",           "Conductor",                                   0f,    0f, float.MaxValue ) },
                { "FindBoss",               new ConfigItem<float>( this.Config, "Traits", "FindBoss",               "Big-Game Hunter",                             0f,    0f, float.MaxValue ) },
                { "FMFFan",                 new ConfigItem<float>( this.Config, "Traits", "FMFFan",                 "FMF Fan",                                     0.25f, 0f, float.MaxValue ) },
                { "FoodSlow",               new ConfigItem<float>( this.Config, "Traits", "FoodSlow",               "Soporific",                                   0f,    0f, float.MaxValue ) },
                { "ForcedChoice",           new ConfigItem<float>( this.Config, "Traits", "ForcedChoice",           "Unhealthy Curiosity",                         0f,    0f, float.MaxValue ) },
                { "FreeRelic",              new ConfigItem<float>( this.Config, "Traits", "FreeRelic",              "Treasure Hunter",                             0f,    0f, float.MaxValue ) },
                { "GainDownStrike",         new ConfigItem<float>( this.Config, "Traits", "GainDownStrike",         "Aerodynamic",                                 0f,    0f, float.MaxValue ) },
                { "GameRunsFaster",         new ConfigItem<float>( this.Config, "Traits", "GameRunsFaster",         "ADHD",                                        0f,    0f, float.MaxValue ) },
                { "GameShake",              new ConfigItem<float>( this.Config, "Traits", "GameShake",              "Clonus",                                      0f,    0f, float.MaxValue ) },
                { "Gay",                    new ConfigItem<float>( this.Config, "Traits", "Gay",                    "Nature",                                      0f,    0f, float.MaxValue ) },
                { "HalloweenHoliday",       new ConfigItem<float>( this.Config, "Traits", "HalloweenHoliday",       "Medium",                                      0f,    0f, float.MaxValue ) },
                { "HighBounce",             new ConfigItem<float>( this.Config, "Traits", "HighBounce",             "Bubbly",                                      0f,    0f, float.MaxValue ) },
                { "HighJump",               new ConfigItem<float>( this.Config, "Traits", "HighJump",               "IIB Muscle Fibers/High Jumper",               0f,    0f, float.MaxValue ) },
                { "HorizontalDarkness",     new ConfigItem<float>( this.Config, "Traits", "HorizontalDarkness",     "Tunnel Vision",                               0.25f, 0f, float.MaxValue ) },
                { "InvulnDash",             new ConfigItem<float>( this.Config, "Traits", "InvulnDash",             "Evasive",                                     0f,    0f, float.MaxValue ) },
                { "ItemsGoFlying",          new ConfigItem<float>( this.Config, "Traits", "ItemsGoFlying",          "Dyspraxia/Butter Fingers",                    0.25f, 0f, float.MaxValue ) },
                { "KickResetMobility",      new ConfigItem<float>( this.Config, "Traits", "KickResetMobility",      "Freerunner",                                  0f,    0f, float.MaxValue ) },
                { "LifeTimer",              new ConfigItem<float>( this.Config, "Traits", "LifeTimer",              "Cardiomyopathy",                              0f,    0f, float.MaxValue ) },
                { "LongerCD",               new ConfigItem<float>( this.Config, "Traits", "LongerCD",               "Chronic Fatigue Syndrome/Exhausted",          0.25f, 0f, float.MaxValue ) },
                { "LowerGravity",           new ConfigItem<float>( this.Config, "Traits", "LowerGravity",           "Hollow Bones",                                0f,    0f, float.MaxValue ) },
                { "LowerStorePrice",        new ConfigItem<float>( this.Config, "Traits", "LowerStorePrice",        "Charismatic",                                 0f,    0f, float.MaxValue ) },
                { "MagicBoost",             new ConfigItem<float>( this.Config, "Traits", "MagicBoost",             "Bookish",                                     0f,    0f, float.MaxValue ) },
                { "MagnetRangeBoost",       new ConfigItem<float>( this.Config, "Traits", "MagnetRangeBoost",       "Biomagnetic",                                 0f,    0f, float.MaxValue ) },
                { "ManaCostAndDamageUp",    new ConfigItem<float>( this.Config, "Traits", "ManaCostAndDamageUp",    "Emotional Dysregularity/Overcompensation",    0f,    0f, float.MaxValue ) },
                { "ManaFromHurt",           new ConfigItem<float>( this.Config, "Traits", "ManaFromHurt",           "Masochism",                                   0.25f, 0f, float.MaxValue ) },
                { "MapReveal",              new ConfigItem<float>( this.Config, "Traits", "MapReveal",              "Cartographer",                                0.25f, 0f, float.MaxValue ) },
                { "MegaHealth",             new ConfigItem<float>( this.Config, "Traits", "MegaHealth",             "Hero Complex",                                0f,    0f, float.MaxValue ) },
                { "MushroomGrow",           new ConfigItem<float>( this.Config, "Traits", "MushroomGrow",           "Fun-Guy/Fun-Gal/Mushroom Man/Mushroom Lady",  0f,    0f, float.MaxValue ) },
                { "NoColor",                new ConfigItem<float>( this.Config, "Traits", "NoColor",                "Colorblind",                                  0.25f, 0f, float.MaxValue ) },
                { "NoEnemyHealthBar",       new ConfigItem<float>( this.Config, "Traits", "NoEnemyHealthBar",       "Alexithymia/Unempathetic",                    0.25f, 0f, float.MaxValue ) },
                { "NoHealthBar",            new ConfigItem<float>( this.Config, "Traits", "NoHealthBar",            "C.I.P",                                       0.25f, 0f, float.MaxValue ) },
                { "NoImmunityWindow",       new ConfigItem<float>( this.Config, "Traits", "NoImmunityWindow",       "Algesia",                                     0.5f,  0f, float.MaxValue ) },
                { "NoManaCap",              new ConfigItem<float>( this.Config, "Traits", "NoManaCap",              "IED/Overexerter",                             0f,    0f, float.MaxValue ) },
                { "NoMap",                  new ConfigItem<float>( this.Config, "Traits", "NoMap",                  "Anterograde Amnesia",                         0f,    0f, float.MaxValue ) },
                { "NoMeat",                 new ConfigItem<float>( this.Config, "Traits", "NoMeat",                 "Vegan",                                       0.75f, 0f, float.MaxValue ) },
                { "NoProjectileIndicators", new ConfigItem<float>( this.Config, "Traits", "NoProjectileIndicators", "Poor Periphery",                              0f,    0f, float.MaxValue ) },
                { "NotMovingSlowGame",      new ConfigItem<float>( this.Config, "Traits", "NotMovingSlowGame",      "Hyperreflexia",                               0f,    0f, float.MaxValue ) },
                { "OldYellowTint",          new ConfigItem<float>( this.Config, "Traits", "OldYellowTint",          "Nostalgic",                                   0.25f, 0f, float.MaxValue ) },
                { "OmniDash",               new ConfigItem<float>( this.Config, "Traits", "OmniDash",               "Superfluid",                                  0f,    0f, float.MaxValue ) },
                { "OneChild",               new ConfigItem<float>( this.Config, "Traits", "OneChild",               "Only child",                                  0f,    0f, float.MaxValue ) },
                { "OneHitDeath",            new ConfigItem<float>( this.Config, "Traits", "OneHitDeath",            "Osteogenesis Imperfecta/Fragile",             2f,    0f, float.MaxValue ) },
                { "Oversaturate",           new ConfigItem<float>( this.Config, "Traits", "Oversaturate",           "Tetrachromat",                                0f,    0f, float.MaxValue ) },
                { "PlayerKnockedFar",       new ConfigItem<float>( this.Config, "Traits", "PlayerKnockedFar",       "Ectomorph",                                   0.25f, 0f, float.MaxValue ) },
                { "PlayerKnockedLow",       new ConfigItem<float>( this.Config, "Traits", "PlayerKnockedLow",       "Endomorph",                                   0f,    0f, float.MaxValue ) },
                { "RandomDamage",           new ConfigItem<float>( this.Config, "Traits", "RandomDamage",           "Dungeon Master",                              0f,    0f, float.MaxValue ) },
                { "RandomizeKit",           new ConfigItem<float>( this.Config, "Traits", "RandomizeKit",           "Contrarian/Innovator",                        0.25f, 0f, float.MaxValue ) },
                { "RandomizeSpells",        new ConfigItem<float>( this.Config, "Traits", "RandomizeSpells",        "Savant",                                      0f,    0f, float.MaxValue ) },
                { "RandomizeWeapons",       new ConfigItem<float>( this.Config, "Traits", "RandomizeWeapons",       "Showboater",                                  0f,    0f, float.MaxValue ) },
                { "RandomSounds",           new ConfigItem<float>( this.Config, "Traits", "RandomSounds",           "Schizophrenia",                               0f,    0f, float.MaxValue ) },
                { "Retro",                  new ConfigItem<float>( this.Config, "Traits", "Retro",                  "Antiquarian",                                 0.25f, 0f, float.MaxValue ) },
                { "RevealAllChests",        new ConfigItem<float>( this.Config, "Traits", "RevealAllChests",        "Spelunker",                                   0f,    0f, float.MaxValue ) },
                { "ShowEnemiesOnMap",       new ConfigItem<float>( this.Config, "Traits", "ShowEnemiesOnMap",       "Eiditic Memory",                              0f,    0f, float.MaxValue ) },
                { "SkillCritsOnly",         new ConfigItem<float>( this.Config, "Traits", "SkillCritsOnly",         "Perfectionist",                               0.5f,  0f, float.MaxValue ) },
                { "SlowTimeTrigger",        new ConfigItem<float>( this.Config, "Traits", "SlowTimeTrigger",        "Hyper Concentration",                         0f,    0f, float.MaxValue ) },
                { "SmallHitbox",            new ConfigItem<float>( this.Config, "Traits", "SmallHitbox",            "Disattuned/Only Heart",                       0f,    0f, float.MaxValue ) },
                { "SummerHoliday",          new ConfigItem<float>( this.Config, "Traits", "SummerHoliday",          "Surfer",                                      0f,    0f, float.MaxValue ) },
                { "SuperFart",              new ConfigItem<float>( this.Config, "Traits", "SuperFart",              "Super IBS",                                   0f,    0f, float.MaxValue ) },
                { "SuperHealer",            new ConfigItem<float>( this.Config, "Traits", "SuperHealer",            "Hypercoagulation/Super Healer",               0f,    0f, float.MaxValue ) },
                { "Swearing",               new ConfigItem<float>( this.Config, "Traits", "Swearing",               "Coprolalia",                                  0f,    0f, float.MaxValue ) },
                { "TwinRelics",             new ConfigItem<float>( this.Config, "Traits", "TwinRelics",             "Compulsive Hoarder/Hoarder",                  0f,    0f, float.MaxValue ) },
                { "UpsideDown",             new ConfigItem<float>( this.Config, "Traits", "UpsideDown",             "Vertigo",                                     0.75f, 0f, float.MaxValue ) },
                { "Vampire",                new ConfigItem<float>( this.Config, "Traits", "Vampire",                "Vampirism",                                   0f,    0f, float.MaxValue ) },
                { "WeaponSpellSwitch",      new ConfigItem<float>( this.Config, "Traits", "WeaponSpellSwitch",      "Left-Handed",                                 0f,    0f, float.MaxValue ) },
                { "WinterHoliday",          new ConfigItem<float>( this.Config, "Traits", "WinterHoliday",          "Festive",                                     0f,    0f, float.MaxValue ) },
                { "WordScramble",           new ConfigItem<float>( this.Config, "Traits", "WordScramble",           "Dyslexia",                                    0f,    0f, float.MaxValue ) },
                { "YouAreBlue",             new ConfigItem<float>( this.Config, "Traits", "YouAreBlue",             "Methemoglobinemia/Blue",                      0f,    0f, float.MaxValue ) },
                { "YouAreLarge",            new ConfigItem<float>( this.Config, "Traits", "YouAreLarge",            "Gigantism",                                   0.25f, 0f, float.MaxValue ) },
                { "YouAreSmall",            new ConfigItem<float>( this.Config, "Traits", "YouAreSmall",            "Dwarfism",                                    0.25f, 0f, float.MaxValue ) },
            };
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Patch for the method that gets the gold increase for a trait
        [HarmonyPatch( typeof( TraitManager ), nameof( TraitManager.GetActualTraitGoldGain ) )]
        static class TraitManager_GetActualTraitGoldGain_Patch {
            static void Postfix( TraitType traitType, ref float __result ) {
                // Check that traits should be giving gold
                if( SkillTreeManager.GetSkillObjLevel( SkillTreeType.Traits_Give_Gold ) > 0 ) {
                    // Get the data for the trait being looked at - this is from the original method parameter
                    TraitData traitData = TraitLibrary.GetTraitData( traitType );
                    if( traitData != null ) {
                        // Create a variable for the config file element
                        ConfigItem<float> configEntry;
                        // Search the config for a setting that has the same name as the internal name of the trait
                        if( configTraits.TryGetValue( traitData.Name, out configEntry ) ) {
                            if( configEntry.Value != traitData.GoldBonus ) {
                                WobPlugin.Log( "Changing bonus for " + traitData.Name + " (" + GetTraitTitles( traitData ) + ") from " + traitData.GoldBonus + " to " + configEntry.Value );
                                // If a matching config setting has been found, calculate the new gold gain using the file value rather than the game value, and overwite the method return value
                                __result = configEntry.Value * ( 1f + SkillTreeManager.GetSkillTreeObj( SkillTreeType.Traits_Give_Gold_Gain_Mod ).CurrentStatGain );
                            } else {
                                WobPlugin.Log( "Same bonus for " + traitData.Name + " (" + GetTraitTitles( traitData ) + ") of " + traitData.GoldBonus );
                            }
                        } else {
                            WobPlugin.Log( "No config for " + traitData.Name + " (" + GetTraitTitles( traitData ) + ") bonus " + traitData.GoldBonus );
                        }
                    }
                }
                // If any of the 'if' statements fail, don't change the return value that the original method decided on
            }
        }

        // Helper to get the UI names of a trait
        private static string GetTraitTitles( TraitData traitData ) {
            // Each trait has 4 possible names - scientific/non-scientific and male/female character
            // First get all 4 variants
            string tScientificM = LocalizationManager.GetString( traitData.Title, false, false );
            string tScientificF = LocalizationManager.GetString( traitData.Title, true, false );
            string tNonScientificM = LocalizationManager.GetString( traitData.Title.Replace( "_1", "_2" ), false, false );
            string tNonScientificF = LocalizationManager.GetString( traitData.Title.Replace( "_1", "_2" ), true, false );
            // Build a return string, suppressing variants if they are the same as one already added
            return tScientificM + ( tScientificM == tScientificF ? "" : "/" + tScientificF ) + ( tScientificM == tNonScientificM ? "" : "/" + tNonScientificM ) + ( ( tNonScientificM == tNonScientificF || tScientificF == tNonScientificF ) ? "" : "/" + tNonScientificF );
        }

    }
}