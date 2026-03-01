using UnityEngine;

public class AmbientAudioManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip rainClip;
    [Range(0f, 1f)] public float rainVolume = 0.50f;

    public AudioClip noirMusicClip;
    [Range(0f, 1f)] public float noirVolume = 0.25f;

    private AudioSource rainSource;
    private AudioSource noirSource;

    void Start()
    {
        // 1. Yağmur Sesi 
        if (rainClip != null)
        {
            rainSource = gameObject.AddComponent<AudioSource>();
            rainSource.clip = rainClip;
            rainSource.volume = rainVolume;
            rainSource.loop = true;          // Sürekli tekrar etsin
            rainSource.spatialBlend = 0f;    // 2D Ses (Her yerden eşit duyulur)
            rainSource.playOnAwake = false;
            rainSource.Play();
        }

        // 2. Noir Sinematik Müzik
        if (noirMusicClip != null)
        {
            noirSource = gameObject.AddComponent<AudioSource>();
            noirSource.clip = noirMusicClip;
            noirSource.volume = noirVolume;
            noirSource.loop = true;          // Sürekli tekrar etsin
            noirSource.spatialBlend = 0f;    // 2D Ses
            noirSource.playOnAwake = false;
            noirSource.Play();
        }
    }
}
