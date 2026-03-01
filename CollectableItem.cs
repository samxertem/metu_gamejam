using UnityEngine;

/// <summary>
/// Dolap etkileşim sistemi.
/// E tuşuna bas -> dolap kapalıysa aç, açıksa içindeki eşyayı al.
/// Dolabın her tarafından etkileşim sağlanır (tek BoxCollider).
/// </summary>
public class CollectableItem : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class CollectableData
    {
        public string itemName = "Obje";
        public bool isKey = false;
        public GameObject objectToHide;
    }

    public CollectableData[] items = new CollectableData[]
    {
        new CollectableData { itemName = "Tabanca" }
    };

    public int currentIndex = 0;

    [Header("Dolap Kapıları (otomatik bulunur)")]
    public CabinetDoor[] cabinetDoors;

    [Header("Dolap Boşaldığında")]
    public bool hideOnEmpty = false;
    public bool hideChildrenOnEmpty = true;

    private void Start()
    {
        if (cabinetDoors == null || cabinetDoors.Length == 0)
        {
            cabinetDoors = GetComponentsInChildren<CabinetDoor>();
        }
    }

    private bool HasCabinetDoors()
    {
        return cabinetDoors != null && cabinetDoors.Length > 0;
    }

    private bool IsAnyCabinetDoorOpen()
    {
        if (!HasCabinetDoors()) return true;

        foreach (var door in cabinetDoors)
        {
            if (door != null && door.isOpen)
                return true;
        }
        return false;
    }

    private void OpenFirstClosedDoor()
    {
        foreach (var door in cabinetDoors)
        {
            if (door != null && !door.isOpen)
            {
                door.Interact();
                return;
            }
        }
    }

    public void Interact()
    {
        // 1) Dolap kapalıysa -> önce kapıyı aç
        if (HasCabinetDoors() && !IsAnyCabinetDoorOpen())
        {
            OpenFirstClosedDoor();
            return;
        }

        // 2) Eşyalar bittiyse hiçbir şey yapma
        if (currentIndex >= items.Length)
        {
            return;
        }

        // 3) Eşyayı al
        CollectableData currentItem = items[currentIndex];

        Debug.Log(currentItem.itemName + " alindi!");

        // Anahtar kontrolü
        if (currentItem.isKey && GameManager.Instance != null)
        {
            GameManager.Instance.hasKey = true;
        }

        // Ekranda bildirim göster
        string notifText = currentItem.isKey ? "Anahtar alındı." : currentItem.itemName + " alındı.";
        if (PickupNotification.Instance != null)
        {
            PickupNotification.Instance.Show(notifText);
        }

        // Varsa spesifik objeyi gizle
        if (currentItem.objectToHide != null)
        {
            currentItem.objectToHide.SetActive(false);
        }

        currentIndex++;

        // Son eşya alındıysa
        if (currentIndex >= items.Length)
        {
            if (hideOnEmpty)
            {
                gameObject.SetActive(false);
            }
            else if (hideChildrenOnEmpty)
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

    public string GetInteractText()
    {
        // Dolap kapalıysa -> "Dolabı Aç"
        if (HasCabinetDoors() && !IsAnyCabinetDoorOpen())
        {
            return "Dolabı Aç";
        }

        // Eşyalar bittiyse
        if (currentIndex >= items.Length)
        {
            return "Dolap boş";
        }

        return items[currentIndex].itemName + " Al";
    }
}
