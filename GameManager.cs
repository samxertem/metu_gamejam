using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton pattern
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public int comfortScore = 0;
    public bool hasKey = false;
    public bool hasFlashlight = false;
    public bool puzzleSolved = false;

    [Header("Global State - Level 02 Specific")]
    public int currentLoop = 1;
    public int totalComfortScore = 0;
    
    // Hangi soruda hangi seçeneği seçmiştik (kötü olanlar soluk kalsın diye dizgede tutuyoruz)
    public List<string> previousBadChoices = new List<string>();

    [Header("Results from Level 02")]
    public int verdictResult = -1; // 0=Beraat, 1=A blok, 2=B blok, 3=C blok
    public bool isGameWon = false;
    public int finalScore = 0;

    private void Awake()
    {
        // DontDestroyOnLoad Singleton implementasyonu
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Birden fazla GameManager kopyası bulundu, yok ediliyor.");
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // DEBUG SHORTCUT: P tuşuna basınca direkt Loop 3 Mahkeme sahnesine atla
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("[DEBUG] P tuşuna basıldı! Loop 3 Mahkeme Sahnesine geçiliyor...");
            currentLoop = 3;
            previousBadChoices.Clear(); // Soruları sıfırla ki 3. döngü mantığı tam çalışsın
            UnityEngine.SceneManagement.SceneManager.LoadScene("Level_02");
        }
    }
}