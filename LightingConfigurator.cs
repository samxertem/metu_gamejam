using UnityEngine;

public class LightingConfigurator : MonoBehaviour
{
    private void Start()
    {
        // Ambient Lighting'i Color moduna alıp belirtilen koyu renge setliyoruz.
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        
        // #05050a renginin RGB float karşılığı
        Color ambientColor = new Color(5f / 255f, 5f / 255f, 10f / 255f);
        RenderSettings.ambientLight = ambientColor;
        
        // Gökyüzü kutusunu kapatıp karartmak için siyah yapıyoruz (İsteğe bağlı)
        if(RenderSettings.skybox != null)
        {
            RenderSettings.skybox.SetColor("_Tint", Color.black);
        }
    }
}