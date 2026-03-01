using UnityEngine;

namespace Project {
    public class SpawnSnapper : MonoBehaviour
    {
        [Header("Spawn Snap Settings")]
        public float clearance = 1.0f;
        public string groundLayer = "Default";

        private void Awake()
        {
            // Raycast against EVERYTHING (ignore layer filtering) to guarantee we find ground
            // The city collider is on Default layer, not World
            int layerMask = ~0; // All layers
            
            Rigidbody rb = GetComponent<Rigidbody>();

            for (int i = 0; i < 5; i++)
            {
                Vector3 origin = transform.position + Vector3.up * 50f;
                
                if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 200f, layerMask, QueryTriggerInteraction.Ignore))
                {
                    // Skip if we hit ourselves
                    if (hit.collider.gameObject == gameObject || hit.transform.IsChildOf(transform))
                        continue;

                    Vector3 pos = transform.position;
                    pos.y = hit.point.y + clearance;
                    transform.position = pos;

                    if (rb != null)
                    {
                        rb.velocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }

                    Debug.Log($"SpawnSnapper: Snapped to Y={pos.y:F2} (ground={hit.point.y:F2}, hit={hit.collider.gameObject.name})");
                    return;
                }
            }

            // Fallback: If no physics colliders exist on the ground, try snapping to the NavMesh
            if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out UnityEngine.AI.NavMeshHit navHit, 50f, UnityEngine.AI.NavMesh.AllAreas))
            {
                Vector3 pos = transform.position;
                pos.y = navHit.position.y + clearance;
                transform.position = pos;

                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                Debug.Log($"SpawnSnapper: Fallback snapped to NavMesh Y={pos.y:F2} (ground={navHit.position.y:F2})");
                return;
            }

            Debug.LogWarning($"SpawnSnapper: No ground found under {gameObject.name} (checked Physics and NavMesh)!");
        }
    }
}
