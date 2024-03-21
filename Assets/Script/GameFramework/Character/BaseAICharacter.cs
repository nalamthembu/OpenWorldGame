using System.Collections;
using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class BaseAICharacter : BaseCharacter
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected virtual void MoveTo(Vector3 position)
    {

    }
}