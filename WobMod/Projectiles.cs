using System.Collections.Generic;
using Wob_Common;
using HarmonyLib;

namespace WobMod {
    internal class Projectiles {

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTING DEFAULTS
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        private enum ProjectilesHit { None, Medium = ProjectileCollisionFlag.ReflectWeak, Large = (ProjectileCollisionFlag.ReflectWeak|ProjectileCollisionFlag.ReflectStrong) }
        private record ProjectileInfo( string[] Names, bool UseLifeSpan, float LifeSpan, bool UseSpeed, float Speed, bool UseDamage, float StrScale, float IntScale, float Knockback, bool HitProjectiles, ProjectilesHit ProjectilesHit );

        private record ProjectileConfigKeys<T>( T TypeKey, string ConfigKey );
        private static readonly Dictionary<ProjectileConfigKeys<AbilityType>, ProjectileInfo> abilityProjectileInfo = new() {
            // Standard class weapons
            { new( AbilityType.DualBladesWeapon,     "Initial"    ), new( new[] { "PlayerDualBladeWeapon"                                           }, false,  0.1f,   false,  0f,     true,   0.5f,   0f,     6f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.DualBladesWeapon,     "Final"      ), new( new[] { "PlayerDualBladeThirdWeapon"                                      }, false,  0.1f,   false,  0f,     true,   1f,     0f,     8f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.AstroWandWeapon,      "Initial"    ), new( new[] { "PlayerAstroWandInitialPopWeapon"                                 }, false,  0.1f,   false,  0f,     true,   0.6f,   0f,     2f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.AstroWandWeapon,      "Tick"       ), new( new[] { "PlayerAstroWandExplosionWeapon"                                  }, true,   3.5f,   false,  0f,     true,   0.2f,   0f,     2f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.AxeWeapon,            "Ground"     ), new( new[] { "PlayerAxeGroundWeapon"                                           }, false,  0.1f,   false,  0f,     true,   2.4f,   0f,     8f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.AxeWeapon,            "Air"        ), new( new[] { "PlayerAxeAirborneWeapon", "PlayerAxeJustLandedWeapon"            }, false,  0.1f,   false,  0f,     true,   0.7f,   0f,     6f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.LuteWeapon,           "Note"       ), new( new[] { "PlayerLuteWeapon"                                                }, true,   6f,     true,   24f,    true,   0.3f,   0f,     2f,   true,   ProjectilesHit.Medium ) },
            { new( AbilityType.LuteWeapon,           "Explosion"  ), new( new[] { "PlayerLuteExplosionWeapon"                                       }, false,  0.15f,  false,  0f,     true,   0f,     1.85f,  6f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.BoxingGloveWeapon,    "Punch"      ), new( new[] { "PlayerBoxingGloveWeapon"                                         }, false,  0.165f, false,  0f,     true,   0.5f,   0f,     8f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.BoxingGloveWeapon,    "Uppercut"   ), new( new[] { "PlayerBoxingGloveUpAttack", "PlayerBoxingGloveDownAttack"        }, false,  0.165f, false,  0f,     true,   1f,     0f,     8f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.FryingPanWeapon,      "Hit"        ), new( new[] { "PlayerLadleStrike"                                               }, false,  0.1f,   false,  0f,     true,   1.65f,  0f,     8f,   true,   ProjectilesHit.Medium ) },
            { new( AbilityType.FryingPanWeapon,      "Fireball"   ), new( new[] { "PlayerLadleFireball"                                             }, true,   5f,     true,   24f,    true,   0f,     1.5f,   8f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.LanceWeapon,          "Swing"      ), new( new[] { "PlayerLanceSwingWeapon"                                          }, false,  0.165f, false,  0f,     true,   2f,     0f,     8f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.LanceWeapon,          "Charge"     ), new( new[] { "PlayerLanceExplosionWeapon"                                      }, false,  0.165f, false,  0f,     true,   2f,     1.6f,   8f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.SaberWeapon,          "Hit"        ), new( new[] { "PlayerSaberWeapon", "PlayerSaberAirWeapon"                       }, false,  0.1f,   false,  0f,     true,   1.8f,   0f,     6f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.PistolWeapon,         "Bullet"     ), new( new[] { "PlayerPistolWeapon"                                              }, true,   0.45f,  true,   26f,    true,   0.4f,   0f,     2f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.SwordWeapon,          "Hit"        ), new( new[] { "PlayerSwordWeapon"                                               }, false,  0.165f, false,  0f,     true,   2f,     0f,     6f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.MagicWandWeapon,      "Hit"        ), new( new[] { "PlayerMagicWandWeapon", "PlayerMagicWandStrike"                  }, false,  0.1f,   false,  0f,     true,   0.5f,   0f,     6f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.MagicWandWeapon,      "Explosion"  ), new( new[] { "PlayerMagicWandExplosionWeapon"                                  }, false,  0.1f,   false,  0f,     true,   1.1f,   0f,     6f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.CannonWeapon,         "Swing"      ), new( new[] { "PlayerCannonSwingWeapon"                                         }, false,  0.1f,   false,  0f,     true,   1.75f,  0f,     10f,  false,  ProjectilesHit.None   ) },
            { new( AbilityType.CannonWeapon,         "Cannonball" ), new( new[] { "PlayerCannonBallWeapon"                                          }, true,   5f,     true,   28f,    false,  0f,     0f,     0f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.CannonWeapon,         "Explosion"  ), new( new[] { "PlayerCannonBallExplosionWeapon"                                 }, false,  0.1f,   false,  0f,     true,   2.3f,   0f,     8f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.BowWeapon,            "Arrow"      ), new( new[] { "PlayerBow"                                                       }, true,   2.5f,   true,   30f,    true,   2.25f,  0f,     6f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.KatanaWeapon,         "Hit"        ), new( new[] { "PlayerKatanaWeapon", "PlayerKatanaAirborneWeapon"                }, false,  0.1f,   false,  0f,     true,   2f,     0f,     6f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.SpearWeapon,          "Hit"        ), new( new[] { "PlayerSpearWeapon", "PlayerSpearSwipeWeapon"                     }, false,  0.1f,   false,  0f,     true,   1.6f,   0f,     6f,   false,  ProjectilesHit.None   ) },
            // Soul shop unlock variant weapons
            { new( AbilityType.TonfaWeapon,          "Initial"    ), new( new[] { "PlayerTonfaWeapon"                                               }, false,  0.15f,  false,  0f,     true,   0.2f,   0f,     6f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.TonfaWeapon,          "Final"      ), new( new[] { "PlayerTonfaWeaponLastStrike"                                     }, false,  0.15f,  false,  0f,     true,   0.4f,   0f,     6f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.CrowStormWeapon,      "Burst"      ), new( new[] { "PlayerCrowStormExplosionWeapon"                                  }, false,  0.2f,   false,  0f,     true,   0f,     0f,     0f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.CrowStormWeapon,      "Crow"       ), new( new[] { "PlayerCrowStormWeapon"                                           }, true,   2.5f,   true,   12.5f,  true,   0.85f,  0f,     0f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.AxeSpinnerWeapon,     "Hit"        ), new( new[] { "PlayerAxeSpinnerWeapon"                                          }, false,  999f,   false,  0f,     true,   0.4f,   0f,     2f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.KineticBowWeapon,     "Hit"        ), new( new[] { "PlayerKineticBow"                                                }, true,   2.5f,   true,   42f,    true,   1.5f,   1.5f,   8f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.ExplosiveHandsWeapon, "Ball"       ), new( new[] { "PlayerExplosiveHandsWeapon"                                      }, true,   0.65f,  true,   28f,    false,  0f,     0f,     0f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.ExplosiveHandsWeapon, "Explosion"  ), new( new[] { "PlayerExplosiveHandsExplosionWeapon"                             }, false,  0.15f,  false,  0f,     true,   2f,     0f,     8f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.SpoonsWeapon,         "Initial"    ), new( new[] { "PlayerSpoon"                                                     }, true,   5f,     true,   28f,    true,   1.75f,  0f,     0f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.SpoonsWeapon,         "Bounced"    ), new( new[] { "PlayerSecondSpoon"                                               }, true,   5f,     true,   28f,    true,   1.75f,  1.75f,  0f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.DragonAspectWeapon,   "Hit"        ), new( new[] { "PlayerDragonAspectWeapon"                                        }, false,  5f,     false,  0f,     true,   2.25f,  0f,     6f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.SniperWeapon,         "Hit"        ), new( new[] { "PlayerSniperWeapon"                                              }, true,   0.8f,   true,   35f,    true,   1.2f,   0f,     0f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.ShotgunWeapon,        "Bullet"     ), new( new[] { "PlayerShotgunWeapon", "PlayerShotgun2Weapon"                     }, true,   0.3f,   true,   31f,    true,   0.5f,   0f,     8f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.ChakramWeapon,        "Hit"        ), new( new[] { "PlayerChakram"                                                   }, true,   5f,     true,   24f,    true,   0.6f,   0f,     2f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.ScytheWeapon,         "Hit"        ), new( new[] { "PlayerScytheWeapon", "PlayerScytheSecondWeapon"                  }, false,  0.175f, false,  0f,     true,   1.25f,  0f,     6f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.SurfboardWeapon,      "Hit"        ), new( new[] { "PlayerAnchorWeapon"                                              }, false,  999f,   false,  0f,     true,   1.4f,   0f,     10f,  false,  ProjectilesHit.None   ) },
            { new( AbilityType.GroundBowWeapon,      "Arrow"      ), new( new[] { "PlayerGroundBow"                                                 }, true,   1f,     true,   60f,    true,   4.25f,  0f,     8f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.UmbrellaWeapon,       "Hit"        ), new( new[] { "PlayerUmbrellaGroundWeapon", "PlayerUmbrellaAirWeapon"           }, false,  999f,   false,  0f,     true,   2.25f,  0f,     8f,   true,   ProjectilesHit.Medium ) },
            { new( AbilityType.HammerWeapon,         "Hammer"     ), new( new[] { "PlayerHammerThrownWeapon", "PlayerHammerReturningWeapon"         }, false,  999f,   true,   26f,    true,   1.5f,   0f,     6f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.HammerWeapon,         "Lightning"  ), new( new[] { "PlayerHammerLandedWeapon"                                        }, false,  5f,     false,  0f,     true,   0f,     0.75f,  2f,   false,  ProjectilesHit.None   ) },
            // Class talents
            { new( AbilityType.CometTalent,          "Hit"        ), new( new[] { "PlayerCometTalent"                                               }, true,   10f,    false,  0f,     true,   0f,     0.2f,   0f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.ShoutTalent,          "Hit"        ), new( new[] { "PlayerShoutTalent"                                               }, false,  0.3f,   false,  0f,     true,   0f,     0.75f,  10f,  true,   ProjectilesHit.Large  ) },
            { new( AbilityType.CrescendoTalent,      "Hit"        ), new( new[] { "PlayerCrescendoTalent"                                           }, false,  0.3f,   false,  0f,     true,   0f,     1.75f,  8f,   true,   ProjectilesHit.Medium ) },
            { new( AbilityType.KnockoutTalent,       "Hit"        ), new( new[] { "PlayerKnockOutTalent"                                            }, false,  0.15f,  false,  0f,     true,   0f,     0.4f,   10f,  true,   ProjectilesHit.Medium ) },
            { new( AbilityType.KnockoutTalent,       "Flying"     ), new( new[] { "StatusEffectKnockout"                                            }, false,  99f,    false,  0f,     true,   0f,     0.8f,   10f,  true,   ProjectilesHit.Medium ) },
            { new( AbilityType.KnockoutTalent,       "Explosion"  ), new( new[] { "StatusEffectKnockoutExplosion"                                   }, false,  0.15f,  false,  0f,     true,   0f,     1f,     10f,  true,   ProjectilesHit.Medium ) },
            { new( AbilityType.StaticWallTalent,     "Shield"     ), new( new[] { "PlayerStaticWallTalent"                                          }, true,   3.26f,  false,  0f,     true,   0f,     0.6f,   10f,  true,   ProjectilesHit.Large  ) },
            { new( AbilityType.RollTalent,           "Hit"        ), new( new[] { "PlayerRollTalent"                                                }, false,  999f,   false,  0f,     true,   0f,     0.8f,   6f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.ManaBombTalent,       "Hit"        ), new( new[] { "PlayerManaBombExplosionTalent", "PlayerManaBombExplosion2Talent" }, false,  2.25f,  false,  0f,     true,   0f,     1.65f,  10f,  true,   ProjectilesHit.Medium ) },
            { new( AbilityType.ShieldBlockTalent,    "Block"      ), new( new[] { "PlayerShieldBlockTalent"                                         }, false,  0.2f,   false,  0f,     true,   0f,     1.25f,  8f,   true,   ProjectilesHit.Large  ) },
            { new( AbilityType.ShieldBlockTalent,    "Perfect"    ), new( new[] { "PlayerShieldBlockPerfectTalent"                                  }, false,  0.2f,   false,  0f,     true,   0f,     2.5f,   8f,   true,   ProjectilesHit.Large  ) },
            { new( AbilityType.CrowsNestTalent,      "Ship"       ), new( new[] { "PlayerCrowsNestPlatformTalent"                                   }, true,   8f,     false,  0f,     true,   0f,     0f,     0f,   true,   ProjectilesHit.Medium ) },
            { new( AbilityType.CrowsNestTalent,      "ShipOnFire" ), new( new[] { "PlayerCrowsNestFallingTalent"                                    }, true,   4f,     true,   26f,    false,  0f,     0f,     10f,  true,   ProjectilesHit.Medium ) },
            { new( AbilityType.CrowsNestTalent,      "Explosion"  ), new( new[] { "PlayerCrowsNestExplosionTalent"                                  }, false,  0.25f,  false,  0f,     true,   0f,     2f,     10f,  false,  ProjectilesHit.None   ) },
            { new( AbilityType.CrowsNestTalent,      "Cannonball" ), new( new[] { "PlayerCrowsNestCannonTalent"                                     }, true,   8f,     true,   34f,    true,   0f,     0.4f,   6f,   false,  ProjectilesHit.None   ) },
            { new( AbilityType.CreatePlatformTalent, "Platform"   ), new( new[] { "PlayerCreatePlatformTalent"                                      }, true,   8f,     false,  0f,     true,   0f,     1f,     0f,   true,   ProjectilesHit.Medium ) },
            { new( AbilityType.TeleSliceTalent,      "Hit"        ), new( new[] { "PlayerTeleSliceTalent"                                           }, false,  0.15f,  false,  0f,     true,   0f,     2.2f,   8f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.SpearSpinTalent,      "Hit"        ), new( new[] { "PlayerSpearSpinTalent"                                           }, false,  999f,   false,  0f,     true,   0f,     0.75f,  6f,   true,   ProjectilesHit.Medium ) },
            // Trait talents
            { new( AbilityType.SuperFart,            "Hit"        ), new( new[] { "PlayerSuperFartTalent"                                           }, false,  0.25f,  false,  0f,     true,   0f,     1f,     10f,  true,   ProjectilesHit.None   ) },
            // Spells
            { new( AbilityType.FlameThrowerSpell,    "Spell"      ), new( new[] { "PlayerFlameThrowerSpell"                                         }, false,  999f,   false,  0f,     true,   0f,     1.2f,   6f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.FireballSpell,        "Ball"       ), new( new[] { "PlayerFireballSpell"                                             }, true,   6f,     true,   28f,    false,  0f,     0f,     0f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.FireballSpell,        "Explosion"  ), new( new[] { "PlayerFireballExplosionSpell"                                    }, false,  0f,     false,  0f,     true,   0f,     1.5f,   8f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.FlameBarrierSpell,    "Spell"      ), new( new[] { "PlayerFlameBarrierSpell"                                         }, false,  999f,   false,  0f,     true,   0f,     0.35f,  6f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.FreezeStrikeSpell,    "Spell"      ), new( new[] { "PlayerFreezeStrikeSpell"                                         }, false,  0.25f,  false,  0f,     true,   0f,     2f,     0f,   true,   ProjectilesHit.Large  ) },
            { new( AbilityType.SporeSpreadSpell,     "Spell"      ), new( new[] { "PlayerSporeSpreadSpell"                                          }, true,   2f,     true,   32f,    true,   0f,     0.6f,   4f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.SporeSpreadSpell,     "Explosion"  ), new( new[] { "PlayerSporeSpreadExplosionSpell"                                 }, false,  0.5f,   false,  0f,     true,   0f,     2f,     6f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.GravityWellSpell,     "Spell"      ), new( new[] { "PlayerGravityWellSpell"                                          }, true,   1.3f,   false,  0f,     true,   0f,     1.25f,  8f,   true,   ProjectilesHit.Large  ) },
            { new( AbilityType.LightningSpell,       "Spell"      ), new( new[] { "PlayerLightningSpell"                                            }, false,  0.4f,   false,  0f,     true,   0f,     2.25f,  6f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.AimLaserSpell,        "Spell"      ), new( new[] { "PlayerAimLaserSpell"                                             }, false,  999f,   false,  0f,     true,   0f,     1f,     2f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.PoolBallSpell,        "Spell"      ), new( new[] { "PlayerPoolBallSpell"                                             }, true,   5f,     true,   32f,    true,   0f,     0.675f, 6f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.AxeSpell,             "Spell"      ), new( new[] { "PlayerAxeSpell"                                                  }, true,   6f,     true,   29f,    true,   0f,     1.6f,   6f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.PoisonBombSpell,      "Ball"       ), new( new[] { "PlayerPoisonBombSpell"                                           }, true,   6f,     true,   25f,    false,  0f,     0f,     0f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.PoisonBombSpell,      "Explosion"  ), new( new[] { "PlayerPoisonBombInitialExplosionSpell"                           }, false,  0.15f,  false,  0f,     true,   0f,     0.25f,  0f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.PoisonBombSpell,      "Cloud"      ), new( new[] { "PlayerPoisonBombLastingExplosionSpell"                           }, true,   5f,     false,  0f,     true,   0f,     0.1f,   0f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.AilmentCurseSpell,    "Spell"      ), new( new[] { "PlayerAilmentCurseSpell"                                         }, false,  0.25f,  false,  0f,     true,   0f,     0.25f,  8f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.StraightBoltSpell,    "Spell"      ), new( new[] { "PlayerStraightBoltSpell"                                         }, true,   0.75f,  true,   34f,    true,   0f,     1.75f,  6f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.SnapSpell,            "Spell"      ), new( new[] { "PlayerSnapSpell"                                                 }, true,   3.5f,   false,  0f,     true,   0f,     1.5f,   10f,  true,   ProjectilesHit.Medium ) },
            { new( AbilityType.EnergyBounceSpell,    "Ball"       ), new( new[] { "PlayerEnergyBounceSpell"                                         }, true,   4f,     true,   32f,    false,  0f,     0f,     0f,   true,   ProjectilesHit.Large  ) },
            { new( AbilityType.EnergyBounceSpell,    "Explosion"  ), new( new[] { "PlayerEnergyBounceExplosionSpell"                                }, false,  0.15f,  false,  0f,     true,   0f,     2.75f,  10f,  true,   ProjectilesHit.Large  ) },
            { new( AbilityType.DamageZoneSpell,      "Ball"       ), new( new[] { "PlayerDamageZoneSpell"                                           }, true,   2.5f,   true,   28f,    false,  0f,     0.5f,   4f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.DamageZoneSpell,      "Explosion"  ), new( new[] { "PlayerDamageZoneExplosionSpell"                                  }, true,   2.6f,   false,  0f,     true,   0f,     0.5f,   4f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.TimeBombSpell,        "Ball"       ), new( new[] { "PlayerTimeBombSpell"                                             }, true,   1.5f,   true,   3.25f,  false,  0f,     0f,     0f,   true,   ProjectilesHit.None   ) },
            { new( AbilityType.TimeBombSpell,        "Explosion"  ), new( new[] { "PlayerTimeBombExplosionSpell"                                    }, false,  0.15f,  false,  0f,     true,   0f,     4.5f,   10f,  true,   ProjectilesHit.None   ) },
            { new( AbilityType.DamageWallSpell,      "Spell"      ), new( new[] { "PlayerDamageWallSpell"                                           }, true,   1.6f,   true,   10f,    true,   0f,     1.5f,   8f,   true,   ProjectilesHit.Large  ) },
        };
        private static readonly Dictionary<ProjectileConfigKeys<RelicType>, ProjectileInfo> relicProjectileInfo = new() {
            { new( RelicType.DamageAuraOnHit,        "Aura"       ), new( new[] { "RelicDamageAuraOnHit"                                            }, true,   1.5f,   false,  0f,     true,   0f,     0.25f,  0f,   true,   ProjectilesHit.None   ) },
            { new( RelicType.DashStrikeDamageUp,     "Wave"       ), new( new[] { "RelicDashStrikeExplosion"                                        }, true,   0.2f,   true,   30f,    true,   0f,     1.25f,  6f,   true,   ProjectilesHit.Medium ) },
            { new( RelicType.LandShockwave,          "Explosion"  ), new( new[] { "RelicLandShockwave"                                              }, false,  0.1f,   false,  0f,     true,   0f,     0.75f,  8f,   true,   ProjectilesHit.Medium ) },
            { new( RelicType.OnHitAreaDamage,        "Hit"        ), new( new[] { "RelicOnHitAreaDamage"                                            }, false,  0.2f,   false,  0f,     true,   0f,     1.5f,   0f,   false,  ProjectilesHit.None   ) },
            { new( RelicType.ProjectileDashStart,    "Cloud"      ), new( new[] { "RelicProjectileDashStart"                                        }, true,   1.01f,  false,  0f,     true,   0f,     0.2f,   0f,   true,   ProjectilesHit.None   ) },
            { new( RelicType.TalentDamageBolt,       "Bolt"       ), new( new[] { "RelicTalentDamageBolt"                                           }, true,   5f,     true,   28f,    true,   0f,     1.6f,   6f,   true,   ProjectilesHit.None   ) },
        };

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ SETTINGS READ/WRITE
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        private static readonly Dictionary<string, ProjectileInfo> projectileLookup = new();

        internal static void RunSetup() {
            foreach( ProjectileConfigKeys<AbilityType> projectileKey in abilityProjectileInfo.Keys ) {
                LoadProjectile( "Abilities." + Abilities.abilityInfo[projectileKey.TypeKey].Slot, Abilities.abilityKeys, projectileKey.TypeKey, Abilities.abilityInfo[projectileKey.TypeKey].Name, projectileKey.ConfigKey, abilityProjectileInfo[projectileKey] );
            }
            foreach( ProjectileConfigKeys<RelicType> projectileKey in relicProjectileInfo.Keys ) {
                LoadProjectile( "Relics", Relics.relicKeys, projectileKey.TypeKey, Relics.relicInfo[projectileKey.TypeKey].Name, projectileKey.ConfigKey, relicProjectileInfo[projectileKey] );
            }
        }

        private static void LoadProjectile<T>( string configFileName, WobSettings.KeyHelper<T> keyHelper, T internalType, string friendlyName, string projectileName, ProjectileInfo projectileInfo ) where T : System.Enum {
            float lifespan      = projectileInfo.LifeSpan;
            float speed         = projectileInfo.Speed;
            float strScale      = projectileInfo.StrScale;
            float intScale      = projectileInfo.IntScale;
            float knockback     = projectileInfo.Knockback;
            ProjectilesHit hits = projectileInfo.ProjectilesHit;
            if( projectileInfo.UseLifeSpan ) {
                if( typeof(T) == typeof( AbilityType ) && (AbilityType)(object)internalType == AbilityType.CrowsNestTalent && projectileName == "Cannonball" ) {
                    lifespan = WobSettings.Get( keyHelper.Get( internalType, "Ship_LifeSpan" ), lifespan );
                } else {
                    WobSettings.Add( new WobSettings.Num<float>( WobMod.configFiles.Get( configFileName ), keyHelper.Get( internalType, projectileName + "_LifeSpan" ), friendlyName + " - " + projectileName + " life span in seconds", lifespan, bounds: (0.1f, 60f) ) );
                    lifespan = WobSettings.Get( keyHelper.Get( internalType, projectileName + "_LifeSpan" ), lifespan );
                }
            }
            if( projectileInfo.UseSpeed ) {
                WobSettings.Add( new WobSettings.Num<float>( WobMod.configFiles.Get( configFileName ), keyHelper.Get( internalType, projectileName + "_Speed" ), friendlyName + " - " + projectileName + " speed", speed, bounds: (0f, 1000f) ) );
                speed = WobSettings.Get( keyHelper.Get( internalType, projectileName + "_Speed" ), speed );
            }
            if( projectileInfo.UseDamage ) {
                WobSettings.Add( new WobSettings.Num<float>( WobMod.configFiles.Get( configFileName ), keyHelper.Get( internalType, projectileName + "_StrScale" ), friendlyName + " - " + projectileName + " damage strength scale", strScale, bounds: (0f, 1000f) ) );
                strScale = WobSettings.Get( keyHelper.Get( internalType, projectileName + "_StrScale" ), strScale );
                WobSettings.Add( new WobSettings.Num<float>( WobMod.configFiles.Get( configFileName ), keyHelper.Get( internalType, projectileName + "_IntScale" ), friendlyName + " - " + projectileName + " damage intelligence scale", intScale, bounds: (0f, 1000f) ) );
                intScale = WobSettings.Get( keyHelper.Get( internalType, projectileName + "_IntScale" ), intScale );
                WobSettings.Add( new WobSettings.Num<float>( WobMod.configFiles.Get( configFileName ), keyHelper.Get( internalType, projectileName + "_Knockback" ), friendlyName + " - " + projectileName + " knockback strength", knockback, bounds: (0f, 1000f) ) );
                knockback = WobSettings.Get( keyHelper.Get( internalType, projectileName + "_Knockback" ), knockback );
            }
            if( projectileInfo.HitProjectiles ) {
                if( typeof( T ) == typeof( AbilityType ) && (AbilityType)(object)internalType == AbilityType.CrowsNestTalent && projectileName == "ShipOnFire" ) {
                    hits = WobSettings.Get( keyHelper.Get( internalType, "Ship_HitProjectiles" ), hits );
                } else {
                    WobSettings.Add( new WobSettings.Enum<ProjectilesHit>( WobMod.configFiles.Get( configFileName ), keyHelper.Get( internalType, projectileName + "_HitProjectiles" ), friendlyName + " - " + projectileName + " destroys projectiles", hits ) );
                    hits = WobSettings.Get( keyHelper.Get( internalType, projectileName + "_HitProjectiles" ), hits );
                }
            }
            foreach( string name in projectileInfo.Names ) {
                projectileLookup.Add( name, new( null, projectileInfo.UseLifeSpan, lifespan, projectileInfo.UseSpeed, speed, projectileInfo.UseDamage, strScale, intScale, knockback, projectileInfo.HitProjectiles, hits ) );
            }
            // Special exception: Lute notes should have the same projectile collision rules when static as when moving
            if( typeof( T ) == typeof( AbilityType ) && (AbilityType)(object)internalType == AbilityType.LuteWeapon && projectileName == "Note" ) {
                projectileLookup.Add( "PlayerLuteStaticWeapon", new( null, false, 0f, false, 0f, false, 0f, 0f, 0f, projectileInfo.HitProjectiles, hits ) );
            }
        }

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // ~~~ PATCHES - PROJECTILES
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // Patch to change the ProjectileData properties on a projectile
        [HarmonyPatch( typeof( Projectile_RL ), nameof( Projectile_RL.ProjectileData ), MethodType.Getter )]
        internal static class Projectile_RL_ProjectileData_Getter_Patch {
            internal static void Postfix( ProjectileData __result ) {
                if( projectileLookup.TryGetValue( __result.Name, out ProjectileInfo projectileSettings ) ) {
                    //WobPlugin.Log( "[Abilities] Edit projectile: " + __result.Name + ", " + __result.DamageType + ", " + __result.StrengthScale + ", " + __result.MagicScale + ", " + __result.ManaGainPerHit + ", " + __result.KnockbackStrength );
                    if( projectileSettings.UseLifeSpan ) {
                        __result.LifeSpan = projectileSettings.LifeSpan;
                    }
                    if( projectileSettings.UseSpeed ) {
                        __result.Speed = projectileSettings.Speed;
                    }
                    if( projectileSettings.UseDamage ) {
                        __result.StrengthScale = projectileSettings.StrScale;
                        __result.MagicScale = projectileSettings.IntScale;
                        __result.KnockbackStrength = projectileSettings.Knockback;
                    }
                }
            }
        }

        // Patch for reads of knockback and speed by triggering the patch in Projectile_RL_ProjectileData_Getter_Patch.Postfix
        [HarmonyPatch( typeof( Projectile_RL ), nameof( Projectile_RL.ResetValues ) )]
        internal static class Projectile_RL_ResetValues_Patch {
            internal static void Prefix( Projectile_RL __instance ) { _ = __instance.ProjectileData; }
        }

        // Patch for reads of lifespan by triggering the patch in Projectile_RL_ProjectileData_Getter_Patch.Postfix
        [HarmonyPatch( typeof( Projectile_RL ), nameof( Projectile_RL.Lifespan ), MethodType.Getter )]
        internal static class Projectile_RL_Lifespan_Getter_Patch {
            internal static void Prefix( Projectile_RL __instance ) { _ = __instance.ProjectileData; }
        }

        // Patch for reads of strength scale by triggering the patch in Projectile_RL_ProjectileData_Getter_Patch.Postfix
        [HarmonyPatch( typeof( Projectile_RL ), nameof( Projectile_RL.ActualDamage ), MethodType.Getter )]
        internal static class Projectile_RL_ActualDamage_Getter_Patch {
            internal static void Prefix( Projectile_RL __instance ) { _ = __instance.ProjectileData; }
        }

        // Patch for reads of projectile collision
        [HarmonyPatch( typeof( Projectile_RL ), nameof( Projectile_RL.CanCollideWithFlags ), MethodType.Getter )]
        internal static class Projectile_RL_CanCollideWithFlags_Getter_Patch {
            internal static void Prefix( Projectile_RL __instance ) {
                if( projectileLookup.TryGetValue( __instance.ProjectileData.Name, out ProjectileInfo projectileSettings ) ) {
                    if( projectileSettings.HitProjectiles ) {
                        Traverse<ProjectileCollisionFlag> m_canCollideWithFlag = Traverse.Create( __instance ).Field<ProjectileCollisionFlag>( "m_canCollideWithFlag" );
                        m_canCollideWithFlag.Value = ( m_canCollideWithFlag.Value & ~(ProjectileCollisionFlag)ProjectilesHit.Large ) | (ProjectileCollisionFlag)projectileSettings.ProjectilesHit;
                    }
                }
            }
        }

    }
}
