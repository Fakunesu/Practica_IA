using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreenManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string nextSceneName = "MainMenu";

    [Header("Timing")]
    [SerializeField] private float splashDuration = 3f;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= splashDuration)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}