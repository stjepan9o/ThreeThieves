Shader "Custom/FogOfWar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            float4x4 _InverseVP;

            float3 _PlayerPos1;
            float _Radius1;
            float3 _PlayerPos2;
            float _Radius2;
            float _EdgeSoftness;

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            float3 ReconstructWorldPos(float2 uv, float depth)
            {
                float4 clip = float4(uv * 2 - 1, depth, 1);
                #if defined(UNITY_UV_STARTS_AT_TOP)
                clip.y = -clip.y;
                #endif
                float4 world = mul(_InverseVP, clip);
                return world.xyz / world.w;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float depth = tex2D(_CameraDepthTexture, i.uv).r;
                float3 worldPos = ReconstructWorldPos(i.uv, depth);

                float d1 = distance(worldPos.xz, _PlayerPos1.xz);
                float d2 = distance(worldPos.xz, _PlayerPos2.xz);

                float vis1 = 1 - smoothstep(_Radius1 - _EdgeSoftness, _Radius1, d1);
                float vis2 = 1 - smoothstep(_Radius2 - _EdgeSoftness, _Radius2, d2);
                float visibility = max(vis1, vis2);

                col.rgb = lerp(fixed3(0,0,0), col.rgb, visibility);
                return col;
            }
            ENDCG
        }
    }
}