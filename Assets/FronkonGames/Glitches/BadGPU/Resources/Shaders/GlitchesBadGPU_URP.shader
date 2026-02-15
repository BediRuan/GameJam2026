////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Martin Bustos @FronkonGames <fronkongames@gmail.com>. All rights reserved.
//
// THIS FILE CAN NOT BE HOSTED IN PUBLIC REPOSITORIES.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
Shader "Hidden/Fronkon Games/Glitches/Bad GPU URP"
{
  Properties
  {
    _MainTex("Main Texture", 2D) = "white" {}
  }

  SubShader
  {
    Tags
    {
      "RenderType" = "Opaque"
      "RenderPipeline" = "UniversalPipeline"
    }
    LOD 100
    ZTest Always ZWrite Off Cull Off

    Pass
    {
      Name "Fronkon Games Glitches Bad GPU Pass"

      HLSLPROGRAM
      #pragma vertex GlitchesVert
      #pragma fragment GlitchesFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash
      #pragma multi_compile_instancing
      #pragma multi_compile _ STEREO_INSTANCING_ON
      #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_MULTIVIEW_ON

      #include "Glitches.hlsl"
      #include "ColorBlend.hlsl"

      float _Balance;
      float _Booster;
      int _Blend;
      float3 _Tint;
      float _Glitch_1;
      float _GlitchSpeed_1;
      float _GlitchThreshold_1;
      float _Glitch_2;
      float _GlitchSpeed_2;
      float _GlitchThreshold_2;
      float _Glitch_3;
      float _GlitchSpeed_3;
      float _GlitchThreshold_3;
      float _Glitch_4;
      float _GlitchSpeed_4;
      float _GlitchThreshold_4;

      // 0b0000dcba -> 0b0d0c0b0a
      uint SpreadBits(uint x)
      {
        x &= 0x0000ffffu;
        x = (x ^ (x <<  8u)) & 0x00ff00ffu;
        x = (x ^ (x <<  4u)) & 0x0f0f0f0fu;
        x = (x ^ (x <<  2u)) & 0x33333333u;
        x = (x ^ (x <<  1u)) & 0x55555555u;

        return x;
      }

      //0b0d0c0b0a -> 0b0000dcba
      uint GatherBits(uint x)
      {
        x &= 0x55555555u;
        x = (x ^ (x >>  1u)) & 0x33333333u;
        x = (x ^ (x >>  2u)) & 0x0f0f0f0fu;
        x = (x ^ (x >>  4u)) & 0x00ff00ffu;
        x = (x ^ (x >>  8u)) & 0x0000ffffu;
        
        return x;
      }

      // 0bhgfedcba -> (0b0000geca, 0b0000hfdb)
      inline float2 MortonToFloat2(uint morton)
      {
        return float2(GatherBits(morton >> 0), GatherBits(morton >> 1));
      }

      // (0b0000dcba, 0b0000hgfe) -> 0bhdgcfbea
      inline uint UvToMorton(float2 uv)
      {
        return SpreadBits(uv.x) | (SpreadBits(uv.y) << 1);
      }

      inline float Hash11(float p, float seed)
      {
        return frac(sin(p) * 999999.9999 + seed * 1.61803398875);
      }

      // By Dave Hoskins.
      float3 Hash31(float p)
      {
        float3 p3 = frac((float3)p * float3(0.1031, 0.1030, 0.0973));
        p3 += dot(p3, p3.yzx + 33.33);

        return frac((p3.xxy + p3.yzz) * p3.zyx); 
      }

      float Noise(float u, float size, float seed)
      {
        float zoom = u * size;
        float index = floor(zoom);
        float progress = frac(zoom);

        return lerp(Hash11(index, seed), Hash11(index + 1.0, seed), smoothstep(0.0, 1.0, progress));
      }

      inline float Posterize(float u, float steps)
      {
        return floor(u * steps + 0.5) / steps;
      }

      inline float Threshold(float u, float edge)
      {
        return u * step(edge, u);
      }

      half4 GlitchesFrag(GlitchesVaryings input) : SV_Target
      {
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        const float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord).xy;
        const float2 coord = uv * _ScreenParams.xy;

        const half4 color = SAMPLE_MAIN(uv);
        half4 pixel = color;

        float i = float(UvToMorton(coord));

        float n1 = Noise(i, 1e-3, floor(_Time.y * _GlitchSpeed_1));
        n1 = Posterize(n1, 4.0);
        n1 = Threshold(n1, _GlitchThreshold_1);
        n1 *= 40.0 * _Glitch_1;

        float n2 = Noise(i, 1e-5, floor(_Time.y * _GlitchSpeed_2));
        n2 = Posterize(n2, 20.0);
        n2 = Threshold(n2, _GlitchThreshold_2);
        n2 *= 100.0 * _Glitch_2;

        float n3 = Noise(i, 1e3, floor(_Time.y * _GlitchSpeed_3));
        n3 = Threshold(n3, _GlitchThreshold_3);
        n3 *= 100.0 * _Glitch_3;

        float n4 = Noise(i, 0.01, floor(_Time.y * _GlitchSpeed_4));
        n4 = Threshold(n4, _GlitchThreshold_4);
        n4 *= 10.0 * _Glitch_4;

        i += n1 + n2 + n3;

        float2 morton = (float2)MortonToFloat2((uint)i) / _ScreenParams.xy;
        pixel.rgb = lerp(SAMPLE_MAIN(morton).rgb, Hash31(i), step(Hash11(floor(_Time.y), 1.0) * 0.1 + 0.88, n4));

        float separation = distance(pixel.rgb, color.rgb);

        pixel.rgb = lerp(color.rgb, ColorBlend(_Blend, color.rgb, pixel.rgb * _Tint * _Booster * separation), saturate(separation + _Balance));

        pixel.rgb = ColorAdjust(pixel.rgb, _Contrast, _Brightness, _Hue, _Gamma, _Saturation);

// Do not use ;)
#if 0
        pixel.rgb = PixelDemo(color.rgb, pixel.rgb, uv);
#endif
        return lerp(color, pixel, _Intensity);
      }

      ENDHLSL
    }
  }
  
  FallBack "Diffuse"
}
