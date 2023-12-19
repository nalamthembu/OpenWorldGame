using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BodyKit", menuName = "Game/Vehicle Parts/Body Kit")]
public class VehicleBodyKitScriptable : ScriptableObject
{
    public string vehicleName;
    public Bonnet[] bonnets;
    public Fender[] fenders;
    public Frontbumper[] frontBumpers;
    public RearBumper[] rearBumpers;
    public Rollcage[] rollCages;
    public Roofscoop[] roofScoops;
    public Sideskirt[] sideSkirts;
    public Spoiler[] spoilers;
    public RimScriptable[] rims;
}

#region ENUMS

public enum VehiclePart
{
    Bonnet,
    Fender,
    FrontBumper,
    RearBumper,
    Rollcage,
    Roofscoop,
    SideSkirt,
    Spoiler
}

#endregion

#region STRUCTS
[Serializable]
public struct Bonnet
{
    public string name;
    public GameObject mesh; //Prefab
}

[Serializable]
public struct Fender
{
    public string name;
    public GameObject mesh;
    public GameObject[] additionalMeshes;
}

[Serializable]
public struct Frontbumper
{
    public string name;
    public GameObject mesh;
}

[Serializable]
public struct RearBumper
{
    public string name;
    public GameObject mesh;
}

[Serializable]
public struct Rollcage
{
    public string name;
    public GameObject mesh;
}

[Serializable]
public struct Roofscoop
{
    public string name;
    public GameObject mesh;
}

[Serializable]
public struct Sideskirt
{
    public string name;
    public GameObject mesh;
}

[Serializable]
public struct Spoiler
{
    public string name;
    public GameObject mesh;
    public Vector3 location;
}
#endregion