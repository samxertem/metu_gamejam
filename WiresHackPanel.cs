using UnityEngine;

/// <summary>
/// Sahnede kapinin yanina yerlestirilen hack paneli.
/// IInteractable implement eder — oyuncu E basinca popup acar.
/// Kazaninca kapıyı açar, kaybedince cooldown uygular.
/// </summary>
public class WiresHackPanel : MonoBehaviour, IInteractable
{
    [Header("Referanslar")]
    public SlidingSecurityDoor targetDoor;
    public GameObject popupRoot;
    public WiresHackMinigame minigame;

    [Header("Durum")]
    public bool unlocked = false;

    private PlayerMovement playerMovement;
    private PlayerCamera playerCamera;
    private PlayerInteract playerInteract;

    void Start()
    {
        // Player scriptlerini bul
        playerMovement = FindObjectOfType<PlayerMovement>();
        playerCamera = FindObjectOfType<PlayerCamera>();
        playerInteract = FindObjectOfType<PlayerInteract>();

        // Event'lere abone ol
        if (minigame != null)
        {
            minigame.OnWin += HandleWin;
            minigame.OnLose += HandleLose;
            minigame.OnEscClose += HandleEscClose;
        }

        // Popup baslangiçta kapali
        if (popupRoot != null)
            popupRoot.SetActive(false);
    }

    void OnDestroy()
    {
        if (minigame != null)
        {
            minigame.OnWin -= HandleWin;
            minigame.OnLose -= HandleLose;
            minigame.OnEscClose -= HandleEscClose;
        }
    }

    public void Interact()
    {
        if (unlocked) return;
        if (minigame != null && minigame.inCooldown) return;

        OpenPopup();
    }

    public string GetInteractText()
    {
        if (unlocked) return "Kapi Acik";
        if (minigame != null && minigame.inCooldown) return "Bekleniyor...";
        return "Hack Paneli (E)";
    }

    void OpenPopup()
    {
        // Player'i kilitle
        LockPlayer();

        // Popup ac
        if (popupRoot != null)
            popupRoot.SetActive(true);

        // Minigame baslat
        if (minigame != null)
            minigame.BeginGame();
    }

    void ClosePopup()
    {
        // Popup kapat
        if (popupRoot != null)
            popupRoot.SetActive(false);

        // Player'i serbest birak
        UnlockPlayer();
    }

    void HandleWin()
    {
        unlocked = true;

        ClosePopup();

        // Kapiyi ac
        if (targetDoor != null)
        {
            targetDoor.isLocked = false;
            targetDoor.Interact();
        }

        // GameManager state
        if (GameManager.Instance != null)
            GameManager.Instance.puzzleSolved = true;

        Debug.Log("Wires Hack basarili! Kapi acildi.");
    }

    void HandleLose()
    {
        ClosePopup();
        Debug.Log("Wires Hack basarisiz! Cooldown basladi.");
    }

    void HandleEscClose()
    {
        ClosePopup();
    }

    void LockPlayer()
    {
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerCamera != null)
            playerCamera.enabled = false;

        if (playerInteract != null)
            playerInteract.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void UnlockPlayer()
    {
        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerCamera != null)
            playerCamera.enabled = true;

        if (playerInteract != null)
            playerInteract.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
