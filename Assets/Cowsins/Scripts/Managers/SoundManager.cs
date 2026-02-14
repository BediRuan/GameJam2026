using UnityEngine;
using System.Collections;
namespace cowsins2D
{
    public class SoundManager : MonoBehaviour
    {
        // Singleton pattern to ensure that there is only one SoundManager instance in the game.
        public static SoundManager Instance;

        // The AudioSource that will be used to play all sounds.
        private AudioSource source;

        private void Awake()
        {
            // If there is no existing SoundManager instance, then set this instance as the singleton.
            if (Instance == null)
            {
                Instance = this;
                AudioConfiguration config = AudioSettings.GetConfiguration();
                config.numRealVoices = 64;
                AudioSettings.Reset(config);
            }

            // Get the AudioSource component on this GameObject.
            source = GetComponent<AudioSource>();
        }

        public void PlaySound(AudioClip clip, float volume)
        {
            if (clip != null)
            {
                //source.volume = volume;
                source.PlayOneShot(clip);
            }
        }
    }
}
