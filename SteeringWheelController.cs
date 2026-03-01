using UnityEngine;

namespace Project {
    public class SteeringWheelController : MonoBehaviour
    {
        [Header("Settings")]
        public CarController3D carController;
        public float maxSteeringAngle = 90f; 
        public float steeringSpeed = 10f;

        private float currentSteerAngle = 0f;
        
        // Pivot noktası için meshin kendi merkezi kullanılacak
        private Vector3 pivotPoint;
        private Quaternion initialRotation;
        private Vector3 initialPosition;

        private void Start()
        {
            initialRotation = transform.localRotation;
            initialPosition = transform.localPosition;
            
            // Eğer atanmadıysa PlayerCar'ı bulmaya çalış
            if (carController == null)
            {
                carController = GetComponentInParent<CarController3D>();
            }

            // Mesh merkezini pivot olarak hesapla
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null)
            {
                pivotPoint = transform.InverseTransformPoint(mr.bounds.center);
            }
            else
            {
                pivotPoint = Vector3.zero;
            }
        }

        private void Update()
        {
            if (carController == null) return;

            // Use Unity's smoothed Input Axis instead of raw discrete keys for buttery smooth wheel turning
            float turnInput = Input.GetAxisRaw("Horizontal");
            
            // Add our own smoothing to turnInput if raw is too snappy, or just use GetAxis. 
            // GetAxis is heavily smoothed, let's use GetAxis.
            turnInput = Input.GetAxis("Horizontal");

            float targetAngle = turnInput * maxSteeringAngle;
            
            // Lerp ile yumuşat
            currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetAngle, steeringSpeed * Time.deltaTime);

            // Önceki hatanın sebebi pozisyonu resetlememekti.
            // RotateAround, pozisyonu değiştirir. Sadece rotasyonu resetlemek yetmez!
            transform.localPosition = initialPosition;
            transform.localRotation = initialRotation;
            
            // Şimdi hesaplanmış gerçek dünya pivot noktası etrafında döndür
            // Direksiyon mili arabanın gidiş yönüne doğru değil, genelde bize (geriye/yukarı) bakar.
            // Fakat FBX'in forward (+Z) veya up (+Y) eksenine göre döner.
            // Genelde araba modelinin direksiyonları modellerken forward ekseni direksiyon mili hizasına getirilir.
            transform.RotateAround(transform.TransformPoint(pivotPoint), transform.forward, -currentSteerAngle);
        }
    }
}
