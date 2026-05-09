using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PacianoInteraction : MonoBehaviour
{
    [Header("UI Signs")]
    public GameObject pressESign;
    public GameObject leadSign;

    [Header("Caption UI")]
    public GameObject captionPanel;
    public TMP_Text captionText;

    [TextArea(2, 5)]
    public string[] dialogueLines;

    [Header("Voice Narration")]
    public AudioSource voiceSource;
    public AudioClip[] dialogueVoices;

    [Header("Movement")]
    public Transform moveTarget;
    public float moveSpeed = 1.2f;
    public float stopDistance = 0.05f;

    [Header("Visual Direction")]
    public Transform pacianoVisualRoot;

    [Tooltip("Turn this on if Paciano faces the wrong direction.")]
    public bool invertFacing = false;

    [Header("Animation")]
    public Animator pacianoAnimator;

    [Header("Dog Puzzle")]
    public DogPuzzleManager dogPuzzleManager;

    private bool playerNearby = false;
    private bool dialogueStarted = false;
    private bool dialogueFinished = false;
    private bool walkingToTarget = false;

    private bool reachedDogTarget = false;
    private bool dogIntroTriggered = false;

    private int currentDialogueIndex = 0;
    private Collider2D physicalCollider;

    private float originalVisualScaleX = 1f;

    void Start()
    {
        physicalCollider = GetComponent<Collider2D>();

        if (pacianoVisualRoot != null)
            originalVisualScaleX = Mathf.Abs(pacianoVisualRoot.localScale.x);

        if (pressESign != null)
            pressESign.SetActive(false);

        if (leadSign != null)
            leadSign.SetActive(false);

        if (captionPanel != null)
            captionPanel.SetActive(false);

        SetPacianoAnimation(false, false);

        FaceLeft();
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        // First Paciano interaction before he walks to the dog
        if (playerNearby && !dialogueStarted && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartDialogue();
            return;
        }

        // Continue first Paciano dialogue
        if (dialogueStarted && !dialogueFinished && Keyboard.current.eKey.wasPressedThisFrame)
        {
            GoToNextDialogue();
            return;
        }

        // Paciano walking to dog target
        if (walkingToTarget)
        {
            MovePacianoToTarget();
        }

        // After Paciano reached the dog target, trigger dog warning once
        // when Rizal approaches Paciano again.
        if (reachedDogTarget && playerNearby && !dogIntroTriggered)
        {
            TriggerDogIntro();
        }
    }

    private void StartDialogue()
    {
        dialogueStarted = true;
        currentDialogueIndex = 0;

        FaceLeft();

        if (pressESign != null)
            pressESign.SetActive(false);

        if (captionPanel != null)
            captionPanel.SetActive(true);

        SetPacianoAnimation(true, false);

        ShowCurrentDialogue();
    }

    private void GoToNextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex >= dialogueLines.Length)
        {
            FinishDialogue();
        }
        else
        {
            ShowCurrentDialogue();
        }
    }

    private void ShowCurrentDialogue()
    {
        if (captionText != null && dialogueLines.Length > 0)
        {
            captionText.text = dialogueLines[currentDialogueIndex];
        }

        if (voiceSource != null)
        {
            voiceSource.Stop();

            if (dialogueVoices != null && currentDialogueIndex < dialogueVoices.Length)
            {
                if (dialogueVoices[currentDialogueIndex] != null)
                {
                    voiceSource.clip = dialogueVoices[currentDialogueIndex];
                    voiceSource.Play();
                }
            }
        }
    }

    private void FinishDialogue()
    {
        dialogueFinished = true;

        if (captionPanel != null)
            captionPanel.SetActive(false);

        if (voiceSource != null && voiceSource.isPlaying)
            voiceSource.Stop();

        if (physicalCollider != null)
            physicalCollider.enabled = false;

        walkingToTarget = true;

        if (leadSign != null)
            leadSign.SetActive(true);

        FaceMoveTarget();

        SetPacianoAnimation(false, true);
    }

    private void MovePacianoToTarget()
    {
        if (moveTarget == null)
            return;

        FaceMoveTarget();

        Vector3 targetPosition = new Vector3(
            moveTarget.position.x,
            transform.position.y,
            transform.position.z
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        float distance = Mathf.Abs(transform.position.x - moveTarget.position.x);

        if (distance <= stopDistance)
        {
            walkingToTarget = false;
            reachedDogTarget = true;

            if (leadSign != null)
                leadSign.SetActive(false);

            SetPacianoAnimation(false, false);

            // Do NOT start the dog intro here anymore.
            // It will start only when Rizal approaches Paciano at the dog area.
        }
    }

    private void TriggerDogIntro()
    {
        dogIntroTriggered = true;

        if (dogPuzzleManager != null)
        {
            dogPuzzleManager.StartDogIntroOnce();
        }
    }

    private void FaceMoveTarget()
    {
        if (moveTarget == null)
            return;

        if (moveTarget.position.x > transform.position.x)
        {
            FaceRight();
        }
        else if (moveTarget.position.x < transform.position.x)
        {
            FaceLeft();
        }
    }

    private void FaceLeft()
    {
        if (pacianoVisualRoot == null)
            return;

        Vector3 scale = pacianoVisualRoot.localScale;

        if (!invertFacing)
            scale.x = -originalVisualScaleX;
        else
            scale.x = originalVisualScaleX;

        pacianoVisualRoot.localScale = scale;
    }

    private void FaceRight()
    {
        if (pacianoVisualRoot == null)
            return;

        Vector3 scale = pacianoVisualRoot.localScale;

        if (!invertFacing)
            scale.x = originalVisualScaleX;
        else
            scale.x = -originalVisualScaleX;

        pacianoVisualRoot.localScale = scale;
    }

    private void SetPacianoAnimation(bool talking, bool walking)
    {
        if (pacianoAnimator == null)
            return;

        pacianoAnimator.SetBool("isTalking", talking);
        pacianoAnimator.SetBool("isWalking", walking);
    }

    public void PlayerEnteredInteractionZone()
    {
        playerNearby = true;

        // If Paciano is already at the dog area, do not show Press E anymore.
        // The dog warning will trigger automatically.
        if (reachedDogTarget)
        {
            if (!dogIntroTriggered)
            {
                TriggerDogIntro();
            }

            return;
        }

        if (!dialogueStarted && pressESign != null)
            pressESign.SetActive(true);
    }

    public void PlayerExitedInteractionZone()
    {
        playerNearby = false;

        if (!dialogueStarted && pressESign != null)
            pressESign.SetActive(false);
    }
}