using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SimplePedestrian : MonoBehaviour
{
    public int sidewalkMask;
    public AnimationClip walkAnim;
    public AnimationClip idleAnim;

    private NavMeshAgent agent;
    private Animator animator;
    private RuntimeAnimatorController simpleOverride;
    private float wanderRadius = 30f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        
        // Basic animation setup - simplistic override controller setup assuming an Idle/Walk float state
        if (animator != null && walkAnim != null && idleAnim != null)
        {
            // Simple approach: force play state animation clips directly or relying on pre-existing controller.
            // Since we are creating from scratch, we might need a generic controller, but let's try direct play override.
            AnimatorOverrideController aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);
            if (aoc != null) 
            {
               // This setup requires the prefab to already have a basic character controller which many standard assets have.
               // We will update the speed float if it exists, otherwise we'll forcibly crossfade
            }
        }

        SetNewDestination();
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            SetNewDestination();
        }

        // Extremely basic animation force
        if (animator != null)
        {
           if (agent.velocity.sqrMagnitude > 0.1f) {
                if(walkAnim != null) animator.CrossFade(walkAnim.name, 0.2f);
           } else {
                if(idleAnim != null) animator.CrossFade(idleAnim.name, 0.2f);
           }
        }
    }

    void SetNewDestination()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, sidewalkMask))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }
    }
}
