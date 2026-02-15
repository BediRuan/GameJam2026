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
Shader "Hidden/Fronkon Games/Spice Up/Shake URP"
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
      Name "Fronkon Games Spice Up Shake"

      HLSLPROGRAM
      #include "SpiceUp.hlsl"

      #pragma vertex SpiceUpVert
      #pragma fragment SpiceUpFrag
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma exclude_renderers d3d9 d3d11_9x ps3 flash

      float _Progress;
      float2 _Direction;
      float _Zoom;
      float _ShakeStrength;
      float _ShakeFrequency;
      float3 _ShakeAberration;
      float _ShakeDistort;
      float _NoiseStrength;
      float _NoiseFrequency;

      // By Sam Hocevar and Emil Persson.
      float3 RGBtoHCV(float3 rgb)
      {
        const float4 p = (rgb.g < rgb.b) ? float4(rgb.bg, -1.0, 2.0 / 3.0) : float4(rgb.gb, 0.0, -1.0 / 3.0);
        const float4 q = (rgb.r < p.x) ? float4(p.xyw, rgb.r) : float4(rgb.r, p.yzx);
        const float c = q.x - min(q.w, q.y);
        const float h = abs((q.w - q.y) / (6.0 * c + 1e-10) + q.z);

        return float3(h, c, q.x);
      }

      half4 SpiceUpFrag(SpiceUpVaryings input) : SV_Target
      {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
        float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord).xy;

        const half4 color = SAMPLE_MAIN(uv);
        half4 pixel = color;

        const float t = smoothstep(0.0, 1.0, sin(_Progress * PI));

        uv -= 0.5;
        uv *= 1.0 - ((_Zoom * _ShakeStrength) * t);
        uv += 0.5;

        const float2 shake = _Direction * (_ShakeStrength * sin(_ShakeFrequency * 2.0 * PI * _Progress) * (_Progress + 0.5) * _Progress * _Progress * (_Progress - 1.0) * (_Progress - 1.0) * 15.5);
        uv += shake;

        const float noise = _NoiseStrength * _ShakeStrength * sin(_NoiseFrequency * 2.0 * PI * _Progress) * -_Progress * (_Progress - 1.0);
        uv += noise * float2(-_Direction.y, _Direction.x) * 2.0;

        const float H = RGBtoHCV(color.rgb).z;
        uv.x += _Direction.x * H * t * _ShakeDistort * TEXEL_SIZE.x;
        uv.y += _Direction.y * H * smoothstep(0.0, 0.75, cos(_Progress * PI)) * _ShakeDistort * TEXEL_SIZE.y;

        pixel.r = SAMPLE_MAIN(uv + (TEXEL_SIZE.xy * _ShakeAberration.x * shake)).r;
        pixel.g = SAMPLE_MAIN(uv + (TEXEL_SIZE.xy * _ShakeAberration.y * shake)).g;
        pixel.b = SAMPLE_MAIN(uv + (TEXEL_SIZE.xy * _ShakeAberration.z * shake)).b;

        // Color adjust.
        pixel.rgb = ColorAdjust(pixel.rgb, _Contrast, _Brightness, _Hue, _Gamma, _Saturation);

        return lerp(color, pixel, _Intensity);
      }

      ENDHLSL
    }
  }
  
  FallBack "Diffuse"
}
