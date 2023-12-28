using UnityEngine;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public QuitGamePrompt quitGamePrompt;

    public PauseMenu pauseMenu;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetPauseMenuEnabled(bool value) => pauseMenu.SetEnabled(value);

    public void ShowQuitPrompt() => quitGamePrompt.SetEnabled(true);

    public void SetQuitToTrue()
    {
        if (quitGamePrompt.IsEnabled)
            quitGamePrompt.playerSureToQuit = true;
    }

    public void CloseQuitPrompt()
    {
        quitGamePrompt.SetEnabled(false);
        quitGamePrompt.GOScreen.SetActive(false);
    }

    private void Update()
    {
        if (quitGamePrompt.IsEnabled)
            quitGamePrompt.Update();

        if (pauseMenu.IsEnabled)
            pauseMenu.Update();
    }
}

public abstract class UIScreen
{
    [SerializeField] protected bool enabled;
    public GameObject GOScreen;

    public bool IsEnabled { get { return enabled; } }

    public void SetEnabled(bool value) => enabled = value;

    public abstract void Update();
}

[System.Serializable]
public class QuitGamePrompt : UIScreen
{

    public bool playerSureToQuit;

    public static event Action PlayerSureToQuit;

    public override void Update()
    {
        if (!GOScreen.activeSelf)
        {
            GOScreen.SetActive(true);
        }

        if (playerSureToQuit)
        {
            PlayerSureToQuit?.Invoke();

            return;
        }
    }
}

[System.Serializable]
public class PauseMenu : UIScreen
{
    public override void Update()
    {
        GOScreen.SetActive(GameStateMachine.Instance.IsGamePaused);
    }
}