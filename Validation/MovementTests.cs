using System;
using UnityEngine;
using RetyKotlety;
public static class MovementTests {
 static int passed;
 static void Check(bool ok,string label){if(!ok)throw new Exception(label);passed++;Console.WriteLine("PASS "+label);}
 static bool Near(float a,float b,float tolerance=.001f){return Math.Abs(a-b)<=tolerance;}
 static float Travel(float dt){var velocity=MovementMath.WallJump(Vector3.zero,Vector3.right,6,7.265f,8);float x=0,y=0,t=0;while(y>=0&&t<4){if(t>=.15f){var h=MovementMath.Horizontal(velocity,Vector3.zero,20,5,1,dt);velocity.x=h.x;velocity.z=h.z;}velocity.y-=9.8f*dt;x+=velocity.x*dt;y+=velocity.y*dt;t+=dt;}return x;}
 public static void Main(){
  var result=MovementMath.WallJump(new Vector3(40,-5,99),Vector3.right,6,7.265f,8);
  Check(Near(result.x,6)&&Near(result.y,7.265f)&&Near(result.z,8),"Wall jump uses 6 m/s outward and caps tangential momentum at 8 m/s");
  Check(new Vector3(result.x,0,result.z).magnitude<=10.001f,"No 49.875 m/s launch survives in wall jump");
  Check(MovementMath.CanWallJump(1,.925f,0,Vector3.right),"Fresh wall contact can jump");
  Check(!MovementMath.CanWallJump(1,.9f,0,Vector3.right),"Expired wall contact cannot jump");
  Check(!MovementMath.CanWallJump(1,.99f,.1f,Vector3.right),"Wall jump respects cooldown");
  Check(!MovementMath.CanWallJump(1,.99f,0,Vector3.up),"Floor normal cannot be used as wall");
  Vector3 v=Vector3.zero;for(int i=0;i<60;i++)v=MovementMath.Horizontal(v,new Vector3(1,0,1),20,20,15,1f/60);
  Check(Near(v.magnitude,20,.01f),"Diagonal ground movement is limited to 20 m/s");
  Check(Near(MovementMath.Horizontal(new Vector3(10,0,0),Vector3.zero,20,20,15,.1f).x,8.5f),"Ground friction decelerates with time");
  float a=Travel(1f/30),b=Travel(1f/60),c=Travel(1f/120);
  Check(b<9,"Unsteered wall jump lands within 9 metres");
  Check(Math.Abs(a-c)<.3f,"Wall jump travel remains consistent at 30, 60 and 120 physics steps");
  Console.WriteLine("Wall jump travel (m): 30 Hz="+a.ToString("F3")+", 60 Hz="+b.ToString("F3")+", 120 Hz="+c.ToString("F3"));
  Console.WriteLine(passed+" movement regression checks passed.");
 }
}
