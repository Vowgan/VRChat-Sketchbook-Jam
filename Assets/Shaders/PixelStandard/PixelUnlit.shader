Shader "Silent/PixelUnlit"
{
    Properties
    {
        [HDR]_Color("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        [Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", Int) = 2

        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Blend [_SrcBlend] [_DstBlend]
        ZWrite [_ZWrite]
        Cull [_CullMode]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            float4 _Color;

            float _Cutoff;

            // Returns pixel sharpened to nearest pixel boundary. 
            // texelSize is Unity _Texture_TexelSize; zw is w/h, xy is 1/wh
            float2 sharpSample( float4 texelSize , float2 p )
            {
                p = p*texelSize.zw;
                float2 c = max(0.0, abs(fwidth(p)));
                p = p + abs(c);
                p = floor(p) + saturate(frac(p) / c);
                p = (p - 0.5)*texelSize.xy;
                return p;
            }


            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 sUV = sharpSample(_MainTex_TexelSize, i.uv);
                // sample the texture
                fixed4 col = tex2D(_MainTex, sUV) * _Color;
                #if defined(_ALPHATEST_ON)
                    float alpha = col.a;
                    clip (alpha - _Cutoff);
                #endif
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
        
    }
    FallBack "VertexLit"
    CustomEditor "SilentTools.AltUnlitShaderGUI"
}
