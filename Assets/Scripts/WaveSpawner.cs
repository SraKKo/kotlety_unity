using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace RetyKotlety {
public class WaveSpawner : MonoBehaviour {
 public int[] waves={3,5,7};public Vector3[] points;public GameObject enemyPrefab;public int currentWave;public bool started,complete;
 public GameObject[] barriers;
 readonly List<Enemy> alive=new List<Enemy>();float delay;
 void OnTriggerEnter(Collider c){if(!started&&c.GetComponent<PlayerController>()){started=true;SetBarriers(true);SpawnWave();}}
 void SetBarriers(bool enabled){if(barriers!=null)foreach(var b in barriers)if(b)b.SetActive(enabled);}
 void Awake(){SetBarriers(false);}
 void Update(){if(!started||complete)return;alive.RemoveAll(e=>!e);if(alive.Count>0)return;delay-=Time.deltaTime;if(delay<=0)SpawnWave();}
 public void SpawnWave(){if(currentWave>=waves.Length){complete=true;SetBarriers(false);return;}if(points==null||points.Length==0||!enemyPrefab){Debug.LogError("Brak punktów lub prefabu fali",this);complete=true;SetBarriers(false);return;}
  for(int i=0;i<waves[currentWave];i++){Vector3 p=points[i%points.Length];if(NavMesh.SamplePosition(p,out var hit,12,NavMesh.AllAreas))p=hit.position;var enemy=Instantiate(enemyPrefab,p,Quaternion.identity).GetComponent<Enemy>();enemy.gameObject.SetActive(true);alive.Add(enemy);}currentWave++;delay=2;
 }
}
}

