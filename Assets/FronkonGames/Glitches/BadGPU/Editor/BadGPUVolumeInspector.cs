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
using UnityEditor;

namespace FronkonGames.Glitches.BadGPU.Editor
{
  /// <summary> BadGPU inspector. </summary>
  [CustomEditor(typeof(BadGPUVolume))]
  public class BadGPUVolumeInspector : Inspector
  {
    protected override void InspectorGUI()
    {
      /////////////////////////////////////////////////
      // Common.
      /////////////////////////////////////////////////
      DrawFloatSliderWithReset("intensity");

      /////////////////////////////////////////////////
      // Bad Signal.
      /////////////////////////////////////////////////
      Separator();

      DrawFloatSliderWithReset("balance");
      DrawFloatSliderWithReset("booster");
      DrawEnumDropdownWithReset("blend", "Blend", ColorBlends.Solid);
      DrawColorWithReset("tint", "Tint", UnityEngine.Color.white);

      Separator();

      DrawFloatSliderWithReset("glitch1", "Glitch 1");
      IndentLevel++;
      DrawFloatSliderWithReset("glitch1Speed", "Speed");
      DrawFloatSliderWithReset("glitch1Threshold", "Threshold");
      IndentLevel--;

      DrawFloatSliderWithReset("glitch2", "Glitch 2");
      IndentLevel++;
      DrawFloatSliderWithReset("glitch2Speed", "Speed");
      DrawFloatSliderWithReset("glitch2Threshold", "Threshold");
      IndentLevel--;

      DrawFloatSliderWithReset("glitch3", "Glitch 3");
      IndentLevel++;
      DrawFloatSliderWithReset("glitch3Speed", "Speed");
      DrawFloatSliderWithReset("glitch3Threshold", "Threshold");
      IndentLevel--;

      DrawFloatSliderWithReset("glitch4", "Glitch 4");
      IndentLevel++;
      DrawFloatSliderWithReset("glitch4Speed", "Speed");
      DrawFloatSliderWithReset("glitch4Threshold", "Threshold");
      IndentLevel--;
    }

    protected override void ResetValues() => ((BadGPUVolume)target).Reset();

    protected override void CheckForErrors()
    {
      if (BadGPU.IsInAnyRenderFeatures() == false)
      {
        Separator();

        EditorGUILayout.HelpBox($"Renderer Feature '{Constants.Asset.Name}' not found. You must add it as a Render Feature.", MessageType.Error);
      }
      else
      {
        BadGPU[] effects = BadGPU.Instances;

        bool anyEnabled = false;
        for (int i = 0; i < effects.Length; i++)
        {
          if (effects[i].isActive == true)
          {
            anyEnabled = true;
            break;
          }
        }

        if (anyEnabled == false)
        {
          Separator();

          EditorGUILayout.HelpBox($"No Renderer Feature '{Constants.Asset.Name}' is active. You must activate it in the Render Features.", MessageType.Warning);
        }
      }
    }
  }
}
