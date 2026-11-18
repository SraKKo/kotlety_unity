using System;
using UnityEngine;
using RetyKotlety;
public static class ProjectileFanChecks {
 static void Check(bool ok,string label){if(!ok)throw new Exception(label);Console.WriteLine("PASS "+label);}
 public static void Run(){
  var forward=Vector3.forward;var up=Vector3.up;
  Check(Vector3.Distance(ProjectileFan.Direction(forward,up,0,1,36),forward)<.0001f,"Single projectile stays on the crosshair");
  var left=ProjectileFan.Direction(forward,up,0,3,ProjectileFan.HalfSpread(3));var middle=ProjectileFan.Direction(forward,up,1,3,ProjectileFan.HalfSpread(3));var right=ProjectileFan.Direction(forward,up,2,3,ProjectileFan.HalfSpread(3));
  Check(Mathf.Abs(Vector3.Angle(left,forward)-6)<.001f&&Vector3.Distance(middle,forward)<.001f&&Mathf.Abs(Vector3.Angle(right,forward)-6)<.001f,"Base Chrupiaca spread remains minus six, zero and plus six degrees");
  var a=ProjectileFan.Direction(forward,up,0,2,ProjectileFan.HalfSpread(2));var b=ProjectileFan.Direction(forward,up,1,2,ProjectileFan.HalfSpread(2));Check(Mathf.Abs(a.x+b.x)<.001f&&Mathf.Abs(a.z-b.z)<.001f&&a.x<0&&b.x>0,"Even volleys are symmetric");
  var tilted=new Vector3(0,.8f,.6f);var tiltedUp=new Vector3(0,.6f,-.8f);
  for(int i=0;i<7;i++){var direction=ProjectileFan.Direction(tilted,tiltedUp,i,7,ProjectileFan.HalfSpread(7));Check(Mathf.Abs(Vector3.Dot(direction,tiltedUp))<.001f&&Mathf.Abs(direction.magnitude-1)<.001f,"Extra projectile remains normalized and in the aiming plane: "+i);}
  Check(ProjectileFan.HalfSpread(1000)==36,"Large volleys stay inside a seventy-two-degree fan");
 }
}
