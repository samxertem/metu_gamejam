using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Project;

namespace Project
{
    public class WakeUpCinematic : MonoBehaviour
    {
        [Header("Cinematic Elements")]
        public Transform playerCar;
        public CarController3D carController;
        public Transform cockpitCamPoint;
        
        [Header("Fade Settings")]
        public Image fadeOverlay;
        public float fadeDuration = 3.0f;
        
        [Header("Camera Pan Settings")]
        public float lookAngle = 45f; // How far left/right to look
        public float lookSpeed = 1.5f;

        private float cinematicTimer = 0f;
        private Quaternion initialCamRotation;
        
        private enum CinematicState { FadingIn, LookingAround, FadingOut, Done }
        private CinematicState state = CinematicState.FadingIn;

        void Awake()
        {
            // Auto-generate UI Canvas if fadeOverlay is not set
            if (fadeOverlay == null)
            {
                GameObject canvasObj = new GameObject("RuntimeWakeUpCanvas");
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999; 
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                
                canvasObj.AddComponent<GraphicRaycaster>();
                
                GameObject imgObj = new GameObject("FadeOverlay");
                imgObj.transform.SetParent(canvasObj.transform, false);
                fadeOverlay = imgObj.AddComponent<Image>();
                fadeOverlay.color = Color.black;
                
                RectTransform rt = fadeOverlay.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
        }

        void Start()
        {
            // Find references if not set
            if (playerCar == null)
            {
                GameObject pc = GameObject.Find("PlayerCar");
                if (pc != null)
                {
                    playerCar = pc.transform;
                    carController = pc.GetComponent<CarController3D>();
                    
                    Transform cp = pc.transform.Find("CockpitCamPoint");
                    if (cp != null) cockpitCamPoint = cp;
                }
            }

            // Lock controls and set car position and fix velocities immediately
            if (playerCar != null)
            {
                if (carController != null)
                {
                    carController.CanControl = false;
                    carController.enabled = false; // Disable entirely to prevent Start() from re-enabling gravity
                }

                Rigidbody rb = playerCar.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true; // completely freeze physics
                    rb.useGravity = false;
                    rb.velocity = Vector3.zero;
                    
                    Vector3 startPos = new Vector3(-288f, 9f, -143.0f);
                    Quaternion startRot = Quaternion.Euler(0f, 180f, 0f); // Face the police cars (+90 more to the right)
                    playerCar.position = startPos;
                    playerCar.rotation = startRot;
                    rb.position = startPos;
                    rb.rotation = startRot;
                }
            }
            
            // Set initial fade to pitch black
            if (fadeOverlay != null)
            {
                fadeOverlay.color = new Color(0, 0, 0, 1f);
                fadeOverlay.gameObject.SetActive(true);
            }

            if (cockpitCamPoint != null)
            {
                cockpitCamPoint.localRotation = Quaternion.identity; // Align with the car body
                initialCamRotation = cockpitCamPoint.localRotation;
            }
        }

        void Update()
        {
            if (state == CinematicState.Done) return;

            // Continually force kinematic to prevent any other scripts from waking physics up
            if (playerCar != null)
            {
                Rigidbody rb = playerCar.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
            }

            cinematicTimer += Time.deltaTime;

            if (state == CinematicState.FadingIn)
            {
                // Fade from black to clear over fadeDuration
                float alpha = Mathf.Lerp(1f, 0f, cinematicTimer / fadeDuration);
                if (fadeOverlay != null)
                {
                    fadeOverlay.color = new Color(0, 0, 0, alpha);
                }

                // Add slight wobble or looking even while fading
                SimulateLooking();

                if (cinematicTimer >= fadeDuration)
                {
                    state = CinematicState.LookingAround;
                    cinematicTimer = 0f;
                }
            }
            else if (state == CinematicState.LookingAround)
            {
                // Continue looking around for another 3 seconds
                SimulateLooking();

                if (cinematicTimer >= 3f)
                {
                    state = CinematicState.FadingOut;
                    cinematicTimer = 0f;
                }
            }
            else if (state == CinematicState.FadingOut)
            {
                // Fade to black and end scene
                float alpha = Mathf.Lerp(0f, 1f, cinematicTimer / 2f);
                if (fadeOverlay != null)
                {
                    fadeOverlay.color = new Color(0, 0, 0, alpha);
                }
                
                // Return camera to center softly
                if (cockpitCamPoint != null)
                {
                    cockpitCamPoint.localRotation = Quaternion.Slerp(cockpitCamPoint.localRotation, initialCamRotation, Time.deltaTime * 2f);
                }

                if (cinematicTimer >= 2.5f)
                {
                    state = CinematicState.Done;
                    // Scene Ends - Reloading Main Menu
                    Debug.Log("Wake Up Cinematic Finished. Returning to Main Menu.");
                    SceneManager.LoadScene("MainMenu");
                }
            }
        }

        private void SimulateLooking()
        {
            if (cockpitCamPoint == null) return;
            
            // A sine wave to sweep left, then right, then back
            // Since time starts at 0, Sin(0) = 0. We want to start at 0, go left (negative angle), then right.
            float currentAngle = Mathf.Sin(Time.time * lookSpeed) * lookAngle;
            
            // Optionally add a little pitch (nodding)
            float pitchAngle = Mathf.Cos(Time.time * lookSpeed * 1.5f) * 10f - 5f; 
            
            cockpitCamPoint.localRotation = initialCamRotation * Quaternion.Euler(pitchAngle, currentAngle, 0);
        }
    }
}
