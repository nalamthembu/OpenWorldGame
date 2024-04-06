using UnityEngine;
using UnityEngine.Audio;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "GunData", menuName = "Game/Weapon/Weapon Data/Gun")]
public class GunData : BaseWeaponData
{
    public int MaxClip;
    public float range = 1000;
    public FireType FireType;
    public WeaponClassification WeaponClass;
    public GameObject BulletPrefab;
    [HideInInspector] [Min(0.1f)] public float FireRateInSeconds;
    public float ReloadTime = 2.167f;
    public GunSound GunSound;
    public AudioMixerGroup mixerGroup;
}

[System.Serializable]
public class GunSound
{
    [SerializeField] AudioClip[] Shot;
    [SerializeField] AudioClip[] IndoorShot;
    [SerializeField] AudioClip[] ShotTail;
    [SerializeField] AudioClip[] ShotIndoorTail;
    [SerializeField] AudioClip Sweetener;
    [SerializeField] AudioClip[] ShotMechanics;
    [SerializeField] AudioClip[] clipOut;
    [SerializeField] AudioClip[] clipIn;
    [SerializeField] AudioClip[] chamberPartOneAndTwo;

    public AudioClip GetShotClip() => Shot[Random.Range(0, Shot.Length)];
    public AudioClip GetShotTail() => ShotTail[Random.Range(0, ShotTail.Length)];
    public AudioClip GetShotIndoorTail() => ShotIndoorTail[Random.Range(0, ShotIndoorTail.Length)];
    public AudioClip GetSweetener() => Sweetener;
    public AudioClip GetIndoorShot() => IndoorShot[Random.Range(0, IndoorShot.Length)];
    public AudioClip GetShotMechanics() => ShotMechanics[Random.Range(0, ShotMechanics.Length)];
    public AudioClip GetClipOut() => clipOut[Random.Range(0, clipOut.Length)];
    public AudioClip GetClipIn() => clipIn[Random.Range(0, clipIn.Length)];
    public AudioClip GetChamber(int part) => chamberPartOneAndTwo[part - 1];
}


public enum FireType
{
    Semi_Auto,
    Full_Auto
};

public enum WeaponClassification
{
    Pistol,
    Rifle,
    Grenade,
    Shotgun,
    RPG,
    Remote_Explosive
};



#if UNITY_EDITOR
[CustomEditor(typeof(GunData))]
public class WeaponDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GunData weaponData = (GunData)target;

        if (weaponData.FireType == FireType.Full_Auto)
        {
            weaponData.FireRateInSeconds =
                EditorGUILayout.FloatField("Fire Rate In Seconds ",
                weaponData.FireRateInSeconds);
        }
    }
}
#endif