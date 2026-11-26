using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
namespace RetyKotlety.Editor {
public static class PostSovietDistrictEditor {
 const string Output="Assets/Generated/PostSovietDistrict";
 const string YardPath="Assets/ItchAssets/PSX_PostSoviet_Yard_v0.1";
 const string RetroPath="Assets/ItchAssets/PSX_Retro_Props_Pack (1)";
 static Transform geometry;
 static GameObject yard,trees;
 static Material asphalt,concrete,brick,grass,rust,yellow,blue,white,roof;
 static readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
 static readonly List<string> checks=new List<string>();
 static int meshOrdinal;
 static readonly Vector3[] centres={new Vector3(-35,0,-32),new Vector3(-64,0,39),new Vector3(15,0,52),new Vector3(48,0,20),new Vector3(53,0,-38)};
 [MenuItem("Rety Kotlety/Environment/Zbuduj Osiedle Zelazna")]
 public static void Build() {
  if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
  Directory.CreateDirectory("Assets/Scenes");
  Directory.CreateDirectory(Output+"/Meshes");Directory.CreateDirectory(Output+"/Materials");Directory.CreateDirectory(Output+"/Textures");
  AssetDatabase.Refresh();materials.Clear();checks.Clear();meshOrdinal=0;
  var yardImporter=(ModelImporter)AssetImporter.GetAtPath(YardPath+"/FBX/PSX_PostSoviet_Yard.fbx");
  if(!yardImporter.isReadable){yardImporter.isReadable=true;yardImporter.SaveAndReimport();}
  yard=AssetDatabase.LoadAssetAtPath<GameObject>(YardPath+"/FBX/PSX_PostSoviet_Yard.fbx");
  var treesImporter=(ModelImporter)AssetImporter.GetAtPath(Output+"/DistrictTrees.fbx");
  if(!treesImporter.isReadable){treesImporter.isReadable=true;treesImporter.SaveAndReimport();}
  trees=AssetDatabase.LoadAssetAtPath<GameObject>(Output+"/DistrictTrees.fbx");
  if(!yard||!trees)throw new Exception("Brak paczki podworka lub eksportu drzew");
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var root=new GameObject("OSIEDLE ZELAZNA");
  geometry=Group("Dzielnica - geometria i assety",root.transform);
  asphalt=Tiled("Asfalt",YardPath+"/Textures/asphalt.png",Color.white,.3f);
  concrete=Tiled("Beton",YardPath+"/Textures/concrete_rough.png",new Color(.83f,.83f,.76f),.35f);
  brick=Tiled("Cegla",YardPath+"/Textures/brick.png",new Color(.56f,.5f,.45f),.3f);
  grass=Tiled("Ziemia i mech",YardPath+"/Textures/concrete_rough.png",new Color(.38f,.43f,.27f),.75f);
  rust=Tiled("Rdza",YardPath+"/Textures/metal_grey.png",new Color(.62f,.32f,.17f),.5f);
  yellow=Solid("Zolte oznaczenia",new Color(.75f,.54f,.15f));
  blue=Solid("Niebieska trasa",new Color(.23f,.43f,.48f));
  white=Solid("Wytarte pasy",new Color(.66f,.64f,.49f));
  roof=Tiled("Papa",YardPath+"/Textures/asphalt.png",new Color(.4f,.42f,.43f),.7f);
  Streets();Housing();Garages();Factory();Park();Movement();Dressing();ArtPass();Backdrop();
  Physics.SyncTransforms();
  var session=WorldFactory.Build(root,false,PostSovietDistrict.SceneName,geometry);
  session.player.transform.rotation=Quaternion.Euler(0,35,0);
  var combat=root.AddComponent<DistrictCombat>();
  combat.encounters=new DistrictCombat.Encounter[centres.Length];
  string[] names={"Podworko blokow","Garazowisko","Targ i kotlownia","Zaklad przemyslowy","Park osiedlowy"};
  Vector3[][] entries={
   new[]{V(-52,-31),V(-35,-49),V(-15,-38),V(-40,-12),V(-24,-20),V(-55,-47)},
   new[]{V(-69,27),V(-51,32),V(-71,51),V(-53,54),V(-65,48),V(-48,40)},
   new[]{V(0,45),V(28,46),V(20,65),V(2,65),V(13,38),V(31,61)},
   new[]{V(29,10),V(51,5),V(66,26),V(40,41),V(61,43),V(27,32)},
   new[]{V(31,-46),V(70,-45),V(64,-25),V(41,-23),V(53,-56),V(29,-34)}
  };
  for(int i=0;i<entries.Length;i++)for(int j=0;j<entries[i].Length;j++)entries[i][j].y=TerrainHeight(entries[i][j].x,entries[i][j].z);
  for(int i=0;i<centres.Length;i++)combat.encounters[i]=new DistrictCombat.Encounter{name=names[i],centre=centres[i],entries=entries[i],melee=7+i*2,ranged=2+i};
  ConfigureLighting(root,session);
  var navPath=Output+"/DistrictNavigation.asset";
  SaveAsset(session.navigation,navPath);session.navigation=AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);
  RenderViews(root);
  Validate(session,combat);
  var scenePath="Assets/Scenes/"+PostSovietDistrict.SceneName+".unity";
  if(!EditorSceneManager.SaveScene(scene,scenePath))throw new Exception("Nie udalo sie zapisac sceny "+scenePath);
  var buildScenes=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
  if(!buildScenes.Any(s=>s.path==scenePath))buildScenes.Add(new EditorBuildSettingsScene(scenePath,true));
  EditorBuildSettings.scenes=buildScenes.ToArray();
  AssetDatabase.SaveAssets();

  Directory.CreateDirectory("Validation/Reports");File.WriteAllLines("Validation/Reports/district-layout-results.txt",checks);
  Debug.Log("Osiedle Zelazna: gotowe / "+geometry.GetComponentsInChildren<Renderer>().Length+" rendererow.");
 }
 public static void BuildBatch(){try{Build();EditorApplication.Exit(0);}catch(Exception error){File.WriteAllText("district-build-error.txt",error.ToString());Debug.LogException(error);EditorApplication.Exit(1);}}
 static Vector3 V(float x,float z)=>new Vector3(x,0,z);
 static Transform Group(string name,Transform parent){var go=new GameObject(name);go.transform.SetParent(parent,false);return go.transform;}
 static GameObject Box(string name,Vector3 centre,Vector3 size,Material material,Transform parent=null,bool solid=true) {
  var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent?parent:geometry,false);go.transform.localPosition=centre;go.transform.localScale=size;
  go.GetComponent<Renderer>().sharedMaterial=material;
  if(!solid)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
  go.isStatic=true;return go;
 }
 static void ColliderBox(Transform parent,Vector3 centre,Vector3 size) {
  var go=new GameObject("Kolizja bryly");go.transform.SetParent(parent,false);go.transform.localPosition=centre;go.AddComponent<BoxCollider>().size=size;go.isStatic=true;
 }
 static Texture2D Texture(string source) {
  string destination=Output+"/Textures/"+Path.GetFileName(source);
  if(!File.Exists(destination)){File.Copy(source,destination);AssetDatabase.ImportAsset(destination);}
  var importer=(TextureImporter)AssetImporter.GetAtPath(destination);
  if(importer.filterMode!=FilterMode.Point||importer.mipmapEnabled){importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.wrapMode=TextureWrapMode.Repeat;importer.SaveAndReimport();}
  return AssetDatabase.LoadAssetAtPath<Texture2D>(destination);
 }
 static Material Material(string name,string source,Color color,bool tiled=false,float tiling=1,bool cutout=false) {
  if(materials.TryGetValue(name,out var cached))return cached;
  var material=new Material(Shader.Find(tiled?"RetyKotlety/PSXWorld":"Standard")){name=name,color=color};
  if(source!=null)material.mainTexture=Texture(source);
  if(tiled)material.SetFloat("_WorldTiling",tiling);
  else{material.SetFloat("_Glossiness",0);material.SetFloat("_Metallic",0);}
  if(cutout){material.SetFloat("_Mode",1);material.SetFloat("_Cutoff",.4f);material.EnableKeyword("_ALPHATEST_ON");material.renderQueue=2450;}
  SaveAsset(material,Output+"/Materials/"+name+".mat");
  cached=AssetDatabase.LoadAssetAtPath<Material>(Output+"/Materials/"+name+".mat");materials[name]=cached;return cached;
 }
 static Material Tiled(string name,string source,Color color,float tiling)=>Material(name,source,color,true,tiling);
 static Material Solid(string name,Color color)=>Material(name,null,color);
 static void SaveAsset(UnityEngine.Object asset,string path) {
  var existing=AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
  if(existing){EditorUtility.CopySerialized(asset,existing);UnityEngine.Object.DestroyImmediate(asset);}else AssetDatabase.CreateAsset(asset,path);
 }
 static Material YardMaterial(string name) {
  string key=name.Replace(" (Instance)","").Replace("M_","");
  return Material("Yard_"+key,YardPath+"/Textures/"+key+".png",Color.white);
 }
 static Transform Yard(string name,Vector3 position,float yaw=0,Transform parent=null,bool collision=true) {
  var part=yard.GetComponentsInChildren<Transform>(true).First(t=>t.name==name);
  if(Mathf.Abs(position.y)<.001f&&!name.StartsWith("PanelWall"))position.y=TerrainHeight(position.x,position.z);
  var wrapper=Group(name,parent?parent:geometry);wrapper.localPosition=position;wrapper.localRotation=Quaternion.Euler(0,yaw,0);
  var instance=UnityEngine.Object.Instantiate(part.gameObject,wrapper,false);instance.transform.localPosition=Vector3.zero;
  foreach(var renderer in instance.GetComponentsInChildren<MeshRenderer>())renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>YardMaterial(m.name)).ToArray();
  foreach(var filter in instance.GetComponentsInChildren<MeshFilter>())if(collision)filter.gameObject.AddComponent<MeshCollider>().sharedMesh=filter.sharedMesh;
  foreach(var t in wrapper.GetComponentsInChildren<Transform>())t.gameObject.isStatic=true;
  return wrapper;
 }
 static float Mound(float x,float z,float cx,float cz,float rx,float rz) {
  float distance=(x-cx)*(x-cx)/(rx*rx)+(z-cz)*(z-cz)/(rz*rz);
  return Mathf.Exp(-distance*2);
 }
 static float OutsideFlat(float x,float z,float cx,float cz,float width,float depth) {
  float distance=Mathf.Max(Mathf.Abs(x-cx)-width*.5f,Mathf.Abs(z-cz)-depth*.5f);
  return Mathf.SmoothStep(0,1,Mathf.Clamp01(distance/4));
 }
 static float TerrainHeight(float x,float z) {
  float height=3.8f*Mound(x,z,70,-55,18,18)+2.7f*Mound(x,z,35,-24,14,15)
   +3.1f*Mound(x,z,-73,-30,18,24)+5.8f*Mound(x,z,96,66,14,24)
   +3.5f*Mound(x,z,-101,80,13,17)+3.2f*Mound(x,z,-103,-65,13,22)
   -2.4f*Mound(x,z,43,-77,27,8);
  height*=OutsideFlat(x,z,0,-7,17,184);
  height*=OutsideFlat(x,z,0,-10,214,13);
  height*=OutsideFlat(x,z,0,58,180,11);
  height*=OutsideFlat(x,z,-35,-32,55,51);
  height*=OutsideFlat(x,z,-62,40,46,62);
  height*=OutsideFlat(x,z,49,24,60,61);
  height*=OutsideFlat(x,z,-62,-61,45,8);
  height*=OutsideFlat(x,z,-92,-12,17,48);
  height*=OutsideFlat(x,z,-8,-80,52,17);
  height*=OutsideFlat(x,z,86,7,17,52);
  height*=OutsideFlat(x,z,-30,78,55,17);
  height*=OutsideFlat(x,z,-25,14,36,14);
  // Keep the cross-shaped park paths at street level and lift the lawns around them.
  if(x>25&&x<78&&z>-65&&z<-16) {
   height*=Mathf.SmoothStep(0,1,Mathf.Clamp01((Mathf.Abs(z+37)-2)/4));
   height*=Mathf.SmoothStep(0,1,Mathf.Clamp01((Mathf.Abs(x-51)-2)/4));
  }
  return height;
 }
 static void Terrain() {
  const int columns=79,rows=73;
  var vertices=new Vector3[columns*rows];
  var triangles=new int[(columns-1)*(rows-1)*6];
  int cursor=0;
  for(int z=0;z<rows;z++)for(int x=0;x<columns;x++) {
   float px=-117+x*3,pz=-108+z*3;
   vertices[z*columns+x]=new Vector3(px,TerrainHeight(px,pz),pz);
   if(x==columns-1||z==rows-1)continue;
   int start=z*columns+x;
   triangles[cursor++]=start;triangles[cursor++]=start+columns;triangles[cursor++]=start+1;
   triangles[cursor++]=start+1;triangles[cursor++]=start+columns;triangles[cursor++]=start+columns+1;
  }
  var mesh=new Mesh{name="Skarpy i rowy dzielnicy",vertices=vertices,triangles=triangles};mesh.RecalculateNormals();mesh.RecalculateBounds();
  string path=Output+"/Meshes/DistrictTerrain.asset";SaveAsset(mesh,path);mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
  var terrain=Group("Teren / skarpy, pagorki i obnizenie",geometry);
  terrain.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;terrain.gameObject.AddComponent<MeshRenderer>().sharedMaterial=grass;
  terrain.gameObject.AddComponent<MeshCollider>().sharedMesh=mesh;terrain.gameObject.isStatic=true;
  Check(vertices.Max(v=>v.y)>4,"Sculpted terrain has banks over four metres high");
  Check(vertices.Min(v=>v.y)<-1.5f,"Service ditch forms a real physical depression");
 }
 static void Streets() {
  Terrain();
  Box("Glowna aleja",new Vector3(0,.015f,-7),new Vector3(14,.03f,180),asphalt);
  Box("Ulica poprzeczna",new Vector3(0,.02f,-10),new Vector3(210,.04f,11),asphalt);
  Box("Obwodnica polnocna",new Vector3(0,.01f,58),new Vector3(176,.02f,9),asphalt);
  Box("Plac blokow",new Vector3(-35,.01f,-32),new Vector3(53,.02f,49),asphalt);
  Box("Plac garazy",new Vector3(-62,.02f,40),new Vector3(42,.04f,53),asphalt);
  Box("Plac zakladu",new Vector3(49,.015f,24),new Vector3(56,.03f,58),asphalt);
  Box("Chodnik startowy",new Vector3(-62,.12f,-61),new Vector3(42,.24f,5),concrete);
  foreach(float x in new[]{-8.3f,8.3f})Box("Chodnik alei",new Vector3(x,.12f,-7),new Vector3(2.5f,.24f,180),concrete);
  for(int z=-82;z<80;z+=10)Box("Wytarta os jezdni",new Vector3(0,.05f,z),new Vector3(.15f,.025f,4),white,null,false);
  for(int x=-91;x<92;x+=7)if(Mathf.Abs(x)>12)Box("Pasy drogi",new Vector3(x,.06f,-10),new Vector3(3,.02f,.13f),white,null,false);
  for(int i=0;i<7;i++)Box("Przejscie dla pieszych",new Vector3(-5+i*1.6f,.07f,-58),new Vector3(.8f,.02f,3),white,null,false);
 }
 static void Building(string name,Vector3 position,int bays,int depthBays,int floors,float yaw) {
  var group=Group(name,geometry);group.localPosition=position;group.localRotation=Quaternion.Euler(0,yaw,0);
  float width=bays*3.2f,depth=depthBays*3.2f,height=floors*2.8f;
  ColliderBox(group,new Vector3(0,height/2,0),new Vector3(width,height,depth));
  for(int side=0;side<4;side++) {
   int count=side%2==0?bays:depthBays;
   float span=count*3.2f;
   for(int floor=0;floor<floors;floor++)for(int column=0;column<count;column++) {
    bool entrance=floor==0&&side==0&&column%5==2;
    string kind=entrance?"PanelWall_Entrance":floor>0&&side%2==0&&column%4==1?"PanelWall_Balcony":column%3==0?"PanelWall_Window_Tiles":"PanelWall_Window";
    float offset=-span/2+1.6f+column*3.2f;
    Vector3 point=side==0?new Vector3(offset,floor*2.8f,depth/2):side==1?new Vector3(width/2,floor*2.8f,-offset):side==2?new Vector3(-offset,floor*2.8f,-depth/2):new Vector3(-width/2,floor*2.8f,offset);
    Yard(kind,point,side*90,group,false);
   }
  }
  Box("Dach / do ladowania",new Vector3(0,height+.12f,0),new Vector3(width+.2f,.24f,depth+.2f),roof,group);
  Box("Cokol",new Vector3(0,.18f,0),new Vector3(width+.25f,.36f,depth+.25f),concrete,group);
  for(int i=0;i<3;i++)Box("Nadbudowka wentylacji",new Vector3(-width*.3f+i*width*.3f,height+.6f,0),new Vector3(1.2f,.8f,1.1f),rust,group);
  Combine(group);
 }
 static void Housing() {
  Building("Blok 01 / polnoc",new Vector3(-30,0,78),16,4,5,0);
  Building("Blok 02 / zachod",new Vector3(-92,0,-12),14,4,5,90);
  Building("Blok 03 / poludnie",new Vector3(-8,0,-80),15,4,5,180);
  Building("Blok 04 / wschod",new Vector3(86,0,7),15,4,6,90);
  Building("Przychodnia / dach trasy",new Vector3(-25,0,14),10,3,3,0);
  Building("Administracja garazy",new Vector3(-55,0,70),6,3,2,180);
 }
 static void Garages() {
  var group=Group("Garazowisko",geometry);
  for(int i=0;i<8;i++) {
   Yard("Garage",new Vector3(-80+i*3.5f,0,20),0,group);
   Yard("Garage",new Vector3(-80+i*3.5f,0,61),180,group);
  }
  for(int i=0;i<5;i++)Yard("Garage",new Vector3(-83,0,29+i*5.2f),90,group);
  Ramp("Wjazd na dach garazy",new Vector3(-76,0,9),new Vector3(-76,2.68f,17.3f),3.8f,rust);
  Box("Kladka nad przejazdem",new Vector3(-66,2.7f,24.5f),new Vector3(13,.25f,2.8f),rust);
  Yard("Kiosk",new Vector3(-49,0,28),-90);
  Yard("CarpetBeater",new Vector3(-44,0,35),90);
  Yard("PullUpBar",new Vector3(-44,0,39),90);
 }
 static void Factory() {
  var group=Group("Zaklad i kotlownia",geometry);
  Box("Hala - posadzka",new Vector3(46,.15f,24),new Vector3(30,.3f,20),concrete,group);
  // Open central hall and four broad exits preserve sightlines and enemy circulation.
  foreach(float x in new[]{31f,61f})foreach(float z in new[]{14f,34f})Box("Slup hali",new Vector3(x,3,z),new Vector3(1,6,1),brick,group);
  Box("Dach hali - arena gorna",new Vector3(46,6.1f,24),new Vector3(32,.3f,22),roof,group);
  foreach(float x in new[]{31f,61f}) {
   Box("Sciana hali",new Vector3(x,2.8f,18),new Vector3(.5f,5.6f,7),brick,group);
   Box("Sciana hali",new Vector3(x,2.8f,30),new Vector3(.5f,5.6f,7),brick,group);
  }
  for(int i=0;i<6;i++)Box("Swiatlik z niebieska blacha",new Vector3(35+i*4.2f,3,34.15f),new Vector3(2.4f,2,.25f),blue,group);
  Ramp("Rampa serwisowa hali",new Vector3(24,0,12),new Vector3(24,6.2f,35),4,concrete);
  Box("Podest rampy",new Vector3(28,6.1f,35),new Vector3(8,.3f,4),rust);
  var chimney=GameObject.CreatePrimitive(PrimitiveType.Cylinder);chimney.name="Komin / punkt orientacyjny";chimney.transform.SetParent(group,false);chimney.transform.position=new Vector3(56,13,46);chimney.transform.localScale=new Vector3(4.2f,13,4.2f);chimney.GetComponent<Renderer>().sharedMaterial=brick;
  for(int y=6;y<26;y+=7)Box("Opaska komina",new Vector3(56,y,46),new Vector3(4.35f,.45f,4.35f),roof,group);
  Yard("Kiosk",new Vector3(13,0,41),180);
  Yard("Kiosk",new Vector3(23,0,69),0);
  Yard("BusStop",new Vector3(5,0,76),0);
 }
 static void Park() {
  var group=Group("Skwer i plac zabaw",geometry);
  Box("Plac skweru",new Vector3(51,.035f,-37),new Vector3(46,.07f,40),grass,group);
  Box("Sciezka skweru",new Vector3(51,.1f,-37),new Vector3(36,.2f,3),concrete,group);
  Box("Sciezka skweru",new Vector3(51,.1f,-37),new Vector3(3,.2f,30),concrete,group);
  for(int i=0;i<4;i++) {
   Yard("Bench",new Vector3(36+i*10,0,-43),0,group);
   Yard("Bench",new Vector3(36+i*10,0,-31),180,group);
  }
  Yard("CarpetBeater",new Vector3(61,0,-52),0,group);
  Yard("PullUpBar",new Vector3(65,0,-52),0,group);
  Box("Piaskownica",new Vector3(37,.18f,-51),new Vector3(7,.36f,6),concrete,group);
  Box("Piasek",new Vector3(37,.38f,-51),new Vector3(6.4f,.12f,5.4f),white,group);
  for(int i=0;i<5;i++)Box("Niskie oslony / lawki betonowe",new Vector3(30+i*9,.55f,-58),new Vector3(3,1.1f,1),concrete,group);
  var positions=new[]{V(29,-54),V(29,-22),V(38,-26),V(44,-54),V(63,-54),V(70,-30),V(75,-53),V(68,-19),V(37,-63),V(53,-66),V(72,-66)};
  for(int i=0;i<positions.Length;i++)Tree(i%3,positions[i],7+i%4,group);
 }
 static void Ramp(string name,Vector3 start,Vector3 end,float width,Material material) {
  var direction=end-start;
  var ramp=Box(name,(start+end)*.5f-Vector3.up*.12f,new Vector3(width,.25f,direction.magnitude+.2f),material);
  ramp.transform.rotation=Quaternion.LookRotation(direction,Vector3.up);
 }
 static void Movement() {
  var group=Group("Petla parkour / rury i dachy",geometry);
  for(int i=0;i<5;i++)Yard("GasPipe_Arch",new Vector3(-48+i*6,0,-54),0,group);
  for(int i=0;i<8;i++)Yard("GasPipe_Straight",new Vector3(-45+i*4,0,-18),0,group);
  Yard("BusStop",new Vector3(-62,0,-64),180);
  Yard("Kiosk",new Vector3(-47,0,-56),0);
  Ramp("Rampa startowa na kiosk",new Vector3(-42,0,-64),new Vector3(-42,3.25f,-54),3,concrete);
  Box("Ladowanie kiosku",new Vector3(-44.5f,3.2f,-54),new Vector3(5,.25f,3.5f),roof);
  Box("Wyspa parkour 01",new Vector3(-42,1.9f,-39),new Vector3(5,3.8f,5),brick);
  Box("Wyspa parkour 02",new Vector3(-36,2.7f,-26),new Vector3(5,5.4f,5),brick);
  Box("Daszek lacznika",new Vector3(-35,6.7f,-12),new Vector3(8,.35f,7),concrete);
  Ramp("Dojscie na dach / dolna rampa",new Vector3(-50,0,-5),new Vector3(-50,4.2f,14),3.5f,concrete);
  Box("Podest nawrotu",new Vector3(-50,4.1f,14),new Vector3(4,.3f,4),rust);
  Ramp("Dojscie na dach / gorna rampa",new Vector3(-50,4.2f,14),new Vector3(-40.4f,8.55f,14),3.5f,rust);
  Box("Podpora estakady",new Vector3(-50,2,14),new Vector3(.7f,4,.7f),concrete);
  Ramp("Lacznik na przychodnie",new Vector3(-35,6.85f,-8),new Vector3(-35,8.55f,8.5f),3.2f,rust);
  Ramp("Podejscie na antresole",new Vector3(-22,0,-7),new Vector3(-22,3,-22),3.5f,concrete);
  Box("Antresola podworka",new Vector3(-22,3,-27),new Vector3(7,.3f,10),roof);
  Box("Wall jump / lewa sciana",new Vector3(-16,3.8f,-25),new Vector3(.6f,7.6f,10),brick);
  Box("Wall jump / prawa sciana",new Vector3(-12,3.8f,-25),new Vector3(.6f,7.6f,10),concrete);
  Box("Szczyt szybu / ladowanie",new Vector3(-14,7.7f,-21),new Vector3(4.7f,.3f,3),rust);
  Box("Kladka nad aleja",new Vector3(5,8.52f,14),new Vector3(29,.28f,3.2f),rust);
  Ramp("Zjazd z kladki na hale",new Vector3(17,8.55f,14),new Vector3(31,6.25f,20),3.2f,rust);
  // A pedestrian slab crosses the sunken service ditch; its banks are traversable slopes.
  Box("Mostek nad rowem serwisowym",new Vector3(42,-.12f,-76),new Vector3(3.4f,.25f,17),concrete);
  Yard("GasPipe_Straight",new Vector3(38,-2,-76),0);
  // Low service passage is optional: standing capsules stop, sliding capsules fit.
  Box("Tunel slide / strop 1.4m",new Vector3(13,1.55f,-35),new Vector3(3.4f,.3f,14),concrete);
  foreach(float x in new[]{10.9f,15.1f})Box("Tunel slide / bok",new Vector3(x,.9f,-35),new Vector3(.65f,1.8f,14),brick);
  foreach(float z in new[]{-43f,-27f})Box("Niebieskie oznaczenie slizgu",new Vector3(13,.045f,z),new Vector3(3,.04f,1),blue,null,false);
  for(int i=0;i<3;i++) {
   float h=3+i*1.7f;
   Box("Podesty dostepu do wysokich dachow",new Vector3(-64,h-.15f,65+i*4),new Vector3(4,.3f,3),rust);
  }
  Pickup(FoodKind.Jetpack,new Vector3(-25,9.4f,14));
  Pickup(FoodKind.Pierog,new Vector3(46,7.3f,24));
  Pickup(FoodKind.Pierog,new Vector3(-66,3.6f,24));
 }
 static void Pickup(FoodKind kind,Vector3 point) {
  // Use the authored model and the existing pickup contract.
  string model=kind==FoodKind.Jetpack?"JetpackPickup":"PierogPickup";
  var root=Group("Znajdzka / "+kind,geometry);root.position=point;
  WorldFactory.Model(model,root);
  var trigger=root.gameObject.AddComponent<SphereCollider>();trigger.radius=.8f;trigger.isTrigger=true;
  root.gameObject.AddComponent<Pickup>().kind=kind;
 }
 static void Retro(string model,Vector3 point,float longest,float yaw=0) {
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(RetroPath+"/Models/"+model+".fbx");
  if(!prefab)throw new Exception("Brak rekwizytu "+model);
  var wrapper=Group(model,geometry);var instance=UnityEngine.Object.Instantiate(prefab,wrapper,false);
  var all=instance.GetComponentsInChildren<Renderer>();var bounds=all[0].bounds;foreach(var renderer in all)bounds.Encapsulate(renderer.bounds);
  float scale=longest/Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z));
  instance.transform.localScale*=scale;
  bounds=all[0].bounds;foreach(var renderer in all)bounds.Encapsulate(renderer.bounds);
  instance.transform.position-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
  point.y+=TerrainHeight(point.x,point.z);
  wrapper.position=point;wrapper.rotation=Quaternion.Euler(0,yaw,0);
  var mat=Material("Retro_rekwizyty",RetroPath+"/Textures/T_Retro_Props_BaseColor.png",Color.white);
  foreach(var renderer in all)renderer.sharedMaterial=mat;
  // Bounds provide clean broad-phase collision on clutter instead of snagging on every slat.
  bounds=all[0].bounds;foreach(var renderer in all)bounds.Encapsulate(renderer.bounds);
  var collider=new GameObject("Kolizja oslony");collider.transform.SetParent(geometry,false);collider.transform.position=bounds.center;collider.AddComponent<BoxCollider>().size=bounds.size;
 }
 static void Tree(int variant,Vector3 position,float height,Transform parent) {
  string[] names={"District_Oak","District_Pine","District_Birch"};
  var part=trees.GetComponentsInChildren<Transform>().First(t=>t.name==names[variant]);
  var wrapper=Group(names[variant],parent);var instance=UnityEngine.Object.Instantiate(part.gameObject,wrapper,false);instance.transform.localPosition=Vector3.zero;
  var renderers=instance.GetComponentsInChildren<MeshRenderer>();
  Bounds bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
  instance.transform.localScale*=height/bounds.size.y;
  bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
  instance.transform.position-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
  position.y+=TerrainHeight(position.x,position.z);
  wrapper.position=position;wrapper.rotation=Quaternion.Euler(0,variant*73+position.x,0);
  foreach(var renderer in renderers) {
   renderer.sharedMaterials=renderer.sharedMaterials.Select(material=> {
    string key=material.name.Replace(" (Instance)","");
    string tex=key=="Material.001"?"Tree_Trunk.png":key=="Material.002"?"Pine_Trunk.jpg":key=="Material.003"?"Oak_Trunk.png":key=="Material.004"?"Tree_leaves.png":key=="Material.005"?"Oak_leaves.png":"Pine_leaves.png";
    bool leaf=tex.Contains("leaves");
    return Material("Tree_"+Path.GetFileNameWithoutExtension(tex),"Assets/ItchAssets/PSx_Environment/Textura/"+tex,leaf?new Color(.65f,.7f,.47f):Color.white,false,1,leaf);
   }).ToArray();
   renderer.shadowCastingMode=ShadowCastingMode.TwoSided;
  }
  Combine(wrapper);
  foreach(var renderer in wrapper.GetComponentsInChildren<MeshRenderer>())renderer.shadowCastingMode=ShadowCastingMode.TwoSided;
  ColliderBox(wrapper,new Vector3(0,height*.3f,0),new Vector3(.55f,height*.6f,.55f));
 }
 static void Dressing() {
  var points=new[]{V(-53,-48),V(-20,-47),V(-51,-17),V(-28,-8),V(-61,34),V(-61,47),V(-43,48),V(16,45),V(28,57),V(37,9),V(52,38),V(64,19)};
  for(int i=0;i<points.Length;i++) {
   Retro(i%2==0?"SM_Crate_Wooden_03":"SM_Barrel_02",points[i],i%2==0?1.7f:1.1f,i*31);
   if(i%3==0)Retro("SM_Cardboard_Box_01",points[i]+new Vector3(1.8f,0,.6f),1.1f,i*50);
  }
  foreach(var point in new[]{V(-60,-54),V(-37,-49),V(-41,-6),V(14,55),V(65,7),V(71,-12)})Yard("TrashContainer",point,80);
  foreach(var point in new[]{V(-45,-44),V(-18,-49),V(16,-5),V(-66,53),V(39,41),V(61,-46)})Yard("Bench",point,40);
  var lights=new[]{V(-52,-59),V(-9,-45),V(9,-20),V(-9,16),V(9,43),V(-68,10),V(-46,55),V(42,51),V(66,-9),V(27,-47)};
  foreach(var point in lights)Yard("StreetLamp",point,point.x<0?90:270);
  for(int i=0;i<6;i++)Tree(i%3,new Vector3(-75+i*9,0,-67),8+i%3,geometry);
  for(int i=0;i<5;i++)Tree(i%3,new Vector3(75,0,49+i*7),9,geometry);
  // Long yellow pipes tie the roofs and alleys together as a navigation landmark.
  for(int i=0;i<9;i++)Yard("GasPipe_Straight",new Vector3(21+i*4,0,49),0);
  for(int i=0;i<5;i++)Yard("GasPipe_Arch",new Vector3(-70+i*8,0,6),0);
 }
 static void ArtPass() {
  var group=Group("Detale osiedla / instalacje i podworka",geometry);
  // Turn the movement islands into utility outbuildings with doors, roof caps and pipes.
  foreach(var point in new[]{new Vector3(-42,0,-39),new Vector3(-36,0,-26)}) {
   float height=point.z<-30?3.8f:5.4f;
   Yard("PanelWall_Entrance",point+new Vector3(0,0,2.55f),0,group,false);
   Box("Papa budynku technicznego",point+Vector3.up*(height+.04f),new Vector3(5.25f,.08f,5.25f),roof,group);
   Box("Kratka wentylacyjna",point+new Vector3(2.53f,height*.65f,0),new Vector3(.1f,.8f,1.6f),blue,group,false);
   for(int i=0;i<5;i++)Box("Zaluzja wentylacji",point+new Vector3(2.6f,height*.65f-.3f+i*.15f,0),new Vector3(.12f,.04f,1.6f),roof,group,false);
  }
  Yard("GasPipe_Arch",new Vector3(-42,0,-36),0,group);
  for(int i=0;i<6;i++)Box("Belka dzwigara hali",new Vector3(34+i*4.8f,5.7f,24),new Vector3(.18f,.45f,20),rust,group);
  for(int i=0;i<3;i++) {
   Retro("SM_Crate_Wooden_04",new Vector3(37+i*7,.3f,28),1.7f,20+i*30);
   Yard("GasPipe_Straight",new Vector3(57,0,17+i*4),90,group);
  }
  foreach(float x in new[]{-58f,-29f}) {
   for(int i=0;i<5;i++)Box("Zuzyty krawężnik podworka",new Vector3(x,.2f,-40+i*4),new Vector3(.35f,.4f,2.8f),concrete,group);
  }
  for(int i=0;i<4;i++) {
   Yard("Kiosk",new Vector3(13+i*6,0,72),0,group);
   Yard("Bench",new Vector3(13+i*6,0,67),180,group);
  }
  foreach(var point in new[]{V(-78,-45),V(-78,-31),V(-73,-18),V(-66,-27),V(92,52),V(96,68),V(94,82),V(82,80),V(-96,75),V(-97,87)})Tree(Mathf.Abs(Mathf.RoundToInt(point.x))%3,point,8,group);
  // Concrete pads break up the large lawns and point towards building entrances.
  foreach(var point in new[]{V(-72,-55),V(-72,-5),V(-14,-65),V(77,-31)}) {
   for(int i=0;i<6;i++) {
    var centre=point+new Vector3(i*1.6f,0,0);centre.y=TerrainHeight(centre.x,centre.z)+.08f;
    Box("Spekana plyta chodnikowa",centre,new Vector3(1.5f,.16f,2.5f),concrete,group);
   }
  }
 }
 static void Backdrop() {
  for(int i=0;i<5;i++) {
   string path="Assets/ItchAssets/buildings/building_"+(i+1).ToString("00")+"_smooth.obj";
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
   if(!prefab)continue;
   var wrapper=Group("Daleki blok "+i,geometry);var model=UnityEngine.Object.Instantiate(prefab,wrapper,false);
   var renderers=model.GetComponentsInChildren<Renderer>();if(renderers.Length==0)continue;
   var bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
   model.transform.localScale*=24/Mathf.Max(.001f,bounds.size.y);
   bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
   model.transform.position-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
   wrapper.position=new Vector3(-80+i*40,0,113);wrapper.rotation=Quaternion.Euler(0,i%2*180,0);
   var mat=Material("Dalekie_bloki","Assets/ItchAssets/buildings/diffuse_.jpg",new Color(.65f,.68f,.7f));
   foreach(var renderer in renderers)renderer.sharedMaterial=mat;
  }
  foreach(float x in new[]{-107f,107f})Box("Mur obrzeza",new Vector3(x,3,0),new Vector3(1,6,194),concrete);
  foreach(float z in new[]{-96f,96f})Box("Mur obrzeza",new Vector3(0,3,z),new Vector3(214,6,1),concrete);
 }
 static void Combine(Transform building) {
  var grouped=new Dictionary<Material,List<CombineInstance>>();
  var renderers=building.GetComponentsInChildren<MeshRenderer>();
  foreach(var renderer in renderers) {
   var filter=renderer.GetComponent<MeshFilter>();if(!filter||!filter.sharedMesh)continue;
   for(int sub=0;sub<filter.sharedMesh.subMeshCount;sub++) {
    var material=renderer.sharedMaterials[Mathf.Min(sub,renderer.sharedMaterials.Length-1)];
    if(!grouped.TryGetValue(material,out var parts)){parts=new List<CombineInstance>();grouped[material]=parts;}
    parts.Add(new CombineInstance{mesh=filter.sharedMesh,subMeshIndex=sub,transform=building.worldToLocalMatrix*filter.transform.localToWorldMatrix});
   }
  }
  foreach(var pair in grouped) {
   var mesh=new Mesh{name=building.name+"_"+pair.Key.name,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(pair.Value.ToArray(),true,true);mesh.RecalculateBounds();
   string path=Output+"/Meshes/Building_"+(meshOrdinal++)+".asset";SaveAsset(mesh,path);
   var child=Group(pair.Key.name,building);child.gameObject.AddComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);child.gameObject.AddComponent<MeshRenderer>().sharedMaterial=pair.Key;child.gameObject.isStatic=true;
  }
  foreach(var renderer in renderers){
   var filter=renderer.GetComponent<MeshFilter>();var go=renderer.gameObject;
   UnityEngine.Object.DestroyImmediate(renderer);if(filter)UnityEngine.Object.DestroyImmediate(filter);
   if(go.transform.childCount==0&&go.GetComponents<Component>().Length==1)UnityEngine.Object.DestroyImmediate(go);
  }
 }
 static void ConfigureLighting(GameObject root,GameSession session) {
  RenderSettings.skybox=null;RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.54f,.57f,.6f);
  RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=95;RenderSettings.fogEndDistance=240;RenderSettings.fogColor=new Color(.46f,.51f,.55f);
  var sun=root.GetComponentsInChildren<Light>().First(l=>l.type==LightType.Directional);sun.color=new Color(1,.83f,.65f);sun.intensity=.85f;sun.transform.rotation=Quaternion.Euler(36,-35,0);sun.shadows=LightShadows.Soft;
  QualitySettings.shadowDistance=100;
  session.player.view.backgroundColor=RenderSettings.fogColor;session.player.view.clearFlags=CameraClearFlags.SolidColor;session.player.view.farClipPlane=300;
 }
 static void Check(bool ok,string text){if(!ok)throw new Exception(text);checks.Add("PASS "+text);}
 static void Validate(GameSession session,DistrictCombat combat) {
  Check(session.player&&session.navigation,"Player and saved navigation are present");
  Check(geometry.GetComponentsInChildren<Renderer>().Length>150,"Imported environment and props populate the district");
  Check(Vector3.Distance(session.player.transform.position,PostSovietDistrict.Spawn)<.001f,"Saved player uses Unity spawn coordinates without mirroring Z");
  var body=session.player.GetComponent<CharacterController>();
  var spawnCentre=session.player.transform.TransformPoint(body.center);
  var half=Vector3.up*Mathf.Max(0,body.height*.5f-body.radius);
  Check(!Physics.CheckCapsule(spawnCentre-half,spawnCentre+half,body.radius,~((1<<8)|(1<<9)|(1<<11)),QueryTriggerInteraction.Ignore),"Entire starting capsule is clear of world geometry");
  Check(Physics.Raycast(spawnCentre,Vector3.down,out var ground,3,1,QueryTriggerInteraction.Ignore)&&ground.normal.y>.7f,"Actual player start has walkable physical ground");
  Check(combat.encounters.Length==5,"Five authored combat sectors are available");
  var nav=NavMesh.AddNavMeshData(session.navigation);
  try {
   Check(NavMesh.SamplePosition(PostSovietDistrict.Spawn,out var spawn,3,NavMesh.AllAreas),"Start is connected to navigation");
   Check(NavMesh.SamplePosition(new Vector3(5,8.7f,14),out var roofNav,1,NavMesh.AllAreas),"Rooftop crossing has navigation");
   var roofPath=new NavMeshPath();
   Check(NavMesh.CalculatePath(spawn.position,roofNav.position,NavMesh.AllAreas,roofPath)&&roofPath.status==NavMeshPathStatus.PathComplete,"Enemy route climbs from street to rooftops");
   foreach(var encounter in combat.encounters) {
    Check(NavMesh.SamplePosition(encounter.centre,out var centre,1,NavMesh.AllAreas),"Sector centre is walkable / "+encounter.name);
    var path=new NavMeshPath();
    Check(NavMesh.CalculatePath(spawn.position,centre.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete,"Ground route reaches sector / "+encounter.name);
    foreach(var entry in encounter.entries)Check(NavMesh.SamplePosition(entry,out var hit,.8f,NavMesh.AllAreas)&&Mathf.Abs(hit.position.y-entry.y)<.3f,"Enemy entry lies on the floor / "+entry);
   }
  }finally{nav.Remove();}
  Check(Physics.Raycast(new Vector3(13,.1f,-35),Vector3.up,out var tunnel,2)&&Mathf.Abs(tunnel.point.y-1.4f)<.05f,"Slide shortcut has 1.4m clearance");
  Check(Physics.Raycast(new Vector3(5,10,14),Vector3.down,out var bridge,3)&&bridge.point.y>8,"High crossing supports the rooftop route");
  foreach(var material in materials.Values)Check(material.shader.isSupported,"Environment shader supported / "+material.name);
 }
 static void RenderViews(GameObject root) {
  var camera=new GameObject("Podglad dzielnicy").AddComponent<Camera>();camera.cullingMask=~((1<<8)|(1<<9)|(1<<11));camera.fieldOfView=68;camera.farClipPlane=450;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=RenderSettings.fogColor;
  camera.gameObject.AddComponent<DistrictRetroEffect>();
  foreach(var other in root.GetComponentsInChildren<Camera>())other.enabled=false;
  Directory.CreateDirectory("Validation/Screenshots");
  Render(camera,new Vector3(-107,58,-105),new Vector3(-5,0,10),"district-overview");
  Render(camera,new Vector3(-62,2.5f,-59),new Vector3(-29,6,-25),"district-street");
  Render(camera,new Vector3(-65,7,10),new Vector3(-62,2.5f,43),"district-garages");
  Render(camera,new Vector3(64,13,-8),new Vector3(36,4,27),"district-factory");
  Render(camera,new Vector3(56,1,-86),new Vector3(40,-1.5f,-76),"district-ditch");
  Render(camera,new Vector3(73,4,-63),new Vector3(41,3,-32),"district-park");
  foreach(var other in root.GetComponentsInChildren<Camera>())other.enabled=true;
  UnityEngine.Object.DestroyImmediate(camera.gameObject);
 }
 static void Render(Camera camera,Vector3 position,Vector3 target,string name) {
  camera.transform.position=position;camera.transform.LookAt(target);
  var rt=new RenderTexture(1280,720,24);camera.targetTexture=rt;camera.Render();
  var previous=RenderTexture.active;RenderTexture.active=rt;
  var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes("Validation/Screenshots/"+name+".png",image.EncodeToPNG());
  RenderTexture.active=previous;camera.targetTexture=null;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);
 }
}
}