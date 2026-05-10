using UnityEngine;

public class CloudMover : MonoBehaviour
{
    public float moveSpeed = 0.5f;

    void Update()
    {
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;
    }
}