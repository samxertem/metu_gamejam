using System.Collections;
using UnityEngine;

public class SimpleMuzzleFlash : MonoBehaviour
{
    [Header("Settings")]
    public float flashDuration = 0.05f;
    public float maxLightIntensity = 5f;
    public float minLightIntensity = 3f;

    [Header("References")]
    private Light muzzleLight;

    void Start()
    {
        // Setup light
        muzzleLight = GetComponent<Light>();
        if (muzzleLight == null)
        {
            muzzleLight = gameObject.AddComponent<Light>();
            muzzleLight.type = LightType.Point;
            muzzleLight.color = new Color(1f, 0.4f, 0f); // Daha derin ve net bir "Turuncu"
            muzzleLight.range = 6.5f; // Biraz daha geniş yayılsın
            muzzleLight.renderMode = LightRenderMode.ForcePixel;
        }
        
        // Start hidden
        muzzleLight.enabled = false;
    }

    public void Flash()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        // 1. Enable and Randomize Intensity to feel like a spark
        muzzleLight.enabled = true;
        muzzleLight.intensity = Random.Range(minLightIntensity, maxLightIntensity);
        
        // 2. Add some random local rotation if you had a 3D mesh here
        transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        // 3. Wait for a split second (cinematic muzzle flash duration)
        yield return new WaitForSeconds(flashDuration);

        // 4. Disable
        muzzleLight.enabled = false;
    }
}
