using UnityEngine;

[RequireComponent(typeof(BaseCharacter))]
public class CharacterFoleyComponent : MonoBehaviour, IEntityComponent
{
    [SerializeField] CharacterFoleyData m_FoleyData;

    [SerializeField] LayerMask m_FootInteractiveLayers = -1;

    AudioSource m_FootstepSource;

    BaseCharacterStateMachine m_CharacterStateMachine;

    private void Awake()
    {
        if (m_FoleyData == null)
        {
            Debug.LogError("Foley Data is not assigned!");
            enabled = false;
            return;
        }

        InitialiseAudioSources();

        m_CharacterStateMachine = GetComponent<BaseCharacterStateMachine>();
    }

    private void InitialiseAudioSources()
    {
        m_FootstepSource = gameObject.AddComponent<AudioSource>();
        m_FootstepSource.loop = false;
        m_FootstepSource.playOnAwake = false;
        m_FootstepSource.outputAudioMixerGroup = m_FoleyData.GetAudioMixerGroup();
        m_FootstepSource.volume = 1f;
        m_FootstepSource.minDistance = 6.0F;
    }

    public void PlayFootstepSound()
    {
        SurfaceType surface = SurfaceType.Generic;
        PaceType pace = default;

        //Get Correct Pacing
        if (m_CharacterStateMachine != null)
        {
            if (m_CharacterStateMachine.IsRunning)
                pace = PaceType.Run;
            else
                pace = PaceType.Walk;
        }

        Vector3 lineStart = transform.position + Vector3.up * 0.1F;
        Vector3 lineEnd = transform.position + -Vector3.up * 0.1F;
        Debug.DrawLine(lineStart, lineEnd, Color.yellow);

        if (Physics.Linecast(lineStart, lineEnd, out var hitInfo, m_FootInteractiveLayers, QueryTriggerInteraction.Ignore))
        {
            //Generic for now
            m_FootstepSource.clip = m_FoleyData.GetFootstepSound(surface, pace);
            m_FootstepSource.pitch = Random.Range(0.95f, 1.15f);
            m_FootstepSource.Play();
        }
    }
}