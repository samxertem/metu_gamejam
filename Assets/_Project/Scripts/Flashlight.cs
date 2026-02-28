using UnityEngine;

[RequireComponent(typeof(Light))]
public class Flashlight : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.G;
    
    [Header("Audio (Optional)")]
    public AudioClip clickSound;
    private AudioSource audioSource;
    private Light spotlight;

    void Start()
    {
        // Force centering on the camera
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        spotlight = GetComponent<Light>();
        
        // Setup light default properties for a flashlight
        spotlight.type = LightType.Spot;
        
        // Setup audio if we have a clip
        if (clickSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            // Toggle the light on/off
            spotlight.enabled = !spotlight.enabled;
            
            // Play sound if assigned
            if (audioSource != null && clickSound != null)
            {
                audioSource.pitch = spotlight.enabled ? 1.0f : 0.8f; // Make turn off sound slightly lower pitch
                audioSource.PlayOneShot(clickSound);
            }
        }
    }
}
