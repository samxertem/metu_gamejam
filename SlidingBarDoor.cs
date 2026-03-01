using UnityEngine;
using System.Collections;

/// <summary>
/// Kayan parmaklık kapı sistemi. Anahtar gerektirir.
/// E ile sağa/sola kayarak açılır/kapanır.
/// </summary>
public class SlidingBarDoor : MonoBehaviour, IInteractable
{
    public enum SlideDirection { Right, Left }

    [Header("Kayma Ayarları")]
    public float slideDistance = 2.5f;
    public float slideDuration = 0.8f;
    public SlideDirection direction = SlideDirection.Right;

    [Header("Durum")]
    public bool isOpen = false;
    public bool requiresKey = true;

    [Header("Ses (Opsiyonel)")]
    public AudioClip slideOpenSound;
    public AudioClip slideCloseSound;
    public AudioClip lockedSound;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isAnimating = false;
    private AudioSource audioSource;

    void Start()
    {
        closedPosition = transform.localPosition;

        // Kayma yönüne göre hedef pozisyon (Hapishane barları Z ekseninde dizili)
        Vector3 offset = new Vector3(0, 0, slideDistance);
        if (direction == SlideDirection.Left)
            offset = -offset;

        openPosition = closedPosition + offset;

        // Eğer baştan açıksa pozisyonu ayarla
        if (isOpen)
            transform.localPosition = openPosition;

        // AudioSource
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
        if (requiresKey && (GameManager.Instance == null || !GameManager.Instance.hasKey))
            return "Kilitli — Anahtar Gerekli";
        return isOpen ? "Hücreyi Kilitle" : "Hücreyi Aç";
    }

    public void Interact()
    {
        if (isAnimating) return;

        // Anahtar kontrolü
        if (requiresKey && (GameManager.Instance == null || !GameManager.Instance.hasKey))
        {
            PlaySound(lockedSound);
            Debug.Log("🔒 Bu kapıyı açmak için anahtar lazım...");
            
            // Ekranda küçük yazıyla göster
            if (PickupNotification.Instance != null)
            {
                PickupNotification.Instance.Show("Anahtar gerekli !");
            }
            return;
        }

        StartCoroutine(SlideAnimation());
    }

    IEnumerator SlideAnimation()
    {
        isAnimating = true;

        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = isOpen ? closedPosition : openPosition;

        // Ses çal
        PlaySound(isOpen ? slideCloseSound : slideOpenSound);

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            // Ease out — başta hızlı sonda yavaşlar
            t = 1f - Mathf.Pow(1f - t, 3f);
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.localPosition = targetPos;
        isOpen = !isOpen;
        isAnimating = false;
    }

    public void ForceOpen()
    {
        if (!isOpen && !isAnimating)
            StartCoroutine(SlideAnimation());
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
