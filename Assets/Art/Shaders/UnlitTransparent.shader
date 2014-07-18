Shader "Somian/Unlit/Transparent" {
 
Properties {
    _Color ("Main Color (A=Opacity)", Color) = (1,1,1,1)
    _MainTex ("Base (A=Opacity)", 2D) = ""
    _Mask ("Culling Mask", 2D) = "white" {}
}
 
Category {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True"}
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha
    AlphaTest GEqual [_Cutoff]
 
    SubShader 
    {
    	Pass {

    	
	        GLSLPROGRAM
	        varying mediump vec2 uv;
	       
	        #ifdef VERTEX
	        uniform mediump vec4 _MainTex_ST;
	        void main() {
	            gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
	            uv = gl_MultiTexCoord0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
	        }
	        #endif
	       
	        #ifdef FRAGMENT
	        uniform lowp sampler2D _MainTex;
	         uniform lowp sampler2D _Mask;
	        uniform lowp vec4 _Color;
	        void main() {
	
	            gl_FragColor = texture2D(_MainTex, uv) * texture2D(_Mask, uv) * _Color;
	        }
	        
	        #endif     
	        ENDGLSL
	        

   	   }

    }
   
    SubShader {Pass {
        SetTexture[_MainTex] {Combine texture * constant ConstantColor[_Color]}
    }}
}
 
}