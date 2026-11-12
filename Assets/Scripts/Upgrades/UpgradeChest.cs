using UnityEngine;
using System.Collections.Generic;
namespace RetyKotlety {
public class UpgradeChest : MonoBehaviour {
 public UpgradeDefinition reward;public bool opened;
 [Min(.1f)] public float disappearDelay=4f;
 GameSession session;bool displaying,claimed;float openedAt;GUIStyle title,body,small,button;Texture2D schabowyIcon,chrupiacaIcon,lososIcon;UpgradeDefinition[] pool;readonly UpgradeDefinition[] cards=new UpgradeDefinition[24];
 const float RollDuration=3.2f;const int Winner=18;const float CardWidth=176,Gap=12,Step=CardWidth+Gap;
 public bool CanOpen {get{
  if(opened)return false;var player=GameSession.Instance?GameSession.Instance.player:null;var upgrades=player?player.GetComponent<PlayerUpgrades>():null;
  if(reward)return !upgrades||!upgrades.Has(reward);
  if(pool!=null)foreach(var definition in pool)if(definition&&(!upgrades||!upgrades.Has(definition)))return true;
  return false;
 }}
 void Awake(){pool=Resources.LoadAll<UpgradeDefinition>("Upgrades");schabowyIcon=Resources.Load<Texture2D>("UI/kotlet_schabowy");chrupiacaIcon=Resources.Load<Texture2D>("UI/kotlet_mielony");lososIcon=Resources.Load<Texture2D>("UI/losos");}

 public void Open(PlayerController player){
  if(!CanOpen||!player)return;session=GameSession.Instance;if(!session||session.paused||session.upgradeOpen)return;
  var upgrades=player.GetComponent<PlayerUpgrades>();if(!upgrades)upgrades=player.gameObject.AddComponent<PlayerUpgrades>();var available=new List<UpgradeDefinition>();foreach(var definition in pool)if(definition&&!upgrades.Has(definition))available.Add(definition);
  if(reward){if(upgrades.Has(reward))return;}else{if(available.Count==0)return;reward=available[Random.Range(0,available.Count)];}
  if(available.Count==0)available.Add(reward);
  for(int i=0;i<cards.Length;i++)cards[i]=available[Random.Range(0,available.Count)];cards[Winner]=reward;
  opened=true;displaying=true;openedAt=Time.unscaledTime;session.SetPause(true);session.upgradeOpen=true;
 }
 void Update(){if(displaying&&Time.unscaledTime-openedAt>=RollDuration&&Input.GetKeyDown(KeyCode.Escape))Claim();}
 void Claim(){
  if(claimed||!displaying||!session||!session.player)return;
  var upgrades=session.player.GetComponent<PlayerUpgrades>();if(!upgrades)return;upgrades.Grant(reward);claimed=true;
  // Make the upgrade immediately testable, including outside the prototype arena.
  session.player.Receive(reward.Weapon);
  Close();var visual=GetComponent<UpgradeChestVisual>();if(visual)visual.RefreshState();
  var collider=GetComponent<Collider>();var bounds=collider?collider.bounds:new Bounds(transform.position,Vector3.one);
  float delay=Mathf.Max(.1f,disappearDelay);ChestSmoke.Emit(bounds,delay);Destroy(gameObject,delay);
 }
 void Close(){displaying=false;if(session&&GameSession.Instance==session){session.upgradeOpen=false;session.modalClosedFrame=Time.frameCount;session.SetPause(false);}}
 void OnDisable(){if(displaying)Close();}
 static void Panel(Rect rect,Color color){var previous=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=previous;}
 void Styles(){
  if(title!=null)return;var font=Resources.Load<Font>("UI/PixelAE-Bold");
  title=new GUIStyle(GUI.skin.label){font=font,fontSize=30,alignment=TextAnchor.MiddleCenter};
  body=new GUIStyle(title){fontSize=18,wordWrap=true};small=new GUIStyle(body){fontSize=13};button=new GUIStyle(GUI.skin.button){font=font,fontSize=22};
 }
 void OnGUI(){
  if(!displaying||!reward)return;Styles();var matrix=GUI.matrix;var color=GUI.color;int depth=GUI.depth;GUI.depth=-100;
  GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/720f,1));
  Panel(new Rect(0,0,1280,720),new Color(.015f,.02f,.03f,.91f));Panel(new Rect(140,100,1000,520),new Color(.055f,.065f,.09f));
  Panel(new Rect(140,100,1000,4),reward.color);GUI.Label(new Rect(180,126,920,50),"SKRZYNIA ULEPSZEN",title);
  GUI.Label(new Rect(180,179,920,30),"Losowe ulepszenie - bez powtorek zdobytych nagrod",small);
  float elapsed=Time.unscaledTime-openedAt,t=Mathf.Clamp01(elapsed/RollDuration);bool revealed=t>=1;
  float eased=1-Mathf.Pow(1-t,4);float final=Winner*Step+CardWidth*.5f-440;
  GUI.BeginGroup(new Rect(200,235,880,205));
  for(int i=0;i<24;i++){
   float x=i*Step-final*eased;if(x>880||x+CardWidth<0)continue;
   var card=cards[i];if(!card)continue;var icon=card.Weapon==FoodKind.Schabowy?schabowyIcon:card.Weapon==FoodKind.Losos?lososIcon:chrupiacaIcon;
   var rect=new Rect(x,0,CardWidth,195);Panel(rect,new Color(.105f,.12f,.17f));Panel(new Rect(x,189,CardWidth,6),card.color);
   if(icon)GUI.DrawTexture(new Rect(x+20,24,CardWidth-40,92),icon,ScaleMode.ScaleToFit);
   GUI.Label(new Rect(x+8,128,CardWidth-16,28),card.Weapon.ToString().ToUpperInvariant(),body);GUI.Label(new Rect(x+8,157,CardWidth-16,24),card.CardName,small);
  }
  GUI.EndGroup();Panel(new Rect(638,225,4,222),new Color(1,.8f,.25f));
  if(revealed){GUI.Label(new Rect(200,461,880,34),reward.title,title);GUI.Label(new Rect(240,502,800,46),reward.description,body);if(GUI.Button(new Rect(440,559,400,44),"ODBIERZ ULEPSZENIE",button))Claim();}
  else GUI.Label(new Rect(240,493,800,60),"OTWIERANIE...",body);
  GUI.matrix=matrix;GUI.color=color;GUI.depth=depth;
 }
}
}
