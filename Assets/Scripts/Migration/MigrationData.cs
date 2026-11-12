using System;
using UnityEngine;
namespace RetyKotlety {
[Serializable] public class PoseData { public Vector3 position; public Quaternion rotation; public Vector3 scale=Vector3.one;
 public Vector3 Position => new Vector3(position.x,position.y,-position.z);
 public Quaternion Rotation => new Quaternion(-rotation.x,-rotation.y,rotation.z,rotation.w);
 public void ApplyLocal(Transform t){t.localPosition=Position;t.localRotation=Rotation;t.localScale=scale;}
 public void Apply(Transform t) { t.position=Position; t.rotation=Rotation; t.localScale=scale; }
}
[Serializable] public class ActorData { public string kind; public PoseData pose; public string model; public ColliderData[] colliders; public Vector3 angularVelocity; }
[Serializable] public class ColliderData { public string kind; public PoseData pose; public Vector3 size; public float radius,height; public Vector3[] points; public bool preserveFaceOrder; }
[Serializable] public class SpawnerData { public PoseData pose; public Vector3[] points; public int[] waves; }
[Serializable] public class LevelData { public PoseData player; public ActorData[] actors; public ColliderData[] colliders; public SpawnerData[] spawners; }
[Serializable] public class ModelBounds {public Vector3 center,size;}
[Serializable] public class PresentationBounds {public ModelBounds SausageLink;}
[Serializable] public class PresentationData {public float bodyHeight,bodyRadius,worldFov,weaponFov;public PoseData camera,rightHand,leftHand;public PresentationBounds bounds;}
public enum FoodKind { Pan, Schabowy, Chrupiaca, Pierog, Jetpack }
}



