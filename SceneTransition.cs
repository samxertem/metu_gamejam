using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project {
    public class SceneTransition : MonoBehaviour
    {
        public static SceneTransition Instance { get; private set; }

        public Image fadeImage;
        public float fadeDuration = 1f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            if (fadeImage != null)
            {
                // Ekran başladığında siyah
                Color c = fadeImage.color;
                c.a = 1f;
                fadeImage.color = c;
                StartCoroutine(FadeInRoutine());
            }
        }

        public void LoadSceneWithFade(string sceneName)
        {
            if (fadeImage != null)
            {
                StartCoroutine(FadeOutRoutine(sceneName));
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        private IEnumerator FadeInRoutine()
        {
            float timer = 0f;
            Color c = fadeImage.color;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                c.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                fadeImage.color = c;
                yield return null;
            }
            c.a = 0f;
            fadeImage.color = c;
        }

        private IEnumerator FadeOutRoutine(string sceneName)
        {
            float timer = 0f;
            Color c = fadeImage.color;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                c.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                fadeImage.color = c;
                yield return null;
            }
            c.a = 1f;
            fadeImage.color = c;

            SceneManager.LoadScene(sceneName);
        }
    }
}