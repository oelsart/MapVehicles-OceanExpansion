Shader "MapVehiclesOcean/SailEmblemMask"
{
    Properties
    {
        _MainTex("Main texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent-100" }

        Pass
        {
            ColorMask 0
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

            fixed4 frag(v2f i) : SV_Target
            {
                clip(0.5 - tex2D(_MainTex, i.uv).r);
                return float4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
    FallBack Off
}
