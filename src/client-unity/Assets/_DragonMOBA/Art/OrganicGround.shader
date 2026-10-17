Shader "DragonMoba/Organic Ground" {
Properties { _Color("Tint",Color)=(1,1,1,1) }
SubShader { Tags { "RenderType"="Opaque" } Pass {
Tags { "LightMode"="ForwardBase" }
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fwdbase
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; fixed4 color:COLOR; };
struct v2f { float4 pos:SV_POSITION; float3 world:TEXCOORD0; float3 normal:TEXCOORD1; fixed4 color:COLOR; SHADOW_COORDS(2) };
fixed4 _Color;
float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.color=v.color;TRANSFER_SHADOW(o);return o;}
fixed4 frag(v2f i):SV_Target{
UNITY_LIGHT_ATTENUATION(atten,i,i.world);
float detail=noise(i.world.xz*.8)*.18+noise(i.world.xz*9)*.07;
float light=.62+.38*saturate(dot(normalize(i.normal),normalize(_WorldSpaceLightPos0.xyz)))*atten;
return fixed4(i.color.rgb*_Color.rgb*(.86+detail)*light,1);
}
ENDCG
} } Fallback "Diffuse"
}

