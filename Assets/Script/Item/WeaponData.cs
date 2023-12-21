using UnityEngine;

public class WeaponData : ScriptableObject
{
    public string weaponName;

    public WeaponType weaponType;

    [Header("Sound")]
    public ObjectSoundData objectSoundData;

    public WeaponVectors holsteredVector;
}

public enum WeaponType
{
    SECONARY,
    PRIMARY,
}