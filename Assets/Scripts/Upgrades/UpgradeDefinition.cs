using UnityEngine;
namespace RetyKotlety {
public enum UpgradeEffect { SchabowyRicochet, ChrupiacaPiercing, ChrupiacaBleed, MielonyBounce, LososOrbit, DoubleBreading, LargePortion, HotGrease, DailySpecial, Takeaway, ChefLaunch, ExtraProjectiles, GreasySlide, DashHaste, PanBowling, MaxHealth, ShotSpeed, AttackRate, Regeneration }
[CreateAssetMenu(menuName="Rety Kotlety/Upgrade",fileName="Upgrade")]
public class UpgradeDefinition : ScriptableObject {
 public string id="1.1",title="SCHABOWY: RYKOSZET";
 [TextArea] public string description="Po trafieniu schabowy automatycznie leci do trzech kolejnych, roznych wrogow.";
 public UpgradeEffect effect;
 public Texture2D icon;
 [Min(1)] public int extraTargets=3;
 [Min(1)] public float targetRange=14;
 [Min(0)] public float bleedDamage=5;
 [Min(.01f)] public float bleedInterval=.25f;
 [Min(.01f)] public float bleedDuration=3;
 [Min(1)] public int groundBounces=2;
 [Min(1)] public int orbitLaps=4;
 public bool stackable;
 [Min(0)] public int maxStacks;
 [Min(0)] public float increment;
 public string shortName;
 [Min(1)] public int shopBaseCost=10;
 public bool IsShopUpgrade=>IsGlobal;
 public bool IsWeaponUpgrade=>!IsGlobal;
 public bool IsGlobal=>effect>=UpgradeEffect.DoubleBreading&&effect!=UpgradeEffect.PanBowling;
 public FoodKind Weapon=>effect==UpgradeEffect.PanBowling?FoodKind.Pan:effect==UpgradeEffect.LososOrbit?FoodKind.Losos:effect==UpgradeEffect.SchabowyRicochet?FoodKind.Schabowy:effect==UpgradeEffect.MielonyBounce?FoodKind.Mielony:FoodKind.Chrupiaca;
 public string CardName=>effect==UpgradeEffect.PanBowling?"OD DECHY DO DECHY":IsGlobal?(string.IsNullOrEmpty(shortName)?title:shortName):effect==UpgradeEffect.LososOrbit?"OKRAZENIE":effect==UpgradeEffect.SchabowyRicochet?"RYKOSZET":effect==UpgradeEffect.ChrupiacaPiercing?"PIERCING":effect==UpgradeEffect.MielonyBounce?"BOUNCE":"KRWAWIENIE";
 public int ShopUnlockLevel {
  get {
   switch(effect) {
    case UpgradeEffect.DoubleBreading:
    case UpgradeEffect.MaxHealth:
    case UpgradeEffect.AttackRate:
    case UpgradeEffect.Regeneration: return 2;
    case UpgradeEffect.ShotSpeed:
    case UpgradeEffect.LargePortion: return 3;
    case UpgradeEffect.ExtraProjectiles:
    case UpgradeEffect.GreasySlide:
    case UpgradeEffect.DashHaste:
    case UpgradeEffect.HotGrease: return 4;
    default: return 5;
   }
  }
 }
 public string ShopBenefit {
  get {
   string amount = FormatShopAmount(1);
   switch(effect) {
    case UpgradeEffect.DoubleBreading: return "Obrażenia " + amount;
    case UpgradeEffect.MaxHealth: return "Maksymalne HP " + amount;
    case UpgradeEffect.AttackRate: return "Szybkość ataku " + amount;
    case UpgradeEffect.Regeneration: return "Regeneracja " + amount;
    case UpgradeEffect.ShotSpeed: return "Szybkość pocisków " + amount;
    case UpgradeEffect.LargePortion: return "Promień efektów " + amount;
    case UpgradeEffect.ExtraProjectiles: return "Dodatkowe pociski " + amount;
    case UpgradeEffect.GreasySlide: return "Ślizg zostawia tłuszcz";
    case UpgradeEffect.DashHaste: return "Dash przyspiesza ataki";
    case UpgradeEffect.HotGrease: return "Krwawienie i tłuszcz " + amount;
    case UpgradeEffect.DailySpecial: return "Bonus za kolejne cele " + amount;
    case UpgradeEffect.Takeaway: return "Bonus za dystans " + amount;
    case UpgradeEffect.ChefLaunch: return "Obrażenia po slamie " + amount;
    default: return title;
   }
  }
 }
 public string ShopDetail {
  get {
   switch(effect) {
    case UpgradeEffect.GreasySlide: return "Plamy ranią i spowalniają wrogów.";
    case UpgradeEffect.DashHaste: return "+35% tempa ataku i lotu łososia przez 3 s.";
    case UpgradeEffect.DailySpecial: return "Za każdego innego wroga trafionego tym pociskiem. Limit +100%.";
    case UpgradeEffect.Takeaway: return "Za każde 5 m lotu pocisku. Limit +100%.";
    case UpgradeEffect.ChefLaunch: return "Przeciw wrogom podrzuconym slamem.";
    case UpgradeEffect.HotGrease: return "Do każdego impulsu obrażeń efektu.";
    case UpgradeEffect.LargePortion: return "Większe plamy tłuszczu i zasięg slamu.";
    case UpgradeEffect.ExtraProjectiles: return "Przy każdym ataku bronią dystansową.";
    default: return "";
   }
  }
 }
 public string ShopPreview(int owned) {
  if(!stackable)return owned > 0 ? "ZDOBYTE" : "Odblokuj nowy efekt";
  return FormatShopAmount(owned) + "  →  " + FormatShopAmount(owned + 1);
 }
 string FormatShopAmount(int stacks) {
  float amount = stacks * increment;
  switch(effect) {
   case UpgradeEffect.MaxHealth: return "+" + amount.ToString("0.#") + " HP";
   case UpgradeEffect.Regeneration: return "+" + amount.ToString("0.#") + " HP/s";
   case UpgradeEffect.HotGrease: return "+" + amount.ToString("0.#") + " obr.";
   case UpgradeEffect.ExtraProjectiles:
    return "+" + (stacks * Mathf.Max(1,Mathf.RoundToInt(increment)));
   default: return "+" + (amount * 100).ToString("0.#") + "%";
  }
 }
 public Color color=new Color(.65f,.35f,1);
}
}
