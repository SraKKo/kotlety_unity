using UnityEngine;
namespace RetyKotlety {
// Pure movement calculations also used by the regression checks.
public static class MovementMath {
 public static Vector3 Horizontal(Vector3 velocity,Vector3 input,float speed,float acceleration,float friction,float dt){
  var current=new Vector3(velocity.x,0,velocity.z);
  var target=Vector3.ClampMagnitude(input,1)*speed;
  return Vector3.MoveTowards(current,target,(input.sqrMagnitude>.0001f?acceleration:friction)*dt);
 }
 public static Vector3 ArenaHorizontal(Vector3 velocity,Vector3 wish,float runSpeed,float acceleration,float braking,bool grounded,float dt){
  velocity.y=0;wish=Vector3.ClampMagnitude(wish,1);
  if(wish.sqrMagnitude<.0001f)return Vector3.MoveTowards(velocity,Vector3.zero,braking*dt);
  float speed=velocity.magnitude;
  // Steering redirects acquired momentum in the air instead of braking to run speed.
  float targetSpeed=grounded?Mathf.MoveTowards(Mathf.Max(runSpeed,speed),runSpeed,braking*dt):Mathf.Max(runSpeed,speed);
  if(!grounded&&speed>runSpeed+.01f){float heading=Mathf.Atan2(velocity.x,velocity.z)*Mathf.Rad2Deg;float target=Mathf.Atan2(wish.x,wish.z)*Mathf.Rad2Deg;heading=Mathf.MoveTowardsAngle(heading,target,acceleration/speed*Mathf.Rad2Deg*dt)*Mathf.Deg2Rad;return new Vector3(Mathf.Sin(heading),0,Mathf.Cos(heading))*speed;}
  return Vector3.MoveTowards(velocity,wish.normalized*targetSpeed,acceleration*dt);
 }
 public static Vector3 WallJump(Vector3 velocity,Vector3 normal,float outwardSpeed,float upwardSpeed,float maxTangentSpeed){
  normal=Vector3.ProjectOnPlane(normal,Vector3.up).normalized;
  var horizontal=new Vector3(velocity.x,0,velocity.z);
  var tangent=Vector3.ClampMagnitude(Vector3.ProjectOnPlane(horizontal,normal),maxTangentSpeed);
  return tangent+normal*outwardSpeed+Vector3.up*upwardSpeed;
 }
 public static bool CanWallJump(float now,float contactTime,float cooldown,Vector3 normal){return cooldown<=0 && now-contactTime<=.08f && Mathf.Abs(normal.y)<.2f && normal.sqrMagnitude>.5f;}
}
}
