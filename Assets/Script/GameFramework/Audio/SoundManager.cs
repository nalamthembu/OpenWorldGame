using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    [SerializeField] InGameSoundScriptable m_InGameScriptable;
    [SerializeField] FrontendSoundScriptable m_FEScriptable;
    [SerializeField] MixerGroupScriptable m_MixerScriptable;

    private readonly Dictionary<string, Sound> m_InGameSoundDict = new();
    private readonly Dictionary<string, FESound> m_FESoundDict = new();
    private readonly Dictionary<SoundType, Mixer> m_MixerDict = new();
    private readonly Dictionary<string, MixerState> m_MixerStates = new();

    private AudioSource m_FESource;
    
    //Allow the methods to be accessed from other scripts.
    public static SoundManager Instance { get; private set; }
    public string CurrentMixerStateID { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitialiseDictionaries();
        InitialiseFrontendSoundSource();
    }

    public bool TryGetMixerGroup(SoundType mixerType, out AudioMixerGroup outMixerGroup)
    {
        if (m_MixerDict.TryGetValue(mixerType, out var mixerValue))
        {
            outMixerGroup = mixerValue.mixerGroup;
            return true;
        }

        Debug.Log($"Could not find mixer of type {mixerType}");
        outMixerGroup = null;
        return false;
    }

    public void TransitionToMixerState(string ID, float time)
    {
        if (m_MixerStates.TryGetValue(ID, out var mixerState))
        {
            mixerState.mixerSnapshot.TransitionTo(time);

            CurrentMixerStateID = ID;
        }
        else
            Debug.LogError($"The mixer state : {ID}  does not exist");
    }

    private void InitialiseFrontendSoundSource()
    {
        m_FESource = gameObject.AddComponent<AudioSource>();

        m_FESource.spatialBlend = 0;

        m_FESource.playOnAwake = false;

        m_FESource.bypassReverbZones = true;

        m_FESource.outputAudioMixerGroup = m_MixerDict[SoundType.FRONTEND].mixerGroup;
    }

    private void InitialiseDictionaries()
    {
        foreach (Sound ingameSound in m_InGameScriptable.sounds)
            m_InGameSoundDict.Add(ingameSound.soundID, ingameSound);

        foreach (FESound feSound in m_FEScriptable.sounds)
            m_FESoundDict.Add(feSound.soundID, feSound);

        foreach (Mixer mixer in m_MixerScriptable.mixers)
            m_MixerDict.Add(mixer.type, mixer);

        foreach (MixerState mixerState in m_MixerScriptable.mixerStates)
            m_MixerStates.Add(mixerState.ID, mixerState);
    }

    /// <summary>
    /// Plays in-game Sounds in 3D Space
    /// </summary>
    public void PlayInGameSound(string soundID, Vector3 position, bool randomisePitch, float minAudibleDist = 1)
    {
        if (m_InGameSoundDict.TryGetValue(soundID, out Sound sound))
        {
            switch(sound.type)
            {
                case SoundType.SPEECH:
                case SoundType.AMBIENCE:
                case SoundType.ONESHOT_AMBIENCE:
                case SoundType.SFX:

                    //Using an object pooling system, can improve performance by reusing objects
                    //instead of constantly creating and destroying them.

                    if (ObjectPoolManager.Instance.TryGetPool("DynamicAudioSource", out Pool pool))
                    {
                        if (pool.TryGetGameObject(out var poolObject))
                        {
                            AudioSource source = poolObject.GetComponent<AudioSource>();

                            source.transform.position = position;

                            source.playOnAwake = false;

                            source.pitch = randomisePitch ? Random.Range(1, 1.25F) : 1;

                            AudioClip randomclip = sound.GetRandomClip();

                            //Make sure to not reassign a clip if it was already assigned before
                            if (source.clip != randomclip)
                                source.clip = randomclip;


                            //Make sure the sound is in 3D Space
                            source.spatialBlend = 1;

                            source.minDistance = minAudibleDist;

                            if (minAudibleDist >= source.maxDistance)
                            {
                                source.maxDistance = minAudibleDist * 2;
                            }

                            //Make sure the route the sound to the correct mixer group and
                            //don't reassign it the source if it was already outputting to the correct mixer
                            if (source.outputAudioMixerGroup != m_MixerDict[sound.type].mixerGroup)
                                source.outputAudioMixerGroup = m_MixerDict[sound.type].mixerGroup;

                            source.Play();

                            ObjectPoolManager.Instance.ReturnGameObject(source.gameObject, source.clip.length + 1);
                        }
                    }

                    break;
            }
        }
        else
        {
            Debug.LogError($"Could not find sound with name : {soundID}, please make sure the spelling is correct or that it exists");
        }
    }

    /// <summary>
    /// Plays an audio clip in 3D Space
    /// </summary>
    public void PlayClip(AudioClip clip, Vector3 position, bool randomisePitch, float minAudibleDist = 1, SoundType soundType = SoundType.SFX)
    {
        switch (soundType)
        {
            case SoundType.SPEECH:
            case SoundType.AMBIENCE:
            case SoundType.ONESHOT_AMBIENCE:
            case SoundType.SFX:

                //Using an object pooling system, can improve performance by reusing objects
                //instead of constantly creating and destroying them.

                if (ObjectPoolManager.Instance.TryGetPool("DynamicAudioSource", out Pool pool))
                {
                    if (pool.TryGetGameObject(out var poolObject))
                    {
                        AudioSource source = poolObject.GetComponent<AudioSource>();

                        source.transform.position = position;

                        source.playOnAwake = false;

                        source.pitch = randomisePitch ? Random.Range(1, 1.25F) : 1;

                        source.clip = clip;

                        //Make sure the sound is in 3D Space
                        source.spatialBlend = 1;

                        source.minDistance = minAudibleDist;

                        if (minAudibleDist >= source.maxDistance)
                        {
                            source.maxDistance = minAudibleDist * 2;
                        }

                        //Make sure the route the sound to the correct mixer group and
                        //don't reassign it the source if it was already outputting to the correct mixer
                        if (source.outputAudioMixerGroup != m_MixerDict[soundType].mixerGroup)
                            source.outputAudioMixerGroup = m_MixerDict[soundType].mixerGroup;

                        source.Play();

                        ObjectPoolManager.Instance.ReturnGameObject(source.gameObject, source.clip.length + 1);
                    }
                }

                break;
        }
    }

    /// <summary>
    /// Play in-game Audio in 3D Space From a source (useful for vehicles, characters, etc.)
    /// </summary>
    public void PlayInGameSound(string soundID, AudioSource source, bool loop, bool randomisePitch = false, bool TwoDSpace = false, float minAudibleDist = 1)
    {
        if (m_InGameSoundDict.TryGetValue(soundID, out Sound sound))
        {
            switch (sound.type)
            {
                case SoundType.SPEECH:
                case SoundType.AMBIENCE:
                case SoundType.SFX:

                    source.pitch = randomisePitch ? Random.Range(1, 1.25F) : 1;

                    AudioClip randomclip = sound.GetRandomClip();

                    //Make sure to not reassign a clip if it was already assigned before
                    if (source.clip != randomclip)
                        source.clip = randomclip;

                    //Make sure the route the sound to the correct mixer group and
                    //don't reassign it the source if it was already outputting to the correct mixer
                    if (source.outputAudioMixerGroup != m_MixerDict[sound.type].mixerGroup)
                        source.outputAudioMixerGroup = m_MixerDict[sound.type].mixerGroup;

                    //Make sure the sound is in 3D Space
                    source.spatialBlend = TwoDSpace ? 0 : 1;

                    source.minDistance = minAudibleDist;

                    if (minAudibleDist >= source.maxDistance)
                    {
                        source.maxDistance = minAudibleDist * 2;
                    }

                    //Useful for ambient loops (City noise, Music, etc.)
                    source.loop = loop;

                    source.Play();

                    break;
            }
        }
        else
        {
            Debug.LogError("Could not find sound with name : " + soundID
                + ", please make sure the spelling is correct or that it exists");
        }
    }

    /// <summary>
    /// Play in-game Audio in 3D Space From a source (useful for vehicles, characters, etc.)
    /// </summary>
    public void PlayInGameSound(string soundID, AudioSource source, bool loop, out float clipLength, bool randomisePitch = false, bool TwoDSpace = false)
    {
        clipLength = 0;

        if (m_InGameSoundDict.TryGetValue(soundID, out Sound sound))
        {
            switch (sound.type)
            {
                case SoundType.SPEECH:
                case SoundType.AMBIENCE:
                case SoundType.SFX:

                    source.pitch = randomisePitch ? Random.Range(1, 1.25F) : 1;

                    AudioClip randomclip = sound.GetRandomClip();

                    //Make sure to not reassign a clip if it was already assigned before
                    if (source.clip != randomclip)
                        source.clip = randomclip;

                    //Make sure the route the sound to the correct mixer group and
                    //don't reassign it the source if it was already outputting to the correct mixer
                    if (source.outputAudioMixerGroup != m_MixerDict[sound.type].mixerGroup)
                        source.outputAudioMixerGroup = m_MixerDict[sound.type].mixerGroup;

                    //Make sure the sound is in 3D Space
                    source.spatialBlend = TwoDSpace ? 0 : 1;

                    //Useful for ambient loops (City noise, Music, etc.)
                    source.loop = loop;

                    clipLength = source.clip.length;

                    source.Play();

                    break;
            }
        }
        else
        {
            Debug.LogError("Could not find sound with name : " + soundID
                + ", please make sure the spelling is correct or that it exists");
        }
    }

    public void PlayInGameSound(string soundID, int soundIndex, AudioSource source, bool loop, out float clipLength, bool randomisePitch = false, bool TwoDSpace = false)
    {
        clipLength = 0;

        if (m_InGameSoundDict.TryGetValue(soundID, out Sound sound))
        {
            switch (sound.type)
            {
                case SoundType.SPEECH:
                case SoundType.AMBIENCE:
                case SoundType.SFX:

                    source.pitch = randomisePitch ? Random.Range(1, 1.25F) : 1;

                    source.clip = sound.clips[soundIndex];

                    //Make sure the route the sound to the correct mixer group and
                    //don't reassign it the source if it was already outputting to the correct mixer
                    if (source.outputAudioMixerGroup != m_MixerDict[sound.type].mixerGroup)
                        source.outputAudioMixerGroup = m_MixerDict[sound.type].mixerGroup;

                    //Make sure the sound is in 3D Space
                    source.spatialBlend = TwoDSpace ? 0 : 1;

                    //Useful for ambient loops (City noise, Music, etc.)
                    source.loop = loop;

                    clipLength = source.clip.length;

                    source.Play();

                    break;
            }
        }
        else
        {
            Debug.LogError("Could not find sound with name : " + soundID
                + ", please make sure the spelling is correct or that it exists");
        }
    }
    public bool TryGetInGameSound(string soundID, out Sound sound)
    {
        if (m_InGameSoundDict.TryGetValue(soundID, out sound))
        {
            return true;
        }

        Debug.LogError($"Couldn't find the In game sound {soundID}.");

        return false;
    }
    public bool TryGetInFESound(string soundID, out FESound sound)
    {
        if (m_FESoundDict.TryGetValue(soundID, out sound))
        {
            return true;
        }

        Debug.LogError("Couldn't find the FRONTEND sound " + soundID);

        return false;
    }

    /// <summary>
    /// Plays frontend sound out of the Frontend AudioSource (UI/Menu Sound essentially)
    /// </summary>
    public void PlayFESound(string soundID, float volume = 1)
    {
        if (m_FESoundDict.TryGetValue(soundID, out FESound sound))
        {
            AudioClip randomclip = sound.GetRandomClip();

            //Make sure to not reassign a clip if it was already assigned before
            if (m_FESource.clip != randomclip)
                m_FESource.clip = randomclip;

            m_FESource.volume = volume;

            //Mixer group is already set when FESource was initialised (Check InitialiseFrontendSoundSource())

            m_FESource.Play();
        }
        else
        {
            Debug.LogError("Could not find sound with name : " + soundID
                + ", please make sure the spelling is correct or that it exists");
        }
    }
}