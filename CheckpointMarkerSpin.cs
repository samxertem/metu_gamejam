using UnityEngine;

namespace Project
{
    /// <summary>
    /// Spins the checkpoint arrow marker to make it visually stand out.
    /// </summary>
    public class CheckpointMarkerSpin : MonoBehaviour
    {
        public float spinSpeed = 90f; // degrees per second
        public float bobSpeed = 1.5f;
        public float bobHeight = 0.5f;

        private Transform marker;
        private Vector3 baseLocalPos;

        private void Start()
        {
            // Find the ArrowMarker child
            marker = transform.Find("ArrowMarker");
            if (marker != null)
                baseLocalPos = marker.localPosition;
        }

        private void Update()
        {
            if (marker == null) return;

            // Spin around Y axis
            marker.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);

            // Bob up and down
            float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            marker.localPosition = baseLocalPos + Vector3.up * yOffset;
        }
    }
}
