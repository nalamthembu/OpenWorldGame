using UnityEngine;

public class BaseCharacterWeaponHandler : MonoBehaviour
{
    //Player Can only carry two firearms 
    Gun m_PrimaryWeapon;
    Gun m_SecondaryWeapon;

    protected BaseCharacter m_ThisCharacter;

    public Gun PrimaryWeapon { get { return m_PrimaryWeapon; } }
    public Gun SecondaryWeapon { get { return m_SecondaryWeapon; } }

    public Gun GetEquippedWeapon()
    {
        if (m_PrimaryWeapon && m_PrimaryWeapon.IsEquipped)
            return PrimaryWeapon;
        else
            if (m_SecondaryWeapon && m_SecondaryWeapon.IsEquipped)
            return m_SecondaryWeapon;
        else
            return null;
    }

    protected virtual void Awake() 
    {
        m_ThisCharacter = GetComponent<BaseCharacter>();
    }

    protected virtual void Update() { /* used by child classes */ }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!GetEquippedWeapon())
            return;

        Gun gun = GetEquippedWeapon();

        if (gun != null && gun.LeftHandIK != null) 
        {
            if (!GetEquippedWeapon())
            {
                m_ThisCharacter.Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                m_ThisCharacter.Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                return;
            }
            else
            {
                GunData gunData = (GunData)gun.WeaponData;

                bool singleHandedWeaponWhenNotAiming = gunData.WeaponClass switch
                {
                    WeaponClassification.Pistol => true,
                    WeaponClassification.Grenade => true,
                    WeaponClassification.Remote_Explosive => true,
                    _ => false
                };

                m_ThisCharacter.Animator.SetIKPosition(AvatarIKGoal.LeftHand, gun.LeftHandIK.position);
                m_ThisCharacter.Animator.SetIKRotation(AvatarIKGoal.LeftHand, gun.LeftHandIK.rotation);
                m_ThisCharacter.Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, singleHandedWeaponWhenNotAiming? 0 : 1);
                m_ThisCharacter.Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, singleHandedWeaponWhenNotAiming ? 0 : 1);
            }
        }
    }

    public virtual void AddWeapon(Weapon weaponToAdd)
    {
        //if this is a gun.
        if (weaponToAdd is Gun thisNewGun)
        {
            switch (thisNewGun.WeaponData.weaponType)
            {
                case WeaponType.Primary:

                    if (m_PrimaryWeapon == null)
                    {
                        m_PrimaryWeapon = thisNewGun;
                        m_PrimaryWeapon.HolsterWeapon();
                    }
                    else
                    {
                        //IF THIS IS THE SAME GUN...
                        if (m_PrimaryWeapon.WeaponData.weaponName.Equals(thisNewGun.WeaponData.weaponName))
                        {
                            //Take all the ammo.
                            m_PrimaryWeapon.AddAmmo(thisNewGun.TakeAllAmmo());
                        }
                        else
                        {
                            //Was the previous primary weapon equipped?
                            bool PreviousWeaponWasEquipped = m_PrimaryWeapon.IsEquipped;

                            //Drop the old one
                            m_PrimaryWeapon.SetCanBePickedUp(true);
                            m_PrimaryWeapon.SetSimulatePhysics(true);
                            m_PrimaryWeapon = null;

                            //Get the new one
                            m_PrimaryWeapon = thisNewGun;

                            if (PreviousWeaponWasEquipped)
                                m_PrimaryWeapon.EquipWeapon();
                            else
                                m_PrimaryWeapon.HolsterWeapon();
                        }
                    }

                    break;

                case WeaponType.Secondary:

                    if (m_SecondaryWeapon == null)
                    {
                        m_SecondaryWeapon = thisNewGun;
                        m_SecondaryWeapon.HolsterWeapon();
                    }
                    else
                    {
                        //IF THIS IS THE SAME GUN...
                        if (m_SecondaryWeapon.WeaponData.weaponName.Equals(thisNewGun.WeaponData.weaponName))
                        {
                            //Take all the ammo.
                            m_SecondaryWeapon.AddAmmo(thisNewGun.TakeAllAmmo());
                        }
                        else
                        {
                            //Was the previous SEC. weapon equipped?
                            bool PreviousWeaponWasEquipped = m_SecondaryWeapon.IsEquipped;

                            //Drop the old one
                            m_SecondaryWeapon.SetCanBePickedUp(true);
                            m_SecondaryWeapon.SetSimulatePhysics(true);
                            m_SecondaryWeapon = null;

                            //Get the new one
                            m_SecondaryWeapon = thisNewGun;

                            if (PreviousWeaponWasEquipped)
                                m_SecondaryWeapon.EquipWeapon();
                            else
                                m_SecondaryWeapon.HolsterWeapon();
                        }
                    }

                    break;
            }
        }
    }

    public virtual void RemoveWeapon(WeaponType weaponToRemove)
    {
        switch(weaponToRemove)
        {
            case WeaponType.Primary:
                m_PrimaryWeapon.SetCanBePickedUp(true);
                m_PrimaryWeapon.SetSimulatePhysics(true);
                m_PrimaryWeapon = null;
                break;

            case WeaponType.Secondary:
                m_SecondaryWeapon.SetCanBePickedUp(true);
                m_SecondaryWeapon.SetSimulatePhysics(false);
                m_SecondaryWeapon = null;
                break;
        }
    }
}