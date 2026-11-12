using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace RetyKotlety {
public class ArenaPractice : MonoBehaviour {
 public bool pressure;public int maxEnemies=18;public float spawnInterval=5;
 readonly List<Enemy> enemies=new List<Enemy>();float nextSpawn;int spawned;
 void Start(){var player=GameSession.Instance.player;player.Receive(FoodKind.Schabowy);player.Receive(FoodKind.Chrupiaca);player.Equip(FoodKind.Schabowy);}
 void Update(){var session=GameSession.Instance;if(!session||session.paused)return;
  if(Input.GetKeyDown(KeyCode.T)){pressure=!pressure;nextSpawn=Time.time+1;}
  enemies.RemoveAll(e=>!e);
  if(!pressure||Time.time<nextSpawn||enemies.Count>=maxEnemies)return;
  nextSpawn=Time.time+spawnInterval;
  for(int i=0;i<3&&enemies.Count<maxEnemies;i++){
   Vector3 position=ArenaLayout.SpawnPoints[spawned%ArenaLayout.SpawnPoints.Length];spawned++;
   if(Vector3.Distance(position,session.player.transform.position)<12)continue;
   if(!NavMesh.SamplePosition(position,out var hit,3,NavMesh.AllAreas))continue;
   bool shooting=spawned%3==0;var source=shooting?session.shootingPrefab:session.meleePrefab;
   var enemy=Instantiate(source,hit.position,Quaternion.identity).GetComponent<Enemy>();enemy.sightRange=100;enemy.gameObject.SetActive(true);enemies.Add(enemy);
  }
 }
 void OnGUI(){if(!GameSession.Instance||GameSession.Instance.paused)return;GUI.Label(new Rect(18,18,520,48),"MOVEMENT ARENA | T: presja "+(pressure?"WLACZONA":"WYLACZONA")+" | Wrogowie: "+enemies.Count+"\nSandbox tras i walki - bez XP i limitu czasu");}
}
}
