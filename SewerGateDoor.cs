using UnityEngine;
using System.Collections;

/// <summary>
/// Sewer haritasındaki demir kapılar. E ile kayarak açılır (yukarı/yana).
/// IInteractable implement eder. Anahtar gerektirmez.
/// </summary>
public class SewerGateDoor : MonoBehaviour, IInteractable
{
    [Header("Kayma Ayarları")]
    public Vector3 slideDirection = Vector3.up;  // Yerel yönde kayar
    public float slideDistance = 3f;              // Kaç metre kayacak
    public float slideDuration = 1.5f;           // Animasyon süresi

    [Header("Durum")]
    public bool isOpen = false;
    public bool canClose = true;                 // Geri kapanabilir mi

    [Header("Ses (Opsiyonel)")]
    public AudioClip openSound;
    public AudioClip closeSound;

    private Vector3 closedLocalPos;
    private Vector3 openLocalPos;
    private bool isAnimating = false;
    private AudioSource audioSource;

    void Start()
    {
        closedLocalPos = transform.localPosition;
        openLocalPos = closedLocalPos + slideDirection.normalized * slideDistance;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = 10f;
        }
    }

    public string GetInteractText()
    {
        if (isAnimating) return "";
        if (isOpen && !canClose) return "";
        return isOpen ? "Kapıyı Kapat" : "Kapıyı Aç";
    }

    public void Interact()
    {
        if (isAnimating) return;
        if (isOpen && !canClose) return;

        StartCoroutine(SlideAnimation(!isOpen));
    }

    IEnumerator SlideAnimation(bool opening)
    {
        isAnimating = true;

        PlaySound(opening ? openSound : closeSound);

        Vector3 from = transform.localPosition;
        Vector3 to = opening ? openLocalPos : closedLocalPos;

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            // Ease out cubic
            t = 1f - Mathf.Pow(1f - t, 3f);
            transform.localPosition = Vector3.Lerp(from, to, t);
            yield return null;
        }

        transform.localPosition = to;
        isOpen = opening;
        isAnimating = false;

        Debug.Log(gameObject.name + (opening ? " açıldı" : " kapandı"));
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
