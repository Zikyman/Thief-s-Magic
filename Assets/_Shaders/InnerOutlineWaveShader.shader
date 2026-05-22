Shader "Custom/PixelArtInnerOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Outline Settings)]
        [IntRange] _OutlineWidth ("Outline Width (Pixels)", Range(0, 10)) = 1
        _OutlineColor ("Base Outline Color", Color) = (1, 0, 0, 1) // Nastavíš jen jednu barvu
        
        [Header(Wave Animation)]
        _PulseSpeed ("Pulse Speed (Time)", Range(0, 20)) = 5
        _WaveFrequency ("Wave Frequency (Space)", Range(0, 30)) = 10
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane" 
            "CanUseSpriteAtlas"="True" 
        }
        
        Cull Off 
        Lighting Off 
        ZWrite Off 
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t 
            { 
                float4 vertex : POSITION; 
                float4 color : COLOR; 
                float2 texcoord : TEXCOORD0; 
            };
            
            struct v2f 
            { 
                float4 vertex : SV_POSITION; 
                fixed4 color : COLOR; 
                float2 texcoord : TEXCOORD0; 
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            
            float _OutlineWidth;
            fixed4 _OutlineColor;
            float _PulseSpeed;
            float _WaveFrequency;

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
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                float w = round(_OutlineWidth);
                
                // 1. Odběr vzorků ze sousedních pixelů (nahoru, dolů, doprava, doleva)
                float alphaUp = tex2D(_MainTex, IN.texcoord + float2(0, _MainTex_TexelSize.y * w)).a;
                float alphaDown = tex2D(_MainTex, IN.texcoord + float2(0, -_MainTex_TexelSize.y * w)).a;
                float alphaRight = tex2D(_MainTex, IN.texcoord + float2(_MainTex_TexelSize.x * w, 0)).a;
                float alphaLeft = tex2D(_MainTex, IN.texcoord + float2(-_MainTex_TexelSize.x * w, 0)).a;

                // 2. Detekce hrany (pokud je pixel neprůhledný, ale dotýká se průhledného místa nebo okraje textury)
                if (c.a > 0.1 && (alphaUp < 0.1 || alphaDown < 0.1 || alphaRight < 0.1 || alphaLeft < 0.1 || 
                    IN.texcoord.x <= _MainTex_TexelSize.x * w || IN.texcoord.x >= 1.0 - _MainTex_TexelSize.x * w ||
                    IN.texcoord.y <= _MainTex_TexelSize.y * w || IN.texcoord.y >= 1.0 - _MainTex_TexelSize.y * w))
                {
                    // 3. Výpočet vlnění
                    float wave = sin(_Time.y * _PulseSpeed + (IN.texcoord.x + IN.texcoord.y) * _WaveFrequency);
                    
                    // 4. Automatický výpočet tří odstínů
                    fixed4 normalColor = _OutlineColor;
                    fixed4 lightColor = lerp(normalColor, fixed4(1, 1, 1, 1), 0.6); // 60% posun k čisté bílé
                    fixed4 darkColor = normalColor * 0.3; // 30% z původního jasu (výrazné ztmavení)

                    fixed4 currentOutlineColor;

                    // 5. Aplikace vlny na barvu
                    if (wave > 0)
                        currentOutlineColor = lerp(normalColor, lightColor, wave);
                    else
                        currentOutlineColor = lerp(normalColor, darkColor, -wave);

                    // Zachování původní průhlednosti hrany spritu
                    currentOutlineColor.a = c.a; 
                    return currentOutlineColor;
                }

                // Pokud to není hrana, vrátí se původní obrázek
                return c;
            }
            ENDCG
        }
    }
}