using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace Wob_CharGen {
	[BepInPlugin( "Wob.CharGen", "Character Generation Mod", "1.0.0" )]
	public partial class CharGen : BaseUnityPlugin {
		public enum TraitName {
			Random,
			Antique,
			BlurOnHit,
			BonusChestGold,
			BonusMagicStrength,
			BounceTerrain,
			BreakPropsForMana,
			CantAttack,
			CheerOnKills,
			ColorTrails,
			DamageBoost,
			DisarmOnHurt,
			EasyBreakables,
			EnemiesBlackFill,
			EnemiesCensored,
			EnemyKnockedFar,
			EnemyKnockedLow,
			ExplosiveChests,
			ExplosiveEnemies,
			FakeSelfDamage,
			Fart,
			FMFFan,
			GainDownStrike,
			Gay,
			HighJump,
			ItemsGoFlying,
			LongerCD,
			LowerGravity,
			LowerStorePrice,
			MagicBoost,
			ManaCostAndDamageUp,
			ManaFromHurt,
			MapReveal,
			MegaHealth,
			NoColor,
			NoEnemyHealthBar,
			NoHealthBar,
			NoImmunityWindow,
			NoMeat,
			OldYellowTint,
			OmniDash,
			OneHitDeath,
			PlayerKnockedFar,
			PlayerKnockedLow,
			RandomizeKit,
			RevealAllChests,
			SkillCritsOnly,
			SmallHitbox,
			SuperFart,
			SuperHealer,
			TwinRelics,
			Vampire,
			YouAreBlue,
			YouAreLarge,
			YouAreSmall,
		}

		// Main method that kicks everything off
		protected void Awake() {
			// Set up the logger and basic config items
			WobPlugin.Initialise( this, this.Logger );
			// Create/read the mod specific configuration options
			WobPlugin.Settings.Add( new WobSettings.Entry[] {
				new WobSettings.Scaled<float>(   "TraitChance1",  "Chance to generate first trait",                  67.5f, 0.01f, bounds: (0f, 100f) ),
				new WobSettings.Scaled<float>(   "TraitChance2",  "Chance to generate second trait",                 35f,   0.01f, bounds: (0f, 100f) ),
				new WobSettings.Scaled<float>(   "AntiqueChance", "Chance for a trait to be an antique",             22f,   0.01f, bounds: (0f, 100f) ),
				new WobSettings.Enum<TraitName>( "TraitType1",    "Name of trait to always use as the first trait",  TraitName.Random                 ),
				new WobSettings.Enum<TraitName>( "TraitType2",    "Name of trait to always use as the second trait", TraitName.Random                 ),
			} );
			// Apply the patches if the mod is enabled
			WobPlugin.Patch();
		}

		// The patch itself - just call my version of random generation, and overwrite the original return value with mine
		[HarmonyPatch( typeof( CharacterCreator ), nameof( CharacterCreator.GetRandomTraits ) )]
		static class CharacterCreator_GetRandomTraits_Patch {
			internal static void Postfix( bool forceRandomizeKit, ref Vector2Int __result ) {
				__result = GetRandomTraits( forceRandomizeKit );
			}
		}

		// This is the original method, simplified, then modified to add fixed spawns from the config
		public static Vector2Int GetRandomTraits( bool forceRandomizeKit = false ) {
			// Initialise the traits to those in the config file, or 'TraitType.None' if we should randomly generate
			TraitType[] traitTypes = { GetTypeFromName( "TraitType1" ), GetTypeFromName( "TraitType2" ) };
			// List for the available traits to choose from
			List<TraitType> traitTypesList = new List<TraitType>();
			// Go through all of traits in the game
			foreach( TraitType traitType in TraitType_RL.TypeArray ) {
				// Exclude the non-trait and special event traits
				if( traitType != TraitType.None &&
						( !HolidayLookController.IsHoliday( HolidayType.Halloween ) || traitType != TraitType.HalloweenHoliday ) &&
						( !HolidayLookController.IsHoliday( HolidayType.Christmas ) || traitType != TraitType.ChristmasHoliday ) ) {
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
			// Override for contrarian on left heir
			if( forceRandomizeKit ) { traitTypes[0] = TraitType.RandomizeKit; }
			// Do for each of the 2 trait slots
			for( int j = 0; j < traitTypes.Length; j++ ) {
				// We only want to randomly generate a trait if the slot is empty
				if( traitTypes[j] == TraitType.None ) {
					// Get the trait roll chances from config
					float[] spawnChance = new float[] { WobPlugin.Settings.Get( "TraitChance1", 0.675f ), WobPlugin.Settings.Get( "TraitChance2", 0.35f ) };
					// Roll for whether to generate a trait
					if( RNGManager.GetRandomNumber( RngID.Lineage, "GetRandomTraitSpawnChance", 0f, 1f ) <= spawnChance[j] ) {
						// Roll for whether the spawned trait should be an antique
						if( RNGManager.GetRandomNumber( RngID.Lineage, "GetAntiqueSpawnChance", 0f, 1f ) < WobPlugin.Settings.Get( "AntiqueChance", 0.22f ) ) {
							traitTypes[j] = TraitType.Antique;
						} else {
							// Check if random traits are disabled in House Rules
							if( !( SaveManager.PlayerSaveData.EnableHouseRules && SaveManager.PlayerSaveData.Assist_DisableTraits ) ) {
								// Generate a random trait that is compatible with the trait in the other slot
								traitTypes[j] = GetCompatibleTrait( traitTypesList, traitTypes[( j == 0 ? 1 : 0 )] );
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
			// Return the traits
            return new Vector2Int( (int)traitTypes[0], (int)traitTypes[1] );
		}

		// Dictionary to translate from the name in config to the game's trait type enum
		private static Dictionary<TraitName, TraitType> nameTypePairs;

		// Method to populate the translation dictionary
		private static void MakeDictionary() {
			// First, turn all enum values into strings to be matched against the trait name
			Dictionary<string, TraitName> names = new Dictionary<string, TraitName>();
			foreach( TraitName name in Enum.GetValues( typeof( TraitName ) ) ) {
				names.Add( name.ToString(), name );
			}
			// Create the dictionary
			nameTypePairs = new Dictionary<TraitName, TraitType>();
			// Go through all traits in the game
			foreach( BaseTrait trait in TraitLibrary.TraitArray ) {
				// Exclude any that are not complete - they have no in-game name or description, so just ignore them
				if( trait.TraitData != null ) {
					// Match the trait name against the enum used in the config file
					if( names.TryGetValue( trait.TraitData.Name, out TraitName name ) ) {
						// Add the translation to the dictionary
						nameTypePairs.Add( name, trait.TraitType );
					}
				}
			}
		}

		// Method to fetch the config value, and translate it to a TraitType
		private static TraitType GetTypeFromName( string settingName ) {
			// If the dictionary hasn't been initialised, do it now
			if( nameTypePairs == null ) { MakeDictionary(); }
			// Variable for the return value, with default of 'None', which means randomly generate
			TraitType traitType = TraitType.None;
			// Get the value from config
			TraitName traitName = WobPlugin.Settings.Get( settingName, TraitName.Random );
			// Only need to change the trait type if it isn't randomly generated
			if( traitName != TraitName.Random ) {
				// Get the trait type for the name
				if( !nameTypePairs.TryGetValue( traitName, out traitType ) ) {
					WobPlugin.Log( "WARNING: Could not find TraitType for " + traitName );
					traitType = TraitType.None;
				}
			}
			// Return the fixed spawn, or 'None' for random generation
			return traitType;
		}

		// Randomly generate a trait that is compatible with an existing trait (all traits are compatible with TraitType.None)
		private static TraitType GetCompatibleTrait( List<TraitType> traitTypesList, TraitType existingTrait ) {
			// Variable for the return value
			TraitType newTrait = TraitType.None;
			// Keep track of the number of attempts to prevent an infinite loop
			int attempts = 0;
			// Continue generating new traits until one is found or we run out of attempts
			while( newTrait == TraitType.None && attempts < 20 ) {
				// Roll for the trait index, and get it from the list
				newTrait = traitTypesList[ RNGManager.GetRandomNumber( RngID.Lineage, "GetRandomTraitChoiceRoll - Trait #?", 0, traitTypesList.Count ) ];
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
			// Return the new trait, or 'None' if no compatible trait could be found in 20 attempts
			return newTrait;
		}

		// Method to access private method CharacterCreator.AreTraitsCompatible
		private static bool CharacterCreator_AreTraitsCompatible( TraitType traitType1, TraitType traitType2 ) {
			return Traverse.Create( typeof( CharacterCreator ) ).Method( "AreTraitsCompatible", new Type[] { typeof( TraitType ), typeof( TraitType ) } ).GetValue<bool>( new object[] { traitType1, traitType2 } );
		}
	}
}