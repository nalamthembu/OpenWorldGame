using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "SpeechData", menuName = "Game/Character Speech/Character Speech Data")]
public class CharacterSpeechData : ScriptableObject
{
    [SerializeField] PainWaveTable[] painWaveTable;
    [SerializeField] Vocalisation[] vocalisations;
    [SerializeField] AudioMixerGroup mixergroup;

    public AudioMixerGroup GetMixerGroup() { return mixergroup; }

    public PainWaveTable GetPainWaveTable(PainLevel level)
    {
        foreach (var p in painWaveTable)
        {
            if (p.GetPainLevel == level)
                return p;
        }

        return null;
    }

    public Vocalisation GetVocalisation(VocalisationType vocalisationType)
    {
        foreach(var v in vocalisations)
        {
            if (v.VocalisationType == vocalisationType)
                return v;
        }

        Debug.LogError($"Could not find sound from vocalisation type {vocalisationType}");

        return null;
    }
}

public enum VocalisationType
{
    HighFall,
    LowFall,
};

public enum PainLevel
{
    Low,
    Medium,
    High,
    Death,
    Death_Headshot
};

public enum FallHeight
{
    High,
    Very_High
};

[System.Serializable]
public class Vocalisation
{
    [SerializeField] string Name;
    [SerializeField] VocalisationType Type;
    [SerializeField] AudioClip[] audioClips;

    public VocalisationType VocalisationType { get { return Type; } }
    public AudioClip GetRandomVocalisationClip() => audioClips[Random.Range(0, audioClips.Length)];
}

[System.Serializable]
public class PainWaveTable
{
    [SerializeField] string Name;
    [SerializeField] PainLevel Level;
    [SerializeField] AudioClip[] audioClips;

    public PainLevel GetPainLevel => Level;

    public AudioClip GetRandomPainClip() => audioClips[Random.Range(0, audioClips.Length)];
}

//TODO : Mission Speech


//TODO : Reaction Speech