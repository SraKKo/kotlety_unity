using UnityEngine;
namespace RetyKotlety {
// Reaches the damage boundary quickly, then holds it long enough to read from first person.
public class SlamWave : MonoBehaviour {
 LineRenderer line;float age,width;const float Lifetime=.65f,Expansion=.12f;
 public void Configure(Material material,Vector3 normal,Vector3 tangent,Vector3 bitangent,SlamImpact impact){
  line=gameObject.AddComponent<LineRenderer>();line.sharedMaterial=material;
  line.useWorldSpace=false;line.loop=true;line.positionCount=96;line.alignment=LineAlignment.View;
  line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;
  width=Mathf.Lerp(.07f,.12f,impact.Power);line.startWidth=line.endWidth=width;
  for(int i=0;i<line.positionCount;i++){float angle=i*Mathf.PI*2/line.positionCount;line.SetPosition(i,normal*.08f+(tangent*Mathf.Cos(angle)+bitangent*Mathf.Sin(angle))*impact.Radius);}
  transform.localScale=Vector3.one*.15f;UpdateColor(1);
 }
 void UpdateColor(float alpha){var color=new Color(1,.83f,.34f,alpha);line.startColor=line.endColor=color;}
 void Update(){if(!line)return;age+=Time.deltaTime;float t=Mathf.Clamp01(age/Expansion);transform.localScale=Vector3.one*Mathf.Lerp(.15f,1,1-Mathf.Pow(1-t,3));UpdateColor(1-Mathf.InverseLerp(.4f,Lifetime,age));line.startWidth=line.endWidth=width*Mathf.Lerp(1,.65f,Mathf.Clamp01(age/Lifetime));if(age>=Lifetime)Destroy(gameObject);}
}
}
