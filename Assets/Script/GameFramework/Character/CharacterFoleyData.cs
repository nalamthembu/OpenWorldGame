using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "CharacterFoley", menuName = "Game/Sound/Character Foley")]
public class CharacterFoleyData : ScriptableObject
{
    [SerializeField] FootstepSound[] m_FootStepSounds;

    //OTHER MOVEMENT FOLEY

    [SerializeField] AudioMixerGroup m_FoleyMixerGroup;

    public AudioMixerGroup GetAudioMixerGroup() => m_FoleyMixerGroup;

    public AudioClip GetFootstepSound(SurfaceType surfaceType, PaceType paceType)
    {
        foreach (FootstepSound footstep in m_FootStepSounds)
        {
            if (footstep.SurfaceType == surfaceType)
            {
                return footstep.GetRandomClip(paceType);
            }
        }

        Debug.Log($"Could not find footstep sound assigned to surface {surfaceType}");

        return null;
    }
}

public enum SurfaceType
{
    Generic,
    Grass,
    Gravel,
    Leaves,
    Metal,
    Mud,
    Rock,
    Sand,
    Snow,
    Tile,
    Water,
    Wood
}

public enum PaceType
{
    Walk,
    Run,
    StartJump,
    LandJump
}

[System.Serializable]
public class FootstepSound
{
    [SerializeField] string m_Name;
    [SerializeField] AudioClip[] m_WalkClips;
    [SerializeField] AudioClip[] m_RunClips;
    [SerializeField] AudioClip[] m_StartJumpClips;
    [SerializeField] AudioClip[] m_LandJumpClips;
    [SerializeField] SurfaceType m_SurfaceType;

    public AudioClip GetRandomClip(PaceType pace)
    {
        return pace switch
        {
            PaceType.Walk => m_WalkClips[Random.Range(0, m_WalkClips.Length)],
            PaceType.Run => m_RunClips[Random.Range(0, m_RunClips.Length)],
            PaceType.LandJump => m_LandJumpClips[Random.Range(0, m_LandJumpClips.Length)],
            PaceType.StartJump => m_StartJumpClips[Random.Range(0, m_StartJumpClips.Length)],
            _ => null,
        };
    }

    public SurfaceType SurfaceType { get { return m_SurfaceType; } }
}