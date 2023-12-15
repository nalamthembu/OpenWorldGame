using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Vehicle_Library", menuName = "Game/Vehicle/Library")]
public class VehicleSpawnerScriptable : ScriptableObject
{
    public AVehicle[] vehicles;
}

//A - Asset Vehicle, not real time, stored data.
[Serializable]
public struct AVehicle
{
    public string name;
    public GameObject vehicle;
}