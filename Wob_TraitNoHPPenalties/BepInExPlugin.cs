using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_TraitHPPenalties {
    [BepInPlugin( "Wob.TraitHPPenalties", "Super Magnet Mod", "0.1.0" )]
    public partial class BepInExPlugin : BaseUnityPlugin {
		// Configuration file entries, globally accessible for patches
		public static Dictionary<TraitType, IScaledConfigItem> configTraits;


		// Main method that kicks everything off
		private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configTraits = new Dictionary<TraitType, IScaledConfigItem> {
				{ TraitType.MegaHealth,         new ScaledConfigItemI( this.Config, "Traits", "MegaHealth",         "Hero Complex - 100% more Health but you can't heal, ever.",                                        100, 0, int.MaxValue, 0.01f ) },

				{ TraitType.BonusMagicStrength, new ScaledConfigItemI( this.Config, "Traits", "BonusMagicStrength", "Crippling Intellect - -50% Health and Weapon Damage. Mana regenerates over time.",                 50,  0, 99,           0.01f ) },
                { TraitType.BounceTerrain,      new ScaledConfigItemI( this.Config, "Traits", "BounceTerrain",      "Clownanthropy - 30% less Health, but you can Spin Kick off terrain.",                              30,  0, 99,           0.01f ) },
                { TraitType.CanNowAttack,       new ScaledConfigItemI( this.Config, "Traits", "CanNowAttack",       "Pacifier - -60% Health and you love to fight!",                                                    60,  0, 99,           0.01f ) },
                { TraitType.CantAttack,         new ScaledConfigItemI( this.Config, "Traits", "CantAttack",         "Pacifist - -60% Health and you can't deal damage.",                                                60,  0, 99,           0.01f ) },
                { TraitType.DamageBoost,        new ScaledConfigItemI( this.Config, "Traits", "DamageBoost",        "Combative - +50% Weapon Damage, -30% Health.",                                                     30,  0, 99,           0.01f ) },
				{ TraitType.InvulnDash,         new ScaledConfigItemI( this.Config, "Traits", "InvulnDash",         "Evasive - Invincible while dashing, but you have 50% less hp, and dashing dodges has a cooldown.", 50,  0, 99,           0.01f ) },
				{ TraitType.MagicBoost,         new ScaledConfigItemI( this.Config, "Traits", "MagicBoost",         "Bookish - +50% Magic Damage and +50 Mana Capacity. -30% HP.",                                      30,  0, 99,           0.01f ) },
                { TraitType.OmniDash,           new ScaledConfigItemI( this.Config, "Traits", "OmniDash",           "Superfluid - -20% Health, but you can dash in ANY direction.",                                     20,  0, 99,           0.01f ) },
                { TraitType.RevealAllChests,    new ScaledConfigItemI( this.Config, "Traits", "RevealAllChests",    "Spelunker - -10% HP but you can see all chests on the map!",                                       10,  0, 99,           0.01f ) },
                { TraitType.SmallHitbox,        new ScaledConfigItemI( this.Config, "Traits", "SmallHitbox",        "Disattuned/Only Heart - 25% less health, but you can only be hit in the heart.",                   25,  0, 99,           0.01f ) },
            };
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

		static float GetConfigValue( TraitType traitType, float defaultValue ) {
			IScaledConfigItem configItem;
            return configTraits.TryGetValue( traitType, out configItem ) ? configItem.ScaledValue : defaultValue;
        }

        // Patch for the method that gets the gold cost for a specific upgrade with labour costs included
        [HarmonyPatch( typeof( PlayerController ), "InitializeTraitHealthMods" )]
        static class PlayerController_InitializeTraitHealthMods_Patch {
            static void Postfix( PlayerController __instance ) {
				float num = 0f;
				if( TraitManager.IsTraitActive( TraitType.BonusHealth ) ) {
					num += GetConfigValue( TraitType.BonusHealth, 0.1f );
				}
				if( TraitManager.IsTraitActive( TraitType.InvulnDash ) ) {
					num -= GetConfigValue( TraitType.InvulnDash, 0.5f );
				}
				if( TraitManager.IsTraitActive( TraitType.MagicBoost ) ) {
					num -= GetConfigValue( TraitType.MagicBoost, 0.3f );
				}
				if( TraitManager.IsTraitActive( TraitType.DamageBoost ) ) {
					num -= GetConfigValue( TraitType.DamageBoost, 0.3f );
				}
				if( TraitManager.IsTraitActive( TraitType.CantAttack ) ) {
					num -= GetConfigValue( TraitType.CantAttack, 0.6f );
				}
				if( TraitManager.IsTraitActive( TraitType.CanNowAttack ) ) {
					num -= GetConfigValue( TraitType.CanNowAttack, 0.6f );
				}
				if( TraitManager.IsTraitActive( TraitType.SmallHitbox ) ) {
					num -= GetConfigValue( TraitType.SmallHitbox, 0.25f );
				}
				if( TraitManager.IsTraitActive( TraitType.BonusMagicStrength ) ) {
					num -= GetConfigValue( TraitType.BonusMagicStrength, 0.5f );
				}
				if( TraitManager.IsTraitActive( TraitType.RevealAllChests ) ) {
					num -= GetConfigValue( TraitType.RevealAllChests, 0.1f );
				}
				if( TraitManager.IsTraitActive( TraitType.SuperHealer ) ) {
					num -= GetConfigValue( TraitType.SuperHealer, 0f );
				}
				if( TraitManager.IsTraitActive( TraitType.OmniDash ) ) {
					num -= GetConfigValue( TraitType.OmniDash, 0.2f );
				}
				if( TraitManager.IsTraitActive( TraitType.BounceTerrain ) ) {
					num -= GetConfigValue( TraitType.BounceTerrain, 0.3f );
				}
				if( TraitManager.IsTraitActive( TraitType.MegaHealth ) ) {
					num += GetConfigValue( TraitType.MegaHealth, 1f );
				}
				__instance.TraitMaxHealthMod = num;
			}
        }
    }
}
