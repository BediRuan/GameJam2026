using UnityEngine;

public class WindowZoomController2D : MonoBehaviour
{
    [System.Serializable]
    public class ResolutionPreset
    {
        public int width = 1280;
        public int height = 720;
        public float cameraSize = 5f;
    }

    public Camera targetCamera;

    [Header("Resolution Presets (Press 1-4)")]
    public ResolutionPreset[] presets = new ResolutionPreset[4];

    void Start()
    {
        if (!targetCamera)
            targetCamera = Camera.main;

        Screen.fullScreenMode = FullScreenMode.Windowed;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            ApplyPreset(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            ApplyPreset(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            ApplyPreset(2);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            ApplyPreset(3);
    }

    void ApplyPreset(int index)
    {
        if (index >= presets.Length) return;

        var p = presets[index];

        Screen.SetResolution(p.width, p.height, FullScreenMode.Windowed);

        if (targetCamera && targetCamera.orthographic)
        {
            targetCamera.orthographicSize = p.cameraSize;
        }
    }
}