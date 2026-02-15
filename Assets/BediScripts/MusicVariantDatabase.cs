using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Music Variant Database")]
public class MusicVariantDatabase : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string sceneName;   // 例如 "Level1"
        public AudioClip clip;     // 该关卡版本
    }

    public List<Entry> entries = new List<Entry>();

    private Dictionary<string, AudioClip> _map;

    public AudioClip GetClipForScene(string sceneName)
    {
        if (_map == null)
        {
            _map = new Dictionary<string, AudioClip>(StringComparer.Ordinal);
            foreach (var e in entries)
            {
                if (!string.IsNullOrEmpty(e.sceneName) && e.clip != null)
                    _map[e.sceneName] = e.clip;
            }
        }

        _map.TryGetValue(sceneName, out var clip);
        return clip;
    }
}
