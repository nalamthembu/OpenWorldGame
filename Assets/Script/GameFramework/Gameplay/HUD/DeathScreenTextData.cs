using UnityEngine;

[CreateAssetMenu(fileName = "DeadScreenText", menuName = "Game/HUD/Death Screen Text")]
public class DeathScreenTextData : ScriptableObject
{
    [Tooltip("The death screen will choose a random one of these to display")]
    public string[] Text;
}
