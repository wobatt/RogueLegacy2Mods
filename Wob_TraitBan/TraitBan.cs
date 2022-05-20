using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using Wob_Common;

namespace Wob_TraitBan {
    [BepInPlugin( "Wob.TraitBan", "Trait Ban Mod", "0.2" )]
    public partial class TraitBan : BaseUnityPlugin {
        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobPlugin.Settings.Add( new WobSettings.Entry[] {
                new WobSettings.EntryBool( "Traits", "Antique",                "Antique - Heir starts with a random relic.",                                                                                      true ),
                new WobSettings.EntryBool( "Traits", "BlurOnHit",              "Panic Attacks/Stressed - Getting hit darkens the screen.",                                                                        true ),
                new WobSettings.EntryBool( "Traits", "BonusChestGold",         "Compulsive Gambling/Lootbox Addict - Only chests drop gold and chest values swing wildly!",                                       true ),
                new WobSettings.EntryBool( "Traits", "BonusMagicStrength",     "Crippling Intellect - -50% Health and Weapon Damage. Mana regenerates over time.",                                                true ),
                new WobSettings.EntryBool( "Traits", "BounceTerrain",          "Clownanthropy - 30% less Health, but you can Spin Kick off terrain.",                                                             true ),
                new WobSettings.EntryBool( "Traits", "BreakPropsForMana",      "Minimalist/Breaker - Breaking things restores Mana.",                                                                             true ),
                new WobSettings.EntryBool( "Traits", "CantAttack",             "Pacifist - -60% Health and you can't deal damage.",                                                                               true ),
                new WobSettings.EntryBool( "Traits", "CheerOnKills",           "Diva - Everyone gets a spotlight but all eyes are on you.",                                                                       true ),
                new WobSettings.EntryBool( "Traits", "ColorTrails",            "Synesthesia - Everything leaves behind color.",                                                                                   true ),
                new WobSettings.EntryBool( "Traits", "DamageBoost",            "Combative - +50% Weapon Damage, -25% Health.",                                                                                    true ),
                new WobSettings.EntryBool( "Traits", "DisarmOnHurt",           "FND/Shocked - Taking damage inflicts Disarmed, meaning the player cannot attack or use spells for 2 seconds.",                    true ),
                new WobSettings.EntryBool( "Traits", "EasyBreakables",         "Clumsy - Objects break on touch.",                                                                                                true ),
                new WobSettings.EntryBool( "Traits", "EnemiesBlackFill",       "Associative Agnosia - Enemies are blacked out.",                                                                                  true ),
                new WobSettings.EntryBool( "Traits", "EnemiesCensored",        "Puritan - Enemies are censored.",                                                                                                 true ),
                new WobSettings.EntryBool( "Traits", "EnemyKnockedFar",        "Hypergonadism - Enemies are knocked far away.",                                                                                   true ),
                new WobSettings.EntryBool( "Traits", "EnemyKnockedLow",        "Muscle Weakness - Enemies barely flinch when hit.",                                                                               true ),
                new WobSettings.EntryBool( "Traits", "ExplosiveChests",        "Paranoid/Explosive Chests - Chests drop an explosive surprise.",                                                                  true ),
                new WobSettings.EntryBool( "Traits", "ExplosiveEnemies",       "Exploding Casket Syndrome/Explosive Enemies - Enemies drop an explosive surprise.",                                               true ),
                new WobSettings.EntryBool( "Traits", "FakeSelfDamage",         "Histrionic - Numbers are exaggerated.",                                                                                           true ),
                new WobSettings.EntryBool( "Traits", "Fart",                   "IBS - Sometimes fart when jumping or dashing.",                                                                                   true ),
                new WobSettings.EntryBool( "Traits", "FMFFan",                 "FMF Fan - You're probably Korean. (No effect)",                                                                                   true ),
                new WobSettings.EntryBool( "Traits", "GainDownStrike",         "Aerodynamic - Your Spinkick is replaced with Downstrike.",                                                                        true ),
                new WobSettings.EntryBool( "Traits", "Gay",                    "Nature - Being true to being you. (No effect)",                                                                                   true ),
                new WobSettings.EntryBool( "Traits", "HighJump",               "IIB Muscle Fibers/High Jumper - Hold [Jump] to Super Jump.",                                                                      true ),
                new WobSettings.EntryBool( "Traits", "ItemsGoFlying",          "Dyspraxia/Butter Fingers - Items go flying!",                                                                                     true ),
                new WobSettings.EntryBool( "Traits", "LongerCD",               "Chronic Fatigue Syndrome/Exhausted - All Spells and Talents have a cooldown.",                                                    true ),
                new WobSettings.EntryBool( "Traits", "LowerGravity",           "Hollow Bones - You fall slowly.",                                                                                                 true ),
                new WobSettings.EntryBool( "Traits", "LowerStorePrice",        "Charismatic - 15% gold discount from all shopkeeps.",                                                                             true ),
                new WobSettings.EntryBool( "Traits", "MagicBoost",             "Bookish - +50% Magic Damage and +50 Mana Capacity. -25% HP.",                                                                     true ),
                new WobSettings.EntryBool( "Traits", "ManaCostAndDamageUp",    "Emotional Dysregularity/Overcompensation - Mana costs and spell damage are increased by 100%.",                                   true ),
                new WobSettings.EntryBool( "Traits", "ManaFromHurt",           "Masochism - Gain 50% of your mana when hit, but can't regain mana from attacks.",                                                 true ),
                new WobSettings.EntryBool( "Traits", "MapReveal",              "Cartographer - Map is revealed but you have no position marker.",                                                                 true ),
                new WobSettings.EntryBool( "Traits", "MegaHealth",             "Hero Complex - 100% more Health but you can't heal, ever.",                                                                       true ),
                new WobSettings.EntryBool( "Traits", "NoColor",                "Colorblind - You can't see colors.",                                                                                              true ),
                new WobSettings.EntryBool( "Traits", "NoEnemyHealthBar",       "Alexithymia/Unempathetic - Can't see damage dealt.",                                                                              true ),
                new WobSettings.EntryBool( "Traits", "NoHealthBar",            "C.I.P - Can't see your health.",                                                                                                  true ),
                new WobSettings.EntryBool( "Traits", "NoImmunityWindow",       "Algesia - No immunity window after taking damage.",                                                                               true ),
                new WobSettings.EntryBool( "Traits", "NoMeat",                 "Vegan - Eating food hurts you.",                                                                                                  true ),
                new WobSettings.EntryBool( "Traits", "OldYellowTint",          "Nostalgic - Everything is old-timey tinted.",                                                                                     true ),
                new WobSettings.EntryBool( "Traits", "OmniDash",               "Superfluid - -20% Health, but you can dash in ANY direction.",                                                                    true ),
                new WobSettings.EntryBool( "Traits", "OneHitDeath",            "One-Hit Wonder/Fragile - You die in one hit.",                                                                                    true ),
                new WobSettings.EntryBool( "Traits", "PlayerKnockedFar",       "Ectomorph - Taking damage knocks you far away.",                                                                                  true ),
                new WobSettings.EntryBool( "Traits", "PlayerKnockedLow",       "Endomorph - You barely flinch when enemies hit you.",                                                                             true ),
                new WobSettings.EntryBool( "Traits", "RandomizeKit",           "Contrarian/Innovator - Your Weapon and Talent are randomized.",                                                                   true ),
                new WobSettings.EntryBool( "Traits", "RevealAllChests",        "Spelunker - -10% HP but you can see all chests on the map!",                                                                      true ),
                new WobSettings.EntryBool( "Traits", "SkillCritsOnly",         "Perfectionist - Only Skill Crits and Spin Kicks deal damage.",                                                                    true ),
                new WobSettings.EntryBool( "Traits", "SmallHitbox",            "Disattuned/Only Heart - 25% less health, but you can only be hit in the heart.",                                                  true ),
                new WobSettings.EntryBool( "Traits", "SuperFart",              "Super IBS - Super Fart Talent that releases a cloud that inflicts 3 seconds of Burn on enemies and launches the player upwards.", true ),
                new WobSettings.EntryBool( "Traits", "SuperHealer",            "Hypercoagulation/Super Healer - HP regenerates, but you lose some Max HP when hit.",                                              true ),
                new WobSettings.EntryBool( "Traits", "TwinRelics",             "Compulsive Hoarder/Hoarder - All Relics are Twin Relics (when possible).",                                                        true ),
                new WobSettings.EntryBool( "Traits", "Vampire",                "Vampirism - Gain 20% of your Weapon Damage as Health, but you take 125% more damage.",                                            true ),
                new WobSettings.EntryBool( "Traits", "YouAreBlue",             "Methemoglobinemia/Blue - You are blue. (No effect)",                                                                              true ),
                new WobSettings.EntryBool( "Traits", "YouAreLarge",            "Gigantism - You are gigantic.",                                                                                                   true ),
                new WobSettings.EntryBool( "Traits", "YouAreSmall",            "Dwarfism - You are Tiny. (Required to access a Scar in Axis Mundi)",                                                              true ),
            } );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        [HarmonyPatch( typeof( TraitType_RL ), nameof( TraitType_RL.TypeArray ), MethodType.Getter )]
        static class TraitType_RL_TypeArray_Patch {
            private static bool runOnce = false;
            static void Prefix() {
                // Only need to run this once, as the new settings are written into the trait data for the session
                if( !runOnce ) {
                    // Get the list of traits from the private field
                    TraitType[] m_typeArray = (TraitType[])Traverse.Create( typeof( TraitType_RL ) ).Field( "m_typeArray" ).GetValue();
                    // Go through each type in the array
                    foreach( TraitType traitType in m_typeArray ) {
                        // Get the trait data that includes rarity info
                        TraitData traitData = TraitLibrary.GetTraitData( traitType );
                        if( traitData != null ) {
                            // Check that the rarity is within the range looked at during character generation
                            // Get the value of the setting that has the same name as the internal name of the trait
                            if( ( traitData.Rarity >= 1 && traitData.Rarity <= 3 ) && !WobPlugin.Settings.Get( "Traits", traitData.Name, true ) ) {
                                // The game seems to use values of 91, 92 and 93 for the rarity of diabled traits, so I will stick to this convention, though any value > 3 would work
                                traitData.Rarity += 90;
                                WobPlugin.Log( "Banning trait " + traitData.Name );
                            }
                        }
                    }
                    runOnce = true;
                }
            }
        }
    }
}