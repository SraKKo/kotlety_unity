using System;
using RetyKotlety;
public static class SlamChecks {
 static void Check(bool value,string label){if(!value)throw new Exception(label);Console.WriteLine("PASS "+label);}
 public static void Main(){
  var stats=SlamStats.Default;
  Check(stats.Resolve(2.99f).LaunchSpeed==0,"Falls below three metres do not launch enemies");
  Check(stats.Resolve(3).LaunchSpeed==6,"Launch starts at three metres with six metres per second");
  Check(stats.Resolve(5).LaunchSpeed>6&&stats.Resolve(5).LaunchSpeed<9,"Higher falls produce stronger launch");
  Check(stats.Resolve(7.5f).LaunchSpeed==9&&stats.Resolve(100).LaunchSpeed==9,"Launch reaches nine metres per second and remains capped");
  Check(stats.Resolve(100).Radius==5&&stats.Resolve(100).Damage==90,"Existing radius and damage caps are preserved");
  Check(stats.Resolve(1).ShakeAmplitude>0&&stats.Resolve(100).ShakeAmplitude<=.035f&&stats.Resolve(3).ShakeDuration==.18f,"Shake remains short and bounded");
  var custom=new SlamStats(1,1,5,12,90,5,4,8,10,0,0);
  Check(custom.Resolve(4).LaunchSpeed==0&&custom.Resolve(5).LaunchSpeed==4&&custom.Resolve(10).LaunchSpeed==8&&custom.Resolve(10).ShakeAmplitude==0,"Custom profiles independently configure launch threshold and disable shake");
 }
}
