using System;
using RetyKotlety;
public static class IncrementalBonusChecks {
 static void Near(float actual,float expected,string label){if(Math.Abs(actual-expected)>.001f)throw new Exception(label+": "+actual+" != "+expected);Console.WriteLine("PASS "+label);}
 public static void Main(){
  var plain=new CombatBonuses();Near(plain.Damage(50),50,"Empty build preserves direct damage");Near(plain.Damage(5,false,true),5,"Empty build preserves DoT");
  var bread=new CombatBonuses(.08f*5,0,0,0,0,0);Near(bread.Damage(100),140,"Five breading stacks add forty percent rather than compounding");
  var dot=new CombatBonuses(.16f,.3f,3,1,1,.2f);Near(dot.Damage(5,false,true,100,1000),9.28f,"Flat DoT bonus is applied before damage multiplier exactly once; flight bonuses do not affect DoT");Near(dot.AreaMultiplier,1.3f,"Area stacks add to base radius");Near(dot.Damage(5,true,true),11.136f,"Airborne DoT uses the target's current state");
  var progress=new AttackDamage<int>(new CombatBonuses(0,0,0,.04f,0,0));Near(progress.Hit(50,1,false),50,"First unique target receives base damage");Near(progress.Hit(50,2,false),52,"Second unique target gains one Daily Special increment");Near(progress.Hit(50,3,false),54,"Third unique target gains two increments");Near(progress.Hit(50,1,false),54,"Return or orbit hits do not create new unique-target stacks");
  var travel=new AttackDamage<int>(new CombatBonuses(0,0,0,0,.03f,0));travel.AddDistance(4.99f);Near(travel.Hit(100,1,false),100,"Incomplete five-metre segment grants no travel bonus");travel.AddDistance(.01f);Near(travel.Hit(100,1,false),103,"Five metres grants one travel increment");travel.AddDistance(15);Near(travel.Hit(100,1,false),112,"Outbound, orbit and return distances accumulate");travel.SetDistance(1);Near(travel.Distance,20,"Flight distance cannot move backwards");
  var capped=new CombatBonuses(0,0,0,2,2,0);Near(capped.Damage(10,false,false,100,1000),30,"Distinct-target and travel bonuses each cap at one hundred percent");
  var air=new CombatBonuses(0,0,0,0,0,.1f*3);Near(air.Damage(100,false),100,"Chef Launch does not affect grounded targets");Near(air.Damage(100,true),130,"Three Chef Launch stacks add thirty percent against launched targets");
  Near(plain.Damage(100),100,"Another build remains unaffected");
  var noBat=new PanBowlingStats(0);Near(noBat.Damage,0,"No pan upgrade means no collision damage");Near(noBat.Speed,0,"No pan upgrade means no launch");
  var bat=new PanBowlingStats(1);Near(bat.Damage,60,"First pan stack gives sixty collision damage");Near(bat.Speed,16,"First pan stack gives sixteen metres per second");
  var stackedBat=new PanBowlingStats(3);Near(stackedBat.Damage,120,"Three pan stacks give one hundred twenty collision damage");Near(stackedBat.Speed,22,"Pan launch speed increases by three per extra stack");Near(bread.Damage(bat.Damage),84,"Collision damage uses the shared player damage bonus once");
  ProjectileFanChecks.Run();
  ProgressionChecks.Run();
 }
}
