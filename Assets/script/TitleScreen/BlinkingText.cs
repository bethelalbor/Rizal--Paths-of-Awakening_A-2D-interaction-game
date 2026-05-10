using UnityEngine;
using TMPro;

public class BlinkingText : MonoBehaviour
{
    public TMP_Text textToBlink;
    public float blinkSpeed = 1f;
    public float minAlpha = 0.15f;
    public float maxAlpha = 1f;

    void Start()
    {
        if (textToBlink == null)
            textToBlink = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (textToBlink == null)
            return;

        float alpha = Mathf.Lerp(
            minAlpha,
            maxAlpha,
            (Mathf.Sin(Time.time * blinkSpeed * Mathf.PI * 2f) + 1f) / 2f
        );

        Color color = textToBlink.color;
        color.a = alpha;
        textToBlink.color = color;
    }
}