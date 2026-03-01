using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float attackRange = 15f; 
    public float strafeDistance = 5f; // Oyuncu bu kadar yakınsa strafe yapar
    public float strafeSpeed = 4f;
    
    [Header("Shooting")]
    public float damage = 10f;
    public float fireRate = 1f; 
    private float nextFireTime = 0f;
    public Transform gunBarrel; 

    [Header("Model Offset (Yere Bastırma)")]
    public Transform characterModel;
    public float yOffset = -1.0f;

    [Header("Audio & FX")]
    public AudioClip shootSound;
    [Range(0f, 1f)] public float gunVolume = 0.35f;
    private AudioSource audioSource;
    public GameObject muzzleFlashPrefab;
    public Vector3 muzzleOffset = new Vector3(0, 1.25f, 0.7f);
    
    public Animator anim;

    private Transform player;
    private NavMeshAgent agent;
    
    private static int spawnCounter = 0;
    
    // Strafe
    private float strafeDirection = 1f; // 1 = sağ, -1 = sol
    private float strafeTimer = 0f;
    private float nextStrafeChange = 2f;
    
    // Ölüm
    private bool isDead = false;

    // Sinematik sırasında tüm düşmanları global olarak dondur
    public static bool gameFrozen = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f;
                audioSource.minDistance = 3f;
                audioSource.maxDistance = 50f;
            }
        }
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        // Her 4 polisten birine el feneri ver
        spawnCounter++;
        if (spawnCounter % 4 == 0)
        {
            Transform lightParent = gunBarrel != null ? gunBarrel : transform;
            GameObject flashlightObj = new GameObject("EnemyFlashlight");
            flashlightObj.transform.SetParent(lightParent, false);
            flashlightObj.transform.localPosition = Vector3.zero;
            flashlightObj.transform.localRotation = Quaternion.identity;
            Light spotLight = flashlightObj.AddComponent<Light>();
            spotLight.type = LightType.Spot;
            spotLight.range = 25f;
            spotLight.spotAngle = 45f;
            spotLight.intensity = 3f;
            spotLight.color = new Color(1f, 0.95f, 0.8f);
        }

        // Rastgele strafe yönü
        strafeDirection = Random.value > 0.5f ? 1f : -1f;
        nextStrafeChange = Random.Range(1.5f, 3f);
    }

    void LateUpdate()
    {
        if (characterModel != null)
        {
            Vector3 localPos = characterModel.localPosition;
            localPos.y = yOffset;
            characterModel.localPosition = localPos;
        }
    }

    void Update()
    {
        if (player == null || gameFrozen || isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Oyuncuya bak
        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0;
        if (lookDirection.magnitude > 0.1f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);

        if (distanceToPlayer <= strafeDistance)
        {
            // === STRAFE MOD: Oyuncu çok yakın, sağa sola kaç ===
            agent.isStopped = true;
            
            if (anim != null)
            {
                anim.SetBool("isMoving", false);
                anim.SetBool("isStrafing", true);
            }

            // Sağa sola hareket et
            strafeTimer += Time.deltaTime;
            if (strafeTimer >= nextStrafeChange)
            {
                strafeDirection *= -1f;
                strafeTimer = 0f;
                nextStrafeChange = Random.Range(1f, 2.5f);
            }

            Vector3 strafeMove = transform.right * strafeDirection * strafeSpeed * Time.deltaTime;
            
            // NavMesh üzerinde kalmasını sağla
            if (NavMesh.SamplePosition(transform.position + strafeMove, out NavMeshHit hitStrafe, 2f, NavMesh.AllAreas))
            {
                transform.position = Vector3.MoveTowards(transform.position, hitStrafe.position, strafeSpeed * Time.deltaTime);
            }

            // Strafe ederken de ateş et
            if (Time.time >= nextFireTime)
            {
                ShootAtPlayer();
                nextFireTime = Time.time + fireRate;
            }
        }
        else if (distanceToPlayer <= attackRange)
        {
            // === IDLE/SHOOT MOD: Menzilde, dur ve ateş et ===
            agent.isStopped = true;
            
            if (anim != null)
            {
                anim.SetBool("isMoving", false);
                anim.SetBool("isStrafing", false);
            }

            if (Time.time >= nextFireTime)
            {
                ShootAtPlayer();
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            // === CHASE MOD: Menzil dışında, oyuncuya koş ===
            agent.isStopped = false;
            agent.SetDestination(player.position);
            
            if (anim != null)
            {
                anim.SetBool("isMoving", true);
                anim.SetBool("isStrafing", false);
            }
        }
    }

    private void ShootAtPlayer()
    {
        Vector3 shootOrigin = gunBarrel != null ? gunBarrel.position : transform.position + Vector3.up;
        Debug.DrawLine(shootOrigin, player.position, Color.red, 0.1f);

        if (audioSource != null && shootSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(shootSound, gunVolume);
        }

        if (muzzleFlashPrefab != null)
        {
            Transform spawnPoint = gunBarrel != null ? gunBarrel : transform;
            Vector3 spawnPos = gunBarrel != null ? gunBarrel.position : transform.position + transform.TransformVector(muzzleOffset);
            Quaternion spawnRot = gunBarrel != null ? gunBarrel.rotation : transform.rotation;
            
            GameObject flash = Instantiate(muzzleFlashPrefab, spawnPos, spawnRot, spawnPoint);
            Destroy(flash, 0.1f);
        }

        PlayerHealth pHealth = player.GetComponent<PlayerHealth>();
        if (pHealth != null)
        {
            pHealth.TakeDamage(damage);
        }
    }

    /// <summary>
    /// Enemy.cs tarafından ölüm anında çağrılır. Dying animasyonu oynatır ve AI'ı durdurur.
    /// </summary>
    public void PlayDeathAnimation()
    {
        isDead = true;
        agent.isStopped = true;
        agent.enabled = false;

        if (anim != null)
        {
            anim.SetBool("isMoving", false);
            anim.SetBool("isStrafing", false);
            anim.SetTrigger("isDead");
        }
    }
}