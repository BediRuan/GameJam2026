using System.Collections;
using UnityEngine;

public class PlayerSpawnController : MonoBehaviour
{
    [Header("Spit FX")]
    public float spitDuration = 0.35f;

    void Start()
    {
        PlaceAtSpawnPoint();
        StartCoroutine(SpitInRoutine());
    }

    void PlaceAtSpawnPoint()
    {
        string needId = PortalTravelData.NextSpawnId;

        // 找匹配spawnId的SpawnPoint
        SpawnPoint[] points = FindObjectsOfType<SpawnPoint>();
        foreach (var p in points)
        {
            if (p.spawnId == needId)
            {
                transform.position = p.transform.position;
                return;
            }
        }

        // fallback：找tag为SpawnPoint
        GameObject tagged = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (tagged != null) transform.position = tagged.transform.position;
    }

    IEnumerator SpitInRoutine()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;

        Vector3 endScale = transform.localScale;
        transform.localScale = Vector3.zero;

        // 先淡入（黑到透明）
        if (SceneFader.Instance != null)
            yield return SceneFader.Instance.FadeIn();

        float t = 0f;
        while (t < spitDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / spitDuration);
            transform.localScale = Vector3.Lerp(Vector3.zero, endScale, k);
            yield return null;
        }

        transform.localScale = endScale;
        if (rb) rb.simulated = true;
    }
}
