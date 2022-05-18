using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_TraitGold {
    [BepInPlugin( "Wob.TraitGold", "Trait Gold Bonus Mod", "0.2" )]
    public partial class TraitGold : BaseUnityPlugin {
        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobPlugin.Settings.Add( new WobSettings.Entry[] {
                new WobSettings.Entry<float>( "Traits", "Antique",                "Antique - Heir starts with a random relic.",                                                                                      0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "BlurOnHit",              "Panic Attacks/Stressed - Getting hit darkens the screen.",                                                                        0.5f,  bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "BonusChestGold",         "Compulsive Gambling/Lootbox Addict - Only chests drop gold and chest values swing wildly!",                                       0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "BonusMagicStrength",     "Crippling Intellect - -50% Health and Weapon Damage. Mana regenerates over time.",                                                0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "BounceTerrain",          "Clownanthropy - 30% less Health, but you can Spin Kick off terrain.",                                                             0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "BreakPropsForMana",      "Minimalist/Breaker - Breaking things restores Mana.",                                                                             0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "CanNowAttack",           "Pacifier - -60% Health and you love to fight!",                                                                                   1.5f,  bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "CantAttack",             "Pacifist - -60% Health and you can't deal damage.",                                                                               1.5f,  bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "CheerOnKills",           "Diva - Everyone gets a spotlight but all eyes are on you.",                                                                       0.75f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "ColorTrails",            "Synesthesia - Everything leaves behind color.",                                                                                   0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "DamageBoost",            "Combative - +50% Weapon Damage, -25% Health.",                                                                                    0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "DisarmOnHurt",           "FND/Shocked - Taking damage inflicts Disarmed, meaning the player cannot attack or use spells for 2 seconds.",                    0.5f,  bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "EasyBreakables",         "Clumsy - Objects break on touch.",                                                                                                0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "EnemiesBlackFill",       "Associative Agnosia - Enemies are blacked out.",                                                                                  0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "EnemiesCensored",        "Puritan - Enemies are censored.",                                                                                                 0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "EnemyKnockedFar",        "Hypergonadism - Enemies are knocked far away.",                                                                                   0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "EnemyKnockedLow",        "Muscle Weakness - Enemies barely flinch when hit.",                                                                               0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "ExplosiveChests",        "Paranoid/Explosive Chests - Chests drop an explosive surprise.",                                                                  0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "ExplosiveEnemies",       "Exploding Casket Syndrome/Explosive Enemies - Enemies drop an explosive surprise.",                                               0.5f,  bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "FakeSelfDamage",         "Histrionic - Numbers are exaggerated.",                                                                                           0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "Fart",                   "IBS - Sometimes fart when jumping or dashing.",                                                                                   0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "FMFFan",                 "FMF Fan - You're probably Korean. (No effect)",                                                                                   0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "GainDownStrike",         "Aerodynamic - Your Spinkick is replaced with Downstrike.",                                                                        0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "Gay",                    "Nature - Being true to being you. (No effect)",                                                                                   0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "HighJump",               "IIB Muscle Fibers/High Jumper - Hold [Jump] to Super Jump.",                                                                      0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "HorizontalDarkness",     "Tunnel Vision - Everything that is not on the same level as the player is black.",                                                0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "ItemsGoFlying",          "Dyspraxia/Butter Fingers - Items go flying!",                                                                                     0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "LongerCD",               "Chronic Fatigue Syndrome/Exhausted - All Spells and Talents have a cooldown.",                                                    0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "LowerGravity",           "Hollow Bones - You fall slowly.",                                                                                                 0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "LowerStorePrice",        "Charismatic - 15% gold discount from all shopkeeps.",                                                                             0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "MagicBoost",             "Bookish - +50% Magic Damage and +50 Mana Capacity. -25% HP.",                                                                     0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "ManaCostAndDamageUp",    "Emotional Dysregularity/Overcompensation - Mana costs and spell damage are increased by 100%.",                                   0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "ManaFromHurt",           "Masochism - Gain 50% of your mana when hit, but can't regain mana from attacks.",                                                 0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "MapReveal",              "Cartographer - Map is revealed but you have no position marker.",                                                                 0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "MegaHealth",             "Hero Complex - 100% more Health but you can't heal, ever.",                                                                       0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "NoColor",                "Colorblind - You can't see colors.",                                                                                              0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "NoEnemyHealthBar",       "Alexithymia/Unempathetic - Can't see damage dealt.",                                                                              0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "NoHealthBar",            "C.I.P - Can't see your health.",                                                                                                  0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "NoImmunityWindow",       "Algesia - No immunity window after taking damage.",                                                                               0.5f,  bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "NoMeat",                 "Vegan - Eating food hurts you.",                                                                                                  0.75f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "OldYellowTint",          "Nostalgic - Everything is old-timey tinted.",                                                                                     0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "OmniDash",               "Superfluid - -20% Health, but you can dash in ANY direction.",                                                                    0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "OneHitDeath",            "One-Hit Wonder/Fragile - You die in one hit.",                                                                                    2f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "PlayerKnockedFar",       "Ectomorph - Taking damage knocks you far away.",                                                                                  0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "PlayerKnockedLow",       "Endomorph - You barely flinch when enemies hit you.",                                                                             0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "RandomizeKit",           "Contrarian/Innovator - Your Weapon and Talent are randomized.",                                                                   0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "RevealAllChests",        "Spelunker - -10% HP but you can see all chests on the map!",                                                                      0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "SkillCritsOnly",         "Perfectionist - Only Skill Crits and Spin Kicks deal damage.",                                                                    0.5f,  bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "SmallHitbox",            "Disattuned/Only Heart - 25% less health, but you can only be hit in the heart.",                                                  0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "SuperFart",              "Super IBS - Super Fart Talent that releases a cloud that inflicts 3 seconds of Burn on enemies and launches the player upwards.", 0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "SuperHealer",            "Hypercoagulation/Super Healer - HP regenerates, but you lose some Max HP when hit.",                                              0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "TwinRelics",             "Compulsive Hoarder/Hoarder - All Relics are Twin Relics (when possible).",                                                        0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "UpsideDown",             "Vertigo - Everything is upside-down.",                                                                                            0.75f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "Vampire",                "Vampirism - Gain 20% of your Weapon Damage as Health, but you take 125% more damage.",                                            0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "YouAreBlue",             "Methemoglobinemia/Blue - You are blue. (No effect)",                                                                              0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "YouAreLarge",            "Gigantism - You are gigantic.",                                                                                                   0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "YouAreSmall",            "Dwarfism - You are Tiny. (Required to access a Scar in Axis Mundi)",                                                              0.25f, bounds: (0f, float.MaxValue) ),
                
                // These are not listed on the wiki, or are listed as removed, so probably don't need them?
                new WobSettings.Entry<float>( "Traits", "AngryOnHit",             "I.E.D/Quick to Anger - Damage and movement speed bonus when hit.",                                                                0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "BlurryClose",            "Optical Migraine - Everything up close is blurry.",                                                                               0.1f,  bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "BlurryFar",              "Near-sighted - Everything far away is blurry.",                                                                                   0.1f,  bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "DarkScreen",             "Glaucoma - Darkness surrounds you.",                                                                                              0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "DisableAttackLock",      "Flexible - You can turn while attacking.",                                                                                        0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "FreeRelic",              "Treasure Hunter - Start with a random Relic.",                                                                                    0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "GameShake",              "Clonus - Game randomly shakes.",                                                                                                  0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "HalloweenHoliday",       "Medium - Everything is spooky!",                                                                                                  0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "InvulnDash",             "Evasive - Invincible while dashing, but you have 50% less hp, and dashing dodges has a cooldown.",                                0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "MushroomGrow",           "Fun-Guy/Fun-Gal/Mushroom Man/Mushroom Lady - You really like mushrooms.",                                                         0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "NoManaCap",              "IED/Overexerter - You can exceed your Mana Capacity but will take damage over time for it.",                                      0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "NotMovingSlowGame",      "Hyperreflexia - Time moves only when you move.",                                                                                  0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "Oversaturate",           "Tetrachromat - All colours are deeper",                                                                                           0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "Retro",                  "Antiquarian - Everything is retro.",                                                                                              0.25f, bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "ShowEnemiesOnMap",       "Eiditic Memory - Enemies are shown on map.",                                                                                      0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "Swearing",               "Coprolalia - You swear when struck.",                                                                                             0f,    bounds: (0f, float.MaxValue) ),
                new WobSettings.Entry<float>( "Traits", "WordScramble",           "Dyslexia - Words are scrambled.",                                                                                                 0f,    bounds: (0f, float.MaxValue) ),
            } );
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
                        // Get the value of the setting that has the same name as the internal name of the trait
                        float goldBonus = WobPlugin.Settings.Get( "Traits", traitData.Name, traitData.GoldBonus );
                        if( goldBonus != traitData.GoldBonus ) {
                            WobPlugin.Log( "Changing bonus for " + traitData.Name + " (" + WobPlugin.GetTraitTitles( traitData ) + ") from " + traitData.GoldBonus + " to " + goldBonus );
                            // If a matching config setting has been found, calculate the new gold gain using the file value rather than the game value, and overwite the method return value
                            __result = goldBonus * ( 1f + SkillTreeManager.GetSkillTreeObj( SkillTreeType.Traits_Give_Gold_Gain_Mod ).CurrentStatGain );
                        } else {
                            WobPlugin.Log( "Same bonus for " + traitData.Name + " (" + WobPlugin.GetTraitTitles( traitData ) + ") of " + traitData.GoldBonus );
                        }
                    }
                }
                // If any of the 'if' statements fail, don't change the return value that the original method decided on
            }
        }
    }
}