using System.Collections.Generic;
using UnityEngine;

namespace JiU
{
    // 需在 Prefab 根或子物体上有 SpriteRenderer，Sorting Order 会按叠层索引统一设置
    /// <summary>
    /// 挂在“可切换平台”Prefab 根物体上。持有背景 Bounds 的 BoxCollider2D 引用及所有子物体上的 DynamicPlatform。
    /// 在 OnEnable 时向 PrefabLayerManager 注册（加入叠层末尾），OnDisable 时注销；注册/注销会触发全局 RecalculateAll。
    /// </summary>
    public class SpawnedPrefabInstance : MonoBehaviour
    {
        [Header("Bounds（用于叠层 X 区间裁剪）")]
        [Tooltip("代表本 Prefab 覆盖范围的背景 BoxCollider2D，建议放在 PrefabBounds 等不与玩家碰撞的 Layer")]
        public BoxCollider2D boundsCollider;

        /// <summary> 本 Prefab 内所有 DynamicPlatform（只读，由 Awake 填充） </summary>
        public IReadOnlyList<DynamicPlatform> Platforms => _platforms;

        List<DynamicPlatform> _platforms = new List<DynamicPlatform>();

        void Awake()
        {
            if (boundsCollider == null)
                boundsCollider = GetComponentInChildren<BoxCollider2D>();

            _platforms.Clear();
            _platforms.AddRange(GetComponentsInChildren<DynamicPlatform>(true));
        }

        void OnEnable()
        {
            if (PrefabLayerManager.Instance != null)
                PrefabLayerManager.Instance.Register(this);
        }

        void OnDisable()
        {
            if (PrefabLayerManager.Instance != null)
                PrefabLayerManager.Instance.Unregister(this);
        }

        /// <summary> 由 PrefabLayerManager.RecalculateAll 调用，对本 Prefab 内所有平台触发重算。 </summary>
        internal void RecalculateAllPlatforms()
        {
            foreach (var p in _platforms)
            {
                if (p != null && p.isActiveAndEnabled)
                    p.RecalculateColliders();
            }
        }

        /// <summary> 由 PrefabLayerManager 调用：传入本实例的 baseOrder（= baseSortingOrder + 叠层索引*2）。背景用 baseOrder，DynamicPlatform 用 baseOrder+1。 </summary>
        public void SetSortingOrder(int baseOrder)
        {
            foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (sr == null) continue;
                var platform = sr.GetComponentInParent<DynamicPlatform>();
                sr.sortingOrder = (platform != null && platform.transform.IsChildOf(transform)) ? baseOrder + 1 : baseOrder;
            }
        }
    }
}
