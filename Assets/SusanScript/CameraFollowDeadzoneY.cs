using UnityEngine;

public class CameraFollowCenter2D_AirSmooth : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                         // 玩家
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Air Smooth Follow")]
    public float airSmoothSpeed = 10f;               // 空中平滑速度

    [Header("Ground Check (2D)")]
    public Rigidbody2D targetRb;                     // 可选：不填会自动抓
    public Transform groundCheckPoint;               // 脚底点（推荐拖一个空物体）
    public float groundCheckRadius = 0.12f;
    public LayerMask groundMask;                     // 只勾 Ground 层

    [Tooltip("如果你没做groundCheckPoint，也可以用Y速度判断是否在空中（不如地面检测稳）")]
    public bool fallbackUseVelocityIfNoGroundCheck = true;
    public float groundedVelocityEpsilon = 0.05f;

    void Awake()
    {
        if (target && targetRb == null)
            targetRb = target.GetComponent<Rigidbody2D>();
    }

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desired = target.position + offset;

        bool grounded = IsGrounded2D();
        bool inAir = !grounded;

        if (inAir)
        {
            float t = 1f - Mathf.Exp(-airSmoothSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, t);
        }
        else
        {
            // 地面：立刻跟随到中心（无平滑）
            transform.position = desired;
        }

        // 2D不旋转
        transform.rotation = Quaternion.identity;
    }

    bool IsGrounded2D()
    {
        if (groundCheckPoint != null)
        {
            return Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundMask) != null;
        }

        if (fallbackUseVelocityIfNoGroundCheck && targetRb != null)
        {
            // 粗略：落地时Y速度≈0
            return Mathf.Abs(targetRb.linearVelocity.y) <= groundedVelocityEpsilon;
        }

        // 没有任何判断手段时，当作在地面（相机不平滑）
        return true;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null) return;
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }
}
