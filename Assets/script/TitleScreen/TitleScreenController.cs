using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class TitleScreenController : MonoBehaviour
{
    [Header("Title UI")]
    public GameObject titlePanel;

    [Header("Cutscene UI")]
    public GameObject cutscenePanel;
    public Image cutsceneImage;
    public TMP_Text cutsceneCaptionText;

    [Header("Cutscene Images")]
    public Sprite[] cutsceneImages;

    [Header("Cutscene Captions")]
    [TextArea(2, 5)]
    public string[] cutsceneCaptions;

    [Tooltip("How long each caption stays on screen. Must match Cutscene Captions size.")]
    public float[] captionDurations;

    [Header("Narration")]
    public AudioSource audioSource;
    public AudioClip fullNarrationClip;

    [Header("Timing")]
    public float delayBeforeFirstCaption = 0.2f;
    public float delayAfterLastCaption = 0.5f;

    [Header("Next Scene")]
    public string gameplaySceneName = "Prototype";

    private bool hasStarted = false;

    void Start()
    {
        if (titlePanel != null)
            titlePanel.SetActive(true);

        if (cutscenePanel != null)
            cutscenePanel.SetActive(false);
    }

    void Update()
    {
        if (hasStarted)
            return;

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            hasStarted = true;
            StartCoroutine(PlayIntroCutscene());
        }
    }

    private IEnumerator PlayIntroCutscene()
    {
        if (titlePanel != null)
            titlePanel.SetActive(false);

        if (cutscenePanel != null)
            cutscenePanel.SetActive(true);

        if (audioSource != null && fullNarrationClip != null)
        {
            audioSource.Stop();
            audioSource.clip = fullNarrationClip;
            audioSource.Play();
        }

        yield return new WaitForSeconds(delayBeforeFirstCaption);

        for (int i = 0; i < cutsceneCaptions.Length; i++)
        {
            if (cutsceneImage != null && cutsceneImages != null && i < cutsceneImages.Length)
            {
                if (cutsceneImages[i] != null)
                    cutsceneImage.sprite = cutsceneImages[i];
            }

            if (cutsceneCaptionText != null)
                cutsceneCaptionText.text = cutsceneCaptions[i];

            float duration = 3f;

            if (captionDurations != null && i < captionDurations.Length)
                duration = captionDurations[i];

            yield return new WaitForSeconds(duration);
        }

        if (cutsceneCaptionText != null)
            cutsceneCaptionText.text = "";

        yield return new WaitForSeconds(delayAfterLastCaption);

        // If narration is still playing, wait until it finishes before loading the gameplay scene.
        if (audioSource != null && audioSource.isPlaying)
        {
            yield return new WaitWhile(() => audioSource.isPlaying);
        }

        SceneManager.LoadScene(gameplaySceneName);
    }
}