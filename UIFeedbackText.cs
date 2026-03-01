using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Feedback text göster ve belirli süre sonra otomatik temizle.
/// </summary>
public class UIFeedbackText : MonoBehaviour
{
    [Header("Referans")]
    public Text feedbackText;

    private Coroutine clearRoutine;

    /// <summary>
    /// Mesaj göster, belirtilen renkte, süre sonra otomatik temizle.
    /// </summary>
    public void ShowMessage(string message, Color color, float duration = 2f)
    {
        if (feedbackText == null) return;

        if (clearRoutine != null)
            StopCoroutine(clearRoutine);

        feedbackText.text = message;
        feedbackText.color = color;

        if (duration > 0f)
            clearRoutine = StartCoroutine(ClearAfter(duration));
    }

    /// <summary>
    /// Mesajı hemen temizle.
    /// </summary>
    public void Clear()
    {
        if (clearRoutine != null)
        {
            StopCoroutine(clearRoutine);
            clearRoutine = null;
        }

        if (feedbackText != null)
            feedbackText.text = "";
    }

    IEnumerator ClearAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (feedbackText != null)
            feedbackText.text = "";
        clearRoutine = null;
    }
}
