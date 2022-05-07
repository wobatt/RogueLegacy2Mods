using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using Wob_Common;

namespace Wob_TraitBan {
    [BepInPlugin( "Wob.TraitBan", "Trait Ban Mod", "0.2" )]
    public partial class BepInExPlugin : BaseUnityPlugin {
        // Static reference to the config item collection so it can be searched in the patch
        public static Dictionary<string, ConfigItem<bool>> configTraits;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configTraits = new Dictionary<string, ConfigItem<bool>> {
                { "Antique",                new ConfigItemBool( this.Config, "Traits", "Antique",                "Antique - Heir starts with a random relic.",                                                                                      true ) },
                { "BlurOnHit",              new ConfigItemBool( this.Config, "Traits", "BlurOnHit",              "Panic Attacks/Stressed - Getting hit darkens the screen.",                                                                        true ) },
                { "BonusChestGold",         new ConfigItemBool( this.Config, "Traits", "BonusChestGold",         "Compulsive Gambling/Lootbox Addict - Only chests drop gold and chest values swing wildly!",                                       true ) },
                { "BonusMagicStrength",     new ConfigItemBool( this.Config, "Traits", "BonusMagicStrength",     "Crippling Intellect - -50% Health and Weapon Damage. Mana regenerates over time.",                                                true ) },
                { "BounceTerrain",          new ConfigItemBool( this.Config, "Traits", "BounceTerrain",          "Clownanthropy - 30% less Health, but you can Spin Kick off terrain.",                                                             true ) },
                { "BreakPropsForMana",      new ConfigItemBool( this.Config, "Traits", "BreakPropsForMana",      "OCD/Breaker - Breaking things restores Mana.",                                                                                    true ) },
                { "CanNowAttack",           new ConfigItemBool( this.Config, "Traits", "CanNowAttack",           "Pacifier - -60% Health and you love to fight!",                                                                                   true ) },
                { "CantAttack",             new ConfigItemBool( this.Config, "Traits", "CantAttack",             "Pacifist - -60% Health and you can't deal damage.",                                                                               true ) },
                { "CheerOnKills",           new ConfigItemBool( this.Config, "Traits", "CheerOnKills",           "Diva - Everyone gets a spotlight but all eyes are on you.",                                                                       true ) },
                { "ColorTrails",            new ConfigItemBool( this.Config, "Traits", "ColorTrails",            "Synesthesia - Everything leaves behind color.",                                                                                   true ) },
                { "DamageBoost",            new ConfigItemBool( this.Config, "Traits", "DamageBoost",            "Combative - +50% Weapon Damage, -30% Health.",                                                                                    true ) },
                { "DisarmOnHurt",           new ConfigItemBool( this.Config, "Traits", "DisarmOnHurt",           "FND/Shocked - Taking damage inflicts Disarmed, meaning the player cannot attack or use spells for 2 seconds.",                    true ) },
                { "EasyBreakables",         new ConfigItemBool( this.Config, "Traits", "EasyBreakables",         "Clumsy - Objects break on touch.",                                                                                                true ) },
                { "EnemiesBlackFill",       new ConfigItemBool( this.Config, "Traits", "EnemiesBlackFill",       "Associative Agnosia - Enemies are blacked out.",                                                                                  true ) },
                { "EnemiesCensored",        new ConfigItemBool( this.Config, "Traits", "EnemiesCensored",        "Puritan - Enemies are censored.",                                                                                                 true ) },
                { "EnemyKnockedFar",        new ConfigItemBool( this.Config, "Traits", "EnemyKnockedFar",        "Hypergonadism - Enemies are knocked far away.",                                                                                   true ) },
                { "EnemyKnockedLow",        new ConfigItemBool( this.Config, "Traits", "EnemyKnockedLow",        "Muscle Weakness - Enemies barely flinch when hit.",                                                                               true ) },
                { "ExplosiveChests",        new ConfigItemBool( this.Config, "Traits", "ExplosiveChests",        "Paranoid/Explosive Chests - Chests drop an explosive surprise.",                                                                  true ) },
                { "ExplosiveEnemies",       new ConfigItemBool( this.Config, "Traits", "ExplosiveEnemies",       "Exploding Casket Syndrome/Explosive Enemies - Enemies drop an explosive surprise.",                                               true ) },
                { "FakeSelfDamage",         new ConfigItemBool( this.Config, "Traits", "FakeSelfDamage",         "Histrionic - Numbers are exaggerated.",                                                                                           true ) },
                { "Fart",                   new ConfigItemBool( this.Config, "Traits", "Fart",                   "IBS - Sometimes fart when jumping or dashing.",                                                                                   true ) },
                { "FMFFan",                 new ConfigItemBool( this.Config, "Traits", "FMFFan",                 "FMF Fan - You're probably Korean. (No effect)",                                                                                   true ) },
                { "GainDownStrike",         new ConfigItemBool( this.Config, "Traits", "GainDownStrike",         "Aerodynamic - Your Spinkick is replaced with Downstrike.",                                                                        true ) },
                { "Gay",                    new ConfigItemBool( this.Config, "Traits", "Gay",                    "Nature - Being true to being you.",                                                                                               true ) },
                { "HighJump",               new ConfigItemBool( this.Config, "Traits", "HighJump",               "IIB Muscle Fibers/High Jumper - Hold [Jump] to Super Jump.",                                                                      true ) },
                { "HorizontalDarkness",     new ConfigItemBool( this.Config, "Traits", "HorizontalDarkness",     "Tunnel Vision - Everything that is not on the same level as the player is black.",                                                true ) },
                { "ItemsGoFlying",          new ConfigItemBool( this.Config, "Traits", "ItemsGoFlying",          "Dyspraxia/Butter Fingers - Items go flying!",                                                                                     true ) },
                { "LongerCD",               new ConfigItemBool( this.Config, "Traits", "LongerCD",               "Chronic Fatigue Syndrome/Exhausted - All Spells and Talents have a cooldown.",                                                    true ) },
                { "LowerGravity",           new ConfigItemBool( this.Config, "Traits", "LowerGravity",           "Hollow Bones - You fall slowly.",                                                                                                 true ) },
                { "LowerStorePrice",        new ConfigItemBool( this.Config, "Traits", "LowerStorePrice",        "Charismatic - 15% gold discount from all shopkeeps.",                                                                             true ) },
                { "MagicBoost",             new ConfigItemBool( this.Config, "Traits", "MagicBoost",             "Bookish - +50% Magic Damage and +50 Mana Capacity. -30% HP.",                                                                     true ) },
                { "ManaCostAndDamageUp",    new ConfigItemBool( this.Config, "Traits", "ManaCostAndDamageUp",    "Emotional Dysregularity/Overcompensation - Mana costs and spell damage are increased by 100%.",                                   true ) },
                { "ManaFromHurt",           new ConfigItemBool( this.Config, "Traits", "ManaFromHurt",           "Masochism - Gain 50% of your mana when hit, but can't regain mana from attacks.",                                                 true ) },
                { "MapReveal",              new ConfigItemBool( this.Config, "Traits", "MapReveal",              "Cartographer - Map is revealed but you have no position marker.",                                                                 true ) },
                { "MegaHealth",             new ConfigItemBool( this.Config, "Traits", "MegaHealth",             "Hero Complex - 100% more Health but you can't heal, ever.",                                                                       true ) },
                { "NoColor",                new ConfigItemBool( this.Config, "Traits", "NoColor",                "Colorblind - You can't see colors.",                                                                                              true ) },
                { "NoEnemyHealthBar",       new ConfigItemBool( this.Config, "Traits", "NoEnemyHealthBar",       "Alexithymia/Unempathetic - Can't see damage dealt.",                                                                              true ) },
                { "NoHealthBar",            new ConfigItemBool( this.Config, "Traits", "NoHealthBar",            "C.I.P - Can't see your health.",                                                                                                  true ) },
                { "NoImmunityWindow",       new ConfigItemBool( this.Config, "Traits", "NoImmunityWindow",       "Algesia - No immunity window after taking damage.",                                                                               true ) },
                { "NoMeat",                 new ConfigItemBool( this.Config, "Traits", "NoMeat",                 "Vegan - Eating food hurts you.",                                                                                                  true ) },
                { "OldYellowTint",          new ConfigItemBool( this.Config, "Traits", "OldYellowTint",          "Nostalgic - Everything is old-timey tinted.",                                                                                     true ) },
                { "OmniDash",               new ConfigItemBool( this.Config, "Traits", "OmniDash",               "Superfluid - -20% Health, but you can dash in ANY direction.",                                                                    true ) },
                { "OneHitDeath",            new ConfigItemBool( this.Config, "Traits", "OneHitDeath",            "Osteogenesis Imperfecta/Fragile - You die in one hit.",                                                                           true ) },
                { "PlayerKnockedFar",       new ConfigItemBool( this.Config, "Traits", "PlayerKnockedFar",       "Ectomorph - Taking damage knocks you far away.",                                                                                  true ) },
                { "PlayerKnockedLow",       new ConfigItemBool( this.Config, "Traits", "PlayerKnockedLow",       "Endomorph - You barely flinch when enemies hit you.",                                                                             true ) },
                { "RandomizeKit",           new ConfigItemBool( this.Config, "Traits", "RandomizeKit",           "Contrarian/Innovator - Your Weapon and Talent are randomized.",                                                                   true ) },
                { "RevealAllChests",        new ConfigItemBool( this.Config, "Traits", "RevealAllChests",        "Spelunker - -10% HP but you can see all chests on the map!",                                                                      true ) },
                { "SkillCritsOnly",         new ConfigItemBool( this.Config, "Traits", "SkillCritsOnly",         "Perfectionist - Only Skill Crits and Spin Kicks deal damage.",                                                                    true ) },
                { "SmallHitbox",            new ConfigItemBool( this.Config, "Traits", "SmallHitbox",            "Disattuned/Only Heart - 25% less health, but you can only be hit in the heart.",                                                  true ) },
                { "SuperFart",              new ConfigItemBool( this.Config, "Traits", "SuperFart",              "Super IBS - Super Fart Talent that releases a cloud that inflicts 3 seconds of Burn on enemies and launches the player upwards.", true ) },
                { "SuperHealer",            new ConfigItemBool( this.Config, "Traits", "SuperHealer",            "Hypercoagulation/Super Healer - HP regenerates, but you lose some Max HP when hit.",                                              true ) },
                { "TwinRelics",             new ConfigItemBool( this.Config, "Traits", "TwinRelics",             "Compulsive Hoarder/Hoarder - All Relics are Twin Relics (when possible).",                                                        true ) },
                { "UpsideDown",             new ConfigItemBool( this.Config, "Traits", "UpsideDown",             "Vertigo - Everything is upside-down.",                                                                                            true ) },
                { "Vampire",                new ConfigItemBool( this.Config, "Traits", "Vampire",                "Vampirism - Gain 20% of your Weapon Damage as Health, but you take 125% more damage.",                                            true ) },
                { "YouAreBlue",             new ConfigItemBool( this.Config, "Traits", "YouAreBlue",             "Methemoglobinemia/Blue - You are blue.",                                                                                          true ) },
                { "YouAreLarge",            new ConfigItemBool( this.Config, "Traits", "YouAreLarge",            "Gigantism - You are gigantic.",                                                                                                   true ) },
                { "YouAreSmall",            new ConfigItemBool( this.Config, "Traits", "YouAreSmall",            "Dwarfism - You are Tiny. (Required to access a Scar in Axis Mundi)",                                                              true ) },
                
                // These are not listed on the wiki, or are listed as removed, so probably don't need them?
                { "AngryOnHit",             new ConfigItemBool( this.Config, "Traits", "AngryOnHit",             "I.E.D/Quick to Anger - Damage and movement speed bonus when hit.",                                                                true ) },
                { "BlurryClose",            new ConfigItemBool( this.Config, "Traits", "BlurryClose",            "Optical Migraine - Everything up close is blurry.",                                                                               true ) },
                { "BlurryFar",              new ConfigItemBool( this.Config, "Traits", "BlurryFar",              "Near-sighted - Everything far away is blurry.",                                                                                   true ) },
                { "DarkScreen",             new ConfigItemBool( this.Config, "Traits", "DarkScreen",             "Glaucoma - Darkness surrounds you.",                                                                                              true ) },
                { "DisableAttackLock",      new ConfigItemBool( this.Config, "Traits", "DisableAttackLock",      "Flexible - You can turn while attacking.",                                                                                        true ) },
                { "FreeRelic",              new ConfigItemBool( this.Config, "Traits", "FreeRelic",              "Treasure Hunter - Start with a random Relic.",                                                                                    true ) },
                { "GameShake",              new ConfigItemBool( this.Config, "Traits", "GameShake",              "Clonus - Game randomly shakes.",                                                                                                  true ) },
                { "HalloweenHoliday",       new ConfigItemBool( this.Config, "Traits", "HalloweenHoliday",       "Medium - Everything is spooky!",                                                                                                  true ) },
                { "InvulnDash",             new ConfigItemBool( this.Config, "Traits", "InvulnDash",             "Evasive - Invincible while dashing, but you have 50% less hp, and dashing dodges has a cooldown.",                                true ) },
                { "MushroomGrow",           new ConfigItemBool( this.Config, "Traits", "MushroomGrow",           "Fun-Guy/Fun-Gal/Mushroom Man/Mushroom Lady - You really like mushrooms.",                                                         true ) },
                { "NoManaCap",              new ConfigItemBool( this.Config, "Traits", "NoManaCap",              "IED/Overexerter - You can exceed your Mana Capacity but will take damage over time for it.",                                      true ) },
                { "NotMovingSlowGame",      new ConfigItemBool( this.Config, "Traits", "NotMovingSlowGame",      "Hyperreflexia - Time moves only when you move.",                                                                                  true ) },
                { "Oversaturate",           new ConfigItemBool( this.Config, "Traits", "Oversaturate",           "Tetrachromat - All colours are deeper",                                                                                           true ) },
                { "Retro",                  new ConfigItemBool( this.Config, "Traits", "Retro",                  "Antiquarian - Everything is retro.",                                                                                              true ) },
                { "ShowEnemiesOnMap",       new ConfigItemBool( this.Config, "Traits", "ShowEnemiesOnMap",       "Eiditic Memory - Enemies are shown on map.",                                                                                      true ) },
                { "Swearing",               new ConfigItemBool( this.Config, "Traits", "Swearing",               "Coprolalia - You swear when struck.",                                                                                             true ) },
                { "WordScramble",           new ConfigItemBool( this.Config, "Traits", "WordScramble",           "Dyslexia - Words are scrambled.",                                                                                                 true ) },

                // These have no in-game description, so don't use them
              //{ "Alcoholic",              new ConfigItemBool( this.Config, "Traits", "Alcoholic",              "Lactose Intolerant - [NO DESCRIPTION] Maybe not fully implemented?",                                                              true ) },
              //{ "BackwardSpell",          new ConfigItemBool( this.Config, "Traits", "BackwardSpell",          "Ambilevous/Flippy - [NO DESCRIPTION] Maybe not fully implemented?",                                                               true ) },
              //{ "Bald",                   new ConfigItemBool( this.Config, "Traits", "Bald",                   "Bald - [NO DESCRIPTION] Maybe not fully implemented?",                                                                            true ) },
              //{ "BiomeBigger",            new ConfigItemBool( this.Config, "Traits", "BiomeBigger",            "Claustrophobia - [NO DESCRIPTION] Maybe not fully implemented?",                                                                  true ) },
              //{ "BiomeSmaller",           new ConfigItemBool( this.Config, "Traits", "BiomeSmaller",           "Agoraphobia - [NO DESCRIPTION] Maybe not fully implemented?",                                                                     true ) },
              //{ "CameraZoomIn",           new ConfigItemBool( this.Config, "Traits", "CameraZoomIn",           "Macropesia - [NO DESCRIPTION] Maybe not fully implemented?",                                                                      true ) },
              //{ "CameraZoomOut",          new ConfigItemBool( this.Config, "Traits", "CameraZoomOut",          "Eagle Eye - [NO DESCRIPTION] Maybe not fully implemented?",                                                                       true ) },
              //{ "CantSeeChildren",        new ConfigItemBool( this.Config, "Traits", "CantSeeChildren",        "Prosopagnosia - [NO DESCRIPTION] Maybe not fully implemented?",                                                                   true ) },
              //{ "ChickensAreEnemies",     new ConfigItemBool( this.Config, "Traits", "ChickensAreEnemies",     "Alektorophobia/Chicken - [NO DESCRIPTION] Maybe not fully implemented?",                                                          true ) },
              //{ "CuteGame",               new ConfigItemBool( this.Config, "Traits", "CuteGame",               "Optimist - [NO DESCRIPTION] Maybe not fully implemented?",                                                                        true ) },
              //{ "Dog",                    new ConfigItemBool( this.Config, "Traits", "Dog",                    "Lycanthropy - [NO DESCRIPTION] Maybe not fully implemented?",                                                                     true ) },
              //{ "FakeEnemies",            new ConfigItemBool( this.Config, "Traits", "FakeEnemies",            "Hallucinations - [NO DESCRIPTION] Maybe not fully implemented?",                                                                  true ) },
              //{ "FastTeleport",           new ConfigItemBool( this.Config, "Traits", "FastTeleport",           "Conductor - [NO DESCRIPTION] Maybe not fully implemented?",                                                                       true ) },
              //{ "FindBoss",               new ConfigItemBool( this.Config, "Traits", "FindBoss",               "Big-Game Hunter - [NO DESCRIPTION] Maybe not fully implemented?",                                                                 true ) },
              //{ "FoodSlow",               new ConfigItemBool( this.Config, "Traits", "FoodSlow",               "Soporific - [NO DESCRIPTION] Maybe not fully implemented?",                                                                       true ) },
              //{ "ForcedChoice",           new ConfigItemBool( this.Config, "Traits", "ForcedChoice",           "Unhealthy Curiosity - [NO DESCRIPTION] Maybe not fully implemented?",                                                             true ) },
              //{ "GameRunsFaster",         new ConfigItemBool( this.Config, "Traits", "GameRunsFaster",         "ADHD - [NO DESCRIPTION] Maybe not fully implemented?",                                                                            true ) },
              //{ "HighBounce",             new ConfigItemBool( this.Config, "Traits", "HighBounce",             "Bubbly - [NO DESCRIPTION] Maybe not fully implemented?",                                                                          true ) },
              //{ "KickResetMobility",      new ConfigItemBool( this.Config, "Traits", "KickResetMobility",      "Freerunner - [NO DESCRIPTION] Maybe not fully implemented?",                                                                      true ) },
              //{ "LifeTimer",              new ConfigItemBool( this.Config, "Traits", "LifeTimer",              "Cardiomyopathy - [NO DESCRIPTION] Maybe not fully implemented?",                                                                  true ) },
              //{ "MagnetRangeBoost",       new ConfigItemBool( this.Config, "Traits", "MagnetRangeBoost",       "Biomagnetic - [NO DESCRIPTION] Maybe not fully implemented?",                                                                     true ) },
              //{ "NoMap",                  new ConfigItemBool( this.Config, "Traits", "NoMap",                  "Anterograde Amnesia - [NO DESCRIPTION] Maybe not fully implemented?",                                                             true ) },
              //{ "NoProjectileIndicators", new ConfigItemBool( this.Config, "Traits", "NoProjectileIndicators", "Poor Periphery - [NO DESCRIPTION] Maybe not fully implemented?",                                                                  true ) },
              //{ "OneChild",               new ConfigItemBool( this.Config, "Traits", "OneChild",               "Only child - [NO DESCRIPTION] Maybe not fully implemented?",                                                                      true ) },
              //{ "RandomDamage",           new ConfigItemBool( this.Config, "Traits", "RandomDamage",           "Dungeon Master - [NO DESCRIPTION] Maybe not fully implemented?",                                                                  true ) },
              //{ "RandomizeSpells",        new ConfigItemBool( this.Config, "Traits", "RandomizeSpells",        "Savant - [NO DESCRIPTION] Maybe not fully implemented?",                                                                          true ) },
              //{ "RandomizeWeapons",       new ConfigItemBool( this.Config, "Traits", "RandomizeWeapons",       "Showboater - [NO DESCRIPTION] Maybe not fully implemented?",                                                                      true ) },
              //{ "RandomSounds",           new ConfigItemBool( this.Config, "Traits", "RandomSounds",           "Schizophrenia - [NO DESCRIPTION] Maybe not fully implemented?",                                                                   true ) },
              //{ "SlowTimeTrigger",        new ConfigItemBool( this.Config, "Traits", "SlowTimeTrigger",        "Hyper Concentration - [NO DESCRIPTION] Maybe not fully implemented?",                                                             true ) },
              //{ "SummerHoliday",          new ConfigItemBool( this.Config, "Traits", "SummerHoliday",          "Surfer - [NO DESCRIPTION] Maybe not fully implemented?",                                                                          true ) },
              //{ "WeaponSpellSwitch",      new ConfigItemBool( this.Config, "Traits", "WeaponSpellSwitch",      "Left-Handed - [NO DESCRIPTION] Maybe not fully implemented?",                                                                     true ) },
              //{ "WinterHoliday",          new ConfigItemBool( this.Config, "Traits", "WinterHoliday",          "Festive - [NO DESCRIPTION] Maybe not fully implemented?",                                                                         true ) },
            };
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        [HarmonyPatch( typeof( TraitType_RL ), nameof( TraitType_RL.TypeArray ), MethodType.Getter )]
        static class TraitType_RL_TypeArray_Patch {
            private static bool runOnce = false;

            static void Prefix() {
                if( !runOnce ) {
                    List<TraitType> typeList = new List<TraitType>( (TraitType[])Traverse.Create( typeof( TraitType_RL ) ).Field( "m_typeArray" ).GetValue() );
                    for( int i = typeList.Count - 1; i >= 0; i-- ) {
                        TraitData traitData = TraitLibrary.GetTraitData( typeList[i] );
                        if( traitData != null ) {
                            // Create a variable for the config file element
                            ConfigItem<bool> traitConfig;
                            // Search the config for a setting that has the same name as the internal name of the trait
                            if( configTraits.TryGetValue( traitData.Name, out traitConfig ) ) {
                                if( !traitConfig.Value ) {
                                    typeList.RemoveAt( i );
                                    WobPlugin.Log( "Banning trait " + traitData.Name );
                                }
                            }
                        }
                    }
                    Traverse.Create( typeof( TraitType_RL ) ).Field( "m_typeArray" ).SetValue( typeList.ToArray() );
                    runOnce = true;
                }
            }
        }

        // This patch simply dumps trair data to the debug log when the Manor skill tree is opened - useful for getting internal names and default values for the traits
        /*[HarmonyPatch( typeof( SkillTreeWindowController ), nameof( SkillTreeWindowController.Initialize ) )]
        static class SkillTreeWindowController_Initialize_Patch {
            static void Postfix( SkillTreeWindowController __instance ) {
                foreach( TraitType traitType in TraitType_RL.TypeArray ) {
                    if( traitType != TraitType.None ) {
                        TraitData traitData = TraitLibrary.GetTraitData( traitType );
                        if( traitData != null ) {
                            WobPlugin.Log( traitData.Name + "|" + traitData.GoldBonus + "|" + WobPlugin.GetTraitTitles( traitData ) );
                            //WobPlugin.Log( traitData.Name + "|" + traitData.GoldBonus + "|" + WobPlugin.GetTraitTitles( traitData ) + "|" + LocalizationManager.GetString( traitData.Description, false, false ) + "|" + LocalizationManager.GetString( traitData.Description_2, false, false ) );
                        }
                    }
                }
            }
        }*/
    }
}