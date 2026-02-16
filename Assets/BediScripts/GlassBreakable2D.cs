using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GlassBreakable2D : MonoBehaviour
{
    [Header("Break Conditions")]
    public string playerTag = "Player";
    [Range(0f, 1f)] public float volumeThreshold = 0.9f;

    [Tooltip("If true, volume-based breaking only happens when switch is ON (if you use VolumeSwitchState).")]
    public bool requireSwitchOnForVolumeBreak = true;

    [Header("Player Contact Break")]
    [Tooltip("OFF = player can stand/walk on it without breaking.")]
    public bool breakOnPlayerContact = false;

    [Tooltip("Only break on contact if impact is strong enough. (0 = ignore impact check)")]
    public float minImpactToBreak = 0f;

    [Header("Animation")]
    public Animator animator;
    public string breakTriggerName = "Break";
    public float disappearDelay = 0.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip breakSfx;
    public bool playOneShot = true;

    [Header("After Break")]
    public bool destroyAfterBreak = true;

    private bool broken;

    private void Reset()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (broken) return;

        bool switchOk = true;
        if (requireSwitchOnForVolumeBreak)
        {
            switchOk = VolumeSwitchState.IsOn;
        }

        if (switchOk && AudioListener.volume >= volumeThreshold)
        {
            Break();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (broken) return;

        // ✅ 关闭玩家接触碎裂：踩上去不会碎
        if (!breakOnPlayerContact) return;

        if (!collision.collider.CompareTag(playerTag)) return;

        // 可选：只有撞击很猛才碎（避免轻轻落地就碎）
        if (minImpactToBreak > 0f && collision.relativeVelocity.magnitude < minImpactToBreak) return;

        Break();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (broken) return;

        // ✅ 关闭玩家接触碎裂：踩上去不会碎
        if (!breakOnPlayerContact) return;

        if (!other.CompareTag(playerTag)) return;

        // Trigger 没有相对速度信息，通常不建议用“踩碎”逻辑
        Break();
    }

    public void Break()
    {
        if (broken) return;
        broken = true;

        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        if (animator != null && !string.IsNullOrEmpty(breakTriggerName))
        {
            animator.ResetTrigger(breakTriggerName);
            animator.SetTrigger(breakTriggerName);
        }

        if (audioSource != null && breakSfx != null)
        {
            if (playOneShot) audioSource.PlayOneShot(breakSfx);
            else
            {
                audioSource.clip = breakSfx;
                audioSource.Play();
            }
        }

        if (disappearDelay <= 0f) FinishDisappear();
        else Invoke(nameof(FinishDisappear), disappearDelay);
    }

    private void FinishDisappear()
    {
        if (destroyAfterBreak) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}
