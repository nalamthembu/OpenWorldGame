using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(HealthComponent))]
public class BaseAICharacter : BaseCharacter
{
    protected NavMeshAgent m_Agent;
    protected NavMeshPath m_NavMeshPath;

    protected virtual void MoveTo(Vector3 position)
    {

    }
}