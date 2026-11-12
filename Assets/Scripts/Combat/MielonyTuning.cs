using UnityEngine;
namespace RetyKotlety {
[CreateAssetMenu(menuName="Rety Kotlety/Mielony tuning",fileName="Mielony")]
public class MielonyTuning : ScriptableObject {
 [Min(.1f)] public float launchSpeed=18,upwardBoost=5,gravity=22,fireCooldown=.85f;
 [Min(.1f)] public float greaseRadius=2.5f,greaseDuration=5,tickInterval=.25f;
 [Min(0)] public float tickDamage=5;
 [Range(.1f,1)] public float slowMultiplier=.8f,bounceRetention=.65f;
 [Min(.1f)] public float minimumBounceSpeed=6;
}
}
