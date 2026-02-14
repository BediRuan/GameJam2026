using UnityEngine;

public class PausableObject : MonoBehaviour
{
    public bool freezeAnimator = true;
    public bool freezeRigidbody2D = true;
    public bool freezeParticles = true;

    public MonoBehaviour[] scriptsToDisable;

    Animator[] animators;
    float[] animatorSpeeds;

    Rigidbody2D[] rbs;
    bool[] rbSimulated;

    ParticleSystem[] particles;

    void Awake()
    {
        animators = GetComponentsInChildren<Animator>(true);
        animatorSpeeds = new float[animators.Length];

        rbs = GetComponentsInChildren<Rigidbody2D>(true);
        rbSimulated = new bool[rbs.Length];

        particles = GetComponentsInChildren<ParticleSystem>(true);
    }

    void OnEnable()
    {
        if (PauseManager.Instance != null)
            PauseManager.Instance.Register(this);
    }

    void OnDisable()
    {
        if (PauseManager.Instance != null)
            PauseManager.Instance.Unregister(this);
    }

    public void Pause()
    {
        if (freezeAnimator)
        {
            for (int i = 0; i < animators.Length; i++)
            {
                if (!animators[i]) continue;
                animatorSpeeds[i] = animators[i].speed;
                animators[i].speed = 0f;
            }
        }

        if (freezeRigidbody2D)
        {
            for (int i = 0; i < rbs.Length; i++)
            {
                if (!rbs[i]) continue;
                rbSimulated[i] = rbs[i].simulated;
                rbs[i].simulated = false;
            }
        }

        if (freezeParticles)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                if (!particles[i]) continue;
                particles[i].Pause(true);
            }
        }

        if (scriptsToDisable != null)
        {
            for (int i = 0; i < scriptsToDisable.Length; i++)
            {
                if (scriptsToDisable[i] != null)
                    scriptsToDisable[i].enabled = false;
            }
        }
    }

    public void Resume()
    {
        if (freezeAnimator)
        {
            for (int i = 0; i < animators.Length; i++)
            {
                if (!animators[i]) continue;
                animators[i].speed = animatorSpeeds[i];
            }
        }

        if (freezeRigidbody2D)
        {
            for (int i = 0; i < rbs.Length; i++)
            {
                if (!rbs[i]) continue;
                rbs[i].simulated = rbSimulated[i];
            }
        }

        if (freezeParticles)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                if (!particles[i]) continue;
                particles[i].Play(true);
            }
        }

        if (scriptsToDisable != null)
        {
            for (int i = 0; i < scriptsToDisable.Length; i++)
            {
                if (scriptsToDisable[i] != null)
                    scriptsToDisable[i].enabled = true;
            }
        }
    }
}
