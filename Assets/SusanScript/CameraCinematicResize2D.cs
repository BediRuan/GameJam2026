using System.Collections;
using UnityEngine;

public class CameraCinematicResize2D : MonoBehaviour
{
    public Camera cam;

    void Awake()
    {
        if (!cam) cam = GetComponent<Camera>();
    }

    // 平滑切到更宽镜头，并且“左下角锁定在目标点”（可带向上偏移）
    public IEnumerator WidenKeepBottomLeft(
        float targetOrthoSize,
        float extraUpWorld,
        float seconds,
        bool realtime = true
    )
    {
        if (!cam || !cam.orthographic) yield break;

        float z = -cam.transform.position.z;

        // 初始左下角
        Vector3 bl0 = cam.ViewportToWorldPoint(new Vector3(0f, 0f, z));

        float startSize = cam.orthographicSize;

        // ✅ 目标左下角（把向上偏移直接算进来）
        Vector3 blTarget = bl0 + new Vector3(0f, extraUpWorld, 0f);

        seconds = Mathf.Max(0.01f, seconds);
        float t = 0f;

        while (t < seconds)
        {
            t += realtime ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = Mathf.Clamp01(t / seconds);
            k = k * k * (3f - 2f * k); // SmoothStep


            // size 插值
            cam.orthographicSize = Mathf.Lerp(startSize, targetOrthoSize, k);

            // 每帧都算当前左下角
            Vector3 blNow = cam.ViewportToWorldPoint(new Vector3(0f, 0f, z));

            // 每帧都把左下角对齐到目标（所以不会最后一下跳）
            Vector3 delta = blTarget - blNow;
            cam.transform.position += new Vector3(delta.x, delta.y, 0f);

            yield return null;
        }

        // ✅ 结束帧：再对齐一次（极小误差），不会突兀
        cam.orthographicSize = targetOrthoSize;

        Vector3 blEnd = cam.ViewportToWorldPoint(new Vector3(0f, 0f, z));
        Vector3 endDelta = blTarget - blEnd;
        cam.transform.position += new Vector3(endDelta.x, endDelta.y, 0f);
    }
}
