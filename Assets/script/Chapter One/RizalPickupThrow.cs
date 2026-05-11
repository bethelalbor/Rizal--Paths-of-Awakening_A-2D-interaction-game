using UnityEngine;
using UnityEngine.InputSystem;

public class RizalPickupThrow : MonoBehaviour
{
    [Header("Pickup / Throw")]
    public Transform holdPoint;
    public float pickupRange = 1.2f;
    public float throwSpeedX = 6f;
    public float defaultThrowSpeedY = 4f;
    public LayerMask pickableLayer;

    [Header("Trajectory")]
    public LineRenderer trajectoryLine;
    public int trajectoryPoints = 20;
    public float timeStep = 0.1f;

    [Header("Automatic Aim While Holding E")]
    public float minThrowSpeedY = 1.5f;
    public float maxThrowSpeedY = 8f;
    public float aimCycleSpeed = 2f;

    [Header("Throw Collision Ignore")]
    public Collider2D[] extraThrowIgnoreColliders;

    [Header("UI Guide")]
    public GameObject throwGuideSign;

    public bool invertFacingDirection = false;

    private GameObject heldObject;
    private Rigidbody2D heldRb;
    private Collider2D heldCollider;

    private GameObject nearbyObject;
    private SpriteRenderer nearbySprite;
    private Color originalNearbyColor;

    private float currentThrowSpeedY;
    private float aimTimer;

    private bool mustReleaseEAfterPickup = false;
    private bool isAimingThrow = false;

    void Start()
    {
        currentThrowSpeedY = defaultThrowSpeedY;

        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
            trajectoryLine.positionCount = trajectoryPoints;
        }

        if (throwGuideSign != null)
            throwGuideSign.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        CheckNearbyPickable();

        if (Keyboard.current.eKey.wasPressedThisFrame && heldObject == null)
        {
            TryPickup();
            return;
        }

        if (heldObject != null)
        {
            HandleThrowControls();
        }
    }

    void LateUpdate()
    {
        if (heldObject != null && holdPoint != null)
        {
            heldObject.transform.position = holdPoint.position;
        }
    }

    void HandleThrowControls()
    {
        // After picking up, player must release E once.
        // This prevents instant throw from the same E press used to pick up.
        if (mustReleaseEAfterPickup)
        {
            if (!Keyboard.current.eKey.isPressed)
            {
                mustReleaseEAfterPickup = false;
            }

            return;
        }

        // Hold E to aim. The trajectory height moves automatically.
        if (Keyboard.current.eKey.isPressed)
        {
            isAimingThrow = true;

            AutoAdjustAim();

            if (trajectoryLine != null)
            {
                trajectoryLine.enabled = true;
                DrawTrajectory();
            }
        }

        // Release E to throw using the last visible trajectory.
        if (isAimingThrow && Keyboard.current.eKey.wasReleasedThisFrame)
        {
            ThrowObject();
        }
    }

    void AutoAdjustAim()
    {
        aimTimer += Time.deltaTime * aimCycleSpeed;

        float t = (Mathf.Sin(aimTimer) + 1f) / 2f;

        currentThrowSpeedY = Mathf.Lerp(
            minThrowSpeedY,
            maxThrowSpeedY,
            t
        );
    }

    float GetFacingDirection()
    {
        if (Keyboard.current == null)
            return 1f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            return -1f;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            return 1f;

        float direction = transform.localScale.x < 0 ? -1f : 1f;

        if (invertFacingDirection)
            direction *= -1f;

        return direction;
    }

    void CheckNearbyPickable()
    {
        if (heldObject != null)
            return;

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

        if (hit == null)
            return;

        heldObject = hit.gameObject;
        heldRb = heldObject.GetComponent<Rigidbody2D>();
        heldCollider = heldObject.GetComponent<Collider2D>();

        ResetNearbyGlow();

        currentThrowSpeedY = defaultThrowSpeedY;
        aimTimer = 0f;

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

        mustReleaseEAfterPickup = true;
        isAimingThrow = false;

        if (throwGuideSign != null)
            throwGuideSign.SetActive(true);
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

        heldRb.linearVelocity = new Vector2(
            direction * throwSpeedX,
            currentThrowSpeedY
        );

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

        isAimingThrow = false;
        mustReleaseEAfterPickup = false;

        if (trajectoryLine != null)
            trajectoryLine.enabled = false;

        if (throwGuideSign != null)
            throwGuideSign.SetActive(false);
    }

    void IgnoreThrowCollisions()
    {
        if (heldCollider == null)
            return;

        Collider2D[] playerColliders = GetComponentsInChildren<Collider2D>();

        foreach (Collider2D playerCol in playerColliders)
        {
            if (playerCol != null && playerCol != heldCollider)
            {
                Physics2D.IgnoreCollision(heldCollider, playerCol, true);
            }
        }

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
        if (heldObject == null || trajectoryLine == null || heldRb == null || holdPoint == null)
            return;

        float direction = GetFacingDirection();

        Vector2 startPos = holdPoint.position;
        Vector2 startVelocity = new Vector2(direction * throwSpeedX, currentThrowSpeedY);

        Vector2 gravity = Physics2D.gravity * heldRb.gravityScale;

        Vector3[] points = new Vector3[trajectoryPoints];

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = i * timeStep;
            Vector2 point = startPos + startVelocity * t + 0.5f * gravity * t * t;

            points[i] = new Vector3(point.x, point.y, -1f);
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