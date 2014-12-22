Shader "Toon/Lighted" {
	Properties {
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
CGPROGRAM
#pragma surface surf ToonRamp


// custom lighting function that uses a texture ramp based
// on angle between light direction and normal
#pragma lighting ToonRamp exclude_path:prepass
inline half4 LightingToonRamp (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
{
		half4 tone;
		if(dot (s.Normal, lightDir)>0) 
		{
		    half3 h = normalize (lightDir + viewDir);

	        half diff = max (0, dot (s.Normal, lightDir));

	        float nh = max (0, dot (s.Normal, h));
	        float spec = pow (nh, 48.0);
			
	        half4 c;
	        c.rgb = ( _LightColor0.rgb * diff + _LightColor0.rgb * spec) * (atten * 2);
	        half lightStr = dot(c.rgb, float3(0.3, 0.59, 0.11)).r;
	        tone.rgb = s.Albedo *floor(lightStr*3)/3* _LightColor0.rgb;
        }
        else
        {
        	tone.rgb = half4(0,0,0,1);
        }
        return tone;
}


sampler2D _MainTex;
float4 _Color;

struct Input {
	float2 uv_MainTex : TEXCOORD0;
};

void surf (Input IN, inout SurfaceOutput o) {
	half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	o.Albedo = c.rgb;
	o.Alpha = c.a;
}
ENDCG

	} 

	Fallback "Diffuse"
}
