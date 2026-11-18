using System.Collections.Generic;
using UnityEngine;
namespace RetyKotlety {
// Each projectile owns this history across its outbound path, orbit and return.
public sealed class AttackDamage<TTarget> {
 readonly HashSet<TTarget> targets=new HashSet<TTarget>();
 public CombatBonuses Bonuses {get;}
 public float Distance {get;private set;}
 public AttackDamage(CombatBonuses bonuses){Bonuses=bonuses;}
 public void SetDistance(float distance){Distance=Mathf.Max(Distance,distance);}
 public void AddDistance(float distance){Distance+=Mathf.Max(0,distance);}
 public float Hit(float damage,TTarget target,bool airborne){targets.Add(target);return Bonuses.Damage(damage,airborne,false,Mathf.Max(0,targets.Count-1),Distance);}
}
}
