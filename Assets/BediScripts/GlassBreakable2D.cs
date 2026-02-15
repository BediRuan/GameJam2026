using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GlassBreakable2D : MonoBehaviour
{
    [Header("Break Conditions")]
    public string playerTag = "Player";
    [Range(0f, 1f)] public float volumeThreshold = 0.9f; // 90%

    [Tooltip("If true, volume-based breaking only happens when switch is ON (if you use VolumeSwitchState).")]
    public bool requireSwitchOnForVolumeBreak = true;

    [Header("Animation")]
    public Animator animator;
    [Tooltip("Animator Trigger name to play breaking animation")]
    public string breakTriggerName = "Break";

    [Tooltip("Seconds to wait before disappearing (match your animation length). If 0, will hide immediately after triggering.")]
    public float disappearDelay = 0.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip breakSfx;

    [Tooltip("If true, will play as OneShot (recommended).")]
    public bool playOneShot = true;

    [Header("After Break")]
    public bool destroyAfterBreak = true; // false = SetActive(false)

    private bool broken;

    private void Reset()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (broken) return;

        // 音量触发碎裂
        bool switchOk = true;
        if (requireSwitchOnForVolumeBreak)
        {
            // 如果你没有 VolumeSwitchState 也没关系：把这一行删掉或让 requireSwitchOnForVolumeBreak=false
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
        if (!collision.collider.CompareTag(playerTag)) return;
        Break();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (broken) return;
        if (!other.CompareTag(playerTag)) return;
        Break();
    }

    public void Break()
    {
        if (broken) return;
        broken = true;

        // 关掉碰撞，避免重复触发
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // 播放动画
        if (animator != null && !string.IsNullOrEmpty(breakTriggerName))
        {
            animator.ResetTrigger(breakTriggerName);
            animator.SetTrigger(breakTriggerName);
        }

        // 播放音效
        if (audioSource != null && breakSfx != null)
        {
            if (playOneShot) audioSource.PlayOneShot(breakSfx);
            else
            {
                audioSource.clip = breakSfx;
                audioSource.Play();
            }
        }

        // 消失
        if (disappearDelay <= 0f)
        {
            FinishDisappear();
        }
        else
        {
            Invoke(nameof(FinishDisappear), disappearDelay);
        }
    }

    private void FinishDisappear()
    {
        if (destroyAfterBreak) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}

