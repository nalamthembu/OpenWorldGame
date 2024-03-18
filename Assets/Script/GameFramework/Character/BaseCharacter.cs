using System;
using UnityEngine;
using UnityEngine.AI;
using RootMotion.Dynamics; //PuppetMaster Active Ragdoll Physics (Dependency)
using Random = UnityEngine.Random;

public enum CharacterState
{
    OnFoot,
    Prone,
    Crouch,
    InVehicle
};

[RequireComponent(typeof(CharacterSpeech))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BaseCharacterWeaponHandler))]
[RequireComponent(typeof(NavMeshAgent))]
public class BaseCharacter : Entity
{
    #region Properties
    protected enum FootUp
    {
        Left,
        Right
    };

    protected NavMeshAgent m_Agent;
    protected NavMeshPath m_NavMeshPath;

    protected Animator m_Animator;

    [SerializeField] CharacterAnimationData m_animData;

    [SerializeField][Range(1, 7)] protected float m_WalkSpeed, m_RunSpeed;

    private PuppetMaster m_PuppetMasterComponent;

    public PuppetMaster PuppetMaster { get { return m_PuppetMasterComponent; } }

    public CharacterState CharacterState { get; protected set; }

    public static event Action<BaseCharacter> OnSpawned;

    protected virtual void OnEnabled() { }
    protected virtual void OnDisabled() { }

    public Animator Animator { get { return m_Animator; } }

    public bool IsAiming { get; set; }
    public bool IsFiring { get; set; }

    #endregion

    protected override void Awake()
    {
        base.Awake();

        m_PuppetMasterComponent = GetComponentInChildren<PuppetMaster>();

        m_Animator = GetComponent<Animator>();

        if (m_animData is null)
        {
            Debug.LogError("There is no animation data assigned to this character!");
        }

        CharacterState = CharacterState.OnFoot;

        b_CanBeDamaged = true;
    }

    public override void OnShot(Projectile projectile, Entity OwnerOfProjectile)
    {
        base.OnShot(projectile, OwnerOfProjectile);
        float dotProduct = Vector3.Dot(projectile.transform.position, transform.position);
        Animator.CrossFadeInFixedTime(GameStrings.GOT_HIT, 0.25F); //Transition to animation
        Animator.SetFloat(GameStrings.HIT_DIRECTION, dotProduct);
    }

    protected override void Start()
    {
        base.Start();
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