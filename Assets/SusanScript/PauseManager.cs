using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    public bool IsPaused { get; private set; }

    public GameObject pauseOverlay;

    readonly List<PausableObject> pausableObjects = new List<PausableObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        PausableObject[] all = FindObjectsOfType<PausableObject>(true);
        for (int i = 0; i < all.Length; i++)
            Register(all[i]);
    }


    public void Register(PausableObject obj)
    {
        if (obj != null && !pausableObjects.Contains(obj))
            pausableObjects.Add(obj);
    }

    public void Unregister(PausableObject obj)
    {
        if (obj != null) pausableObjects.Remove(obj);
    }

    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    public void SetPaused(bool paused)
    {
        if (IsPaused == paused) return;
        IsPaused = paused;

        for (int i = 0; i < pausableObjects.Count; i++)
        {
            if (pausableObjects[i] == null) continue;
            if (paused) pausableObjects[i].Pause();
            else pausableObjects[i].Resume();
        }

        if (pauseOverlay != null)
            pauseOverlay.SetActive(paused);
    }
}
