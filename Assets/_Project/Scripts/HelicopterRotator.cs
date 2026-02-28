using UnityEngine;

public class HelicopterRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float topRotorSpeed = 1200f;
    public float tailRotorSpeed = 1500f;

    [Header("Audio Settings")]
    public AudioClip heliSoundClip;
    [Range(0f, 1f)] public float soundVolume = 0.5f;
    public float maxHearingDistance = 100f; 

    private AudioSource heliSource;

    private Transform mainRotor;
    private Transform tailRotor;

    void Start()
    {
        // Scripti ana objeye atsa bile alt objeleri otomatik bulması için tarıyoruz
        Transform[] allChildren = GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildren)
        {
            if (child.name.Contains("Main_Rotor") || child.name.ToLower().Contains("main_rotor"))
            {
                mainRotor = child;
            }
            else if (child.name.Contains("Tail_Rotor") || child.name.ToLower().Contains("tail_rotor"))
            {
                tailRotor = child;
            }
        }

        // --- 3D AUDIO KURULUMU ---
        #if UNITY_EDITOR
        if (heliSoundClip == null)
        {
            heliSoundClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/dragon-studio-helicopter-sound-8d-372463.mp3");
        }
        #endif

        if (heliSoundClip != null)
        {
            heliSource = gameObject.AddComponent<AudioSource>();
            heliSource.clip = heliSoundClip;
            heliSource.volume = soundVolume;
            heliSource.loop = true;

            // Sesin 3D olarak uzaklaştıkça azalması için:
            heliSource.spatialBlend = 1f; 
            heliSource.rolloffMode = AudioRolloffMode.Linear;
            heliSource.minDistance = 10f; // Sesi en net duyduğun min mesafe
            heliSource.maxDistance = maxHearingDistance; 

            heliSource.Play();
            StartCoroutine(StopHeliSoundAfterLoops(4)); // 4 Kere çalma kuralı
        }
    }

    void Update()
    {
        // Ana pervane genel olarak Y ekseninde (Yukarı/Aşağı etrafında) döner
        if (mainRotor != null)
        {
            mainRotor.Rotate(Vector3.up * topRotorSpeed * Time.deltaTime, Space.Self);
        }

        // Kuyruk pervanesi genel olarak X ekseninde (Sağ/Sol etrafında) döner
        if (tailRotor != null)
        {
            tailRotor.Rotate(Vector3.right * tailRotorSpeed * Time.deltaTime, Space.Self);
        }
        
        // Eğer script direkt olarak pervaneye atıldıysa (Kullanıcı yanlış atanmışsa), kendini döndür
        if (mainRotor == null && tailRotor == null)
        {
             transform.Rotate(Vector3.up * topRotorSpeed * Time.deltaTime, Space.Self);
        }
    }

    private System.Collections.IEnumerator StopHeliSoundAfterLoops(int loops)
    {
        if (heliSoundClip != null)
        {
            yield return new WaitForSeconds(heliSoundClip.length * loops);
            if (heliSource != null)
            {
                heliSource.Stop();
                heliSource.loop = false;
            }
        }
    }
}
