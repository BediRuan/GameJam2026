using UnityEngine;
using UnityEngine.Events;
using cowsins2D;

namespace cowsins2D
{
    public class HpDrivenMovingPlatform : Trigger
    {
        [Header("HP Source")]
        public PlayerHealth playerHealth;
        public string playerTag = "Player";

        [Header("HP To Position")]
        public Transform leftPoint;
        public Transform rightPoint;
        public float followSpeed = 8f;
        public bool lockY = true;

        float fixedY;
        Transform currentPlayer;

        void Awake()
        {
            fixedY = transform.position.y;

            if (!playerHealth)
            {
                GameObject p = GameObject.FindGameObjectWithTag(playerTag);
                if (p) playerHealth = p.GetComponent<PlayerHealth>();
            }
        }

        void Update()
        {
            if (!playerHealth || !leftPoint || !rightPoint) return;

            float hp01 = playerHealth.maxHp <= 0 ? 0f : (float)playerHealth.currentHp / playerHealth.maxHp;
            hp01 = Mathf.Clamp01(hp01);

            Vector3 target = Vector3.Lerp(leftPoint.position, rightPoint.position, hp01);
            if (lockY) target.y = fixedY;

            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * followSpeed);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;

            // 进来就记录一下玩家引用
            currentPlayer = other.transform;

            TryParentPlayer(other.gameObject);
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;

            currentPlayer = other.transform;

            // 如果你项目里没有 Grapple / PlayerDependencies，这段就删掉
            // 有的话就保留，等同官方逻辑
            Grapple grapple = other.GetComponent<Grapple>();
            if (grapple != null && grapple.isGrappling)
            {
                other.transform.SetParent(null);
                return;
            }

            PlayerDependencies deps = other.GetComponent<PlayerDependencies>();
            if (deps != null && deps.InputManager != null && deps.InputManager.PlayerInputs.HorizontalMovement != 0)
            {
                other.transform.SetParent(null);
                return;
            }

            TryParentPlayer(other.gameObject);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;

            if (other.transform.parent == transform)
                other.transform.SetParent(null);

            if (currentPlayer == other.transform)
                currentPlayer = null;
        }

        void TryParentPlayer(GameObject player)
        {
            // 必须是玩家在平台上方才绑定，避免侧面/底下乱绑定
            if (player.transform.position.y > transform.position.y)
            {
                if (player.transform.parent != transform)
                    player.transform.SetParent(transform);
            }
        }
    }
}
