using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
namespace RetyKotlety.Editor {
[InitializeOnLoad]
public static class ProgressionIntegrationChecks {
 const string Key="RetyKotlety.ProgressionIntegrationChecks";static double start;static int phase;
 static readonly List<string> results=new List<string>(),errors=new List<string>();
 static ProgressionIntegrationChecks(){if(SessionState.GetBool(Key,false))EditorApplication.update+=Tick;}
 public static void Run(){if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EditorSceneManager.OpenScene("Assets/Scenes/MovementArena.unity");SessionState.SetBool(Key,true);EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;}
 static void Check(bool ok,string label){if(!ok)throw new Exception(label);results.Add("PASS "+label);}
 static void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
 static Enemy MakeEnemy(bool dummy){var root=new GameObject("Progression test enemy");root.transform.position=new Vector3(1000,10,1000);root.AddComponent<NavMeshAgent>().enabled=false;var enemy=root.AddComponent<Enemy>();enemy.enabled=false;enemy.dummy=dummy;enemy.health=1;return enemy;}
 static void Tick(){
  if(!EditorApplication.isPlaying)return;if(start==0){start=EditorApplication.timeSinceStartup;Application.logMessageReceived+=Log;}
  if(EditorApplication.timeSinceStartup-start<2)return;
  try{
   var session=GameSession.Instance;Check(session&&session.player,"Arena installs player and session");var player=session.player;player.enabled=false;
   var progression=player.GetComponent<PlayerProgression>();var upgrades=player.GetComponent<PlayerUpgrades>();var shop=session.GetComponent<LevelShop>();
   Check(progression&&shop,"Progression and shop install automatically");
   if(phase==0){
   var definitions=Resources.LoadAll<UpgradeDefinition>("Upgrades");Check(definitions.Count(d=>d.IsShopUpgrade)==13&&definitions.Count(d=>d.IsWeaponUpgrade)==6,"Shop contains eleven incremental items and two movement unlocks; chests contain six weapon upgrades");
   foreach(var point in UnityEngine.Object.FindObjectsByType<PointPickup>())UnityEngine.Object.DestroyImmediate(point.gameObject);
   var enemy=MakeEnemy(false);enemy.Damage(1);enemy.Damage(1);
   Check(progression.State.Kills==1&&progression.State.Experience==10&&progression.State.Points==0,"Death gives XP once while currency remains on the ground");
   var drops=UnityEngine.Object.FindObjectsByType<PointPickup>();Check(drops.Length==1,"Death creates exactly one currency pickup");
   drops[0].transform.position=player.transform.position+Vector3.up*.1f;
   typeof(PointPickup).GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(drops[0],null);
   typeof(PointPickup).GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(drops[0],null);
   Check(progression.State.Points==5,"Collecting a pickup credits its value exactly once");
   var dummy=MakeEnemy(true);dummy.Damage(1);Check(progression.State.Kills==1&&progression.State.Experience==10,"Training targets do not farm XP or currency");
   shop.Open();Check(!session.shopOpen,"Shop is locked before first level up");
   var farDrop=player.transform.position+Vector3.right*30;
   progression.RewardKill(farDrop,false);
   Check(progression.State.Points==5,"Distant points wait on the ground before level up");
   progression.RewardKill(farDrop,false);
   Check(progression.State.Points==5,"Level up keeps old and triggering drops visible until the collection animation");
   Check(progression.State.Level==2&&progression.LevelNoticePending,"Level up queues a shop notice");
   session.SetBuildOpen(true);shop.Open();Check(!session.shopOpen&&progression.LevelNoticePending,"Queued shop waits while another menu is open");session.SetBuildOpen(false);
   session.modalClosedFrame=-1;shop.Open();Check(session.shopOpen&&session.paused&&Time.timeScale==0&&!progression.LevelNoticePending,"Shop pauses combat and acknowledges the level");
   Check(!shop.ReadyToTrade&&!progression.TryBuy(Resources.Load<UpgradeDefinition>("Upgrades/DoubleBreading")),"Purchases wait for point collection and the shop reveal");
   phase=1;return;
   }
   if(EditorApplication.timeSinceStartup-start>20)throw new Exception("Shop transition timed out");
   if(!shop.ReadyToTrade)return;
   Check(session.paused&&Time.timeScale==0&&progression.State.Points==15,"All distant drops arrive exactly once during pause before trading opens");
   Check(shop.Offers.Count==3&&shop.Offers.Distinct().Count()==3,"Shop shows three distinct cards");
   Check(shop.Offers.All(d=>d.ShopUnlockLevel==2),"First level offers only introductory upgrades");
   var extra=Resources.Load<UpgradeDefinition>("Upgrades/ExtraProjectiles");
   Check(!progression.TryBuy(extra)&&progression.State.Points==15,"Hidden or locked offers cannot be purchased");
   progression.State.AddPoints(100);
   var offered=shop.Offers[0];int before=progression.State.Points,owned=upgrades.Count(offered),price=progression.Price(offered);
   Check(progression.TryBuy(offered)&&upgrades.Count(offered)==owned+1&&progression.State.Points==before-price,"Buying a displayed card grants its stack and spends its price");
   price=progression.Price(offered);before=progression.State.Points;
   Check(progression.TryBuy(offered)&&upgrades.Count(offered)==owned+2&&progression.State.Points==before-price,"Repeat purchase uses the next stack price");
   var previous=shop.Offers.ToArray();before=progression.State.Points;
   Check(shop.TryReroll()&&shop.Offers.Except(previous).Any()&&progression.State.Points==before,"Free reroll introduces an unseen card without spending points");
   Check(!shop.TryReroll(),"Only one reroll is allowed on a level");
   var bread=Resources.Load<UpgradeDefinition>("Upgrades/DoubleBreading");
   shop.Close();Check(!session.paused&&!session.shopOpen&&Time.timeScale==1,"Closing the shop restores combat");Check(!progression.TryBuy(bread),"Purchases outside an open shop are rejected");
   int savedPoints=progression.State.Points;
   PointPickup.Spawn(player.transform.position+Vector3.right*30,7);
   var groundPoints=UnityEngine.Object.FindObjectsByType<PointPickup>().Where(p=>!p.IsCollected).ToArray();
   session.modalClosedFrame=-1;shop.Open();
   Check(groundPoints.All(p=>p.GetComponent<Renderer>().enabled)&&progression.State.Points==savedPoints,"Manual shop opening leaves ground points visible and uncredited");
   shop.Close();
   Check(groundPoints.All(p=>p&&!p.IsCollected)&&progression.State.Points==savedPoints,"Closing a manually opened shop cannot collect ground points");
   var root=new GameObject("Progression test chest");var chest=root.AddComponent<UpgradeChest>();chest.reward=bread;chest.Open(player);
   var cards=(UpgradeDefinition[])typeof(UpgradeChest).GetField("cards",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(chest);
   Check(chest.opened&&chest.reward.IsWeaponUpgrade&&cards.All(d=>d&&d.IsWeaponUpgrade),"Chest rejects a legacy item reward and rolls only weapon upgrades");UnityEngine.Object.DestroyImmediate(root);
   Check(!session.upgradeOpen&&!session.paused,"Closing a chest does not leave a stuck modal pause");
   Check(errors.Count==0,"No runtime errors in progression integration checks");Finish(0);
  }catch(Exception exception){errors.Add(exception.ToString());Debug.LogException(exception);Finish(1);}
 }
 static void Finish(int code){SessionState.SetBool(Key,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;Directory.CreateDirectory("Validation/Reports");File.WriteAllLines("Validation/Reports/progression-integration-results.txt",results);File.WriteAllLines("Validation/Reports/progression-integration-errors.txt",errors);if(Application.isBatchMode)EditorApplication.Exit(code);else EditorApplication.isPlaying=false;}
}
}
