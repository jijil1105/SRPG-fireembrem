Shader "Custom/Rim_Lighting"
{
    SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard 
		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
			float3 worldNormal;
      		float3 viewDir;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 baseColor = fixed4(0.05, 0.1, 0, 1);
			fixed4 rimColor  = fixed4(0.5,0.7,0.5,1);

			o.Albedo = baseColor;
			float rim = 1 - saturate(dot(IN.viewDir, o.Normal));
     			o.Emission = rimColor * pow(rim, 2.5);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
