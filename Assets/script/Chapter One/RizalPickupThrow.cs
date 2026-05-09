using UnityEngine;
using UnityEngine.InputSystem;

public class RizalPickupThrow : MonoBehaviour
{
    public Transform holdPoint;
    public float pickupRange = 1.2f;
    public float throwSpeedX = 6f;
    public float defaultThrowSpeedY = 4f;
    public LayerMask pickableLayer;

    public LineRenderer trajectoryLine;
    public int trajectoryPoints = 20;
    public float timeStep = 0.1f;

    public float minThrowSpeedY = 1.5f;
    public float maxThrowSpeedY = 8f;
    public float aimAdjustSpeed = 4f;

    [Header("Throw Collision Ignore")]
    public Collider2D[] extraThrowIgnoreColliders;

    // Turn this on if your sprite faces the opposite direction visually
    public bool invertFacingDirection = false;

    private GameObject heldObject;
    private Rigidbody2D heldRb;
    private Collider2D heldCollider;
    private SpriteRenderer playerSprite;

    private GameObject nearbyObject;
    private SpriteRenderer nearbySprite;
    private Color originalNearbyColor;

    private float currentThrowSpeedY;

    void Start()
    {
        playerSprite = GetComponent<SpriteRenderer>();
        currentThrowSpeedY = defaultThrowSpeedY;

        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
            trajectoryLine.positionCount = trajectoryPoints;
        }
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        CheckNearbyPickable();

        if (Keyboard.current.eKey.wasPressedThisFrame && heldObject == null)
        {
            TryPickup();
        }

        if (heldObject != null)
        {
            if (Keyboard.current.qKey.isPressed)
            {
                AdjustAim();

                if (trajectoryLine != null)
                {
                    trajectoryLine.enabled = true;
                    DrawTrajectory();
                }
            }

            if (Keyboard.current.qKey.wasReleasedThisFrame)
            {
                ThrowObject();
            }
        }
    }

    void LateUpdate()
    {
        if (heldObject != null)
        {
            heldObject.transform.position = holdPoint.position;
        }
    }

    void AdjustAim()
    {
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            currentThrowSpeedY += aimAdjustSpeed * Time.deltaTime;
        }

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            currentThrowSpeedY -= aimAdjustSpeed * Time.deltaTime;
        }

        currentThrowSpeedY = Mathf.Clamp(currentThrowSpeedY, minThrowSpeedY, maxThrowSpeedY);
    }

    float GetFacingDirection()
    {
        if (Keyboard.current == null)
            return 1f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            return -1f;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            return 1f;

        return transform.localScale.x < 0 ? -1f : 1f;
    }

    void CheckNearbyPickable()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, pickupRange, pickableLayer);

        if (hit != null)
        {
            if (nearbyObject != hit.gameObject)
            {
                ResetNearbyGlow();

                nearbyObject = hit.gameObject;
                nearbySprite = nearbyObject.GetComponent<SpriteRenderer>();

                if (nearbySprite != null)
                {
                    originalNearbyColor = nearbySprite.color;
                    nearbySprite.color = new Color(1f, 1f, 0.6f, 1f);
                }
            }
        }
        else
        {
            ResetNearbyGlow();
        }
    }

    void ResetNearbyGlow()
    {
        if (nearbySprite != null)
        {
            nearbySprite.color = originalNearbyColor;
        }

        nearbyObject = null;
        nearbySprite = null;
    }

    void TryPickup()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, pickupRange, pickableLayer);

        if (hit != null)
        {
            heldObject = hit.gameObject;
            heldRb = heldObject.GetComponent<Rigidbody2D>();
            heldCollider = heldObject.GetComponent<Collider2D>();

            ResetNearbyGlow();
            currentThrowSpeedY = defaultThrowSpeedY;

            if (heldRb != null)
            {
                heldRb.linearVelocity = Vector2.zero;
                heldRb.angularVelocity = 0f;
                heldRb.bodyType = RigidbodyType2D.Kinematic;
            }

            if (heldCollider != null)
            {
                heldCollider.isTrigger = true;
            }
        }
    }

    void ThrowObject()
    {
        if (heldObject == null || heldRb == null)
            return;

        if (heldCollider != null)
        {
            heldCollider.isTrigger = false;
        }
        IgnoreThrowCollisions();

        heldRb.bodyType = RigidbodyType2D.Dynamic;

        float direction = GetFacingDirection();


        // Set the exact launch velocity so the throw matches the preview more closely
        heldRb.linearVelocity = new Vector2(direction * throwSpeedX, currentThrowSpeedY);

        RockHitGuard rockHitGuard = heldObject.GetComponent<RockHitGuard>();
        if (rockHitGuard != null)
        {
            rockHitGuard.MarkAsThrown();
        }

        BoneDistractor boneDistractor = heldObject.GetComponent<BoneDistractor>();
        if (boneDistractor != null)
        {
            boneDistractor.MarkAsThrown();
        }

        heldObject = null;
        heldRb = null;
        heldCollider = null;

        if (trajectoryLine != null)
            trajectoryLine.enabled = false;
    }


    void IgnoreThrowCollisions()
    {
    if (heldCollider == null)
        return;

    // Ignore collision with Rizal's own colliders
    Collider2D[] playerColliders = GetComponentsInChildren<Collider2D>();

    foreach (Collider2D playerCol in playerColliders)
    {
        if (playerCol != null && playerCol != heldCollider)
        {
            Physics2D.IgnoreCollision(heldCollider, playerCol, true);
        }
    }

    // Ignore collision with assigned objects such as the dog collider
    if (extraThrowIgnoreColliders != null)
    {
        foreach (Collider2D col in extraThrowIgnoreColliders)
        {
            if (col != null)
            {
                Physics2D.IgnoreCollision(heldCollider, col, true);
            }
        }
    }
    }

    void DrawTrajectory()
    {
        if (heldObject == null || trajectoryLine == null || heldRb == null)
            return;

        float direction = GetFacingDirection();

        Vector2 startPos = holdPoint.position;
        Vector2 startVelocity = new Vector2(direction * throwSpeedX, currentThrowSpeedY);

        // Use the rigidbody's gravityScale so the line matches the object better
        Vector2 gravity = Physics2D.gravity * heldRb.gravityScale;

        Vector3[] points = new Vector3[trajectoryPoints];

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = i * timeStep;
            Vector2 point = startPos + startVelocity * t + 0.5f * gravity * t * t;
            points[i] = point;
        }

        trajectoryLine.positionCount = trajectoryPoints;
        trajectoryLine.SetPositions(points);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}