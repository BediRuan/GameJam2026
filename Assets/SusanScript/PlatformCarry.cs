using UnityEngine;

public class PlatformCarry : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;
        collision.transform.SetParent(transform);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;
        collision.transform.SetParent(null);
    }
}
