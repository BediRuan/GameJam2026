using UnityEngine;

public class PlatformToggleSwitch : MonoBehaviour
{
    [Header("Which objects to toggle (one group)")]
    [SerializeField] private GameObject platformGroup;   // 平台组父物体

    [Header("Player tag")]
    [SerializeField] private string playerTag = "Player";

    [Header("Start state")]
    [SerializeField] private bool startVisible = false;

    private bool isVisible;

    private void Awake()
    {
        if (platformGroup == null)
        {
            Debug.LogError($"[{name}] platformGroup is not assigned!");
            return;
        }

        isVisible = startVisible;
        platformGroup.SetActive(isVisible);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        Toggle();
    }

    private void Toggle()
    {
        isVisible = !isVisible;
        if (platformGroup != null)
            platformGroup.SetActive(isVisible);
    }
}
