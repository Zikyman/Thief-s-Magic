Shader "Custom/PixelArtPulsingShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Color Swap Settings)]
        _TargetColor ("Target Color (Mask)", Color) = (1,0,1,1)
        _ReplaceColor ("Replacement Color", Color) = (1,0,0,1)
        _Tolerance ("Tolerance", Range(0, 1)) = 0.05
        
        [Header(Breathing Animation)]
        _PulseSpeed ("Breathing Speed", Range(0, 20)) = 3
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t { float4 vertex : POSITION; float4 color : COLOR; float2 texcoord : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 texcoord : TEXCOORD0; };

            sampler2D _MainTex;
            fixed4 _Color;
            
            fixed4 _TargetColor;
            fixed4 _ReplaceColor;
            float _Tolerance;
            float _PulseSpeed;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord);
                float dist = distance(c.rgb, _TargetColor.rgb); 
                
                if (dist < _Tolerance && c.a > 0.01)
                {
                    // MATEMATIKA BEZ PROSTORU: Závisí POUZE na čase
                    float wave = sin(_Time.y * _PulseSpeed);
                    
                    fixed4 baseColor = _ReplaceColor;
                    fixed4 lightColor = lerp(baseColor, fixed4(1, 1, 1, 1), 0.5); // Zesvětlení
                    fixed4 darkColor = baseColor * 0.5; // Ztmavení
                    
                    // Celá zbraň plynule pulzuje najednou
                    if (wave > 0)
                        c.rgb = lerp(baseColor.rgb, lightColor.rgb, wave);
                    else
                        c.rgb = lerp(baseColor.rgb, darkColor.rgb, -wave);
                }

                return c * IN.color;
            }
            ENDCG
        }
    }
}