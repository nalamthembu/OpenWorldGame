using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class BaseAICharacter : BaseCharacter
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();

        HandleNavMeshAgent();
    }
}