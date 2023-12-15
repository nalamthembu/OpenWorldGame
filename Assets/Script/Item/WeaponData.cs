using UnityEngine;

public class WeaponData : ScriptableObject
{
    public string weaponName;

    [Header("Sound")]
    public ObjectSoundData objectSoundData;

    public WeaponVectors holsteredVector;
}