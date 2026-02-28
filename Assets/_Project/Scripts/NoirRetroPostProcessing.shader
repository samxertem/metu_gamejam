Shader "Hidden/NoirRetro"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Contrast ("Contrast", Float) = 1.5
        _Brightness ("Brightness", Float) = 1.0
        _VignetteIntensity ("Vignette Intensity", Float) = 1.3
        _NoiseIntensity ("Noise Intensity", Float) = 0.15
        _TimeX ("Time", Float) = 0.0
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _Contrast;
            float _Brightness;
            float _VignetteIntensity;
            float _NoiseIntensity;
            float _TimeX;

            float rand(float2 co){
                return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // 1. Grayscale Conversion (Luminance preserving)
                float gray = dot(col.rgb, float3(0.299, 0.587, 0.114));

                // 2. Contrast
                gray = (gray - 0.5) * _Contrast + 0.5;
                
                // 3. Brightness
                gray *= _Brightness;

                // 4. Vignette (Dark edges)
                float2 center = i.uv - 0.5;
                float dist = length(center);
                float vignette = smoothstep(0.8, 0.2, dist * _VignetteIntensity);
                gray *= vignette;

                // 5. Film Grain (Noise)
                float noise = (rand(i.uv + _TimeX) - 0.5) * _NoiseIntensity;
                gray += noise;

                // Clamp values between 0 and 1
                gray = clamp(gray, 0.0, 1.0);

                // Classic Noir Cinematic Tint
                // Deep blacks, bright whites, but with a very tiny microscopic hint of blue to avoid flat digital B&W
                col.rgb = float3(gray * 0.95, gray * 0.95, gray * 1.0);
                col.a = 1.0;
                
                return col;
            }
            ENDCG
        }
    }
}
