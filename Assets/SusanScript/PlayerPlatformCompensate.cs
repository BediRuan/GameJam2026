using UnityEngine;

public class PlayerPlatformCompensate : MonoBehaviour
{
    Rigidbody2D rb;
    PlatformVelocityProvider currentPlatform;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (currentPlatform == null) return;

        // 只补偿平台的X速度
        Vector2 v = rb.linearVelocity;
        v.x += currentPlatform.Velocity.x;
        rb.linearVelocity = v;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TrySetPlatform(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        TrySetPlatform(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (currentPlatform != null && collision.collider.GetComponentInParent<PlatformVelocityProvider>() == currentPlatform)
            currentPlatform = null;
    }

    void TrySetPlatform(Collision2D collision)
    {
        // 确保是从上方站在平台上
        if (collision.contactCount <= 0) return;

        ContactPoint2D c = collision.GetContact(0);
        if (c.normal.y < 0.5f) return;

        PlatformVelocityProvider p = collision.collider.GetComponentInParent<PlatformVelocityProvider>();
        if (p != null) currentPlatform = p;
    }
}
