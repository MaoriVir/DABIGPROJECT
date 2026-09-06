using System.Collections;
using UnityEngine;

public class FlashEffect : MonoBehaviour
{
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.15f;

    private SpriteRenderer spriteRenderer;
    private MeshRenderer meshRenderer;
    private Color originalColor;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        // Automatically grab whatever renderer is attached to this object
        spriteRenderer = GetComponent<SpriteRenderer>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        else if (meshRenderer != null)
            originalColor = meshRenderer.material.color;
    }

    public void Flash()
    {
        // If a flash is already running, stop it so they don't stack up errors
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        // Set the flash color
        SetColor(flashColor);

        // Wait for the duration
        yield return new WaitForSeconds(flashDuration);

        // Reset to original color
        SetColor(originalColor);
        flashCoroutine = null;
    }

    private void SetColor(Color color)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = color;
        else if (meshRenderer != null)
            meshRenderer.material.color = color;
    }
}