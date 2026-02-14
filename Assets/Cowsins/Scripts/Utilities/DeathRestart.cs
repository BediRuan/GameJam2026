using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace cowsins2D
{
    public class DeathRestart : MonoBehaviour
    {
        [SerializeField] private PlayerDependencies playerDependencies;
        [SerializeField] private Animator container;

        private CheckPointManager checkPointManager;

        private void Start()
        {
            container.gameObject.SetActive(false);
            checkPointManager = playerDependencies.CheckpointManager;
        }

        private void Update()
        {
            // Avoid showing the death screen if there exists a checkpoint already
            // Avoid running the reload code if container is not active either
            if (checkPointManager != null && checkPointManager.lastCheckpoint != null || !container.gameObject.activeInHierarchy) return;

            if (Keyboard.current.rKey.wasPressedThisFrame) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ShowDeathScreen()
        {
            checkPointManager = playerDependencies.CheckpointManager;

            if (checkPointManager.lastCheckpoint == null)
            {
                container.gameObject.SetActive(true);
                container.SetTrigger("PlayDeath"); 
            }
        }
    }
}