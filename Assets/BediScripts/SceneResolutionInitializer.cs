using UnityEngine;

public class SceneResolutionInitializer : MonoBehaviour
{
    public enum ResolutionMode
    {
        WindowedCustom,
        FullscreenWindow,
        ExclusiveFullscreen
    }

    [Header("Resolution Mode For This Scene")]
    public ResolutionMode mode = ResolutionMode.WindowedCustom;

    [Header("If WindowedCustom")]
    public int width = 1280;
    public int height = 720;

    [Header("Optional Camera Size Override")]
    public bool overrideCameraSize = false;
    public float cameraOrthoSize = 5f;

    public Camera targetCamera;

    void Awake()
    {
        if (!targetCamera)
            targetCamera = Camera.main;

        ApplyResolution();
    }

    void ApplyResolution()
    {
        switch (mode)
        {
            case ResolutionMode.WindowedCustom:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                Screen.SetResolution(width, height, FullScreenMode.Windowed);
                break;

            case ResolutionMode.FullscreenWindow:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;

            case ResolutionMode.ExclusiveFullscreen:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
        }

        if (overrideCameraSize && targetCamera && targetCamera.orthographic)
        {
            targetCamera.orthographicSize = cameraOrthoSize;
        }

        Debug.Log($"[SceneResolution] Mode={mode}, Size={width}x{height}");
    }
}
