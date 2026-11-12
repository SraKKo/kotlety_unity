using UnityEngine;
namespace RetyKotlety {
// Per-player values, separate from the shared asset. Future upgrades can produce a new snapshot.
public readonly struct SlamStats {
 public readonly float BaseRadius,RadiusPerMeter,MaxRadius,DamagePerMeter,MaxDamage;
 public readonly float LaunchThreshold,LaunchSpeed,MaxLaunchSpeed,FullLaunchHeight,ShakeAmplitude,ShakeDuration;
 public SlamStats(float baseRadius,float radiusPerMeter,float maxRadius,float damagePerMeter,float maxDamage,float launchThreshold=3,float launchSpeed=6,float maxLaunchSpeed=9,float fullLaunchHeight=7.5f,float shakeAmplitude=.035f,float shakeDuration=.18f){
  LaunchThreshold=Mathf.Max(0,launchThreshold);LaunchSpeed=Mathf.Max(0,launchSpeed);MaxLaunchSpeed=Mathf.Max(LaunchSpeed,maxLaunchSpeed);FullLaunchHeight=Mathf.Max(LaunchThreshold+.01f,fullLaunchHeight);ShakeAmplitude=Mathf.Max(0,shakeAmplitude);ShakeDuration=Mathf.Max(0,shakeDuration);
  MaxRadius=Mathf.Max(0,maxRadius);BaseRadius=Mathf.Clamp(baseRadius,0,MaxRadius);
  RadiusPerMeter=Mathf.Max(0,radiusPerMeter);DamagePerMeter=Mathf.Max(0,damagePerMeter);MaxDamage=Mathf.Max(0,maxDamage);
 }
 public SlamImpact Resolve(float fallHeight){
  float height=Mathf.Max(0,fallHeight);
  float launch=height>=LaunchThreshold?Mathf.Lerp(LaunchSpeed,MaxLaunchSpeed,Mathf.InverseLerp(LaunchThreshold,FullLaunchHeight,height)):0;
  return new SlamImpact(Mathf.Min(MaxRadius,BaseRadius+height*RadiusPerMeter),Mathf.Min(MaxDamage,height*DamagePerMeter),MaxDamage,launch,ShakeAmplitude,ShakeDuration);
 }
 public static SlamStats Default=>new SlamStats(1f,4f/7.5f,5f,12f,90f);
}
// One resolved impact is shared by damage and presentation so their radii cannot diverge.
public readonly struct SlamImpact {
 public readonly float Radius,Damage,Power,LaunchSpeed,ShakeAmplitude,ShakeDuration;
 public SlamImpact(float radius,float damage,float maxDamage,float launchSpeed=0,float shakeAmplitude=0,float shakeDuration=0){Radius=radius;Damage=damage;Power=maxDamage>0?Mathf.Clamp01(damage/maxDamage):0;LaunchSpeed=launchSpeed;ShakeAmplitude=shakeAmplitude*Mathf.Lerp(.3f,1,Power);ShakeDuration=shakeDuration;}
}
}
