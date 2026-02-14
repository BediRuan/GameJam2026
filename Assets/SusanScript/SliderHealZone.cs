using UnityEngine;

public class SliderHealZone : MonoBehaviour
{
    public int healPerTick = 1;
    public float healInterval = 0.5f;

    float nextHealTime;

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        ContactPoint2D contact = collision.GetContact(0);

        // 确保玩家是在平台上方
        if (contact.normal.y < 0.5f) return;

        if (Time.time >= nextHealTime)
        {
            PlayerHealth hp = collision.collider.GetComponent<PlayerHealth>();

            if (hp)
            {
                hp.Heal(healPerTick);
                nextHealTime = Time.time + healInterval;
            }
        }
    }

}
