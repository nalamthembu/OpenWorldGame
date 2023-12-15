using UnityEngine;

[CreateAssetMenu(fileName = "Vehicle Part Library", menuName = "Game/Vehicle Parts/")]
public class VehiclePartLibrary : ScriptableObject
{
    public TyreScriptable[] tyres;
    public RimScriptable[] rims;
}