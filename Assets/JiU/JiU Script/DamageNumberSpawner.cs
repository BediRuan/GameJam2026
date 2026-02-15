using UnityEngine;

namespace JiU
{
    /// <summary>
    /// 挂在玩家身上，监听 PlayerHealth 的伤害事件，在头顶生成伤害数字。
    /// </summary>
    public class DamageNumberSpawner : MonoBehaviour
    {
        [Header("生成")]
        [Tooltip("伤害数字预制体，需包含 DamageNumber 和 BoxCollider2D")]
        public GameObject damageNumberPrefab;

        [Header("生成位置")]
        [Tooltip("相对于玩家 Transform 的偏移，如 (0, 1.2, 0) 表示头顶上方")]
        public Vector3 spawnOffset = new Vector3(0f, 1.2f, 0f);

        [Tooltip("若勾选，会在玩家 Collider2D 顶部之上生成，并加上 spawnOffset")]
        public bool useColliderTop = true;

        PlayerHealth _playerHealth;

        void Awake()
        {
            _playerHealth = GetComponent<PlayerHealth>();
            if (!_playerHealth)
                _playerHealth = GetComponentInChildren<PlayerHealth>();
        }

        void OnEnable()
        {
            if (_playerHealth != null)
                _playerHealth.onDamageTaken += OnDamageTaken;
        }

        void OnDisable()
        {
            if (_playerHealth != null)
                _playerHealth.onDamageTaken -= OnDamageTaken;
        }

        void OnDamageTaken(int amount)
        {
            if (damageNumberPrefab == null) return;

            Vector3 pos = GetSpawnPosition();
            GameObject go = Instantiate(damageNumberPrefab, pos, Quaternion.identity);

            var dn = go.GetComponent<DamageNumber>();
            if (dn != null)
                dn.Init(amount);
        }

        Vector3 GetSpawnPosition()
        {
            Vector3 basePos = transform.position;

            if (useColliderTop)
            {
                var col = GetComponent<Collider2D>();
                if (!col) col = GetComponentInChildren<Collider2D>();
                if (col != null)
                    basePos.y = col.bounds.max.y;
            }

            return basePos + spawnOffset;
        }
    }
}
