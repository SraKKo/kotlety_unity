using UnityEngine;
namespace RetyKotlety {
public class TestRotator : MonoBehaviour {
 public Vector3 radiansPerSecond;Quaternion origin;float elapsed;
 void Awake(){origin=transform.localRotation;}
 void FixedUpdate(){elapsed+=Time.fixedDeltaTime;transform.localRotation=origin*Quaternion.Euler(radiansPerSecond*Mathf.Rad2Deg*elapsed);}
}
}
