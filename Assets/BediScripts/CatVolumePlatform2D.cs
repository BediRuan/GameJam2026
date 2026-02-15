using UnityEngine;

public class CatVolumePlatform2D : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Tooltip("Animator Int parameter: 0=Low, 1=Mid, 2=High")]
    public string moodParam = "MoodLevel";

    [Tooltip("Animator Trigger parameter for attack")]
    public string attackTrigger = "Attack";

    [Header("Volume Thresholds")]
    [Range(0f, 1f)] public float lowMax = 0.30f;   // < 30%  -> Low
    [Range(0f, 1f)] public float midMax = 0.50f;   // 30-50 -> Mid
    [Range(0f, 1f)] public float platformMax = 0.90f; // <90% 可当平台；>=90% 攻击/推开

    [Header("Colliders")]
    [Tooltip("Non-trigger collider used as platform")]
    public Collider2D platformCollider;

    [Tooltip("Trigger collider used to detect player proximity")]
    public Collider2D aggroTrigger;

    [Header("Player")]
    public string playerTag = "Player";

    [Header("Pushback")]
    public float pushVelocity = 12f;     // 推开速度（更像“弹开”）
    public float pushUpVelocity = 2f;    // 额外向上（防止卡进猫）
    public float attackCooldown = 0.6f;  // 攻击冷却，避免每帧触发

    [Tooltip("If true, only attack when VolumeSwitchState.IsOn == true (if you have that system).")]
    public bool requireSwitchOn = false;

    private int lastMood = -1;
    private float lastAttackTime = -999f;

    private void Reset()
    {
        animator = GetComponent<Animator>();
        // 平台/触发器需要你手动拖，避免误指向
    }

    private void Awake()
    {
        ApplyMoodFromVolume(true);
        ApplyPlatformFromVolume(true);
    }

    private void Update()
    {
        ApplyMoodFromVolume(false);
        ApplyPlatformFromVolume(false);
    }

    private float GetEffectiveVolume()
    {
        // 如果你做了 on/off 系统，Off 时 AudioListener.volume 通常会被你锁成 0
        // 这里提供一个可选的“必须开关为On才生效”
        if (requireSwitchOn)
        {
            // 没有 VolumeSwitchState 的话就把 requireSwitchOn 关掉
            if (!VolumeSwitchState.IsOn) return 0f;
        }
        return AudioListener.volume;
    }

    private void ApplyMoodFromVolume(bool force)
    {
        if (animator == null) return;

        float v = GetEffectiveVolume();

        int mood;
        if (v < lowMax) mood = 0;
        else if (v < midMax) mood = 1;
        else mood = 2;

        if (force || mood != lastMood)
        {
            animator.SetInteger(moodParam, mood);
            lastMood = mood;
        }
    }

    private void ApplyPlatformFromVolume(bool force)
    {
        float v = GetEffectiveVolume();
        bool platformOn = v < platformMax;

        if (platformCollider != null && (force || platformCollider.enabled != platformOn))
            platformCollider.enabled = platformOn;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 只有 aggroTrigger 才处理（避免别的 trigger 干扰）
        if (aggroTrigger == null) return;
        if (other == null || other != other) return; // 防御性（可忽略）
        if (!other.CompareTag(playerTag)) return;

        // 确保是进入“靠近触发器”时触发
        // 如果脚本挂在根物体，Unity会把所有 Trigger 事件都打到这里，
        // 所以建议 aggroTrigger 就在同一个物体上（根物体），这样更简单。
        // 如果你把 aggroTrigger 放在子物体上，也能用，把子物体也挂同脚本或转发。

        float v = GetEffectiveVolume();
        if (v < platformMax) return; // <90% 不攻击不推开

        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        // 播放攻击动画（一次）
        if (animator != null && !string.IsNullOrEmpty(attackTrigger))
            animator.SetTrigger(attackTrigger);

        // 推开玩家
        PushPlayerAway(other);
    }

    private void PushPlayerAway(Collider2D playerCol)
    {
        // 推开方向：从猫指向玩家
        Vector2 dir = (playerCol.transform.position - transform.position);
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;

        dir.Normalize();

        // 优先用 Rigidbody2D（最靠谱）
        Rigidbody2D rb = playerCol.attachedRigidbody;
        if (rb == null) rb = playerCol.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // 直接设置速度，效果干脆，避免 AddForce 受质量/阻尼影响
            Vector2 vel = rb.linearVelocity;
            vel.x = dir.x * pushVelocity;
            vel.y = Mathf.Max(vel.y, pushUpVelocity);
            rb.linearVelocity = vel;
        }
        else
        {
            // 没有 Rigidbody2D 就硬挪开一点（兜底）
            playerCol.transform.position += (Vector3)(dir * 0.35f);
        }
    }
}
