using UnityEngine;
namespace RetyKotlety {
[CreateAssetMenu(menuName="Rety Kotlety/Losos tuning",fileName="Losos")]
public class LososTuning : ScriptableObject {
 // Outbound leg: straight along the crosshair, slowing down towards the end of the range.
 [Min(1)] public float launchSpeed=34,range=22,returnSpeed=40;
 // Damage is dealt once per enemy on each leg of the flight.
 [Min(0)] public float damage=40;
 // Sideways kick at the turning point; larger values give a wider return arc.
 [Min(0)] public float curve=9,spinSpeed=1080;
 // Orbit upgrade: distance from the enemy's centre and seconds per lap.
 [Min(.1f)] public float orbitRadius=1.6f,orbitLapTime=.3f;
 [Min(.1f)] public float catchDistance=1.3f,maxFlightTime=6,modelLength=.9f;
}
}
