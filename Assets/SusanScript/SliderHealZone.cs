using UnityEngine;

public class SliderHealZone : MonoBehaviour
{
    public int healPerTick = 1;
    public float healInterval = 0.5f;

    [Header("Player Foot Check")]
    public Transform playerFoot;          // 玩家脚底检测点
    public Vector2 footCheckSize = new Vector2(0.18f, 0.08f); // 小盒子，覆盖脚底
    public LayerMask platformMask;        // 平台所在Layer

    float nextHealTime;

    void Update()
    {
        if (playerFoot == null) return;
        if (Time.time < nextHealTime) return;

        // 只要脚底盒子与平台层发生重叠，就算“站在上面”
        Collider2D hit = Physics2D.OverlapBox(playerFoot.position, footCheckSize, 0f, platformMask);
        if (hit != null)
        {
            PlayerHealth hp = playerFoot.GetComponentInParent<PlayerHealth>();
            if (hp != null)
            {
                hp.Heal(healPerTick);
                nextHealTime = Time.time + healInterval;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (playerFoot == null) return;
        Gizmos.DrawWireCube(playerFoot.position, footCheckSize);
    }

}
