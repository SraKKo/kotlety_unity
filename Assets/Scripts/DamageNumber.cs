using System.Collections.Generic;
using UnityEngine;
namespace RetyKotlety {
public class DamageNumber : MonoBehaviour {
 static readonly Dictionary<int,DamageNumber> active=new Dictionary<int,DamageNumber>();
 public float total;public int targetId;float age;Vector3 origin;TextMesh[] labels;Camera view;
 public static DamageNumber Show(int target,float amount,Vector3 point){
  if(active.TryGetValue(target,out var existing)&&existing&&existing.age<.12f){existing.total+=amount;existing.age=0;existing.UpdateText();return existing;}
  var g=new GameObject("Obrażenia");var number=g.AddComponent<DamageNumber>();number.targetId=target;number.total=amount;number.origin=point;number.view=GameSession.Instance&&GameSession.Instance.player?GameSession.Instance.player.view:Camera.main;
  var font=Resources.Load<Font>("UI/PixelAE-Bold");font.RequestCharactersInTexture("0123456789",64,FontStyle.Normal);
  number.labels=new TextMesh[5];var offsets=new[]{new Vector3(-.025f,-.025f,.003f),new Vector3(.025f,-.025f,.003f),new Vector3(-.025f,.025f,.003f),new Vector3(.025f,.025f,.003f),Vector3.zero};
  for(int i=0;i<5;i++){var child=new GameObject(i==4?"Liczba":"Obrys");child.transform.SetParent(g.transform,false);child.transform.localPosition=offsets[i];var text=child.AddComponent<TextMesh>();text.font=font;text.fontSize=64;text.characterSize=10f/text.fontSize;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=i==4?new Color(.8f,.025f,.01f):Color.black;child.GetComponent<MeshRenderer>().sharedMaterial=font.material;number.labels[i]=text;}
  number.UpdateText();active[target]=number;number.UpdatePose();return number;
 }
 void UpdateText(){if(labels==null)return;foreach(var text in labels)text.text=Mathf.RoundToInt(total).ToString();}
 void UpdatePose(){if(!view)return;transform.rotation=view.transform.rotation;transform.position=origin+Vector3.up*(age*.85f)+view.transform.right*((targetId%3-1)*.07f);float scale=Mathf.Clamp(Vector3.Distance(view.transform.position,origin)*.045f,.35f,1.2f)*(1+.18f*Mathf.Sin(Mathf.Clamp01(age/.12f)*Mathf.PI));transform.localScale=Vector3.one*scale;}
 void LateUpdate(){age+=Time.deltaTime;UpdatePose();float alpha=1-Mathf.InverseLerp(.4f,.85f,age);for(int i=0;i<labels.Length;i++){var color=labels[i].color;color.a=alpha;labels[i].color=color;}if(age>=.85f)Destroy(gameObject);}
 void OnDestroy(){if(active.TryGetValue(targetId,out var instance)&&instance==this)active.Remove(targetId);}
}
}

