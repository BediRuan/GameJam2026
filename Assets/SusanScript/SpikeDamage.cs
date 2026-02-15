using UnityEngine;

public class SpikeDamageSmooth : MonoBehaviour
{
    public int damagePerTick = 2;          // 每次掉几格
    public float tickInterval = 0.25f;     // 多久掉一次
    public string playerTag = "Player";

    float nextTickTime = 0f;

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (Time.time < nextTickTime) return;

        PlayerHealth hp = other.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.TakeDamage(damagePerTick);
            nextTickTime = Time.time + tickInterval;
        }
    }
}
