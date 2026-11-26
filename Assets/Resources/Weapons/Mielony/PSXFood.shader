Shader "RetyKotlety/PSXFood" {
 Properties { _Color ("Tint", Color) = (1,1,1,1) }
 SubShader {
  Tags { "RenderType"="Opaque" }
  LOD 150
  CGPROGRAM
  #pragma surface surf Lambert fullforwardshadows
  #pragma target 3.0
  fixed4 _Color;
  struct Input { fixed4 color : COLOR; };
  void surf(Input IN, inout SurfaceOutput o) {
   fixed3 albedo = IN.color.rgb;
   #ifndef UNITY_COLORSPACE_GAMMA
    albedo = GammaToLinearSpace(albedo);
   #endif
   o.Albedo = albedo * _Color.rgb;
   o.Alpha = 1;
  }
  ENDCG
 }
 Fallback "Diffuse"
}