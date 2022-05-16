using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_ManaRegen {
    [BepInPlugin( "Wob.ManaRegen", "Mana Regen Mod", "0.2.0" )]
    public partial class ManaRegeneration : BaseUnityPlugin {
        // Configuration file entries, globally accessible for patches
        public static ConfigItemBool configEnableRegen;
        public static ConfigItemBool configRegenDelay;
        public static ConfigItem<int> configManaMax;
        public static ConfigItem<int> configRegenAdd;
        public static ConfigItem<int> configRegenTicks;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configEnableRegen = new ConfigItemBool( this.Config, "Options", "EnableRegen", "Enable mana regeneration", true );
            configRegenDelay = new ConfigItemBool( this.Config, "Options", "RegenDelay", "Enable the 2 second delay to mana regeneration after casting a spell", true );
            configManaMax = new ConfigItem<int>( this.Config, "Options", "MaxMana", "Additional max mana", 0, 0, 10000 );
            configRegenTicks = new ConfigItem<int>( this.Config, "Options", "RegenTicks", "Number of ticks/frames between adding mana - higher number means slower regen", 1, 1, 1000 );
            configRegenAdd = new ConfigItem<int>( this.Config, "Options", "RegenAdd", "When mana is added, this is the amount to add", 1, 1, 10000 );
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
        static class ManaRegen_Update_Patch {
            // Counter for number of frames - every X ticks add Y mana
            private static int tick = 0;
            // Increment counter and return amount of mana to regen
            private static int Tick() {
                tick = ( tick + 1 ) % configRegenTicks.Value;
                return tick == 0 ? configRegenAdd.Value : 0;
            }

            static void Prefix( ManaRegen __instance ) {
                // The default is to only use regen if the trait 'Crippling Intellect' is present, so check for it
                bool regenTrait = TraitManager.IsTraitActive( TraitType.BonusMagicStrength );
                // Continue if we are always enabling regen or the trait is present
                if( configEnableRegen.Value || regenTrait ) {
                    // Get a reference to the private field where current player info is stored
                    PlayerController m_playerController = (PlayerController)Traverse.Create( __instance ).Field( "m_playerController" ).GetValue();
                    // Get the current mana state
                    float actualMaxMana = m_playerController.ActualMaxMana;
                    float currentMana = m_playerController.CurrentMana;
                    // Check that mana is missing, and whether to respect the delay after casing
                    if( currentMana < actualMaxMana && !( __instance.IsManaRegenDelayed && configRegenDelay.Value ) ) {
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