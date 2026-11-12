using UnityEngine;
namespace RetyKotlety {
public class FpsCounter : MonoBehaviour {
 const string Preference="show_fps";float elapsed,fps;int frames;string display="FPS: --";GUIStyle style;
 public static bool Visible {
  get=>PlayerPrefs.GetInt(Preference,1)!=0;
  set{if(value==Visible)return;PlayerPrefs.SetInt(Preference,value?1:0);PlayerPrefs.Save();}
 }
 void Update(){
  // Count rendered frames with unscaled time, including while gameplay is paused.
  float dt=Time.unscaledDeltaTime;if(dt<=0)return;elapsed+=dt;frames++;
  if(elapsed<.5f)return;fps=frames/elapsed;display="FPS: "+Mathf.RoundToInt(fps);frames=0;elapsed=0;
 }
 void OnGUI(){
  if(!Visible)return;
  if(style==null)style=new GUIStyle(GUI.skin.label){fontSize=18,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};
  var matrix=GUI.matrix;var color=GUI.color;int depth=GUI.depth;GUI.matrix=Matrix4x4.identity;GUI.depth=-250;
  var rect=new Rect(Screen.width-128,12,116,32);GUI.color=new Color(0,0,0,.72f);GUI.DrawTexture(rect,Texture2D.whiteTexture);
  GUI.color=Color.white;style.normal.textColor=fps>=55?new Color(.65f,1,.65f):fps>=30?new Color(1,.85f,.35f):new Color(1,.45f,.4f);GUI.Label(rect,display,style);
  GUI.matrix=matrix;GUI.color=color;GUI.depth=depth;
 }
}
}
