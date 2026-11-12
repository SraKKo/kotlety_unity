using UnityEngine;
namespace RetyKotlety {
[CreateAssetMenu(menuName="Rety Kotlety/Slam tuning",fileName="Slam")]
public class SlamTuning : ScriptableObject {
 [Header("Promien - metry")]
 [Min(0)] public float baseRadius=1f;
 [Min(0)] public float radiusPerMeter=4f/7.5f;
 [Min(0)] public float maxRadius=5;
 [Header("Obrazenia")]
 [Min(0)] public float damagePerMeter=12;
 [Min(0)] public float maxDamage=90;
 public SlamStats CreateStats(){return new SlamStats(baseRadius,radiusPerMeter,maxRadius,damagePerMeter,maxDamage);}
 void OnValidate(){maxRadius=Mathf.Max(0,maxRadius);baseRadius=Mathf.Clamp(baseRadius,0,maxRadius);radiusPerMeter=Mathf.Max(0,radiusPerMeter);damagePerMeter=Mathf.Max(0,damagePerMeter);maxDamage=Mathf.Max(0,maxDamage);}
}
}
