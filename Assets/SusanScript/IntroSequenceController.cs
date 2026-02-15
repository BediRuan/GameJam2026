using System.Collections;
using UnityEngine;

public class IntroSequenceController : MonoBehaviour
{
    [Header("Fade")]
    public ScreenFader fader;
    public float blackHoldSeconds = 2f;   // 新增：黑屏停留

    public float fadeOutSeconds = 4;     // 你想慢就把它调大，比如 2.5~4

    [Header("Camera")]
    public Camera cam;
    public CameraFollow2D follow2D;      // MainCamera上的 CameraFollow2D
    public IntroCameraPan introPan;      // MainCamera上的 IntroCameraPan（可选）

    [Header("Camera Zoom (Orthographic)")]
    public bool useZoom = true;
    public float introZoom = 6.5f;
    public float followZoom = 4.5f;
    public float zoomSeconds = 0.25f;

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
    public Vector2 fallbackGroundCheckOffset = new Vector2(0f, -0.6f);
    public float groundCheckRadius = 0.12f;

    [Header("After 2nd Dialogue: Widen + Up + Fixed View")]
    public bool doWidenAfterLandingDialogue = true;

    [Tooltip("扩展后的镜头尺寸（越大视野越大）")]
    public float widenedOrthoSize = 7.5f;

    [Tooltip("扩展后额外向上看多少世界单位")]
    public float widenedExtraUpWorld = 0.8f;

    [Tooltip("扩展动画时长（建议 2~4 秒）")]
    public float widenSeconds = 3.0f;

    [Header("Snap To Player (Spawn Cut)")]
    [Tooltip("Spawn后相机切到玩家的平滑时间（0.12~0.25s 推荐）")]
    public float snapSeconds = 0.18f;

    [Header("Follow While Falling")]
    [Tooltip("坠落阶段相机跟随速度（10~20 推荐，越大越跟手）")]
    public float fallingFollowSpeed = 16f;

    [Header("Spawn Hole (destroy after landing)")]
    public GameObject spawnHoleObject;      // 洞口对象（场景里那个）
    public float destroyHoleDelay = 0f;     // 想延迟就填 0.3 / 1.0 之类

    [Header("Door (show after 2nd dialogue)")]
    public GameObject doorToReveal;         // 门对象（场景里那个，初始隐藏）


    Transform _playerFollowTarget;
    Rigidbody2D _rb;

    bool _introHasRun = false;
    GameObject _spawnedPlayer;



    void Reset()
    {
        if (!cam) cam = Camera.main;
        if (!follow2D && cam) follow2D = cam.GetComponent<CameraFollow2D>();
        if (!introPan && cam) introPan = cam.GetComponent<IntroCameraPan>();
    }

    void Start()
    {
        if (_introHasRun) return;
        _introHasRun = true;

        if (!cam) cam = Camera.main;
        StartCoroutine(Run());
        if (doorToReveal) doorToReveal.SetActive(false);


    }

    IEnumerator Run()
    {

        // 0) 开始黑屏
        if (fader) fader.SetBlackImmediate();
        // ✅ 0.5) 纯黑停留一段时间（用 realtime，不受 timeScale 影响）
        if (blackHoldSeconds > 0f)
            yield return new WaitForSecondsRealtime(blackHoldSeconds);

        // 1) 开场：不跟随，只展示
        if (follow2D)
        {
            follow2D.target = null;
            follow2D.enabled = false; // 开场展示期间别让它动相机
        }

        if (introPan)
        {
            introPan.enabled = true;
            introPan.ResetToA();
        }

        if (useZoom && cam && cam.orthographic)
            cam.orthographicSize = introZoom;

        // 2) 黑 -> 透明
        if (fader) yield return fader.FadeOut(fadeOutSeconds, realtime: true);

        // 3) 第一段对话：确保真正开始，然后等完全结束
        if (introDialogue)
        {
            if (introDialogue.lines == null || introDialogue.lines.Count == 0)
            {
                Debug.LogError("[Intro] introDialogue has no lines. Abort.");
                yield break;
            }

            introDialogue.startDisabled = false;
            introDialogue.waitSecondsAfterGrounded = 0f;
            introDialogue.PlayNow();

            yield return new WaitUntil(() => introDialogue != null && introDialogue.IsPlaying);
            yield return WaitDialogueFullyDone(introDialogue);
        }

        // 4) 生成玩家（只一次）
        SpawnPlayer();
        if (_playerFollowTarget == null)
        {
            Debug.LogError("[Intro] No follow target found on spawned player.");
            yield break;
        }

        // 5) 从展示镜头切到玩家：先关Pan/Follow，再做一个很短的平滑“咻过去”
        if (introPan) introPan.enabled = false;
        if (follow2D) follow2D.enabled = false;

        yield return SnapToPlayer(snapSeconds);

        // 5.5) 坠落阶段：开启跟随（不要太慢，否则追不上坠落）
        if (follow2D)
        {
            follow2D.offset = new Vector3(0f, 0f, -10f);
            follow2D.target = _playerFollowTarget;
            follow2D.SetFollowSpeed(fallingFollowSpeed);
            follow2D.enabled = true;
        }

        // 6) Zoom in（更贴身）
        if (useZoom && cam && cam.orthographic)
            yield return ZoomTo(followZoom, zoomSeconds);

        // 7) 启用物理让它坠落
        if (_rb && spawnWithPhysicsDisabled)
            _rb.simulated = true;

        // 8) 等落地
        yield return WaitUntilPlayerGrounded();
        // ✅ 落地后销毁洞口
        if (spawnHoleObject)
        {
            if (destroyHoleDelay <= 0f) Destroy(spawnHoleObject);
            else Destroy(spawnHoleObject, destroyHoleDelay);
        }


        // 9) 落地后第二段对话
        if (landingDialogue && landingDialogue.lines != null && landingDialogue.lines.Count > 0)
        {
            landingDialogue.startDisabled = false;
            landingDialogue.waitSecondsAfterGrounded = 0f;
            landingDialogue.player = _playerFollowTarget;
            landingDialogue.PlayNow();

            yield return WaitDialogueFullyDone(landingDialogue);

            // ✅✅ 关键修复：在 EndOfFrame 之前就断开 follow
            // 否则这一帧的 LateUpdate 会把相机“拉一下”，下一帧 cinematic 才开始 -> 你看到“咔一下”
            if (follow2D)
            {
                follow2D.enabled = false;
                follow2D.target = null;
            }

            if (doorToReveal) doorToReveal.SetActive(true);
            // ✅ 给一帧缓冲（此时不会再被 follow 拉相机）
            yield return new WaitForEndOfFrame();

            // 10) 第二段结束后：慢慢扩展 + 左下角锁定（扩展完固定镜头）
            if (doWidenAfterLandingDialogue && cam)
            {
                var cinematic = cam.GetComponent<CameraCinematicResize2D>();
                if (cinematic != null)
                {
                    yield return cinematic.WidenKeepBottomLeft(
                        targetOrthoSize: widenedOrthoSize,
                        extraUpWorld: widenedExtraUpWorld,
                        seconds: widenSeconds,
                        realtime: true
                    );

                    // 扩展完：固定镜头（不跟随）
                    if (follow2D)
                    {
                        follow2D.enabled = false;
                        follow2D.target = null;
                    }

                    if (introPan) introPan.enabled = false;
                }
                else
                {
                    Debug.LogWarning("[Intro] CameraCinematicResize2D not found on camera.");
                }
            }
        }
        else
        {
            Debug.LogWarning("[Intro] landingDialogue missing or empty lines.");
        }
    }

    IEnumerator WaitDialogueFullyDone(DialogueSequence dlg)
    {
        if (dlg == null) yield break;

        // 你现在用 IsFinished，OK
        yield return new WaitUntil(() => dlg == null || dlg.IsFinished);

        // 小保险：避免结束那一帧还在做吞键/销毁逻辑
        yield return new WaitForSecondsRealtime(0.06f);
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

        _spawnedPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);

        _rb = _spawnedPlayer.GetComponentInChildren<Rigidbody2D>(true);
        _playerFollowTarget = _rb ? _rb.transform : _spawnedPlayer.transform;

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
            if (_playerFollowTarget == null) yield break;

            Vector2 checkPos = (Vector2)_playerFollowTarget.position + fallbackGroundCheckOffset;
            bool grounded = Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundMask);

            if (grounded) yield break;
            yield return null;
        }
    }

    IEnumerator ZoomTo(float targetSize, float seconds)
    {
        if (!cam || !cam.orthographic) yield break;

        float start = cam.orthographicSize;
        float t = 0f;
        seconds = Mathf.Max(0.01f, seconds);

        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / seconds);
            k = k * k * (3f - 2f * k); // SmoothStep 更顺
            cam.orthographicSize = Mathf.Lerp(start, targetSize, k);
            yield return null;
        }

        cam.orthographicSize = targetSize;
    }

    IEnumerator SnapToPlayer(float seconds)
    {
        if (!cam || _playerFollowTarget == null) yield break;

        Vector3 start = cam.transform.position;
        Vector3 end = new Vector3(_playerFollowTarget.position.x, _playerFollowTarget.position.y, start.z);

        seconds = Mathf.Max(0.01f, seconds);
        float t = 0f;

        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / seconds);
            k = k * k * (3f - 2f * k); // SmoothStep，收尾不硬

            cam.transform.position = Vector3.Lerp(start, end, k);
            yield return null;
        }

        cam.transform.position = end;
    }
}
