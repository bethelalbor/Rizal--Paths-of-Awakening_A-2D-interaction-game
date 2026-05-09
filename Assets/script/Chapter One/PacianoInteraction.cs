using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Cinemachine;

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

    [Header("Notice Wall Movement")]
    public Transform noticeWallMoveTarget;

    [Header("Notice Wall Dialogue")]
    [TextArea(2, 5)]
    public string noticeWallLine =
        "Paciano: They connect their names to the mutiny, but many still ask where the proof is.";

    public AudioClip noticeWallVoice;
    public float noticeWallDialogueDuration = 5f;

    [Header("Notice Wall Camera")]
    public CinemachineCamera cinemachineCamera;
    public Transform rizalCameraTarget;
    public Transform noticeWallCameraTarget;
    public float noticeWallCameraHoldBeforeDialogue = 1f;

    [Header("Notice Wall Camera Zoom")]
    public float normalCameraSize = 5f;
    public float noticeWallCameraSize = 3f;
    public float zoomDuration = 0.5f;

    [Header("Fact Popup")]
    public FactPopupTrigger noticeFactTrigger;

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

    private bool walkingToNoticeWall = false;
    private bool reachedNoticeWall = false;
    private bool noticeWallDialogueTriggered = false;
    private bool noticeWallDialoguePlaying = false;

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

        if (playerNearby && !dialogueStarted && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartDialogue();
            return;
        }

        if (dialogueStarted && !dialogueFinished && Keyboard.current.eKey.wasPressedThisFrame)
        {
            GoToNextDialogue();
            return;
        }

        if (walkingToTarget)
        {
            MovePacianoToDogTarget();
        }

        if (walkingToNoticeWall)
        {
            MovePacianoToNoticeWall();
        }

        if (reachedDogTarget && playerNearby && !dogIntroTriggered)
        {
            TriggerDogIntro();
        }

        if (reachedNoticeWall && playerNearby && !noticeWallDialogueTriggered && !noticeWallDialoguePlaying)
        {
            StartCoroutine(PlayNoticeWallDialogueRoutine());
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

        FaceMoveTarget(moveTarget);

        SetPacianoAnimation(false, true);
    }

    private void MovePacianoToDogTarget()
    {
        if (moveTarget == null)
            return;

        FaceMoveTarget(moveTarget);

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

            // Do NOT re-enable Paciano's body collider here.
            // InteractionZone will still trigger Rizal's approach.
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

    public void MoveToNoticeWallAfterDogSolved()
    {
        if (noticeWallMoveTarget == null)
            return;

        if (walkingToNoticeWall || reachedNoticeWall)
            return;

        walkingToNoticeWall = true;
        reachedDogTarget = false;

        if (leadSign != null)
            leadSign.SetActive(true);

        if (physicalCollider != null)
            physicalCollider.enabled = false;

        FaceMoveTarget(noticeWallMoveTarget);
        SetPacianoAnimation(false, true);
    }

    private void MovePacianoToNoticeWall()
    {
        if (noticeWallMoveTarget == null)
            return;

        FaceMoveTarget(noticeWallMoveTarget);

        Vector3 targetPosition = new Vector3(
            noticeWallMoveTarget.position.x,
            transform.position.y,
            transform.position.z
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        float distance = Mathf.Abs(transform.position.x - noticeWallMoveTarget.position.x);

        if (distance <= stopDistance)
        {
            walkingToNoticeWall = false;
            reachedNoticeWall = true;

            if (leadSign != null)
                leadSign.SetActive(false);

            SetPacianoAnimation(false, false);

            // Do NOT re-enable Paciano's body collider here.
            // Rizal must be able to pass through Paciano.
        }
    }

    private IEnumerator PlayNoticeWallDialogueRoutine()
    {
        noticeWallDialogueTriggered = true;
        noticeWallDialoguePlaying = true;

        if (cinemachineCamera != null && noticeWallCameraTarget != null)
        {
            cinemachineCamera.Follow = noticeWallCameraTarget;
            yield return StartCoroutine(ZoomCameraRoutine(noticeWallCameraSize));
        }

        yield return new WaitForSeconds(noticeWallCameraHoldBeforeDialogue);

        if (captionPanel != null)
            captionPanel.SetActive(true);

        if (captionText != null)
            captionText.text = noticeWallLine;

        SetPacianoAnimation(true, false);

        if (voiceSource != null && noticeWallVoice != null)
        {
            voiceSource.Stop();
            voiceSource.clip = noticeWallVoice;
            voiceSource.Play();

            yield return new WaitForSeconds(noticeWallVoice.length);
        }
        else
        {
            yield return new WaitForSeconds(noticeWallDialogueDuration);
        }

        if (voiceSource != null && voiceSource.isPlaying)
            voiceSource.Stop();

        if (captionPanel != null)
            captionPanel.SetActive(false);

        SetPacianoAnimation(false, false);

        if (cinemachineCamera != null && rizalCameraTarget != null)
        {
            yield return StartCoroutine(ZoomCameraRoutine(normalCameraSize));
            cinemachineCamera.Follow = rizalCameraTarget;
        }

        noticeWallDialoguePlaying = false;

        if (noticeFactTrigger != null)
        {
            noticeFactTrigger.EnableTrigger();
        }
    }

    private IEnumerator ZoomCameraRoutine(float targetSize)
    {
    if (cinemachineCamera == null)
        yield break;

    float startSize = cinemachineCamera.Lens.OrthographicSize;
    float elapsed = 0f;

    while (elapsed < zoomDuration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / zoomDuration;

        cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, t);

        yield return null;
    }

    cinemachineCamera.Lens.OrthographicSize = targetSize;
    }

    private void FaceMoveTarget(Transform target)
    {
        if (target == null)
            return;

        if (target.position.x > transform.position.x)
        {
            FaceRight();
        }
        else if (target.position.x < transform.position.x)
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

        if (reachedDogTarget)
        {
            if (!dogIntroTriggered)
            {
                TriggerDogIntro();
            }

            return;
        }

        if (reachedNoticeWall)
        {
            if (!noticeWallDialogueTriggered && !noticeWallDialoguePlaying)
            {
                StartCoroutine(PlayNoticeWallDialogueRoutine());
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