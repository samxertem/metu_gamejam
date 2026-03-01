using System.Collections.Generic;
using UnityEngine;

namespace Project {
    [RequireComponent(typeof(Rigidbody))]
    public class TrafficCarAI : MonoBehaviour
    {
        public List<Transform> waypoints = new List<Transform>();
        public float speed = 10f;
        public float waypointTolerance = 1f;

        private int currentWaypointIndex = 0;
        private Rigidbody rb;

        public float laneX = 2.5f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (waypoints == null || waypoints.Count == 0) return;

            Transform target = waypoints[currentWaypointIndex];

            // Distance check
            float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), 
                                              new Vector3(target.position.x, 0, target.position.z));
                                              
            if (distance < waypointTolerance)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Count)
                {
                    currentWaypointIndex = 0; // Loop back
                    // Teleport to the first waypoint directly to avoid turning around
                    rb.MovePosition(new Vector3(laneX, rb.position.y, waypoints[0].position.z));
                    return;
                }
                target = waypoints[currentWaypointIndex];
            }

            // Move towards waypoint
            Vector3 direction = (target.position - transform.position).normalized;
            // Ignore vertical movement mostly
            direction.y = 0;
            
            // Apply speed for Kinematic Rigidbody
            Vector3 moveAmount = direction * speed * Time.fixedDeltaTime;
            
            // Enforce lane clamp
            Vector3 nextPos = rb.position + moveAmount;
            nextPos.x = laneX;
            rb.MovePosition(nextPos);

            // Rotate strictly forward to avoid looking at an offset Waypoint
            rb.MoveRotation(Quaternion.LookRotation(Vector3.forward));
        }
    }
}