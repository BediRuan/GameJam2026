using UnityEngine;
using UnityEngine.SceneManagement;

public class Level3_FirstDamageDialogue : MonoBehaviour
{
    [Header("Only trigger in this scene")]
    public string sceneName = "level3"; // 你说就叫 level3

    [Header("Dialogue")]
    public DialogueSequence dialogue;   // 拖“第二段对话”的 DialogueSequence 组件

    bool triggered = false;
    PlayerHealth hp;

    void Awake()
    {
        // 只在 level3 生效（忽略大小写，避免你场景名是 Level3 这种）
        string cur = SceneManager.GetActiveScene().name;
        if (!string.Equals(cur, sceneName, System.StringComparison.OrdinalIgnoreCase))
        {
            enabled = false;
            return;
        }
    }

    void OnEnable()
    {
        triggered = false;

        hp = FindFirstObjectByType<PlayerHealth>();
        if (hp == null)
        {
            Debug.LogError("[Level3_FirstDamageDialogue] PlayerHealth not found in scene.");
            return;
        }

        hp.OnDamaged += OnPlayerDamaged;
        Debug.Log("[Level3_FirstDamageDialogue] Subscribed to PlayerHealth.OnDamaged");
    }

    void OnDisable()
    {
        if (hp != null) hp.OnDamaged -= OnPlayerDamaged;
    }

    void OnPlayerDamaged()
    {
        if (triggered) return;
        triggered = true;

        if (dialogue == null)
        {
            Debug.LogError("[Level3_FirstDamageDialogue] dialogue is NULL. Did you drag DialogueSequence into inspector?");
            return;
        }

        // 保险：防止它自己“落地自动播”
        dialogue.startDisabled = true;
        dialogue.autoStartWhenGrounded = false;

        Debug.Log("[Level3_FirstDamageDialogue] First damage -> Play dialogue");
        dialogue.PlayNow();
    }
}
