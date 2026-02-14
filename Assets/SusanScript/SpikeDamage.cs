using UnityEngine;

public class SpikeDamage : MonoBehaviour
{
    public int damage = 1;
    public float hitCooldown = 0.25f;

    float nextHitTime;

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time < nextHitTime) return;

        PlayerHealth hp = other.GetComponent<PlayerHealth>();
        if (hp)
        {
            hp.TakeDamage(damage);
            nextHitTime = Time.time + hitCooldown;
        }
    }
}
