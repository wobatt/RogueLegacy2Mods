using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MoreMountains.CorgiEngine;
using UnityEngine;
using Wob_Common;

namespace WobMod {
    internal class Classes {

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTING DEFAULTS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        
        private record ClassInfo( string Config, float HP, float MP, float Arm, float Vit, float Str, float Int, float Dex, float Foc, float WCC, float MCC, float WCD, float MCD, float SCC, bool Astro, bool Bard, bool Boxer, bool Chef, bool Duel, bool Gun, bool Mage );
		private static readonly Dictionary<ClassType, ClassInfo> classInfo = new() {
            //                                                  HP   MP    Arm  Vit  Str  Int  Dex  Foc  WCC  MCC  WCD  MCD  SCC  Astro  Bard   Boxer  Chef   Duel   Gun    Mage   Clown
            { ClassType.DualBladesClass,  new( "Assassin",     -30f, 50f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  10f, false, false, false, false, false, false, false ) },
            { ClassType.AstroClass,       new( "Astromancer",  -40f, 100f, 0f,  0f,  0f,  20f, 0f,  0f,  0f,  0f,  0f,  0f,  0f,  true,  false, false, false, false, false, false ) },
            { ClassType.AxeClass,         new( "Barbarian",     0f,  25f,  0f,  20f, 0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  false, false, false, false, false, false, false ) },
            { ClassType.LuteClass,        new( "Bard",         -15f, 50f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  false, true,  false, false, false, false, false ) },
            { ClassType.BoxingGloveClass, new( "Boxer",         0f,  25f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  false, false, true,  false, false, false, false ) },
            { ClassType.LadleClass,       new( "Chef",         -30f, 50f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  false, false, false, true,  false, false, false ) },
            { ClassType.LanceClass,       new( "DragonLancer",  0f,  25f,  20f, 0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  false, false, false, false, false, false, false ) },
            { ClassType.SaberClass,       new( "Duelist",      -15f, 25f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  false, false, false, false, true,  false, false ) },
            { ClassType.GunClass,         new( "Gunslinger",   -30f, 50f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  false, false, false, false, false, true,  false ) },
            { ClassType.SwordClass,       new( "Knight",        0f,  50f,  0f,  0f,  0f,  0f,  0f,  0f,  5f,  0f,  0f,  0f,  0f,  false, false, false, false, false, false, false ) },
            { ClassType.MagicWandClass,   new( "Mage",         -30f, 100f, 0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  false, false, false, false, false, false, true  ) },
            { ClassType.CannonClass,      new( "Pirate",        0f,  25f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  10f, 10f, 0f,  false, false, false, false, false, false, false ) },
            { ClassType.BowClass,         new( "Ranger",       -15f, 25f,  0f,  0f,  10f, 0f,  10f, 0f,  0f,  0f,  0f,  0f,  0f,  false, false, false, false, false, false, false ) },
            { ClassType.KatanaClass,      new( "Ronin",        -40f, 50f,  0f,  0f,  20f, 0f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  false, false, false, false, false, false, false ) },
            { ClassType.SpearClass,       new( "Valkyrie",     -15f, 50f,  0f,  0f,  0f,  0f,  0f,  0f,  0f,  5f,  0f,  10f, 0f,  false, false, false, false, false, false, false ) },
        };

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTINGS READ/WRITE
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        internal static readonly WobSettings.KeyHelper<ClassType> classKeys = new( "Class" );

        internal static void RunSetup() {
            WobMod.configFiles.Add( "Classes", "Classes" );
            foreach( ClassType classType in classInfo.Keys ) {
                // Register the translation of internal class name to user-friendly name
                classKeys.Add( classType, classInfo[classType].Config );
                // Create a setting for each stat
                WobSettings.Add( new WobSettings.Entry[] {
                    new WobSettings.Num<float>( WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "MaxHPMod"            ), "Max health percent modifier for "                        + classInfo[classType].Config, classInfo[classType].HP,  0.01f, bounds: (-99f, 1000000f) ),
                    new WobSettings.Num<float>( WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "MaxManaMod"          ), "Max mana percent modifier for "                          + classInfo[classType].Config, classInfo[classType].MP,  0.01f, bounds: (-99f, 1000000f) ),
                    new WobSettings.Num<float>( WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "ArmorMod"            ), "Armor percent modifier for "                             + classInfo[classType].Config, classInfo[classType].Arm, 0.01f, bounds: (-99f, 1000000f) ),
                    new WobSettings.Num<float>( WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "VitalityMod"         ), "Vitality percent modifier for "                          + classInfo[classType].Config, classInfo[classType].Vit, 0.01f, bounds: (-99f, 1000000f) ),
                    new WobSettings.Num<float>( WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "StrengthMod"         ), "Strength percent modifier for "                          + classInfo[classType].Config, classInfo[classType].Str, 0.01f, bounds: (-99f, 1000000f) ),
                    new WobSettings.Num<float>( WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "IntelligenceMod"     ), "Intelligence percent modifier for "                      + classInfo[classType].Config, classInfo[classType].Int, 0.01f, bounds: (-99f, 1000000f) ),
                    new WobSettings.Num<float>( WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "DexterityMod"        ), "Dexterity percent modifier for "                         + classInfo[classType].Config, classInfo[classType].Dex, 0.01f, bounds: (-99f, 1000000f) ),
                    new WobSettings.Num<float>( WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "FocusMod"            ), "Focus percent modifier for "                             + classInfo[classType].Config, classInfo[classType].Foc, 0.01f, bounds: (-99f, 1000000f) ),
                    new WobSettings.Num<float>( WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "WeaponCritChanceAdd" ), "Weapon crit chance percent modifier for "                + classInfo[classType].Config, classInfo[classType].WCC, 0.01f, bounds: (-99f, 1000000f) ),
                    new WobSettings.Num<float>( WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "MagicCritChanceAdd"  ), "Magic crit chance percent modifier for "                 + classInfo[classType].Config, classInfo[classType].MCC, 0.01f, bounds: (-99f, 1000000f) ),
                    new WobSettings.Num<float>( WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "WeaponCritDamageAdd" ), "Weapon crit damage percent modifier for "                + classInfo[classType].Config, classInfo[classType].WCD, 0.01f, bounds: (-99f, 1000000f) ),
                    new WobSettings.Num<float>( WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "MagicCritDamageAdd"  ), "Magic crit damage percent modifier for "                 + classInfo[classType].Config, classInfo[classType].MCD, 0.01f, bounds: (-99f, 1000000f) ),
                    new WobSettings.Num<float>( WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "SuperCritChance"     ), "Super crit chance percent modifier for "                 + classInfo[classType].Config, classInfo[classType].SCC, 0.01f, bounds: (-99f, 1000000f) ),
                    new WobSettings.Boolean(    WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "AttackManaRegen"     ), "Enable Astromancer's mana regen on all hits for "        + classInfo[classType].Config, classInfo[classType].Astro                                ),
                    new WobSettings.Boolean(    WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "SpinKickDance"       ), "Enable Bard's spin kicks apply dance for "               + classInfo[classType].Config, classInfo[classType].Bard                                 ),
                    new WobSettings.Boolean(    WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "NoContactDamage"     ), "Enable Boxer's no contact damage for "                   + classInfo[classType].Config, classInfo[classType].Boxer                                ),
                    new WobSettings.Boolean(    WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "Burn"                ), "Enable Chef's attacks apply burn for "                   + classInfo[classType].Config, classInfo[classType].Chef                                 ),
                    new WobSettings.Boolean(    WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "TalentCrit"          ), "Enable Duelist's crit on talent use for "                + classInfo[classType].Config, classInfo[classType].Duel                                 ),
                    new WobSettings.Boolean(    WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "WeaponMagicDamage"   ), "Enable Gunslinger's weapons deal +15% magic damage for " + classInfo[classType].Config, classInfo[classType].Gun                                  ),
                    new WobSettings.Boolean(    WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "ManaLeech"           ), "Enable Mage's weapons apply mana leech for "             + classInfo[classType].Config, classInfo[classType].Mage                                 ),
                    new WobSettings.Boolean(    WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "ClownBounce"         ), "Enable Clown trait's spin kick terrain for "             + classInfo[classType].Config, false                                                     ),
                    new WobSettings.Boolean(    WobMod.configFiles.Get( "Classes" ), classKeys.Get( classType, "LimitlessMana"       ), "Enable Limitless trait's infinite mana overcharge for "  + classInfo[classType].Config, false                                                     ),
                } );
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - BASIC STATS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        
        // Patch for class passive stat data
        [HarmonyPatch( typeof( ClassLibrary ), "Instance", MethodType.Getter )]
        internal static class ClassLibrary_Instance_Patch {
            private static bool runOnce = false;
            internal static void Postfix( ref ClassLibrary __result ) {
                if( !runOnce ) {
                    ClassTypeClassDataDictionary classLibrary = (ClassTypeClassDataDictionary)Traverse.Create( __result ).Field( "m_classLibrary" ).GetValue();
                    foreach( ClassType classType in classLibrary.Keys ) {
                        if( classKeys.Exists( classType ) ) {
                            ClassPassiveData passiveData = classLibrary[classType].PassiveData;
                            if( passiveData != null ) {
                                // Set each of the properties on the passive data set to the value in config - value +1 for multipliers, without for additions
                                passiveData.MaxHPMod            = WobSettings.Get( classKeys.Get( classType, "MaxHPMod"            ), passiveData.MaxHPMod            ) + 1f;
                                passiveData.MaxManaMod          = WobSettings.Get( classKeys.Get( classType, "MaxManaMod"          ), passiveData.MaxManaMod          ) + 1f;
                                passiveData.ArmorMod            = WobSettings.Get( classKeys.Get( classType, "ArmorMod"            ), passiveData.ArmorMod            ) + 1f;
                                passiveData.VitalityMod         = WobSettings.Get( classKeys.Get( classType, "VitalityMod"         ), passiveData.VitalityMod         ) + 1f;
                                passiveData.StrengthMod         = WobSettings.Get( classKeys.Get( classType, "StrengthMod"         ), passiveData.StrengthMod         ) + 1f;
                                passiveData.IntelligenceMod     = WobSettings.Get( classKeys.Get( classType, "IntelligenceMod"     ), passiveData.IntelligenceMod     ) + 1f;
                                passiveData.DexterityMod        = WobSettings.Get( classKeys.Get( classType, "DexterityMod"        ), passiveData.DexterityMod        ) + 1f;
                                passiveData.FocusMod            = WobSettings.Get( classKeys.Get( classType, "FocusMod"            ), passiveData.FocusMod            ) + 1f;
                                passiveData.WeaponCritChanceAdd = WobSettings.Get( classKeys.Get( classType, "WeaponCritChanceAdd" ), passiveData.WeaponCritChanceAdd );
                                passiveData.MagicCritChanceAdd  = WobSettings.Get( classKeys.Get( classType, "MagicCritChanceAdd"  ), passiveData.MagicCritChanceAdd  );
                                passiveData.WeaponCritDamageAdd = WobSettings.Get( classKeys.Get( classType, "WeaponCritDamageAdd" ), passiveData.WeaponCritDamageAdd );
                                passiveData.MagicCritDamageAdd  = WobSettings.Get( classKeys.Get( classType, "MagicCritDamageAdd"  ), passiveData.MagicCritDamageAdd  );
                            }
                        }
                    }
                    runOnce = true;
                }
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - CLASS SPECIALS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Assassin - +10% Super Crit. Chance
        [HarmonyPatch( typeof( EnemyController ), nameof( EnemyController.CalculateDamageTaken ) )]
        internal static class EnemyController_CalculateDamageTaken_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "EnemyController.CalculateDamageTaken" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        // if (PlayerManager.GetPlayerController().CharacterClass.ClassType == ClassType.DualBladesClass)
                        /*  0 */ new( OpCodes.Callvirt, name: "get_CharacterClass" ), // PlayerManager.GetPlayerController().CharacterClass
                        /*  1 */ new( OpCodes.Callvirt, name: "get_ClassType"      ), // PlayerManager.GetPlayerController().CharacterClass.ClassType
                        /*  2 */ new( OpCodeSet.Ldc_I4, ClassType.DualBladesClass  ), // ClassType.DualBladesClass
                        /*  3 */ new( OpCodes.Bne_Un                               ), // PlayerManager.GetPlayerController().CharacterClass.ClassType == ClassType.DualBladesClass
                        // num5 = num5 + 0.1f
                        /*  4 */ new( OpCodeSet.Ldloc                              ), // num5
                        /*  5 */ new( OpCodes.Ldc_R4                               ), // 0.1f
                        /*  6 */ new( OpCodes.Add                                  ), // num5 + 0.1f
                        /*  7 */ new( OpCodeSet.Stloc                              ), // num5 = num5 + 0.1f
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetInstruction( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Check_SuperCritChance( ClassType.DualBladesClass ) ) ),
                            new WobTranspiler.OpAction_SetOpcode( 3, OpCodes.Brfalse ),
                            new WobTranspiler.OpAction_SetInstruction( 5, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Get_SuperCritChance() ) ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool Check_SuperCritChance( ClassType playerClass ) {
                return classKeys.Exists( playerClass ) && ( WobSettings.Get( classKeys.Get( playerClass, "SuperCritChance" ), 0f ) != 0f );
            }

            private static float Get_SuperCritChance() {
                ClassType playerClass = PlayerManager.GetPlayerController().CharacterClass.ClassType;
                return WobSettings.Get( classKeys.Get( playerClass, "SuperCritChance" ), 0.1f );
            }
        }

        // Astromancer - All attacks generate mana
        [HarmonyPatch( typeof( ManaRegen ), "OnEnemyHit" )]
        internal static class ManaRegen_OnEnemyHit_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "ManaRegen.OnEnemyHit" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldfld, name: "CurrentCharacter" ), // SaveManager.PlayerSaveData.CurrentCharacter
                        /*  1 */ new( OpCodes.Ldfld, name: "ClassType"        ), // SaveManager.PlayerSaveData.CurrentCharacter.ClassType
                        /*  2 */ new( OpCodeSet.Ldc_I4, ClassType.AstroClass  ), // ClassType.AstroClass
                        /*  3 */ new( OpCodes.Ceq                             ), // SaveManager.PlayerSaveData.CurrentCharacter.ClassType == ClassType.AstroClass
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetInstruction( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Check_AttackManaRegen( ClassType.AstroClass ) ) ),
                            new WobTranspiler.OpAction_Remove( 3, 1 ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool Check_AttackManaRegen( ClassType playerClass ) {
                return classKeys.Exists( playerClass ) && WobSettings.Get( classKeys.Get( playerClass, "AttackManaRegen" ), playerClass == ClassType.AstroClass );
            }
        }

        // Chef - Weapon applies Burn
        [HarmonyPatch( typeof( BaseAbility_RL ), nameof( BaseAbility_RL.InitializeProjectile ) )]
        internal static class BaseAbility_RL_InitializeProjectile_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "BaseAbility_RL.InitializeProjectile" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Callvirt, name: "get_CharacterClass" ), // ...CharacterClass
                        /*  1 */ new( OpCodes.Callvirt, name: "get_ClassType"      ), // ...CharacterClass.ClassType
                        /*  2 */ new( OpCodeSet.Ldc_I4, ClassType.LadleClass       ), // ClassType.LadleClass
                        /*  3 */ new( OpCodes.Bne_Un                               ), // ...CharacterClass.ClassType == ClassType.LadleClass
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetInstruction( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Check_Burn( ClassType.LadleClass ) ) ),
                            new WobTranspiler.OpAction_SetOpcode( 3, OpCodes.Brfalse ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool Check_Burn( ClassType playerClass ) {
                return classKeys.Exists( playerClass ) && WobSettings.Get( classKeys.Get( playerClass, "Burn" ), playerClass == ClassType.LadleClass );
            }
        }

        // Mage - Weapon applies Mana Leech
        [HarmonyPatch( typeof( EnemyHitResponse ), "CharacterDamaged" )]
        internal static class EnemyHitResponse_CharacterDamaged_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "EnemyHitResponse.CharacterDamaged" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Callvirt, name: "get_CharacterClass" ), // ...CharacterClass
                        /*  1 */ new( OpCodes.Callvirt, name: "get_ClassType"      ), // ...CharacterClass.ClassType
                        /*  2 */ new( OpCodeSet.Ldc_I4, ClassType.MagicWandClass   ), // ClassType.MagicWandClass
                        /*  3 */ new( OpCodes.Bne_Un                               ), // ...CharacterClass.ClassType == ClassType.MagicWandClass
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetInstruction( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Check_ManaLeech( ClassType.MagicWandClass ) ) ),
                            new WobTranspiler.OpAction_SetOpcode( 3, OpCodes.Brfalse ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool Check_ManaLeech( ClassType playerClass ) {
                return classKeys.Exists( playerClass ) && WobSettings.Get( classKeys.Get( playerClass, "ManaLeech" ), playerClass == ClassType.MagicWandClass );
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
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "CastAbility_RL.CastAbility" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Callvirt, name: "get_CharacterClass" ), // ...CharacterClass
                        /*  1 */ new( OpCodes.Callvirt, name: "get_ClassType"      ), // ...CharacterClass.ClassType
                        /*  2 */ new( OpCodeSet.Ldc_I4, ClassType.SaberClass       ), // ClassType.SaberClass
                        /*  3 */ new( OpCodes.Bne_Un                               ), // ...CharacterClass.ClassType == ClassType.SaberClass
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetInstruction( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Check_TalentCrit( ClassType.SaberClass ) ) ),
                            new WobTranspiler.OpAction_SetOpcode( 3, OpCodes.Brfalse ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool Check_TalentCrit( ClassType playerClass ) {
                return classKeys.Exists( playerClass ) && WobSettings.Get( classKeys.Get( playerClass, "TalentCrit" ), playerClass == ClassType.SaberClass );
            }
        }

        // Duelist - Talents grant Charged
        [HarmonyPatch( typeof( CastAbility_RL ), nameof( CastAbility_RL.StopAbility ) )]
        internal static class CastAbility_RL_StopAbility_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "CastAbility_RL.StopAbility" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Callvirt, name: "get_CharacterClass" ), // ...CharacterClass
                        /*  1 */ new( OpCodes.Callvirt, name: "get_ClassType"      ), // ...CharacterClass.ClassType
                        /*  2 */ new( OpCodeSet.Ldc_I4, ClassType.SaberClass       ), // ClassType.SaberClass
                        /*  3 */ new( OpCodes.Bne_Un                               ), // ...CharacterClass.ClassType == ClassType.SaberClass
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetInstruction( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Check_TalentCrit( ClassType.SaberClass ) ) ),
                            new WobTranspiler.OpAction_SetOpcode( 3, OpCodes.Brfalse ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool Check_TalentCrit( ClassType playerClass ) {
                return classKeys.Exists( playerClass ) && WobSettings.Get( classKeys.Get( playerClass, "TalentCrit" ), playerClass == ClassType.SaberClass );
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
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "PlayerController.ApplyPermanentStatusEffectsCoroutine" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Call, name: "get_CharacterClass"     ), // ...CharacterClass
                        /*  1 */ new( OpCodes.Callvirt, name: "get_ClassType"      ), // ...CharacterClass.ClassType
                        /*  2 */ new( OpCodeSet.Ldc_I4, ClassType.BoxingGloveClass ), // ClassType.BoxingGloveClass
                        /*  3 */ new( OpCodes.Bne_Un                               ), // ...CharacterClass.ClassType == ClassType.BoxingGloveClass
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetInstruction( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Check_NoContactDamage( ClassType.BoxingGloveClass ) ) ),
                            new WobTranspiler.OpAction_SetOpcode( 3, OpCodes.Brfalse ),
                    }, expected: 1 );
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Call, name: "get_CharacterClass" ), // ...CharacterClass
                        /*  1 */ new( OpCodes.Callvirt, name: "get_ClassType"  ), // ...CharacterClass.ClassType
                        /*  2 */ new( OpCodeSet.Ldc_I4, ClassType.GunClass     ), // ClassType.GunClass
                        /*  3 */ new( OpCodes.Bne_Un                           ), // ...CharacterClass.ClassType == ClassType.GunClass
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetInstruction( 2, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Check_WeaponMagicDamage( ClassType.GunClass ) ) ),
                            new WobTranspiler.OpAction_SetOpcode( 3, OpCodes.Brfalse ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool Check_NoContactDamage( ClassType playerClass ) {
                return classKeys.Exists( playerClass ) && WobSettings.Get( classKeys.Get( playerClass, "NoContactDamage" ), playerClass == ClassType.BoxingGloveClass );
            }

            private static bool Check_WeaponMagicDamage( ClassType playerClass ) {
                return classKeys.Exists( playerClass ) && WobSettings.Get( classKeys.Get( playerClass, "WeaponMagicDamage" ), playerClass == ClassType.GunClass );
            }
        }

        // Clown trait - Can spin kick terrain
        [HarmonyPatch( typeof( CharacterDownStrike_RL ), "DownStrike" )]
        public static class CharacterDownStrike_RL_DownStrike_Patch {
            // Find the correct method - this is an implicitly defined method
            // CharacterDownStrike_RL.DownStrike returns an IEnumerator, and we need to patch the MoveNext method on that
            internal static MethodInfo TargetMethod() {
                // Find the nested class the method implicitly created
                System.Type type = AccessTools.FirstInner( typeof( CharacterDownStrike_RL ), t => t.Name.Contains( "<DownStrike>d__" ) );
                // Find the 'MoveNext' method on the nested class
                return AccessTools.FirstMethod( type, method => method.Name.Contains( "MoveNext" ) );
            }

            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "CharacterDownStrike_RL.DownStrike" );
                // Perform the patching - should match 2 occurrences
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldc_I4, TraitType.BounceTerrain  ), // TraitType.BounceTerrain
                        /*  1 */ new( OpCodes.Call, name: "IsTraitActive"      ), // TraitManager.IsTraitActive(TraitType.BounceTerrain)
                        /*  2 */ new( OpCodeSet.Brfalse                        ), // if (TraitManager.IsTraitActive(TraitType.BounceTerrain))

                        /*  3 */ new( OpCodeSet.Ldloc                          ), // characterDownStrike_RL
                        /*  4 */ new( OpCodes.Ldfld, name: "m_firedProjectile" ), // characterDownStrike_RL.m_firedProjectile
                        /*  5 */ new( OpCodes.Ldc_I4_1                         ), // true
                        /*  6 */ new( OpCodes.Callvirt, name: "set_CanHitWall" ), // characterDownStrike_RL.m_firedProjectile.CanHitWall = true
                            
                        /*  7 */ new( OpCodeSet.Br                             ), // else

                        /*  8 */ new( OpCodeSet.Ldloc                          ), // characterDownStrike_RL
                        /*  9 */ new( OpCodes.Ldfld, name: "m_firedProjectile" ), // characterDownStrike_RL.m_firedProjectile
                        /* 10 */ new( OpCodes.Ldc_I4_0                         ), // false
                        /* 11 */ new( OpCodes.Callvirt, name: "set_CanHitWall" ), // characterDownStrike_RL.m_firedProjectile.CanHitWall = false
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetInstruction( 10, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => Check_ClownBounce() ) ), // Change false -> call method that returns bool
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool Check_ClownBounce() {
                ClassType playerClass = SaveManager.PlayerSaveData.CurrentCharacter.ClassType;
                return classKeys.Exists( playerClass ) && WobSettings.Get( classKeys.Get( playerClass, "ClownBounce" ), false );
            }
        }

        // Bard - Spin Kicks stack Dance, up to 5 times
        [HarmonyPatch( typeof( LuteWeaponProjectileLogic ), "OnExplosionCollision" )]
        internal static class LuteWeaponProjectileLogic_OnExplosionCollision_Patch {
            internal static void Postfix( Projectile_RL projectile, GameObject colliderObj ) { ApplyDance( projectile, colliderObj ); }
        }
        [HarmonyPatch( typeof( CharacterDownStrike_RL ), nameof( CharacterDownStrike_RL.Bounce ) )]
        internal static class CharacterDownStrike_RL_Bounce_Patch {
            internal static void Postfix( Projectile_RL projectile, GameObject colliderObj ) { ApplyDance( projectile, colliderObj ); }
        }
        internal static void ApplyDance( Projectile_RL projectile, GameObject colliderObj ) {
            // Bard already adds stacks, so just add when playing another class
            Collider2D lastCollidedWith = projectile.HitboxController.LastCollidedWith;
            if( lastCollidedWith.CompareTag( "Enemy" ) || colliderObj.CompareTag( "Enemy" ) ) {
                if( PlayerManager.GetPlayerController().CharacterClass.ClassType != ClassType.LuteClass && SaveManager.PlayerSaveData.GetRelic( RelicType.DanceStacks ).Level == 0 ) {
                    PlayerManager.GetPlayerController().StatusEffectController.StartStatusEffect( StatusEffectType.Player_Dance, 0f, projectile );
                }
            }
        }
        [HarmonyPatch( typeof( DanceStatusEffect ), "StartEffectCoroutine" )]
        internal static class DanceStatusEffect_StartEffectCoroutine_Patch {
            // Find the correct method - this is an implicitly defined method
            // 'StartEffectCoroutine' returns an IEnumerator, and we need to patch the 'MoveNext' method on that
            internal static MethodInfo TargetMethod() {
                // Find the nested class of 'DanceStatusEffect' that 'StartEffectCoroutine' implicitly created
                System.Type type = AccessTools.FirstInner( typeof( DanceStatusEffect ), t => t.Name.Contains( "<StartEffectCoroutine>d__" ) );
                // Find the 'MoveNext' method on the nested class
                return AccessTools.FirstMethod( type, method => method.Name.Contains( "MoveNext" ) );
            }

            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "DanceStatusEffect.StartEffectCoroutine" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodeSet.Ldloc                              ), // playerController
                        /*  2 */ new( OpCodes.Callvirt, name: "get_CharacterClass" ), // playerController.CharacterClass
                        /*  3 */ new( OpCodes.Callvirt, name: "get_ClassType"      ), // playerController.CharacterClass.ClassType
                        /*  4 */ new( OpCodes.Ldc_I4, ClassType.LuteClass          ), // ClassType.LuteClass
                        /*  5 */ new( OpCodeSet.Bne_Un                             ), // if (playerController.CharacterClass.ClassType == ClassType.LuteClass)
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_Remove( 0, 5 ), // Remove the conditional, apply for all classes
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Limitless trait - mana has infinite overcharge
        [HarmonyPatch( typeof( PlayerController ), nameof( PlayerController.SetMana ) )]
        internal static class PlayerController_SetMana_Patch {
            internal static void Prefix( ref bool canExceedMax ) {
                ClassType playerClass = SaveManager.PlayerSaveData.CurrentCharacter.ClassType;
                if( classKeys.Exists( playerClass ) && WobSettings.Get( classKeys.Get( playerClass, "LimitlessMana" ), false ) ) {
                    canExceedMax = true;
                }
            }
        }

        // Limitless trait - mana has infinite overcharge
        [HarmonyPatch( typeof( ManaRegen ), "LateUpdate" )]
            internal static class ManaRegen_LateUpdate_Patch {
                internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) { return CommonTranspiler( instructions, "ManaRegen.LateUpdate" );
            }
        }
        [HarmonyPatch( typeof( BossTunnel ), "RegenHealthAndMana" )]
            internal static class BossTunnel_RegenHealthAndMana_Patch {
                internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) { return CommonTranspiler( instructions, "BossTunnel.RegenHealthAndMana" );
            }
        }
        [HarmonyPatch( typeof( ManaDrop ), "Collect" )]
            internal static class ManaDrop_Collect_Patch {
                internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) { return CommonTranspiler( instructions, "ManaDrop.Collect" );
            }
        }
        [HarmonyPatch( typeof( PlayerHitResponse ), "CharacterDamaged" )]
            internal static class PlayerHitResponse_CharacterDamaged_Patch {
                internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) { return CommonTranspiler( instructions, "PlayerHitResponse.CharacterDamaged" );
            }
        }

        // Common transpiler for checking if the Limitless trait is active
        private static IEnumerable<CodeInstruction> CommonTranspiler( IEnumerable<CodeInstruction> instructions, string methodName ) {
            // Set up the transpiler handler with the instruction list
            WobTranspiler transpiler = new( instructions, methodName );
            // Perform the patching
            transpiler.PatchAll(
                // Define the IL code instructions that should be matched
                new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldc_I4, TraitType.NoManaCap ), // TraitType.NoManaCap
                        /*  1 */ new( OpCodes.Call, name: "IsTraitActive" ), // TraitManager.IsTraitActive(TraitType.NoManaCap)
                },
                // Define the actions to take when an occurrence is found
                new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetInstruction( 1, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => PatchCheckNoManaCap( TraitType.NoManaCap ) ) ),
                }, expected: 1 );
            // Return the modified instructions
            return transpiler.GetResult();
        }

        // Patched method used in the common transpiler
        private static bool PatchCheckNoManaCap( TraitType traitType ) {
            ClassType playerClass = SaveManager.PlayerSaveData.CurrentCharacter.ClassType;
            return TraitManager.IsTraitActive( traitType ) || ( classKeys.Exists( playerClass ) && WobSettings.Get( classKeys.Get( playerClass, "LimitlessMana" ), false ) );
        }

    }
}
