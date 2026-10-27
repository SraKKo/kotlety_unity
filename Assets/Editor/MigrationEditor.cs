using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace RetyKotlety.Editor {
public class FoodModelImporter : AssetPostprocessor {
 void OnPreprocessAsset(){
  if(!assetPath.StartsWith("Assets/Resources/Converted/")||!assetPath.EndsWith(".glb"))return;
  var serialized=new SerializedObject(assetImporter);var animation=serialized.FindProperty("importSettings.animationMethod");if(animation!=null){animation.intValue=1;serialized.ApplyModifiedPropertiesWithoutUndo();}
 }
}
public static class MigrationEditor {
 [MenuItem("Rety Kotlety/Przygotuj edytowalne sceny")]
 public static void BuildScenes(){
  Directory.CreateDirectory("Assets/Generated");
  BuildScene("Level001",false);BuildScene("MainMenu",true);BuildScene("TestLevel",false);
  EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity",true),new EditorBuildSettingsScene("Assets/Scenes/Level001.unity",true),new EditorBuildSettingsScene("Assets/Scenes/TestLevel.unity",true)};
  PlayerSettings.companyName="Rety Kotlety";PlayerSettings.productName="Rety Kotlety";PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;
  AssetDatabase.SaveAssets();Debug.Log("Migracja: przygotowano sceny Unity.");
 }
 static void BuildScene(string name,bool menu){
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var root=new GameObject("Rety Kotlety");
  var session=WorldFactory.Build(root,menu,name);
  var generated=name=="TestLevel"?"Assets/Generated/TestLevel":"Assets/Generated";Directory.CreateDirectory(generated);
  if(session.navigation){var path=generated+"/Navigation.asset";AssetDatabase.DeleteAsset(path);AssetDatabase.CreateAsset(session.navigation,path);}
  var colliders=root.GetComponentsInChildren<MeshCollider>(true);int count=0;
  foreach(var collider in colliders){if(!collider.sharedMesh||AssetDatabase.Contains(collider.sharedMesh))continue;var path=generated+"/Collision_"+(count++)+".asset";AssetDatabase.DeleteAsset(path);AssetDatabase.CreateAsset(collider.sharedMesh,path);}
  EditorSceneManager.SaveScene(scene,"Assets/Scenes/"+name+".unity");
 }
 [MenuItem("Rety Kotlety/Przygotuj poziom testowy")]
 public static void BuildTestLevel(){
  BuildScene("TestLevel",false);
  var scenes=new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
  if(!scenes.Exists(s=>s.path=="Assets/Scenes/TestLevel.unity"))scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/TestLevel.unity",true));
  EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
 }
 [MenuItem("Rety Kotlety/Sprawdź dane migracji")]
 public static void Validate(){
  var asset=Resources.Load<TextAsset>("Converted/level");if(!asset)throw new Exception("Brak danych poziomu");var data=JsonUtility.FromJson<LevelData>(asset.text);
  if(data.colliders.Length<100||data.spawners.Length!=6||data.actors.Length!=9)throw new Exception("Niekompletne dane poziomu");
  foreach(var name in new[]{"Level001","RightHandRig","LeftHandRig","MeleeEnemyRig","ShootingEnemyRig","SausageLink","SchabowyProjectile","ChrupiacaProjectile","SchabowyPickup","ChrupiacaPickup","PierogPickup","JetpackPickup"})if(!Resources.Load<GameObject>("Converted/"+name))throw new Exception("Brak modelu: "+name);
  foreach(var spawner in data.spawners)if(spawner.points.Length==0||spawner.waves.Length==0)throw new Exception("Pusta arena");
  Debug.Log("Dane migracji i modele poprawne.");
 }
 public static void BuildAndValidate(){BuildScenes();Validate();}
}
}



