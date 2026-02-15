using System.Collections;
using UnityEngine;

namespace JiU
{
    /// <summary>
    /// 游戏开始时先将镜头锁定到“终点”等目标（仅水平或水平+垂直），再回到正常跟随目标（如玩家）。
    /// 开场阶段会无视 CameraBoundsClamp，并暂停 Cowsins CameraController 的跟随。
    /// 挂在镜头根物体上（与 CameraController、CameraBoundsClamp 同物体），并拖好引用。
    /// </summary>
    public class CameraIntroPan : MonoBehaviour
    {
        [Header("Targets")]
        [Tooltip("开场要展示的目标（如终点位置）")]
        public Transform introTarget;
        [Tooltip("正常跟随目标（如玩家），回到此目标后恢复跟随")]
        public Transform normalTarget;

        [Header("Timing")]
        [Tooltip("移动到 intro 目标所用时间（秒）")]
        public float moveToIntroDuration = 2f;
        [Tooltip("在 intro 目标处停留时间（秒）")]
        public float holdIntroDuration = 1f;
        [Tooltip("从 intro 回到 normal 目标所用时间（秒）")]
        public float moveBackDuration = 1.5f;

        [Header("Movement")]
        [Tooltip("勾选则只水平移动；不勾选则 X、Y 都移动")]
        public bool horizontalOnly = true;

        [Header("References (同物体可留空)")]
        [Tooltip("开场期间要禁用的跟随脚本（如 Cowsins CameraController），需在 Inspector 里拖入")]
        public MonoBehaviour cameraFollowToDisable;
        [Tooltip("开场期间要忽略的边界钳制，不填则自动从同物体获取 CameraBoundsClamp")]
        public CameraBoundsClamp boundsClampToIgnore;

        Transform _rig;
        bool _done;
        Quaternion _lockedRotation;

        void Awake()
        {
            _rig = transform;
            if (boundsClampToIgnore == null)
                boundsClampToIgnore = GetComponent<CameraBoundsClamp>();
        }

        void Start()
        {
            if (introTarget == null || normalTarget == null)
                return;
            StartCoroutine(IntroRoutine());
        }

        IEnumerator IntroRoutine()
        {
            _done = false;

            if (cameraFollowToDisable != null)
                cameraFollowToDisable.enabled = false;
            if (boundsClampToIgnore != null)
                boundsClampToIgnore.ignoreClamp = true;

            _lockedRotation = _rig.rotation;

            Vector3 startPos = _rig.position;
            Vector3 introPos = introTarget.position;
            if (horizontalOnly)
                introPos.y = startPos.y;
            introPos.z = startPos.z;

            float t = 0f;
            while (t < moveToIntroDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / moveToIntroDuration);
                k = k * k * (3f - 2f * k);
                _rig.position = Vector3.Lerp(startPos, introPos, k);
                _rig.rotation = _lockedRotation;
                yield return null;
            }
            _rig.position = introPos;
            _rig.rotation = _lockedRotation;

            yield return new WaitForSeconds(holdIntroDuration);

            Vector3 backPos = normalTarget.position;
            if (horizontalOnly)
                backPos.y = _rig.position.y;
            backPos.z = _rig.position.z;

            t = 0f;
            while (t < moveBackDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / moveBackDuration);
                k = k * k * (3f - 2f * k);
                _rig.position = Vector3.Lerp(introPos, backPos, k);
                _rig.rotation = _lockedRotation;
                yield return null;
            }
            _rig.position = backPos;
            _rig.rotation = _lockedRotation;

            if (cameraFollowToDisable != null)
                cameraFollowToDisable.enabled = true;
            if (boundsClampToIgnore != null)
                boundsClampToIgnore.ignoreClamp = false;

            _done = true;
        }

        /// <summary> 是否已完成开场平移（只读） </summary>
        public bool IsDone => _done;
    }
}
