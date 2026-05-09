using UnityEngine;
using UnityEngine.SceneManagement;

public class FailUIManager : MonoBehaviour
{
    public void RetryScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}