Shader "DragonMoba/Fighter Toon" {
Properties {
_Color("Gi",Color)=(.9,.28,.035,1) _Skin("Skin",Color)=(.96,.64,.39,1) _Trim("Trim",Color)=(.025,.1,.24,1)
_MinY("Mesh min Y",Float)=0 _SpanY("Mesh height",Float)=2 _Width("Mesh width",Float)=1
_HeightAxis("Height axis",Vector)=(0,0,1,0)
_Outfit("Outfit bands",Float)=1
}
SubShader { Tags { "RenderType"="Opaque" }
Pass { Tags { "LightMode"="ForwardBase" } CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fwdbase
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;};
struct v2f {float4 pos:SV_POSITION;float3 normal:TEXCOORD0;float3 world:TEXCOORD1;float3 local:TEXCOORD2;SHADOW_COORDS(3)};
fixed4 _Color,_Skin,_Trim;float _MinY,_SpanY,_Width,_Outfit;float4 _HeightAxis;
v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.local=v.vertex.xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;TRANSFER_SHADOW(o);return o;}
fixed4 frag(v2f i):SV_Target{
float h=(dot(i.local,_HeightAxis.xyz)-_MinY)/max(.001,_SpanY), x=abs(i.local.x)/max(.001,_Width);
fixed3 c=_Color.rgb;
if(_Outfit>.5){
 if(h>.88 || (x>.20 && h>.44)) c=_Skin.rgb;
 if(h<.15 || (h>.47 && h<.525) || (x>.3 && h>.42 && h<.49)) c=_Trim.rgb;
 if(h>.70 && h<.87 && x<(h-.70)*.64) c=_Trim.rgb;
}
UNITY_LIGHT_ATTENUATION(atten,i,i.world);
float l=saturate(dot(normalize(i.normal),normalize(_WorldSpaceLightPos0.xyz)))*atten;
float ramp=l>.65?1:(l>.25?.78:.48);
float rim=pow(1-saturate(dot(normalize(i.normal),normalize(_WorldSpaceCameraPos-i.world))),3)*.1;
return fixed4(c*ramp+rim*c,1);
}
ENDCG } }
Fallback "Diffuse"
}

