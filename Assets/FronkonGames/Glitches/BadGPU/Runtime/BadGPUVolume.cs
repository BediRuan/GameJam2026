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
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FronkonGames.Glitches.BadGPU
{
  /// <summary> BadGPU Volume. </summary>
  [Serializable, VolumeComponentMenu("Fronkon Games/Glitches/BadGPU")]
  public sealed class BadGPUVolume : VolumeComponent, IPostProcessComponent
  {
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Common settings.

    /// <summary> Controls the intensity of the effect [0, 1]. Default 1. </summary>
    /// <remarks> An effect with Intensity equal to 0 will not be executed. </remarks>
    [FloatSliderWithReset(1.0f, 0.0f, 1.0f, "Controls the intensity of the effect [0, 1]. Default 1.")]
    public ClampedFloatParameter intensity = new(1.0f, 0.0f, 1.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Bad Signal settings.

    /// <summary> Balance between original image (<0) and effect (>0) [-1, 1]. Default 0. </summary>
    [FloatSliderWithReset(0.0f, -1.0f, 1.0f, "Balance between original image (<0) and effect (>0) [-1, 1]. Default 0.")]
    public ClampedFloatParameter balance = new(0.0f, -1.0f, 1.0f);

    /// <summary> Enhances intensity and brightness [0, 10]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 10.0f, "Enhances intensity and brightness [0, 10]. Default 1.")]
    public ClampedFloatParameter booster = new(1.0f, 0.0f, 10.0f);

    /// <summary> Color blend operation. Default Solid. </summary>
    [EnumDropdown((int)ColorBlends.Solid, "Color blend operation. Default Solid.")]
    public EnumParameter<ColorBlends> blend = new(ColorBlends.Solid);

    /// <summary> Color tint. Default White. </summary>
    public ColorParameter tint = new(Color.white);

    /// <summary> Intensity of the first glitch [0, 1]. Default 0.1. </summary>
    [FloatSliderWithReset(0.1f, 0.0f, 1.0f, "Intensity of the first glitch [0, 1]. Default 0.1.")]
    public ClampedFloatParameter glitch1 = new(0.1f, 0.0f, 1.0f);

    /// <summary> Speed of the first glitch [0, 10]. Default 0.15. </summary>
    [FloatSliderWithReset(0.15f, 0.0f, 10.0f, "Speed of the first glitch [0, 10]. Default 0.15.")]
    public ClampedFloatParameter glitch1Speed = new(0.15f, 0.0f, 10.0f);

    /// <summary> Threshold of the first glitch [0, 1]. Default 0.7. </summary>
    [FloatSliderWithReset(0.7f, 0.0f, 1.0f, "Threshold of the first glitch [0, 1]. Default 0.7.")]
    public ClampedFloatParameter glitch1Threshold = new(0.7f, 0.0f, 1.0f);

    /// <summary> Intensity of the second glitch [0, 1]. Default 0.1. </summary>
    [FloatSliderWithReset(0.1f, 0.0f, 1.0f, "Intensity of the second glitch [0, 1]. Default 0.1.")]
    public ClampedFloatParameter glitch2 = new(0.1f, 0.0f, 1.0f);

    /// <summary> Speed of the second glitch [0, 10]. Default 3. </summary>
    [FloatSliderWithReset(3.0f, 0.0f, 10.0f, "Speed of the second glitch [0, 10]. Default 3.")]
    public ClampedFloatParameter glitch2Speed = new(3.0f, 0.0f, 10.0f);

    /// <summary> Threshold of the second glitch [0, 1]. Default 0.9. </summary>
    [FloatSliderWithReset(0.9f, 0.0f, 1.0f, "Threshold of the second glitch [0, 1]. Default 0.9.")]
    public ClampedFloatParameter glitch2Threshold = new(0.9f, 0.0f, 1.0f);

    /// <summary> Intensity of the third glitch [0, 1]. Default 0.1. </summary>
    [FloatSliderWithReset(0.1f, 0.0f, 1.0f, "Intensity of the third glitch [0, 1]. Default 0.1.")]
    public ClampedFloatParameter glitch3 = new(0.1f, 0.0f, 1.0f);

    /// <summary> Speed of the third glitch [0, 10]. Default 0.2. </summary>
    [FloatSliderWithReset(0.2f, 0.0f, 10.0f, "Speed of the third glitch [0, 10]. Default 0.2.")]
    public ClampedFloatParameter glitch3Speed = new(0.2f, 0.0f, 10.0f);

    /// <summary> Threshold of the third glitch [0, 1]. Default 0.62. </summary>
    [FloatSliderWithReset(0.62f, 0.0f, 1.0f, "Threshold of the third glitch [0, 1]. Default 0.62.")]
    public ClampedFloatParameter glitch3Threshold = new(0.62f, 0.0f, 1.0f);

    /// <summary> Intensity of the fourth glitch [0, 1]. Default 0.1. </summary>
    [FloatSliderWithReset(0.1f, 0.0f, 1.0f, "Intensity of the fourth glitch [0, 1]. Default 0.1.")]
    public ClampedFloatParameter glitch4 = new(0.1f, 0.0f, 1.0f);

    /// <summary> Speed of the fourth glitch [0, 10]. Default 0.03. </summary>
    [FloatSliderWithReset(0.03f, 0.0f, 10.0f, "Speed of the fourth glitch [0, 10]. Default 0.03.")]
    public ClampedFloatParameter glitch4Speed = new(0.03f, 0.0f, 10.0f);

    /// <summary> Threshold of the fourth glitch [0, 1]. Default 0.99. </summary>
    [FloatSliderWithReset(0.99f, 0.0f, 1.0f, "Threshold of the fourth glitch [0, 1]. Default 0.99.")]
    public ClampedFloatParameter glitch4Threshold = new(0.99f, 0.0f, 1.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Color settings.

    /// <summary> Brightness [-1, 1]. Default 0. </summary>
    [FloatSliderWithReset(0.0f, -1.0f, 1.0f, "Brightness [-1, 1]. Default 0.")]
    public ClampedFloatParameter brightness = new(0.0f, -1.0f, 1.0f);

    /// <summary> Contrast [0, 10]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 10.0f, "Contrast [0, 10]. Default 1.")]
    public ClampedFloatParameter contrast = new(1.0f, 0.0f, 10.0f);

    /// <summary> Gamma [0.1, 10]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.1f, 10.0f, "Gamma [0.1, 10]. Default 1.")]
    public ClampedFloatParameter gamma = new(1.0f, 0.1f, 10.0f);

    /// <summary> The color wheel [0, 1]. Default 0. </summary>
    [FloatSliderWithReset(0.0f, 0.0f, 1.0f, "The color wheel [0, 1]. Default 0.")]
    public ClampedFloatParameter hue = new(0.0f, 0.0f, 1.0f);

    /// <summary> Intensity of a colors [0, 2]. Default 1. </summary>
    [FloatSliderWithReset(1.0f, 0.0f, 2.0f, "Intensity of a colors [0, 2]. Default 1.")]
    public ClampedFloatParameter saturation = new(1.0f, 0.0f, 2.0f);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Advanced settings.

    /// <summary> Does it affect the Scene View? </summary>
    [ToggleWithReset(false, "Does it affect the Scene View?")]
    public BoolParameter affectSceneView = new(false);

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary> Reset to default values. </summary> 
    public void Reset()
    {
      intensity.value = 1.0f;

      balance.value = 0.0f;
      booster.value = 1.0f;
      blend.value = ColorBlends.Solid;
      tint.value = Color.white;
      glitch1.value = glitch2.value = glitch3.value = glitch4.value = 0.1f;
      glitch1Speed.value = 0.15f;
      glitch2Speed.value = 3.0f;
      glitch3Speed.value = 0.2f;
      glitch4Speed.value = 0.03f;
      glitch1Threshold.value = 0.7f;
      glitch2Threshold.value = 0.9f;
      glitch3Threshold.value = 0.62f;
      glitch4Threshold.value = 0.99f;

      brightness.value = 0.0f;
      contrast.value = 1.0f;
      gamma.value = 1.0f;
      hue.value = 0.0f;
      saturation.value = 1.0f;

      affectSceneView.value = false;
    }

    /// <summary> Is the effect active? </summary>
    public bool IsActive() => intensity.overrideState == true && intensity.value > 0.0f;

    /// <summary> Is the effect tile compatible? </summary>
    public bool IsTileCompatible() => false;
  }
}
