Shader "DragonMoba/Hero Identity" {
Properties {
 _Color("Identity",Color)=(.1,.3,.7,1) _Skin("Skin",Color)=(1,.72,.48,1) _Accent("Accent",Color)=(.8,.1,.05,1)
 _Style("Style",Float)=0 _MinY("Min Y",Float)=0 _SpanY("Height",Float)=2 _Width("Width",Float)=1 _HeightAxis("Axis",Vector)=(0,0,1,0)
 _RootWorld("Root world",Vector)=(0,0,0,0) _RightWorld("Root right",Vector)=(1,0,0,0) _BodyHeight("Body height",Float)=2
}
SubShader { Tags { "RenderType"="Opaque" } Pass { Tags { "LightMode"="ForwardBase" }
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fwdbase
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"
struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;};
struct v2f {float4 pos:SV_POSITION;float3 normal:TEXCOORD0;float3 world:TEXCOORD1;float3 local:TEXCOORD2;SHADOW_COORDS(3)};
fixed4 _Color,_Skin,_Accent;float _Style,_MinY,_SpanY,_Width,_BodyHeight;float4 _HeightAxis,_RootWorld,_RightWorld;
v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.local=v.vertex.xyz;o.normal=UnityObjectToWorldNormal(v.normal);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;TRANSFER_SHADOW(o);return o;}
fixed4 frag(v2f i):SV_Target {
 float h=(i.world.y-_RootWorld.y)/max(.001,_BodyHeight), x=abs(dot(i.world-_RootWorld.xyz,_RightWorld.xyz))/max(.001,_BodyHeight*.45);
 fixed3 c=_Color.rgb;
 if(_Style>.5 && _Style<1.5){
  if(h>.87 || (x>.23 && h>.48))c=_Skin.rgb;
  if(h<.14 || (h>.45&&h<.51) || (x>.3&&h>.43&&h<.49))c=_Accent.rgb;
 }
 if(_Style>1.5 && _Style<2.5){
  float3 p=i.local*17;float spots=step(.77,sin(p.x*2.3+p.z)*sin(p.y*2.8-p.x));
  c=lerp(c,c*.22,spots);
  if((h>.49&&h<.74&&x<.16)||h<.12)c=_Accent.rgb;
  if(x>.22&&h>.41&&h<.57)c=lerp(c,fixed3(.64,.38,.60),.75);
 }
 if(_Style>2.5&&_Style<3.5){c*=.8+.2*abs(sin(i.local.x*51+i.local.z*31));if(h<.12)c=_Skin.rgb;}
 if(_Style>3.5){if(h>.48)c=_Skin.rgb;else if(h<.14)c=_Accent.rgb;}
 UNITY_LIGHT_ATTENUATION(atten,i,i.world);
 float light=saturate(dot(normalize(i.normal),normalize(_WorldSpaceLightPos0.xyz)))*atten;
 float ramp=light>.65?1:light>.22?.82:.57;
 float rim=pow(1-saturate(dot(normalize(i.normal),normalize(_WorldSpaceCameraPos-i.world))),3)*.12;
 return fixed4(c*(ramp+rim),1);
}
ENDCG
} } Fallback "Diffuse"
}
