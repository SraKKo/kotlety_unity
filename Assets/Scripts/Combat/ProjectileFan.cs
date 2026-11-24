using UnityEngine;
namespace RetyKotlety {
// All ranged weapons share a symmetric, horizontal spread relative to the aiming camera.
public static class ProjectileFan {
 public static float HalfSpread(int count,float gap=6){return Mathf.Min(36,Mathf.Max(0,count-1)*Mathf.Max(0,gap)*.5f);}
 public static Vector3 Direction(Vector3 forward,Vector3 up,int index,int count,float halfSpread){
  forward=forward.normalized;if(count<=1)return forward;
  var right=Vector3.Cross(up,forward).normalized;
  if(right.sqrMagnitude<.001f)right=Vector3.Cross(Mathf.Abs(forward.y)>.9f?Vector3.forward:Vector3.up,forward).normalized;
  float angle=Mathf.Lerp(-halfSpread,halfSpread,Mathf.Clamp01((float)index/(count-1)))*Mathf.Deg2Rad;
  return (forward*Mathf.Cos(angle)+right*Mathf.Sin(angle)).normalized;
 }
}
}
