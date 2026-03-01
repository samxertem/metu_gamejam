using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Sürüklenebilir kablo parçası. Doğru sokete bırakılınca snap olur ve kilitlenir.
/// Canvas scaleFactor ile uyumlu sürükleme.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class DraggableWire : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Kablo Rengi")]
    public WireColorType wireColor;

    [Header("Referanslar")]
    public Canvas canvas;

    [HideInInspector] public bool locked = false;
    [HideInInspector] public bool snappedThisFrame = false;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform startParent;
    private Vector2 startPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        // startParent/startPosition Awake'de kaydedilmeli
        // cunku PopupRoot inactive basliyor, Start() gecikebilir
        // ama BeginGame→ResetWire hemen cagirilabilir
        startParent = transform.parent;
        startPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (locked)
        {
            eventData.pointerDrag = null;
            return;
        }

        snappedThisFrame = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;

        // En üstte çizilsin diye canvas root'a taşı
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (locked) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (!snappedThisFrame)
        {
            ReturnToStart();
        }
    }

    /// <summary>
    /// Kabloyu soket pozisyonuna snap et ve kilitle.
    /// </summary>
    public void SnapTo(RectTransform socketRect)
    {
        snappedThisFrame = true;
        locked = true;

        transform.SetParent(socketRect);
        rectTransform.anchoredPosition = Vector2.zero;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Kabloyu başlangıç pozisyonuna döndür.
    /// </summary>
    public void ReturnToStart()
    {
        transform.SetParent(startParent);
        rectTransform.anchoredPosition = startPosition;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Kabloyu tamamen resetle (yeni oyun için).
    /// </summary>
    public void ResetWire()
    {
        locked = false;
        snappedThisFrame = false;
        transform.SetParent(startParent);
        rectTransform.anchoredPosition = startPosition;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }
}
