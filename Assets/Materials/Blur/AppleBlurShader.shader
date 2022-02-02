// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/APPLE_BLUR" {

    Properties{
        _Color("Main Color", Color) = (1,1,1,1)
        _BumpAmt("Distortion", Range(0,128)) = 10
        _MainTex("Tint Color (RGB)", 2D) = "white" {}
        _BumpMap("Normalmap", 2D) = "bump" {}
        _Size("Size", Int) = 1
    }

        Category{

            Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Opaque" }


            SubShader {

                GrabPass {
                    Tags { "LightMode" = "Always" }
                }
                Pass {
                    Tags { "LightMode" = "Always" }

                    CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #pragma fragmentoption ARB_precision_hint_fastest
                    #include "UnityCG.cginc"

                    struct appdata_t {
                        float4 vertex : POSITION;
                        float2 texcoord: TEXCOORD0;
                    };

                    struct v2f {
                        float4 vertex : POSITION;
                        float4 uvgrab : TEXCOORD0;
                        float2 uvmain : TEXCOORD2;
                    };

                    float4 _MainTex_ST;

                    v2f vert(appdata_t v) {
                        v2f o;
                        o.vertex = UnityObjectToClipPos(v.vertex);
                        #if UNITY_UV_STARTS_AT_TOP
                        float scale = -1.0;
                        #else
                        float scale = 1.0;
                        #endif
                        o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
                        o.uvgrab.zw = o.vertex.zw;
                        o.uvmain = TRANSFORM_TEX(v.texcoord, _MainTex);
                        return o;
                    }

                    sampler2D _GrabTexture;
                    float4 _GrabTexture_TexelSize;
                    float _Size;
                    sampler2D _MainTex;
                    float4 _MainTex_TexelSize;

                    half4 frag(v2f i) : COLOR {
#define MAINALPHATEST(posx) (tex2D(_MainTex, float2(i.uvmain.x + _MainTex_TexelSize.x * posx, i.uvmain.y)).a <1 ? 0 : 1)
#define GRABPIXEL(weight,kernelx) (tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(float4(i.uvgrab.x + _GrabTexture_TexelSize.x * kernelx, i.uvgrab.y, i.uvgrab.z, i.uvgrab.w))) * weight)

                        half4 mainTex = tex2D(_MainTex, i.uvmain);
                        if (mainTex.a < 1.0)
                        {
                            return GRABPIXEL(1, 0.0);
                        }
                        int sizeHD = _GrabTexture_TexelSize.z / 1920 * _Size;
                        half4 sum = half4(0,0,0,0);
                        int size = sizeHD * 2 + 1;
                        int center = sizeHD;
                        int total = (sizeHD + 1) * (sizeHD + 1);
                        float weightSum = 0;
                        [loop]
                        for (int idx = 0; idx < size; idx++)
                        {
                            int dist = idx - center;
                            int absDist = dist * ((dist < 0) ? -1 : 1);
                            float numerator = center - absDist + 1;
                            float pixweight = numerator / total;
                            weightSum += pixweight * MAINALPHATEST(dist);
                            sum += GRABPIXEL(pixweight, dist) * MAINALPHATEST(dist);
                        }
                        sum = (weightSum == 0) ? GRABPIXEL(1, 0.0) : sum * (1.0/weightSum);
                        
                        //half4 bg = GRABPIXEL(1, 0.0);
                        //half4 col = half4(0, 0, 0, 0);
                        //col.r = bg.r * (1 - mainTex.a) + sum.r * mainTex.a;
                        //col.g = bg.g * (1 - mainTex.a) + sum.g * mainTex.a;
                        //col.b = bg.b * (1 - mainTex.a) + sum.b * mainTex.a;
                        //col.a = bg.a * (1 - mainTex.a) + sum.a * mainTex.a;

                        return sum;
                    }
                    ENDCG
                }

                GrabPass {
                    Tags { "LightMode" = "Always" }
                }
                Pass {
                    Tags { "LightMode" = "Always" }

                    CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #pragma fragmentoption ARB_precision_hint_fastest
                    #include "UnityCG.cginc"

                    struct appdata_t {
                        float4 vertex : POSITION;
                        float2 texcoord: TEXCOORD0;
                    };

                    struct v2f {
                        float4 vertex : POSITION;
                        float4 uvgrab : TEXCOORD0;
                        float2 uvmain : TEXCOORD2;
                    };

                    float4 _MainTex_ST;

                    v2f vert(appdata_t v) {
                        v2f o;
                        o.vertex = UnityObjectToClipPos(v.vertex);
                        #if UNITY_UV_STARTS_AT_TOP
                        float scale = -1.0;
                        #else
                        float scale = 1.0;
                        #endif
                        o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
                        o.uvgrab.zw = o.vertex.zw;
                        o.uvmain = TRANSFORM_TEX(v.texcoord, _MainTex);
                        return o;
                    }

                    sampler2D _GrabTexture;
                    float4 _GrabTexture_TexelSize;
                    float _Size;
                    sampler2D _MainTex;
                    float4 _MainTex_TexelSize;

                    half4 frag(v2f i) : COLOR{
                        //return tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvgrab.x, i.uvgrab.y , i.uvgrab.z, i.uvgrab.w)));

#define MAINALPHATEST(posy) (tex2D(_MainTex, float2(i.uvmain.x, i.uvmain.y + _MainTex_TexelSize.y * posy)).a <1 ? 0 : 1)
#define GRABPIXEL(weight,kernely) (tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(float4(i.uvgrab.x, i.uvgrab.y + _GrabTexture_TexelSize.y * kernely, i.uvgrab.z, i.uvgrab.w))) * weight)
                        half4 mainTex = tex2D(_MainTex, i.uvmain);
                        if (mainTex.a < 1.0)
                        {
                            return GRABPIXEL(1, 0.0);
                        }
                        half4 sum = half4(0,0,0,0);

                        int sizeHD = _GrabTexture_TexelSize.w / 1080 * _Size;
                        int size = sizeHD * 2 + 1;
                        int center = sizeHD;
                        int total = (sizeHD + 1) * (sizeHD + 1);
                        float weightSum = 0;
                        [loop]
                        for (int idx = 0; idx < size; idx++)
                        {
                            int dist = idx - center;
                            int absDist = dist * ((dist < 0) ? -1 : 1);
                            float numerator = center - absDist + 1;
                            float pixweight = numerator / total;
                            weightSum += pixweight * MAINALPHATEST(dist);
                            sum += GRABPIXEL(pixweight, dist) * MAINALPHATEST(dist);
                        }
                        sum = (weightSum == 0) ? GRABPIXEL(1, 0.0) : sum * (1.0 / weightSum);
                        return sum;
                    }
                    ENDCG
                }

                GrabPass {
                    Tags { "LightMode" = "Always" }
                }
                Pass {
                    Tags { "LightMode" = "Always" }

                    CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #pragma fragmentoption ARB_precision_hint_fastest
                    #include "UnityCG.cginc"

                    struct appdata_t {
                        float4 vertex : POSITION;
                        float2 texcoord: TEXCOORD0;
                    };

                    struct v2f {
                        float4 vertex : POSITION;
                        float4 uvgrab : TEXCOORD0;
                        float2 uvbump : TEXCOORD1;
                        float2 uvmain : TEXCOORD2;
                    };

                    float _BumpAmt;
                    float4 _BumpMap_ST;
                    float4 _MainTex_ST;

                    v2f vert(appdata_t v) {
                        v2f o;
                        o.vertex = UnityObjectToClipPos(v.vertex);
                        #if UNITY_UV_STARTS_AT_TOP
                        float scale = -1.0;
                        #else
                        float scale = 1.0;
                        #endif
                        o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
                        o.uvgrab.zw = o.vertex.zw;
                        o.uvbump = TRANSFORM_TEX(v.texcoord, _BumpMap);
                        o.uvmain = TRANSFORM_TEX(v.texcoord, _MainTex);
                        return o;
                    }

                    fixed4 _Color;
                    sampler2D _GrabTexture;
                    float4 _GrabTexture_TexelSize;
                    sampler2D _BumpMap;
                    sampler2D _MainTex;
                    float4 _MainTex_TexelSize;

                    half4 frag(v2f i) : COLOR {
                        half4 tint = tex2D(_MainTex, i.uvmain) * _Color;
                        if (tint.a == 0)
                        {
                            return tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(i.uvgrab.x, i.uvgrab.y, i.uvgrab.z, i.uvgrab.w)));
                        }

                        #define GRABPIXEL(weight,kernely) tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(float4(i.uvgrab.x, i.uvgrab.y, i.uvgrab.z, i.uvgrab.w))) * weight

                        half2 bump = UnpackNormal(tex2D(_BumpMap, i.uvbump)).rg;
                        float2 offset = bump * _BumpAmt * _GrabTexture_TexelSize.xy;
                        i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;

                        half4 blur = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));

                        half4 mainTex = tex2D(_MainTex, i.uvmain);
                        half4 tintblur = blur * tint;
                        half4 col = half4(0, 0, 0, 0);
                        col.r = blur.r * (1 - mainTex.a) + blur.r * mainTex.a;
                        col.g = blur.g * (1 - mainTex.a) + blur.g * mainTex.a;
                        col.b = blur.b * (1 - mainTex.a) + blur.b * mainTex.a;
                        col.a = blur.a * (1 - mainTex.a) + blur.a * mainTex.a;
                        tint.r = tint.r * tint.a;
                        tint.g = tint.g * tint.a;
                        tint.b = tint.b * tint.a;
                        half4 final = (col * (1 - tint.a)) + (tint * tint.a);
                        return final;
                    }
                    ENDCG
                }
            }
        }
}