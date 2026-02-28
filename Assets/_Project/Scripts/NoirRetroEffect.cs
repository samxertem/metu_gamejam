using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class NoirRetroEffect : MonoBehaviour
{
    public Shader noirShader;
    private Material noirMaterial;

    [Header("Noir Settings")]
    [Range(0f, 3f)] public float contrast = 1.5f;
    [Range(0f, 2f)] public float brightness = 1.0f;
    [Range(0f, 3f)] public float vignetteIntensity = 1.3f;
    [Range(0f, 1f)] public float noiseIntensity = 0.15f;

    void OnEnable()
    {
        if (noirShader == null)
        {
            noirShader = Shader.Find("Hidden/NoirRetro");
        }

        if (noirMaterial == null && noirShader != null)
        {
            noirMaterial = new Material(noirShader);
            noirMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (noirShader == null)
        {
            noirShader = Shader.Find("Hidden/NoirRetro");
        }
        
        if (noirMaterial == null && noirShader != null)
        {
            noirMaterial = new Material(noirShader);
            noirMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        if (noirMaterial != null)
        {
            noirMaterial.SetFloat("_Contrast", contrast);
            noirMaterial.SetFloat("_Brightness", brightness);
            noirMaterial.SetFloat("_VignetteIntensity", vignetteIntensity);
            noirMaterial.SetFloat("_NoiseIntensity", noiseIntensity);
            noirMaterial.SetFloat("_TimeX", Time.time);

            Graphics.Blit(source, destination, noirMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    void OnDisable()
    {
        if (noirMaterial != null)
        {
            if (Application.isPlaying) Destroy(noirMaterial);
            else DestroyImmediate(noirMaterial);
        }
    }
}
