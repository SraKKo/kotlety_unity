using System.Collections.Generic;
using UnityEngine;
namespace RetyKotlety {
public static class SlamCombat {
 public static int Strike(Vector3 point,Vector3 normal,SlamImpact impact){
  float damage=impact.Damage;if(damage<=0)return 0;
  var seen=new HashSet<Enemy>();int hits=0;var origin=point+normal*.2f;
  // Check terrain separately so another enemy cannot shield the whole crowd.
  const int terrainMask=~((1<<8)|(1<<9)|(1<<11));
  foreach(var collider in Physics.OverlapSphere(point,impact.Radius,1<<9,QueryTriggerInteraction.Ignore)){
   var enemy=collider.GetComponentInParent<Enemy>();if(!enemy||seen.Contains(enemy)||enemy.health<=0)continue;
   var target=collider.bounds.center;var delta=target-origin;
   if(Physics.Raycast(origin,delta.normalized,delta.magnitude,terrainMask,QueryTriggerInteraction.Ignore))continue;
   seen.Add(enemy);enemy.Damage(damage,collider.ClosestPoint(origin),normal,false);if(enemy.health>0&&impact.LaunchSpeed>0)enemy.Launch(impact.LaunchSpeed);hits++;
  }
  return hits;
 }
}
}
