using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Fareyi Çevirince Gecikme (Sway)")]
    public float smooth = 8f;
    public float swayMultiplier = 2f;

    [Header("Yürürken Sallanma (Bobbing)")]
    public float bobSpeed = 14f;
    public float bobAmount = 0.05f;

    private Vector3 initialPosition;
    private float timer = 0f;

    void Start()
    {
        // Silahın başlangıç pozisyonunu hafızaya al
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        // --- 1. SWAY (Fareyi çevirince silahın hafif geriden gelmesi) ---
        float mouseX = Input.GetAxis("Mouse X") * swayMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * swayMultiplier;

        // Silahı farenin tersi yönüne hafifçe döndür
        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);
        Quaternion targetRotation = rotationX * rotationY;

        // Yumuşak bir şekilde dön
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smooth * Time.deltaTime);

        // --- 2. BOBBING (Yürürken silahın sekmesi) ---
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 targetPosition = initialPosition;

        // Eğer oyuncu hareket ediyorsa (WASD basıyorsa)
        if (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f)
        {
            timer += Time.deltaTime * bobSpeed;
            targetPosition = new Vector3(
                initialPosition.x + Mathf.Cos(timer / 2) * bobAmount, 
                initialPosition.y + Mathf.Sin(timer) * bobAmount, 
                initialPosition.z
            );
        }
        else
        {
            // Duruyorsa yavaşça merkeze dön
            timer = 0f;
            targetPosition = new Vector3(
                Mathf.Lerp(transform.localPosition.x, initialPosition.x, Time.deltaTime * smooth),
                Mathf.Lerp(transform.localPosition.y, initialPosition.y, Time.deltaTime * smooth),
                initialPosition.z
            );
        }

        // Pozisyonu yumuşakça uygula
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * smooth);
    }
}