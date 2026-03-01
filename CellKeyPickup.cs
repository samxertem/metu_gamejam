using UnityEngine;

/// <summary>
/// Hücre anahtarı pickup. E ile alınır, GameManager.hasKey = true olur.
/// Dolabın yanında sarı ışıkla parlar.
/// </summary>
public class CellKeyPickup : MonoBehaviour, IInteractable
{
    public string GetInteractText()
    {
        return "Hücre Anahtarını Al";
    }

    public void Interact()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.hasKey = true;
        }

        Debug.Log("🔑 Hücre anahtarını aldın!");

        // Ekranda bildirim göster
        if (PickupNotification.Instance != null)
        {
            PickupNotification.Instance.Show("Anahtar alındı.");
        }

        // Objeyi (ışık dahil) yok et
        Destroy(gameObject);
    }
}
