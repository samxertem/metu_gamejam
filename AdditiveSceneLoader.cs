using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to any GameObject in your main scene.
/// It will additively load the specified scene on Awake.
/// </summary>
public class AdditiveSceneLoader : MonoBehaviour
{
    [Tooltip("Name of the scene to load additively (must be in Build Settings)")]
    public string sceneToLoad = "demo_city_night";

    private void Awake()
    {
        // Don't load if it's already loaded
        Scene targetScene = SceneManager.GetSceneByName(sceneToLoad);
        if (targetScene.isLoaded)
        {
            Debug.Log($"[AdditiveSceneLoader] '{sceneToLoad}' is already loaded, skipping.");
            return;
        }

        Debug.Log($"[AdditiveSceneLoader] Loading '{sceneToLoad}' additively...");
        SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
    }
}
