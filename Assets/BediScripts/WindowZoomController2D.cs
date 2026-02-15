using UnityEngine;

public class WindowZoomAndWallsController2D : MonoBehaviour
{
    [System.Serializable]
    public class Preset
    {
        [Header("Label (Optional)")]
        public string name;

        [Header("Window Resolution (Standalone Windowed)")]
        public int width = 1280;
        public int height = 720;

        [Header("Camera")]
        [Tooltip("Orthographic Size to use for this preset")]
        public float cameraOrthoSize = 5f;

        [Header("Walls")]
        [Tooltip("Parent object that contains all colliders for this preset")]
        public GameObject wallsRoot;
    }

    [Header("References")]
    public Camera targetCamera;

    [Header("Presets (Press 1-4)")]
    public Preset[] presets = new Preset[4];

    [Header("Startup")]
    [Tooltip("0 = key '1', 1 = key '2' ...")]
    public int defaultIndex = 0;

    [Header("Options")]
    [Tooltip("Force windowed mode on start/apply")]
    public bool forceWindowed = true;

    [Tooltip("If true, will re-apply camera size & walls after resolution changes (some platforms apply SetResolution next frame).")]
    public bool reapplyNextFrame = true;

    int pendingReapply = -1;

    void Awake()
    {
        if (!targetCamera) targetCamera = Camera.main;
        if (forceWindowed) Screen.fullScreenMode = FullScreenMode.Windowed;
    }

    void Start()
    {
        ApplyPreset(defaultIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) ApplyPreset(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ApplyPreset(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ApplyPreset(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) ApplyPreset(3);
    }

    void LateUpdate()
    {
        // 有些情况下 SetResolution 需要下一帧才真正生效；这里补打一枪，保证 camera/walls 最终状态正确
        if (pendingReapply >= 0)
        {
            int idx = pendingReapply;
            pendingReapply = -1;
            ApplyCameraAndWallsOnly(idx);
        }
    }

    public void ApplyPreset(int index)
    {
        if (presets == null || presets.Length == 0) return;
        if (index < 0 || index >= presets.Length) return;

        if (forceWindowed) Screen.fullScreenMode = FullScreenMode.Windowed;

        // 1) 先禁用所有墙组
        for (int i = 0; i < presets.Length; i++)
        {
            if (presets[i] != null && presets[i].wallsRoot)
                presets[i].wallsRoot.SetActive(false);
        }

        // 2) 设置分辨率（Build里有效；Editor里窗口可能不变，但逻辑仍会跑）
        var p = presets[index];
        Screen.SetResolution(p.width, p.height, FullScreenMode.Windowed);

        // 3) 设置相机 & 启用对应墙组
        ApplyCameraAndWallsOnly(index);

        // 4) 下一帧再应用一次（更稳）
        if (reapplyNextFrame)
            pendingReapply = index;

        Debug.Log($"[Preset {index + 1}] {p.name} {p.width}x{p.height}, ortho={p.cameraOrthoSize}, walls={(p.wallsRoot ? p.wallsRoot.name : "NULL")}");
    }

    void ApplyCameraAndWallsOnly(int index)
    {
        if (index < 0 || index >= presets.Length) return;
        var p = presets[index];

        // Camera
        if (targetCamera && targetCamera.orthographic)
        {
            targetCamera.orthographicSize = p.cameraOrthoSize;
        }

        // Walls
        if (p.wallsRoot)
        {
            p.wallsRoot.SetActive(true);
        }
    }
}
