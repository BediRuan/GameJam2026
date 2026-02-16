using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
public class PanicWindowSequenceTrigger : MonoBehaviour
{
    [Header("Trigger")]
    public string playerTag = "Player";
    public bool triggerOnce = true;

    [Header("Phase 1: Shrink Window")]
    public Vector2Int shrunkResolution = new Vector2Int(960, 540);

    [Tooltip("缩窗时强制 Windowed 模式")]
    public bool forceWindowedOnShrink = true;

    [Header("Phase 2: WINDOW SHAKE (ramp up)")]
    [Tooltip("抖动持续时间（秒），强度从轻微到剧烈")]
    public float shakeDuration = 2f;

    [Tooltip("起始抖动幅度（像素）")]
    public float shakeAmplitudeStartPx = 2f;

    [Tooltip("结束抖动幅度（像素）")]
    public float shakeAmplitudeEndPx = 28f;

    [Tooltip("抖动频率（越大越快）")]
    public float shakeFrequency = 45f;

    [Tooltip("强度曲线：x=时间0~1, y=强度倍率")]
    public AnimationCurve shakeRamp = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Phase 3: Restore Borderless Fullscreen")]
    [Tooltip("抖动结束后切回无边框全屏")]
    public bool restoreToBorderlessFullscreen = true;

    [Header("Phase 4: Volume Weight 0->1")]
    public Volume targetVolume;
    public float volumeFadeDuration = 2f;

    [Header("Phase 5: Load Next Scene")]
    public string nextSceneName;

    [Header("Safety")]
    [Tooltip("缩窗后等待多久再开始读取窗口rect并抖动（避免rect暂时变0导致锁到左上角）")]
    public float waitAfterResizeSeconds = 0.05f;

    [Tooltip("抖动开始前先把窗口居中，避免跨屏/DPI造成奇怪偏移")]
    public bool centerWindowBeforeShake = true;

    private bool _triggered;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void Awake()
    {
        if (targetVolume) targetVolume.weight = 0f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerOnce && _triggered) return;
        if (!other.CompareTag(playerTag)) return;

        _triggered = true;
        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        // 1) shrink window
        if (forceWindowedOnShrink)
            Screen.fullScreenMode = FullScreenMode.Windowed;

        Screen.SetResolution(shrunkResolution.x, shrunkResolution.y, FullScreenMode.Windowed);

        // 2) shake window (ramp up)
        yield return StartCoroutine(ShakeWindowRamp(shakeDuration));

        // 3) restore borderless fullscreen
        if (restoreToBorderlessFullscreen)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }

        // 4) volume weight 0->1
        if (targetVolume)
        {
            targetVolume.weight = 0f;
            yield return StartCoroutine(FadeVolumeWeight(targetVolume, 0f, 1f, volumeFadeDuration));
        }
        else
        {
            yield return new WaitForSecondsRealtime(volumeFadeDuration);
        }

        // 5) load scene
        if (!string.IsNullOrWhiteSpace(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[PanicWindowShakeWin32Trigger] nextSceneName is empty. No scene loaded.");
        }
    }

    IEnumerator FadeVolumeWeight(Volume v, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / duration);
            v.weight = Mathf.Lerp(from, to, u);
            yield return null;
        }
        v.weight = to;
    }

    IEnumerator ShakeWindowRamp(float duration)
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        // ? 关键修复：SetResolution 后先等一帧 + 一点点时间，让系统完成窗口重排
        yield return null;
        if (waitAfterResizeSeconds > 0f)
            yield return new WaitForSecondsRealtime(waitAfterResizeSeconds);

        IntPtr hwnd = GetForegroundWindow(); // ? 比 GetActiveWindow 稳定
        if (hwnd == IntPtr.Zero)
        {
            Debug.LogWarning("[WindowShake] GetForegroundWindow failed.");
            yield return new WaitForSecondsRealtime(duration);
            yield break;
        }

        if (!GetWindowRect(hwnd, out RECT r))
        {
            Debug.LogWarning("[WindowShake] GetWindowRect failed.");
            yield return new WaitForSecondsRealtime(duration);
            yield break;
        }

        if (centerWindowBeforeShake)
        {
            CenterWindowOnMainDisplay(hwnd, r);
            // 重新获取居中后的 rect 作为基准
            GetWindowRect(hwnd, out r);
        }

        int baseX = r.Left;
        int baseY = r.Top;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / duration);

            float ramp = (shakeRamp != null) ? shakeRamp.Evaluate(u) : u;
            float amp = Mathf.Lerp(shakeAmplitudeStartPx, shakeAmplitudeEndPx, ramp);

            // Perlin 输出 -1~1，抖动更“自然”
            float nx = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0.123f) - 0.5f) * 2f;
            float ny = (Mathf.PerlinNoise(0.456f, Time.time * shakeFrequency) - 0.5f) * 2f;

            int x = baseX + Mathf.RoundToInt(nx * amp);
            int y = baseY + Mathf.RoundToInt(ny * amp);

            // 只移动位置，不改大小，不抢焦点
            SetWindowPos(hwnd, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);

            yield return null;
        }

        // 恢复原位
        SetWindowPos(hwnd, IntPtr.Zero, baseX, baseY, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
#else
        // 非 Windows：Unity 无通用 API 移动窗口，所以这里保持时间轴一致
        yield return new WaitForSecondsRealtime(duration);
#endif
    }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    // ---- Win32 ----
    private const int SWP_NOSIZE = 0x0001;
    private const int SWP_NOZORDER = 0x0004;
    private const int SWP_NOACTIVATE = 0x0010;

    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")] private static extern bool SetWindowPos(
        IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags
    );

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    private void CenterWindowOnMainDisplay(IntPtr hwnd, RECT r)
    {
        int w = r.Right - r.Left;
        int h = r.Bottom - r.Top;

        // 用 Unity 的主显示器系统分辨率做居中（够稳定、简单）
        int screenW = Display.main.systemWidth;
        int screenH = Display.main.systemHeight;

        int x = (screenW - w) / 2;
        int y = (screenH - h) / 2;

        SetWindowPos(hwnd, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
    }
#endif
}