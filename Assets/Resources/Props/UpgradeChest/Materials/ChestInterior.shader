Shader "RetyKotlety/ChestInterior" {
 Properties {
  _Color ("Interior color", Color) = (0.008, 0.006, 0.004, 1)
  [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 1
 }
 SubShader {
  Tags { "RenderType"="Opaque" "Queue"="Geometry" }
  Pass {
   Cull [_Cull]
   ZWrite On
   ZTest LEqual
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   fixed4 _Color;
   float4 vert(appdata_base v) : SV_POSITION { return UnityObjectToClipPos(v.vertex); }
   fixed4 frag() : SV_Target { return fixed4(_Color.rgb, 1); }
   ENDCG
  }
 }
}
