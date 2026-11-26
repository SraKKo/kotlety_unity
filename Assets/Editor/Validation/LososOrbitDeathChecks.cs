using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
namespace RetyKotlety.Editor {
public static class LososOrbitDeathChecks {
 static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Call(LososBoomerang fish,string method,params object[] args){typeof(LososBoomerang).GetMethod(method,Flags).Invoke(fish,args);}
 static T Read<T>(LososBoomerang fish,string name)=>(T)typeof(LososBoomerang).GetField(name,Flags).GetValue(fish);
 static Enemy Target(Vector3 position,float health) {
  var root=GameObject.CreatePrimitive(PrimitiveType.Capsule);root.name="Orbit regression target";root.layer=9;root.transform.position=position;
  root.AddComponent<NavMeshAgent>().enabled=false;
  var enemy=root.AddComponent<Enemy>();enemy.enabled=false;enemy.dummy=true;enemy.health=health;return enemy;
 }
 public static void RunChecks(GameObject owner,Transform home) {
  var results=new List<string>();
  Action<bool,string> check=(ok,text)=>{if(!ok)throw new Exception(text);results.Add("PASS "+text);};
  var tuning=LososBoomerang.Tuning;
  var target=Target(new Vector3(1000,2,1000),1);
  var collider=target.GetComponent<Collider>();var centre=collider.bounds.center;
  var fish=LososBoomerang.Spawn(centre-Vector3.forward*1.6f,Vector3.forward,Vector3.right,owner,home);fish.enabled=false;fish.SetOrbit(4);
  Call(fish,"StartOrbit",target,collider,centre);Call(fish,"Orbit",tuning.orbitLapTime*1.05f,tuning);
  check(target.health<=0&&fish.Orbiting&&!fish.Returning,"A killing orbit hit does not cancel remaining laps");
  UnityEngine.Object.DestroyImmediate(target.gameObject);
  Call(fish,"Orbit",tuning.orbitLapTime,tuning);
  check(fish.Orbiting&&Vector3.Distance(Read<Vector3>(fish,"orbitCentre"),centre)<.01f,"Orbit centre stays fixed after the corpse is removed");
  check(Mathf.Abs(Vector3.Distance(fish.transform.position,centre)-tuning.orbitRadius)<.1f,"Fish keeps moving on its ring around the death location");
  Call(fish,"Orbit",tuning.orbitLapTime*2,tuning);
  check(!fish.Orbiting&&fish.Returning&&Read<int>(fish,"lapsDone")==4,"Fish completes all four laps before returning");UnityEngine.Object.DestroyImmediate(fish.gameObject);
  target=Target(new Vector3(1020,2,1003),1);centre=target.GetComponent<Collider>().bounds.center;
  fish=LososBoomerang.Spawn(new Vector3(1020,2,1000),Vector3.forward,Vector3.right,owner,home);fish.enabled=false;fish.SetOrbit(4);Physics.SyncTransforms();
  Call(fish,"Move",Vector3.forward*5,tuning);
  check(target.health<=0&&fish.Orbiting,"A lethal outbound hit still starts the upgrade orbit");UnityEngine.Object.DestroyImmediate(target.gameObject);
  Call(fish,"Orbit",tuning.orbitLapTime*4.05f,tuning);
  check(fish.Returning&&Read<int>(fish,"lapsDone")==4,"Lethal first hit also completes the full lap budget");UnityEngine.Object.DestroyImmediate(fish.gameObject);
  target=Target(new Vector3(1040,2,1000),10000);collider=target.GetComponent<Collider>();
  fish=LososBoomerang.Spawn(target.transform.position-Vector3.forward*1.6f,Vector3.forward,Vector3.right,owner,home);fish.enabled=false;fish.SetOrbit(4);
  Call(fish,"StartOrbit",target,collider,collider.bounds.center);
  target.transform.position+=Vector3.right*3;Physics.SyncTransforms();Call(fish,"Orbit",.02f,tuning);
  centre=collider.bounds.center;check(Vector3.Distance(Read<Vector3>(fish,"orbitCentre"),centre)<.01f,"Orbit continues following a living moving enemy");
  UnityEngine.Object.DestroyImmediate(target.gameObject);Call(fish,"Orbit",.02f,tuning);
  check(fish.Orbiting&&Vector3.Distance(Read<Vector3>(fish,"orbitCentre"),centre)<.01f,"An externally destroyed target leaves its last centre usable");UnityEngine.Object.DestroyImmediate(fish.gameObject);
  Directory.CreateDirectory("Validation/Reports");File.WriteAllLines("Validation/Reports/losos-orbit-death-results.txt",results);
 }
}
}