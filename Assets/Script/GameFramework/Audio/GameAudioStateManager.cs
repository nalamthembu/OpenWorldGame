﻿using UnityEngine;
using UnityEngine.Audio;

public class GameAudioStateManager : MonoBehaviour
{
    [Header("----------General----------")]
    [Tooltip("How fast we transition from whatever state to low health state.")]
    [SerializeField] float m_LowHealthTransitionTime = 0.5F;
    [Tooltip("How fast we transition from whatever state to death state.")]
    [SerializeField] float m_DeathTransitionTime = 0.25F;
    [Tooltip("How fast we transition from whatever state to normal state.")]
    [SerializeField] float m_NormalStateTransitionTime = 2.5F;
    [Tooltip("How fast we transition from whatever state to no sound except UI state")]
    [SerializeField] float m_FrontendOnlyTransitionTime = 3.0F;

    [SerializeField] AudioMixer m_AudioMixer;

    bool m_HasPlayedCloseToDeathSound = false;

    private void Start()
    {
        if (!SoundManager.Instance)
            Debug.LogError("There is no Sound Manager in this scene!");
    }

    private void OnEnable()
    {
        PlayerHealthComponent.OnHealthChange += OnPlayerHealthChange;
        PlayerHealthComponent.OnPlayerDeath += OnPlayerDeath;
        LevelManager.OnLoadingStart += OnLoadingStart;
        LevelManager.OnLoadingComplete += OnLoadingComplete;
        GameManager.OnGamePaused += OnGamePaused;
        GameManager.OnGameResume += OnGameResume;
    }

    private void OnDisable()
    {
        PlayerHealthComponent.OnHealthChange -= OnPlayerHealthChange;
        PlayerHealthComponent.OnPlayerDeath -= OnPlayerDeath;
        LevelManager.OnLoadingStart -= OnLoadingStart;
        LevelManager.OnLoadingComplete -= OnLoadingComplete;
        GameManager.OnGamePaused -= OnGamePaused;
        GameManager.OnGameResume -= OnGameResume;
    }

    private void Update()
    {
        m_AudioMixer.SetFloat("MasterPitch", Time.timeScale);
    }

    private void OnGameResume() => SwitchToNormalGameSound();
    private void OnLoadingComplete() => SwitchToNormalGameSound();
    private void OnLoadingStart() => SwitchToFrontEndOnly();
    private void OnPlayerDeath() => SwitchToDeathGameSound();
    private void OnGamePaused() => SwitchToFrontEndOnly();

    private void SwitchToNormalGameSound()
    {
        m_HasPlayedCloseToDeathSound = false;
        SoundManager.Instance.TransitionToMixerState("Normal", m_NormalStateTransitionTime);
    }
    private void SwitchToFrontEndOnly () => SoundManager.Instance.TransitionToMixerState("FrontendOnly", m_FrontendOnlyTransitionTime);
    private void SwitchToDeathGameSound()
    {
        SoundManager.Instance.TransitionToMixerState("Death", m_DeathTransitionTime);
        SoundManager.Instance.PlayFESound("Death");
    }
    private void SwitchToLowHealthGameSound()
    {
        if (!m_HasPlayedCloseToDeathSound)
        {
            SoundManager.Instance.PlayFESound("CloseToDeath");
            m_HasPlayedCloseToDeathSound = true;
        }

        SoundManager.Instance.TransitionToMixerState("LowHealth", m_LowHealthTransitionTime);
    }


    private void OnPlayerHealthChange(float health, float armor)
    {
        if (health > 0 && health <= 30)
            SwitchToLowHealthGameSound();

        if (health > 30)
            SwitchToNormalGameSound();
    }

}