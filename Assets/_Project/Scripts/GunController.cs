using UnityEngine;
using System.Collections;

public class GunController : MonoBehaviour
{
    [Header("Gun Mechanics")]
    public float damage = 25f;
    public float range = 100f;
    public float fireRate = 0.15f; 
    
    [Header("Ammo & Reloading")]
    public int maxAmmo = 10;           // Şarjör kapasitesi
    private int currentAmmo;
    public float reloadTime = 1.5f;    // Şarjör değiştirme süresi
    private bool isReloading = false;
    
    [Header("Visuals & Audio")]
    public GameObject muzzleFlashPrefab; // JMO WarFX Prefab'ı buraya sürüklenecek
    public Transform gunBarrel;          // Namlunun ucu (Boş obje)
    public Vector3 muzzleOffset = new Vector3(0, 0.05f, 0.82f); // Yedek namlu ucu ayarı
    public float muzzleFlashScale = 2.0f; // Muzzle Flash büyüklük ayarı
    
    public AudioSource audioSource;    // Silahın üzerindeki AudioSource
    public AudioClip shootSound;       // Ateş sesi
    public AudioClip reloadSound;      // Şarjör değiştirme sesi
    public AudioClip emptyClickSound;  // Mermi bitince gelen "tık" sesi
    [Range(0f, 1f)] public float gunVolume = 0.35f; // Silah sesi seviyesi

    [Header("Recoil & Crosshair")]
    public CrosshairController crosshair; 
    public Recoil proceduralRecoil;    // Yeni Procedural Recoil Sistemimiz
    public float spreadPerShot = 15f;  

    private float nextTimeToFire = 0f;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam == null)
        {
            mainCam = GetComponentInParent<Camera>();
        }
        
        // Otomatik AudioSource bul veya yarat
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        // Oyuna başlarken şarjörü fullüyoruz
        UpdateUI();
    }

    void Update()
    {
        // Şarjör değiştirirken ateş edemezsin
        if (isReloading) return;

        // R tuşuna basılırsa ve şarjör tam dolu değilse Reload yap
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return;
        }

        // Ateş etme (Sol Tık)
        if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Şarjör Değiştiriliyor...");
        
        // Şarjör sesini çal
        if(audioSource != null && reloadSound != null)
            audioSource.PlayOneShot(reloadSound, gunVolume);

        // Şarjör animasyonu/süresi kadar bekle
        yield return new WaitForSeconds(reloadTime);

        // Mermiyi fulle
        currentAmmo = maxAmmo;
        isReloading = false;
        
        UpdateUI();
        Debug.Log("Şarjör Doldu!");
    }

    private void Shoot()
    {
        // 1. Mermi Kontrolü
        if (currentAmmo <= 0)
        {
            // Mermi yoksa "Tık" sesi çal
            if(audioSource != null && emptyClickSound != null)
                audioSource.PlayOneShot(emptyClickSound, gunVolume);
            return;
        }

        // Mermiyi azalt
        currentAmmo--;
        UpdateUI();
        
        // 2. Ses ve Efektler
        if (audioSource != null && shootSound != null)
            audioSource.PlayOneShot(shootSound, gunVolume);

        if (muzzleFlashPrefab != null)
        {
            Transform spawnPoint = gunBarrel != null ? gunBarrel : transform;
            Vector3 spawnPos = gunBarrel != null ? gunBarrel.position : transform.position + transform.TransformVector(muzzleOffset);
            Quaternion spawnRot = gunBarrel != null ? gunBarrel.rotation : transform.rotation;

            GameObject flash = Instantiate(muzzleFlashPrefab, spawnPos, spawnRot, spawnPoint);
            // Boyutunu büyütüyoruz (Player'ın kendi silahında daha görkemli durması için)
            flash.transform.localScale = new Vector3(muzzleFlashScale, muzzleFlashScale, muzzleFlashScale);
            
            // Kusursuz Muzzle Flash: Sonsuza dek açık kalmaması için 0.1 saniye sonra siliyoruz.
            Destroy(flash, 0.1f);
        }

        // Procedural Recoil Sistemi
        if (proceduralRecoil != null)
        {
            proceduralRecoil.FireRecoil();
        }

        // 3. Vuruş İşlemleri (Hitscan)
        if (mainCam != null)
        {
            Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, range))
            {
                // Mermi İzi ve Kıvılcım
                Debug.DrawLine(mainCam.transform.position, hit.point, Color.yellow, 0.1f);
                CreateHitImpact(hit);

                // Crosshair Açılması
                if(crosshair != null) crosshair.AddSpread(spreadPerShot);
                
                // Düşmana Hasar Ver
                Enemy target = hit.transform.GetComponentInParent<Enemy>();
                if (target != null)
                {
                    target.TakeDamage(damage);
                }
            }
        }
    }

    private void UpdateUI()
    {
        // UIManager devredeyse ekran güncellenir
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateAmmo(currentAmmo, maxAmmo);
        }
    }

    // Mermi çarptığında minik bir efekt oluşturmak için yardımcı metod
    private void CreateHitImpact(RaycastHit hit)
    {
        GameObject impact = GameObject.CreatePrimitive(PrimitiveType.Cube);
        impact.transform.position = hit.point;
        // Kıvılcımı biraz daha küçülttüm ki daha gerçekçi dursun
        impact.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        impact.GetComponent<Renderer>().material.color = Color.yellow;
        Destroy(impact.GetComponent<Collider>()); 
        Destroy(impact, 0.05f);
    }

}