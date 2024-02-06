using UnityEngine;
using System;


[RequireComponent(typeof(SphereCollider))]
public class BasePickup : Entity
{
    [SerializeField] BasePickupData m_PickupData;

    private SphereCollider m_SphereCollider;

    [Header("----------Decorative----------")]
    public PickupBehaviour m_PickupBehaviour;
    [Tooltip("How fast do you want this to spin?")]
    [HideInInspector] public float m_SpinRate = 100;
    [HideInInspector] public float m_HeightOscillationRate = 5;
    [HideInInspector] public float m_OscillationSpeed = 5;

    protected bool m_IsPickedUp;

    public static event Action<BasePickup> OnPickUp;
    public static event Action<BasePickup> OnEnterPickupRadius;
    public static event Action<BasePickup> OnExitPickupRadius;

    public BasePickupData PickupData { get { return m_PickupData; } }

    protected override void Awake()
    {
        if (!TryGetComponent(out m_SphereCollider))
        {
            Debug.LogError("This Pickup does not have a sphere collider attached!");
        }

        Initialise();
    }

    public void SetCanBePickedUp(bool CanThisBePickedUp)
    {
        //IF WE CAN PICK THIS UP, isPickedUp will be false, that's why it has to be the opposite of CanThisBePickedUp.
        m_IsPickedUp = !CanThisBePickedUp;

        //Remove the previous owner
        Owner = null;
        
        //Remove the previous transform parent
        transform.parent = null;
    }

    public virtual void Initialise() { }

    //Spinny Pickup!
    protected void DoPickUpBobAndHover()
    {
        if (GameManager.Instance && GameManager.Instance.GameIsPaused)
            return;

        transform.eulerAngles += m_SpinRate * Time.deltaTime * Vector3.up;

        transform.position += m_HeightOscillationRate * Time.deltaTime * Mathf.Sin(Time.time * m_OscillationSpeed) * Vector3.up;
    }

    protected override void Update()
    {
        if (m_PickupBehaviour == PickupBehaviour.BobAndSpin)
            DoPickUpBobAndHover();
    }

    protected virtual void OnValidate()
    {
        if (TryGetComponent(out m_SphereCollider))
        {
            if (!m_SphereCollider.isTrigger)
                m_SphereCollider.isTrigger = true;
        }
    }

    protected virtual void DoPlayerPickUp()
    {
        OnPickUp?.Invoke(this);
        PlayerPickupSound();
    }

    protected virtual void PlayerPickupSound()
    {
        if (SoundManager.Instance)
        {
            SoundManager.Instance.PlayFESound("Pickup");
        }
    }

    protected virtual void OnTriggerEnter(Collider other) => OnEnterPickupRadius?.Invoke(this);

    protected virtual void OnTriggerExit(Collider other) => OnExitPickupRadius?.Invoke(this);

    protected virtual void OnTriggerStay(Collider other)
    {
        if (m_IsPickedUp)
            return;

        if (other.TryGetComponent<PlayerCharacter>(out var player))
        {
            //only pick up items if the player pressed the pickup button.
            if (PlayerController.Instance && PlayerController.Instance.PickUpItemPressed)
            {
                if (PlayerCharacter.Instance != null && player == PlayerCharacter.Instance)
                {
                    m_IsPickedUp = true;

                    DoPlayerPickUp();

                    Owner = player;

                    Debug.Log("Player picked up a " + GetType().Name);

                    m_SphereCollider.enabled = false;

                    OnExitPickupRadius?.Invoke(this);
                }
            }
        }
    }
}

public enum PickupBehaviour
{
    BobAndSpin,
    PhysicsBased
}