using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_TalentCooldown {
    [BepInPlugin( "Wob.TalentCooldown", "Talent Cooldown Mod", "0.1.0" )]
    public partial class TalentCooldown : BaseUnityPlugin {
        // Configuration file entries, globally accessible for patches
        public static ConfigItemBool configZeroCooldown;
        public static ConfigItem<int> configMaxAmmo;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configZeroCooldown = new ConfigItemBool( this.Config, "Options", "ZeroCooldown", "When 'true' talents have no cooldown or any need to wait until finished; when 'false' they have 1/100 of a second cooldown", false );
            configMaxAmmo = new ConfigItem<int>( this.Config, "Options", "MaxAmmo", "Maximum number of charges for Stew (Chef) talent; set to 0 for infinite charges", 99, 0, 99 );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        [HarmonyPatch( typeof( AbilityLibrary ), nameof( AbilityLibrary.Initialize ) )]
        static class AbilityLibrary_Initialize_Patch {
            static void Postfix() {
                foreach( AbilityType talentType in AbilityType_RL.TalentAbilityArray ) {
                    BaseAbility_RL talent = AbilityLibrary.GetAbility( talentType );
                    if( talent != null ) {
                        AbilityData talentData = talent.AbilityData;
                        if( talentData != null ) {
                            if( talentType == AbilityType.CookingTalent ) {
                                WobPlugin.Log( talentType + " (" + LocalizationManager.GetString( talentData.Title, false ) + "): MaxAmmo " + talentData.MaxAmmo + " -> " + configMaxAmmo.Value );
                                talentData.MaxAmmo = configMaxAmmo.Value;
                            } else {
                                if( configZeroCooldown.Value ) {
                                    WobPlugin.Log( talentType + " (" + LocalizationManager.GetString( talentData.Title, false ) + "): Cooldown " + talentData.CooldownTime + " -> 0" + ( talentData.MaxAmmo == 0 ? "" : ", MaxAmmo " + talentData.MaxAmmo + " -> 0" ) );
                                    talentData.CooldownTime = 0;
                                    talentData.MaxAmmo = 0;
                                } else {
                                    WobPlugin.Log( talentType + " (" + LocalizationManager.GetString( talentData.Title, false ) + "): Cooldown " + talentData.CooldownTime + " -> 0.01 seconds" + ( talentData.MaxAmmo == 0 ? "" : ", MaxAmmo " + talentData.MaxAmmo + " -> 0" ) );
                                    talentData.CooldownTime = 0.01f;
                                    talentData.CooldownDecreaseOverTime = true;
                                    talentData.CooldownDecreasePerHit = false;
                                    talentData.MaxAmmo = 0;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}