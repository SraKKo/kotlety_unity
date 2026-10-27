using UnityEngine;
namespace RetyKotlety {
public class MigrationBootstrap : MonoBehaviour {
 public bool menu;
 void Awake(){if(transform.childCount==0)WorldFactory.Build(gameObject,menu);}
}
}
