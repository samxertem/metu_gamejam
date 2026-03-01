using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class CarCrashCinematic : MonoBehaviour
{
    [Header("Cinematic Trigger Setup")]
    public Transform playerTransform;
    public Vector3 triggerPoint = new Vector3(250f, 0f, 15f);
    public float triggerRadius = 15f;

    [Header("Car Setup")]
    [Tooltip("Sahnedeki arabayı sürükle veya boş bırak (otomatik bulur)")]
    public GameObject sceneCarObject;
    public float carSpeed = 40f;
    public float crashDistance = 3.5f;

    [Header("Car Start Offset")]
    public float carStartDistance = 50f;
    
    private Vector3 carOriginalPosition;
    private bool isCinematicActive = false;
    private bool hasCrashed = false;
    
    private Camera playerCamera;
    private PerfectPlayerController playerController;
    private GunController gunController;
    
    // Audio - iki kaynak: biri korna (loop), biri çarpışma (one-shot)
    private AudioSource honkAudioSource;
    private AudioSource crashAudioSource;

    void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
            else Debug.LogError("[CarCrash] Player bulunamadı!");
        }

        if (playerTransform != null)
        {
            playerCamera = playerTransform.GetComponentInChildren<Camera>();
            playerController = playerTransform.GetComponent<PerfectPlayerController>();
            gunController = playerTransform.GetComponentInChildren<GunController>();
        }

        // Arabayı otomatik bul
        if (sceneCarObject == null)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
            foreach (GameObject obj in allObjects)
            {
                if (obj == this.gameObject) continue;
                if (obj.GetComponent<CarCrashCinematic>() != null) continue;
                if (obj.GetComponent<Canvas>() != null) continue;
                if (obj.layer == 5) continue;
                
                string lowerName = obj.name.ToLower();
                if (lowerName.Contains("car") && !lowerName.Contains("canvas") && !lowerName.Contains("cartoon"))
                {
                    if (obj.GetComponentInChildren<MeshRenderer>() != null)
                    {
                        sceneCarObject = obj;
                        Debug.Log("[CarCrash] Araba bulundu: " + obj.name);
                        break;
                    }
                }
            }
        }

        if (sceneCarObject != null)
            carOriginalPosition = sceneCarObject.transform.position;

        // İki ayrı AudioSource
        honkAudioSource = gameObject.AddComponent<AudioSource>();
        honkAudioSource.playOnAwake = false;
        honkAudioSource.spatialBlend = 0f;
        honkAudioSource.volume = 0.6f;
        honkAudioSource.loop = true; // Korna sürekli çalsın

        crashAudioSource = gameObject.AddComponent<AudioSource>();
        crashAudioSource.playOnAwake = false;
        crashAudioSource.spatialBlend = 0f;
        crashAudioSource.volume = 1f;

        Debug.Log("[CarCrash] Hazır. Trigger: " + triggerPoint + " R: " + triggerRadius);
    }

    void Update()
    {
        if (playerTransform == null || hasCrashed) return;

        Vector3 pPos = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
        Vector3 tPos = new Vector3(triggerPoint.x, 0, triggerPoint.z);
        float distToTrigger = Vector3.Distance(pPos, tPos);

        // Feneri trigger bölgesine yaklaşırken erken kapat
        if (!isCinematicActive && distToTrigger <= triggerRadius + 5f)
        {
            Flashlight flashlight = playerTransform.GetComponentInChildren<Flashlight>();
            if (flashlight != null && flashlight.enabled)
            {
                Light spotLight = flashlight.GetComponent<Light>();
                if (spotLight != null) spotLight.enabled = false;
                flashlight.enabled = false;
            }
        }

        if (!isCinematicActive && distToTrigger <= triggerRadius)
        {
            Debug.Log("[CarCrash] Trigger! Sinematik başlıyor...");
            StartCinematic();
        }

        if (isCinematicActive && sceneCarObject != null && !hasCrashed)
        {
            // Arabayı oyuncuya doğru sür
            Vector3 targetPos = new Vector3(playerTransform.position.x, sceneCarObject.transform.position.y, playerTransform.position.z);
            sceneCarObject.transform.position = Vector3.MoveTowards(sceneCarObject.transform.position, targetPos, carSpeed * Time.deltaTime);
            sceneCarObject.transform.LookAt(new Vector3(playerTransform.position.x, sceneCarObject.transform.position.y, playerTransform.position.z));

            // Kamerayı arabaya çevir
            if (playerCamera != null)
            {
                Vector3 lookDir = sceneCarObject.transform.position - playerCamera.transform.position;
                lookDir.y = 0;
                if (lookDir.magnitude > 0.1f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDir);
                    playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRot, Time.deltaTime * 8f);
                    playerCamera.transform.localRotation = Quaternion.Slerp(playerCamera.transform.localRotation, Quaternion.identity, Time.deltaTime * 8f);
                }
            }

            float distToCar = Vector3.Distance(
                new Vector3(playerTransform.position.x, 0, playerTransform.position.z),
                new Vector3(sceneCarObject.transform.position.x, 0, sceneCarObject.transform.position.z)
            );

            // Çarpışma sesini erken çal (25 metre kala)
            if (distToCar <= 25f && crashAudioSource != null && !crashAudioSource.isPlaying)
            {
                #if UNITY_EDITOR
                AudioClip crashClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/pwlpl-car_crash-377291.mp3");
                if (crashClip != null)
                    crashAudioSource.PlayOneShot(crashClip, 1f);
                #endif
            }
            
            if (distToCar <= crashDistance)
            {
                Debug.Log("[CarCrash] ÇARPIŞMA!");
                TriggerCrash();
            }
        }
    }

    private void StartCinematic()
    {
        isCinematicActive = true;

        // --- TÜM OYNAYI DONDUR ---
        // Oyuncu kontrolünü kapat
        if (playerController != null) playerController.enabled = false;
        if (gunController != null) gunController.enabled = false;
        
        // Recoil scriptini de kapat
        Recoil recoilScript = playerTransform.GetComponentInChildren<Recoil>();
        if (recoilScript != null) recoilScript.enabled = false;

        // TÜM düşmanları dondur
        FreezeAllEnemies();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Arabayı konumlandır
        if (sceneCarObject == null)
        {
            #if UNITY_EDITOR
            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Azerilo/Car Model No.1201 Asset/Prefab/Car 5.prefab");
            if (prefab != null)
            {
                Vector3 spawnPos = new Vector3(triggerPoint.x, playerTransform.position.y, triggerPoint.z + carStartDistance);
                sceneCarObject = Instantiate(prefab, spawnPos, Quaternion.identity);
            }
            #endif
        }
        else
        {
            Vector3 dirFromPlayer = (carOriginalPosition - playerTransform.position).normalized;
            if (dirFromPlayer.magnitude < 0.1f) dirFromPlayer = Vector3.forward;
            
            Vector3 spawnPos = playerTransform.position + dirFromPlayer * carStartDistance;
            spawnPos.y = playerTransform.position.y;
            sceneCarObject.transform.position = spawnPos;
            sceneCarObject.SetActive(true);
        }

        if (sceneCarObject != null)
        {
            Rigidbody rb = sceneCarObject.GetComponent<Rigidbody>();
            if (rb == null) rb = sceneCarObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        // Korna çalmaya başla (loop)
        #if UNITY_EDITOR
        AudioClip honkClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/freesounds123-car-horn-338827 (mp3cut.net).mp3");
        if (honkClip != null && honkAudioSource != null)
        {
            honkAudioSource.clip = honkClip;
            honkAudioSource.Play(); // Loop olarak çalacak
        }
        #endif
    }

    private void FreezeAllEnemies()
    {
        // Global bayrak — yeni spawn olanlar dahil HERKESE etki eder
        EnemyAI.gameFrozen = true;

        // Mevcut düşmanları durdur
        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            enemy.enabled = false;
            UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }
        }

        // Spawner'ın coroutine'lerini tamamen durdur
        EnemySpawner[] spawners = FindObjectsOfType<EnemySpawner>();
        foreach (EnemySpawner spawner in spawners)
        {
            spawner.StopAllCoroutines();
            spawner.enabled = false;
        }

        Debug.Log("[CarCrash] Tüm düşmanlar ve spawnerlar tamamen donduruldu.");
    }

    private void TriggerCrash()
    {
        hasCrashed = true;
        
        // Kornayı durdur
        if (honkAudioSource != null) honkAudioSource.Stop();
        StartCoroutine(FadeToBlack());
    }

    private IEnumerator FadeToBlack()
    {
        // Çarpma anını bekleme, direkt karartıp videoya geçelim 
        // (Çünkü zaten çarpışmanın kendisini videoda göreceğiz)
        if (ScreenFader.Instance != null)
        {
            yield return ScreenFader.Instance.FadeOut(0.5f);
        }
        else
        {
            // Fallback for Editor testing without ScreenFader
            GameObject fallbackCanvasObj = new GameObject("FallbackFadeCanvas");
            Canvas fallbackCanvas = fallbackCanvasObj.AddComponent<Canvas>();
            fallbackCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fallbackCanvas.sortingOrder = 999;
            Image fadeImg = fallbackCanvasObj.AddComponent<Image>();
            fadeImg.color = new Color(0, 0, 0, 0);
            
            float alpha = 0;
            while (alpha < 1f)
            {
                alpha += Time.deltaTime * 2f; 
                fadeImg.color = new Color(0, 0, 0, Mathf.Clamp01(alpha));
                yield return null;
            }
            fadeImg.color = Color.black;
        }

        // Kırmızı hasar ekranı gibi kalıntıları temizle (Level 04'ten Courtroom'a geçerken)
        GameObject damageVig = GameObject.Find("DamageVignette");
        if (damageVig != null)
        {
            Destroy(damageVig);
        }

        // Siyah ekrana geçtik. ŞİMDİ VİDEOYU OYNAT.
        yield return PlayVideoSequence();
    }

    private IEnumerator PlayVideoSequence()
    {
        // Video Oynatıcı UI oluştur
        GameObject videoCanvasObj = new GameObject("VideoCanvas");
        Canvas vCanvas = videoCanvasObj.AddComponent<Canvas>();
        vCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        vCanvas.sortingOrder = 1000; // En üstte olsun
        
        RawImage rawImage = videoCanvasObj.AddComponent<RawImage>();
        rawImage.color = Color.black; // Video yüklenene kadar siyah
        
        VideoPlayer videoPlayer = videoCanvasObj.AddComponent<VideoPlayer>();
        
        // Videonun yolunu bul (AssetDatabase kullanarak Editor'de)
        #if UNITY_EDITOR
        var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/_Project/Video/Level04_CrashVideo.mp4");
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.clip = clip;
        #else
        // Build ortamında çalması için proje klasöründe StreamingAssets veya Resources içine koymak gerekebilir.
        // Fakat şuan Asset üzerinden çözüyoruz.
        #endif

        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        
        // Render texture oluştur (1920x1080)
        RenderTexture rt = new RenderTexture(1920, 1080, 16, RenderTextureFormat.ARGB32);
        rt.Create();

        videoPlayer.targetTexture = rt;
        rawImage.texture = rt;
        rawImage.color = Color.white; // Video geldi
        
        videoPlayer.Prepare();
        
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }
        
        videoPlayer.Play();
        Debug.Log("[CarCrash] Video oynamaya başladı.");

        // Video bitene kadar bekle
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }
        
        // GameManager'da loop'u artır ve Level 02'ye dön (Mahkeme)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentLoop++;
            Debug.Log($"[CarCrash] Current Loop arttırıldı: {GameManager.Instance.currentLoop}");
        }
        
        Debug.Log("[CarCrash] Level_02 yükleniyor (Loop başlıyor)...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level_02");
    }
}
