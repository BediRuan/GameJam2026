using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;   // 玩家 Transform

    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    // Z 必须是 -10，否则Camera会贴到玩家上

    [Header("Follow Settings")]
    public float followSpeed = 10f;
    // 越大越跟手
    // 推荐 5 ~ 15

    [Header("Clamp (Optional)")]
    public bool useClamp = false;

    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    void LateUpdate()
    {
        if (target == null)
            return;

        // 目标位置
        Vector3 desiredPosition = target.position + offset;

        // 平滑移动
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        // Clamp 限制范围（可选）
        if (useClamp)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);
        }

        transform.position = smoothedPosition;
    }
}
