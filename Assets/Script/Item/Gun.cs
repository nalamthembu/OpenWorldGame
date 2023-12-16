using UnityEngine;

public class Gun : Weapon
{
    [SerializeField] protected Transform m_LeftHandIKTransform;
    public Transform LeftHandIK { get { return m_LeftHandIKTransform; } }

    private GunData m_GunData;

    protected override void Awake()
    {
        base.Awake();

        if (m_WeaponData != null && m_WeaponData is GunData gunData)
        {
            m_GunData = gunData;
        }
    }

    private void Update()
    {
        if (m_IsPickedUp)
        {
            if (m_IsEquipped)
            {
                //Check parent
                if (transform.parent != m_Owner.Animator.GetBoneTransform(HumanBodyBones.RightHand))
                    transform.parent = m_Owner.Animator.GetBoneTransform(HumanBodyBones.RightHand);


                //if owner is not aiming.
                if (!m_Owner.IsAiming)
                {
                    transform.SetLocalPositionAndRotation(
                    m_GunData.relaxedPosRot.position,
                    Quaternion.Euler(m_GunData.relaxedPosRot.rotation)
                    );
                }

                //If owner is aiming.
                if (m_Owner.IsAiming)
                {
                   
                    transform.SetLocalPositionAndRotation(
                    m_GunData.aimingPosRot.position,
                    Quaternion.Euler(m_GunData.aimingPosRot.rotation)
                    );
                }
            }
            else
            {
                //Check parent
                switch (m_WeaponData.weaponType)
                {
                    case WeaponType.LONG_ARM:
                        if (transform.parent != m_Owner.WeaponInventory.LongArmHolster)
                            transform.parent = m_Owner.WeaponInventory.LongArmHolster;
                        break;

                    case WeaponType.SIDE_ARM:
                        if (transform.parent != m_Owner.WeaponInventory.SideArmHolster)
                            transform.parent = m_Owner.WeaponInventory.SideArmHolster;
                        break;
                }

                transform.SetLocalPositionAndRotation(
                    WeaponData.holsteredVector.position,
                    Quaternion.Euler(WeaponData.holsteredVector.rotation)
                    );
            }
        }
    }

    //Collision
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 2)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayInGameSound
                    (
                        WeaponData.objectSoundData.CollisionSoundNames,
                        transform.position,
                        true
                    );

            }
        }
    }
}