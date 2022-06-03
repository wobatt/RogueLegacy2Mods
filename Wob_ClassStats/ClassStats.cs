using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_ClassStats {
    [BepInPlugin( "Wob.ClassStats", "Class Stats Mod", "1.0.1" )]
    public partial class ClassStats : BaseUnityPlugin {

        private static readonly WobSettings.KeyHelper<ClassType> keys = new WobSettings.KeyHelper<ClassType>( "Class_" );

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
            ( "SuperCritChance",     "Super crit chance modifier for "  ),
        };
        private static readonly (string name, string desc)[] effects = {
            ( "AttackManaRegen",   "Enable Astromancer's mana regen on all hits for "        ),
            ( "SpinKickDance",     "Enable Bard's spin kicks apply dance for "               ),
            ( "NoContactDamage",   "Enable Boxer's no contact damage for "                   ),
            ( "Burn",              "Enable Chef's attacks apply burn for "                   ),
            ( "TalentCrit",        "Enable Duelist's crit on talent use for "                ),
            ( "WeaponMagicDamage", "Enable Gunslinger's weapons deal +15% magic damage for " ),
            ( "ManaLeech",         "Enable Mage's weapons apply mana leech for "             ),
        };

        // Create all stat settings for a class at once
        public static void CreateSettings( ClassType internalName, string configName, float[] statDefaults, bool[] effectDefaults ) {
            // Make sure the right number of stats have been provided
            if( statDefaults.Length != stats.Length ) {
                WobPlugin.Log( "ERROR: Wrong number of stats for " + configName, WobPlugin.ERROR );
                return;
            }
            if( effectDefaults.Length != effects.Length ) {
                WobPlugin.Log( "ERROR: Wrong number of effects for " + configName, WobPlugin.ERROR );
                return;
            }
            // Register the translation of internal class name to user-friendly name
            keys.Add( internalName, configName );
            // Go through each stat
            for( int i = 0; i < stats.Length; i++ ) {
                // Create a setting for the stat
                WobSettings.Add( new WobSettings.Num<float>( keys.Get( internalName, stats[i].name ), stats[i].desc + configName, statDefaults[i], 0.01f, bounds: (-99f, 1000000f) ) );
            }
            // Go through each effect
            for( int i = 0; i < effects.Length; i++ ) {
                // Create a setting for the effect
                WobSettings.Add( new WobSettings.Boolean( keys.Get( internalName, effects[i].name ), effects[i].desc + configName, effectDefaults[i] ) );
            }
        }

        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create the settings in bulk                                             HP   MP    Arm  Vit  Str  Int  Dex  Foc  WCC  MCC  WCD  MCD  SCC                 Astro  Bard   Boxer  Chef   Duel   Gun    Mage
            CreateSettings( ClassType.DualBladesClass,  "Assassin",     new float[] { -30f, 25f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  10f }, new bool[] { false, false, false, false, false, false, false } );
            CreateSettings( ClassType.AstroClass,       "Astromancer",  new float[] { -40f, 100f, 0f,  0f,  0f,  20f, 0f,  0f,  0f,  0f,  0f,  0f,  0f  }, new bool[] { true,  false, false, false, false, false, false } );
            CreateSettings( ClassType.AxeClass,         "Barbarian",    new float[] {  0f,  25f,  0f,  20f, 0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  }, new bool[] { false, false, false, false, false, false, false } );
            CreateSettings( ClassType.LuteClass,        "Bard",         new float[] { -15f, 50f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  }, new bool[] { false, true,  false, false, false, false, false } );
            CreateSettings( ClassType.BoxingGloveClass, "Boxer",        new float[] {  0f,  25f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  }, new bool[] { false, false, true,  false, false, false, false } );
            CreateSettings( ClassType.LadleClass,       "Chef",         new float[] { -30f, 100f, 0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  }, new bool[] { false, false, false, true,  false, false, false } );
            CreateSettings( ClassType.LanceClass,       "DragonLancer", new float[] {  0f,  25f,  20f, 0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  }, new bool[] { false, false, false, false, false, false, false } );
            CreateSettings( ClassType.SaberClass,       "Duelist",      new float[] { -15f, 25f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  }, new bool[] { false, false, false, false, true,  false, false } );
            CreateSettings( ClassType.GunClass,         "Gunslinger",   new float[] { -30f, 50f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  }, new bool[] { false, false, false, false, false, true,  false } );
            CreateSettings( ClassType.SwordClass,       "Knight",       new float[] {  0f,  50f,  0f,  0f,  0f,  0f,  0f,  0f,  5f,  0f,  0f,  0f,  0f  }, new bool[] { false, false, false, false, false, false, false } );
            CreateSettings( ClassType.MagicWandClass,   "Mage",         new float[] { -30f, 100f, 0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  }, new bool[] { false, false, false, false, false, false, true  } );
            CreateSettings( ClassType.CannonClass,      "Pirate",       new float[] {  0f,  25f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  10f, 10f, 0f  }, new bool[] { false, false, false, false, false, false, false } );
            CreateSettings( ClassType.BowClass,         "Ranger",       new float[] { -15f, 25f,  0f,  0f,  10f, 0f,  10f, 0f,  0f,  0f,  0f,  0f,  0f  }, new bool[] { false, false, false, false, false, false, false } );
            CreateSettings( ClassType.KatanaClass,      "Ronin",        new float[] { -40f, 50f,  0f,  0f,  20f, 0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f  }, new bool[] { false, false, false, false, false, false, false } );
            CreateSettings( ClassType.SpearClass,       "Valkyrie",     new float[] { -15f, 50f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  5f,  0f,  10f, 0f  }, new bool[] { false, false, false, false, false, false, false } );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Patch for class passive stat data
        [HarmonyPatch( typeof( ClassLibrary ), "Instance", MethodType.Getter )]
        internal static class ClassLibrary_Instance_Patch {
            private static bool runOnce = false;
            internal static void Postfix( ref ClassLibrary __result ) {
                if( !runOnce ) {
                    ClassTypeClassDataDictionary classLibrary = (ClassTypeClassDataDictionary)Traverse.Create( __result ).Field( "m_classLibrary" ).GetValue();
                    foreach( ClassType classType in classLibrary.Keys ) {
                        if( keys.Exists( classType ) ) {
                            ClassPassiveData passiveData = classLibrary[classType].PassiveData;
                            if( passiveData != null ) {
                                // Set each of the properties on the passive data set to the value in config - value +1 for multipliers, without for additions
                                passiveData.MaxHPMod            = WobSettings.Get( keys.Get( classType, "MaxHPMod"            ), passiveData.MaxHPMod            ) + 1f;
                                passiveData.MaxManaMod          = WobSettings.Get( keys.Get( classType, "MaxManaMod"          ), passiveData.MaxManaMod          ) + 1f;
                                passiveData.ArmorMod            = WobSettings.Get( keys.Get( classType, "ArmorMod"            ), passiveData.ArmorMod            ) + 1f;
                                passiveData.VitalityMod         = WobSettings.Get( keys.Get( classType, "VitalityMod"         ), passiveData.VitalityMod         ) + 1f;
                                passiveData.StrengthMod         = WobSettings.Get( keys.Get( classType, "StrengthMod"         ), passiveData.StrengthMod         ) + 1f;
                                passiveData.IntelligenceMod     = WobSettings.Get( keys.Get( classType, "IntelligenceMod"     ), passiveData.IntelligenceMod     ) + 1f;
                                passiveData.DexterityMod        = WobSettings.Get( keys.Get( classType, "DexterityMod"        ), passiveData.DexterityMod        ) + 1f;
                                passiveData.FocusMod            = WobSettings.Get( keys.Get( classType, "FocusMod"            ), passiveData.FocusMod            ) + 1f;
                                passiveData.WeaponCritChanceAdd = WobSettings.Get( keys.Get( classType, "WeaponCritChanceAdd" ), passiveData.WeaponCritChanceAdd );
                                passiveData.MagicCritChanceAdd  = WobSettings.Get( keys.Get( classType, "MagicCritChanceAdd"  ), passiveData.MagicCritChanceAdd  );
                                passiveData.WeaponCritDamageAdd = WobSettings.Get( keys.Get( classType, "WeaponCritDamageAdd" ), passiveData.WeaponCritDamageAdd );
                                passiveData.MagicCritDamageAdd  = WobSettings.Get( keys.Get( classType, "MagicCritDamageAdd"  ), passiveData.MagicCritDamageAdd  );
                            }
                        }
                    }
                    runOnce = true;
                }
            }
        }

        // Assassin - +10% Super Crit. Chance
        [HarmonyPatch( typeof( EnemyController ), nameof( EnemyController.CalculateDamageTaken ) )]
        internal static class EnemyController_CalculateDamageTaken_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "EnemyController.CalculateDamageTaken Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            // if (PlayerManager.GetPlayerController().CharacterClass.ClassType == ClassType.DualBladesClass)
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_CharacterClass" ), // PlayerManager.GetPlayerController().CharacterClass
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_ClassType"      ), // PlayerManager.GetPlayerController().CharacterClass.ClassType
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4, ClassType.DualBladesClass  ), // ClassType.DualBladesClass
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Bne_Un                               ), // PlayerManager.GetPlayerController().CharacterClass.ClassType == ClassType.DualBladesClass
                            // num5 = num5 + 0.1f
                            /*  4 */ new WobTranspiler.OpTest( OpCodeSet.Ldloc                              ), // num5
                            /*  5 */ new WobTranspiler.OpTest( OpCodes.Ldc_R4                               ), // 0.1f
                            /*  6 */ new WobTranspiler.OpTest( OpCodes.Add                                  ), // num5 + 0.1f
                            /*  7 */ new WobTranspiler.OpTest( OpCodeSet.Stloc                              ), // num5 = num5 + 0.1f
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_SetInstruction( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Check_SuperCritChance( ClassType.DualBladesClass ) ) ),
                                new WobTranspiler.OpAction_SetOpcode( 3, OpCodes.Brfalse ),
                                new WobTranspiler.OpAction_SetInstruction( 5, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Get_SuperCritChance() ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool Check_SuperCritChance( ClassType playerClass ) {
                return keys.Exists( playerClass ) && ( WobSettings.Get( keys.Get( playerClass, "SuperCritChance" ), 0f ) != 0f );
            }

            private static float Get_SuperCritChance() {
                ClassType playerClass = PlayerManager.GetPlayerController().CharacterClass.ClassType;
                return WobSettings.Get( keys.Get( playerClass, "SuperCritChance" ), 0.1f );
            }
        }

        // Astromancer - All attacks generate mana
        [HarmonyPatch( typeof( ManaRegen ), "OnEnemyHit" )]
        internal static class ManaRegen_OnEnemyHit_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "ManaRegen.OnEnemyHit Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldfld, name: "CurrentCharacter" ), // SaveManager.PlayerSaveData.CurrentCharacter
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Ldfld, name: "ClassType"        ), // SaveManager.PlayerSaveData.CurrentCharacter.ClassType
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4, ClassType.AstroClass  ), // ClassType.AstroClass
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Ceq                             ), // SaveManager.PlayerSaveData.CurrentCharacter.ClassType == ClassType.AstroClass
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_SetInstruction( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Check_AttackManaRegen( ClassType.AstroClass ) ) ),
                                new WobTranspiler.OpAction_Remove( 3, 1 ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool Check_AttackManaRegen( ClassType playerClass ) {
                return keys.Exists( playerClass ) && WobSettings.Get( keys.Get( playerClass, "AttackManaRegen" ), playerClass == ClassType.AstroClass );
            }
        }

        // Bard - Spin Kicks stack Dance, up to 5 times
        [HarmonyPatch( typeof( LuteWeaponProjectileLogic ), "OnExplosionCollision" )]
        internal static class LuteWeaponProjectileLogic_OnExplosionCollision_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "LuteWeaponProjectileLogic.OnExplosionCollision Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_CharacterClass" ), // ...CharacterClass
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_ClassType"      ), // ...CharacterClass.ClassType
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4, ClassType.LuteClass        ), // ClassType.LuteClass
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Bne_Un                               ), // ...CharacterClass.ClassType == ClassType.LuteClass
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_SetInstruction( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Check_SpinKickDance( ClassType.LuteClass ) ) ),
                                new WobTranspiler.OpAction_SetOpcode( 3, OpCodes.Brfalse ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool Check_SpinKickDance( ClassType playerClass ) {
                return keys.Exists( playerClass ) && WobSettings.Get( keys.Get( playerClass, "SpinKickDance" ), playerClass == ClassType.LuteClass );
            }
        }

        // Chef - Weapon applies Burn
        [HarmonyPatch( typeof( BaseAbility_RL ), nameof( BaseAbility_RL.InitializeProjectile ) )]
        internal static class BaseAbility_RL_InitializeProjectile_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "BaseAbility_RL.InitializeProjectile Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_CharacterClass" ), // ...CharacterClass
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_ClassType"      ), // ...CharacterClass.ClassType
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4, ClassType.LadleClass       ), // ClassType.LadleClass
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Bne_Un                               ), // ...CharacterClass.ClassType == ClassType.LadleClass
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_SetInstruction( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Check_Burn( ClassType.LadleClass ) ) ),
                                new WobTranspiler.OpAction_SetOpcode( 3, OpCodes.Brfalse ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool Check_Burn( ClassType playerClass ) {
                return keys.Exists( playerClass ) && WobSettings.Get( keys.Get( playerClass, "Burn" ), playerClass == ClassType.LadleClass );
            }
        }

        // Mage - Weapon applies Mana Leech
        [HarmonyPatch( typeof( EnemyHitResponse ), "CharacterDamaged" )]
        internal static class EnemyHitResponse_CharacterDamaged_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "EnemyHitResponse.CharacterDamaged Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_CharacterClass" ), // ...CharacterClass
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_ClassType"      ), // ...CharacterClass.ClassType
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4, ClassType.MagicWandClass   ), // ClassType.MagicWandClass
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Bne_Un                               ), // ...CharacterClass.ClassType == ClassType.MagicWandClass
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_SetInstruction( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Check_ManaLeech( ClassType.MagicWandClass ) ) ),
                                new WobTranspiler.OpAction_SetOpcode( 3, OpCodes.Brfalse ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool Check_ManaLeech( ClassType playerClass ) {
                return keys.Exists( playerClass ) && WobSettings.Get( keys.Get( playerClass, "ManaLeech" ), playerClass == ClassType.MagicWandClass );
            }
        }

        // Duelist - Talents grant Charged
        [HarmonyPatch()]
        internal static class CastAbility_RL_CastAbility_Patch {
            internal static MethodInfo TargetMethod() {
                // Find the nested class of 'CastAbility_RL' that 'CastAbility' implicitly created
                System.Type type = AccessTools.FirstInner( typeof( CastAbility_RL ), t => t.Name.Contains( "<CastAbility>d__" ) );
                // Find the 'MoveNext' method on the nested class
                return AccessTools.FirstMethod( type, method => method.Name.Contains( "MoveNext" ) );
            }

            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "CastAbility_RL.CastAbility Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_CharacterClass" ), // ...CharacterClass
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_ClassType"      ), // ...CharacterClass.ClassType
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4, ClassType.SaberClass       ), // ClassType.SaberClass
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Bne_Un                               ), // ...CharacterClass.ClassType == ClassType.SaberClass
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_SetInstruction( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Check_TalentCrit( ClassType.SaberClass ) ) ),
                                new WobTranspiler.OpAction_SetOpcode( 3, OpCodes.Brfalse ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool Check_TalentCrit( ClassType playerClass ) {
                return keys.Exists( playerClass ) && WobSettings.Get( keys.Get( playerClass, "TalentCrit" ), playerClass == ClassType.SaberClass );
            }
        }

        // Duelist - Talents grant Charged
        [HarmonyPatch( typeof( CastAbility_RL ), nameof( CastAbility_RL.StopAbility ) )]
        internal static class CastAbility_RL_StopAbility_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "CastAbility_RL.StopAbility Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_CharacterClass" ), // ...CharacterClass
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_ClassType"      ), // ...CharacterClass.ClassType
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4, ClassType.SaberClass       ), // ClassType.SaberClass
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Bne_Un                               ), // ...CharacterClass.ClassType == ClassType.SaberClass
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_SetInstruction( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Check_TalentCrit( ClassType.SaberClass ) ) ),
                                new WobTranspiler.OpAction_SetOpcode( 3, OpCodes.Brfalse ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool Check_TalentCrit( ClassType playerClass ) {
                return keys.Exists( playerClass ) && WobSettings.Get( keys.Get( playerClass, "TalentCrit" ), playerClass == ClassType.SaberClass );
            }
        }

        // Boxer - No contact damage
        // Gunslinger - Weapons deal +15% magic damage
        [HarmonyPatch()]
        internal static class PlayerController_ApplyPermanentStatusEffectsCoroutine_Patch {
            internal static MethodInfo TargetMethod() {
                // Find the nested class of 'PlayerController' that 'ApplyPermanentStatusEffectsCoroutine' implicitly created
                System.Type type = AccessTools.FirstInner( typeof( PlayerController ), t => t.Name.Contains( "<ApplyPermanentStatusEffectsCoroutine>d__" ) );
                // Find the 'MoveNext' method on the nested class
                return AccessTools.FirstMethod( type, method => method.Name.Contains( "MoveNext" ) );
            }

            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "PlayerController.ApplyPermanentStatusEffectsCoroutine Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Call, name: "get_CharacterClass"     ), // ...CharacterClass
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_ClassType"      ), // ...CharacterClass.ClassType
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4, ClassType.BoxingGloveClass ), // ClassType.BoxingGloveClass
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Bne_Un                               ), // ...CharacterClass.ClassType == ClassType.BoxingGloveClass
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_SetInstruction( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Check_NoContactDamage( ClassType.BoxingGloveClass ) ) ),
                                new WobTranspiler.OpAction_SetOpcode( 3, OpCodes.Brfalse ),
                        } );
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Call, name: "get_CharacterClass" ), // ...CharacterClass
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Callvirt, name: "get_ClassType"  ), // ...CharacterClass.ClassType
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Ldc_I4, ClassType.GunClass     ), // ClassType.GunClass
                            /*  3 */ new WobTranspiler.OpTest( OpCodes.Bne_Un                           ), // ...CharacterClass.ClassType == ClassType.GunClass
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_SetInstruction( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Check_WeaponMagicDamage( ClassType.GunClass ) ) ),
                                new WobTranspiler.OpAction_SetOpcode( 3, OpCodes.Brfalse ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool Check_NoContactDamage( ClassType playerClass ) {
                return keys.Exists( playerClass ) && WobSettings.Get( keys.Get( playerClass, "NoContactDamage" ), playerClass == ClassType.BoxingGloveClass );
            }

            private static bool Check_WeaponMagicDamage( ClassType playerClass ) {
                return keys.Exists( playerClass ) && WobSettings.Get( keys.Get( playerClass, "WeaponMagicDamage" ), playerClass == ClassType.GunClass );
            }
        }
    }
}