using UnityEngine;

public class UnfinishedAreaTrigger : MonoBehaviour
{
    public UnfinishedAreaManager unfinishedAreaManager;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered)
            return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;

            if (unfinishedAreaManager != null)
                unfinishedAreaManager.ShowUnfinishedScreen();
        }
    }
}