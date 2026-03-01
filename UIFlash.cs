using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// UI overlay flash efekti. Kırmızı (hata) veya yeşil (başarı) kısa flash.
/// </summary>
public class UIFlash : MonoBehaviour
{
    [Header("Referans")]
    public Image flashImage;

    private Coroutine currentFlash;

    /// <summary>
    /// Belirtilen renkle kısa flash efekti oynat.
    /// </summary>
    public void Flash(Color color, float duration = 0.3f)
    {
        if (flashImage == null) return;

        if (currentFlash != null)
            StopCoroutine(currentFlash);

        currentFlash = StartCoroutine(FlashRoutine(color, duration));
    }

    IEnumerator FlashRoutine(Color color, float duration)
    {
        color.a = 0.4f;
        flashImage.color = color;
        flashImage.enabled = true;

        float elapsed = 0f;
        float half = duration * 0.5f;

        // Fade in
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / half;
            color.a = Mathf.Lerp(0f, 0.4f, t);
            flashImage.color = color;
            yield return null;
        }

        // Fade out
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / half;
            color.a = Mathf.Lerp(0.4f, 0f, t);
            flashImage.color = color;
            yield return null;
        }

        color.a = 0f;
        flashImage.color = color;
        flashImage.enabled = false;
        currentFlash = null;
    }
}
