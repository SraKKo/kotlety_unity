using UnityEngine;
namespace RetyKotlety {
public class BuildMenu : MonoBehaviour {
 GameSession session;GUIStyle title,heading,body,muted,button;Texture2D pan,schabowy,chrupiaca,mielony;Vector2 scroll;
 void Awake(){session=GetComponent<GameSession>();pan=Resources.Load<Texture2D>("UI/patelnia");schabowy=Resources.Load<Texture2D>("UI/kotlet_schabowy");chrupiaca=Resources.Load<Texture2D>("UI/kotlet_mielony");mielony=Resources.Load<Texture2D>("UI/kotlet_mielony");}
 static void Panel(Rect rect,Color color){var previous=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=previous;}
 void Styles(){
  if(title!=null)return;var font=Resources.Load<Font>("UI/PixelAE-Bold");
  title=new GUIStyle(GUI.skin.label){font=font,fontSize=34};
  heading=new GUIStyle(title){fontSize=21};body=new GUIStyle(title){fontSize=16,wordWrap=true};muted=new GUIStyle(body){fontSize=14};muted.normal.textColor=new Color(.64f,.69f,.76f);button=new GUIStyle(GUI.skin.button){font=font,fontSize=17};
 }
 void Weapon(Rect rect,Texture2D icon,string name,string detail,bool owned,bool selected){
  Panel(rect,selected?new Color(.2f,.17f,.10f):new Color(.10f,.12f,.16f));
  Panel(new Rect(rect.x,rect.y,4,rect.height),selected?new Color(1,.78f,.22f):new Color(.22f,.27f,.34f));
  var old=GUI.color;if(!owned)GUI.color=new Color(.4f,.4f,.4f);
  if(icon)GUI.DrawTexture(new Rect(rect.x+14,rect.y+8,66,58),icon,ScaleMode.ScaleToFit);GUI.color=old;
  GUI.Label(new Rect(rect.x+92,rect.y+10,210,25),name,heading);
  GUI.Label(new Rect(rect.x+92,rect.y+40,222,30),owned?(selected?"W RECE  |  ":"")+detail:"NIEZDOBYTA",muted);
 }
 void OnGUI(){
  if(!session||!session.buildOpen||!session.player)return;Styles();var player=session.player;var upgrades=player.GetComponent<PlayerUpgrades>();
  var oldMatrix=GUI.matrix;var oldColor=GUI.color;int oldDepth=GUI.depth;GUI.depth=-90;
  GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/720f,1));
  Panel(new Rect(0,0,1280,720),new Color(.015f,.02f,.03f,.94f));Panel(new Rect(90,55,1100,610),new Color(.045f,.055f,.075f));
  Panel(new Rect(90,55,1100,4),new Color(1,.78f,.22f));
  GUI.Label(new Rect(120,82,700,52),"TWOJ BUILD",title);
  GUI.Label(new Rect(120,135,850,30),"Biezace podejscie  /  Gra zatrzymana  /  TAB lub ESC: powrot",muted);
  if(GUI.Button(new Rect(1000,90,155,39),"ZAMKNIJ",button))session.SetBuildOpen(false);
  GUI.Label(new Rect(120,181,340,32),"BRONIE",heading);
  Weapon(new Rect(120,220,340,70),pan,"PATELNIA","Walka wrecz",true,player.weapon==FoodKind.Pan);
  string ricochet=upgrades&&upgrades.SchabowyExtraTargets>0?"+"+upgrades.SchabowyExtraTargets+" cele / rykoszet":"Pojedynczy pocisk";
  Weapon(new Rect(120,300,340,70),schabowy,"SCHABOWY",ricochet,player.hasSchabowy,player.weapon==FoodKind.Schabowy);
  string chrupiacaInfo=upgrades&&upgrades.ChrupiacaPiercing?"Piercing":"3 pociski / wachlarz";if(upgrades&&upgrades.ChrupiacaBleedDamage>0)chrupiacaInfo+=" + bleed";
  Weapon(new Rect(120,380,340,70),chrupiaca,"CHRUPIACA",chrupiacaInfo,player.hasChrupiaca,player.weapon==FoodKind.Chrupiaca);
  Weapon(new Rect(120,460,340,70),mielony,"MIELONY",upgrades&&upgrades.MielonyGroundBounces>0?"Bounce x"+upgrades.MielonyGroundBounces+" / DoT + slow":"Luk / DoT + slow",player.hasMielony,player.weapon==FoodKind.Mielony);
  Panel(new Rect(120,542,340,88),new Color(.085f,.10f,.135f));
  GUI.Label(new Rect(136,550,300,25),"WYPOSAZENIE",heading);
  GUI.Label(new Rect(136,581,310,44),"Jetpack: "+(player.canFly?"TAK (SPACJA)":"niezdobyty")+"\nPaliwo: "+Mathf.RoundToInt(player.fuel)+"  |  Pierogi: "+player.pierogi,body);
  int count=upgrades?upgrades.Acquired.Count:0;
  GUI.Label(new Rect(490,181,650,32),"ULEPSZENIA  /  "+count,heading);
  Panel(new Rect(490,220,665,235),new Color(.075f,.085f,.115f));
  if(count==0){GUI.Label(new Rect(520,270,605,90),"Nie masz jeszcze ulepszen.\nOtworz skrzynie, aby rozpoczac skladanie buildu.",body);}
  else {
   scroll=GUI.BeginScrollView(new Rect(500,230,645,215),scroll,new Rect(0,0,620,Mathf.Max(215,count*118)));
   for(int i=0;i<count;i++){
    var definition=upgrades.Acquired[i];if(!definition)continue;float y=i*118;
    Panel(new Rect(0,y,620,106),new Color(.105f,.12f,.16f));Panel(new Rect(0,y,5,106),definition.color);
    GUI.Label(new Rect(18,y+10,584,30),definition.title,heading);GUI.Label(new Rect(18,y+46,584,54),definition.description,body);
   }
   GUI.EndScrollView();
  }
  var slam=player.SlamStats;
  Panel(new Rect(490,470,665,160),new Color(.085f,.10f,.135f));GUI.Label(new Rect(508,482,620,30),"SLAM / PARAMETRY",heading);
  GUI.Label(new Rect(508,523,620,90),"Promien: "+slam.BaseRadius.ToString("0.##")+" - "+slam.MaxRadius.ToString("0.##")+" m\nObrazenia: "+slam.DamagePerMeter.ToString("0.##")+" / m spadku  |  Limit: "+slam.MaxDamage.ToString("0.##")+"\nWzrost promienia: "+slam.RadiusPerMeter.ToString("0.##")+" m / m spadku",body);
  GUI.matrix=oldMatrix;GUI.color=oldColor;GUI.depth=oldDepth;
 }
}
}
