using UnityEngine;

public class CameraFollow2DIn3D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                 // 玩家
    public Vector3 offset = new Vector3(0f, 0f, -10f); // 2D常用：相机在Z=-10

    [Header("Smoothing")]
    public bool smoothFollow = true;
    [Tooltip("越大越跟手；越小越丝滑")]
    public float smoothSpeed = 12f;

    [Header("Clamp (Optional)")]
    public bool clampToBounds = false;
    public Vector2 minXY; // 例如 (-20, -10)
    public Vector2 maxXY; // 例如 ( 20,  15)

    void Reset()
    {
        // 确保相机不旋转（2D效果）
        transform.rotation = Quaternion.identity;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 只跟随XY，不做旋转；Z固定
        Vector3 desired = new Vector3(target.position.x, target.position.y, 0f) + offset;

        // 可选：限制相机范围
        if (clampToBounds)
        {
            desired.x = Mathf.Clamp(desired.x, minXY.x, maxXY.x);
            desired.y = Mathf.Clamp(desired.y, minXY.y, maxXY.y);
        }

        if (smoothFollow)
        {
            // 平滑跟随（帧率无关）
            float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, t);
        }
        else
        {
            // 直接跟随（无延迟）
            transform.position = desired;
        }

        // 强制不旋转（防止其他脚本/动画影响）
        transform.rotation = Quaternion.identity;
    }
}
