using UnityEngine;
namespace RetyKotlety {
public class Pickup : MonoBehaviour { public FoodKind kind; public Transform visual; Vector3 origin;void Start(){if(visual)origin=visual.localPosition;}void Update(){if(visual){visual.Rotate(0,60*Time.deltaTime,0);visual.localPosition=origin+Vector3.up*Mathf.Sin(Time.time*2)*.1f;}} }
}
