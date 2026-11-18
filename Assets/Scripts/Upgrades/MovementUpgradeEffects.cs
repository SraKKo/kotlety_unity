using UnityEngine;
namespace RetyKotlety {
// Samples the ground after movement; time and distance gates avoid pouring while stationary.
public class MovementUpgradeEffects : MonoBehaviour {
 PlayerUpgrades upgrades;MielonyTuning grease;Vector3 lastPatch;float nextPatch;bool laying;
 void Awake(){
  upgrades=GetComponent<PlayerUpgrades>();grease=ScriptableObject.CreateInstance<MielonyTuning>();
  grease.greaseRadius=.7f;grease.greaseDuration=3;grease.tickDamage=2;grease.tickInterval=.25f;grease.slowMultiplier=.8f;
 }
 public void Step(bool sliding,bool grounded,bool dashStarted,Vector3 feet){
  if(dashStarted)upgrades.OnDash();
  if(!upgrades.GreasySlide||!sliding||!grounded){laying=false;return;}
  if(laying&&(Time.time<nextPatch||(feet-lastPatch).sqrMagnitude<1.44f))return;
  const int groundMask=~((1<<8)|(1<<9)|(1<<11));
  if(!Physics.Raycast(feet+Vector3.up*.5f,Vector3.down,out var hit,1,groundMask,QueryTriggerInteraction.Ignore)||hit.normal.y<.5f)return;
  GreasePatch.Spawn(hit.point,hit.normal,grease,upgrades.Bonuses);
  lastPatch=feet;nextPatch=Time.time+.15f;laying=true;
 }
 void OnDestroy(){if(grease)Destroy(grease);}
}
}
