using UnityEngine;

public class NPCAnimationState : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Starting State")]
    [SerializeField] private bool startWalking;
    [SerializeField] private bool startTalking;

    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int IsTalking = Animator.StringToHash("isTalking");

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogWarning("NPCAnimationState: No Animator found on " + gameObject.name);
            return;
        }

        if (startWalking && startTalking)
        {
            Debug.LogWarning(gameObject.name + " has both Start Walking and Start Talking enabled. Talking will be prioritized.");
            startWalking = false;
        }

        animator.SetBool(IsWalking, startWalking);
        animator.SetBool(IsTalking, startTalking);
    }
}