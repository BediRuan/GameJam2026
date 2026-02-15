using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    [Header("References")]
    [Tooltip("全屏黑色 Image（RectTransform 拉满屏幕）")]
    public Image fadeImage;

    [Tooltip("可选：如果你的 FadeImage 上有 CanvasGroup，可用于阻挡点击")]
    public CanvasGroup canvasGroup;

    [Header("Default")]
    [Tooltip("游戏开始时是否默认黑屏（alpha=1）")]
    public bool startBlack = true;

    [Tooltip("淡入淡出时是否阻挡点击（blocksRaycasts）")]
    public bool blockInputDuringFade = true;

    [Header("Safety")]
    [Tooltip("当完全透明时是否自动隐藏 fadeImage（节省Raycast & 避免误挡）")]
    public bool disableWhenClear = true;

    Coroutine _fadeCo;

    void Awake()
    {
        if (fadeImage == null)
        {
            Debug.LogError("[ScreenFader] fadeImage is missing.");
            enabled = false;
            return;
        }

        // 初始化 alpha，避免闪一下
        if (startBlack) SetBlackImmediate();
        else SetClearImmediate();
    }

    // ====== Public API ======

    public void SetBlackImmediate()
    {
        StopFadeIfRunning();
        SetAlpha(1f);
        SetActiveForAlpha(1f);
        SetBlockInput(true);
    }

    public void SetClearImmediate()
    {
        StopFadeIfRunning();
        SetAlpha(0f);
        SetBlockInput(false);

        if (disableWhenClear)
            fadeImage.gameObject.SetActive(false);
        else
            fadeImage.gameObject.SetActive(true);
    }

    /// <summary>透明 -> 黑</summary>
    public IEnumerator FadeIn(float seconds, bool realtime = true)
    {
        yield return Fade(0f, 1f, seconds, realtime);
    }

    /// <summary>黑 -> 透明</summary>
    public IEnumerator FadeOut(float seconds, bool realtime = true)
    {
        yield return Fade(1f, 0f, seconds, realtime);
    }

    /// <summary>任意 alpha -> 任意 alpha</summary>
    public IEnumerator Fade(float from, float to, float seconds, bool realtime = true)
    {
        StopFadeIfRunning();
        _fadeCo = StartCoroutine(FadeRoutine(from, to, seconds, realtime));
        yield return _fadeCo;
    }

    // ====== Internals ======

    void StopFadeIfRunning()
    {
        if (_fadeCo != null)
        {
            StopCoroutine(_fadeCo);
            _fadeCo = null;
        }
    }

    IEnumerator FadeRoutine(float from, float to, float seconds, bool realtime)
    {
        seconds = Mathf.Max(0.01f, seconds);

        SetActiveForAlpha(Mathf.Max(from, to));
        SetAlpha(from);

        // 开始时要不要挡输入
        if (blockInputDuringFade)
            SetBlockInput(true);

        float t = 0f;

        while (t < seconds)
        {
            t += realtime ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = Mathf.Clamp01(t / seconds);
            k = k * k * (3f - 2f * k);   // SmoothStep
            float a = Mathf.Lerp(from, to, k);

            SetAlpha(a);
            yield return null;
        }

        SetAlpha(to);

        // 淡出到透明：解除输入阻挡 & 可选隐藏
        if (to <= 0.0001f)
        {
            SetBlockInput(false);

            if (disableWhenClear)
                fadeImage.gameObject.SetActive(false);
        }
        else
        {
            // 黑屏结束：如果需要，继续挡输入（比如切场景中）
            if (blockInputDuringFade)
                SetBlockInput(true);
        }

        _fadeCo = null;
    }

    void SetActiveForAlpha(float alpha)
    {
        if (!fadeImage.gameObject.activeSelf)
            fadeImage.gameObject.SetActive(true);

        // 如果你希望 fadeImage 在完全透明也继续存在（但不挡输入），disableWhenClear=false 即可
    }

    void SetAlpha(float a)
    {
        Color c = fadeImage.color;
        fadeImage.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(a));

        // 同步 canvasGroup（可选）
        if (canvasGroup != null)
            canvasGroup.alpha = Mathf.Clamp01(a);
    }

    void SetBlockInput(bool block)
    {
        if (!canvasGroup) return;

        canvasGroup.blocksRaycasts = block;
        canvasGroup.interactable = block;
    }
}
