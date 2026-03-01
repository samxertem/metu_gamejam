using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CityTrafficSystem : MonoBehaviour
{
    [Header("Cars")]
    public GameObject[] carPrefabs;
    public int carCount = 10;
    public float carSpeed = 8f;

    [Header("Pedestrians")]
    public GameObject[] pedestrianPrefabs;
    public int pedestrianCount = 15;
    public float pedSpeed = 1.5f;

    [Header("Animations")]
    public AnimationClip pedWalkAnim;
    public AnimationClip pedIdleAnim;

    private int roadLayerMask;
    private int sidewalkLayerMask;

#if UNITY_EDITOR
    [ContextMenu("Setup Default Assets")]
    public void SetupDefaultAssets()
    {
        // Load Car Prefabs
        GameObject no1201 = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Azerilo/Car Model No.1201 Asset/Prefab/Car_NO.1201.prefab");
        GameObject hatchback = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Hatchback and Sedan/prefabs/HATCHBACK_1988.prefab");
        GameObject sedan = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Hatchback and Sedan/prefabs/SEDAN.prefab");

        carPrefabs = new GameObject[] { no1201, hatchback, sedan };

        // Load NPC Prefab
        GameObject npc00 = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/npc_casual_set_00/Prefabs/npc_casual_set_00.prefab");
        pedestrianPrefabs = new GameObject[] { npc00 };

        // Load Animations
        AnimationClip walk = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/VanillaLoopStudio/FreeSampleAnimationSet/Art/Animations/StairsSet/Mannequin/RootMotion/Walk/A_Stairs_WalkFwd_Up_Loop.fbx");
        AnimationClip idle = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/VanillaLoopStudio/FreeSampleAnimationSet/Art/Animations/SurvivalSet/Mannequin/Flashlight/Idle/RightHand/Type01/A_Flashlight_Idle_01_R.fbx");

        pedWalkAnim = walk;
        pedIdleAnim = idle;
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    void Start()
    {
        roadLayerMask = 1 << LayerMask.NameToLayer("World_Road");
        sidewalkLayerMask = 1 << LayerMask.NameToLayer("World_Sidewalk");

        SpawnCars();
        SpawnPedestroups();
    }

    void SpawnCars()
    {
        if (carPrefabs == null || carPrefabs.Length == 0) return;

        for (int i = 0; i < carCount; i++)
        {
            Vector3 spawnPos = GetRandomNavMeshPosition(roadLayerMask);
            if (spawnPos != Vector3.zero)
            {
                GameObject prefab = carPrefabs[Random.Range(0, carPrefabs.Length)];
                GameObject car = Instantiate(prefab, spawnPos, Quaternion.identity);
                car.layer = LayerMask.NameToLayer("TrafficCar");
                
                // Set Up Child renderers just in case
                foreach (Transform child in car.GetComponentsInChildren<Transform>(true))
                {
                    child.gameObject.layer = LayerMask.NameToLayer("TrafficCar");
                }

                // Add NavMeshAgent
                NavMeshAgent agent = car.AddComponent<NavMeshAgent>();
                agent.speed = carSpeed;
                agent.angularSpeed = 120f;
                agent.acceleration = 8f;
                agent.stoppingDistance = 2f;
                agent.radius = 1f; // Adjust based on car size
                agent.height = 1.5f;
                // Avoid pedestrians crossing accidentally
                agent.areaMask = roadLayerMask; 

                // Add simple controller
                SimpleTrafficCar controller = car.AddComponent<SimpleTrafficCar>();
                controller.roadMask = roadLayerMask;
            }
        }
    }

    void SpawnPedestroups()
    {
        if (pedestrianPrefabs == null || pedestrianPrefabs.Length == 0) return;

        for (int i = 0; i < pedestrianCount; i++)
        {
            Vector3 spawnPos = GetRandomNavMeshPosition(sidewalkLayerMask);
            if (spawnPos != Vector3.zero)
            {
                GameObject prefab = pedestrianPrefabs[Random.Range(0, pedestrianPrefabs.Length)];
                GameObject ped = Instantiate(prefab, spawnPos, Quaternion.identity);
                ped.layer = LayerMask.NameToLayer("Pedestrian");

                foreach (Transform child in ped.GetComponentsInChildren<Transform>(true))
                {
                    child.gameObject.layer = LayerMask.NameToLayer("Pedestrian");
                }

                NavMeshAgent agent = ped.AddComponent<NavMeshAgent>();
                agent.speed = pedSpeed;
                agent.angularSpeed = 120f;
                agent.acceleration = 8f;
                agent.stoppingDistance = 0.5f;
                agent.radius = 0.3f;
                agent.height = 1.8f;
                agent.areaMask = sidewalkLayerMask;

                // Setup animation if needed
                Animator anim = ped.GetComponentInChildren<Animator>();
                if (anim == null) anim = ped.AddComponent<Animator>();
                
                SimplePedestrian controller = ped.AddComponent<SimplePedestrian>();
                controller.sidewalkMask = sidewalkLayerMask;
                controller.walkAnim = pedWalkAnim;
                controller.idleAnim = pedIdleAnim;
            }
        }
    }

    Vector3 GetRandomNavMeshPosition(int areaMask)
    {
        // Try multiple times to find a valid spot
        for (int i = 0; i < 30; i++)
        {
            // Pick a large random area (assuming city is around 500x500 max)
            Vector2 randomPoint = Random.insideUnitCircle * 200f;
            Vector3 testPos = new Vector3(randomPoint.x, 0, randomPoint.y) + transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(testPos, out hit, 10f, areaMask))
            {
                return hit.position;
            }
        }
        return Vector3.zero;
    }
}
