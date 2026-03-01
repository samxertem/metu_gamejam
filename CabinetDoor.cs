using UnityEngine;
using System.Collections;

public class CabinetDoor : MonoBehaviour, IInteractable
{
    [Header("Açılma Tipi")]
    public DoorType doorType = DoorType.Rotate;
    public OpenAxis openAxis = OpenAxis.Y;

    [Header("Rotasyon Ayarı (Rotate tipi için)")]
    public float openAngle = 110f;
    public float animationDuration = 0.4f;
    public bool openInward = false;

    [Header("Kayma Ayarı (Slide tipi için)")]
    public float slideDistance = 0.8f;
    public SlideAxis slideAxis = SlideAxis.X;

    [Header("Durum")]
    public bool isOpen = false;

    [Header("İçerik")]
    public GameObject[] hiddenObjects;

    [Header("Ses")]
    public AudioClip openSound;
    public AudioClip closeSound;

    public enum DoorType  { Rotate, Slide }
    public enum OpenAxis  { X, Y, Z }
    public enum SlideAxis { X, Y, Z }

    private Vector3 closedRotation;
    private Vector3 openRotation;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isAnimating = false;
    private AudioSource audioSource;

    void Start()
    {
        closedRotation = transform.localEulerAngles;
        closedPosition = transform.localPosition;

        // Açık rotasyonu hesapla
        float angle = openInward ? -openAngle : openAngle;
        switch (openAxis)
        {
            case OpenAxis.Y:
                openRotation = closedRotation + new Vector3(0, angle, 0);
                break;
            case OpenAxis.X:
                openRotation = closedRotation + new Vector3(angle, 0, 0);
                break;
            case OpenAxis.Z:
                openRotation = closedRotation + new Vector3(0, 0, angle);
                break;
        }

        // Açık pozisyonu hesapla (slide için)
        switch (slideAxis)
        {
            case SlideAxis.X:
                openPosition = closedPosition + new Vector3(slideDistance, 0, 0);
                break;
            case SlideAxis.Y:
                openPosition = closedPosition + new Vector3(0, slideDistance, 0);
                break;
            case SlideAxis.Z:
                openPosition = closedPosition + new Vector3(0, 0, slideDistance);
                break;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Başlangıçta objeleri durumuna göre ayarla
        if (hiddenObjects != null)
        {
            foreach (var obj in hiddenObjects)
            {
                if (obj != null) obj.SetActive(isOpen);
            }
        }
    }

    public string GetInteractText()
    {
        return isOpen ? "Dolabi Kapat" : "Dolabi Ac";
    }

    public void Interact()
    {
        if (isAnimating) return;

        if (doorType == DoorType.Rotate)
            StartCoroutine(RotateAnimation());
        else
            StartCoroutine(SlideAnimation());
    }

    IEnumerator RotateAnimation()
    {
        isAnimating = true;

        Quaternion startRot = transform.localRotation;
        Quaternion endRot = Quaternion.Euler(isOpen ? closedRotation : openRotation);

        AudioClip clip = isOpen ? closeSound : openSound;
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            t = 1f - Mathf.Pow(1f - t, 3f); // ease out cubic
            transform.localRotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

        transform.localRotation = endRot;
        isOpen = !isOpen;
        isAnimating = false;

        // Dolap açıldıysa içindeki objeleri göster
        if (isOpen && hiddenObjects != null)
            foreach (var obj in hiddenObjects)
                if (obj != null) obj.SetActive(true);

        // Dolap kapandıysa gizle
        if (!isOpen && hiddenObjects != null)
            foreach (var obj in hiddenObjects)
                if (obj != null) obj.SetActive(false);
    }

    IEnumerator SlideAnimation()
    {
        isAnimating = true;

        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = isOpen ? closedPosition : openPosition;

        AudioClip clip = isOpen ? closeSound : openSound;
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            t = 1f - Mathf.Pow(1f - t, 3f);
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.localPosition = targetPos;
        isOpen = !isOpen;
        isAnimating = false;

        // Dolap açıldıysa içindeki objeleri göster
        if (isOpen && hiddenObjects != null)
            foreach (var obj in hiddenObjects)
                if (obj != null) obj.SetActive(true);

        // Dolap kapandıysa gizle
        if (!isOpen && hiddenObjects != null)
            foreach (var obj in hiddenObjects)
                if (obj != null) obj.SetActive(false);
    }
}
