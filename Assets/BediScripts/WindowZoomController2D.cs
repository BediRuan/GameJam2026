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

    /// <summary> ??????????? ApplyPreset ?????????????? </summary>
    public int CurrentPresetIndex { get; private set; }

    /// <summary> ????????????????? </summary>
    public int PresetCount => presets != null ? presets.Length : 0;

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
        // ???????? SetResolution ??????????????????????????????? camera/walls ?????????
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

        // 1) ????????????
        for (int i = 0; i < presets.Length; i++)
        {
            if (presets[i] != null && presets[i].wallsRoot)
                presets[i].wallsRoot.SetActive(false);
        }

        // 2) ?????????Build???????Editor?????????????????????
        var p = presets[index];
        Screen.SetResolution(p.width, p.height, FullScreenMode.Windowed);

        // 3) ??????? & ?????????
        ApplyCameraAndWallsOnly(index);

        // 4) ??????????????????
        if (reapplyNextFrame)
            pendingReapply = index;

        CurrentPresetIndex = index;
        Debug.Log($"[Preset {index + 1}] {p.name} {p.width}x{p.height}, ortho={p.cameraOrthoSize}, walls={(p.wallsRoot ? p.wallsRoot.name : "NULL")}");
    }

    /// <summary> ???????????+1???????????????????? </summary>
    public void ApplyNextPreset()
    {
        if (PresetCount == 0) return;
        int next = Mathf.Min(CurrentPresetIndex + 1, PresetCount - 1);
        if (next != CurrentPresetIndex)
            ApplyPreset(next);
    }

    /// <summary> ???????????-1??????????????????? </summary>
    public void ApplyPreviousPreset()
    {
        if (PresetCount == 0) return;
        int prev = Mathf.Max(CurrentPresetIndex - 1, 0);
        if (prev != CurrentPresetIndex)
            ApplyPreset(prev);
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
