﻿using System;
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
public class BaseCharacter : Entity
{
    #region Properties
    protected enum FootUp
    {
        Left,
        Right
    };

    [SerializeField] CharacterAnimationData m_animData;

    [SerializeField][Range(1, 7)] protected float m_WalkSpeed, m_RunSpeed;

    [SerializeField] BehaviourPuppet m_BehaviourPuppet;

    [SerializeField] private BehaviourFall m_FallBehaviour;

    [SerializeField] protected LayerMask  m_GroundLayers = -1;

    protected HealthComponent m_HealthComponent;

    protected NavMeshAgent m_NavMeshAgent;

    protected Animator m_Animator;

    // Ragdoll Behaviour 

    private PuppetMaster m_PuppetMasterComponent;
    public PuppetMaster PuppetMaster { get { return m_PuppetMasterComponent; } }

    public CharacterState CharacterState { get; protected set; }
    protected virtual void OnEnabled() { }
    protected virtual void OnDisabled() { }

    public Animator Animator { get { return m_Animator; } }
    public bool IsAiming { get; set; }
    public bool IsFiring { get; set; }
    public bool CharacterIsGrounded { get; private set; }

    public HealthComponent GetHealthComponent() => m_HealthComponent;

    public static event Action<BaseCharacter> OnSpawned;

    #endregion

    protected override void Awake()
    {
        base.Awake();

        m_HealthComponent = GetComponent<HealthComponent>();

        m_PuppetMasterComponent = GetComponentInChildren<PuppetMaster>();

        m_Animator = GetComponent<Animator>();

        if (m_animData is null)
        {
            Debug.LogError("There is no animation data assigned to this character!");
        }

        CharacterState = CharacterState.OnFoot;

        b_CanBeDamaged = true;
    }

    protected void HandleNavMeshAgent()
    {
        if (m_NavMeshAgent)
        {
            if (GetHealthComponent().CharacterIsFalling || GetHealthComponent().IsDead)
            {
                m_NavMeshAgent.isStopped = true;

                if (GetHealthComponent().IsDead)
                {
                    m_NavMeshAgent.enabled = false;
                }

                Debug.Log("Stopping Nav Mesh Agent, the ragdoll is active.");
            }
        }
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

    protected override void Update()
    {
        base.Update();

        // Keep the agent disabled while the puppet is unbalanced.
        if (m_NavMeshAgent && m_BehaviourPuppet)
            m_NavMeshAgent.enabled = m_BehaviourPuppet.state == BehaviourPuppet.State.Puppet;

        //Don't do anything if you're dead.
        if (GetHealthComponent().IsDead)
            return;

        if (!IsGrounded() && GetHealthComponent() != null && m_FallBehaviour != null)
        {
            //FALL
            m_FallBehaviour.Activate();
        }
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

    public bool IsGrounded()
    {
        Transform RagdollTransform = null;

        if (m_FallBehaviour)
            RagdollTransform = m_FallBehaviour.puppetMaster.targetRoot;

        Vector3 origin = m_FallBehaviour ? RagdollTransform.position + transform.up * 0.2F : transform.position + transform.up * 0.2F;

        bool isGrounded = Physics.Linecast(origin, origin + Vector3.down * 0.25f, out var hit, m_GroundLayers, QueryTriggerInteraction.Ignore);

        Debug.DrawLine(origin, hit.point, Color.green);

        CharacterIsGrounded = isGrounded;

        return isGrounded;
    }
}