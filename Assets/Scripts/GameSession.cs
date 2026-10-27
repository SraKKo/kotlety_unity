using UnityEngine;
using UnityEngine.SceneManagement;
namespace RetyKotlety {
public class GameSession : MonoBehaviour {
 public static GameSession Instance { get; private set; }
 public bool menuScene, paused, options, won;
 public PlayerController player;
 public GameObject schabowyModel, chrupiacaModel, tomatoModel, meleePrefab, shootingPrefab;
 public Texture2D background,logo,healthEmpty,healthFill;
 public Font interfaceFont;
 public AudioClip menuMusic,jumpSound,throwSound,bounceSound,hookSound,healSound;
 public UnityEngine.AI.NavMeshData navigation;
 UnityEngine.AI.NavMeshDataInstance navInstance;
 AudioSource music; GUIStyle title,label,button;
 public float sensitivity=0.1146f;
 public static float Volume { get => PlayerPrefs.GetFloat("volume",0.8f); set { PlayerPrefs.SetFloat("volume",value); AudioListener.volume=value; } }
 public void InstallNavigation(){if(navInstance.valid)navInstance.Remove();if(navigation)navInstance=UnityEngine.AI.NavMesh.AddNavMeshData(navigation);}
 void Start(){if(!menuScene&&player)GameHud.Install(player);Cursor.lockState=menuScene?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=menuScene;if(menuScene&&menuMusic&&!music){music=gameObject.AddComponent<AudioSource>();music.clip=menuMusic;music.loop=true;music.volume=PlayerPrefs.GetFloat("music",.6f);music.Play();}}
 void Awake() {
  Instance=this; Time.timeScale=1; AudioListener.volume=Volume;
  sensitivity=PlayerPrefs.GetFloat("sensitivity",0.1146f);
  if(navigation) navInstance=UnityEngine.AI.NavMesh.AddNavMeshData(navigation);
  Cursor.lockState=menuScene?CursorLockMode.None:CursorLockMode.Locked; Cursor.visible=menuScene;
  if(menuMusic && menuScene) { music=gameObject.AddComponent<AudioSource>(); music.clip=menuMusic; music.loop=true; music.volume=PlayerPrefs.GetFloat("music",0.6f); music.Play(); }
 }
 void OnDestroy() { if(navInstance.valid) navInstance.Remove(); Time.timeScale=1; if(Instance==this) Instance=null; }
 void Update() { if(!menuScene && !won && Input.GetKeyDown(KeyCode.Escape)) SetPause(!paused); }
 public void SetPause(bool value) { paused=value; options=false; Time.timeScale=value?0:1; Cursor.lockState=value?CursorLockMode.None:CursorLockMode.Locked; Cursor.visible=value; }
 public void Win() { won=true; SetPause(true); }
 public void StartGame() { Time.timeScale=1; SceneManager.LoadScene("Level001"); }
 public void StartTestLevel(){Time.timeScale=1;SceneManager.LoadScene("TestLevel");}
 public void RestartLevel(){Time.timeScale=1;SceneManager.LoadScene(SceneManager.GetActiveScene().name);}
 public void MainMenu() { Time.timeScale=1; SceneManager.LoadScene("MainMenu"); }
 public void Sound(AudioClip clip,Vector3 point) { if(clip) AudioSource.PlayClipAtPoint(clip,point,PlayerPrefs.GetFloat("effects",1)); }
 void Styles() {
  if(title!=null) return;
  title=new GUIStyle(GUI.skin.label){fontSize=48,alignment=TextAnchor.MiddleCenter}; title.normal.textColor=new Color(1,.85f,.35f);
  label=new GUIStyle(GUI.skin.label){fontSize=22};
  button=new GUIStyle(GUI.skin.button){fontSize=24};
  if(interfaceFont){title.font=interfaceFont;label.font=interfaceFont;button.font=interfaceFont;}
 }
 void OnGUI() {
  Styles(); GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/720f,1));
  if(menuScene || paused) {
   if(menuScene && background) GUI.DrawTexture(new Rect(0,0,1280,720),background,ScaleMode.ScaleAndCrop);
   GUI.Box(new Rect(400,70,480,580),GUIContent.none);
   if(logo && menuScene) GUI.DrawTexture(new Rect(435,82,410,150),logo,ScaleMode.ScaleToFit);
   else GUI.Label(new Rect(410,90,460,100),won?"Smacznego! Wygrana!":"Pauza",title);
   if(options) {
    GUI.Label(new Rect(440,240,400,35),"Głośność główna",label); Volume=GUI.HorizontalSlider(new Rect(440,285,400,24),Volume,0,1);
    GUI.Label(new Rect(440,320,400,35),"Muzyka",label); float mv=GUI.HorizontalSlider(new Rect(440,365,400,24),PlayerPrefs.GetFloat("music",.6f),0,1); PlayerPrefs.SetFloat("music",mv); if(music) music.volume=mv;
    GUI.Label(new Rect(440,400,400,35),"Efekty dźwiękowe",label); PlayerPrefs.SetFloat("effects",GUI.HorizontalSlider(new Rect(440,445,400,24),PlayerPrefs.GetFloat("effects",1),0,1));
    GUI.Label(new Rect(440,475,400,35),"Czułość myszy",label); sensitivity=GUI.HorizontalSlider(new Rect(440,520,400,24),sensitivity,.02f,.4f); PlayerPrefs.SetFloat("sensitivity",sensitivity);
    if(GUI.Button(new Rect(440,565,400,50),"Powrót",button)) {options=false;PlayerPrefs.Save();}
   } else {
    if(GUI.Button(new Rect(460,260,360,60),menuScene?"Graj":won?"Zagraj ponownie":"Wznów",button)) {if(menuScene)StartGame();else if(won)RestartLevel();else SetPause(false);}
    if(menuScene&&GUI.Button(new Rect(460,340,360,60),"Poziom testowy",button))StartTestLevel();
    if(GUI.Button(new Rect(460,menuScene?420:340,360,60),"Opcje",button)) options=true;
    if(!menuScene && GUI.Button(new Rect(460,420,360,60),"Menu główne",button)) MainMenu();
    if(GUI.Button(new Rect(460,500,360,60),"Wyjdź",button)) Application.Quit();
   }
   return;
  }
  if(!player) return;
  if(player.pickupHint!="") GUI.Label(new Rect(430,470,500,40),player.pickupHint,label);
 }
}
}




