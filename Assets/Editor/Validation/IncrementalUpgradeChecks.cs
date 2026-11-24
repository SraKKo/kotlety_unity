using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace RetyKotlety.Editor {
public static class IncrementalUpgradeChecks {
 public static void Run(){
  var scene=EditorSceneManager.NewPreviewScene();var results=new List<string>();
  Action<bool,string> check=(ok,label)=>{if(!ok)throw new Exception(label);results.Add("PASS "+label);};
  try{
   var root=new GameObject("Incremental upgrade checks");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,scene);var upgrades=root.AddComponent<PlayerUpgrades>();
   var bread=Resources.Load<UpgradeDefinition>("Upgrades/DoubleBreading");var area=Resources.Load<UpgradeDefinition>("Upgrades/LargePortion");var grease=Resources.Load<UpgradeDefinition>("Upgrades/HotGrease");var daily=Resources.Load<UpgradeDefinition>("Upgrades/DailySpecial");var travel=Resources.Load<UpgradeDefinition>("Upgrades/Takeaway");var chef=Resources.Load<UpgradeDefinition>("Upgrades/ChefLaunch");var ricochet=Resources.Load<UpgradeDefinition>("Upgrades/SchabowyRicochet");
   check(bread&&area&&grease&&daily&&travel&&chef&&ricochet,"All new and existing upgrade definitions load");
   for(int i=0;i<5;i++)check(upgrades.Grant(bread),"Repeatable item grants another stack");
   check(upgrades.Count(bread)==5&&upgrades.TotalStacks==5&&upgrades.Acquired.Count==1,"Repeated items use one build card with the correct stack count");
   check(Mathf.Abs(upgrades.Bonuses.Damage(100)-140)<.001f,"Player damage bonus adds across five stacks");
   var snapshot=upgrades.Bonuses;upgrades.Grant(bread);check(Mathf.Abs(snapshot.Damage(100)-140)<.001f,"Existing attack snapshots do not mutate after acquiring another stack");
   foreach(var definition in new[]{area,grease,daily,travel,chef}){upgrades.Grant(definition);upgrades.Grant(definition);}
   check(upgrades.Acquired.Count==6&&upgrades.TotalStacks==16,"Six distinct items keep independent counts");
   check(Mathf.Abs(upgrades.Bonuses.AreaMultiplier-1.2f)<.001f&&upgrades.Bonuses.DotBonus==2,"Area and DoT increment independently");
   check(upgrades.Grant(ricochet)&&!upgrades.Grant(ricochet)&&upgrades.SchabowyExtraTargets==3,"Weapon unlocks remain single acquisition");
   check(bread.increment==.08f&&area.increment==.1f&&grease.increment==1,"Shared asset values are never modified");
   var extra=Resources.Load<UpgradeDefinition>("Upgrades/ExtraProjectiles");check(extra&&upgrades.Grant(extra)&&upgrades.Grant(extra)&&upgrades.ExtraProjectiles==2,"Extra projectile item adds one projectile for every acquired stack");
   check(upgrades.Count(extra)==2&&upgrades.CanAcquire(extra),"Extra projectile item remains available after repeated grants");
   var slide=Resources.Load<UpgradeDefinition>("Upgrades/GreasySlide");var dash=Resources.Load<UpgradeDefinition>("Upgrades/DashHaste");
   check(slide&&dash&&slide.IsGlobal&&dash.IsGlobal,"Movement upgrades load without granting a weapon");
   check(upgrades.Grant(slide)&&upgrades.GreasySlide&&!upgrades.Grant(slide),"Greasy slide unlocks once");
   check(upgrades.AttackSpeedMultiplier==1,"Attack speed stays unchanged before a dash");
   check(upgrades.Grant(dash)&&!upgrades.Grant(dash),"Dash haste unlocks once");upgrades.OnDash();
   check(Mathf.Abs(upgrades.AttackSpeedMultiplier-1.35f)<.001f,"Dash activates thirty-five percent attack speed");
   var expiry=typeof(PlayerUpgrades).GetField("dashHasteUntil",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
   expiry.SetValue(upgrades,Time.time-1);check(upgrades.AttackSpeedMultiplier==1,"Dash haste expires without leaving a permanent bonus");
   upgrades.OnDash();check(Mathf.Abs(upgrades.AttackSpeedMultiplier-1.35f)<.001f,"Another dash refreshes the bonus");
   var bowling=Resources.Load<UpgradeDefinition>("Upgrades/PanBowling");
   check(bowling&&bowling.id=="0.1"&&!bowling.IsGlobal&&bowling.Weapon==FoodKind.Pan,"Bowling definition belongs to the pan");
   check(upgrades.Grant(bowling)&&upgrades.Grant(bowling)&&upgrades.PanBowlingStacks==2&&upgrades.Count(bowling)==2,"Pan upgrade stacks and remains in the reward pool");
   var second=new GameObject("Second player");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(second,scene);var clean=second.AddComponent<PlayerUpgrades>();check(clean.Acquired.Count==0&&clean.TotalStacks==0&&clean.Bonuses.Damage(100)==100,"New runs and other players have separate upgrade state");
  }finally{EditorSceneManager.ClosePreviewScene(scene);}
  Directory.CreateDirectory("Validation/Reports");File.WriteAllLines("Validation/Reports/incremental-upgrades-results.txt",results);Debug.Log("Incremental upgrade checks passed: "+results.Count);
 }
}
}
