using UnityEngine;
using UnityEngine.Audio;

public class CarAudioController : MonoBehaviour
{
    public AudioMixerGroup outputMixerGroup; // Assign your output mixer group in the Unity Editor

    public AudioClip idleClip;
    public AudioClip lowAccelerationClip;
    public AudioClip lowDecelerationClip;
    public AudioClip highAccelerationClip;
    public AudioClip highDecelerationClip;

    private AudioSource idleSound;
    private AudioSource lowAccelerationSound;
    private AudioSource lowDecelerationSound;
    private AudioSource highAccelerationSound;
    private AudioSource highDecelerationSound;

    private VehicleEngine engine;

    public float maxRPM = 7000f; // Maximum RPM of the car engine
    public float pitchMultiplier = 2f; // Adjust this value to control the pitch variation

    private void Awake()
    {
        engine = GetComponent<VehicleEngine>();
    }

    void Start()
    {
        // Create and set up each AudioSource
        idleSound = CreateAudioSource(idleClip);
        lowAccelerationSound = CreateAudioSource(lowAccelerationClip);
        lowDecelerationSound = CreateAudioSource(lowDecelerationClip);
        highAccelerationSound = CreateAudioSource(highAccelerationClip);
        highDecelerationSound = CreateAudioSource(highDecelerationClip);

        // Set the output mixer group for each AudioSource
        SetOutputMixerGroup(idleSound);
        SetOutputMixerGroup(lowAccelerationSound);
        SetOutputMixerGroup(lowDecelerationSound);
        SetOutputMixerGroup(highAccelerationSound);
        SetOutputMixerGroup(highDecelerationSound);
    }

    void Update()
    {
        // Assume you have a variable like carRPM that holds the current RPM of the car engine
        // Update this value according to your game logic
        float carRPM = GetCarRPM(); // Replace this with your actual method to get the RPM

        // Update the audio based on RPM
        UpdateAudio(carRPM);
    }

    void UpdateAudio(float rpm)
    {
        // Normalize the RPM to a value between 0 and 1
        float normalizedRPM = Mathf.Clamp01(rpm / maxRPM);

        // Adjust the pitch based on the normalized RPM
        float pitch = 1f + (normalizedRPM * pitchMultiplier);

        // Set pitch for each audio source
        idleSound.pitch = pitch;
        lowAccelerationSound.pitch = pitch;
        lowDecelerationSound.pitch = pitch;
        highAccelerationSound.pitch = pitch;
        highDecelerationSound.pitch = pitch;

        // Crossfade volumes based on normalized RPM
        idleSound.volume = 1f - normalizedRPM;
        lowAccelerationSound.volume = Mathf.Clamp01(1f - normalizedRPM * 2f);
        lowDecelerationSound.volume = Mathf.Clamp01(normalizedRPM * 2f);
        highAccelerationSound.volume = Mathf.Clamp01(normalizedRPM * 2f);
        highDecelerationSound.volume = 1f - normalizedRPM;
    }

    AudioSource CreateAudioSource(AudioClip clip)
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.spatialBlend = 1f; // Full 3D spatialization
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.Play();

        return audioSource;
    }

    void SetOutputMixerGroup(AudioSource audioSource)
    {
        // Set the output mixer group for the AudioSource
        if (audioSource.outputAudioMixerGroup == null)
        {
            audioSource.outputAudioMixerGroup = outputMixerGroup;
        }
    }

    float GetCarRPM()
    {
        return engine.RPM;
    }
}