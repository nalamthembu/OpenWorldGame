using UnityEngine;

[CreateAssetMenu(fileName = "GunData", menuName = "Game/Weapon/Weapon Data/Gun Data")]

public class GunData : WeaponData
{
    [Header("Ammunition")]

    public int maxClip;

    public float fireRate;

    public float damage = 1;

    public float range;

    public GunType gunType;

    [Header("Vectors")]
    public WeaponVectors relaxedPosRot;
    public WeaponVectors aimingPosRot;

    private void OnValidate()
    {
        if (damage <= 0)
            damage = 1;
    }
}

[System.Serializable]
public struct WeaponVectors
{
    public Vector3 position;
    public Vector3 rotation;
}

public enum GunType
{
    Rifle,
    Pistol,
    SMG,
    Sniper
}