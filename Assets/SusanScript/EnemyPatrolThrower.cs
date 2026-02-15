using UnityEngine;

public class EnemyPatrolThrower : MonoBehaviour
{
    [Header("Refs")]
    public Rigidbody2D rb;
    public Transform groundCheck;
    public Transform wallCheck;
    public Transform throwPoint;
    public GameObject projectilePrefab;

    [Header("Tags & Masks")]
    public string playerTag = "Player";
    public LayerMask groundMask;

    [Header("Move")]
    public float moveSpeed = 2.5f;
    public float groundCheckRadius = 0.12f;
    public float wallCheckDistance = 0.2f;

    [Header("Detect & Throw")]
    public float detectRange = 6f;
    public float throwCooldown = 1.2f;
    public float throwSpeed = 9f;
    public float throwUpBoost = 0.8f;

    [Header("Animation & Facing")]
    [Tooltip("不填则自动从子物体获取。驱动 Bool Moving、Int Facing(1=右 -1=左)，基础朝向为朝右。")]
    public Animator animator;
    public string movingParam = "Moving";
    public string facingParam = "Facing";

    /// <summary> 1=朝右，-1=朝左，基础朝向为朝右。 </summary>
    public int Facing => dir;

    int dir = 1;
    float nextThrowTime = 0f;
    Transform player;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        dir = 1;
        FlipVisual();
        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p) player = p.transform;
    }

    void FixedUpdate()
    {
        if (!player)
        {
            Patrol();
            return;
        }

        float dist = Vector2.Distance(rb.position, player.position);

        if (dist <= detectRange)
        {
            FacePlayer();
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (Time.time >= nextThrowTime)
            {
                Throw();
                nextThrowTime = Time.time + throwCooldown;
            }
        }
        else
        {
            Patrol();
        }

        UpdateAnimator();
    }

    void UpdateAnimator()
    {
        if (animator == null) return;
        if (!string.IsNullOrEmpty(movingParam))
            animator.SetBool(movingParam, Mathf.Abs(rb.linearVelocity.x) > 0.01f);
        if (!string.IsNullOrEmpty(facingParam))
            animator.SetInteger(facingParam, dir);
    }

    void Patrol()
    {
        bool hasGroundAhead = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
        bool hitWall = Physics2D.Raycast(wallCheck.position, Vector2.right * dir, wallCheckDistance, groundMask);

        if (!hasGroundAhead || hitWall)
        {
            dir *= -1;
            FlipVisual();
        }

        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }

    void FacePlayer()
    {
        int want = player.position.x >= transform.position.x ? 1 : -1;
        if (want != dir)
        {
            dir = want;
            FlipVisual();
        }
    }

    void FlipVisual()
    {
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * dir;
        transform.localScale = s;
    }

    void Throw()
    {
        if (!projectilePrefab || !throwPoint || !player) return;

        GameObject go = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);

        Rigidbody2D prb = go.GetComponent<Rigidbody2D>();
        if (!prb) return;

        Vector2 start = throwPoint.position;
        Vector2 target = player.position;

        float gravity = Mathf.Abs(Physics2D.gravity.y * prb.gravityScale);

        float height = Mathf.Max(1.5f, target.y - start.y + 1.5f);

        float timeUp = Mathf.Sqrt(2f * height / gravity);
        float timeDown = Mathf.Sqrt(2f * Mathf.Max(0.01f, height - (target.y - start.y)) / gravity);

        float totalTime = timeUp + timeDown;

        Vector2 velocity;

        velocity.x = (target.x - start.x) / totalTime;
        velocity.y = gravity * timeUp;

        prb.linearVelocity = velocity;
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
