using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "GunData", menuName = "Game/Weapon/Weapon Data/Gun")]
public class GunData : BaseWeaponData
{
    public int MaxClip;
    public FireType FireType;
    public WeaponClassification WeaponClass;
    public GameObject BulletPrefab;
    public string fireShotAudioID, fireShotOutDoorTailID, fireShotIndoorTailID;
    [HideInInspector] [Min(0.1f)] public float FireRateInSeconds;
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