using UnityEngine;

/// <summary>
/// F tuşu ile fener aç/kapat. Kameranın baktığı yöne SpotLight yansıtır.
/// GameManager.hasFlashlight kontrolü yapar — fener yoksa çalışmaz.
/// Kamera objesine eklenmeli (PlayerCamera ile aynı obje).
/// </summary>
public class PlayerFlashlight : MonoBehaviour
{
    [Header("Fener Ayarları")]
    public float range = 15f;
    public float spotAngle = 55f;
    public float intensity = 3f;
    public Color lightColor = new Color(1f, 0.95f, 0.85f); // Sıcak beyaz

    [Header("Ses (Opsiyonel)")]
    public AudioClip toggleSound;

    private Light flashlight;
    private AudioSource audioSource;
    private bool isOn = false;

    void Start()
    {
        // SpotLight oluştur (kameranın child'ı)
        GameObject lightObj = new GameObject("Flashlight_SpotLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = new Vector3(0.15f, -0.1f, 0.3f); // Sağ alt, biraz ileride
        lightObj.transform.localRotation = Quaternion.identity;

        flashlight = lightObj.AddComponent<Light>();
        flashlight.type = LightType.Spot;
        flashlight.range = range;
        flashlight.spotAngle = spotAngle;
        flashlight.innerSpotAngle = spotAngle * 0.4f;
        flashlight.intensity = intensity;
        flashlight.color = lightColor;
        flashlight.shadows = LightShadows.Soft;
        flashlight.shadowStrength = 0.7f;
        flashlight.enabled = false; // Başta kapalı

        // AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // 2D ses (UI sesi gibi)
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
        }
    }

    void ToggleFlashlight()
    {
        isOn = !isOn;
        flashlight.enabled = isOn;

        // Ses çal
        if (toggleSound != null && audioSource != null)
            audioSource.PlayOneShot(toggleSound);

        Debug.Log("Fener " + (isOn ? "ACILDI" : "KAPANDI"));
    }

    /// <summary>
    /// Dışarıdan fener durumunu ayarlamak için (örn. pickup sonrası otomatik aç)
    /// </summary>
    public void SetFlashlight(bool on)
    {
        isOn = on;
        if (flashlight != null)
            flashlight.enabled = isOn;
    }

    public bool IsOn() => isOn;
}
