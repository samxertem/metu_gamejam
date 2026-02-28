using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;
    public float spawnRate = 3f;
    public Transform[] spawnPoints;
    
    [Header("Police Effects")]
    public AudioClip sirenClip;

    [Header("Spawn Protection")]
    [Tooltip("Oyuncu bu kadar metre uzaklaşmadan düşman spawn olmaz")]
    public float safeDistance = 8f;

    private Transform playerTransform;
    private Vector3 playerSpawnPoint;
    private bool isSpawningActive = false;
    private Coroutine spawnCoroutine;

    private void Start()
    {
        // Atmosferik polis lambalarını spawn noktalarına ekle
        if (spawnPoints != null)
        {
            foreach (Transform sp in spawnPoints)
            {
                if (sp != null)
                {
                    PoliceLightFlasher flasher = sp.gameObject.GetComponent<PoliceLightFlasher>();
                    if (flasher == null)
                    {
                        flasher = sp.gameObject.AddComponent<PoliceLightFlasher>();
                    }
                    
                    if (sirenClip != null)
                    {
                        flasher.sirenClip = sirenClip;
                    }
                }
            }
        }

        // Oyuncuyu bul ve başlangıç noktasını kaydet
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerSpawnPoint = playerTransform.position;
        }

        // Spawn sistemi başlat
        if (enemyPrefab != null && spawnPoints.Length > 0)
        {
            spawnCoroutine = StartCoroutine(SpawnEnemiesRoutine());
        }
        else
        {
            Debug.LogWarning("EnemySpawner is missing the EnemyPrefab or SpawnPoints!");
        }
    }

    private IEnumerator SpawnEnemiesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnRate);

            // Oyuncu spawn noktasından yeterince uzaklaşmadıysa spawn yapma
            if (playerTransform != null)
            {
                float distFromSpawn = Vector3.Distance(playerTransform.position, playerSpawnPoint);
                if (distFromSpawn < safeDistance)
                {
                    continue; // Henüz uzaklaşmadı, bu döngüyü atla
                }
            }

            SpawnRandomEnemy();
        }
    }

    private void SpawnRandomEnemy()
    {
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomIndex];
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    /// <summary>
    /// Respawn sonrası çağrılır — eski coroutine'i durdurur, yeni başlatır.
    /// Oyuncunun yeni konumunu spawn noktası olarak günceller.
    /// </summary>
    public void RestartSpawning()
    {
        // Eski coroutine varsa durdur
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
        StopAllCoroutines();

        // Oyuncunun mevcut konumunu yeni güvenli bölge merkezi yap
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
        
        if (playerTransform != null)
            playerSpawnPoint = playerTransform.position;

        // Yeniden spawn başlat
        if (enemyPrefab != null && spawnPoints != null && spawnPoints.Length > 0)
        {
            spawnCoroutine = StartCoroutine(SpawnEnemiesRoutine());
        }
    }
}
