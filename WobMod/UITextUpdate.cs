using System.Collections.Generic;
using HarmonyLib;
using Wob_Common;

namespace WobMod {
    class UITextUpdate {

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - COMMON TEXT REPLACEMENT
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // This will do search & relpace, which will only work once - this variable makes that happen
        private static bool runOnce = false;

        // Method to perform a search & relpace within a string found in the dictionaries
        private static void ReplaceInString( List<Dictionary<string, string>> dictionaries, string locID, string oldText, string newText ) {
            // Loop through the dictionaries - there are male and female variant dictionaries
            foreach( Dictionary<string, string> dictionary in dictionaries ) {
                // Find the text to be edited based on the localisation ID
                if( dictionary.TryGetValue( locID, out string fullText ) ) {
                    // Replace the text in the string
                    dictionary[locID] = fullText.Replace( oldText, newText );
                }
            }
        }

        // Method to run through each of the replacements
        private static void UpdateText() {
            if( !runOnce ) {
                // Load the game's text dictionaries
                List<Dictionary<string, string>> dictionaries = new() {
                    Traverse.Create( LocalizationManager.Instance ).Field( "m_maleLocDict"   ).GetValue<Dictionary<string, string>>(),
                    Traverse.Create( LocalizationManager.Instance ).Field( "m_femaleLocDict" ).GetValue<Dictionary<string, string>>(),
                };
                // Traits
                { // Crippling Intellect
                    string hp = WobSettings.Get( Traits.traitKeys.Get( TraitType.BonusMagicStrength, "Health" ), -0.5f ).ToString( "+0.##%;-0.##%;-0%" );
                    string dmg = WobSettings.Get( Traits.traitKeys.Get( TraitType.BonusMagicStrength, "WeaponDamage" ), -0.5f ).ToString( "+0.##%;-0.##%;-0%" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION_BonusMagicStrength_1", "50%<color=purple> Health</color>, and <color=purple>Weapon Damage</color>", hp + " <color=purple>Health</color>, and " + dmg + " <color=purple>Weapon Damage</color>" );
                }
                { // Clownanthropy
                    string hp = WobSettings.Get( Traits.traitKeys.Get( TraitType.BounceTerrain, "Health" ), -0.3f ).ToString( "+0.##%;-0.##%;-0%" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION_BounceTerrain_1", "30% less", hp );
                }
                { // Pacifier
                    string hp = WobSettings.Get( Traits.traitKeys.Get( TraitType.CanNowAttack, "Health" ), -0.6f ).ToString( "+0.##%;-0.##%;-0%" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION_CanNowAttack_1", "60% less", hp );
                }
                { // Pacifist
                    string hp = WobSettings.Get( Traits.traitKeys.Get( TraitType.CantAttack, "Health" ), -0.6f ).ToString( "+0.##%;-0.##%;-0%" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION_CantAttack_1", "60% less", hp );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION2_CantAttack_1", "60% less", hp );
                }
                { // Combative
                    string hp = WobSettings.Get( Traits.traitKeys.Get( TraitType.DamageBoost, "Health" ), -0.25f ).ToString( "+0.##%;-0.##%;-0%" );
                    string dmg = WobSettings.Get( Traits.traitKeys.Get( TraitType.DamageBoost, "WeaponDamage" ), 0.5f ).ToString( "+0.##%;-0.##%;+0%" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION_DamageBoost_1", "-25%", hp );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION_DamageBoost_1", "+50%", dmg );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION2_DamageBoost_1", "-25%", hp );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION2_DamageBoost_1", "+50%", dmg );
                }
                { // Bookish
                    string hp = WobSettings.Get( Traits.traitKeys.Get( TraitType.MagicBoost, "Health" ), -0.25f ).ToString( "+0.##%;-0.##%;-0%" );
                    string mp = WobSettings.Get( Traits.traitKeys.Get( TraitType.MagicBoost, "MaxMana" ), 0.5f ).ToString( "+0.##%;-0.##%;+0%" );
                    string dmg = WobSettings.Get( Traits.traitKeys.Get( TraitType.MagicBoost, "MagicDamage" ), 0.5f ).ToString( "+0.##%;-0.##%;+0%" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION_MagicBoost_1", "-25% <color=purple>HP</color>", hp + " <color=purple>Health</color>" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION_MagicBoost_1", "<color=purple>+50% MP Capacity</color>", mp + " <color=purple>Mana Capacity</color>" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION_MagicBoost_1", "+50% <color=purple>Magic Damage</color>", dmg + " <color=purple>Magic Damage</color>" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION2_MagicBoost_1", "-25%", hp );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION2_MagicBoost_1", "MP Capacity", mp + " MP Capacity" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION2_MagicBoost_1", "+50%", dmg );
                }
                { // Emotional Dysregularity/Overcompensation
                    string dmg = WobSettings.Get( Traits.traitKeys.Get( TraitType.ManaCostAndDamageUp, "SpellDamage" ), 1f ).ToString( "+0.##%;-0.##%;+0%" );
                    string cost = WobSettings.Get( Traits.traitKeys.Get( TraitType.ManaCostAndDamageUp, "SpellCost" ), 1f ).ToString( "+0.##%;-0.##%;+0%" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION_ManaCostAndDamageUp_1", "Mana costs and Spell damage increased by 100%", dmg + " <color=purple>Spell Damage</color> and " + cost + " <color=purple>Mana Costs</color>" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION2_ManaCostAndDamageUp_1", "Mana costs and damage increased by 100%", dmg + " Spell Damage and " + cost + " Mana Costs" );
                }
                { // Masochism
                    string regen = WobSettings.Get( Traits.traitKeys.Get( TraitType.ManaFromHurt, "ManaRegen" ), 0.5f ).ToString( "+0.##%;-0.##%;+0%" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION_ManaFromHurt_1", "50%", regen );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION2_ManaFromHurt_1", "50%", regen );
                }
                { // Hero Complex
                    string hp = WobSettings.Get( Traits.traitKeys.Get( TraitType.MegaHealth, "Health" ), 1f ).ToString( "+0.##%;-0.##%;+0%" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION_MegaHealth_1", "<color=#0C8420>100%</color> more", "<color=#0C8420>" + hp + "</color>" );
                }
                { // Superfluid
                    string hp = WobSettings.Get( Traits.traitKeys.Get( TraitType.OmniDash, "Health" ), -0.2f ).ToString( "+0.##%;-0.##%;-0%" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION_OmniDash_1", "20% less", hp );
                }
                { // Spelunker
                    string hp = WobSettings.Get( Traits.traitKeys.Get( TraitType.RevealAllChests, "Health" ), -0.1f ).ToString( "+0.##%;-0.##%;-0%" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION_RevealAllChests_1", "-10% HP", hp + " <color=purple>Health</color>" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION2_RevealAllChests_1", "-10%", hp );
                }
                { // Disattuned/Only Heart
                    string hp = WobSettings.Get( Traits.traitKeys.Get( TraitType.SmallHitbox, "Health" ), -0.25f ).ToString( "+0.##%;-0.##%;-0%" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION_SmallHitbox_1", "-25%", hp );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION2_SmallHitbox_1", "-25%", hp );
                }
                { // Vampirism
                    string regen = WobSettings.Get( Traits.traitKeys.Get( TraitType.Vampire, "DamageRegen" ), 0.2f ).ToString( "+0.##%;-0.##%;+0%" );
                    string taken = WobSettings.Get( Traits.traitKeys.Get( TraitType.Vampire, "DamageTaken" ), 1.25f ).ToString( "+0.##%;-0.##%;+0%" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION_Vampire_1", "20%", regen );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION_Vampire_1", "125%</color> more", taken + "</color>" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION2_Vampire_1", "20%", regen );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION2_Vampire_1", "125% more", taken );
                }
                { // Limitless
                    string taken = WobSettings.Get( Traits.traitKeys.Get( TraitType.NoManaCap, "DamageTaken" ), 0.5f ).ToString( "+0.##%;-0.##%;+0%" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION_NoManaCap_1", "<b>+50%</b></color> more", taken + "</color>" );
                    ReplaceInString( dictionaries, "LOC_ID_TRAIT_DESCRIPTION2_NoManaCap_1", "<b>+50%</b></color> more", taken + "</color> damage" );
                }
                // Relics
                { // Icarus' Wings Bargain
                    string taken = WobSettings.Get( Relics.relicKeys.Get( RelicType.FlightBonusCurse, "DamageTaken" ), 0.75f ).ToString( "+0.##%;-0.##%;+0%" );
                    ReplaceInString( dictionaries, "LOC_ID_RELIC_DESCRIPTION_FlightBonusCurse_1", "{0}%", taken );
                }
                // All done - set the variable to prevent a repeat
                runOnce = true;
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - GAME TEXT LOOKUPS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // These patches are all for methods that look up the UI text from the localization ID, so we can edit the text just before it is displayed

        [HarmonyPatch( typeof( LineageDescriptionUpdater ), "UpdateText" )]
        internal static class LineageDescriptionUpdater_UpdateText_Patch {
            internal static void Prefix() { UpdateText(); }
        }

        [HarmonyPatch( typeof( PlayerCardRightPageEntry ), "UpdateCard" )]
        internal static class PlayerCardRightPageEntry_UpdateCard_Patch {
            internal static void Prefix() { UpdateText(); }
        }

        [HarmonyPatch( typeof( ObjectiveCompleteHUDController ), "UpdateObjectiveCompleteText" )]
        internal static class ObjectiveCompleteHUDController_UpdateObjectiveCompleteText_Patch {
            internal static void Prefix() { UpdateText(); }
        }

        [HarmonyPatch( typeof( RelicRoomPropController ), nameof( RelicRoomPropController.InitializeTextBox ) )]
        internal static class RelicRoomPropController_InitializeTextBox_Patch {
            internal static void Prefix() { UpdateText(); }
        }

    }
}
