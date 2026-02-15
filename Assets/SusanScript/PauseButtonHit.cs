using System.Collections;
using UnityEngine;
using UnityEngine.Rendering; // ✅ Volume 在这里

public class PauseButtonHit : MonoBehaviour
{
    public float requireRelativeUpVelocity = 0.2f;
    public float requireBelowTolerance = 0.02f;

    public float bumpUp = 0.18f;
    public float bumpTime = 0.08f;
    public float hitAlpha = 0.6f;

    [Header("Sprite Swap")]
    public bool swapSpriteOnRecover = true;
    public Sprite spriteAfterRecover;   // 图片2（开始）
    public bool onlySwapOnce = true;

    [Header("Volume Weight Toggle")]
    public Volume targetVolume;         // ✅ 拖你的 Volume 进来
    public float weightWhenOn = 1f;      // 默认切到 1
    public float weightWhenOff = 0f;     // 默认切回 0

    bool swapped = false;

    SpriteRenderer sr;
    Color startColor;
    Vector3 startPos;
    Coroutine bumpCo;

    Collider2D myCol;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) startColor = sr.color;

        myCol = GetComponent<Collider2D>();
        startPos = transform.position;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        bool movingUpIntoButton = collision.relativeVelocity.y > requireRelativeUpVelocity;

        bool playerIsBelow = true;
        if (myCol != null)
        {
            float playerTop = collision.collider.bounds.max.y;
            float buttonBottom = myCol.bounds.min.y;
            playerIsBelow = playerTop <= buttonBottom + requireBelowTolerance;
        }

        if (!movingUpIntoButton || !playerIsBelow) return;

        if (bumpCo != null) StopCoroutine(bumpCo);
        bumpCo = StartCoroutine(BumpRoutine());

        // ✅ 先切 volume 的 weight（或你也可以放到 TogglePause 后面）
        ToggleVolumeWeight();

        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.TogglePause();
        }
        else
        {
            Debug.LogError("PauseManager.Instance is null");
        }
    }

    void ToggleVolumeWeight()
    {
        if (targetVolume == null) return;

        // 认为 “接近 0” 就算是关；否则算开
        bool isOff = targetVolume.weight <= 0.001f;
        targetVolume.weight = isOff ? weightWhenOn : weightWhenOff;
    }

    IEnumerator BumpRoutine()
    {
        startPos = transform.position;
        Vector3 upPos = startPos + Vector3.up * bumpUp;

        if (sr != null)
        {
            sr.enabled = true;
            Color c = startColor;
            c.a = hitAlpha;
            sr.color = c;
        }

        float t = 0f;
        while (t < bumpTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / bumpTime);
            transform.position = Vector3.Lerp(startPos, upPos, k);
            yield return null;
        }

        t = 0f;
        while (t < bumpTime)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / bumpTime);
            transform.position = Vector3.Lerp(upPos, startPos, k);
            yield return null;
        }

        transform.position = startPos;

        if (sr != null)
            sr.color = startColor;

        // ⭐恢复时刻切换图片2
        if (sr != null && swapSpriteOnRecover && spriteAfterRecover != null)
        {
            if (!onlySwapOnce || !swapped)
            {
                sr.sprite = spriteAfterRecover;
                swapped = true;
            }
        }
    }
}
