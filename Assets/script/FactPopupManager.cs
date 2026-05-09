using UnityEngine;

public class FactPopupManager : MonoBehaviour
{
    [Header("Fact Popup UI")]
    public GameObject factPopupPanel;

    [Header("Player")]
    public PlayerController playerController;
    public RizalPickupThrow pickupThrowScript;

    private bool popupOpen = false;

    void Start()
    {
        if (factPopupPanel != null)
            factPopupPanel.SetActive(false);
    }

    public void ShowFactPopup()
    {
        if (popupOpen)
            return;

        popupOpen = true;

        if (factPopupPanel != null)
            factPopupPanel.SetActive(true);

        if (playerController != null)
            playerController.SetCanMove(false);

        if (pickupThrowScript != null)
            pickupThrowScript.enabled = false;

        Time.timeScale = 0f;
    }

    public void CloseFactPopup()
    {
        Time.timeScale = 1f;

        if (factPopupPanel != null)
            factPopupPanel.SetActive(false);

        if (playerController != null)
            playerController.SetCanMove(true);

        if (pickupThrowScript != null)
            pickupThrowScript.enabled = true;

        popupOpen = false;
    }
}