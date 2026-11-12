Shader "RetyKotlety/PSXWorld" {
 Properties {
  _MainTex ("PSX albedo", 2D) = "white" {}
  _Color ("Route tint", Color) = (1,1,1,1)
  _WorldTiling ("Tiles per metre", Float) = 0.35
 }
 SubShader {
  Tags { "RenderType"="Opaque" }
  LOD 150
  CGPROGRAM
  #pragma surface surf Lambert fullforwardshadows
  #pragma target 3.0
  sampler2D _MainTex;
  fixed4 _Color;
  float _WorldTiling;
  struct Input { float3 worldPos; float3 worldNormal; };
  void surf(Input IN, inout SurfaceOutput o) {
   float3 weight=pow(abs(IN.worldNormal),8);
   weight/=max(weight.x+weight.y+weight.z,0.0001);
   float3 p=IN.worldPos*_WorldTiling;
   fixed3 albedo=tex2D(_MainTex,p.zy).rgb*weight.x+tex2D(_MainTex,p.xz).rgb*weight.y+tex2D(_MainTex,p.xy).rgb*weight.z;
   o.Albedo=albedo*_Color.rgb;o.Alpha=1;
  }
  ENDCG
 }
 Fallback "Diffuse"
}
