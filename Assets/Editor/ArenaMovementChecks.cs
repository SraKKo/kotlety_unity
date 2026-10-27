using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace RetyKotlety.Editor {
[InitializeOnLoad]
public static class ArenaMovementChecks {
 const string Key="RetyKotlety.ArenaMovementChecks";static int phase;static double start;static float phaseTime,maxFov,dashStart;static Vector3 spawn;static PlayerController player;static readonly List<string> results=new List<string>(),errors=new List<string>(),editorIssues=new List<string>();
 static ArenaMovementChecks(){if(SessionState.GetBool(Key,false))EditorApplication.update+=Tick;}
 static void Check(bool value,string name){if(!value)throw new Exception(name);results.Add("PASS "+name);Debug.Log("ARENA MOVEMENT: "+name);}
 static ArenaMoveContact Ground=>new ArenaMoveContact{grounded=true,groundNormal=Vector3.up};
 static ArenaMoveInput Wish(Vector3 direction)=>new ArenaMoveInput{wish=direction,forward=Vector3.forward};
 static void CoreChecks(){
  var p=Resources.Load<MovementTuning>("Movement/ArenaMovement");Check(p&&p.gravity==40,"Shared profile loads for already saved scenes");float dt=1f/60;
  var m=new ArenaMovementMotor(p);for(int i=0;i<6;i++)m.Step(Wish(Vector3.forward),Ground,dt);Check(m.velocity.z>16.4f,"Ground movement reaches full speed in 0.1 seconds");for(int i=0;i<12;i++)m.Step(Wish(Vector3.zero),Ground,dt);Check(new Vector2(m.velocity.x,m.velocity.z).magnitude<.01f,"Ground release brakes within 0.2 seconds");
  m=new ArenaMovementMotor(p);m.velocity=Vector3.forward*42;for(int i=0;i<120;i++)m.Step(Wish(Vector3.zero),new ArenaMoveContact(),dt);Check(Mathf.Abs(m.velocity.z-42)<.01f,"Airborne momentum survives without input");for(int i=0;i<30;i++)m.Step(Wish(Vector3.right),new ArenaMoveContact(),dt);Check(Mathf.Abs(new Vector2(m.velocity.x,m.velocity.z).magnitude-42)<.01f&&m.velocity.x>15,"Air steering redirects momentum without deleting speed");
  m=new ArenaMovementMotor(p);for(int i=0;i<3;i++)m.Step(new ArenaMoveInput{dash=true,forward=Vector3.forward},new ArenaMoveContact(),dt);Check(m.stamina==0,"Three dashes consume three charges");m.Step(new ArenaMoveInput{dash=true,forward=Vector3.forward},new ArenaMoveContact(),dt);Check(!m.DashStarted&&m.stamina>=0,"Fourth dash is rejected without negative stamina");for(int i=0;i<120;i++)m.Step(Wish(Vector3.zero),Ground,dt);Check(m.stamina>1,"Stamina recovers over time");
  m=new ArenaMovementMotor(p);m.Step(Wish(Vector3.zero),Ground,dt);m.Step(Wish(Vector3.zero),new ArenaMoveContact(),.05f);m.Step(new ArenaMoveInput{jump=true},new ArenaMoveContact(),dt);Check(m.Jumped,"Coyote time accepts jump just after a ledge");
  m=new ArenaMovementMotor(p);m.Step(new ArenaMoveInput{jump=true},new ArenaMoveContact(),dt);m.Step(Wish(Vector3.zero),Ground,dt);Check(m.Jumped,"Buffered jump triggers on landing");
  m=new ArenaMovementMotor(p);m.Step(new ArenaMoveInput{slideHeld=true,slidePressed=true,forward=Vector3.forward},Ground,dt);Check(m.Sliding&&Mathf.Abs(m.velocity.z-24)<.01f,"Slide starts immediately at 24 m/s");m.Step(new ArenaMoveInput{slideHeld=true,jump=true,forward=Vector3.forward},Ground,dt);Check(m.Jumped&&Mathf.Abs(m.velocity.z-24)<.01f,"Slide jump keeps horizontal momentum");
  m=new ArenaMovementMotor(p);m.velocity=Vector3.forward*52;for(int i=0;i<20;i++){m.Step(new ArenaMoveInput{slideHeld=true,forward=Vector3.forward},Ground,dt);m.Step(Wish(Vector3.forward),Ground,dt);}Check(m.velocity.z<=52.01f,"Repeated slide presses cannot multiply speed");
  m=new ArenaMovementMotor(p);m.Step(new ArenaMoveInput{dash=true,forward=Vector3.forward},Ground,dt);m.Step(new ArenaMoveInput{jump=true,forward=Vector3.forward},Ground,dt);Check(m.Jumped&&m.stamina==1&&Mathf.Abs(m.velocity.z-42)<.01f,"Dash jump carries boost and costs an extra charge");
  m=new ArenaMovementMotor(p);var wall=new ArenaMoveContact{wall=true,wallNormal=Vector3.right};for(int j=0;j<3;j++){m.Step(new ArenaMoveInput{jump=true},wall,dt);Check(m.Jumped,"Wall jump "+(j+1)+" succeeds");for(int i=0;i<15;i++)m.Step(Wish(Vector3.zero),wall,dt);}m.Step(new ArenaMoveInput{jump=true},wall,dt);Check(!m.Jumped&&m.WallJumpsRemaining==0,"Airborne wall jump budget is capped at three");m.Step(Wish(Vector3.zero),Ground,dt);Check(m.WallJumpsRemaining==3,"Landing replenishes wall jumps");
  m=new ArenaMovementMotor(p);m.Step(new ArenaMoveInput{slidePressed=true,slideHeld=true},new ArenaMoveContact{height=10},dt);Check(m.Slamming&&m.velocity.y==-80,"Air crouch starts controlled downward slam");m.Step(new ArenaMoveInput{jump=true},new ArenaMoveContact{grounded=true,groundNormal=Vector3.up,height=0},dt);Check(m.SlamLanded&&m.Jumped&&m.velocity.y>15&&m.velocity.y<22,"Buffered slam bounce is strong but capped");
  m=new ArenaMovementMotor(p);m.Step(new ArenaMoveInput{jump=true},Ground,dt);float y=0,peak=0,time=0;while((y>=0||time==0)&&time<2){y+=m.velocity.y*dt;peak=Mathf.Max(peak,y);m.Step(Wish(Vector3.zero),new ArenaMoveContact(),dt);time+=dt;}Check(peak>1.7f&&peak<2.2f&&time<.7f,"Jump has a short responsive arc, not floaty gravity");
 }
 public static void Run(){CoreChecks();File.WriteAllLines("Validation/arena-movement-core-results.txt",results);results.Clear();SessionState.SetBool(Key,true);EditorSceneManager.OpenScene("Assets/Scenes/TestLevel.unity");EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;}
  static void Log(string message,string stack,LogType type){
  if(type!=LogType.Error&&type!=LogType.Exception&&type!=LogType.Assert)return;
  // A Unity 6000.3 Search startup bug is unrelated to the player. Preserve its full evidence separately.
  if(message.StartsWith("ArgumentOutOfRangeException")&&stack.Contains("UnityEditor.Search.SearchDatabase")&&stack.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup")){editorIssues.Add(message+"\n"+stack);return;}
  errors.Add(message+"\n"+stack);
 }
 static void Queue(string name){typeof(PlayerController).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(player,true);}
 static void Tick(){if(!EditorApplication.isPlaying)return;if(start==0){start=EditorApplication.timeSinceStartup;Application.logMessageReceived+=Log;}try{
  if(EditorApplication.timeSinceStartup-start>35)throw new Exception("Movement Play Mode timeout");
  if(phase==0){if(EditorApplication.timeSinceStartup-start<2)return;player=GameSession.Instance.player;Check(player.movement&&player.speed==16.5f&&player.gravity==40,"Existing player scene uses new tuning rather than old serialized values");
   foreach(var enemy in UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))enemy.enabled=false;
   var platform=GameObject.CreatePrimitive(PrimitiveType.Cube);platform.transform.position=new Vector3(500,100,500);platform.transform.localScale=new Vector3(50,.5f,50);
   var cc=player.GetComponent<CharacterController>();cc.enabled=false;player.transform.position=new Vector3(500,101.3f,500);cc.enabled=true;player.velocity=Vector3.zero;spawn=player.transform.position;phase=1;phaseTime=Time.time;
  }else if(phase==1){if(Time.time-phaseTime<.25f)return;Check(player.GetComponent<CharacterController>().isGrounded,"New movement stands on real collision");dashStart=player.transform.position.z;maxFov=player.view.fieldOfView;Queue("dashQueued");phase=2;phaseTime=Time.time;
  }else if(phase==2){maxFov=Mathf.Max(maxFov,player.view.fieldOfView);if(Time.time-phaseTime<.17f)return;float distance=player.transform.position.z-dashStart;Check(distance>5&&distance<8,"Real dash travels a short controlled distance");Check(player.DashStamina<2.1f&&player.DashStamina>=1.9f,"Runtime dash consumes one charge");Check(maxFov>87,"Speed feedback smoothly widens world FOV");phase=3;phaseTime=Time.time;
  }else if(phase==3){if(Time.time-phaseTime<.65f)return;
   var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=new Vector3(500,102,504);wall.transform.localScale=new Vector3(12,4,1);
   var cc=player.GetComponent<CharacterController>();cc.enabled=false;player.transform.position=spawn;cc.enabled=true;player.velocity=Vector3.zero;Queue("dashQueued");phase=4;phaseTime=Time.time;
  }else if(phase==4){if(Time.time-phaseTime<.3f)return;Check(player.transform.position.z<=503.03f,"Dash cannot tunnel through a wall");Check(Mathf.Abs(player.velocity.z)<.1f,"Wall collision removes blocked dash momentum");Queue("jumpQueued");phase=5;phaseTime=Time.time;
  }else if(phase==5){if(Time.time-phaseTime<.14f)return;Check(!player.GetComponent<CharacterController>().isGrounded,"Ground jump leaves the real floor");Queue("jumpQueued");phase=6;phaseTime=Time.time;
  }else if(phase==6){if(Time.time-phaseTime<.05f)return;Check(player.velocity.z < -5&&player.WallJumpsRemaining==2,"Fresh wall probe enables a controlled real wall jump");phase=7;phaseTime=Time.time;
  }else if(phase==7){if(Time.time-phaseTime<.8f)return;Check(player.GetComponent<CharacterController>().isGrounded,"Wall jump returns to the floor quickly");Check(player.WallJumpsRemaining==3,"Physical landing resets wall jump budget");Check(!float.IsNaN(player.view.transform.position.x)&&player.view.fieldOfView<95,"Interpolated camera remains stable after teleport and wall jump");Check(errors.Count==0,"No runtime movement errors");Finish(0);}
 }catch(Exception e){errors.Add(e.ToString());Debug.LogException(e);Finish(1);}}
 static void Finish(int code){Application.logMessageReceived-=Log;EditorApplication.update-=Tick;SessionState.SetBool(Key,false);var combined=new List<string>(File.ReadAllLines("Validation/arena-movement-core-results.txt"));combined.AddRange(results);File.WriteAllLines("Validation/arena-movement-results.txt",combined);File.WriteAllLines("Validation/arena-movement-errors.txt",errors);File.WriteAllLines("Validation/arena-movement-editor-issues.txt",editorIssues);EditorApplication.Exit(code);}
}
}
