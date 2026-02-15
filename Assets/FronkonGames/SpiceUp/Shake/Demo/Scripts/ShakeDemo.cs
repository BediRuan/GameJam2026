using System;
using UnityEngine;
using FronkonGames.SpiceUp.Shake;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(ShakeDemo))]
public class ShakeWarning : Editor
{
  private GUIStyle Style => style ??= new GUIStyle(GUI.skin.GetStyle("HelpBox")) { richText = true, fontSize = 14, alignment = TextAnchor.MiddleCenter };
  private GUIStyle style;
  public override void OnInspectorGUI() =>
    EditorGUILayout.TextArea($"\nThis code is only for the demo\n\n<b>DO NOT USE</b> it in your projects\n\nIf you have any questions,\ncheck the <a href='{Constants.Support.Documentation}'>online help</a> or use the <a href='mailto:{Constants.Support.Email}'>support email</a>,\n<b>thanks!</b>\n", Style);
}
#endif

/// <summary> Spice Up: Shake demo. </summary>
/// <remarks>
/// This code is designed for a simple demo, not for production environments.
/// </remarks>
public class ShakeDemo : MonoBehaviour
{
  private Shake.Settings settings;

  private GUIStyle styleTitle;
  private GUIStyle styleLabel;
  private GUIStyle styleButton;

  private void Awake()
  {
    if (Shake.IsInRenderFeatures() == false)
    {
      Debug.LogWarning($"Effect '{Constants.Asset.Name}' not found. You must add it as a Render Feature.");
#if UNITY_EDITOR
      if (EditorUtility.DisplayDialog($"Effect '{Constants.Asset.Name}' not found", $"You must add '{Constants.Asset.Name}' as a Render Feature.", "Quit") == true)
        EditorApplication.isPlaying = false;
#endif
    }

    this.enabled = Shake.IsInRenderFeatures();
  }

  private void Start()
  {
    settings = Shake.Instance.settings;
    ResetDemo();
  }

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
      GUILayout.BeginVertical("box", GUILayout.Width(300.0f), GUILayout.Height(Screen.height));
      {
        const float space = 10.0f;

        GUILayout.Space(space);

        GUILayout.Label("SHAKE DEMO", styleTitle);

        GUILayout.Space(space * 2.0f);

        settings.intensity = Slider("Intensity", settings.intensity);

        GUILayout.Space(space);

        settings.duration = Slider("Duration", settings.duration, 0.0f, 2.0f);
        settings.zoom = Slider("Zoom", settings.zoom, 0.0f, 2.0f);

        GUILayout.Space(space);

        settings.shakeStrength = Slider("Shake", settings.shakeStrength);
        settings.shakeFrequency = Slider("  Frequency", settings.shakeFrequency, 0.0f, 20.0f);
        settings.shakeDistort = Slider("  Distort", settings.shakeDistort);

        GUILayout.Space(space);

        settings.noiseStrength = Slider("Noise", settings.noiseStrength);
        settings.noiseFrequency = Slider("  Frequency", settings.noiseFrequency, 0.0f, 20.0f);

        GUILayout.Space(space * 2.0f);

        if (GUILayout.Button("SHAKE", styleButton) == true)
        {
          settings.direction = UnityEngine.Random.insideUnitCircle.normalized;
          Shake.Start();
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("RESET", styleButton) == true)
        {
          settings.ResetDefaultValues();
          settings.intensity = 1.0f;
        }

        GUILayout.Space(4.0f);

        if (GUILayout.Button("ONLINE DOCUMENTATION", styleButton) == true)
          Application.OpenURL(Constants.Support.Documentation);

        GUILayout.Space(4.0f);

        if (GUILayout.Button("❤️ LEAVE A REVIEW ❤️", styleButton) == true)
          Application.OpenURL(Constants.Support.Store);
      }
      GUILayout.EndVertical();

      GUILayout.FlexibleSpace();
    }
    GUILayout.EndHorizontal();
  }

  private void OnDestroy()
  {
    settings?.ResetDefaultValues();
  }

  private void ResetDemo()
  {
    settings.ResetDefaultValues();
    settings.intensity = 1.0f;
  }

  private bool Toggle(string label, bool value)
  {
    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      value = GUILayout.Toggle(value, string.Empty);
    }
    GUILayout.EndHorizontal();

    return value;
  }

  private float Slider(string label, float value, float min = 0.0f, float max = 1.0f)
  {
    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      value = GUILayout.HorizontalSlider(value, min, max);
    }
    GUILayout.EndHorizontal();

    return value;
  }

  private int Slider(string label, int value, int min, int max)
  {
    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      value = (int)GUILayout.HorizontalSlider(value, min, max);
    }
    GUILayout.EndHorizontal();

    return value;
  }

  private Color Color(string label, Color value, bool alpha = true)
  {
    GUILayout.BeginHorizontal();
    {
      GUILayout.Label(label, styleLabel);

      float originalAlpha = value.a;

      UnityEngine.Color.RGBToHSV(value, out float h, out float s, out float v);
      h = GUILayout.HorizontalSlider(h, 0.0f, 1.0f);
      value = UnityEngine.Color.HSVToRGB(h, s, v);

      if (alpha == false)
        value.a = originalAlpha;
    }
    GUILayout.EndHorizontal();

    return value;
  }

  private Vector3 Vector3(string label, Vector3 value, string x = "X", string y = "Y", string z = "Z", float min = 0.0f, float max = 1.0f)
  {
    GUILayout.Label(label, styleLabel);

    value.x = Slider($"   {x}", value.x, min, max);
    value.y = Slider($"   {y}", value.y, min, max);
    value.z = Slider($"   {z}", value.z, min, max);

    return value;
  }

  private T Enum<T>(string label, T value) where T : Enum
  {
    string[] names = System.Enum.GetNames(typeof(T));
    Array values = System.Enum.GetValues(typeof(T));
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
