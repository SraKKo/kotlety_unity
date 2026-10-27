using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
namespace RetyKotlety {
public static class WorldFactory {
 static Transform world;
 static readonly Dictionary<string,GameObject> models=new Dictionary<string,GameObject>();
 static GameObject Node(string name,Transform parent){var g=new GameObject(name);g.transform.SetParent(parent,false);return g;}
 public static GameObject Model(string name,Transform parent){
  if(!models.TryGetValue(name,out var source)||!source){source=Resources.Load<GameObject>("Converted/"+name);models[name]=source;}
  if(!source)throw new System.InvalidOperationException("Nie zaimportowano modelu GLB: "+name+". Poczekaj na zakończenie importu glTFast.");
  var wrapper=Node(name,parent);var instance=Object.Instantiate(source,wrapper.transform);instance.transform.localPosition=Vector3.zero;instance.transform.localRotation=Quaternion.Euler(0,180,0);return wrapper;
 }
 public static GameSession Build(GameObject root,bool menu,string levelName="Level001"){
  world=root.transform;var session=Node("Sesja gry",world).AddComponent<GameSession>();session.menuScene=menu;
  session.interfaceFont=Resources.Load<Font>("UI/PixelAE-Bold");
  session.background=Resources.Load<Texture2D>("UI/MainMenuBg");session.logo=Resources.Load<Texture2D>("UI/LogoRetyKotlety");session.healthEmpty=Resources.Load<Texture2D>("UI/HealthBarEmpty");session.healthFill=Resources.Load<Texture2D>("UI/HealthBarFill");
  session.menuMusic=Resources.Load<AudioClip>("Audio/muza_main_menu ");session.jumpSound=Resources.Load<AudioClip>("Audio/skok");session.throwSound=Resources.Load<AudioClip>("Audio/rzut kotletem ");session.bounceSound=Resources.Load<AudioClip>("Audio/odbicie");session.hookSound=Resources.Load<AudioClip>("Audio/hook");session.healSound=Resources.Load<AudioClip>("Audio/bek1 ");
  if(menu){var c=Node("Menu Camera",world);c.AddComponent<Camera>().backgroundColor=new Color(.13f,.08f,.04f);c.AddComponent<AudioListener>();return session;}
  RenderSettings.ambientLight=new Color(.65f,.55f,.46f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.933f,.796f,.686f);RenderSettings.fogDensity=.0084f;RenderSettings.fogMode=FogMode.Exponential;
  var sun=Node("Słońce",world);sun.transform.rotation=Quaternion.Euler(50,-30,0);var light=sun.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.2f;light.shadows=LightShadows.Soft;
  var dataAsset=Resources.Load<TextAsset>("Converted/"+(levelName=="TestLevel"?"test_level":"level"));if(!dataAsset)throw new System.InvalidOperationException("Brak danych poziomu");
  var data=JsonUtility.FromJson<LevelData>(dataAsset.text);
  Model(levelName,world);
  var collisionRoot=Node("Kolizje poziomu",world);
  foreach(var cd in data.colliders)CreateCollider(cd,collisionRoot.transform);
  Physics.SyncTransforms();
  var settings=NavMesh.GetSettingsByID(0);settings.agentRadius=.4f;settings.agentHeight=1.8f;settings.agentClimb=.5f;settings.agentSlope=55;
  var sources=new List<NavMeshBuildSource>();NavMeshBuilder.CollectSources(collisionRoot.transform,1,NavMeshCollectGeometry.PhysicsColliders,0,new List<NavMeshBuildMarkup>(),sources);
  var bakeBounds=new Bounds(new Vector3(-100,15,35),new Vector3(600,150,600));
  if(levelName=="TestLevel"){var all=collisionRoot.GetComponentsInChildren<Collider>();if(all.Length>0){bakeBounds=all[0].bounds;foreach(var c in all)bakeBounds.Encapsulate(c.bounds);bakeBounds.Expand(10);}}
  var navigation=NavMeshBuilder.BuildNavMeshData(settings,sources,bakeBounds,Vector3.zero,Quaternion.identity);
  session.navigation=navigation;if(Application.isPlaying)session.InstallNavigation();
  var templates=Node("Szablony przeciwników",world);templates.SetActive(false);
  session.meleePrefab=CreateEnemy(templates.transform,false);session.shootingPrefab=CreateEnemy(templates.transform,true);
  var projectileTemplates=Node("Modele pocisków",world);projectileTemplates.SetActive(false);
  session.schabowyModel=Model("SchabowyProjectile",projectileTemplates.transform);session.chrupiacaModel=Model("ChrupiacaProjectile",projectileTemplates.transform);
  var presentation=JsonUtility.FromJson<PresentationData>(Resources.Load<TextAsset>("Converted/presentation").text);
  var p=Node("Player",world);p.layer=8;data.player.Apply(p.transform);
  var cc=p.AddComponent<CharacterController>();cc.height=presentation.bodyHeight;cc.radius=presentation.bodyRadius;cc.center=Vector3.zero;cc.slopeLimit=55;cc.stepOffset=.35f;cc.skinWidth=.03f;
  var camera=Node("Camera",p.transform);camera.tag="MainCamera";presentation.camera.ApplyLocal(camera.transform);
  var view=camera.AddComponent<Camera>();view.fieldOfView=presentation.worldFov;view.nearClipPlane=.05f;view.farClipPlane=600;view.cullingMask=~(1<<11);camera.AddComponent<AudioListener>();
  var pc=p.AddComponent<PlayerController>();pc.view=view;pc.cameraRest=presentation.camera.Position;pc.standingHeight=presentation.bodyHeight;pc.crouchingHeight=presentation.bodyHeight*.5f;session.player=pc;
  var weaponView=Node("Weapon Camera",camera.transform).AddComponent<Camera>();weaponView.fieldOfView=presentation.weaponFov;weaponView.nearClipPlane=.01f;weaponView.farClipPlane=100;weaponView.depth=view.depth+1;weaponView.clearFlags=CameraClearFlags.Depth;weaponView.cullingMask=1<<11;weaponView.allowHDR=view.allowHDR;
  var right=Model("RightHandRig",weaponView.transform);presentation.rightHand.ApplyLocal(right.transform);SetLayer(right,11);pc.hand=right.transform;
  var weapons=right.AddComponent<WeaponPresentation>();weapons.animationPlayer=WeaponPresentation.FindAnimation(right,"melee_attack2");
  weapons.pan=WeaponPresentation.Find(right.transform,"Patelnia").gameObject;weapons.schabowy=WeaponPresentation.Find(right.transform,"schabow_gun").gameObject;weapons.chrupiaca=WeaponPresentation.Find(right.transform,"chrupiaca_gun").gameObject;
  pc.weapons=weapons;pc.armAnimation=weapons.animationPlayer;pc.pan=weapons.pan;pc.schabowy=weapons.schabowy;pc.chrupiaca=weapons.chrupiaca;weapons.Equip(FoodKind.Pan);
  var left=Model("LeftHandRig",weaponView.transform);presentation.leftHand.ApplyLocal(left.transform);SetLayer(left,11);
  var hook=left.AddComponent<HookHand>();hook.anchor=WeaponPresentation.Find(left.transform,"pento_point");hook.Configure();pc.hookHand=hook;
  var rope=Node("Kiełbasiana linka",world).AddComponent<SausageRope>();rope.source=hook.anchor;rope.worldCamera=view;rope.weaponCamera=weaponView;
  rope.modelCenter=presentation.bounds.SausageLink.center;rope.modelCenter.z=-rope.modelCenter.z;rope.modelSize=presentation.bounds.SausageLink.size;rope.Configure();pc.rope=rope;
  foreach(var actor in data.actors){
   if(!string.IsNullOrEmpty(actor.model)){CreateTestObject(actor);continue;}
   if(actor.kind=="exit"){var exit=Node("Wyjście",world);actor.pose.Apply(exit.transform);var box=exit.AddComponent<BoxCollider>();box.isTrigger=true;box.center=new Vector3(9.49428f,.572095f,-3.48351f);box.size=new Vector3(1.31689f,1,1);exit.AddComponent<ExitArea>();continue;}
   if(actor.kind=="melee"||actor.kind=="shooting"||actor.kind=="dummy"){var enemy=Object.Instantiate(actor.kind=="shooting"?session.shootingPrefab:session.meleePrefab,world);actor.pose.Apply(enemy.transform);if(actor.kind=="dummy")enemy.GetComponent<Enemy>().dummy=true;Snap(enemy);if(!Application.isPlaying)enemy.GetComponent<NavMeshAgent>().enabled=false;continue;}
   var kind=actor.kind=="pierog"?FoodKind.Pierog:actor.kind=="jetpack"?FoodKind.Jetpack:actor.kind=="chrupiaca"?FoodKind.Chrupiaca:FoodKind.Schabowy;
   var pickup=Node("Pickup "+kind,world);actor.pose.Apply(pickup.transform);var component=pickup.AddComponent<Pickup>();component.kind=kind;var visual=Model(kind.ToString()+"Pickup",pickup.transform);component.visual=visual.transform;
   var collider=pickup.AddComponent<SphereCollider>();collider.radius=.7f;collider.isTrigger=true;
  }
  var finalSpawners=new List<WaveSpawner>();
  foreach(var spec in data.spawners){var g=Node("Arena — fale",world);spec.pose.Apply(g.transform);g.transform.localScale=Vector3.one;
   var trigger=g.AddComponent<SphereCollider>();trigger.isTrigger=true;trigger.radius=4.85928f*Mathf.Max(1,spec.pose.scale.x);
   var waves=g.AddComponent<WaveSpawner>();waves.enemyPrefab=session.meleePrefab;waves.waves=spec.waves;waves.points=new Vector3[spec.points.Length];for(int i=0;i<spec.points.Length;i++)waves.points[i]=new Vector3(spec.points[i].x,spec.points[i].y,-spec.points[i].z);
   // The original large invisible enclosure belongs only to the final arena.
   if(spec.pose.position.x < -130){finalSpawners.Add(waves);}
  }
  if(levelName=="Level001"){
  var gate=Node("Brama finałowej areny",world);var gateTrigger=gate.AddComponent<BoxCollider>();gateTrigger.isTrigger=true;gateTrigger.center=new Vector3(-151.514f,3.4458f,57.6475f);gateTrigger.size=new Vector3(32.6387f,7.8916f,28.0164f);var gateLogic=gate.AddComponent<ArenaGate>();gateLogic.barriers=CreateBarriers(world);gateLogic.spawners=finalSpawners.ToArray();
  }
  return session;
 }
 static void CreateTestObject(ActorData data){
  var g=Node(data.kind=="rotating"?"Obrotowa platforma":data.kind=="dummy"?"Manekin treningowy":"Piłka",world);g.SetActive(false);data.pose.Apply(g.transform);
  Model(data.model,g.transform);foreach(var collider in data.colliders)CreateCollider(collider,g.transform,true);
  if(data.kind=="rigid"){var body=g.AddComponent<Rigidbody>();body.mass=1;body.interpolation=RigidbodyInterpolation.Interpolate;body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;}
  else if(data.kind=="rotating"){var body=g.AddComponent<Rigidbody>();body.isKinematic=true;var rotator=g.AddComponent<TestRotator>();rotator.radiansPerSecond=new Vector3(-data.angularVelocity.x,-data.angularVelocity.y,data.angularVelocity.z);}
  else if(data.kind=="dummy"){g.layer=9;SetLayer(g,9);var agent=g.AddComponent<NavMeshAgent>();agent.enabled=false;var enemy=g.AddComponent<Enemy>();enemy.dummy=true;enemy.health=200;}
  g.SetActive(true);
 }
 static GameObject[] CreateBarriers(Transform parent){
  var positions=new[]{new Vector3(-150.267f,31.8224f,101.958f),new Vector3(-150.267f,31.8224f,31.5004f),new Vector3(-100.401f,31.8224f,44.7403f),new Vector3(-183.926f,31.8224f,50.6328f)};
  var walls=new GameObject[4];for(int i=0;i<4;i++){var wall=Node("Bariera areny",parent);wall.transform.position=positions[i];if(i>=2)wall.transform.rotation=Quaternion.Euler(0,90,0);wall.AddComponent<BoxCollider>().size=new Vector3(111.611f,64.6449f,.1f);wall.SetActive(false);walls[i]=wall;}return walls;
 }
 static void Snap(GameObject enemy){if(NavMesh.SamplePosition(enemy.transform.position,out var hit,12,NavMesh.AllAreas))enemy.transform.position=hit.position;}
 static GameObject CreateEnemy(Transform parent,bool shooting){
  var g=Node(shooting?"Strzelający przeciwnik":"Przeciwnik wręcz",parent);g.layer=9;
  var visual=Model(shooting?"ShootingEnemyRig":"MeleeEnemyRig",g.transform);SetLayer(visual,9);
  // Dimensions and offsets from the authored CollisionShape3D, without normalizing the renderers.
  var agent=g.AddComponent<NavMeshAgent>();agent.radius=shooting?.5f:.3375f;agent.height=shooting?2.39978f:1.6534773f;agent.baseOffset=shooting?1.030855f:.0409386f;
  var capsule=g.AddComponent<CapsuleCollider>();capsule.height=agent.height;capsule.radius=agent.radius;capsule.center=shooting?new Vector3(0,.169035f,.0480754f):new Vector3(0,.785801f,0);
  var enemy=g.AddComponent<Enemy>();enemy.shooting=shooting;enemy.health=shooting?120:200;return g;
 }
 static void SetLayer(GameObject go,int layer){go.layer=layer;foreach(Transform c in go.transform)SetLayer(c.gameObject,layer);}
 static void CreateCollider(ColliderData d,Transform parent,bool local=false){var g=Node("Collision "+d.kind,parent);if(local)d.pose.ApplyLocal(g.transform);else d.pose.Apply(g.transform);
  switch(d.kind){case "box":var scale=g.transform.localScale;g.transform.localScale=new Vector3(Mathf.Abs(scale.x),Mathf.Abs(scale.y),Mathf.Abs(scale.z));g.AddComponent<BoxCollider>().size=d.size;break;case "sphere":g.AddComponent<SphereCollider>().radius=d.radius;break;case "capsule":var c=g.AddComponent<CapsuleCollider>();c.radius=d.radius;c.height=d.height;break;
   case "mesh":case "convex":if(d.points==null||d.points.Length<3)return;var mesh=new Mesh(){name="Godot collision"};if(d.points.Length>65535)mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;var verts=new Vector3[d.points.Length];var triangles=new int[d.points.Length];for(int i=0;i<verts.Length;i++){var v=d.points[i];verts[i]=new Vector3(v.x,v.y,-v.z);triangles[i]=i;}
    // Concave faces are independent triangles. Convex point clouds need hull cooking.
    if(d.kind=="mesh"&&!d.preserveFaceOrder){for(int i=0;i+2<triangles.Length;i+=3){triangles[i+1]=i+2;triangles[i+2]=i+1;}}
    else if(d.kind=="convex"){var indices=new List<int>();for(int i=1;i+1<verts.Length;i++){indices.Add(0);indices.Add(i);indices.Add(i+1);}triangles=indices.ToArray();}
    mesh.vertices=verts;mesh.triangles=triangles;mesh.RecalculateBounds();var mc=g.AddComponent<MeshCollider>();mc.sharedMesh=mesh;mc.convex=d.kind=="convex";break;
  }
 }
}
}







