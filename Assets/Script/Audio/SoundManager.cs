using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    [SerializeField] InGameSoundScriptable m_InGameScriptable;
    [SerializeField] FrontendSoundScriptable m_FEScriptable;
    [SerializeField] MixerGroupScriptable m_MixerScriptable;

    private readonly Dictionary<string, Sound> m_InGameSoundDict = new();
    private readonly Dictionary<string, FESound> m_FESoundDict = new();
    private readonly Dictionary<SoundType, Mixer> m_MixerDict = new();

    private AudioSource m_FESource;
    
    //Allow the methods to be accessed from other scripts.
    public static SoundManager Instance { get; private set; }

    private void Awake()
    {
        //Singleton
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitialiseDictionaries();
        InitialiseFrontendSoundSource();
    }

    private void InitialiseFrontendSoundSource()
    {
        m_FESource = gameObject.AddComponent<AudioSource>();

        m_FESource.spatialBlend = 0;

        m_FESource.playOnAwake = false;

        m_FESource.outputAudioMixerGroup = m_MixerDict[SoundType.FRONTEND].mixerGroup;
    }

    private void InitialiseDictionaries()
    {
        foreach (Sound ingameSound in m_InGameScriptable.sounds)
        {
            m_InGameSoundDict.Add(ingameSound.name, ingameSound);
        }

        foreach (FESound feSound in m_FEScriptable.sounds)
        {
            m_FESoundDict.Add(feSound.name, feSound);
        }

        foreach(Mixer mixer in m_MixerScriptable.mixers)
        {
            m_MixerDict.Add(mixer.type, mixer);
        }
    }

    //Plays in-game Sounds in 3D Space
    public void PlayInGameSound(string name, Vector3 position, bool randomisePitch)
    {
        if (m_InGameSoundDict.TryGetValue(name, out Sound sound))
        {
            switch(sound.type)
            {
                case SoundType.SPEECH:
                case SoundType.AMBIENCE:
                case SoundType.ONESHOT_AMBIENCE:
                case SoundType.SFX:

                    //Using an object pooling system, can improve performance by reusing objects
                    //instead of constantly creating and destroying them.
                    AudioSource source = ObjectPoolManager.Instance.GetPool("DynamicAudioSource").Value.GetGameObject().GetComponent<AudioSource>();

                    source.transform.position = position;

                    source.playOnAwake = false;

                    source.pitch = randomisePitch ? Random.Range(1, 1.25F) : 1;

                    source.clip = sound.GetRandomClip();

                    //Make sure the sound is in 3D Space
                    source.spatialBlend = 1;

                    //Make sure the route the sound to the correct mixer group.
                    source.outputAudioMixerGroup = m_MixerDict[sound.type].mixerGroup;

                    source.Play();

                    ObjectPoolManager.Instance.ReturnGameObject(source.gameObject, source.clip.length + 1);

                    break;
            }
        }
        else
        {
            Debug.LogError("Could not find sound with name : " + name 
                + ", please make sure the spelling is correct or that it exists");
        }
    }

    //Play in-game Audio in 3D Space From a source (useful for vehicles, characters, etc.)
    public void PlayInGameSound(string name, AudioSource source, bool loop, bool randomisePitch = false, bool TwoDSpace = false)
    {
        if (m_InGameSoundDict.TryGetValue(name, out Sound sound))
        {
            switch (sound.type)
            {
                case SoundType.SPEECH:
                case SoundType.AMBIENCE:
                case SoundType.SFX:

                    source.pitch = randomisePitch ? Random.Range(1, 1.25F) : 1;

                    source.clip = sound.GetRandomClip();

                    //Make sure the route the sound to the correct mixer group.
                    source.outputAudioMixerGroup = m_MixerDict[sound.type].mixerGroup;

                    //Make sure the sound is in 3D Space
                    source.spatialBlend = TwoDSpace ? 0 : 1;

                    //Useful for ambient loops (City noise, Music, etc.)
                    source.loop = loop;

                    source.Play();

                    break;
            }
        }
        else
        {
            Debug.LogError("Could not find sound with name : " + name
                + ", please make sure the spelling is correct or that it exists");
        }
    }

    //Plays frontend sound out of the Frontend AudioSource (UI/Menu Sound essentially)
    public void PlayFESound(string name)
    {
        if (m_FESoundDict.TryGetValue(name, out FESound sound))
        {
            m_FESource.clip = sound.GetRandomClip();

            //Mixer group is already set when FESource was initialised (Check InitialiseFrontendSoundSource())

            m_FESource.Play();
        }
        else
        {
            Debug.LogError("Could not find sound with name : " + name
                + ", please make sure the spelling is correct or that it exists");
        }
    }
}