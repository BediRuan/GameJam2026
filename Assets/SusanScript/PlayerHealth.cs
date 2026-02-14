using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHp = 5;
    public int currentHp;

    public System.Action<int, int> onHpChanged;

    bool isDead = false;

    void Awake()
    {
        currentHp = maxHp;
        onHpChanged?.Invoke(currentHp, maxHp);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHp = Mathf.Max(0, currentHp - amount);
        onHpChanged?.Invoke(currentHp, maxHp);

        if (currentHp <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHp = Mathf.Min(maxHp, currentHp + amount);
        onHpChanged?.Invoke(currentHp, maxHp);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player Dead Reload Scene");

        StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        // 先禁用玩家，避免相机/平台/子弹还在读玩家产生 MissingReference 或继续受伤
        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr) sr.enabled = false;

        yield return new WaitForSeconds(0.5f);

        // 如果你有 SceneFader，就先淡出
        if (SceneFader.Instance != null)
            yield return SceneFader.Instance.FadeOut();
        else
            yield return new WaitForSeconds(0.15f);

        // 重新加载当前关卡
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
