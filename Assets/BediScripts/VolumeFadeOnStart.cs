using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class VolumeFadeOnStart : MonoBehaviour
{
    [Header("Volume Reference")]
    public Volume targetVolume;

    [Header("Fade Settings")]
    public float fadeDuration = 3f; // Ω•±‰ ±º‰

    void Start()
    {
        if (targetVolume == null)
        {
            Debug.LogError("Volume not assigned!");
            return;
        }

        targetVolume.weight = 1f;
        StartCoroutine(FadeVolume());
    }

    IEnumerator FadeVolume()
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            targetVolume.weight = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        targetVolume.weight = 0f;
    }
}
