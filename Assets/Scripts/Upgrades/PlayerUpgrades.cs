using System.Collections.Generic;
using UnityEngine;
namespace RetyKotlety {
// Run-scoped state: resetting the scene resets upgrades; shared definitions are never mutated.
public class PlayerUpgrades : MonoBehaviour {
 readonly HashSet<string> acquired=new HashSet<string>();
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
 public bool Has(UpgradeDefinition definition){return definition&&acquired.Contains(definition.id);}
 public bool Grant(UpgradeDefinition definition){
  if(!definition||string.IsNullOrEmpty(definition.id)||!acquired.Add(definition.id))return false;
  switch(definition.effect){case UpgradeEffect.SchabowyRicochet:SchabowyExtraTargets=Mathf.Max(SchabowyExtraTargets,definition.extraTargets);SchabowyTargetRange=Mathf.Max(SchabowyTargetRange,definition.targetRange);break;
   case UpgradeEffect.MielonyBounce:MielonyGroundBounces=Mathf.Max(MielonyGroundBounces,definition.groundBounces);break;
   case UpgradeEffect.LososOrbit:LososOrbitLaps=Mathf.Max(LososOrbitLaps,definition.orbitLaps);break;
   case UpgradeEffect.ChrupiacaPiercing:ChrupiacaPiercing=true;break;
   case UpgradeEffect.ChrupiacaBleed:ChrupiacaBleedDamage=definition.bleedDamage;ChrupiacaBleedInterval=definition.bleedInterval;ChrupiacaBleedDuration=definition.bleedDuration;break;
  }
  definitions.Add(definition);return true;
 }
 public void ConfigureLosos(LososBoomerang boomerang){if(boomerang)boomerang.SetOrbit(LososOrbitLaps);}
 public void ConfigureMielony(MielonyProjectile projectile){if(projectile)projectile.SetGroundBounces(MielonyGroundBounces);}
 public void ConfigureChrupiaca(Projectile projectile){if(!projectile)return;projectile.SetPiercing(ChrupiacaPiercing);if(ChrupiacaBleedDamage>0)projectile.SetBleed(ChrupiacaBleedDamage,ChrupiacaBleedInterval,ChrupiacaBleedDuration);}
 public void ConfigureSchabowy(Projectile projectile){if(projectile&&SchabowyExtraTargets>0)projectile.EnableEnemyRicochet(SchabowyExtraTargets,SchabowyTargetRange);}
}
}
