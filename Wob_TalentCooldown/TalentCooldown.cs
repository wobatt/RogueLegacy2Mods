using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace Wob_TalentCooldown {
    [BepInPlugin( "Wob.TalentCooldown", "Talent Cooldown Mod", "1.1.0" )]
    public partial class TalentCooldown : BaseUnityPlugin {
        
        private static readonly WobSettings.KeyHelper<AbilityType> keys = new WobSettings.KeyHelper<AbilityType>( "Talent" );
        
        private static readonly Dictionary<AbilityType, string> descriptions = new Dictionary<AbilityType, string>() {
            // Class talents
            { AbilityType.CloakTalent,          "Obscura - Creates a cloak that avoids all damage. Attacking will apply Vulnerable and end the effect. Recharges on hit."                            }, // Assassin
            { AbilityType.CometTalent,          "Comet Form - Gain Flight and Damage Immunity. Recharges over time."                                                                                 }, // Astromancer
            { AbilityType.ShoutTalent,          "Winter's Shout - Destroys Large Projectiles, and Freezes enemies. Recharges after getting hit."                                                     }, // Barbarian
            { AbilityType.CrescendoTalent,      "Crescendo - Shout, converting all Mid-sized Projectiles into notes. Recharges over time."                                                           }, // Bard
            { AbilityType.KnockoutTalent,       "Knockout Punch - Can be aimed. Consumes all Combo stacks. Damage increased for every consumed stack. Recharges on hit."                             }, // Boxer
            { AbilityType.CookingTalent,        "Stew - Restores your Health and Mana. Recharges by collecting Health Drops (Holds up to 3 charges)."                                                }, // Chef
            { AbilityType.StaticWallTalent,     "Bastion - Destroys Large Projectiles and pushes enemies back. Recharges on hit."                                                                    }, // Dragon Lancer
            { AbilityType.RollTalent,           "Combat Roll - Immune to all damage while Rolling. Recharges over time."                                                                             }, // Duelist
            { AbilityType.ManaBombTalent,       "Makeshift Explosive - Blocks Mid-sized Projectiles (holds 2 charges). Recharges over time (refills all charges)."                                   }, // Gunslinger
            { AbilityType.ShieldBlockTalent,    "Shield Block - Hold to block 50% of incoming damage and apply Vulnerable. Last second blocks prevent 100% of incoming damage. Recharges over time." }, // Knight
            { AbilityType.CrowsNestTalent,      "Pirate Ship - Creates a flying Pirate ship that you can freely move around. Shoots stuff and blows itself up. Recharges on hit."                    }, // Pirate
            { AbilityType.CreatePlatformTalent, "Ivy Canopy - Creates an ivy platform that blocks Mid-sized Projectiles and grants Spore Burst. Recharges over time."                                }, // Ranger
            { AbilityType.TeleSliceTalent,      "Immortal Kotetsu - Hold to aim. Teleport a set distance, hitting everything in between. Recharges over time."                                       }, // Ronin
            { AbilityType.SpearSpinTalent,      "Deflect - Destroy all Mid-sized Projectiles, restoring Mana. Recharges on hit and fully recharges on successful counters."                          }, // Valkyrie
            // Trait talents
            { AbilityType.SuperFart,            "Super Fart - Lifts you and applies Burn. Recharges over time."                                                                                      }, // Super IBS trait
        };

        public static void CreateSettings( AbilityType talentType, string settingName, bool cdIsTime, float cdAmount, int maxAmmo ) {
            if( !cdIsTime ) {
                WobSettings.Add( new WobSettings.Boolean( keys.New( talentType, settingName, "CooldownUseTime" ), "Use timed cooldown instead of default for " + descriptions[talentType], false ) );
            }
            WobSettings.Add( new WobSettings.Num<float>( keys.New( talentType, settingName, "CooldownAmount" ), "Cooldown seconds/hits/pickups for " + descriptions[talentType], cdAmount, bounds: (0f, 1000f) ) );
            if( maxAmmo > 0 ) {
                WobSettings.Add( new WobSettings.Num<int>( keys.New( talentType, settingName, "MaxAmmo" ), "Max charges (0 for infinite) for " + descriptions[talentType], maxAmmo, bounds: (0, 99) ) );
            }
        }

        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            // Main settings created in bulk
            CreateSettings( AbilityType.CloakTalent,          "Assassin_Obscura",              false, 10f, 0 );
            CreateSettings( AbilityType.CometTalent,          "Astromancer_CometForm",         true,  6f,  0 );
            CreateSettings( AbilityType.ShoutTalent,          "Barbarian_WintersShout",        false, 1f,  0 );
            CreateSettings( AbilityType.CrescendoTalent,      "Bard_Crescendo",                true,  12f, 0 );
            CreateSettings( AbilityType.KnockoutTalent,       "Boxer_KnockoutPunch",           false, 3f,  0 );
            CreateSettings( AbilityType.CookingTalent,        "Chef_Stew",                     false, 1f,  3 );
            CreateSettings( AbilityType.StaticWallTalent,     "DragonLancer_Bastion",          false, 6f,  0 );
            CreateSettings( AbilityType.RollTalent,           "Duelist_CombatRoll",            true,  2f,  0 );
            CreateSettings( AbilityType.ManaBombTalent,       "Gunslinger_MakeshiftExplosive", true,  8f,  2 );
            CreateSettings( AbilityType.ShieldBlockTalent,    "Knight_ShieldBlock",            true,  10f, 0 );
            CreateSettings( AbilityType.CrowsNestTalent,      "Pirate_PirateShip",             false, 8f,  0 );
            CreateSettings( AbilityType.CreatePlatformTalent, "Ranger_IvyCanopy",              true,  7f,  0 );
            CreateSettings( AbilityType.TeleSliceTalent,      "Ronin_ImmortalKotetsu",         true,  5f,  0 );
            CreateSettings( AbilityType.SpearSpinTalent,      "Valkyrie_Deflect",              false, 5f,  0 );
            CreateSettings( AbilityType.SuperFart,            "SuperIBS_SuperFart",            true,  3f,  0 );
            // Additional settings
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Boolean(    keys.Get( AbilityType.CreatePlatformTalent, "CooldownFromCast" ), "Start the cooldown timer at the start of the effect, not the end", false                     ),
                new WobSettings.Num<float>( keys.Get( AbilityType.ShieldBlockTalent,    "PerfectBlockTime" ), "Time between starting blocking and impact to get a perfect block", 0.135f, bounds: (0f, 60f) ),
                new WobSettings.Boolean(    keys.Get( AbilityType.ManaBombTalent,       "RefreshAllAmmo"   ), "Refresh all charges on cooldown instead of just 1 charge",         true                      ),
                new WobSettings.Num<float>( keys.Get( AbilityType.StaticWallTalent,     "Lifespan"         ), "Duration of the talent effect in seconds",                         3.26f,  bounds: (1f, 60f) ),
                new WobSettings.Num<float>( keys.Get( AbilityType.CreatePlatformTalent, "Lifespan"         ), "Duration of the talent effect in seconds",                         8f,     bounds: (1f, 60f) ),
            } );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        [HarmonyPatch( typeof( AbilityLibrary ), nameof( AbilityLibrary.Initialize ) )]
        internal static class AbilityLibrary_Initialize_Patch {
            private static bool runOnce = false;
            internal static void Postfix() {
                if( !runOnce ) {
                    foreach( AbilityType talentType in AbilityType_RL.TalentAbilityArray ) {
                        BaseAbility_RL talent = AbilityLibrary.GetAbility( talentType );
                        if( talent != null ) {
                            AbilityData talentData = talent.AbilityData;
                            if( talentData != null && keys.Exists( talentType ) ) {
                                bool cdTimeOverride = WobSettings.Get( keys.Get( talentType, "CooldownUseTime" ), false );
                                if( cdTimeOverride && !talentData.CooldownDecreaseOverTime ) {
                                    WobPlugin.Log( talentType + ": cooldown type -> time" );
                                    talentData.CooldownDecreaseOverTime = true;
                                    talentData.CooldownDecreasePerHit = false;
                                }
                                float cooldown = WobSettings.Get( keys.Get( talentType, "CooldownAmount" ), talentData.CooldownTime );
                                if( !( cdTimeOverride || talentData.CooldownDecreaseOverTime ) && cooldown > 0 ) {
                                    float cooldown2 = Mathf.Max( 1, Mathf.RoundToInt( cooldown ) );
                                    if( cooldown != cooldown2 ) {
                                        WobPlugin.Log( "CONFIG ERROR: Cooldown should be whole number if it isn't a time - using rounded value", WobPlugin.ERROR );
                                    }
                                    cooldown = cooldown2;
                                }
                                if( cooldown != talentData.CooldownTime ) {
                                    WobPlugin.Log( talentType + ": cooldown " + talentData.CooldownTime + " -> " + cooldown );
                                    talentData.CooldownTime = cooldown;
                                }
                                if( talentData.MaxAmmo > 0 ) {
                                    int maxAmmo = WobSettings.Get( keys.Get( talentType, "MaxAmmo"), talentData.MaxAmmo );
                                    WobPlugin.Log( talentType + ": max ammo " + talentData.MaxAmmo + " -> " + maxAmmo );
                                    talentData.MaxAmmo = maxAmmo;
                                }
                                if( talentType == AbilityType.ManaBombTalent ) {
                                    talentData.CooldownRefreshesAllAmmo = WobSettings.Get( keys.Get( talentType, "RefreshAllAmmo" ), true );
                                }
                            } else {
                                WobPlugin.Log( "WARNING: No settings for " + talentType );
                            }
                        }
                    }
                    runOnce = true;
                }
            }
        }

        // Set lifespan of cast effects for talent projectiles
        [HarmonyPatch( typeof( Projectile_RL ), nameof( Projectile_RL.Lifespan ), MethodType.Getter )]
        internal static class CaveLanternPostProcessingController_DarknessAmountWhenFullyLit_Patch {
            internal static void Postfix( Projectile_RL __instance, ref float __result ) {
                // Dragon Lancer's Bastion
                if( __instance.ProjectileData.Name == "PlayerStaticWallTalent" ) {
                    __result = WobSettings.Get( keys.Get( AbilityType.StaticWallTalent, "Lifespan" ), __result );
                    return;
                }
                // Ranger's Ivy Canopy
                if( __instance.ProjectileData.Name == "PlayerCreatePlatformTalent" ) {
                    __result = WobSettings.Get( keys.Get( AbilityType.CreatePlatformTalent, "Lifespan" ), __result );
                    return;
                }
            }
        }

        // Dragon Lancer's Bastion - show cooldown timer if overriding to timed cooldown
        [HarmonyPatch( typeof( StaticWall_Ability ), "FireProjectile" )]
        internal static class StaticWall_Ability_FireProjectile_Patch {
            internal static void Postfix( CreatePlatform_Ability __instance ) {
                if( WobSettings.Get( keys.Get( AbilityType.StaticWallTalent, "CooldownUseTime" ), false ) ) {
                    __instance.DecreaseCooldownOverTime = true;
                    __instance.DisplayPausedAbilityCooldown = false;
                }
            }
        }

        // Ranger's Ivy Canopy - start cooldown on cast rather than end of effect
        [HarmonyPatch( typeof( CreatePlatform_Ability ), "FireProjectile" )]
        internal static class CreatePlatform_Ability_FireProjectile_Patch {
            internal static void Postfix( CreatePlatform_Ability __instance ) {
                if( WobSettings.Get( keys.Get( AbilityType.CreatePlatformTalent, "CooldownFromCast" ), false ) ) {
                    __instance.DecreaseCooldownOverTime = true;
                    __instance.DisplayPausedAbilityCooldown = false;
                }
            }
        }

        // Knight's Shield - set the timing of ability perfect blocks
        [HarmonyPatch( typeof( ShieldBlock_Ability ), "OnPlayerBlocked" )]
        internal static class ShieldBlock_Ability_OnPlayerBlocked_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "ShieldBlock_Ability.OnPlayerBlocked Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            //m_isPerfectBlock = Time.time < m_abilityController.PlayerController.BlockStartTime + 0.135f;
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Call, name: "get_time"                 ), // Time.time
                            /*  1 */ new WobTranspiler.OpTest( OpCodeSet.Ldarg                                ), // this
                            /*  2 */ new WobTranspiler.OpTest( OpCodes.Ldfld                                  ), // this.m_abilityController
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_PlayerController" ), // this.m_abilityController.PlayerController
                            /*  4 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_BlockStartTime"   ), // this.m_abilityController.PlayerController.BlockStartTime
                            /*  5 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4                                 ), // 0.135f
                            /*  6 */ new WobTranspiler.OpTest( OpCodes.Add                                    ), // m_abilityController.PlayerController.BlockStartTime + 0.135f
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( keys.Get( AbilityType.ShieldBlockTalent, "PerfectBlockTime" ), 0.135f ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }
    }
}