using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine : MonoBehaviour
{
    [SerializeField] private string winSceneName = "Win";
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Finish Line Reached!");
            SceneManager.LoadScene(winSceneName);
        }
    }
}

