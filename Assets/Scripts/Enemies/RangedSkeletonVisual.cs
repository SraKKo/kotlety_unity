using UnityEngine;
using UnityEngine.AI;
namespace RetyKotlety {
public class RangedSkeletonVisual : MonoBehaviour {
 Transform model;Vector3 rest;float recoil;NavMeshAgent agent;
 public static void Install(Enemy enemy){
  if(!enemy||!enemy.shooting)return;
  var component=enemy.GetComponent<RangedSkeletonVisual>();if(!component)component=enemy.gameObject.AddComponent<RangedSkeletonVisual>();
  var existing=enemy.transform.Find("Skeleton - ranged");
  if(existing){component.Bind(existing);return;}
  var source=Resources.Load<GameObject>("Enemies/Skeleton/Models/Skeleton");if(!source)return;
  var wrapper=new GameObject("Skeleton - ranged");wrapper.transform.SetParent(enemy.transform,false);
  var instance=Instantiate(source,wrapper.transform);instance.transform.localPosition=Vector3.zero;
  var material=Resources.Load<Material>("Enemies/Skeleton/Materials/Skeleton");
  var renderers=instance.GetComponentsInChildren<Renderer>();if(renderers.Length==0){Destroy(wrapper);return;}
  var bounds=renderers[0].bounds;foreach(var renderer in renderers){bounds.Encapsulate(renderer.bounds);if(material)renderer.sharedMaterial=material;}
  var capsule=enemy.GetComponent<CapsuleCollider>();float targetHeight=capsule?capsule.height:2.4f;float bottom=capsule?capsule.center.y-capsule.height*.5f:0;
  wrapper.transform.localScale=Vector3.one*(targetHeight/Mathf.Max(.01f,bounds.size.y/Mathf.Abs(enemy.transform.lossyScale.y)));
  bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
  var centre=enemy.transform.InverseTransformPoint(bounds.center);float minY=enemy.transform.InverseTransformPoint(new Vector3(bounds.center.x,bounds.min.y,bounds.center.z)).y;
  wrapper.transform.localPosition=new Vector3(-centre.x,bottom-minY,-centre.z);
  foreach(var collider in instance.GetComponentsInChildren<Collider>()){collider.enabled=false;if(Application.isPlaying)Destroy(collider);else DestroyImmediate(collider);}
  SetLayer(wrapper,9);
  // Already serialized maps can retain the old imported model; swap their presentation on startup.
  foreach(Transform child in enemy.transform)if(child.name.StartsWith("ShootingEnemyRig"))child.gameObject.SetActive(false);
  component.Bind(wrapper.transform);
 }
 static void SetLayer(GameObject root,int layer){root.layer=layer;foreach(Transform child in root.transform)SetLayer(child.gameObject,layer);}
 void Bind(Transform target){model=target;rest=target.localPosition;agent=GetComponent<NavMeshAgent>();}
 public void Fire(){recoil=1;}
 void LateUpdate(){
  if(!model||!Application.isPlaying||GameSession.Instance&&GameSession.Instance.paused)return;
  recoil=Mathf.MoveTowards(recoil,0,Time.deltaTime*6);
  bool moving=agent&&agent.enabled&&agent.isOnNavMesh&&agent.velocity.sqrMagnitude>.1f;
  model.localPosition=rest+(moving?Vector3.up*(Mathf.Abs(Mathf.Sin(Time.time*9))*.025f):Vector3.zero)-transform.InverseTransformDirection(transform.forward)*recoil*.07f;
  model.localRotation=Quaternion.Euler(recoil*-7,0,moving?Mathf.Sin(Time.time*9)*2:0);
 }
}
}
