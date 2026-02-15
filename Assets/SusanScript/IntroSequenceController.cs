using System.Collections;
using UnityEngine;

public class IntroSequenceController : MonoBehaviour
{
    [Header("Fade")]
    public ScreenFader fader;
    public float fadeOutSeconds = 1.2f;

    [Header("Camera")]
    public Camera cam;
    public CameraFollow2D follow2D;      // MainCamera上的 CameraFollow2D
    public IntroCameraPan introPan;      // MainCamera上的 IntroCameraPan（可选）

    [Header("Camera Zoom (Orthographic)")]
    public bool useZoom = true;
    public float introZoom = 6.5f;       // 开场展示镜头大小
    public float followZoom = 4.5f;      // 跟随玩家时 zoom in
    public float zoomSeconds = 0.25f;    // 缩放时间（realtime）

    [Header("Dialogue")]
    public DialogueSequence introDialogue;    // 第一段旁白
    public DialogueSequence landingDialogue;  // 落地后旁白

    [Header("Player Spawn (NOT in scene)")]
    public GameObject playerPrefab;
    public Transform playerSpawnPoint;

    [Tooltip("生成后先让RB不模拟，等相机切好再启用")]
    public bool spawnWithPhysicsDisabled = true;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public Transform groundCheckPointOnPlayer; // 可选：如果你的Player prefab里有GroundCheckPoint，拖这个“子物体引用”不方便；建议用Tag/Find或脚本取
    public Vector2 fallbackGroundCheckOffset = new Vector2(0f, -0.6f);
    public float groundCheckRadius = 0.12f;

    Transform _player;
    Rigidbody2D _rb;

    private bool _introHasRun = false;
    private GameObject _spawnedPlayer;


    void Start()
    {
        if (_introHasRun) return;
        _introHasRun = true;
        StartCoroutine(Run());
    }


    IEnumerator Run()
    {
        // 0) 开始黑屏
        if (fader) fader.SetBlackImmediate();

        // 1) 开场：不跟随，只展示
        if (follow2D) follow2D.target = null;
        if (introPan) { introPan.enabled = true; introPan.ResetToA(); }

        if (useZoom && cam && cam.orthographic)
            cam.orthographicSize = introZoom;

        // 2) 黑 -> 透明
        if (fader) yield return fader.FadeOut(fadeOutSeconds, realtime: true);

        // 3) 第一段对话
        if (introDialogue)
        {
            introDialogue.startDisabled = false;
            introDialogue.waitSecondsAfterGrounded = 0f;
            // 3) 第一段对话：必须确实开始播放，然后等它结束
            if (introDialogue == null)
            {
                Debug.LogError("[Intro] introDialogue is NULL. Not spawning player.");
                yield break;
            }

            if (introDialogue.lines == null || introDialogue.lines.Count == 0)
            {
                Debug.LogError("[Intro] introDialogue has no lines. Not spawning player.");
                yield break;
            }

            introDialogue.startDisabled = false;
            introDialogue.waitSecondsAfterGrounded = 0f;

            Debug.Log("[Intro] Play intro dialogue");
            introDialogue.PlayNow();

            // 等它真的进入播放状态（避免“没开始就直接结束”）
            yield return new WaitUntil(() => introDialogue.IsPlaying);

            // 再等结束
            yield return new WaitUntil(() => !introDialogue.IsPlaying);

            Debug.Log("[Intro] Intro dialogue finished. Now spawning player...");
            SpawnPlayer();

        }

        // 4) 对话结束：生成玩家
        SpawnPlayer();

        // 5) 相机 shua 到玩家 + 切回跟随
        if (cam && _player)
        {
            Vector3 cp = cam.transform.position;
            Vector3 pp = _player.position;
            cam.transform.position = new Vector3(pp.x, pp.y, cp.z);
        }

        if (introPan) introPan.enabled = false;
        if (follow2D) follow2D.target = _player;

        // 6) Zoom in
        if (useZoom && cam && cam.orthographic)
            yield return ZoomTo(followZoom, zoomSeconds);

        // 7) 启用物理让它坠落
        if (_rb && spawnWithPhysicsDisabled)
            _rb.simulated = true;

        // 8) 等落地
        yield return WaitUntilPlayerGrounded();

        // 9) 落地后第二段对话
        if (landingDialogue)
        {
            landingDialogue.startDisabled = false;
            landingDialogue.waitSecondsAfterGrounded = 0f;
            landingDialogue.PlayNow();
            yield return new WaitUntil(() => !landingDialogue.IsPlaying);
        }
    }

    void SpawnPlayer()
    {
        if (_spawnedPlayer != null)
        {
            Debug.LogWarning("[Intro] SpawnPlayer called again. Ignored.");
            return;
        }

        if (!playerPrefab || !playerSpawnPoint)
        {
            Debug.LogError("[Intro] playerPrefab / playerSpawnPoint missing.");
            return;
        }

        Debug.Log("[Intro] Spawning player ONCE.");

        _spawnedPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
        _rb = _spawnedPlayer.GetComponentInChildren<Rigidbody2D>(true);
        _player = _rb ? _rb.transform : _spawnedPlayer.transform;

        if (_rb && spawnWithPhysicsDisabled)
        {
            _rb.simulated = false;
            _rb.linearVelocity = Vector2.zero;
        }
    }


    IEnumerator WaitUntilPlayerGrounded()
    {
        yield return null;

        while (true)
        {
            if (_player == null) yield break;

            Vector2 checkPos;

            // 如果你Player prefab里有一个GroundCheck子物体，最推荐：在Player脚底放个空物体并在生成后用Find获取
            // 这里给一个安全的fallback：用玩家位置 + offset
            checkPos = (Vector2)_player.position + fallbackGroundCheckOffset;

            bool grounded = Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundMask);
            if (grounded) yield break;

            yield return null;
        }
    }

    IEnumerator ZoomTo(float targetSize, float seconds)
    {
        float start = cam.orthographicSize;
        float t = 0f;
        seconds = Mathf.Max(0.01f, seconds);

        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / seconds);
            cam.orthographicSize = Mathf.Lerp(start, targetSize, k);
            yield return null;
        }

        cam.orthographicSize = targetSize;
    }
}
