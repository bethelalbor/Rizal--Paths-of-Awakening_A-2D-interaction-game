using System.Collections;
using UnityEngine;
using TMPro;

public class DogPuzzleManager : MonoBehaviour
{
    [Header("Caption UI")]
    public GameObject captionPanel;
    public TMP_Text captionText;

    [TextArea(2, 5)]
    public string pacianoWarningLine =
        "Paciano: Do not force your way through. Look around. There is always another way.";

    public float warningDuration = 5f;

    [Header("Voice Narration")]
    public AudioSource voiceSource;
    public AudioClip pacianoWarningVoice;

    [Header("Fail UI")]
    public GameObject failPanel;
    public TMP_Text failText;

    [TextArea(2, 5)]
    public string failMessage =
        "Rizal was hurt before reaching Bagumbayan. Some dangers cannot be solved by force.";

    [Header("Player")]
    public Transform rizal;
    public PlayerController playerController;
    public RizalPickupThrow pickupThrowScript;
    public Transform respawnPoint;

    [Header("Dog")]
    public Transform dog;
    public Transform dogVisualRoot;
    public Animator dogAnimator;
    public Collider2D dogBlockCollider;
    public Collider2D dogBiteTrigger;

    public float dogMoveSpeed = 2f;
    public float dogStopDistance = 0.1f;

    [Tooltip("Check this if the dog faces the wrong way.")]
    public bool invertDogFacing = true;

    [Header("Dog Animator Parameters")]
    public string dogWalkBoolName = "isWalk";
    public string dogSniffBoolName = "isSniffing";
    public string dogBiteTriggerName = "bite";
    public string dogIdleStateName = "dog_idle";
    public float biteAnimationDuration = 1.2f;

    [Header("Testing")]
    public bool activateDogDangerAtStartForTesting = false;

    private bool introHasPlayed = false;
    private bool dogIsDangerous = false;
    private bool dogIsDistracted = false;
    private bool dogMovingToBone = false;
    private bool failInProgress = false;

    private Vector3 boneTargetPosition;
    private float originalDogScaleX = 1f;

    private Coroutine biteRoutine;

    void Start()
    {
        if (dogVisualRoot != null)
            originalDogScaleX = Mathf.Abs(dogVisualRoot.localScale.x);

        if (captionPanel != null)
            captionPanel.SetActive(false);

        if (failPanel != null)
            failPanel.SetActive(false);

        if (dogBiteTrigger != null)
            dogBiteTrigger.enabled = false;

        if (dogBlockCollider != null)
            dogBlockCollider.enabled = true;

        ForceDogIdle();

        if (activateDogDangerAtStartForTesting)
        {
            ActivateDogDanger();
        }
    }

    void Update()
    {
        if (dogMovingToBone)
        {
            MoveDogToBone();
        }
    }

    public void StartDogIntroOnce()
    {
        if (introHasPlayed)
            return;

        introHasPlayed = true;
        StartCoroutine(PlayDogIntroRoutine());
    }

    private IEnumerator PlayDogIntroRoutine()
    {
        if (captionPanel != null)
            captionPanel.SetActive(true);

        if (captionText != null)
            captionText.text = pacianoWarningLine;

        if (voiceSource != null && pacianoWarningVoice != null)
        {
            voiceSource.Stop();
            voiceSource.clip = pacianoWarningVoice;
            voiceSource.Play();
        }

        yield return new WaitForSeconds(warningDuration);

        if (voiceSource != null && voiceSource.isPlaying)
            voiceSource.Stop();

        if (captionPanel != null)
            captionPanel.SetActive(false);

        ActivateDogDanger();
    }

    private void ActivateDogDanger()
    {
        dogIsDangerous = true;
        dogIsDistracted = false;
        failInProgress = false;

        if (dogBiteTrigger != null)
            dogBiteTrigger.enabled = true;

        if (dogBlockCollider != null)
            dogBlockCollider.enabled = true;

        ForceDogIdle();
    }

    public void TriggerDogAttack()
    {
        if (!dogIsDangerous)
            return;

        if (dogIsDistracted)
            return;

        if (failInProgress)
            return;

        failInProgress = true;

        if (playerController != null)
            playerController.SetCanMove(false);

        if (pickupThrowScript != null)
            pickupThrowScript.enabled = false;

        FaceRizal();

        if (failPanel != null)
            failPanel.SetActive(true);
        else
            Debug.LogWarning("DogPuzzleManager: Fail Panel is not assigned.");

        if (failText != null)
            failText.text = failMessage;
        else
            Debug.LogWarning("DogPuzzleManager: Fail Text is not assigned.");

        if (biteRoutine != null)
            StopCoroutine(biteRoutine);

        biteRoutine = StartCoroutine(PlayBiteOnceRoutine());
    }

    private IEnumerator PlayBiteOnceRoutine()
    {
        if (dogAnimator == null)
            yield break;

        SetDogWalking(false);
        SetDogSniffing(false);

        if (!string.IsNullOrEmpty(dogBiteTriggerName))
        {
            dogAnimator.ResetTrigger(dogBiteTriggerName);
            dogAnimator.SetTrigger(dogBiteTriggerName);
        }

        yield return new WaitForSeconds(biteAnimationDuration);

        ForceDogIdle();
    }

    public void RetryAfterDogFail()
    {
        if (failPanel != null)
            failPanel.SetActive(false);

        if (rizal != null && respawnPoint != null)
            rizal.position = respawnPoint.position;

        if (playerController != null)
            playerController.SetCanMove(true);

        if (pickupThrowScript != null)
            pickupThrowScript.enabled = true;

        failInProgress = false;

        // The dog is still dangerous after retry because the player has not solved the puzzle yet.
        dogIsDangerous = true;
        dogIsDistracted = false;

        if (dogBiteTrigger != null)
            dogBiteTrigger.enabled = true;

        if (dogBlockCollider != null)
            dogBlockCollider.enabled = true;

        ForceDogIdle();

        Debug.Log("Dog retry button clicked.");
    }

    public void DistractDog(Vector3 thrownBonePosition)
    {
        if (dogIsDistracted)
            return;

        dogIsDistracted = true;
        dogIsDangerous = false;
        failInProgress = false;

        if (dog == null)
            return;

        boneTargetPosition = new Vector3(
            thrownBonePosition.x,
            dog.position.y,
            dog.position.z
        );

        if (dogBiteTrigger != null)
            dogBiteTrigger.enabled = false;

        if (dogBlockCollider != null)
            dogBlockCollider.enabled = true;

        SetDogSniffing(false);
        SetDogWalking(true);

        FaceBoneTarget();

        dogMovingToBone = true;
    }

    private void MoveDogToBone()
    {
        if (dog == null)
            return;

        FaceBoneTarget();

        dog.position = Vector3.MoveTowards(
            dog.position,
            boneTargetPosition,
            dogMoveSpeed * Time.deltaTime
        );

        float distance = Mathf.Abs(dog.position.x - boneTargetPosition.x);

        if (distance <= dogStopDistance)
        {
            dogMovingToBone = false;

            SetDogWalking(false);
            SetDogSniffing(true);

            if (dogBlockCollider != null)
                dogBlockCollider.enabled = false;

            if (dogBiteTrigger != null)
                dogBiteTrigger.enabled = false;
        }
    }

    private void ForceDogIdle()
    {
        SetDogWalking(false);
        SetDogSniffing(false);

        if (dogAnimator == null)
            return;

        if (!string.IsNullOrEmpty(dogBiteTriggerName))
            dogAnimator.ResetTrigger(dogBiteTriggerName);

        if (!string.IsNullOrEmpty(dogIdleStateName))
            dogAnimator.CrossFade(dogIdleStateName, 0.05f);
    }

    private void FaceBoneTarget()
    {
        if (dog == null)
            return;

        if (boneTargetPosition.x > dog.position.x)
            FaceRight();
        else if (boneTargetPosition.x < dog.position.x)
            FaceLeft();
    }

    private void FaceRizal()
    {
        if (dog == null || rizal == null)
            return;

        if (rizal.position.x > dog.position.x)
            FaceRight();
        else if (rizal.position.x < dog.position.x)
            FaceLeft();
    }

    private void FaceLeft()
    {
        if (dogVisualRoot == null)
            return;

        Vector3 scale = dogVisualRoot.localScale;

        if (!invertDogFacing)
            scale.x = -originalDogScaleX;
        else
            scale.x = originalDogScaleX;

        dogVisualRoot.localScale = scale;
    }

    private void FaceRight()
    {
        if (dogVisualRoot == null)
            return;

        Vector3 scale = dogVisualRoot.localScale;

        if (!invertDogFacing)
            scale.x = originalDogScaleX;
        else
            scale.x = -originalDogScaleX;

        dogVisualRoot.localScale = scale;
    }

    private void SetDogWalking(bool value)
    {
        if (dogAnimator == null)
            return;

        dogAnimator.SetBool(dogWalkBoolName, value);
    }

    private void SetDogSniffing(bool value)
    {
        if (dogAnimator == null)
            return;

        dogAnimator.SetBool(dogSniffBoolName, value);
    }
}