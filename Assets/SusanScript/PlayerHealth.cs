using UnityEngine;

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
        if (currentHp <= 0) return;

        currentHp = Mathf.Min(maxHp, currentHp + amount);

        onHpChanged?.Invoke(currentHp, maxHp);
    }


    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player Dead");

        Destroy(gameObject);
    }
}
