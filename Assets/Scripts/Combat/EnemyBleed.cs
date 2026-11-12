using UnityEngine;
namespace RetyKotlety {
[RequireComponent(typeof(Enemy))]
public class EnemyBleed : MonoBehaviour {
 readonly BleedState state=new BleedState();Enemy enemy;
 public void Apply(float damage,float interval,float duration){if(!enemy)enemy=GetComponent<Enemy>();if(enemy&&enemy.health>0)state.Apply(damage,interval,duration);}
 void Update(){
  if(!state.Active||Time.deltaTime<=0)return;
  if(!enemy||enemy.health<=0)return;
  int ticks=state.Advance(Time.deltaTime);for(int i=0;i<ticks&&enemy.health>0;i++)enemy.Damage(state.TickDamage);
 }
}
}
