using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace JiU
{
    /// <summary>
    /// 受伤时生成的伤害数字：上浮、可作为平台站立、支持暂停。
    /// 
    /// Prefab 搭建：
    /// 1. 根物体：DamageNumber + BoxCollider2D（Is Trigger 关闭，作为平台），Layer 设为 Platforms 或 MovingPlatforms
    /// 2. 子物体：Canvas (Render Mode: World Space, 小 Scale 如 0.01) + Text 或 TextMeshProUGUI 显示 "-1"
    /// 3. BoxCollider2D 的 size 设为可站立大小（如 1.5 x 0.2）
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class DamageNumber : MonoBehaviour
    {
        [Header("上浮")]
        [Tooltip("每秒上浮的垂直距离")]
        public float floatUpSpeed = 1.5f;

        [Header("生命周期")]
        [Tooltip("存在多少秒后销毁")]
        public float lifeTime = 3f;

        [Header("显示")]
        [Tooltip("优先使用：若指定则显示在此 TextMeshProUGUI 上")]
        public TMP_Text textTMP;
        [Tooltip("若未指定 textTMP，则使用此 Text")]
        public Text textUI;

        float _spawnTime;

        void Awake()
        {
            if (!textTMP)
                textTMP = GetComponentInChildren<TMP_Text>(true);
            if (!textUI)
                textUI = GetComponentInChildren<Text>(true);
        }

        /// <summary>
        /// 初始化伤害数字，设置显示文本。
        /// </summary>
        public void Init(int damageAmount)
        {
            string s = damageAmount > 0 ? "-" + damageAmount : "0";
            if (textTMP != null)
                textTMP.text = s;
            else if (textUI != null)
                textUI.text = s;
            _spawnTime = Time.time;
        }

        void Update()
        {
            // 暂停时不移动
            if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
                return;

            transform.position += Vector3.up * (floatUpSpeed * Time.deltaTime);

            if (Time.time - _spawnTime >= lifeTime)
                Destroy(gameObject);
        }
    }
}
