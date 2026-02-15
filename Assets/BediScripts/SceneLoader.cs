using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneToLoad; // 在Inspector输入场景名字

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            LoadScene();
        }
    }

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("Scene name is empty!");
        }
    }
}
