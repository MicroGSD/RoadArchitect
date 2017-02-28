//Courtesy of http://answers.unity3d.com/questions/306921/add-specular-map-to-shader.html
Shader "GSD/DX11/Road Shoulder Shader" {
    Properties {
    	_Color ("Main Color", Color) = (1,1,1,1)
    	//_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
   	 	//_Shininess ("Shininess", Range (0, 1)) = 0.078125
    	_MainTex ("Base (RGB) N/A (A)", 2D) = "white" {}
    	//_BumpMap ("Normalmap", 2D) = "bump" {}
    }
    SubShader {
    	Tags { "RenderType"="Opaque" }
    	//Tags { "Queue" = "Geometry+10" }
    	LOD 400
     	
	    CGPROGRAM
	    #pragma surface surf BlinnPhong
	    sampler2D _MainTex;
	    //sampler2D _BumpMap;
	    fixed4 _Color;
	    //half _Shininess;

	    struct Input {
	    	float2 uv_MainTex;
	    	//float2 uv_BumpMap;
	    };
     
    	void surf (Input IN, inout SurfaceOutput o) {
	    	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	    	o.Albedo = tex.rgb * _Color.rgb;
	    	//o.Gloss = tex.a;
	    	o.Alpha = tex.a * _Color.a;
	    	//o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
	    }
	    ENDCG
    }
    FallBack "Diffuse"
}
