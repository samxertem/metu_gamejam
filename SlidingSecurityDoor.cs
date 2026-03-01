using UnityEngine;
using System.Collections;

/// <summary>
/// Futuristic güvenlik kapısı. Sol ve sağ paneller kayarak açılır/kapanır.
/// E tuşuyla etkileşim. IInteractable implement eder.
/// Futuristic Security Door (Future_Door_Final) prefab'ı ile uyumlu.
/// Kapı açıkken ana collider devre dışı kalır → oyuncu geçebilir.
/// </summary>
public class SlidingSecurityDoor : MonoBehaviour, IInteractable
{
    [Header("Kapı Panelleri")]
    public Transform leftDoor;    // Left_Door_Final
    public Transform rightDoor;   // Right_Door_Final

    [Header("Kapı Ayarları")]
    public float slideDistance = 1.2f;     // Her panel ne kadar kayacak (önceki 0.7 yetersizdi)
    public float slideDuration = 0.8f;    // Animasyon süresi
    public bool isLocked = false;
    public bool isOpen = false;

    [Header("Ses (Opsiyonel)")]
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockedSound;

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;
    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;
    private AudioSource audioSource;
    private bool isAnimating = false;
    private BoxCollider doorCollider;

    void Start()
    {
        // Ana collider referansı (geçişi engelleyen)
        doorCollider = GetComponent<BoxCollider>();

        // Panellerin başlangıç pozisyonlarını kaydet
        if (leftDoor != null)
        {
            leftClosedPos = leftDoor.localPosition;
            leftOpenPos = leftClosedPos + Vector3.right * slideDistance;
        }
        if (rightDoor != null)
        {
            rightClosedPos = rightDoor.localPosition;
            rightOpenPos = rightClosedPos + Vector3.left * slideDistance;
        }

        // AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D ses
            audioSource.maxDistance = 8f;
        }
    }

    public void Interact()
    {
        if (isAnimating) return;

        if (isLocked)
        {
            PlaySound(lockedSound);
            Debug.Log("Bu kapı kilitli...");
            return;
        }

        StartCoroutine(SlideRoutine());
    }

    public string GetInteractText()
    {
        if (isLocked) return "Kilitli";
        return isOpen ? "Kapıyı Kapat (E)" : "Kapıyı Aç (E)";
    }

    IEnumerator SlideRoutine()
    {
        isAnimating = true;

        Vector3 leftStart, leftEnd, rightStart, rightEnd;

        if (!isOpen)
        {
            // Açılıyor
            leftStart = leftClosedPos;
            leftEnd = leftOpenPos;
            rightStart = rightClosedPos;
            rightEnd = rightOpenPos;
            PlaySound(openSound);
        }
        else
        {
            // Kapanıyor
            leftStart = leftOpenPos;
            leftEnd = leftClosedPos;
            rightStart = rightOpenPos;
            rightEnd = rightClosedPos;
            PlaySound(closeSound);
        }

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            // Ease-out cubic
            float smooth = 1f - Mathf.Pow(1f - t, 3f);

            if (leftDoor != null)
                leftDoor.localPosition = Vector3.Lerp(leftStart, leftEnd, smooth);
            if (rightDoor != null)
                rightDoor.localPosition = Vector3.Lerp(rightStart, rightEnd, smooth);

            yield return null;
        }

        // Kesin pozisyon
        if (leftDoor != null) leftDoor.localPosition = leftEnd;
        if (rightDoor != null) rightDoor.localPosition = rightEnd;

        isOpen = !isOpen;
        isAnimating = false;

        // Kapı açıkken collider'ı kapat → oyuncu geçebilsin
        // Kapı kapalıyken collider'ı aç → yol engellensin
        if (doorCollider != null)
        {
            doorCollider.enabled = !isOpen;
        }
    }

    /// <summary>
    /// Kapının kilidini açar.
    /// </summary>
    public void Unlock()
    {
        isLocked = false;
        Debug.Log(gameObject.name + " kapısının kilidi açıldı!");
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
