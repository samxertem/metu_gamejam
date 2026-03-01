using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elemanları")]
    public Button playButton;
    public GameObject blackScreenPanel;
    public TextMeshProUGUI introText;

    [Header("Ses")]
    public AudioClip introVoice;

    [Header("Ayarlar")]
    public string firstLevelName = "Level_00";
    public float introDuration = 14f;

    private void Start()
    {
        // Başlangıçta siyah ekran gizli olmalı
        if (blackScreenPanel != null)
        {
            blackScreenPanel.SetActive(false);
        }

        // Butona tıklama olayını bağla
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
    }

    private void OnPlayButtonClicked()
    {
        // Butona tekrar tıklanmasını engelle
        playButton.interactable = false;

        // Intro sekansını başlat
        StartCoroutine(PlayIntroSequence());
    }

    private void Update()
    {
        // DEBUG SHORTCUT: P tuşuna basınca direkt Loop 3 Mahkeme sahnesine atla
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("[DEBUG] MainMenu: P tuşuna basıldı! Loop 3 Mahkeme Sahnesine geçiliyor...");
            
            // GameManager varsa loop'u 3 yap
            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentLoop = 3;
                GameManager.Instance.previousBadChoices.Clear();
            }
            else
            {
                // Yoksa geçici bir obje ile taşı
                GameObject tempManager = new GameObject("TempLoopManager");
                GameManager gm = tempManager.AddComponent<GameManager>();
                gm.currentLoop = 3;
            }

            SceneManager.LoadScene("Level_02");
        }
    }

    private IEnumerator PlayIntroSequence()
    {
        // Siyah ekranı ve yazıyı aktif et
        if (blackScreenPanel != null)
        {
            blackScreenPanel.SetActive(true);
        }
        
        if (introText != null)
        {
            introText.text = ""; // Sadece siyah ekran kalması için yazıyı sildik
            introText.gameObject.SetActive(true);
        }

        // DışSes1 sesini sahne geçişinde kesilmeyecek şekilde çal
        if (introVoice != null)
        {
            GameObject audioObj = new GameObject("IntroVoice_Persistent");
            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.clip = introVoice;
            source.Play();
            DontDestroyOnLoad(audioObj);

            // Ses bitince kendini yok etsin
            Destroy(audioObj, introVoice.length);
        }

        // 14 saniye bekle
        yield return new WaitForSeconds(introDuration);

        // Scene 0 (Level_00) yükle
        SceneManager.LoadScene(firstLevelName);
    }
}

