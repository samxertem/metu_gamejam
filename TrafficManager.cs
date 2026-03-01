using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class TrafficManager : MonoBehaviour
{
    public List<GameObject> trafficPrefabs;
    
    public int maxCars = 150;
    public float spawnInterval = 0.5f; // Restored for editor compatibility and performance
 
    private float spawnTimer = 0f;
    private int currentCarCount = 0;
    private List<GameObject> activeCars = new List<GameObject>();
    private int roadMask;
    private Transform playerTransform;
    
    public float spawnRadius = 300f; // 300m cloud radius
    public float despawnRadius = 600f; // Extra buffer to prevent flickering
    public float spawnDistanceInterval = 30f; // 30m spacing rule
    public int maxCloudCars = 20; // Reduced to 20 per user request
    public LayerMask obstacleMask; // For building avoidance (Default)

    void Start()
    {
        // Strictly filter to only Walkable (Roads) and explicitly exclude Sidewalks
        int walkableArea = NavMesh.GetAreaFromName("Walkable");
        if (walkableArea != -1) roadMask = 1 << walkableArea;
        else roadMask = NavMesh.AllAreas;

        if (trafficPrefabs == null || trafficPrefabs.Count == 0)
        {
            Debug.LogError("TrafficManager has no prefabs assigned.");
            enabled = false;
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
        
        obstacleMask = 1 << LayerMask.NameToLayer("Default"); // Building avoidance
    }

    void Update()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
            else return;
        }

        ManageCarCloud();
        
        activeCars.RemoveAll(car => car == null);
        currentCarCount = activeCars.Count;

        if (currentCarCount < maxCloudCars)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                TrySpawnInCloud();
                spawnTimer = 0f;
            }
        }
    }

    void ManageCarCloud()
    {
        for (int i = activeCars.Count - 1; i >= 0; i--)
        {
            GameObject car = activeCars[i];
            if (car == null) continue;

            float dist = Vector3.Distance(car.transform.position, playerTransform.position);
            // Despawn if it exceeds 2 times the spawn radius
            if (dist > (spawnRadius * 2f))
            {
                Destroy(car);
                activeCars.RemoveAt(i);
            }
        }
    }

    void TrySpawnInCloud()
    {
        int trafficLayer = LayerMask.NameToLayer("TrafficCar");
        
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 randomPos = playerTransform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
 
        // Ensure it spawns in the half-circle (semi-circle) in front of the player
        Vector3 dirToSpawn = (randomPos - playerTransform.position).normalized;
        if (Vector3.Dot(playerTransform.forward, dirToSpawn) < 0f) return;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPos, out hit, 10f, roadMask))
        {
            Vector3 spawnPos = hit.position;
 
            // 4m exact clearance from edge -> check box of size footprint + 4m on all sides
            // Car is approx 2m wide, 4m long. So Box half-extents = (1+4, 1+4, 2+4) = (5f, 5f, 6f)
            if (Physics.CheckBox(spawnPos + Vector3.up * 1f, new Vector3(5f, 2f, 6f), Quaternion.identity, obstacleMask | (1 << trafficLayer))) return;
 
            // Strict precise ground level check (no ramps, no bridges, no floating)
            if (Mathf.Abs(spawnPos.y - playerTransform.position.y) > 0.8f) return;
 
            SpawnCar(spawnPos);
        }
    }
 
    void SpawnCar(Vector3 spawnPos)
    {
        GameObject selectedPrefab = trafficPrefabs[Random.Range(0, trafficPrefabs.Count)];
        GameObject newCar = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
        newCar.name = $"TR_TrafficCloud_{activeCars.Count}";
        
        int trafficLayer = LayerMask.NameToLayer("TrafficCar");
        PrepareCarVisually(newCar, trafficLayer);
 
        // Force a standard BoxCollider for hard physical impacts. We DO NOT want a trigger, or cars pass through each other.
        BoxCollider bc = newCar.GetComponent<BoxCollider>();
        if (bc == null) bc = newCar.AddComponent<BoxCollider>();
        bc.size = new Vector3(2f, 1.5f, 4.2f);
        bc.center = new Vector3(0, 0.75f, 0);
        bc.isTrigger = false; // MUST BE FALSE to bounce off objects

        // Enable Gravity and Physical Collisions, but Freeze Tipping
        Rigidbody rb = newCar.GetComponent<Rigidbody>();
        if (rb == null) rb = newCar.AddComponent<Rigidbody>();
        rb.isKinematic = false; // MUST BE FALSE for gravity to apply
        rb.useGravity = true;
        rb.mass = 1500f; // Give it car weight
        rb.drag = 1f; // Add dampening so physics explosions are mitigated
        rb.angularDrag = 2f;
        // Freeze X and Z rotation so the car cannot flip entirely
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
 
        NavMeshAgent agent = newCar.GetComponent<NavMeshAgent>();
        if (agent == null) agent = newCar.AddComponent<NavMeshAgent>();
        
        agent.enabled = false;
        agent.speed = Random.Range(15f, 25f); // Increased speed
        agent.angularSpeed = 150f;
        agent.acceleration = 16f; // Increased acceleration
        agent.radius = 2.0f; // high radius so they avoid each other generously
        agent.height = 1.5f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.autoBraking = false;
        agent.areaMask = roadMask;
        agent.transform.position = spawnPos;
        agent.Warp(spawnPos);
        agent.enabled = true;
 
        TrafficVehicleController controller = newCar.GetComponent<TrafficVehicleController>();
        if (controller == null) controller = newCar.AddComponent<TrafficVehicleController>();
        controller.speed = agent.speed;
 
        activeCars.Add(newCar);
    }
 
    void PrepareCarVisually(GameObject obj, int newLayer)
    {
        if (null == obj) return;

        // Recursively activate everything and assign layers, this fixes missing wheels/parts
        Transform[] allDescendants = obj.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in allDescendants)
        {
            t.gameObject.layer = newLayer;
            t.gameObject.SetActive(true);
            
            Renderer r = t.GetComponent<Renderer>();
            if (r != null) r.enabled = true;

            // FIX: Unity 5+ does not allow non-convex MeshColliders on non-kinematic Rigidbodies.
            // We set it to convex immediately to suppress the console spam, then destroy it 
            // completely since we manually add a unified BoxCollider to the root.
            MeshCollider mc = t.GetComponent<MeshCollider>();
            if (mc != null)
            {
                mc.convex = true;
                Destroy(mc);
            }
            
            // Also destroy any other sub-colliders so they don't interfere with our root box
            if (t.gameObject != obj)
            {
                Collider col = t.GetComponent<Collider>();
                if (col != null) Destroy(col);
            }
        }
    }

    // Keeping the original helper just in case, though unused now
    Vector3 GetRandomNavMeshPosition(int areaMask)
    {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        if (triangulation.vertices.Length == 0) {
            return Vector3.zero; 
        }

        for (int i = 0; i < 50; i++)
        {
            int randomIndex = Random.Range(0, triangulation.vertices.Length);
            Vector3 randomVertex = triangulation.vertices[randomIndex];
            randomVertex += new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomVertex, out hit, 15f, areaMask))
            {
                if (hit.position.y > 1.2f || hit.position.y < -0.5f) continue;
                if (hit.mask != 0 && (hit.mask & areaMask) != 0) return hit.position;
            }
        }
        return Vector3.zero;
    }
}
