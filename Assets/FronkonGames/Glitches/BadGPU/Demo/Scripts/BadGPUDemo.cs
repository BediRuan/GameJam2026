using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace FronkonGames.Glitches.BadGPU
{
  /// <summary> Glitches: Bad GPU demo. </summary>
  /// <remarks>
  /// This code is designed for a simple demo, not for production environments.
  /// </remarks>
  public class BadGPUDemo : MonoBehaviour
  {
    [Header("This code is only for the demo, not for production environments.")]

    [Space(20.0f), SerializeField]
    private VolumeProfile volumeProfile;

    private BadGPUVolume volume;

    private GUIStyle styleTitle;
    private GUIStyle styleLabel;
    private GUIStyle styleButton;

    private void ResetEffect() => volume?.Reset();

    private void Awake()
    {
      if (BadGPU.IsInRenderFeatures() == false)
      {
        Debug.LogWarning($"Effect '{Constants.Asset.Name}' not found. You must add it as a Render Feature.");
#if UNITY_EDITOR
        if (UnityEditor.EditorUtility.DisplayDialog($"Effect '{Constants.Asset.Name}' not found", $"You must add '{Constants.Asset.Name}' as a Render Feature.", "Quit") == true)
          UnityEditor.EditorApplication.isPlaying = false;
#endif
      }

      volume = volumeProfile != null && volumeProfile.TryGet(out BadGPUVolume vol) ? vol : null;
      this.enabled = BadGPU.IsInRenderFeatures() && volume != null;
    }

    private void OnEnable() => ResetEffect();

    private void OnGUI()
    {
      styleTitle = new GUIStyle(GUI.skin.label)
      {
        alignment = TextAnchor.LowerCenter,
        fontSize = 32,
        fontStyle = FontStyle.Bold
      };

      styleLabel = new GUIStyle(GUI.skin.label)
      {
        alignment = TextAnchor.UpperLeft,
        fontSize = 24
      };

      styleButton = new GUIStyle(GUI.skin.button)
      {
        fontSize = 24
      };

      GUILayout.BeginHorizontal();
      {
        GUILayout.BeginVertical("box", GUILayout.Width(500.0f), GUILayout.Height(Screen.height));
        {
          const float space = 10.0f;

          GUILayout.Space(space);

          GUILayout.Label(Constants.Asset.Name.ToUpper(), styleTitle);

          GUILayout.Space(space);

          volume.intensity.value = SliderField("Intensity", volume.intensity.value);
          volume.balance.value = SliderField("Balance", volume.balance.value, -1.0f, 1.0f);
          volume.booster.value = SliderField("Booster", volume.booster.value, 0.0f, 10.0f);

          GUILayout.Space(space);

          volume.blend.value = EnumField("Blend", volume.blend.value);

          GUILayout.Space(space);

          volume.glitch1.value = SliderField("Glitch #1", volume.glitch1.value);
          volume.glitch1Speed.value = SliderField("  Speed", volume.glitch1Speed.value, 0.0f, 10.0f);
          volume.glitch1Threshold.value = SliderField("  Threshold", volume.glitch1Threshold.value);

          GUILayout.Space(space);

          volume.glitch2.value = SliderField("Glitch #2", volume.glitch2.value);
          volume.glitch2Speed.value = SliderField("  Speed", volume.glitch2Speed.value, 0.0f, 10.0f);
          volume.glitch2Threshold.value = SliderField("  Threshold", volume.glitch2Threshold.value);

          GUILayout.Space(space);

          volume.glitch3.value = SliderField("Glitch #3", volume.glitch3.value);
          volume.glitch3Speed.value = SliderField("  Speed", volume.glitch3Speed.value, 0.0f, 10.0f);
          volume.glitch3Threshold.value = SliderField("  Threshold", volume.glitch3Threshold.value);

          GUILayout.Space(space);

          volume.glitch4.value = SliderField("Glitch #4", volume.glitch4.value);
          volume.glitch4Speed.value = SliderField("  Speed", volume.glitch4Speed.value, 0.0f, 10.0f);
          volume.glitch4Threshold.value = SliderField("  Threshold", volume.glitch4Threshold.value);

          GUILayout.FlexibleSpace();

          if (GUILayout.Button("RESET", styleButton) == true)
            ResetEffect();

          GUILayout.Space(4.0f);

          if (GUILayout.Button("ONLINE DOCUMENTATION", styleButton) == true)
            Application.OpenURL(Constants.Support.Documentation);

          GUILayout.Space(4.0f);

          if (GUILayout.Button("❤️ LEAVE A REVIEW ❤️", styleButton) == true)
            Application.OpenURL(Constants.Support.Store);

          GUILayout.Space(space * 2.0f);
        }
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
      }
      GUILayout.EndHorizontal();
    }

    private void OnDestroy() => volume?.Reset();

    private bool ToggleField(string label, bool value)
    {
      GUILayout.BeginHorizontal();
      {
        GUILayout.Label(label, styleLabel);

        value = GUILayout.Toggle(value, string.Empty);
      }
      GUILayout.EndHorizontal();

      return value;
    }

    private float SliderField(string label, float value, float min = 0.0f, float max = 1.0f)
    {
      GUILayout.BeginHorizontal();
      {
        GUILayout.Label(label, styleLabel);

        value = GUILayout.HorizontalSlider(value, min, max);
      }
      GUILayout.EndHorizontal();

      return value;
    }

    private int SliderField(string label, int value, int min, int max)
    {
      GUILayout.BeginHorizontal();
      {
        GUILayout.Label(label, styleLabel);

        value = (int)GUILayout.HorizontalSlider(value, min, max);
      }
      GUILayout.EndHorizontal();

      return value;
    }

    private Color ColorField(string label, Color value, bool alpha = true)
    {
      GUILayout.BeginHorizontal();
      {
        GUILayout.Label(label, styleLabel);

        float originalAlpha = value.a;

        Color.RGBToHSV(value, out float h, out float s, out float v);
        h = GUILayout.HorizontalSlider(h, 0.0f, 1.0f);
        value = Color.HSVToRGB(h, s, v);

        if (alpha == false)
          value.a = originalAlpha;
      }
      GUILayout.EndHorizontal();

      return value;
    }

    private T EnumField<T>(string label, T value) where T : Enum
    {
      string[] names = Enum.GetNames(typeof(T));
      Array values = Enum.GetValues(typeof(T));
      int index = Array.IndexOf(values, value);

      GUILayout.BeginHorizontal();
      {
        GUILayout.Label(label, styleLabel);

        if (GUILayout.Button("<", styleButton) == true)
          index = index > 0 ? index - 1 : values.Length - 1;

        GUILayout.Label(names[index], styleLabel);

        if (GUILayout.Button(">", styleButton) == true)
          index = index < values.Length - 1 ? index + 1 : 0;
      }
      GUILayout.EndHorizontal();

      return (T)(object)index;
    }
  }
}
