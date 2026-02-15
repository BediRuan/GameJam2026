using UnityEngine;

namespace JiU
{
    /// <summary>
    /// 挂在“跟随移动的镜头”物体上（与 Cowsins CameraController 同物体即可）。
    /// 在编辑器里设置上下左右四个世界坐标极限，每帧将摄像机位置限制在此框内，镜头不会超出设定边界。
    /// 建议将此脚本的 Script Execution Order 设得比 CameraController 大，以便在跟随计算之后再钳制。
    /// </summary>
    [ExecuteAlways]
    public class CameraBoundsClamp : MonoBehaviour
    {
        [Header("Bounds (World)")]
        [Tooltip("左边界 X（世界坐标）")]
        public float boundLeft = -20f;
        [Tooltip("右边界 X（世界坐标）")]
        public float boundRight = 20f;
        [Tooltip("下边界 Y（世界坐标）")]
        public float boundBottom = -10f;
        [Tooltip("上边界 Y（世界坐标）")]
        public float boundTop = 10f;

        [Header("Optional")]
        [Tooltip("要钳制的 Transform，不填则用当前物体")]
        public Transform targetTransform;
        [Tooltip("为 true 时不钳制（例如开场镜头平移时由 CameraIntroPan 设为 true）")]
        public bool ignoreClamp;

        [Header("Smooth Recovery")]
        [Tooltip("勾选后越界时平滑拉回边界，避免与跟随镜头产生生硬转折")]
        public bool useSmoothRecovery = true;
        [Tooltip("平滑拉回的时间（秒），越小拉回越快，推荐 0.05～0.15")]
        [Range(0.02f, 0.5f)]
        public float smoothRecoveryTime = 0.08f;

        Vector3 _smoothVel = Vector3.zero;

        void OnValidate()
        {
            if (boundRight < boundLeft) boundRight = boundLeft;
            if (boundTop < boundBottom) boundTop = boundBottom;
        }

        Vector3 GetClampedPosition(Vector3 p)
        {
            p.x = Mathf.Clamp(p.x, boundLeft, boundRight);
            p.y = Mathf.Clamp(p.y, boundBottom, boundTop);
            return p;
        }

        void LateUpdate()
        {
            if (ignoreClamp) return;
            Transform t = targetTransform != null ? targetTransform : transform;
            Vector3 p = t.position;
            Vector3 clamped = GetClampedPosition(p);

            if (useSmoothRecovery)
            {
                p = Vector3.SmoothDamp(p, clamped, ref _smoothVel, smoothRecoveryTime, Mathf.Infinity, Time.deltaTime);
                p = GetClampedPosition(p);
            }
            else
            {
                p = clamped;
            }

            t.position = p;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            float cx = (boundLeft + boundRight) * 0.5f;
            float cy = (boundBottom + boundTop) * 0.5f;
            float w = boundRight - boundLeft;
            float h = boundTop - boundBottom;
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.3f);
            Gizmos.DrawWireCube(new Vector3(cx, cy, 0f), new Vector3(w, h, 0f));
        }
#endif
    }
}
