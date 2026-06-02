using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환 하려고 씀

public class TitleManager : MonoBehaviour
{
    public void GameStart()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
