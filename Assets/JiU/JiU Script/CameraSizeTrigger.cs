using UnityEngine;

namespace JiU
{
    /// <summary>
    /// 挂在带 Collider2D（Is Trigger）的物体上。玩家进入触发器时切换摄像机的 Orthographic Size。
    /// 可与 Cowsins CameraController 配合：摄像机需为 Orthographic，Size 由此脚本或触发器驱动。
    /// </summary>
    public class CameraSizeTrigger : MonoBehaviour
    {
        [Header("Trigger")]
        [Tooltip("触发器的 Tag 检测")]
        public string playerTag = "Player";

        [Header("Camera & Size")]
        [Tooltip("不填则用 Camera.main")]
        public Camera targetCamera;
        [Tooltip("进入该 Trigger 后摄像机目标 Orthographic Size")]
        public float targetSize = 8f;
        [Tooltip("离开 Trigger 后恢复的 Size（若 &lt;= 0 则不恢复）")]
        public float exitSize = 0f;
        [Tooltip("是否平滑过渡")]
        public bool smoothTransition = true;
        [Tooltip("平滑速度（Size/秒），smoothTransition 为 true 时有效")]
        public float smoothSpeed = 4f;

        float _currentTargetSize;
        bool _insideTrigger;

        void Awake()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        void Start()
        {
            if (targetCamera != null && targetCamera.orthographic)
                _currentTargetSize = targetCamera.orthographicSize;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            _insideTrigger = true;
            _currentTargetSize = targetSize;
            if (!smoothTransition && targetCamera != null && targetCamera.orthographic)
                targetCamera.orthographicSize = targetSize;
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            _insideTrigger = false;
            if (exitSize > 0f)
            {
                _currentTargetSize = exitSize;
                if (!smoothTransition && targetCamera != null && targetCamera.orthographic)
                    targetCamera.orthographicSize = exitSize;
            }
        }

        void LateUpdate()
        {
            if (targetCamera == null || !targetCamera.orthographic) return;
            if (!smoothTransition) return;

            float step = smoothSpeed * Time.deltaTime;
            targetCamera.orthographicSize = Mathf.MoveTowards(
                targetCamera.orthographicSize,
                _currentTargetSize,
                step
            );
        }
    }
}
