using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace Wob_EquipStats {
    [BepInPlugin( "Wob.EquipStats", "Equipment Stats Mod", "0.1.0" )]
    public partial class EquipStats : BaseUnityPlugin {

        private static readonly WobSettings.KeyHelper<RuneType> keys = new WobSettings.KeyHelper<RuneType>( "Equip" );

        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            for( int i = 0; i < Economy_EV.EQUIPMENT_LEVEL_GOLD_MOD.Length; i++ ) {
                WobSettings.Add( new WobSettings.Num<int>( "ScalingGoldCost", "GoldCostMult_Level" + ( i + 1 ), "Multiply base gold cost by this value to get the equipment cost for level " + ( i + 1 ), Economy_EV.EQUIPMENT_LEVEL_GOLD_MOD[i], bounds: (1, 1000000) ) );
            }
            for( int i = 0; i < Economy_EV.EQUIPMENT_LEVEL_ORE_MOD.Length; i++ ) {
                WobSettings.Add( new WobSettings.Num<int>( "ScalingOreCost", "OreCostMult_Level" + ( i + 1 ), "Multiply base ore cost by this value to get the equipment cost for level " + ( i + 1 ), Economy_EV.EQUIPMENT_LEVEL_ORE_MOD[i], bounds: (1, 1000000) ) );
            }
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        // Patch to change scaling gold costs
        [HarmonyPatch( typeof( EquipmentObj ), nameof( EquipmentObj.GoldCostToUpgrade ), MethodType.Getter )]
        internal static class EquipmentObj_GoldCostToUpgrade_Patch {
            private static bool runOnce = false;
            internal static void Prefix() {
                if( !runOnce ) {
                    for( int i = 0; i < Economy_EV.EQUIPMENT_LEVEL_GOLD_MOD.Length; i++ ) {
                        Economy_EV.EQUIPMENT_LEVEL_GOLD_MOD[i] = WobSettings.Get( "ScalingGoldCost", "GoldCostMult_Level" + ( i + 1 ), Economy_EV.EQUIPMENT_LEVEL_GOLD_MOD[i] );
                    }
                    runOnce = true;
                }
            }
        }

        // Patch to change scaling ore costs
        [HarmonyPatch( typeof( EquipmentObj ), nameof( EquipmentObj.OreCostToUpgrade ), MethodType.Getter )]
        internal static class EquipmentObj_OreCostToUpgrade_Patch {
            private static bool runOnce = false;
            internal static void Prefix() {
                if( !runOnce ) {
                    for( int i = 0; i < Economy_EV.EQUIPMENT_LEVEL_ORE_MOD.Length; i++ ) {
                        Economy_EV.EQUIPMENT_LEVEL_ORE_MOD[i] = WobSettings.Get( "ScalingOreCost", "OreCostMult_Level" + ( i + 1 ), Economy_EV.EQUIPMENT_LEVEL_ORE_MOD[i] );
                    }
                    runOnce = true;
                }
            }
        }
    }
}