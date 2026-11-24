using System;
namespace RetyKotlety {
// Pure run state: no persistence across death/restart and no dependency on Unity frames.
public sealed class RunProgression {
 public int Level {get;private set;}=1;
 public int Experience {get;private set;}
 public int Points {get;private set;}
 public int Kills {get;private set;}
 public int ExperienceRequired=>30+20*(Level-1);
 public bool ShopUnlocked=>Level>=2;
 public int AddExperience(int amount){
  if(amount<=0)return 0;Experience=(int)Math.Min(int.MaxValue,(long)Experience+amount);int gained=0;
  while(Experience>=ExperienceRequired){Experience-=ExperienceRequired;Level++;gained++;}
  return gained;
 }
 public int RecordKill(bool ranged)=>RecordKill(ranged?15:10);
 public int RecordKill(int xp){Kills++;return AddExperience(xp);}
 public void AddPoints(int amount){if(amount>0)Points=(int)Math.Min(int.MaxValue,(long)Points+amount);}
 public bool TrySpend(int amount){if(amount<=0||amount>Points)return false;Points-=amount;return true;}
 public static int Price(int baseCost,int owned){return (int)Math.Min(int.MaxValue,Math.Max(1,(long)baseCost)+Math.Max(5L,baseCost/2L)*Math.Max(0,owned));}
}
}
