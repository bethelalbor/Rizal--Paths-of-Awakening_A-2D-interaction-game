using UnityEngine;

public class RockHitGuard : MonoBehaviour
{
    public GameObject failPanel;

    private bool hasBeenThrown = false;

    public void MarkAsThrown()
    {
        hasBeenThrown = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasBeenThrown)
            return;

        if (collision.gameObject.CompareTag("Guard"))
        {
            if (failPanel != null)
            {
                failPanel.SetActive(true);
            }

            Time.timeScale = 0f;
        }
    }
}