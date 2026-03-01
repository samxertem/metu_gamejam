using UnityEngine;

namespace Project {
    [RequireComponent(typeof(Rigidbody))]
    public class CarController3D : MonoBehaviour
    {
        public bool CanControl = true;
        
        [Header("Movement")]
        [Header("Movement")]
        public float maxSpeed = 40f;
        public float acceleration = 25f;
        public float brakeForce = 35f;
        public float reverseMaxSpeed = 10f;
        
        [Header("Coasting (W bırakınca)")]
        [Tooltip("Gaz bırakınca ne kadar yavaş yavaşlasın (düşük = daha uzun kayar)")]
        public float rollingFriction = 3f;
        
        [Header("Turning")]
        public float minSteer = 55f;
        public float maxSteer = 140f;
        
        [Header("Grip")]
        public float lateralGrip = 8f;

        [Header("Audio")]
        public AudioSource idleEngineSound;
        public AudioSource runningEngineSound;
        public float minPitch = 0.8f;
        public float maxPitch = 2.0f;

        private Rigidbody rb;
        private float debugTimer = 0f;

        public float CurrentSpeed => rb != null ? rb.velocity.magnitude : 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            if (rb != null)
            {
                rb.centerOfMass = new Vector3(0, -0.5f, 0);
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.drag = 0f;
            }
        }

        private void FixedUpdate()
        {
            if (rb == null || !CanControl) return;

            rb.WakeUp();

            // Direct KeyCode input
            float moveInput = 0f;
            float turnInput = 0f;
            
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                moveInput = 1f;
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                moveInput = -1f;
            
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                turnInput = 1f;
            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                turnInput = -1f;

            // Current forward speed
            float currentForwardSpeed = Vector3.Dot(rb.velocity, transform.forward);

            // Determine target speed and deceleration rate
            float targetSpeed;
            float rate;

            if (moveInput > 0.1f)
            {
                // Accelerating forward
                targetSpeed = maxSpeed;
                rate = acceleration;
            }
            else if (moveInput < -0.1f)
            {
                if (currentForwardSpeed > 1f)
                {
                    // Moving forward but pressing S = BRAKE (fast stop)
                    targetSpeed = 0f;
                    rate = brakeForce;
                }
                else
                {
                    // Reversing
                    targetSpeed = -reverseMaxSpeed;
                    rate = acceleration * 0.7f;
                }
            }
            else
            {
                // NO INPUT = COAST (slow gradual deceleration)
                targetSpeed = 0f;
                rate = rollingFriction;  // Very gentle — car rolls forward naturally
            }

            // Smoothly move toward target speed
            float newForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, targetSpeed, rate * Time.fixedDeltaTime);

            // Build velocity: forward + damped lateral + preserved gravity Y
            Vector3 localVel = transform.InverseTransformDirection(rb.velocity);
            localVel.z = newForwardSpeed;
            localVel.x *= (1f - lateralGrip * Time.fixedDeltaTime);

            Vector3 worldVel = transform.TransformDirection(localVel);
            worldVel.y = rb.velocity.y;  // Preserve gravity
            rb.velocity = worldVel;

            // Turning
            if (Mathf.Abs(currentForwardSpeed) > 0.5f)
            {
                float direction = currentForwardSpeed >= 0f ? 1f : -1f;
                float speed01 = Mathf.Clamp01(Mathf.Abs(currentForwardSpeed) / maxSpeed);
                float steerStrength = Mathf.Lerp(maxSteer, minSteer, speed01);
                
                float yawAngle = turnInput * direction * steerStrength * Time.fixedDeltaTime;
                Quaternion deltaRot = Quaternion.Euler(0f, yawAngle, 0f);
                rb.MoveRotation(rb.rotation * deltaRot);
            }

            // Debug
            debugTimer += Time.fixedDeltaTime;
            if (debugTimer >= 1f)
            {
                debugTimer = 0f;
                // Debug.Log($"[Car] V:{moveInput} H:{turnInput} fwd:{currentForwardSpeed:F1} speed:{rb.velocity.magnitude:F1}");
            }
        }

        private void Update()
        {
            UpdateAudio();
        }

        private void UpdateAudio()
        {
            if (idleEngineSound == null || runningEngineSound == null) return;

            float speedRatio = Mathf.Clamp01(CurrentSpeed / maxSpeed);
            float pitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);

            // Set Pitch
            idleEngineSound.pitch = pitch;
            runningEngineSound.pitch = pitch;

            // Simple crossfade
            // at speed 0, idle is volume 1, running is volume 0
            // at speed max, idle is volume 0, running is volume 1
            idleEngineSound.volume = Mathf.Lerp(1f, 0f, speedRatio * 2f); // Fades out quicker
            runningEngineSound.volume = Mathf.Lerp(0f, 1f, speedRatio);
        }
    }
}