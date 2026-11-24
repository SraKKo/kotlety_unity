using System;
using UnityEngine;
namespace RetyKotlety {
public class LevelShop : MonoBehaviour {
 GameSession session;UpgradeDefinition[] offers;GUIStyle title,body,small,button,hudText;Vector2 scroll;
 PlayerProgression Progression=>session&&session.player?session.player.GetComponent<PlayerProgression>():null;
 void Awake(){session=GetComponent<GameSession>();offers=Array.FindAll(Resources.LoadAll<UpgradeDefinition>("Upgrades"),d=>d&&d.IsShopUpgrade);Array.Sort(offers,(a,b)=>string.CompareOrdinal(a.id,b.id));}
 bool CanOpen=>session&&!session.menuScene&&!session.won&&!session.paused&&!session.upgradeOpen&&!session.buildOpen&&!session.debugOpen&&Time.frameCount!=session.modalClosedFrame;
 void Update(){
  if(!session)return;
  if(session.shopOpen){if(Input.GetKeyDown(KeyCode.B)||Input.GetKeyDown(KeyCode.Escape))Close();return;}
  var progression=Progression;if(!progression||!progression.State.ShopUnlocked||!CanOpen)return;
  if(progression.LevelNoticePending||Input.GetKeyDown(KeyCode.B))Open();
 }
 public void Open(){var progression=Progression;if(!CanOpen||!progression||!progression.State.ShopUnlocked)return;progression.AcknowledgeLevel();session.shopOpen=true;session.SetPause(true);}
 public void Close(){if(!session||!session.shopOpen)return;session.shopOpen=false;session.modalClosedFrame=Time.frameCount;session.SetPause(false);}
 void OnDisable(){Close();}
 static void Panel(Rect r,Color color){var old=GUI.color;GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
 void Styles(){if(title!=null)return;var font=Resources.Load<Font>("UI/PixelAE-Bold");title=new GUIStyle(GUI.skin.label){font=font,fontSize=30};body=new GUIStyle(title){fontSize=18,wordWrap=true};small=new GUIStyle(body){fontSize=15};button=new GUIStyle(GUI.skin.button){font=font,fontSize=18};hudText=new GUIStyle(small){wordWrap=false};}
 void OnGUI(){
  var progression=Progression;if(!session||session.menuScene||!progression)return;Styles();
  var matrix=GUI.matrix;var oldColor=GUI.color;int depth=GUI.depth;
  GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/720f,1));
  var state=progression.State;
  if(!session.shopOpen){
   if(!session.paused&&!session.won){
    GUI.depth=-20;float scale=GameHud.HudScale;hudText.fontSize=Mathf.RoundToInt(15*scale);var bar=new Rect(640-230*scale,20,460*scale,66*scale);Panel(bar,new Color(.03f,.04f,.06f,.85f));
    GUI.Label(new Rect(bar.x+10*scale,bar.y+3*scale,bar.width-20*scale,25*scale),"LV "+state.Level+"   XP "+state.Experience+" / "+state.ExperienceRequired+"   PKT "+state.Points,hudText);
    Panel(new Rect(bar.x+10*scale,bar.y+30*scale,bar.width-20*scale,5*scale),new Color(.2f,.22f,.25f));Panel(new Rect(bar.x+10*scale,bar.y+30*scale,(bar.width-20*scale)*state.Experience/state.ExperienceRequired,5*scale),new Color(.3f,.85f,1));
    GUI.Label(new Rect(bar.x+10*scale,bar.y+38*scale,bar.width-20*scale,22*scale),state.ShopUnlocked?"B: sklep":"Awans odblokuje sklep",hudText);
   }
  }else {
   GUI.depth=-100;Panel(new Rect(0,0,1280,720),new Color(.015f,.02f,.03f,.96f));Panel(new Rect(100,50,1080,620),new Color(.055f,.065f,.09f));
   GUI.Label(new Rect(130,76,800,45),"SKLEP / POZIOM "+state.Level,title);GUI.Label(new Rect(130,126,990,35),"Punkty: "+state.Points+"   /   Kupuj stacki za punkty z wrogow. B / ESC: powrot",body);
   var upgrades=session.player.GetComponent<PlayerUpgrades>();
   scroll=GUI.BeginScrollView(new Rect(125,178,1030,405),scroll,new Rect(0,0,1000,Mathf.Max(405,offers.Length*112)));
   for(int i=0;i<offers.Length;i++){
    var offer=offers[i];float y=i*112;int owned=upgrades.Count(offer),price=progression.Price(offer);bool available=upgrades.CanAcquire(offer);
    Panel(new Rect(0,y,995,103),new Color(.105f,.12f,.16f));Panel(new Rect(0,y,4,103),offer.color);
    GUI.Label(new Rect(16,y+5,740,28),offer.id+" / "+offer.title+"  x"+owned,body);GUI.Label(new Rect(16,y+34,740,62),offer.description,small);
    bool enabled=GUI.enabled;GUI.enabled=available&&state.Points>=price;
    if(GUI.Button(new Rect(790,y+22,185,55),available?"KUP / "+price+" PKT":"ZDOBYTE",button))progression.TryBuy(offer);
    GUI.enabled=enabled;
   }
   GUI.EndScrollView();
   if(GUI.Button(new Rect(430,609,420,42),"WROC DO WALKI",button))Close();
  }
  GUI.matrix=matrix;GUI.color=oldColor;GUI.depth=depth;
 }
}
}
