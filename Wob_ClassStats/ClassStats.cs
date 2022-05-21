using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_ClassStats {
    [BepInPlugin( "Wob.ClassStats", "Class Stats Mod", "1.0.0" )]
    public partial class ClassStats : BaseUnityPlugin {
        private static class ClassSettings {
            // Names and descriptions used in the config file, repeated per class
            private static readonly (string name, string desc)[] stats = {
                ( "MaxHPMod",            "Max health modifier for "         ),
                ( "MaxManaMod",          "Max mana modifier for "           ),
                ( "ArmorMod",            "Armor modifier for "              ),
                ( "VitalityMod",         "Vitality modifier for "           ),
                ( "StrengthMod",         "Strength modifier for "           ),
                ( "IntelligenceMod",     "Intelligence modifier for "       ),
                ( "DexterityMod",        "Dexterity modifier for "          ),
                ( "FocusMod",            "Focus modifier for "              ),
                ( "WeaponCritChanceAdd", "Weapon crit chance modifier for " ),
                ( "MagicCritChanceAdd",  "Magic crit chance modifier for "  ),
                ( "WeaponCritDamageAdd", "Weapon crit damage modifier for " ),
                ( "MagicCritDamageAdd",  "Magic crit damage modifier for "  ),
            };

            // Translation from internal class name to user-friendly name used in config
            private static readonly Dictionary<string, string> classSettingNames = new Dictionary<string, string>();

            // Create all stat settings for a class at once
            public static void Create( string settingName, string className, float[] statDefaults ) {
                // Make sure the right number of stats have been provided
                if( statDefaults.Length != stats.Length ) {
                    WobPlugin.Log( "ERROR: Wrong number of stats for " + settingName, WobPlugin.ERROR );
                    return;
                }
                // Add the name translation to the dictionary
                classSettingNames.Add( className, settingName );
                // Prefix the UI name with Class so that the sections in the config file per class are grouped together, and all after the mod's basic settings
                string sectionName = "Class_" + settingName;
                // Go through each stat
                for( int i = 0; i < stats.Length; i++ ) {
                    // Create a setting for the stat
                    WobPlugin.Settings.Add( new WobSettings.Scaled<float>( sectionName, settingName + "_" + stats[i].name, stats[i].desc + className, statDefaults[i], 0.01f, bounds: (-99f, 1000000f) ) );
                }
            }

            // Get the value of a stat setting from the class name
            public static float Get( string className, string modifierName, float defaultValue ) {
                // Get the user-friendly name of the setting from the internal class name
                if( classSettingNames.TryGetValue( className, out string settingName ) ) {
                    // Get the value of the setting
                    return WobPlugin.Settings.Get( "Class_" + settingName, settingName + "_" + modifierName, defaultValue );
                } else {
                    // No translation found, so just return the default value with no changes
                    return defaultValue;
                }
            }
        }

        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create the settings in bulk                                           HP   MP    Arm  Vit  Str  Int  Dex  Foc  WCC  MCC  WCD  MCD
            ClassSettings.Create( "Assassin",     "DualBladesClass",  new float[] { -30f, 25f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            ClassSettings.Create( "Astromancer",  "AstroClass",       new float[] { -40f, 100f, 0f,  0f,  0f,  20f, 0f,  0f,  0f,  0f,  0f,  0f  } );
            ClassSettings.Create( "Barbarian",    "AxeClass",         new float[] {  0f,  25f,  0f,  20f, 0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            ClassSettings.Create( "Bard",         "LuteClass",        new float[] { -15f, 50f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            ClassSettings.Create( "Boxer",        "BoxingGloveClass", new float[] {  0f,  25f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            ClassSettings.Create( "Chef",         "LadleClass",       new float[] { -30f, 100f, 0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            ClassSettings.Create( "DragonLancer", "LanceClass",       new float[] {  0f,  25f,  20f, 0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            ClassSettings.Create( "Duelist",      "SaberClass",       new float[] { -15f, 25f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            ClassSettings.Create( "Gunslinger",   "GunClass",         new float[] { -30f, 50f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            ClassSettings.Create( "Knight",       "SwordClass",       new float[] {  0f,  50f,  0f,  0f,  0f,  0f,  0f,  0f,  5f,  0f,  0f,  0f  } );
            ClassSettings.Create( "Mage",         "MagicWandClass",   new float[] { -30f, 100f, 0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            ClassSettings.Create( "Pirate",       "CannonClass",      new float[] {  0f,  25f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  10f, 10f } );
            ClassSettings.Create( "Ranger",       "BowClass",         new float[] { -15f, 25f,  0f,  0f,  10f, 0f,  10f, 0f,  0f,  0f,  0f,  0f  } );
            ClassSettings.Create( "Ronin",        "KatanaClass",      new float[] { -40f, 50f,  0f,  0f,  20f, 0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            ClassSettings.Create( "Valkyrie",     "SpearClass",       new float[] { -15f, 50f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  5f,  0f,  10f } );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Patch for class passive stat data
        [HarmonyPatch( typeof( ClassData ), nameof( ClassData.PassiveData ), MethodType.Getter )]
        internal static class ClassData_PassiveData_Patch {
            internal static void Postfix( ref ClassPassiveData __result ) {
                // Set each of the properties on the passive data set to the value in config - value +1 for multipliers, without for additions
                __result.MaxHPMod            = ClassSettings.Get( __result.ClassName, "MaxHPMod",            __result.MaxHPMod            ) + 1f;
                __result.MaxManaMod          = ClassSettings.Get( __result.ClassName, "MaxManaMod",          __result.MaxManaMod          ) + 1f;
                __result.ArmorMod            = ClassSettings.Get( __result.ClassName, "ArmorMod",            __result.ArmorMod            ) + 1f;
                __result.VitalityMod         = ClassSettings.Get( __result.ClassName, "VitalityMod",         __result.VitalityMod         ) + 1f;
                __result.StrengthMod         = ClassSettings.Get( __result.ClassName, "StrengthMod",         __result.StrengthMod         ) + 1f;
                __result.IntelligenceMod     = ClassSettings.Get( __result.ClassName, "IntelligenceMod",     __result.IntelligenceMod     ) + 1f;
                __result.DexterityMod        = ClassSettings.Get( __result.ClassName, "MaxManaMod",          __result.DexterityMod        ) + 1f;
                __result.FocusMod            = ClassSettings.Get( __result.ClassName, "FocusMod",            __result.FocusMod            ) + 1f;
                __result.WeaponCritChanceAdd = ClassSettings.Get( __result.ClassName, "WeaponCritChanceAdd", __result.WeaponCritChanceAdd );
                __result.MagicCritChanceAdd  = ClassSettings.Get( __result.ClassName, "MagicCritChanceAdd",  __result.MagicCritChanceAdd  );
                __result.WeaponCritDamageAdd = ClassSettings.Get( __result.ClassName, "WeaponCritDamageAdd", __result.WeaponCritDamageAdd );
                __result.MagicCritDamageAdd  = ClassSettings.Get( __result.ClassName, "MagicCritDamageAdd",  __result.MagicCritDamageAdd  );
            }
        }

    }
}