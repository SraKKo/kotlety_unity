using System.Collections.Generic;
using UnityEngine;
namespace RetyKotlety {
// The 1280x720 Godot HUD was shown on a Sprite3D: 0.01 units/pixel,
// 4.14258 units from the weapon camera, turned 15 degrees around Y.
public class GameHud : MonoBehaviour {
 public PlayerController player;public Texture2D SelectedWeaponTexture {get;private set;}
 public float HealthFill {get;private set;}public float FuelFill {get;private set;}
 public bool FuelVisible=>player&&player.canFly;
 readonly List<Mesh> meshes=new List<Mesh>();readonly Dictionary<Texture,Material> materials=new Dictionary<Texture,Material>();
 HudImage health,icon,fuelBase,fuel;TextMesh healthText,pierogiText;FoodKind lastWeapon=(FoodKind)(-1);
 class HudImage {public GameObject gameObject;public MeshFilter filter;public MeshRenderer renderer;public Rect rect;public bool vertical;public float fill=-1;public Texture texture;}
 public static GameHud Install(PlayerController player){var existing=player.GetComponentInChildren<GameHud>(true);if(existing)return existing;var camera=player.view.transform.Find("Weapon Camera");var root=new GameObject("HUD Godot");root.transform.SetParent(camera,false);root.transform.localPosition=new Vector3(0,0,4.14258f);root.transform.localRotation=Quaternion.Euler(0,-15,0);root.layer=11;var hud=root.AddComponent<GameHud>();hud.player=player;hud.Build();if(!player.GetComponent<JetpackEffects>())player.gameObject.AddComponent<JetpackEffects>();return hud;}
 Material GetMaterial(Texture texture){texture.filterMode=FilterMode.Point;if(!materials.TryGetValue(texture,out var material)){material=new Material(Shader.Find("Sprites/Default")){name="HUD "+texture.name,mainTexture=texture};materials.Add(texture,material);}return material;}
 HudImage Image(string name,Texture texture,Rect rect,int order,Color color,bool vertical=false,Transform parent=null){
  var g=new GameObject(name);g.layer=11;g.transform.SetParent(parent?parent:transform,false);g.transform.localPosition=new Vector3(0,0,-order*.002f);
  var image=new HudImage{gameObject=g,filter=g.AddComponent<MeshFilter>(),renderer=g.AddComponent<MeshRenderer>(),rect=rect,texture=texture,vertical=vertical};
  var mesh=new Mesh(){name=name};meshes.Add(mesh);image.filter.sharedMesh=mesh;image.renderer.sharedMaterial=GetMaterial(texture);image.renderer.sortingOrder=order;SetFill(image,1,color);return image;
 }
 void SetFill(HudImage image,float value,Color color){value=Mathf.Clamp01(value);if(Mathf.Abs(image.fill-value)<.0001f)return;image.fill=value;image.renderer.enabled=value>0;if(value<=0)return;
  Rect r=image.rect;float u=1,v=1;if(image.vertical){r.y+=r.height*(1-value);r.height*=value;v=value;}else{r.width*=value;u=value;}
  float x=(r.x-640)*.01f,y=(360-r.y)*.01f,w=r.width*.01f,h=r.height*.01f;
  var mesh=image.filter.sharedMesh;mesh.Clear();mesh.vertices=new[]{new Vector3(x,y-h),new Vector3(x,y),new Vector3(x+w,y),new Vector3(x+w,y-h)};mesh.uv=new[]{new Vector2(0,0),new Vector2(0,v),new Vector2(u,v),new Vector2(u,0)};mesh.colors=new[]{color,color,color,color};mesh.triangles=new[]{0,1,2,0,2,3};mesh.RecalculateBounds();
 }
 TextMesh Text(string name,Vector2 center,float size){var g=new GameObject(name);g.layer=11;g.transform.SetParent(transform,false);g.transform.localPosition=new Vector3((center.x-640)*.01f,(360-center.y)*.01f,-.014f);var text=g.AddComponent<TextMesh>();text.font=Resources.Load<Font>("UI/PixelAE-Bold");text.fontSize=48;text.characterSize=size*.01f*10/text.fontSize;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=Color.white;var renderer=g.GetComponent<MeshRenderer>();renderer.sharedMaterial=text.font.material;renderer.sortingOrder=7;return text;}
 void Build(){
  var frame=Resources.Load<Texture2D>("UI/HealthBarEmpty");var fill=Resources.Load<Texture2D>("UI/HealthBarFill");
  Image("Panel",Texture2D.whiteTexture,new Rect(10,375,582,335),0,new Color(.12f,.12f,.12f,.7f));
  health=Image("Wypełnienie życia",fill,new Rect(55,620,fill.width,78),3,Color.white);Image("Ramka życia",frame,new Rect(20,620,485,78),4,Color.white);
  icon=Image("Wybrana broń",Resources.Load<Texture2D>("UI/kotlet_schabowy"),new Rect(33.75f,279,496.5f,480),2,Color.white);
  fuelBase=Image("Jetpack pusty",Resources.Load<Texture2D>("UI/jetpack_keczup_musztarda_dis"),new Rect(512,623,73.5f,73.5f),4,Color.white);
  fuel=Image("Paliwo jetpacka",Resources.Load<Texture2D>("UI/jetpack_keczup_musztarda"),new Rect(512,623,73.5f,73.5f),5,Color.white,true);
  healthText=Text("Punkty życia",new Vector2(56,661),22);pierogiText=Text("Pierogi",new Vector2(350,609),17);
  // The crosshair belongs to the camera overlay, not to the angled HUD panel.
  var crossRoot=new GameObject("Celownik");crossRoot.layer=11;crossRoot.transform.SetParent(transform.parent,false);crossRoot.transform.localPosition=new Vector3(0,0,.5f);
  var camera=transform.parent.GetComponent<Camera>();float height=2*Mathf.Tan(camera.fieldOfView*Mathf.Deg2Rad*.5f)*.5f*(40f/720f);
  var cross=Image("Oryginalny celownik",Resources.Load<Texture2D>("UI/crosshair"),new Rect(640-height*50,360-height*50,height*100,height*100),10,Color.white,false,crossRoot.transform);cross.gameObject.transform.localPosition=Vector3.zero;
  Refresh();
 }
 public void Refresh(){if(!player||health==null)return;HealthFill=Mathf.Clamp01(player.health/100);FuelFill=Mathf.Clamp01(player.fuel/100);SetFill(health,HealthFill,Color.white);SetFill(fuel,FuelFill,Color.white);fuelBase.gameObject.SetActive(FuelVisible);fuel.gameObject.SetActive(FuelVisible);healthText.text=Mathf.RoundToInt(player.health).ToString();pierogiText.text="Pierogi: "+player.pierogi;
  if(lastWeapon!=player.weapon){lastWeapon=player.weapon;string name=player.weapon==FoodKind.Schabowy?"kotlet_schabowy":player.weapon==FoodKind.Chrupiaca?"kotlet_mielony":"patelnia";SelectedWeaponTexture=Resources.Load<Texture2D>("UI/"+name);if(!SelectedWeaponTexture)SelectedWeaponTexture=Resources.Load<Texture2D>("UI/widelec");icon.texture=SelectedWeaponTexture;icon.renderer.sharedMaterial=GetMaterial(SelectedWeaponTexture);}
 }
 void LateUpdate(){Refresh();}
 void OnDestroy(){foreach(var mesh in meshes)if(mesh)Destroy(mesh);foreach(var material in materials.Values)if(material)Destroy(material);}
}
}

