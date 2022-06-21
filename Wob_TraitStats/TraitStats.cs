using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace Wob_TraitStats {
    [BepInPlugin( "Wob.TraitStats", "Trait Stats Mod", "1.1.0" )]
	[BepInIncompatibility( "Wob.TraitGold" )]
	[BepInIncompatibility( "Wob.TraitBan" )]
	// The function of these plugins is replaced by this one
	public partial class TraitStats : BaseUnityPlugin {

		private static readonly WobSettings.KeyHelper<TraitType> keys = new WobSettings.KeyHelper<TraitType>( "Trait" );

		private static readonly Dictionary<TraitType,(string Config, string Name, bool Spawn, int Gold)> TraitInfo = new Dictionary<TraitType,(string Config, string Name, bool Spawn, int Gold)>() {
			{ TraitType.Antique,             ( "Antique",              "Antique - Heir starts with a random relic.",                                                                                      true,  0   ) },
			{ TraitType.BlurOnHit,           ( "BlurOnHit",            "Panic Attacks/Stressed - Getting hit darkens the screen.",                                                                        true,  50  ) },
			{ TraitType.BonusChestGold,      ( "BonusChestGold",       "Compulsive Gambling/Lootbox Addict - Only chests drop gold and chest values swing wildly!",                                       true,  25  ) },
			{ TraitType.BonusMagicStrength,  ( "BonusMagicStrength",   "Crippling Intellect - -50% Health and Weapon Damage. Mana regenerates over time.",                                                true,  0   ) },
			{ TraitType.BounceTerrain,       ( "BounceTerrain",        "Clownanthropy - -30% Health, but you can Spin Kick off terrain.",                                                                 true,  0   ) },
			{ TraitType.BreakPropsForMana,   ( "BreakPropsForMana",    "Minimalist/Breaker - Breaking things restores Mana.",                                                                             true,  0   ) },
			{ TraitType.CanNowAttack,        ( "CanNowAttack",         "Pacifier - -60% Health and you love to fight!",                                                                                   false, 150 ) },
			{ TraitType.CantAttack,          ( "CantAttack",           "Pacifist - -60% Health and you can't deal damage.",                                                                               true,  150 ) },
			{ TraitType.CheerOnKills,        ( "CheerOnKills",         "Diva - Everyone gets a spotlight but all eyes are on you.",                                                                       true,  75  ) },
			{ TraitType.ColorTrails,         ( "ColorTrails",          "Synesthesia - Everything leaves behind color.",                                                                                   true,  25  ) },
			{ TraitType.DamageBoost,         ( "DamageBoost",          "Combative - +50% Weapon Damage, -25% Health.",                                                                                    true,  0   ) },
			{ TraitType.DisarmOnHurt,        ( "DisarmOnHurt",         "FND/Shocked - Taking damage inflicts Disarmed, meaning the player cannot attack or use spells for 2 seconds.",                    true,  50  ) },
			{ TraitType.EasyBreakables,      ( "EasyBreakables",       "Clumsy - Objects break on touch.",                                                                                                true,  0   ) },
			{ TraitType.EnemiesBlackFill,    ( "EnemiesBlackFill",     "Associative Agnosia - Enemies are blacked out.",                                                                                  true,  25  ) },
			{ TraitType.EnemiesCensored,     ( "EnemiesCensored",      "Puritan - Enemies are censored.",                                                                                                 true,  25  ) },
			{ TraitType.EnemyKnockedFar,     ( "EnemyKnockedFar",      "Hypergonadism - Enemies are knocked far away.",                                                                                   true,  0   ) },
			{ TraitType.EnemyKnockedLow,     ( "EnemyKnockedLow",      "Muscle Weakness - Enemies barely flinch when hit.",                                                                               true,  25  ) },
			{ TraitType.ExplosiveChests,     ( "ExplosiveChests",      "Paranoid/Explosive Chests - Chests drop an explosive surprise.",                                                                  true,  25  ) },
			{ TraitType.ExplosiveEnemies,    ( "ExplosiveEnemies",     "Exploding Casket Syndrome/Explosive Enemies - Enemies drop an explosive surprise.",                                               true,  50  ) },
			{ TraitType.FMFFan,              ( "FMFFan",               "FMF Fan - You're probably Korean. (No effect)",                                                                                   true,  25  ) },
			{ TraitType.FakeSelfDamage,      ( "FakeSelfDamage",       "Histrionic - Numbers are exaggerated.",                                                                                           true,  0   ) },
			{ TraitType.Fart,                ( "Fart",                 "IBS - Sometimes fart when jumping or dashing.",                                                                                   true,  0   ) },
			{ TraitType.GainDownStrike,      ( "GainDownStrike",       "Aerodynamic - Your Spinkick is replaced with Downstrike.",                                                                        true,  0   ) },
			{ TraitType.Disposition,         ( "Gay",                  "Nature - Being true to being you. (No effect)",                                                                                   true,  0   ) },
			{ TraitType.HighJump,            ( "HighJump",             "IIB Muscle Fibers/High Jumper - Hold [Jump] to Super Jump.",                                                                      true,  0   ) },
			{ TraitType.ItemsGoFlying,       ( "ItemsGoFlying",        "Dyspraxia/Butter Fingers - Items go flying!",                                                                                     true,  25  ) },
			{ TraitType.LongerCD,            ( "LongerCD",             "Chronic Fatigue Syndrome/Exhausted - All Spells and Talents have a cooldown.",                                                    true,  25  ) },
			{ TraitType.LowerGravity,        ( "LowerGravity",         "Hollow Bones - You fall slowly.",                                                                                                 true,  0   ) },
			{ TraitType.LowerStorePrice,     ( "LowerStorePrice",      "Charismatic - 15% gold discount from all shopkeeps.",                                                                             true,  0   ) },
			{ TraitType.MagicBoost,          ( "MagicBoost",           "Bookish - +50% Magic Damage and +50 Mana Capacity. -25% Health.",                                                                 true,  0   ) },
			{ TraitType.ManaCostAndDamageUp, ( "ManaCostAndDamageUp",  "Emotional Dysregularity/Overcompensation - Mana costs and spell damage are increased by 100%.",                                   true,  0   ) },
			{ TraitType.ManaFromHurt,        ( "ManaFromHurt",         "Masochism - Gain 50% of your mana when hit, but can't regain mana from attacks.",                                                 true,  25  ) },
			{ TraitType.MapReveal,           ( "MapReveal",            "Cartographer - Map is revealed but you have no position marker.",                                                                 true,  25  ) },
			{ TraitType.MegaHealth,          ( "MegaHealth",           "Hero Complex - +100% Health but you can't heal, ever.",                                                                           true,  0   ) },
			{ TraitType.NoColor,             ( "NoColor",              "Colorblind - You can't see colors.",                                                                                              true,  25  ) },
			{ TraitType.NoEnemyHealthBar,    ( "NoEnemyHealthBar",     "Alexithymia/Unempathetic - Can't see damage dealt.",                                                                              true,  25  ) },
			{ TraitType.NoHealthBar,         ( "NoHealthBar",          "C.I.P - Can't see your health.",                                                                                                  true,  25  ) },
			{ TraitType.NoImmunityWindow,    ( "NoImmunityWindow",     "Algesia - No immunity window after taking damage.",                                                                               true,  50  ) },
			{ TraitType.NoMeat,              ( "NoMeat",               "Vegan - Eating food hurts you.",                                                                                                  true,  75  ) },
			{ TraitType.OldYellowTint,       ( "OldYellowTint",        "Nostalgic - Everything is old-timey tinted.",                                                                                     true,  25  ) },
			{ TraitType.OmniDash,            ( "OmniDash",             "Superfluid - -20% Health, but you can dash in ANY direction.",                                                                    true,  0   ) },
			{ TraitType.OneHitDeath,         ( "OneHitDeath",          "One-Hit Wonder/Fragile - You die in one hit.",                                                                                    true,  200 ) },
			{ TraitType.PlayerKnockedFar,    ( "PlayerKnockedFar",     "Ectomorph - Taking damage knocks you far away.",                                                                                  true,  25  ) },
			{ TraitType.PlayerKnockedLow,    ( "PlayerKnockedLow",     "Endomorph - You barely flinch when enemies hit you.",                                                                             true,  0   ) },
			{ TraitType.RandomizeKit,        ( "RandomizeKit",         "Contrarian/Innovator - Your Weapon and Talent are randomized.",                                                                   true,  25  ) },
			{ TraitType.RevealAllChests,     ( "RevealAllChests",      "Spelunker - -10% Health but you can see all chests on the map!",                                                                  true,  0   ) },
			{ TraitType.SkillCritsOnly,      ( "SkillCritsOnly",       "Perfectionist - Only Skill Crits and Spin Kicks deal damage.",                                                                    true,  50  ) },
			{ TraitType.SmallHitbox,         ( "SmallHitbox",          "Disattuned/Only Heart - -25% health, but you can only be hit in the heart.",                                                      true,  0   ) },
			{ TraitType.SuperFart,           ( "SuperFart",            "Super IBS - Super Fart Talent that releases a cloud that inflicts 3 seconds of Burn on enemies and launches the player upwards.", true,  0   ) },
			{ TraitType.SuperHealer,         ( "SuperHealer",          "Hypercoagulation/Super Healer - HP regenerates, but you lose some Max HP when hit.",                                              true,  0   ) },
			{ TraitType.TwinRelics,          ( "TwinRelics",           "Compulsive Hoarder/Hoarder - All Relics are Twin Relics (when possible).",                                                        true,  0   ) },
			{ TraitType.Vampire,             ( "Vampire",              "Vampirism - Gain 20% of your Weapon Damage as Health, but you take 125% more damage.",                                            true,  0   ) },
			{ TraitType.YouAreBlue,          ( "YouAreBlue",           "Methemoglobinemia/Blue - You are blue. (No effect)",                                                                              true,  0   ) },
			{ TraitType.YouAreLarge,         ( "YouAreLarge",          "Gigantism - You are gigantic.",                                                                                                   true,  25  ) },
			{ TraitType.YouAreSmall,         ( "YouAreSmall",          "Dwarfism - You are Tiny.",                                                                                                        true,  25  ) },
		};

		// Main method that kicks everything off
		protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
			// Create/read the mod specific configuration options
			foreach( TraitType traitType in TraitInfo.Keys ) {
				if( !keys.Exists( traitType ) ) {
					keys.Add( traitType, TraitInfo[traitType].Config );
					if( !WobSettings.Exists( keys.Get( traitType, "Enabled" ) ) && TraitInfo[traitType].Spawn ) {
						WobSettings.Add( new WobSettings.Boolean( keys.Get( traitType, "Enabled" ), "Allow random spawn for " + TraitInfo[traitType].Name, TraitInfo[traitType].Spawn ) );
					}
					if( !WobSettings.Exists( keys.Get( traitType, "GoldBonus" ) ) ) {
						WobSettings.Add( new WobSettings.Num<int>( keys.Get( traitType, "GoldBonus" ), "Gold bonus for " + TraitInfo[traitType].Name, TraitInfo[traitType].Gold, 0.01f, bounds: (0, 1000000) ) );
					}
				}
			}
			WobSettings.Add( new WobSettings.Entry[] {
				// Antique spawn
				new WobSettings.Num<float>( keys.Get( TraitType.Antique,             "SpawnChance"  ), "Additional chance for a trait to be an antique (even if disabled above)",        22f,   0.01f, bounds: (0f,  100f    ) ),
				// Positive health modifiers
				new WobSettings.Num<int>(   keys.Get( TraitType.MegaHealth,          "Health"       ), "Health modifier for " + TraitInfo[TraitType.MegaHealth].Name,                    100,   0.01f, bounds: (-99, 1000000 ) ),
				// Negative health modifiers
				new WobSettings.Num<int>(   keys.Get( TraitType.BonusMagicStrength,  "Health"       ), "Health modifier for " + TraitInfo[TraitType.BonusMagicStrength].Name,            -50,   0.01f, bounds: (-99, 1000000 ) ),
				new WobSettings.Num<int>(   keys.Get( TraitType.BounceTerrain,       "Health"       ), "Health modifier for " + TraitInfo[TraitType.BounceTerrain].Name,                 -30,   0.01f, bounds: (-99, 1000000 ) ),
				new WobSettings.Num<int>(   keys.Get( TraitType.CanNowAttack,        "Health"       ), "Health modifier for " + TraitInfo[TraitType.CanNowAttack].Name,                  -60,   0.01f, bounds: (-99, 1000000 ) ),
				new WobSettings.Num<int>(   keys.Get( TraitType.CantAttack,          "Health"       ), "Health modifier for " + TraitInfo[TraitType.CantAttack].Name,                    -60,   0.01f, bounds: (-99, 1000000 ) ),
				new WobSettings.Num<int>(   keys.Get( TraitType.DamageBoost,         "Health"       ), "Health modifier for " + TraitInfo[TraitType.DamageBoost].Name,                   -25,   0.01f, bounds: (-99, 1000000 ) ),
				new WobSettings.Num<int>(   keys.Get( TraitType.MagicBoost,          "Health"       ), "Health modifier for " + TraitInfo[TraitType.MagicBoost].Name,                    -25,   0.01f, bounds: (-99, 1000000 ) ),
				new WobSettings.Num<int>(   keys.Get( TraitType.OmniDash,            "Health"       ), "Health modifier for " + TraitInfo[TraitType.OmniDash].Name,                      -20,   0.01f, bounds: (-99, 1000000 ) ),
				new WobSettings.Num<int>(   keys.Get( TraitType.RevealAllChests,     "Health"       ), "Health modifier for " + TraitInfo[TraitType.RevealAllChests].Name,               -10,   0.01f, bounds: (-99, 1000000 ) ),
				new WobSettings.Num<int>(   keys.Get( TraitType.SmallHitbox,         "Health"       ), "Health modifier for " + TraitInfo[TraitType.SmallHitbox].Name,                   -25,   0.01f, bounds: (-99, 1000000 ) ),
				// Health loss per hit modifiers
				new WobSettings.Num<float>( keys.Get( TraitType.SuperHealer,         "LossPerHit"   ), "Max health percent lost per hit for " + TraitInfo[TraitType.SuperHealer].Name,   6.25f, 0.01f, bounds: (0f,  100f    ) ),
				// Max mana modifiers
				new WobSettings.Num<int>(   keys.Get( TraitType.MagicBoost,          "MaxMana"      ), "Max mana modifier for " + TraitInfo[TraitType.MagicBoost].Name,                  50,    0.01f, bounds: (-99, 1000000 ) ),
				// Mana from taking damage modifiers
				new WobSettings.Num<int>(   keys.Get( TraitType.ManaFromHurt,        "ManaRegen"    ), "Mana gain from damage for " + TraitInfo[TraitType.ManaFromHurt].Name,            50,    0.01f, bounds: (0,   100     ) ),
				// Vampiric regen modifiers
				new WobSettings.Num<int>(   keys.Get( TraitType.Vampire,             "DamageRegen"  ), "Health from damage modifier for " + TraitInfo[TraitType.Vampire].Name,           20,    0.01f, bounds: (0,   1000000 ) ),
				// Damage taken modifiers
				new WobSettings.Num<int>(   keys.Get( TraitType.Vampire,             "DamageTaken"  ), "Damage taken modifier for " + TraitInfo[TraitType.Vampire].Name,                 125,   0.01f, bounds: (-99, 1000000 ) ),
				// Weapon damage modifiers
				new WobSettings.Num<int>(   keys.Get( TraitType.BonusMagicStrength,  "WeaponDamage" ), "Weapon damage modifier for " + TraitInfo[TraitType.BonusMagicStrength].Name,     -50,   0.01f, bounds: (-99, 1000000 ) ),
				new WobSettings.Num<int>(   keys.Get( TraitType.DamageBoost,         "WeaponDamage" ), "Weapon damage modifier for " + TraitInfo[TraitType.DamageBoost].Name,            50,    0.01f, bounds: (-99, 1000000 ) ),
				// Magic damage modifiers
				new WobSettings.Num<int>(   keys.Get( TraitType.MagicBoost,          "MagicDamage"  ), "Magic damage modifier for " + TraitInfo[TraitType.MagicBoost].Name,              50,    0.01f, bounds: (-99, 1000000 ) ),
				// Spell damage modifiers
				new WobSettings.Num<int>(   keys.Get( TraitType.ManaCostAndDamageUp, "SpellDamage"  ), "Spell damage modifier for " + TraitInfo[TraitType.ManaCostAndDamageUp].Name,     100,   0.01f, bounds: (-99, 1000000 ) ),
				// Spell cost modifiers
				new WobSettings.Num<int>(   keys.Get( TraitType.ManaCostAndDamageUp, "SpellCost"    ), "Spell cost modifier for " + TraitInfo[TraitType.ManaCostAndDamageUp].Name,       100,   0.01f, bounds: (-99, 1000000 ) ),
				// Disarm time
				new WobSettings.Num<float>( keys.Get( TraitType.DisarmOnHurt,        "DisarmTime"   ), "Seconds of being disarmed for " + TraitInfo[TraitType.DisarmOnHurt].Name,        2f,           bounds: (0f,  60f     ) ),
			} );
			// Apply the patches if the mod is enabled
			WobPlugin.Patch();
        }

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
						// Get the trait data that includes rarity info
						TraitData traitData = TraitLibrary.GetTraitData( traitType );
						if( traitData != null ) {
							// Check that the rarity is within the range looked at during character generation
							// Get the value of the setting that has the same name as the internal name of the trait
							if( ( traitData.Rarity >= 1 && traitData.Rarity <= 3 ) && !WobSettings.Get( keys.Get( traitType, "Enabled" ), true ) ) {
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

		// Patch for random trait generation to set the additional chance of Antique spawn
		[HarmonyPatch( typeof( CharacterCreator ), nameof( CharacterCreator.GetRandomTraits ) )]
		internal static class CharacterCreator_GetRandomTraits_Patch {
			internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
				WobPlugin.Log( "CharacterCreator.GetRandomTraits Transpiler Patch" );
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
							new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( keys.Get( TraitType.Antique, "SpawnChance" ), 0.22f ) ),
						} );
				// Return the modified instructions
				return transpiler.GetResult();
			}
		}

		// Patch for the method that gets the gold increase for a trait
		[HarmonyPatch( typeof( TraitManager ), nameof( TraitManager.GetActualTraitGoldGain ) )]
		internal static class TraitManager_GetActualTraitGoldGain_Patch {
			internal static void Postfix( TraitType traitType, ref float __result ) {
				// Check that traits should be giving gold
				if( SkillTreeManager.GetSkillObjLevel( SkillTreeType.Traits_Give_Gold ) > 0 ) {
					// Get the data for the trait being looked at - this is from the original method parameter
					TraitData traitData = TraitLibrary.GetTraitData( traitType );
					if( traitData != null ) {
						// Get the value of the setting that has the same name as the internal name of the trait
						float goldBonus = WobSettings.Get( keys.Get( traitType, "GoldBonus" ), traitData.GoldBonus );
						if( goldBonus != traitData.GoldBonus ) {
							WobPlugin.Log( "Changing bonus for " + traitData.Name + " from " + traitData.GoldBonus + " to " + goldBonus );
							// If a matching config setting has been found, calculate the new gold gain using the file value rather than the game value, and overwite the method return value
							__result = goldBonus * ( 1f + SkillTreeManager.GetSkillTreeObj( SkillTreeType.Traits_Give_Gold_Gain_Mod ).CurrentStatGain );
						} else {
							WobPlugin.Log( "Same bonus for " + traitData.Name + " of " + traitData.GoldBonus );
						}
					}
				}
				// If any of the 'if' statements fail, don't change the return value that the original method decided on
			}
		}

		// Method that checks if a trait is active on the current heir, and gets the new modifier from settings if it is
		private static float GetActiveMod( TraitType traitType, string modType, float defaultMod ) {
			float modifier = 0f;
			if( TraitManager.IsTraitActive( traitType ) ) {
				modifier = WobSettings.Get( keys.Get( traitType, "Health" ), defaultMod );
				if( modifier != defaultMod ) {
					WobPlugin.Log( "Changing " + traitType + " " + modType + " mod to " + modifier );
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
				healthMod += GetActiveMod( TraitType.MegaHealth,         "Health", 1f    );
				// Negative modifiers
				healthMod += GetActiveMod( TraitType.BonusMagicStrength, "Health", -0.5f  );
				healthMod += GetActiveMod( TraitType.BounceTerrain,      "Health", -0.3f  );
				healthMod += GetActiveMod( TraitType.CanNowAttack,       "Health", -0.6f  );
				healthMod += GetActiveMod( TraitType.CantAttack,         "Health", -0.6f  );
				healthMod += GetActiveMod( TraitType.DamageBoost,        "Health", -0.25f );
				healthMod += GetActiveMod( TraitType.MagicBoost,         "Health", -0.25f );
				healthMod += GetActiveMod( TraitType.OmniDash,           "Health", -0.2f  );
				healthMod += GetActiveMod( TraitType.RevealAllChests,    "Health", -0.1f  );
				healthMod += GetActiveMod( TraitType.SmallHitbox,        "Health", -0.25f );
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
		[HarmonyPatch( typeof( ProjectileManager ), nameof( ProjectileManager.ApplyProjectileDamage ) )]
		internal static class ProjectileManager_ApplyProjectileDamage_Patch {
			internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
				WobPlugin.Log( "ProjectileManager.ApplyProjectileDamage Transpiler Patch" );
				// Set up the transpiler handler with the instruction list
				WobTranspiler transpiler = new WobTranspiler( instructions );
				// Perform the patching for weapon damage modifiers
				transpiler.PatchAll(
						// Define the IL code instructions that should be matched
						new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, TraitType.BonusMagicStrength ), // TraitType.BonusMagicStrength
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Call, name: "IsTraitActive"          ), // TraitManager.IsTraitActive(TraitType.BonusMagicStrength)
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                            ), // if (TraitManager.IsTraitActive(TraitType.BonusMagicStrength))
                            /*  3 */ new WobTranspiler.OpTest( OpCodeSet.Ldarg                              ), // projectile
                            /*  4 */ new WobTranspiler.OpTest( OpCodes.Dup                                  ), // projectile
                            /*  5 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_DamageMod"      ), // projectile.DamageMod
                            /*  6 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4                               ), // -0.5f
                            /*  7 */ new WobTranspiler.OpTest( OpCodes.Add                                  ), // projectile.DamageMod + -0.5f
                            /*  8 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "set_DamageMod"      ), // projectile.DamageMod = projectile.DamageMod + -0.5f

                            /*  9 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, TraitType.DamageBoost        ), // TraitType.DamageBoost
                            /* 10 */ new WobTranspiler.OpTest( OpCodes.Call, name: "IsTraitActive"          ), // TraitManager.IsTraitActive(TraitType.DamageBoost)
                            /* 11 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                            ), // if (TraitManager.IsTraitActive(TraitType.DamageBoost))
                            /* 12 */ new WobTranspiler.OpTest( OpCodeSet.Ldarg                              ), // projectile
                            /* 13 */ new WobTranspiler.OpTest( OpCodes.Dup                                  ), // projectile
                            /* 14 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_DamageMod"      ), // projectile.DamageMod
                            /* 15 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4                               ), // 0.5f
                            /* 16 */ new WobTranspiler.OpTest( OpCodes.Add                                  ), // projectile.DamageMod + 0.5f
                            /* 17 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "set_DamageMod"      ), // projectile.DamageMod = projectile.DamageMod + 0.5f
                        },
						// Define the actions to take when an occurrence is found
						new List<WobTranspiler.OpAction> {
							new WobTranspiler.OpAction_SetOperand( 6,  WobSettings.Get( keys.Get( TraitType.BonusMagicStrength, "WeaponDamage" ), -0.5f ) ),
							new WobTranspiler.OpAction_SetOperand( 15, WobSettings.Get( keys.Get( TraitType.DamageBoost,        "WeaponDamage" ),  0.5f ) ),
                        } );
				// Perform the patching for magic damage modifiers
				transpiler.PatchAll(
						// Define the IL code instructions that should be matched
						new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, TraitType.BonusMagicStrength ), // TraitType.BonusMagicStrength
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Call, name: "IsTraitActive"          ), // TraitManager.IsTraitActive(TraitType.BonusMagicStrength)
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                            ), // if (TraitManager.IsTraitActive(TraitType.BonusMagicStrength))
                            /*  3 */ new WobTranspiler.OpTest( OpCodeSet.Ldarg                              ), // projectile
                            /*  4 */ new WobTranspiler.OpTest( OpCodes.Dup                                  ), // projectile
                            /*  5 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_DamageMod"      ), // projectile.DamageMod
                            /*  6 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4                               ), // 0.0f
                            /*  7 */ new WobTranspiler.OpTest( OpCodes.Add                                  ), // projectile.DamageMod + 0.0f
                            /*  8 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "set_DamageMod"      ), // projectile.DamageMod = projectile.DamageMod + 0.0f

                            /*  9 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, TraitType.MagicBoost         ), // TraitType.MagicBoost
                            /* 10 */ new WobTranspiler.OpTest( OpCodes.Call, name: "IsTraitActive"          ), // TraitManager.IsTraitActive(TraitType.MagicBoost)
                            /* 11 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                            ), // if (TraitManager.IsTraitActive(TraitType.MagicBoost))
                            /* 12 */ new WobTranspiler.OpTest( OpCodeSet.Ldarg                              ), // projectile
                            /* 13 */ new WobTranspiler.OpTest( OpCodes.Dup                                  ), // projectile
                            /* 14 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_DamageMod"      ), // projectile.DamageMod
                            /* 15 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4                               ), // 0.5f
                            /* 16 */ new WobTranspiler.OpTest( OpCodes.Add                                  ), // projectile.DamageMod + 0.5f
                            /* 17 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "set_DamageMod"      ), // projectile.DamageMod = projectile.DamageMod + 0.5f
                        },
						// Define the actions to take when an occurrence is found
						new List<WobTranspiler.OpAction> {
							//new WobTranspiler.OpAction_SetOperand( 6,  WobSettings.Get( keys.Get( TraitType.BonusMagicStrength, "MagicDamage" ), 0.0f ) ),
							new WobTranspiler.OpAction_SetOperand( 15, WobSettings.Get( keys.Get( TraitType.MagicBoost, "MagicDamage" ), 0.5f ) ),
						} );
				// Perform the patching for spell damage modifiers
				transpiler.PatchAll(
						// Define the IL code instructions that should be matched
						new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, TraitType.ManaCostAndDamageUp ), // TraitType.ManaCostAndDamageUp
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Call, name: "IsTraitActive"           ), // TraitManager.IsTraitActive(TraitType.ManaCostAndDamageUp)
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                             ), // if (TraitManager.IsTraitActive(TraitType.ManaCostAndDamageUp))

                            /*  3 */ new WobTranspiler.OpTest( OpCodeSet.Ldarg                               ), // projectile
                            /*  4 */ new WobTranspiler.OpTest( OpCodes.Ldstr                                 ), // "PlayerProjectile"
                            /*  5 */ new WobTranspiler.OpTest( OpCodes.Callvirt                              ), // projectile.CompareTag("PlayerProjectile")
                            /*  6 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                             ), // if (projectile.CompareTag("PlayerProjectile"))

                            /*  7 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc                               ), // playerController
                            /*  8 */ new WobTranspiler.OpTest( OpCodes.Callvirt                              ), // playerController.CastAbility
                            /*  9 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc                               ), // lastCastAbilityTypeCasted
                            /* 10 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4                              ), // false
                            /* 11 */ new WobTranspiler.OpTest( OpCodes.Callvirt                              ), // playerController.CastAbility.GetAbility(lastCastAbilityTypeCasted)
                            /* 12 */ new WobTranspiler.OpTest( OpCodeSet.Stloc                               ), // BaseAbility_RL ability = playerController.CastAbility.GetAbility(lastCastAbilityTypeCasted)
							
							/* 13 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc                               ), // ability
							/* 14 */ new WobTranspiler.OpTest( OpCodes.Call                                  ), // (bool)ability
							/* 15 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                             ), // if ((bool)ability)

							/* 16 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc                               ), // ability
							/* 17 */ new WobTranspiler.OpTest( OpCodes.Callvirt                              ), // ability.BaseCost
							/* 18 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4                              ), // 0
							/* 19 */ new WobTranspiler.OpTest( OpCodeSet.Ble                                 ), // if (ability.BaseCost > 0)

							/* 20 */ new WobTranspiler.OpTest( OpCodeSet.Ldarg                               ), // projectile
							/* 21 */ new WobTranspiler.OpTest( OpCodes.Dup                                   ), // projectile
							/* 22 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_DamageMod"       ), // projectile.DamageMod
							/* 23 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4                                ), // 1f
							/* 24 */ new WobTranspiler.OpTest( OpCodes.Add                                   ), // projectile.DamageMod + 1f
							/* 25 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "set_DamageMod"       ), // projectile.DamageMod = projectile.DamageMod + 1f
                        },
						// Define the actions to take when an occurrence is found
						new List<WobTranspiler.OpAction> {
							new WobTranspiler.OpAction_SetOperand( 23, WobSettings.Get( keys.Get( TraitType.ManaCostAndDamageUp, "SpellDamage" ), 1f ) ),
						} );
				// Return the modified instructions
				return transpiler.GetResult();
			}
		}

		// Apply damage taken modifiers
		[HarmonyPatch( typeof( EnemyHitResponse ), "CharacterDamaged" )]
		internal static class EnemyHitResponse_CharacterDamaged_Patch {
			internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
				WobPlugin.Log( "EnemyHitResponse.CharacterDamaged Transpiler Patch" );
				// Set up the transpiler handler with the instruction list
				WobTranspiler transpiler = new WobTranspiler( instructions );
				// Perform the patching
				transpiler.PatchAll(
						// Define the IL code instructions that should be matched
						new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, TraitType.Vampire   ), // TraitType.Vampire
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Call, name: "IsTraitActive" ), // TraitManager.IsTraitActive(TraitType.Vampire)
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                   ), // if (TraitManager.IsTraitActive(TraitType.Vampire))

                            /*  3 */ new WobTranspiler.OpTest( OpCodeSet.Ldarg                     ), // this
                            /*  4 */ new WobTranspiler.OpTest( OpCodes.Ldfld                       ), // this.m_enemyController
                            /*  5 */ new WobTranspiler.OpTest( OpCodes.Callvirt                    ), // this.m_enemyController.EnemyType
                            /*  6 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4                    ), // EnemyType.Dummy
                            /*  7 */ new WobTranspiler.OpTest( OpCodeSet.Beq                       ), // this.m_enemyController.EnemyType != EnemyType.Dummy

                            /*  8 */ new WobTranspiler.OpTest( OpCodeSet.Ldarg                     ), // this
                            /*  9 */ new WobTranspiler.OpTest( OpCodes.Ldfld                       ), // this.m_enemyController
                            /* 10 */ new WobTranspiler.OpTest( OpCodes.Callvirt                    ), // this.m_enemyController.EnemyType
                            /* 11 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4                    ), // EnemyType.Eggplant
                            /* 12 */ new WobTranspiler.OpTest( OpCodeSet.Beq                       ), // this.m_enemyController.EnemyType != EnemyType.Eggplant

                            /* 13 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc                     ), // num3
                            /* 14 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc                     ), // num
                            /* 15 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4                      ), // 0.2f
                            /* 16 */ //new WobTranspiler.OpTest( OpCodes.Mul                         ), // num * 0.2f
                            /* 17 */ //new WobTranspiler.OpTest( OpCodes.Add                         ), // num3 + num * 0.2f
                            /* 18 */ //new WobTranspiler.OpTest( OpCodes.Stloc                       ), // num3 = num3 + num * 0.2f
                        },
						// Define the actions to take when an occurrence is found
						new List<WobTranspiler.OpAction> {
							new WobTranspiler.OpAction_SetOperand( 15, WobSettings.Get( keys.Get( TraitType.Vampire, "DamageRegen" ), 0.2f ) ),
                        } );
				// Return the modified instructions
				return transpiler.GetResult();
			}
		}

		// Apply damage taken modifiers
		[HarmonyPatch( typeof( PlayerController ), nameof( PlayerController.CalculateDamageTaken ) )]
		internal static class PlayerController_CalculateDamageTaken_Patch {
			internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
				WobPlugin.Log( "PlayerController.CalculateDamageTaken Transpiler Patch" );
				// Set up the transpiler handler with the instruction list
				WobTranspiler transpiler = new WobTranspiler( instructions );
				// Perform the patching
				transpiler.PatchAll(
						// Define the IL code instructions that should be matched
						new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, TraitType.Vampire   ), // TraitType.Vampire
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Call, name: "IsTraitActive" ), // TraitManager.IsTraitActive(TraitType.Vampire)
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                   ), // if (TraitManager.IsTraitActive(TraitType.Vampire))
                            /*  3 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc                     ), // num
                            /*  4 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4                      ), // 1.25f
                            /*  5 */ new WobTranspiler.OpTest( OpCodes.Add                         ), // num + 1.25f
                        },
						// Define the actions to take when an occurrence is found
						new List<WobTranspiler.OpAction> {
							new WobTranspiler.OpAction_SetOperand( 4, WobSettings.Get( keys.Get( TraitType.Vampire, "DamageTaken" ), 1.25f ) ),
                        } );
				// Return the modified instructions
				return transpiler.GetResult();
			}
		}

		// Apply damage taken modifiers
		[HarmonyPatch( typeof( ManaRegen ), "OnPlayerHit" )]
		internal static class ManaRegen_OnPlayerHit_Patch {
			internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
				WobPlugin.Log( "ManaRegen.OnPlayerHit Transpiler Patch" );
				// Set up the transpiler handler with the instruction list
				WobTranspiler transpiler = new WobTranspiler( instructions );
				// Perform the patching
				transpiler.PatchAll(
						// Define the IL code instructions that should be matched
						new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_ActualMaxMana" ), // PlayerManager.GetPlayerController().ActualMaxMana
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Conv_R4                             ), // (float)PlayerManager.GetPlayerController().ActualMaxMana
                            /*  2 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4                              ), // 0.5f
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Mul                                 ), // (float)PlayerManager.GetPlayerController().ActualMaxMana * 0.5f
                        },
						// Define the actions to take when an occurrence is found
						new List<WobTranspiler.OpAction> {
							new WobTranspiler.OpAction_SetOperand( 2, WobSettings.Get( keys.Get( TraitType.ManaFromHurt, "ManaRegen" ), 0.5f ) ),
                        } );
				// Return the modified instructions
				return transpiler.GetResult();
			}
		}
		
		// Apply max health loss per hit modifiers
		[HarmonyPatch( typeof( SuperHealer_Trait ), "OnPlayerHit" )]
		internal static class SuperHealer_Trait_OnPlayerHit_Patch {
			internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
				WobPlugin.Log( "SuperHealer_Trait.OnPlayerHit Transpiler Patch" );
				// Set up the transpiler handler with the instruction list
				WobTranspiler transpiler = new WobTranspiler( instructions );
				// Perform the patching
				transpiler.PatchAll(
						// Define the IL code instructions that should be matched
						new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldfld, name: "TemporaryMaxHealthMods" ), // TemporaryMaxHealthMods
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4                                ), // 0.0625f
                            /*  2 */ new WobTranspiler.OpTest( OpCodes.Sub                                   ), // TemporaryMaxHealthMods - 0.0625f
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "TemporaryMaxHealthMods" ), // TemporaryMaxHealthMods = TemporaryMaxHealthMods - 0.0625f
                        },
						// Define the actions to take when an occurrence is found
						new List<WobTranspiler.OpAction> {
							new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( keys.Get( TraitType.SuperHealer, "LossPerHit" ), 0.0625f ) ),
                        } );
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
				WobPlugin.Log( "BaseAbility_RL.ActualCost Transpiler Patch" );
				// Set up the transpiler handler with the instruction list
				WobTranspiler transpiler = new WobTranspiler( instructions );
				// Perform the patching
				transpiler.PatchAll(
						// Define the IL code instructions that should be matched
						new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc ), // baseCost
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Conv_R4 ), // (float)baseCost
                            /*  2 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4  ), // 2f
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Mul     ), // (float)baseCost * 2f
                        },
						// Define the actions to take when an occurrence is found
						new List<WobTranspiler.OpAction> {
							new WobTranspiler.OpAction_SetOperand( 2, 1f ),
                        } );
				// Return the modified instructions
				return transpiler.GetResult();
			}
		}

		// Edit English display text
		[HarmonyPatch( typeof( LineageDescriptionUpdater ), "UpdateText" )]
		internal static class LineageDescriptionUpdater_UpdateText_Patch {
			private static bool runOnce = false;
			internal static void Prefix() {
				if( !runOnce ) {
					DsiplayTextUpdater.Update();
					runOnce = true;
				}
			}
		}

		// Edit English display text
		[HarmonyPatch( typeof( PlayerCardRightPageEntry ), "UpdateCard" )]
		internal static class PlayerCardRightPageEntry_UpdateCard_Patch {
			private static bool runOnce = false;
			internal static void Prefix() {
				if( !runOnce ) {
					DsiplayTextUpdater.Update();
					runOnce = true;
				}
			}
		}

		// Edit English display text
		[HarmonyPatch( typeof( ObjectiveCompleteHUDController ), "UpdateObjectiveCompleteText" )]
		internal static class ObjectiveCompleteHUDController_UpdateObjectiveCompleteText_Patch {
			private static bool runOnce = false;
			internal static void Prefix() {
				if( !runOnce ) {
					DsiplayTextUpdater.Update();
					runOnce = true;
				}
			}
		}

		// Common code for editing display text
		private static class DsiplayTextUpdater {
			private class DictionaryEditor {
				readonly Dictionary<string, string> m_maleLocDict;
				readonly Dictionary<string, string> m_femaleLocDict;

				public DictionaryEditor() {
					this.m_maleLocDict = (Dictionary<string, string>)Traverse.Create( LocalizationManager.Instance ).Field( "m_maleLocDict" ).GetValue();
					this.m_femaleLocDict = (Dictionary<string, string>)Traverse.Create( LocalizationManager.Instance ).Field( "m_femaleLocDict" ).GetValue();
				}

				public void EditString( string locID, string oldText, string newText ) {
					string text;
					if( this.m_maleLocDict.TryGetValue( locID, out text ) ) {
						m_maleLocDict[locID] = text.Replace( oldText, newText );

					}
					if( this.m_femaleLocDict.TryGetValue( locID, out text ) ) {
						m_femaleLocDict[locID] = text.Replace( oldText, newText );
					}
				}
			}

			private static bool runOnce = false;

			public static void Update() {
				if( !runOnce ) {
					DictionaryEditor dictionary = new DictionaryEditor();
					{ // Crippling Intellect
						string hp = WobSettings.Get( keys.Get( TraitType.BonusMagicStrength, "Health" ), -0.5f ).ToString( "+0.##%;-0.##%;-0%" );
						string dmg = WobSettings.Get( keys.Get( TraitType.BonusMagicStrength, "WeaponDamage" ), -0.5f ).ToString( "+0.##%;-0.##%;-0%" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION_BonusMagicStrength_1", "50%<color=purple> Health</color>, and <color=purple>Weapon Damage</color>", hp + " <color=purple>Health</color>, and " + dmg + " <color=purple>Weapon Damage</color>" );
					}
					{ // Clownanthropy
						string hp = WobSettings.Get( keys.Get( TraitType.BounceTerrain, "Health" ), -0.3f ).ToString( "+0.##%;-0.##%;-0%" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION_BounceTerrain_1", "30% less", hp );
                    }
					{ // Pacifier
						string hp = WobSettings.Get( keys.Get( TraitType.CanNowAttack, "Health" ), -0.6f ).ToString( "+0.##%;-0.##%;-0%" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION_CanNowAttack_1", "60% less", hp );
                    }
					{ // Pacifist
						string hp = WobSettings.Get( keys.Get( TraitType.CantAttack, "Health" ), -0.6f ).ToString( "+0.##%;-0.##%;-0%" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION_CantAttack_1", "60% less", hp );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION2_CantAttack_1", "60% less", hp );
                    }
					{ // Combative
						string hp = WobSettings.Get( keys.Get( TraitType.DamageBoost, "Health" ), -0.25f ).ToString( "+0.##%;-0.##%;-0%" );
						string dmg = WobSettings.Get( keys.Get( TraitType.DamageBoost, "WeaponDamage" ), 0.5f ).ToString( "+0.##%;-0.##%;+0%" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION_DamageBoost_1", "-25%", hp );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION_DamageBoost_1", "+50%", dmg );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION2_DamageBoost_1", "-25%", hp );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION2_DamageBoost_1", "+50%", dmg );
					}
					{ // Bookish
						string hp = WobSettings.Get( keys.Get( TraitType.MagicBoost, "Health" ), -0.25f ).ToString( "+0.##%;-0.##%;-0%" );
						string mp = WobSettings.Get( keys.Get( TraitType.MagicBoost, "MaxMana" ), 0.5f ).ToString( "+0.##%;-0.##%;+0%" );
						string dmg = WobSettings.Get( keys.Get( TraitType.MagicBoost, "MagicDamage" ), 0.5f ).ToString( "+0.##%;-0.##%;+0%" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION_MagicBoost_1", "-25% <color=purple>HP</color>", hp + " <color=purple>Health</color>" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION_MagicBoost_1", "<color=purple>+50% MP Capacity</color>", mp + " <color=purple>Mana Capacity</color>" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION_MagicBoost_1", "+50% <color=purple>Magic Damage</color>", dmg + " <color=purple>Magic Damage</color>" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION2_MagicBoost_1", "-25%", hp );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION2_MagicBoost_1", "MP Capacity", mp + " MP Capacity" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION2_MagicBoost_1", "+50%", dmg );
                    }
					{ // Emotional Dysregularity/Overcompensation
						string dmg = WobSettings.Get( keys.Get( TraitType.ManaCostAndDamageUp, "SpellDamage" ), 1f ).ToString( "+0.##%;-0.##%;+0%" );
						string cost = WobSettings.Get( keys.Get( TraitType.ManaCostAndDamageUp, "SpellCost" ), 1f ).ToString( "+0.##%;-0.##%;+0%" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION_ManaCostAndDamageUp_1", "Mana costs and Spell damage increased by 100%", dmg + " <color=purple>Spell Damage</color> and " + cost + " <color=purple>Mana Costs</color>" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION2_ManaCostAndDamageUp_1", "Mana costs and damage increased by 100%", dmg + " Spell Damage and " + cost + " Mana Costs" );
                    }
					{ // Masochism
						string regen = WobSettings.Get( keys.Get( TraitType.ManaFromHurt, "ManaRegen" ), 0.5f ).ToString( "+0.##%;-0.##%;+0%" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION_ManaFromHurt_1", "50%", regen );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION2_ManaFromHurt_1", "50%", regen );
                    }
					{ // Hero Complex
						string hp = WobSettings.Get( keys.Get( TraitType.MegaHealth, "Health" ), 1f ).ToString( "+0.##%;-0.##%;+0%" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION_MegaHealth_1", "<color=#0C8420>100%</color> more", "<color=#0C8420>" + hp + "</color>" );
                    }
					{ // Superfluid
						string hp = WobSettings.Get( keys.Get( TraitType.OmniDash, "Health" ), -0.2f ).ToString( "+0.##%;-0.##%;-0%" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION_OmniDash_1", "20% less", hp );
                    }
					{ // Spelunker
						string hp = WobSettings.Get( keys.Get( TraitType.RevealAllChests, "Health" ), -0.1f ).ToString( "+0.##%;-0.##%;-0%" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION_RevealAllChests_1", "-10% HP", hp + " <color=purple>Health</color>" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION2_RevealAllChests_1", "-10%", hp );
                    }
					{ // Disattuned/Only Heart
						string hp = WobSettings.Get( keys.Get( TraitType.SmallHitbox, "Health" ), -0.25f ).ToString( "+0.##%;-0.##%;-0%" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION_SmallHitbox_1", "-25%", hp );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION2_SmallHitbox_1", "-25%", hp );
                    }
					{ // Vampirism
						string regen = WobSettings.Get( keys.Get( TraitType.Vampire, "DamageRegen" ), 0.2f ).ToString( "+0.##%;-0.##%;+0%" );
						string taken = WobSettings.Get( keys.Get( TraitType.Vampire, "DamageTaken" ), 1.25f ).ToString( "+0.##%;-0.##%;+0%" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION_Vampire_1", "20%", regen );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION_Vampire_1", "125%</color> more", taken + "</color>" );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION2_Vampire_1", "20%", regen );
						dictionary.EditString( "LOC_ID_TRAIT_DESCRIPTION2_Vampire_1", "125% more", taken );
					}
					runOnce = true;
				}
			}
		}

		

	}
}
