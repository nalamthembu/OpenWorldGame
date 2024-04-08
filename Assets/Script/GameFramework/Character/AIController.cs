using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    Animator m_Animator;

    NavMeshAgent m_NavMeshAgent;
    [SerializeField] bool debug;
    [SerializeField] Transform debug_target;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (m_NavMeshAgent != null)
        {
            Animate();

            if (debug)
                m_NavMeshAgent.SetDestination(debug_target.position);
        }
    }

    private void Animate()
    {
        float angle = Vector3.Angle(transform.position, debug_target.position);

        float agentSpeed = m_NavMeshAgent.velocity.magnitude;
        m_Animator.SetFloat(GameStrings.INPUT_MAGNITUDE, m_NavMeshAgent.velocity.normalized.magnitude);
        m_Animator.SetFloat(GameStrings.TARGET_ROTATION, angle, 0.25f, Time.deltaTime);
        m_Animator.SetFloat(GameStrings.SPEED, agentSpeed, 1, Time.deltaTime);
        m_Animator.SetBool(GameStrings.LU, agentSpeed <= 0);
    }
}