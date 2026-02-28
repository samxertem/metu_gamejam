using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PerfectPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float crouchSpeed = 2.5f;
    public float jumpHeight = 2f;
    public float gravity = -20f;

    [Header("Audio")]
    public AudioClip footstepSound;
    [Range(0f, 1f)] public float footstepVolume = 0.2f;
    public float walkStepInterval = 0.5f;
    public float sprintStepInterval = 0.3f;
    public float crouchStepInterval = 0.7f;
    private AudioSource audioSource;
    private float stepTimer = 0f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;
    public float standHeight = 2f;
    public float crouchTransitionSpeed = 10f;
    public LayerMask obstacleMask = ~0; // Used to detect if we can stand up

    [Header("Look")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 85f;
    public Transform playerCamera;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;

    private bool isCrouching = false;
    private float targetHeight;
    private float targetCamHeight;
    private float standCamHeight;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Hide and lock the cursor to screen center
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Auto-assign camera if not set
        if (playerCamera == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) playerCamera = cam.transform;
        }

        // Initialize AudioSource for footsteps securely
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        targetHeight = standHeight;
        if (playerCamera != null)
        {
            standCamHeight = playerCamera.localPosition.y;
            targetCamHeight = standCamHeight;
        }
    }

    void Update()
    {
        HandleMouseLook();
        HandleCrouch();
        HandleMovement();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleCrouch()
    {
        // 1. Check for Crouch Input
        if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl))
        {
            isCrouching = true;
            targetHeight = crouchHeight;
            targetCamHeight = standCamHeight - (standHeight - crouchHeight) * 0.5f; 
        }
        else
        {
            // 2. Try to stand up, but check for obstacles above the head first
            if (isCrouching)
            {
                // Cast a ray upwards from the center of the capsule to see if there's roof above
                Vector3 rayOrigin = transform.position + (Vector3.up * (controller.height / 2f));
                float distanceToCeiling = (standHeight - controller.height) + 0.1f;
                
                if (!Physics.Raycast(rayOrigin, Vector3.up, distanceToCeiling, obstacleMask))
                {
                    // No obstacle found, safe to stand up
                    isCrouching = false;
                    targetHeight = standHeight;
                    targetCamHeight = standCamHeight;
                }
            }
        }

        // 3. Smoothly adjust character controller height
        float lastHeight = controller.height;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        
        // Adjust the controller center so the player stays perfectly planted on the ground
        Vector3 newCenter = controller.center;
        newCenter.y = controller.height / 2f;
        controller.center = newCenter;

        // 4. Smoothly adjust the camera position down
        if (playerCamera != null)
        {
            Vector3 camPos = playerCamera.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, targetCamHeight, Time.deltaTime * crouchTransitionSpeed);
            playerCamera.localPosition = camPos;
        }
    }

    private void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small constant downward force to stick to ground
        }

        // Gather input
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        if (move.magnitude > 1f) move.Normalize();

        // Determine Speed based on State
        float currentSpeed = walkSpeed;
        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = sprintSpeed;
        }

        // Move horizontally
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jump (only allow if grounded and NOT crouching under an obstacle)
        // If they are crouching in the open, they can jump, but if stuck under a vent, no jumping.
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply Gravity
        velocity.y += gravity * Time.deltaTime;
        
        // Move vertically
        controller.Move(velocity * Time.deltaTime);

        // 5. Handle Footstep Audio
        HandleFootsteps(move.magnitude, isGrounded);
    }

    private void HandleFootsteps(float movementMagnitude, bool isGrounded)
    {
        if (!isGrounded || movementMagnitude < 0.1f) return;

        stepTimer += Time.deltaTime;

        float currentStepInterval = walkStepInterval;
        if (isCrouching) currentStepInterval = crouchStepInterval;
        else if (Input.GetKey(KeyCode.LeftShift)) currentStepInterval = sprintStepInterval;

        if (stepTimer >= currentStepInterval)
        {
            PlayFootstepSound();
            stepTimer = 0f;
        }
    }

    private void PlayFootstepSound()
    {
        if (audioSource != null && footstepSound != null)
        {
            // Vary pitch slightly to avoid robotic looping
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(footstepSound, footstepVolume);
        }
    }
}
