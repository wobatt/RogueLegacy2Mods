using BepInEx;
using HarmonyLib;
using Wob_Common;

namespace Wob_SoulShopCost {
    [BepInPlugin( "Wob.SoulShopCost", "Soul Shop Cost Mod", "0.1.0" )]
    public partial class BepInExPlugin : BaseUnityPlugin {
        // Configuration file entries, globally accessible for patches
        public static ConfigItem<float> configSoulCost;

        // Main method that kicks everything off
        private void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            configSoulCost = new ConfigItem<float>( this.Config, "Options", "CostScaler", "Multiply soul shop costs by this value", 1f, 0, float.MaxValue );
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

        /*[HarmonyPatch( typeof( SoulShopObj ), nameof( SoulShopObj.InitialCost ), MethodType.Getter )]
        static class SoulShopObj_InitialCost_Patch {
            private static bool runOnce = false;
            static void Prefix() {
                if( !runOnce ) {
                    foreach( SoulShopType shopType in SoulShopType_RL.TypeArray ) {
                        SoulShopData shopObj = SoulShopLibrary.GetSoulShopData( shopType );
                        if( shopObj != null ) {
                            WobPlugin.Log( shopType + "|" + shopObj.BaseCost + "|" + shopObj.ScalingCost );
                            shopObj.BaseCost = (int)System.Math.Floor( shopObj.BaseCost * configSoulCost.Value );
                            shopObj.ScalingCost = (int)System.Math.Floor( shopObj.ScalingCost * configSoulCost.Value );
                        }
                    }
                    runOnce = true;
                }
            }
        }*/
    }
}