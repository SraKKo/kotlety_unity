using UnityEngine;
namespace RetyKotlety {
// Native scene geometry remains editable; actors and navigation are initialized on Play.
public class ArenaBootstrap : MonoBehaviour {
 void Awake(){if(!GetComponentInChildren<GameSession>())WorldFactory.Build(gameObject,false,ArenaLayout.SceneName,transform.Find("Arena - geometria i trasy"));}
}
}
