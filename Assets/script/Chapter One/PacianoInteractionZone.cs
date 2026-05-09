using UnityEngine;

public class PacianoInteractionZone : MonoBehaviour
{
    public PacianoInteraction pacianoInteraction;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            pacianoInteraction.PlayerEnteredInteractionZone();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            pacianoInteraction.PlayerExitedInteractionZone();
        }
    }
}