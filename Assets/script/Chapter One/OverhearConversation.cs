using System.Collections;
using UnityEngine;
using TMPro;

public class OverhearConversation : MonoBehaviour
{
    [Header("Caption UI")]
    public GameObject captionPanel;
    public TMP_Text captionText;

    [Header("NPC References")]
    public Animator rightFilipinaAnimator;
    public Animator leftFilipinaAnimator;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip rightFilipinaVoice;
    public AudioClip leftFilipinaVoice;

    [Header("Dialogue Lines")]
    [TextArea(2, 4)]
    public string rightFilipinaLine = "I heard that the priests planned a rebellion.";

    [TextArea(2, 4)]
    public string leftFilipinaLine = "That’s what they say, or so the authorities claim. But who can question them now?";

    [Header("Animator Settings")]
    public string talkingBoolName = "isTalking";

    [Header("Timing")]
    public float delayBetweenLines = 0.3f;
    public float defaultLineDuration = 4f;

    private bool hasPlayed = false;
    private bool isPlaying = false;

    private void Start()
    {
        if (captionPanel != null)
            captionPanel.SetActive(false);

        SetTalking(rightFilipinaAnimator, false);
        SetTalking(leftFilipinaAnimator, false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasPlayed)
            return;

        if (isPlaying)
            return;

        if (other.CompareTag("Player"))
        {
            hasPlayed = true;
            StartCoroutine(PlayConversationRoutine());
        }
    }

    private IEnumerator PlayConversationRoutine()
    {
        isPlaying = true;

        // First speaker: Filipina on the right
        yield return PlayLine(
            rightFilipinaAnimator,
            rightFilipinaLine,
            rightFilipinaVoice
        );

        yield return new WaitForSeconds(delayBetweenLines);

        // Second speaker: Filipina on the left
        yield return PlayLine(
            leftFilipinaAnimator,
            leftFilipinaLine,
            leftFilipinaVoice
        );

        if (captionPanel != null)
            captionPanel.SetActive(false);

        SetTalking(rightFilipinaAnimator, false);
        SetTalking(leftFilipinaAnimator, false);

        isPlaying = false;
    }

    private IEnumerator PlayLine(Animator speakerAnimator, string line, AudioClip voiceClip)
    {
        SetTalking(rightFilipinaAnimator, false);
        SetTalking(leftFilipinaAnimator, false);

        SetTalking(speakerAnimator, true);

        if (captionPanel != null)
            captionPanel.SetActive(true);

        if (captionText != null)
            captionText.text = line;

        if (audioSource != null && voiceClip != null)
        {
            audioSource.Stop();
            audioSource.clip = voiceClip;
            audioSource.Play();

            yield return new WaitForSeconds(voiceClip.length);
        }
        else
        {
            yield return new WaitForSeconds(defaultLineDuration);
        }

        SetTalking(speakerAnimator, false);
    }

    private void SetTalking(Animator anim, bool value)
    {
        if (anim == null)
            return;

        anim.SetBool(talkingBoolName, value);
    }
}