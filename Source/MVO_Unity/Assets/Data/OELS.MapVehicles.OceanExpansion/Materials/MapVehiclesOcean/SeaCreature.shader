Shader "MapVehiclesOcean/SeaCreature"
{
    Properties
    {
        _MainTex("Main texture", 2D) = "white" {}
        _MaskTex("Mask texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _ColorTwo("ColorTwo", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "IgnoreProjector" = "true"
            "Queue" = "Transparent-150"
            "RenderType" = "Transparent"
        }

        // 水面下部分の描画（乗算ブレンド + 水面の揺らめき）
        Pass
        {
            Blend DstColor OneMinusSrcAlpha
            ZWrite Off

            Stencil
            {
                Ref 232
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

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _Color;
            float4 _ColorTwo;
            float _GameSeconds;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float aSum = 0;
                float bSum = 0;
                for (int j = -1; j < 2; j++)
                    for (int k = -1; k < 2; k++)
                    {
                        float2 uv = float2(i.uv.x + 0.02 * j, i.uv.y + 0.001 * k);
                        aSum += 1 - tex2D(_MainTex, uv).a;
                        bSum += tex2D(_MaskTex, uv).b;
                    }

                float wave = sin(_GameSeconds * 7.5 + i.uv.y * 100) * 0.03 * aSum / 9 * bSum / 9;
                float2 uv = i.uv;
                uv.x += wave;
                float4 mainTexColor = tex2D(_MainTex, uv);
                float4 maskTexColor = tex2D(_MaskTex, uv);
                clip(mainTexColor.a - 0.5);

                float u = maskTexColor.r;
                float v = maskTexColor.g;
                float x = 1.0 - u - v;
                float4 finalColor = mainTexColor * (_Color * u + _ColorTwo * v + float4(1, 1, 1, 1) * x);
                return finalColor;
            }
            ENDCG
        }

        // 水上部分の描画
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

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
            float4 _Color;
            float4 _ColorTwo;

            fixed4 frag(v2f i) : SV_Target
            {
                float4 mainTexColor = tex2D(_MainTex, i.uv);
                float4 maskTexColor = tex2D(_MaskTex, i.uv);

                float u = maskTexColor.r;
                float v = maskTexColor.g;
                float x = 1.0 - u - v;
                float4 finalColor = mainTexColor * (_Color * u + _ColorTwo * v + float4(1, 1, 1, 1) * x);

                clip(finalColor.a - maskTexColor.b - 0.5);
                return finalColor;
            }
            ENDCG
        }
    }
    Fallback Off
}