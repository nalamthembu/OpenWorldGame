using System;
using UnityEngine;
using System.Collections.Generic;

public class WeaponInventory : MonoBehaviour
{
    [Header("Holsters")]
    [SerializeField] Transform m_LongArmHolster;
    [SerializeField] Transform m_SideArmHolster;

    private Dictionary<string, Weapon> m_Weapons;

    public static event Action AddedWeaponToInventory;

    private Weapon currentWeapon;

    private void Awake()
    {
        InitialiseWeaponInventory();
    }

    private void InitialiseWeaponInventory()
    {
        m_Weapons = new();
    }

    public bool AddWeapon(Weapon weapon)
    {
        string weaponName = weapon.WeaponData.weaponName;

        bool addingWeaponWasSuccessful = m_Weapons.TryAdd(weaponName, weapon);

        //Trigger event for all listeners.
        if (addingWeaponWasSuccessful)
        {
            AddedWeaponToInventory?.Invoke();

            currentWeapon = weapon;

            //Set Parent
            if (currentWeapon is TwoHandedWeapon)
                currentWeapon.transform.parent = m_LongArmHolster;
            //Set Parent
            if (currentWeapon is OneHandedWeapon)
                currentWeapon.transform.parent = m_SideArmHolster;

            //Set Vectors
            currentWeapon.transform.localEulerAngles *= 0;
            currentWeapon.transform.localPosition *= 0;

            EquipWeapon(weaponName);
        }

        //Tell me if I was successful.
        return addingWeaponWasSuccessful;
    }

    public void EquipWeapon(string weaponName)
    {
        if (m_Weapons.TryGetValue(weaponName, out var weapon))
        {
            weapon.gameObject.SetActive(true);

            weapon.SetEquipStatus(true);
        }
    }
}