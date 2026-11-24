using UnityEngine;
namespace RetyKotlety {
public class ProgressionArenaBootstrap : MonoBehaviour {
 public const string SceneName="ProgressionArena";
 void Awake(){
  if(GetComponentInChildren<GameSession>())return;
  var session=WorldFactory.Build(gameObject,false,SceneName,transform.Find("Arena - geometria i trasy"));
  RenderSettings.ambientLight=new Color(.65f,.7f,.76f);RenderSettings.fog=false;RenderSettings.skybox=null;
  session.player.view.backgroundColor=new Color(.10f,.15f,.21f);session.player.view.clearFlags=CameraClearFlags.SolidColor;
  if(!GetComponent<ProgressionArenaWaves>())gameObject.AddComponent<ProgressionArenaWaves>();
 }
}
}
