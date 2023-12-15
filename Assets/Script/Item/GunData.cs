using UnityEngine;

[CreateAssetMenu(fileName = "GunData", menuName = "Game/Weapon/Weapon Data/Gun Data")]

public class GunData : WeaponData
{
    [Header("Ammunition")]

    public int maxClip;

    public float fireRate;

    public float range;

    [Header("Vectors")]
    public WeaponVectors relaxedPosRot;
    public WeaponVectors aimingPosRot;
}

[System.Serializable]
public struct WeaponVectors
{
    public Vector3 position;
    public Vector3 rotation;
}