﻿using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] int m_SandBoxLevelIndex = 2;

    [SerializeField] TMP_Text m_CameraSensitivityText;

    [SerializeField] Slider m_CameraSensitivitySlider;

    public static event Action OnEnterMainMenu;

    private void Awake()
    {
        OnEnterMainMenu?.Invoke();
    }

    public void ExitGame()
    {
#if UNITY_EDITOR

        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
#endif

        Application.Quit();
    }
    
    public void SelectOption()
    {
        if (SoundManager.Instance)
        {
            SoundManager.Instance.PlayFESound("ButtonSelect");
        }
        else
            Debug.LogError("There is no Sound Manager in this scene!");
    }

    public void PromptOption()
    {
        if (SoundManager.Instance)
        {
            SoundManager.Instance.PlayFESound("Prompt");
        }
        else
            Debug.LogError("There is no Sound Manager in this scene!");
    }

    public void ReturnOption()
    {
        if (SoundManager.Instance)
        {
            SoundManager.Instance.PlayFESound("ButtonReturn");
        }
        else
            Debug.LogError("There is no Sound Manager in this scene!");
    }

    public void StartGame()
    {
        if (LevelManager.Instance)
        {
            LevelManager.Instance.LoadLevel(m_SandBoxLevelIndex);
        }
    }
}