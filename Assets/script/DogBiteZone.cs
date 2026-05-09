using UnityEngine;

public class DogBiteZone : MonoBehaviour
{
    public DogPuzzleManager dogPuzzleManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            dogPuzzleManager.TriggerDogAttack();
        }
    }
}