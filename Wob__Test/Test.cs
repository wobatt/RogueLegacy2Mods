using System;
using System.Collections.Generic;
using System.Linq;
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
                foreach( EquipmentType type in EquipmentType_RL.TypeArray ) {
                    if( type != EquipmentType.None ) {
                        EquipmentSetData setData = EquipmentSetLibrary.GetEquipmentSetData( type );
                        if( setData != null ) {
                            WobPlugin.Log( "~1~  " + type + "|" + setData.Name + "|"
                                    + setData.SetRequirement01 + "|" + setData.SetBonus01.BonusType + "|" + setData.SetBonus01.StatGain + "|"
                                    + setData.SetRequirement02 + "|" + setData.SetBonus02.BonusType + "|" + setData.SetBonus02.StatGain + "|"
                                    + setData.SetRequirement03 + "|" + setData.SetBonus03.BonusType + "|" + setData.SetBonus03.StatGain + "|"
                                    + LocalizationManager.GetString( setData.Title, false ) );
                        }
                    }
                }
                foreach( EquipmentType type in EquipmentType_RL.TypeArray ) {
                    if( type != EquipmentType.None ) {
                        foreach( EquipmentCategoryType catType in EquipmentType_RL.CategoryTypeArray ) {
                            if( catType != EquipmentCategoryType.None ) {
                                EquipmentData data = EquipmentLibrary.GetEquipmentData( catType, type );
                                if( data != null && !data.Disabled ) {
                                    WobPlugin.Log( "~2~  " + type + "|" + catType + "|" + data.Name + "|" + data.BlacksmithUIIndex + "|" 
                                        + data.ChestLevelRequirement + "|" + data.ChestRarityRequirement + "|"
                                        + data.MaximumLevel + "|" + data.ScalingItemLevel + "|" 
                                        + data.BaseEquipmentSetLevel + "|" + data.ScalingEquipmentSetLevel + "|"
                                        + data.GoldCost + "|" + data.ScalingGoldCost + "|" 
                                        + data.OreCost + "|" + data.ScalingOreCost + "|" 
                                        + data.BaseWeight + "|" + data.ScalingWeight + "|" 
                                        + data.BaseHealth + "|" + data.ScalingHealth + "|" 
                                        + data.BaseMana + "|" + data.ScalingMana + "|" 
                                        + data.BaseArmor + "|" + data.ScalingArmor + "|" 
                                        + data.BaseStrengthDamage + "|" + data.ScalingStrengthDamage + "|" 
                                        + data.BaseMagicDamage + "|" + data.ScalingMagicDamage + "|" 
                                        + data.BaseStrengthCritDamage + "|" + data.ScalingStrengthCritDamage + "|" 
                                        + data.BaseMagicCritDamage + "|" + data.ScalingMagicCritDamage + "|" 
                                        + data.BaseStrengthCritChance + "|" + data.ScalingStrengthCritChance + "|" 
                                        + data.BaseMagicCritChance + "|" + data.ScalingMagicCritChance + "|" 
                                        + LocalizationManager.GetString( data.Title, false ) );
                                }
                            }
                        }
                    }
                }
            }
        }*/


    }
}