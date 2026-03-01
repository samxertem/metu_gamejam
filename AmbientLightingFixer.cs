using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Editördeyken (oyun açık değilken bile) çalışmasını sağlamak için [ExecuteInEditMode] ekliyoruz.
[ExecuteInEditMode]
public class AmbientLightingFixer : MonoBehaviour
{
    private void OnEnable()
    {
        ApplyLightingSetup();
    }

    public void ApplyLightingSetup()
    {
        // 1. Scene içindeki Directional Light'ı bul ve karanlık bir hapishane hissi için sil
        Light[] allLights = FindObjectsOfType<Light>();
        foreach (Light l in allLights)
        {
            if (l.type == LightType.Directional)
            {
                // Silmek veya devredışı bırakmak
                l.gameObject.SetActive(false);
            }
        }

        // 2. Ambient Işıklandırmayı simsiyah mora çalan renge (#03030a) çek
        Color darkAmbient;
        if (ColorUtility.TryParseHtmlString("#03030a", out darkAmbient))
        {
            RenderSettings.ambientLight = darkAmbient;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientIntensity = 0f;
        }

        Debug.Log("Ambient Lighting Confirmed to Dark mode (#03030a).");
    }
}
