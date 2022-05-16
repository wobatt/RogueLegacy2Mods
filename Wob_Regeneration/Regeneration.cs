using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_Regeneration {
    [BepInPlugin( "Wob.Regeneration", "Regeneration Mod", "0.3.0" )]
    public partial class Regeneration : BaseUnityPlugin {
        // Configuration file entries, globally accessible for patches
        public static ConfigItemBool configHealthRegenEnabled;
        public static ConfigItem<int> configHealthRegenTicks;
        public static ConfigItem<int> configHealthRegenAdd;

        public static ConfigItemBool configManaRegenEnabled;
        public static ConfigItem<int> configManaRegenTicks;
        public static ConfigItem<int> configManaRegenAdd;
        public static ConfigItemBool configManaRegenDelay;
        public static ConfigItem<int> configManaMax;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configHealthRegenEnabled = new ConfigItemBool( this.Config, "Options", "HealthRegenEnabled", "Enable mana regeneration", true );
            configHealthRegenTicks = new ConfigItem<int>( this.Config, "Options", "HealthRegenTicks", "Number of ticks/frames between adding health - higher number means slower regen", 30, 1, 1000 );
            configHealthRegenAdd = new ConfigItem<int>( this.Config, "Options", "HealthRegenAdd", "When health is added, this is the amount to add", 1, 1, 10000 );

            configManaRegenEnabled = new ConfigItemBool( this.Config, "Options", "ManaRegenEnabled", "Enable mana regeneration", true );
            configManaRegenTicks = new ConfigItem<int>( this.Config, "Options", "ManaRegenTicks", "Number of ticks/frames between adding mana - higher number means slower regen", 1, 1, 1000 );
            configManaRegenAdd = new ConfigItem<int>( this.Config, "Options", "ManaRegenAdd", "When mana is added, this is the amount to add", 1, 1, 10000 );
            configManaRegenDelay = new ConfigItemBool( this.Config, "Options", "ManaRegenDelay", "Enable the 2 second delay to mana regeneration after casting a spell", true );
            configManaMax = new ConfigItem<int>( this.Config, "Options", "MaxMana", "Additional max mana", 0, 0, 10000 );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Patch to add extra max mana - this method gets the max mana added by runes, just add a flat amount to its return value
        [HarmonyPatch( typeof( RuneLogicHelper ), nameof( RuneLogicHelper.GetMaxManaFlat ) )]
        static class RuneLogicHelper_GetMaxManaFlat_Patch {
            static void Postfix( ref int __result ) {
                __result += configManaMax.Value;
            }
        }

        // Patch to the method that controls mana regen
        [HarmonyPatch( typeof( ManaRegen ), "Update" )]
        static class ManaRegen_Update_Health_Patch {
            // Counter for number of frames - every X ticks add Y health
            private static int tick = 0;
            // Increment counter and return amount of health to regen
            private static int Tick() {
                tick = ( tick + 1 ) % configHealthRegenTicks.Value;
                return tick == 0 ? configHealthRegenAdd.Value : 0;
            }
            static void Prefix( ManaRegen __instance ) {
                // Continue if we are enabling regen
                if( configHealthRegenEnabled.Value ) {
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
                tick = ( tick + 1 ) % configManaRegenTicks.Value;
                return tick == 0 ? configManaRegenAdd.Value : 0;
            }
            static void Prefix( ManaRegen __instance ) {
                // The default is to only use regen if the trait 'Crippling Intellect' is present, so check for it
                bool regenTrait = TraitManager.IsTraitActive( TraitType.BonusMagicStrength );
                // Continue if we are always enabling regen or the trait is present
                if( configManaRegenEnabled.Value || regenTrait ) {
                    // Get a reference to the private field where current player info is stored
                    PlayerController m_playerController = (PlayerController)Traverse.Create( __instance ).Field( "m_playerController" ).GetValue();
                    // Get the current mana state
                    float actualMaxMana = m_playerController.ActualMaxMana;
                    float currentMana = m_playerController.CurrentMana;
                    // Check that mana is missing, and whether to respect the delay after casing
                    if( currentMana < actualMaxMana && !( __instance.IsManaRegenDelayed && configManaRegenDelay.Value ) ) {
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
                // Set up the transpiler handler's parameters
                WobTranspiler transpiler = new WobTranspiler( instructions,
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTestLine> {
                            /*  0 */ new WobTranspiler.OpTestLine( OpCodes.Ldarg_0                           ), // this
                            /*  1 */ new WobTranspiler.OpTestLine( OpCodes.Ldfld, name: "m_playerController" ), // this.m_playerController
                            /*  2 */ new WobTranspiler.OpTestLine( OpCodeSet.LdLoc                           ), // local variable parameter
                            /*  3 */ new WobTranspiler.OpTestLine( OpCodeSet.Ldc_I4_Bool                     ), // bool literal parameter
                            /*  4 */ new WobTranspiler.OpTestLine( OpCodeSet.Ldc_I4_Bool                     ), // bool literal parameter
                            /*  5 */ new WobTranspiler.OpTestLine( OpCodeSet.Ldc_I4_Bool                     ), // bool literal parameter
                            /*  6 */ new WobTranspiler.OpTestLine( OpCodes.Callvirt, name: "SetMana"         ), // this.m_playerController.SetMana
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_Remove( 0, 7 ), // Blank out the found instructions with nop instructions
                        } );
                // Perform the patching and return the modified instructions
                return transpiler.PatchAll();
            }
        }
    }
}