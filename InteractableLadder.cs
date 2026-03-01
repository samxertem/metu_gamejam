using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class InteractableLadder : MonoBehaviour, IInteractable
{
    [Header("Transition")]
    public AudioClip transitionVoice; // Vo_1_kurtuldum

    private bool isInteracting = false;

    public string GetInteractText()
    {
        return "Kaçış Noktası";
    }

    public void Interact()
    {
        if (isInteracting) return;
        StartCoroutine(ClimbRoutine());
    }

    private IEnumerator ClimbRoutine()
    {
        isInteracting = true;

        // Oyuncu hareketini durdur (Level 04'te PlayerMovement kullanılıyor)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null) pm.enabled = false;
        }

        // 1. Fade Out (Ekranı karart)
        if (ScreenFader.Instance != null)
        {
            yield return ScreenFader.Instance.FadeOut(2f); // 2 saniyede kararır
        }
        else
        {
            // Eğer ScreenFader yoksa (test ederken vb.) manuel bekle
            yield return new WaitForSeconds(2f);
        }

        // 2. "Part 4: Firar" yazısını göster ve Vo_1_kurtuldum sesini çal
        SceneTransitionHelper.ShowTransition("Part 4: Firar", transitionVoice);

        // 3. 18 saniye siyah ekran bekle
        Debug.Log("[InteractableLadder] 18 saniyelik siyah ekran başladı...");
        yield return new WaitForSeconds(18f);

        // 4. Level_04 sahnesini yükle
        Debug.Log("[InteractableLadder] Level_04 yükleniyor...");
        SceneManager.LoadScene("Level_04");
    }
}

