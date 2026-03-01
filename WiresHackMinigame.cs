using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Wires Hack minigame ana mantığı.
/// Timer (coroutine), doğru/yanlış sayaçları, kazanma/kaybetme durumları.
/// </summary>
public class WiresHackMinigame : MonoBehaviour
{
    [Header("Oyun Ayarlari")]
    public int totalWires = 4;
    public int wrongLimit = 3;
    public float timeLimitSeconds = 15f;
    public float cooldownSeconds = 3f;

    [Header("Referanslar")]
    public DraggableWire[] wires;
    public Text timerText;
    public UIFeedbackText feedback;
    public UIFlash flash;

    [Header("Durum (Okunur)")]
    public bool isRunning = false;
    public bool inCooldown = false;

    // Events
    public System.Action OnWin;
    public System.Action OnLose;
    public System.Action OnEscClose;

    private int connectedCount;
    private int wrongCount;
    private bool hasTriggeredEnd;
    private Coroutine timerCoroutine;
    private Coroutine cooldownCoroutine;

    // GC-free timer string cache
    private static readonly string[] timerStrings;

    static WiresHackMinigame()
    {
        // 0-99 arasi string cache (GC-free timer guncelleme)
        timerStrings = new string[100];
        for (int i = 0; i < 100; i++)
        {
            timerStrings[i] = i.ToString("D2");
        }
    }

    void Update()
    {
        // ESC ile popup kapat (win/lose tetiklenmeden)
        if (isRunning && Input.GetKeyDown(KeyCode.Escape))
        {
            StopGame();
            OnEscClose?.Invoke();
        }
    }

    /// <summary>
    /// Yeni oyun baslar. Wire'lari resetle, counter'lari sifirla, timer baslat.
    /// </summary>
    public void BeginGame()
    {
        // Reset state
        connectedCount = 0;
        wrongCount = 0;
        hasTriggeredEnd = false;
        isRunning = true;

        // Wire'lari resetle
        if (wires != null)
        {
            for (int i = 0; i < wires.Length; i++)
            {
                if (wires[i] != null)
                    wires[i].ResetWire();
            }
        }

        // Feedback temizle
        if (feedback != null)
            feedback.Clear();

        // Timer baslat
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(TimerCountdown());
    }

    /// <summary>
    /// Oyunu durdur (ESC veya dış çağrı).
    /// </summary>
    public void StopGame()
    {
        isRunning = false;

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    /// <summary>
    /// Dogru baglanti bildirimi.
    /// </summary>
    public void NotifyCorrect(DraggableWire wire)
    {
        if (!isRunning || hasTriggeredEnd) return;

        connectedCount++;

        if (feedback != null)
            feedback.ShowMessage("BAGLANTI BASARILI", Color.green, 1f);

        // Tum kablolar baglandi mi?
        if (connectedCount >= totalWires)
        {
            Win();
        }
    }

    /// <summary>
    /// Yanlis baglanti bildirimi.
    /// </summary>
    public void NotifyWrong()
    {
        if (!isRunning || hasTriggeredEnd) return;

        wrongCount++;

        // Kirmizi flash
        if (flash != null)
            flash.Flash(Color.red, 0.3f);

        if (feedback != null)
            feedback.ShowMessage("YANLIS BAGLANTI (" + wrongCount + "/" + wrongLimit + ")",
                Color.red, 1.5f);

        // Limit asildi mi?
        if (wrongCount >= wrongLimit)
        {
            Lose();
        }
    }

    void Win()
    {
        if (hasTriggeredEnd) return;
        hasTriggeredEnd = true;
        isRunning = false;

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        // Yesil flash
        if (flash != null)
            flash.Flash(Color.green, 0.5f);

        if (feedback != null)
            feedback.ShowMessage("ERISIM ONAYLANDI", Color.green, 0f); // Otomatik temizleme yok

        StartCoroutine(WinDelay());
    }

    IEnumerator WinDelay()
    {
        yield return new WaitForSeconds(0.8f);
        OnWin?.Invoke();
    }

    void Lose()
    {
        if (hasTriggeredEnd) return;
        hasTriggeredEnd = true;
        isRunning = false;

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        // Kirmizi flash
        if (flash != null)
            flash.Flash(Color.red, 0.5f);

        if (feedback != null)
            feedback.ShowMessage("ERISIM REDDEDILDI", Color.red, 0f);

        StartCoroutine(LoseDelay());
    }

    IEnumerator LoseDelay()
    {
        yield return new WaitForSeconds(1.0f);
        OnLose?.Invoke();

        // Cooldown baslat
        if (cooldownCoroutine != null)
            StopCoroutine(cooldownCoroutine);
        cooldownCoroutine = StartCoroutine(CooldownRoutine());
    }

    IEnumerator CooldownRoutine()
    {
        inCooldown = true;
        yield return new WaitForSeconds(cooldownSeconds);
        inCooldown = false;
        cooldownCoroutine = null;
    }

    IEnumerator TimerCountdown()
    {
        float remaining = timeLimitSeconds;

        while (remaining > 0f)
        {
            // Timer text guncelle (GC-free)
            UpdateTimerDisplay(remaining);

            yield return new WaitForSeconds(1f);
            remaining -= 1f;

            if (!isRunning || hasTriggeredEnd)
                yield break;
        }

        // Sure doldu
        UpdateTimerDisplay(0f);
        Lose();
    }

    void UpdateTimerDisplay(float seconds)
    {
        if (timerText == null) return;

        int totalSec = Mathf.Max(0, Mathf.CeilToInt(seconds));
        int min = totalSec / 60;
        int sec = totalSec % 60;

        // GC-free: cached string kullan
        if (min < 100 && sec < 100)
        {
            timerText.text = timerStrings[min] + ":" + timerStrings[sec];
        }
    }
}
