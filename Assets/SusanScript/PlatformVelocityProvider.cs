using UnityEngine;

public class PlatformVelocityProvider : MonoBehaviour
{
    public Vector2 Velocity { get; private set; }

    Vector2 lastPos;
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lastPos = rb ? rb.position : (Vector2)transform.position;
    }

    void FixedUpdate()
    {
        Vector2 now = rb ? rb.position : (Vector2)transform.position;
        Velocity = (now - lastPos) / Time.fixedDeltaTime;
        lastPos = now;
    }
}
