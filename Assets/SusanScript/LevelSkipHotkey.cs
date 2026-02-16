using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSkipHotkey : MonoBehaviour
{
    [Header("Hotkey")]
    public KeyCode nextKey = KeyCode.Tab;
    public bool allowShiftGoPrevious = true;

    [Header("Behavior")]
    public bool loopAtEnd = false;          // 到最后一关是否回到第一关
    public bool requireCtrl = false;        // 防误触：需要按住Ctrl才生效
    public bool allowInBuildOnly = false;   // true=只在Build里可用（Editor里禁用）

    private void Update()
    {
        if (allowInBuildOnly && Application.isEditor) return;

        if (!Input.GetKeyDown(nextKey)) return;

        if (requireCtrl && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            return;

        bool goPrev = allowShiftGoPrevious && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        int current = SceneManager.GetActiveScene().buildIndex;
        int total = SceneManager.sceneCountInBuildSettings;

        if (total <= 0)
        {
            Debug.LogWarning("[LevelSkipHotkey] No scenes in Build Settings.");
            return;
        }

        int target = goPrev ? current - 1 : current + 1;

        if (target >= total)
        {
            if (!loopAtEnd) return;
            target = 0;
        }
        else if (target < 0)
        {
            if (!loopAtEnd) return;
            target = total - 1;
        }

        SceneManager.LoadScene(target);
    }
}
