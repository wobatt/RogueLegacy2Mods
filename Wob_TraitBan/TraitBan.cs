using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Wob_Common;

namespace Wob_TraitBan {
    [BepInPlugin( "Wob.TraitBan", "Trait Ban Mod", "1.0.0" )]
    [BepInIncompatibility( "Wob.TraitStats" )]
    public partial class TraitBan : BaseUnityPlugin {
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Boolean( "Traits", "Antique",                "Antique - Heir starts with a random relic.",                                                                                      true ),
                new WobSettings.Boolean( "Traits", "BlurOnHit",              "Panic Attacks/Stressed - Getting hit darkens the screen.",                                                                        true ),
                new WobSettings.Boolean( "Traits", "BonusChestGold",         "Compulsive Gambling/Lootbox Addict - Only chests drop gold and chest values swing wildly!",                                       true ),
                new WobSettings.Boolean( "Traits", "BonusMagicStrength",     "Crippling Intellect - -50% Health and Weapon Damage. Mana regenerates over time.",                                                true ),
                new WobSettings.Boolean( "Traits", "BounceTerrain",          "Clownanthropy - 30% less Health, but you can Spin Kick off terrain.",                                                             true ),
                new WobSettings.Boolean( "Traits", "BreakPropsForMana",      "Minimalist/Breaker - Breaking things restores Mana.",                                                                             true ),
                new WobSettings.Boolean( "Traits", "CantAttack",             "Pacifist - -60% Health and you can't deal damage.",                                                                               true ),
                new WobSettings.Boolean( "Traits", "CheerOnKills",           "Diva - Everyone gets a spotlight but all eyes are on you.",                                                                       true ),
                new WobSettings.Boolean( "Traits", "ColorTrails",            "Synesthesia - Everything leaves behind color.",                                                                                   true ),
                new WobSettings.Boolean( "Traits", "DamageBoost",            "Combative - +50% Weapon Damage, -25% Health.",                                                                                    true ),
                new WobSettings.Boolean( "Traits", "DisarmOnHurt",           "FND/Shocked - Taking damage inflicts Disarmed, meaning the player cannot attack or use spells for 2 seconds.",                    true ),
                new WobSettings.Boolean( "Traits", "EasyBreakables",         "Clumsy - Objects break on touch.",                                                                                                true ),
                new WobSettings.Boolean( "Traits", "EnemiesBlackFill",       "Associative Agnosia - Enemies are blacked out.",                                                                                  true ),
                new WobSettings.Boolean( "Traits", "EnemiesCensored",        "Puritan - Enemies are censored.",                                                                                                 true ),
                new WobSettings.Boolean( "Traits", "EnemyKnockedFar",        "Hypergonadism - Enemies are knocked far away.",                                                                                   true ),
                new WobSettings.Boolean( "Traits", "EnemyKnockedLow",        "Muscle Weakness - Enemies barely flinch when hit.",                                                                               true ),
                new WobSettings.Boolean( "Traits", "ExplosiveChests",        "Paranoid/Explosive Chests - Chests drop an explosive surprise.",                                                                  true ),
                new WobSettings.Boolean( "Traits", "ExplosiveEnemies",       "Exploding Casket Syndrome/Explosive Enemies - Enemies drop an explosive surprise.",                                               true ),
                new WobSettings.Boolean( "Traits", "FakeSelfDamage",         "Histrionic - Numbers are exaggerated.",                                                                                           true ),
                new WobSettings.Boolean( "Traits", "Fart",                   "IBS - Sometimes fart when jumping or dashing.",                                                                                   true ),
                new WobSettings.Boolean( "Traits", "FMFFan",                 "FMF Fan - You're probably Korean. (No effect)",                                                                                   true ),
                new WobSettings.Boolean( "Traits", "GainDownStrike",         "Aerodynamic - Your Spinkick is replaced with Downstrike.",                                                                        true ),
                new WobSettings.Boolean( "Traits", "Gay",                    "Nature - Being true to being you. (No effect)",                                                                                   true ),
                new WobSettings.Boolean( "Traits", "HighJump",               "IIB Muscle Fibers/High Jumper - Hold [Jump] to Super Jump.",                                                                      true ),
                new WobSettings.Boolean( "Traits", "ItemsGoFlying",          "Dyspraxia/Butter Fingers - Items go flying!",                                                                                     true ),
                new WobSettings.Boolean( "Traits", "LongerCD",               "Chronic Fatigue Syndrome/Exhausted - All Spells and Talents have a cooldown.",                                                    true ),
                new WobSettings.Boolean( "Traits", "LowerGravity",           "Hollow Bones - You fall slowly.",                                                                                                 true ),
                new WobSettings.Boolean( "Traits", "LowerStorePrice",        "Charismatic - 15% gold discount from all shopkeeps.",                                                                             true ),
                new WobSettings.Boolean( "Traits", "MagicBoost",             "Bookish - +50% Magic Damage and +50 Mana Capacity. -25% HP.",                                                                     true ),
                new WobSettings.Boolean( "Traits", "ManaCostAndDamageUp",    "Emotional Dysregularity/Overcompensation - Mana costs and spell damage are increased by 100%.",                                   true ),
                new WobSettings.Boolean( "Traits", "ManaFromHurt",           "Masochism - Gain 50% of your mana when hit, but can't regain mana from attacks.",                                                 true ),
                new WobSettings.Boolean( "Traits", "MapReveal",              "Cartographer - Map is revealed but you have no position marker.",                                                                 true ),
                new WobSettings.Boolean( "Traits", "MegaHealth",             "Hero Complex - 100% more Health but you can't heal, ever.",                                                                       true ),
                new WobSettings.Boolean( "Traits", "NoColor",                "Colorblind - You can't see colors.",                                                                                              true ),
                new WobSettings.Boolean( "Traits", "NoEnemyHealthBar",       "Alexithymia/Unempathetic - Can't see damage dealt.",                                                                              true ),
                new WobSettings.Boolean( "Traits", "NoHealthBar",            "C.I.P - Can't see your health.",                                                                                                  true ),
                new WobSettings.Boolean( "Traits", "NoImmunityWindow",       "Algesia - No immunity window after taking damage.",                                                                               true ),
                new WobSettings.Boolean( "Traits", "NoMeat",                 "Vegan - Eating food hurts you.",                                                                                                  true ),
                new WobSettings.Boolean( "Traits", "OldYellowTint",          "Nostalgic - Everything is old-timey tinted.",                                                                                     true ),
                new WobSettings.Boolean( "Traits", "OmniDash",               "Superfluid - -20% Health, but you can dash in ANY direction.",                                                                    true ),
                new WobSettings.Boolean( "Traits", "OneHitDeath",            "One-Hit Wonder/Fragile - You die in one hit.",                                                                                    true ),
                new WobSettings.Boolean( "Traits", "PlayerKnockedFar",       "Ectomorph - Taking damage knocks you far away.",                                                                                  true ),
                new WobSettings.Boolean( "Traits", "PlayerKnockedLow",       "Endomorph - You barely flinch when enemies hit you.",                                                                             true ),
                new WobSettings.Boolean( "Traits", "RandomizeKit",           "Contrarian/Innovator - Your Weapon and Talent are randomized.",                                                                   true ),
                new WobSettings.Boolean( "Traits", "RevealAllChests",        "Spelunker - -10% HP but you can see all chests on the map!",                                                                      true ),
                new WobSettings.Boolean( "Traits", "SkillCritsOnly",         "Perfectionist - Only Skill Crits and Spin Kicks deal damage.",                                                                    true ),
                new WobSettings.Boolean( "Traits", "SmallHitbox",            "Disattuned/Only Heart - 25% less health, but you can only be hit in the heart.",                                                  true ),
                new WobSettings.Boolean( "Traits", "SuperFart",              "Super IBS - Super Fart Talent that releases a cloud that inflicts 3 seconds of Burn on enemies and launches the player upwards.", true ),
                new WobSettings.Boolean( "Traits", "SuperHealer",            "Hypercoagulation/Super Healer - HP regenerates, but you lose some Max HP when hit.",                                              true ),
                new WobSettings.Boolean( "Traits", "TwinRelics",             "Compulsive Hoarder/Hoarder - All Relics are Twin Relics (when possible).",                                                        true ),
                new WobSettings.Boolean( "Traits", "Vampire",                "Vampirism - Gain 20% of your Weapon Damage as Health, but you take 125% more damage.",                                            true ),
                new WobSettings.Boolean( "Traits", "YouAreBlue",             "Methemoglobinemia/Blue - You are blue. (No effect)",                                                                              true ),
                new WobSettings.Boolean( "Traits", "YouAreLarge",            "Gigantism - You are gigantic.",                                                                                                   true ),
                new WobSettings.Boolean( "Traits", "YouAreSmall",            "Dwarfism - You are Tiny. (Required to access a Scar in Axis Mundi)",                                                              true ),
            } );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

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
                        // Get the trait data that includes rarity info
                        TraitData traitData = TraitLibrary.GetTraitData( traitType );
                        if( traitData != null ) {
                            // Check that the rarity is within the range looked at during character generation
                            // Get the value of the setting that has the same name as the internal name of the trait
                            if( ( traitData.Rarity >= 1 && traitData.Rarity <= 3 ) && !WobSettings.Get( "Traits", traitData.Name, true ) ) {
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

        // Patch for random trait generation to remove the additional chance of Antique spawn if it is banned
        [HarmonyPatch( typeof( CharacterCreator ), nameof( CharacterCreator.GetRandomTraits ) )]
        static class CharacterCreator_GetRandomTraits_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "CharacterCreator.GetRandomTraits Transpiler Patch" );
                if( !WobSettings.Get( "Traits", "Antique", true ) ) {
                    // Set up the transpiler handler with the instruction list
                    WobTranspiler transpiler = new WobTranspiler( instructions );
                    // Perform the patching
                    transpiler.PatchAll(
                            // Define the IL code instructions that should be matched
                            new List<WobTranspiler.OpTest> {
                                /*  0 */ new WobTranspiler.OpTest( OpCodes.Call, name: "GetRandomNumber" ), // RNGManager.GetRandomNumber(RngID.Lineage, "GetAntiqueSpawnChance", 0f, 1f)
                                /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4                        ), // 0.22f
                                /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Bge_Un                      ), // if (RNGManager.GetRandomNumber(RngID.Lineage, "GetAntiqueSpawnChance", 0f, 1f) < 0.22f)
                            },
                            // Define the actions to take when an occurrence is found
                            new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_SetOperand( 1, 0f ),
                            } );
                    // Return the modified instructions
                    return transpiler.GetResult();
                } else {
                    return instructions;
                }
            }
        }
    }
}