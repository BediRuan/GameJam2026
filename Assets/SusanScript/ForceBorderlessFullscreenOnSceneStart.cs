using System.Collections;
using UnityEngine;

public class ForceBorderlessFullscreenOnSceneStart : MonoBehaviour
{
    [Tooltip("有些机器切场景/改变模式需要下一帧才稳定；建议保持开启")]
    public bool applyNextFrameToo = true;

    void Awake()
    {
        Apply();
    }

    IEnumerator Start()
    {
        if (!applyNextFrameToo) yield break;

        // 等一帧，确保切场景后的窗口状态稳定
        yield return null;
        Apply();
    }

    void Apply()
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;

        // 一般不需要 SetResolution；无边框全屏通常会用桌面分辨率
        // 如果你“必须”固定成1080p，请看下面的可选方案

        Debug.Log($"[ForceFullscreen] mode={Screen.fullScreenMode}, current={Screen.width}x{Screen.height}");
    }
}