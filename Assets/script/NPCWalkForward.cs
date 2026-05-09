using UnityEngine;

public class NPCWalkForward : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private bool moveOnStart = true;

    private int direction;

    private void Awake()
    {
        direction = transform.localScale.x < 0 ? -1 : 1;
    }

    private void Update()
    {
        if (!moveOnStart)
            return;

        transform.position += Vector3.right * direction * moveSpeed * Time.deltaTime;
    }
}