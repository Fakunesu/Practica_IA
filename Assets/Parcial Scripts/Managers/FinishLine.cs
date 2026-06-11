using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine : MonoBehaviour
{
    private EnemyControllerFSM restartScene;

    private string finishGameScene = "Win";
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(finishGameScene);
        }
    }
}

