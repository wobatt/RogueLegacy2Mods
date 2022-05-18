using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_Regeneration {
    [BepInPlugin( "Wob.Regeneration", "Regeneration Mod", "0.3.0" )]
    public partial class Regeneration : BaseUnityPlugin {
        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobPlugin.Settings.Add( new WobSettings.Entry[] {
                // Health related
                new WobSettings.EntryBool(  "HealthRegenEnabled", "Enable mana regeneration",                                                        true                   ),
                new WobSettings.Entry<int>( "HealthRegenTicks",   "Number of ticks/frames between adding health - higher number means slower regen", 30, bounds: (1, 1000)  ),
                new WobSettings.Entry<int>( "HealthRegenAdd",     "When health is added, this is the amount to add",                                 1,  bounds: (1, 10000) ),
                // Mana related
                new WobSettings.EntryBool(  "ManaRegenEnabled",   "Enable mana regeneration",                                                        true                   ),
                new WobSettings.Entry<int>( "ManaRegenTicks",     "Number of ticks/frames between adding mana - higher number means slower regen",   1,  bounds: (1, 1000)  ),
                new WobSettings.Entry<int>( "ManaRegenAdd",       "When mana is added, this is the amount to add",                                   1,  bounds: (1, 10000) ),
                new WobSettings.EntryBool(  "ManaRegenDelay",     "Enable the 2 second delay to mana regeneration after casting a spell",            true                   ),
                new WobSettings.Entry<int>( "MaxMana",            "Additional max mana",                                                             0,  bounds: (0, 10000) ),
            } );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Patch to add extra max mana - this method gets the max mana added by runes, just add a flat amount to its return value
        [HarmonyPatch( typeof( RuneLogicHelper ), nameof( RuneLogicHelper.GetMaxManaFlat ) )]
        static class RuneLogicHelper_GetMaxManaFlat_Patch {
            static void Postfix( ref int __result ) {
                __result += WobPlugin.Settings.Get( "ManaMax", 0 );
            }
        }

        // Patch to the method that controls mana regen
        [HarmonyPatch( typeof( ManaRegen ), "Update" )]
        static class ManaRegen_Update_Health_Patch {
            // Counter for number of frames - every X ticks add Y health
            private static int tick = 0;
            // Increment counter and return amount of health to regen
            private static int Tick() {
                tick = ( tick + 1 ) % WobPlugin.Settings.Get( "HealthRegenTicks", 30 );
                return tick == 0 ? WobPlugin.Settings.Get( "HealthRegenAdd", 1 ) : 0;
            }
            static void Prefix( ManaRegen __instance ) {
                // Continue if we are enabling regen
                if( WobPlugin.Settings.Get( "HealthRegenEnabled", false ) ) {
                    // Get a reference to the private field where current player info is stored
                    PlayerController m_playerController = (PlayerController)Traverse.Create( __instance ).Field( "m_playerController" ).GetValue();
                    // Get the current health state
                    float actualMaxHealth = m_playerController.ActualMaxHealth;
                    float currentHealth = m_playerController.CurrentHealth;
                    // Check that health is missing
                    if( currentHealth < actualMaxHealth ) {
                        // Calculate how much health to regenerate this frame
                        float regenRate = Tick();
                        if( regenRate > 0 ) {
                            // Check if this would take us over the maximum
                            if( regenRate + currentHealth >= actualMaxHealth ) {
                                // Set health to maximum
                                m_playerController.SetHealth( actualMaxHealth, false, true );
                            } else {
                                // Add regen amount to current health
                                m_playerController.SetHealth( regenRate, true, true );
                            }
                        }
                    }
                }
            }
        }

        // Patch to the method that controls mana regen
        [HarmonyPatch( typeof( ManaRegen ), "Update" )]
        static class ManaRegen_Update_Mana_Patch {
            // Counter for number of frames - every X ticks add Y mana
            private static int tick = 0;
            // Increment counter and return amount of mana to regen
            private static int Tick() {
                tick = ( tick + 1 ) % WobPlugin.Settings.Get( "ManaRegenTicks", 1 );
                return tick == 0 ? WobPlugin.Settings.Get( "ManaRegenAdd", 1 ) : 0;
            }
            static void Prefix( ManaRegen __instance ) {
                // The default is to only use regen if the trait 'Crippling Intellect' is present, so check for it
                bool regenTrait = TraitManager.IsTraitActive( TraitType.BonusMagicStrength );
                // Continue if we are always enabling regen or the trait is present
                if( WobPlugin.Settings.Get( "ManaRegenEnabled", false ) || regenTrait ) {
                    // Get a reference to the private field where current player info is stored
                    PlayerController m_playerController = (PlayerController)Traverse.Create( __instance ).Field( "m_playerController" ).GetValue();
                    // Get the current mana state
                    float actualMaxMana = m_playerController.ActualMaxMana;
                    float currentMana = m_playerController.CurrentMana;
                    // Check that mana is missing, and whether to respect the delay after casing
                    if( currentMana < actualMaxMana && !( __instance.IsManaRegenDelayed && WobPlugin.Settings.Get( "ManaRegenDelay", true ) ) ) {
                        // Calculate how much mana to regenerate this frame
                        float regenRate = Tick();
                        if( regenRate > 0 ) {
                            // Check if this would take us over the maximum
                            if( regenRate + currentMana >= actualMaxMana ) {
                                // Set mana to maximum
                                m_playerController.SetMana( actualMaxMana, false, true );
                            } else {
                                // Add regen amount to current mana
                                m_playerController.SetMana( regenRate, true, true );
                            }
                        }
                    }
                }
            }
        }

        // Patch to the method that controls mana regen - just removing effects of original method
        [HarmonyPatch( typeof( ManaRegen ), "Update" )]
        static class ManaRegen_Update_Transpiler_Patch {
            static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "ManaRegen.Update Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldarg_0                           ), // this
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldfld, name: "m_playerController" ), // this.m_playerController
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc                           ), // local variable parameter
                            /*  3 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4_Bool                     ), // bool literal parameter
                            /*  4 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4_Bool                     ), // bool literal parameter
                            /*  5 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4_Bool                     ), // bool literal parameter
                            /*  6 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "SetMana"         ), // this.m_playerController.SetMana
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_Remove( 0, 7 ), // Blank out the found instructions with nop instructions
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }
    }
}