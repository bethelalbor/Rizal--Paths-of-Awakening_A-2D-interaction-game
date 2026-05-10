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

    [Header("Title Background Music")]
    public AudioSource titleMusicSource;

    [Header("Cutscene Background Music")]
    public AudioSource cutsceneMusicSource;
    public float cutsceneMusicVolume = 0.3f;
    public float cutsceneMusicFadeOutDuration = 2f;

    [Header("Opening Execution Effect")]
    public Camera titleCamera;
    public AudioSource sfxAudioSource;
    public AudioClip gunshotSFX;

    public float zoomInSize = 3f;
    public float zoomDuration = 0.25f;
    public float delayAfterGunshot = 1.5f;
    public float delayBeforeNarration = 0.8f;
    private bool hasStarted = false;

    [Header("Black Screen")]
    public GameObject blackScreenPanel;
    public float blackScreenDelayBeforeCutscene = 1.5f;
    public float blackScreenCutTimeDuringZoom = 0.12f;

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

            if (titleMusicSource != null)
                titleMusicSource.Stop();

            StartCoroutine(PlayIntroCutscene());
        }
    }

    private IEnumerator PlayIntroCutscene()
    {
        // Start gunshot and zoom at the same time.
        if (sfxAudioSource != null && gunshotSFX != null)
        {
            sfxAudioSource.Stop();
            sfxAudioSource.clip = gunshotSFX;
            sfxAudioSource.Play();
        }

        yield return StartCoroutine(QuickZoomRoutine());

        // Start cutscene background music after the gunshot.
        if (cutsceneMusicSource != null)
        {
            cutsceneMusicSource.volume = cutsceneMusicVolume;
            cutsceneMusicSource.Play();
        }

        // Dramatic pause on black screen before the cutscene appears.
        yield return new WaitForSeconds(blackScreenDelayBeforeCutscene);

        // Now show cutscene, but narration still waits for delayBeforeNarration.
        if (blackScreenPanel != null)
            blackScreenPanel.SetActive(false);

        if (cutscenePanel != null)
            cutscenePanel.SetActive(true);

        yield return new WaitForSeconds(delayBeforeNarration);

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

        if (audioSource != null && audioSource.isPlaying)
        {
            yield return new WaitWhile(() => audioSource.isPlaying);
        }

        // Fade out cutscene music before loading the gameplay scene.
        if (cutsceneMusicSource != null && cutsceneMusicSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutAudioRoutine(
                cutsceneMusicSource,
                cutsceneMusicFadeOutDuration
            ));
        }

        SceneManager.LoadScene(gameplaySceneName);
    }

    private IEnumerator QuickZoomRoutine()
    {
        if (titleCamera == null)
            yield break;

        float startSize = titleCamera.orthographicSize;
        float elapsed = 0f;
        bool blackScreenShown = false;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;

            titleCamera.orthographicSize = Mathf.Lerp(startSize, zoomInSize, t);

            if (!blackScreenShown && elapsed >= blackScreenCutTimeDuringZoom)
            {
                blackScreenShown = true;

                if (blackScreenPanel != null)
                    blackScreenPanel.SetActive(true);

                if (titlePanel != null)
                    titlePanel.SetActive(false);

                if (cutscenePanel != null)
                    cutscenePanel.SetActive(false);
            }

            yield return null;
        }

        titleCamera.orthographicSize = zoomInSize;

        // Safety: if the zoom was very short, still make sure black screen appears.
        if (!blackScreenShown)
        {
            if (blackScreenPanel != null)
                blackScreenPanel.SetActive(true);

            if (titlePanel != null)
                titlePanel.SetActive(false);

            if (cutscenePanel != null)
                cutscenePanel.SetActive(false);
        }
    }

    private IEnumerator FadeOutAudioRoutine(AudioSource source, float duration)
    {
        if (source == null)
            yield break;

        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            source.volume = Mathf.Lerp(startVolume, 0f, t);

            yield return null;
        }

        source.volume = 0f;
        source.Stop();
    }
}