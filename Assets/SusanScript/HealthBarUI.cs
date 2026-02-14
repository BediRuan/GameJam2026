using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Image fillImage;

    void Start()
    {
        if (!playerHealth)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) playerHealth = p.GetComponent<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            playerHealth.onHpChanged += OnHpChanged;
            OnHpChanged(playerHealth.currentHp, playerHealth.maxHp);
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.onHpChanged -= OnHpChanged;
    }

    void OnHpChanged(int cur, int max)
    {
        if (!fillImage) return;
        fillImage.fillAmount = max <= 0 ? 0f : (float)cur / max;
    }
}
