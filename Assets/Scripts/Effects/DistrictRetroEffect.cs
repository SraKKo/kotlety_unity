using UnityEngine;
namespace RetyKotlety {
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class DistrictRetroEffect : MonoBehaviour {
 [Range(180,1080)] public int verticalResolution=432;
 [Range(8,64)] public int colorLevels=32;
 [Range(0,1)] public float ditherStrength=.65f;
 [Range(0,1.5f)] public float saturation=.9f;
 [Range(.8f,1.3f)] public float contrast=1.06f;
 [Range(0,.4f)] public float vignette=.12f;
 Material material;
 bool warned;
 void OnRenderImage(RenderTexture source,RenderTexture destination) {
  if(!material) {
   var shader=Resources.Load<Shader>("PSX/Shaders/DistrictRetro");
   if(!shader||!shader.isSupported) {
    if(!warned){Debug.LogWarning("Efekt PSX dzielnicy nie jest dostepny na tym urzadzeniu.",this);warned=true;}
    Graphics.Blit(source,destination);
    return;
   }
   material=new Material(shader){hideFlags=HideFlags.HideAndDontSave};
  }
  int height=Mathf.Clamp(verticalResolution,1,source.height);
  int width=Mathf.Max(1,Mathf.RoundToInt(height*(float)source.width/source.height));
  material.SetVector("_Grid",new Vector4(width,height,0,0));
  material.SetFloat("_ColorLevels",Mathf.Max(2,colorLevels));
  material.SetFloat("_Dither",ditherStrength);
  material.SetFloat("_Saturation",saturation);
  material.SetFloat("_Contrast",contrast);
  material.SetFloat("_Vignette",vignette);
  Graphics.Blit(source,destination,material);
 }
 void OnDisable() {
  if(!material)return;
  if(Application.isPlaying)Destroy(material);
  else DestroyImmediate(material);
  material=null;
 }
}
}