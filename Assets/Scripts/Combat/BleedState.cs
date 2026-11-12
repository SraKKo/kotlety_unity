using UnityEngine;
namespace RetyKotlety {
// One refreshable status per enemy; preserve the next tick when a fast volley refreshes it.
public sealed class BleedState {
 float remaining,elapsed,interval;
 public float TickDamage {get;private set;}
 public bool Active=>remaining>0;
 public void Apply(float damage,float tickInterval,float duration){
  if(damage<=0||tickInterval<=0||duration<=0)return;
  if(!Active){elapsed=0;TickDamage=damage;interval=tickInterval;}else TickDamage=Mathf.Max(TickDamage,damage);
  remaining=Mathf.Max(remaining,duration);
 }
 public int Advance(float dt){
  if(!Active||dt<=0)return 0;
  float step=Mathf.Min(dt,remaining);remaining=Mathf.Max(0,remaining-step);elapsed+=step;
  int ticks=Mathf.FloorToInt((elapsed+.000001f)/interval);elapsed=Mathf.Max(0,elapsed-ticks*interval);return ticks;
 }
}
}
