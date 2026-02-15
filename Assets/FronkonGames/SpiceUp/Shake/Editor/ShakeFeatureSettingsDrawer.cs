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
using UnityEditor;
using System.Collections;
using static FronkonGames.SpiceUp.Shake.Inspector;

namespace FronkonGames.SpiceUp.Shake
{
  /// <summary> Spice Up Shake inspector. </summary>
  [CustomPropertyDrawer(typeof(Shake.Settings))]
  public class ShakeFeatureSettingsDrawer : Drawer
  {
    private Shake.Settings settings;

    private IEnumerator coroutine;

    protected override void ResetValues() => settings?.ResetDefaultValues();

    protected override void InspectorGUI()
    {
      settings ??= GetSettings<Shake.Settings>();

      /////////////////////////////////////////////////
      // Common.
      /////////////////////////////////////////////////
      settings.intensity = Slider("Intensity", "Controls the intensity of the effect [0, 1]. Default 0.", settings.intensity, 0.0f, 1.0f, 0.0f);

      /////////////////////////////////////////////////
      // Shake.
      /////////////////////////////////////////////////
      Separator();

      settings.direction = Vector2Field("Direction", "Direction of shake. Default (0, 1).", settings.direction, Vector2.up);
      settings.duration = Slider("Duration", "Duration of one shake [0.0, 2.0]. Default 0.5.", settings.duration, 0.0f, 2.0f, 0.5f);
      settings.zoom = Slider("Zoom", "Zoom during the shake [0.0, 2.0]. Default 0.2.", settings.zoom, 0.0f, 2.0f, 0.2f);

      settings.shakeStrength = Slider("Shake", "Shake force [0.0, 1.0]. Default 0.1.", settings.shakeStrength, 0.0f, 1.0f, 0.1f);
      IndentLevel++;
      settings.shakeFrequency = Slider("Frequency", "Frequency of movements [0.0, 20.0]. Default 5.", settings.shakeFrequency, 0.0f, 20.0f, 5.0f);
      settings.shakeDistort = Slider("Distort", "Image distortion [0.0, 1.0]. Default 0.05.", settings.shakeDistort, 0.0f, 1.0f, 0.05f);
      settings.shakeAberration = Vector3Field("Aberration", "Shifting color channels [0.0, 20.0]. Default 2.", settings.shakeAberration, Shake.Settings.DefaultAberration);
      IndentLevel--;

      settings.noiseStrength = Slider("Noise", "Noise force perpendicular to the direction of travel [0.0, 1.0]. Default 0.2.", settings.noiseStrength, 0.0f, 1.0f, 0.2f);
      IndentLevel++;
      settings.noiseFrequency = Slider("Frequency", "Frequency of noise movement[0.0, 20.0]. Default 2.", settings.noiseFrequency, 0.0f, 20.0f, 2.0f);
      IndentLevel--;

      Separator();

      BeginHorizontal();
      {
        Separator(LabelWidth);

        GUI.enabled = EditorApplication.isPlaying == false && settings.intensity > 0.0f && settings.shakeStrength > 0.0f;

        if (GUILayout.Button("Test") == true)
        {
          if (coroutine != null)
            CoroutineManager.Stop(coroutine);

          coroutine = CoroutineManager.Start(TestShake());
        }

        GUI.enabled = true;
      }
      EndHorizontal();

      /////////////////////////////////////////////////
      // Color.
      /////////////////////////////////////////////////
      Separator();

      if (Foldout("Color") == true)
      {
        IndentLevel++;

        settings.brightness = Slider("Brightness", "Brightness [-1.0, 1.0]. Default 0.", settings.brightness, -1.0f, 1.0f, 0.0f);
        settings.contrast = Slider("Contrast", "Contrast [0.0, 10.0]. Default 1.", settings.contrast, 0.0f, 10.0f, 1.0f);
        settings.gamma = Slider("Gamma", "Gamma [0.1, 10.0]. Default 1.", settings.gamma, 0.01f, 10.0f, 1.0f);
        settings.hue = Slider("Hue", "The color wheel [0.0, 1.0]. Default 0.", settings.hue, 0.0f, 1.0f, 0.0f);
        settings.saturation = Slider("Saturation", "Intensity of a colors [0.0, 2.0]. Default 1.", settings.saturation, 0.0f, 2.0f, 1.0f);

        IndentLevel--;
      }

      /////////////////////////////////////////////////
      // Advanced.
      /////////////////////////////////////////////////
      Separator();

      if (Foldout("Advanced") == true)
      {
        IndentLevel++;

#if !UNITY_6000_0_OR_NEWER
        settings.filterMode = (FilterMode)EnumPopup("Filter mode", "Filter mode. Default Bilinear.", settings.filterMode, FilterMode.Bilinear);
#endif
        settings.affectSceneView = Toggle("Affect the Scene View?", "Does it affect the Scene View?", settings.affectSceneView);
        settings.whenToInsert = (UnityEngine.Rendering.Universal.RenderPassEvent)EnumPopup("RenderPass event",
          "Render pass injection. Default BeforeRenderingPostProcessing.",
          settings.whenToInsert,
          UnityEngine.Rendering.Universal.RenderPassEvent.BeforeRenderingPostProcessing);
#if !UNITY_6000_0_OR_NEWER
        settings.enableProfiling = Toggle("Enable profiling", "Enable render pass profiling", settings.enableProfiling);
#endif

        IndentLevel--;
      }
    }

    private IEnumerator TestShake()
    {
      float time = 0.0f;

      settings.progress = 0.0f;

      while (time < settings.duration)
      {
        settings.progress = time / settings.duration;
        time += CoroutineManager.DeltaTime();

        yield return null;
      }

      settings.progress = 0.0f;
      coroutine = null;
    }
  }
}
