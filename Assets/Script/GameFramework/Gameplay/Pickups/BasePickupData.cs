using UnityEngine;

[CreateAssetMenu(fileName = "PickupData", menuName = "Game/Pickups/Pickup Data")]
public class BasePickupData : ScriptableObject
{
    [SerializeField] protected string m_PickupName;

    public string GetPickupName() => m_PickupName;
}