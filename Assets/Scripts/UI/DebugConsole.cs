using System;
using System.Collections.Generic;
using UnityEngine;
namespace RetyKotlety {
public class DebugConsole : MonoBehaviour {
 GameSession session;string input="";readonly List<string> lines=new List<string>();Vector2 scroll;bool focus;
 GUIStyle text,field;UpgradeDefinition[] definitions;
 void Awake(){session=GetComponent<GameSession>();definitions=Resources.LoadAll<UpgradeDefinition>("Upgrades");Array.Sort(definitions,(a,b)=>string.CompareOrdinal(a.id,b.id));Print("Konsola gotowa. help / list / give 1.1 / clear");}
 void Update(){
  if(!session||session.menuScene||session.won)return;
  bool toggle=Input.GetKeyDown(KeyCode.BackQuote)||Input.inputString.Contains("§")||Input.inputString.Contains("~");
  if(toggle){if(session.debugOpen)Close();else if(!session.paused&&!session.upgradeOpen&&!session.buildOpen)Open();}
  else if(session.debugOpen&&Input.GetKeyDown(KeyCode.Escape))Close();
 }
 void Open(){session.debugOpen=true;session.SetPause(true);focus=true;input="";}
 void Close(){session.debugOpen=false;session.modalClosedFrame=Time.frameCount;session.SetPause(false);}
 void OnDisable(){if(session&&session.debugOpen&&GameSession.Instance==session)Close();}
 void Print(string line){lines.Add(line);if(lines.Count>100)lines.RemoveAt(0);scroll.y=float.MaxValue;}
 public string Execute(string command){
  var parts=command.Trim().Split(new[]{' ','\t'},StringSplitOptions.RemoveEmptyEntries);if(parts.Length==0)return "";
  string verb=parts[0].ToLowerInvariant();
  if(verb=="clear"&&parts.Length==1){lines.Clear();return "";}
  if(verb=="help"&&parts.Length==1)return "give <id> - nadaj ulepszenie | give <bron> - nadaj bron (schabowy, chrupiaca, mielony, losos) | list - lista ID | clear - wyczysc | § / ~ / Esc - zamknij";
  if(verb=="list"&&parts.Length==1){var result=new List<string>();foreach(var definition in definitions)result.Add(definition.id+"  "+definition.title);return string.Join("\n",result);}
  if(verb!="give"||parts.Length!=2)return "Nieznana komenda lub argumenty. Wpisz help.";
  if(!session||!session.player)return "Brak aktywnego gracza.";
  // Weapons can be granted by name, e.g. "give losos".
  if(!char.IsDigit(parts[1][0])&&Enum.TryParse<FoodKind>(parts[1],true,out var food)){if(food==FoodKind.Pan)return "Patelnie masz zawsze.";session.player.Receive(food);return "Dodano: "+food+".";}
  UpgradeDefinition found=null;foreach(var definition in definitions)if(string.Equals(definition.id,parts[1],StringComparison.OrdinalIgnoreCase)){found=definition;break;}
  if(!found)return "Nieznane ID: "+parts[1]+". Wpisz list.";
  var upgrades=session.player.GetComponent<PlayerUpgrades>();if(!upgrades)upgrades=session.player.gameObject.AddComponent<PlayerUpgrades>();
  if(!upgrades.Grant(found))return "Masz juz ulepszenie "+found.id+".";
  session.player.Receive(found.Weapon);return "Dodano "+found.id+": "+found.title+". Sprawdz build pod Tab.";
 }
 void Submit(){
  string command=input.Trim();if(command.Length==0){focus=true;return;}
  Print("> "+command);string result=Execute(command);if(!string.IsNullOrEmpty(result))Print(result);input="";focus=true;
 }
 void OnGUI(){
  if(!session||!session.debugOpen)return;
  // TextField consumes Return; capture and use it before drawing the control.
  var current=Event.current;bool submit=current.type==EventType.KeyDown&&(current.keyCode==KeyCode.Return||current.keyCode==KeyCode.KeypadEnter);
  if(submit)current.Use();
  if(text==null){text=new GUIStyle(GUI.skin.label){fontSize=16,wordWrap=true,richText=false};text.normal.textColor=new Color(.75f,1,.8f);field=new GUIStyle(GUI.skin.textField){fontSize=19};}
  var matrix=GUI.matrix;var color=GUI.color;int depth=GUI.depth;GUI.depth=-200;
  GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/720f,1));
  GUI.color=new Color(.015f,.02f,.025f,.98f);GUI.DrawTexture(new Rect(0,0,1280,360),Texture2D.whiteTexture);GUI.color=Color.white;
  GUI.Label(new Rect(22,12,1230,30),"RETY KOTLETY / DEBUG   |   ENTER: wyslij   |   § / ~ / Esc: powrot do gry",text);
  string output=string.Join("\n",lines);float height=Mathf.Max(245,text.CalcHeight(new GUIContent(output),1210));
  scroll=GUI.BeginScrollView(new Rect(22,50,1235,247),scroll,new Rect(0,0,1210,height));GUI.Label(new Rect(0,0,1210,height),output,text);GUI.EndScrollView();
  GUI.Label(new Rect(22,311,25,35),">",text);GUI.SetNextControlName("DebugCommand");input=GUI.TextField(new Rect(50,305,1040,38),input,256,field).Replace("§","").Replace("`","").Replace("~","");
  if(focus){GUI.FocusControl("DebugCommand");focus=false;}
  bool clicked=GUI.Button(new Rect(1105,305,150,38),"WYSLIJ");
  if(submit||clicked)Submit();
  GUI.matrix=matrix;GUI.color=color;GUI.depth=depth;
 }
}
}
