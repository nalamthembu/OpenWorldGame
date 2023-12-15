using System.Collections.Generic;
using UnityEngine;

public class AmbienceSoundManager : MonoBehaviour
{
    [SerializeField] AmbienceScriptable ambienceLib;
    [SerializeField] string initialAmbience;

    Dictionary<string, Ambience> ambienceDictionary = new();

    public static AmbienceSoundManager instance;

    private Ambience currentAmbience;

    private float ambiencePlayTimer;

    private float maxTime;

    private float lastClipLength;

    private float secondaryTimer;

    private AudioSource loopSource;

    bool Initialised = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        loopSource = gameObject.AddComponent<AudioSource>();

        for (int i = 0; i < ambienceLib.ambiences.Length; i++)
            ambienceDictionary.Add(ambienceLib.ambiences[i].name, ambienceLib.ambiences[i]);

        SetCurrentAmbience(initialAmbience);

        Initialised = true;
    }

    private void Start()
    {
        PlayAllLoops();
    }

    private void PlayAllLoops()
    {
        //Play all loops.
        foreach (string s in currentAmbience.ambienceLoops)
        {
            if (SoundManager.Instance is null)
            {
                Debug.LogError("SOUND_MANAGER IS NULL");
                return;
            }

            SoundManager.Instance.PlayInGameSound(s, loopSource, true, false, true);
        }
    }

    private void Update()
    {
        if (secondaryTimer < lastClipLength)
        {
            secondaryTimer += Time.deltaTime;
            return;
        }

        ambiencePlayTimer += Time.deltaTime;

        if (ambiencePlayTimer >= maxTime && currentAmbience.clipNames.Length > 0)
        {
            ambiencePlayTimer = 0;

            secondaryTimer = 0;

            lastClipLength = 5.0F;

            maxTime = Random.Range(currentAmbience.minRandomTime, currentAmbience.maxRandomTime);
        }
    }

    public void SetCurrentAmbience(string ambName)
    {
        if (ambienceDictionary.TryGetValue(ambName, out Ambience ambience))
            currentAmbience = ambience;
        else
        {
            Debug.Log("Could not find requested ambience : " + ambName);
            return;
        }

        print("playing ambience : " + ambName);

        if (Initialised)
        {
            PlayAllLoops();
        }
    }

}
