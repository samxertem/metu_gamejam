using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SimpleTrafficCar : MonoBehaviour
{
    public int roadMask;
    private NavMeshAgent agent;
    private float wanderRadius = 100f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        SetNewDestination();
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 3f)
        {
            SetNewDestination();
        }
    }

    void SetNewDestination()
    {
        for (int i = 0; i < 10; i++) // Retry logic
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, roadMask))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }
    }
}
