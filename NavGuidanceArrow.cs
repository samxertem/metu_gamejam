using UnityEngine;
using UnityEngine.AI;

namespace Project
{
    public class NavGuidanceArrow : MonoBehaviour
    {
        public Transform targetB;
        public Transform playerLocation;
        
        public float repathInterval = 0.25f;
        public float arrowHeight = 2.0f;
        public float forwardOffset = 4.5f;

        private NavMeshPath path;
        private float pathTimer;

        private void Start()
        {
            path = new NavMeshPath();
        }

        private void Update()
        {
            if (targetB == null || playerLocation == null) return;

            // Re-center arrow in front of the car
            transform.position = playerLocation.position + playerLocation.forward * forwardOffset + playerLocation.up * arrowHeight;

            // Timer for NavMesh pathfinding
            pathTimer += Time.deltaTime;
            if (pathTimer >= repathInterval)
            {
                pathTimer = 0f;
                // Calculate path from player's current location to target B
                NavMesh.CalculatePath(playerLocation.position, targetB.position, NavMesh.AllAreas, path);
            }

            // Aim the arrow at the first corner of the path
            Vector3 lookDirection = Vector3.zero;

            if (path.corners.Length >= 2)
            {
                // corners[0] is roughly player pos, corners[1] is the next node
                Vector3 nextCorner = path.corners[1];
                lookDirection = nextCorner - transform.position;
                lookDirection.y = 0; // Keep horizontal
            }
            else
            {
                // Fallback to direct look if no path exists or target is extremely close
                lookDirection = targetB.position - transform.position;
                lookDirection.y = 0;
            }

            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDirection);
                // Smooth interpolation to avoid snapping
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }
        }
    }
}