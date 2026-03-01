using UnityEngine;
using System.Collections;

/// <summary>
/// Manhole kapağı. Anahtar gerektirir. E ile yana kayarak açılır.
/// Açıldığında altındaki TunnelHole objesini aktif eder.
/// SlidingBarDoor.cs pattern'ini takip eder.
/// </summary>
public class ManholeDoor : MonoBehaviour, IInteractable
{
    [Header("Kayma Ayarları")]
    public float slideDistance = 1.2f;
    public float slideDuration = 1.2f;
    public Vector3 slideDirection = Vector3.left; // X negatif yönde kayar

    [Header("Durum")]
    public bool isOpen = false;
    public bool requiresKey = true;

    [Header("Delik Referansı")]
    [Tooltip("Açıldığında aktif edilecek delik objesi (TunnelHole)")]
    public GameObject tunnelHoleObject;

    [Header("Ses (Opsiyonel)")]
    public AudioClip slideSound;
    public AudioClip lockedSound;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isAnimating = false;
    private AudioSource audioSource;

    void Start()
    {
        closedPosition = transform.localPosition;
        openPosition = closedPosition + slideDirection.normalized * slideDistance;

        // AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = 8f;
        }

        // Delik başta gizli
        if (tunnelHoleObject != null)
            tunnelHoleObject.SetActive(false);
    }

    public string GetInteractText()
    {
        if (isOpen)
            return ""; // Zaten açık, bir şey yapma

        if (requiresKey && (GameManager.Instance == null || !GameManager.Instance.hasKey))
            return "Kilitli — Anahtar Gerekli";

        return "Mazgalı Aç";
    }

    public void Interact()
    {
        // Zaten açıksa veya animasyon devamsa çık
        if (isAnimating || isOpen) return;

        // Anahtar kontrolü
        if (requiresKey && (GameManager.Instance == null || !GameManager.Instance.hasKey))
        {
            PlaySound(lockedSound);
            Debug.Log("🔒 Bu mazgalı açmak için anahtar lazım...");
            return;
        }

        StartCoroutine(SlideOpen());
    }

    IEnumerator SlideOpen()
    {
        isAnimating = true;
        PlaySound(slideSound);

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            // Ease out cubic — başta hızlı sonda yavaşlar (SlidingBarDoor ile aynı)
            t = 1f - Mathf.Pow(1f - t, 3f);
            transform.localPosition = Vector3.Lerp(closedPosition, openPosition, t);
            yield return null;
        }

        transform.localPosition = openPosition;
        isOpen = true;
        isAnimating = false;

        // Deliği göster
        if (tunnelHoleObject != null)
            tunnelHoleObject.SetActive(true);

        Debug.Log("✓ Mazgal açıldı! Tünel girişi görünür.");
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
