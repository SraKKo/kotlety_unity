Shader "Hidden/RetyKotlety/DistrictRetro" {
 Properties { _MainTex ("World image", 2D) = "white" {} }
 SubShader {
  Cull Off ZWrite Off ZTest Always
  Pass {
   CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   sampler2D _MainTex;
   float4 _MainTex_TexelSize;
   float2 _Grid;
   float _ColorLevels, _Dither, _Saturation, _Contrast, _Vignette;
   float Bayer(float2 pixel) {
    float2 cell=fmod(floor(pixel),4);
    float4 row;
    if(cell.y<1)row=float4(0,8,2,10);
    else if(cell.y<2)row=float4(12,4,14,6);
    else if(cell.y<3)row=float4(3,11,1,9);
    else row=float4(15,7,13,5);
    float value=cell.x<1?row.x:cell.x<2?row.y:cell.x<3?row.z:row.w;
    return (value+.5)/16-.5;
   }
   fixed4 frag(v2f_img input) : SV_Target {
    float2 pixel=floor(input.uv*_Grid);
    float2 uv=(pixel+.5)/_Grid;
    // Fetch a source texel centre to keep scaling crisp even with bilinear source filtering.
    float2 sourceSize=abs(_MainTex_TexelSize.zw);
    uv=(floor(uv*sourceSize)+.5)/sourceSize;
    float4 source=tex2D(_MainTex,uv);
    float3 color=max(source.rgb,0);
    #ifndef UNITY_COLORSPACE_GAMMA
     color=LinearToGammaSpace(color);
    #endif
    float luminance=dot(color,float3(.2126,.7152,.0722));
    color=lerp(luminance.xxx,color,_Saturation);
    float light=smoothstep(.12,.85,luminance);
    color*=lerp(float3(.93,.97,1.02),float3(1.04,1.01,.95),light);
    color=(color-.5)*_Contrast+.5;
    float2 edge=((pixel+.5)/_Grid-.5)*2;
    color*=1-_Vignette*dot(edge,edge)*.25;
    float steps=max(1,_ColorLevels-1);
    color=floor(saturate(color)*steps+.5+Bayer(pixel)*_Dither)/steps;
    color=saturate(color);
    #ifndef UNITY_COLORSPACE_GAMMA
     color=GammaToLinearSpace(color);
    #endif
    return float4(color,source.a);
   }
   ENDCG
  }
 }
 Fallback Off
}