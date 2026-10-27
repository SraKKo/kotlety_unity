using UnityEngine;
using UnityEngine.SceneManagement;
namespace RetyKotlety {
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
 public Animation armAnimation;
 public Camera view; public Transform hand; public GameObject pan,schabowy,chrupiaca;
 public WeaponPresentation weapons; public HookHand hookHand; public SausageRope rope;
 public float health=100,fuel; public int pierogi; public bool canFly,hasSchabowy,hasChrupiaca;
 public FoodKind weapon=FoodKind.Pan; public string pickupHint="";
 [Header("Ruch — metry i sekundy")]
 public float speed=20,jumpForce=7.265f,gravity=9.8f;
 public float groundAcceleration=20,airAcceleration=5,groundFriction=15,airFriction=1;
 [Min(0)] public float wallJumpOutwardSpeed=6,wallJumpTangentSpeed=8,wallJumpCooldown=.3f;
 public float standingHeight=2,crouchingHeight=1;
 public Vector3 cameraRest=new Vector3(0,1.091f,-.294f);
 public Vector3 velocity;
 CharacterController controller; Vector3 moveInput,wallNormal,groundNormal=Vector3.up,hookPoint;
 Quaternion cameraRotation; Enemy hookedEnemy; float pitch,slideTimer,hookCooldown,wallTimer,wallSteerLock,ropeLength;
 float lastWallContact=float.NegativeInfinity; bool sliding,hooked,jumpQueued,slideHeld,flightHeld;
 const int WorldMask=~((1<<8)|(1<<11));
 public bool IsFlying=>canFly&&fuel>0&&flightHeld&&GameSession.Instance&&!GameSession.Instance.paused;
 public bool Blocking=>weapon==FoodKind.Pan && weapons && weapons.Blocking;
 void Awake(){controller=GetComponent<CharacterController>();}
 void Start(){cameraRotation=Quaternion.Euler(0,view.transform.localEulerAngles.y,0);var game=GameSession.Instance;if(game)game.player=this;Equip(FoodKind.Pan);}
 void Update(){
  var game=GameSession.Instance;if(!game||game.paused)return;
  transform.Rotate(0,Input.GetAxisRaw("Mouse X")*game.sensitivity*10,0);
  pitch=Mathf.Clamp(pitch-Input.GetAxisRaw("Mouse Y")*game.sensitivity*10,-89,89);
  moveInput=Vector3.ClampMagnitude(new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical")),1);
  view.transform.localRotation=cameraRotation*Quaternion.Euler(pitch,0,-moveInput.x*2);
  var cameraTarget=cameraRest;cameraTarget.y=sliding?cameraRest.y*.5f:cameraRest.y;
  view.transform.localPosition=Vector3.Lerp(view.transform.localPosition,cameraTarget,1-Mathf.Exp(-15*Time.deltaTime));
  if(Input.GetKeyDown(KeyCode.Space))jumpQueued=true;
  slideHeld=Input.GetKey(KeyCode.LeftShift);flightHeld=Input.GetKey(KeyCode.Tab);
  if(Input.GetKeyDown(KeyCode.V))Equip(FoodKind.Pan);
  if(Input.GetKeyDown(KeyCode.Alpha1)&&hasSchabowy)Equip(FoodKind.Schabowy);
  if(Input.GetKeyDown(KeyCode.Alpha2)&&hasChrupiaca)Equip(FoodKind.Chrupiaca);
  if(Input.mouseScrollDelta.y!=0){if(weapon==FoodKind.Pan)Equip(hasSchabowy?FoodKind.Schabowy:hasChrupiaca?FoodKind.Chrupiaca:FoodKind.Pan);else if(weapon==FoodKind.Schabowy&&hasChrupiaca)Equip(FoodKind.Chrupiaca);else Equip(FoodKind.Pan);}
  Interact();if(Input.GetKeyDown(KeyCode.E))Heal();
  if(Input.GetKeyDown(KeyCode.Q)){
   if(hooked||hookedEnemy||(hookHand&&hookHand.IsBusy))ReleaseHook();
   else if(hookCooldown<=0&&hookHand){
    bool hit=Physics.Raycast(view.transform.position,view.transform.forward,out var target,80,WorldMask,QueryTriggerInteraction.Ignore);
    hookHand.Begin(hit?(System.Action)(()=>AttachHook(target)):null,hit);
    game.Sound(game.hookSound,transform.position);
   }
  }
  if(hookedEnemy)hookPoint=hookedEnemy.transform.position+Vector3.up;
  if(hooked||hookedEnemy){if(rope)rope.Show(hookPoint);if(hookHand)hookHand.Aim(hookPoint);}
  else if(rope)rope.Hide();
  if(weapons){weapons.SetBlock(weapon==FoodKind.Pan&&Input.GetMouseButton(1));if((weapon==FoodKind.Chrupiaca?Input.GetMouseButton(0):Input.GetMouseButtonDown(0)))weapons.Attack(AttackImpact);}
 }
 void FixedUpdate(){
  var game=GameSession.Instance;if(!game||game.paused)return;
  float dt=Time.fixedDeltaTime;wallTimer=Mathf.Max(0,wallTimer-dt);wallSteerLock=Mathf.Max(0,wallSteerLock-dt);slideTimer=Mathf.Max(0,slideTimer-dt);hookCooldown=Mathf.Max(0,hookCooldown-dt);
  bool grounded=controller.isGrounded;
  if(grounded&&velocity.y<0)velocity.y=-2;
  var direction=transform.TransformDirection(moveInput);
  if(!sliding&&slideHeld&&grounded&&slideTimer<=0){sliding=true;var h=new Vector3(velocity.x,0,velocity.z);if(h.sqrMagnitude<.01f)h=transform.forward;h*=1.5f;velocity.x=h.x;velocity.z=h.z;}
  if(sliding&&!slideHeld&&TryHeight(standingHeight)){sliding=false;slideTimer=.2f;}
  if(sliding)TryHeight(crouchingHeight);
  bool wallJump=false;
  if(jumpQueued){
   if(grounded){if(sliding&&TryHeight(standingHeight))sliding=false;velocity.y=jumpForce;game.Sound(game.jumpSound,transform.position);}
   else if(MovementMath.CanWallJump(Time.fixedTime,lastWallContact,wallTimer,wallNormal)){
    velocity=MovementMath.WallJump(velocity,wallNormal,wallJumpOutwardSpeed,jumpForce,wallJumpTangentSpeed);
    wallTimer=wallJumpCooldown;wallSteerLock=.15f;lastWallContact=float.NegativeInfinity;wallNormal=Vector3.zero;wallJump=true;
   }
   jumpQueued=false;
  }
  if(!sliding&&!wallJump&&wallSteerLock<=0){var h=MovementMath.Horizontal(velocity,direction,speed,grounded?groundAcceleration:airAcceleration,grounded?groundFriction:airFriction,dt);velocity.x=h.x;velocity.z=h.z;}
  else if(sliding&&grounded){var h=Vector3.ProjectOnPlane(new Vector3(velocity.x,0,velocity.z),groundNormal);h+=Vector3.ProjectOnPlane(Vector3.down*gravity,groundNormal)*dt;velocity.x=h.x;velocity.z=h.z;if(h.y<0)velocity.y=h.y-2;}
  velocity.y-=gravity*dt;
  if(canFly&&fuel>0&&flightHeld){velocity.y=Mathf.MoveTowards(velocity.y,6,48*dt);fuel=Mathf.Max(0,fuel-30*dt);}
  if(hooked){
   if(Input.GetKey(KeyCode.R))ropeLength=Mathf.Max(2,ropeLength-15*dt);
   var offset=transform.position-hookPoint;float distance=offset.magnitude;
   if(distance>ropeLength){var normal=offset/distance;float outward=Vector3.Dot(velocity,normal);if(outward>0)velocity-=normal*outward;velocity-=normal*Mathf.Min((distance-ropeLength)/dt,15);}
   velocity+=direction*10*dt;
  }
  if(hookedEnemy){hookedEnemy.Pull(transform.position,25*dt);if(Vector3.Distance(transform.position,hookPoint)<2)ReleaseHook();}
  var flags=controller.Move(velocity*dt);
  if((flags&CollisionFlags.Above)!=0&&velocity.y>0)velocity.y=0;
  if(transform.position.y < -50)Damage(1000);
 }
 bool TryHeight(float height){
  if(Mathf.Abs(controller.height-height)<.001f)return true;
  float old=controller.height;Vector3 feet=transform.position+controller.center-Vector3.up*old*.5f;
  if(height>old){float r=controller.radius*.95f;if(Physics.CheckCapsule(feet+Vector3.up*(controller.radius+.03f),feet+Vector3.up*(height-controller.radius),r,WorldMask,QueryTriggerInteraction.Ignore))return false;}
  controller.height=height;controller.center=Vector3.zero;controller.Move(Vector3.up*(height-old)*.5f);return true;
 }
 void OnControllerColliderHit(ControllerColliderHit hit){
  if(hit.normal.y>.5f)groundNormal=hit.normal;
  else if(Mathf.Abs(hit.normal.y)<.2f&&Vector3.Dot(hit.moveDirection,hit.normal)<-.01f){wallNormal=hit.normal;lastWallContact=Time.fixedTime;}
 }
 void AttachHook(RaycastHit hit){if(!hit.collider){ReleaseHook();return;}hookPoint=hit.point;ropeLength=Vector3.Distance(transform.position,hookPoint);hookedEnemy=hit.collider.GetComponentInParent<Enemy>();hooked=!hookedEnemy;}
 public void ReleaseHook(){hooked=false;hookedEnemy=null;hookCooldown=1;if(rope)rope.Hide();if(hookHand)hookHand.Release();}
 public void Equip(FoodKind kind){weapon=kind;if(weapons)weapons.Equip(kind);else{if(pan)pan.SetActive(kind==FoodKind.Pan);if(schabowy)schabowy.SetActive(kind==FoodKind.Schabowy);if(chrupiaca)chrupiaca.SetActive(kind==FoodKind.Chrupiaca);}}
 public void Receive(FoodKind kind){switch(kind){case FoodKind.Schabowy:hasSchabowy=true;Equip(kind);break;case FoodKind.Chrupiaca:hasChrupiaca=true;Equip(kind);break;case FoodKind.Pierog:pierogi++;break;case FoodKind.Jetpack:canFly=true;fuel=100;break;}}
 void Interact(){pickupHint="";if(Physics.Raycast(view.transform.position,view.transform.forward,out var hit,4,WorldMask,QueryTriggerInteraction.Collide)){var pickup=hit.collider.GetComponentInParent<Pickup>();if(pickup){pickupHint="F — "+pickup.kind;if(Input.GetKeyDown(KeyCode.F)){Receive(pickup.kind);Destroy(pickup.gameObject);}}}}
 public void Heal(){if(pierogi<=0||health>=100)return;pierogi--;health=Mathf.Min(100,health+20);var g=GameSession.Instance;if(g)g.Sound(g.healSound,transform.position);}
 public void Damage(float amount){health=Mathf.Max(0,health-amount);if(health<=0)SceneManager.LoadScene(SceneManager.GetActiveScene().name);}
 void AttackImpact(){
  var game=GameSession.Instance;if(!game||game.paused)return;
  if(weapon==FoodKind.Pan){var hits=Physics.OverlapSphere(view.transform.position+view.transform.forward*1.5f,1.5f,WorldMask,QueryTriggerInteraction.Ignore);var seen=new System.Collections.Generic.HashSet<Enemy>();foreach(var c in hits){var e=c.GetComponentInParent<Enemy>();if(e&&seen.Add(e)&&Vector3.Dot(view.transform.forward,(e.transform.position-transform.position).normalized)>0)e.Damage(100+5*velocity.magnitude,c.ClosestPoint(view.transform.position),-view.transform.forward,true);}return;}
  game.Sound(game.throwSound,transform.position);
  var origin=view.transform.position+view.transform.forward*.65f;
  if(weapon==FoodKind.Chrupiaca)Projectile.SpawnVolley(origin,view.transform.forward,view.transform.up,game.chrupiacaModel,gameObject);
  else Projectile.Spawn(origin,view.transform.forward,30,50,5,false,game.schabowyModel,gameObject);
 }
}
}



