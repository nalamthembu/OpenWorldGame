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
    SIDE_ARM,
    LONG_ARM
}