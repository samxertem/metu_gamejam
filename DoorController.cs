using UnityEngine;

/// <summary>
/// Kapı kontrol scripti. E tuşuyla açılır/kapanır.
/// IInteractable implement eder. AudioSource ile ses desteği.
/// </summary>
public class DoorController : MonoBehaviour, IInteractable
{
    [Header("Kapı Ayarları")]
    public float openAngle = 90f;
    public float animationDuration = 0.5f;
    public bool isOpen = false;
    public bool isLocked = false;

    [Header("Ses (Opsiyonel)")]
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockedSound;

    private Vector3 closedRotation;
    private Vector3 targetRotation;
    private Vector3 openRotationVec;
    private AudioSource audioSource;
    private bool isAnimating = false;
    private float animTimer = 0f;
    private Vector3 startRotation;

    void Start()
    {
        closedRotation = transform.eulerAngles;
        openRotationVec = closedRotation + new Vector3(0f, openAngle, 0f);

        // AudioSource yoksa ekle
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D ses
            audioSource.maxDistance = 8f;
        }
    }

    void Update()
    {
        if (!isAnimating) return;

        animTimer += Time.deltaTime;
        float t = Mathf.Clamp01(animTimer / animationDuration);

        // Smooth ease in-out
        float smooth = t * t * (3f - 2f * t);

        Vector3 current = Vector3.Lerp(startRotation, targetRotation, smooth);
        transform.eulerAngles = current;

        if (t >= 1f)
        {
            transform.eulerAngles = targetRotation;
            isAnimating = false;
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

        ToggleDoor();
    }

    public string GetInteractText()
    {
        if (isLocked) return "Kilitli";
        return isOpen ? "Kapıyı Kapat" : "Kapıyı Aç";
    }

    public void ToggleDoor()
    {
        if (isAnimating) return;

        isAnimating = true;
        animTimer = 0f;
        startRotation = transform.eulerAngles;
        targetRotation = isOpen ? closedRotation : openRotationVec;

        // Ses çal
        PlaySound(isOpen ? closeSound : openSound);

        isOpen = !isOpen;
    }

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
