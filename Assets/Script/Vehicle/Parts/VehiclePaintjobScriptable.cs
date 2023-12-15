using UnityEngine;

[CreateAssetMenu(fileName = "PaintjobScriptable", menuName = "Game/Vehicle Parts/Paint Job")]
public class VehiclePaintjobScriptable : ScriptableObject
{
    public PaintJob[] paintJobs;
}

[System.Serializable]
public struct PaintJob
{
    public string name;
    public Texture2D texture;
}