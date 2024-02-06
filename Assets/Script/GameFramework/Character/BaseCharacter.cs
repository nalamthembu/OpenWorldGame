using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BaseCharacterWeaponHandler))]
public class BaseCharacter : Entity
{
    protected enum FootUp
    {
        Left,
        Right
    };

    protected Animator m_Animator;

    [SerializeField] CharacterAnimationData m_animData;

    [SerializeField][Range(1, 7)] protected float m_WalkSpeed, m_RunSpeed;

    public CharacterState CharacterState { get; protected set; }

    public static event Action<BaseCharacter> OnSpawned;

    protected virtual void OnEnabled() { }
    protected virtual void OnDisabled() { }

    public Animator Animator { get { return m_Animator; } }

    public bool IsAiming { get; set; }
    public bool IsFiring { get; set; }

    protected override void Awake()
    {
        base.Awake();

        m_Animator = GetComponent<Animator>();

        if (m_animData is null)
        {
            Debug.LogError("There is no animation data assigned to this character!");
        }

        CharacterState = CharacterState.OnFoot;
    }

    protected virtual void Start()
    {
        OnSpawned?.Invoke(this);
    }

    public override void Teleport(Vector3 position)
    {
        int MAX_ITERATIONS = 1000;

        Vector3 evaluatedPosition = position;

        //Try get a random position a set number of times

        for (int i = 0; i < MAX_ITERATIONS; i++)
        {
            //Test the navMesh
            if (NavMesh.SamplePosition(evaluatedPosition, out var hit, float.MaxValue, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                break;
            }
            else
            {
                Vector3 FlatVectorSphere = Random.insideUnitSphere * 100.0F;

                FlatVectorSphere.y = transform.position.y;

                evaluatedPosition += FlatVectorSphere;
            }
        }

        Debug.Log("Could not teleport this base entity");
    }

    protected FootUp CheckWhichFootIsUp()
    {
        Vector3 leftFootPos = m_Animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
        Vector3 rightFootPos = m_Animator.GetBoneTransform(HumanBodyBones.RightFoot).position;

        if (leftFootPos.y > rightFootPos.y)
            return FootUp.Left;
        else
            return FootUp.Right;
    }
}

public enum CharacterState
{
    OnFoot,
    Prone,
    Crouch,
    InVehicle
};