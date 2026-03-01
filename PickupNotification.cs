using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ekranda kısa süreliğine eşya alındı bildirimi gösterir.
/// Sahnede bir Canvas + Text ile kullanılır. Singleton pattern.
/// </summary>
public class PickupNotification : MonoBehaviour
{
    public static PickupNotification Instance { get; private set; }

    [Header("UI")]
    public Text notificationText;

    [Header("Ayarlar")]
    public float displayDuration = 2f;
    public float fadeSpeed = 2f;

    private Coroutine currentRoutine;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        if (notificationText != null)
            notificationText.gameObject.SetActive(false);
    }

    public void Show(string message)
    {
        if (notificationText == null) return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowNotification(message));
    }

    private IEnumerator ShowNotification(string message)
    {
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);
        canvasGroup.alpha = 1f;

        // Ekranda bekle
        yield return new WaitForSeconds(displayDuration);

        // Yavaşça kaybol
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * fadeSpeed;
            canvasGroup.alpha = 1f - elapsed;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        notificationText.gameObject.SetActive(false);
        currentRoutine = null;
    }
}
