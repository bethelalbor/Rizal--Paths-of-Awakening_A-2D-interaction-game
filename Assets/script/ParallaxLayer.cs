using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public Transform player;
    public float parallaxEffect = 0.9f;
    public bool moveY = false;

    private Vector3 startObjectPosition;
    private Vector3 startPlayerPosition;

    void Start()
    {
        startObjectPosition = transform.position;
        startPlayerPosition = player.position;
    }

    void LateUpdate()
    {
        Vector3 delta = player.position - startPlayerPosition;

        transform.position = new Vector3(
            startObjectPosition.x + delta.x * parallaxEffect,
            moveY ? startObjectPosition.y + delta.y * parallaxEffect : startObjectPosition.y,
            startObjectPosition.z
        );
    }
}