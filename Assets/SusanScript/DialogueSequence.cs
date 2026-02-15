using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DialogueSequence : MonoBehaviour
{
    [Header("Dialogue Lines (点 + 增加句子)")]
    [TextArea(2, 5)]
    public List<string> lines = new List<string>();

    [Header("UI References")]
    public GameObject panelRoot;      // 对话框 Image 那层
    public CanvasGroup panelGroup;    // 挂在 panelRoot 上的 CanvasGroup
    public TMP_Text dialogueText;     // TMP_Text

    [Header("Input")]
    //public KeyCode nextKey = KeyCode.Return; // Enter（主键盘）
    //public bool alsoAcceptKeypadEnter = true;

    [Header("Start Condition")]
    public bool startDisabled = true;           // 游戏开始不播，等落地
    public Transform player;                    // 可不填：自动找 tag=Player
    public LayerMask groundMask = ~0;           // 只勾 Ground 更稳
    public Vector2 groundCheckOffset = new Vector2(0f, -0.6f);
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.12f);

    [Header("Timing")]
    public float waitSecondsAfterGrounded = 0.8f; // ⭐落地后等（不冻结）
    public float fadeInSeconds = 0.35f;           // ⭐淡入

    [Header("Typewriter")]
    public float typeCharInterval = 0.03f;        // realtime 打字速度
    public bool allowSkipTyping = true;           // Enter 时若正在打字：直接显示整句

    [Header("Freeze Rules")]
    [Tooltip("是否冻结世界（Time.timeScale=0）。注意：Level0会强制不冻结。")]
    public bool freezeWorld = true;

    [Tooltip("BuildIndex==0 时强制不冻结（Level0开场展示镜头需要动）。")]
    public bool forceNoFreezeInLevel0 = true;

    [Header("Exit Safety (Fix Input Leak)")]
    public float unfreezeDelayRealtime = 0.06f;   // ⭐关框后等多久再恢复时间
    public int swallowFramesAfterUnfreeze = 1;    // ⭐恢复后吞几帧，防止Enter/Space穿透触发跳等

    [Header("Optional: Freeze Animator To Stop 1-Frame Flicker")]
    public Animator playerAnimator;               // 可不填；冻结期间 anim.speed=0
    float animSpeedBefore = 1f;

    [Header("Debug")]
    public bool drawGizmos = true;

    int index = 0;
    bool started = false;
    bool isPlaying = false;

    bool isTyping = false;
    string currentLineFull = "";

    Coroutine beginCo;
    Coroutine typingCo;
    Coroutine endCo;

    // ====== Public API (给Intro总控用) ======
    public bool IsPlaying => isPlaying;
    public bool HasStarted => started;

    public void PlayNow()
    {
        // 外部强制开始（不依赖落地触发）
        if (started) return;
        BeginNow();
    }

    public void PlayWithLines(List<string> newLines)
    {
        if (newLines != null) lines = newLines;
        PlayNow();
    }

    bool ShouldFreezeWorld()
    {
        if (forceNoFreezeInLevel0 && SceneManager.GetActiveScene().buildIndex == 0)
            return false;
        return freezeWorld;
    }

    void Awake()
    {
        // 保险：避免上次暂停没恢复
        if (Time.timeScale == 0f) Time.timeScale = 1f;

        if (panelRoot != null) panelRoot.SetActive(false);
        if (panelGroup != null) panelGroup.alpha = 0f;

        if (dialogueText != null)
            dialogueText.text = "";
    }

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (!startDisabled)
            BeginNow();
    }

    void Update()
    {
        // 还没开始：等落地触发一次
        if (!started && startDisabled)
        {
            if (IsPlayerGrounded())
                BeginNow();
            return;
        }

        if (!isPlaying) return;

        bool nextPressed = Input.GetKeyDown(KeyCode.Return)
                || Input.GetKeyDown(KeyCode.KeypadEnter);


        if (!nextPressed) return;

        if (isTyping)
        {
            if (allowSkipTyping) FinishTypingInstant();
            return;
        }

        NextLine();
    }

    void BeginNow()
    {
        if (started) return;
        started = true;

        if (beginCo != null) StopCoroutine(beginCo);
        beginCo = StartCoroutine(BeginDialogueFlow());
    }

    IEnumerator BeginDialogueFlow()
    {
        // 1) 落地后先等（不冻结，玩家可动）
        float wait = Mathf.Max(0f, waitSecondsAfterGrounded);
        if (wait > 0f)
            yield return new WaitForSeconds(wait); // 正常时间（timeScale=1）

        bool freeze = ShouldFreezeWorld();

        // 2) 可选：冻结世界
        if (freeze)
        {
            Time.timeScale = 0f;

            // 可选：冻结玩家动画
            if (playerAnimator != null)
            {
                animSpeedBefore = playerAnimator.speed;
                playerAnimator.speed = 0f;
            }
        }

        // 3) 打开对话框，淡入（淡入永远用 realtime）
        if (panelRoot != null) panelRoot.SetActive(true);
        if (dialogueText != null) dialogueText.text = "";

        if (panelGroup != null)
        {
            panelGroup.alpha = 0f;
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;

            float dur = Mathf.Max(0.01f, fadeInSeconds);
            float t = 0f;

            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                panelGroup.alpha = Mathf.Clamp01(t / dur);
                yield return null;
            }

            panelGroup.alpha = 1f;
            panelGroup.interactable = true;
            panelGroup.blocksRaycasts = true;
        }

        // 4) 淡入完才开始播放文字（打字）
        StartDialogueContent();

        beginCo = null;
    }

    void StartDialogueContent()
    {
        if (lines == null || lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        isPlaying = true;
        index = 0;
        PlayLine(index);
    }

    void NextLine()
    {
        index++;
        if (index >= lines.Count)
        {
            EndDialogue();
            return;
        }
        PlayLine(index);
    }

    void PlayLine(int i)
    {
        if (dialogueText == null) return;

        if (typingCo != null) StopCoroutine(typingCo);

        currentLineFull = lines[i] ?? "";
        typingCo = StartCoroutine(TypeRoutine(currentLineFull));
    }

    IEnumerator TypeRoutine(string full)
    {
        isTyping = true;

        dialogueText.text = "";
        float interval = Mathf.Max(0.001f, typeCharInterval);

        for (int c = 0; c < full.Length; c++)
        {
            dialogueText.text += full[c];
            yield return new WaitForSecondsRealtime(interval);
        }

        isTyping = false;
        typingCo = null;
    }

    void FinishTypingInstant()
    {
        if (dialogueText == null) return;

        if (typingCo != null) StopCoroutine(typingCo);
        typingCo = null;

        dialogueText.text = currentLineFull;
        isTyping = false;
    }

    void EndDialogue()
    {
        if (!isPlaying) return;
        isPlaying = false;

        // 停掉打字
        if (typingCo != null) StopCoroutine(typingCo);
        typingCo = null;
        isTyping = false;

        // 先让对话框消失
        if (panelRoot != null) panelRoot.SetActive(false);

        // 开始安全退出流程（延迟恢复 + 吞键）
        if (endCo != null) StopCoroutine(endCo);
        endCo = StartCoroutine(EndDialogueRoutine());
    }

    IEnumerator EndDialogueRoutine()
    {
        bool freeze = ShouldFreezeWorld();

        // 关框后等一点 realtime，再恢复时间（避免键穿透）
        float delay = Mathf.Max(0f, unfreezeDelayRealtime);
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        if (freeze)
        {
            // 恢复时间
            Time.timeScale = 1f;

            // 恢复玩家动画
            if (playerAnimator != null)
                playerAnimator.speed = animSpeedBefore;

            // 吞几帧（让最后一次Enter/Space不会被“恢复的第一帧”读到）
            int frames = Mathf.Max(0, swallowFramesAfterUnfreeze);
            for (int i = 0; i < frames; i++)
                yield return null;
        }
        else
        {
            // 不冻结模式（Level0）：也可以吞几帧，防止下一帧触发别的输入逻辑
            int frames = Mathf.Max(0, swallowFramesAfterUnfreeze);
            for (int i = 0; i < frames; i++)
                yield return null;
        }

        Destroy(gameObject);
    }

    bool IsPlayerGrounded()
    {
        if (player == null) return false;

        Vector2 center = (Vector2)player.position + groundCheckOffset;
        Collider2D hit = Physics2D.OverlapBox(center, groundCheckSize, 0f, groundMask);
        return hit != null;
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        if (player == null) return;

        Gizmos.color = Color.yellow;
        Vector2 center = (Vector2)player.position + groundCheckOffset;
        Gizmos.DrawWireCube(center, groundCheckSize);
    }

    void OnDisable()
    {
        // 双保险：防止被禁用导致卡在暂停
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;

        if (playerAnimator != null)
            playerAnimator.speed = animSpeedBefore;
    }

    void OnDestroy()
    {
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;

        if (playerAnimator != null)
            playerAnimator.speed = animSpeedBefore;
    }
}
