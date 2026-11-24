using UnityEngine;
namespace RetyKotlety {
// A snapshot per attack, so already airborne projectiles never change after collecting an item.
public readonly struct CombatBonuses {
 public readonly float DamageBonus,AreaBonus,DotBonus,DistinctBonus,JourneyBonus,AirborneBonus;
 public CombatBonuses(float damage,float area,float dot,float distinct,float journey,float airborne){DamageBonus=Mathf.Max(0,damage);AreaBonus=Mathf.Max(0,area);DotBonus=Mathf.Max(0,dot);DistinctBonus=Mathf.Max(0,distinct);JourneyBonus=Mathf.Max(0,journey);AirborneBonus=Mathf.Max(0,airborne);}
 public float AreaMultiplier=>1+AreaBonus;
 public float Damage(float amount,bool airborne=false,bool dot=false,int previousTargets=0,float distance=0){
  float path=dot?0:Mathf.Min(1,JourneyBonus*Mathf.Floor(Mathf.Max(0,distance)/5));
  float distinct=dot?0:Mathf.Min(1,DistinctBonus*Mathf.Max(0,previousTargets));
  return Mathf.Max(0,amount+(dot?DotBonus:0))*(1+DamageBonus)*(1+path+distinct)*(1+(airborne?AirborneBonus:0));
 }
 public static CombatBonuses FromOwner(GameObject owner){var upgrades=owner?owner.GetComponent<PlayerUpgrades>():null;return upgrades?upgrades.Bonuses:default;}
}
}
