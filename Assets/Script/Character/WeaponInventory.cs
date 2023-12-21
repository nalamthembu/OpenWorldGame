using System;
using UnityEngine;
using System.Collections.Generic;

public class WeaponInventory : MonoBehaviour
{
    [Header("Holsters")]
    [SerializeField] Transform m_LongArmHolster;
    [SerializeField] Transform m_SideArmHolster;

    [SerializeField] DebugWeaponInventory debug;

    public Transform PrimaryWeaponHolster { get { return m_LongArmHolster; } }
    public Transform SecondaryWeaponHolster { get { return m_SideArmHolster; } }

    private Dictionary<WeaponType, Weapon> m_Weapons;

    public static event Action AddedWeaponToInventory;

    private Weapon currentWeapon;

    public bool HasWeaponEquipped { get; private set; }

    public Weapon CurrentWeapon { get { return currentWeapon; } }

    private void Awake()
    {
        InitialiseWeaponInventory();

        debug = new(this);
    }

    private void Update()
    {
        if (debug.enabled)
        {
            debug.Update();
        }
    }

    private void InitialiseWeaponInventory()
    {
        m_Weapons = new();
    }

    public bool AddWeapon(Weapon weapon)
    {
        string weaponName = weapon.WeaponData.weaponName;

        bool addingWeaponWasSuccessful = m_Weapons.TryAdd(weapon.WeaponData.weaponType, weapon);

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

            currentWeapon.SetEquipStatus(false);
        }

        //Tell me if I was successful.
        return addingWeaponWasSuccessful;
    }

    public bool TryEquipWeapon(WeaponType weaponType)
    {
        if (m_Weapons.TryGetValue(weaponType, out var weapon))
        {
            if (currentWeapon != null)
            {
                currentWeapon.SetEquipStatus(false);
            }

            weapon.SetEquipStatus(true);

            HasWeaponEquipped = true;

            return true;
        }

        Debug.Log("Could not equip " + weaponType);

        return false;
    }
}

[System.Serializable]
public struct DebugWeaponInventory
{
    WeaponInventory weaponInventory;

    public bool enabled;

    public DebugWeaponInventory(WeaponInventory weaponInventory)
    {
        this.weaponInventory = weaponInventory;
        this.enabled = false;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            MaxAmmoCheat();

            HUDManager.instance.GetNotifications().ShowNotification("Weapon Cheat");
        }
    }

    public void MaxAmmoCheat()
    {
        if (weaponInventory.CurrentWeapon != null)
        {
            if (weaponInventory.CurrentWeapon is Gun gun)
            {
                gun.AddAmmo(9999);
            }
        }
    }
}