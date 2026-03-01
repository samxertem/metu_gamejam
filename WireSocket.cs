using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Kablo soketi. Doğru renkli kablo bırakıldığında kabul eder, yanlışta reddeder.
/// </summary>
public class WireSocket : MonoBehaviour, IDropHandler
{
    [Header("Soket Rengi")]
    public WireColorType socketColor;

    [Header("Referanslar")]
    public WiresHackMinigame manager;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        DraggableWire wire = eventData.pointerDrag.GetComponent<DraggableWire>();
        if (wire == null || wire.locked) return;

        if (manager == null || !manager.isRunning) return;

        RectTransform socketRect = GetComponent<RectTransform>();

        if (wire.wireColor == socketColor)
        {
            // Doğru eşleşme
            wire.SnapTo(socketRect);
            manager.NotifyCorrect(wire);
        }
        else
        {
            // Yanlış eşleşme
            wire.ReturnToStart();
            manager.NotifyWrong();
        }
    }
}
