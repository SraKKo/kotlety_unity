using UnityEngine;
namespace RetyKotlety {
public class ArenaGate : MonoBehaviour {
 public GameObject[] barriers; public WaveSpawner[] spawners; bool entered;
 void OnTriggerEnter(Collider other){if(!entered&&other.GetComponent<PlayerController>()){entered=true;foreach(var wall in barriers)wall.SetActive(true);}}
 void Update(){if(!entered)return;foreach(var spawner in spawners)if(!spawner.complete)return;foreach(var wall in barriers)wall.SetActive(false);enabled=false;}
}
}
