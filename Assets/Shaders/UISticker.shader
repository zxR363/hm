Shader "UI/UISticker"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 10)) = 1

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float4 _ClipRect;
            float4 _MainTex_ST;
            
            // FIX: Declare _TextureSampleAdd
            fixed4 _TextureSampleAdd;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                
                // --- OUTLINE LOGIC ---
                // If the pixel is transparent (or semi-transparent), check neighbors.
                // If any neighbor is opaque, this pixel should be outline color.
                
                float alpha = color.a;
                float maxAlpha = alpha;
                
                float2 uv = IN.texcoord;
                float2 offset = _MainTex_TexelSize.xy * _OutlineWidth;

                // Sample 8 neighbors
                maxAlpha = max(maxAlpha, tex2D(_MainTex, uv + float2(0, offset.y)).a);       // N
                maxAlpha = max(maxAlpha, tex2D(_MainTex, uv + float2(0, -offset.y)).a);      // S
                maxAlpha = max(maxAlpha, tex2D(_MainTex, uv + float2(offset.x, 0)).a);       // E
                maxAlpha = max(maxAlpha, tex2D(_MainTex, uv + float2(-offset.x, 0)).a);      // W
                
                maxAlpha = max(maxAlpha, tex2D(_MainTex, uv + float2(offset.x, offset.y)).a);   // NE
                maxAlpha = max(maxAlpha, tex2D(_MainTex, uv + float2(-offset.x, offset.y)).a);  // NW
                maxAlpha = max(maxAlpha, tex2D(_MainTex, uv + float2(offset.x, -offset.y)).a);  // SE
                maxAlpha = max(maxAlpha, tex2D(_MainTex, uv + float2(-offset.x, -offset.y)).a); // SW

                // Calculate outline factor
                // We want to blend between OutlineColor and TextureColor based on 'alpha'.
                // But we also want to boost alpha if it's an outline.
                
                fixed4 finalColor = lerp(_OutlineColor, color, alpha);
                finalColor.a = max(alpha, maxAlpha * _OutlineColor.a);
                
                // ---------------------

                #ifdef UNITY_UI_CLIP_RECT
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (finalColor.a - 0.001);
                #endif

                return finalColor;
            }
            ENDCG
        }
    }
}
