using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_RandomRolls {
    [BepInPlugin( "Wob.RandomRolls", "Random Rolls Mod", "0.1.0" )]
    public partial class Test : BaseUnityPlugin {
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Num<int>( "TwinRelicResolve", "Always spawn twin relics if you have this amount of total resolve (use -1 to disable)", 500, 0.01f, bounds: (-1, 1000000) ),
                new WobSettings.Boolean( "UnlockedOnly",   "Limit random weapons and talents to be from classes or variants that have been unlocked in the upgrade tree or soul shop", false ),
                new WobSettings.Boolean( "EqualWeighting", "Give equal chance to roll all weapons, rather than the default more likely to roll soul shop variant weapons",             false ),
            } );
            UnlockedOnly_Enabled  = WobSettings.Get( "UnlockedOnly",   false );
            UnlockedOnly_Distinct = WobSettings.Get( "EqualWeighting", false );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        [HarmonyPatch( typeof( RelicRoomPropController ), "RollRelicMod" )]
        internal static class RelicRoomPropController_RollRelicMod_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                WobPlugin.Log( "RelicRoomPropController.RollRelicMod Transpiler Patch" );
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new WobTranspiler( instructions );
                // Perform the patching
                transpiler.PatchAll(
                        // Define the IL code instructions that should be matched
                        new List<WobTranspiler.OpTest> {
							// if (TraitManager.IsTraitActive(TraitType.TwinRelics))
                            /*  0 */ new WobTranspiler.OpTest( OpCodes.Ldc_I4, TraitType.TwinRelics ), // TraitType.TwinRelics
                            /*  1 */ new WobTranspiler.OpTest( OpCodes.Call, name: "IsTraitActive"  ), // TraitManager.IsTraitActive(TraitType.TwinRelics)
                            /*  2 */ new WobTranspiler.OpTest( OpCodeSet.Brfalse                    ), // if (TraitManager.IsTraitActive(TraitType.TwinRelics))
                        },
                        // Define the actions to take when an occurrence is found
                        new List<WobTranspiler.OpAction> {
                                new WobTranspiler.OpAction_SetOperand( 1, SymbolExtensions.GetMethodInfo( () => IsTraitActiveOrOverride( TraitType.None ) ) ),
                        } );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static bool IsTraitActiveOrOverride( TraitType traitType ) {
                float config = WobSettings.Get( "TwinRelicResolve", -1f );
                return TraitManager.IsTraitActive( traitType ) || ( config >= 0 && GetResolveTotal() > config );
            }

            private static float GetResolveTotal() {
                float current = 1f + PlayerManager.GetPlayerController().ResolveAdd;
                float spent = SaveManager.PlayerSaveData.GetTotalRelicResolveCost();
                float total = current + spent;
                WobPlugin.Log( "~~  Resolve = " + current + " + " + spent + " = " + total );
                return total;
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        internal static bool UnlockedOnly_Enabled = false;
        internal static bool UnlockedOnly_Distinct = false;

        private static readonly Dictionary<AbilityType,SkillTreeType> ClassWeapons = new Dictionary<AbilityType,SkillTreeType>() {
            { AbilityType.AstroWandWeapon,   SkillTreeType.Astro_Class_Unlock       },
            { AbilityType.AxeWeapon,         SkillTreeType.Axe_Class_Unlock         },
            { AbilityType.BowWeapon,         SkillTreeType.Bow_Class_Unlock         },
            { AbilityType.BoxingGloveWeapon, SkillTreeType.BoxingGlove_Class_Unlock },
            { AbilityType.CannonWeapon,      SkillTreeType.Pirate_Class_Unlock      },
            { AbilityType.DualBladesWeapon,  SkillTreeType.DualBlades_Class_Unlock  },
            { AbilityType.FryingPanWeapon,   SkillTreeType.Ladle_Class_Unlock       },
            { AbilityType.KatanaWeapon,      SkillTreeType.Samurai_Class_Unlock     },
            { AbilityType.LanceWeapon,       SkillTreeType.Lancer_Class_Unlock      },
            { AbilityType.LuteWeapon,        SkillTreeType.Music_Class_Unlock       },
            { AbilityType.MagicWandWeapon,   SkillTreeType.Wand_Class_Unlock        },
            { AbilityType.PistolWeapon,      SkillTreeType.Gun_Class_Unlock         },
            { AbilityType.SaberWeapon,       SkillTreeType.Saber_Class_Unlock       },
            { AbilityType.SpearWeapon,       SkillTreeType.Spear_Class_Unlock       },
        };
        private static readonly Dictionary<AbilityType,SoulShopType> VariantWeapons = new Dictionary<AbilityType,SoulShopType>() {
            { AbilityType.AxeSpinnerWeapon,     SoulShopType.AxeVariant    },
            { AbilityType.ExplosiveHandsWeapon, SoulShopType.BoxerVariant  },
            { AbilityType.GroundBowWeapon,      SoulShopType.ArcherVariant },
            { AbilityType.SpoonsWeapon,         SoulShopType.LadleVariant  },
            { AbilityType.ChakramWeapon,        SoulShopType.SwordVariant  },
            { AbilityType.ScytheWeapon,         SoulShopType.WandVariant   },
            { AbilityType.KineticBowWeapon,     SoulShopType.LuteVariant   },
        };
        private static readonly Dictionary<AbilityType,SkillTreeType> ClassTalents = new Dictionary<AbilityType,SkillTreeType>() {
            { AbilityType.CloakTalent,          SkillTreeType.DualBlades_Class_Unlock  },
            { AbilityType.CometTalent,          SkillTreeType.Astro_Class_Unlock       },
            { AbilityType.CookingTalent,        SkillTreeType.Ladle_Class_Unlock       },
            { AbilityType.CreatePlatformTalent, SkillTreeType.Bow_Class_Unlock         },
            { AbilityType.CrescendoTalent,      SkillTreeType.Music_Class_Unlock       },
            { AbilityType.CrowsNestTalent,      SkillTreeType.Pirate_Class_Unlock      },
            { AbilityType.KnockoutTalent,       SkillTreeType.BoxingGlove_Class_Unlock },
            { AbilityType.ManaBombTalent,       SkillTreeType.Gun_Class_Unlock         },
            { AbilityType.RollTalent,           SkillTreeType.Saber_Class_Unlock       },
            { AbilityType.ShoutTalent,          SkillTreeType.Axe_Class_Unlock         },
            { AbilityType.SpearSpinTalent,      SkillTreeType.Spear_Class_Unlock       },
            { AbilityType.StaticWallTalent,     SkillTreeType.Lancer_Class_Unlock      },
            { AbilityType.TeleSliceTalent,      SkillTreeType.Samurai_Class_Unlock     },
        };

        [HarmonyPatch( typeof( SwapAbilityRoomPropController ), "GetAbilityArray" )]
        internal static class SwapAbilityRoomPropController_GetAbilityArray_Patch {
            internal static void Postfix( CastAbilityType castAbilityType, ref AbilityType[] __result ) {
                if( !UnlockedOnly_Enabled ) { return; }
                if( castAbilityType == CastAbilityType.Spell ) { return; }
                List<AbilityType> newAbilityList = new List<AbilityType>();
                foreach( AbilityType ability in __result ) {
                    bool add = true;
                    if( castAbilityType == CastAbilityType.Weapon ) {
                        if( ClassWeapons.TryGetValue( ability, out SkillTreeType skillTreeType ) ) {
                            add = SkillTreeManager.GetSkillObjLevel( skillTreeType ) > 0;
                        }
                        if( VariantWeapons.TryGetValue( ability, out SoulShopType soulShopType ) ) {
                            add = SaveManager.ModeSaveData.GetSoulShopObj( soulShopType ).CurrentOwnedLevel > 0;
                        }
                    } else {
                        if( ClassTalents.TryGetValue( ability, out SkillTreeType skillTreeType ) ) {
                            add = SkillTreeManager.GetSkillObjLevel( skillTreeType ) > 0;
                        }
                    }
                    if( add ) {
                        if( !UnlockedOnly_Distinct || !newAbilityList.Contains( ability ) ) {
                            newAbilityList.Add( ability );
                        } else {
                            WobPlugin.Log( "Preventing spawn of " + ability + " - no duplicates" );
                        }
                    } else {
                        WobPlugin.Log( "Preventing spawn of " + ability + " - not unlocked" );
                    }
                }
                __result = newAbilityList.ToArray();
            }
        }

        [HarmonyPatch( typeof( CharacterCreator ), nameof( CharacterCreator.GetAvailableWeapons ) )]
        internal static class CharacterCreator_GetAvailableWeapons_Patch {
            internal static void Postfix( ClassType classType, ref AbilityType[] __result ) {
                if( !UnlockedOnly_Enabled ) { return; }
                if( classType == ClassType.CURIO_SHOPPE_CLASS ) {
                    List<AbilityType> newAbilityList = new List<AbilityType>();
                    foreach( AbilityType ability in __result ) {
                        bool add = true;
                        if( ClassWeapons.TryGetValue( ability, out SkillTreeType skillTreeType ) ) {
                            add = SkillTreeManager.GetSkillObjLevel( skillTreeType ) > 0;
                        }
                        if( VariantWeapons.TryGetValue( ability, out SoulShopType soulShopType ) ) {
                            add = SaveManager.ModeSaveData.GetSoulShopObj( soulShopType ).CurrentOwnedLevel > 0;
                        }
                        if( add ) {
                            if( !UnlockedOnly_Distinct || !newAbilityList.Contains( ability ) ) {
                                newAbilityList.Add( ability );
                            } else {
                                WobPlugin.Log( "Preventing spawn of " + ability + " - no duplicates" );
                            }
                        } else {
                            WobPlugin.Log( "Preventing spawn of " + ability + " - not unlocked" );
                        }
                    }
                    __result = newAbilityList.ToArray();
                }
            }
        }

        [HarmonyPatch( typeof( CharacterCreator ), nameof( CharacterCreator.GetAvailableTalents ) )]
        internal static class CharacterCreator_GetAvailableTalents_Patch {
            internal static void Postfix( ClassType classType, ref AbilityType[] __result ) {
                if( !UnlockedOnly_Enabled ) { return; }
                if( classType == ClassType.CURIO_SHOPPE_CLASS ) {
                    List<AbilityType> newAbilityList = new List<AbilityType>();
                    foreach( AbilityType ability in __result ) {
                        bool add = true;
                        if( ClassTalents.TryGetValue( ability, out SkillTreeType skillTreeType ) ) {
                            add = SkillTreeManager.GetSkillObjLevel( skillTreeType ) > 0;
                        }
                        if( add ) {
                            if( !UnlockedOnly_Distinct || !newAbilityList.Contains( ability ) ) {
                                newAbilityList.Add( ability );
                            } else {
                                WobPlugin.Log( "Preventing spawn of " + ability + " - no duplicates" );
                            }
                        } else {
                            WobPlugin.Log( "Preventing spawn of " + ability + " - not unlocked" );
                        }
                    }
                    __result = newAbilityList.ToArray();
                }
            }
        }
    }
}