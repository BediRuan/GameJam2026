using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHp = 5;
    public int currentHp;

    public System.Action<int, int> onHpChanged;

    void Awake()
    {
        currentHp = maxHp;
        onHpChanged?.Invoke(currentHp, maxHp);
    }

    public void TakeDamage(int amount)
    {
        currentHp = Mathf.Max(0, currentHp - amount);
        onHpChanged?.Invoke(currentHp, maxHp);

        if (currentHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player Dead");
        // 你后面再接死亡逻辑
    }
}
