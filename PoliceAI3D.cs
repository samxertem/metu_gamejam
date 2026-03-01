using UnityEngine;
using UnityEngine.AI;

namespace Project {
    [RequireComponent(typeof(NavMeshAgent))]
    public class PoliceAI3D : MonoBehaviour
    {
        private NavMeshAgent agent;
        private Transform player;

        public float baseSpeed = 14f;
        public float aggressiveSpeed = 20f;
        public float rammingDistance = 6f;
        public float followDistance = 7f;
        public float lateralOffset = 1.5f;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("PoliceAI3D: Player not found! Ensure PlayerCar has the 'Player' tag.");
            }
        }

        private void Update()
        {
            if (player == null || agent == null || !agent.isOnNavMesh) return;

            // Follow a point behind and slightly to the side of the player
            Vector3 targetPos = player.position - player.forward * followDistance + player.right * lateralOffset;
            agent.SetDestination(targetPos);

            float distance = Vector3.Distance(transform.position, player.position);
            Vector3 policeToPlayer = transform.position - player.position;
            
            // If police is in front of the player (dot > 0), slow down so player can pass
            if (Vector3.Dot(player.forward, policeToPlayer) > 0)
            {
                agent.speed = baseSpeed * 0.3f;
            }
            else if (distance < rammingDistance)
            {
                // Slow down strictly to prevent rear-ending
                agent.speed = baseSpeed * 0.5f;
            }
            else
            {
                // Speed up to catch up to the offset position
                agent.speed = aggressiveSpeed;
            }
        }
    }
}