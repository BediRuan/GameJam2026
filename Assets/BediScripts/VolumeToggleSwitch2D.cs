using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class VolumeToggleSwitch2D : MonoBehaviour
{
    [Header("Refs")]
    public GameObject volumeSliderObject;   // 你之前的“音量控制条object”（整个物体）
    public Sprite onSprite;
    public Sprite offSprite;

    [Header("Collision")]
    public string playerTag = "Player";
    public bool ignoreWhileCooldown = true;
    public float toggleCooldown = 0.2f;

    [Header("Start State")]
    public bool startOn = true;

    private SpriteRenderer sr;
    private float lastToggleTime = -999f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        SetState(startOn, true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag(playerTag)) return;
        TryToggle();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        TryToggle();
    }

    private void TryToggle()
    {
        if (ignoreWhileCooldown && Time.time - lastToggleTime < toggleCooldown) return;
        lastToggleTime = Time.time;

        SetState(!VolumeSwitchState.IsOn, false);
    }

    private void SetState(bool on, bool instant)
    {
        VolumeSwitchState.IsOn = on;

        // 显示/隐藏控制条对象
        if (volumeSliderObject != null)
            volumeSliderObject.SetActive(on);

        // 切换图片
        if (sr != null)
            sr.sprite = on ? onSprite : offSprite;

        // Off 时音量立刻归零（避免残留）
        if (!on)
            AudioListener.volume = 0f;
        else if (instant)
        {
            // on 的瞬间不强制音量，交给控制条脚本在 Update 里马上刷新
        }
    }
}
