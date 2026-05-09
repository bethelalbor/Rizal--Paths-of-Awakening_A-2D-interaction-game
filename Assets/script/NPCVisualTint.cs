using UnityEngine;

[ExecuteAlways]
public class NPCVisualTint : MonoBehaviour
{
    [Header("Tint Settings")]
    [SerializeField] private Color tintColor = Color.white;

    [Header("Options")]
    [SerializeField] private bool applyInEditor = true;

    private SpriteRenderer[] spriteRenderers;

    private void OnEnable()
    {
        RefreshRenderers();
        ApplyTint();
    }

    private void OnValidate()
    {
        if (!applyInEditor)
            return;

        RefreshRenderers();
        ApplyTint();
    }

    public void RefreshRenderers()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    public void ApplyTint()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            RefreshRenderers();

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr != null)
            {
                sr.color = tintColor;
            }
        }
    }
}