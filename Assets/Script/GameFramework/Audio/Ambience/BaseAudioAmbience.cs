using UnityEngine;
using System.Collections;

public class BaseAudioAmbience : MonoBehaviour
{
    [SerializeField] protected AudioAmbienceData m_AmbienceData;
    protected AudioSource[] m_AudioSources;
    private float m_OneshotTimer = 0;
    private float m_CurrentTriggerTime = 0; //how long we wait before triggering a one shot.
    [SerializeField] float m_MaxOneshotDistance = 50;
    [SerializeField] protected float m_TransitionTime = 0.5F; //takes x seconds to transition to ambience.

    protected bool m_IsActive;

    protected virtual void Start() => InitialiseAudioSource();
    
    protected virtual void InitialiseAudioSource()
    {
        int length = m_AmbienceData.GetMainLoops().Length;

        int index = 0;

        m_AudioSources = new AudioSource[length];

        foreach (AudioClip clip in m_AmbienceData.GetMainLoops())
        {
            m_AudioSources[index] = gameObject.AddComponent<AudioSource>();
            m_AudioSources[index].spatialBlend = 0;
            m_AudioSources[index].playOnAwake = false;
            m_AudioSources[index].loop = true;
            m_AudioSources[index].clip = clip;

            if (SoundManager.Instance && SoundManager.Instance.TryGetMixerGroup(SoundType.AMBIENCE, out var outMixerGroup))
                m_AudioSources[index].outputAudioMixerGroup = outMixerGroup;
        }
    }

    protected virtual void OnDrawGizmos()
    {
        if (m_AmbienceData && m_AmbienceData.UsesOneshots)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, m_MaxOneshotDistance);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (!m_IsActive)
            return;

        if (m_AmbienceData == null)
            return;

        if (m_AmbienceData.UsesOneshots)
            HandleOneshots();
    }

    protected IEnumerator FadeIn()
    {
        if (m_AmbienceData == null)
            yield return null;

        float referenceFloat = 0;

        float targetVolume = m_AmbienceData.MainLoopVolume;

        foreach (AudioSource audioSources in m_AudioSources)
        { 
            audioSources.enabled = true;

            audioSources.Play();

            while (audioSources.volume < targetVolume)
            {
                audioSources.volume = Mathf.SmoothDamp(audioSources.volume, targetVolume, ref referenceFloat, m_TransitionTime);

                if (audioSources.volume - Time.deltaTime <= targetVolume)
                    audioSources.volume = targetVolume;

                yield return new WaitForEndOfFrame();
            }
        }

        m_IsActive = true;
    }

    protected IEnumerator FadeOut()
    {
        if (m_AmbienceData == null)
            yield return null;

        float referenceFloat = 0;

        float targetVolume = 0;

        foreach (AudioSource audioSources in m_AudioSources)
        {
            while (audioSources.volume > targetVolume)
            {
                audioSources.volume = Mathf.SmoothDamp(audioSources.volume, targetVolume, ref referenceFloat, m_TransitionTime);

                if (audioSources.volume + Time.deltaTime >= targetVolume)
                    audioSources.volume = targetVolume;

                yield return new WaitForEndOfFrame();
            }

            audioSources.Stop();

            audioSources.enabled = false;
        }

        m_IsActive = false;
    }

    private void HandleOneshots()
    {
        if (m_AmbienceData != null && m_AmbienceData.UsesOneshots)
        {
            if (m_OneshotTimer >= m_CurrentTriggerTime)
            {
                PlayOneshot();

                m_OneshotTimer = 0;
                m_CurrentTriggerTime = m_AmbienceData.GetRandomOneshotTriggerTime();
            }
            else
            {
                m_OneshotTimer += Time.deltaTime;
            }
        }
    }

    private void PlayOneshot()
    {
        if (ThirdPersonCamera.Instance != null)
        {
            if (SoundManager.Instance)
            {
                Vector3 cameraPosition = ThirdPersonCamera.Instance.transform.position;

                Vector3 finalRandomPosition = cameraPosition + Random.insideUnitSphere * m_MaxOneshotDistance;

                SoundManager.Instance.PlayClip(m_AmbienceData.GetRandomOneShot(), finalRandomPosition, true, 5, SoundType.AMBIENCE);
            }
        }
    }
}