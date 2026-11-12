using UnityEngine;
namespace RetyKotlety {
// Per-player values, separate from the shared asset. Future upgrades can produce a new snapshot.
public readonly struct SlamStats {
 public readonly float BaseRadius,RadiusPerMeter,MaxRadius,DamagePerMeter,MaxDamage;
 public SlamStats(float baseRadius,float radiusPerMeter,float maxRadius,float damagePerMeter,float maxDamage){
  MaxRadius=Mathf.Max(0,maxRadius);BaseRadius=Mathf.Clamp(baseRadius,0,MaxRadius);
  RadiusPerMeter=Mathf.Max(0,radiusPerMeter);DamagePerMeter=Mathf.Max(0,damagePerMeter);MaxDamage=Mathf.Max(0,maxDamage);
 }
 public SlamImpact Resolve(float fallHeight){
  float height=Mathf.Max(0,fallHeight);
  return new SlamImpact(Mathf.Min(MaxRadius,BaseRadius+height*RadiusPerMeter),Mathf.Min(MaxDamage,height*DamagePerMeter),MaxDamage);
 }
 public static SlamStats Default=>new SlamStats(1f,4f/7.5f,5f,12f,90f);
}
// One resolved impact is shared by damage and presentation so their radii cannot diverge.
public readonly struct SlamImpact {
 public readonly float Radius,Damage,Power;
 public SlamImpact(float radius,float damage,float maxDamage){Radius=radius;Damage=damage;Power=maxDamage>0?Mathf.Clamp01(damage/maxDamage):0;}
}
}
