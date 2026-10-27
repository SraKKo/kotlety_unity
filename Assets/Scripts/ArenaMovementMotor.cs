using UnityEngine;
namespace RetyKotlety {
public struct ArenaMoveInput {
 public Vector3 wish,forward;public bool jump,dash,slidePressed,slideHeld;
}
public struct ArenaMoveContact {
 public bool grounded,wall;public Vector3 wallNormal,groundNormal;public float height;
}
// Timers and movement state use the physics timestep; no polling of Input or Time here.
public sealed class ArenaMovementMotor {
 readonly MovementTuning tuning;
 public Vector3 velocity;
 public float stamina {get;private set;}
 public bool Sliding {get;private set;}public bool Slamming {get;private set;}
 public bool Dashing=>dashRemaining>0;
 public int WallJumpsRemaining {get;private set;}
 public bool Jumped {get;private set;}public bool DashStarted {get;private set;}public bool SlamLanded {get;private set;}
 public float SlamBounceRemaining=>bounceRemaining;
 public bool HasBufferedJump=>jumpRemaining>0;
 float dashRemaining,regenDelay,jumpRemaining,coyote,wallCooldown,steerLock,wallWeight,sinceJump=1,bounceRemaining,bounceSpeed,slamHeight;
 Vector3 dashDirection;
 public ArenaMovementMotor(MovementTuning tuning){this.tuning=tuning;stamina=tuning.maxStamina;WallJumpsRemaining=tuning.wallJumps;}
 public void ClearJumpBuffer(){jumpRemaining=0;}
 public void Step(ArenaMoveInput input,ArenaMoveContact contact,float dt){
  Jumped=false;DashStarted=false;SlamLanded=false;if(dt<=0)return;
  wallCooldown=Mathf.Max(0,wallCooldown-dt);steerLock=Mathf.Max(0,steerLock-dt);regenDelay=Mathf.Max(0,regenDelay-dt);bounceRemaining=Mathf.Max(0,bounceRemaining-dt);sinceJump+=dt;
  jumpRemaining=input.jump?tuning.jumpBuffer:Mathf.Max(0,jumpRemaining-dt);
  coyote=contact.grounded?tuning.coyoteTime:Mathf.Max(0,coyote-dt);
  var horizontal=new Vector3(velocity.x,0,velocity.z);
  Vector3 wish=Vector3.ClampMagnitude(Vector3.ProjectOnPlane(input.wish,Vector3.up),1);
  Vector3 forward=Vector3.ProjectOnPlane(input.forward,Vector3.up).normalized;if(forward.sqrMagnitude<.5f)forward=Vector3.forward;
  if(contact.grounded){WallJumpsRemaining=tuning.wallJumps;wallWeight=0;
   if(Slamming){Slamming=false;SlamLanded=true;bounceRemaining=.15f;float height=Mathf.Max(0,slamHeight-contact.height);bounceSpeed=Mathf.Clamp(Mathf.Sqrt(2*tuning.gravity*(height+1)),tuning.slamBounceMin,tuning.slamBounceMax);}
   if(velocity.y<0)velocity.y=-2;
  }
  if(!input.slideHeld)Sliding=false;
  if(input.dash&&stamina>=1){
   stamina-=1;regenDelay=tuning.staminaDelay;dashRemaining=tuning.dashDuration;dashDirection=wish.sqrMagnitude>.01f?wish.normalized:forward;horizontal=dashDirection*tuning.dashSpeed;velocity.y=0;Slamming=false;Sliding=false;DashStarted=true;
  }
  if(input.slideHeld&&contact.grounded&&!Sliding&&!Dashing){
   Sliding=true;float speed=Mathf.Clamp(Mathf.Max(horizontal.magnitude,tuning.slideSpeed),tuning.slideSpeed,tuning.maxMomentum);
   Vector3 heading=horizontal.magnitude>tuning.slideSpeed+.1f?horizontal.normalized:wish.sqrMagnitude>.01f?wish.normalized:horizontal.sqrMagnitude>.01f?horizontal.normalized:forward;horizontal=heading*speed;
  }
  if(input.slidePressed&&!contact.grounded&&sinceJump>.08f&&!Dashing){Slamming=true;Sliding=false;slamHeight=contact.height;horizontal=Vector3.zero;velocity.y=-tuning.slamSpeed;}
  if(jumpRemaining>0){
   bool floorJump=coyote>0;
   bool wallJump=!floorJump&&contact.wall&&wallCooldown<=0&&WallJumpsRemaining>0;
   if(floorJump||wallJump){
    if(wallJump){var jump=MovementMath.WallJump(new Vector3(horizontal.x,0,horizontal.z),contact.wallNormal,tuning.wallOutwardSpeed,tuning.jumpSpeed,tuning.wallTangentSpeed);horizontal=new Vector3(jump.x,0,jump.z);velocity.y=jump.y;WallJumpsRemaining--;wallCooldown=tuning.wallCooldown;steerLock=tuning.wallSteerLock;}
    else {velocity.y=bounceRemaining>0?bounceSpeed:tuning.jumpSpeed;
     if(Dashing){if(stamina>=1){stamina-=1;horizontal=dashDirection*tuning.dashSpeed;}else horizontal=Vector3.ClampMagnitude(horizontal,tuning.runSpeed);dashRemaining=0;}
    }
    Slamming=false;Jumped=true;jumpRemaining=0;coyote=0;bounceRemaining=0;sinceJump=0;
   }
  }
  if(Dashing){horizontal=dashDirection*tuning.dashSpeed;velocity.y=0;dashRemaining=Mathf.Max(0,dashRemaining-dt);if(dashRemaining<.00001f)dashRemaining=0;}
  else if(Slamming){horizontal=Vector3.MoveTowards(horizontal,wish*.48f,2*dt);velocity.y=-tuning.slamSpeed;}
  else {
   bool grounded=contact.grounded&&!Jumped;
   if(Sliding){
    float speed=horizontal.magnitude;if(grounded)speed=Mathf.MoveTowards(speed,tuning.slideSpeed,tuning.slideDrag*dt);
    if(wish.sqrMagnitude>.01f&&speed>.01f)horizontal=Vector3.RotateTowards(horizontal.normalized,wish.normalized,tuning.slideTurnRate*dt,0)*speed;else horizontal=horizontal.normalized*speed;
    if(grounded&&contact.groundNormal.y>.1f){var slope=Vector3.ProjectOnPlane(Vector3.down*tuning.gravity,contact.groundNormal);horizontal+=new Vector3(slope.x,0,slope.z)*dt;}
   }else if(steerLock<=0){horizontal=MovementMath.ArenaHorizontal(horizontal,wish,tuning.runSpeed,grounded?tuning.groundAcceleration:tuning.airAcceleration,grounded?tuning.groundBraking:tuning.airFriction,grounded,dt);}
   velocity.y=Mathf.Max(-tuning.terminalSpeed,velocity.y-tuning.gravity*dt);
   if(!grounded&&contact.wall&&velocity.y<0&&Vector3.Dot(wish,contact.wallNormal)<-.2f){wallWeight+=dt;velocity.y=Mathf.Max(velocity.y,-tuning.wallFallSpeed-wallWeight*12);}
  }
  horizontal=Vector3.ClampMagnitude(horizontal,tuning.maxMomentum);velocity.x=horizontal.x;velocity.z=horizontal.z;
  if(!Dashing&&!Sliding&&regenDelay<=0)stamina=Mathf.Min(tuning.maxStamina,stamina+tuning.staminaRegen*dt);
 }
 public void HitWall(Vector3 normal){
  float into=Vector3.Dot(velocity,normal);if(into<0)velocity-=normal*into;
  if(Dashing&&Vector3.Dot(dashDirection,normal)<-.1f)dashRemaining=0;
 }
}
}
