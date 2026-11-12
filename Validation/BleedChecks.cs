using System;
using RetyKotlety;
public static class BleedChecks {
 static void Check(bool value,string label){if(!value)throw new Exception(label);Console.WriteLine("PASS "+label);}
 public static void Main(){
  var bleed=new BleedState();bleed.Apply(5,.25f,3);int ticks=0;for(int i=0;i<240;i++)ticks+=bleed.Advance(1f/60);
  Check(ticks==12&&ticks*bleed.TickDamage==60&&!bleed.Active,"Bleed deals twelve ticks and sixty damage at 60 FPS");
  bleed.Apply(5,.25f,3);Check(bleed.Advance(4)==12&&!bleed.Active,"Long frames catch up without extra ticks past expiration");
  bleed.Apply(5,.25f,3);bleed.Apply(5,.25f,3);bleed.Apply(5,.25f,3);Check(bleed.Advance(3)==12,"Three simultaneous contacts refresh one bleed instead of stacking");
  bleed.Apply(5,.25f,3);ticks=0;for(int i=0;i<10;i++){bleed.Apply(5,.25f,3);ticks+=bleed.Advance(.1f);}Check(ticks==4,"Frequent hits do not postpone the next bleed tick");
  Check(bleed.Advance(0)==0&&bleed.Active,"Pausing does not advance or expire bleed");
  bleed=new BleedState();bleed.Apply(5,0,3);Check(!bleed.Active,"Invalid intervals do not activate bleed");
 }
}
