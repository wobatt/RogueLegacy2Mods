using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using MoreMountains.CorgiEngine;
using UnityEngine;
using Wob_Common;

namespace Wob__Test {
    [BepInPlugin( "Wob._Test", "Wob's Test Mod", "0.1.0" )]
    public partial class Test : BaseUnityPlugin {
        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options



            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // This patch simply dumps skill tree data to the debug log when the Manor skill tree is opened - useful for getting internal names and default values for the upgrades
        /*[HarmonyPatch( typeof( SkillTreeWindowController ), nameof( SkillTreeWindowController.Initialize ) )]
        internal static class SkillTreeWindowController_Initialize_Patch {
            internal static void Postfix() {
                foreach( BurdenType burdenType in BurdenType_RL.TypeArray ) {
                    if( burdenType != BurdenType.None ) {
                        BurdenData burdenData = BurdenLibrary.GetBurdenData( burdenType );
                        if( burdenData != null && !burdenData.Disabled ) {
                            WobPlugin.Log( "~~  " + burdenType + "|" + burdenData.MaxBurdenLevel + "|" + burdenData.InitialBurdenCost + "|" + burdenData.StatsGain + "|" + LocalizationManager.GetString( burdenData.Title, false, false ) + "|" + LocalizationManager.GetString( burdenData.Description2, false, false ) );
                        }
                    }
                }
            }
        }*/

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        [HarmonyPatch( typeof( CaveLanternPostProcessingController ), "DarknessAmountWhenFullyLit", MethodType.Getter )]
        internal static class CaveLanternPostProcessingController_DarknessAmountWhenFullyLit_Patch {
            internal static void Postfix( ref float __result ) {
                __result = 0f;
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        [HarmonyPatch( typeof( RelicObj ), nameof( RelicObj.WasSeen ), MethodType.Getter )]
        internal static class RelicObj_WasSeen_Patch {
            internal static void Postfix( RelicObj __instance, ref bool __result ) {
                if( !__result ) {
                    Traverse.Create( __instance ).Field( "m_wasSeen" ).SetValue( true );
                    __result = true;
                }
            }
        }
        
        [HarmonyPatch( typeof( PlayerSaveData ), nameof( PlayerSaveData.GetSpellSeenState ) )]
        internal static class PlayerSaveData_GetSpellSeenState_Patch {
            internal static void Postfix( PlayerSaveData __instance, AbilityType spellType, ref bool __result ) {
                if( !__result ) {
                    __instance.SetSpellSeenState( spellType, true );
                    __result = true;
                }
            }
        }
        
        [HarmonyPatch( typeof( TraitManager ), nameof( TraitManager.GetTraitSeenState ) )]
        internal static class TraitManager_GetTraitFoundState_Patch {
            internal static void Postfix( TraitType traitType, ref TraitSeenState __result ) {
                if( __result < TraitSeenState.SeenTwice ) {
                    TraitManager.SetTraitSeenState( traitType, TraitSeenState.SeenTwice, false );
                    __result = TraitSeenState.SeenTwice;
                }
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        // Helper to get the UI names of a trait
        /*public static string GetTraitTitles( TraitData traitData ) {
            // Each trait has 4 possible names - scientific/non-scientific and male/female character
            // First get all 4 variants
            string tScientificM = LocalizationManager.GetString( traitData.Title, false, false );
            string tScientificF = LocalizationManager.GetString( traitData.Title, true, false );
            string tNonScientificM = LocalizationManager.GetString( traitData.Title.Replace( "_1", "_2" ), false, false );
            string tNonScientificF = LocalizationManager.GetString( traitData.Title.Replace( "_1", "_2" ), true, false );
            // Build a return string, suppressing variants if they are the same as one already added
            return tScientificM + ( tScientificM == tScientificF ? "" : "/" + tScientificF ) + ( tScientificM == tNonScientificM ? "" : "/" + tNonScientificM ) + ( ( tNonScientificM == tNonScientificF || tScientificF == tNonScientificF ) ? "" : "/" + tNonScientificF );
        }*/
    }
}