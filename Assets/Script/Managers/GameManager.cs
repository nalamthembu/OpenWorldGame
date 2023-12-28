using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(GameStateMachine))]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        if (instance is null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SetCursorState(false, CursorLockMode.Confined);

    }

    public void SetTimeScale(float timeScale) => Time.timeScale = timeScale;

    private void OnEnable() => QuitGamePrompt.PlayerSureToQuit += QuitGame;


    private void OnDisable() => QuitGamePrompt.PlayerSureToQuit -= QuitGame;


    public void SetCursorState(bool isVisible, CursorLockMode lockState)
    {
        Cursor.visible = isVisible;
        Cursor.lockState = lockState;
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}