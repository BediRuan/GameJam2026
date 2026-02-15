using System.Collections;
using UnityEngine;

namespace JiU
{
    /// <summary>
    /// 以 PauseButtonHit 为参考：玩家从下往上顶到该物体时触发。
    /// 左按钮：减少预设索引（到 0 为止）；右按钮：增加预设索引（到最后一个为止）。
    /// 自动读取场景中的 WindowZoomAndWallsController2D 及其预设数量，无需额外配置。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class WindowPresetButtonHit : MonoBehaviour
    {
        public enum ButtonSide { Left, Right }

        [Header("按钮方向")]
        [Tooltip("Left = 顶击后索引减一；Right = 顶击后索引加一")]
        public ButtonSide buttonSide = ButtonSide.Left;

        [Tooltip("若未指定，会在场景中查找 WindowZoomAndWallsController2D")]
        public WindowZoomAndWallsController2D windowZoomController;

        [Header("顶击判定 (同 PauseButtonHit)")]
        public float requireRelativeUpVelocity = 0.2f;
        public float requireBelowTolerance = 0.02f;
        public string playerTag = "Player";

        [Header("顶击反馈")]
        public float bumpUp = 0.18f;
        public float bumpTime = 0.08f;
        public float hitAlpha = 0.6f;

        SpriteRenderer _sr;
        Color _startColor;
        Vector3 _startPos;
        Coroutine _bumpCo;
        Collider2D _col;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _startColor = _sr.color;
            _col = GetComponent<Collider2D>();
            _startPos = transform.position;

            if (windowZoomController == null)
                windowZoomController = FindFirstObjectByType<WindowZoomAndWallsController2D>();
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.collider.CompareTag(playerTag)) return;

            bool movingUpIntoButton = collision.relativeVelocity.y > requireRelativeUpVelocity;

            bool playerIsBelow = true;
            if (_col != null)
            {
                float playerTop = collision.collider.bounds.max.y;
                float buttonBottom = _col.bounds.min.y;
                playerIsBelow = playerTop <= buttonBottom + requireBelowTolerance;
            }

            if (!movingUpIntoButton || !playerIsBelow) return;

            if (_bumpCo != null) StopCoroutine(_bumpCo);
            _bumpCo = StartCoroutine(BumpRoutine());

            if (windowZoomController == null)
            {
                Debug.LogWarning("[WindowPresetButtonHit] WindowZoomAndWallsController2D not found.");
                return;
            }

            if (buttonSide == ButtonSide.Left)
                windowZoomController.ApplyPreviousPreset();
            else
                windowZoomController.ApplyNextPreset();
        }

        IEnumerator BumpRoutine()
        {
            _startPos = transform.position;
            Vector3 upPos = _startPos + Vector3.up * bumpUp;

            if (_sr != null)
            {
                _sr.enabled = true;
                Color c = _startColor;
                c.a = hitAlpha;
                _sr.color = c;
            }

            float t = 0f;
            while (t < bumpTime)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / bumpTime);
                transform.position = Vector3.Lerp(_startPos, upPos, k);
                yield return null;
            }

            t = 0f;
            while (t < bumpTime)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / bumpTime);
                transform.position = Vector3.Lerp(upPos, _startPos, k);
                yield return null;
            }

            transform.position = _startPos;
            if (_sr != null)
                _sr.color = _startColor;

            _bumpCo = null;
        }
    }
}
