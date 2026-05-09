using UnityEngine;

public class FactPopupTrigger : MonoBehaviour
{
    public FactPopupManager factPopupManager;

    private bool canTrigger = false;
    private bool hasTriggered = false;

    void Start()
    {
        canTrigger = false;
    }

    public void EnableTrigger()
    {
        canTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canTrigger)
            return;

        if (hasTriggered)
            return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;

            if (factPopupManager != null)
                factPopupManager.ShowFactPopup();
        }
    }
}