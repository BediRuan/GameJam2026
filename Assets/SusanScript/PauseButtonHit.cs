using System.Collections;
using UnityEngine;

public class PauseButtonHit : MonoBehaviour
{
    public float requireRelativeUpVelocity = 0.2f;
    public float requireBelowTolerance = 0.02f;

    public float bumpUp = 0.18f;
    public float bumpTime = 0.08f;
    public float hitAlpha = 0.6f;

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

        // 关键：不要用玩家rb的linearVelocity，用碰撞相对速度判断“是否从下往上顶”
        // relativeVelocity 是 other 相对 this 的速度
        bool movingUpIntoButton = collision.relativeVelocity.y > requireRelativeUpVelocity;

        // 关键：用bounds判断玩家确实在按钮下方
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

        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.TogglePause();
        }
        else
        {
            Debug.LogError("PauseManager.Instance is null");
        }
    }

    IEnumerator BumpRoutine()
    {
        // 如果按钮是会被移动的，最好每次触发重新记一次当前位置
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
    }
}
