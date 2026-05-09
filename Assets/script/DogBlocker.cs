using UnityEngine;

public class DogBlocker : MonoBehaviour
{
    public DogPuzzleManager dogPuzzleManager;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (dogPuzzleManager != null)
            {
                dogPuzzleManager.TriggerDogAttack();
            }
        }
    }
}