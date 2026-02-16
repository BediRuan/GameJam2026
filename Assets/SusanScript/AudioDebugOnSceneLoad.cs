using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioDebugOnSceneLoad : MonoBehaviour
{
    public AudioSource music;

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene s, LoadSceneMode mode)
    {
        int listeners = FindObjectsOfType<AudioListener>(true).Length;
        Debug.Log($"[AudioDebug] Scene={s.name} listeners={listeners} " +
                  $"Listener.volume={AudioListener.volume} pause={AudioListener.pause} " +
                  $"musicNull={(music == null)} playing={(music != null && music.isPlaying)} " +
                  $"musicEnabled={(music != null && music.enabled)} mute={(music != null && music.mute)} vol={(music != null ? music.volume : -1f)}");
    }
}
