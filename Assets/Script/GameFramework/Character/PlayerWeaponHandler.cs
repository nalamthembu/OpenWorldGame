using UnityEngine;
using System;

public class PlayerWeaponHandler : BaseCharacterWeaponHandler
{
    public static event Action OnPlayerReload;
    public static event Action OnPlayerIsDoneReloading;

    public static PlayerWeaponHandler Instance;

    //WERE WE EQUIPPED WITH A WEAPON BEFORE GOING PRONE?
    private bool m_WasPreviouslyEquippedWithPrime;
    private bool m_WasPreviouslyEquippedWithSec;

    private const float TIME_BEFORE_REEQUIPPING = 1.0F;
    private float m_RequipTimer = 0;

    protected override void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        PlayerController.OnReload += ManualReload;
        PlayerController.OnQuickSwap += QuickSwapWeapon;
    }


    protected override void OnDisable()
    {
        base.OnEnable();
        PlayerController.OnReload -= ManualReload;
        PlayerController.OnQuickSwap -= QuickSwapWeapon;
    }

    protected override void Update()
    {
        base.Update();

        //TODO : Make this work with AI as well.
        //Holster the weapon if we go prone
        if (PlayerStateMachine.Instance && PlayerStateMachine.Instance.currentState is ProneState)
        {
            if (GetEquippedWeapon())
            {
                switch(GetEquippedWeapon().WeaponData.weaponType)
                {
                    case WeaponType.Primary:
                        m_WasPreviouslyEquippedWithPrime = true;
                        m_WasPreviouslyEquippedWithSec = false;
                        break;

                    case WeaponType.Secondary:
                        m_WasPreviouslyEquippedWithPrime = false;
                        m_WasPreviouslyEquippedWithSec = true;
                        break;
                }

                GetEquippedWeapon().HolsterWeapon();

                m_RequipTimer = 0;
            }
        }
        else
        {
            m_RequipTimer += Time.deltaTime;

            if (m_RequipTimer >= TIME_BEFORE_REEQUIPPING)
            {
                if (m_WasPreviouslyEquippedWithPrime)
                {
                    PrimaryWeapon.EquipWeapon();
                    m_WasPreviouslyEquippedWithPrime = false;
                    m_WasPreviouslyEquippedWithSec = false;
                }

                if (m_WasPreviouslyEquippedWithSec)
                {
                    SecondaryWeapon.EquipWeapon();
                    m_WasPreviouslyEquippedWithPrime = false;
                    m_WasPreviouslyEquippedWithSec = false;
                }

                m_RequipTimer = 0;
            }
        }
    }
}