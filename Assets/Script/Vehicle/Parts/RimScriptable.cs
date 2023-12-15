using UnityEngine;

[CreateAssetMenu(fileName = "Rim", menuName = "Game/Vehicle Parts/Rim")]
public class RimScriptable : ScriptableObject
{
    new public string name;
    public GameObject mesh;

    //TO-DO : Add Weight to influence the over all weight of the vehicle.
}