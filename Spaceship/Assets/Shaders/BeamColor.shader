Shader "Unlit/BeamColor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_MultiplyColor("MultiplyColor", Color) = (1,1,1,1)
    }
    SubShader
    {
		Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
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
			float4 _MultiplyColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

			float DistanceFromCenter(float2 currentPoint)
			{
				float2 centerPoint = float2(0.5f, 0.5f);

				return sqrt((0.5f - currentPoint.x)*(0.5f - currentPoint.x) + (0.5f - currentPoint.y)*(0.5f - currentPoint.y));
			}

			fixed4 lerp(fixed4 col1, fixed4 col2, float t)
			{
				return (1 - t) * col1 + t * col2;
			}


            fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				if (col.a < 0.9f)
				{
					col = col * _MultiplyColor;
				}
				else
				{
					col = lerp(col, _MultiplyColor, 0.1f);
				}
				//col = col * (_MultiplyColor*col.a);

                return col;
            }
            ENDCG
        }
    }
}
