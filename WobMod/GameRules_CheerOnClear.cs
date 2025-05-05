using System;
using System.Collections;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Wob_Common;
using RLAudio;
using System.Reflection;
using RL_Windows;

namespace WobMod {
    internal partial class GameRules {

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ MANAGER CLASS - CHEER ON CLEAR
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Class to manage crowd cheers, tracking kills in each room
        internal class CheerOnClear {
            // Static reference to current instance of the crowd cheer manager
            private static CheerOnClear instance = null;

            // Method to initialise, get, or refresh the static reference to the crowd cheer manager
            internal static CheerOnClear GetInstance( bool refresh = false ) {
                // Create a new crowd cheer manager if one doesn't already exist
                if( instance == null ) {
                    instance = new CheerOnClear();
                } else {
                    // Check if a refresh is wanted
                    if( refresh ) {
                        // Remove event listeners in the old instance
                        instance.RemoveListeners();
                        // Create a new instance to replace it
                        instance = new CheerOnClear();
                    }
                }
                // Return a reference to the current crowd cheer manager
                return instance;
            }

            // Property to check if cheering is enabled in user config
            private readonly bool enabled;
            // List of enemies in the current room with the coroutine added to them, so they can be cleared whn moving to the next room
            private readonly List<EnemyController> trackedEnemies = new();
            // Variables to track progress in the current room
            private int numEnemiesToKill = 0;
            private int numEnemiesKilled = 0;
            private bool useLongCheer = false;
            // References to the listener methods, so they can be cleaned up when the instance is replaced
            private readonly Action<MonoBehaviour, EventArgs> onPlayerEnterRoom;
            private readonly Action<MonoBehaviour, EventArgs> onEnemySummoned;
            private readonly Action<MonoBehaviour, EventArgs> onEnemyDeath;
            private readonly Action<MonoBehaviour, EventArgs> onPlayerDeath;
            // Audio resource paths extracted from the serialized data files
            private const string AUDIO_PATH_CHEER_SHORT  = "event:/UI/InGame/ui_ig_trait_diva_crowdCheer_short";
            private const string AUDIO_PATH_CHEER_LONG   = "event:/UI/InGame/ui_ig_trait_diva_crowdCheer_long";
            private const string AUDIO_PATH_ROSES_THROWN = "event:/SFX/Interactables/sfx_trait_diva_flowers";
            private const string AUDIO_PATH_PLAYER_DEATH = "event:/UI/FrontEnd/ui_fe_death_diva_crowd";

            // Constructor that reads user config and sets up event listeners
            private CheerOnClear() {
                // Read user config to check if cheering is wanted
                this.enabled = WobSettings.Get( "Display", "CheerOnClear", false );
                // Create references to the listener methods
                this.onPlayerEnterRoom = new Action<MonoBehaviour, EventArgs>( this.OnPlayerEnterRoom );
                this.onEnemySummoned   = new Action<MonoBehaviour, EventArgs>( this.OnEnemySummoned   );
                this.onEnemyDeath      = new Action<MonoBehaviour, EventArgs>( this.OnEnemyDeath      );
                this.onPlayerDeath     = new Action<MonoBehaviour, EventArgs>( this.OnPlayerDeath     );
                // Only add listeners if cheering is wanted - this class will do nothing without this
                if( this.enabled ) {
                    Messenger<GameMessenger, GameEvent>.AddListener( GameEvent.PlayerEnterRoom, this.onPlayerEnterRoom );
                    Messenger<GameMessenger, GameEvent>.AddListener( GameEvent.EnemySummoned,   this.onEnemySummoned   );
                    Messenger<GameMessenger, GameEvent>.AddListener( GameEvent.EnemyDeath,      this.onEnemyDeath      );
                    Messenger<GameMessenger, GameEvent>.AddListener( GameEvent.PlayerDeath,     this.onPlayerDeath     );
                }
            }

            // Method to remove event listeners when the static instance will be replaced
            private void RemoveListeners() {
                // Only remove listeners if they were set up previously (i.e. cheering enabled in user config)
                if( this.enabled ) {
                    Messenger<GameMessenger, GameEvent>.RemoveListener( GameEvent.PlayerEnterRoom, this.onPlayerEnterRoom );
                    Messenger<GameMessenger, GameEvent>.RemoveListener( GameEvent.EnemySummoned,   this.onEnemySummoned   );
                    Messenger<GameMessenger, GameEvent>.RemoveListener( GameEvent.EnemyDeath,      this.onEnemyDeath      );
                    Messenger<GameMessenger, GameEvent>.RemoveListener( GameEvent.PlayerDeath,     this.onPlayerDeath     );
                }
            }

            // Event listener method for when you move into a new room
            private void OnPlayerEnterRoom( MonoBehaviour sender, EventArgs args ) {
                // Reset enemy totals
                numEnemiesKilled = 0;
                numEnemiesToKill = 0;
                useLongCheer = false;
                // Stop each of the coroutines that logs when an enemy activates for each enemy in the old room
                foreach( EnemyController enemy in trackedEnemies ) {
                    enemy.StopCoroutine( this.EnableOnEnemyCoroutine( enemy, true ) );
                }
                // Empty the list ready for the new room
                this.trackedEnemies.Clear();
                // Get the room you have just entered
                BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
                // Loop through each of the enemy spawns
                foreach( EnemySpawnController enemySpawnController in currentPlayerRoom.SpawnControllerManager.EnemySpawnControllers ) {
                    // Not interested in any that don't spawn
                    if( enemySpawnController.ShouldSpawn ) {
                        // Get the enemy from the spawner
                        EnemyController enemyInstance = enemySpawnController.EnemyInstance;
                        if( enemyInstance ) {
                            // Add the enemy to the list for clean-up in the next room
                            this.trackedEnemies.Add( enemyInstance );
                            // Start the coroutines that logs when the enemy activates and should be added to the 'to kill' total
                            enemyInstance.StartCoroutine( this.EnableOnEnemyCoroutine( enemyInstance, true ) );
                        }
                    }
                }
            }

            // Event listener method for when an enemy is activated or summoned in a room
            private void OnEnemySummoned( MonoBehaviour sender, EventArgs args ) {
                if( args is EnemySummonedEventArgs enemySummonedEventArgs ) {
                    EnemyController summonedEnemy = enemySummonedEventArgs.SummonedEnemy;
                    if( summonedEnemy ) {
                        // Add the enemy to the list for clean-up in the next room
                        this.trackedEnemies.Add( summonedEnemy );
                        // Start the coroutines that logs when the enemy activates and should be added to the 'to kill' total
                        summonedEnemy.StartCoroutine( this.EnableOnEnemyCoroutine( summonedEnemy, true ) );
                    }
                }
            }

            // Coroutine that logs when the enemy activates and should be added to the 'to kill' total
            private IEnumerator EnableOnEnemyCoroutine( EnemyController enemyInstance, bool increment ) {
                // Wait for the enemy to be initialized
                while( !enemyInstance.IsInitialized ) {
                    yield return null;
                }
                // Check if they are summoned or will be when the fairy room lever is activated
                if( enemyInstance.IsSummoned || enemyInstance.ActivatedByFairyRoomTrigger ) {
                    // Wait until they are summoned
                    while( !enemyInstance.IsBeingSummoned ) {
                        if( !enemyInstance.gameObject.activeSelf ) {
                            yield break;
                        } else {
                            yield return null;
                        }
                    }
                }
                // Check if we should add the enemy to the room total
                if( increment ) {
                    EnemyType enemyType = enemyInstance.EnemyType;
                    if( enemyType != EnemyType.BouncySpike && ( enemyType != EnemyType.Dummy || ( enemyType == EnemyType.Dummy && enemyInstance.EnemyRank == EnemyRank.Miniboss ) ) && enemyType != EnemyType.Eggplant ) {
                        // Add the enemy to the room total
                        numEnemiesToKill++;
                    }
                }
                // All done
                yield break;
            }

            // Method used in the final boss room to add the boss to the 'to kill' total
            internal void ApplyOnEnemy( EnemyController enemyInstance, bool increment ) {
                // Check that cheers are wanted
                if( this.enabled ) {
                    // Add the enemy to the list for clean-up in the next room
                    this.trackedEnemies.Add( enemyInstance );
                    // Start the coroutines that logs when the enemy activates and should be added to the 'to kill' total
                    enemyInstance.StartCoroutine( this.EnableOnEnemyCoroutine( enemyInstance, increment ) );
                }
            }

            // Event listener method for when an enemy dies
            private void OnEnemyDeath( MonoBehaviour sender, EventArgs args ) {
                // Add to the number of kills
                this.numEnemiesKilled++;
                // Make sure the room isn't summoning more enemies
                if( PlayerManager.GetCurrentPlayerRoom().GetComponentInChildren<SummonRuleController>()?.StillSummoning ?? false ) { return; }
                // Make sure the Diva trait isn't going to cheer as well
                if( TraitManager.IsTraitActive( TraitType.CheerOnKills ) ) { return; }
                // Check the enemy to decide which cheer sound to play on clearing the room
                this.useLongCheer = this.useLongCheer || this.UseLongCheer( ( args as EnemyDeathEventArgs ).Victim );
                // Check if all enemies have been killed
                if( this.numEnemiesKilled >= this.numEnemiesToKill && this.numEnemiesToKill > 0 ) {
                    // Show the thrown roses visual effect
                    EffectManager.PlayEffect( PlayerManager.GetPlayerController().gameObject, null, "CheerOnKills_Roses_Effect", CameraController.ForegroundCam.transform.position, 0f, EffectStopType.Gracefully, EffectTriggerDirection.None );
                    // Audio for the thrown roses
                    AudioManager.PlayOneShot( null, AUDIO_PATH_ROSES_THROWN );
                    // Check which cheer sound to play
                    if( this.useLongCheer ) {
                        // Play a long crowd cheer audio effect
                        AudioManager.PlayOneShot( null, AUDIO_PATH_CHEER_LONG );
                    } else {
                        // Play a short croed cheer audio effect
                        AudioManager.PlayOneShot( null, AUDIO_PATH_CHEER_SHORT );
                    }
                }
            }

            private bool UseLongCheer( EnemyController enemy ) {
                // bosses.Contains( ( args as EnemyDeathEventArgs ).Victim.EnemyType ) || commanders.Contains( ( args as EnemyDeathEventArgs ).Victim.EnemyRank )
                WobPlugin.Log( "[GameRules.CheerOnClear] Enemy killed - Type: " + enemy.EnemyType + ", Rank: " + enemy.EnemyRank + ", Commander: " + enemy.IsCommander + ", Boss: " + enemy.IsBoss + ", Progress: " + this.numEnemiesKilled + "/" + this.numEnemiesToKill );
                return enemy.IsCommander || enemy.IsBoss || enemy.EnemyRank == EnemyRank.Miniboss;
            }

            // Event listener method for when the player dies
            private void OnPlayerDeath( MonoBehaviour sender, EventArgs args ) {
                // Check if the death is actually retirement following victory
                if( ( WindowManager.GetWindowController( WindowID.PlayerDeath ) as PlayerDeathWindowController )?.RunVictoryDeathScreen ?? false ) {
                    // Victory - play a crowd cheer
                    AudioManager.PlayOneShot( null, AUDIO_PATH_CHEER_LONG );
                } else {
                    // Death - play an 'oof'
                    AudioManager.PlayOneShot( null, AUDIO_PATH_PLAYER_DEATH );
                }
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - CHEER ON CLEAR
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Patch one of the methods that control the final boss room so that cheering is enabled
        [HarmonyPatch()]
        internal static class FinalBossRoomController_StartIntro_Patch {
            // Find the 'MoveNext' method on the nested class of 'FinalBossRoomController' that 'StartIntro' implicitly created
            internal static MethodInfo TargetMethod() {
                return AccessTools.FirstMethod( AccessTools.FirstInner( typeof( FinalBossRoomController ), t => t.Name.Contains( "<StartIntro>d__" ) ), method => method.Name.Contains( "MoveNext" ) );
            }
            // Pass the code to the common transpiler method for both patches
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                return CommonTranspiler( instructions, "FinalBossRoomController.StartIntro" );
            }
        }

        // Patch one of the methods that control the final boss room so that cheering is enabled
        [HarmonyPatch()]
        internal static class FinalBossRoomController_StartIntro2_Patch {
            // Find the 'MoveNext' method on the nested class of 'FinalBossRoomController' that 'StartIntro2' implicitly created
            internal static MethodInfo TargetMethod() {
                return AccessTools.FirstMethod( AccessTools.FirstInner( typeof( FinalBossRoomController ), t => t.Name.Contains( "<StartIntro2>d__" ) ), method => method.Name.Contains( "MoveNext" ) );
            }
            // Pass the code to the common transpiler method for both patches
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                return CommonTranspiler( instructions, "FinalBossRoomController.StartIntro2" );
            }
        }

        // Common transpiler for both final boss room methods
        private static IEnumerable<CodeInstruction> CommonTranspiler( IEnumerable<CodeInstruction> instructions, string methodName ) {
            // Set up the transpiler handler with the instruction list
            WobTranspiler transpiler = new( instructions, methodName );
            // Perform the patching
            transpiler.PatchAll(
                // Define the IL code instructions that should be matched
                new List<WobTranspiler.OpTest> {
                    /*  0 */ new( OpCodes.Ldc_I4, TraitType.CheerOnKills ), // TraitType.CheerOnKills
                    /*  1 */ new( OpCodes.Call, name: "IsTraitActive"    ), // TraitManager.IsTraitActive( TraitType.CheerOnKills )
                    /*  2 */ new( OpCodeSet.Brfalse                      ), // if( TraitManager.IsTraitActive( TraitType.CheerOnKills ) )
                    /*  3 */ new( OpCodes.Ldc_I4, TraitType.CheerOnKills ), // TraitType.CheerOnKills
                    /*  4 */ new( OpCodes.Call, name: "GetActiveTrait"   ), // TraitManager.GetActiveTrait( TraitType.CheerOnKills )
                    /*  5 */ new( OpCodes.Isinst                         ), // TraitManager.GetActiveTrait( TraitType.CheerOnKills ) as CheerOnKills_Trait
                    /*  6 */ new( OpCodeSet.Ldloc                        ), // base
                    /*  7 */ new( OpCodes.Call, name: "get_Boss"         ), // base.Boss
                    /*  8 */ new( OpCodes.Ldc_I4_0                       ), // false
                    /*  9 */ new( OpCodes.Callvirt, name: "ApplyOnEnemy" ), // ( TraitManager.GetActiveTrait( TraitType.CheerOnKills ) as CheerOnKills_Trait ).ApplyOnEnemy( base.Boss, false )
                },
                // Define the actions to take when an occurrence is found
                new List<WobTranspiler.OpAction> {
                    new WobTranspiler.OpAction_Remove( 0, 6 ), // Remove the conditional and get active trait
                    new WobTranspiler.OpAction_SetInstruction( 9, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => PatchApplyOnEnemy( null, false ) ) ), // replace the final method call with a patched one
                }, expected: 1 );
            // Return the modified instructions
            return transpiler.GetResult();
        }

        // Patched method used in the common transpiler
        private static void PatchApplyOnEnemy( EnemyController enemyInstance, bool increment ) {
            WobPlugin.Log( "[GameRules.CheerOnClear] Patch on boss called " + enemyInstance.EnemyType );
            // Do the check that was removed from the original for the Diva trait
            if( TraitManager.IsTraitActive( TraitType.CheerOnKills ) ) {
                // Apply the Diva trait cheer check from the original
                ( TraitManager.GetActiveTrait( TraitType.CheerOnKills ) as CheerOnKills_Trait ).ApplyOnEnemy( enemyInstance, increment );
            } else {
                // Apply the new cheer check instead
                CheerOnClear.GetInstance().ApplyOnEnemy( enemyInstance, increment );
            }
        }

    }
}
