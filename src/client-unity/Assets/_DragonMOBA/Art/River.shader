Shader "DragonMoba/Flowing River" {
Properties { _Color("Deep water",Color)=(.035,.28,.32,1) _Shallow("Shallow water",Color)=(.18,.58,.56,1) }
SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" } Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
Pass { CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;};
fixed4 _Color,_Shallow;
v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
fixed4 frag(v2f i):SV_Target{
float edge=pow(abs(i.uv.y*2-1),7);
float wave=sin(i.world.x*2.5+i.world.z*5-_Time.y*1.4)*sin(i.world.x*.7-i.world.z*9+_Time.y);
float glint=pow(saturate(wave),14);
float flow=sin(i.world.x*1.7+i.world.z*15-_Time.y*2)*.03;
return fixed4(lerp(_Color.rgb,_Shallow.rgb,edge*.8)+glint*.18+flow, .96);
}
ENDCG }
} }

