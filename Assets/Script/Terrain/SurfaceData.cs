using UnityEngine;

[CreateAssetMenu(fileName = "SurfaceData", menuName = "Game/Terrain/Surface Data")]

public class SurfaceData : ScriptableObject
{
    public Surface[] surfaces;

    private void OnValidate()
    {
        if (surfaces.Length <= 0)
        {
            surfaces = new Surface[1]
            {
                new Surface()
                {
                    surfaceID = "DEFAULT_SURFACE",
                    walkSoundID = "DEFAULT_WALK_FOOTSTEPS",
                    runSoundID = "DEFAULT_RUN_FOOTSTEPS",
                    jumpStartSoundID = "DEFAULT_JUMPSTART_FOOTSTEPS",
                    jumpEndSoundID = "DEFAULT_JUMPEND_FOOTSTEPS"
                }
            };
        }
    }

    public bool TryGetSurface(string surfaceID, out Surface surface)
    {
        for (int i = 0; i < surfaceID.Length; i++)
        {
            if (surfaces[i].surfaceID == surfaceID)
            {
                surface = surfaces[i];

                return true;
            }
        }

        Debug.LogError("Could not find requested surfaceID : " + surfaceID);

        surface = default;

        return false;
    }
}

[System.Serializable]
public struct Surface
{
    public string surfaceID;
    public Texture2D[] associatedTextures;
    public string walkSoundID;
    public string runSoundID;
    public string jumpStartSoundID;
    public string jumpEndSoundID;
}