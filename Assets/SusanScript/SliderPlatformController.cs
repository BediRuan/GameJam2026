using UnityEngine;

public class SliderPlatformController : MonoBehaviour
{
    public PlayerHealth playerHealth;

    [Header("World Range")]
    public Transform leftPoint;
    public Transform rightPoint;

    [Header("Movement")]
    public bool smoothMove = true;
    public float smoothSpeed = 12f;

    [Header("Optional")]
    public bool lockY = true;

    float fixedY;

    void Start()
    {
        if (!playerHealth)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) playerHealth = p.GetComponent<PlayerHealth>();
        }

        fixedY = transform.position.y;
    }

    void Update()
    {
        if (!playerHealth || !leftPoint || !rightPoint) return;

        float hp01 = playerHealth.maxHp <= 0 ? 0f : (float)playerHealth.currentHp / playerHealth.maxHp;
        hp01 = Mathf.Clamp01(hp01);

        // 规则：血满在右边，血空在左边
        Vector3 targetPos = Vector3.Lerp(leftPoint.position, rightPoint.position, hp01);

        if (lockY) targetPos.y = fixedY;

        if (smoothMove)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothSpeed);
        }
        else
        {
            transform.position = targetPos;
        }
    }
}
