using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;

    [SerializeField] private Transform visualRoot;

    private float moveInput;
    private float originalVisualScaleX;

    private bool canMove = true;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (visualRoot != null)
            originalVisualScaleX = Mathf.Abs(visualRoot.localScale.x);
    }

    void Update()
    {
        if (!canMove)
        {
            moveInput = 0f;

            if (anim != null)
                anim.SetBool("isMoving", false);

            return;
        }

        moveInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                moveInput = -1f;
            }
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                moveInput = 1f;
            }
        }

        if (anim != null)
            anim.SetBool("isMoving", moveInput != 0f);

        if (visualRoot != null)
        {
            if (moveInput > 0)
            {
                visualRoot.localScale = new Vector3(
                    originalVisualScaleX,
                    visualRoot.localScale.y,
                    visualRoot.localScale.z
                );
            }
            else if (moveInput < 0)
            {
                visualRoot.localScale = new Vector3(
                    -originalVisualScaleX,
                    visualRoot.localScale.y,
                    visualRoot.localScale.z
                );
            }
        }
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove && rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}