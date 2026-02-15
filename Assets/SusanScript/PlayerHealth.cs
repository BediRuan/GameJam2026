using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHp = 100;
    public int currentHp = 100;

    [Header("Damage Cooldown")]
    public float invincibleSeconds = 0.8f;

    [Header("Blink")]
    public float blinkInterval = 0.08f;

    public enum BlinkMode { ByDuration, ByCount }
    public BlinkMode blinkMode = BlinkMode.ByDuration;

    public float blinkDuration = 0.25f;   // 闪多久（可调）
    public int blinkToggles = 6;          // 闪几下开关（可调，6=可见/不可见切换6次）

    [Header("Blink Renderers")]
    public SpriteRenderer[] blinkRenderers;

    [Header("Regen Tick")]
    public bool regenEnabled = true;
    public int regenTickAmount = 3;
    public float regenTickInterval = 0.35f;
    public float regenPauseAfterDamage = 0.8f;

    public System.Action<int, int> onHpChanged;
    /// <summary> 受到伤害时触发，参数为伤害值。供 JiU 伤害数字等系统订阅。 </summary>
    public System.Action<int> onDamageTaken;

    bool isDead = false;

    float invincibleUntil = -999f;
    float nextRegenTime = 0f;
    float regenResumeAt = 0f;

    Coroutine blinkCo;

    void Awake()
    {
        // 如果你希望每次进关卡都满血
        // currentHp = maxHp;

        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        onHpChanged?.Invoke(currentHp, maxHp);

        if (blinkRenderers == null || blinkRenderers.Length == 0)
            blinkRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    void Update()
    {
        if (isDead) return;
        if (!regenEnabled) return;
        if (currentHp >= maxHp) return;

        if (Time.time < regenResumeAt) return;

        if (Time.time >= nextRegenTime)
        {
            int before = currentHp;
            currentHp = Mathf.Min(maxHp, currentHp + Mathf.Max(0, regenTickAmount));

            if (currentHp != before)
                onHpChanged?.Invoke(currentHp, maxHp);

            nextRegenTime = Time.time + Mathf.Max(0.01f, regenTickInterval);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        // 暂停时不受伤
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused) return;

        // 无敌期间不受伤
        if (Time.time < invincibleUntil) return;

        currentHp = Mathf.Max(0, currentHp - amount);
        onHpChanged?.Invoke(currentHp, maxHp);
        onDamageTaken?.Invoke(amount);

        invincibleUntil = Time.time + Mathf.Max(0f, invincibleSeconds);

        regenResumeAt = Time.time + Mathf.Max(0f, regenPauseAfterDamage);
        nextRegenTime = regenResumeAt + Mathf.Max(0.01f, regenTickInterval);

        StartHitBlink();

        if (currentHp <= 0)
            Die();
    }

    void StartHitBlink()
    {
        if (blinkCo != null) StopCoroutine(blinkCo);

        // 受伤后立刻保证可见，然后开始闪
        SetRenderersVisible(true);
        blinkCo = StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        if (blinkRenderers == null || blinkRenderers.Length == 0)
        {
            blinkCo = null;
            yield break;
        }

        bool visible = true;

        if (blinkMode == BlinkMode.ByDuration)
        {
            float endTime = Time.time + Mathf.Max(0f, blinkDuration);

            while (!isDead && Time.time < endTime)
            {
                visible = !visible;
                SetRenderersVisible(visible);
                yield return new WaitForSecondsRealtime(Mathf.Max(0.02f, blinkInterval));
            }
        }
        else
        {
            int toggles = Mathf.Max(0, blinkToggles);

            for (int i = 0; i < toggles && !isDead; i++)
            {
                visible = !visible;
                SetRenderersVisible(visible);
                yield return new WaitForSecondsRealtime(Mathf.Max(0.02f, blinkInterval));
            }
        }

        // 闪完后保持正常显示
        SetRenderersVisible(true);
        blinkCo = null;
    }

    void SetRenderersVisible(bool v)
    {
        if (blinkRenderers == null) return;
        for (int i = 0; i < blinkRenderers.Length; i++)
        {
            if (blinkRenderers[i] != null)
                blinkRenderers[i].enabled = v;
        }
    }

    public void HealInstant(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        int before = currentHp;
        currentHp = Mathf.Min(maxHp, currentHp + amount);

        if (currentHp != before)
            onHpChanged?.Invoke(currentHp, maxHp);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (blinkCo != null) StopCoroutine(blinkCo);
        SetRenderersVisible(false);

        StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        yield return new WaitForSecondsRealtime(0.25f);

        if (SceneFader.Instance != null)
            yield return SceneFader.Instance.FadeOut();
        else
            yield return new WaitForSecondsRealtime(0.15f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
