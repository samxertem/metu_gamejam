using UnityEngine;

namespace Project
{
    public class NavigationArrow : MonoBehaviour
    {
        public Transform target;
        public Transform playerLocation;
        public float hoverHeight = 3.5f;

        private void Update()
        {
            if (target == null || playerLocation == null) return;
            
            // Position the arrow above and in front of the car
            transform.position = playerLocation.position 
                + playerLocation.forward * 5f 
                + Vector3.up * hoverHeight;
            
            // Look at target, but flatten Y so the arrow stays horizontal
            Vector3 directionToTarget = target.position - transform.position;
            directionToTarget.y = 0f; // Keep arrow horizontal
            
            if (directionToTarget.sqrMagnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
            }
        }
    }
}