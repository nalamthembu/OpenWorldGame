using MyBox;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioAmbienceData", menuName= "Game/Sound/Ambience Data")]
public class AudioAmbienceData : ScriptableObject
{
    [SerializeField] string AmbienceName;
    [SerializeField] AudioClip[] MainLoopClips;
    [SerializeField] float m_MainLoopVolume;
    [SerializeField] bool m_UsesOneshots;
    [ConditionalField("m_UsesOneshots", false, true)]
    [SerializeField] AudioClip[] m_OneShotClips;
    [ConditionalField("m_UsesOneshots", false, true)]
    [SerializeField] Vector2 m_MinMaxRandomOneShotTriggerTime = new(5, 30);

    public float MainLoopVolume => m_MainLoopVolume;

    public bool UsesOneshots => m_UsesOneshots;

    public float GetRandomOneshotTriggerTime() => Random.Range(m_MinMaxRandomOneShotTriggerTime.x, m_MinMaxRandomOneShotTriggerTime.y);

    public AudioClip GetRandomOneShot()
    {
        if (!m_UsesOneshots)
        {
            Debug.LogError($"This ambience ({AmbienceName}) does not use oneshots");
            return null;
        }

        return m_OneShotClips[Random.Range(0, m_OneShotClips.Length)];
    }

    public AudioClip[] GetMainLoops() => MainLoopClips;
}