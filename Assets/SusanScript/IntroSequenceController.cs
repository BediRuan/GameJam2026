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

    [Header("After 2nd Dialogue: Widen + Up + Slow Follow")]
    public bool doWidenAfterLandingDialogue = true;

    [Tooltip("扩展后的镜头尺寸（越大视野越大）")]
    public float widenedOrthoSize = 7.5f;

    [Tooltip("扩展后额外向上看多少世界单位")]
    public float widenedExtraUpWorld = 0.8f;

    [Tooltip("扩展动画时长（建议 2~4 秒）")]
    public float widenSeconds = 3.0f;

    [Tooltip("扩展完成后：慢跟随速度")]
    public float slowFollowSpeed = 2.5f;

    [Tooltip("扩展完成后：相机偏移（让玩家不在中心，更多显示右上远景）")]
    public Vector3 widenedFollowOffset = new Vector3(3.5f, 1.2f, -10f);

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

            // 等它真的开始播放
            yield return new WaitUntil(() => introDialogue != null && introDialogue.IsPlaying);

            // 等它真正结束（推荐 IsFinished）
            yield return WaitDialogueFullyDone(introDialogue);
        }

        // 4) 生成玩家（只一次）
        SpawnPlayer();

        if (_playerFollowTarget == null)
        {
            Debug.LogError("[Intro] No follow target found on spawned player.");
            yield break;
        }

        // 5) 相机“咻一下”平滑切到玩家（替代硬切，避免突兀）
        if (introPan) introPan.enabled = false;
        if (follow2D) follow2D.enabled = false;     // 避免 LateUpdate 抢相机
        yield return SnapToPlayer(0.18f);           // 0.12~0.25 都可，越小越“快”

        // 5.5) 切回跟随（让坠落时相机能追上，但不慢）
        if (follow2D)
        {
            follow2D.offset = new Vector3(0f, 0f, -10f);
            follow2D.target = _playerFollowTarget;
            follow2D.SetFollowSpeed(16f);  // 跟手一点（10~20 之间试）
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

        // 9) 落地后第二段对话 + 对话结束后一帧缓冲 + 扩展镜头
        if (landingDialogue && landingDialogue.lines != null && landingDialogue.lines.Count > 0)
        {
            landingDialogue.startDisabled = false;
            landingDialogue.waitSecondsAfterGrounded = 0f;
            landingDialogue.player = _playerFollowTarget;

            landingDialogue.PlayNow();

            // 等它真正结束
            yield return WaitDialogueFullyDone(landingDialogue);

            // ✅ 关键：给“结束对话那一帧”喘口气（避免卡一下）
            yield return new WaitForEndOfFrame();

            // 10) 第二段结束后：慢慢扩展 + 左下角锁定
            if (doWidenAfterLandingDialogue && cam)
            {
                var cinematic = cam.GetComponent<CameraCinematicResize2D>();
                if (cinematic != null)
                {
                    // 扩展期间关跟随（避免拉回玩家）
                    if (follow2D) follow2D.enabled = false;

                    yield return cinematic.WidenKeepBottomLeft(
                        targetOrthoSize: widenedOrthoSize,
                        extraUpWorld: widenedExtraUpWorld,
                        seconds: widenSeconds,
                        realtime: true
                    );

                    // ✅ 扩展完：固定镜头，不再跟随
                    if (follow2D)
                    {
                        follow2D.target = null;
                        follow2D.enabled = false;
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


        IEnumerator WaitDialogueFullyDone(DialogueSequence dlg)
        {
            if (dlg == null) yield break;

            // 如果你加了 IsFinished，就等它真正完成（含吞键、恢复时间、销毁前）
            // 否则退化：等 IsPlaying=false 后再等一小段 realtime，避免“还没走完EndRoutine”
            float safetyRealtime = 0.12f;

            // 优先走 IsFinished
            // 注意：如果你的 DialogueSequence 还没加 IsFinished，这行会编译报错
            // -> 你就按我下面“DialogueSequence补丁”加上即可
            yield return new WaitUntil(() => dlg == null || dlg.IsFinished);

            // 额外保险
            yield return new WaitForSecondsRealtime(safetyRealtime);
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

            // 相机跟随：跟“有 Rigidbody2D 的那层级”
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
}
