using System.Collections.Generic;
using UnityEngine;
namespace RetyKotlety {
// Run-scoped state: resetting the scene resets upgrades; shared definitions are never mutated.
public class PlayerUpgrades : MonoBehaviour {
 readonly Dictionary<string,int> acquired=new Dictionary<string,int>();
 float damageBonus,areaBonus,dotBonus,distinctBonus,journeyBonus,airborneBonus;
 public CombatBonuses Bonuses=>new CombatBonuses(damageBonus,areaBonus,dotBonus,distinctBonus,journeyBonus,airborneBonus);
 public int PanBowlingStacks {get;private set;}
 public bool GreasySlide {get;private set;}
 public bool DashHaste {get;private set;}
 float dashHasteUntil;
 public float BonusHealth {get;private set;}
 public float RegenPerSecond {get;private set;}
 public float ProjectileSpeedMultiplier {get;private set;}=1;
 public float BaseAttackRate {get;private set;}=1;
 public float DashSpeedMultiplier=>DashHaste&&Time.time<dashHasteUntil?1.35f:1;
 public float AttackSpeedMultiplier=>BaseAttackRate*DashSpeedMultiplier;
 public static float ShotSpeedFor(GameObject owner){
  return owner && owner.TryGetComponent<PlayerUpgrades>(out var upgrades) ? upgrades.ProjectileSpeedMultiplier : 1;
 }
 public void OnDash(){if(DashHaste)dashHasteUntil=Time.time+3;}
 public int ExtraProjectiles {get;private set;}
 public int TotalStacks {get;private set;}
 public int Count(UpgradeDefinition definition){return definition&&acquired.TryGetValue(definition.id,out var count)?count:0;}
 public bool CanAcquire(UpgradeDefinition definition){return definition&&!string.IsNullOrEmpty(definition.id)&&(!Has(definition)||definition.stackable&&(definition.maxStacks<=0||Count(definition)<definition.maxStacks));}
 readonly List<UpgradeDefinition> definitions=new List<UpgradeDefinition>();
 public IReadOnlyList<UpgradeDefinition> Acquired=>definitions;
 public int SchabowyExtraTargets {get;private set;}
 public float SchabowyTargetRange {get;private set;}
 public int MielonyGroundBounces {get;private set;}
 public int LososOrbitLaps {get;private set;}
 public bool ChrupiacaPiercing {get;private set;}
 public float ChrupiacaBleedDamage {get;private set;}
 public float ChrupiacaBleedInterval {get;private set;}
 public float ChrupiacaBleedDuration {get;private set;}
 public bool Has(UpgradeDefinition definition){return definition&&acquired.ContainsKey(definition.id);}
 public bool Grant(UpgradeDefinition definition){
  if(!CanAcquire(definition))return false;int count=Count(definition);acquired[definition.id]=count+1;TotalStacks++;
  switch(definition.effect){case UpgradeEffect.SchabowyRicochet:SchabowyExtraTargets=Mathf.Max(SchabowyExtraTargets,definition.extraTargets);SchabowyTargetRange=Mathf.Max(SchabowyTargetRange,definition.targetRange);break;
   case UpgradeEffect.MielonyBounce:MielonyGroundBounces=Mathf.Max(MielonyGroundBounces,definition.groundBounces);break;
   case UpgradeEffect.LososOrbit:LososOrbitLaps=Mathf.Max(LososOrbitLaps,definition.orbitLaps);break;
   case UpgradeEffect.MaxHealth:
    BonusHealth+=definition.increment;
    if(TryGetComponent<PlayerController>(out var player))player.RestoreHealth(definition.increment);
    break;
   case UpgradeEffect.ShotSpeed:ProjectileSpeedMultiplier+=definition.increment;break;
   case UpgradeEffect.AttackRate:BaseAttackRate+=definition.increment;break;
   case UpgradeEffect.Regeneration:RegenPerSecond+=definition.increment;break;
   case UpgradeEffect.DoubleBreading:damageBonus+=definition.increment;break;
   case UpgradeEffect.LargePortion:areaBonus+=definition.increment;break;
   case UpgradeEffect.HotGrease:dotBonus+=definition.increment;break;
   case UpgradeEffect.DailySpecial:distinctBonus+=definition.increment;break;
   case UpgradeEffect.Takeaway:journeyBonus+=definition.increment;break;
   case UpgradeEffect.ChefLaunch:airborneBonus+=definition.increment;break;
   case UpgradeEffect.PanBowling:PanBowlingStacks++;break;
   case UpgradeEffect.GreasySlide:GreasySlide=true;break;
   case UpgradeEffect.DashHaste:DashHaste=true;break;
   case UpgradeEffect.ExtraProjectiles:ExtraProjectiles+=Mathf.Max(1,Mathf.RoundToInt(definition.increment));break;
   case UpgradeEffect.ChrupiacaPiercing:ChrupiacaPiercing=true;break;
   case UpgradeEffect.ChrupiacaBleed:ChrupiacaBleedDamage=definition.bleedDamage;ChrupiacaBleedInterval=definition.bleedInterval;ChrupiacaBleedDuration=definition.bleedDuration;break;
  }
  if(count==0)definitions.Add(definition);return true;
 }
 public void ConfigureLosos(LososBoomerang boomerang){if(boomerang)boomerang.SetOrbit(LososOrbitLaps);}
 public void ConfigureMielony(MielonyProjectile projectile){if(projectile)projectile.SetGroundBounces(MielonyGroundBounces);}
 public void ConfigureChrupiaca(Projectile projectile){if(!projectile)return;projectile.SetPiercing(ChrupiacaPiercing);if(ChrupiacaBleedDamage>0)projectile.SetBleed(ChrupiacaBleedDamage,ChrupiacaBleedInterval,ChrupiacaBleedDuration);}
 public void ConfigureSchabowy(Projectile projectile){if(projectile&&SchabowyExtraTargets>0)projectile.EnableEnemyRicochet(SchabowyExtraTargets,SchabowyTargetRange);}
}
}
