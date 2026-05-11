using UnityEngine;
using UnityEngine.SceneManagement;

public class UnfinishedAreaManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject unfinishedPanel;

    [Header("Player")]
    public PlayerController playerController;
    public RizalPickupThrow pickupThrowScript;

    [Header("Scene")]
    public string titleSceneName = "TitleScreen";

    private bool isOpen = false;

    void Start()
    {
        if (unfinishedPanel != null)
            unfinishedPanel.SetActive(false);
    }

    public void ShowUnfinishedScreen()
    {
        if (isOpen)
            return;

        isOpen = true;

        if (unfinishedPanel != null)
            unfinishedPanel.SetActive(true);

        if (playerController != null)
            playerController.SetCanMove(false);

        if (pickupThrowScript != null)
            pickupThrowScript.enabled = false;

        Time.timeScale = 0f;
    }

    public void ContinueRoaming()
    {
        Time.timeScale = 1f;

        if (unfinishedPanel != null)
            unfinishedPanel.SetActive(false);

        if (playerController != null)
            playerController.SetCanMove(true);

        if (pickupThrowScript != null)
            pickupThrowScript.enabled = true;

        isOpen = false;
    }

    public void RestartFromTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(titleSceneName);
    }
}