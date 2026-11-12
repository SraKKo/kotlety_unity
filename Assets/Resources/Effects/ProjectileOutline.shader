Shader "RetyKotlety/ProjectileOutline" {
 Properties {
  _Color ("Outline color", Color) = (0.065, 0.022, 0.008, 1)
  _Width ("Outline width in pixels", Float) = 3
 }
 SubShader {
  Tags { "Queue"="Geometry+1" "RenderType"="Opaque" }
  Pass {
   Cull Front
   ZWrite Off
   ZTest LEqual
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   fixed4 _Color;
   float _Width;
   float4 vert(appdata_base v) : SV_POSITION {
    float3 world = mul(unity_ObjectToWorld, v.vertex).xyz;
    float3 normal = UnityObjectToWorldNormal(v.normal);
    float4 clip = UnityWorldToClipPos(world);
    float4 tip = UnityWorldToClipPos(world + normal);
    float2 projected = tip.xy * clip.w - clip.xy * tip.w;
    float2 pixelDirection = projected * _ScreenParams.xy;
    pixelDirection /= max(length(pixelDirection), 0.00001);
    clip.xy += pixelDirection * (2.0 * _Width / _ScreenParams.xy) * clip.w;
    return clip;
   }
   fixed4 frag() : SV_Target { return _Color; }
   ENDCG
  }
 }
}
