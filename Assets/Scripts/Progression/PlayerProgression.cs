using UnityEngine;
namespace RetyKotlety {
[RequireComponent(typeof(PlayerUpgrades))]
public class PlayerProgression : MonoBehaviour {
 public RunProgression State {get;}=new RunProgression();
 public bool LevelNoticePending {get;private set;}
 public void RewardKill(Vector3 position,bool ranged){
  RewardKill(position,ranged?15:10,ranged?8:5);
 }
 public void RewardKill(Vector3 position,int xp,int points){
  PointPickup.Spawn(position,points);
  if(State.RecordKill(xp)>0)LevelNoticePending=true;
 }
 public void AcknowledgeLevel(){LevelNoticePending=false;}
 public int Price(UpgradeDefinition definition)=>RunProgression.Price(definition.shopBaseCost,GetComponent<PlayerUpgrades>().Count(definition));
 public bool TryBuy(UpgradeDefinition definition){
  var session=GameSession.Instance;
  if(!session||session.player==null||session.player.gameObject!=gameObject||!session.shopOpen||!State.ShopUnlocked||!definition||!definition.IsShopUpgrade)return false;
  var upgrades=GetComponent<PlayerUpgrades>();if(!upgrades.CanAcquire(definition))return false;
  int price=Price(definition);if(State.Points<price||!upgrades.Grant(definition))return false;
  return State.TrySpend(price);
 }
}
}
