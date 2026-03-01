using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform playerBody;
    public float mouseSensitivity = 80f;

    float xRotation = 0f;
    float yRotation = 0f;
    Camera cam;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Kamera referansı
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.nearClipPlane = 0.01f;  // Duvara yakın clipping engelle
            cam.fieldOfView = 75f;       // FOV sabit
        }

        // Player objesini otomatik bul
        if (playerBody == null)
        {
            GameObject p = GameObject.Find("Player");
            if (p != null) playerBody = p.transform;
        }
        if (playerBody == null)
        {
            CharacterController cc = FindObjectOfType<CharacterController>();
            if (cc != null) playerBody = cc.transform;
        }

        if (playerBody != null)
        {
            yRotation = playerBody.eulerAngles.y;
        }

        // Kamera lokal pozisyonunu sıfırla (arkaya kayma engelle)
        transform.localPosition = Vector3.zero;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Yukarı-Aşağı
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -45f, 60f);

        // Sağa-Sola
        yRotation += mouseX;

        // Kamerayı her iki eksende döndür
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);

        // Player gövdesini de sağa-sola döndür (yürüme yönü için)
        if (playerBody != null)
        {
            playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }

        // FOV sabit tut — scroll wheel veya başka bir şey değiştirmesin
        if (cam != null && cam.fieldOfView != 75f)
        {
            cam.fieldOfView = 75f;
        }
    }

}