using UnityEngine;

namespace Project
{
    public class CheckpointSystem : MonoBehaviour
    {
        public static CheckpointSystem Instance;

        [Header("Checkpoint Transforms (in order 1→7)")]
        public Transform[] checkpoints;

        [Header("References")]
        public NavigationArrow navigationArrow;
        public MinimapIcons minimapIcons;

        private int currentIndex = 0;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Deactivate all checkpoints except the first
            for (int i = 0; i < checkpoints.Length; i++)
            {
                if (checkpoints[i] != null)
                    checkpoints[i].gameObject.SetActive(i == 0);
            }

            UpdateTargets();
        }

        /// <summary>
        /// Called when a checkpoint is reached. Disables current, enables next.
        /// </summary>
        public void OnCheckpointReached(Transform reachedCheckpoint)
        {
            if (currentIndex >= checkpoints.Length) return;
            if (checkpoints[currentIndex] != reachedCheckpoint) return;

            Debug.Log($"<color=green>Checkpoint {currentIndex + 1} tamamlandı!</color>");

            // Deactivate current checkpoint (hides 3D marker + collider)
            checkpoints[currentIndex].gameObject.SetActive(false);

            currentIndex++;

            if (currentIndex < checkpoints.Length)
            {
                // Activate next checkpoint
                checkpoints[currentIndex].gameObject.SetActive(true);
                UpdateTargets();
            }
            else
            {
                // All checkpoints reached!
                Debug.Log("<color=yellow>Tüm checkpoint'ler tamamlandı! Başardın!</color>");

                // Stop the timer
                if (CountdownTimer.Instance != null)
                    CountdownTimer.Instance.StopTimer();

                // Clear navigation targets
                if (navigationArrow != null)
                    navigationArrow.target = null;
                if (minimapIcons != null)
                    minimapIcons.pointB = null;

                // End of checkpoints reached. Cinematic triggers automatically upon distance.
            }
        }

        private void UpdateTargets()
        {
            if (currentIndex >= checkpoints.Length) return;

            Transform current = checkpoints[currentIndex];

            // Update navigation arrow target
            if (navigationArrow != null)
                navigationArrow.target = current;

            // Update minimap B point
            if (minimapIcons != null)
                minimapIcons.pointB = current;
        }

        public Transform GetCurrentCheckpoint()
        {
            if (currentIndex < checkpoints.Length)
                return checkpoints[currentIndex];
            return null;
        }

        public int GetCurrentIndex() => currentIndex;
        public int GetTotalCheckpoints() => checkpoints.Length;
    }
}
