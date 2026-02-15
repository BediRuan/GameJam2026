using System.Collections;
using UnityEngine;

public class IntroSequenceController : MonoBehaviour
{
    [Header("Fade")]
    public ScreenFader fader;
    public float blackHoldSeconds = 2f;
    public float fadeOutSeconds = 4f;

    [Header("Camera")]
    public Camera cam;
    public CameraFollow2D follow2D;
    public IntroCameraPan introPan;

    [Header("Camera Zoom (Orthographic)")]
    public bool useZoom = true;
    public float introZoom = 6.5f;
    public float followZoom = 4.5f;
    public float zoomSeconds = 0.25f;

    [Header("Dialogue")]
    public DialogueSequence introDialogue;      // 第一段
    public DialogueSequence landingDialogue;    // 第二段（5句）
    public DialogueSequence endingDialogue;     // 第三段（镜头扩展结束后）

    [Header("Player Spawn (NOT in scene)")]
    public GameObject playerPrefab;
    public Transform playerSpawnPoint;
    public bool spawnWithPhysicsDisabled = true;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public Vector2 fallbackGroundCheckOffset = new Vector2(0f, -0.6f);
    public float groundCheckRadius = 0.12f;

    [Header("After 2nd Dialogue: Widen + Up + Fixed View")]
    public bool doWidenAfterLandingDialogue = true;
    public float widenedOrthoSize = 7.5f;
    public float widenedExtraUpWorld = 0.8f;
    public float widenSeconds = 3.0f;

    [Header("Snap To Player (Spawn Cut)")]
    public float snapSeconds = 0.18f;

    [Header("Follow While Falling")]
    public float fallingFollowSpeed = 16f;

    [Header("Spawn Hole (destroy after landing)")]
    public GameObject spawnHoleObject;
    public float destroyHoleDelay = 0f;

    [Header("Door (show during 2nd dialogue)")]
    public GameObject doorToReveal;

    [Header("Landing Dialogue Door Trigger")]
    [Tooltip("第二段对话播到第几句结束时显示门（1-based）。4 = 第四句。")]
    public int revealDoorAfterLineNumber = 4;

    [Header("Door Reveal SFX")]
    public AudioSource doorRevealAudio;
    public AudioClip doorRevealClip; // ✅ 推荐用 OneShot（不会打断别的声音）

    Transform _playerFollowTarget;
    Rigidbody2D _rb;

    bool _introHasRun = false;
    GameObject _spawnedPlayer;

    bool _doorRevealed = false;

    void Start()
    {
        if (_introHasRun) return;
        _introHasRun = true;

        if (!cam) cam = Camera.main;
        if (doorToReveal) doorToReveal.SetActive(false);

        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        // 0) 黑屏
        if (fader) fader.SetBlackImmediate();
        if (blackHoldSeconds > 0f)
            yield return new WaitForSecondsRealtime(blackHoldSeconds);

        // 1) 开场展示
        if (follow2D)
        {
            follow2D.target = null;
            follow2D.enabled = false;
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

        // 3) 第一段
        if (introDialogue && introDialogue.lines != null && introDialogue.lines.Count > 0)
        {
            introDialogue.startDisabled = false;
            introDialogue.waitSecondsAfterGrounded = 0f;
            introDialogue.PlayNow();

            yield return new WaitUntil(() => introDialogue != null && introDialogue.IsPlaying);
            yield return WaitDialogueFullyDone(introDialogue);
        }

        // 4) Spawn Player
        SpawnPlayer();
        if (_playerFollowTarget == null) yield break;

        // 5) 切到玩家 + 坠落跟随
        if (introPan) introPan.enabled = false;
        if (follow2D) follow2D.enabled = false;

        yield return SnapToPlayer(snapSeconds);

        if (follow2D)
        {
            follow2D.offset = new Vector3(0f, 0f, -10f);
            follow2D.target = _playerFollowTarget;
            follow2D.SetFollowSpeed(fallingFollowSpeed);
            follow2D.enabled = true;
        }

        if (useZoom && cam && cam.orthographic)
            yield return ZoomTo(followZoom, zoomSeconds);

        if (_rb && spawnWithPhysicsDisabled)
            _rb.simulated = true;

        // 6) 等落地 + 删洞口
        yield return WaitUntilPlayerGrounded();
        if (spawnHoleObject)
        {
            if (destroyHoleDelay <= 0f) Destroy(spawnHoleObject);
            else Destroy(spawnHoleObject, destroyHoleDelay);
        }

        // 7) 第二段：第4句结束 reveal 门 + 播音效
        if (landingDialogue && landingDialogue.lines != null && landingDialogue.lines.Count > 0)
        {
            _doorRevealed = false;

            int targetIndex = Mathf.Max(1, revealDoorAfterLineNumber) - 1;

            // 先清一次避免重复绑定
            landingDialogue.OnLineFinished -= OnLandingLineFinished;
            landingDialogue.OnLineFinished += OnLandingLineFinished;

            void OnLandingLineFinished(int lineIndex)
            {
                if (_doorRevealed) return;
                if (lineIndex != targetIndex) return;

                RevealDoorOnce();
            }

            landingDialogue.startDisabled = false;
            landingDialogue.waitSecondsAfterGrounded = 0f;
            landingDialogue.player = _playerFollowTarget;
            landingDialogue.PlayNow();

            yield return WaitDialogueFullyDone(landingDialogue);

            landingDialogue.OnLineFinished -= OnLandingLineFinished;

            // ✅ 对话结束后断开 follow，避免“咔一下”再进 cinematic
            if (follow2D)
            {
                follow2D.enabled = false;
                follow2D.target = null;
            }

            yield return new WaitForEndOfFrame();

            // 8) 镜头扩展定格
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
                }
            }

            // 9) 扩展结束后：第三段 endingDialogue
            if (endingDialogue && endingDialogue.lines != null && endingDialogue.lines.Count > 0)
            {
                endingDialogue.startDisabled = false;
                endingDialogue.waitSecondsAfterGrounded = 0f;
                endingDialogue.player = _playerFollowTarget;

                endingDialogue.PlayNow();
                yield return WaitDialogueFullyDone(endingDialogue);
            }
        }
    }

    void RevealDoorOnce()
    {
        _doorRevealed = true;

        if (doorToReveal) doorToReveal.SetActive(true);

        // ✅ 音效：推荐 OneShot
        if (doorRevealAudio)
        {
            if (doorRevealClip) doorRevealAudio.PlayOneShot(doorRevealClip);
            else doorRevealAudio.Play();
        }
    }

    IEnumerator WaitDialogueFullyDone(DialogueSequence dlg)
    {
        if (dlg == null) yield break;
        yield return new WaitUntil(() => dlg == null || dlg.IsFinished);
        yield return new WaitForSecondsRealtime(0.06f);
    }

    void SpawnPlayer()
    {
        if (_spawnedPlayer != null) return;
        if (!playerPrefab || !playerSpawnPoint) return;

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
            k = k * k * (3f - 2f * k);
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
            k = k * k * (3f - 2f * k);

            cam.transform.position = Vector3.Lerp(start, end, k);
            yield return null;
        }

        cam.transform.position = end;
    }
}
