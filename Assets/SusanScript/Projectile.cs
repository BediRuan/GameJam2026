using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 1;
    public float lifeSeconds = 6f;

    public string playerTag = "Player";
    public LayerMask groundMask;

    void Start()
    {
        Destroy(gameObject, lifeSeconds);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(playerTag))
        {
            PlayerHealth hp = collision.collider.GetComponent<PlayerHealth>();
            if (hp) hp.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        int layerBit = 1 << collision.collider.gameObject.layer;
        if ((groundMask.value & layerBit) != 0)
        {
            Destroy(gameObject);
        }
    }
}
