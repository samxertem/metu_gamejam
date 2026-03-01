using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class TrafficVehicleController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public float wanderRadius = 150f;

    [Header("Player Avoidance")]
    public float detectionDistance = 15f;
    public float slowSpeed = 4f;

    private float destinationCooldown = 0f;
    private Rigidbody rb;

    private NavMeshAgent agent;
    private Transform playerTarget;
    private int roadMask;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        
        // We are using gravity and physics now. Ensure Agent doesn't fight the Rigidbody violently.
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // The surface was baked to Walkable, so we use AllAreas
        roadMask = NavMesh.AllAreas;

        agent.speed = speed;
        agent.acceleration = 16f;
        agent.angularSpeed = 200f; // Snappier turns
        agent.stoppingDistance = 0f;
        agent.autoBraking = false;
        
        // Crucial for Physics compatibility: Agent drives the logic, Rigidbody drives the physical body
        // updatePosition = false means the Agent won't instantly teleport the model, allowing RB gravity to win.
        agent.updatePosition = false; 
        agent.updateRotation = true;
        
        // Ensure they only drive on the road NavMesh
        agent.areaMask = roadMask;

        // Try to find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("PlayerCar");
        if (player != null) playerTarget = player.transform;

        SetNewRandomDestination();
    }

    void Update()
    {
        // Sync the NavMeshAgent's logical coordinate with our rigid physical body coordinate.
        // This allows Unity gravity to push the car down while the Agent pulls it forward.
        if (agent.isOnNavMesh)
        {
            agent.nextPosition = transform.position;
        }

        destinationCooldown -= Time.deltaTime;

        if (!agent.pathPending && agent.remainingDistance < 6f && destinationCooldown <= 0f)
        {
            SetNewRandomDestination();
        }

        CheckPlayerDistance();
    }
    
    void FixedUpdate()
    {
        // Actually move the Rigidbody based on the NavMesh desired velocity so collisions work.
        if (agent.isOnNavMesh && rb != null && !rb.isKinematic)
        {
            // Only apply X and Z lateral movement. Let Gravity handle Y.
            Vector3 desiredVelocity = agent.desiredVelocity;
            desiredVelocity.y = rb.velocity.y; // Keep gravity's downward acceleration
            
            // Set the physical velocity
            rb.velocity = desiredVelocity;

            // Sync Rotation: the physical box collider should face the direction the agent is turning.
            if (desiredVelocity.sqrMagnitude > 0.1f)
            {
                Quaternion lookRot = Quaternion.LookRotation(new Vector3(desiredVelocity.x, 0, desiredVelocity.z));
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, lookRot, Time.fixedDeltaTime * agent.angularSpeed));
            }
        }
    }

    void SetNewRandomDestination()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        destinationCooldown = 1.0f; // Force a 1 second delay minimum before searching again to save FPS

        // Performance fix: Drop iterations from 30 down to 5.
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection.y = 0; // Flatten the search
            
            // Allow a wider angle (0.1f instead of 0.4f) to find points faster, saving CPU
            if (Vector3.Dot(transform.forward, randomDirection.normalized) > 0.1f)
            {
                randomDirection += transform.position;
                
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, 20f, roadMask)) // Smaller sample radius for speed
                {
                    agent.SetDestination(hit.position);
                    return;
                }
            }
        }
    }

    void CheckPlayerDistance()
    {
        if (playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        
        // Check if player is somewhat in front
        Vector3 dirToPlayer = (playerTarget.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, dirToPlayer);

        if (distanceToPlayer < detectionDistance && dot > 0.5f)
        {
            agent.speed = Mathf.Lerp(agent.speed, slowSpeed, Time.deltaTime * 5f);
        }
        else
        {
            agent.speed = Mathf.Lerp(agent.speed, speed, Time.deltaTime * 2f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If we hit something physical (another car, player, building), temporarily pause the agent
        // so it doesn't try to hyper-accelerate through a solid wall
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.velocity = Vector3.zero;
            
            // Re-roll destination if we are stuck on a building or another car
            if (collision.gameObject.layer != LayerMask.NameToLayer("World_Road"))
            {
                destinationCooldown = 0f; // Force an immediate re-roll next frame
            }
        }
    }
}
