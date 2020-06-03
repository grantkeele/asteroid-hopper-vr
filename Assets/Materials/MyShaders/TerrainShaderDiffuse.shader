// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MyShaders/TerrainShaderDiffuse"
{
	Properties {		
		_TexArray("TexArray", 2DArray) = "" {}
		_NumTextures("Slices", Range(0,16)) = 6

		_MainTex("Base (RGB)", 2D) = "white" {}

		//_VoxelArray("_VoxelArray", 3D) = "" {}

		_Blend("Blending", Range (0.01, 0.55)) = 0.2
		_Color("Main Color", Color) = (1, 1, 1, 1)
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}

	SubShader {
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard vertex:vert fullforwardshadows
		#pragma target 3.0
		#pragma require 2darray

		#include "UnityCG.cginc"

		int _NumTextures;
		fixed4 _Color;
		sampler2D _MainTex;
		sampler3D _VoxelArray;
		float4 _MainTex_ST;
		fixed _Blend;
		half _Glossiness;
		half _Metallic;

		struct Input {
			float3 weight : TEXCOORD0;
			float3 worldPos;
		};


		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			fixed3 n = max(abs(v.normal) - _Blend, 0);
			o.weight = n / (n.x + n.y + n.z).xxx;
		}

		UNITY_DECLARE_TEX2DARRAY(_TexArray);

		void surf(Input IN, inout SurfaceOutputStandard o) {
			
			float3 oPos = mul(unity_WorldToObject, fixed4(IN.worldPos, 1.0)).xyz;
			fixed2 uvx = (oPos.yz - _MainTex_ST.zw) * _MainTex_ST.xy;
			fixed2 uvy = (oPos.xz - _MainTex_ST.zw) * _MainTex_ST.xy;
			fixed2 uvz = (oPos.xy - _MainTex_ST.zw) * _MainTex_ST.xy;

			float4 vox = tex3D(_VoxelArray, float3(oPos.x, oPos.y, oPos.z));

			fixed3 uvxi = fixed3(uvx.x, uvx.y, vox.r * 255);
			fixed3 uvyi = fixed3(uvy.x, uvy.y, vox.r * 255);
			fixed3 uvzi = fixed3(uvz.x, uvz.y, vox.r * 255);

			fixed4 cz = UNITY_SAMPLE_TEX2DARRAY(_TexArray, uvxi) * IN.weight.xxxx;
			fixed4 cy = UNITY_SAMPLE_TEX2DARRAY(_TexArray, uvyi) * IN.weight.yyyy;
			fixed4 cx = UNITY_SAMPLE_TEX2DARRAY(_TexArray, uvzi) * IN.weight.zzzz;
			fixed4 col = (cz + cy + cx) * _Color;
			o.Albedo = col.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = col.a;
		}
		ENDCG
	}

	FallBack "Standard"
}
