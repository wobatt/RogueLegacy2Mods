using BepInEx;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace Wob_ManaRegen {
    [BepInPlugin( "Wob.ManaRegen", "Mana Regen Mod", "0.1.0" )]
    public partial class ManaRegen : BaseUnityPlugin {
        // Configuration file entries, globally accessible for patches
        public static ConfigItemBool configManaRegen;
        public static ConfigItemBool configRegenDelay;
        public static ConfigItem<int> configManaMax;
        public static ScaledConfigItemF configRegenRate;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configManaRegen = new ConfigItemBool( this.Config, "Options", "EnableRegen", "Enable mana regeneration", true );
            configRegenDelay = new ConfigItemBool( this.Config, "Options", "RegenDelay", "Enable the 2 second delay to mana regeneration after casting a spell", true );
            configManaMax = new ConfigItem<int>( this.Config, "Options", "MaxMana", "Additional max mana - regeneration rate scales with max mana", 0, 0, 10000 );
            configRegenRate = new ScaledConfigItemF( this.Config, "Options", "RegenRate", "Percent of max mana to regenerate per second (rounded up to integer value)", 75f, 0, 1000f, 0.01f );
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
        [HarmonyPatch( typeof( global::ManaRegen ), "Update" )]
        static class ManaRegen_Update_Patch {
            static bool Prefix( global::ManaRegen __instance ) {
                // The default is to only use regen if the trait 'Crippling Intellect' is present, so check for it
                bool regenTrait = TraitManager.IsTraitActive( TraitType.BonusMagicStrength );
                // Continue if we are always enabling regen or the trait is present
                if( configManaRegen.Value || regenTrait ) {
                    // Get a reference to the private field where current player info is stored
                    PlayerController m_playerController = (PlayerController)Traverse.Create( __instance ).Field( "m_playerController" ).GetValue();
                    // Get the current mana state
                    float actualMaxMana = m_playerController.ActualMaxMana;
                    float currentMana = m_playerController.CurrentMana;
                    // Check that mana is missing, and whether to respect the delay after casing
                    if( currentMana < actualMaxMana && !( __instance.IsManaRegenDelayed && configRegenDelay.Value ) ) {
                        // Calculate how much mana to regenerate this frame
                        float regenRate = Mathf.CeilToInt( configRegenRate.ScaledValue * actualMaxMana * Time.deltaTime );
                        // Check if this would take us over the maximum
                        if( regenRate + currentMana > actualMaxMana ) {
                            // Set mana to maximum
                            m_playerController.SetMana( actualMaxMana, false, true );
                        } else {
                            // Add regen amount to current mana
                            m_playerController.SetMana( regenRate, true, true );
                        }
                    }
                }
                return false;
            }
        }
    }
}