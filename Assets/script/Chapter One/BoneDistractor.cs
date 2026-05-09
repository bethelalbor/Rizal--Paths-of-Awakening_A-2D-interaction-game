using UnityEngine;

public class BoneDistractor : MonoBehaviour
{
    public DogPuzzleManager dogPuzzleManager;

    [Header("Landing")]
    public float freezeDelay = 0.05f;

    private bool hasBeenThrown = false;
    private bool alreadyUsed = false;

    private Rigidbody2D rb;
    private Collider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public void MarkAsThrown()
    {
        hasBeenThrown = true;
        alreadyUsed = false;

        if (col != null)
            col.isTrigger = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasBeenThrown)
            return;

        if (alreadyUsed)
            return;

        if (collision.gameObject.CompareTag("Player"))
            return;

        alreadyUsed = true;

        Invoke(nameof(FreezeBoneAndDistractDog), freezeDelay);
    }

    private void FreezeBoneAndDistractDog()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // After landing, the bone should no longer block anything.
        if (col != null)
            col.isTrigger = true;

        if (dogPuzzleManager != null)
        {
            dogPuzzleManager.DistractDog(transform.position);
        }
    }
}