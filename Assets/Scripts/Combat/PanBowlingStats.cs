using UnityEngine;
namespace RetyKotlety {
public readonly struct PanBowlingStats {
 public readonly float Damage,Speed;
 public PanBowlingStats(int stacks){Damage=stacks>0?60+30*(stacks-1):0;Speed=stacks>0?16+3*(stacks-1):0;}
}
}
