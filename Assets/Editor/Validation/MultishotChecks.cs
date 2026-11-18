using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace RetyKotlety.Editor {
[InitializeOnLoad]
public static class MultishotChecks {
 const string Key="RetyKotlety.MultishotChecks";static double start;
 static readonly List<string> results=new List<string>(),errors=new List<string>();
 static MultishotChecks(){if(SessionState.GetBool(Key,false))EditorApplication.update+=Tick;}
 public static void Run(){if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EditorSceneManager.OpenScene("Assets/Scenes/MovementArena.unity");SessionState.SetBool(Key,true);EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;}
 static void Check(bool ok,string label){if(!ok)throw new Exception(label);results.Add("PASS "+label);}
 static void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
 static void Fire(PlayerController player,FoodKind weapon){player.Receive(weapon);typeof(PlayerController).GetMethod("AttackImpact",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(player,null);var burst=Value<System.Collections.IEnumerator>(player,"pendingBurst");if(burst!=null){player.StopCoroutine(burst);while(burst.MoveNext()){} }}
 static T Value<T>(object instance,string name){return (T)instance.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(instance);}
 static void Clear(){foreach(var shot in UnityEngine.Object.FindObjectsByType<Projectile>())UnityEngine.Object.DestroyImmediate(shot.gameObject);foreach(var shot in UnityEngine.Object.FindObjectsByType<MielonyProjectile>())UnityEngine.Object.DestroyImmediate(shot.gameObject);foreach(var fish in UnityEngine.Object.FindObjectsByType<LososBoomerang>())UnityEngine.Object.DestroyImmediate(fish.gameObject);}
 static void Tick(){
  if(!EditorApplication.isPlaying)return;if(start==0){start=EditorApplication.timeSinceStartup;Application.logMessageReceived+=Log;}
  if(EditorApplication.timeSinceStartup-start<2)return;
  try{
   var session=GameSession.Instance;Check(session&&session.player,"Arena has an active player");var player=session.player;player.enabled=false;var upgrades=player.GetComponent<PlayerUpgrades>();Clear();
   Fire(player,FoodKind.Schabowy);Check(UnityEngine.Object.FindObjectsByType<Projectile>().Length==1,"Base Schabowy fires once");Clear();
   Fire(player,FoodKind.Chrupiaca);Check(UnityEngine.Object.FindObjectsByType<Projectile>().Length==3,"Base Chrupiaca fires three times");Clear();
   Fire(player,FoodKind.Mielony);Check(UnityEngine.Object.FindObjectsByType<MielonyProjectile>().Length==1,"Base Mielony fires once");Clear();
   Fire(player,FoodKind.Losos);Check(LososBoomerang.InFlightCount==1,"Base Losos throws one fish");Clear();Check(!LososBoomerang.InFlight,"Clearing the last fish unlocks throwing");
   foreach(var name in new[]{"SchabowyRicochet","ChrupiacaPiercing","ChrupiacaBleed","MielonyBounce","LososOrbit"})upgrades.Grant(Resources.Load<UpgradeDefinition>("Upgrades/"+name));
   var extra=Resources.Load<UpgradeDefinition>("Upgrades/ExtraProjectiles");upgrades.Grant(extra);upgrades.Grant(extra);Check(upgrades.ExtraProjectiles==2,"Two stacks grant two extra projectiles");
   Fire(player,FoodKind.Schabowy);var shots=UnityEngine.Object.FindObjectsByType<Projectile>();Check(shots.Length==3&&shots.All(s=>Value<int>(s,"chainHitsRemaining")==3),"Every Schabowy in the burst retains ricochet");Clear();
   Fire(player,FoodKind.Chrupiaca);shots=UnityEngine.Object.FindObjectsByType<Projectile>();Check(shots.Length==5&&shots.All(s=>Value<bool>(s,"piercing")&&Value<float>(s,"bleedDamage")==5),"Every extra Chrupiaca retains piercing and bleed");Check(shots.All(s=>Mathf.Abs(Vector3.Dot(s.direction,player.view.transform.up))<.001f),"Extra pellets stay in a horizontal fan");Clear();
   Fire(player,FoodKind.Mielony);var lobbed=UnityEngine.Object.FindObjectsByType<MielonyProjectile>();Check(lobbed.Length==3&&lobbed.All(s=>s.GroundBouncesRemaining==2),"Every extra Mielony retains ground bounce");Clear();
   Fire(player,FoodKind.Losos);var fish=UnityEngine.Object.FindObjectsByType<LososBoomerang>();Check(fish.Length==3&&fish.All(f=>Value<int>(f,"orbitLaps")==4),"Every fish in a volley retains orbit");
   UnityEngine.Object.DestroyImmediate(fish[0].gameObject);Check(LososBoomerang.InFlight&&LososBoomerang.InFlightCount==2,"One returning fish does not unlock the next volley");Fire(player,FoodKind.Losos);Check(LososBoomerang.InFlightCount==2,"Another fish volley cannot overlap the previous one");
   UnityEngine.Object.DestroyImmediate(fish[1].gameObject);Check(LososBoomerang.InFlight,"The final fish keeps the hand busy");UnityEngine.Object.DestroyImmediate(fish[2].gameObject);Check(!LososBoomerang.InFlight,"The final fish returning unlocks the hand");
   Check(errors.Count==0,"No runtime errors in multishot checks");Finish(0);
  }catch(Exception exception){errors.Add(exception.ToString());Debug.LogException(exception);Finish(1);}
 }
 static void Finish(int code){SessionState.SetBool(Key,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;Directory.CreateDirectory("Validation/Reports");File.WriteAllLines("Validation/Reports/multishot-results.txt",results);File.WriteAllLines("Validation/Reports/multishot-errors.txt",errors);if(Application.isBatchMode)EditorApplication.Exit(code);else EditorApplication.isPlaying=false;}
}
}
