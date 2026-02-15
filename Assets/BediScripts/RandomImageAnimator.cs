using UnityEngine;

public class RandomImageAnimator : MonoBehaviour
{
    public Animator animator;

    public string[] clipNames;   // 填入 4 个 clip 的名字

    private string currentClip;

    void Start()
    {
        PlayRandom();
    }

    void PlayRandom()
    {
        string nextClip = clipNames[Random.Range(0, clipNames.Length)];

        // 防止连续播放同一个
        while (nextClip == currentClip && clipNames.Length > 1)
        {
            nextClip = clipNames[Random.Range(0, clipNames.Length)];
        }

        currentClip = nextClip;

        animator.Play(currentClip);

        // 获取当前动画长度
        float clipLength = GetClipLength(currentClip);

        Invoke(nameof(PlayRandom), clipLength);
    }

    float GetClipLength(string clipName)
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }

        return 1f;
    }
}
