using UnityEngine;

public class BaseWeaponData : BasePickupData
{
    public string weaponName;
    public float damage;
    public HumanBodyBones holsterTargetBone;
    public HumanBodyBones equippedTargetBone = HumanBodyBones.RightHand;
    public WeaponType weaponType;

    [Header("----------Holstered Settings----------")]
    public Vector3 holsteredPosition;
    public Vector3 holsteredRotation;

    [Header("----------Armed Settings----------")]
    public Vector3 armedPosition;
    public Vector3 armedRotation;

    private void OnValidate() => m_PickupName = weaponName;   
}

public enum WeaponType
{
    Primary,
    Secondary,
    Melee
};