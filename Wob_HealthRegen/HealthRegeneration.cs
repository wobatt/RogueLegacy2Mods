using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_HealthRegen {
    [BepInPlugin( "Wob.HealthRegen", "Health Regen Mod", "0.1.0" )]
    public partial class HealthRegeneration : BaseUnityPlugin {
        // Configuration file entries, globally accessible for patches
        public static ConfigItemBool configEnableRegen;
        public static ConfigItem<int> configRegenAdd;
        public static ConfigItem<int> configRegenTicks;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configEnableRegen = new ConfigItemBool( this.Config, "Options", "EnableRegen", "Enable health regeneration", true );
            configRegenTicks = new ConfigItem<int>( this.Config, "Options", "RegenTicks", "Number of ticks/frames between adding health - higher number means slower regen", 30, 1, 1000 );
            configRegenAdd = new ConfigItem<int>( this.Config, "Options", "RegenAdd", "When health is added, this is the amount to add", 1, 1, 10000 );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Patch to the method that controls mana regen
        [HarmonyPatch( typeof( ManaRegen ), "Update" )]
        static class ManaRegen_Update_Patch {
            // Counter for number of frames - every X ticks add Y health
            private static int tick = 0;
            // Increment counter and return amount of health to regen
            private static int Tick() {
                tick = ( tick + 1 ) % configRegenTicks.Value;
                return tick == 0 ? configRegenAdd.Value : 0;
            }

            static void Prefix( ManaRegen __instance ) {
                // Continue if we are enabling regen
                if( configEnableRegen.Value ) {
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
    }
}