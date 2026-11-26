using UnityEngine;
namespace RetyKotlety {
// Currency is credited only on collection, independently of kill XP.
public class PointPickup : MonoBehaviour {
 static Material gold;int value;Vector3 rest;float age;bool collected;
 PlayerProgression collectionOwner;
 Vector2 flightOrigin;
 float flightStarted, flightDelay;
 Renderer pickupRenderer;
 GUIStyle flightLabel;
 const float FlightDuration = .85f;
 public bool IsCollected => collected;
 const int GroundMask=~((1<<8)|(1<<9)|(1<<11));
 public static void Spawn(Vector3 position,int value){
  if(value<=0)return;
  var root=GameObject.CreatePrimitive(PrimitiveType.Cube);root.name="Punkty +"+value;root.layer=2;
  var collider=root.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
  if(Physics.Raycast(position+Vector3.up,Vector3.down,out var hit,80,GroundMask,QueryTriggerInteraction.Ignore))position=hit.point;
  root.transform.position=position+Vector3.up*.35f;root.transform.localScale=Vector3.one*.24f;
  if(!gold)gold=new Material(Shader.Find("Sprites/Default")){name="Zlote punkty",color=new Color(1,.8f,.15f)};
  root.GetComponent<Renderer>().sharedMaterial=gold;
  var pickup=root.AddComponent<PointPickup>();pickup.value=value;pickup.rest=root.transform.position;
 }
 public void Collect(PlayerProgression progression){
  if(collected||!progression)return;
  collected=true;
  progression.State.AddPoints(value);
  Destroy(gameObject);
 }
 public void BeginShopCollection(PlayerProgression owner, int index, int count) {
  if(collected || collectionOwner || !owner)return;
  collectionOwner = owner;
  flightStarted = Time.unscaledTime;
  flightDelay = count > 1 ? .35f * index / (count - 1) : 0;
  var camera = owner.GetComponent<PlayerController>().view;
  var projected = camera.WorldToViewportPoint(transform.position);
  var direction = new Vector2(projected.x - .5f, projected.y - .5f);
  if(projected.z < 0)direction = -direction;
  if(direction.sqrMagnitude < .001f)direction = Vector2.right;
  if(projected.z <= 0)direction = direction.normalized * .6f;
  flightOrigin = new Vector2(Mathf.Clamp(.5f + direction.x, .04f, .96f), Mathf.Clamp(.5f - direction.y, .07f, .9f));
  pickupRenderer = GetComponent<Renderer>();
  if(pickupRenderer)pickupRenderer.enabled = false;
 }
 float FlightProgress => Mathf.Clamp01((Time.unscaledTime - flightStarted - flightDelay) / FlightDuration);
 void OnGUI() {
  if(!collectionOwner || collected)return;
  float progress = FlightProgress;
  var origin = new Vector2(flightOrigin.x * Screen.width, flightOrigin.y * Screen.height);
  var target = new Vector2(Screen.width * .5f, Screen.height * .64f);
  float eased = progress * progress * progress;
  var position = Vector2.Lerp(origin, target, eased);
  position.y -= Mathf.Sin(progress * Mathf.PI) * Screen.height * .09f;
  float size = Mathf.Clamp(Screen.height / 90f, 6, 16);
  var oldColor = GUI.color;
  var oldMatrix = GUI.matrix;
  GUI.matrix = Matrix4x4.identity;
  int depth = GUI.depth;
  GUI.depth = -80;
  for(int i = 3; i > 0; i--) {
   var trail = Vector2.Lerp(position, origin, i * .035f * progress);
   GUI.color = new Color(1, .62f, .08f, .18f);
   GUI.DrawTexture(new Rect(trail.x - size / 2, trail.y - size / 2, size, size), Texture2D.whiteTexture);
  }
  GUI.color = new Color(1, .85f, .25f);
  GUI.DrawTexture(new Rect(position.x - size / 2, position.y - size / 2, size, size), Texture2D.whiteTexture);
  if(flightLabel == null)flightLabel = new GUIStyle(GUI.skin.label) { font = Resources.Load<Font>("UI/PixelAE-Bold"), alignment = TextAnchor.MiddleCenter };
  flightLabel.fontSize = Mathf.RoundToInt(size * 1.8f);
  GUI.Label(new Rect(position.x - 36, position.y - size * 3, 72, size * 3), "+" + value, flightLabel);
  GUI.color = oldColor;
  GUI.matrix = oldMatrix;
  GUI.depth = depth;
 }
 void Update(){
  if(collectionOwner) {
   if(FlightProgress >= 1)Collect(collectionOwner);
   return;
  }
  var session=GameSession.Instance;if(collected||!session||session.paused||!session.player)return;
  var progression=session.player.GetComponent<PlayerProgression>();if(!progression)return;
  age+=Time.deltaTime;transform.Rotate(35*Time.deltaTime,100*Time.deltaTime,0,Space.Self);
  var target=session.player.transform.position;float distance=Vector3.Distance(transform.position,target);
  // A ray check keeps magnet pickups on the player's side of walls/floors.
  bool reachable=distance<5&&!Physics.Linecast(transform.position,target,GroundMask,QueryTriggerInteraction.Ignore);
  if(reachable){transform.position=Vector3.MoveTowards(transform.position,target,14*Time.deltaTime);rest=transform.position;if(Vector3.Distance(transform.position,target)<.85f)Collect(progression);}
  else {transform.position=new Vector3(rest.x,rest.y+Mathf.Sin(age*3)*.08f,rest.z);}
 }
}
}
