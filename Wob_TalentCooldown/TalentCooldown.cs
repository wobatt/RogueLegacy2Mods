using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_TalentCooldown {
    [BepInPlugin( "Wob.TalentCooldown", "Talent Cooldown Mod", "1.0.0" )]
    public partial class TalentCooldown : BaseUnityPlugin {
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobPlugin.Settings.Add( new WobSettings.Entry[] {
                new WobSettings.EntryBool( "ZeroCooldown", "When 'true' talents have no cooldown or any need to wait until finished; when 'false' they have 1/100 of a second cooldown", false ),
                new WobSettings.Entry<int>( "MaxAmmo", "Maximum number of charges for Stew (Chef) talent; set to 0 for infinite charges", 99, bounds: (0, 99) ),
            } );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        [HarmonyPatch( typeof( AbilityLibrary ), nameof( AbilityLibrary.Initialize ) )]
        internal static class AbilityLibrary_Initialize_Patch {
            internal static void Postfix() {
                foreach( AbilityType talentType in AbilityType_RL.TalentAbilityArray ) {
                    BaseAbility_RL talent = AbilityLibrary.GetAbility( talentType );
                    if( talent != null ) {
                        AbilityData talentData = talent.AbilityData;
                        if( talentData != null ) {
                            if( talentType == AbilityType.CookingTalent ) {
                                WobPlugin.Log( talentType + " (" + LocalizationManager.GetString( talentData.Title, false ) + "): MaxAmmo " + talentData.MaxAmmo + " -> " + WobPlugin.Settings.Get( "MaxAmmo", 3 ) );
                                talentData.MaxAmmo = WobPlugin.Settings.Get( "MaxAmmo", 3 );
                            } else {
                                if( WobPlugin.Settings.Get( "ZeroCooldown", false ) ) {
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