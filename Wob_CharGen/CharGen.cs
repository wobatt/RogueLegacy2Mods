using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace Wob_CharGen {
	[BepInPlugin( "Wob.CharGen", "Character Generation Mod", "1.1.0" )]
	public partial class CharGen : BaseUnityPlugin {
		public enum TraitName {
			Random,
			Antique,            BlurOnHit,          BonusChestGold,     BonusMagicStrength, BounceTerrain,      BreakPropsForMana,  CantAttack,         CheerOnKills,       ColorTrails,        DamageBoost,        
			DisarmOnHurt,       EasyBreakables,     EnemiesBlackFill,   EnemiesCensored,    EnemyKnockedFar,    EnemyKnockedLow,    ExplosiveChests,    ExplosiveEnemies,   FakeSelfDamage,     Fart,               
			FMFFan,             GainDownStrike,     Gay,                HighJump,           ItemsGoFlying,      LongerCD,           LowerGravity,       LowerStorePrice,    MagicBoost,         ManaCostAndDamageUp,
			ManaFromHurt,       MapReveal,          MegaHealth,         NoColor,            NoEnemyHealthBar,   NoHealthBar,        NoImmunityWindow,   NoMeat,             OldYellowTint,      OmniDash,           
			OneHitDeath,        PlayerKnockedFar,   PlayerKnockedLow,   RandomizeKit,       RevealAllChests,    SkillCritsOnly,     SmallHitbox,        SuperFart,          SuperHealer,        TwinRelics,         
			Vampire,            YouAreBlue,         YouAreLarge,        YouAreSmall,        
		}

		private static readonly List<(string Section, string Name, string Desc, int Male, int Female)> looks = new List<(string Section, string Name, string Desc, int Male, int Female)> {
			( "Looks_EyeType",    "Eyes_Default",        "Normal eyes",           100, 100 ),
			( "Looks_EyeType",    "Eyes_Bored",          "Bored eyes",             30,  30 ),
			( "Looks_EyeType",    "Eyes_Eyelashes",      "Eyelashes",               7, 100 ),
			( "Looks_EyeType",    "Eyes_Squinty",        "Squinty eyes",           30,  30 ),
			( "Looks_EyeType",    "Eyes_BoredEyelashes", "Bored eyelashes",         3,  50 ),

			( "Looks_MouthType",  "Mouth_Default",       "Normal mouth",          100, 100 ),
			( "Looks_MouthType",  "Mouth_Toothy",        "Toothy mouth",           50,  50 ),
			( "Looks_MouthType",  "Mouth_SmileTeeth",    "Smile mouth",            25,  25 ),

			( "Looks_HairType",   "Hair_Bald",           "Bald",                  100,   5 ),
			( "Looks_HairType",   "Hair_Pompadour",      "Pompadour hair",        100,  10 ),
			( "Looks_HairType",   "Hair_Braid",          "Braid hair",             10, 100 ),
			( "Looks_HairType",   "Hair_LongStraight",   "Long straight hair",     10, 100 ),
			( "Looks_HairType",   "Hair_LongMessy",      "Long messy hair",        50,  50 ),
			( "Looks_HairType",   "Hair_DreadsTied",     "Dreads hair",            10, 100 ),
			( "Looks_HairType",   "Hair_LongWavy",       "Long wavy hair",         10, 100 ),
			( "Looks_HairType",   "Hair_LongCurls",      "Long curls hair",        50,  50 ),
			( "Looks_HairType",   "Hair_ShortCurls",     "Short curls hair",      100,  10 ),
			( "Looks_HairType",   "Hair_Puffy",          "Puffy hair",            100,  10 ),

			( "Looks_HeadType",   "Head_NoFacialHair",   "No facial hair",        100, 100 ),
			( "Looks_HeadType",   "Head_Goatee",         "Goatee facial hair",     25,   0 ),
			( "Looks_HeadType",   "Head_Beard",          "Beard facial hair",      25,   0 ),
			( "Looks_HeadType",   "Head_MoustacheSmall", "Moustache facial hair",  25,   0 ),
			( "Looks_HeadType",   "Head_Scruffy",        "Scruffy facial hair",    25,   0 ),

			( "Looks_SkinColour", "Skin_FDE7AD",         "#FDE7AD skin",          100, 100 ),
			( "Looks_SkinColour", "Skin_F9D4A0",         "#F9D4A0 skin",          100, 100 ),
			( "Looks_SkinColour", "Skin_FFBB94",         "#FFBB94 skin",          100, 100 ),
			( "Looks_SkinColour", "Skin_D99565",         "#D99565 skin",          100, 100 ),
			( "Looks_SkinColour", "Skin_AD8A60",         "#AD8A60 skin",          100, 100 ),
			( "Looks_SkinColour", "Skin_B26644",         "#B26644 skin",          100, 100 ),
			( "Looks_SkinColour", "Skin_BC6E43",         "#BC6E43 skin",          100, 100 ),
			( "Looks_SkinColour", "Skin_8C502D",         "#8C502D skin",          100, 100 ),
			( "Looks_SkinColour", "Skin_733F17",         "#733F17 skin",          100, 100 ),

			( "Looks_HairColour", "Hair_FFECA7",         "#FFECA7 hair",          100, 100 ),
			( "Looks_HairColour", "Hair_E0B545",         "#E0B545 hair",          100, 100 ),
			( "Looks_HairColour", "Hair_FF9678",         "#FF9678 hair",          100, 100 ),
			( "Looks_HairColour", "Hair_E07900",         "#E07900 hair",          100, 100 ),
			( "Looks_HairColour", "Hair_A16334",         "#A16334 hair",          100, 100 ),
			( "Looks_HairColour", "Hair_643C19",         "#643C19 hair",          100, 100 ),
			( "Looks_HairColour", "Hair_282C38",         "#282C38 hair",          100, 100 ),
			( "Looks_HairColour", "Hair_4D4C5E",         "#4D4C5E hair",          100, 100 ),
			( "Looks_HairColour", "Hair_FFFFFF",         "#FFFFFF hair",           50,  50 ),
			( "Looks_HairColour", "Hair_3991CD",         "#3991CD hair",           50,  50 ),
			( "Looks_HairColour", "Hair_58BC76",         "#58BC76 hair",           50,  50 ),
			( "Looks_HairColour", "Hair_E6648C",         "#E6648C hair",           50,  50 ),
			( "Looks_HairColour", "Hair_402310",         "#402310 hair",          100, 100 ),
			( "Looks_HairColour", "Hair_908A84",         "#908A84 hair",          100, 100 ),
		};

		// Main method that kicks everything off
		protected void Awake() {
			// Set up the logger and basic config items
			WobPlugin.Initialise( this, this.Logger );
			// Create/read the mod specific configuration options
			WobSettings.Add( new WobSettings.Entry[] {
				new WobSettings.Boolean(         "SoulShop", "AllSlotsLock_Class",      "Locking a class in the Soul Shop affects all slots, not just the last",     false                            ),
				new WobSettings.Boolean(         "SoulShop", "AllSlotsLock_Spell",      "Locking a spell in the Soul Shop affects all slots, not just the last",     false                            ),
				new WobSettings.Boolean(         "SoulShop", "AllSlotsLock_Contrarian", "Locking contrarian in the Soul Shop affects all slots, not just the first", false                            ),
				new WobSettings.Boolean(         "SoulShop", "ClassLockContrarian",     "Allow the last slot to be contrarian while a class is locked",              false                            ),
				new WobSettings.Num<float>(      "Traits",   "TraitChance1",            "Chance to generate first trait",                                            67.5f, 0.01f, bounds: (0f, 100f) ),
				new WobSettings.Num<float>(      "Traits",   "TraitChance2",            "Chance to generate second trait",                                           35f,   0.01f, bounds: (0f, 100f) ),
				new WobSettings.Num<float>(      "Traits",   "AntiqueChance",           "Additional chance for a trait to be an antique",                            22f,   0.01f, bounds: (0f, 100f) ),
				new WobSettings.Enum<TraitName>( "Traits",   "TraitType1",              "Name of trait to always use as the first trait",                            TraitName.Random                 ),
				new WobSettings.Enum<TraitName>( "Traits",   "TraitType2",              "Name of trait to always use as the second trait",                           TraitName.Random                 ),
			} );
			for( int i = 0; i < looks.Count; i++ ) {
				WobSettings.Add( new WobSettings.Num<int>( looks[i].Section, looks[i].Name + "_MaleWeight",   looks[i].Desc + " weighting for male heirs",   looks[i].Male,   0.01f, bounds: (0, 1000) ) );
				WobSettings.Add( new WobSettings.Num<int>( looks[i].Section, looks[i].Name + "_FemaleWeight", looks[i].Desc + " weighting for female heirs", looks[i].Female, 0.01f, bounds: (0, 1000) ) );
            }
			// Apply the patches if the mod is enabled
			WobPlugin.Patch();
		}

		[HarmonyPatch( typeof( LookLibrary ), nameof( LookLibrary.GetEyeLookData ), new Type[] { } )]
		internal static class LookLibrary_GetEyeLookData_Patch {
			private static bool runOnce = false;
			internal static void Postfix( ref List<MaterialWeightObject> __result ) {
				if( !runOnce ) {
					foreach( MaterialWeightObject lookWeight in __result ) {
						EditLookWeight( lookWeight, LookType.Eyes );
					}
					runOnce = true;
				}
			}
		}
		
		[HarmonyPatch( typeof( LookLibrary ), nameof( LookLibrary.GetMouthLookData ), new Type[] { } )]
		internal static class LookLibrary_GetMouthLookData_Patch {
			private static bool runOnce = false;
			internal static void Postfix( ref List<MaterialWeightObject> __result ) {
				if( !runOnce ) {
					foreach( MaterialWeightObject lookWeight in __result ) {
						EditLookWeight( lookWeight, LookType.Mouth );
					}
					runOnce = true;
				}
			}
		}
		
		[HarmonyPatch( typeof( LookLibrary ), nameof( LookLibrary.GetHairLookData ), new Type[] { } )]
		internal static class LookLibrary_GetHairLookData_Patch {
			private static bool runOnce = false;
			internal static void Postfix( ref List<MaterialWeightObject> __result ) {
				if( !runOnce ) {
					foreach( MaterialWeightObject lookWeight in __result ) {
						EditLookWeight( lookWeight, LookType.Hair );
					}
					runOnce = true;
				}
			}
		}
		
		[HarmonyPatch( typeof( LookLibrary ), nameof( LookLibrary.GetFacialHairLookData ), new Type[] { } )]
		internal static class LookLibrary_GetFacialHairLookData_Patch {
			private static bool runOnce = false;
			internal static void Postfix( ref List<MaterialWeightObject> __result ) {
				if( !runOnce ) {
					foreach( MaterialWeightObject lookWeight in __result ) {
						EditLookWeight( lookWeight, LookType.FacialHair );
					}
					runOnce = true;
				}
			}
		}
		
		[HarmonyPatch( typeof( LookLibrary ), nameof( LookLibrary.GetSkinColorLookData ), new Type[] { } )]
		internal static class LookLibrary_GetSkinColorLookData_Patch {
			private static bool runOnce = false;
			internal static void Postfix( ref List<ColorWeightObject> __result ) {
				if( !runOnce ) {
					foreach( ColorWeightObject lookWeight in __result ) {
						EditLookWeight( lookWeight, LookType.SkinColor );
					}
					runOnce = true;
				}
			}
		}
		
		[HarmonyPatch( typeof( LookLibrary ), nameof( LookLibrary.GetHairColorLookData ), new Type[] { } )]
		internal static class LookLibrary_GetHairColorLookData_Patch {
			private static bool runOnce = false;
			internal static void Postfix( ref List<ColorWeightObject> __result ) {
				if( !runOnce ) {
					foreach( ColorWeightObject lookWeight in __result ) {
						EditLookWeight( lookWeight, LookType.HairColor );
					}
					runOnce = true;
				}
			}
		}

		private static void EditLookWeight( MaterialWeightObject lookWeight, LookType lookType ) {
			string lookup = ( lookWeight.Material.name ).Replace( "Player_", null ).Replace( "_Material", null );
			string section = null;
			switch( lookType ) {
				case LookType.Eyes:
					section = "Looks_EyeType";
					break;
				case LookType.Mouth:
					section = "Looks_MouthType";
					break;
				case LookType.Hair:
					section = "Looks_HairType";
					break;
				case LookType.FacialHair:
					section = "Looks_HeadType";
					break;
			}
			if( section != null ) {
				Traverse.Create( lookWeight ).Field<float>( "m_maleWeight"   ).Value = WobSettings.Get( section, lookup + "_MaleWeight",   lookWeight.MaleWeight   );
				Traverse.Create( lookWeight ).Field<float>( "m_femaleWeight" ).Value = WobSettings.Get( section, lookup + "_FemaleWeight", lookWeight.FemaleWeight );
			}
		}

		private static void EditLookWeight( ColorWeightObject lookWeight, LookType lookType ) {
			string lookup = ColorUtility.ToHtmlStringRGB( lookWeight.Color );
			string section = null;
			switch( lookType ) {
				case LookType.SkinColor:
					section = "Looks_SkinColour";
					lookup = "Skin_" + lookup;
					break;
				case LookType.HairColor:
					section = "Looks_HairColour";
					lookup = "Hair_" + lookup;
					break;
			}
			if( section != null ) {
				Traverse.Create( lookWeight ).Field<float>( "m_maleWeight"   ).Value = WobSettings.Get( section, lookup + "_MaleWeight",   lookWeight.MaleWeight   );
				Traverse.Create( lookWeight ).Field<float>( "m_femaleWeight" ).Value = WobSettings.Get( section, lookup + "_FemaleWeight", lookWeight.FemaleWeight );
			}
		}

		[HarmonyPatch( typeof( CharacterCreator ), nameof( CharacterCreator.GenerateClass ) )]
		internal static class CharacterCreator_GenerateClass_Patch {
			internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
				WobPlugin.Log( "CharacterCreator.GenerateClass Transpiler Patch" );
				// Set up the transpiler handler with the instruction list
				WobTranspiler transpiler = new WobTranspiler( instructions );
				// Perform the patching
				transpiler.PatchAll(
						// Define the IL code instructions that should be matched
						new List<WobTranspiler.OpTest> {
							// charDataToMod.ClassType = classType;
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldarg_1                  ), // charDataToMod
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                  ), // classType
                            /*  2 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "ClassType" ), // charDataToMod.ClassType = classType
                        },
						// Define the actions to take when an occurrence is found
						new List<WobTranspiler.OpAction> {
								new WobTranspiler.OpAction_Insert( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => OverrideClass( ClassType.None ) ) ),
						} );
				transpiler.PatchAll(
						// Define the IL code instructions that should be matched
						new List<WobTranspiler.OpTest> {
							// charDataToMod.Spell = abilityType2;
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldarg_1              ), // charDataToMod
                            /*  1 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc              ), // abilityType2
                            /*  2 */ new WobTranspiler.OpTest( OpCodes.Stfld, name: "Spell" ), // charDataToMod.Spell = abilityType2
                        },
						// Define the actions to take when an occurrence is found
						new List<WobTranspiler.OpAction> {
								new WobTranspiler.OpAction_Insert( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => OverrideSpell( AbilityType.None ) ) ),
						} );
				// Return the modified instructions
				return transpiler.GetResult();
			}

			private static ClassType OverrideClass( ClassType classType ) {
				if( WobSettings.Get( "SoulShop", "AllSlotsLock_Class", false ) ) {
					SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj( SoulShopType.ChooseYourClass );
					if( !soulShopObj.IsNativeNull() && soulShopObj.CurrentEquippedLevel > 0 ) {
						ClassType soulShopClassChosen = SaveManager.ModeSaveData.SoulShopClassChosen;
						if( soulShopClassChosen != ClassType.None ) {
							return soulShopClassChosen;
						}
					}
				}
				return classType;
			}

			private static AbilityType OverrideSpell( AbilityType spellType ) {
				if( WobSettings.Get( "SoulShop", "AllSlotsLock_Spell", false ) ) {
					SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj( SoulShopType.ChooseYourSpell );
					if( !soulShopObj.IsNativeNull() && soulShopObj.CurrentEquippedLevel > 0 ) {
						AbilityType soulShopSpellChosen = SaveManager.ModeSaveData.SoulShopSpellChosen;
						if( soulShopSpellChosen != AbilityType.None ) {
							return soulShopSpellChosen;
						}
					}
				}
				return spellType;
			}
		}

		// Complete override of the original method, starting with the guaranteed spawns, then randomly generating compatible traits
		[HarmonyPatch( typeof( CharacterCreator ), nameof( CharacterCreator.GetRandomTraits ) )]
		static class CharacterCreator_GetRandomTraits_Patch {
			internal static void Postfix( bool forceRandomizeKit, ref Vector2Int __result ) {
				// Initialise the traits to those in the config file, or 'TraitType.None' if we should randomly generate
				TraitType[] traitTypes = { TraitSettingDecoder.Get( "TraitType1" ), TraitSettingDecoder.Get( "TraitType2" ) };
				// Make sure that the traits are different
				if( traitTypes[0] == traitTypes[1] && traitTypes[0] != TraitType.None ) {
					WobPlugin.Log( "CONFIG ERROR: Traits in config file should not be the same - " + traitTypes[0] + ", " + traitTypes[1], WobPlugin.ERROR );
					traitTypes[0] = TraitType.None;
				}
				// Make sure the traits are compatible
				if( !CharacterCreator_AreTraitsCompatible( traitTypes[0], traitTypes[1] ) ) {
					WobPlugin.Log( "CONFIG ERROR: Traits in config file are not compatible - " + traitTypes[0] + ", " + traitTypes[1], WobPlugin.ERROR );
					traitTypes[1] = TraitType.None;
				}
				// Override for contrarian on left heir, if it is not in the guaranteed spawns
				if( ( forceRandomizeKit || WobSettings.Get( "SoulShop", "AllSlotsLock_Contrarian", false ) ) && traitTypes[0] != TraitType.RandomizeKit && traitTypes[1] != TraitType.RandomizeKit ) {
					traitTypes[0] = TraitType.RandomizeKit;
				}
				// List for the available traits to choose from
				List<TraitType> traitTypesList = GetAllowedTraits();
				// Do for each of the 2 trait slots
				for( int j = 0; j < traitTypes.Length; j++ ) {
					// We only want to randomly generate a trait if the slot is empty
					if( traitTypes[j] == TraitType.None ) {
						// Get the trait roll chances from config
						float[] spawnChance = new float[] { WobSettings.Get( "Traits", "TraitChance1", 0.675f ), WobSettings.Get( "Traits", "TraitChance2", 0.35f ) };
						// Roll for whether to generate a trait
						if( RNGManager.GetRandomNumber( RngID.Lineage, "GetRandomTraitSpawnChance", 0f, 1f ) <= spawnChance[j] ) {
							// Roll for whether the spawned trait should be an antique
							if( RNGManager.GetRandomNumber( RngID.Lineage, "GetAntiqueSpawnChance", 0f, 1f ) < WobSettings.Get( "Traits", "AntiqueChance", 0.22f ) ) {
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
				__result = new Vector2Int( (int)traitTypes[0], (int)traitTypes[1] );
			}

			// Get a list of the traits that can be randomly added to an heir
			private static List<TraitType> GetAllowedTraits() {
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
				return traitTypesList;
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
				// Return the new trait, or 'None' if no compatible trait could be found in 20 attempts
				return newTrait;
			}

			// Method to access private method CharacterCreator.AreTraitsCompatible
			private static bool CharacterCreator_AreTraitsCompatible( TraitType traitType1, TraitType traitType2 ) {
				return Traverse.Create( typeof( CharacterCreator ) ).Method( "AreTraitsCompatible", new Type[] { typeof( TraitType ), typeof( TraitType ) } ).GetValue<bool>( new object[] { traitType1, traitType2 } );
			}

			private class TraitSettingDecoder {
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
				public static TraitType Get( string settingName ) {
					// If the dictionary hasn't been initialised, do it now
					if( nameTypePairs == null ) { MakeDictionary(); }
					// Variable for the return value, with default of 'None', which means randomly generate
					TraitType traitType = TraitType.None;
					// Get the value from config
					TraitName traitName = WobSettings.Get( "Traits", settingName, TraitName.Random );
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
			}
		}

		// Allowing contrarian on last heir slot
		[HarmonyPatch( typeof( LineageWindowController ), "CreateRandomCharacters" )]
		internal static class LineageWindowController_CreateRandomCharacters_Patch {
			internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
				WobPlugin.Log( "LineageWindowController.CreateRandomCharacters Transpiler Patch" );
				if( WobSettings.Get( "SoulShop", "ClassLockContrarian", false ) ) {
					// Set up the transpiler handler with the instruction list
					WobTranspiler transpiler = new WobTranspiler( instructions );
					// Perform the patching
					int invalidTraitType = Enum.GetValues( typeof( TraitType ) ).Cast<int>().Min() - 1;
					transpiler.PatchAll(
							// Define the IL code instructions that should be matched
							new List<WobTranspiler.OpTest> {
								// if (this.m_characterDataArray[i].TraitOne == TraitType.RandomizeKit)
								// if (this.m_characterDataArray[i].TraitTwo == TraitType.RandomizeKit)
								/*  0 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                             ), // this
								/*  1 */ new WobTranspiler.OpTest( OpCodes.Ldfld, name: "m_characterDataArray" ), // this.m_characterDataArray
								/*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc                             ), // i
								/*  3 */ new WobTranspiler.OpTest( OpCodes.Ldelem_Ref                          ), // this.m_characterDataArray[i]
								/*  4 */ new WobTranspiler.OpTest( OpCodes.Ldfld                               ), // this.m_characterDataArray[i].TraitOne
								/*  5 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, TraitType.RandomizeKit      ), // TraitType.RandomizeKit
								/*  6 */ new WobTranspiler.OpTest( OpCodes.Bne_Un                              ), // this.m_characterDataArray[i].TraitOne == TraitType.RandomizeKit
							},
							// Define the actions to take when an occurrence is found
							new List<WobTranspiler.OpAction> {
								new WobTranspiler.OpAction_SetOperand( 5, invalidTraitType ), // Set to a value outside the range of the TraitType enum
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