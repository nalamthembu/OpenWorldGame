using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class BaseAICharacter : BaseCharacter
{
    Blackboard m_Blackboard;

    protected override void Awake()
    {
        base.Awake();

        m_Blackboard = new Blackboard();
    }

    protected virtual void MoveTo(Vector3 position)
    {

    }
}