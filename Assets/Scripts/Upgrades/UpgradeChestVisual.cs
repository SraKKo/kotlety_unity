using UnityEngine;
namespace RetyKotlety {
[ExecuteAlways,RequireComponent(typeof(UpgradeChest))]
public class UpgradeChestVisual : MonoBehaviour {
 Transform closedModel,openModel;
 void OnEnable(){EnsureVisual();}
 public void EnsureVisual(){
  if(closedModel&&openModel){RefreshState();return;}
  ClearVisual();
  closedModel=Create("Closed",0);openModel=Create("Open",90);
  var placeholder=GetComponent<MeshRenderer>();if(placeholder)placeholder.enabled=!(closedModel&&openModel);
  RefreshState();
 }
 Transform Create(string state,float yaw){
  var source=Resources.Load<GameObject>("Props/UpgradeChest/"+state+"/Models/UpgradeChest"+state);if(!source)return null;
  var container=new GameObject("Skrzynka - "+state);container.transform.SetParent(transform,false);
  container.hideFlags=HideFlags.DontSaveInEditor|HideFlags.DontSaveInBuild;
  var model=Instantiate(source,container.transform);model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.identity;model.transform.localScale=Vector3.one;
  // Both exports contain different scene offsets. Align around the body's local frame first.
  foreach(var mesh in model.GetComponentsInChildren<MeshFilter>())if(mesh.name=="cube"){
   var inverse=Quaternion.Inverse(mesh.transform.localRotation);
   model.transform.localRotation=inverse;model.transform.localPosition=-(inverse*mesh.transform.localPosition);break;
  }
  container.transform.localRotation=Quaternion.Euler(0,yaw,0);
  foreach(var collider in model.GetComponentsInChildren<Collider>(true)){if(Application.isPlaying)Destroy(collider);else DestroyImmediate(collider);}
  var material=Resources.Load<Material>("Props/UpgradeChest/Materials/"+state);
  foreach(var renderer in model.GetComponentsInChildren<MeshRenderer>())if(material)renderer.sharedMaterial=material;
  // The lid changes total bounds. Size and centre by the bottom footprint so the body never jumps.
  var bounds=BoundsInChest(container.transform,false);var footprint=BoundsInChest(container.transform,true);
  float width=Mathf.Max(footprint.size.x,footprint.size.z);
  if(width<.0001f){Release(container.transform);return null;}
  container.transform.localScale=Vector3.one/width;
  footprint=BoundsInChest(container.transform,true);
  container.transform.localPosition=new Vector3(-footprint.center.x,-.5f-footprint.min.y,-footprint.center.z);
  if(state=="Closed"){
   bounds=BoundsInChest(container.transform,false);var box=GetComponent<BoxCollider>();if(box){box.center=bounds.center;box.size=bounds.size;}
  }
  if(state=="Open")AddInterior(model);
  return container.transform;
 }
 void AddInterior(GameObject model){
  // Darken only the bottom; keep the authored rim, walls and lid materials.
  var floor=Resources.Load<Material>("Props/UpgradeChest/Materials/InteriorFloor");if(!floor)return;
  foreach(var mesh in model.GetComponentsInChildren<MeshFilter>())if(mesh.name=="plane"){
   var renderer=mesh.GetComponent<MeshRenderer>();if(renderer)renderer.sharedMaterial=floor;
  }
 }
 Bounds BoundsInChest(Transform root,bool footprint){
  var bounds=new Bounds();bool initialized=false;float bottom=float.PositiveInfinity;
  var meshes=root.GetComponentsInChildren<MeshFilter>();
  if(footprint)foreach(var mesh in meshes)if(mesh.sharedMesh&&mesh.sharedMesh.isReadable)foreach(var vertex in mesh.sharedMesh.vertices)bottom=Mathf.Min(bottom,transform.InverseTransformPoint(mesh.transform.TransformPoint(vertex)).y);
  foreach(var mesh in meshes){
   if(!mesh.sharedMesh)continue;
   if(footprint&&mesh.sharedMesh.isReadable){
    foreach(var vertex in mesh.sharedMesh.vertices){var point=transform.InverseTransformPoint(mesh.transform.TransformPoint(vertex));if(point.y>bottom+.001f)continue;if(!initialized){bounds=new Bounds(point,Vector3.zero);initialized=true;}else bounds.Encapsulate(point);}
   }else{
    var b=mesh.sharedMesh.bounds;
    for(int i=0;i<8;i++){var corner=b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));var point=transform.InverseTransformPoint(mesh.transform.TransformPoint(corner));if(!initialized){bounds=new Bounds(point,Vector3.zero);initialized=true;}else bounds.Encapsulate(point);}
   }
  }
  return bounds;
 }
 public void RefreshState(){bool used=GetComponent<UpgradeChest>().opened;if(closedModel)closedModel.gameObject.SetActive(!used);if(openModel)openModel.gameObject.SetActive(used);}
 void Release(Transform root){if(!root)return;root.gameObject.SetActive(false);if(Application.isPlaying)Destroy(root.gameObject);else DestroyImmediate(root.gameObject);}
 void ClearVisual(){Release(closedModel);Release(openModel);closedModel=openModel=null;}
 void OnDisable(){ClearVisual();var placeholder=GetComponent<MeshRenderer>();if(placeholder)placeholder.enabled=true;}
}
}
