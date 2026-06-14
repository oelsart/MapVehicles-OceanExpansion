Shader "MapVehiclesOcean/CutoutComplexOutline"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
        _MaskTex("Albedo (RGB)", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _ColorTwo("ColorTwo", Color) = (1,1,1,1)
        _StencilID("Stencil ID", Float) = 1
        _OutlineWidth("Outline Width", Float) = 8
    }
    SubShader
    {
        Tags
        {
            "IgnoreProjector" = "true"
            "Queue" = "Transparent-100"
            "RenderType" = "Transparent"
        }
		Pass
		{
		    Blend SrcAlpha OneMinusSrcAlpha
		    ZWrite Off

		    Stencil
		    {
		        Ref [_StencilID]
		        Comp NotEqual
		        Pass Keep
		    }

		    CGPROGRAM
		    #pragma vertex vert
		    #pragma fragment frag
		    #include "UnityCG.cginc"

		    struct appdata
		    {
		        float4 vertex : POSITION;
		        float2 uv : TEXCOORD0;
		    };

		    struct v2f
		    {
		        float2 uv : TEXCOORD0;
		        float4 vertex : SV_POSITION;
		    };

		    sampler2D _MainTex;
		    float4 _MainTex_TexelSize;
		    float _OutlineWidth;

		    v2f vert(appdata v)
		    {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
		    }

		    fixed4 frag(v2f i) : SV_Target
		    {
			    float maxAlpha = 0.0;
		    	for (int x = -2; x <= 2; x++)
		    	{
		    		for (int y = -2; y <= 2; y++)
		    		{
		    			float2 normalized = normalize(float2(x, y));
		    			maxAlpha = max(maxAlpha, tex2D(_MainTex, i.uv + _MainTex_TexelSize * normalized * _OutlineWidth).a);
		    		}
		    	}

			    clip(maxAlpha - 0.5);
			    return fixed4(0, 0, 0, 1);
		    }

		    ENDCG
		}

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
	        Stencil
	        {
	            Ref [_StencilID]
	            Comp NotEqual
	            Pass Replace
	        }

            CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _MaskTex;

			float4 _MainTexColor;
			float4 _MaskTexColor;

			float4 finalColor;

			float4 _Color : _Color;
			float4 _ColorTwo : _ColorTwo;

			fixed4 frag(v2f i) : SV_Target
			{
				_MainTexColor = tex2D(_MainTex, i.uv);
				_MaskTexColor = tex2D(_MaskTex, i.uv);
				finalColor = _MainTexColor;

				float u = _MaskTexColor.r;
				float v = _MaskTexColor.g;
				float x = 1 - u - v;

				finalColor *= _Color * u + _ColorTwo * v + float4(1,1,1,1) * x;

				clip(finalColor.a - 0.5);
				return finalColor;
			}
			ENDCG
        }
    }
    Fallback Off
}