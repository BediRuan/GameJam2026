using cowsins2D;
using System.Collections;
using UnityEngine;

public class HpDrivenMovingPlatform : MonoBehaviour
{
    [Header("HP Source")]
    public PlayerHealth playerHealth;
    public string playerTag = "Player";

    [Header("HP To Position")]
    public Transform leftPoint;
    public Transform rightPoint;
    public bool lockY = true;

    float fixedY;
    Coroutine parentCo;

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

        // 直接同步，不做追随
        transform.position = target;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        RequestParent(other.transform);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        // 只在玩家在平台上方时才允许绑定
        if (other.transform.position.y <= transform.position.y) return;

        // 如果你有 Grapple / PlayerDependencies 才用；没有就删掉这两段
        Grapple grapple = other.GetComponent<Grapple>();
        if (grapple != null && grapple.isGrappling)
        {
            SafeUnparent(other.transform);
            return;
        }

        PlayerDependencies deps = other.GetComponent<PlayerDependencies>();
        if (deps != null && deps.InputManager != null && deps.InputManager.PlayerInputs.HorizontalMovement != 0)
        {
            SafeUnparent(other.transform);
            return;
        }

        RequestParent(other.transform);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        SafeUnparent(other.transform);
    }

    void RequestParent(Transform player)
    {
        if (player == null) return;

        // 平台如果已被禁用，就别开协程
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy) return;

        if (player.parent == transform) return;

        if (parentCo != null) StopCoroutine(parentCo);
        parentCo = StartCoroutine(DoParentNextFrame(player));
    }

    IEnumerator DoParentNextFrame(Transform player)
    {
        yield return null;

        if (player == null) yield break;
        if (!gameObject.activeInHierarchy) yield break;

        player.SetParent(transform);
    }

    void SafeUnparent(Transform player)
    {
        if (player == null) return;
        if (player.parent != transform) return;

        // 平台如果已被禁用，就别开协程
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
        {
            // 这里直接硬解绑也行（不需要协程了）
            player.SetParent(null);
            return;
        }

        if (parentCo != null) StopCoroutine(parentCo);
        StartCoroutine(DoUnparentNextFrame(player));

        IEnumerator DoUnparentNextFrame(Transform player)
        {
            yield return null;

            if (player == null) yield break;
            player.SetParent(null);
        }
    }
}
