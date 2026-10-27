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
 [Header("Profil ruchu arenowego")]
 public MovementTuning movement;
 [HideInInspector]
 public float speed=20,jumpForce=7.265f,gravity=9.8f;
 [HideInInspector] public float groundAcceleration=20,airAcceleration=5,groundFriction=15,airFriction=1;
 [HideInInspector] public float wallJumpOutwardSpeed=6,wallJumpTangentSpeed=8,wallJumpCooldown=.3f;
 public float standingHeight=2,crouchingHeight=1;
 public Vector3 cameraRest=new Vector3(0,1.091f,-.294f);
 public Vector3 velocity;
 CharacterController controller; Vector3 moveInput,wallNormal,groundNormal=Vector3.up,hookPoint;
 Quaternion cameraRotation; Enemy hookedEnemy; float pitch,hookCooldown,ropeLength;
 float lastWallContact=float.NegativeInfinity; bool sliding,hooked,jumpQueued,slideHeld,flightHeld;
 ArenaMovementMotor motor;bool dashQueued,slidePressedQueued;float eyeHeight,roll,baseFov,landingKick;Vector3 previousPosition,currentPosition;
 public float DashStamina=>motor!=null?motor.stamina:3;
 public bool IsDashing=>motor!=null&&motor.Dashing;
 public bool IsSlamming=>motor!=null&&motor.Slamming;
 public int WallJumpsRemaining=>motor!=null?motor.WallJumpsRemaining:3;
 const int WorldMask=~((1<<8)|(1<<11));
 public bool IsFlying=>canFly&&fuel>0&&flightHeld&&!IsDashing&&!IsSlamming&&GameSession.Instance&&!GameSession.Instance.paused;
 public bool Blocking=>weapon==FoodKind.Pan && weapons && weapons.Blocking;
 void Awake(){
  controller=GetComponent<CharacterController>();if(!movement)movement=Resources.Load<MovementTuning>("Movement/ArenaMovement");if(!movement)movement=ScriptableObject.CreateInstance<MovementTuning>();motor=new ArenaMovementMotor(movement);
 }
 void OnEnable(){
  // Script reloads in the open editor can recreate non-serialized motor state.
  if(!controller)controller=GetComponent<CharacterController>();
  if(!movement)movement=Resources.Load<MovementTuning>("Movement/ArenaMovement");
  if(!movement)movement=ScriptableObject.CreateInstance<MovementTuning>();
  if(motor==null){motor=new ArenaMovementMotor(movement);motor.velocity=velocity;}
  previousPosition=currentPosition=transform.position;
  if(view&&baseFov<=0){baseFov=view.fieldOfView;eyeHeight=cameraRest.y;cameraRotation=Quaternion.Euler(0,view.transform.localEulerAngles.y,0);}
 }
 void Start(){speed=movement.runSpeed;jumpForce=movement.jumpSpeed;gravity=movement.gravity;groundAcceleration=movement.groundAcceleration;airAcceleration=movement.airAcceleration;groundFriction=movement.groundBraking;airFriction=movement.airFriction;wallJumpOutwardSpeed=movement.wallOutwardSpeed;wallJumpTangentSpeed=movement.wallTangentSpeed;wallJumpCooldown=movement.wallCooldown;eyeHeight=cameraRest.y;baseFov=view.fieldOfView;previousPosition=currentPosition=transform.position;cameraRotation=Quaternion.Euler(0,view.transform.localEulerAngles.y,0);var game=GameSession.Instance;if(game)game.player=this;Equip(FoodKind.Pan);}
 void Update(){
  var game=GameSession.Instance;if(!game||game.paused){jumpQueued=dashQueued=slidePressedQueued=false;motor?.ClearJumpBuffer();return;}
  transform.Rotate(0,Input.GetAxisRaw("Mouse X")*game.sensitivity*10,0);
  pitch=Mathf.Clamp(pitch-Input.GetAxisRaw("Mouse Y")*game.sensitivity*10,-89,89);
  moveInput=Vector3.ClampMagnitude(new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical")),1);
  roll=Mathf.LerpAngle(roll,-moveInput.x*2,1-Mathf.Exp(-movement.cameraResponse*Time.deltaTime));
  view.transform.localRotation=cameraRotation*Quaternion.Euler(pitch,0,roll);
  if(Input.GetKeyDown(KeyCode.Space))jumpQueued=true;
  if(Input.GetKeyDown(movement.dashKey))dashQueued=true;
  slideHeld=Input.GetKey(movement.slideKey)||Input.GetKey(KeyCode.RightControl)||Input.GetKey(movement.alternateSlideKey);
  if(Input.GetKeyDown(movement.slideKey)||Input.GetKeyDown(KeyCode.RightControl)||Input.GetKeyDown(movement.alternateSlideKey))slidePressedQueued=true;
  flightHeld=Input.GetKey(KeyCode.Tab);
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
  float dt=Time.fixedDeltaTime;hookCooldown=Mathf.Max(0,hookCooldown-dt);
  previousPosition=transform.position;
  bool freshWall=Time.fixedTime-lastWallContact<=.08f;
  if(!freshWall&&(jumpQueued||motor.HasBufferedJump)){ProbeWall();freshWall=Time.fixedTime-lastWallContact<=.08f;}
  bool jump=jumpQueued;
  if(jump&&(hooked||hookedEnemy)){ReleaseHook();velocity.y=Mathf.Max(velocity.y,movement.jumpSpeed*.5f);jump=false;}
  motor.velocity=velocity;
  var input=new ArenaMoveInput{wish=transform.TransformDirection(moveInput),forward=transform.forward,jump=jump,dash=dashQueued,slidePressed=slidePressedQueued,slideHeld=slideHeld};
  motor.Step(input,new ArenaMoveContact{grounded=controller.isGrounded,wall=freshWall,wallNormal=wallNormal,groundNormal=groundNormal,height=transform.position.y},dt);
  jumpQueued=dashQueued=slidePressedQueued=false;
  velocity=motor.velocity;sliding=motor.Sliding;
  if(motor.DashStarted||motor.Slamming){if(hooked||hookedEnemy)ReleaseHook();}
  if(motor.Jumped){game.Sound(game.jumpSound,transform.position);lastWallContact=float.NegativeInfinity;}
  if(motor.SlamLanded){landingKick=.1f;Effect.Emit(transform.position-Vector3.up*controller.height*.5f,Vector3.up,ImpactEffect.Bounce);}
  TryHeight(sliding?crouchingHeight:standingHeight);
  if(IsFlying){velocity.y=Mathf.MoveTowards(velocity.y,6,48*dt);fuel=Mathf.Max(0,fuel-30*dt);}
  if(hooked){
   if(Input.GetKey(KeyCode.R))ropeLength=Mathf.Max(2,ropeLength-15*dt);
   var offset=transform.position-hookPoint;float distance=offset.magnitude;
   if(distance>ropeLength){var normal=offset/distance;float outward=Vector3.Dot(velocity,normal);if(outward>0)velocity-=normal*outward;velocity-=normal*Mathf.Min((distance-ropeLength)/dt,15);}
   velocity+=input.wish*10*dt;
  }
  if(hookedEnemy){hookedEnemy.Pull(transform.position,25*dt);if(Vector3.Distance(transform.position,hookPoint)<2)ReleaseHook();}
  motor.velocity=velocity;
  var flags=controller.Move(velocity*dt);velocity=motor.velocity;
  if((flags&CollisionFlags.Above)!=0&&velocity.y>0)velocity.y=0;
  if((flags&CollisionFlags.Below)!=0&&velocity.y<0)velocity.y=-2;
  currentPosition=transform.position;
  if(transform.position.y < -50)Damage(1000);
 }
 void LateUpdate(){
  if(!view||!GameSession.Instance||GameSession.Instance.paused)return;
  float dt=Time.deltaTime;float response=1-Mathf.Exp(-movement.cameraResponse*dt);
  bool low=controller.height<standingHeight-.1f;
  eyeHeight=Mathf.Lerp(eyeHeight,cameraRest.y*(low?.5f:1),response);landingKick=Mathf.Lerp(landingKick,0,response);
  var target=cameraRest;target.y=eyeHeight-landingKick;
  if((currentPosition-transform.position).sqrMagnitude>4)previousPosition=currentPosition=transform.position;
  if(movement.interpolateCamera){float alpha=Mathf.Clamp01((Time.time-Time.fixedTime)/Time.fixedDeltaTime);target+=transform.InverseTransformVector(Vector3.Lerp(previousPosition,currentPosition,alpha)-transform.position);}
  view.transform.localPosition=target;
  float fast=Mathf.Clamp01((new Vector3(velocity.x,0,velocity.z).magnitude-movement.runSpeed)/Mathf.Max(1,movement.dashSpeed-movement.runSpeed));
  view.fieldOfView=Mathf.Lerp(view.fieldOfView,baseFov+movement.speedFov*fast,response);
 }
 void ProbeWall(){
  var directions=new[]{transform.forward,-transform.forward,transform.right,-transform.right};float nearest=.16f;
  foreach(var direction in directions)if(Physics.SphereCast(transform.position+controller.center,controller.radius*.9f,direction,out var hit,nearest,WorldMask&~(1<<9),QueryTriggerInteraction.Ignore)&&Mathf.Abs(hit.normal.y)<.2f){nearest=hit.distance;wallNormal=hit.normal;lastWallContact=Time.fixedTime;}
 }
 bool TryHeight(float height){
  if(Mathf.Abs(controller.height-height)<.001f)return true;
  float old=controller.height;Vector3 feet=transform.position+controller.center-Vector3.up*old*.5f;
  if(height>old){float r=controller.radius*.95f;if(Physics.CheckCapsule(feet+Vector3.up*(controller.radius+.03f),feet+Vector3.up*(height-controller.radius),r,WorldMask,QueryTriggerInteraction.Ignore))return false;}
  controller.height=height;controller.center=Vector3.zero;controller.Move(Vector3.up*(height-old)*.5f);return true;
 }
 void OnControllerColliderHit(ControllerColliderHit hit){
  if(hit.normal.y>.5f)groundNormal=hit.normal;
  else if(Mathf.Abs(hit.normal.y)<.2f&&Vector3.Dot(hit.moveDirection,hit.normal)<-.001f){wallNormal=hit.normal;lastWallContact=Time.fixedTime;motor?.HitWall(hit.normal);}
 }
 void AttachHook(RaycastHit hit){if(!hit.collider){ReleaseHook();return;}hookPoint=hit.point;ropeLength=Vector3.Distance(transform.position,hookPoint);hookedEnemy=hit.collider.GetComponentInParent<Enemy>();hooked=!hookedEnemy;}
 public void ReleaseHook(){hooked=false;hookedEnemy=null;hookCooldown=1;if(rope)rope.Hide();if(hookHand)hookHand.Release();}
 public void Equip(FoodKind kind){weapon=kind;if(weapons)weapons.Equip(kind);else{if(pan)pan.SetActive(kind==FoodKind.Pan);if(schabowy)schabowy.SetActive(kind==FoodKind.Schabowy);if(chrupiaca)chrupiaca.SetActive(kind==FoodKind.Chrupiaca);}}
 public void Receive(FoodKind kind){switch(kind){case FoodKind.Schabowy:hasSchabowy=true;Equip(kind);break;case FoodKind.Chrupiaca:hasChrupiaca=true;Equip(kind);break;case FoodKind.Pierog:pierogi++;break;case FoodKind.Jetpack:canFly=true;fuel=100;break;}}
 void Interact(){pickupHint="";if(Physics.Raycast(view.transform.position,view.transform.forward,out var hit,4,WorldMask,QueryTriggerInteraction.Collide)){var pickup=hit.collider.GetComponentInParent<Pickup>();if(pickup){pickupHint="F — "+pickup.kind;if(Input.GetKeyDown(KeyCode.F)){Receive(pickup.kind);Destroy(pickup.gameObject);}}}}
 public void Heal(){if(pierogi<=0||health>=100)return;pierogi--;health=Mathf.Min(100,health+20);var g=GameSession.Instance;if(g)g.Sound(g.healSound,transform.position);}
 public void Damage(float amount){health=Mathf.Max(0,health-amount);if(health<=0)SceneManager.LoadScene(SceneManager.GetActiveScene().name);}
 void AttackImpact(){
  var game=GameSession.Instance;if(!game||game.paused){jumpQueued=dashQueued=slidePressedQueued=false;motor?.ClearJumpBuffer();return;}
  if(weapon==FoodKind.Pan){var hits=Physics.OverlapSphere(view.transform.position+view.transform.forward*1.5f,1.5f,WorldMask,QueryTriggerInteraction.Ignore);var seen=new System.Collections.Generic.HashSet<Enemy>();foreach(var c in hits){var e=c.GetComponentInParent<Enemy>();if(e&&seen.Add(e)&&Vector3.Dot(view.transform.forward,(e.transform.position-transform.position).normalized)>0)e.Damage(100+5*velocity.magnitude,c.ClosestPoint(view.transform.position),-view.transform.forward,true);}return;}
  game.Sound(game.throwSound,transform.position);
  var origin=view.transform.position+view.transform.forward*.65f;
  if(weapon==FoodKind.Chrupiaca)Projectile.SpawnVolley(origin,view.transform.forward,view.transform.up,game.chrupiacaModel,gameObject);
  else Projectile.Spawn(origin,view.transform.forward,30,50,5,false,game.schabowyModel,gameObject);
 }
}
}
