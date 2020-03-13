Shader "Unlit/Scroll"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_ScrollValue("Scroll Amount", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _ScrollValue;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				float2 newMove = float2(_ScrollValue.x*_ScrollValue.z*_Time.x, _ScrollValue.y*_ScrollValue.w*_Time.x);
                fixed4 col = tex2D(_MainTex, i.uv + newMove );

                return col;
            }
            ENDCG
        }
    }
}
