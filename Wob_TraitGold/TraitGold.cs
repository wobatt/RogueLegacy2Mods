﻿using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using Wob_Common;

namespace Wob_TraitGold {
    [BepInPlugin( "Wob.TraitGold", "Trait Gold Bonus Mod", "0.2" )]
    public partial class TraitGold : BaseUnityPlugin {
        // Static reference to the config item collection so it can be searched in the patch
        public static Dictionary<string, ConfigItem<float>> configTraits;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configTraits = new Dictionary<string, ConfigItem<float>> {
                { "Antique",                new ConfigItem<float>( this.Config, "Traits", "Antique",                "Antique - Heir starts with a random relic.",                                                                                      0f,    0f, float.MaxValue ) },
                { "BlurOnHit",              new ConfigItem<float>( this.Config, "Traits", "BlurOnHit",              "Panic Attacks/Stressed - Getting hit darkens the screen.",                                                                        0.5f,  0f, float.MaxValue ) },
                { "BonusChestGold",         new ConfigItem<float>( this.Config, "Traits", "BonusChestGold",         "Compulsive Gambling/Lootbox Addict - Only chests drop gold and chest values swing wildly!",                                       0.25f, 0f, float.MaxValue ) },
                { "BonusMagicStrength",     new ConfigItem<float>( this.Config, "Traits", "BonusMagicStrength",     "Crippling Intellect - -50% Health and Weapon Damage. Mana regenerates over time.",                                                0f,    0f, float.MaxValue ) },
                { "BounceTerrain",          new ConfigItem<float>( this.Config, "Traits", "BounceTerrain",          "Clownanthropy - 30% less Health, but you can Spin Kick off terrain.",                                                             0f,    0f, float.MaxValue ) },
                { "BreakPropsForMana",      new ConfigItem<float>( this.Config, "Traits", "BreakPropsForMana",      "OCD/Breaker - Breaking things restores Mana.",                                                                                    0f,    0f, float.MaxValue ) },
                { "CanNowAttack",           new ConfigItem<float>( this.Config, "Traits", "CanNowAttack",           "Pacifier - -60% Health and you love to fight!",                                                                                   1.5f,  0f, float.MaxValue ) },
                { "CantAttack",             new ConfigItem<float>( this.Config, "Traits", "CantAttack",             "Pacifist - -60% Health and you can't deal damage.",                                                                               1.5f,  0f, float.MaxValue ) },
                { "CheerOnKills",           new ConfigItem<float>( this.Config, "Traits", "CheerOnKills",           "Diva - Everyone gets a spotlight but all eyes are on you.",                                                                       0.5f,  0f, float.MaxValue ) },
                { "ColorTrails",            new ConfigItem<float>( this.Config, "Traits", "ColorTrails",            "Synesthesia - Everything leaves behind color.",                                                                                   0.25f, 0f, float.MaxValue ) },
                { "DamageBoost",            new ConfigItem<float>( this.Config, "Traits", "DamageBoost",            "Combative - +50% Weapon Damage, -30% Health.",                                                                                    0f,    0f, float.MaxValue ) },
                { "DisarmOnHurt",           new ConfigItem<float>( this.Config, "Traits", "DisarmOnHurt",           "FND/Shocked - Taking damage inflicts Disarmed, meaning the player cannot attack or use spells for 2 seconds.",                    0.5f,  0f, float.MaxValue ) },
                { "EasyBreakables",         new ConfigItem<float>( this.Config, "Traits", "EasyBreakables",         "Clumsy - Objects break on touch.",                                                                                                0f,    0f, float.MaxValue ) },
                { "EnemiesBlackFill",       new ConfigItem<float>( this.Config, "Traits", "EnemiesBlackFill",       "Associative Agnosia - Enemies are blacked out.",                                                                                  0.25f, 0f, float.MaxValue ) },
                { "EnemiesCensored",        new ConfigItem<float>( this.Config, "Traits", "EnemiesCensored",        "Puritan - Enemies are censored.",                                                                                                 0.25f, 0f, float.MaxValue ) },
                { "EnemyKnockedFar",        new ConfigItem<float>( this.Config, "Traits", "EnemyKnockedFar",        "Hypergonadism - Enemies are knocked far away.",                                                                                   0f,    0f, float.MaxValue ) },
                { "EnemyKnockedLow",        new ConfigItem<float>( this.Config, "Traits", "EnemyKnockedLow",        "Muscle Weakness - Enemies barely flinch when hit.",                                                                               0.25f, 0f, float.MaxValue ) },
                { "ExplosiveChests",        new ConfigItem<float>( this.Config, "Traits", "ExplosiveChests",        "Paranoid/Explosive Chests - Chests drop an explosive surprise.",                                                                  0.25f, 0f, float.MaxValue ) },
                { "ExplosiveEnemies",       new ConfigItem<float>( this.Config, "Traits", "ExplosiveEnemies",       "Exploding Casket Syndrome/Explosive Enemies - Enemies drop an explosive surprise.",                                               0.5f,  0f, float.MaxValue ) },
                { "FakeSelfDamage",         new ConfigItem<float>( this.Config, "Traits", "FakeSelfDamage",         "Histrionic - Numbers are exaggerated.",                                                                                           0f,    0f, float.MaxValue ) },
                { "Fart",                   new ConfigItem<float>( this.Config, "Traits", "Fart",                   "IBS - Sometimes fart when jumping or dashing.",                                                                                   0f,    0f, float.MaxValue ) },
                { "FMFFan",                 new ConfigItem<float>( this.Config, "Traits", "FMFFan",                 "FMF Fan - You're probably Korean. (No effect)",                                                                                   0.25f, 0f, float.MaxValue ) },
                { "GainDownStrike",         new ConfigItem<float>( this.Config, "Traits", "GainDownStrike",         "Aerodynamic - Your Spinkick is replaced with Downstrike.",                                                                        0f,    0f, float.MaxValue ) },
                { "Gay",                    new ConfigItem<float>( this.Config, "Traits", "Gay",                    "Nature - Being true to being you. (No effect)",                                                                                   0f,    0f, float.MaxValue ) },
                { "HighJump",               new ConfigItem<float>( this.Config, "Traits", "HighJump",               "IIB Muscle Fibers/High Jumper - Hold [Jump] to Super Jump.",                                                                      0f,    0f, float.MaxValue ) },
                { "HorizontalDarkness",     new ConfigItem<float>( this.Config, "Traits", "HorizontalDarkness",     "Tunnel Vision - Everything that is not on the same level as the player is black.",                                                0.25f, 0f, float.MaxValue ) },
                { "ItemsGoFlying",          new ConfigItem<float>( this.Config, "Traits", "ItemsGoFlying",          "Dyspraxia/Butter Fingers - Items go flying!",                                                                                     0.25f, 0f, float.MaxValue ) },
                { "LongerCD",               new ConfigItem<float>( this.Config, "Traits", "LongerCD",               "Chronic Fatigue Syndrome/Exhausted - All Spells and Talents have a cooldown.",                                                    0.25f, 0f, float.MaxValue ) },
                { "LowerGravity",           new ConfigItem<float>( this.Config, "Traits", "LowerGravity",           "Hollow Bones - You fall slowly.",                                                                                                 0f,    0f, float.MaxValue ) },
                { "LowerStorePrice",        new ConfigItem<float>( this.Config, "Traits", "LowerStorePrice",        "Charismatic - 15% gold discount from all shopkeeps.",                                                                             0f,    0f, float.MaxValue ) },
                { "MagicBoost",             new ConfigItem<float>( this.Config, "Traits", "MagicBoost",             "Bookish - +50% Magic Damage and +50 Mana Capacity. -30% HP.",                                                                     0f,    0f, float.MaxValue ) },
                { "ManaCostAndDamageUp",    new ConfigItem<float>( this.Config, "Traits", "ManaCostAndDamageUp",    "Emotional Dysregularity/Overcompensation - Mana costs and spell damage are increased by 100%.",                                   0f,    0f, float.MaxValue ) },
                { "ManaFromHurt",           new ConfigItem<float>( this.Config, "Traits", "ManaFromHurt",           "Masochism - Gain 50% of your mana when hit, but can't regain mana from attacks.",                                                 0.25f, 0f, float.MaxValue ) },
                { "MapReveal",              new ConfigItem<float>( this.Config, "Traits", "MapReveal",              "Cartographer - Map is revealed but you have no position marker.",                                                                 0.25f, 0f, float.MaxValue ) },
                { "MegaHealth",             new ConfigItem<float>( this.Config, "Traits", "MegaHealth",             "Hero Complex - 100% more Health but you can't heal, ever.",                                                                       0f,    0f, float.MaxValue ) },
                { "NoColor",                new ConfigItem<float>( this.Config, "Traits", "NoColor",                "Colorblind - You can't see colors.",                                                                                              0.25f, 0f, float.MaxValue ) },
                { "NoEnemyHealthBar",       new ConfigItem<float>( this.Config, "Traits", "NoEnemyHealthBar",       "Alexithymia/Unempathetic - Can't see damage dealt.",                                                                              0.25f, 0f, float.MaxValue ) },
                { "NoHealthBar",            new ConfigItem<float>( this.Config, "Traits", "NoHealthBar",            "C.I.P - Can't see your health.",                                                                                                  0.25f, 0f, float.MaxValue ) },
                { "NoImmunityWindow",       new ConfigItem<float>( this.Config, "Traits", "NoImmunityWindow",       "Algesia - No immunity window after taking damage.",                                                                               0.5f,  0f, float.MaxValue ) },
                { "NoMeat",                 new ConfigItem<float>( this.Config, "Traits", "NoMeat",                 "Vegan - Eating food hurts you.",                                                                                                  0.75f, 0f, float.MaxValue ) },
                { "OldYellowTint",          new ConfigItem<float>( this.Config, "Traits", "OldYellowTint",          "Nostalgic - Everything is old-timey tinted.",                                                                                     0.25f, 0f, float.MaxValue ) },
                { "OmniDash",               new ConfigItem<float>( this.Config, "Traits", "OmniDash",               "Superfluid - -20% Health, but you can dash in ANY direction.",                                                                    0f,    0f, float.MaxValue ) },
                { "OneHitDeath",            new ConfigItem<float>( this.Config, "Traits", "OneHitDeath",            "Osteogenesis Imperfecta/Fragile - You die in one hit.",                                                                           2f,    0f, float.MaxValue ) },
                { "PlayerKnockedFar",       new ConfigItem<float>( this.Config, "Traits", "PlayerKnockedFar",       "Ectomorph - Taking damage knocks you far away.",                                                                                  0.25f, 0f, float.MaxValue ) },
                { "PlayerKnockedLow",       new ConfigItem<float>( this.Config, "Traits", "PlayerKnockedLow",       "Endomorph - You barely flinch when enemies hit you.",                                                                             0f,    0f, float.MaxValue ) },
                { "RandomizeKit",           new ConfigItem<float>( this.Config, "Traits", "RandomizeKit",           "Contrarian/Innovator - Your Weapon and Talent are randomized.",                                                                   0.25f, 0f, float.MaxValue ) },
                { "RevealAllChests",        new ConfigItem<float>( this.Config, "Traits", "RevealAllChests",        "Spelunker - -10% HP but you can see all chests on the map!",                                                                      0f,    0f, float.MaxValue ) },
                { "SkillCritsOnly",         new ConfigItem<float>( this.Config, "Traits", "SkillCritsOnly",         "Perfectionist - Only Skill Crits and Spin Kicks deal damage.",                                                                    0.5f,  0f, float.MaxValue ) },
                { "SmallHitbox",            new ConfigItem<float>( this.Config, "Traits", "SmallHitbox",            "Disattuned/Only Heart - 25% less health, but you can only be hit in the heart.",                                                  0f,    0f, float.MaxValue ) },
                { "SuperFart",              new ConfigItem<float>( this.Config, "Traits", "SuperFart",              "Super IBS - Super Fart Talent that releases a cloud that inflicts 3 seconds of Burn on enemies and launches the player upwards.", 0f,    0f, float.MaxValue ) },
                { "SuperHealer",            new ConfigItem<float>( this.Config, "Traits", "SuperHealer",            "Hypercoagulation/Super Healer - HP regenerates, but you lose some Max HP when hit.",                                              0f,    0f, float.MaxValue ) },
                { "TwinRelics",             new ConfigItem<float>( this.Config, "Traits", "TwinRelics",             "Compulsive Hoarder/Hoarder - All Relics are Twin Relics (when possible).",                                                        0f,    0f, float.MaxValue ) },
                { "UpsideDown",             new ConfigItem<float>( this.Config, "Traits", "UpsideDown",             "Vertigo - Everything is upside-down.",                                                                                            0.75f, 0f, float.MaxValue ) },
                { "Vampire",                new ConfigItem<float>( this.Config, "Traits", "Vampire",                "Vampirism - Gain 20% of your Weapon Damage as Health, but you take 125% more damage.",                                            0f,    0f, float.MaxValue ) },
                { "YouAreBlue",             new ConfigItem<float>( this.Config, "Traits", "YouAreBlue",             "Methemoglobinemia/Blue - You are blue. (No effect)",                                                                              0f,    0f, float.MaxValue ) },
                { "YouAreLarge",            new ConfigItem<float>( this.Config, "Traits", "YouAreLarge",            "Gigantism - You are gigantic.",                                                                                                   0.25f, 0f, float.MaxValue ) },
                { "YouAreSmall",            new ConfigItem<float>( this.Config, "Traits", "YouAreSmall",            "Dwarfism - You are Tiny. (Required to access a Scar in Axis Mundi)",                                                              0.25f, 0f, float.MaxValue ) },
                
                // These are not listed on the wiki, or are listed as removed, so probably don't need them?
                { "AngryOnHit",             new ConfigItem<float>( this.Config, "Traits", "AngryOnHit",             "I.E.D/Quick to Anger - Damage and movement speed bonus when hit.",                                                                0f,    0f, float.MaxValue ) },
                { "BlurryClose",            new ConfigItem<float>( this.Config, "Traits", "BlurryClose",            "Optical Migraine - Everything up close is blurry.",                                                                               0.1f,  0f, float.MaxValue ) },
                { "BlurryFar",              new ConfigItem<float>( this.Config, "Traits", "BlurryFar",              "Near-sighted - Everything far away is blurry.",                                                                                   0.1f,  0f, float.MaxValue ) },
                { "DarkScreen",             new ConfigItem<float>( this.Config, "Traits", "DarkScreen",             "Glaucoma - Darkness surrounds you.",                                                                                              0.25f, 0f, float.MaxValue ) },
                { "DisableAttackLock",      new ConfigItem<float>( this.Config, "Traits", "DisableAttackLock",      "Flexible - You can turn while attacking.",                                                                                        0f,    0f, float.MaxValue ) },
                { "FreeRelic",              new ConfigItem<float>( this.Config, "Traits", "FreeRelic",              "Treasure Hunter - Start with a random Relic.",                                                                                    0f,    0f, float.MaxValue ) },
                { "GameShake",              new ConfigItem<float>( this.Config, "Traits", "GameShake",              "Clonus - Game randomly shakes.",                                                                                                  0f,    0f, float.MaxValue ) },
                { "HalloweenHoliday",       new ConfigItem<float>( this.Config, "Traits", "HalloweenHoliday",       "Medium - Everything is spooky!",                                                                                                  0f,    0f, float.MaxValue ) },
                { "InvulnDash",             new ConfigItem<float>( this.Config, "Traits", "InvulnDash",             "Evasive - Invincible while dashing, but you have 50% less hp, and dashing dodges has a cooldown.",                                0f,    0f, float.MaxValue ) },
                { "MushroomGrow",           new ConfigItem<float>( this.Config, "Traits", "MushroomGrow",           "Fun-Guy/Fun-Gal/Mushroom Man/Mushroom Lady - You really like mushrooms.",                                                         0f,    0f, float.MaxValue ) },
                { "NoManaCap",              new ConfigItem<float>( this.Config, "Traits", "NoManaCap",              "IED/Overexerter - You can exceed your Mana Capacity but will take damage over time for it.",                                      0f,    0f, float.MaxValue ) },
                { "NotMovingSlowGame",      new ConfigItem<float>( this.Config, "Traits", "NotMovingSlowGame",      "Hyperreflexia - Time moves only when you move.",                                                                                  0f,    0f, float.MaxValue ) },
                { "Oversaturate",           new ConfigItem<float>( this.Config, "Traits", "Oversaturate",           "Tetrachromat - All colours are deeper",                                                                                           0f,    0f, float.MaxValue ) },
                { "Retro",                  new ConfigItem<float>( this.Config, "Traits", "Retro",                  "Antiquarian - Everything is retro.",                                                                                              0.25f, 0f, float.MaxValue ) },
                { "ShowEnemiesOnMap",       new ConfigItem<float>( this.Config, "Traits", "ShowEnemiesOnMap",       "Eiditic Memory - Enemies are shown on map.",                                                                                      0f,    0f, float.MaxValue ) },
                { "Swearing",               new ConfigItem<float>( this.Config, "Traits", "Swearing",               "Coprolalia - You swear when struck.",                                                                                             0f,    0f, float.MaxValue ) },
                { "WordScramble",           new ConfigItem<float>( this.Config, "Traits", "WordScramble",           "Dyslexia - Words are scrambled.",                                                                                                 0f,    0f, float.MaxValue ) },
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
                                WobPlugin.Log( "Changing bonus for " + traitData.Name + " (" + WobPlugin.GetTraitTitles( traitData ) + ") from " + traitData.GoldBonus + " to " + configEntry.Value );
                                // If a matching config setting has been found, calculate the new gold gain using the file value rather than the game value, and overwite the method return value
                                __result = configEntry.Value * ( 1f + SkillTreeManager.GetSkillTreeObj( SkillTreeType.Traits_Give_Gold_Gain_Mod ).CurrentStatGain );
                            } else {
                                WobPlugin.Log( "Same bonus for " + traitData.Name + " (" + WobPlugin.GetTraitTitles( traitData ) + ") of " + traitData.GoldBonus );
                            }
                        } else {
                            WobPlugin.Log( "No config for " + traitData.Name + " (" + WobPlugin.GetTraitTitles( traitData ) + ") bonus " + traitData.GoldBonus );
                        }
                    }
                }
                // If any of the 'if' statements fail, don't change the return value that the original method decided on
            }
        }
    }
}