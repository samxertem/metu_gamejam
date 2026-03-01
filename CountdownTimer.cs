using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Project
{
    public class CountdownTimer : MonoBehaviour
    {
        public static CountdownTimer Instance;

        [Header("Settings")]
        public float durationSeconds = 90f; // 1.5 mins default
        public float restartDelay = 2.0f;
        public string restartSceneName = "Level_01";

        [Header("UI References")]
        public TMP_Text timerText;
        public GameObject failPanel;

        private bool isRunning = true;
        private bool hasFailed = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (failPanel != null) failPanel.SetActive(false);

            // Auto-find timer text if not assigned
            if (timerText == null)
            {
                var timerObj = transform.Find("TimerText");
                if (timerObj != null)
                    timerText = timerObj.GetComponent<TMP_Text>();
            }
        }

        private void Update()
        {
            if (!isRunning || hasFailed) return;

            durationSeconds -= Time.deltaTime;

            if (durationSeconds <= 0f)
            {
                durationSeconds = 0f;
                FailLevel();
            }

            UpdateTimerUI();
        }

        private void UpdateTimerUI()
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(durationSeconds / 60f);
                int seconds = Mathf.FloorToInt(durationSeconds % 60f);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }

        public void StopTimer()
        {
            isRunning = false;
        }

        private void FailLevel()
        {
            hasFailed = true;
            if (failPanel != null) failPanel.SetActive(true);
            Invoke(nameof(RestartScene), restartDelay);
        }

        private void RestartScene()
        {
            SceneManager.LoadScene(restartSceneName);
        }
    }
}