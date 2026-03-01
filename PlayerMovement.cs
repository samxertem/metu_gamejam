using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float sprintMultiplier = 1.8f;
    public float crouchMultiplier = 0.5f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;
    public float fallMultiplier = 2.5f; // Düşerken yerçekimi çarpanı (daha hızlı iniş)

    [Header("Crouch Settings")]
    public float normalHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchTransitionSpeed = 8f;
    public float normalCameraY = 0f;       // Normal kamera yüksekliği (local Y)
    public float crouchCameraY = -0.5f;    // Çömelme kamera yüksekliği (local Y)

    [Header("Audio Settings")]
    public float stepInterval = 0.5f;
    public AudioClip[] footstepSounds;
    private AudioSource audioSource;
    private bool isStepping = false;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    // Crouch state
    private bool isCrouching = false;
    private bool wantsToCrouch = false;
    private float currentHeight;
    private float targetHeight;
    private float currentCamY;
    private float targetCamY;

    [Header("Camera Lookup Setup")]
    public Transform cameraTarget;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        // Kamera atanmamışsa çocuğundan veya ana kameradan bul
        if (cameraTarget == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraTarget = cam.transform;
            else if (Camera.main != null) cameraTarget = Camera.main.transform;
        }

        // Normal yüksekliği kaydet
        normalHeight = controller.height;
        currentHeight = normalHeight;
        targetHeight = normalHeight;

        // Kamera yüksekliğini kaydet
        if (cameraTarget != null)
        {
            normalCameraY = cameraTarget.localPosition.y;
            crouchCameraY = normalCameraY - (normalHeight - crouchHeight) * 0.5f;
            currentCamY = normalCameraY;
            targetCamY = normalCameraY;
        }

        // AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        // 1. Yer çekimi ve Zemin kontrolü
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // 2. Çömelme kontrolü (Ctrl)
        HandleCrouch();

        // Zıplama kontrolü (Space)
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            // Eğer üstte bir engel yoksa zıpla
            if (!IsBlockedAbove())
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        // 3. WASD Girdi Okuma
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        if (move.magnitude > 1f)
            move.Normalize();

        // 4. Hız hesapla (crouch < normal < sprint)
        float currentSpeed = moveSpeed;
        if (isCrouching)
        {
            currentSpeed = moveSpeed * crouchMultiplier;
        }
        else if (Input.GetKey(KeyCode.LeftShift) && move.magnitude > 0.1f)
        {
            currentSpeed = moveSpeed * sprintMultiplier;
        }

        // 5. Hareketi Uygulama
        controller.Move(move * currentSpeed * Time.deltaTime);

        // 6. Yer çekimi (düşerken ekstra hızlı — floaty hissi engellemek için)
        if (velocity.y < 0)
        {
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
        controller.Move(velocity * Time.deltaTime);

        // 7. Adım Sesleri (Yerdeyken ve hareket ediyorken)
        if (isGrounded && move.magnitude > 0.1f && !isStepping)
        {
            float interval = isCrouching ? stepInterval * 1.5f : stepInterval;
            if (currentSpeed > moveSpeed) interval = stepInterval * 0.65f; // Sprint: daha sık adım
            StartCoroutine(PlayFootstepSound(interval));
        }
    }

    private void HandleCrouch()
    {
        wantsToCrouch = Input.GetKey(KeyCode.LeftControl);

        if (wantsToCrouch && !isCrouching)
        {
            // Çömel
            isCrouching = true;
            targetHeight = crouchHeight;
            targetCamY = crouchCameraY;
        }
        else if (!wantsToCrouch && isCrouching)
        {
            // Kalkmaya çalış — üstte engel var mı?
            if (!IsBlockedAbove())
            {
                isCrouching = false;
                targetHeight = normalHeight;
                targetCamY = normalCameraY;
            }
        }

        // Smooth yükseklik geçişi
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        controller.height = currentHeight;

        // CharacterController center'ı ayarla (ayaklar yerde kalsın)
        float centerY = currentHeight * 0.5f;
        controller.center = new Vector3(0f, centerY, 0f);

        // Kamera yüksekliğini ayarla
        if (cameraTarget != null)
        {
            currentCamY = Mathf.Lerp(currentCamY, targetCamY, crouchTransitionSpeed * Time.deltaTime);
            Vector3 camPos = cameraTarget.localPosition;
            camPos.y = currentCamY;
            cameraTarget.localPosition = camPos;
        }
    }

    /// <summary>
    /// Üstte engel var mı kontrol et (çömelme bırakıldığında)
    /// </summary>
    private bool IsBlockedAbove()
    {
        float checkDistance = normalHeight - crouchHeight;
        Vector3 origin = transform.position + Vector3.up * crouchHeight;
        return Physics.Raycast(origin, Vector3.up, checkDistance + 0.1f);
    }

    public bool IsCrouching => isCrouching;
    public bool IsSprinting => !isCrouching && Input.GetKey(KeyCode.LeftShift);

    private IEnumerator PlayFootstepSound(float interval)
    {
        isStepping = true;

        if (footstepSounds != null && footstepSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            audioSource.PlayOneShot(clip);
        }

        yield return new WaitForSeconds(interval);
        isStepping = false;
    }
}
