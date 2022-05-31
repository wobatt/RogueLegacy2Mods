using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_ClassStats {
    [BepInPlugin( "Wob.ClassStats", "Class Stats Mod", "1.0.1" )]
    public partial class ClassStats : BaseUnityPlugin {

        private static readonly WobSettings.KeyHelper<string> keys = new WobSettings.KeyHelper<string>( "Class_" );

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

        // Create all stat settings for a class at once
        public static void CreateSettings( string internalName, string configName, float[] statDefaults ) {
            // Make sure the right number of stats have been provided
            if( statDefaults.Length != stats.Length ) {
                WobPlugin.Log( "ERROR: Wrong number of stats for " + configName, WobPlugin.ERROR );
                return;
            }
            // Register the translation of internal class name to user-friendly name
            keys.Add( internalName, configName );
            // Go through each stat
            for( int i = 0; i < stats.Length; i++ ) {
                // Create a setting for the stat
                WobSettings.Add( new WobSettings.Num<float>( keys.Get( internalName, stats[i].name ), stats[i].desc + configName, statDefaults[i], 0.01f, bounds: (-99f, 1000000f) ) );
            }
        }

        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create the settings in bulk                                     HP   MP    Arm  Vit  Str  Int  Dex  Foc  WCC  MCC  WCD  MCD
            CreateSettings( "DualBladesClass",  "Assassin",     new float[] { -30f, 25f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            CreateSettings( "AstroClass",       "Astromancer",  new float[] { -40f, 100f, 0f,  0f,  0f,  20f, 0f,  0f,  0f,  0f,  0f,  0f  } );
            CreateSettings( "AxeClass",         "Barbarian",    new float[] {  0f,  25f,  0f,  20f, 0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            CreateSettings( "LuteClass",        "Bard",         new float[] { -15f, 50f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            CreateSettings( "BoxingGloveClass", "Boxer",        new float[] {  0f,  25f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            CreateSettings( "LadleClass",       "Chef",         new float[] { -30f, 100f, 0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            CreateSettings( "LanceClass",       "DragonLancer", new float[] {  0f,  25f,  20f, 0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            CreateSettings( "SaberClass",       "Duelist",      new float[] { -15f, 25f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            CreateSettings( "GunClass",         "Gunslinger",   new float[] { -30f, 50f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            CreateSettings( "SwordClass",       "Knight",       new float[] {  0f,  50f,  0f,  0f,  0f,  0f,  0f,  0f,  5f,  0f,  0f,  0f  } );
            CreateSettings( "MagicWandClass",   "Mage",         new float[] { -30f, 100f, 0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            CreateSettings( "CannonClass",      "Pirate",       new float[] {  0f,  25f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  10f, 10f } );
            CreateSettings( "BowClass",         "Ranger",       new float[] { -15f, 25f,  0f,  0f,  10f, 0f,  10f, 0f,  0f,  0f,  0f,  0f  } );
            CreateSettings( "KatanaClass",      "Ronin",        new float[] { -40f, 50f,  0f,  0f,  20f, 0f,  0f,  0f,  0f,  0f,  0f,  0f  } );
            CreateSettings( "SpearClass",       "Valkyrie",     new float[] { -15f, 50f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  5f,  0f,  10f } );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Patch for class passive stat data
        [HarmonyPatch( typeof( ClassData ), nameof( ClassData.PassiveData ), MethodType.Getter )]
        internal static class ClassData_PassiveData_Patch {
            internal static void Postfix( ref ClassPassiveData __result ) {
                if( keys.Exists( __result.ClassName ) ) {
                    // Set each of the properties on the passive data set to the value in config - value +1 for multipliers, without for additions
                    __result.MaxHPMod            = WobSettings.Get( keys.Get( __result.ClassName, "MaxHPMod"            ), __result.MaxHPMod            ) + 1f;
                    __result.MaxManaMod          = WobSettings.Get( keys.Get( __result.ClassName, "MaxManaMod"          ), __result.MaxManaMod          ) + 1f;
                    __result.ArmorMod            = WobSettings.Get( keys.Get( __result.ClassName, "ArmorMod"            ), __result.ArmorMod            ) + 1f;
                    __result.VitalityMod         = WobSettings.Get( keys.Get( __result.ClassName, "VitalityMod"         ), __result.VitalityMod         ) + 1f;
                    __result.StrengthMod         = WobSettings.Get( keys.Get( __result.ClassName, "StrengthMod"         ), __result.StrengthMod         ) + 1f;
                    __result.IntelligenceMod     = WobSettings.Get( keys.Get( __result.ClassName, "IntelligenceMod"     ), __result.IntelligenceMod     ) + 1f;
                    __result.DexterityMod        = WobSettings.Get( keys.Get( __result.ClassName, "DexterityMod"        ), __result.DexterityMod        ) + 1f;
                    __result.FocusMod            = WobSettings.Get( keys.Get( __result.ClassName, "FocusMod"            ), __result.FocusMod            ) + 1f;
                    __result.WeaponCritChanceAdd = WobSettings.Get( keys.Get( __result.ClassName, "WeaponCritChanceAdd" ), __result.WeaponCritChanceAdd );
                    __result.MagicCritChanceAdd  = WobSettings.Get( keys.Get( __result.ClassName, "MagicCritChanceAdd"  ), __result.MagicCritChanceAdd  );
                    __result.WeaponCritDamageAdd = WobSettings.Get( keys.Get( __result.ClassName, "WeaponCritDamageAdd" ), __result.WeaponCritDamageAdd );
                    __result.MagicCritDamageAdd  = WobSettings.Get( keys.Get( __result.ClassName, "MagicCritDamageAdd"  ), __result.MagicCritDamageAdd  );
                }
            }
        }
    }
}