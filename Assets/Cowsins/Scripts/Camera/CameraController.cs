using UnityEngine;
using UnityEditor;

namespace cowsins2D
{
    public class CameraController : MonoBehaviour
    {
        [System.Serializable]
        public enum CameraControllerMethod
        {
            Static,
            FollowTarget,
            FollowTargetWhenCloseToBounds
        };

        [Tooltip("Configures the style of the camera, the way it behaves")] public CameraControllerMethod cameraMethod;

        [SerializeField, Tooltip("What object does this camera follow.")] private Transform target;

        [SerializeField, Tooltip("Camera position variation")] private Vector3 cameraOffset;

        [SerializeField, Tooltip("Size of the camera vision. Once the target is outside of these boundaries, it will start following the target.")] private Vector2 boundary;

        [SerializeField, Tooltip("The lower, the faster it will reach the destination.")] private float cameraLaziness;

        [SerializeField] private float fovSmoothness;

        [SerializeField] private Rigidbody2D targetRb;   // 可选：拖玩家的 Rigidbody2D
        [SerializeField] private float smoothTime = 0.12f; // 平时跟随的平滑时间（越小越紧）
        [SerializeField] private float maxFollowSpeed = 999f; // 允许相机追赶的最大速度（很大即可）

        [Header("Fast Fall Catch-up")]
        [SerializeField] private bool catchUpOnFastFall = true;
        [SerializeField] private float fastFallSpeedThreshold = -12f; // 玩家y速度小于这个就算快速坠落
        [SerializeField] private float fastFallSmoothTime = 0.02f;     // 快速坠落时的平滑（越小越贴）
        [SerializeField] private float snapDistanceY = 2.5f;           // 若相机与玩家Y差距超过这个，直接贴上

        private Vector3 _smoothVel;



        private float fov;

        private Camera cam;

        private void Awake()
        {
            cam = transform.GetChild(0).GetComponent<Camera>();
            fov = cam.fieldOfView;
        }

        private void Update()
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, fovSmoothness * Time.deltaTime);
        }


        // Called in LateUpdate for proper smoothness
        private void LateUpdate()
        {
            // Depending on the camera method, call different functions
            switch (cameraMethod)
            {
                case CameraControllerMethod.FollowTarget:
                    SimpleFollowTarget();
                    break;
                case CameraControllerMethod.FollowTargetWhenCloseToBounds:
                    BoundsFollow();
                    break;
            }
        }

        // Simply lerp the position of this object from the current one to the target and add the camera offset
        private void SimpleFollowTarget()
        {
            if (!target) return;

            Vector3 desired = target.position + (Vector3)cameraOffset;

            float curSmooth = smoothTime;

            // 如果玩家在快速下落：相机更“紧”，必要时直接贴住Y
            if (catchUpOnFastFall && targetRb != null)
            {
                if (targetRb.linearVelocity.y <= fastFallSpeedThreshold)
                {
                    curSmooth = fastFallSmoothTime;

                    float dy = Mathf.Abs(desired.y - transform.position.y);
                    if (dy > snapDistanceY)
                    {
                        // 只在Y轴瞬间贴上（X仍保持平滑）
                        transform.position = new Vector3(transform.position.x, desired.y, desired.z);
                    }
                }
            }

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desired,
                ref _smoothVel,
                curSmooth,
                maxFollowSpeed,
                Time.deltaTime
            );
        }


        // Only follow if out of bounds
        private void BoundsFollow()
        {
            Vector2 dif = Vector2.zero;

            // Grab the distance in the horizontal axis from this to the target
            float difX = target.position.x - transform.position.x;


            // If this distance is greater than the boundary horizontal size ( left or right )
            if (difX > boundary.x || difX < -boundary.x)
            {
                if (transform.position.x < target.position.x) dif.x = difX - boundary.x;
                else dif.x = difX + boundary.x;
            }

            // Same process for the vertical axis (y)
            float difY = target.position.y - transform.position.y;

            if (difY > boundary.y || difY < -boundary.y)
            {
                if (transform.position.y < target.position.y) dif.y = difY - boundary.y;
                else dif.y = difY + boundary.y; // ✅ 修复
            }

            // Lerp the position
            transform.position = Vector3.Lerp(transform.position, transform.position + (Vector3)dif, Time.deltaTime * 1 / cameraLaziness);
        }

        public void CameraToStatic() => cameraMethod = CameraControllerMethod.Static;

        public void CameraToSimple() => cameraMethod = CameraControllerMethod.FollowTarget;

        public void CameraToBoundary() => cameraMethod = CameraControllerMethod.FollowTargetWhenCloseToBounds;
    }


#if UNITY_EDITOR
    [System.Serializable]
    [CustomEditor(typeof(CameraController))]
    public class CameraControllerEditor : Editor
    {
        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            CameraController myScript = target as CameraController;

            #region variables
            GUILayout.Space(20);
            EditorGUILayout.LabelField("CAMERA CONTROLLER", EditorStyles.boldLabel);
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraMethod"));
            if (myScript.cameraMethod == CameraController.CameraControllerMethod.FollowTargetWhenCloseToBounds)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("boundary"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraOffset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraLaziness"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fovSmoothness"));
            #endregion



            serializedObject.ApplyModifiedProperties();

        }
    }
#endif
}
