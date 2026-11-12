using System.Collections.Generic;
using UnityEngine;
namespace RetyKotlety {
// Uses the authored kielba.tscn mesh and transforms; no substitute line renderer.
public class SausageRope : MonoBehaviour {
 public Transform source; public Camera worldCamera,weaponCamera;
 public Vector3 modelCenter,modelSize;
 public float growthSpeed=30;
 readonly List<GameObject> links=new List<GameObject>();GameObject template;Vector3 target;float length;bool visible;
 void Start(){Configure();}
 public void Configure(){if(template)return;var existing=transform.Find("SausageLink");template=existing?existing.gameObject:WorldFactory.Model("SausageLink",transform);template.SetActive(false);}
 public void Show(Vector3 point){target=point;visible=true;}
 public void Hide(){visible=false;length=0;foreach(var link in links)link.SetActive(false);}
 void LateUpdate(){if(!visible||!source)return;
  Vector3 origin=source.position;
  if(worldCamera&&weaponCamera)origin=worldCamera.ViewportToWorldPoint(weaponCamera.WorldToViewportPoint(origin));
  var delta=target-origin;float distance=delta.magnitude;if(distance<.01f){Hide();return;}var direction=delta/distance;
  length=Mathf.Min(distance,length+growthSpeed*Time.deltaTime);
  float stride=Mathf.Max(.1f,modelSize.z);int count=Mathf.Min(256,Mathf.CeilToInt(length/stride));
  while(links.Count<count){var link=Instantiate(template,transform);link.name="Kiełbasa "+links.Count;links.Add(link);}
  var rotation=Quaternion.LookRotation(direction);
  for(int i=0;i<links.Count;i++){var link=links[i];link.SetActive(i<count);if(i>=count)continue;float start=i*stride;float portion=Mathf.Min(stride,length-start);var scale=new Vector3(1,1,portion/stride);var center=Vector3.Scale(modelCenter,scale);link.transform.localScale=scale;link.transform.rotation=rotation;link.transform.position=origin+direction*(start+portion*.5f)-rotation*center;}
 }
}
}

