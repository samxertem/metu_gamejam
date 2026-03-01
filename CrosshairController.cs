using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    public RectTransform top, bottom, left, right;
    public float idleSpread = 8f; // Başlangıç açıklığı
    public float currentSpread;
    public float spreadSpeed = 10f;

    void Start()
    {
        currentSpread = idleSpread;
    }

    void Update()
    {
        // Zamanla crosshair'ı eski haline (idle) döndür
        currentSpread = Mathf.Lerp(currentSpread, idleSpread, Time.deltaTime * spreadSpeed);

        // Çizgilerin pozisyonlarını güncelle
        top.anchoredPosition = new Vector2(0, currentSpread);
        bottom.anchoredPosition = new Vector2(0, -currentSpread);
        left.anchoredPosition = new Vector2(-currentSpread, 0);
        right.anchoredPosition = new Vector2(currentSpread, 0);
    }

    // Ateş edildiğinde dışarıdan çağrılacak fonksiyon
    public void AddSpread(float amount)
    {
        currentSpread += amount;
    }
}