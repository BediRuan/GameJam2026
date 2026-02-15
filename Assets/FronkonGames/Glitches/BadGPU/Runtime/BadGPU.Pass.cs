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
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

namespace FronkonGames.Glitches.BadGPU
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Render Pass. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class BadGPU
  {
    [DisallowMultipleRendererFeature]
    private sealed class RenderPass : ScriptableRenderPass
    {
      // Internal use only.
      internal Material material { get; set; }

      private BadGPUVolume volume;

      private static class ShaderIDs
      {
        internal static readonly int Intensity = Shader.PropertyToID("_Intensity");

        internal static readonly int Balance = Shader.PropertyToID("_Balance");
        internal static readonly int Booster = Shader.PropertyToID("_Booster");
        internal static readonly int Blend = Shader.PropertyToID("_Blend");
        internal static readonly int Tint = Shader.PropertyToID("_Tint");
        internal static readonly int Glitch1 = Shader.PropertyToID("_Glitch_1");
        internal static readonly int GlitchSpeed1 = Shader.PropertyToID("_GlitchSpeed_1");
        internal static readonly int GlitchThreshold1 = Shader.PropertyToID("_GlitchThreshold_1");
        internal static readonly int Glitch2 = Shader.PropertyToID("_Glitch_2");
        internal static readonly int GlitchSpeed2 = Shader.PropertyToID("_GlitchSpeed_2");
        internal static readonly int GlitchThreshold2 = Shader.PropertyToID("_GlitchThreshold_2");
        internal static readonly int Glitch3 = Shader.PropertyToID("_Glitch_3");
        internal static readonly int GlitchSpeed3 = Shader.PropertyToID("_GlitchSpeed_3");
        internal static readonly int GlitchThreshold3 = Shader.PropertyToID("_GlitchThreshold_3");
        internal static readonly int Glitch4 = Shader.PropertyToID("_Glitch_4");
        internal static readonly int GlitchSpeed4 = Shader.PropertyToID("_GlitchSpeed_4");
        internal static readonly int GlitchThreshold4 = Shader.PropertyToID("_GlitchThreshold_4");

        internal static readonly int Brightness = Shader.PropertyToID("_Brightness");
        internal static readonly int Contrast = Shader.PropertyToID("_Contrast");
        internal static readonly int Gamma = Shader.PropertyToID("_Gamma");
        internal static readonly int Hue = Shader.PropertyToID("_Hue");
        internal static readonly int Saturation = Shader.PropertyToID("_Saturation");
      }

      /// <summary> Render pass constructor. </summary>
      public RenderPass() : base()
      {
        profilingSampler = new ProfilingSampler(Constants.Asset.AssemblyName);
      }

      /// <summary> Destroy the render pass. </summary>
      ~RenderPass() => material = null;

      private void UpdateMaterial()
      {
        material.shaderKeywords = null;
        material.SetFloat(ShaderIDs.Intensity, volume.intensity.value);

        material.SetFloat(ShaderIDs.Balance, volume.balance.value);
        material.SetFloat(ShaderIDs.Booster, volume.booster.value * 100.0f);
        material.SetInt(ShaderIDs.Blend, (int)volume.blend.value);
        material.SetColor(ShaderIDs.Tint, volume.tint.value);
        material.SetFloat(ShaderIDs.Glitch1, volume.glitch1.value * 100.0f);
        material.SetFloat(ShaderIDs.GlitchSpeed1, volume.glitch1Speed.value);
        material.SetFloat(ShaderIDs.GlitchThreshold1, volume.glitch1Threshold.value);
        material.SetFloat(ShaderIDs.Glitch2, volume.glitch2.value * 100.0f);
        material.SetFloat(ShaderIDs.GlitchSpeed2, volume.glitch2Speed.value);
        material.SetFloat(ShaderIDs.GlitchThreshold2, volume.glitch2Threshold.value);
        material.SetFloat(ShaderIDs.Glitch3, volume.glitch3.value * 100.0f);
        material.SetFloat(ShaderIDs.GlitchSpeed3, volume.glitch3Speed.value);
        material.SetFloat(ShaderIDs.GlitchThreshold3, volume.glitch3Threshold.value);
        material.SetFloat(ShaderIDs.Glitch4, volume.glitch4.value * 100.0f);
        material.SetFloat(ShaderIDs.GlitchSpeed4, volume.glitch4Speed.value);
        material.SetFloat(ShaderIDs.GlitchThreshold4, volume.glitch4Threshold.value);

        material.SetFloat(ShaderIDs.Brightness, volume.brightness.value);
        material.SetFloat(ShaderIDs.Contrast, volume.contrast.value);
        material.SetFloat(ShaderIDs.Gamma, 1.0f / volume.gamma.value);
        material.SetFloat(ShaderIDs.Hue, volume.hue.value);
        material.SetFloat(ShaderIDs.Saturation, volume.saturation.value);
      }

      /// <inheritdoc/>
      public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
      {
        volume = VolumeManager.instance.stack.GetComponent<BadGPUVolume>();
        if (material == null || volume == null || volume.IsActive() == false)
          return;

        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        if (resourceData.isActiveTargetBackBuffer == true)
          return;

        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        if (cameraData.camera.cameraType == CameraType.SceneView && volume.affectSceneView.value == false || cameraData.postProcessEnabled == false)
          return;

        TextureHandle source = resourceData.activeColorTexture;
        TextureHandle destination = renderGraph.CreateTexture(source.GetDescriptor(renderGraph));

        UpdateMaterial();

        RenderGraphUtils.BlitMaterialParameters pass = new(source, destination, material, 0);
        renderGraph.AddBlitPass(pass, $"{Constants.Asset.AssemblyName}.Pass");

        resourceData.cameraColor = destination;
      }
    }
  }
}
