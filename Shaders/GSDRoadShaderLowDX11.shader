//Courtesy of http://answers.unity3d.com/questions/306921/add-specular-map-to-shader.html
Shader "GSD/DX11/Road Shader Low Quality" {
    Properties {
    	_Color ("Main Color", Color) = (1,1,1,1)
    	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
    }
    SubShader {
    	Tags { "RenderType"="Opaque" }
    	Tags { "Queue" = "Geometry+10" }
    	LOD 200
     	
	    CGPROGRAM
	    #pragma surface surf BlinnPhong

		sampler2D _MainTex;
	    fixed4 _Color;
	     
	    struct Input {
	    	float2 uv_MainTex;
	    };
     
    	void surf (Input IN, inout SurfaceOutput o) {
	    	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	    	o.Albedo = tex.rgb * _Color.rgb;
	    	o.Gloss = tex.a;
	    	o.Alpha = tex.a * _Color.a;
	    }
	    ENDCG
    }
    FallBack "VertexLit"
}
