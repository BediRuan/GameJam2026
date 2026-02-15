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
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FronkonGames.SpiceUp.Shake
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Settings. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class Shake
  {
    /// <summary> Settings. </summary>
    [Serializable]
    public sealed class Settings
    {
      public Settings() => ResetDefaultValues();

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Common settings.

      /// <summary> Controls the intensity of the effect [0, 1]. Default 0. </summary>
      /// <remarks> An effect with Intensity equal to 0 will not be executed. </remarks>
      public float intensity = 0.0f;
      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Shake settings.

      /// <summary> Direction of shake. Default (0, 1). </summary>
      public Vector2 direction = Vector2.up;

      /// <summary> Duration of one shake [0.0, 2.0]. Default 0.5. </summary>
      public float duration = 0.5f;

      /// <summary> Zoom during the shake [0.0, 2.0]. Default 0.2. </summary>
      public float zoom = 0.2f;

      /// <summary> Shake force [0.0, 1.0]. Default 0.1. </summary>
      public float shakeStrength = 0.1f;

      /// <summary> Frequency of movements [0.0, 20.0]. Default 5. </summary>
      public float shakeFrequency = 5.0f;

      /// <summary> Shifting color channels [0.0, 20.0]. Default 2. </summary>
      public Vector3 shakeAberration = DefaultAberration;

      /// <summary> Image distortion [0.0, 1.0]. Default 0.05. </summary>
      public float shakeDistort = 0.05f;

      /// <summary> Noise force perpendicular to the direction of travel [0.0, 1.0]. Default 0.2. </summary>
      public float noiseStrength = 0.2f;

      /// <summary> Frequency of noise movement[0.0, 20.0]. Default 2. </summary>
      public float noiseFrequency = 2.0f;

      /// <summary> Internal use [0.0, 1.0]. Default 0. </summary>
      public float progress = 0.0f;

      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Color settings.

      /// <summary> Brightness [-1.0, 1.0]. Default 0. </summary>
      public float brightness = 0.0f;

      /// <summary> Contrast [0.0, 10.0]. Default 1. </summary>
      public float contrast = 1.0f;

      /// <summary>Gamma [0.1, 10.0]. Default 1. </summary>
      public float gamma = 1.0f;

      /// <summary> The color wheel [0.0, 1.0]. Default 0. </summary>
      public float hue = 0.0f;

      /// <summary> Intensity of a colors [0.0, 2.0]. Default 1. </summary>
      public float saturation = 1.0f;
      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Advanced settings.

      /// <summary> Does it affect the Scene View? </summary>
      public bool affectSceneView = false;

#if !UNITY_6000_0_OR_NEWER
      /// <summary> Enable render pass profiling. </summary>
      public bool enableProfiling = false;

      /// <summary> Filter mode. Default Bilinear. </summary>
      public FilterMode filterMode = FilterMode.Bilinear;
#endif

      /// <summary> Render pass injection. Default BeforeRenderingPostProcessing. </summary>
      public RenderPassEvent whenToInsert = RenderPassEvent.BeforeRenderingPostProcessing;
      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      public static Vector3 DefaultAberration = new(2.0f, 4.0f, 6.0f);

      /// <summary> Reset to default values. </summary>
      public void ResetDefaultValues()
      {
        intensity = 0.0f;

        direction = Vector2.up;
        duration = 0.5f;
        zoom = 0.2f;
        shakeStrength = 0.1f;
        shakeFrequency = 5.0f;
        shakeAberration = DefaultAberration;
        shakeDistort = 0.05f;
        noiseStrength = 0.2f;
        noiseFrequency = 2.0f;

        brightness = 0.0f;
        contrast = 1.0f;
        gamma = 1.0f;
        hue = 0.0f;
        saturation = 1.0f;

        affectSceneView = false;
#if !UNITY_6000_0_OR_NEWER
        enableProfiling = false;
        filterMode = FilterMode.Bilinear;
#endif
        whenToInsert = RenderPassEvent.BeforeRenderingPostProcessing;
      }
    }
  }
}
