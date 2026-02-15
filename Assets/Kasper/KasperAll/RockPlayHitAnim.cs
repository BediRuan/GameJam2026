using UnityEngine;

public class RockPlayHitAnim : MonoBehaviour
{
    private Animator anim;
    private static readonly int Hit = Animator.StringToHash("Hit");

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // 空气墙的 Collider2D 勾了 Is Trigger 时，用这个
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("AirWall"))
        {
            anim.SetTrigger(Hit);
        }
    }

    // 如果你没用 Trigger，而是正常碰撞，用这个（两者保留也没问题）
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("AirWall"))
        {
            anim.SetTrigger(Hit);
        }
    }
}