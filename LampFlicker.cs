using UnityEngine;
using System.Collections;

public class LampFlicker : MonoBehaviour
{
    private Light pLight;
    private Material emissionMat;
    private float defaultIntensity;
    private float defaultEmissionInt;
    
    // Rastgele yanıp sönme bekleme süresi
    private float minWaitTime = 8f;
    private float maxWaitTime = 20f;

    private void Start()
    {
        pLight = GetComponent<Light>();
        
        // Sphere'in materyalini bulabilmek için ebeveynde veya kendi üstündeki Renderer aranır
        Renderer r = GetComponent<Renderer>();
        if (r == null && transform.parent != null)
        {
            r = transform.parent.GetComponent<Renderer>();
        }

        if (r != null)
        {
            emissionMat = r.material; // Instance alınıyor
        }

        if (pLight != null)
        {
            defaultIntensity = pLight.intensity;
        }

        defaultEmissionInt = 2f; // Default 2 olarak belirlenmişti.

        StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // Bekleme Evresi
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            // Flicker (dalgalanma) Evresi
            int flickerCount = Random.Range(2, 5); // 2-4 kere gidip gelme

            for (int i = 0; i < flickerCount; i++)
            {
                // Işığı aniden düşür
                SetIntensityMultiplier(Random.Range(0.1f, 0.4f));
                yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));

                // Tekrar normale al
                SetIntensityMultiplier(1f);
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            }
        }
    }

    private void SetIntensityMultiplier(float multiplier)
    {
        if (pLight != null)
            pLight.intensity = defaultIntensity * multiplier;

        if (emissionMat != null)
        {
            Color baseEmission = new Color(1f, 0.8f, 0.26f); // #ffcc44
            emissionMat.SetColor("_EmissionColor", baseEmission * defaultEmissionInt * multiplier);
        }
    }
}