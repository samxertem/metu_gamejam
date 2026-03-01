using UnityEngine;

namespace Project
{
    public class PoliceLights : MonoBehaviour
    {
        public Light redLight;
        public Light blueLight;
        public float flashSpeed = 0.15f;

        private float timer;
        private bool isRed = true;

        private void Start()
        {
            if (redLight != null) { redLight.color = Color.red; redLight.intensity = 5f; }
            if (blueLight != null) { blueLight.color = Color.blue; blueLight.intensity = 0f; }
        }

        private void Update()
        {
            if (redLight == null || blueLight == null) return;

            timer += Time.deltaTime;
            if (timer >= flashSpeed)
            {
                timer = 0f;
                isRed = !isRed;
                
                // Rapid blink effect
                redLight.intensity = isRed ? 2f : 0f;
                blueLight.intensity = isRed ? 0f : 2f;
            }
        }
    }
}