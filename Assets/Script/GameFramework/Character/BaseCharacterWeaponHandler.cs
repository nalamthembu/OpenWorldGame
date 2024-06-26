﻿using UnityEngine;

public class BaseCharacterWeaponHandler : MonoBehaviour
{
    //Player Can only carry two firearms 
    Gun m_PrimaryWeapon;
    Gun m_SecondaryWeapon;

    protected BaseCharacter m_ThisCharacter;

    public Gun PrimaryWeapon { get { return m_PrimaryWeapon; } }
    public Gun SecondaryWeapon { get { return m_SecondaryWeapon; } }

    float m_IKWeight;
    float m_IKWeightVelRef;
    const float IKWEIGHTSMOOTHTIME = 0.25F;

    public bool HasAnyWeapon() => PrimaryWeapon || SecondaryWeapon;

    protected virtual void Awake() => m_ThisCharacter = GetComponent<BaseCharacter>();

    protected virtual void OnEnable() => Gun.OnReload += OnGunBeginReload;

    protected virtual void OnDisable() => Gun.OnReload -= OnGunBeginReload;

    protected virtual void ManualReload()
    {
        if (GetEquippedWeapon() == null)
            return;

        if (!GetEquippedWeapon().IsReloading && GetEquippedWeapon().CanReload)
            GetEquippedWeapon().Reload();
    }

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

    public void EquipFirstAvailableWeapon()
    {
        if (m_PrimaryWeapon != null)
            m_PrimaryWeapon.EquipWeapon();
        else if (SecondaryWeapon != null)
            m_SecondaryWeapon.EquipWeapon();
        else
        Debug.Log("No weapons available");
    }

    //Quickly swap to other weapon (used when the character is in eminent danger and reloading would be too slow)
    public void QuickSwapWeapon()
    {
        if (GetEquippedWeapon() == null)
            return;

        if (GetEquippedWeapon() == m_PrimaryWeapon)
        {
            //Swap to secondary 
            m_PrimaryWeapon.HolsterWeapon();
            m_SecondaryWeapon.EquipWeapon();
        }
        else
        {
            //Swap to primary 
            m_PrimaryWeapon.EquipWeapon();
            m_SecondaryWeapon.HolsterWeapon();
        }
    }

    protected virtual void OnGunBeginReload(Gun targetGun)
    {
        bool TargetGunIsEquippedWeapon = targetGun == GetEquippedWeapon();

        if (!TargetGunIsEquippedWeapon) 
            return;

        m_ThisCharacter.Animator.SetTrigger(GameStrings.IS_RELOADING);
    }

    protected virtual void Update() { /* used by child classes */ }

    protected virtual void OnAnimatorIK(int layerIndex)
    {
        if (!GetEquippedWeapon())
            return;

        Gun gun = GetEquippedWeapon();

        if (gun != null && gun.LeftHandIK != null) 
        {
            //Disable IK when reloading or not equipped
            if (!GetEquippedWeapon() || GetEquippedWeapon() && GetEquippedWeapon().IsReloading)
            {
                //Smoothly Interpolate
                m_IKWeight = Mathf.SmoothDamp(m_IKWeight, 0, ref m_IKWeightVelRef, IKWEIGHTSMOOTHTIME);

                m_ThisCharacter.Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, m_IKWeight);
                m_ThisCharacter.Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, m_IKWeight);
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
                } && !m_ThisCharacter.IsAiming;

                //Smoothly Interpolate
                m_IKWeight = Mathf.SmoothDamp(m_IKWeight, singleHandedWeaponWhenNotAiming ? 0 : 1, ref m_IKWeightVelRef, IKWEIGHTSMOOTHTIME);

                m_ThisCharacter.Animator.SetIKPosition(AvatarIKGoal.LeftHand, gun.LeftHandIK.position);
                m_ThisCharacter.Animator.SetIKRotation(AvatarIKGoal.LeftHand, gun.LeftHandIK.rotation);
                m_ThisCharacter.Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, m_IKWeight);
                m_ThisCharacter.Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, m_IKWeight);
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
                            RemoveWeapon(WeaponType.Primary);

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
                            RemoveWeapon(WeaponType.Secondary);

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