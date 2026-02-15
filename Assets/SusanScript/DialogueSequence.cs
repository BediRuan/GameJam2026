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

    public bool IsFinished { get; private set; } = false;

    [Header("UI References")]
    public GameObject panelRoot;
    public CanvasGroup panelGroup;
    public TMP_Text dialogueText;

    [Header("Start Condition")]
    public bool startDisabled = true;
    public Transform player;
    public LayerMask groundMask = ~0;
    public Vector2 groundCheckOffset = new Vector2(0f, -0.6f);
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.12f);

    [Header("Timing")]
    public float waitSecondsAfterGrounded = 0.8f;
    public float fadeInSeconds = 0.35f;

    [Header("Typewriter")]
    public float typeCharInterval = 0.03f;
    public bool allowSkipTyping = true;

    [Header("Freeze Rules")]
    public bool freezeWorld = true;
    public bool forceNoFreezeInLevel0 = true;

    [Header("Exit Safety (Fix Input Leak)")]
    public float unfreezeDelayRealtime = 0.06f;
    public int swallowFramesAfterUnfreeze = 1;

    [Header("Optional: Freeze Animator To Stop 1-Frame Flicker")]
    public Animator playerAnimator;
    float animSpeedBefore = 1f;

    // ✅ 句子播完事件：参数是当前句子的 index（0-based）
    public System.Action<int> OnLineFinished;

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

    public bool IsPlaying => isPlaying;
    public bool HasStarted => started;

    public void PlayNow()
    {
        if (started) return;
        BeginNow();
    }

    bool ShouldFreezeWorld()
    {
        if (forceNoFreezeInLevel0 && SceneManager.GetActiveScene().name == "SusanIntro")
            return false;
        return freezeWorld;
    }

    void Awake()
    {
        if (Time.timeScale == 0f) Time.timeScale = 1f;

        if (panelRoot != null) panelRoot.SetActive(false);
        if (panelGroup != null) panelGroup.alpha = 0f;
        if (dialogueText != null) dialogueText.text = "";
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
        IsFinished = false;

        if (started) return;
        started = true;

        if (beginCo != null) StopCoroutine(beginCo);
        beginCo = StartCoroutine(BeginDialogueFlow());
    }

    IEnumerator BeginDialogueFlow()
    {
        float wait = Mathf.Max(0f, waitSecondsAfterGrounded);
        if (wait > 0f)
            yield return new WaitForSeconds(wait);

        bool freeze = ShouldFreezeWorld();

        if (freeze)
        {
            Time.timeScale = 0f;

            if (playerAnimator != null)
            {
                animSpeedBefore = playerAnimator.speed;
                playerAnimator.speed = 0f;
            }
        }

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
        // ✅ 在进入下一句之前：认为“当前句播完”
        OnLineFinished?.Invoke(index);

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

        if (typingCo != null) StopCoroutine(typingCo);
        typingCo = null;
        isTyping = false;

        if (panelRoot != null) panelRoot.SetActive(false);

        if (endCo != null) StopCoroutine(endCo);
        endCo = StartCoroutine(EndDialogueRoutine());
    }

    IEnumerator EndDialogueRoutine()
    {
        bool freeze = ShouldFreezeWorld();

        float delay = Mathf.Max(0f, unfreezeDelayRealtime);
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        if (freeze)
        {
            Time.timeScale = 1f;

            if (playerAnimator != null)
                playerAnimator.speed = animSpeedBefore;
        }

        int frames = Mathf.Max(0, swallowFramesAfterUnfreeze);
        for (int i = 0; i < frames; i++)
            yield return null;

        IsFinished = true;
        Destroy(gameObject);
    }

    bool IsPlayerGrounded()
    {
        if (player == null) return false;

        Vector2 center = (Vector2)player.position + groundCheckOffset;
        Collider2D hit = Physics2D.OverlapBox(center, groundCheckSize, 0f, groundMask);
        return hit != null;
    }

    void OnDisable()
    {
        if (Time.timeScale == 0f) Time.timeScale = 1f;
        if (playerAnimator != null) playerAnimator.speed = animSpeedBefore;
    }

    void OnDestroy()
    {
        if (Time.timeScale == 0f) Time.timeScale = 1f;
        if (playerAnimator != null) playerAnimator.speed = animSpeedBefore;
    }
}
