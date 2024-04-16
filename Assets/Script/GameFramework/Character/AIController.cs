using UnityEngine;
using UnityEngine.AI;

public class AIController : BaseCharacterController
{
    NavMeshAgent m_NavMeshAgent;
    [SerializeField] bool debug;
    [SerializeField] Transform debug_target;

    private void Awake()
    {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (m_NavMeshAgent != null)
        {
            if (debug)
                m_NavMeshAgent.SetDestination(debug_target.position);
        }
    }

    public Vector2 GetInputDirection()
    {
        // Get the velocity of the NavMeshAgent
        Vector3 velocity = m_NavMeshAgent.velocity;

        // Convert the velocity from world space to local space
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);

        // Normalize the local velocity to get a directional Vector2
        Vector2 directionalVector = new Vector2(localVelocity.x, localVelocity.z).normalized;

        return directionalVector;
    }
}