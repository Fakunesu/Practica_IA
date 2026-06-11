using UnityEngine;
using UnityEngine.SceneManagement;

public class WinMenu : MonoBehaviour
{
    public void BackToGame()
    {
        SceneManager.LoadScene("Parcial_1");
    }
}