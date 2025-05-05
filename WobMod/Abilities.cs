using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Wob_Common;

namespace WobMod {
    internal class Abilities {

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTING DEFAULTS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        internal record AbilityInfo( string Config, string Name, CastAbilityType Slot, bool CDIsTime, bool CDIsHits, float CDAmount, int MaxAmmo, int Mana, string Description );
        internal static readonly Dictionary<AbilityType, AbilityInfo> abilityInfo = new() {
            // Standard class weapons
            { AbilityType.DualBladesWeapon,     new( "Assassin_DualBlades",       "Assassin's Dual Blades",       CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.AstroWandWeapon,      new( "Astromancer_Sceptre",       "Astromancer's Sceptre",        CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.AxeWeapon,            new( "Barbarian_Labrys",          "Barbarian's Labrys",           CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.LuteWeapon,           new( "Bard_Lute",                 "Bard's Lute",                  CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.BoxingGloveWeapon,    new( "Boxer_BoxingGloves",        "Boxer's Boxing Gloves",        CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.FryingPanWeapon,      new( "Chef_FryingPan",            "Chef's Frying Pan",            CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.LanceWeapon,          new( "DragonLancer_ChargeLance",  "DragonLancer's Charge Lance",  CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.SaberWeapon,          new( "Duelist_Saber",             "Duelist's Saber",              CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.PistolWeapon,         new( "Gunslinger_Revolver",       "Gunslinger's Revolver",        CastAbilityType.Weapon, true,  false, 0f,  22, 0,   ""                                                                                                                                    ) },
            { AbilityType.SwordWeapon,          new( "Knight_GreatSword",         "Knight's Great Sword",         CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.MagicWandWeapon,      new( "Mage_WandOfBlasting",       "Mage's Wand of Blasting",      CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.CannonWeapon,         new( "Pirate_Cannon",             "Pirate's Cannon",              CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.BowWeapon,            new( "Ranger_WarBow",             "Ranger's War Bow",             CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.KatanaWeapon,         new( "Ronin_Katana",              "Ronin's Katana",               CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.SpearWeapon,          new( "Valkyrie_Fauchard",         "Valkyrie's Fauchard",          CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            // Soul shop unlock variant weapons
            { AbilityType.TonfaWeapon,          new( "Assassin_DragonFangs",      "Assassin's Dragon Fangs",      CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.CrowStormWeapon,      new( "Astromancer_CrowStorm",     "Astromancer's Crow Storm",     CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.AxeSpinnerWeapon,     new( "Barbarian_Hammer",          "Barbarian's Hammer",           CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.KineticBowWeapon,     new( "Bard_ElectricLute",         "Bard's Electric Lute",         CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.ExplosiveHandsWeapon, new( "Boxer_EnkindledGauntlets",  "Boxer's Enkindled Gauntlets",  CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.SpoonsWeapon,         new( "Chef_BagOfSpoons",          "Chef's Bag o' Spoons",         CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.DragonAspectWeapon,   new( "DragonLancer_DragonPuppet", "DragonLancer's Dragon Puppet", CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) }, // DragonPistolWeapon?
            { AbilityType.SniperWeapon,         new( "Duelist_Triangulator",      "Duelist's Triangulator",       CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.ShotgunWeapon,        new( "Gunslinger_Blunderbuss",    "Gunslinger's Blunderbuss",     CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.ChakramWeapon,        new( "Knight_PepperoniPizza",     "Knight's Pepperoni Pizza",     CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.ScytheWeapon,         new( "Mage_CharonsScythe",        "Mage's Charon's Scythe",       CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.SurfboardWeapon,      new( "Pirate_Surfboard",          "Pirate's Surfboard",           CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.GroundBowWeapon,      new( "Ranger_HandheldBallista",   "Ranger's Handheld Ballista",   CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.UmbrellaWeapon,       new( "Ronin_Kasaobake",           "Ronin's Kasa-obake",           CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            { AbilityType.HammerWeapon,         new( "Valkyrie_Mjolnir",          "Valkyrie's Mjolnir",           CastAbilityType.Weapon, true,  false, 0f,  0,  0,   ""                                                                                                                                    ) },
            // Class talents
            { AbilityType.CloakTalent,          new( "Assassin_Obscura",          "Assassin's Obscura",           CastAbilityType.Talent, false, true,  10f, 0,  0,   "Creates a cloak that avoids all damage. Attacking will apply Vulnerable and end the effect. Recharges on hit."                       ) },
            { AbilityType.CometTalent,          new( "Astromancer_CometForm",     "Astromancer's Comet Form",     CastAbilityType.Talent, true,  false, 6f,  0,  0,   "Gain Flight and Damage Immunity. Recharges over time."                                                                               ) },
            { AbilityType.ShoutTalent,          new( "Barbarian_WintersShout",    "Barbarian's Winter's Shout",   CastAbilityType.Talent, false, false, 1f,  0,  0,   "Destroys Large Projectiles, and Freezes enemies. Recharges after getting hit."                                                       ) },
            { AbilityType.CrescendoTalent,      new( "Bard_Crescendo",            "Bard's Crescendo",             CastAbilityType.Talent, true,  false, 12f, 0,  0,   "Shout, converting all Mid-sized Projectiles into notes. Recharges over time."                                                        ) },
            { AbilityType.KnockoutTalent,       new( "Boxer_KnockoutPunch",       "Boxer's Knockout Punch",       CastAbilityType.Talent, false, true,  3f,  0,  0,   "Can be aimed. Consumes all Combo stacks. Damage increased for every consumed stack. Recharges on hit."                               ) },
            { AbilityType.CookingTalent,        new( "Chef_Stew",                 "Chef's Stew",                  CastAbilityType.Talent, false, false, 1f,  3,  0,   "Restores your Health and Mana. Recharges by collecting Health Drops (Holds up to 3 charges)."                                        ) },
            { AbilityType.StaticWallTalent,     new( "DragonLancer_Bastion",      "DragonLancer's Bastion",       CastAbilityType.Talent, false, true,  6f,  0,  0,   "Destroys Large Projectiles and pushes enemies back. Recharges on hit."                                                               ) },
            { AbilityType.RollTalent,           new( "Duelist_CombatRoll",        "Duelist's Combat Roll",        CastAbilityType.Talent, true,  false, 2f,  0,  0,   "Immune to all damage while Rolling. Recharges over time."                                                                            ) },
            { AbilityType.ManaBombTalent,       new( "Gunslinger_Explosive",      "Gunslinger's Explosive",       CastAbilityType.Talent, true,  false, 8f,  2,  0,   "Blocks Mid-sized Projectiles (holds 2 charges). Recharges over time (refills all charges)."                                          ) },
            { AbilityType.ShieldBlockTalent,    new( "Knight_ShieldBlock",        "Knight's Shield Block",        CastAbilityType.Talent, true,  false, 10f, 0,  0,   "Hold to block 50% of incoming damage and apply Vulnerable. Last second blocks prevent 100% of incoming damage. Recharges over time." ) },
            { AbilityType.CrowsNestTalent,      new( "Pirate_PirateShip",         "Pirate's Pirate Ship",         CastAbilityType.Talent, false, true,  8f,  0,  0,   "Creates a flying Pirate ship that you can freely move around. Shoots stuff and blows itself up. Recharges on hit."                   ) },
            { AbilityType.CreatePlatformTalent, new( "Ranger_IvyCanopy",          "Ranger's Ivy Canopy",          CastAbilityType.Talent, true,  false, 7f,  0,  0,   "Creates an ivy platform that blocks Mid-sized Projectiles and grants Spore Burst. Recharges over time."                              ) },
            { AbilityType.TeleSliceTalent,      new( "Ronin_ImmortalKotetsu",     "Ronin's Immortal Kotetsu",     CastAbilityType.Talent, true,  false, 5f,  0,  0,   "Hold to aim. Teleport a set distance, hitting everything in between. Recharges over time."                                           ) },
            { AbilityType.SpearSpinTalent,      new( "Valkyrie_Deflect",          "Valkyrie's Deflect",           CastAbilityType.Talent, false, true,  5f,  0,  0,   "Destroy all Mid-sized Projectiles, restoring Mana. Recharges on hit and fully recharges on successful counters."                     ) },
            // Trait talents
            { AbilityType.SuperFart,            new( "SuperIBS_SuperFart",        "Super IBS trait Super Fart",   CastAbilityType.Talent, true,  false, 3f,  0,  0,   "Lifts you and applies Burn. Recharges over time."                                                                                    ) },
            // Spells
            { AbilityType.FlameThrowerSpell,    new( "BlazeBellow",               "Blaze Bellow",                 CastAbilityType.Spell,  false, false, 0f,  0,  10,  ""                                                                                                                                    ) },
            { AbilityType.FireballSpell,        new( "Fireball",                  "Fireball",                     CastAbilityType.Spell,  false, false, 0f,  0,  50,  ""                                                                                                                                    ) },
            { AbilityType.FlameBarrierSpell,    new( "FlameBarrier",              "Flame Barrier",                CastAbilityType.Spell,  false, false, 0f,  0,  15,  ""                                                                                                                                    ) },
            { AbilityType.FreezeStrikeSpell,    new( "FreezeStrike",              "Freeze Strike",                CastAbilityType.Spell,  false, false, 0f,  0,  100, ""                                                                                                                                    ) },
            { AbilityType.SporeSpreadSpell,     new( "FungalSpread",              "Fungal Spread",                CastAbilityType.Spell,  false, false, 0f,  0,  75,  ""                                                                                                                                    ) },
            { AbilityType.GravityWellSpell,     new( "GravityBeam",               "Gravity Beam",                 CastAbilityType.Spell,  false, false, 0f,  0,  100, ""                                                                                                                                    ) },
            { AbilityType.LightningSpell,       new( "LightningStorm",            "Lightning Storm",              CastAbilityType.Spell,  false, false, 0f,  0,  50,  ""                                                                                                                                    ) },
            { AbilityType.AimLaserSpell,        new( "LucentBeam",                "Lucent Beam",                  CastAbilityType.Spell,  false, false, 0f,  0,  10,  ""                                                                                                                                    ) },
            { AbilityType.PoolBallSpell,        new( "Magic8Ball",                "Magic 8 Ball",                 CastAbilityType.Spell,  false, false, 0f,  0,  50,  ""                                                                                                                                    ) },
            { AbilityType.AxeSpell,             new( "MagmaMass",                 "Magma Mass",                   CastAbilityType.Spell,  false, false, 0f,  0,  50,  ""                                                                                                                                    ) },
            { AbilityType.PoisonBombSpell,      new( "PoisonBomb",                "Poison Bomb",                  CastAbilityType.Spell,  false, false, 0f,  0,  75,  ""                                                                                                                                    ) },
            { AbilityType.AilmentCurseSpell,    new( "PrismaticSpectrum",         "Prismatic Spectrum",           CastAbilityType.Spell,  false, false, 0f,  0,  100, ""                                                                                                                                    ) },
            { AbilityType.StraightBoltSpell,    new( "SearingShot",               "Searing Shot",                 CastAbilityType.Spell,  false, false, 0f,  0,  50,  ""                                                                                                                                    ) },
            { AbilityType.SnapSpell,            new( "ShieldOfThorns",            "Shield of Thorns",             CastAbilityType.Spell,  false, false, 0f,  0,  100, ""                                                                                                                                    ) },
            { AbilityType.EnergyBounceSpell,    new( "Shockwave",                 "Shockwave",                    CastAbilityType.Spell,  false, false, 0f,  0,  100, ""                                                                                                                                    ) },
            { AbilityType.DamageZoneSpell,      new( "TeslaSpike",                "Tesla Spike",                  CastAbilityType.Spell,  false, false, 0f,  0,  50,  ""                                                                                                                                    ) },
            { AbilityType.TimeBombSpell,        new( "WhiteStar",                 "White Star",                   CastAbilityType.Spell,  false, false, 0f,  0,  75,  ""                                                                                                                                    ) },
            { AbilityType.DamageWallSpell,      new( "WindWall",                  "Wind Wall",                    CastAbilityType.Spell,  false, false, 0f,  0,  100, ""                                                                                                                                    ) },
        };

        private record DefaultAbilities( AbilityType Fallback, System.Func<AbilityType[]> GetArray );
        private static readonly Dictionary<CastAbilityType, DefaultAbilities> defaultAbilities = new() {
            { CastAbilityType.Weapon, new( AbilityType.SwordWeapon,       () => AbilityType_RL.ValidWeaponTypeArray ) },
            { CastAbilityType.Talent, new( AbilityType.ShieldBlockTalent, () => AbilityType_RL.ValidTalentTypeArray ) },
            { CastAbilityType.Spell,  new( AbilityType.FireballSpell,     () => AbilityType_RL.ValidSpellTypeArray  ) },
        };

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTINGS READ/WRITE
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        internal static readonly WobSettings.KeyHelper<AbilityType> abilityKeys = new();

        private record AbilityCooldown( bool FromCast, bool UseTime, bool UseHits );
        private static readonly Dictionary<AbilityType, AbilityCooldown> abilityCooldowns = new();

        internal static void RunSetup() {
            WobMod.configFiles.Add( "Abilities." + CastAbilityType.Weapon, "Abilities.Weapons" );
            WobMod.configFiles.Add( "Abilities." + CastAbilityType.Talent, "Abilities.Talents" );
            WobMod.configFiles.Add( "Abilities." + CastAbilityType.Spell,  "Abilities.Spells"  );
            foreach( AbilityType abilityType in abilityInfo.Keys ) {
                abilityKeys.Add( abilityType, abilityInfo[abilityType].Slot.ToString(), abilityInfo[abilityType].Config );
                WobSettings.Add( new WobSettings.Boolean( WobMod.configFiles.Get( "Abilities." + abilityInfo[abilityType].Slot ), abilityKeys.Get( abilityType, "EnabledContrarian" ), abilityInfo[abilityType].Name + " is allowed in Contrarian trait random rolls", abilityType != AbilityType.SuperFart ) );
                WobSettings.Add( new WobSettings.Boolean( WobMod.configFiles.Get( "Abilities." + abilityInfo[abilityType].Slot ), abilityKeys.Get( abilityType, "EnabledCurioShop"  ), abilityInfo[abilityType].Name + " is allowed in Curio Shop random rolls",       abilityType != AbilityType.SuperFart ) );
                if( abilityInfo[abilityType].Slot == CastAbilityType.Spell ) {
                    WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "Abilities." + abilityInfo[abilityType].Slot ), abilityKeys.Get( abilityType, "ManaCost" ), "Mana cost for " + abilityInfo[abilityType].Name, abilityInfo[abilityType].Mana, bounds: (0, 1000) ) );
                }
                if( abilityInfo[abilityType].Slot == CastAbilityType.Talent ) {
                    WobSettings.Add( new WobSettings.Boolean( WobMod.configFiles.Get( "Abilities." + abilityInfo[abilityType].Slot ), abilityKeys.Get( abilityType, "CooldownUseTime" ), "Cooldown reduces over time for " + abilityInfo[abilityType].Name + " - " + abilityInfo[abilityType].Description, abilityInfo[abilityType].CDIsTime ) );
                    WobSettings.Add( new WobSettings.Boolean( WobMod.configFiles.Get( "Abilities." + abilityInfo[abilityType].Slot ), abilityKeys.Get( abilityType, "CooldownUseHits" ), "Cooldown reduces when hitting enemies for " + abilityInfo[abilityType].Name + " - " + abilityInfo[abilityType].Description, abilityInfo[abilityType].CDIsHits ) );
                    WobSettings.Add( new WobSettings.Num<float>( WobMod.configFiles.Get( "Abilities." + abilityInfo[abilityType].Slot ), abilityKeys.Get( abilityType, "CooldownAmount" ), "Cooldown seconds/hits/pickups for " + abilityInfo[abilityType].Name + " - " + abilityInfo[abilityType].Description, abilityInfo[abilityType].CDAmount, bounds: (0f, 1000f) ) );
                    if( abilityInfo[abilityType].MaxAmmo > 0 ) {
                        if( abilityType == AbilityType.PistolWeapon ) {
                            WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "Abilities." + abilityInfo[abilityType].Slot ), abilityKeys.Get( abilityType, "MaxAmmo" ), "Max ammo for " + abilityInfo[abilityType].Name + " - " + abilityInfo[abilityType].Description, abilityInfo[abilityType].MaxAmmo, bounds: (1, 99) ) );
                        } else {
                            WobSettings.Add( new WobSettings.Num<int>( WobMod.configFiles.Get( "Abilities." + abilityInfo[abilityType].Slot ), abilityKeys.Get( abilityType, "MaxAmmo" ), "Max charges (0 for infinite) for " + abilityInfo[abilityType].Name + " - " + abilityInfo[abilityType].Description, abilityInfo[abilityType].MaxAmmo, bounds: (0, 99) ) );
                        }
                    }
                }
            }
            WobSettings.Add( new WobSettings.Entry[] {
                new WobSettings.Boolean(    WobMod.configFiles.Get( "Abilities." + CastAbilityType.Talent ), abilityKeys.Get( AbilityType.CometTalent,          "CooldownFromCast" ), "Start the cooldown timer at the start of the effect, not the end.", false                     ),
                new WobSettings.Boolean(    WobMod.configFiles.Get( "Abilities." + CastAbilityType.Talent ), abilityKeys.Get( AbilityType.StaticWallTalent,     "CooldownFromCast" ), "Start the cooldown timer at the start of the effect, not the end.", false                     ),
                new WobSettings.Boolean(    WobMod.configFiles.Get( "Abilities." + CastAbilityType.Talent ), abilityKeys.Get( AbilityType.CrowsNestTalent,      "CooldownFromCast" ), "Start the cooldown timer at the start of the effect, not the end.", false                     ),
                new WobSettings.Boolean(    WobMod.configFiles.Get( "Abilities." + CastAbilityType.Talent ), abilityKeys.Get( AbilityType.CreatePlatformTalent, "CooldownFromCast" ), "Start the cooldown timer at the start of the effect, not the end.", false                     ),
                new WobSettings.Num<float>( WobMod.configFiles.Get( "Abilities." + CastAbilityType.Talent ), abilityKeys.Get( AbilityType.ShieldBlockTalent,    "PerfectBlockTime" ), "Time between starting blocking and impact to get a perfect block.", Ability_EV.SHIELD_BLOCK_PERFECT_TIMING, bounds: (0f, 60f) ),
                new WobSettings.Boolean(    WobMod.configFiles.Get( "Abilities." + CastAbilityType.Talent ), abilityKeys.Get( AbilityType.ManaBombTalent,       "RefreshAllAmmo"   ), "Refresh all charges on cooldown instead of just 1 charge.",         true                      ),
                new WobSettings.Boolean(    WobMod.configFiles.Get( "Abilities." + CastAbilityType.Weapon ), abilityKeys.Get( AbilityType.PistolWeapon,         "SkipReload"       ), "When the revolver fires its last shot, instantly reload it.",       false                     ),
                new WobSettings.Boolean(    WobMod.configFiles.Get( "Abilities." + CastAbilityType.Weapon ), abilityKeys.Get( AbilityType.ShotgunWeapon,        "SkipReload"       ), "When the blunderbuss fires its last shot, instantly reload it.",    false                     ),
            } );
            PistolWeapon_Ability_FireProjectile_Patch.Enabled = WobSettings.Get( abilityKeys.Get( AbilityType.PistolWeapon, "SkipReload" ), false );
            Shotgun_Ability_OnExitExitLogic_Patch.Enabled = WobSettings.Get( abilityKeys.Get( AbilityType.ShotgunWeapon, "SkipReload" ), false );
            LoadAbilityCooldown( AbilityType.CometTalent );
            LoadAbilityCooldown( AbilityType.StaticWallTalent );
            LoadAbilityCooldown( AbilityType.CrowsNestTalent );
            LoadAbilityCooldown( AbilityType.CreatePlatformTalent );
        }

        private static void LoadAbilityCooldown( AbilityType abilityType ) {
            bool fromCast = WobSettings.Get( abilityKeys.Get( abilityType, "CooldownFromCast" ), false );
            bool useTime = WobSettings.Get( abilityKeys.Get( abilityType, "CooldownUseTime" ), false );
            bool useHits = WobSettings.Get( abilityKeys.Get( abilityType, "CooldownUseHits" ), false );
            if( !useTime && !useHits ) {
                if( abilityInfo[abilityType].CDIsTime ) { useTime = true; }
                if( abilityInfo[abilityType].CDIsHits ) { useHits = true; }
            }
            abilityCooldowns.Add( abilityType, new( fromCast, useTime, useHits ) );
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - ABILITY ROLLS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        private static readonly List<AbilityType> usedAbilities = new();

        [HarmonyPatch( typeof( CharacterCreator ), nameof( CharacterCreator.GenerateRandomCharacter ) )]
        internal static class CharacterCreator_GenerateRandomCharacter_Patch {
            internal static void Postfix( CharacterData __result ) {
                WobPlugin.Log( "[Abilities] GenerateRandomCharacter.Postfix called" );
                if( WobSettings.Get( SoulShop.soulShopKeys.Get( SoulShopType.ForceRandomizeKit, "AllSlots" ), false ) && WobSettings.Get( SoulShop.soulShopKeys.Get( SoulShopType.ForceRandomizeKit, "PreventDuplicates" ), false ) ) {
                    usedAbilities.Add( __result.Weapon );
                    usedAbilities.Add( __result.Talent );
                    usedAbilities.Add( __result.Spell );
                }
            }
        }

        [HarmonyPatch( typeof( LineageWindowController ), "CreateRandomCharacters" )]
        internal static class LineageWindowController_CreateRandomCharacters_Patch {
            internal static void Postfix() {
                usedAbilities.Clear();
            }
        }

        // Method used is several patches to filter abilities according to user settings - Also used by patches in Soul Shop class!
        internal static AbilityType[] FilterAbilityArray( CastAbilityType castAbilityType, ClassType classType, AbilityType[] oldAbilityList, bool isCurioShop ) {
            // Curio Shop class is used for both curio shops and during character creation for the Contrarian trait
            if( classType == ClassType.CURIO_SHOPPE_CLASS || castAbilityType == CastAbilityType.Spell ) {
                // Create a new list to hold the filtered abilitis list
                List<AbilityType> newAbilityList = new();
                // Step through the abilities that the old method returned
                foreach( AbilityType ability in oldAbilityList ) {
                    // Filter duplicates, unlocks in the skill tree and soul shop, and user disabled
                    if( newAbilityList.Contains( ability ) ) {
                        WobPlugin.Log( "[Abilities] Preventing spawn of " + ability + " - no duplicates" );
                    } else if(  isCurioShop && !WobSettings.Get( abilityKeys.Get( ability, "EnabledCurioShop"  ), ability != AbilityType.SuperFart ) ) {
                        WobPlugin.Log( "[Abilities] Preventing spawn of " + ability + " - disabled in user config" );
                    } else if( !isCurioShop && !WobSettings.Get( abilityKeys.Get( ability, "EnabledContrarian" ), ability != AbilityType.SuperFart ) ) {
                        WobPlugin.Log( "[Abilities] Preventing spawn of " + ability + " - disabled in user config" );
                    } else if( usedAbilities.Contains( ability ) ) {
                        WobPlugin.Log( "[Abilities] Preventing spawn of " + ability + " - is in exclude list" );
                    } else {
                        // Any that get through all filters are allowed
                        newAbilityList.Add( ability );
                    }
                }
                // Add Super Fart to curio shop talents if enabled in settings
                if( castAbilityType == CastAbilityType.Talent && !usedAbilities.Contains( AbilityType.SuperFart ) ) {
                    if( ( isCurioShop && WobSettings.Get( abilityKeys.Get( AbilityType.SuperFart, "EnabledCurioShop"  ), false ) ) || ( !isCurioShop && WobSettings.Get( abilityKeys.Get( AbilityType.SuperFart, "EnabledContrarian" ), false ) ) ) {
                        newAbilityList.Add( AbilityType.SuperFart );
                    }
                }
                // Safety check to prevent return of an empty list - give the ability from the tutorial Knight
                if( newAbilityList.Count == 0 ) { newAbilityList.Add( defaultAbilities[castAbilityType].Fallback ); }
                // Return the new list of abilities
                return newAbilityList.ToArray();
            } else {
                // For standard classes in character creation, don't change the abilities
                return oldAbilityList;
            }
        }

        // Method used when rolling abilities in curio shops, patched to filter abilities according to user settings, and reset available abilities list if rerolled through all of them instead of using the sword weapon ability
        [HarmonyPatch( typeof( SwapAbilityRoomPropController ), nameof( SwapAbilityRoomPropController.RollAbilities ) )]
        internal static class SwapAbilityRoomPropController_RollAbilities_Patch {
            internal static void Prefix( SwapAbilityRoomPropController __instance, bool addToTotalRoomRolls ) {
                WobPlugin.Log( "[Abilities] RollAbilities.Prefix called" );
                // Find out if the ability should be a weapon, talent, or spell
                CastAbilityType castAbilityType = Traverse.Create( __instance ).Field( "m_castAbilityTypeToSwap" ).GetValue<CastAbilityType>();
                // Get a reference to the current list of abilities
                List<AbilityType> m_potentialAbilityList = Traverse.Create( typeof( SwapAbilityRoomPropController ) ).Field( "m_potentialAbilityList" ).GetValue<List<AbilityType>>();
                // Check if the room is in setup mode (addToTotalRoomRolls == false), or the list is empty, or only contains the current ability so is effectively empty 
                if( !addToTotalRoomRolls || ( m_potentialAbilityList.Count == 0 ) || ( m_potentialAbilityList.Count == 1 && m_potentialAbilityList[0] == GetCurrentPlayerAbility( castAbilityType ) ) ) {
                    // Clear the list of abilities ready for repopulation
                    m_potentialAbilityList.Clear();
                    // Get an array of valid abilities and pass it through the filter
                    AbilityType[] abilityArray = FilterAbilityArray( castAbilityType, ClassType.CURIO_SHOPPE_CLASS, defaultAbilities[castAbilityType].GetArray().Where( x => x != AbilityType.None && AbilityLibrary.GetAbility( x ) ).ToArray(), true );
                    // Copy each of the abilities to the original abilities list
                    for( int i = 0; i < abilityArray.Length; i++ ) {
                        m_potentialAbilityList.Add( abilityArray[i] );
                    }
                }
            }

            // Copy of private method SwapAbilityRoomPropController.GetCurrentPlayerAbility
            private static AbilityType GetCurrentPlayerAbility( CastAbilityType castAbilityType ) {
                BaseAbility_RL ability = PlayerManager.GetPlayerController().CastAbility.GetAbility(castAbilityType, false);
                return ability ? ability.AbilityType : AbilityType.None;
            }
        }

        // Patch the method that is used during character creation for weapons, once for standard class weapons, and again for Contrarian trait
        [HarmonyPatch( typeof( CharacterCreator ), nameof( CharacterCreator.GetAvailableWeapons ) )]
        internal static class CharacterCreator_GetAvailableWeapons_Patch {
            internal static void Postfix( ClassType classType, ref AbilityType[] __result ) {
                WobPlugin.Log( "[Abilities] GetAbilityArray called for " + classType + " weapon, with " + __result.Length + " abilities in array" );
                __result = FilterAbilityArray( CastAbilityType.Weapon, classType, __result, false );
            }
        }

        // Patch the method that is used during character creation for talents, once for standard class talents, and again for Contrarian trait
        [HarmonyPatch( typeof( CharacterCreator ), nameof( CharacterCreator.GetAvailableTalents ) )]
        internal static class CharacterCreator_GetAvailableTalents_Patch {
            internal static void Postfix( ClassType classType, ref AbilityType[] __result ) {
                WobPlugin.Log( "[Abilities] GetAbilityArray called for " + classType + " talent, with " + __result.Length + " abilities in array" );
                __result = FilterAbilityArray( CastAbilityType.Talent, classType, __result, false );
            }
        }

        // Patch the method that is used during character creation for spells
        [HarmonyPatch( typeof( CharacterCreator ), nameof( CharacterCreator.GetAvailableSpells ) )]
        internal static class CharacterCreator_GetAvailableSpells_Patch {
            internal static void Postfix( ClassType classType, ref AbilityType[] __result ) {
                WobPlugin.Log( "[Abilities] GetAbilityArray called for " + classType + " spell, with " + __result.Length + " abilities in array" );
                __result = FilterAbilityArray( CastAbilityType.Spell, classType, __result, false );
                if( WobSettings.Get( SoulShop.soulShopKeys.Get( SoulShopType.ChooseYourSpell, "AllSlots" ), false ) ) {
                    SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj( SoulShopType.ChooseYourSpell );
                    if( !soulShopObj.IsNativeNull() && soulShopObj.CurrentEquippedLevel > 0 ) {
                        AbilityType soulShopSpellChosen = SaveManager.ModeSaveData.SoulShopSpellChosen;
                        if( soulShopSpellChosen != AbilityType.None ) {
                            WobPlugin.Log( "[SoulShop] CharacterCreator.GetAvailableSpells: Replacing spell" );
                            __result = new AbilityType[] { soulShopSpellChosen };
                        }
                    }
                }
            }
        }


        // Correct the total resolve cost calculation to prevent negative costs from reduced relic costs or increased effect of Archeology Camp (Relic_Cost_Down)
        [HarmonyPatch( typeof( CharacterCreator ), nameof( CharacterCreator.ApplyRandomizeKitTrait ) )]
        internal static class CharacterCreator_ApplyRandomizeKitTrait_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "CharacterCreator.ApplyRandomizeKitTrait" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldc_I4, ClassType.CURIO_SHOPPE_CLASS ), // ClassType.CURIO_SHOPPE_CLASS     1000
                        /*  1 */ new( OpCodes.Call, name:"GetAvailableWeapons"     ), // CharacterCreator.GetAvailableWeapons( ClassType.CURIO_SHOPPE_CLASS )
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetInstruction( 1, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => GetAvailableWeapons( ClassType.CURIO_SHOPPE_CLASS, true ) ) ),
                        new WobTranspiler.OpAction_Insert( 1, OpCodes.Ldarg_3 ), // useLineageSeed     parameter is true when in character creator, false when applying relic effects
                    }, expected: 1 );
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldc_I4, ClassType.CURIO_SHOPPE_CLASS ), // ClassType.CURIO_SHOPPE_CLASS     1000
                        /*  1 */ new( OpCodes.Call, name:"GetAvailableTalents"     ), // CharacterCreator.GetAvailableTalents( ClassType.CURIO_SHOPPE_CLASS )
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetInstruction( 1, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => GetAvailableTalents( ClassType.CURIO_SHOPPE_CLASS, true ) ) ),
                        new WobTranspiler.OpAction_Insert( 1, OpCodes.Ldarg_3 ), // useLineageSeed     parameter is true when in character creator, false when applying relic effects
                    }, expected: 1 );
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldc_I4, ClassType.CURIO_SHOPPE_CLASS ), // ClassType.CURIO_SHOPPE_CLASS     1000
                        /*  1 */ new( OpCodes.Call, name:"GetAvailableSpells"      ), // CharacterCreator.GetAvailableSpells( ClassType.CURIO_SHOPPE_CLASS )
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetInstruction( 1, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => GetAvailableSpells( ClassType.CURIO_SHOPPE_CLASS, true ) ) ),
                        new WobTranspiler.OpAction_Insert( 1, OpCodes.Ldarg_3 ), // useLineageSeed     parameter is true when in character creator, false when applying relic effects
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static AbilityType[] GetAvailableWeapons( ClassType classType, bool inCreator ) {
                WobPlugin.Log( "[Abilities] ApplyRandomizeKitTrait called GetAvailableWeapons, in creator: " + inCreator );
                return FilterAbilityArray( CastAbilityType.Weapon, classType, ClassLibrary.GetClassData( classType ).WeaponData.WeaponAbilityArray, !inCreator );
            }

            private static AbilityType[] GetAvailableTalents( ClassType classType, bool inCreator ) {
                WobPlugin.Log( "[Abilities] ApplyRandomizeKitTrait called GetAvailableTalents, in creator: " + inCreator );
                return FilterAbilityArray( CastAbilityType.Talent, classType, ClassLibrary.GetClassData( classType ).TalentData.TalentAbilityArray, !inCreator );
            }

            private static AbilityType[] GetAvailableSpells( ClassType classType, bool inCreator ) {
                WobPlugin.Log( "[Abilities] ApplyRandomizeKitTrait called GetAvailableSpells, in creator: " + inCreator );
                return FilterAbilityArray( CastAbilityType.Spell, classType, ClassLibrary.GetClassData( classType ).SpellData.SpellAbilityArray, !inCreator );
            }
        }


        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - TALENTS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        [HarmonyPatch( typeof( AbilityLibrary ), nameof( AbilityLibrary.Initialize ) )]
        internal static class AbilityLibrary_Initialize_Patch {
            private static bool runOnce = false;
            internal static void Postfix() {
                if( !runOnce ) {
                    foreach( AbilityType talentType in AbilityType_RL.ValidTalentTypeArray ) {
                        PatchAbility( talentType, CastAbilityType.Talent );
                    }
                    PatchAbility( AbilityType.SuperFart, CastAbilityType.Talent );
                    foreach( AbilityType talentType in AbilityType_RL.ValidSpellTypeArray ) {
                        PatchAbility( talentType, CastAbilityType.Spell );
                    }
                    runOnce = true;
                }
            }

            private static void PatchAbility( AbilityType abilityType, CastAbilityType castAbilityType ) {
                if( abilityType == AbilityType.None ) { return; }
                BaseAbility_RL ability = AbilityLibrary.GetAbility( abilityType );
                if( ability != null ) {
                    AbilityData abilityData = ability.AbilityData;
                    if( abilityData != null && abilityKeys.Exists( abilityType ) ) {
                        if( castAbilityType == CastAbilityType.Spell ) {
                            int manaCost = WobSettings.Get( abilityKeys.Get( abilityType, "ManaCost" ), abilityData.BaseCost );
                            if( manaCost != abilityData.BaseCost ) {
                                WobPlugin.Log( "[Abilities] " + abilityType + ": mana cost " + abilityData.BaseCost + " -> " + manaCost );
                                abilityData.BaseCost = manaCost;
                            }
                        }
                        if( castAbilityType == CastAbilityType.Talent ) {
                            bool useTime = WobSettings.Get( abilityKeys.Get( abilityType, "CooldownUseTime" ), false );
                            if( useTime != abilityData.CooldownDecreaseOverTime ) {
                                WobPlugin.Log( "[Abilities] " + abilityType + ": cooldown use time -> " + useTime );
                            }
                            abilityData.CooldownDecreaseOverTime = useTime;
                            bool useHits = WobSettings.Get( abilityKeys.Get( abilityType, "CooldownUseHits" ), false );
                            if( useHits != abilityData.CooldownDecreasePerHit ) {
                                WobPlugin.Log( "[Abilities] " + abilityType + ": cooldown use hits -> " + useHits );
                            }
                            abilityData.CooldownDecreasePerHit = useHits;
                            float cooldown = WobSettings.Get( abilityKeys.Get( abilityType, "CooldownAmount" ), abilityData.CooldownTime );
                            if( !abilityData.CooldownDecreaseOverTime && cooldown > 0 ) {
                                float cooldown2 = Mathf.Max( 1, Mathf.RoundToInt( cooldown ) );
                                if( cooldown != cooldown2 ) {
                                    WobPlugin.Log( "[Abilities] CONFIG ERROR: Cooldown should be whole number if it isn't a time - using rounded value", WobPlugin.ERROR );
                                }
                                cooldown = cooldown2;
                            }
                            if( cooldown != abilityData.CooldownTime ) {
                                WobPlugin.Log( "[Abilities] " + abilityType + ": cooldown " + abilityData.CooldownTime + " -> " + cooldown );
                                abilityData.CooldownTime = cooldown;
                            }
                            if( abilityData.MaxAmmo > 0 ) {
                                int maxAmmo = WobSettings.Get( abilityKeys.Get( abilityType, "MaxAmmo"), abilityData.MaxAmmo );
                                WobPlugin.Log( "[Abilities] " + abilityType + ": max ammo " + abilityData.MaxAmmo + " -> " + maxAmmo );
                                abilityData.MaxAmmo = maxAmmo;
                            }
                            if( abilityType == AbilityType.ManaBombTalent ) {
                                abilityData.CooldownRefreshesAllAmmo = WobSettings.Get( abilityKeys.Get( abilityType, "RefreshAllAmmo" ), true );
                            }
                        }
                    } else {
                        WobPlugin.Log( "[Abilities] WARNING: No settings for " + abilityType, WobPlugin.ERROR );
                    }
                }
            }
        }

        private static void AbilityEffectStart( BaseAbility_RL ability, AbilityType abilityType ) {
            if( abilityCooldowns[abilityType].FromCast ) {
                if( abilityCooldowns[abilityType].UseTime ) { ability.DecreaseCooldownOverTime = true; }
                if( abilityCooldowns[abilityType].UseHits ) { ability.DecreaseCooldownWhenHit = true; }
                ability.DisplayPausedAbilityCooldown = false;
            } else {
                if( abilityCooldowns[abilityType].UseTime ) { ability.DecreaseCooldownOverTime = false; }
                if( abilityCooldowns[abilityType].UseHits ) { ability.DecreaseCooldownWhenHit = false; }
                ability.DisplayPausedAbilityCooldown = true;
            }
        }

        private static void AbilityEffectEnd( BaseAbility_RL ability, AbilityType abilityType ) {
            if( abilityCooldowns[abilityType].UseTime ) { ability.DecreaseCooldownOverTime = true; }
            if( abilityCooldowns[abilityType].UseHits ) { ability.DecreaseCooldownWhenHit = true; }
            ability.DisplayPausedAbilityCooldown = false;
        }

        // Astromancer's Comet Form - start cooldown on cast rather than end of effect, and use timed and on-hit enemy cooldown
        [HarmonyPatch( typeof( Comet_Ability ), "FireProjectile" )]
        internal static class Comet_Ability_FireProjectile_Patch {
            internal static void Postfix( Comet_Ability __instance ) { AbilityEffectStart( __instance, AbilityType.CometTalent ); }
        }
        [HarmonyPatch( typeof( Comet_Ability ), "StopAbility" )]
        internal static class Comet_Ability_StopAbility_Patch {
            internal static void Postfix( Comet_Ability __instance ) { AbilityEffectEnd( __instance, AbilityType.CometTalent ); }
        }
        
        // Dragon Lancer's Bastion - start cooldown on cast rather than end of effect, and use timed and on-hit enemy cooldown
        [HarmonyPatch( typeof( StaticWall_Ability ), "FireProjectile" )]
        internal static class StaticWall_Ability_FireProjectile_Patch {
            internal static void Postfix( StaticWall_Ability __instance ) { AbilityEffectStart( __instance, AbilityType.StaticWallTalent ); }
        }
        [HarmonyPatch( typeof( StaticWall_Ability ), "ResumeCooldown" )]
        internal static class StaticWall_Ability_ResumeCooldown_Patch {
            internal static void Postfix( StaticWall_Ability __instance ) { AbilityEffectEnd( __instance, AbilityType.StaticWallTalent ); }
        }

        // Pirate's Pirate Ship - start cooldown on cast rather than end of effect, and use timed and on-hit enemy cooldown
        [HarmonyPatch( typeof( CrowsNest_Ability ), "FireProjectile" )]
        internal static class CrowsNest_Ability_FireProjectile_Patch {
            internal static void Postfix( CrowsNest_Ability __instance ) { AbilityEffectStart( __instance, AbilityType.CrowsNestTalent ); }
        }
        [HarmonyPatch( typeof( CreatePlatform_Ability ), "ResumeCooldown" )]
        internal static class CrowsNest_Ability_ResumeCooldown_Patch {
            internal static void Postfix( CrowsNest_Ability __instance ) { AbilityEffectEnd( __instance, AbilityType.CrowsNestTalent ); }
        }
        [HarmonyPatch( typeof( CrowsNest_Ability ), "ResumeCooldownIfPlayerExitsRoom" )]
        internal static class CrowsNest_Ability_ResumeCooldownIfPlayerExitsRoom_Patch {
            internal static void Postfix( CrowsNest_Ability __instance ) { AbilityEffectEnd( __instance, AbilityType.CrowsNestTalent ); }
        }

        // Ranger's Ivy Canopy - start cooldown on cast rather than end of effect, and use timed and on-hit enemy cooldown
        [HarmonyPatch( typeof( CreatePlatform_Ability ), "FireProjectile" )]
        internal static class CreatePlatform_Ability_FireProjectile_Patch {
            internal static void Postfix( CreatePlatform_Ability __instance ) { AbilityEffectStart( __instance, AbilityType.CreatePlatformTalent ); }
        }
        [HarmonyPatch( typeof( CreatePlatform_Ability ), "ResumeCooldown" )]
        internal static class CreatePlatform_Ability_ResumeCooldown_Patch {
            internal static void Postfix( CreatePlatform_Ability __instance ) { AbilityEffectEnd( __instance, AbilityType.CreatePlatformTalent ); }
        }
        [HarmonyPatch( typeof( CreatePlatform_Ability ), "ResumeCooldownIfPlayerExitsRoom" )]
        internal static class CreatePlatform_Ability_ResumeCooldownIfPlayerExitsRoom_Patch {
            internal static void Postfix( CreatePlatform_Ability __instance ) { AbilityEffectEnd( __instance, AbilityType.CreatePlatformTalent ); }
        }

        // Knight's Shield - set the timing of ability perfect blocks
        [HarmonyPatch( typeof( ShieldBlock_Ability ), "OnPlayerBlocked" )]
        internal static class ShieldBlock_Ability_OnPlayerBlocked_Patch {
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "ShieldBlock_Ability.OnPlayerBlocked" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        //m_isPerfectBlock = Time.time < m_abilityController.PlayerController.BlockStartTime + 0.135f;
                        /*  0 */ new( OpCodes.Call, name: "get_time"                 ), // Time.time
                        /*  1 */ new( OpCodeSet.Ldarg                                ), // this
                        /*  2 */ new( OpCodes.Ldfld                                  ), // this.m_abilityController
                        /*  3 */ new( OpCodes.Callvirt, name: "get_PlayerController" ), // this.m_abilityController.PlayerController
                        /*  4 */ new( OpCodes.Callvirt, name: "get_BlockStartTime"   ), // this.m_abilityController.PlayerController.BlockStartTime
                        /*  5 */ new( OpCodes.Ldc_R4                                 ), // 0.135f
                        /*  6 */ new( OpCodes.Add                                    ), // m_abilityController.PlayerController.BlockStartTime + 0.135f
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                        new WobTranspiler.OpAction_SetOperand( 1, WobSettings.Get( abilityKeys.Get( AbilityType.ShieldBlockTalent, "PerfectBlockTime" ), Ability_EV.SHIELD_BLOCK_PERFECT_TIMING ) ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }
        }

        // Gunslinger's Revolver - no reloads
        [HarmonyPatch( typeof( PistolWeapon_Ability ), "FireProjectile" )]
        internal static class PistolWeapon_Ability_FireProjectile_Patch {
            internal static bool Enabled = false;
            internal static void Postfix( PistolWeapon_Ability __instance ) {
                if( Enabled && __instance.CurrentAmmo <= 0 ) {
                    __instance.CurrentAmmo = __instance.MaxAmmo;
                }
            }
        }

        // Gunslinger's Bluderbuss - no reloads
        [HarmonyPatch( typeof( Shotgun_Ability ), "OnExitExitLogic" )]
        internal static class Shotgun_Ability_OnExitExitLogic_Patch {
            internal static bool Enabled = false;
            internal static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions ) {
                // Set up the transpiler handler with the instruction list
                WobTranspiler transpiler = new( instructions, "Shotgun_Ability.OnExitExitLogic" );
                // Perform the patching
                transpiler.PatchAll(
                    // Define the IL code instructions that should be matched
                    new List<WobTranspiler.OpTest> {
                        /*  0 */ new( OpCodes.Ldarg_0                              ), // this
                        /*  1 */ new( OpCodes.Ldfld, name: "m_abilityController"   ), // this.m_abilityController
                        /*  2 */ new( OpCodes.Ldc_I4_0                             ), // CastAbilityType.Weapon
                        /*  3 */ new( OpCodes.Ldc_I4_1                             ), // true
                        /*  4 */ new( OpCodes.Ldc_I4_0                             ), // false
                        /*  5 */ new( OpCodes.Callvirt, name: "StartAbility"       ), // this.m_abilityController.StartAbility(CastAbilityType.Weapon, true, false)
                    },
                    // Define the actions to take when an occurrence is found
                    new List<WobTranspiler.OpAction> {
                            new WobTranspiler.OpAction_SetInstruction( 1, OpCodes.Call, SymbolExtensions.GetMethodInfo( () => InstantReload( null ) ) ),
                            new WobTranspiler.OpAction_Remove( 2, 4 ),
                    }, expected: 1 );
                // Return the modified instructions
                return transpiler.GetResult();
            }

            private static void InstantReload( Shotgun_Ability __instance ) {
                if( Enabled ) {
                    __instance.CurrentAmmo = __instance.MaxAmmo;
                }
            }
        }

    }
}
