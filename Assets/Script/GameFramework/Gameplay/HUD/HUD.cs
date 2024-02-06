using UnityEngine;

public class HUD : MonoBehaviour
{
    [Header("----------HUD Object----------")]
    [SerializeField] protected GameObject m_HUDObject;

    protected virtual void Awake() { /* used by child classes */ }
    protected virtual void Start() { /* used by child classes */ }

    protected virtual void OnEnable()
    {
        GameManager.OnGameEnded += OnGameOver;
        GameManager.OnGamePaused += OnGamePaused;
        GameManager.OnGameResume += OnGameResume;
    }

    protected virtual void OnDisable()
    {
        GameManager.OnGameEnded -= OnGameOver;
        GameManager.OnGamePaused -= OnGamePaused;
        GameManager.OnGameResume -= OnGameResume;
    }

    private void DisableObject()
    {
        if (m_HUDObject)
        {
            m_HUDObject.SetActive(false);
        }
        else
            Debug.LogError("There is no HUD game object assigned!");
    }
    private void EnableObject()
    {
        if (m_HUDObject)
        {
            m_HUDObject.SetActive(true);
        }
        else
            Debug.LogError("There is no HUD game object assigned!");
    }


    private void OnGameOver() => DisableObject();
    private void OnGameResume() => EnableObject();
    private void OnGamePaused() => DisableObject();
}