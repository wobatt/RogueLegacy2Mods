using BepInEx;
using Wob_Common;

namespace WobMod {
    [BepInPlugin( "WobMod", "Wob's RL2 Configuration Mod", "2.0.0" )]
    public class WobMod : BaseUnityPlugin {

        // Registry of config files to organise user settings
        internal static readonly WobSettings.FileHelper<string> configFiles = new();

        // Main method that kicks everything off
        protected void Awake() {
            // Set up the logger and basic config items
            WobPlugin.Initialise( this, this.Logger );
            // Create/read the mod specific configuration options
            Abilities.RunSetup();
            CastleSkills.RunSetup();
            Chests.RunSetup();
            Classes.RunSetup();
            Equipment.RunSetup();
            GameRules.RunSetup();
            NewGamePlus.RunSetup();
            Relics.RunSetup();
            Runes.RunSetup();
            Scars.RunSetup();
            SoulShop.RunSetup();
            Traits.RunSetup();
            Projectiles.RunSetup(); // Must run AFTER Abilities and Relics
            // Apply the patches if the mod is enabled
            WobPlugin.Patch();
        }

    }

}

namespace System.Runtime.CompilerServices {
    internal static class IsExternalInit { }
}
