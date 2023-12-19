using UnityEngine;
using UnityEngine.AI;
using RootMotion.FinalIK;

[
    RequireComponent
    (
        typeof(FullBodyBipedIK),
        typeof(GrounderFBBIK)
    )
]

[
    RequireComponent
    (
        typeof(LocomotionStateMachine),
        typeof(CharacterStateMachine),
        typeof(NavMeshAgent)
    )
]

public class Character : MonoBehaviour
{
    [SerializeField][Range(1, 10)] protected float m_WalkSpeed = 2;
    [SerializeField][Range(1, 10)] protected float m_RunSpeed = 7;
    [SerializeField][Range(0, 100)] protected float m_Health = 100;
    [SerializeField][Range(0, 100)] protected float m_Armour = 0;
    [SerializeField][Range(0, 5)] protected float m_SpeedSmoothTime;
    [SerializeField][Range(0, 5)] protected float m_RotSmoothTime;
    [SerializeField][Range(-24, 0)] protected float m_Gravity = -12;
    [SerializeField] protected LayerMask m_GroundLayers;

    protected float m_CurrentSpeed;
    public Animator Animator { get; protected set; }
    public float WalkSpeed => m_WalkSpeed;
    public float RunSpeed => m_RunSpeed; 
    public float TargetSpeed { get; set; }
    public float CurrentSpeed => m_CurrentSpeed;

    private Vehicle m_Vehicle;
    public Vehicle CurrentVehicle => m_Vehicle;

    protected WeaponInventory m_WeaponInventory;

    private Transform destinationTransform;

    public Vector3 destinationVector;

    public WeaponInventory WeaponInventory { get { return m_WeaponInventory; } }

    public bool IsAiming { get; private set; }

    public bool IsFiring { get; private set; }

    protected NavMeshAgent m_Agent;

    protected virtual void Awake()
    {
        Animator = GetComponent<Animator>();

        m_Agent = GetComponent<NavMeshAgent>();

        InitialiseCharacter(100, 0);
    }

    public virtual void InitialiseCharacter(float health, float armour)
    {
        m_Health = health;
        m_Armour = armour;

        if (m_WeaponInventory is null)
            m_WeaponInventory = GetComponent<WeaponInventory>();
    }

    public bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1F;

        Vector3 end = -transform.up * 0.1F;

        Debug.DrawLine(origin, origin + end, Color.blue);

        return Physics.Linecast(origin, origin + end, m_GroundLayers);
    }

    public void Freeze() => Animator.SetAnimatorSpeed(0);

    public void UnFreeze() => Animator.SetAnimatorSpeed(1);

    public void SetVehicle(Vehicle vehicle) => m_Vehicle = vehicle;

    public void Teleport(Vector3 pos)
    {
        if (this is PlayerController)
        {
            transform.position = pos;
            Debug.Log("Teleported Player to Location : " + pos);
            return;
        }
        else
        {
            m_Agent.Warp(pos);

            string info = string.Format("Teleported {0} to Location : " + pos, gameObject.name);

            Debug.Log(info);
        }
    }

    public void GoToPosition(Vector3 pos) 
    {
        if (m_Agent.enabled != true)
            m_Agent.enabled = true;

        m_Agent.SetDestination(pos);
        destinationVector = pos;
    }

    public void GoToPosition(Transform target)
    {
        if (m_Agent.enabled != true)
            m_Agent.enabled = true;

        m_Agent.SetDestination(target.position);
        destinationTransform = target;
    }
}