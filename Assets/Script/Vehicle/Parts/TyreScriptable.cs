using UnityEngine;

[CreateAssetMenu(fileName = "Tyre", menuName = "Game/Vehicle Parts/Tyre")]
public class TyreScriptable : ScriptableObject
{
    new public string name;
    public GameObject mesh;

    //TO-DO : Add Weight to influence the over all weight of the vehicle.
}