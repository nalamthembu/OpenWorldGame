using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class InteractiveMusicManager : MonoBehaviour
{
    [SerializeField] InteractiveMusicData m_MusicData;

    [SerializeField] AudioMixerGroup m_MusicMixerGroup;

    [SerializeField] MusicIntensity m_CurrentIntensity;

    List<AudioSource> m_AudioSources = new();

    private bool m_IsPlaying = false;

    private bool m_SoundtrackIsStopping;

    private readonly float[] m_VolumeVelocities = new float[12];

    public static InteractiveMusicManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);


        InitialiseMusic();
    }

    private void OnValidate()
    {
        //IF THE DESIGNER CHANGES THE INTENSITY, Do that
        SetIntensity(m_CurrentIntensity);
    }

    [ContextMenu("Debug : Start Soundtrack")]
    public void StartSoundTrack()
    {
        foreach (AudioSource source in m_AudioSources)
        {
            source.Play();
        }

        SetIntensity(m_CurrentIntensity);

        m_IsPlaying = true;
    }

    [ContextMenu("Debug : Stop Soundtrack")]
    public void StopSoundTrack()
    {
        if (m_SoundtrackIsStopping)
            return;

        m_IsPlaying = false;

        StartCoroutine(StopSoundTrack_Coroutine());

        m_SoundtrackIsStopping = true;
    }

    private IEnumerator StopSoundTrack_Coroutine()
    {
        bool allSourcesAreAtZeroVolume = false;

        int numOfSourcesMuted = 0;

        while (!allSourcesAreAtZeroVolume)
        {
            foreach (AudioSource source in m_AudioSources)
            {
                if (source.volume <= 0)
                {
                    numOfSourcesMuted++;
                    continue;
                }

                source.volume -= Time.deltaTime / 10;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    [ContextMenu("Debug : Pause Soundtrack")]
    public void PauseSoundTrack()
    {
        foreach (AudioSource source in m_AudioSources)
        {
            source.Pause();
        }

        m_IsPlaying = false;
    }

    [ContextMenu("Debug : Resume Soundtrack")]
    public void ResumeSoundTrack()
    {
        foreach (AudioSource source in m_AudioSources)
        {
            source.UnPause();
        }

        m_IsPlaying = true;
    }

    public void SetSeektime(float newSeektime)
    {
        //The soundtrack elements all have to be the exact same length.

        foreach (AudioSource source in m_AudioSources)
        {
            //if the requested seek time is beyond length of the clip
            if (newSeektime > source.clip.length)
            {
                Debug.LogError("Requested seek time was longer than the actual sound clip");

                break;
            }
            else
            {
                source.time = newSeektime;
            }
        }
    }

    [ContextMenu("Debug : Reinit Soundtrack")]
    public void InitialiseMusic()
    {
        if (m_AudioSources.Count > 0)
        {
            for (int i = 0; i < m_AudioSources.Count; i++)
            {
                Destroy(m_AudioSources[i]);
            }

            m_AudioSources.Clear();
        }
        else
        {
            m_AudioSources = new();
        }

        if (m_MusicData is null)
        {
            Debug.LogError("Music Data was not assigned!");
            return;
        }

        if (m_AudioSources is null)
            Debug.LogError("Audio Sources list was not assigned!");

        if (m_MusicData.musicElements == null)
            Debug.LogError("Music data has no elements!");

        if (m_MusicMixerGroup == null)
            Debug.LogError("There is no music mixer group attached!");

        //Initialise all audio sources
        foreach (MusicElement musicElement in m_MusicData.musicElements)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();

            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0;
            source.volume = 0;
            source.bypassReverbZones = true;
            source.outputAudioMixerGroup = m_MusicMixerGroup;
            source.clip = musicElement.audioClip;
            m_AudioSources.Add(source);
        }
    }

    public void SetIntensity(MusicIntensity intensity) => m_CurrentIntensity = intensity;

    private void Update()
    {
        if (m_IsPlaying)
        {
            UpdateAudioSources();
        }
    }

    private void UpdateAudioSources()
    {
        if (m_AudioSources != null)
        {
            for(int i = 0; i < m_AudioSources.Count; i++)
            {
                IntensitySettings inten = m_MusicData.musicElements[i].GetIntensitySetting(m_CurrentIntensity);

                m_AudioSources[i].volume = Mathf.SmoothDamp(m_AudioSources[i].volume, inten.targetVolume, ref m_VolumeVelocities[i], inten.fadeTime);
            }
        }
    }
}