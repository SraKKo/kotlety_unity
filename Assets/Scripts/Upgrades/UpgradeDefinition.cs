using UnityEngine;
namespace RetyKotlety {
public enum UpgradeEffect { SchabowyRicochet, ChrupiacaPiercing, ChrupiacaBleed, MielonyBounce }
[CreateAssetMenu(menuName="Rety Kotlety/Upgrade",fileName="Upgrade")]
public class UpgradeDefinition : ScriptableObject {
 public string id="1.1",title="SCHABOWY: RYKOSZET";
 [TextArea] public string description="Po trafieniu schabowy automatycznie leci do trzech kolejnych, roznych wrogow.";
 public UpgradeEffect effect;
 [Min(1)] public int extraTargets=3;
 [Min(1)] public float targetRange=14;
 [Min(0)] public float bleedDamage=5;
 [Min(.01f)] public float bleedInterval=.25f;
 [Min(.01f)] public float bleedDuration=3;
 [Min(1)] public int groundBounces=2;
 public FoodKind Weapon=>effect==UpgradeEffect.SchabowyRicochet?FoodKind.Schabowy:effect==UpgradeEffect.MielonyBounce?FoodKind.Mielony:FoodKind.Chrupiaca;
 public string CardName=>effect==UpgradeEffect.SchabowyRicochet?"RYKOSZET":effect==UpgradeEffect.ChrupiacaPiercing?"PIERCING":effect==UpgradeEffect.MielonyBounce?"BOUNCE":"KRWAWIENIE";
 public Color color=new Color(.65f,.35f,1);
}
}
