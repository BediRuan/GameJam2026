using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SeamlessLevelMusic : MonoBehaviour
{
    public static SeamlessLevelMusic Instance { get; private set; }

    [Header("Database")]
    public MusicVariantDatabase database;

    [Header("Audio")]
    public AudioSource source;

    [Tooltip("切歌时淡出/淡入（秒）。想更“无缝”可设为 0。")]
    public float crossFadeTime = 0.08f;

    [Tooltip("如果不同版本长度不一致，用归一化进度映射。")]
    public bool useNormalizedProgressIfLengthDiffers = true;

    int _lastTimeSamples;
    float _lastNormalized; // 0~1
    string _currentScene;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (source == null) source = GetComponent<AudioSource>();
        if (source == null) source = gameObject.AddComponent<AudioSource>();

        source.loop = true; // 通常 BGM 循环
        SceneManager.sceneLoaded += OnSceneLoaded;
        _currentScene = SceneManager.GetActiveScene().name;
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        // 持续记录进度（即使你不手动调用切歌）
        if (source != null && source.clip != null && source.isPlaying)
        {
            _lastTimeSamples = source.timeSamples;
            _lastNormalized = Mathf.Clamp01(source.time / Mathf.Max(0.0001f, source.clip.length));
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var nextScene = scene.name;
        if (database == null) return;

        var nextClip = database.GetClipForScene(nextScene);
        if (nextClip == null) return;

        // 如果同一关重复加载，或 clip 不变，可以直接不处理
        if (source.clip == nextClip && source.isPlaying)
        {
            _currentScene = nextScene;
            return;
        }

        StopAllCoroutines();
        StartCoroutine(SwapClipSeamlessly(nextClip));

        _currentScene = nextScene;
    }

    IEnumerator SwapClipSeamlessly(AudioClip nextClip)
    {
        // 1) 记录当前进度（多一层保险）
        int prevSamples = _lastTimeSamples;
        float prevNorm = _lastNormalized;
        var prevClip = source.clip;

        // 2) 可选淡出
        float originalVol = source.volume;
        if (crossFadeTime > 0f && source.isPlaying)
        {
            for (float t = 0; t < crossFadeTime; t += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(originalVol, 0f, t / crossFadeTime);
                yield return null;
            }
            source.volume = 0f;
        }

        // 3) 换 clip
        source.Stop();
        source.clip = nextClip;

        // 4) 对齐时间
        if (prevClip != null && prevClip.frequency == nextClip.frequency && prevClip.samples == nextClip.samples)
        {
            // 最理想：完全同规格（无缝）
            source.timeSamples = Mathf.Clamp(prevSamples, 0, nextClip.samples - 1);
        }
        else if (useNormalizedProgressIfLengthDiffers)
        {
            // 不同长度：用归一化进度映射
            float targetTime = prevNorm * nextClip.length;
            source.time = Mathf.Clamp(targetTime, 0f, Mathf.Max(0f, nextClip.length - 0.02f));
        }
        else
        {
            // 退而求其次：尽量用 samples（可能会略偏）
            source.timeSamples = Mathf.Clamp(prevSamples, 0, nextClip.samples - 1);
        }

        // 5) 开始播放
        source.Play();

        // 6) 可选淡入
        if (crossFadeTime > 0f)
        {
            for (float t = 0; t < crossFadeTime; t += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(0f, originalVol, t / crossFadeTime);
                yield return null;
            }
        }
        source.volume = originalVol;
    }
}
