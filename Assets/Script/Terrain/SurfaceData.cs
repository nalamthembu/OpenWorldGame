using UnityEngine;

[CreateAssetMenu(fileName = "SurfaceData", menuName = "Game/Terrain/Surface Data")]

public class SurfaceData : ScriptableObject
{
    
}

[System.Serializable]
public struct Surface
{
    public string name;
    public Texture2D[] associatedTextures;
    public string associatedSoundID;
}