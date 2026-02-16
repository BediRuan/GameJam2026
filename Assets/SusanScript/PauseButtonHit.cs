using System.Collections;
using UnityEngine;
using UnityEngine.Rendering; // ✅ Volume 在这里
using UnityEngine.SceneManagement; // ✅ 记得加在顶部


public class PauseButtonHit : MonoBehaviour
{
    public float requireRelativeUpVelocity = 0.2f;
    public float requireBelowTolerance = 0.02f;

    public float bumpUp = 0.18f;
    public float bumpTime = 0.08f;
    public float hitAlpha = 0.6f;

    [Header("Per-Level Dialogue (One-shot)")]
    public bool enableDialogueOnVolumeOn = true;

    // 你第二段对话的Canvas（或对话根物体）
    [Header("Dialogue To Trigger")]
    public DialogueSequence dialogueToPlay; // ✅ 拖第二段 DialogueSequence 组件进来


    bool _dialoguePlayedThisScene = false;
    int _cachedSceneBuildIndex = -1;

    [Tooltip("If empty, will use current scene name as key.")]
    public string levelKeyOverride = "";




    [Header("Sprite Swap (Pause ⇄ Run)")]
    [Tooltip("未暂停时显示的按钮图（留空则用物体当前 Sprite）")]
    public Sprite pauseButtonSprite;
    [Tooltip("暂停时显示的按钮图（顶击后切换）")]
    public Sprite runButtonSprite;
    [Tooltip("顶击时是否根据暂停状态切换 Sprite")]
    public bool swapSpriteOnTrigger = true;

    [Header("Volume Weight Toggle")]
    public Volume targetVolume;         // ✅ 拖你的 Volume 进来
    public float weightWhenOn = 1f;      // 默认切到 1
    public float weightWhenOff = 0f;     // 默认切回 0

    SpriteRenderer sr;
    Color startColor;
    Vector3 startPos;
    Coroutine bumpCo;

    Collider2D myCol;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            startColor = sr.color;
            if (pauseButtonSprite == null)
                pauseButtonSprite = sr.sprite;
        }

        myCol = GetComponent<Collider2D>();
        startPos = transform.position;
    }

    void Update()
    {
        EnsureSceneCache();
}
    void Start()
    {
        SyncSpriteToPauseState();
    }

    /// <summary> 根据当前暂停状态显示对应 Sprite（暂停=运行按钮，未暂停=暂停按钮）。 </summary>
    void SyncSpriteToPauseState()
    {
        if (sr == null || !swapSpriteOnTrigger) return;
        bool paused = PauseManager.Instance != null && PauseManager.Instance.IsPaused;
        sr.sprite = paused ? runButtonSprite : pauseButtonSprite;
        if (sr.sprite == null) sr.sprite = pauseButtonSprite;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        bool movingUpIntoButton = collision.relativeVelocity.y > requireRelativeUpVelocity;

        bool playerIsBelow = true;
        if (myCol != null)
        {
            float playerTop = collision.collider.bounds.max.y;
            float buttonBottom = myCol.bounds.min.y;
            playerIsBelow = playerTop <= buttonBottom + requireBelowTolerance;
        }

        if (!movingUpIntoButton || !playerIsBelow) return;

        if (bumpCo != null) StopCoroutine(bumpCo);
        bumpCo = StartCoroutine(BumpRoutine());

        ToggleVolumeWeight();

        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.TogglePause();
            if (swapSpriteOnTrigger)
                SyncSpriteToPauseState();
        }
        else
        {
            Debug.LogError("PauseManager.Instance is null");
        }
    }

    void ToggleVolumeWeight()
    {
        if (targetVolume == null) return;

        bool isOff = targetVolume.weight <= 0.001f;
        bool turningOn = isOff;

        targetVolume.weight = isOff ? weightWhenOn : weightWhenOff;

        if (turningOn)
            TryPlayDialogueOnceThisScene();
    }



    IEnumerator BumpRoutine()
    {
        startPos = transform.position;
        Vector3 upPos = startPos + Vector3.up * bumpUp;

        if (sr != null)
        {
            sr.enabled = true;
            Color c = startColor;
            c.a = hitAlpha;
            sr.color = c;
        }

        float t = 0f;
        while (t < bumpTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / bumpTime);
            transform.position = Vector3.Lerp(startPos, upPos, k);
            yield return null;
        }

        t = 0f;
        while (t < bumpTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / bumpTime);
            transform.position = Vector3.Lerp(upPos, startPos, k);
            yield return null;
        }

        transform.position = startPos;

        if (sr != null)
            sr.color = startColor;
    }

    void TryPlayDialogueOnceThisScene()
    {
        EnsureSceneCache();
        if (!enableDialogueOnVolumeOn) return;
        if (_dialoguePlayedThisScene) return;

        _dialoguePlayedThisScene = true;

        if (dialogueToPlay == null)
        {
            Debug.LogWarning("[PauseButtonHit] dialogueToPlay is NULL. Did you drag DialogueSequence into inspector?");
            return;
        }

        // ✅ 关键：由 DialogueSequence 自己负责打开 panelRoot、打字、冻结等
        dialogueToPlay.PlayNow();
    }



    void EnsureSceneCache()
    {
        int cur = SceneManager.GetActiveScene().buildIndex;
        if (_cachedSceneBuildIndex != cur)
        {
            _cachedSceneBuildIndex = cur;
            _dialoguePlayedThisScene = false; // ✅ 进入/切换到新关卡时，重置“第一次”
        }
    }


}
