using System.Collections.Generic;
using UnityEngine;

namespace JiU
{
    /// <summary>
    /// 全局管理当前场景中所有“可切换平台 Prefab”实例的叠放顺序（按生成时间，index 0 为最底层，末尾为最顶层）。
    /// 任一 Prefab 生成或销毁时调用 RecalculateAll()，驱动所有 DynamicPlatform 按叠层做 X 区间裁剪并重建碰撞体。
    /// </summary>
    public class PrefabLayerManager : MonoBehaviour
    {
        public static PrefabLayerManager Instance { get; private set; }

        [Header("叠层绘制顺序")]
        [Tooltip("基准值。每个实例占 2 层：SpawnedPrefabInstance 背景 = base + index*2，其下 DynamicPlatform = base + index*2 + 1；下一实例整体在上方。")]
        public int baseSortingOrder = 0;

        /// <summary> 当前存活的 Prefab 实例列表，按生成顺序（0=最底，Count-1=最顶） </summary>
        readonly List<SpawnedPrefabInstance> _activeInstances = new List<SpawnedPrefabInstance>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Register(SpawnedPrefabInstance instance)
        {
            if (instance == null || _activeInstances.Contains(instance)) return;
            _activeInstances.Add(instance);
            RecalculateAll();
        }

        public void Unregister(SpawnedPrefabInstance instance)
        {
            if (instance == null) return;
            _activeInstances.Remove(instance);
            RecalculateAll();
        }

        /// <summary> 返回 instance 在叠层中的索引，0=最底。若未注册返回 -1。 </summary>
        public int GetStackIndex(SpawnedPrefabInstance instance)
        {
            return _activeInstances.IndexOf(instance);
        }

        /// <summary> 返回所有“在给定索引之上”的实例（叠层更高，会遮挡下方平台）。 </summary>
        public List<SpawnedPrefabInstance> GetInstancesAbove(int stackIndex)
        {
            var list = new List<SpawnedPrefabInstance>();
            for (int i = stackIndex + 1; i < _activeInstances.Count; i++)
                list.Add(_activeInstances[i]);
            return list;
        }

        /// <summary> 任一 Prefab 生成或销毁后调用，让所有 DynamicPlatform 按叠层重新计算并重建碰撞体，并更新绘制顺序。 </summary>
        public void RecalculateAll()
        {
            foreach (var instance in _activeInstances)
            {
                if (instance == null) continue;
                instance.RecalculateAllPlatforms();
            }
            UpdateAllSortingOrders();
        }

        /// <summary> 按叠层索引设置每个实例的 Sorting Order：每实例占 2 层（背景、平台），下一实例 base+2，与叠层逻辑一致。 </summary>
        void UpdateAllSortingOrders()
        {
            for (int i = 0; i < _activeInstances.Count; i++)
            {
                if (_activeInstances[i] != null)
                    _activeInstances[i].SetSortingOrder(baseSortingOrder + i * 2);
            }
        }
    }
}
