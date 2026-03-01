using UnityEngine;

public class Recoil : MonoBehaviour
{
    [Header("Recoil Targets")]
    public Transform cameraTransform; 
    [Tooltip("If empty, uses the GameObject this script is attached to.")]
    public Transform weaponTransform; 

    [Header("Weapon Sway (Fare Gecikmesi)")]
    public float swaySmooth = 8f;
    public float swayMultiplier = 2f;

    [Header("Weapon Bobbing (Yürüme Sekmesi)")]
    public float bobSpeed = 14f;
    public float bobAmount = 0.05f;

    [Header("Rotational Recoil (Camera & Gun)")]
    public Vector3 recoilRotation = new Vector3(-2f, 2f, 0.5f);
    
    [Header("Positional Recoil (Gun only)")]
    public Vector3 kickBackPosition = new Vector3(0f, 0f, -0.2f);

    [Header("Spring Settings")]
    public float snappiness = 6f; // How fast the recoil kicks
    public float returnSpeed = 2f; // How fast it settles back to normal

    // Procedural states
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    private Vector3 currentPosition;
    private Vector3 targetPosition;
    
    // Sway & Bob states
    private float bobTimer = 0f;
    private Vector3 smoothedBobPos;
    private Quaternion smoothedSwayRot = Quaternion.identity;

    // Base Rest offsets
    private Vector3 weaponOriginalLocalPos;
    private Quaternion weaponOriginalLocalRot;

    void Start()
    {
        if (weaponTransform == null)
            weaponTransform = transform;

        weaponOriginalLocalPos = weaponTransform.localPosition;
        weaponOriginalLocalRot = weaponTransform.localRotation;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // 1. RECOIL SPRING (Target values constantly return to 0)
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        targetPosition = Vector3.Lerp(targetPosition, Vector3.zero, returnSpeed * Time.deltaTime);

        // Current values chase the target values rapidly (the snap)
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);
        currentPosition = Vector3.Lerp(currentPosition, targetPosition, snappiness * Time.deltaTime);

        // 2. SWAY (Fare takibi)
        float mouseX = Input.GetAxis("Mouse X") * swayMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * swayMultiplier;
        
        Quaternion swayRotX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion swayRotY = Quaternion.AngleAxis(mouseX, Vector3.up);
        Quaternion targetSwayRotation = swayRotX * swayRotY;
        
        // Slerp sway to make it buttery smooth
        smoothedSwayRot = Quaternion.Slerp(smoothedSwayRot, targetSwayRotation, Time.deltaTime * swaySmooth);

        // 3. BOBBING (Yürüme sekmesi)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 targetBobPosition = Vector3.zero;
        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            targetBobPosition = new Vector3(
                Mathf.Cos(bobTimer / 2) * bobAmount, 
                Mathf.Sin(bobTimer) * bobAmount, 
                0f
            );
        }
        else
        {
            bobTimer = 0f;
        }

        // Lerp bobbing
        smoothedBobPos = Vector3.Lerp(smoothedBobPos, targetBobPosition, Time.deltaTime * swaySmooth);

        // 4. APPLY TO WEAPON
        if (weaponTransform != null)
        {
            // Combine: Base Position + Smooth Bobbing + Snappy Recoil Kickback
            // CRITICAL FIX: The weapon must maintain its starting offset so it doesn't fly into the camera.
            weaponTransform.localPosition = weaponOriginalLocalPos + smoothedBobPos + currentPosition;

            // CRITICAL FIX: Ensure we don't accidentally override the camera's pitch controller
            if (weaponTransform != cameraTransform)
            {
                // Combine: Base Rotation + Smooth Sway + Snappy Recoil Rotation
                weaponTransform.localRotation = weaponOriginalLocalRot * smoothedSwayRot * Quaternion.Euler(currentRotation);
            }
        }
    }

    void LateUpdate()
    {
        // 5. APPLY RECOIL KICK TO CAMERA
        // We do this in LateUpdate so it applies AFTER the PlayerController calculates the Mouse Look!
        if (cameraTransform != null && currentRotation.magnitude > 0.01f)
        {
            cameraTransform.localRotation *= Quaternion.Euler(currentRotation);
        }
    }

    /// <summary>
    /// Call this method from the GunController every time a shot is fired.
    /// </summary>
    public void FireRecoil()
    {
        // Add a burst of recoil rotation
        targetRotation += new Vector3(
            recoilRotation.x, 
            Random.Range(-recoilRotation.y, recoilRotation.y), 
            Random.Range(-recoilRotation.z, recoilRotation.z)
        );
        
        // Add a burst of backward positional kick
        targetPosition += new Vector3(
            Random.Range(-kickBackPosition.x, kickBackPosition.x), 
            Random.Range(-kickBackPosition.y, kickBackPosition.y), 
            kickBackPosition.z
        );
    }
}
