using UnityEngine;

public class PoliceLightFlasher : MonoBehaviour
{
    [Header("Light Settings")]
    public float flashSpeed = 5f;
    // HDR Color trick to pierce through B&W post-processing effects
    public float maxIntensity = 5.0f; // Kullanıcı isteği üzerine bir kez daha kısıldı
    
    [Header("Audio Settings")]
    public AudioClip sirenClip;
    [Range(0f, 1f)] public float sirenVolume = 0.08f;
    public float maxHearingDistance = 40f; // Sesi duyabileceğin max mesafe

    private Light pLight;
    private Color redColor;
    private Color blueColor;
    private bool isRed = true;
    private float timer = 0f;
    private AudioSource sirenSource;

    void Start()
    {
        // 1. Işığı ayarla
        GameObject lightObj = new GameObject("SirenLightEmitter");
        lightObj.transform.SetParent(transform, false);
        lightObj.transform.localPosition = new Vector3(0, 2.5f, 0);

        pLight = lightObj.AddComponent<Light>();
        pLight.type = LightType.Point;
        pLight.range = 10f;
        pLight.intensity = maxIntensity;
        pLight.shadows = LightShadows.Hard;

        // Post Processing'i delmek için renkleri HDR mantığıyla (1'den büyük) ayarlıyoruz
        // Kullanıcının "biraz daha kıs" isteğine göre 3.5'ten 2.0'a düşürüldü
        redColor = new Color(2.0f, 0f, 0f, 1f); 
        blueColor = new Color(0f, 0f, 2.0f, 1f);

        // 2. 3D Ses ayarla (Siren)
        #if UNITY_EDITOR
        if (sirenClip == null)
        {
            sirenClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/soundreality-police-operation-siren-144229.mp3");
        }
        #endif

        if (sirenClip != null)
        {
            sirenSource = gameObject.AddComponent<AudioSource>();
            sirenSource.clip = sirenClip;
            sirenSource.volume = sirenVolume;
            sirenSource.loop = true;
            
            // Tamamen 3D Ses: Uzaklaştıkça azalacak
            sirenSource.spatialBlend = 1f; 
            sirenSource.rolloffMode = AudioRolloffMode.Linear;
            sirenSource.minDistance = 5f; // Seçilen ses düşüşü başlama mesafesi
            sirenSource.maxDistance = maxHearingDistance; 

            AudioReverbFilter reverb = gameObject.AddComponent<AudioReverbFilter>();
            reverb.reverbPreset = AudioReverbPreset.City;

            sirenSource.Play();
            StartCoroutine(StopSirenAfterLoops(5));
        }
    }

    void Update()
    {
        timer += Time.deltaTime * flashSpeed;

        if (timer >= 1f)
        {
            isRed = !isRed;
            timer = 0f;
        }

        pLight.color = isRed ? redColor : blueColor;
    }

    private System.Collections.IEnumerator StopSirenAfterLoops(int loops)
    {
        if (sirenClip != null)
        {
            yield return new WaitForSeconds(sirenClip.length * loops);
            if (sirenSource != null)
            {
                sirenSource.Stop();
                sirenSource.loop = false;
            }
        }
    }
}
