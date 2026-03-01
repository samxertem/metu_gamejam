using UnityEngine;
using System.Collections;

/// <summary>
/// Trigger zone: oyuncu girince fade-to-black → teleport → fade-in.
/// İki yönlü kullanım: tünele giriş ve tünelden çıkış.
/// BoxCollider (isTrigger) gerektirir.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class TunnelTeleporter : MonoBehaviour
{
    [Header("Teleport Ayarları")]
    [Tooltip("Oyuncunun taşınacağı hedef nokta")]
    public Transform destinationPoint;

    [Tooltip("Fade süresi (saniye)")]
    public float fadeDuration = 0.5f;

    [Header("Ses Efekti")]
    [Tooltip("Teleport olduktan sonra çalınacak ambiyans sesi (opsiyonel)")]
    public AudioClip teleportAmbience;
    
    [Header("Yön Ayarı")]
    [Tooltip("Teleport sonrası oyuncunun bakış yönünü ayarla")]
    public bool overrideLookDirection = false;

    [Tooltip("Hedef bakış açısı (Euler)")]
    public Vector3 targetLookEuler = Vector3.zero;

    private bool isTeleporting = false;

    void Start()
    {
        // Collider'ın trigger olduğundan emin ol
        BoxCollider bc = GetComponent<BoxCollider>();
        bc.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isTeleporting) return;
        if (destinationPoint == null) return;

        // Sadece Player'ı teleport et
        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc == null)
        {
            // Belki parent'ta?
            cc = other.GetComponentInParent<CharacterController>();
        }
        if (cc == null) return;

        StartCoroutine(TeleportRoutine(cc));
    }

    IEnumerator TeleportRoutine(CharacterController player)
    {
        isTeleporting = true;

        // 1. Player hareketini devre dışı bırak
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = false;

        // Kamera hareketini de devre dışı bırak
        PlayerCamera camScript = player.GetComponentInChildren<PlayerCamera>();
        if (camScript != null) camScript.enabled = false;

        // 2. Ekranı karart
        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut(fadeDuration);
        else
            yield return new WaitForSeconds(fadeDuration);

        // 3. Teleport: CharacterController'ı devre dışı bırak, taşı, tekrar aç
        player.enabled = false;
        player.transform.position = destinationPoint.position;

        // Bakış yönünü ayarla
        if (overrideLookDirection)
        {
            player.transform.rotation = Quaternion.Euler(0f, targetLookEuler.y, 0f);
        }

        player.enabled = true;

        // Eger ambiyans sesi atandiysa cal
        if (teleportAmbience != null)
        {
            GameObject sfxObj = new GameObject("TeleportAmbience_" + teleportAmbience.name);
            sfxObj.transform.position = destinationPoint.position;
            AudioSource src = sfxObj.AddComponent<AudioSource>();
            src.clip = teleportAmbience;
            src.loop = true;
            src.spatialBlend = 0f; // 2D ambians gibi her yerden ayni seviyede duyulsun
            src.volume = 0.6f;
            src.Play();
        }

        // 4. Kısa bekleme (siyah ekranda)
        yield return new WaitForSeconds(0.3f);

        // 5. Ekranı aç
        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeIn(fadeDuration);

        // 6. Player hareketini geri aç
        if (pm != null) pm.enabled = true;
        if (camScript != null) camScript.enabled = true;

        // 7. Cooldown (tekrar tetiklemeyi önle)
        yield return new WaitForSeconds(1.5f);
        isTeleporting = false;
    }
}
