using UnityEngine;

public class VolumeBarMover : MonoBehaviour
{
    [Header("Waypoints (size must be 3): Left, Middle, Right")]
    public Transform[] waypoints = new Transform[3];

    [Header("Move")]
    public float moveSpeed = 3f;
    public float arriveDistance = 0.02f;

    [Header("Collision")]
    public string playerTag = "Player";
    public bool ignoreWhileMoving = true;

    [Header("Start")]
    [Tooltip("0=Left, 1=Middle, 2=Right")]
    public int startIndex = 0;

    [Header("Global Volume By Position")]
    [Tooltip("Volume when object is at Left waypoint")]
    [Range(0f, 1f)] public float minVolume = 0.2f;

    [Tooltip("Volume when object is at Right waypoint")]
    [Range(0f, 1f)] public float maxVolume = 1.0f;

    [Tooltip("How fast volume follows position (0 = instant)")]
    public float volumeSmoothSpeed = 8f;

    private int currentIndex;
    private int dir = 1; // +1 往右，-1 往左
    private int targetIndex;
    private bool isMoving;

    private void Awake()
    {
        currentIndex = Mathf.Clamp(startIndex, 0, 2);
        targetIndex = currentIndex;

        if (waypoints[currentIndex] != null)
            transform.position = waypoints[currentIndex].position;

        dir = (currentIndex == 2) ? -1 : 1;

        // 初始化音量（避免开局突然跳）
        UpdateGlobalVolume(true);
    }

    private void Update()
    {
        // 移动
        if (isMoving)
        {
            Transform target = waypoints[targetIndex];
            if (target != null)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target.position,
                    moveSpeed * Time.deltaTime
                );

                if (Vector3.Distance(transform.position, target.position) <= arriveDistance)
                {
                    transform.position = target.position;
                    currentIndex = targetIndex;
                    isMoving = false;

                    if (currentIndex == 2) dir = -1;
                    else if (currentIndex == 0) dir = 1;
                }
            }
            else
            {
                isMoving = false;
            }
        }

        // 音量随位置变化（即使不移动也可保持正确）
        UpdateGlobalVolume(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag(playerTag)) return;
        if (ignoreWhileMoving && isMoving) return;
        TryStartMoveToNext();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (ignoreWhileMoving && isMoving) return;
        TryStartMoveToNext();
    }

    private void TryStartMoveToNext()
    {
        if (waypoints == null || waypoints.Length != 3) return;
        if (waypoints[0] == null || waypoints[1] == null || waypoints[2] == null) return;

        // ping-pong：0->1->2->1->0...
        int next = currentIndex + dir;

        // 到边界强制回到中间
        if (currentIndex == 2) next = 1;
        if (currentIndex == 0) next = 1;

        next = Mathf.Clamp(next, 0, 2);

        targetIndex = next;
        isMoving = (targetIndex != currentIndex);
    }

    private void UpdateGlobalVolume(bool instant)
    {
        if (waypoints == null || waypoints.Length < 3) return;
        if (waypoints[0] == null || waypoints[2] == null) return;

        float leftX = waypoints[0].position.x;
        float rightX = waypoints[2].position.x;

        // 避免左右重合导致除0
        float t = 0f;
        if (Mathf.Abs(rightX - leftX) > 0.0001f)
        {
            t = Mathf.InverseLerp(leftX, rightX, transform.position.x); // 0..1
        }

        float targetVolume = Mathf.Lerp(minVolume, maxVolume, t);

        if (instant || volumeSmoothSpeed <= 0f)
        {
            AudioListener.volume = targetVolume;
        }
        else
        {
            AudioListener.volume = Mathf.Lerp(AudioListener.volume, targetVolume, volumeSmoothSpeed * Time.deltaTime);
        }
    }
}