using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace Wob_TraitStats {
    [BepInPlugin( "Wob.TraitStats", "Trait Stats Mod", "0.1.0" )]
	// The function of these plugins is replaced by this one
	public partial class TraitStats : BaseUnityPlugin {
		// Main method that kicks everything off
		private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
			// Create/read the mod specific configuration options
			WobPlugin.Settings.Add( new WobSettings.Entry[] {
				// Positive health modifiers
				new WobSettings.Scaled<int>(   "Trait_MegaHealth",          "Health",       "Health modifier for Hero Complex - +100% Health but you can't heal, ever.",                                                                          100,   0.01f, bounds: (-99, 1000000) ),
				// Negative health modifiers
				new WobSettings.Scaled<int>(   "Trait_BonusMagicStrength",  "Health",       "Health modifier for Crippling Intellect - -50% Health and -50% Weapon Damage. Mana regenerates over time.",                                          -50,   0.01f, bounds: (-99, 1000000) ),
				new WobSettings.Scaled<int>(   "Trait_BounceTerrain",       "Health",       "Health modifier for Clownanthropy - -30% Health, but you can Spin Kick off terrain.",                                                                -30,   0.01f, bounds: (-99, 1000000) ),
				new WobSettings.Scaled<int>(   "Trait_CanNowAttack",        "Health",       "Health modifier for Pacifier - -60% Health and you love to fight!",                                                                                  -60,   0.01f, bounds: (-99, 1000000) ),
				new WobSettings.Scaled<int>(   "Trait_CantAttack",          "Health",       "Health modifier for Pacifist - -60% Health and you can't deal damage.",                                                                              -60,   0.01f, bounds: (-99, 1000000) ),
				new WobSettings.Scaled<int>(   "Trait_DamageBoost",         "Health",       "Health modifier for Combative - +50% Weapon Damage, -25% Health.",                                                                                   -25,   0.01f, bounds: (-99, 1000000) ),
				new WobSettings.Scaled<int>(   "Trait_InvulnDash",          "Health",       "Health modifier for Evasive - Invincible while dashing, but you have -50% Health, and dashing dodges has a cooldown.",                               -50,   0.01f, bounds: (-99, 1000000) ),
				new WobSettings.Scaled<int>(   "Trait_MagicBoost",          "Health",       "Health modifier for Bookish - +50% Magic Damage and +50 Mana Capacity. -25% Health.",                                                                -25,   0.01f, bounds: (-99, 1000000) ),
				new WobSettings.Scaled<int>(   "Trait_OmniDash",            "Health",       "Health modifier for Superfluid - -20% Health, but you can dash in ANY direction.",                                                                   -20,   0.01f, bounds: (-99, 1000000) ),
				new WobSettings.Scaled<int>(   "Trait_RevealAllChests",     "Health",       "Health modifier for Spelunker - -10% Health but you can see all chests on the map!",                                                                 -10,   0.01f, bounds: (-99, 1000000) ),
				new WobSettings.Scaled<int>(   "Trait_SmallHitbox",         "Health",       "Health modifier for Disattuned/Only Heart - -25% health, but you can only be hit in the heart.",                                                     -25,   0.01f, bounds: (-99, 1000000) ),
				// Health loss per hit modifiers
				new WobSettings.Scaled<float>( "Trait_SuperHealer",         "LossPerHit",   "Max health percent lost per hit for Hypercoagulation/Super Healer - HP regenerates, but you lose some Max HP when hit.",                             6.25f, 0.01f, bounds: (0f,  100f   ) ),
				// Max mana modifiers
				new WobSettings.Scaled<int>(   "Trait_MagicBoost",          "MaxMana",      "Max mana modifier for Bookish - +50% Magic Damage and +50 Mana Capacity. -30% Health.",                                                              50,    0.01f, bounds: (-99, 1000000) ),
				// Mana from taking damage modifiers
				new WobSettings.Scaled<int>(   "Trait_ManaFromHurt",        "ManaRegen",    "Mana gain from damage for Masochism - Gain 50% of your mana when hit, but can't regain mana from attacks.",                                          50,    0.01f, bounds: (0,   1000000) ),
				// Vampiric regen modifiers
				new WobSettings.Scaled<int>(   "Trait_Vampire",             "DamageRegen",  "Health from damage modifier for Vampirism - Gain 20% of your Weapon Damage as Health, but you take 125% more damage.",                               20,    0.01f, bounds: (0,   1000000) ),
				// Damage taken modifiers
				new WobSettings.Scaled<int>(   "Trait_Vampire",             "DamageTaken",  "Damage taken modifier for Vampirism - Gain 20% of your Weapon Damage as Health, but you take 125% more damage.",                                     125,   0.01f, bounds: (-99, 1000000) ),
				// Weapon damage modifiers
				new WobSettings.Scaled<int>(   "Trait_BonusMagicStrength",  "WeaponDamage", "Weapon damage modifier for Crippling Intellect - -50% Health and -50% Weapon Damage. Mana regenerates over time.",                                   -50,   0.01f, bounds: (-99, 1000000) ),
				new WobSettings.Scaled<int>(   "Trait_DamageBoost",         "WeaponDamage", "Weapon damage modifier for Combative - +50% Weapon Damage, -25% Health.",                                                                            50,    0.01f, bounds: (-99, 1000000) ),
				// Magic damage modifiers
				new WobSettings.Scaled<int>(   "Trait_BonusMagicStrength",  "MagicDamage",  "Magic damage modifier for Crippling Intellect - -50% Health and -50% Weapon Damage. Mana regenerates over time.",                                    0,     0.01f, bounds: (-99, 1000000) ),
				new WobSettings.Scaled<int>(   "Trait_MagicBoost",          "MagicDamage",  "Magic damage modifier for Bookish - +50% Magic Damage and +50 Mana Capacity. -30% Health.",                                                          50,    0.01f, bounds: (-99, 1000000) ),
				// Spell damage modifiers
				new WobSettings.Scaled<int>(   "Trait_ManaCostAndDamageUp", "SpellDamage",  "Spell damage modifier for Emotional Dysregularity/Overcompensation - Mana costs and spell damage are increased by 100%.",                            100,   0.01f, bounds: (-99, 1000000) ),
				// Spell cost modifiers
				new WobSettings.Scaled<int>(   "Trait_ManaCostAndDamageUp", "SpellCost",    "Spell cost modifier for Emotional Dysregularity/Overcompensation - Mana costs and spell damage are increased by 100%.",                              100,   0.01f, bounds: (-99, 1000000) ),
				// Disarm time
				new WobSettings.Entry<float>(  "Trait_DisarmOnHurt",        "DisarmTime",   "Seconds of being disarmed for FND/Shocked - Taking damage inflicts Disarmed, meaning the player cannot attack or use spells for 2 seconds.",         2f,           bounds: (0f,  3600f  ) ),
			} );
			
			// Apply the patches if the mod is enabled
			WobPlugin.Patch();
        }

		// Method that checks if a trait is active on the current heir, and gets the new modifier from settings if it is
		private static float GetActiveMod( TraitType traitType, string modType, float defaultMod ) {
			float modifier = 0f;
			if( TraitManager.IsTraitActive( traitType ) ) {
				modifier = WobPlugin.Settings.Get( "Trait_" + traitType.ToString(), "Health", defaultMod );
				if( modifier != defaultMod ) {
					WobPlugin.Log( "Changing " + traitType + " " + modType + " mod to " + modifier );
				}
			}
			return modifier;
		}

		// Apply max health modifiers
		[HarmonyPatch( typeof( PlayerController ), "InitializeTraitHealthMods" )]
        static class PlayerController_InitializeTraitHealthMods_Patch {
            static void Postfix( PlayerController __instance ) {
				float healthMod = 0f;
				// I have no idea what this trait is, but it is in the original method so I'm including it here
				if( TraitManager.IsTraitActive( TraitType.BonusHealth ) ) { healthMod += 0.1f; }
				// Positive modifiers
				healthMod += GetActiveMod( TraitType.MegaHealth,         "Health", 1f    );
				// Negative modifiers
				healthMod += GetActiveMod( TraitType.BonusMagicStrength, "Health", -0.5f  );
				healthMod += GetActiveMod( TraitType.BounceTerrain,      "Health", -0.3f  );
				healthMod += GetActiveMod( TraitType.CanNowAttack,       "Health", -0.6f  );
				healthMod += GetActiveMod( TraitType.CantAttack,         "Health", -0.6f  );
				healthMod += GetActiveMod( TraitType.DamageBoost,        "Health", -0.25f );
				healthMod += GetActiveMod( TraitType.InvulnDash,         "Health", -0.5f  );
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
		static class PlayerController_InitializeTraitMaxManaMods_Patch {
			static void Postfix( PlayerController __instance ) {
				float manaMod = 0f;
				// Positive modifiers
				manaMod += GetActiveMod( TraitType.MagicBoost, "MaxMana", 0.5f );
				// Return the calculated value, overriding the original method
				__instance.TraitMaxManaMod = manaMod;
			}
		}

		// Apply disarm time
		[HarmonyPatch( typeof( StatusEffectController ), nameof( StatusEffectController.StartStatusEffect ) )]
		static class StatusEffectController_StartStatusEffect_Patch {
			static bool Prefix( StatusEffectType statusEffectType, ref float duration, IDamageObj caster ) {
				if( statusEffectType == StatusEffectType.Player_Disarmed && caster == null ) {
					duration = GetActiveMod( TraitType.DisarmOnHurt, "DisarmTime", 2f );
					return duration > 0;
				}
				return true;
			}
		}

		// Apply weapon and magic damage modifiers
		[HarmonyPatch( typeof( ProjectileManager ), nameof( ProjectileManager.ApplyProjectileDamage ) )]
		static class ProjectileManager_ApplyProjectileDamage_Patch {
			static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
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
							new WobTranspiler.OpAction_SetOperand( 6,  WobPlugin.Settings.Get( "Trait_BonusMagicStrength", "WeaponDamage", -0.5f ) ),
							new WobTranspiler.OpAction_SetOperand( 15, WobPlugin.Settings.Get( "Trait_DamageBoost",        "WeaponDamage",  0.5f ) ),
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
							new WobTranspiler.OpAction_SetOperand( 6,  WobPlugin.Settings.Get( "Trait_BonusMagicStrength", "MagicDamage", 0.0f ) ),
							new WobTranspiler.OpAction_SetOperand( 15, WobPlugin.Settings.Get( "Trait_MagicBoost",        "MagicDamage", 0.5f ) ),
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
							new WobTranspiler.OpAction_SetOperand( 23, WobPlugin.Settings.Get( "Trait_ManaCostAndDamageUp", "SpellDamage", 1f ) ),
						} );
				// Return the modified instructions
				return transpiler.GetResult();
			}
		}

		// Apply damage taken modifiers
		[HarmonyPatch( typeof( EnemyHitResponse ), "CharacterDamaged" )]
		static class EnemyHitResponse_CharacterDamaged_Patch {
			static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
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
							new WobTranspiler.OpAction_SetOperand( 15, WobPlugin.Settings.Get( "Trait_Vampire", "DamageRegen", 0.2f ) ),
                        } );
				// Return the modified instructions
				return transpiler.GetResult();
			}
		}

		// Apply damage taken modifiers
		[HarmonyPatch( typeof( PlayerController ), nameof( PlayerController.CalculateDamageTaken ) )]
		static class PlayerController_CalculateDamageTaken_Patch {
			static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
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
							new WobTranspiler.OpAction_SetOperand( 4, WobPlugin.Settings.Get( "Trait_Vampire", "DamageTaken", 1.25f ) ),
                        } );
				// Return the modified instructions
				return transpiler.GetResult();
			}
		}

		// Apply damage taken modifiers
		[HarmonyPatch( typeof( ManaRegen ), "OnPlayerHit" )]
		static class ManaRegen_OnPlayerHit_Patch {
			static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
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
							new WobTranspiler.OpAction_SetOperand( 2, WobPlugin.Settings.Get( "Trait_ManaFromHurt", "ManaRegen", 0.5f ) ),
                        } );
				// Return the modified instructions
				return transpiler.GetResult();
			}
		}
		
		// Apply max health loss per hit modifiers
		[HarmonyPatch( typeof( SuperHealer_Trait ), "OnPlayerHit" )]
		static class SuperHealer_Trait_OnPlayerHit_Patch {
			static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
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
							new WobTranspiler.OpAction_SetOperand( 1, WobPlugin.Settings.Get( "Trait_SuperHealer", "LossPerHit", 0.0625f ) ),
                        } );
				// Return the modified instructions
				return transpiler.GetResult();
			}
		}
		
		// Apply spell cost modifiers
		[HarmonyPatch( typeof( BaseAbility_RL ), nameof( BaseAbility_RL.ActualCost ), MethodType.Getter )]
		static class BaseAbility_RL_ActualCost_Patch {
			// Multiply the cost by the modifier + 1 (100% + additional % from config)
			static void Postfix( ref int __result ) {
				__result = Mathf.RoundToInt( __result * ( 1f + GetActiveMod( TraitType.ManaCostAndDamageUp, "SpellCost", 1f ) ) );
			}
			// Patch to set the multiplier in the original method to 1, effectively removing it so we can apply a new modifier in the postfix patch
			static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
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
		
	}
}
