using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(HealthComponent))]
public class BaseAICharacter : BaseCharacter
{
    protected virtual void MoveTo(Vector3 position)
    {

    }
}