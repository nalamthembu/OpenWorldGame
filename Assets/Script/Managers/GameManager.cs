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

    public void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }

    public void SetCursorState(bool isVisible, CursorLockMode lockState)
    {
        Cursor.visible = isVisible;
        Cursor.lockState = lockState;
    }
}