using UnityEngine;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    public Transform interactPoint;
    public float interactRadius = 2.5f;
    public float raycastDistance = 3.5f;

    [Header("UI (Opsiyonel)")]
    public Text interactUIText;

    private IInteractable currentInteractable;

    void Start()
    {
        if (interactPoint == null)
        {
            Transform t = transform.Find("InteractPoint");
            if (t != null)
                interactPoint = t;
        }
    }

    void Update()
    {
        CheckForInteractable();

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    void CheckForInteractable()
    {
        IInteractable found = null;

        // YOL 1: Kameradan Raycast (ekranın ortasından)
        Camera cam = Camera.main;
        if (cam != null)
        {
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, raycastDistance))
            {
                found = hit.collider.GetComponent<IInteractable>();
                // Parent'ta da ara (prefab child collider olabilir)
                if (found == null)
                    found = hit.collider.GetComponentInParent<IInteractable>();
            }
        }

        // YOL 2: Fallback — OverlapSphere (Ray ıskaladıysa yakındaki objeleri tara)
        if (found == null && interactPoint != null)
        {
            Collider[] cols = Physics.OverlapSphere(interactPoint.position, interactRadius);
            float minDist = float.MaxValue;
            foreach (Collider col in cols)
            {
                IInteractable inter = col.GetComponent<IInteractable>();
                if (inter == null)
                    inter = col.GetComponentInParent<IInteractable>();
                if (inter != null)
                {
                    // Line of Sight kontrolü eklendi (arada duvar/kapak var mı?)
                    Vector3 start = interactPoint.position;
                    // Hedef objenin merkeze yakın bir noktasına ışın yolla
                    Vector3 end = col.bounds.center; 

                    // Kendi collider'ımıza veya hedefe doğrudan çarpmasını önemsemiyoruz, 
                    // sadece "arada" başka bir solid obje var mı ona bakıyoruz.
                    RaycastHit hit;
                    bool blocked = false;
                    
                    if (Physics.Linecast(start, end, out hit))
                    {
                        // Eğer çarptığımız şey hedefin kendisi veya parent/child'ı değilse
                        if (hit.collider != col && !hit.collider.transform.IsChildOf(col.transform))
                        {
                            blocked = true;
                        }
                    }

                    if (!blocked)
                    {
                        float d = Vector3.Distance(interactPoint.position, col.transform.position);
                        if (d < minDist)
                        {
                            minDist = d;
                            found = inter;
                        }
                    }
                }
            }
        }

        currentInteractable = found;

        // UI güncelle
        if (currentInteractable != null)
        {
            string txt = currentInteractable.GetInteractText();
            if (string.IsNullOrEmpty(txt))
            {
                // Etkileşim metni yoksa (örn. dolap kapalı) etkileşimi iptal et
                currentInteractable = null;
            }
            else
            {
                string msg = "[ E ]  " + txt;
                if (interactUIText != null)
                {
                    interactUIText.text = msg;
                    interactUIText.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            if (interactUIText != null)
            {
                interactUIText.text = "";
                interactUIText.gameObject.SetActive(false);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (interactPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(interactPoint.position, interactRadius);
        }
    }
}