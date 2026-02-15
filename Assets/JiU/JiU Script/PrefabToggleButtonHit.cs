using System.Collections;
using UnityEngine;

namespace JiU
{
    /// <summary>
    /// 与 PauseButtonHit 相同的“从下顶击”判定；顶击后切换：若当前无实例则生成 Prefab 到指定位置并登记叠层，若有则销毁实例。
    /// 每个按钮绑定一个 Prefab 和一个生成位置；叠层由 PrefabLayerManager 按生成顺序管理，销毁再生成会重新排到最顶。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PrefabToggleButtonHit : MonoBehaviour
    {
        [Header("Toggle 目标")]
        [Tooltip("要生成/销毁的 Prefab，根物体上需有 SpawnedPrefabInstance")]
        public GameObject prefab;
        [Tooltip("生成位置（世界坐标）；若指定 Transform 则用其 position")]
        public Transform spawnPosition;
        [Tooltip("若 spawnPosition 为空，则用此世界坐标")]
        public Vector3 spawnPositionFallback = Vector3.zero;

        [Header("顶击判定（同 PauseButtonHit）")]
        public float requireRelativeUpVelocity = 0.2f;
        public float requireBelowTolerance = 0.02f;
        public string playerTag = "Player";

        [Header("顶击反馈")]
        public float bumpUp = 0.18f;
        public float bumpTime = 0.08f;
        public float hitAlpha = 0.6f;

        SpriteRenderer _sr;
        Color _startColor;
        Vector3 _startPos;
        Coroutine _bumpCo;
        Collider2D _col;

        /// <summary> 当前由本按钮生成的实例（有则顶击会销毁，无则生成） </summary>
        public GameObject CurrentInstance { get; private set; }

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _startColor = _sr.color;
            _col = GetComponent<Collider2D>();
            _startPos = transform.position;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.collider.CompareTag(playerTag)) return;

            bool movingUpIntoButton = collision.relativeVelocity.y > requireRelativeUpVelocity;
            bool playerIsBelow = true;
            if (_col != null)
            {
                float playerTop = collision.collider.bounds.max.y;
                float buttonBottom = _col.bounds.min.y;
                playerIsBelow = playerTop <= buttonBottom + requireBelowTolerance;
            }
            if (!movingUpIntoButton || !playerIsBelow) return;

            if (_bumpCo != null) StopCoroutine(_bumpCo);
            _bumpCo = StartCoroutine(BumpRoutine());

            if (CurrentInstance != null)
            {
                Destroy(CurrentInstance);
                CurrentInstance = null; // 销毁后清空引用；SpawnedPrefabInstance.OnDisable 会 Unregister 并触发 RecalculateAll
            }
            else if (prefab != null)
            {
                Vector3 pos = spawnPosition != null ? spawnPosition.position : spawnPositionFallback;
                CurrentInstance = Instantiate(prefab, pos, Quaternion.identity);
            }
        }

        IEnumerator BumpRoutine()
        {
            _startPos = transform.position;
            Vector3 upPos = _startPos + Vector3.up * bumpUp;

            if (_sr != null)
            {
                _sr.enabled = true;
                Color c = _startColor;
                c.a = hitAlpha;
                _sr.color = c;
            }

            float t = 0f;
            while (t < bumpTime)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / bumpTime);
                transform.position = Vector3.Lerp(_startPos, upPos, k);
                yield return null;
            }
            t = 0f;
            while (t < bumpTime)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / bumpTime);
                transform.position = Vector3.Lerp(upPos, _startPos, k);
                yield return null;
            }
            transform.position = _startPos;
            if (_sr != null) _sr.color = _startColor;
            _bumpCo = null;
        }
    }
}
