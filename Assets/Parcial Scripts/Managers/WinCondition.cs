using UnityEngine;
using UnityEngine.SceneManagement;

public class WinCondition : MonoBehaviour
{
    private EnemyControllerFSM restartScene;
    [SerializeField] private int sceneNPCs;

    [SerializeField] private int savedNPCCounter;

    public int SavedNPCCounter { get { return savedNPCCounter; } set { savedNPCCounter = value; } }

    private string finishGameScene = "Win";

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (savedNPCCounter == sceneNPCs)
            {
                Scene currentScene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(finishGameScene);
            }

        }
    }

    public void addSavedNPC()
    {
        savedNPCCounter++;
    }
}

