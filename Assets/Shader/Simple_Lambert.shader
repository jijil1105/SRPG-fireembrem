Shader "Custom/Simple_Lambert"
{
    Properties
    {
        _Color("Color", color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf SimpleLambert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        float4 _Color;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = _Color;
        }

        half4 LightingSimpleLambert(SurfaceOutput s, half3 lightDir, half atten)
		{
			 half NdotL = max(0, dot (s.Normal, lightDir));
			 half4 c;
			 c.rgb = s.Albedo * _LightColor0.rgb * NdotL + fixed4(0.2f, 0.2f, 0.2f, 1);
			 c.a = s.Alpha;
			 return c;
		}
        ENDCG
    }
    FallBack "Diffuse"
}
