using System;
using RetyKotlety;
public static class ProgressionChecks {
 static void Check(bool ok,string label){if(!ok)throw new Exception(label);Console.WriteLine("PASS "+label);}
 public static void Run(){
  var state=new RunProgression();Check(state.Level==1&&state.Experience==0&&state.Points==0&&!state.ShopUnlocked,"Fresh run starts level one with no shop or money");
  Check(state.RecordKill(false)==0&&state.Experience==10&&state.Kills==1,"Melee kill grants ten XP without granting currency directly");
  state.RecordKill(false);Check(state.RecordKill(false)==1&&state.Level==2&&state.Experience==0&&state.ExperienceRequired==50&&state.ShopUnlocked,"Third melee kill unlocks shop and increases next XP threshold");
  Check(state.Points==0,"Level up does not create currency from uncollected drops");
  Check(state.AddExperience(130)==2&&state.Level==4&&state.Experience==10&&state.ExperienceRequired==90,"Large XP rewards carry overflow through multiple levels");
  int xp=state.Experience;state.AddExperience(0);state.AddExperience(-100);Check(state.Experience==xp,"Invalid XP rewards cannot lower progress");
  var ranged=new RunProgression();ranged.RecordKill(true);Check(ranged.Experience==15,"Ranged kill grants fifteen XP");
  state.AddPoints(25);Check(!state.TrySpend(26)&&state.Points==25,"Insufficient funds cannot create debt");
  Check(!state.TrySpend(0)&&!state.TrySpend(-1)&&state.Points==25,"Invalid purchase amounts leave wallet unchanged");
  Check(state.TrySpend(10)&&state.Points==15,"Successful purchase subtracts its price exactly once");
  state.AddPoints(-10);Check(state.Points==15,"Negative pickups do not remove currency");
  Check(RunProgression.Price(10,0)==10&&RunProgression.Price(10,1)==15&&RunProgression.Price(10,3)==25,"Each owned stack raises price by five points");
  Check(RunProgression.Price(30,2)==60,"Special item base price is respected");
  state.AddExperience(90);Check(state.Points==15,"Unspent points survive the next level");
  var fresh=new RunProgression();Check(fresh.Points==0&&fresh.Level==1&&fresh.Kills==0,"Restarted run has independent progression and wallet");
  state.AddPoints(int.MaxValue);Check(state.Points==int.MaxValue,"Large currency rewards cannot overflow the wallet");
  Check(RunProgression.Price(int.MaxValue,int.MaxValue)==int.MaxValue,"Extreme stack prices cannot wrap negative");
 }
}
