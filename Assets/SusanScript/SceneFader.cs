using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    [Header("Assign in Inspector")]
    public Image fadeImage;

    [Header("Fade Settings")]
    public float fadeSeconds = 0.25f;

    void Awake()
    {
        // 单例 + 跨场景保留
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator FadeOut() => Fade(0f, 1f);
    public IEnumerator FadeIn() => Fade(1f, 0f);

    IEnumerator Fade(float from, float to)
    {
        if (!fadeImage) yield break;

        Color c = fadeImage.color;
        float t = 0f;

        // 确保起始alpha正确
        c.a = from;
        fadeImage.color = c;

        while (t < fadeSeconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeSeconds);
            c.a = Mathf.Lerp(from, to, k);
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;
    }
}
