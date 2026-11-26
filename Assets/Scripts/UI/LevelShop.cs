using System;
using System.Collections.Generic;
using UnityEngine;
namespace RetyKotlety {
public class LevelShop : MonoBehaviour {
 GameSession session;
 UpgradeDefinition[] catalog;
 UpgradeDefinition[] offers = Array.Empty<UpgradeDefinition>();
 PlayerProgression offerOwner;
 int offerLevel = -1;
 bool rerollUsed;
 float cardsStarted;
 GUIStyle title,body,small,button,hudText,cardTitle,cardName,cardInfo,cardPreview;
 public IReadOnlyList<UpgradeDefinition> Offers => offers;
 PointPickup[] incoming = Array.Empty<PointPickup>();
 bool gathering;
 float collectionStarted, revealStarted, collectionDuration;
 int pointsBeforeCollection;
 const float RevealDuration = .9f;
 public bool ReadyToTrade => session && session.shopOpen && !gathering && Time.unscaledTime - revealStarted >= RevealDuration && Time.unscaledTime - cardsStarted >= .45f;
 PlayerProgression Progression=>session&&session.player?session.player.GetComponent<PlayerProgression>():null;
 void Awake(){session=GetComponent<GameSession>();catalog=Array.FindAll(Resources.LoadAll<UpgradeDefinition>("Upgrades"),d=>d&&d.IsShopUpgrade);Array.Sort(catalog,(a,b)=>string.CompareOrdinal(a.id,b.id));}
 bool CanOpen=>session&&!session.menuScene&&!session.won&&!session.paused&&!session.upgradeOpen&&!session.buildOpen&&!session.debugOpen&&Time.frameCount!=session.modalClosedFrame;
 void Update() {
  if(!session)return;
  if(session.shopOpen) {
   if(gathering) {
    bool complete = true;
    foreach(var pickup in incoming)if(pickup && !pickup.IsCollected)complete = false;
    if(complete && Time.unscaledTime - collectionStarted >= collectionDuration) {
     gathering = false;
     revealStarted = Time.unscaledTime;
     cardsStarted = revealStarted;
     incoming = Array.Empty<PointPickup>();
    }
   } else if(ReadyToTrade && (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Escape)))Close();
   return;
  }
  var progression = Progression;
  if(!progression || !progression.State.ShopUnlocked || !CanOpen)return;
  if(progression.LevelNoticePending || Input.GetKeyDown(KeyCode.B))Open();
 }
 public void Open() {
  var progression = Progression;
  if(!CanOpen || !progression || !progression.State.ShopUnlocked)return;
  if(offerOwner != progression || offerLevel != progression.State.Level) {
   offerOwner = progression;
   offerLevel = progression.State.Level;
   rerollUsed = false;
   offers = Array.Empty<UpgradeDefinition>();
   RollOffers();
  }
  bool levelUp = progression.LevelNoticePending;
  progression.AcknowledgeLevel();
  session.shopOpen = true;
  session.SetPause(true);
  incoming = Array.Empty<PointPickup>();
  gathering = levelUp;
  revealStarted = cardsStarted = Time.unscaledTime;
  if(!levelUp)return;
  collectionStarted = Time.unscaledTime;
  pointsBeforeCollection = progression.State.Points;
  incoming = Array.FindAll(UnityEngine.Object.FindObjectsByType<PointPickup>(),
   pickup => pickup.gameObject.scene == progression.gameObject.scene && !pickup.IsCollected);
  collectionDuration = incoming.Length > 0 ? 1.4f : .6f;
  for(int i = 0; i < incoming.Length; i++)incoming[i].BeginShopCollection(progression, i, incoming.Length);
 }
 bool CanOffer(UpgradeDefinition definition) {
  var progression = Progression;
  return progression && definition && definition.IsShopUpgrade
   && progression.State.Level >= definition.ShopUnlockLevel
   && progression.GetComponent<PlayerUpgrades>().CanAcquire(definition);
 }
 public bool IsOffered(UpgradeDefinition definition) => Array.IndexOf(offers,definition) >= 0;
 public bool CanReroll {
  get {
   if(!ReadyToTrade || rerollUsed)return false;
   foreach(var definition in catalog)if(CanOffer(definition) && !IsOffered(definition))return true;
   return false;
  }
 }
 public bool TryReroll() {
  if(!CanReroll)return false;
  RollOffers();
  rerollUsed = true;
  cardsStarted = Time.unscaledTime;
  return true;
 }
 void RollOffers() {
  var fresh = new List<UpgradeDefinition>();
  var previous = new List<UpgradeDefinition>();
  var ids = new HashSet<string>();
  foreach(var definition in catalog) {
   if(!CanOffer(definition) || !ids.Add(definition.id))continue;
   if(IsOffered(definition))previous.Add(definition);
   else fresh.Add(definition);
  }
  Shuffle(fresh);
  Shuffle(previous);
  // Prefer unseen cards; a small early pool can still reuse part of the previous hand.
  fresh.AddRange(previous);
  offers = fresh.GetRange(0,Mathf.Min(3,fresh.Count)).ToArray();
 }
 static void Shuffle(List<UpgradeDefinition> definitions) {
  for(int i = definitions.Count - 1; i > 0; i--) {
   int other = UnityEngine.Random.Range(0,i + 1);
   var value = definitions[i];
   definitions[i] = definitions[other];
   definitions[other] = value;
  }
 }
 public void Close() {
  if(!session || !session.shopOpen)return;
  // Finish only this shop's pending transfers if its UI is disabled during the intro.
  var progression = Progression;
  if(progression)foreach(var pickup in incoming)if(pickup)pickup.Collect(progression);
  incoming = Array.Empty<PointPickup>();
  gathering = false;
  session.shopOpen = false;
  session.modalClosedFrame = Time.frameCount;
  if(!session.won)session.SetPause(false);
 }
 void OnDisable(){Close();}
 static void Panel(Rect r,Color color){var old=GUI.color;GUI.color=old*color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
 void Styles(){if(title!=null)return;var font=Resources.Load<Font>("UI/PixelAE-Bold");title=new GUIStyle(GUI.skin.label){font=font,fontSize=30};body=new GUIStyle(title){fontSize=18,wordWrap=true};small=new GUIStyle(body){fontSize=15};button=new GUIStyle(GUI.skin.button){font=font,fontSize=18};hudText=new GUIStyle(small){wordWrap=false};
  cardTitle = new GUIStyle(body) { fontSize=23, alignment=TextAnchor.MiddleCenter };
  cardName = new GUIStyle(small) { fontSize=15, alignment=TextAnchor.MiddleCenter };
  cardInfo = new GUIStyle(small) { alignment=TextAnchor.MiddleCenter };
  cardPreview = new GUIStyle(body) { fontSize=21, alignment=TextAnchor.MiddleCenter };}
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
  }else if(gathering) {
   GUI.depth = -70;
   float elapsed = Time.unscaledTime - collectionStarted;
   float fade = Mathf.SmoothStep(0, 1, elapsed / .2f);
   Panel(new Rect(0,0,1280,720),new Color(.015f,.02f,.03f,.24f * fade));
   var centered = new GUIStyle(title) { alignment = TextAnchor.MiddleCenter };
   GUI.Label(new Rect(340,150,600,48),"POZIOM " + state.Level,centered);
   var counter = new GUIStyle(body) { alignment = TextAnchor.MiddleCenter };
   GUI.Label(new Rect(340,480,600,38),"PKT " + state.Points + "   (+" + (state.Points - pointsBeforeCollection) + ")",centered);
   GUI.Label(new Rect(340,530,600,32),"ZBIERANIE PUNKTOW",counter);
  }else {
   GUI.depth=-100;
   float reveal = Mathf.Clamp01((Time.unscaledTime - revealStarted) / .55f);
   float smooth = Mathf.SmoothStep(0, 1, reveal);
   Panel(new Rect(0,0,1280,720),new Color(.015f,.02f,.03f,.96f * smooth));
   float back = 1 + 2.70158f * Mathf.Pow(reveal - 1, 3) + 1.70158f * Mathf.Pow(reveal - 1, 2);
   float zoom = Mathf.LerpUnclamped(.94f, 1, back);
   GUI.matrix *= Matrix4x4.Translate(new Vector3(640,360 + 65 * (1 - back),0)) * Matrix4x4.Scale(new Vector3(zoom,zoom,1)) * Matrix4x4.Translate(new Vector3(-640,-360,0));
   GUI.color = new Color(oldColor.r,oldColor.g,oldColor.b,oldColor.a * smooth);
   bool previousEnabled = GUI.enabled;
   GUI.enabled = previousEnabled && ReadyToTrade;
   Panel(new Rect(100,50,1080,620),new Color(.055f,.065f,.09f));
   GUI.Label(new Rect(140,76,740,45),"SKLEP / POZIOM " + state.Level,title);
   GUI.Label(new Rect(890,82,250,35),"PUNKTY " + state.Points,body);
   GUI.Label(new Rect(140,130,1000,35),"Wybierz ulepszenia lub zachowaj punkty na kolejny awans.",body);
   var upgrades = session.player.GetComponent<PlayerUpgrades>();
   for(int i = 0; i < offers.Length; i++) {
    var offer = offers[i];
    float cardReveal = Mathf.Clamp01((Time.unscaledTime - cardsStarted - i * .04f) / .3f);
    GUI.color = new Color(oldColor.r,oldColor.g,oldColor.b,oldColor.a * smooth * cardReveal);
    float x = 140 + i * 340;
    float y = 182 + 18 * (1 - Mathf.SmoothStep(0,1,cardReveal));
    Panel(new Rect(x,y,320,390),new Color(.105f,.12f,.16f));
    Panel(new Rect(x,y,320,4),offer.color);
    if(offer.icon)GUI.DrawTexture(new Rect(x+104,y+16,112,112),offer.icon,ScaleMode.ScaleToFit);
    int owned = upgrades.Count(offer);
    int price = progression.Price(offer);
    bool available = upgrades.CanAcquire(offer);
    GUI.Label(new Rect(x+16,y+134,288,64),offer.ShopBenefit,cardTitle);
    GUI.Label(new Rect(x+16,y+203,288,38),offer.title,cardName);
    GUI.Label(new Rect(x+16,y+245,288,38),available ? offer.ShopPreview(owned) : "ZDOBYTE",cardPreview);
    GUI.Label(new Rect(x+16,y+285,288,46),offer.ShopDetail,cardInfo);
    GUI.enabled = previousEnabled && ReadyToTrade && available && state.Points >= price;
    string purchase = !available ? "ZDOBYTE" : state.Points < price ? "BRAKUJE " + (price-state.Points) + " PKT" : "KUP / " + price + " PKT";
    if(GUI.Button(new Rect(x+20,y+338,280,38),purchase,button))progression.TryBuy(offer);
   }
   GUI.color = new Color(oldColor.r,oldColor.g,oldColor.b,oldColor.a * smooth);
   if(offers.Length == 0)GUI.Label(new Rect(200,300,880,80),"Wszystkie dostępne ulepszenia zostały zdobyte. Punkty zachowasz na później.",cardTitle);
   GUI.enabled = previousEnabled && CanReroll;
   if(GUI.Button(new Rect(140,594,320,48),rerollUsed ? "PRZELOSOWANIE WYKORZYSTANE" : "PRZELOSUJ / ZA DARMO",button))TryReroll();
   GUI.enabled = previousEnabled && ReadyToTrade;
   if(GUI.Button(new Rect(480,594,660,48),"ZACHOWAJ PUNKTY I WRÓĆ DO WALKI",button))Close();   GUI.enabled = previousEnabled;
  }
  GUI.matrix=matrix;GUI.color=oldColor;GUI.depth=depth;
 }
}
}
