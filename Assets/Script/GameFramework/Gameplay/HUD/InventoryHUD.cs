using UnityEngine.UI;
using UnityEngine;

public class InventoryHUD : BaseHUD
{
    [Header("----------General----------")]
    [SerializeField] Button m_PrimaryWeaponButton;
    [SerializeField] Button m_SecondaryWeaponButton;

    protected override void OnEnable()
    {
        base.OnEnable();

        PlayerController.OnInventoryHold += OnInventoryHoldOpen;
        PlayerController.OnInventoryClose += OnInventoryClose;
        m_PrimaryWeaponButton.onClick.AddListener(OnPrimaryWeaponSelected);
        m_SecondaryWeaponButton.onClick.AddListener(OnSecondaryWeaponSelected);
    }

    protected override void Start()
    {
        base.Start();
    }

    private void OnPrimaryWeaponSelected()
    {
        if (PlayerWeaponHandler.Instance != null &&
            PlayerWeaponHandler.Instance.PrimaryWeapon != null)
        {
            //IF THE SECONDARY WEAPON IS NOT HOLSTERED, DO THAT
            if (PlayerWeaponHandler.Instance.SecondaryWeapon != null)
                PlayerWeaponHandler.Instance.SecondaryWeapon.HolsterWeapon();

            PlayerWeaponHandler.Instance.PrimaryWeapon.EquipWeapon();
        }
    }

    private void OnSecondaryWeaponSelected()
    {
        if (PlayerWeaponHandler.Instance != null &&
        PlayerWeaponHandler.Instance.SecondaryWeapon != null)
        {
            //IF THE PRIMARY WEAPON IS NOT HOLSTERED, DO THAT
            if (PlayerWeaponHandler.Instance.PrimaryWeapon != null)
                PlayerWeaponHandler.Instance.PrimaryWeapon.HolsterWeapon();

            PlayerWeaponHandler.Instance.SecondaryWeapon.EquipWeapon();
        }
    }

    private void OnInventoryClose()
    {
        //TODO : PLAY SOUND
        m_HUDObject.SetActive(false);

        Cursor.visible = false;
    }

    private void OnInventoryHoldOpen()
    {
        //TODO : PLAY SOUND
        m_HUDObject.SetActive(true);

        Cursor.visible = true;
    }
}
