using System;
using System.Collections.Generic;
using UnityEngine;
namespace RetyKotlety {
public static class ArenaLayout {
 public const string SceneName="MovementArena";
 [Serializable] public class Item {public string group,name,text;public Vector3 position,size,rotation;public Color color;public bool collider,upgradeChest;public float textSize;}
 [Serializable] public class Layout {public Item[] items;}
 static readonly Dictionary<Color,Material> materials=new Dictionary<Color,Material>();
 public static readonly Vector3[] SpawnPoints={new Vector3(-42,0,-40),new Vector3(42,0,-40),new Vector3(-42,0,40),new Vector3(42,0,40),new Vector3(-42,4,0),new Vector3(42,4,0),new Vector3(0,8,34),new Vector3(0,8,-34)};
 static Material Material(Color color){if(!materials.TryGetValue(color,out var material)||!material){material=new Material(Shader.Find("Standard")){name="Arena "+ColorUtility.ToHtmlStringRGB(color),color=color};material.SetFloat("_Glossiness",.12f);materials[color]=material;}return material;}
 public static Transform Build(Transform parent){
  var arena=new GameObject("Arena - geometria i trasy");arena.transform.SetParent(parent,false);
  var layout=JsonUtility.FromJson<Layout>(Resources.Load<TextAsset>("Levels/MovementArena").text);
  var groups=new Dictionary<string,Transform>();
  foreach(var item in layout.items){
   if(!groups.TryGetValue(item.group,out var group)){group=new GameObject(item.group).transform;group.SetParent(arena.transform,false);groups[item.group]=group;}
   bool label=!string.IsNullOrEmpty(item.text);
   var g=label?new GameObject(item.name):GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=item.name;
   g.transform.SetParent(group,false);g.transform.localPosition=item.position;g.transform.localRotation=Quaternion.Euler(item.rotation);g.transform.localScale=item.size;
   if(label){var text=g.AddComponent<TextMesh>();text.text=item.text;text.fontSize=48;text.characterSize=item.textSize;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=item.color;g.AddComponent<ArenaSign>();}
   else{g.GetComponent<Renderer>().sharedMaterial=Material(item.color);if(item.upgradeChest){g.AddComponent<UpgradeChest>();g.AddComponent<UpgradeChestVisual>();}if(!item.collider)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());}
  }
  return arena.transform;
 }
}
}
