using UnityEngine.Audio;
using UnityEngine;

//This class represents data for a single score (piece of music, all the elements of the music are in the array).
[CreateAssetMenu(fileName = "InteractiveMusicData", menuName = "Game/Music/Interactive Music Data")]
public class InteractiveMusicData : ScriptableObject
{
    public string SoundtrackName;

    //All parts of the song.
    public MusicElement[] musicElements;

    private void OnValidate()
    {
        //Make sure all the elements have 3 intensity setting.

        if (musicElements.Length > 0)
        {
            for (int i = 0; i < musicElements.Length; i++)
            {
                if (musicElements[i].intensitySettings.Length <= 0)
                {
                    musicElements[i].intensitySettings = new IntensitySettings[1];
                }
            }
        }
    }
}

[System.Serializable]
public struct MusicElement
{
    //this is used like an ID.
    public string Name;

    //the actual audio clip that will be played.
    public AudioClip audioClip;

    //Each element has its own values at set intensities.
    public IntensitySettings[] intensitySettings;

    public IntensitySettings GetIntensitySetting(MusicIntensity requestedIntensitySetting)
    {
        foreach (IntensitySettings setting in intensitySettings)
        {
            if (setting.musicIntensity == requestedIntensitySetting)
            {
                return setting;
            }
        }

        Debug.Log("Could not find the requested intensity settings '" + requestedIntensitySetting + "' on " + Name);

        return null;
    }
}

public enum MusicIntensity
{
    LOW,
    MEDIUM,
    HIGH
};


[System.Serializable]
public class IntensitySettings
{

    public MusicIntensity musicIntensity;
    [Range(0,1)] public float targetVolume;
    [Min(0.05F)] public float fadeTime;

    public IntensitySettings()
    {
        musicIntensity = MusicIntensity.LOW;
        targetVolume = 1;
        fadeTime = 1;
    }
}