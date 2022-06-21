using BepInEx;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace Wob_Regeneration {
    [BepInPlugin( "Wob.Regeneration", "Regeneration Mod", "1.1.0" )]
    [BepInIncompatibility( "Wob.ManaRegen" )]
    public partial class Regeneration : BaseUnityPlugin {
        internal enum RegenStat { NoRegen, MaxHealth, MaxMana, Vitality, Strength, Dexterity, Intelligence, Focus, Constant100 }

        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobSettings.Add( new WobSettings.Entry[] {
                // Health related
                new WobSettings.Enum<RegenStat>( "HealthRegenStat",  "Use this character stat to calculate health regeneration rate", RegenStat.MaxHealth               ),
                new WobSettings.Num<float>(      "HealthRegenScale", "Regenerate this percent of the health regen stat per second",   0.5f, 0.01f, bounds: (0f, 10000f) ),
                // Mana related
                new WobSettings.Enum<RegenStat>( "ManaRegenStat",    "Use this character stat to calculate mana regeneration rate",   RegenStat.MaxMana                 ),
                new WobSettings.Num<float>(      "ManaRegenScale",   "Regenerate this percent of the mana regen stat per second",     1f,   0.01f, bounds: (0f, 10000f) ),
                new WobSettings.Boolean(         "ManaRegenDelay",   "Enable the 2 second delay to mana regen after casting a spell", true                              ),
            } );
            ManaRegen_Update_Patch.healthRegenStat  = WobSettings.Get( "HealthRegenStat",  RegenStat.NoRegen );
            ManaRegen_Update_Patch.healthRegenScale = WobSettings.Get( "HealthRegenScale", 0f                );
            ManaRegen_Update_Patch.manaRegenStat    = WobSettings.Get( "ManaRegenStat",    RegenStat.NoRegen );
            ManaRegen_Update_Patch.manaRegenScale   = WobSettings.Get( "ManaRegenScale",   0f                );
            ManaRegen_Update_Patch.manaRegenDelay   = WobSettings.Get( "ManaRegenDelay",   true              );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Patch to the method that controls mana regen
        [HarmonyPatch( typeof( ManaRegen ), "Update" )]
        internal static class ManaRegen_Update_Patch {
            // Health regen settings
            internal static RegenStat healthRegenStat;
            internal static float healthRegenScale;
            // Health regen fractional running total
            private static float healthRegenTotal = 0f;
            // Mana regen settings
            internal static RegenStat manaRegenStat;
            internal static float manaRegenScale;
            internal static bool manaRegenDelay;
            // Mana regen fractional running total
            private static float manaRegenTotal = 0f;

            internal static void Prefix( ManaRegen __instance ) {
                // Get a reference to the private field where current player info is stored
                PlayerController m_playerController = Traverse.Create( __instance ).Field( "m_playerController" ).GetValue<PlayerController>();
                // Continue if we are enabling health regen
                if( healthRegenScale > 0f ) {
                    // Get the current health state
                    float actualMaxHealth = m_playerController.ActualMaxHealth;
                    float currentHealth = m_playerController.CurrentHealth;
                    // Check that health is missing
                    if( currentHealth < actualMaxHealth ) {
                        // Add current frame's regen to the running total
                        healthRegenTotal += GetRegenStatValue( m_playerController, healthRegenStat ) * healthRegenScale * Time.deltaTime;
                        // After the total is over 1 do the actual regeneration
                        if( healthRegenTotal > 1f ) {
                            // The regen to add is the integer portion of the total
                            float regenNow = Mathf.FloorToInt( healthRegenTotal );
                            // Subtract the regen from the running total so it won't be added twice
                            healthRegenTotal -= regenNow;
                            // Check if the regen will take the current health over the maximum
                            if( regenNow + currentHealth >= actualMaxHealth ) {
                                // Set health to maximum
                                m_playerController.SetHealth( actualMaxHealth, false, true );
                            } else {
                                // Add regen amount to current health
                                m_playerController.SetHealth( regenNow, true, true );
                            }
                        }
                    }
                }
                // Continue if we are enabling mana regen
                if( manaRegenScale > 0f ) {
                    // Get the current mana state
                    float actualMaxMana = m_playerController.ActualMaxMana;
                    float currentMana = m_playerController.CurrentMana;
                    // Check that mana is missing
                    if( currentMana < actualMaxMana && !( manaRegenDelay && __instance.IsManaRegenDelayed ) ) {
                        // Add current frame's regen to the running total
                        manaRegenTotal += GetRegenStatValue( m_playerController, manaRegenStat ) * manaRegenScale * Time.deltaTime;
                        // After the total is over 1 do the actual regeneration
                        if( manaRegenTotal > 1f ) {
                            // The regen to add is the integer portion of the total
                            float regenNow = Mathf.FloorToInt( manaRegenTotal );
                            // Subtract the regen from the running total so it won't be added twice
                            manaRegenTotal -= regenNow;
                            // Check if the regen will take the current mana over the maximum
                            if( regenNow + currentMana >= actualMaxMana ) {
                                // Set mana to maximum
                                m_playerController.SetMana( actualMaxMana, false, true );
                            } else {
                                // Add regen amount to current mana
                                m_playerController.SetMana( regenNow, true, true );
                            }
                        }
                    }
                }
            }

            private static float GetRegenStatValue( PlayerController player, RegenStat regenStat ) {
                switch( regenStat ) {
                    case RegenStat.MaxHealth:
                        return player.ActualMaxHealth;
                    case RegenStat.MaxMana:
                        return player.ActualMaxMana;
                    case RegenStat.Vitality:
                        return player.ActualVitality;
                    case RegenStat.Strength:
                        return player.ActualStrength;
                    case RegenStat.Dexterity:
                        return player.ActualDexterity;
                    case RegenStat.Intelligence:
                        return player.ActualMagic;
                    case RegenStat.Focus:
                        return player.ActualMaxHealth;
                    case RegenStat.Constant100:
                        return 100f;
                    default:
                        return 0f;
                }
            }
        }
    }
}