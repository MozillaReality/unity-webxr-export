// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "VR/reticle"
{
	Properties
	{
		_MainTex("Base (RGB), Alpha (A)", 2D) = "white" {}

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_ColorMask("Color Mask", Float) = 15
	}

		SubShader
		{
			LOD 100

			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
			}

			Stencil
			{
				Ref[_Stencil]
				Comp[_StencilComp]
				Pass[_StencilOp]
			}

			Cull Off
			Lighting Off
			ZWrite Off
			ZTest Always
			Offset -1, -1
			Fog { Mode Off }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask[_ColorMask]

			Pass
			{
				CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					#include "UnityCG.cginc"

					struct appdata_t
					{
						float4 vertex : POSITION;
						float2 texcoord : TEXCOORD0;
						fixed4 color : COLOR;
					};

					struct v2f
					{
						float4 vertex : SV_POSITION;
						half2 texcoord : TEXCOORD0;
						fixed4 color : COLOR;
					};

					sampler2D _MainTex;
					float4 _MainTex_ST;

					v2f vert(appdata_t v)
					{
						v2f o;
						o.vertex = UnityObjectToClipPos(v.vertex);
						o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
						o.color = v.color;
						return o;
					}

					fixed4 frag(v2f i) : COLOR
					{
						fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;
						clip(col.a - 0.01);
						return col;
					}
				ENDCG
			}
		}
}
