using UnityEngine;
using UnityEngine.SceneManagement;
using Project; // to access CarController3D
using TMPro;

public class AutoCrashCinematic : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("The exact spawn point for the thief and the target for the car.")]
    public Vector3 targetCoordinates = new Vector3(-289.23f, 7.64f, -140.61f);
    [Tooltip("Target rotation Y for the thief.")]
    public float thiefRotationY = -7.137f;
    [Tooltip("Distance from target coordinates to trigger the cinematic.")]
    public float triggerRadius = 30f;

    [Header("Cinematic Elements")]
    public Transform playerCar;
    public CarController3D carController;
    public Rigidbody carRigidbody;
    
    [Tooltip("The actual character object in the scene to crash into.")]
    public GameObject thiefCharacter;
    
    [Header("Cinematic Settings")]
    [Tooltip("Speed at which the car is forced towards the target.")]
    public float crashSpeed = 25f;
    [Tooltip("Time to wait after the crash before reloading the scene.")]
    public float reloadDelay = 3f;

    [Header("Transition")]
    public AudioClip transitionVoice; // yargıçloop1
    public AudioClip crashSound;     // pwlpl-car_crash-377291

    [Header("Fade Settings")]
    [Tooltip("Distance at which the screen starts to fade to black.")]
    public float fadeStartDistance = 10f;

    private bool hasTriggered = false;
    private bool hasCrashed = false;
    private bool crashSoundPlayed = false;
    
    private Canvas cinematicCanvas;
    private UnityEngine.UI.Image fadeImage;

    private void Start()
    {
        // Try finding components if not assigned
        if (playerCar == null)
        {
            GameObject p = GameObject.Find("PlayerCar");
            if (p != null) playerCar = p.transform;
            else Debug.LogError("[AutoCrashCinematic] PlayerCar not assigned or found in scene!");
        }

        if (playerCar != null && carController == null)
            carController = playerCar.GetComponent<CarController3D>();

        if (playerCar != null && carRigidbody == null)
            carRigidbody = playerCar.GetComponent<Rigidbody>();

        // Create Fade UI Canvas
        SetupFadeUI();

        // Extra camera setup removed based on user request.
    }

    private void Update()
    {
        if (hasTriggered || playerCar == null) return;

        // Check 2D distance (ignoring Y/height differences)
        Vector2 playerPos2D = new Vector2(playerCar.position.x, playerCar.position.z);
        Vector2 targetPos2D = new Vector2(targetCoordinates.x, targetCoordinates.z);

        if (Vector2.Distance(playerPos2D, targetPos2D) <= triggerRadius)
        {
            StartCinematic();
        }
    }

    private void FixedUpdate()
    {
        if (!hasTriggered || hasCrashed || playerCar == null || carRigidbody == null) return;

        // Force the car to drive towards the target coordinates
        Vector3 direction = (targetCoordinates - playerCar.position).normalized;
        direction.y = 0; // Keep horizontal

        // Smoothly rotate car towards the target
        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            carRigidbody.MoveRotation(Quaternion.Slerp(playerCar.rotation, targetRot, Time.fixedDeltaTime * 5f));
        }

        // Apply forward velocity
        carRigidbody.velocity = direction * crashSpeed;

        // Check if we reached the target (crash impact)
        float distanceToTarget = Vector3.Distance(new Vector3(playerCar.position.x, 0, playerCar.position.z), new Vector3(targetCoordinates.x, 0, targetCoordinates.z));
        
        // Handle distance-based fade
        if (fadeImage != null)
        {
            if (distanceToTarget <= fadeStartDistance)
            {
                // Full black at 3.5f (impact distance), clear at fadeStartDistance
                float alpha = Mathf.InverseLerp(fadeStartDistance, 3.5f, distanceToTarget);
                Color c = fadeImage.color;
                c.a = alpha;
                fadeImage.color = c;
            }
        }

        // 20 metre kala kaza sesini çal
        if (!crashSoundPlayed && distanceToTarget < 23.5f && crashSound != null)
        {
            AudioSource.PlayClipAtPoint(crashSound, playerCar.position, 1f);
            crashSoundPlayed = true;
        }

        if (distanceToTarget < 3.5f) // Impact threshold
        {
            TriggerCrash();
        }
    }

    private void StartCinematic()
    {
        Debug.Log("[AutoCrashCinematic] Triggered! Taking control away from player.");
        hasTriggered = true;

        // 1. Disable player control
        if (carController != null)
        {
            carController.CanControl = false;
        }

        // The character is already in the scene, so we don't need to spawn it here.
        if (thiefCharacter == null)
        {
            Debug.LogError("[AutoCrashCinematic] Thief character is missing!");
        }

        // We no longer switch to a cinematic camera or hide canvases,
        // so the player experiences the crash from the normal driving camera perspective.
    }

    private void TriggerCrash()
    {
        if (hasCrashed) return;

        Debug.Log("[AutoCrashCinematic] CRASH IMPACT!");
        hasCrashed = true;

        // Stop the car immediately
        if (carRigidbody != null)
        {
            carRigidbody.velocity = Vector3.zero;
            carRigidbody.angularVelocity = Vector3.zero;
            carRigidbody.isKinematic = true; // Freeze it in place
        }

        // Optional: Knock the character back
        if (thiefCharacter != null)
        {
            Rigidbody charRb = thiefCharacter.GetComponent<Rigidbody>();
            if (charRb == null) charRb = thiefCharacter.AddComponent<Rigidbody>();
            
            charRb.mass = 60f;
            Vector3 knockbackDir = (targetCoordinates - playerCar.position).normalized + Vector3.up * 0.5f;
            charRb.AddForce(knockbackDir * 1000f, ForceMode.Impulse);
        }

        // 14 saniye siyah ekran bekle, sonra Level_02'yi yükle
        StartCoroutine(WaitAndLoadNextLevel());
    }

    private System.Collections.IEnumerator WaitAndLoadNextLevel()
    {
        // Ekranın tamamen siyah olduğundan emin ol
        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 1);

        // Araba motor sesini kapat
        if (playerCar != null)
        {
            AudioSource[] carAudioSources = playerCar.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource src in carAudioSources)
                src.Stop();
        }

        // "Part 2: Mahkeme" yazısını göster ve yargıçloop1 sesini çal
        SceneTransitionHelper.ShowTransition("Part 2: Mahkeme", transitionVoice);

        // 14 saniye siyah ekran bekle
        yield return new WaitForSeconds(14f);

        // Level_02 courtroom sahnesini yükle
        Debug.Log("[AutoCrashCinematic] Loading Level_02...");
        SceneManager.LoadScene("Level_02");
    }

    private void SetupFadeUI()
    {
        GameObject canvasObj = new GameObject("CinematicFadeCanvas");
        cinematicCanvas = canvasObj.AddComponent<Canvas>();
        cinematicCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        cinematicCanvas.sortingOrder = 999; // Render on top of everything

        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        
        fadeImage = imageObj.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // Start fully transparent
        
        RectTransform rt = fadeImage.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
    }
}
