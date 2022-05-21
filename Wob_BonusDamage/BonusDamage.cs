using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_BonusDamage {
    [BepInPlugin( "Wob.BonusDamage", "Bonus Damage Mod", "1.0.0" )]
    public partial class BonusDamage : BaseUnityPlugin {
        // Cache of settings for quick lookup against the EnemyRank enum
        private static readonly Dictionary<int,float> rankBonus = new Dictionary<int,float>();
        private static int EnemyRank_Boss = -1;
        // List of bosses to give the bonus on
        private static readonly HashSet<EnemyType> bosses = new HashSet<EnemyType> {
            /* Lamech  */ EnemyType.SpellswordBoss,
            /* Pirates */ EnemyType.SkeletonBossA, EnemyType.SkeletonBossB,
            /* Naamah  */ EnemyType.DancingBoss,
            /* Enoch   */ EnemyType.StudyBoss, EnemyType.MimicChestBoss,
            /* Irad    */ EnemyType.EyeballBoss_Left, EnemyType.EyeballBoss_Right, EnemyType.EyeballBoss_Bottom, EnemyType.EyeballBoss_Middle,
            /* Tubal   */ EnemyType.CaveBoss,
            /* Jonah   */ EnemyType.TraitorBoss,
            /* Cain    */ EnemyType.FinalBoss,
        };

        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobPlugin.Settings.Add( new WobSettings.Entry[] {
                new WobSettings.Scaled<float>( "Tier1Bonus",    "Deal this % bonus damage to all tier 1 (basic) variant enemies",     0f,  0.01f, bounds: (0f, 1000000f) ),
                new WobSettings.Scaled<float>( "Tier2Bonus",    "Deal this % bonus damage to all tier 2 (advanced) variant enemies",  0f,  0.01f, bounds: (0f, 1000000f) ),
                new WobSettings.Scaled<float>( "Tier3Bonus",    "Deal this % bonus damage to all tier 3 (commander) variant enemies", 0f,  0.01f, bounds: (0f, 1000000f) ),
                new WobSettings.Scaled<float>( "MinibossBonus", "Deal this % bonus damage to all minibosses",                         0f,  0.01f, bounds: (0f, 1000000f) ),
                new WobSettings.Scaled<float>( "BossBonus",     "Deal this % bonus damage to all bosses",                             0f,  0.01f, bounds: (0f, 1000000f) ),
                new WobSettings.Scaled<float>( "InsightBonus",  "Deal this % bonus damage to a boss for resolving their insight",     15f, 0.01f, bounds: (0f, 1000000f) ),
            } );
            // Cache the settings into a dictionary based on the EnemyRank enum
            rankBonus.Add( (int)EnemyRank.Basic,    WobPlugin.Settings.Get( "Tier1Bonus",    0f ) );
            rankBonus.Add( (int)EnemyRank.Advanced, WobPlugin.Settings.Get( "Tier2Bonus",    0f ) );
            rankBonus.Add( (int)EnemyRank.Expert,   WobPlugin.Settings.Get( "Tier3Bonus",    0f ) );
            rankBonus.Add( (int)EnemyRank.Miniboss, WobPlugin.Settings.Get( "MinibossBonus", 0f ) );
            // Bosses don't have a unique EnemyRank, so use one outside the range in the enum
            EnemyRank_Boss = Enum.GetValues( typeof( EnemyRank ) ).Cast<int>().Min() - 1;
            rankBonus.Add( EnemyRank_Boss, WobPlugin.Settings.Get( "BossBonus", 0f ) );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Apply damage dealt modifiers on boss fights
        [HarmonyPatch( typeof( EnemyController ), "GetInsightPlayerDamageMod" )]
        internal static class EnemyController_GetInsightPlayerDamageMod_Patch {
            // Modify the insight damage bonuses
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "EnemyController.GetInsightPlayerDamageMod Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4, Insight_EV.INSIGHT_PLAYER_DAMAGE_MOD ), // 1.15f
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ret                                          ), // return 1.15f
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetOperand( 0, ( WobPlugin.Settings.Get( "InsightBonus", 0.15f ) + 1f ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            // Add the additional bonus for all
            internal static void Postfix( EnemyController __instance, ref float __result ) {
                // Check if the enemy is a boss
                if( bosses.Contains( __instance.EnemyType ) ) {
                    __result += rankBonus[EnemyRank_Boss];
                } else {
                    // For non-bosses, get the bonus based on the rank
                    if( rankBonus.TryGetValue( (int)__instance.EnemyRank, out float bonus ) ) {
                        __result += bonus;
                    }
                }
                // Print to the log if the multiplier is not one - i.e. has probably been changed by these patches
                if( __result != 1f ) {
                    WobPlugin.Log( __instance.EnemyType + "." + __instance.EnemyRank + " hit, damage modifier " + __result );
                }
            }
        }
    }
}