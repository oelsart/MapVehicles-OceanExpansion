Shader "MapVehiclesOcean/CutoutMultiply"
{
    Properties
    {
        _MainTex ("Main texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
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
            Blend DstColor Zero
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float2 uv : TEXCOORD;
                float4 color : COLOR;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 color = tex2D(_MainTex, i.uv);
                clip(color.a - 0.5);
                return color * _Color * i.color;
            }
            ENDCG
        }
    }
    Fallback Off
}