using System.Collections.Generic;
using UnityEngine;

namespace JiU
{
    /// <summary>
    /// 挂在“水平平台”子物体上，该物体上需有 BoxCollider2D 表示平台范围。
    /// 保留原有叠层检测：当被上层实例的 Bounds 完全覆盖时禁用 BoxCollider2D，否则启用，不做区间裁剪与动态生成，碰撞体与平台实际大小一致。
    /// </summary>
    public class DynamicPlatform : MonoBehaviour
    {
        [Header("只读（运行时从碰撞体/精灵计算，用于覆盖检测）")]
        [SerializeField] float _originalXMin;
        [SerializeField] float _originalXMax;
        [SerializeField] float _colliderYCenter;
        [SerializeField] float _colliderHeight;

        public float WorldXMin => _originalXMin;
        public float WorldXMax => _originalXMax;
        public float WorldYMin => _colliderYCenter - _colliderHeight * 0.5f;
        public float WorldYMax => _colliderYCenter + _colliderHeight * 0.5f;

        SpawnedPrefabInstance _parentInstance;
        bool _initialized;

        void Awake()
        {
            CacheOriginalGeometry();
            _parentInstance = GetComponentInParent<SpawnedPrefabInstance>();
        }

        void CacheOriginalGeometry()
        {
            if (_initialized) return;
            var colliders = GetComponents<BoxCollider2D>();
            if (colliders == null || colliders.Length == 0) return;

            Bounds union = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
                union.Encapsulate(colliders[i].bounds);

            _originalXMin = union.min.x;
            _originalXMax = union.max.x;
            _colliderYCenter = union.center.y;
            _colliderHeight = union.size.y;

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Bounds spriteBounds = sr.bounds;
                _colliderYCenter = spriteBounds.center.y;
                _colliderHeight = spriteBounds.size.y;
                _originalXMin = Mathf.Max(_originalXMin, spriteBounds.min.x);
                _originalXMax = Mathf.Min(_originalXMax, spriteBounds.max.x);
            }

            _initialized = true;
        }

        /// <summary> 由 PrefabLayerManager 触发：被上层 Bounds 完全覆盖则禁用本物体上的 BoxCollider2D，否则启用。 </summary>
        public void RecalculateColliders()
        {
            if (!_initialized) CacheOriginalGeometry();
            if (_originalXMax <= _originalXMin) return;
            if (_parentInstance == null) _parentInstance = GetComponentInParent<SpawnedPrefabInstance>();
            if (PrefabLayerManager.Instance == null || _parentInstance == null)
            {
                SetCollidersEnabled(true);
                return;
            }

            int myIndex = PrefabLayerManager.Instance.GetStackIndex(_parentInstance);
            var above = PrefabLayerManager.Instance.GetInstancesAbove(myIndex);

            float yMin = WorldYMin;
            float yMax = WorldYMax;
            bool covered = false;

            foreach (var instance in above)
            {
                if (instance == null || instance.boundsCollider == null) continue;
                Bounds b = instance.boundsCollider.bounds;
                if (b.max.y <= yMin || b.min.y >= yMax) continue;
                if (b.min.x <= _originalXMin && b.max.x >= _originalXMax)
                {
                    covered = true;
                    break;
                }
            }

            SetCollidersEnabled(!covered);
        }

        void SetCollidersEnabled(bool enabled)
        {
            foreach (var col in GetComponents<BoxCollider2D>())
            {
                if (col != null)
                    col.enabled = enabled;
            }
        }
    }
}
