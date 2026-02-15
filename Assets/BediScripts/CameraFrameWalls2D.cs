using UnityEngine;

public class CameraFrameWalls2D : MonoBehaviour
{
    public Camera targetCamera;

    [Header("Wall Settings")]
    [Tooltip("墙的厚度（世界单位）")]
    public float thickness = 1f;

    [Tooltip("墙比可视范围多出来的余量，防止高速穿透/边缘抖动")]
    public float margin = 0.2f;

    [Tooltip("墙的Z位置（2D一般无所谓，但保持一致更稳）")]
    public float wallZ = 0f;

    // 四面墙
    Transform left, right, top, bottom;

    float lastOrtho;
    float lastAspect;

    void Awake()
    {
        if (!targetCamera) targetCamera = Camera.main;
        EnsureWalls();
        ForceUpdate();
    }

    void LateUpdate()
    {
        if (!targetCamera || !targetCamera.orthographic) return;

        // 只在相机尺寸/比例变化时更新（性能更好）
        if (!Mathf.Approximately(lastOrtho, targetCamera.orthographicSize) ||
            !Mathf.Approximately(lastAspect, targetCamera.aspect))
        {
            ForceUpdate();
        }
    }

    public void ForceUpdate()
    {
        if (!targetCamera || !targetCamera.orthographic) return;

        lastOrtho = targetCamera.orthographicSize;
        lastAspect = targetCamera.aspect;

        // 计算相机当前可视范围（世界单位）
        float halfH = targetCamera.orthographicSize;
        float halfW = halfH * targetCamera.aspect;

        Vector3 c = targetCamera.transform.position;

        float leftX = c.x - halfW;
        float rightX = c.x + halfW;
        float topY = c.y + halfH;
        float bottomY = c.y - halfH;

        // 墙的长度要覆盖整个边
        float fullW = halfW * 2f + margin * 2f;
        float fullH = halfH * 2f + margin * 2f;

        // 左墙
        SetWall(left,
            new Vector3(leftX - thickness * 0.5f - margin, c.y, wallZ),
            new Vector2(thickness, fullH + thickness * 2f));

        // 右墙
        SetWall(right,
            new Vector3(rightX + thickness * 0.5f + margin, c.y, wallZ),
            new Vector2(thickness, fullH + thickness * 2f));

        // 上墙
        SetWall(top,
            new Vector3(c.x, topY + thickness * 0.5f + margin, wallZ),
            new Vector2(fullW + thickness * 2f, thickness));

        // 下墙
        SetWall(bottom,
            new Vector3(c.x, bottomY - thickness * 0.5f - margin, wallZ),
            new Vector2(fullW + thickness * 2f, thickness));
    }

    void EnsureWalls()
    {
        left = CreateWall("Wall_Left");
        right = CreateWall("Wall_Right");
        top = CreateWall("Wall_Top");
        bottom = CreateWall("Wall_Bottom");
    }

    Transform CreateWall(string name)
    {
        Transform t = transform.Find(name);
        if (t) return t;

        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        go.transform.position = Vector3.zero;

        // collider
        var col = go.AddComponent<BoxCollider2D>();
        col.usedByComposite = false;

        // 让它成为“静态碰撞体”：加一个 Rigidbody2D 并设为 Static（更稳）
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        rb.simulated = true;

        return go.transform;
    }

    void SetWall(Transform wall, Vector3 pos, Vector2 size)
    {
        wall.position = pos;

        var col = wall.GetComponent<BoxCollider2D>();
        col.size = size;
        col.offset = Vector2.zero;
    }
}
