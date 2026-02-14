using UnityEngine;

public class SliderPlatformController : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Transform leftPoint;
    public Transform rightPoint;

    public float smoothSpeed = 8f;
    public bool lockY = true;

    Rigidbody2D rb;
    float fixedY;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (!playerHealth)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) playerHealth = p.GetComponent<PlayerHealth>();
        }

        fixedY = rb.position.y;
    }

    void FixedUpdate()
    {
        if (!playerHealth || !leftPoint || !rightPoint) return;

        float hp01 = playerHealth.maxHp <= 0 ? 0f : (float)playerHealth.currentHp / playerHealth.maxHp;
        hp01 = Mathf.Clamp01(hp01);

        Vector2 target = Vector2.Lerp(leftPoint.position, rightPoint.position, hp01);
        if (lockY) target.y = fixedY;

        Vector2 next = Vector2.Lerp(rb.position, target, Time.fixedDeltaTime * smoothSpeed);
        rb.MovePosition(next);
    }
}
