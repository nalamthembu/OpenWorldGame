using UnityEngine;

public class Weapon : BasePickup
{
    [Header("----------General----------")]
    [SerializeField] protected BaseWeaponData m_WeaponData;

    protected MeshCollider m_MeshCollider;

    protected Rigidbody m_RigidBody;

    [Header("----------Debug----------")]
    [SerializeField] bool m_DebugHolsterPosition;
    [SerializeField] bool m_DebugEquippedPosition;

    public bool IsEquipped { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        m_MeshCollider = GetComponent<MeshCollider>();

        m_RigidBody = GetComponent<Rigidbody>();

        if (!m_RigidBody)
            Debug.LogError("There is no rigidbody attached to this weapon : " + gameObject.name);

        if (!m_MeshCollider)
            Debug.LogError("There is no mesh collider attached to this weapon : " + gameObject.name);
    }

    protected override void Update()
    {
        base.Update();

        if (m_DebugHolsterPosition)
        {
            m_DebugEquippedPosition = false;
            transform.SetLocalPositionAndRotation(m_WeaponData.holsteredPosition,
                Quaternion.Euler(m_WeaponData.holsteredRotation));
        }

        if (m_DebugEquippedPosition)
        {
            m_DebugHolsterPosition = false;
            transform.SetLocalPositionAndRotation(m_WeaponData.armedPosition,
            Quaternion.Euler(m_WeaponData.armedRotation));
        }
    }

    public BaseWeaponData WeaponData { get { return m_WeaponData; } }

    public virtual void HolsterWeapon()
    {
        if (Owner)
        {
            transform.SetParent(Owner.GetComponent<Animator>().GetBoneTransform(WeaponData.holsterTargetBone));
        }

        transform.SetLocalPositionAndRotation
            (
                m_WeaponData.holsteredPosition, 
                Quaternion.Euler(m_WeaponData.holsteredRotation)
            );

        IsEquipped = false;
    }

    public virtual void EquipWeapon()
    {
        if (Owner)
        {
            transform.SetParent(Owner.GetComponent<Animator>().GetBoneTransform(WeaponData.equippedTargetBone));
        }

        transform.SetLocalPositionAndRotation(m_WeaponData.armedPosition,
            Quaternion.Euler(m_WeaponData.armedRotation));
        IsEquipped = true;
    }

    protected override void PlayerPickupSound()
    {
        //TODO : Play Weapon Pickup Sound
    }

    protected override void OnTriggerStay(Collider other)
    {
        //The owner is set in the base.
        base.OnTriggerStay(other);
    }

    public override void SetOwner(Entity owner)
    {
        Owner = owner;

        if (owner)
        {
            Animator ownerAnimator = owner.GetComponent<Animator>();

            if (owner && ownerAnimator)
            {
                //Attach to targetBone.
                transform.SetParent(ownerAnimator.GetBoneTransform(m_WeaponData.holsterTargetBone));
            }
        }
    }


    //TODO: Might move this up to Entity.cs
    public void SetSimulatePhysics(bool status)
    {
        switch(status)
        {
            case false:
                m_MeshCollider.enabled = false;
                m_RigidBody.isKinematic = true;
                break;
            case true:
                m_MeshCollider.enabled = true;
                m_RigidBody.isKinematic = false;
                break;
        }
    }

    public override void DoGenericCharacterPickup(BaseCharacter character)
    {
        base.DoGenericCharacterPickup(character);

        BaseCharacterWeaponHandler weaponHandler = character.GetComponent<BaseCharacterWeaponHandler>();

        if (weaponHandler)
        {
            weaponHandler.AddWeapon(this);
        }

        //Disable collider and rigidbody

        SetSimulatePhysics(false);
    }

    protected override void DoPlayerPickUp()
    {
        base.DoPlayerPickUp();

        if (PlayerCharacter.Instance)
        {
            SetOwner(PlayerCharacter.Instance);

            PlayerWeaponHandler playerWeaponHandler = PlayerCharacter.Instance.GetComponent<PlayerWeaponHandler>();

            if (playerWeaponHandler)
            {
                playerWeaponHandler.AddWeapon(this);
            }

            //Disable Collider and rigidbody
            SetSimulatePhysics(false);
        }
    }


}