using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Target Scene")]
    public string targetSceneName = "Level2";

    [Header("Optional Random")]
    public bool useRandomFromList = false;
    public string[] randomSceneList;

    [Header("Spawn In Next Scene")]
    public string nextSpawnId = "Default";

    [Header("Behavior")]
    public bool consumeAfterUse = true;
    public float suckDuration = 0.45f;

    [Header("FX (Optional)")]
    public Animator animator;
    public string triggerAnim = "Use";
    public SpriteRenderer spriteToHide; // 控制洞口可见性的SpriteRenderer组件

    bool _used = false;

    void Reset()
    {
        // 自动获取Animator组件用于播放洞口动画
        animator = GetComponent<Animator>();

        // 自动获取SpriteRenderer组件用于隐藏洞口
        spriteToHide = GetComponent<SpriteRenderer>();

        // 确保Collider为Trigger模式以检测进入事件
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 如果已经使用过则不再触发
        if (_used) return;

        // 只响应带有Player标签的对象
        if (!other.CompareTag("Player")) return;

        // 启动传送流程协程
        StartCoroutine(TeleportRoutine(other.gameObject));
    }

    IEnumerator TeleportRoutine(GameObject player)
    {
        // 标记为已使用，防止重复触发
        _used = true;

        // 禁用自身碰撞体避免再次进入
        var myCol = GetComponent<Collider2D>();
        if (myCol) myCol.enabled = false;

        // 暂停玩家物理系统防止吸入过程中产生移动
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // 播放洞口动画
        if (animator) animator.SetTrigger(triggerAnim);

        // 将玩家逐渐缩小模拟被吸入效果
        Vector3 startScale = player.transform.localScale;
        float t = 0f;
        while (t < suckDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / suckDuration);
            player.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, k);
            yield return null;
        }

        // 隐藏洞口视觉元素使其看起来已经消失
        if (consumeAfterUse)
        {
            if (spriteToHide) spriteToHide.enabled = false;

            // 禁用所有子物体以隐藏洞口相关视觉效果
            foreach (Transform child in transform)
                child.gameObject.SetActive(false);
        }

        // 记录下一场景的出生点标识
        PortalTravelData.NextSpawnId = string.IsNullOrEmpty(nextSpawnId) ? "Default" : nextSpawnId;

        // 根据设置选择目标场景
        string sceneToLoad = targetSceneName;
        if (useRandomFromList && randomSceneList != null && randomSceneList.Length > 0)
        {
            sceneToLoad = randomSceneList[Random.Range(0, randomSceneList.Length)];
        }

        // 执行淡出效果使画面变黑
        if (SceneFader.Instance != null)
            yield return SceneFader.Instance.FadeOut();

        // 加载目标场景完成传送
        SceneManager.LoadScene(sceneToLoad);
    }
}
